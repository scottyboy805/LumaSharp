
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
        internal LambdaStatementSyntax(StatementSyntax statement)
            : this(
                  new SyntaxToken(SyntaxTokenKind.LambdaSymbol),
                  statement)
        { 
        }

        internal LambdaStatementSyntax(SyntaxToken lambda, StatementSyntax statement)
        {
            // Check kind
            if(lambda.Kind != SyntaxTokenKind.LambdaSymbol)
                throw new ArgumentException(nameof(lambda) + " must be of kind: " + SyntaxTokenKind.LambdaSymbol);

            // Check null
            if(statement == null)
                throw new ArgumentNullException(nameof(statement));

            this.lambda = lambda;
            this.statement = statement;
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
