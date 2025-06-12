
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class ConstructorSyntax : MemberSyntax
    {
        // Private
        private readonly SyntaxToken thisKeyword;
        private readonly ParameterListSyntax parameters;
        private readonly ConstructorInvokeSyntax constructorInvoke;
        private readonly StatementBlockSyntax body;
        private readonly LambdaSyntax lambda;

        // Properties
        public override SyntaxToken StartToken
        {
            get
            {
                // Check for attribute
                if (HasAttributes == true)
                    return Attributes[0].StartToken;

                // Check for modifiers
                if (HasAccessModifiers == true)
                    return AccessModifiers[0];

                // Return type
                return thisKeyword;
            }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                // Check for body
                if (HasBody == true)
                    return body.EndToken;

                // Check for lambda
                if(HasLambda == true)
                    return lambda.EndToken;

                // Parameter
                return parameters.EndToken;
            }
        }

        public SyntaxToken ThisKeyword
        {
            get { return thisKeyword; }
        }

        public ParameterListSyntax Parameters
        {
            get { return parameters; }
        }

        public ConstructorInvokeSyntax ConstructorInvoke
        {
            get { return constructorInvoke; }
        }

        public StatementBlockSyntax Body
        {
            get { return body; }
        }

        public LambdaSyntax Lambda
        {
            get { return lambda; }
        }

        public bool HasParameters
        {
            get { return parameters.HasParameters; }
        }

        public bool HasBody
        {
            get { return body != null; }
        }

        public bool HasLambda
        {
            get { return lambda != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                yield return parameters;

                if (HasBody == true)
                    yield return body;

                if(HasLambda == true)
                    yield return lambda;
            }
        }

        // Constructor
        internal ConstructorSyntax(SyntaxToken thisKeyword, AttributeSyntax[] attributes, SyntaxToken[] accessModifiers, ParameterListSyntax parameters, ConstructorInvokeSyntax constructorInvoke, StatementBlockSyntax body, LambdaSyntax lambda)
            : base(new SyntaxToken(SyntaxTokenKind.Identifier, thisKeyword.Text, thisKeyword.Span), attributes, accessModifiers)
        {
            // Check kind
            if(thisKeyword.Kind != SyntaxTokenKind.ThisKeyword)
                throw new ArgumentException(nameof(thisKeyword) + " must be of kind: " + SyntaxTokenKind.ThisKeyword);

            // Check null
            if(parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if(body == null && lambda == null)
                throw new ArgumentNullException(nameof(body) + " or " + nameof(lambda) + " must be provided");

            this.thisKeyword = thisKeyword;
            this.parameters = parameters;
            this.constructorInvoke = constructorInvoke;
            this.body = body;
            this.lambda = lambda;

            // Set parent
            parameters.parent = this;
            if (body != null) body.parent = this;
            if (lambda != null) lambda.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitConstructor(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitConstructor(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Generate attributes
            base.GetSourceText(writer);

            // Keyword
            thisKeyword.GetSourceText(writer);

            // Parameter list
            parameters.GetSourceText(writer);

            // Body
            if (HasBody == true)
            {
                body.GetSourceText(writer);
            }

            // Lambda
            if(HasLambda == true)
            {
                lambda.GetSourceText(writer);
            }
        }
    }
}
