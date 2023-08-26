
namespace LumaSharp_Compiler.Syntax
{
    public sealed class NamespaceName : SyntaxNode
    {
        // Private
        private SyntaxToken[] identifiers = null;

        // Properties
        public SyntaxToken[] Identifiers
        {
            get { return identifiers; }
        }

        public override SyntaxToken StartToken
        {
            get { return identifiers[0]; }
        }

        public override SyntaxToken EndToken
        {
            get { return identifiers[identifiers.Length - 1]; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal NamespaceName(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.NamespaceNameContext name)
            : base(tree, parent)
        {
            // Create identifiers
            this.identifiers = name.IDENTIFIER().Select(i => new SyntaxToken(i)).ToArray();
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            for(int i = 0; i < identifiers.Length; i++)
            {
                // Write name
                writer.Write(identifiers[i].Text);

                // Write separator
                if (i < identifiers.Length - 1)
                    writer.Write(':');
            }
        }
    }
}
