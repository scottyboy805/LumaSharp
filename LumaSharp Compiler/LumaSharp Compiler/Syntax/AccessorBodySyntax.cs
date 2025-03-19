
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
            : base(parent)
        {
            this.lambda = Syntax.KeywordOrSymbol(SyntaxTokenKind.LambdaSymbol);
            this.keyword = op == AccessorOperation.Read
                ? Syntax.KeywordOrSymbol(SyntaxTokenKind.ReadKeyword)
                : Syntax.KeywordOrSymbol(SyntaxTokenKind.WriteKeyword);
            this.colon = Syntax.KeywordOrSymbol(SyntaxTokenKind.ColonSymbol);

            this.inlineBody = inlineBody;
        }

        internal AccessorBodySyntax(SyntaxNode parent, AccessorOperation op, BlockSyntax<StatementSyntax> bodyBlock)
            : base(parent)
        {
            this.lambda = Syntax.KeywordOrSymbol(SyntaxTokenKind.LambdaSymbol);
            this.keyword = op == AccessorOperation.Read
                ? Syntax.KeywordOrSymbol(SyntaxTokenKind.ReadKeyword)
                : Syntax.KeywordOrSymbol(SyntaxTokenKind.WriteKeyword);
            this.colon = Syntax.KeywordOrSymbol(SyntaxTokenKind.ColonSymbol);

            this.blockBody = bodyBlock;
        }

        internal AccessorBodySyntax(SyntaxNode parent, LumaSharpParser.ExpressionLambdaContext lambda)
            : base(parent)
        {
            // Get the lambda
            this.lambda = new SyntaxToken(SyntaxTokenKind.LambdaSymbol, lambda.LAMBDA());

            // Get expression
            this.inlineBody = new ReturnStatementSyntax(this, 
                new[] { ExpressionSyntax.Any(this, lambda.expression()) });

            // Get comma
            if (lambda.COMMA() != null)
                this.comma = new SyntaxToken(SyntaxTokenKind.CommaSymbol, lambda.COMMA());            
        }

        internal AccessorBodySyntax(SyntaxNode parent, LumaSharpParser.AccessorReadWriteContext readWrite)
            : base(parent)
        {
            // Get lambda
            this.lambda = new SyntaxToken(SyntaxTokenKind.LambdaSymbol, readWrite.LAMBDA());

            // Get keyword
            this.keyword = readWrite.READ() != null
                ? new SyntaxToken(SyntaxTokenKind.ReadKeyword, readWrite.READ())
                : new SyntaxToken(SyntaxTokenKind.WriteKeyword, readWrite.WRITE());

            // Get colon
            this.colon = new SyntaxToken(SyntaxTokenKind.ColonSymbol, readWrite.COLON());

            // Get comma
            if (readWrite.COMMA() != null)
                this.comma = new SyntaxToken(SyntaxTokenKind.CommaSymbol, readWrite.COMMA());

            // Statement
            if (readWrite.statement() != null)
                this.inlineBody = StatementSyntax.Any(this, readWrite.statement());

            // Statement block
            if (readWrite.statementBlock() != null)
                this.blockBody = new BlockSyntax<StatementSyntax>(this, readWrite.statementBlock());
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
