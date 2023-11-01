
namespace LumaSharp_Compiler.Semantics
{
    public interface IFieldReferenceSymbol : IIdentifierReferenceSymbol
    {
        // Properties
        string FieldName { get; }

        bool IsGlobal { get; }

        ITypeReferenceSymbol DeclaringTypeSymbol { get; }
    }
}
