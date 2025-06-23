
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class MethodSyntax : MemberSyntax
    {
        // Private
        private readonly SeparatedSyntaxList<TypeReferenceSyntax> returnTypes;
        private readonly GenericParameterListSyntax genericParameters;
        private readonly ParameterListSyntax parameters;
        private readonly StatementSyntax body;
        private readonly LambdaSyntax lambda;
        private readonly SyntaxToken? overrideKeyword;

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
                return returnTypes.StartToken;
            }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                // Check for body
                if (HasBody == true)
                    return body.EndToken;

                // Check for override
                if (overrideKeyword != null)
                    return overrideKeyword.Value;

                // Parameter
                return parameters.EndToken;
            }
        }

        public SyntaxToken? Override
        {
            get { return overrideKeyword; }
        }

        public SeparatedSyntaxList<TypeReferenceSyntax> ReturnTypes
        {
            get { return returnTypes; }
        }

        public GenericParameterListSyntax GenericParameters
        {
            get { return genericParameters; }
        }

        public ParameterListSyntax Parameters
        {
            get { return parameters; }
        }

        public StatementSyntax Body
        {
            get { return body; }
        }

        public LambdaSyntax Lambda
        {
            get { return lambda; }
        }

        public bool HasReturnType
        {
            get { return returnTypes != null && returnTypes.Count > 0; }
        }

        public bool HasGenericParameters
        {
            get { return genericParameters != null && genericParameters.HasGenericParameters; }
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
            get { return body?.Descendants; }
        }

        // Constructor
        internal MethodSyntax(SyntaxToken identifier, AttributeSyntax[] attributes, SyntaxToken[] accessModifiers, SeparatedSyntaxList<TypeReferenceSyntax> returnTypes, GenericParameterListSyntax genericParameters, ParameterListSyntax parameters, SyntaxToken? overrideToken, StatementSyntax body, LambdaSyntax lambda)
            : base(identifier, attributes, accessModifiers)
        {
            // Check for parameters
            if (parameters == null)
                parameters = new ParameterListSyntax(null);

            // Check for invalid body
            if (body != null && (body is StatementBlockSyntax) == false && (body is EmptyStatementSyntax) == false)
                throw new ArgumentException("Body must be a statement block or empty statement");

            // Check for no return
            if (returnTypes == null)
                returnTypes = new SeparatedSyntaxList<TypeReferenceSyntax>(SyntaxTokenKind.CommaSymbol,
                    new TypeReferenceSyntax(PrimitiveType.Void, null));

            // Check for no body
            if (body == null && lambda == null)
                body = new EmptyStatementSyntax();

            this.returnTypes = returnTypes;
            this.genericParameters = genericParameters;
            this.parameters = parameters;
            this.overrideKeyword = overrideToken;

            this.body = body;
            this.lambda = lambda;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitMethod(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitMethod(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Generate attributes
            base.GetSourceText(writer);

            // Return type
            returnTypes.GetSourceText(writer);

            // Identifier
            identifier.GetSourceText(writer);

            // Parameter list
            parameters.GetSourceText(writer);

            // Override
            if(overrideKeyword != null)
            {
                overrideKeyword?.GetSourceText(writer);
            }

            // Body
            if(HasBody == true)
            {
                body.GetSourceText(writer);
            }

            // Lambda
            if (HasLambda == true)
            {
                lambda.GetSourceText(writer);
            }
        }
    }
}
