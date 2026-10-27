using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class WhileStatement : StatementSyntax
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly SyntaxToken lParen;
        private readonly SyntaxToken rParen;
        private readonly ExpressionSyntax expression;
        private readonly StatementSyntax statement;

        // Properties
        public override SyntaxToken StartToken => keyword;
        public override SyntaxToken EndToken => statement.EndToken;
        public SyntaxToken Keyword => keyword;
        public SyntaxToken LParen => lParen;
        public SyntaxToken RParen => rParen;
        public ExpressionSyntax Expression => expression;
        public StatementSyntax Statement => statement;

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                yield return expression;
                yield return statement;
            }
        }

        // Constructor
        internal WhileStatement(SyntaxToken keyword, SyntaxToken lParen, ExpressionSyntax expression, SyntaxToken rParen, StatementSyntax statement)
        {
            // Check null
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if(statement == null)
                throw new ArgumentNullException(nameof(statement));

            // Check kind
            if (keyword.Kind != SyntaxTokenKind.WhileKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.WhileKeyword);

            if (lParen.Kind != SyntaxTokenKind.LParenSymbol)
                throw new ArgumentException(nameof(lParen) + " must be of kind: " + SyntaxTokenKind.LParenSymbol);

            if (rParen.Kind != SyntaxTokenKind.RParenSymbol)
                throw new ArgumentException(nameof(rParen) + " must be of kind: " + SyntaxTokenKind.RParenSymbol);

            this.keyword = keyword;
            this.lParen = lParen;
            this.expression = expression;
            this.rParen = rParen;
            this.statement = statement;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitWhileStatement(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitWhileStatement(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            keyword.GetSourceText(writer);

            // LParen
            lParen.GetSourceText(writer);

            // Expression
            expression.GetSourceText(writer);

            // RParen
            rParen.GetSourceText(writer);

            // Statement
            statement.GetSourceText(writer);
        }
    }

    public static partial class Syntax
    {
        // Methods
        public static WhileStatement While(ExpressionSyntax condition, StatementSyntax statement = null)
            => new(Token(SyntaxTokenKind.WhileKeyword), Token(SyntaxTokenKind.LParenSymbol), condition, Token(SyntaxTokenKind.RParenSymbol), statement ?? new EmptyStatementSyntax());


        public static WhileStatement WithStatementBlock(this WhileStatement whileStatement, params StatementSyntax[] statements)
            => new(whileStatement.Keyword, whileStatement.LParen, whileStatement.Expression, whileStatement.RParen, new StatementBlockSyntax(statements));

        public static WhileStatement WithStatementBlock(this WhileStatement whileStatement, StatementBlockSyntax block)
            => new(whileStatement.Keyword, whileStatement.LParen, whileStatement.Expression, whileStatement.RParen, block);

        public static WhileStatement WithInlineStatement(this WhileStatement whileStatement, StatementSyntax statement)
            => new(whileStatement.Keyword, whileStatement.LParen, whileStatement.Expression, whileStatement.RParen, statement);

    }
}
