using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class ImportSyntax : SyntaxNode
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly SeparatedTokenList name;
        private readonly SyntaxToken aliasIdentifier;
        private readonly TypeReferenceSyntax aliasTypeReference;
        private readonly SyntaxToken asKeyword;
        private readonly SyntaxToken dot;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                // Check for alias
                if (HasAlias == true)
                    return aliasTypeReference.EndToken;

                return name.EndToken;
            }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public SeparatedTokenList Name
        {
            get { return name; }
        }

        public SyntaxToken AliasIdentifier
        {
            get { return aliasIdentifier; }
        }

        public TypeReferenceSyntax AliasTypeReference
        {
            get { return aliasTypeReference; }
        }

        public SyntaxToken As
        {
            get { return asKeyword; }
        }

        public SyntaxToken Dot
        {
            get { return dot; }
        }

        public bool HasAlias
        {
            get { return aliasIdentifier.Kind != SyntaxTokenKind.Invalid; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                // Get the name
                yield return name;

                // Check for alias
                if (HasAlias == true)
                    yield return aliasTypeReference;
            }
        }

        // Constructor
        internal ImportSyntax(SeparatedTokenList importName)
            : this(
                  Syntax.Token(SyntaxTokenKind.ImportKeyword),
                  importName)
        {
        }

        internal ImportSyntax(SyntaxToken keyword, SeparatedTokenList importName)
        {
            // Check kind
            if(keyword.Kind != SyntaxTokenKind.ImportKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.ImportKeyword);

            // Check null
            if(importName == null)
                throw new ArgumentNullException(nameof(importName));

            this.keyword = keyword;
            this.name = importName;
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
            // Write text
            keyword.GetSourceText(writer);

            // Check for alias
            if(HasAlias == true)
            {
                // Write alias name
                aliasIdentifier.GetSourceText(writer);

                // Write as
                asKeyword.GetSourceText(writer);

                // Write namespace name
                name.GetSourceText(writer);

                // Write dot
                dot.GetSourceText(writer);

                // Write alias type
                aliasTypeReference.GetSourceText(writer);
            }
            else
            {
                // Get namespace name
                name.GetSourceText(writer);
            }
        }        
    }
}
