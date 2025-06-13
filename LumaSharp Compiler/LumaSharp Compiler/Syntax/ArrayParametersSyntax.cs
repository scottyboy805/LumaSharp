
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class ArrayParametersSyntax : SyntaxNode
    {
        // Private
        private readonly SyntaxToken lArray;
        private readonly SyntaxToken rArray;
        private readonly SyntaxToken[] separators;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return lArray; }
        }

        public override SyntaxToken EndToken
        {
            get { return rArray; }
        }

        public SyntaxToken LArray
        {
            get { return lArray; }
        }

        public SyntaxToken RArray
        {
            get { return rArray; }
        }

        public SyntaxToken[] Separators
        {
            get { return separators; }
        }

        public int Rank
        {
            get { return separators != null ? separators.Length + 1 : 1; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal ArrayParametersSyntax(int rank)
            : this(
                  Enumerable.Repeat(
                    Syntax.Token(SyntaxTokenKind.CommaSymbol), rank - 1)
                    .ToArray())
        {
        }

        internal ArrayParametersSyntax(SyntaxToken[] separators)
            : this(
                  Syntax.Token(SyntaxTokenKind.LArraySymbol),
                  separators,
                  Syntax.Token(SyntaxTokenKind.RArraySymbol))
        {
        }

        internal ArrayParametersSyntax(SyntaxToken lArray, SyntaxToken[] separators, SyntaxToken rArray)
        {
            // Check for invalid rank
            if (separators != null)
            {
                // Check for rank too high
                if (separators.Length > 2)
                    throw new ArgumentException("Rank cannot be greater than 3");

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
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitArrayParameters(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitArrayParameters(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Array start
            lArray.GetSourceText(writer);

            // Write separator
            if (separators != null)
            {
                for (int i = 0; i < separators.Length; i++)
                    separators[i].GetSourceText(writer);
            }

            // Array end
            rArray.GetSourceText(writer);
        }
    }
}
