
namespace LumaSharp.Compiler.AST
{
    public sealed class LambdaStatementSyntax : SyntaxNode
    {
        // Private
        private readonly SyntaxToken lambda;
        private readonly StatementSyntax statement;
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
                // Check for comma
                if (HasComma == true)
                    return comma;

                return statement.EndToken;
            }
        }

        public SyntaxToken Lambda
        {
            get { return lambda; }
        }

        public StatementSyntax Statement
        {
            get { return statement; }
        }

        public SyntaxToken Comma
        {
            get { return comma; }
        }

        public bool HasComma
        {
            get { return comma.Kind != SyntaxTokenKind.Invalid; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield return statement; }
        }

        // Constructor
        internal LambdaStatementSyntax(SyntaxNode parent, StatementSyntax statement)
            : base(parent)
        {
            this.lambda = Syntax.KeywordOrSymbol(SyntaxTokenKind.LambdaSymbol);
            this.statement = statement;
        }

        internal LambdaStatementSyntax(SyntaxNode parent, LumaSharpParser.StatementLambdaContext lambda)
            : base(parent)
        {
            // Lambda
            this.lambda = new SyntaxToken(SyntaxTokenKind.LambdaSymbol, lambda.LAMBDA());

            // Statement
            this.statement = StatementSyntax.Any(this, lambda.statement());

            // Comma
            this.comma = new SyntaxToken(SyntaxTokenKind.CommaSymbol, lambda.COMMA());
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Lambda
            lambda.GetSourceText(writer);

            // Statement
            statement.GetSourceText(writer);

            // Comma
            if(HasComma == true)
                comma.GetSourceText(writer);
        }
    }
}
