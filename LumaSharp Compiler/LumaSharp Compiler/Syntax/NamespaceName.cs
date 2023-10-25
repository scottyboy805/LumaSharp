
using Antlr4.Runtime.Tree;

namespace LumaSharp_Compiler.AST
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

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal NamespaceName(string identifier)
            : base(new SyntaxToken(identifier))
        {
            this.identifiers = identifier.Split('.').Select(i => new SyntaxToken(i)).ToArray();
        }

        internal NamespaceName(SyntaxTree tree, SyntaxNode parent, ITerminalNode[] identifiers)
            : base(tree, parent, null)
        {
            // Create identifiers
            this.identifiers = identifiers.Select(i => new SyntaxToken(i)).ToArray();
        }

        internal NamespaceName(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.NamespaceNameContext name)
            : base(tree, parent, name)
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
