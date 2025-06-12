
using LumaSharp.Compiler.AST.Visitor;

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
        private readonly StatementSyntax statement;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return lambda; }
        }

        public override SyntaxToken EndToken
        {
            get { return statement.EndToken; }
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

        public StatementSyntax Statement
        {
            get { return statement; }
        }

        public bool IsReadBody
        {
            get { return keyword.Kind == SyntaxTokenKind.ReadKeyword; }
        }

        public bool IsWriteBody
        {
            get { return keyword.Kind == SyntaxTokenKind.WriteKeyword; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield return statement; }
        }

        // Constructor
        internal AccessorBodySyntax(SyntaxToken keyword, StatementSyntax statement)
            : this(
                  Syntax.Token(SyntaxTokenKind.LambdaSymbol),
                  keyword,
                  Syntax.Token(SyntaxTokenKind.ColonSymbol),
                  statement)
        {
        }


        internal AccessorBodySyntax(SyntaxToken lambda, SyntaxToken keyword, SyntaxToken colon, StatementSyntax statement)
        {
            // Check kind
            if (lambda.Kind != SyntaxTokenKind.LambdaSymbol)
                throw new ArgumentException(nameof(lambda) + " must be of kind: " + SyntaxTokenKind.LambdaSymbol);

            if (keyword.Kind != SyntaxTokenKind.ReadKeyword && keyword.Kind != SyntaxTokenKind.WriteKeyword)
                throw new ArgumentException(nameof(keyword) + " must be a valid accessor keyword");

            if(colon.Kind != SyntaxTokenKind.ColonSymbol)
                throw new ArgumentException(nameof(colon) + " must be of kind: " + SyntaxTokenKind.ColonSymbol);

            // Check null
            if(statement == null)
                throw new ArgumentNullException(nameof(statement));

            this.lambda = lambda;
            this.keyword = keyword;
            this.colon = colon;
            this.statement = statement;

            // Set parent
            statement.parent = this;
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitAccessorBody(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Lambda
            lambda.GetSourceText(writer);

            // Keyword
            keyword.GetSourceText(writer);

            // Colon
            colon.GetSourceText(writer);

            // Statement
            statement.GetSourceText(writer);
        }
    }
}
