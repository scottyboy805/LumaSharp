
namespace LumaSharp_Compiler.Semantics
{
    public interface IFieldReferenceSymbol : IReferenceSymbol
    {
        // Properties
        string FieldName { get; }

        ITypeReferenceSymbol DeclaringTypeSymbol { get; }

        ITypeReferenceSymbol FieldTypeSymbol { get; }
    }
}
