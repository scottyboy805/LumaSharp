using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public class ImportSyntax : SyntaxNode
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly SeparatedTokenList name;
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

        public SeparatedTokenList Name
        {
            get { return name; }
        }

        public SyntaxToken Semicolon
        {
            get { return semicolon; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                // Get the name
                yield return name;
            }
        }

        // Constructor
        internal ImportSyntax(SeparatedTokenList importName)
            : this(
                  Syntax.Token(SyntaxTokenKind.ImportKeyword),
                  importName,
                  Syntax.Token(SyntaxTokenKind.SemicolonSymbol))
        {
        }

        internal ImportSyntax(SyntaxToken keyword, SeparatedTokenList importName, SyntaxToken semicolon)
        {
            // Check kind
            if(keyword.Kind != SyntaxTokenKind.ImportKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.ImportKeyword);

            if (semicolon.Kind != SyntaxTokenKind.SemicolonSymbol)
                throw new ArgumentException(nameof(semicolon) + " must be of kind: " + SyntaxTokenKind.SemicolonSymbol);

            // Check null
            if(importName == null)
                throw new ArgumentNullException(nameof(importName));

            this.keyword = keyword;
            this.name = importName;
            this.semicolon = semicolon;

            // Set parent
            importName.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitImport(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitImport(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            keyword.GetSourceText(writer);

            // Name
            name.GetSourceText(writer);

            // Semicolon
            semicolon.GetSourceText(writer);
        }        
    }
}
