
namespace LumaSharp_Compiler.Syntax.Statement
{
    public sealed class ConditionStatementSyntax : StatementSyntax
    {
        // Private
        private SyntaxToken keyword = null;
        private SyntaxToken lparen = null;
        private SyntaxToken rparen = null;
        private ExpressionSyntax conditionExpression = null;
        private StatementSyntax inlineStatement = null;
        private BlockSyntax<StatementSyntax> blockStatement = null;
        private SyntaxToken semicolon = null;

        private ConditionStatementSyntax alternate = null;

        // Properties
        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public SyntaxToken LParen
        {
            get { return lparen; }
        }

        public SyntaxToken RParen
        {
            get { return rparen; }
        }

        public ExpressionSyntax ConditionExpression
        {
            get { return conditionExpression; }
        }

        public StatementSyntax InlineStatement
        {
            get { return inlineStatement; }
        }

        public BlockSyntax<StatementSyntax> BlockStatement
        {
            get { return blockStatement; }
        }

        public SyntaxToken Semicolon
        {
            get { return semicolon; }
        }

        public ConditionStatementSyntax Alternate
        {
            get { return alternate; }
        }

        public bool HasCondition
        {
            get { return conditionExpression != null; }
        }

        public bool HasInlineStatement
        {
            get { return inlineStatement != null; }
        }

        public bool HasBlockStatement
        {
            get { return blockStatement != null; }
        }

        public bool HasAlternate
        {
            get { return alternate != null; }
        }

        // Constructor
        internal ConditionStatementSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.IfStatementContext condition)
            : base(tree, parent, condition)
        {
            // Keyword
            this.keyword = new SyntaxToken(condition.IF());

            // LR paren
            this.lparen = new SyntaxToken(condition.lparen);
            this.rparen = new SyntaxToken(condition.rparen);

            // Condition
            this.conditionExpression = ExpressionSyntax.Any(tree, this, condition.expression());

            // Statement inline
            if(condition.statement() != null)
            {
                //this.inlineStatement = condition.statement();
            }

            // Statement block
            if(condition.statementBlock() != null)
            {

            }

            // Semi
            if(condition.semi != null)
                this.semicolon = new SyntaxToken(condition.semi);
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
