using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class ImportAliasSyntax : ImportSyntax
    {
        // Private
        private readonly SyntaxToken identifier;
        private readonly SyntaxToken asKeyword;
        private readonly TypeReferenceSyntax asType;

        // Properties
        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public SyntaxToken As
        {
            get { return asKeyword; }
        }

        public TypeReferenceSyntax Type
        {
            get { return asType; }
        }

        // Constructor
        internal ImportAliasSyntax(SyntaxToken identifier, TypeReferenceSyntax asType)
            : this(
                  Syntax.Token(SyntaxTokenKind.ImportKeyword),
                  identifier,
                  Syntax.Token(SyntaxTokenKind.AsKeyword),
                  asType,
                  Syntax.Token(SyntaxTokenKind.SemicolonSymbol))
        {
        }

        internal ImportAliasSyntax(SyntaxToken keyword, SyntaxToken identifier, SyntaxToken asKeyword, TypeReferenceSyntax asType, SyntaxToken semicolon)
            : base(keyword, asType?.Namespace ?? new(SyntaxTokenKind.ColonSymbol, (IEnumerable<SyntaxToken>)null, SyntaxTokenKind.Identifier), semicolon)
        {
            // Check kind
            if (identifier.Kind != SyntaxTokenKind.Identifier)
                throw new ArgumentException(nameof(identifier) + " must be of kind: " + SyntaxTokenKind.Identifier);

            if(asKeyword.Kind != SyntaxTokenKind.AsKeyword)
                throw new ArgumentException(nameof(asKeyword) + " must be of kind: " + SyntaxTokenKind.AsKeyword);

            // Check null
            if(asType == null)
                throw new ArgumentNullException(nameof(asType));

            this.identifier = identifier;
            this.asKeyword = asKeyword;
            this.asType = asType;

            // Set parent
            asType.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitImpotAlias(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitImpotAlias(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            Keyword.GetSourceText(writer);

            // Alias
            identifier.GetSourceText(writer);

            // As
            asKeyword.GetSourceText(writer);

            // Type - this will include the namespace
            asType.GetSourceText(writer);

            // Semicolon
            Semicolon.GetSourceText(writer);
        }
    }
}
