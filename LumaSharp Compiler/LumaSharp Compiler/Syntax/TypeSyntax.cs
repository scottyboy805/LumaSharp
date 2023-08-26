
namespace LumaSharp_Compiler.Syntax
{
    public sealed class TypeSyntax : MemberSyntax, IMemberSyntaxContainer
    {
        // Private
        private SyntaxToken keyword = null;
        private SyntaxToken start = null;
        private SyntaxToken end = null;
        private GenericParametersSyntax genericParameters = null;
        private TypeReferenceSyntax[] baseTypeReferences = null;

        private BlockSyntax<MemberSyntax> memberBlock = null;

        // Properties
        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public override SyntaxToken StartToken
        {
            get { return start; }
        }

        public override SyntaxToken EndToken
        {
            get { return end; }
        }

        public GenericParametersSyntax GenericParameters
        {
            get { return genericParameters; }
        }

        public TypeReferenceSyntax[] BaseTypeReferences
        {
            get { return baseTypeReferences; }
        }

        public BlockSyntax<MemberSyntax> MemberBlock
        {
            get { return memberBlock; }
        }

        public IEnumerable<MemberSyntax> Members
        {
            get { return memberBlock.Elements; }
        }

        public int GenericParameterCount
        {
            get { return HasGenericParameters ? genericParameters.GenericTypeCount : 0; }
        }

        public int BaseTypeCount
        {
            get { return HasBaseTypes ? baseTypeReferences.Length : 0; }
        }

        public int MemberCount
        {
            get { return HasMembers ? memberBlock.ElementCount: 0; }
        }        

        public bool HasGenericParameters
        {
            get { return genericParameters != null; }
        }

        public bool HasBaseTypes
        {
            get { return baseTypeReferences != null; }
        }

        public bool HasMembers
        {
            get { return memberBlock.HasElements; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { return memberBlock.Descendants; }
        }

        // Constructor
        internal TypeSyntax(SyntaxTree tree, SyntaxNode parent, string identifier)
            : base(identifier, tree, parent)
        {
            this.keyword = new SyntaxToken("type");

            this.start = this.keyword;
            this.end = this.identifier;
        }

        internal TypeSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.TypeDeclarationContext typeDef)
            : base(typeDef.IDENTIFIER(), tree, parent, typeDef.attributeDeclaration(), typeDef.accessModifier())
        {
            // Type keyword
            this.keyword = new SyntaxToken(typeDef.TYPE());            

            // Get generics
            LumaSharpParser.GenericParametersContext generics = typeDef.genericParameters();

            if(generics != null)
            {
                this.genericParameters = new GenericParametersSyntax(tree, this, generics);
            }

            // Get base
            LumaSharpParser.InheritParametersContext inherit = typeDef.inheritParameters();

            if (inherit != null)
            {
                this.baseTypeReferences = inherit.typeReference().Select(t => new TypeReferenceSyntax(tree, this, t)).ToArray();
            }

            // Get members
            LumaSharpParser.MemberBlockContext block = typeDef.memberBlock();

            this.memberBlock = new BlockSyntax<MemberSyntax>(tree, this, block);
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {

        }
    }
}
