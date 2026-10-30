namespace LumaSharp.Compiler;

public sealed record CompilationOptions(
    bool WarningsAsErrors = false,
    bool UnsafeEnabled = false,
    bool StaticLibrary = false,
    IReadOnlySet<string>? DisabledWarnings = null,
    string TargetTriple = "host",
    bool Optimize = false,
    bool EmitLlvmIrOnly = false);
public sealed record CompilationResult(IReadOnlyList<Diagnostic> Diagnostics, string? LlvmIr)
{
    public bool Succeeded => LlvmIr is not null && !Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
}

public static class LumaCompiler
{
    public static CompilationResult Compile(IEnumerable<(string Path, string Text)> sources, CompilationOptions? options = null, CancellationToken cancellationToken = default)
    {
        var sourceList = sources.ToArray();
        var pathComparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        var runtimePaths = sourceList
            .Select(source => Path.GetFullPath(source.Path))
            .Where(IsRuntimeLibraryPath)
            .ToHashSet(pathComparer);
        return CompileCore(sourceList, options, runtimePaths, cancellationToken);
    }

    internal static CompilationResult CompileWithRuntimeLibrary(IEnumerable<(string Path, string Text)> sources, CompilationOptions options,
        IEnumerable<string> runtimeSourcePaths, CancellationToken cancellationToken = default)
    {
        var pathComparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        var runtimePaths = runtimeSourcePaths.Select(Path.GetFullPath).ToHashSet(pathComparer);
        return CompileCore(sources, options, runtimePaths, cancellationToken);
    }

    private static CompilationResult CompileCore(IEnumerable<(string Path, string Text)> sources, CompilationOptions? options,
        IReadOnlySet<string> runtimeSourcePaths, CancellationToken cancellationToken)
    {
        options ??= new();
        var diagnostics = new DiagnosticBag();
        IReadOnlyList<Diagnostic> FinalDiagnostics() => diagnostics.Items
            .Where(d => d.Severity != DiagnosticSeverity.Warning || options.DisabledWarnings?.Contains(d.Code) != true)
            .Select(d => options.WarningsAsErrors && d.Severity == DiagnosticSeverity.Warning ? d with { Severity = DiagnosticSeverity.Error } : d)
            .ToArray();
        try
        {
            var units = new List<CompilationUnit>();
            foreach (var (path, text) in sources)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var tokens = new Lexer(path, text, diagnostics, cancellationToken).Scan();
                var isRuntimeSource = runtimeSourcePaths.Contains(Path.GetFullPath(path));
                units.Add(new Parser(tokens, diagnostics, options.UnsafeEnabled || isRuntimeSource, cancellationToken).ParseCompilationUnit());
            }
            cancellationToken.ThrowIfCancellationRequested();
            var analysis = new SemanticAnalyzer(options, diagnostics, runtimeSourcePaths, cancellationToken).Analyze(units);
            if (!options.StaticLibrary && analysis.EntryPoint is null)
                diagnostics.Add("LS2201", DiagnosticSeverity.Error, "Executable compilation requires exactly one public static Main with a supported signature.");
            if (FinalDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error)) return new(FinalDiagnostics(), null);

            cancellationToken.ThrowIfCancellationRequested();
            try { return new(FinalDiagnostics(), new LlvmEmitter(analysis, !options.StaticLibrary && options.Optimize, cancellationToken).Emit()); }
            catch (NotSupportedException ex) { diagnostics.Add("LMC3000", DiagnosticSeverity.Error, ex.Message); return new(FinalDiagnostics(), null); }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            diagnostics.Add("LMC9000", DiagnosticSeverity.Error, $"Internal compiler failure: {ex.Message}");
            return new(FinalDiagnostics(), null);
        }
    }

    private static bool IsRuntimeLibraryPath(string path) =>
        path.Replace('\\', '/').Contains("/LumaSharp.Library/", StringComparison.OrdinalIgnoreCase);
}
