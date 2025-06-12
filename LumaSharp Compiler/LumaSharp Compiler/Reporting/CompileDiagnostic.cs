
using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Reporting
{
    internal struct CompileDiagnostic : ICompileDiagnostic
    {
        // Private
        private int code;
        private MessageSeverity severity;
        private SyntaxSpan source;
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

        public SyntaxSpan Source
        {
            get { return source; }
        }

        public string Message
        {
            get { return message; }
        }

        // Constructor
        internal CompileDiagnostic(int code, MessageSeverity severity, SyntaxSpan source, string message)
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
