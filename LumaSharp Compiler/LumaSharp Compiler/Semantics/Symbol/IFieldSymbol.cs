
namespace LumaSharp.Compiler.Semantics
{
    public interface IFieldSymbol : IMemberSymbol
    {
        // Properties
        bool IsConst { get; }
        bool IsReadOnly { get; }
        bool IsAssigned { get; }

        // Methods
        ITypeSymbol GetFieldType();
    }
}
