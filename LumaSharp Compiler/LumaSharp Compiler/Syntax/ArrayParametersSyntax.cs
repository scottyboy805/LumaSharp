
namespace LumaSharp.Compiler.AST
{
    public sealed class ArrayParametersSyntax : SyntaxNode
    {
        // Private
        private readonly SyntaxToken lArray;
        private readonly SyntaxToken rArray;
        private readonly SyntaxToken[] separators;

        // Properties
        public override SyntaxToken StartToken => lArray;
        public override SyntaxToken EndToken => rArray;
        public SyntaxToken LArray => lArray;
        public SyntaxToken RArray => rArray;
        public SyntaxToken[] Separators => separators;
        public int Rank => separators != null ? separators.Length + 1 : 1;

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal ArrayParametersSyntax(int rank)
            : this(
                  Enumerable.Repeat(
                    new SyntaxToken(SyntaxTokenKind.CommaSymbol), rank)
                    .ToArray())
        {
        }

        internal ArrayParametersSyntax(SyntaxToken[] separators)
            : this(
                  new SyntaxToken(SyntaxTokenKind.LArraySymbol),
                  separators,
                  new SyntaxToken(SyntaxTokenKind.RArraySymbol))
        {
        }

        internal ArrayParametersSyntax(SyntaxToken lArray, SyntaxToken[] separators, SyntaxToken rArray)
        {
            // Check for invalid rank
            if (separators != null)
            {
                // Check for rank too high
                if (separators.Length > 1)
                    throw new ArgumentException("Rank cannot be greater than 2");

                // Check for invalid separators
                foreach (SyntaxToken separator in separators)
                {
                    // Require commas
                    if (separator.Kind != SyntaxTokenKind.CommaSymbol)
                        throw new ArgumentException("Separator must be of kind: " + SyntaxTokenKind.CommaSymbol);
                }
            }

            this.lArray = lArray;
            this.rArray = rArray;
            this.separators = separators;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Array start
            lArray.GetSourceText(writer);

            // Write separator
            for (int i = 0; i < separators.Length; i++)
                separators[i].GetSourceText(writer);

            // Array end
            rArray.GetSourceText(writer);
        }
    }
}
