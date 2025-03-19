
namespace LumaSharp.Compiler.AST
{
    public abstract class SyntaxNode
    {
        // Internal
        internal SyntaxNode parent = null;

        // Properties
        public virtual SyntaxTree SyntaxTree
        {
            get { return parent.SyntaxTree; }
        }

        public SyntaxNode Parent
        {
            get { return parent; }
            internal set { parent = value; }
        }

        public abstract SyntaxToken StartToken { get; }

        public abstract SyntaxToken EndToken { get; }

        internal abstract IEnumerable<SyntaxNode> Descendants { get; }

        // Constructor
        protected SyntaxNode(SyntaxNode parent)
        {
            this.parent = parent;
        }

        // Methods
        public override string ToString()
        {
            return string.Format("{0}({1})", GetType().Name, GetSourceText());
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

        public IEnumerable<T> DescendantsOfType<T>(bool withChildren = false) where T : SyntaxNode
        {
            foreach(SyntaxNode node in Descendants)
            {
                if (node is T)
                    yield return node as T;

                // Check for children
                if(withChildren == true)
                {
                    foreach (SyntaxNode child in node.DescendantsOfType<T>(withChildren))
                        if(child is T)
                            yield return child as T;
                }
            }
        }

        public IEnumerable<SyntaxNode> DescendantsOfType(Type type, bool withChildren = false)
        {
            foreach (SyntaxNode node in Descendants)
            {
                if (type.IsAssignableFrom(node.GetType()) == true)
                    yield return node;

                // Check for children
                if (withChildren == true)
                {
                    foreach (SyntaxNode child in node.DescendantsOfType(type, withChildren))
                        if (type.IsAssignableFrom(child.GetType()) == true)
                            yield return child;
                }
            }
        }
    }
}
