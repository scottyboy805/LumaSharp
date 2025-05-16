
namespace LumaSharp.Compiler.AST
{
    public sealed class StatementBlockSyntax : StatementSyntax
    {
        // Private
        private readonly SyntaxToken lBlock;
        private readonly SyntaxToken rBlock;
        private readonly List<StatementSyntax> statements = new();

        // Properties
        public override SyntaxToken StartToken => lBlock;
        public override SyntaxToken EndToken => rBlock;

        // Constructor
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
            if(statements != null)
                this.statements.AddRange(statements.Where(s => s != null));
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
