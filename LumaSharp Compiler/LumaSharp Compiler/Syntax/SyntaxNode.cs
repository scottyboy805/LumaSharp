
namespace LumaSharp_Compiler.Syntax
{
    public abstract class SyntaxNode
    {
        // Private
        private SyntaxTree tree = null;
        private SyntaxNode parent = null;

        // Properties
        public SyntaxTree Tree
        {
            get { return tree; }
        }

        public SyntaxNode Parent
        {
            get { return parent; }
        }

        public abstract SyntaxToken StartToken { get; }

        public abstract SyntaxToken EndToken { get; }

        internal abstract IEnumerable<SyntaxNode> Descendants { get; }

        // Constructor
        protected SyntaxNode(SyntaxTree tree, SyntaxNode parent)
        {
            this.tree = tree;
            this.parent = parent;
        }

        // Methods
        public override string ToString()
        {
            return GetSourceText();
        }

        public abstract void GetSourceText(TextWriter writer);

        public string GetSourceText()
        {
            // Create the writer
            using(StringWriter writer = new StringWriter())
            {
                // Write the text
                GetSourceText(writer);

                // Get full string
                return writer.ToString();
            }
        }

        public IEnumerable<T> DescendantsOfType<T>() where T : SyntaxNode
        {
            foreach(SyntaxNode node in Descendants)
            {
                if (node is T)
                    yield return node as T;
            }
        }

        public IEnumerable<SyntaxNode> DescendantsOfType(Type type)
        {
            foreach (SyntaxNode node in Descendants)
            {
                if (type.IsAssignableFrom(node.GetType()) == true)
                    yield return node;
            }
        }
    }
}
