
namespace LumaSharp_Compiler.Semantics
{
    public interface IFieldReferenceSymbol : IIdentifierReferenceSymbol
    {
        // Properties
        string FieldName { get; }

        ITypeReferenceSymbol DeclaringTypeSymbol { get; }
    }
}
