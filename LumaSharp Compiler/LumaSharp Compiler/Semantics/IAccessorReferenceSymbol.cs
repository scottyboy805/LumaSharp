
namespace LumaSharp.Compiler.Semantics
{
    public interface IAccessorReferenceSymbol : IIdentifierReferenceSymbol
    {
        // Properties
        ITypeReferenceSymbol DeclaringTypeSymbol { get; }

        bool HasReadBody { get; }

        bool HasWriteBody { get; }
    }
}
