
namespace LumaSharp.Compiler.AST
{
    public sealed class CompilationUnitSyntax : SyntaxNode
    {
        // Private
        private readonly ImportSyntax[] imports;
        private readonly NamespaceSyntax[] namespaces;
        private readonly MemberSyntax[] rootMembers;

        // Properties
        public override SyntaxToken StartToken
        {
            get
            {
                // Check for imports
                if (imports != null)
                    ;
                return default;
            }
        }

        public override SyntaxToken EndToken => throw new NotImplementedException();

        internal override IEnumerable<SyntaxNode> Descendants => throw new NotImplementedException();

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
