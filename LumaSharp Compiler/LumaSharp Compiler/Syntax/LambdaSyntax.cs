
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class LambdaSyntax : SyntaxNode
    {
        // Private
        private readonly SyntaxToken lambda;
        private readonly StatementSyntax statement;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return lambda; }
        }

        public override SyntaxToken EndToken
        {
            get{ return statement.EndToken; }
        }

        public SyntaxToken Lambda
        {
            get { return lambda; }
        }

        public StatementSyntax Statement
        {
            get { return statement; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield return statement; }
        }

        // Constructor
        internal LambdaSyntax(StatementSyntax statement)
            : this(
                  Syntax.Token(SyntaxTokenKind.LambdaSymbol),
                  statement)
        { 
        }

        internal LambdaSyntax(SyntaxToken lambda, StatementSyntax statement)
        {
            // Check kind
            if(lambda.Kind != SyntaxTokenKind.LambdaSymbol)
                throw new ArgumentException(nameof(lambda) + " must be of kind: " + SyntaxTokenKind.LambdaSymbol);

            // Check null
            if(statement == null)
                throw new ArgumentNullException(nameof(statement));

            this.lambda = lambda;
            this.statement = statement;

            // Set parent
            statement.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitLambda(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Lambda
            lambda.GetSourceText(writer);

            // Statement
            statement.GetSourceText(writer);
        }
    }
}
