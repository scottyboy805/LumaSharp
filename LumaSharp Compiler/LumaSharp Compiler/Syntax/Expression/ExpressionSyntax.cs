
namespace LumaSharp_Compiler.Syntax
{
    public abstract class ExpressionSyntax : SyntaxNode
    {
        // Properties
        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        protected ExpressionSyntax(SyntaxTree tree, SyntaxNode node)
            : base(tree, node) 
        {
        }
    }
}
