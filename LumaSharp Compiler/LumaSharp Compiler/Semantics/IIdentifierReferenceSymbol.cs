
namespace LumaSharp.Compiler.Semantics
{
    public interface IIdentifierReferenceSymbol : IReferenceSymbol
    {
        // Properties
        ITypeReferenceSymbol TypeSymbol { get; }

        string IdentifierName { get; }
    }
}
