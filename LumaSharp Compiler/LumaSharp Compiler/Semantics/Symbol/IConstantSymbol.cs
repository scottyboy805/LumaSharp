
namespace LumaSharp.Compiler.Semantics
{
    public enum ConstantKind
    {
        Null,
        Boolean,
        Integer,
        Decimal,
        String,
    }

    public interface IConstantSymbol : ISymbol
    {
        // Properties
        ConstantKind ConstantKind { get; }

        bool? BooleanValue { get; }
        ulong? IntegerValue { get; }
        double? DecimalValue { get; }
        string StringValue { get; }
    }
}
