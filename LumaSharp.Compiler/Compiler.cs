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
                units.Add(new Parser(tokens, diagnostics, options.UnsafeEnabled, cancellationToken).ParseCompilationUnit());
            }
            cancellationToken.ThrowIfCancellationRequested();
            var analysis = new SemanticAnalyzer(options, diagnostics, cancellationToken).Analyze(units);
            if (!options.StaticLibrary && analysis.EntryPoint is null)
                diagnostics.Add("LS2201", DiagnosticSeverity.Error, "Executable compilation requires exactly one public static Main with a supported signature.");
            if (FinalDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error)) return new(FinalDiagnostics(), null);

            cancellationToken.ThrowIfCancellationRequested();
            try { return new(FinalDiagnostics(), new LlvmEmitter(analysis, cancellationToken).Emit()); }
            catch (NotSupportedException ex) { diagnostics.Add("LMC3000", DiagnosticSeverity.Error, ex.Message); return new(FinalDiagnostics(), null); }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            diagnostics.Add("LMC9000", DiagnosticSeverity.Error, $"Internal compiler failure: {ex.Message}");
            return new(FinalDiagnostics(), null);
        }
    }
}
