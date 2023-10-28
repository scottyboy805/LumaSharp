
using LumaSharp_Compiler.AST;

namespace LumaSharp_Compiler.Reporting
{
    internal static class CompilerErrors
    {
        // Public
        public static readonly Dictionary<int, string> messages = new Dictionary<int, string>
        {

        };

        public static readonly Dictionary<int, string> warnings = new Dictionary<int, string>
        {

        };

        public static readonly Dictionary<int, string> errors = new Dictionary<int, string>
        {
            // Lexer

            // Syntax

            // Semantic
            { 1001, "The type `{0}` could not be found. Are you missing a library reference or import statement?" },
            { 1002, "Cannot apply generic parameters to built in type `{0}`" },
            { 1003, "Cannot apply array indexing to built in type `{0}`" },

            { 1031, "The identifier `{0}` does not exist in the current context" },


            // Logical
        };

        // Methods
        public static string GetFormattedErrorMessage(int code, MessageSeverity severity, string message, SyntaxSource source)
        {
            // Create formatted
            string formatted = string.Format("[{0}] LS{1}: {2}", severity.ToString().ToUpper(), code, message);

            // Check for source
            if(source != null)
            {
                // Create formatted source
                string formattedSource = string.Format(" in source file `{0}` at line `{1}`, column `{2}`", 
                    string.IsNullOrEmpty(source.SourceName) == false ? source.SourceName : "<unknown>", 
                    source.Line >= 0 ? source.Line : "<?>", 
                    source.Column >= 0 ? source.Column : "<?>");

                // Join strings
                formatted = string.Concat(formatted, formattedSource);
            }
            return formatted;
        }
    }
}
