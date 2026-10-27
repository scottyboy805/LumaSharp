using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Reporting
{
    internal interface IDiagnosticReporter
    {
        // Methods
        void Report(Code id, DiagnosticSeverity severity, SyntaxSpan? source, params object[] args);
    }
}
