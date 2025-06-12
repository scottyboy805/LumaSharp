
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class ReturnStatementSyntax : StatementSyntax
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly SeparatedSyntaxList<ExpressionSyntax> expressions;
        private readonly SyntaxToken semicolon;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get { return semicolon; }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public SyntaxToken Semicolon
        {
            get { return semicolon; }
        }

        public SeparatedSyntaxList<ExpressionSyntax> Expressions
        {
            get { return expressions; }
        }

        public bool HasExpressions
        {
            get { return expressions != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                if (HasExpressions == true)
                    yield return expressions;
            }
        }

        // Constructor
        internal ReturnStatementSyntax(SeparatedSyntaxList<ExpressionSyntax> expressions)
            : this(
                  Syntax.Token(SyntaxTokenKind.ReturnKeyword),
                  expressions,
                  Syntax.Token(SyntaxTokenKind.SemicolonSymbol))
        {
        }

        internal ReturnStatementSyntax(SyntaxToken keyword, SeparatedSyntaxList<ExpressionSyntax> expressions, SyntaxToken semicolon)
        {
            // Check kind
            if(keyword.Kind != SyntaxTokenKind.ReturnKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.ReturnKeyword);

            // Check kind
            if (semicolon.Kind != SyntaxTokenKind.SemicolonSymbol)
                throw new ArgumentException(nameof(semicolon) + " must be of kind: " + SyntaxTokenKind.SemicolonSymbol);


            this.keyword = keyword;
            this.expressions = expressions;
            this.semicolon = semicolon;

            // Set parent
            if(expressions != null) expressions.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitReturnStatement(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            keyword.GetSourceText(writer);

            // Return statement
            if(HasExpressions == true)
            {
                expressions.GetSourceText(writer);
            }

            // Semicolon
            semicolon.GetSourceText(writer);
        }
    }
}
