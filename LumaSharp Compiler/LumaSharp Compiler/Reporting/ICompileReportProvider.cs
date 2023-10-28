using LumaSharp_Compiler.AST;

namespace LumaSharp_Compiler.Reporting
{
    public interface ICompileReportProvider
    {
        // Properties
        IEnumerable<ICompileMessage> Messages { get; }

        int MessageCount { get; }

        bool HasAnyMessages { get; }

        // Methods
        void ReportMessage(int id, MessageSeverity severity, SyntaxSource source, params object[] args);

        IEnumerable<ICompileMessage> GetMessages(MessageSeverity severity);

        bool HasMessages(MessageSeverity severity);
    }
}
