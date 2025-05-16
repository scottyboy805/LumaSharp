
namespace LumaSharp.Compiler.AST
{
    public sealed class EnumSyntax : MemberSyntax, IMemberSyntaxContainer
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly TypeReferenceSyntax underlyingTypeReference;
        private readonly SyntaxToken colon;

        private readonly BlockSyntax<FieldSyntax> fieldBlock;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get { return fieldBlock.EndToken; }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public SyntaxToken Colon
        {
            get { return colon; }
        }

        public SeparatedTokenList Namespace
        {
            get
            {
                SyntaxNode current = Parent;

                // Move up until end or namespace
                while (current != null && (current is NamespaceSyntax) == false)
                    current = current.Parent;

                // Try to get namespace
                NamespaceSyntax ns = current as NamespaceSyntax;

                // Get the name
                if (ns != null)
                    return ns.Name;

                return null;
            }
        }

        public TypeReferenceSyntax UnderlyingTypeReference
        {
            get { return underlyingTypeReference; }
        }

        public BlockSyntax<FieldSyntax> FieldBlock
        {
            get { return fieldBlock; }
        }

        public IEnumerable<FieldSyntax> Fields
        {
            get { return fieldBlock.Elements; }
        }

        public int FieldCount
        {
            get { return fieldBlock.ElementCount; }
        }

        public bool HasFields
        {
            get { return fieldBlock.HasElements; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { return fieldBlock.Descendants; }
        }

        // Constructor
        internal EnumSyntax(SyntaxToken identifier, AttributeReferenceSyntax[] attributes, SyntaxToken[] accessModifiers, TypeReferenceSyntax underlyingType, BlockSyntax<FieldSyntax> fieldBlock)
            : base(identifier, attributes, accessModifiers)
        {
            this.keyword = Syntax.Token(SyntaxTokenKind.EnumKeyword);

            if (underlyingType != null)
            {
                this.colon = Syntax.Token(SyntaxTokenKind.ColonSymbol);
                this.underlyingTypeReference = underlyingType;
            }

            this.fieldBlock = fieldBlock;

            // Check for null block
            if (fieldBlock == null)
                this.fieldBlock = new BlockSyntax<FieldSyntax>(this, Enumerable.Empty<FieldSyntax>());
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Generate attributes
            base.GetSourceText(writer);

            // Keyword
            keyword.GetSourceText(writer);

            // Identifier
            identifier.GetSourceText(writer);

            // Underlying type
            if(underlyingTypeReference != null)
            {
                // Colon
                colon.GetSourceText(writer);

                // Underlying type
                underlyingTypeReference.GetSourceText(writer);
            }

            // Member block
            fieldBlock.GetSourceText(writer);
        }

        public void AddMember(MemberSyntax member)
        {
            // Check for field
            if ((member is FieldSyntax) == false)
                throw new NotSupportedException("Must be a field");

            ((IMemberSyntaxContainer)fieldBlock).AddMember(member);

            // Update hierarchy
            member.parent = this;
        }
    }
}
