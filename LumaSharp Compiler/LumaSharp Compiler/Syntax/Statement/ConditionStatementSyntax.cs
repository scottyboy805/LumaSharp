
namespace LumaSharp.Compiler.AST
{
    public sealed class ConditionStatementSyntax : StatementSyntax
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly ExpressionSyntax condition;
        private readonly StatementSyntax inlineStatement;
        private readonly BlockSyntax<StatementSyntax> blockStatement;

        private readonly ConditionStatementSyntax alternate;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                // Check for alternate
                if (HasAlternate == true)
                    return alternate.EndToken;

                // Check for block
                if (HasBlockStatement == true)
                    return blockStatement.EndToken;

                // Check for condition
                if(HasCondition == true)
                    return condition.EndToken;

                return keyword;
            }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }        

        public ExpressionSyntax Condition
        {
            get { return condition; }
        }

        public StatementSyntax InlineStatement
        {
            get { return inlineStatement; }
        }

        public BlockSyntax<StatementSyntax> BlockStatement
        {
            get { return blockStatement; }
        }

        public ConditionStatementSyntax Alternate
        {
            get { return alternate; }
        }

        public bool HasCondition
        {
            get { return condition != null; }
        }

        public bool HasInlineStatement
        {
            get { return inlineStatement != null; }
        }

        public bool HasBlockStatement
        {
            get { return blockStatement != null; }
        }

        public bool IsAlternate
        {
            get { return keyword.Kind != SyntaxTokenKind.IfKeyword; }
        }

        public bool HasAlternate
        {
            get { return alternate != null; }
        }

        // Constructor
        internal ConditionStatementSyntax(SyntaxNode parent, ExpressionSyntax condition, bool isAlternate, ConditionStatementSyntax alternate, BlockSyntax<StatementSyntax> body, StatementSyntax inlineStatement)
            : base(parent)
        {
            if (condition != null)
            {
                this.keyword = isAlternate == false
                    ? Syntax.KeywordOrSymbol(SyntaxTokenKind.IfKeyword)
                    : Syntax.KeywordOrSymbol(SyntaxTokenKind.ElseifKeyword);
            }
            else
            {
                this.keyword = Syntax.KeywordOrSymbol(SyntaxTokenKind.ElseKeyword);
            }

            // Condition
            this.condition = condition;
            this.alternate = alternate;

            this.blockStatement = body;
            this.inlineStatement = inlineStatement;
        }

        internal ConditionStatementSyntax(SyntaxNode parent, LumaSharpParser.IfStatementContext condition)
            : base(parent)
        {
            // Keyword
            this.keyword = new SyntaxToken(SyntaxTokenKind.IfKeyword, condition.IF());

            // Condition
            this.condition = ExpressionSyntax.Any(this, condition.expression());

            // Statement inline
            if(condition.statement() != null)
            {
                this.inlineStatement = Any(this, condition.statement());
            }

            // Statement block
            if(condition.statementBlock() != null)
            {
                this.blockStatement = new BlockSyntax<StatementSyntax>(this, condition.statementBlock());
            }

            // Get alternate
            if(condition.elseifStatement() != null)
            {
                //alternate = new ConditionStatementSyntax(this, condition.elseifStatement());
            }
            if(condition.elseStatement() != null)
            {
                alternate = new ConditionStatementSyntax(this, condition.elseStatement());
            }
        }

        internal ConditionStatementSyntax(SyntaxNode parent, LumaSharpParser.ElseifStatementContext alternate)
            : base(parent)
        {
            // Keyword
            this.keyword = new SyntaxToken(SyntaxTokenKind.ElseifKeyword, alternate.ELSEIF());

            // Condition
            this.condition = ExpressionSyntax.Any(this, alternate.expression());

            // Statement inline
            if (alternate.statement() != null)
            {
                this.inlineStatement = Any(this, alternate.statement());
            }

            // Statement block
            if (alternate.statementBlock() != null)
            {
                this.blockStatement = new BlockSyntax<StatementSyntax>(this, alternate.statementBlock());
            }


            // Get alternate
        }

        internal ConditionStatementSyntax(SyntaxNode parent, LumaSharpParser.ElseStatementContext alternate)
            : base(parent)
        {
            // Keyword
            this.keyword = new SyntaxToken(SyntaxTokenKind.ElseKeyword, alternate.ELSE());

            // Statement inline
            if(alternate.statement() != null)
            {
                this.inlineStatement = Any(this, alternate.statement());
            }

            // Statement block
            if(alternate.statementBlock() != null)
            {
                this.blockStatement = new BlockSyntax<StatementSyntax>(this, alternate.statementBlock());
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
                // Condition
                condition.GetSourceText(writer);
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

            // Write alternate
            if(HasAlternate == true)
            {
                alternate.GetSourceText(writer);
            }
        }

        //internal void MakeAlternate()
        //{
        //    keyword = conditionExpression != null
        //        ? SyntaxToken.Elif()
        //        : SyntaxToken.Else()
        //            .WithTrailingWhitespace(" ");
        //}
    }
}
