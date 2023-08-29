
namespace LumaSharp_Compiler.Syntax.Statement
{
    public sealed class ForeachStatementSyntax : StatementSyntax
    {
        // Private
        private SyntaxToken keyword = null;
        private TypeReferenceSyntax variableType = null;
        private SyntaxToken identifier = null;
        private ExpressionSyntax inExpression = null;
        private SyntaxToken lparen = null;
        private SyntaxToken rparen = null;
        private SyntaxToken inKeyword = null;
        private StatementSyntax inlineStatement = null;
        private BlockSyntax<StatementSyntax> blockStatement = null;
        private SyntaxToken semicolon = null;

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

        public TypeReferenceSyntax VariableType
        {
            get { return variableType; }
        }

        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public ExpressionSyntax InExpression
        {
            get { return inExpression; }
        }

        public SyntaxToken InKeyword
        {
            get { return inKeyword; }
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

        public bool HasInlineStatement
        {
            get { return inlineStatement != null; }
        }

        public bool HasBlockStatement
        {
            get { return blockStatement != null; }
        }

        // Constructor
        internal ForeachStatementSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ForeachStatementContext statement)
            : base(tree, parent, statement)
        {
            // Keyword
            this.keyword = new SyntaxToken(statement.FOREACH());

            // LR paren
            this.lparen = new SyntaxToken(statement.lparen);
            this.rparen = new SyntaxToken(statement.rparen);

            // Type
            this.variableType = new TypeReferenceSyntax(tree, this, statement.typeReference());

            // Identifier
            this.identifier = new SyntaxToken(statement.IDENTIFIER());

            // In
            this.inKeyword = new SyntaxToken(statement.IN());

            // In expression
            this.inExpression = ExpressionSyntax.Any(tree, this, statement.expression());

            // Statement inline
            if (statement.statement() != null)
            {
                this.inlineStatement = Any(tree, this, statement.statement());
            }

            // Statement block
            if (statement.statementBlock() != null)
            {
                this.blockStatement = new BlockSyntax<StatementSyntax>(tree, this, statement.statementBlock());
            }

            // Semi
            if (statement.semi != null)
                this.semicolon = new SyntaxToken(statement.semi);
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            writer.Write(keyword.Text);

            // LParen
            writer.Write(lparen.Text);

            // Type
            variableType.GetSourceText(writer);

            // Identifier
            writer.Write(identifier.Text);

            // In
            writer.Write(inKeyword.Text);

            // Expression
            inExpression.GetSourceText(writer);

            // RParen
            writer.Write(rparen.Text);


            // Check for inline
            if (HasInlineStatement == true)
            {
                inlineStatement.GetSourceText(writer);
            }
            // Check for block
            else if (HasBlockStatement == true)
            {
                BlockStatement.GetSourceText(writer);
            }
            // Fall back to empty statement
            else if (semicolon != null)
            {
                writer.Write(semicolon.Text);
            }
        }
    }
}
