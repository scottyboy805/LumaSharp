
using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Reporting
{
    public enum DiagnosticSeverity
    {
        Message,
        Warning,
        Error,
    }

    public readonly struct CompilerDiagnostic
    {
        // Private
        private readonly int code;
        private readonly DiagnosticSeverity severity;
        private readonly SyntaxSpan source;
        private readonly string message;

        // Properties
        public int Code
        {
            get { return code; }
        }

        public DiagnosticSeverity Severity
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
        internal CompilerDiagnostic(int code, DiagnosticSeverity severity, SyntaxSpan source, string message)
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
