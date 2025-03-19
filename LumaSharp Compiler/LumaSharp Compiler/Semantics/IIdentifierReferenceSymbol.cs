
namespace LumaSharp.Compiler.Semantics
{
    public interface IIdentifierReferenceSymbol : IReferenceSymbol
    {
        // Properties
        IReferenceSymbol ParentSymbol { get; }

        ITypeReferenceSymbol TypeSymbol { get; }

        string IdentifierName { get; }
    }
}
