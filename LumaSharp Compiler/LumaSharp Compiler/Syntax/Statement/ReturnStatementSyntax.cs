
namespace LumaSharp.Compiler.AST
{
    public sealed class ReturnStatementSyntax : StatementSyntax
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly SeparatedListSyntax<ExpressionSyntax> expressions;

        // Properties
        public override SyntaxToken StartToken => keyword;
        public override SyntaxToken EndToken
        {
            get
            {
                // Check for returns
                if (HasExpressions == true)
                    return expressions.EndToken;

                return keyword;
            }
        }

        public SyntaxToken Keyword => keyword;
        public SeparatedListSyntax<ExpressionSyntax> Expressions => expressions;

        public bool HasExpressions
        {
            get { return expressions != null; }
        }

        // Constructor
        internal ReturnStatementSyntax(SeparatedListSyntax<ExpressionSyntax> expressions)
            : this(
                  new SyntaxToken(SyntaxTokenKind.ReturnKeyword),
                  expressions,
                  new SyntaxToken(SyntaxTokenKind.SemicolonSymbol))
        {
        }

        internal ReturnStatementSyntax(SyntaxToken keyword, SeparatedListSyntax<ExpressionSyntax> expressions, SyntaxToken semicolon)
            : base(semicolon)
        {
            // Check kind
            if(keyword.Kind != SyntaxTokenKind.ReturnKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.ReturnKeyword);

            // Check kind
            if (semicolon.Kind != SyntaxTokenKind.SemicolonSymbol)
                throw new ArgumentException(nameof(semicolon) + " must be of kind: " + SyntaxTokenKind.SemicolonSymbol);


            this.keyword = keyword;
            this.expressions = expressions;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            keyword.GetSourceText(writer);

            // Return statement
            if(HasExpressions == true)
            {
                expressions.GetSourceText(writer);
            }
        }
    }
}
