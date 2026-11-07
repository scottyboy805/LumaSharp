using System.Diagnostics;
using System.Text.Json;
using LumaSharp.Compiler;
using Xunit;

namespace LumaSharp.Tests;

public sealed class PreprocessorTests
{
    private static CompilationResult Compile(string source, CompilationOptions? options = null) =>
        LumaCompiler.Compile([("preprocessor-test.ls", source)], options);

    [Fact]
    public void DefinesAndNestedConditionalExpressionsSelectOnlyActiveCode()
    {
        const string source = """
            #define LOCAL
            #if UNKNOWN || (DEBUG && LOCAL && TRACE)
            public class Program { public static int32 Main() { return 42; } }
            #elif DEBUG
            this branch must not be lexed @@@
            #else
            neither should this branch {{{
            #endif
            """;
        var result = Compile(source, new CompilationOptions(PreprocessorSymbols: new HashSet<string>(["DEBUG", "TRACE"], StringComparer.Ordinal)));

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(42, LlvmJit.RunMain(result.LlvmIr!).ExitCode);
    }

    [Fact]
    public void LeadingUtf8BomBeforeASourceDirectiveIsIgnored()
    {
        var result = Compile("\uFEFF#define LOCAL\n#if LOCAL\npublic class Program { public static int32 Main() { return 0; } }\n#endif");

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(result.LlvmIr!).ExitCode);
    }

    [Fact]
    public void SourceDefinesAreScopedToOneFile()
    {
        var sources = new[]
        {
            ("defines.ls", "#define PRIVATE_TO_FILE\npublic class Helper { }"),
            ("program.ls", "#if PRIVATE_TO_FILE\n#error a source define leaked into another file\n#endif\npublic class Program { public static int32 Main() { return 0; } }")
        };
        var result = LumaCompiler.Compile(sources);

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Code == "LS0003");
    }

    [Fact]
    public void WarningAndErrorDirectivesRespectConditionalBranches()
    {
        const string warningSource = """
            #disable 3005
            #warning this warning is disabled
            #enable LS3005
            #warning this warning remains visible
            #if false
            #error inactive errors do not fail compilation
            #endif
            public class Program { public static int32 Main() { return 0; } }
            """;
        var warningResult = Compile(warningSource);
        Assert.True(warningResult.Succeeded, string.Join(Environment.NewLine, warningResult.Diagnostics));
        var warning = Assert.Single(warningResult.Diagnostics, diagnostic => diagnostic.Code == "LS3005");
        Assert.Equal(DiagnosticSeverity.Warning, warning.Severity);
        Assert.Contains("remains visible", warning.Message, StringComparison.Ordinal);

        var errorResult = Compile("#error requested failure\npublic class Program { public static int32 Main() { return 0; } }");
        Assert.False(errorResult.Succeeded);
        Assert.Contains(errorResult.Diagnostics, diagnostic => diagnostic.Code == "LS0003" &&
            diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Message == "requested failure");
        Assert.Null(errorResult.LlvmIr);
    }

    [Fact]
    public void DiagnosticDisableAndEnableRangesApplyBeforeWarningPromotion()
    {
        const string source = """
            #disable 3001
            public class Program
            {
                public static int32 Main() { return 0; let suppressed = 1; }
            }
            #enable LS3001
            public class Other
            {
                public static void Run() { return; let visible = 2; }
            }
            """;
        var result = Compile(source, new CompilationOptions(WarningsAsErrors: true));

        Assert.False(result.Succeeded);
        var diagnostic = Assert.Single(result.Diagnostics, item => item.Code == "LS3001");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal(9, diagnostic.Location!.Line);
        Assert.Null(result.LlvmIr);
    }

    [Theory]
    [InlineData("#if DEBUG ||\npublic class Program { public static int32 Main() { return 0; } }\n#endif")]
    [InlineData("#elif DEBUG\npublic class Program { public static int32 Main() { return 0; } }")]
    [InlineData("#else\npublic class Program { public static int32 Main() { return 0; } }")]
    [InlineData("#endif\npublic class Program { public static int32 Main() { return 0; } }")]
    [InlineData("#if DEBUG\npublic class Program { public static int32 Main() { return 0; } }")]
    [InlineData("public class Program { public static int32 Main() { return 0; } }\n#define LATE")]
    [InlineData("#unknown directive\npublic class Program { public static int32 Main() { return 0; } }")]
    [InlineData("#disable 103\npublic class Program { public static int32 Main() { return 0; } }")]
    public void MalformedDirectivesProduceStableDiagnostics(string source)
    {
        var result = Compile(source);
        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS0002" && diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.Null(result.LlvmIr);
    }

    [Fact]
    public void DirectiveLookalikesInCommentsAndVerbatimStringsAreNotProcessed()
    {
        const string source = """
            /*
            #if NEVER_DEFINED
            */
            public class Program
            {
                public static int32 Main()
                {
                    string text = @"first line
            #error this text is inside a string
            second line";
                    return text.Length > 0 ? 0 : 1;
                }
            }
            """;
        var result = Compile(source);

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(result.LlvmIr!).ExitCode);
    }

    [Fact]
    public void ProjectJsonPreprocessorSymbolsControlTheCompilationBatch()
    {
        using var workspace = new TemporaryWorkspace(AppContext.BaseDirectory);
        var repositoryRoot = FindRepositoryRoot();
        workspace.Write("src/Feature.ls", "#if DEBUG\npublic class Feature { public static int32 Value() => 42; }\n#else\nnot valid Luma source @@@\n#endif");
        workspace.Write("luma.json", JsonSerializer.Serialize(new
        {
            schemaVersion = 1,
            outputKind = "staticLibrary",
            outputName = "Feature",
            outputDirectory = "out",
            intermediateDirectory = "obj",
            runtimeLibraryPath = Path.Combine(repositoryRoot, "LumaSharp.Library"),
            preprocessorSymbols = new[] { "DEBUG", "TRACE" },
            logLevel = "quiet"
        }));
        var cli = FindCliAssembly(repositoryRoot);
        using var process = Process.Start(new ProcessStartInfo("dotnet")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            ArgumentList = { cli, "build", Path.Combine(workspace.Root, "luma.json"), "--no-llvm", "--log-level", "quiet" }
        })!;
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        Assert.True(process.ExitCode == 0, $"CLI exit code {process.ExitCode}. stdout: {stdout} stderr: {stderr}");
        Assert.True(File.Exists(Path.Combine(workspace.Root, "obj", "Feature.ll")));
        Assert.Contains("Feature", File.ReadAllText(Path.Combine(workspace.Root, "obj", "Feature.ll")), StringComparison.Ordinal);
    }

    [Fact]
    public void ProjectJsonRejectsMalformedPreprocessorSymbolSettings()
    {
        using var workspace = new TemporaryWorkspace(AppContext.BaseDirectory);
        var repositoryRoot = FindRepositoryRoot();
        workspace.Write("Program.ls", "public class Program { public static int32 Main() { return 0; } }");
        workspace.Write("luma.json", JsonSerializer.Serialize(new
        {
            schemaVersion = 1,
            outputKind = "staticLibrary",
            outputName = "InvalidSymbols",
            runtimeLibraryPath = Path.Combine(repositoryRoot, "LumaSharp.Library"),
            preprocessorSymbols = "DEBUG"
        }));
        using var process = Process.Start(new ProcessStartInfo("dotnet")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            ArgumentList = { FindCliAssembly(repositoryRoot), "build", Path.Combine(workspace.Root, "luma.json") }
        })!;
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        Assert.Equal(2, process.ExitCode);
        Assert.Contains("preprocessorSymbols must be an array", stderr, StringComparison.Ordinal);
        Assert.DoesNotContain("preprocessorSymbols must be an array", stdout, StringComparison.Ordinal);
    }

    [Fact]
    public void ElifEvaluatesEqualityAndNegationAndSelectsTheMatchingBranch()
    {
        const string source = """
            #define LOCAL
            #if false
            #error inactive first branch
            #elif LOCAL == true && !MISSING && (false != LOCAL)
            public class Program { public static int32 Main() { return 17; } }
            #else
            this fallback must not be parsed @@@
            #endif
            """;
        var result = Compile(source);

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(17, LlvmJit.RunMain(result.LlvmIr!).ExitCode);
    }

    [Fact]
    public void InvalidProjectSymbolsBecomeCompilerDiagnostics()
    {
        const string source = "public class Program { public static int32 Main() { return 0; } }";
        var result = Compile(source, new CompilationOptions(PreprocessorSymbols: new HashSet<string>(["not-valid"], StringComparer.Ordinal)));

        Assert.False(result.Succeeded);
        var diagnostic = Assert.Single(result.Diagnostics, item => item.Code == "LS0002");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("not a valid identifier", diagnostic.Message, StringComparison.Ordinal);
        Assert.Null(result.LlvmIr);
    }
    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "LumaSharp.slnx"))) return directory.FullName;
        throw new DirectoryNotFoundException("Could not locate the LumaSharp repository root.");
    }

    private static string FindCliAssembly(string repositoryRoot)
    {
        foreach (var configuration in new[] { "Debug", "Release" })
        {
            var path = Path.Combine(repositoryRoot, "Build", configuration, "net10.0", "Luma.dll");
            if (File.Exists(path)) return path;
        }
        throw new FileNotFoundException("Could not locate the built Luma CLI assembly.");
    }

    private sealed class TemporaryWorkspace : IDisposable
    {
        public string Root { get; }

        public TemporaryWorkspace(string parent)
        {
            Root = Path.Combine(parent, "luma-preprocessor-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
        }

        public string Write(string relativePath, string text)
        {
            var path = Path.Combine(Root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, text);
            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
        }
    }
}
