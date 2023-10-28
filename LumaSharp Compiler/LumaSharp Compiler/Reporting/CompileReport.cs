using LumaSharp_Compiler.AST;

namespace LumaSharp_Compiler.Reporting
{
    internal sealed class CompileReport : ICompileReportProvider
    {
        // Private
        private List<CompileMessage> messages = new List<CompileMessage>();

        // Properties
        public IEnumerable<ICompileMessage> Messages
        {
            get
            {
                foreach(CompileMessage message in messages)
                    yield return message;
            }
        }

        public int MessageCount
        {
            get { return messages.Count; }
        }

        public bool HasAnyMessages
        {
            get { return messages.Count > 0; }
        }

        // Methods
        public void ReportMessage(int id, MessageSeverity severity, SyntaxSource source, params object[] args)
        {
            Dictionary<int, string> messageSet;

            // Check severity
            messageSet = severity switch
            {
                MessageSeverity.Message => CompilerErrors.messages,
                MessageSeverity.Warning => CompilerErrors.warnings,
                MessageSeverity.Error => CompilerErrors.errors,
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
                messages.Add(new CompileMessage(severity, source,
                    string.Format(message, args)));
            }
            catch(FormatException)
            {
                throw new InvalidOperationException("Invalid message args: " + args.Length);
            }
        }

        public IEnumerable<ICompileMessage> GetMessages(MessageSeverity severity)
        {
            return messages.Where(m => m.Severity == severity)
                .Cast<ICompileMessage>();
        }

        public bool HasMessages(MessageSeverity severity)
        {
            return messages.Any(m => m.Severity == severity);
        }

        public void Combine(ICompileReportProvider fromReport)
        {
            foreach(CompileMessage message in fromReport.Messages)
            {
                messages.Add(message);
            }
        }
    }
}
