
using Antlr4.Runtime;

namespace LumaSharp_Compiler.AST
{
    public abstract class StatementSyntax : SyntaxNode
    {
        // Protected
        protected SyntaxToken statementEnd = null;     // Semicolon or block

        // Properties
        public SyntaxToken StatementEnd
        {
            get { return statementEnd; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal StatementSyntax(SyntaxToken token)
            : base(token)
        {
        }

        internal StatementSyntax(SyntaxTree tree, SyntaxNode parent, ParserRuleContext context)
            : base(tree, parent, context)
        {
            this.statementEnd = new SyntaxToken(context.Stop);
        }

        // Methods
        public static StatementSyntax Any(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.StatementContext statement)
        {
            return null;
        }
    }
}
