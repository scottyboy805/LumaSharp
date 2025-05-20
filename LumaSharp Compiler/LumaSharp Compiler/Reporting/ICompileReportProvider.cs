using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Reporting
{
    public interface ICompileReportProvider : ICompileReport
    {
        // Methods
        void ReportDiagnostic(Code id, MessageSeverity severity, SyntaxSource source, params object[] args);
    }
}
