using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Semantics
{
    public interface ISymbol
    {
        // Properties
        string Name { get; }

        SyntaxSpan? Source { get; }
    }
}
