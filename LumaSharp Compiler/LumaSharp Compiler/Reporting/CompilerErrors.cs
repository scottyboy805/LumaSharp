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
        ExpectedStatement = 105,

        InvalidConversion = 331,

        InvalidOperation = 401,
        NoBuiltInOperation = 402,
        NoOperation = 403,

        KeywordNotValid = 501,

        MultipleModifiers = 531,
        ModifierNotValid = 532,
        ModifierCantChange = 533,

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
        FieldRequiresAssign = 1065,
        FieldUsedLikeMethod = 1066,
        FieldMustBeStatic = 1067,
        FieldCannotBeAssigned = 1068,

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
        InvalidSelfBaseInterface = 1602,
        InvalidEnumBaseType = 1603,
        InvalidStructBaseType = 1604,
        InvalidEnumBaseInterface = 1605,
        InvalidStructBaseInterface = 1606,
        MultipleBaseTypes = 1607,
        FirstBaseType = 1608,

        InvalidPrimitiveGenericConstraint = 1701,

        InvalidNoGenericArgument = 1801,
        InvalidCountGenericArgument = 1802,
        InvalidConstraintGenericArgument = 1803,

        NoImplicitConversion = 1901,
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
            { Code.ExpectedStatement, "Expected statement" },

            // Syntax

            // Semantic
            { Code.InvalidConversion, "Cannot implicitly convert type `{0}` to `{1}`. Are you missing a cast?" },

            { Code.InvalidOperation, "Operation is not supported" },
            { Code.NoBuiltInOperation, "Cannot apply operation `{0}` to `{1}` and `{2}`" },
            { Code.NoOperation, "Cannot apply operation `{0}` to `{1}` and `{2}`. No suitable operator found" },

            { Code.KeywordNotValid, "Keyword `{0}` is not valid in this context" },

            { Code.MultipleModifiers, "Modifier `{0}` is specified multiple times" },
            { Code.ModifierNotValid, "Modifier `{0}` is not valid in this context" },
            { Code.ModifierCantChange, "Cannot change visibility modifier `{0}` when overriding" },

            { Code.NamespaceNotFound, "Namespace `{0}` could not be found. Are you missing a using statement?" },
            { Code.SubNamespaceNotFound, "Namespace name `{0}` could not be found in namespace `{1}`" },

            { Code.BuiltInTypeNotFound, "Built in type `{0}` could not be found. Are you missing a reference to `runtime`?" },
            { Code.TypeNotFound, "Type `{0}` could not be found. Are you missing a library reference or import statement?" },
            { Code.TypeNotFoundNamespace, "The type `{0}` does not exist in the namespace `{0}`" },
            { Code.TypeGenericPrimitive, "Cannot apply generic parameters to built in type `{0}`" },
            { Code.TypeArrayPrimitive, "Cannot apply array indexing to built in type `{0}`" },

            { Code.IdentifierNotFound, "Identifier `{0}` does not exist in the current context" },
            { Code.IdentifierUsedBeforeDeclared, "Identifier `{0}` cannot be used before it is declared" },
            { Code.MultipleLocalIdentifiers, "Local `{0}` is defined multiple times" },
            { Code.MultipleParameterIdentifiers, "Parameter `{0}` is defined multiple times" },

            { Code.FieldAccessorNotFound, "Field or accessor `{0}` is not defined on the type `{1}`" },
            { Code.FieldRequiresInstance, "Field `{0}` must be accessed via an instance" },
            { Code.FieldRequiresType, "Field `{0}` is marked as static and must be accessed via a type qualifier" },
            { Code.FieldRequiresAssign, "Const field `{0}` must be assigned a value" },
            { Code.FieldReadOnly, "Field `{0}` is marked as read only and cannot be assigned after initialization" },
            { Code.FieldMustBeStatic, "Cannot declare instance field `{0}` in a static class" },
            { Code.FieldCannotBeAssigned, "Field `{0}` cannot be initialized as a member of a struct" },

            { Code.AccessorRequiresInstance, "The accessor `{0}` must be accessed via an instance" },
            { Code.AccessorRequiresType, "The accessor `{0}` is marked as static must be accessed via a type qualifier" },
            { Code.AccessorNoRead, "The accessor `{0}` does not define a read implementation" },
            { Code.AccessorNoWrite, "The accessor `{0}` does not define a write implementation and cannot be assigned" },

            { Code.MethodNotFound, "Method `{0}` is not defined on the type `{1}`" },
            { Code.MethodRequiresInstance, "Method `{0}` must be invoked via an instance" },
            { Code.MethodRequiresType, "Method `{0}` is marked as static must be invoked via a type qualifier" },
            { Code.MethodNoMatch, "No method found matching or inferable from the provided arguments" },
            { Code.MethodAmbiguousMatch, "Multiple ambiguous methods found matching or inferable from the provided arguments" },

            { Code.OperatorMustBeGlobal, "Operator `{0}` must be declared as static" },
            { Code.OperatorIncorrectReturn, "Operator `{0}` must have the return type `{1}`" },
            { Code.OperatorIncorrectVoidParameter, "Operator `{0}` cannot accept any parameters" },
            { Code.OperatorIncorrectParameterCount, "Operator `{0}` does not have the required parameter count `{1}`" },
            { Code.OperatorIncorrectParameter, "Operator `{0}` does not have the correct parameter type. Expected `{1}`" },

            { Code.InvalidSelfBaseType, "Inheritance cycle detected for base type `{0}`" },
            { Code.InvalidSelfBaseInterface, "Inheritance cycle detected for base interface implementation `{0}`" },
            { Code.InvalidEnumBaseType, "Cannot derive from enum type `{0}`" },
            { Code.InvalidStructBaseType, "Cannot derive from struct type `{0}`" },
            { Code.InvalidEnumBaseInterface, "Enum type `{0}` cannot be used as an interface implementation" },
            { Code.InvalidStructBaseInterface, "Struct `{0}` cannot be used as an interface implementation" },
            { Code.MultipleBaseTypes, "Cannot have multiple base types" },
            { Code.FirstBaseType, "Base type `{0}` must come before any interface implementations" },

            { Code.InvalidPrimitiveGenericConstraint, "Primitive type `{0}` cannot be used as a generic constraint" },

            { Code.InvalidNoGenericArgument, "Type `{0}` does not accept any generic arguments" },
            { Code.InvalidCountGenericArgument, "Type `{0}` does not accept `{1}` generic arguments" },
            { Code.InvalidConstraintGenericArgument, "Generic argument `{0}` cannot be implicitly converted to constraint type `{1}`" },

            { Code.NoImplicitConversion, " Cannot implicitly convert `{0}` to `{1}`" },

            // Logical
        };

        // Methods
        public static string GetFormattedErrorMessage(int code, DiagnosticSeverity severity, string message, SyntaxSpan? source)
        {
            // Create formatted
            string formatted = string.Format("[{0}] LS{1}: {2}", severity.ToString().ToUpper(), code, message);

            // Check for source
            if(source != null)
            {
                // Create formatted source
                string formattedSource = string.Format(" in source file `{0}` at line `{1}`, column `{2}`", 
                    string.IsNullOrEmpty(source?.Document) == false ? source?.Document : "<unknown>", 
                    source?.Start.Line >= 0 ? source?.Start.Line : "<?>", 
                    source?.Start.Column >= 0 ? source?.Start.Column : "<?>");

                // Join strings
                formatted = string.Concat(formatted, formattedSource);
            }
            return formatted;
        }
    }
}
