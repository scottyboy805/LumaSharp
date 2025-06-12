using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Reporting
{
    public enum MessageSeverity
    {
        Message,
        Warning,
        Error,
    }

    public interface ICompileDiagnostic
    {
        // Properties
        int Code { get; }

        MessageSeverity Severity { get; }

        SyntaxSpan Source { get; }

        string Message { get; }
    }
}
