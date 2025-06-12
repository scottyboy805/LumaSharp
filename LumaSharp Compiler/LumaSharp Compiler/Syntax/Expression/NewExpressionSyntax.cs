
using LumaSharp.Compiler.AST.Visitor;

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
            get { return keyword; }
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
                  Syntax.Token(SyntaxTokenKind.NewKeyword),
                  newType,
                  arguments)
        {
        }

        internal NewExpressionSyntax(SyntaxToken keyword, TypeReferenceSyntax newType, ArgumentListSyntax arguments)
        {
            // Check kind
            if(keyword.Kind != SyntaxTokenKind.NewKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " +  SyntaxTokenKind.NewKeyword);

            if (arguments == null)
                arguments = new ArgumentListSyntax(null);

            this.keyword = keyword;
            this.newType = newType;
            this.arguments = arguments;

            // Set parent
            newType.parent = this;
            if(arguments != null) arguments.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitNewExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitNewExpression(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            keyword.GetSourceText(writer);

            // Type reference
            newType.GetSourceText(writer);

            // Argument list
            arguments.GetSourceText(writer);
        }        
    }
}
