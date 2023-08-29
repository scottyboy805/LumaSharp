
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
                this.inlineStatement = Any(tree, this, condition.statement());
            }

            // Statement block
            if(condition.statementBlock() != null)
            {
                this.blockStatement = new BlockSyntax<StatementSyntax>(tree, this, condition.statementBlock());
            }

            // Semi
            if(condition.semi != null)
                this.semicolon = new SyntaxToken(condition.semi);

            // Get alternate
        }

        internal ConditionStatementSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ElseifStatementContext alternate)
            : base(tree, parent, alternate)
        {
            // Keyword
            this.keyword = new SyntaxToken(alternate.ELSEIF());

            // LR paren
            this.lparen = new SyntaxToken(alternate.lparen);
            this.rparen = new SyntaxToken(alternate.rparen);

            // Condition
            this.conditionExpression = ExpressionSyntax.Any(tree, this, alternate.expression());

            // Statement inline
            if (alternate.statement() != null)
            {
                this.inlineStatement = Any(tree, this, alternate.statement());
            }

            // Statement block
            if (alternate.statementBlock() != null)
            {
                this.blockStatement = new BlockSyntax<StatementSyntax>(tree, this, alternate.statementBlock());
            }


            // Get alternate
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            writer.Write(keyword.Text);

            // Check for condition
            if(HasCondition == true)
            {
                // Lparen
                writer.Write(lparen.Text);

                // Condition
                conditionExpression.GetSourceText(writer);

                // Rparen
                writer.Write(rparen.Text);
            }

            // Check for inline
            if(HasInlineStatement == true)
            {
                inlineStatement.GetSourceText(writer);
            }
            // Check for block
            else if(HasBlockStatement == true)
            {
                BlockStatement.GetSourceText(writer);
            }
            // Fall back to empty statement
            else if(semicolon != null)
            {
                writer.Write(semicolon.Text);
            }

            // Write alternate
            if(HasAlternate == true)
            {
                alternate.GetSourceText(writer);
            }
        }
    }
}
