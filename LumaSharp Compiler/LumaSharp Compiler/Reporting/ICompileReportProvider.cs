using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Reporting
{
    public interface ICompileReportProvider : ICompileReport
    {
        // Methods
        void ReportDiagnostic(Code id, MessageSeverity severity, SyntaxSpan source, params object[] args);
    }
}
