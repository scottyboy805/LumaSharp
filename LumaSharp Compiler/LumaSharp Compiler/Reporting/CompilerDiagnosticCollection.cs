using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Reporting
{
    internal sealed class CompilerDiagnosticCollection : IDiagnosticReporter
    {
        // Private
        private readonly List<CompilerDiagnostic> diagnostics = new();

        // Public
        public static readonly CompilerDiagnosticCollection Empty = new();

        // Properties
        public IReadOnlyList<CompilerDiagnostic> AllDiagnostics => diagnostics;

        // Methods
        public void Report(Code id, DiagnosticSeverity severity, SyntaxSpan? source, params object[] args)
        {
            Dictionary<Code, string> messageSet;

            // Check severity
            messageSet = severity switch
            {
                DiagnosticSeverity.Message => CompilerErrors.messages,
                DiagnosticSeverity.Warning => CompilerErrors.warnings,
                DiagnosticSeverity.Error => CompilerErrors.errors,

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
                diagnostics.Add(new CompilerDiagnostic((int)id, severity,
                    source != null ? source.Value : default,
                    string.Format(message, args)));
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("Invalid message args: " + args.Length);
            }
        }

        public IEnumerable<CompilerDiagnostic> GetDiagnostics()
        {
            return diagnostics;
        }

        public IEnumerable<CompilerDiagnostic> GetDiagnostics(DiagnosticSeverity severity)
        {
            return diagnostics.Where(m => m.Severity == severity)
                .Cast<CompilerDiagnostic>();
        }

        public bool HasDiagnostics(DiagnosticSeverity severity)
        {
            return diagnostics.Any(m => m.Severity == severity);
        }
    }
}
