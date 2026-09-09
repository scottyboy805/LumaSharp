
namespace LumaSharp.Compiler.SemanticsOld
{
    public interface IIdentifierReferenceSymbol : IReferenceSymbol
    {
        // Properties
        ITypeReferenceSymbol TypeSymbol { get; }

        string IdentifierName { get; }
    }
}
