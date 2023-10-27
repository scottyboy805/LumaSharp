
namespace LumaSharp_Compiler.AST
{
    public sealed class ImportSyntax : SyntaxNode
    {
        // Private
        private SyntaxToken keyword = null;
        private NamespaceName name = null;
        private SyntaxToken aliasIdentifier = null;
        private TypeReferenceSyntax aliasTypeReference = null;
        private SyntaxToken asKeyword = null;
        private SyntaxToken dot = null;
        private SyntaxToken semicolon = null;

        // Properties
        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public NamespaceName Name
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

        public bool HasAlias
        {
            get { return aliasIdentifier != null; }
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
        internal ImportSyntax(string alias, TypeReferenceSyntax aliasType, string[] identifiers)
            : base(SyntaxToken.Import(), SyntaxToken.Semi())
        {
            this.keyword = base.StartToken.WithTrailingWhitespace(" ");
            this.aliasIdentifier = new SyntaxToken(alias);
            this.aliasTypeReference = aliasType;
            this.asKeyword = SyntaxToken.As()
                .WithLeadingWhitespace(" ")
                .WithTrailingWhitespace(" ");
            this.dot = SyntaxToken.Dot();
            this.name = new NamespaceName(identifiers);
            this.semicolon = base.EndToken;
        }

        internal ImportSyntax(string[] identifiers)
            : base(SyntaxToken.Import(), SyntaxToken.Semi())
        {
            this.keyword = base.StartToken.WithTrailingWhitespace(" ");
            this.name = new NamespaceName(identifiers);            
            this.semicolon = base.EndToken;
        }

        internal ImportSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ImportElementContext import)
            : base(tree, parent, import)
        {
            // Get import options
            LumaSharpParser.ImportStatementContext statement = import.importStatement();
            LumaSharpParser.ImportAliasContext alias = import.importAlias();

            // Check for statement
            if(statement != null)
            {
                // Get keyword
                this.keyword = new SyntaxToken(statement.IMPORT());

                // Get namespace
                this.name = new NamespaceName(tree, this, statement.namespaceName());

                // Create semi
                this.semicolon = new SyntaxToken(statement.semi);
            }
            else if(alias != null)
            {
                // Get keyword
                this.keyword = new SyntaxToken(alias.IMPORT());

                // Get alias
                this.aliasIdentifier = new SyntaxToken(alias.IDENTIFIER());

                // Get namespace
                this.name = new NamespaceName(tree, this, alias.namespaceName());
                
                // Get alias type
                this.aliasTypeReference = new TypeReferenceSyntax(tree, this, alias.typeReference());

                // Create semi
                this.semicolon = new SyntaxToken(alias.semi);
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

            // End statement
            semicolon.GetSourceText(writer);
        }        
    }
}
