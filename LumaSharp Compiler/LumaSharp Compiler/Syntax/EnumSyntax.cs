
namespace LumaSharp_Compiler.AST
{
    public sealed class EnumSyntax : MemberSyntax
    {
        // Private
        private SyntaxToken keyword = null;
        private TypeReferenceSyntax underlyingTypeReference = null;

        private BlockSyntax<FieldSyntax> fieldBlock = null;

        // Properties
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
        internal EnumSyntax(string identifier)
            : base(identifier)
        {
            this.fieldBlock = new BlockSyntax<FieldSyntax>();
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
            throw new NotImplementedException();
        }
    }
}
