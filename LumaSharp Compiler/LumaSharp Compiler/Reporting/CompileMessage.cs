
using LumaSharp_Compiler.AST;

namespace LumaSharp_Compiler.Reporting
{
    internal struct CompileMessage : ICompileMessage
    {
        // Private
        private MessageSeverity severity;
        private SyntaxSource source;
        private string message;

        // Properties
        public MessageSeverity Severity
        {
            get { return severity; }
        }

        public SyntaxSource Source
        {
            get { return source; }
        }

        public string Message
        {
            get { return message; }
        }

        // Constructor
        internal CompileMessage(MessageSeverity severity, SyntaxSource source, string message)
        {
            this.severity = severity;
            this.source = source;
            this.message = message;
        }
    }
}
