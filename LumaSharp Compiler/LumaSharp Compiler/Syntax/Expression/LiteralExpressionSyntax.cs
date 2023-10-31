
namespace LumaSharp_Compiler.AST
{
    public sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        // Private
        private SyntaxToken value = null;
        private SyntaxToken descriptor = null;

        // Properties
        public SyntaxToken Value
        {
            get { return value; }
        }

        public SyntaxToken Descriptor
        {
            get { return descriptor; }
        }

        public bool HasDescriptor
        {
            get { return descriptor != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        //internal LiteralExpressionSyntax(SyntaxTree tree, SyntaxNode parent)
        //    : base(tree, parent)
        //{
        //}

        internal LiteralExpressionSyntax(SyntaxToken value, SyntaxToken descriptor = null)
            : base(value)
        {
            this.value = value;
            this.descriptor = descriptor;
        }

        internal LiteralExpressionSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.EndExpressionContext end)
            : base(tree, parent, end)
        {
            // Create value
            this.value = new SyntaxToken(end.Start);

            // Create descriptor
            if (end.Start != end.Stop)
                this.descriptor = new SyntaxToken(end.Stop);
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write value
            writer.Write(value.Text);

            // Write descriptor
            if (HasDescriptor == true)
                writer.Write(descriptor.Text);
        }
    }
}
