
namespace LumaSharp_Compiler.Syntax
{
    public class AccessorBodySyntax : SyntaxNode
    {
        // Private
        private SyntaxToken keyword = null;
        private StatementSyntax inlineBody = null;
        private BlockSyntax<StatementSyntax> blockBody = null;

        // Properties
        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public StatementSyntax InlineBody
        {
            get { return inlineBody; }
        }

        public BlockSyntax<StatementSyntax> BlockBody
        {
            get { return blockBody; }
        }

        public bool HasInlineBody
        {
            get { return inlineBody != null; }
        }

        public bool HasBlockBody
        {
            get { return blockBody != null; }
        }

        public bool IsReadBody
        {
            get { return keyword.Text == "read"; }
        }

        public bool IsWriteBody
        {
            get { return keyword.Text == "write"; }
        }

        public override SyntaxToken StartToken => throw new NotImplementedException();

        public override SyntaxToken EndToken => throw new NotImplementedException();

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                if (inlineBody != null)
                    yield return inlineBody;

                if(blockBody != null)
                    yield return blockBody;
            }
        }

        // Constructor
        internal AccessorBodySyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.AccessorReadContext read)
            : base(tree, parent)
        {
            // Get the keyword
            this.keyword = new SyntaxToken(read.READ());

            // Get the statement
            LumaSharpParser.StatementContext statement = read.statement();

            if(statement != null)
            {
                //this.inlineBody = new StatementSyntax(statement);
            }

            // Get the block statements
            LumaSharpParser.StatementBlockContext statementBlock = read.statementBlock();

            if(statementBlock != null)
            {
                this.blockBody = new BlockSyntax<StatementSyntax>(tree, this, statementBlock);
            }
        }

        internal AccessorBodySyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.AccessorWriteContext write)
            : base(tree, parent)
        {
            // Get the keyword
            this.keyword = new SyntaxToken(write.WRITE());

            // Get the statement
            LumaSharpParser.StatementContext statement = write.statement();

            if (statement != null)
            {
                //this.inlineBody = new StatementSyntax(statement);
            }

            // Get the block statements
            LumaSharpParser.StatementBlockContext statementBlock = write.statementBlock();

            if (statementBlock != null)
            {
                this.blockBody = new BlockSyntax<StatementSyntax>(tree, this, statementBlock);
            }
        }

        public override void GetSourceText(TextWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
