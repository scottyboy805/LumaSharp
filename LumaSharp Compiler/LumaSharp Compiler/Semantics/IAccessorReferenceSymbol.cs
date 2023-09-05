
namespace LumaSharp_Compiler.Semantics
{
    public interface IAccessorReferenceSymbol : IReferenceSymbol
    {
        // Properties
        string AccessorName { get; }

        ITypeReferenceSymbol DeclaringTypeSymbol { get; }

        ITypeReferenceSymbol TypeSymbol { get; }

        bool HasReadBody { get; }

        bool HasWriteBody { get; }
    }
}
