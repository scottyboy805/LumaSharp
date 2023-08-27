
namespace LumaSharp_Compiler.Semantics
{
    public interface IIdentifierReferenceSymbol
    {
        // Properties
        IReferenceSymbol ParentSymbol { get; }

        ITypeReferenceSymbol TypeSymbol { get; }

        string IdentifierName { get; }
    }
}
