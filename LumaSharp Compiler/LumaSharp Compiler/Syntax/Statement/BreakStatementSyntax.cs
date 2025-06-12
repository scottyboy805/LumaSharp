
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class BreakStatementSyntax : StatementSyntax
    {
        // Private
        private readonly SyntaxToken keyword;
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

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal BreakStatementSyntax()
            : this(
                  Syntax.Token(SyntaxTokenKind.BreakKeyword),
                  Syntax.Token(SyntaxTokenKind.SemicolonSymbol))
        {
        }

        internal BreakStatementSyntax(SyntaxToken keyword, SyntaxToken semicolon)
        {
            // Check kind
            if(keyword.Kind != SyntaxTokenKind.BreakKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.BreakKeyword);

            // Check kind
            if (semicolon.Kind != SyntaxTokenKind.SemicolonSymbol)
                throw new ArgumentException(nameof(semicolon) + " must be of kind: " + SyntaxTokenKind.SemicolonSymbol);

            this.keyword = keyword;
            this.semicolon = semicolon;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitBreakStatement(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitBreakStatement(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            keyword.GetSourceText(writer);
            semicolon.GetSourceText(writer);
        }
    }
}
