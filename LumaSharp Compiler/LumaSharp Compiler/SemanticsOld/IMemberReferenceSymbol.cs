
namespace LumaSharp.Compiler.SemanticsOld
{
    public interface IMemberReferenceSymbol : IIdentifierReferenceSymbol
    {
        // Properties
        bool IsStatic { get; }

        ITypeReferenceSymbol DeclaringTypeSymbol { get; }
    }
}
