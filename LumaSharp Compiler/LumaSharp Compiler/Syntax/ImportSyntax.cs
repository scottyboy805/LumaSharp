
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
        internal ImportSyntax(SyntaxNode parent, string[] identifiers)
            : base(parent)
        {
            this.keyword = Syntax.KeywordOrSymbol(SyntaxTokenKind.ImportKeyword);
            this.name = new SeparatedTokenList(this, SyntaxTokenKind.ColonSymbol, SyntaxTokenKind.Identifier);

            // Add identifiers
            foreach (string identifier in identifiers)
                this.name.AddElement(Syntax.Identifier(identifier), Syntax.KeywordOrSymbol(SyntaxTokenKind.ColonSymbol));
        }

        internal ImportSyntax(SyntaxNode parent, string alias, TypeReferenceSyntax aliasType, string[] identifiers)
            : base(parent)
        {
            this.keyword = Syntax.KeywordOrSymbol(SyntaxTokenKind.ImportKeyword);
            this.aliasIdentifier = Syntax.Identifier(alias);
            this.asKeyword = Syntax.KeywordOrSymbol(SyntaxTokenKind.AsKeyword);
            this.dot = Syntax.KeywordOrSymbol(SyntaxTokenKind.DotSymbol);

            this.aliasTypeReference = aliasType;            
            this.name = new SeparatedTokenList(this, SyntaxTokenKind.ColonSymbol, SyntaxTokenKind.Identifier);

            // Add identifiers
            foreach (string identifier in identifiers)
                this.name.AddElement(Syntax.Identifier(identifier), Syntax.KeywordOrSymbol(SyntaxTokenKind.ColonSymbol));
        }

        internal ImportSyntax(SyntaxNode parent, LumaSharpParser.ImportElementContext import)
            : base(parent)
        {
            // Get import options
            LumaSharpParser.ImportStatementContext statement = import.importStatement();
            LumaSharpParser.ImportAliasContext alias = import.importAlias();

            // Check for statement
            if(statement != null)
            {
                // Get keyword
                this.keyword = new SyntaxToken(SyntaxTokenKind.ImportKeyword, statement.IMPORT());

                // Get namespace
                this.name = new SeparatedTokenList(this, statement.namespaceName());
            }
            else if(alias != null)
            {
                // Get keywords
                this.keyword = new SyntaxToken(SyntaxTokenKind.ImportKeyword, alias.IMPORT());
                this.asKeyword = new SyntaxToken(SyntaxTokenKind.AsKeyword, alias.AS());

                // Get alias
                this.aliasIdentifier = new SyntaxToken(SyntaxTokenKind.Identifier, alias.IDENTIFIER());

                // Get namespace
                this.name = new SeparatedTokenList(this, alias.namespaceName());
                
                // Get alias type
                this.aliasTypeReference = new TypeReferenceSyntax(this, null, alias.typeReference());
            }
        }

        // Methods
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
