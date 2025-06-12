
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class EnumBlockSyntax : SeparatedSyntaxList<EnumFieldSyntax>
    {
        // Private
        private readonly SyntaxToken lBlock;
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

        // Constructor
        internal EnumBlockSyntax(SeparatedSyntaxList<EnumFieldSyntax> fields)
            : this(
                  Syntax.Token(SyntaxTokenKind.LBlockSymbol),
                  fields,
                  Syntax.Token(SyntaxTokenKind.RBlockSymbol))
        {
        }

        internal EnumBlockSyntax(SyntaxToken lBlock, SeparatedSyntaxList<EnumFieldSyntax> fields, SyntaxToken rBlock)
            : base(fields)
        {
            // Check kind
            if (lBlock.Kind != SyntaxTokenKind.LBlockSymbol)
                throw new ArgumentException(nameof(lBlock) + " must be of kind: " + SyntaxTokenKind.LBlockSymbol);

            if (rBlock.Kind != SyntaxTokenKind.RBlockSymbol)
                throw new ArgumentException(nameof(rBlock) + " must be of kind: " + SyntaxTokenKind.RBlockSymbol);

            this.lBlock = lBlock;
            this.rBlock = rBlock;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitEnumBlock(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitEnumBlock(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Block start
            lBlock.GetSourceText(writer);

            // Block fields
            base.GetSourceText(writer);

            // Block end
            rBlock.GetSourceText(writer);
        }
    }
}
