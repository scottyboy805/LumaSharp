
namespace LumaSharp_Compiler.AST
{
    public sealed class EnumSyntax : MemberSyntax
    {
        // Private
        private SyntaxToken keyword = null;
        private TypeReferenceSyntax underlyingTypeReference = null;
        private SyntaxToken colon = null;

        private BlockSyntax<FieldSyntax> fieldBlock = null;

        // Properties
        public override SyntaxToken EndToken
        {
            get { return fieldBlock.EndToken; }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public NamespaceName Namespace
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
        internal EnumSyntax(string identifier, TypeReferenceSyntax underlyingType)
            : base(identifier, SyntaxToken.Enum(), null)
        {
            this.keyword = base.StartToken.WithTrailingWhitespace(" ");
            this.underlyingTypeReference = underlyingType;
            this.fieldBlock = new BlockSyntax<FieldSyntax>();
            this.colon = SyntaxToken.Colon();
        }

        internal EnumSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.EnumDeclarationContext enumDef)
            : base(enumDef.IDENTIFIER(), tree, parent, enumDef, enumDef.attributeDeclaration(), enumDef.accessModifier())
        {
            // Enum keyword
            this.keyword = new SyntaxToken(enumDef.ENUM());

            // Enum type
            if (enumDef.primitiveType() != null)
            {
                this.underlyingTypeReference = new TypeReferenceSyntax(tree, parent, enumDef.primitiveType());
            }
            else
            {
                this.underlyingTypeReference = new TypeReferenceSyntax(tree, this, PrimitiveType.I32);
            }

            // Get fields
            LumaSharpParser.EnumBlockContext block = enumDef.enumBlock();

            this.fieldBlock = new BlockSyntax<FieldSyntax>(tree, this, underlyingTypeReference, block);
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
    }
}
