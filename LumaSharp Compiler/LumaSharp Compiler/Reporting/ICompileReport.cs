using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Reporting
{
    public interface ICompileReport
    {
        // Properties
        IEnumerable<ICompileDiagnostic> Diagnostics { get; }

        int DiagnosticCount { get; }

        bool HasAnyDiagnostics { get; }

        // Methods
        IEnumerable<ICompileDiagnostic> GetDiagnostics(MessageSeverity severity);

        bool HasDiagnostics(MessageSeverity severity);
    }
}
