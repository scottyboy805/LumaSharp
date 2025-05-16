
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
        internal NewExpressionSyntax(TypeReferenceSyntax newType, ArgumentListSyntax arguments)
            : this(
                  new SyntaxToken(SyntaxTokenKind.NewKeyword),
                  newType,
                  arguments)
        {
        }

        internal NewExpressionSyntax(SyntaxToken keyword, TypeReferenceSyntax newType, ArgumentListSyntax arguments)
        {
            // Check kind
            if(keyword.Kind != SyntaxTokenKind.NewKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " +  SyntaxTokenKind.NewKeyword);

            // Check for null
            if(arguments == null)
                throw new ArgumentNullException(nameof(arguments));

            this.keyword = keyword;
            this.newType = newType;
            this.arguments = arguments;
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
