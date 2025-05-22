
namespace LumaSharp.Compiler.AST
{
    public sealed class ForeachStatementSyntax : StatementSyntax
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly TypeReferenceSyntax typeReference;
        private readonly SyntaxToken identifier;
        private readonly SyntaxToken inKeyword;
        private readonly ExpressionSyntax expression;
        private readonly StatementSyntax statement;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get { return expression.EndToken; }
        }

        public TypeReferenceSyntax TypeReference
        {
            get { return typeReference; }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public SyntaxToken In
        {
            get { return inKeyword; }
        }

        public ExpressionSyntax Expression
        {
            get { return expression; }
        }

        public StatementSyntax Statement
        {
            get { return statement; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                yield return typeReference;
                yield return expression;
                yield return statement;
            }
        }

        // Constructor
        internal ForeachStatementSyntax(SyntaxToken keyword, TypeReferenceSyntax typeReference, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax expression, StatementSyntax statement)
        {
            // Check kind
            if(keyword.Kind != SyntaxTokenKind.ForKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.ForKeyword);

            if (identifier.Kind != SyntaxTokenKind.Identifier)
                throw new ArgumentException(nameof(identifier) + " must be of kind: " + SyntaxTokenKind.Identifier);

            if(inKeyword.Kind != SyntaxTokenKind.InKeyword)
                throw new ArgumentException(nameof(inKeyword) + " must be of kind: " + SyntaxTokenKind.InKeyword);

            // Check for null
            if(expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (statement == null)
                throw new ArgumentNullException(nameof(statement));

            this.keyword = keyword;
            this.typeReference = typeReference;
            this.identifier = identifier;
            this.inKeyword = inKeyword;
            this.expression = expression;
            this.statement = statement;

            // Set parent
            if (typeReference != null) typeReference.parent = this;
            expression.parent = this;
            statement.parent = this;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            keyword.GetSourceText(writer);

            // Type reference
            if(typeReference != null)
                typeReference.GetSourceText(writer);

            // Identifier
            identifier.GetSourceText(writer);

            // In
            inKeyword.GetSourceText(writer);

            // Expression
            expression.GetSourceText(writer);

            // Statement
            statement.GetSourceText(writer);
        }
    }
}
