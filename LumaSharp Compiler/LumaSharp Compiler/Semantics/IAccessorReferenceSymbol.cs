
namespace LumaSharp_Compiler.Semantics
{
    public interface IAccessorReferenceSymbol : IIdentifierReferenceSymbol
    {
        // Properties
        string AccessorName { get; }

        ITypeReferenceSymbol DeclaringTypeSymbol { get; }

        bool HasReadBody { get; }

        bool HasWriteBody { get; }
    }
}
