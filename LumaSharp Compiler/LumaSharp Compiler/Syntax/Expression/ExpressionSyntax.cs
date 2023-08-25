
namespace LumaSharp_Compiler.Syntax
{
    public abstract class ExpressionSyntax : SyntaxNode
    {
        // Constructor
        protected ExpressionSyntax(SyntaxTree tree, SyntaxNode node)
            : base(tree, node) 
        {
        }
    }
}
