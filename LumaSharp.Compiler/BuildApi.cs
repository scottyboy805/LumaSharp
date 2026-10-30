using System.Diagnostics;
using System.Text;

namespace LumaSharp.Compiler;

/// <summary>Describes one complete native or verified-IR build.</summary>
public sealed record CompilerBuildRequest(
    IReadOnlyList<string> SourcePaths,
    CompilationOptions Options,
    string OutputDirectory,
    string IntermediateDirectory,
    string OutputName,
    string? RuntimeLibraryPath = null);

/// <summary>Structured result from the standalone compiler-library build API.</summary>
public sealed record CompilerBuildResult(
    bool Succeeded,
    IReadOnlyList<Diagnostic> Diagnostics,
    IReadOnlyList<string> OutputPaths,
    bool Cancelled = false);

/// <summary>
/// Complete build orchestration for callers that do not use the command-line executable.
/// The CLI is intentionally only responsible for parsing arguments and rendering this result.
/// </summary>
public static class LumaBuild
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private static StringComparer PathComparer => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    public static async Task<CompilerBuildResult> BuildAsync(CompilerBuildRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var outputs = new List<string>();
        var diagnostics = new List<Diagnostic>();
        string? objectPath = null;
        string? temporaryOutput = null;
        string? stagingOutput = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (request.SourcePaths is null || request.SourcePaths.Count == 0)
                return Failure("LMC1005", "At least one source file is required.");
            if (request.Options is null)
                return Failure("LMC1002", "Build options are required.");
            if (string.IsNullOrWhiteSpace(request.OutputName) || Path.GetFileName(request.OutputName) != request.OutputName || request.OutputName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return Failure("LMC1002", "OutputName must be a valid file name without directory separators.");

            var sources = new List<(string Path, string Text)>();
            var seen = new HashSet<string>(PathComparer);
            var inputPaths = request.SourcePaths.ToList();
            var runtimeDirectory = RuntimeLibraryResolver.Resolve(request.RuntimeLibraryPath, inputPaths);
            var runtimeSourcePaths = runtimeDirectory is null
                ? Array.Empty<string>()
                : Directory.EnumerateFiles(runtimeDirectory, "*.ls", SearchOption.AllDirectories).ToArray();
            if (runtimeDirectory is not null)
                inputPaths.AddRange(runtimeSourcePaths);
            foreach (var inputPath in inputPaths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(inputPath) || !Path.GetExtension(inputPath).Equals(".ls", StringComparison.OrdinalIgnoreCase))
                    return Failure("LMC1005", $"Source path '{inputPath}' must name a .ls file.");
                var fullPath = Path.GetFullPath(inputPath);
                if (!seen.Add(fullPath)) continue;
                if (!File.Exists(fullPath)) return Failure("LMC1005", $"Source file '{fullPath}' does not exist.");
                var bytes = await File.ReadAllBytesAsync(fullPath, cancellationToken).ConfigureAwait(false);
                string text;
                try { text = StrictUtf8.GetString(bytes); }
                catch (DecoderFallbackException ex) { return Failure("LMC1006", $"Source file '{fullPath}' is not valid UTF-8: {ex.Message}"); }
                sources.Add((fullPath, text));
            }

            var targetValidation = LlvmToolchain.ValidateTarget(request.Options.TargetTriple);
            if (!targetValidation.Succeeded)
                return Failure("LMC2001", targetValidation.Error ?? $"LLVM target '{request.Options.TargetTriple}' is unavailable.");
            var target = targetValidation.TargetTriple!;

            var compilation = LumaCompiler.CompileWithRuntimeLibrary(sources, request.Options, runtimeSourcePaths, cancellationToken);
            diagnostics.AddRange(compilation.Diagnostics);
            if (!compilation.Succeeded) return new(false, diagnostics.ToArray(), []);

            var outputDirectory = Path.GetFullPath(request.OutputDirectory);
            var intermediateDirectory = Path.GetFullPath(request.IntermediateDirectory);
            Directory.CreateDirectory(outputDirectory);
            Directory.CreateDirectory(intermediateDirectory);
            var irPath = Path.Combine(intermediateDirectory, request.OutputName + ".ll");
            await File.WriteAllTextAsync(irPath, compilation.LlvmIr!, new UTF8Encoding(false), cancellationToken).ConfigureAwait(false);

            objectPath = Path.Combine(intermediateDirectory, request.OutputName + "." + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
            var backend = LlvmToolchain.VerifyAndEmitObject(compilation.LlvmIr!, objectPath, target, request.Options.Optimize, cancellationToken);
            if (!backend.Succeeded)
            {
                var code = backend.Error?.Contains("unavailable", StringComparison.OrdinalIgnoreCase) == true ? "LMC2001" : "LMC2002";
                return Failure(code, backend.Error ?? "LLVM verification or native object emission failed.", diagnostics);
            }

            if (request.Options.EmitLlvmIrOnly)
            {
                TryDelete(objectPath);
                objectPath = null;
                outputs.Add(irPath);
                return new(true, diagnostics.ToArray(), outputs.ToArray());
            }

            cancellationToken.ThrowIfCancellationRequested();
            var extension = request.Options.StaticLibrary
                ? (OperatingSystem.IsWindows() ? ".lib" : ".a")
                : (OperatingSystem.IsWindows() ? ".exe" : "");
            var finalOutput = Path.Combine(outputDirectory, request.OutputName + extension);
            temporaryOutput = Path.Combine(intermediateDirectory, request.OutputName + "." + Guid.NewGuid().ToString("N") + ".tmp" + extension);
            var needsWindowsShell32 = compilation.LlvmIr!.Contains("@CommandLineToArgvW", StringComparison.Ordinal);
            var needsLinuxDl = target.Contains("linux", StringComparison.OrdinalIgnoreCase) &&
                compilation.LlvmIr.Contains("@dlopen(", StringComparison.Ordinal);
            var link = await LinkAsync(objectPath, temporaryOutput, request.Options.StaticLibrary, target, needsWindowsShell32, needsLinuxDl, cancellationToken).ConfigureAwait(false);
            if (link is not null) return Failure("LMC2002", link, diagnostics);

            cancellationToken.ThrowIfCancellationRequested();
            // Stage on the destination volume so replacement remains atomic even when obj and bin differ.
            stagingOutput = Path.Combine(outputDirectory, request.OutputName + "." + Guid.NewGuid().ToString("N") + ".publish" + extension);
            File.Copy(temporaryOutput, stagingOutput, overwrite: false);
            File.Move(stagingOutput, finalOutput, true);
            stagingOutput = null;
            TryDelete(temporaryOutput);
            temporaryOutput = null;
            outputs.Add(finalOutput);
            return new(true, diagnostics.ToArray(), outputs.ToArray());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new(false, [new("LMC1007", DiagnosticSeverity.Error, "Compilation was cancelled safely; no final output was published.")], [], true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return Failure("LMC1006", $"Build input or output error: {ex.Message}", diagnostics);
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            return Failure("LMC2002", $"Could not start the native toolchain: {ex.Message}", diagnostics);
        }
        catch (Exception ex)
        {
            return Failure("LMC9000", $"Internal compiler failure: {ex.Message}", diagnostics);
        }
        finally
        {
            if (objectPath is not null) TryDelete(objectPath);
            if (temporaryOutput is not null) TryDelete(temporaryOutput);
            if (stagingOutput is not null) TryDelete(stagingOutput);
        }

        CompilerBuildResult Failure(string code, string message, IReadOnlyList<Diagnostic>? prior = null)
        {
            var result = prior is null ? new List<Diagnostic>() : [.. prior];
            result.Add(new(code, DiagnosticSeverity.Error, message));
            return new(false, result.ToArray(), []);
        }
    }

    private static async Task<string?> LinkAsync(string objectPath, string temporaryOutput, bool staticLibrary, string target,
        bool needsWindowsShell32, bool needsLinuxDl, CancellationToken cancellationToken)
    {
        var tool = staticLibrary
            ? OperatingSystem.IsWindows()
                ? FindTool("llvm-lib") ?? FindTool("lib") ?? FindTool("llvm-ar")
                : FindTool("llvm-ar") ?? FindTool("ar")
            : FindTool("clang") ?? FindTool("clang-cl") ?? FindTool("link");
        if (tool is null)
            return staticLibrary
                ? "No compatible archive tool is installed for static-library output."
                : "LLVM generated a verified native object, but no compatible system linker is installed.";

        var toolName = Path.GetFileNameWithoutExtension(tool);
        var hostOnlyTargetError = ValidateHostOnlyLinkerTarget(toolName, target, LlvmToolchain.GetHostTargetTriple());
        if (hostOnlyTargetError is not null) return hostOnlyTargetError;

        IEnumerable<string> arguments;
        if (staticLibrary)
        {
            var toolFileName = Path.GetFileNameWithoutExtension(tool);
            var isWindowsLibrarian = OperatingSystem.IsWindows() &&
                (toolFileName.Equals("lib", StringComparison.OrdinalIgnoreCase) ||
                 toolFileName.Equals("llvm-lib", StringComparison.OrdinalIgnoreCase));
            arguments = isWindowsLibrarian
                ? ["/NOLOGO", $"/OUT:{temporaryOutput}", objectPath]
                : ["rcs", temporaryOutput, objectPath];
        }
        else
        {
            var name = Path.GetFileNameWithoutExtension(tool);
            if (name.Equals("link", StringComparison.OrdinalIgnoreCase))
            {
                var linkArguments = new List<string> { $"/OUT:{temporaryOutput}", "/SUBSYSTEM:CONSOLE", objectPath };
                if (OperatingSystem.IsWindows())
                {
                    linkArguments.AddRange(["/DEFAULTLIB:libcmt.lib", "/DEFAULTLIB:libvcruntime.lib", "/DEFAULTLIB:oldnames.lib", "/DEFAULTLIB:kernel32.lib"]);
                    if (needsWindowsShell32) linkArguments.Add("/DEFAULTLIB:shell32.lib");
                }
                arguments = linkArguments;
            }
            else if (name.Equals("clang-cl", StringComparison.OrdinalIgnoreCase))
            {
                var clangClArguments = new List<string> { objectPath, $"/Fe:{temporaryOutput}" };
                if (needsWindowsShell32) clangClArguments.Add("shell32.lib");
                arguments = clangClArguments;
            }
            else
            {
                var clangArguments = new List<string>();
                var host = LlvmToolchain.GetHostTargetTriple();
                if (!string.Equals(target, host, StringComparison.Ordinal)) { clangArguments.Add("--target"); clangArguments.Add(target); }
                clangArguments.Add(objectPath); clangArguments.Add("-o"); clangArguments.Add(temporaryOutput);
                if (needsWindowsShell32) clangArguments.Add("-lshell32");
                if (needsLinuxDl) clangArguments.Add("-ldl");
                arguments = clangArguments;
            }
        }

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(tool)
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        foreach (var argument in arguments) process.StartInfo.ArgumentList.Add(argument);
        process.Start();
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        try { await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false); }
        catch (OperationCanceledException)
        {
            try { if (!process.HasExited) process.Kill(entireProcessTree: true); } catch { }
            await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
            throw;
        }
        var error = await errorTask.ConfigureAwait(false);
        var output = await outputTask.ConfigureAwait(false);
        var details = string.Join(Environment.NewLine, new[] { error.Trim(), output.Trim() }.Where(text => text.Length > 0));
        return process.ExitCode == 0 ? null : $"Native {(staticLibrary ? "archive" : "linking")} failed: {details}";
    }

    internal static string? ValidateHostOnlyLinkerTarget(string linkerName, string requestedTarget, string? hostTarget)
    {
        if (!Path.GetFileNameWithoutExtension(linkerName).Equals("clang-cl", StringComparison.OrdinalIgnoreCase) &&
            !Path.GetFileNameWithoutExtension(linkerName).Equals("link", StringComparison.OrdinalIgnoreCase))
            return null;
        if (string.IsNullOrWhiteSpace(hostTarget))
            return $"Cannot verify host-only linker '{linkerName}' for requested target '{requestedTarget}' because LLVM did not provide the host target triple.";
        if (string.Equals(requestedTarget, hostTarget, StringComparison.OrdinalIgnoreCase)) return null;
        return $"The selected linker '{linkerName}' is configured for host target '{hostTarget}' and cannot safely link requested target '{requestedTarget}'. Install a target-capable linker for the requested target.";
    }

    private static string? FindTool(string name)
    {
        var extensions = OperatingSystem.IsWindows()
            ? (Environment.GetEnvironmentVariable("PATHEXT") ?? ".COM;.EXE;.BAT;.CMD").Split(';', StringSplitOptions.RemoveEmptyEntries)
            : [""];
        var directories = new List<string>((Environment.GetEnvironmentVariable("PATH") ?? "")
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries));
        foreach (var variable in new[] { "LLVM_HOME", "LLVM_ROOT" })
        {
            var root = Environment.GetEnvironmentVariable(variable);
            if (string.IsNullOrWhiteSpace(root)) continue;
            root = root.Trim('"');
            directories.Add(root);
            directories.Add(Path.Combine(root, "bin"));
        }
        if (OperatingSystem.IsWindows())
        {
            foreach (var variable in new[] { "ProgramW6432", "ProgramFiles", "ProgramFiles(x86)" })
                if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(variable)))
                    directories.Add(Path.Combine(Environment.GetEnvironmentVariable(variable)!, "LLVM", "bin"));
            var visualStudio = Environment.GetEnvironmentVariable("VSINSTALLDIR");
            if (!string.IsNullOrWhiteSpace(visualStudio))
                directories.Add(Path.Combine(visualStudio, "VC", "Tools", "Llvm", "bin"));
        }
        else if (OperatingSystem.IsMacOS())
        {
            directories.Add("/opt/homebrew/opt/llvm/bin");
            directories.Add("/usr/local/opt/llvm/bin");
        }

        var seenDirectories = new HashSet<string>(OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal);
        foreach (var directory in directories)
        {
            var normalizedDirectory = directory.Trim('"');
            if (!seenDirectories.Add(normalizedDirectory)) continue;
            foreach (var extension in extensions)
            {
                var candidate = Path.Combine(normalizedDirectory, name + extension);
                if (File.Exists(candidate)) return candidate;
            }
        }
        return null;
    }

    private static void TryDelete(string path) { try { File.Delete(path); } catch { } }
}
