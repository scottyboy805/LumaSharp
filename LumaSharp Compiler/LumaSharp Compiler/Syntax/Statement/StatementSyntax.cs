
namespace LumaSharp_Compiler.Syntax
{
    public abstract class StatementSyntax : SyntaxNode
    {
        // Protected
        protected SyntaxToken statementEnd = null;     // Semicolon

        // Properties
        public override SyntaxToken EndToken
        {
            get { return statementEnd; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal StatementSyntax(SyntaxTree tree, SyntaxNode parent, SyntaxToken statementEnd)
            : base(tree, parent)
        {
            this.statementEnd = statementEnd;
        }
    }
}
