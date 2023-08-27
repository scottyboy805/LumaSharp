using Antlr4.Runtime;

namespace LumaSharp_Compiler.Syntax
{
    public abstract class SyntaxNode
    {
        // Private
        private SyntaxTree tree = null;
        private SyntaxNode parent = null;

        private SyntaxToken start = null;
        private SyntaxToken end = null;

        // Properties
        public SyntaxTree Tree
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
        protected SyntaxNode(SyntaxTree tree, SyntaxNode parent, SyntaxToken token)
        {
            this.tree = tree;
            this.parent = parent;

            this.start = token;
            this.end = token;
        }

        protected SyntaxNode(SyntaxTree tree, SyntaxNode parent, ParserRuleContext context)
        {
            this.tree = tree;
            this.parent = parent;

            this.start = new SyntaxToken(context.Start);
            this.end = new SyntaxToken(context.Stop);
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
