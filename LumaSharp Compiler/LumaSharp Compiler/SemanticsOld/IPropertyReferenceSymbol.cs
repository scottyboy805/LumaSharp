
namespace LumaSharp.Compiler.SemanticsOld
{
    public interface IPropertyReferenceSymbol : IMemberReferenceSymbol
    {
        // Properties
        IMemberReferenceSymbol Getter { get; }
        IMemberReferenceSymbol Setter { get; }
    }
}
