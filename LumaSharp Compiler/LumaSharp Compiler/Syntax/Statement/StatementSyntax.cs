
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

        // Constructor
        internal StatementSyntax(SyntaxToken statementEnd)
        {
            this.statementEnd = statementEnd;
        }
    }
}
