
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace LumaSharp_Compiler.Syntax
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

        public override SyntaxToken StartToken
        {
            get { return value; }
        }

        public override SyntaxToken EndToken
        {
            get { return value; }
        }

        public bool HasDescriptor
        {
            get { return descriptor != null; }
        }

        // Constructor
        internal LiteralExpressionSyntax(SyntaxTree tree, SyntaxNode parent)
            : base(tree, parent)
        {
        }

        internal LiteralExpressionSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.EndExpressionContext end)
            : base(tree, parent)
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
