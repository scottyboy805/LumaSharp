using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Reporting
{
    // Type
    public enum Code
    {
        UnexpectedToken = 100,
        ExpectedToken = 101,
        ExpectedIdentifier = 102,
        ExpectedType = 103,
        ExpectedExpression = 104,
        ExprectedStatement = 105,

        InvalidConversion = 331,

        InvalidOperation = 401,
        NoBuiltInOperation = 402,
        NoOperation = 403,

        KeywordNotValid = 501,

        MultipleAccessModifiers = 531,
        AccessModifierNotValid = 532,
        AccessModifierCantChange = 533,

        NamespaceNotFound = 971,
        SubNamespaceNotFound = 972,

        BuiltInTypeNotFound = 1001,
        TypeNotFound = 1002,
        TypeNotFoundNamespace = 1003,
        TypeGenericPrimitive = 1004,
        TypeArrayPrimitive = 1005,

        IdentifierNotFound = 1031,
        IdentifierUsedBeforeDeclared = 1032,
        MultipleLocalIdentifiers = 1033,
        MultipleParameterIdentifiers = 1034,

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

        OperatorMustBeGlobal = 1401,
        OperatorIncorrectReturn = 1402,
        OperatorIncorrectVoidParameter = 1403,
        OperatorIncorrectParameterCount = 1404,
        OperatorIncorrectParameter = 1405,

        InvalidSelfBaseType = 1601,
        InvalidSelfBaseContract = 1602,
        InvalidEnumBaseType = 1603,
        InvalidEnumBaseContract = 1604,
        InvalidTypeBaseContract = 1605,
        MultipleBaseTypes = 1606,
        FirstBaseType = 1607,

        InvalidPrimitiveGenericConstraint = 1701,

        InvalidNoGenericArgument = 1801,
        InvalidCountGenericArgument = 1802,
        InvalidConstraintGenericArgument = 1803,
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
            { Code.UnexpectedToken, "Unexpected '{0}'" },
            { Code.ExpectedToken, "Expected '{0}'" },
            { Code.ExpectedIdentifier, "Expected identifier" },
            { Code.ExpectedType, "Expected type" },
            { Code.ExpectedExpression, "Expected expression" },
            { Code.ExprectedStatement, "Expected statement" },

            // Syntax

            // Semantic
            { Code.InvalidConversion, "Cannot implicitly convert type `{0}` to `{1}`. Are you missing a cast?" },

            { Code.InvalidOperation, "Operation is not supported" },
            { Code.NoBuiltInOperation, "Cannot apply operation `{0}` to `{1}` and `{2}`" },
            { Code.NoOperation, "Cannot apply operation `{0}` to `{1}` and `{2}`. No suitable operator found" },

            { Code.KeywordNotValid, "Keyword `{0}` is not valid in the current context" },

            { Code.MultipleAccessModifiers, "Multiple access modifiers" },
            { Code.AccessModifierNotValid, "Access modifier `{0}` is not valid in the current context" },
            { Code.AccessModifierCantChange, "Cannot change access modifier `{0}` when overriding" },

            { Code.NamespaceNotFound, "The namespace `{0}` could not be found. Are you missing a library reference?" },
            { Code.SubNamespaceNotFound, "The namespace name `{0}` could not be found in namespace `{1}`. Are you missing a library reference?" },

            { Code.BuiltInTypeNotFound, "The built in type `{0}` could not be found. Are you missing a reference to `runtime`?" },
            { Code.TypeNotFound, "The type `{0}` could not be found. Are you missing a library reference or import statement?" },
            { Code.TypeNotFoundNamespace, "The type `{0}` does not exist in the namespace `{0}`" },
            { Code.TypeGenericPrimitive, "Cannot apply generic parameters to built in type `{0}`" },
            { Code.TypeArrayPrimitive, "Cannot apply array indexing to built in type `{0}`" },

            { Code.IdentifierNotFound, "The identifier `{0}` does not exist in the current context" },
            { Code.IdentifierUsedBeforeDeclared, "The identifier `{0}` cannot be used before it is declared" },
            { Code.MultipleLocalIdentifiers, "The local identifier `{0}` is defined multiple times" },
            { Code.MultipleParameterIdentifiers, "The parameter identifier `{0}` is defined multiple times" },

            { Code.FieldAccessorNotFound, "The field or accessor `{0}` is not defined on the type `{1}`" },
            { Code.FieldRequiresInstance, "The field `{0}` must be accessed via an instance" },
            { Code.FieldRequiresType, "The field `{0}` is marked as global and must be accessed via a type qualifier" },
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

            { Code.OperatorMustBeGlobal, "Operator `{0}` must be declared as global" },
            { Code.OperatorIncorrectReturn, "Operator `{0}` must have the return type `{1}`" },
            { Code.OperatorIncorrectVoidParameter, "Operator `{0}` cannot accept any parameters" },
            { Code.OperatorIncorrectParameterCount, "Operator `{0}` does not have the required parameter count `{1}`" },
            { Code.OperatorIncorrectParameter, "Operator `{0}` does not have the correct parameter type. Expected `{1}`" },

            { Code.InvalidSelfBaseType, "Inheritance cycle detected for base type `{0}`" },
            { Code.InvalidSelfBaseContract, "Inheritance cycle detected for base contract implementation `{0}`" },
            { Code.InvalidEnumBaseType, "Enum type `{0}` cannot be used as a base type" },
            { Code.InvalidEnumBaseContract, "Enum type `{0}` cannot be used as a base contract implementation" },
            { Code.InvalidTypeBaseContract, "Type `{0}` cannot be used as a base contract implementation" },
            { Code.MultipleBaseTypes, "Cannot have multiple base types `{0}` and `{1}`" },
            { Code.FirstBaseType, "Base type `{0}` must come before any contract implementations" },

            { Code.InvalidPrimitiveGenericConstraint, "Primitive type `{0}` cannot be used as a generic constraint" },

            { Code.InvalidNoGenericArgument, "Type `{0}` does not accept any generic arguments" },
            { Code.InvalidCountGenericArgument, "Type `{0}` does not accept `{1}` generic arguments" },
            { Code.InvalidConstraintGenericArgument, "Generic argument `{0}` cannot be implicitly converted to constraint type `{1}`" },

            // Logical
        };

        // Methods
        public static string GetFormattedErrorMessage(int code, MessageSeverity severity, string message, SyntaxSource source)
        {
            // Create formatted
            string formatted = string.Format("[{0}] LS{1}: {2}", severity.ToString().ToUpper(), code, message);

            // Check for source
            if(source.SourceName != null)
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
