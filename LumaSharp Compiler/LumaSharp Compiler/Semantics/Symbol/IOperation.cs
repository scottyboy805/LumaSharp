
namespace LumaSharp.Compiler.Semantics
{
    public enum OperationKind
    {
        BinaryOr,
        BinaryAnd,

        // Math
        BinaryAdd,
        BinarySubtract,
        BinaryMultiply,
        BinaryDivide,
        BinaryModulus,
        BinaryBitwiseAnd,
        BinaryBitwiseOr,
        BinaryBitwiseXOr,
        BinaryBitshiftLeft,
        BinaryBitshiftRight,
        

        // Conditional
        BinaryEqual,
        BinaryNotEqual,
        BinaryLess,
        BinaryLessEqual,
        BinaryGreater,
        BinaryGreaterEqual,

        // Unary
        UnaryNegate,
        UnaryPostIncrement,
        UnaryPostDecrement,
        UnaryPreIncrement,
        UnaryPreDecrement,
    }

    public interface IOperation : ISymbol
    {
        // Properties
        OperationKind Kind { get; }
        bool IsBinary { get; }
        bool IsUnary { get; }

        // Methods
        ITypeSymbol GetOperationType();
        ISymbol GetLeft();
        ISymbol GetRight();
        ISymbol GetSingle();
    }
}
