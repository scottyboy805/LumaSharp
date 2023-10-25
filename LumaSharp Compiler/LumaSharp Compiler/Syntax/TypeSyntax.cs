
namespace LumaSharp_Compiler.AST
{
    public sealed class TypeSyntax : MemberSyntax, IMemberSyntaxContainer
    {
        // Private
        private SyntaxToken keyword = null;
        private GenericParameterListSyntax genericParameters = null;
        private TypeReferenceSyntax[] baseTypeReferences = null;

        private BlockSyntax<MemberSyntax> memberBlock = null;

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
                while(current != null && (current is NamespaceSyntax) == false)
                    current = current.Parent;

                // Try to get namespace
                NamespaceSyntax ns = current as NamespaceSyntax;

                // Get the name
                if (ns != null)
                    return ns.Name;

                return null;
            }
        }

        public GenericParameterListSyntax GenericParameters
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
            get { return HasGenericParameters ? genericParameters.GenericParameterCount : 0; }
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
        internal TypeSyntax(string identifier)
            : base(identifier)
        {
            // Members
            this.memberBlock = new BlockSyntax<MemberSyntax>();
        }

        internal TypeSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.TypeDeclarationContext typeDef)
            : base(typeDef.IDENTIFIER(), tree, parent, typeDef, typeDef.attributeDeclaration(), typeDef.accessModifier())
        {
            // Type keyword
            this.keyword = new SyntaxToken(typeDef.TYPE());

            // Get generics
            if (typeDef.genericParameterList() != null)
            {
                this.genericParameters = new GenericParameterListSyntax(tree, this, typeDef.genericParameterList());
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

        void IMemberSyntaxContainer.AddMember(MemberSyntax member)
        {
            ((IMemberSyntaxContainer)memberBlock).AddMember(member);

            // Update hierarchy
            member.tree = tree;
            member.parent = this;
        }
    }
}
