using System.Diagnostics;
using Xunit;

namespace LumaSharp.Tests;

public sealed class CliTests
{
    [Fact]
    public async Task CompilesMultipleFilesToIntermediateLlvm()
    {
        using var workspace = new TemporaryWorkspace();
        var main = workspace.Write("src/Main.ls", "class App { public static int32 Main() { return MathUtil.Add(20, 22); } }");
        var helper = workspace.Write("src/Math.ls", "class MathUtil { public static int32 Add(int32 a, int32 b) { return a + b; } }");
        var result = await Run(workspace.Root, "build", main, helper, "--no-llvm", "--out-dir", "bin", "--intermediate-dir", "obj");
        Assert.Equal(0, result.ExitCode);
        Assert.True(File.Exists(Path.Combine(workspace.Root, "obj", "LumaProgram.ll")));
        Assert.Contains("call i32 @luma_MathUtil_Add_2", File.ReadAllText(Path.Combine(workspace.Root, "obj", "LumaProgram.ll")));
    }

    [Fact]
    public async Task ProjectModeRecursivelyDiscoversFilesAndExcludesOutputTrees()
    {
        using var workspace = new TemporaryWorkspace();
        workspace.Write("nested/Program.ls", "class Program { public static int32 Main() { return 42; } }");
        workspace.Write("bin/Generated.ls", "invalid generated input");
        workspace.Write("obj/Generated.ls", "invalid generated input");
        var project = workspace.Write("sample.luma.json", """{"schemaVersion":1,"outputDirectory":"bin","intermediateDirectory":"obj"}""");
        var result = await Run(workspace.Root, "build", project, "--no-llvm");
        Assert.Equal(0, result.ExitCode);
        Assert.True(File.Exists(Path.Combine(workspace.Root, "obj", "sample.luma.ll")));
    }

    [Fact]
    public async Task ProjectModeExcludesCommandLineOutputOverridesAndConfiguredDirectories()
    {
        using var workspace = new TemporaryWorkspace();
        workspace.Write("nested/Program.ls", "class Program { public static int32 Main() { return 42; } }");
        workspace.Write("custom-bin/Generated.ls", "this is generated and must not be parsed");
        workspace.Write("custom-obj/Generated.ls", "this is generated and must not be parsed");
        workspace.Write("generated-src/Generated.ls", "this is excluded by project configuration");
        var project = workspace.Write("sample.json", """{"schemaVersion":1,"outputDirectory":"bin","intermediateDirectory":"obj","excludeDirectories":["generated-src"]}""");

        var result = await Run(workspace.Root, "build", project, "--out-dir", "custom-bin", "--intermediate-dir", "custom-obj", "--no-llvm");

        Assert.Equal(0, result.ExitCode);
        Assert.True(File.Exists(Path.Combine(workspace.Root, "custom-obj", "sample.ll")));
    }

    [Fact]
    public async Task RejectsForbiddenProjectSourceListsAndUnknownProperties()
    {
        using var workspace = new TemporaryWorkspace();
        var withSources = workspace.Write("bad.luma.json", """{"schemaVersion":1,"sources":[]}""");
        var sourcesResult = await Run(workspace.Root, "build", withSources, "--no-llvm");
        Assert.Equal(2, sourcesResult.ExitCode);
        Assert.Contains("LMC1003", sourcesResult.StandardError);
        var unknown = workspace.Write("unknown.luma.json", """{"schemaVersion":1,"optmize":true}""");
        var unknownResult = await Run(workspace.Root, "build", unknown, "--no-llvm");
        Assert.Equal(2, unknownResult.ExitCode);
        Assert.Contains("LMC1002", unknownResult.StandardError);

        var traversal = workspace.Write("traversal.json", """{"schemaVersion":1,"excludeDirectories":["../outside"]}""");
        var traversalResult = await Run(workspace.Root, "build", traversal, "--no-llvm");
        Assert.Equal(2, traversalResult.ExitCode);
        Assert.Contains("LMC1002", traversalResult.StandardError);
    }

    [Fact]
    public async Task ReportsUnknownOptionsWithUsageExitCode()
    {
        using var workspace = new TemporaryWorkspace();
        var result = await Run(workspace.Root, "build", "--bogus");
        Assert.Equal(2, result.ExitCode);
        Assert.Contains("LMC1001", result.StandardError);
    }

    private static async Task<ProcessResult> Run(string workingDirectory, params string[] arguments)
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var cli = Path.Combine(root, "src", "LumaSharp.Cli", "bin", "Debug", "net10.0", "LumaSharp.Cli.dll");
        var start = new ProcessStartInfo("dotnet") { WorkingDirectory = workingDirectory, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
        start.ArgumentList.Add(cli); foreach (var arg in arguments) start.ArgumentList.Add(arg);
        using var process = Process.Start(start)!;
        var stdout = process.StandardOutput.ReadToEndAsync(); var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(); return new(process.ExitCode, await stdout, await stderr);
    }

    private sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);

    private sealed class TemporaryWorkspace : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "luma-cli-tests-" + Guid.NewGuid().ToString("N"));
        public TemporaryWorkspace() => Directory.CreateDirectory(Root);
        public string Write(string relativePath, string contents)
        {
            var path = Path.Combine(Root, relativePath); Directory.CreateDirectory(Path.GetDirectoryName(path)!); File.WriteAllText(path, contents); return path;
        }
        public void Dispose() { if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true); }
    }
}
