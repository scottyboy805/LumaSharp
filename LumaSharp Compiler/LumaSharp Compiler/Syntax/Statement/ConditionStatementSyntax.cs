
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
        internal ConditionStatementSyntax(ExpressionSyntax condition, bool isAlternate, ConditionStatementSyntax alternate, BlockSyntax<StatementSyntax> body, StatementSyntax inlineStatement)
        {
            if (condition != null)
            {
                //this.keyword = isAlternate == false
                //    ? Syntax.KeywordOrSymbol(SyntaxTokenKind.IfKeyword)
                //    : Syntax.KeywordOrSymbol(SyntaxTokenKind.ElseifKeyword);
            }
            else
            {
                this.keyword = Syntax.Token(SyntaxTokenKind.ElseKeyword);
            }

            // Condition
            this.condition = condition;
            this.alternate = alternate;

            this.blockStatement = body;
            this.inlineStatement = inlineStatement;
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
