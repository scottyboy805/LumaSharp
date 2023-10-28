using LumaSharp_Compiler.AST;

namespace LumaSharp_Compiler.Reporting
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
        MessageSeverity Severity { get; }

        SyntaxSource Source { get; }

        string Message { get; }
    }
}
