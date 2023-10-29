using LumaSharp_Compiler.AST;

namespace LumaSharp_Compiler.Reporting
{
    public interface ICompileReportProvider : ICompileReport
    {
        // Methods
        void ReportMessage(Code id, MessageSeverity severity, SyntaxSource source, params object[] args);
    }
}
