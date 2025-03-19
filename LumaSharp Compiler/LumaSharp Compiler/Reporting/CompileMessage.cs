
using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Reporting
{
    internal struct CompileMessage : ICompileMessage
    {
        // Private
        private int code;
        private MessageSeverity severity;
        private SyntaxSource source;
        private string message;

        // Properties
        public int Code
        {
            get { return code; }
        }

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
        internal CompileMessage(int code, MessageSeverity severity, SyntaxSource source, string message)
        {
            this.code = code;
            this.severity = severity;
            this.source = source;
            this.message = message;
        }

        // Methods
        public override string ToString()
        {
            return CompilerErrors.GetFormattedErrorMessage(code, severity, message, source);
        }
    }
}
