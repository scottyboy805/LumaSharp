
namespace LumaSharp.Compiler.AST
{
    public enum AccessorOperation
    {
        Read,
        Write,
    }

    public sealed class AccessorBodySyntax : SyntaxNode
    {
        // Private
        private readonly SyntaxToken lambda;
        private readonly SyntaxToken keyword;
        private readonly SyntaxToken colon;
        private readonly StatementSyntax inlineBody;
        private readonly BlockSyntax<StatementSyntax> blockBody;
        private readonly SyntaxToken comma;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return lambda; }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                if (HasBlockBody == true)
                    return blockBody.EndToken;

                return blockBody.EndToken;
            }
        }

        public SyntaxToken Lambda
        {
            get { return lambda; }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public SyntaxToken Colon
        {
            get { return colon; }
        }

        public SyntaxToken Comma
        {
            get { return comma; }
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

        public bool HasComma
        {
            get { return comma.Kind != SyntaxTokenKind.Invalid; }
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
        internal AccessorBodySyntax(SyntaxNode parent, AccessorOperation op, StatementSyntax inlineBody)
        {
            this.lambda = Syntax.Token(SyntaxTokenKind.LambdaSymbol);
            this.keyword = op == AccessorOperation.Read
                ? Syntax.Token(SyntaxTokenKind.ReadKeyword)
                : Syntax.Token(SyntaxTokenKind.WriteKeyword);
            this.colon = Syntax.Token(SyntaxTokenKind.ColonSymbol);

            this.inlineBody = inlineBody;
        }

        internal AccessorBodySyntax(SyntaxNode parent, AccessorOperation op, BlockSyntax<StatementSyntax> bodyBlock)
        {
            this.lambda = Syntax.Token(SyntaxTokenKind.LambdaSymbol);
            this.keyword = op == AccessorOperation.Read
                ? Syntax.Token(SyntaxTokenKind.ReadKeyword)
                : Syntax.Token(SyntaxTokenKind.WriteKeyword);
            this.colon = Syntax.Token(SyntaxTokenKind.ColonSymbol);

            this.blockBody = bodyBlock;
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Lambda
            lambda.GetSourceText(writer);

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

            // Check for comma
            if (HasComma == true)
                comma.GetSourceText(writer);
        }
    }
}
