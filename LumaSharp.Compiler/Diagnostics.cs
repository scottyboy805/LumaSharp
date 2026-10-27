namespace LumaSharp.Compiler;

public enum DiagnosticSeverity { Info, Warning, Error }
public sealed record SourceLocation(string File, int Line, int Column);
public sealed record Diagnostic(string Code, DiagnosticSeverity Severity, string Message, SourceLocation? Location = null)
{
    public override string ToString() => Location is null
        ? $"{Code}: {Severity.ToString().ToLowerInvariant()}: {Message}"
        : $"{Location.File}({Location.Line},{Location.Column}): {Severity.ToString().ToLowerInvariant()} {Code}: {Message}";
}
public sealed class DiagnosticBag
{
    private readonly List<Diagnostic> _items = [];
    private readonly HashSet<Diagnostic> _seen = [];
    public IReadOnlyList<Diagnostic> Items => _items;
    public bool HasErrors => _items.Any(d => d.Severity == DiagnosticSeverity.Error);
    public void Add(string code, DiagnosticSeverity severity, string message, SourceLocation? location = null)
    {
        var diagnostic = new Diagnostic(code, severity, message, location);
        if (_seen.Add(diagnostic)) _items.Add(diagnostic);
    }

    internal void DiscardSince(int count)
    {
        if (count < 0 || count > _items.Count) throw new ArgumentOutOfRangeException(nameof(count));
        if (count == _items.Count) return;
        _items.RemoveRange(count, _items.Count - count);
        _seen.Clear();
        _seen.UnionWith(_items);
    }
}
