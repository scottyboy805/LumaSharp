
namespace LumaSharp_Compiler.Semantics
{
    public interface ILocalIdentifierReferenceSymbol : IIdentifierReferenceSymbol
    {
        // Properties
        bool IsLocal { get; }

        bool IsParameter { get; }

        bool IsOptional { get; }

        int Index { get; }
    }
}
