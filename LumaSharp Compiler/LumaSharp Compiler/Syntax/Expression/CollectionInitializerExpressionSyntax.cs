
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class CollectionInitializerExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly SyntaxToken lBlock;
        private readonly SeparatedSyntaxList<ExpressionSyntax> initializerExpressions;
        private readonly SyntaxToken rBlock;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return lBlock; }
        }

        public override SyntaxToken EndToken
        {
            get { return rBlock; }
        }

        public SyntaxToken LBlock
        {
            get { return lBlock; }
        }

        public SyntaxToken RBlock
        {
            get { return rBlock; }
        }

        public SeparatedSyntaxList<ExpressionSyntax> InitializerExpressions
        {
            get { return initializerExpressions; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                if (initializerExpressions != null)
                    yield return initializerExpressions;
            }
        }

        // Constructor
        internal CollectionInitializerExpressionSyntax(SeparatedSyntaxList<ExpressionSyntax> initializerExpressions)
            : this(
                  new SyntaxToken(SyntaxTokenKind.LBlockSymbol),
                  initializerExpressions,
                  new SyntaxToken(SyntaxTokenKind.RBlockSymbol))
        {
        }

        internal CollectionInitializerExpressionSyntax(SyntaxToken lBlock, SeparatedSyntaxList<ExpressionSyntax> initializerExpressions, SyntaxToken rBlock)
        {
            this.lBlock = lBlock;
            this.initializerExpressions = initializerExpressions;
            this.rBlock = rBlock;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write lBlock
            lBlock.GetSourceText(writer);

            // Write initializers
            if(initializerExpressions != null)
                initializerExpressions.GetSourceText(writer);

            // Write rBlock
            rBlock.GetSourceText(writer);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCollectionInitializerExpression(this);
        }
    }
}
