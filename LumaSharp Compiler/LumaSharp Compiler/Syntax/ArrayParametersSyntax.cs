
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
            get { return separators.Length + 1; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal ArrayParametersSyntax(SyntaxNode parent, int rank)
            : base(parent)
        {
            if (rank < 1)
                throw new ArgumentException("Rank must be greater than 0");

            // Check for invalid rank
            if (rank > 2)
                throw new ArgumentException("Rank cannot be greater than 2");

            this.lArray = Syntax.KeywordOrSymbol(SyntaxTokenKind.LArraySymbol);
            this.rArray = Syntax.KeywordOrSymbol(SyntaxTokenKind.RArraySymbol);
            this.separators = new SyntaxToken[rank - 1];

            for (int i = 0; i < rank - 1; i++)
                this.separators[i] = Syntax.KeywordOrSymbol(SyntaxTokenKind.CommaSymbol);
        }

        internal ArrayParametersSyntax(SyntaxNode node, LumaSharpParser.ArrayParametersContext array)
            : base(node)
        {
            this.lArray = new SyntaxToken(SyntaxTokenKind.LArraySymbol, array.LARRAY());
            this.rArray = new SyntaxToken(SyntaxTokenKind.RArraySymbol, array.RARRAY());

            // Get separators
            this.separators = array.COMMA().Select(c => new SyntaxToken(SyntaxTokenKind.CommaSymbol, c)).ToArray();
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
