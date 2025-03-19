using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Reporting
{
    public enum MessageSeverity
    {
        Message,
        Warning,
        Error,
    }

    public interface ICompileMessage
    {
        // Properties
        int Code { get; }

        MessageSeverity Severity { get; }

        SyntaxSource Source { get; }

        string Message { get; }
    }
}
