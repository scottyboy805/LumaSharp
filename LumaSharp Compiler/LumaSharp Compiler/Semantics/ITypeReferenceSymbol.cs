using LumaSharp_Compiler.AST;

namespace LumaSharp_Compiler.Semantics
{
    public interface ITypeReferenceSymbol : IReferenceSymbol
    {
        // Properties
        string TypeName { get; }

        PrimitiveType PrimitiveType { get; }

        bool IsPrimitive { get; }
        
        bool IsType { get; }
        
        bool IsContract { get; }

        bool IsEnum { get; }

        ITypeReferenceSymbol DeclaringTypeSymbol { get; }

        IGenericParameterIdentifierReferenceSymbol[] GenericParameterSymbols { get; }

        ITypeReferenceSymbol[] BaseTypeSymbols { get; }

        IFieldReferenceSymbol[] FieldMemberSymbols { get; }

        IFieldReferenceSymbol[] AccessorMemberSymbols { get; }

        IMethodReferenceSymbol[] MethodMemberSymbols { get; }
    }
}
