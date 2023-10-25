
namespace LumaSharp_Compiler.AST.Factory
{
    public interface ISyntaxFactory
    {
        // Properties
        SyntaxTree SyntaxTree { get; }

        SyntaxNode Parent { get; }
    }
}
