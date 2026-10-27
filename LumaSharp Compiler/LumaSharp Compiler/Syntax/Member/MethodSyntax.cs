
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class MethodSyntax : MemberSyntax
    {
        // Private
        private readonly TypeReferenceSyntax returnType;
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
                if (HasModifiers == true)
                    return Modifiers[0];

                // Return type
                return returnType.StartToken;
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

        public TypeReferenceSyntax ReturnType
        {
            get { return returnType; }
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
            get { return returnType != null; }
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
        internal MethodSyntax(AttributeListSyntax attributes, SyntaxToken[] accessModifiers, TypeReferenceSyntax returnType, SyntaxToken identifier, GenericParameterListSyntax genericParameters, ParameterListSyntax parameters, SyntaxToken? overrideToken, StatementSyntax body, LambdaSyntax lambda)
            : base(identifier, attributes, accessModifiers)
        {
            // Check for parameters
            if (parameters == null)
                parameters = new ParameterListSyntax(null);

            // Check for invalid body
            if (body != null && (body is StatementBlockSyntax) == false && (body is EmptyStatementSyntax) == false)
                throw new ArgumentException("Body must be a statement block or empty statement");

            // Check for no return
            if (returnType == null)
                returnType = new TypeReferenceSyntax(ReservedType.Void, null);

            // Check for no body
            if (body == null && lambda == null)
                body = new EmptyStatementSyntax();

            this.returnType = returnType;
            this.genericParameters = genericParameters;
            this.parameters = parameters;
            this.overrideKeyword = overrideToken;

            this.body = body;
            this.lambda = lambda;

            // Set parent
            this.attributes?.parent = this;
            this.returnType.parent = this;
            this.genericParameters?.parent = this;
            this.parameters?.parent = this;
            this.body?.parent = this;
            this.lambda?.parent = this;
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
            returnType.GetSourceText(writer);

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

    public static partial class Syntax
    {
        // Methods
        public static MethodSyntax Method(SyntaxToken identifier, TypeReferenceSyntax returnType) 
            => new (null, null, returnType, identifier, null, null, null, null, null);

        public static MethodSyntax Method(SyntaxToken identifier) 
            => new (null, null, null, identifier, null, null, null, null, null);


        public static MethodSyntax WithAttributes(this MethodSyntax method, AttributeListSyntax attributes)
            => new (attributes, method.Modifiers, method.ReturnType, method.Identifier, method.GenericParameters, method.Parameters, method.Override, method.Body, method.Lambda);

        public static MethodSyntax WithAccessModifiers(this MethodSyntax method, params SyntaxToken[] modifiers)
            => new (method.Attributes, modifiers, method.ReturnType, method.Identifier, method.GenericParameters, method.Parameters, method.Override, method.Body, method.Lambda);

        public static MethodSyntax WithReturn(this MethodSyntax method, TypeReferenceSyntax returnType)
            => new (method.Attributes, method.Modifiers, returnType, method.Identifier, method.GenericParameters, method.Parameters, method.Override, method.Body, method.Lambda);


        public static MethodSyntax WithGenericParameters(this MethodSyntax method, params GenericParameterSyntax[] genericParameters)
            => method.WithGenericParameters(new GenericParameterListSyntax(new(SyntaxTokenKind.CommaSymbol, genericParameters)));

        public static MethodSyntax WithGenericParameters(this MethodSyntax method, GenericParameterListSyntax genericParameters)
            => new (method.Attributes, method.Modifiers, method.ReturnType, method.Identifier, genericParameters, method.Parameters, method.Override, method.Body, method.Lambda);

        public static MethodSyntax WithParameters(this MethodSyntax method, params ParameterSyntax[] parameters)
            => new (method.Attributes, method.Modifiers, method.ReturnType, method.Identifier, method.GenericParameters, new ParameterListSyntax(new(SyntaxTokenKind.CommaSymbol, parameters)), method.Override, method.Body, method.Lambda);

        public static MethodSyntax WithBody(this MethodSyntax method, params StatementSyntax[] bodyStatements)
            => new (method.Attributes, method.Modifiers, method.ReturnType, method.Identifier, method.GenericParameters, method.Parameters, method.Override, new StatementBlockSyntax(bodyStatements), null);

        public static MethodSyntax WithBody(this MethodSyntax method, StatementBlockSyntax body)
            => new (method.Attributes, method.Modifiers, method.ReturnType, method.Identifier, method.GenericParameters, method.Parameters, method.Override, body, null);

        public static MethodSyntax WithLambda(this MethodSyntax method, StatementSyntax inlineStatement)
            => new (method.Attributes, method.Modifiers, method.ReturnType, method.Identifier, method.GenericParameters, method.Parameters, method.Override, null, new LambdaSyntax(inlineStatement));

        public static MethodSyntax WithLambda(this MethodSyntax method, LambdaSyntax lambda)
            => new (method.Attributes, method.Modifiers, method.ReturnType, method.Identifier, method.GenericParameters, method.Parameters, method.Override, null, lambda);
    }
}
