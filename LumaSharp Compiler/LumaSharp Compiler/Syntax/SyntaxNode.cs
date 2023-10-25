using Antlr4.Runtime;

namespace LumaSharp_Compiler.AST
{
    public abstract class SyntaxNode
    {
        // Internal
        internal SyntaxTree tree = null;
        internal SyntaxNode parent = null;

        // Private
        private SyntaxToken start = null;
        private SyntaxToken end = null;

        // Properties
        public SyntaxTree SyntaxTree
        {
            get { return tree; }
        }

        public SyntaxNode Parent
        {
            get { return parent; }
        }

        public SyntaxToken StartToken
        {
            get { return start; }
        }

        public SyntaxToken EndToken
        {
            get { return end; }
        }

        internal abstract IEnumerable<SyntaxNode> Descendants { get; }

        // Constructor
        protected SyntaxNode(SyntaxToken token)
        {
            this.start = token;
            this.end = token;
        }

        protected SyntaxNode(SyntaxTree tree, SyntaxNode parent, ParserRuleContext context)
        {
            this.tree = tree;
            this.parent = parent;

            if (context != null)
            {
                this.start = new SyntaxToken(context.Start);
                this.end = new SyntaxToken(context.Stop);
            }
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
