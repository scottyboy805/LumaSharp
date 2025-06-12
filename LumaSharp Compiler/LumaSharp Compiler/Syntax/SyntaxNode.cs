
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public abstract class SyntaxNode
    {
        // Internal
        internal SyntaxNode parent = null;

        // Properties
        public virtual SyntaxTree SyntaxTree => parent.SyntaxTree;
        public SyntaxNode Parent => parent;

        public abstract SyntaxToken StartToken { get; }

        public abstract SyntaxToken EndToken { get; }

        internal abstract IEnumerable<SyntaxNode> Descendants { get; }

        // Constructor
        protected SyntaxNode()
        {
        }

        // Methods
        public override string ToString()
        {
            return string.Format("{0}({1})", GetType().Name, GetSourceText());
        }

        public abstract void Accept(SyntaxVisitor visitor);

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

        public SyntaxSpan GetSpan()
        {
            // Get start and end spans
            SyntaxSpan start = StartToken.Span;
            SyntaxSpan end = EndToken.Span;

            // Create the total span
            return new SyntaxSpan(start.Document, start.Start, end.End);
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
