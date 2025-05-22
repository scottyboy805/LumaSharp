
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class RangeExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly ExpressionSyntax from;
        private readonly SyntaxToken range;
        private readonly ExpressionSyntax to;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return from.StartToken; }
        }

        public override SyntaxToken EndToken
        {
            get { return to.EndToken; }
        }

        public SyntaxToken Range
        {
            get { return range; }
        }

        public ExpressionSyntax From
        {
            get { return from; }
        }

        public ExpressionSyntax To
        {
            get { return to; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                yield return from;
                yield return to;
            }
        }

        // Constructor
        internal RangeExpressionSyntax(ExpressionSyntax from, SyntaxToken range, ExpressionSyntax to)
        {
            // Check for null
            if(from == null)
                throw new ArgumentNullException(nameof(from));

            if(to == null)
                throw new ArgumentNullException(nameof(to));

            // Check kind
            if(range.IsRange == false)
                throw new ArgumentException(nameof(range) + " must be a valid range kind");

            this.from = from;
            this.range = range;
            this.to = to;

            // Set parent
            from.parent = this;
            to.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public override void GetSourceText(TextWriter writer)
        {
            // From
            from.GetSourceText(writer);

            // Range
            range.GetSourceText(writer);

            // To
            to.GetSourceText(writer);
        }
    }
}
