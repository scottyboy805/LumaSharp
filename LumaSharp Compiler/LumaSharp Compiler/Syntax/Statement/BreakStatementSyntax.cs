
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class BreakStatementSyntax : StatementSyntax
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly SyntaxToken? label;
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

        public SyntaxToken? Label
        {
            get { return label; }
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
        internal BreakStatementSyntax(SyntaxToken keyword, SyntaxToken? label, SyntaxToken semicolon)
        {
            // Check kind
            if(keyword.Kind != SyntaxTokenKind.BreakKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.BreakKeyword);

            // Check kind
            if (label.HasValue == true && label.Value.Kind != SyntaxTokenKind.Identifier)
                throw new ArgumentException(nameof(label) + " must be of kind: " + SyntaxTokenKind.Identifier);

            // Check kind
            if (semicolon.Kind != SyntaxTokenKind.SemicolonSymbol)
                throw new ArgumentException(nameof(semicolon) + " must be of kind: " + SyntaxTokenKind.SemicolonSymbol);

            this.keyword = keyword;
            this.label = label;
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

    public static partial class Syntax
    {
        // Methods
        public static BreakStatementSyntax Break(string label = null) =>
            new(Token(SyntaxTokenKind.BreakKeyword), label != null ? Identifier(label) : null, Token(SyntaxTokenKind.SemicolonSymbol));

        public static BreakStatementSyntax Break(SyntaxToken keyword, SyntaxToken semicolon, string label = null) =>
            new(keyword, label != null ? Identifier(label) : null, semicolon);
    }
}
