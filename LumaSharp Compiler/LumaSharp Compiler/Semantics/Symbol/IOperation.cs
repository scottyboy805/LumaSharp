using System;
using System.Collections.Generic;
using System.Text;

namespace LumaSharp.Compiler.Semantics.Symbol
{
    public enum OperationKind
    {
        Assign,
        BinaryAdd,
        BinarySubtract,
        BinaryMultiply,
        BinaryDivide,
        UnaryNegate,
    }

    public interface IOperation
    {
        // Properties
        OperationKind Kind { get; }

        // Methods
        ISymbol GetLeft();
        ISymbol GetRight();
        ISymbol GetSingle();
    }
}
