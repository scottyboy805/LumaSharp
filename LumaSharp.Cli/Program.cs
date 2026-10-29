using System.Text.Json;
using LumaSharp.Compiler;

return await Cli.RunAsync(args);

internal static class Cli
{
    private static readonly HashSet<string> ProjectProperties = ["schemaVersion", "outputKind", "outputName", "outputDirectory", "intermediateDirectory", "targetTriple", "runtimeLibraryPath", "unsafe", "optimize", "logLevel", "warningsAsErrors", "disabledWarnings", "excludeDirectories"];
    public static async Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0 || args[0] is "--help" or "-h") return Help();
        if (args[0] == "--version") { Console.WriteLine("LumaSharp 0.2.0-preview"); return 0; }
        if (args[0] != "build") return Usage("Unknown command.");
        if (args.Length == 1 || args[1] is "--help" or "-h") return Help();
        var positionals = new List<string>(); var overrides = new Overrides();
        for (var i = 1; i < args.Length; i++)
        {
            var argument = args[i];
            if (!argument.StartsWith("--", StringComparison.Ordinal)) { positionals.Add(argument); continue; }
            string? Value() => i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal) ? args[++i] : null;
            switch (argument)
            {
                case "--kind": overrides.Kind = Value(); if (overrides.Kind is not ("executable" or "staticLibrary")) return Usage("--kind expects executable or staticLibrary."); break;
                case "--name": overrides.Name = Value(); if (overrides.Name is null) return Usage("--name requires a value."); break;
                case "--out-dir": overrides.Output = Value(); if (overrides.Output is null) return Usage("--out-dir requires a value."); break;
                case "--intermediate-dir": overrides.Intermediate = Value(); if (overrides.Intermediate is null) return Usage("--intermediate-dir requires a value."); break;
                case "--runtime-library": overrides.RuntimeLibraryPath = Value(); if (overrides.RuntimeLibraryPath is null) return Usage("--runtime-library requires a directory path."); break;
                case "--target": overrides.Target = Value(); if (overrides.Target is null) return Usage("--target requires a value."); break;
                case "--log-level": overrides.LogLevel = Value(); if (overrides.LogLevel is not ("quiet" or "normal" or "verbose" or "debug")) return Usage("--log-level expects quiet, normal, verbose, or debug."); break;
                case "--no-warn": var code = Value(); if (code is null) return Usage("--no-warn requires a diagnostic code."); overrides.DisabledWarnings.Add(code); break;
                case "--unsafe": overrides.Unsafe = true; break;
                case "--no-unsafe": overrides.Unsafe = false; break;
                case "--optimize": overrides.Optimize = true; break;
                case "--no-optimize": overrides.Optimize = false; break;
                case "--warnings-as-errors": overrides.WarningsAsErrors = true; break;
                case "--no-llvm": overrides.EmitIrOnly = true; break;
                default: return Usage($"Unknown option '{argument}'.");
            }
        }
        if (positionals.Count == 0) return Usage("A source file or project file is required.");
        var projectMode = positionals.Count == 1 && Path.GetExtension(positionals[0]).Equals(".json", StringComparison.OrdinalIgnoreCase);
        if (projectMode && positionals.Count != 1) return Usage("Project mode cannot be combined with source files.");
        var cwd = Directory.GetCurrentDirectory(); var projectRoot = cwd; var sourcePaths = new List<string>(); var settings = new Settings();
        try
        {
            if (projectMode)
            {
                var projectPath = Path.GetFullPath(positionals[0]); projectRoot = Path.GetDirectoryName(projectPath)!; settings.Name = Path.GetFileNameWithoutExtension(projectPath);
                if (!File.Exists(projectPath)) return Fail("LMC1005", $"Project file '{projectPath}' does not exist.", 2);
                LoadProject(projectPath, settings);
                ApplyOverrides(settings, overrides, projectRoot);
                sourcePaths = DiscoverSources(projectRoot, settings.Output, settings.Intermediate, settings.ExcludedDirectories);
            }
            else
            {
                if (positionals.Any(p => !Path.GetExtension(p).Equals(".ls", StringComparison.OrdinalIgnoreCase))) return Usage("Source mode accepts only .ls paths.");
                var canonical = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
                foreach (var path in positionals) { var full = Path.GetFullPath(path); if (!File.Exists(full)) return Fail("LMC1005", $"Source file '{path}' does not exist.", 2); if (canonical.Add(full)) sourcePaths.Add(full); }
                settings.Name = sourcePaths.Count == 1 ? Path.GetFileNameWithoutExtension(sourcePaths[0]) : "LumaProgram";
                ApplyOverrides(settings, overrides, cwd);
            }
            if (sourcePaths.Count == 0) return Fail("LMC1004", "No .ls source files were found.", 2);
            var options = new CompilationOptions(
                WarningsAsErrors: settings.WarningsAsErrors,
                UnsafeEnabled: settings.Unsafe,
                StaticLibrary: settings.Kind == "staticLibrary",
                DisabledWarnings: settings.DisabledWarnings,
                TargetTriple: settings.Target ?? "host",
                Optimize: settings.Optimize,
                EmitLlvmIrOnly: overrides.EmitIrOnly);
            var build = await LumaBuild.BuildAsync(new CompilerBuildRequest(
                sourcePaths,
                options,
                settings.Output!,
                settings.Intermediate!,
                settings.Name,
                settings.RuntimeLibraryPath));
            foreach (var diagnostic in build.Diagnostics) Console.Error.WriteLine(diagnostic);
            if (!build.Succeeded)
            {
                if (build.Cancelled) return 130;
                return build.Diagnostics.Any(d => d.Code.StartsWith("LMC2", StringComparison.Ordinal) || d.Code.StartsWith("LMC9", StringComparison.Ordinal) || d.Code == "LMC1006") ? 3 : 1;
            }
            if (settings.LogLevel != "quiet")
                Console.WriteLine(overrides.EmitIrOnly ? $"Verified LLVM IR written to {build.OutputPaths[0]}" : $"Built {build.OutputPaths[0]}");
            return 0;
        }
        catch (ProjectException ex) { return Fail(ex.Code, ex.Message, 2); }
        catch (JsonException ex) { return Fail("LMC1002", $"Invalid project JSON: {ex.Message}", 2); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        { return Fail("LMC1006", $"Build input/output error: {ex.Message}", 3); }
    }
    private static int Help()
    {
        Console.WriteLine("LumaSharp compiler\n\nUsage:\n  luma build <source.ls> [additional-source.ls ...] [options]\n  luma build <project.json> [options]\n  luma --help | --version\n\nOptions:\n  --kind <executable|staticLibrary> --name <name>\n  --out-dir <path> --intermediate-dir <path> --runtime-library <directory>\n  --target <host|LLVM triple> --unsafe | --no-unsafe\n  --optimize | --no-optimize --log-level <quiet|normal|verbose|debug>\n  --warnings-as-errors --no-warn <code> --no-llvm (write LLVM IR only)"); return 0;
    }
    private static int Usage(string message) { Console.Error.WriteLine($"LMC1001: error: {message}\nRun 'luma --help' for usage."); return 2; }
    private static int Fail(string code, string message, int exitCode) { Console.Error.WriteLine($"{code}: error: {message}"); return exitCode; }
    private static void LoadProject(string path, Settings settings)
    {
        string projectText;
        try { projectText = File.ReadAllText(path, new System.Text.UTF8Encoding(false, true)); }
        catch (System.Text.DecoderFallbackException ex) { throw new ProjectException("LMC1002", $"Project file is not valid UTF-8: {ex.Message}"); }
        using var document = JsonDocument.Parse(projectText); var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object) throw new JsonException("Project root must be an object.");
        var rootPath = Path.GetDirectoryName(path)!;
        var seenProperties = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in root.EnumerateObject())
        {
            if (!seenProperties.Add(property.Name)) throw new ProjectException("LMC1002", $"Project property '{property.Name}' is specified more than once.");
            if (property.Name is "sources" or "sourceFiles" or "includeFiles") throw new ProjectException("LMC1003", "Project files cannot enumerate source files.");
            if (!ProjectProperties.Contains(property.Name)) throw new ProjectException("LMC1002", $"Unknown project property '{property.Name}'.");
        }
        if (root.TryGetProperty("schemaVersion", out var schema)) { if (schema.ValueKind != JsonValueKind.Number || !schema.TryGetInt32(out var version) || version != 1) throw new ProjectException("LMC1002", "Unsupported or invalid schemaVersion; expected integer 1."); }
        if (root.TryGetProperty("outputKind", out var kind)) settings.Kind = String(kind, "outputKind");
        if (settings.Kind is not ("executable" or "staticLibrary")) throw new ProjectException("LMC1002", "outputKind must be executable or staticLibrary.");
        if (root.TryGetProperty("outputName", out var name)) settings.Name = String(name, "outputName");
        if (root.TryGetProperty("outputDirectory", out var output)) settings.Output = String(output, "outputDirectory");
        if (root.TryGetProperty("intermediateDirectory", out var intermediate)) settings.Intermediate = String(intermediate, "intermediateDirectory");
        if (root.TryGetProperty("runtimeLibraryPath", out var runtimeLibraryPath)) settings.RuntimeLibraryPath = Resolve(rootPath, String(runtimeLibraryPath, "runtimeLibraryPath"));
        if (root.TryGetProperty("targetTriple", out var target)) settings.Target = String(target, "targetTriple");
        if (root.TryGetProperty("unsafe", out var unsafeElement)) settings.Unsafe = Boolean(unsafeElement, "unsafe");
        if (root.TryGetProperty("optimize", out var optimize)) settings.Optimize = Boolean(optimize, "optimize");
        if (root.TryGetProperty("warningsAsErrors", out var wae)) settings.WarningsAsErrors = Boolean(wae, "warningsAsErrors");
        if (root.TryGetProperty("logLevel", out var log)) { settings.LogLevel = String(log, "logLevel"); if (settings.LogLevel is not ("quiet" or "normal" or "verbose" or "debug")) throw new ProjectException("LMC1002", "Invalid logLevel."); }
        if (root.TryGetProperty("disabledWarnings", out var disabled)) { if (disabled.ValueKind != JsonValueKind.Array) throw new ProjectException("LMC1002", "disabledWarnings must be an array."); foreach (var item in disabled.EnumerateArray()) settings.DisabledWarnings.Add(String(item, "disabledWarnings item")); }
        if (root.TryGetProperty("excludeDirectories", out var excluded)) { if (excluded.ValueKind != JsonValueKind.Array) throw new ProjectException("LMC1002", "excludeDirectories must be an array."); foreach (var item in excluded.EnumerateArray()) settings.ExcludedDirectories.Add(String(item, "excludeDirectories item")); }
        settings.Output = Resolve(rootPath, settings.Output ?? "bin"); settings.Intermediate = Resolve(rootPath, settings.Intermediate ?? "obj");
        var exclusions = settings.ExcludedDirectories.ToArray();
        foreach (var exclusion in exclusions)
        {
            if (string.IsNullOrWhiteSpace(exclusion) || Path.IsPathRooted(exclusion)) throw new ProjectException("LMC1002", "excludeDirectories entries must be non-empty project-relative directory paths.");
            var fullPath = Resolve(rootPath, exclusion);
            if (!IsWithinRoot(rootPath, fullPath)) throw new ProjectException("LMC1002", $"Excluded directory '{exclusion}' must remain inside the project root.");
            settings.ExcludedDirectories.Remove(exclusion);
            settings.ExcludedDirectories.Add(Path.GetRelativePath(rootPath, fullPath));
        }
        settings.ExcludedDirectories.Add(Path.GetRelativePath(rootPath, settings.Output));
        settings.ExcludedDirectories.Add(Path.GetRelativePath(rootPath, settings.Intermediate));
    }
    private static string String(JsonElement element, string name) => element.ValueKind == JsonValueKind.String ? element.GetString()! : throw new ProjectException("LMC1002", $"'{name}' must be a string.");
    private static bool Boolean(JsonElement element, string name) => element.ValueKind is JsonValueKind.True or JsonValueKind.False ? element.GetBoolean() : throw new ProjectException("LMC1002", $"'{name}' must be a boolean.");
    private static List<string> DiscoverSources(string root, string? output, string? intermediate, HashSet<string> additionalExclusions)
    {
        var excluded = new HashSet<string>([".git", ".hg", ".svn", ".vs", "bin", "obj"], StringComparer.OrdinalIgnoreCase);
        foreach (var relative in additionalExclusions) if (!Path.IsPathRooted(relative)) excluded.Add(NormalizeRelative(relative));
        foreach (var path in new[] { output, intermediate })
        {
            if (string.IsNullOrWhiteSpace(path)) continue;
            var fullPath = Path.GetFullPath(path);
            if (IsWithinRoot(root, fullPath)) excluded.Add(NormalizeRelative(Path.GetRelativePath(root, fullPath)));
        }
        var result = new List<string>(); var pending = new Stack<string>(); pending.Push(root);
        while (pending.TryPop(out var directory))
        {
            foreach (var entry in Directory.EnumerateFileSystemEntries(directory))
            {
                var attr = File.GetAttributes(entry); var relative = Path.GetRelativePath(root, entry);
                if ((attr & FileAttributes.Directory) != 0)
                {
                    if ((attr & FileAttributes.ReparsePoint) != 0) continue;
                    if (excluded.Any(x => !string.IsNullOrEmpty(x) && IsSameOrChild(relative, x))) continue;
                    pending.Push(entry);
                }
                else if (Path.GetExtension(entry).Equals(".ls", StringComparison.OrdinalIgnoreCase)) result.Add(Path.GetFullPath(entry));
            }
        }
        result.Sort((a, b) => StringComparer.Ordinal.Compare(Path.GetRelativePath(root, a).Replace('\\', '/'), Path.GetRelativePath(root, b).Replace('\\', '/'))); return result;
    }
    private static bool IsSameOrChild(string path, string dir) => path.Equals(dir, StringComparison.OrdinalIgnoreCase) || path.StartsWith(dir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || path.StartsWith(dir + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    private static string NormalizeRelative(string path) => path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar);
    private static bool IsWithinRoot(string root, string path)
    {
        var relative = Path.GetRelativePath(Path.GetFullPath(root), Path.GetFullPath(path));
        return relative != ".." && !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) && !Path.IsPathRooted(relative);
    }
    private static void ApplyOverrides(Settings settings, Overrides overrides, string root)
    {
        if (overrides.Kind is not null) settings.Kind = overrides.Kind; if (overrides.Name is not null) settings.Name = overrides.Name;
        if (overrides.Output is not null) settings.Output = Resolve(root, overrides.Output);
        else settings.Output ??= Resolve(root, "bin");
        if (overrides.Intermediate is not null) settings.Intermediate = Resolve(root, overrides.Intermediate);
        else settings.Intermediate ??= Resolve(root, "obj");
        if (overrides.RuntimeLibraryPath is not null) settings.RuntimeLibraryPath = Resolve(root, overrides.RuntimeLibraryPath);
        if (overrides.Target is not null) settings.Target = overrides.Target; if (overrides.Unsafe is not null) settings.Unsafe = overrides.Unsafe.Value;
        if (overrides.Optimize is not null) settings.Optimize = overrides.Optimize.Value; if (overrides.LogLevel is not null) settings.LogLevel = overrides.LogLevel;
        if (overrides.WarningsAsErrors) settings.WarningsAsErrors = true; settings.DisabledWarnings.UnionWith(overrides.DisabledWarnings);
        if (string.IsNullOrWhiteSpace(settings.Name) || Path.GetFileName(settings.Name) != settings.Name || settings.Name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ProjectException("LMC1002", "outputName must be a valid file name without directory separators.");
    }
    private static string Resolve(string root, string path) => Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(root, path));
    private sealed class ProjectException(string code, string message) : Exception(message) { public string Code { get; } = code; }
    private sealed class Settings
    {
        public string Kind { get; set; } = "executable"; public string Name { get; set; } = "LumaProgram"; public string? Output { get; set; } public string? Intermediate { get; set; } public string? Target { get; set; } = "host";
        public string? RuntimeLibraryPath { get; set; }
        public bool Unsafe { get; set; } public bool Optimize { get; set; } public bool WarningsAsErrors { get; set; } public string LogLevel { get; set; } = "normal";
        public HashSet<string> DisabledWarnings { get; } = new(StringComparer.OrdinalIgnoreCase); public HashSet<string> ExcludedDirectories { get; } = new(StringComparer.OrdinalIgnoreCase);
    }
    private sealed class Overrides
    {
        public string? Kind { get; set; } public string? Name { get; set; } public string? Output { get; set; } public string? Intermediate { get; set; } public string? Target { get; set; } public string? RuntimeLibraryPath { get; set; }
        public bool? Unsafe { get; set; } public bool? Optimize { get; set; } public bool WarningsAsErrors { get; set; } public bool EmitIrOnly { get; set; } public string? LogLevel { get; set; }
        public HashSet<string> DisabledWarnings { get; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
