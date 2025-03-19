using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Reporting
{
    public interface ICompileReport
    {
        // Properties
        IEnumerable<ICompileMessage> Messages { get; }

        int MessageCount { get; }

        bool HasAnyMessages { get; }

        // Methods
        IEnumerable<ICompileMessage> GetMessages(MessageSeverity severity);

        bool HasMessages(MessageSeverity severity);
    }
}
