using LumaSharp.Compiler.AST;
using System.Text;

namespace LumaSharp.Compiler.Reporting
{
    internal sealed class CompileReport : ICompileReportProvider
    {
        // Private
        private List<CompileDiagnostic> diagnostics = new List<CompileDiagnostic>();

        // Properties
        public IEnumerable<ICompileDiagnostic> Diagnostics
        {
            get
            {
                foreach(CompileDiagnostic message in diagnostics)
                    yield return message;
            }
        }

        public int DiagnosticCount
        {
            get { return diagnostics.Count; }
        }

        public bool HasAnyDiagnostics
        {
            get { return diagnostics.Count > 0; }
        }

        // Methods
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            // Process all
            foreach(CompileDiagnostic message in diagnostics)
                builder.AppendLine(message.ToString());

            return builder.ToString();
        }

        public void ReportDiagnostic(Code id, MessageSeverity severity, SyntaxSource source, params object[] args)
        {
            Dictionary<Code, string> messageSet;

            // Check severity
            messageSet = severity switch
            {
                MessageSeverity.Message => CompilerErrors.messages,
                MessageSeverity.Warning => CompilerErrors.warnings,
                MessageSeverity.Error => CompilerErrors.errors,

                _ => throw new NotImplementedException(),
            };

            // Check for error
            if (messageSet == null)
                throw new InvalidOperationException("Unknown message severity: " + severity);

            // Try to get error string
            string message;
            if (messageSet.TryGetValue(id, out message) == false)
                throw new InvalidOperationException("Invalid message id: " + id);

            // Build error string
            try
            {
                diagnostics.Add(new CompileDiagnostic((int)id, severity, source,
                    string.Format(message, args)));
            }
            catch(FormatException)
            {
                throw new InvalidOperationException("Invalid message args: " + args.Length);
            }
        }

        public IEnumerable<ICompileDiagnostic> GetDiagnostics(MessageSeverity severity)
        {
            return diagnostics.Where(m => m.Severity == severity)
                .Cast<ICompileDiagnostic>();
        }

        public bool HasDiagnostics(MessageSeverity severity)
        {
            return diagnostics.Any(m => m.Severity == severity);
        }

        public void Combine(ICompileReportProvider fromReport)
        {
            foreach(CompileDiagnostic message in fromReport.Diagnostics)
            {
                diagnostics.Add(message);
            }
        }
    }
}
