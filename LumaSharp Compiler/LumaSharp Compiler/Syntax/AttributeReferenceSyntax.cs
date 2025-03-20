
namespace LumaSharp.Compiler.AST
{
    public sealed class AttributeReferenceSyntax : SyntaxNode
    {
        // Private
        private readonly SyntaxToken hash;
        private readonly TypeReferenceSyntax attributeType;
        private readonly ArgumentListSyntax argumentList;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return hash; }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                // Get arg end token
                if (HasArgumentList == true)
                    return argumentList.EndToken;

                // Get type end token
                return attributeType.EndToken;
            }
        }

        public SyntaxToken Hash
        {
            get { return hash; }
        }

        public TypeReferenceSyntax AttributeType
        {
            get { return attributeType; }
        }

        public ArgumentListSyntax ArgumentList
        {
            get { return argumentList; }
        }

        public bool HasArgumentList
        {
            get { return argumentList != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                // Type
                yield return attributeType;

                // Expressions
                if(HasArgumentList == true)
                {
                    foreach (SyntaxNode argument in argumentList.Descendants)
                        yield return argument;
                }
            }
        }

        // Constructor
        internal AttributeReferenceSyntax(SyntaxNode parent, TypeReferenceSyntax attributeType, ArgumentListSyntax argumentList)
            : base(parent)
        {
            this.hash = Syntax.KeywordOrSymbol(SyntaxTokenKind.HashSymbol);
            this.attributeType = attributeType;
            this.argumentList = argumentList;
        }

        internal AttributeReferenceSyntax(SyntaxNode parent, LumaSharpParser.AttributeReferenceContext attribute)
            : base(parent)
        {
            // Get hash
            this.hash = new SyntaxToken(SyntaxTokenKind.HashSymbol, attribute.HASH());

            // Type reference
            this.attributeType = new TypeReferenceSyntax(this, null, attribute.typeReference());

            // Argument list
            if(attribute.argumentList() != null)
                this.argumentList = new ArgumentListSyntax(this, attribute.argumentList());
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write hash
            hash.GetSourceText(writer);

            // Write type
            attributeType.GetSourceText(writer);

            // Check for argument list
            if(HasArgumentList == true)
                argumentList.GetSourceText(writer);
        }
    }
}
