
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
        internal ForStatementSyntax(SyntaxNode parent, VariableDeclarationStatementSyntax variable, ExpressionSyntax condition, SeparatedListSyntax<ExpressionSyntax> increments, BlockSyntax<StatementSyntax> body, StatementSyntax inlineStatement)
            : base(parent)
        {
            this.keyword = Syntax.KeywordOrSymbol(SyntaxTokenKind.ForKeyword);
            this.variable = variable;
            this.condition = condition;
            this.increments = increments;

            this.blockStatement = body;
            this.inlineStatement = inlineStatement;
        }

        internal ForStatementSyntax(SyntaxNode parent, LumaSharpParser.ForStatementContext forStatement)
            : base(parent)
        {
            // Keyword
            this.keyword = new SyntaxToken(SyntaxTokenKind.ForKeyword, forStatement.FOR());

            // For variable
            if (forStatement.localVariableStatement() != null)
                this.variable = new VariableDeclarationStatementSyntax(this, forStatement.localVariableStatement());

            // For condition
            if (forStatement.expression() != null)
                this.condition = ExpressionSyntax.Any(this, forStatement.expression());

            // For increments
            if(forStatement.expressionList() != null)
                this.increments = ExpressionSyntax.List(this, forStatement.expressionList());    
                        

            // Statement inline
            if(forStatement.statement() != null)
            {
                this.inlineStatement = Any(this, forStatement.statement());
            }

            // Statement block
            if(forStatement.statementBlock() != null)
            {
                this.blockStatement = new BlockSyntax<StatementSyntax>(this, forStatement.statementBlock());
            }
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
