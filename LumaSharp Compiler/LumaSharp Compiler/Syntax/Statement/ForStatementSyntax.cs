
namespace LumaSharp.Compiler.AST
{
    public sealed class ForStatementSyntax : StatementSyntax
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly VariableDeclarationStatementSyntax variable;
        private readonly ExpressionSyntax condition;
        private readonly SeparatedListSyntax<ExpressionSyntax> increments;

        private readonly StatementSyntax inlineStatement;
        private readonly BlockSyntax<StatementSyntax> blockStatement;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                // Check for block
                if (HasBlockStatement == true)
                    return blockStatement.EndToken;

                // Check for inline
                if(HasInlineStatement == true)
                    return inlineStatement.EndToken;

                return SyntaxToken.Invalid;
            }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public VariableDeclarationStatementSyntax Variable
        {
            get { return variable; }
        }

        public ExpressionSyntax Condition
        {
            get { return condition; }
        }

        public SeparatedListSyntax<ExpressionSyntax> Increments
        {
            get { return increments; }
        }

        public StatementSyntax InlineStatement
        {
            get { return inlineStatement; }
        }

        public BlockSyntax<StatementSyntax> BlockStatement
        {
            get { return blockStatement; }
        }

        public bool HasVariable
        { 
            get { return variable != null; }
        }

        public bool HasCondition
        {
            get { return condition != null; }
        }

        public bool HasIncrements
        {
            get { return increments != null; }
        }

        public bool HasInlineStatement
        {
            get { return inlineStatement != null; }
        }

        public bool HasBlockStatement
        {
            get { return blockStatement != null; }
        }

        // Constructor
        internal ForStatementSyntax(VariableDeclarationStatementSyntax variable, ExpressionSyntax condition, SeparatedListSyntax<ExpressionSyntax> increments, BlockSyntax<StatementSyntax> body, StatementSyntax inlineStatement)
            : this(
                  new SyntaxToken(SyntaxTokenKind.ForKeyword),
                  variable,
                  condition,
                  increments,
                  body,
                  inlineStatement)
        {
        }

        internal ForStatementSyntax(SyntaxToken keyword, VariableDeclarationStatementSyntax variable, ExpressionSyntax condition, SeparatedListSyntax<ExpressionSyntax> increments, BlockSyntax<StatementSyntax> body, StatementSyntax inlineStatement)
        {
            this.keyword = keyword;
            this.variable = variable;
            this.condition = condition;
            this.increments = increments;

            this.blockStatement = body;
            this.inlineStatement = inlineStatement;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            keyword.GetSourceText(writer);

            // Variable
            if(HasVariable == true)
                variable.GetSourceText(writer);

            // Foreach condition
            if(HasCondition == true)
                condition.GetSourceText(writer);

            // For expression
            if (HasIncrements == true) 
                increments.GetSourceText(writer);


            // Check for inline
            if(HasInlineStatement == true)
            {
                inlineStatement.GetSourceText(writer);
            }
            // Check for block
            else if(HasBlockStatement == true)
            {
                blockStatement.GetSourceText(writer);
            }
        }
    }
}
