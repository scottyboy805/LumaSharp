
namespace LumaSharp_Compiler.AST.Statement
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

        private bool isAlternate = false;
        private ConditionStatementSyntax alternate = null;

        // Properties
        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                if(HasAlternate == true)
                {
                    return alternate.EndToken;
                }
                else if(HasBlockStatement == true)
                {
                    return blockStatement.EndToken;
                }
                return base.EndToken;
            }
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
            internal set { inlineStatement = value; }
        }

        public BlockSyntax<StatementSyntax> BlockStatement
        {
            get { return blockStatement; }
            internal set { blockStatement = value; }
        }

        public SyntaxToken Semicolon
        {
            get { return statementEnd; }
        }

        public ConditionStatementSyntax Alternate
        {
            get { return alternate; }
            internal set { alternate = value; }
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
        internal ConditionStatementSyntax(ExpressionSyntax condition)
            : base(SyntaxToken.If())
        {
            this.keyword = base.StartToken;
            this.lparen = SyntaxToken.LParen();
            this.rparen = SyntaxToken.RParen();

            // Condition
            this.conditionExpression = condition;
        }

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
                this.statementEnd = new SyntaxToken(condition.semi);

            // Get alternate
            if(condition.elseifStatement() != null)
            {

            }
            if(condition.elseStatement() != null)
            {
                alternate = new ConditionStatementSyntax(tree, this, condition.elseStatement());
            }
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

        internal ConditionStatementSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ElseStatementContext alternate)
            : base(tree, parent, alternate)
        {
            // Keyword
            this.keyword = new SyntaxToken(alternate.ELSE());

            // LR paren
            //this.lparen = new SyntaxToken(alternate.lp)

            // Statement inline
            if(alternate.statement() != null)
            {
                this.inlineStatement = Any(tree, this, alternate.statement());
            }

            // Statement block
            if(alternate.statementBlock() != null)
            {
                this.blockStatement = new BlockSyntax<StatementSyntax>(tree, this, alternate.statementBlock());
            }
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            keyword.GetSourceText(writer);

            // Check for condition
            if(HasCondition == true)
            {
                // Lparen
                lparen.GetSourceText(writer);

                // Condition
                conditionExpression.GetSourceText(writer);

                // Rparen
                rparen.GetSourceText(writer);
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
            else if(statementEnd != null)
            {
                statementEnd.GetSourceText(writer);
            }

            // Write alternate
            if(HasAlternate == true)
            {
                alternate.GetSourceText(writer);
            }
        }

        internal void MakeAlternate()
        {
            isAlternate = true;
            keyword = conditionExpression != null
                ? SyntaxToken.Elif()
                : SyntaxToken.Else()
                    .WithTrailingWhitespace(" ");
        }
    }
}
