
namespace LumaSharp.Compiler.SemanticsOld
{
    public interface IFieldReferenceSymbol : IMemberReferenceSymbol
    {
        // Properties
        ITypeReferenceSymbol FieldTypeSymbol { get; }
    }
}
