
namespace LumaSharp.Compiler.AST
{
    public sealed class NewExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly TypeReferenceSyntax newType;
        private readonly ArgumentListSyntax arguments;

        // Properties
        public override SyntaxToken StartToken
        {
            get
            {
                // Check for keyword
                if (HasKeyword == true)
                    return keyword;

                // Type
                return newType.StartToken;
            }
        }

        public override SyntaxToken EndToken
        {
            get { return arguments.EndToken; }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public TypeReferenceSyntax NewType
        {
            get { return newType; }
        }

        public ArgumentListSyntax ArgumentList
        {
            get { return arguments; }
        }

        public int ArgumentCount
        {
            get { return HasArguments == true ? arguments.Count : 0; }
        }

        public bool HasArguments
        {
            get { return arguments != null; }
        }

        public bool HasKeyword
        {
            get { return keyword.Kind != SyntaxTokenKind.Invalid; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                yield return newType;

                foreach (SyntaxNode node in arguments.Descendants)
                    yield return node;
            }
        }

        // Constructor
        internal NewExpressionSyntax(SyntaxNode parent, TypeReferenceSyntax newType, ArgumentListSyntax arguments)
            : base(parent, null)
        {
            this.keyword = Syntax.KeywordOrSymbol(SyntaxTokenKind.NewKeyword);
            this.newType = newType;
            this.arguments = arguments;
        }

        internal NewExpressionSyntax(SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(parent, expression)
        {
            LumaSharpParser.NewExpressionContext newExpression = expression.newExpression();

            // Keyword
            if (newExpression.NEW() != null)
            {
                this.keyword = new SyntaxToken(SyntaxTokenKind.NewKeyword, newExpression.NEW());
            }

            // New type
            this.newType = new TypeReferenceSyntax(this, expression, newExpression.typeReference());

            // Init arguments
            LumaSharpParser.ArgumentListContext argumentList = newExpression.argumentList();

            if(argumentList != null)
                this.arguments = new ArgumentListSyntax(this, argumentList);
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            if (HasKeyword == true)
                keyword.GetSourceText(writer);

            // Type reference
            newType.GetSourceText(writer);

            // Argument list
            arguments.GetSourceText(writer);
        }
    }
}
