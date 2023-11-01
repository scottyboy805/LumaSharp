
using LumaSharp_Compiler.AST;

namespace LumaSharp_Compiler.Reporting
{
    // Type
    public enum Code
    {
        InvalidConversion = 331,

        KeywordNotValid = 501,

        MultipleAccessModifiers = 531,
        AccessModifierNotValid = 532,
        AccessModifierCantChange = 533,

        NamespaceNotFound = 971,
        SubNamespaceNotFound = 972,

        TypeNotFound = 1001,
        TypeNotFoundNamespace = 1002,
        TypeGenericPrimitive = 1003,
        TypeArrayPrimitive = 1004,

        IdentifierNotFound = 1031,
        IdentifierUsedBeforeDeclared = 1032,

        FieldAccessorNotFound = 1061,
        FieldRequiresInstance = 1062,
        FieldRequiresType = 1063,
        FieldReadOnly = 1064,
        FieldUsedLikeMethod = 1065,

        AccessorRequiresInstance = 1092,
        AccessorRequiresType = 1093,
        AccessorNoRead = 1094,
        AccessorNoWrite = 1095,
        AccessorUsedLikeMethod = 1096,

        MethodNotFound = 1121,
        MethodRequiresInstance = 1122,
        MethodRequiresType = 1123,
        MethodNoMatch = 1124,
        MethodAmbiguousMatch = 1125,
        MethodUsedLikeFieldAccessor = 1126,
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
            { Code.InvalidConversion, "Cannot implicitly convert type `{0}` to `{1}`. Are you missing a cast?" },

            { Code.KeywordNotValid, "Keyword `{0}` is not valid in the current context" },

            { Code.MultipleAccessModifiers, "Multiple access modifiers" },
            { Code.AccessModifierNotValid, "Access modifier `{0}` is not valid in the current context" },
            { Code.AccessModifierCantChange, "Cannot change access modifier `{0}` when overriding" },

            { Code.NamespaceNotFound, "The namespace `{0}` could not be found. Are you missing a library reference?" },
            { Code.SubNamespaceNotFound, "The namespace name `{0}` could not be found in namespace `{1}`. Are you missing a library reference?" },

            { Code.TypeNotFound, "The type `{0}` could not be found. Are you missing a library reference or import statement?" },
            { Code.TypeNotFoundNamespace, "The type `{0}` does not exist in the namespace `{0}`" },
            { Code.TypeGenericPrimitive, "Cannot apply generic parameters to built in type `{0}`" },
            { Code.TypeArrayPrimitive, "Cannot apply array indexing to built in type `{0}`" },

            { Code.IdentifierNotFound, "The identifier `{0}` does not exist in the current context" },
            { Code.IdentifierUsedBeforeDeclared, "The identifier `{0}` cannot be accessed before it is declared" },

            { Code.FieldAccessorNotFound, "The field or accessor `{0}` is not defined on the type `{1}`" },
            { Code.FieldRequiresInstance, "The field `{0}` must be accessed via an instance" },
            { Code.FieldRequiresType, "The field `{0}` is marked as global must be accessed via a type qualifier" },
            { Code.FieldReadOnly, "The field `{0}` is marked as read only and cannot be assigned after initialization" },

            { Code.AccessorRequiresInstance, "The accessor `{0}` must be accessed via an instance" },
            { Code.AccessorRequiresType, "The accessor `{0}` is marked as global must be accessed via a type qualifier" },
            { Code.AccessorNoRead, "The accessor `{0}` does not define a read implementation" },
            { Code.AccessorNoWrite, "The accessor `{0}` does not define a write implementation and cannot be assigned" },

            { Code.MethodNotFound, "The method `{0}` is not defined on the type `{1}`" },
            { Code.MethodRequiresInstance, "The method `{0}` must be invoked via an instance" },
            { Code.MethodRequiresType, "The method `{0}` is marked as global must be invoked via a type qualifier" },
            { Code.MethodNoMatch, "No method found matching or inferable from the provided arguments" },
            { Code.MethodAmbiguousMatch, "Multiple ambiguous methods found matching or inferable from the provided arguments" },

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
