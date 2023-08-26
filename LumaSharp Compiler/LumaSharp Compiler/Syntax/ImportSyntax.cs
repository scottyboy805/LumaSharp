
namespace LumaSharp_Compiler.Syntax
{
    public sealed class ImportSyntax : SyntaxNode
    {
        // Private
        private SyntaxToken keyword = null;
        private NamespaceName name = null;
        private SyntaxToken aliasIdentifier = null;
        private TypeReferenceSyntax aliasTypeReference = null;
        private SyntaxToken end = null;

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

        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get { return end; }
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
        internal ImportSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ImportElementContext import)
            : base(tree, parent)
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
            }
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write text
        }        
    }
}
