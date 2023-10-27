
namespace LumaSharp_Compiler.AST
{
    public class AccessorBodySyntax : SyntaxNode
    {
        // Private
        private SyntaxToken keyword = null;
        private StatementSyntax inlineBody = null;
        private BlockSyntax<StatementSyntax> blockBody = null;
        private SyntaxToken colon = null;

        // Properties
        public override SyntaxToken EndToken
        {
            get
            {
                if (HasBlockBody == true)
                    return blockBody.EndToken;

                return base.EndToken;
            }
        }

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
        internal AccessorBodySyntax(SyntaxToken keyword, StatementSyntax inlineBody)
            : base(keyword, inlineBody.EndToken)
        {
            this.keyword = keyword;
            this.inlineBody = inlineBody;
            this.colon = SyntaxToken.Colon();
        }

        internal AccessorBodySyntax(SyntaxToken keyword, StatementSyntax[] blockBody)
            : base(keyword, null)
        {
            this.keyword = keyword;
            this.blockBody = new BlockSyntax<StatementSyntax>(blockBody);
            this.colon = SyntaxToken.Colon();
        }

        internal AccessorBodySyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.AccessorReadContext read)
            : base(tree, parent, read)
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
            : base(tree, parent, write)
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
            // Keyword
            keyword.GetSourceText(writer);

            // Colon
            colon.GetSourceText(writer);

            // Check for inline
            if(HasInlineBody == true)
            {
                inlineBody.GetSourceText(writer);
            }
            else if(HasBlockBody == true)
            {
                blockBody.GetSourceText(writer);
            }
        }
    }
}
