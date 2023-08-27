
namespace LumaSharp_Compiler.Syntax
{
    public class ArrayParametersSyntax : SyntaxNode
    {
        // Private
        private SyntaxToken indexStart = null;
        private SyntaxToken indexEnd = null;
        private int rank = 0;

        // Properties
        public int Rank
        {
            get { return rank; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        //internal ArrayParametersSyntax(SyntaxTree tree, SyntaxNode parent, int rank)
        //    : base(tree, parent)
        //{
        //    // Check for invalid rank
        //    if (rank > 3)
        //        throw new ArgumentException("Rank cannot be greater than 3");

        //    this.indexStart = new SyntaxToken("[");
        //    this.indexEnd = new SyntaxToken("]");
        //    this.rank = rank;
        //}

        internal ArrayParametersSyntax(SyntaxTree tree, SyntaxNode node, LumaSharpParser.ArrayParametersContext array)
            : base(tree, node, array)
        {
            this.indexStart = new SyntaxToken(array.Start);
            this.indexEnd = new SyntaxToken(array.Stop);

            // Get rank
            this.rank = array.ChildCount - 1;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Array start
            writer.Write(indexStart.ToString());

            // Write separator
            for (int i = 0; i < rank - 1; i++)
                writer.Write(',');

            // Array end
            writer.Write(indexEnd.ToString());
        }
    }
}
