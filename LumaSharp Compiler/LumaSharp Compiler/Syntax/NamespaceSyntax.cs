
namespace LumaSharp_Compiler.Syntax
{
    public sealed class NamespaceSyntax : BlockSyntax<MemberSyntax>
    {
        // Private
        private SyntaxToken[] identifiers = null;

        // Properties
        public SyntaxToken[] Identifiers
        {
            get { return identifiers; }
        }

        // Constructor
        internal NamespaceSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.NamespaceDeclarationContext namespaceDef) 
            : base(tree, parent, namespaceDef.rootMemberBlock())
        {
            // Create identifiers
            this.identifiers =  namespaceDef.IDENTIFIER().Select(i => new SyntaxToken(i)).ToArray();
        }
    }
}
