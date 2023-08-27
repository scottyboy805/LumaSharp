
namespace LumaSharp_Compiler.Syntax
{
    public sealed class NamespaceSyntax : SyntaxNode
    {
        // Private
        private SyntaxToken keyword = null;
        private NamespaceName name = null;
        private BlockSyntax<MemberSyntax> members = null;

        // Properties
        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public NamespaceName Name
        {
            get { return name; }
        }

        public BlockSyntax<MemberSyntax> Block
        {
            get { return members; }
        }

        public IEnumerable<MemberSyntax> Members
        {
            get { return members.Elements; }
        }

        public int MemberCount
        {
            get { return members.ElementCount; }
        }

        public bool HasMembers
        {
            get { return members.HasElements; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { return members.Descendants; }
        }

        // Constructor
        internal NamespaceSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.NamespaceDeclarationContext namespaceDef) 
            : base(tree, parent, namespaceDef)
        {
            // Get keyword
            this.keyword = new SyntaxToken(namespaceDef.NAMESPACE());

            // Create name
            this.name = new NamespaceName(tree, this, namespaceDef.namespaceName());

            // Create block
            this.members = new BlockSyntax<MemberSyntax>(tree, this, namespaceDef.rootMemberBlock());
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write keyword
            writer.Write(keyword.Text);

            // Write namespace name
            name.GetSourceText(writer);

            // Write block
            members.GetSourceText(writer);
        }
    }
}
