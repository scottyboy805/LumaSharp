
using LumaSharp_Compiler.AST;

namespace LumaSharp_Compiler.Reporting
{
    // Type
    public enum Code
    {
        TypeNotFound = 1001,
        TypeGenericPrimitive = 1002,
        TypeArrayPrimitive = 1003,

        IdentifierNotFound = 1031,
    }

    internal static class CompilerErrors
    {
        // Public
        public static readonly Dictionary<Code, string> messages = new Dictionary<Code, string>
        {

        };

        public static readonly Dictionary<Code, string> warnings = new Dictionary<Code, string>
        {

        };

        public static readonly Dictionary<Code, string> errors = new Dictionary<Code, string>
        {
            // Lexer

            // Syntax

            // Semantic
            { Code.TypeNotFound, "The type `{0}` could not be found. Are you missing a library reference or import statement?" },
            { Code.TypeGenericPrimitive, "Cannot apply generic parameters to built in type `{0}`" },
            { Code.TypeArrayPrimitive, "Cannot apply array indexing to built in type `{0}`" },

            { Code.IdentifierNotFound, "The identifier `{0}` does not exist in the current context" },


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
