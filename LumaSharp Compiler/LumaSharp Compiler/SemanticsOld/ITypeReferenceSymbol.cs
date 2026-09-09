using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.SemanticsOld
{
    public interface ITypeReferenceSymbol : IReferenceSymbol
    {
        // Properties
        string TypeName { get; }

        string[] NamespaceName { get; }

        ReservedType PrimitiveType { get; }

        bool IsPrimitive { get; }
        
        bool IsClass { get; }
        bool IsStruct { get; }

        bool IsInterface { get; }

        bool IsEnum { get; }

        INamespaceReferenceSymbol NamespaceSymbol { get; }

        ITypeReferenceSymbol DeclaringTypeSymbol { get; }

        IGenericParameterIdentifierReferenceSymbol[] GenericParameterSymbols { get; }

        ITypeReferenceSymbol[] BaseTypeSymbols { get; }

        ITypeReferenceSymbol[] TypeMemberSymbols { get; }

        IFieldReferenceSymbol[] FieldMemberSymbols { get; }

        IPropertyReferenceSymbol[] AccessorMemberSymbols { get; }

        IMethodReferenceSymbol[] MethodMemberSymbols { get; }

        IMethodReferenceSymbol[] OperatorMemberSymbols { get; }
    }
}
