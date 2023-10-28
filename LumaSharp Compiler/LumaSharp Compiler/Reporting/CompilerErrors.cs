
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
            { 1001, "The type `{0}` could not be found. Are you missing a library reference or import statement?" }

            // Logical
        };
    }
}
