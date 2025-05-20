
namespace LumaSharp.Compiler.AST
{
    public sealed class StatementBlockSyntax : StatementSyntax
    {
        // Private
        private readonly SyntaxToken lBlock;
        private readonly SyntaxToken rBlock;
        private readonly List<StatementSyntax> statements = new();

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

        public IEnumerable<StatementSyntax> Statements
        {
            get { return statements; }
        }

        // Constructor
        internal StatementBlockSyntax(IEnumerable<StatementSyntax> statements)
            : this(
                  new SyntaxToken(SyntaxTokenKind.LBlockSymbol),
                  statements,
                  new SyntaxToken(SyntaxTokenKind.RBlockSymbol))
        {
        }

        internal StatementBlockSyntax(SyntaxToken lBlock, IEnumerable<StatementSyntax> statements, SyntaxToken rBlock)
            : base(SyntaxToken.Invalid)
        {
            // Check kind
            if (lBlock.Kind != SyntaxTokenKind.LBlockSymbol)
                throw new ArgumentException(nameof(lBlock) + " must be of kind: " + SyntaxTokenKind.LBlockSymbol);

            if (rBlock.Kind != SyntaxTokenKind.RBlockSymbol)
                throw new ArgumentException(nameof(rBlock) + " must be of kind: " + SyntaxTokenKind.RBlockSymbol);

            this.lBlock = lBlock;
            this.rBlock = rBlock;

            // Check for any
            if (statements != null)
            {
                this.statements.AddRange(statements.Where(s => s != null));

                // Set parent
                foreach (StatementSyntax s in this.statements)
                    s.parent = this;
            }
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // LBlock
            lBlock.GetSourceText(writer);

            // Statements
            foreach(StatementSyntax statement in statements)
                statement.GetSourceText();

            // RBlock
            rBlock.GetSourceText(writer);
        }
    }
}
