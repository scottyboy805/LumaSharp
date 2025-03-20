
namespace LumaSharp.Compiler.AST
{
    public sealed class MethodSyntax : MemberSyntax
    {
        // Private
        private readonly SeparatedListSyntax<TypeReferenceSyntax> returnTypes;
        private readonly GenericParameterListSyntax genericParameters;
        private readonly ParameterListSyntax parameters;
        private readonly BlockSyntax<StatementSyntax> body;
        private readonly LambdaStatementSyntax lambdaStatement;
        private readonly SyntaxToken overrideKeyword;

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
                if(HasBody == true)
                    return body.EndToken;

                // Check for lambda
                if(HasLambdaStatement == true)
                    return lambdaStatement.EndToken;

                // Check for override
                if (IsOverride == true)
                    return overrideKeyword;

                // Parameter
                return parameters.EndToken;
            }
        }

        public SyntaxToken Override
        {
            get { return overrideKeyword; }
        }

        public SeparatedListSyntax<TypeReferenceSyntax> ReturnTypes
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

        public BlockSyntax<StatementSyntax> Body
        {
            get { return body; }
        }

        public LambdaStatementSyntax LambdaStatement
        {
            get { return lambdaStatement; }
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

        public bool HasLambdaStatement
        {
            get { return LambdaStatement != null; }
        }

        public bool IsOverride
        {
            get { return overrideKeyword.Kind != SyntaxTokenKind.Invalid; }
        }

        internal override IEnumerable<SyntaxNode> Descendants => throw new NotImplementedException();

        // Constructor
        internal MethodSyntax(SyntaxNode parent, string identifier, AttributeReferenceSyntax[] attributes, SyntaxToken[] accessModifiers, SeparatedListSyntax<TypeReferenceSyntax> returnTypes, GenericParameterListSyntax genericParameters, ParameterListSyntax parameters, bool isOverride, BlockSyntax<StatementSyntax> body, LambdaStatementSyntax lambda)
            : base(parent, identifier, attributes, accessModifiers)
        {
            this.returnTypes = returnTypes;
            this.genericParameters = genericParameters;
            this.parameters = parameters;

            // Check for override
            if (isOverride == true)
                this.overrideKeyword = Syntax.KeywordOrSymbol(SyntaxTokenKind.OverrideKeyword);

            this.body = body;
            this.lambdaStatement = lambda;

            // Ensure return
            if (returnTypes == null)
            {
                this.returnTypes = new SeparatedListSyntax<TypeReferenceSyntax>(this, SyntaxTokenKind.CommaSymbol);
                this.returnTypes.AddElement(Syntax.TypeReference(PrimitiveType.Void), Syntax.KeywordOrSymbol(SyntaxTokenKind.CommaSymbol));
            }

            // Ensure parameters
            if (parameters == null)
                this.parameters = new ParameterListSyntax(this, (ParameterSyntax[])null);
        }

        internal MethodSyntax(SyntaxNode parent, LumaSharpParser.MethodDeclarationContext methodDef)
            : base(methodDef.IDENTIFIER(), parent, methodDef.attributeReference(), methodDef.accessModifier())
        {
            // Get return
            LumaSharpParser.MethodReturnListContext methodReturn = methodDef.methodReturnList();

            // Check for void type
            if(methodReturn.VOID() != null)
            {
                this.returnTypes = new SeparatedListSyntax<TypeReferenceSyntax>(this, SyntaxTokenKind.CommaSymbol);
                this.returnTypes.AddElement(new TypeReferenceSyntax(null, PrimitiveType.Void), null);
            }
            else
            {
                // Get type list
                this.returnTypes = ExpressionSyntax.List(this, methodReturn.typeReferenceList());
            }

            // Generics
            if (methodDef.genericParameterList() != null)
            {
                this.genericParameters = new GenericParameterListSyntax(this, methodDef.genericParameterList());
            }

            // Parameters
            if (methodDef.methodParameterList() != null)
            {
                this.parameters = new ParameterListSyntax(this, methodDef.methodParameterList());
            }

            // Override
            if (methodDef.OVERRIDE() != null)
            {
                this.overrideKeyword = new SyntaxToken(SyntaxTokenKind.OverrideKeyword, methodDef.OVERRIDE());
            }

            // Create body
            LumaSharpParser.StatementBlockContext bodyBlock = methodDef.statementBlock();

            // Check for body 
            if(bodyBlock != null)
            {
                this.body = new BlockSyntax<StatementSyntax>(this, bodyBlock);
            }

            // Create for lambda
            LumaSharpParser.StatementLambdaContext lambdaStatement = methodDef.statementLambda();

            // Check for lambda
            if(lambdaStatement != null)
            {
                this.lambdaStatement = new LambdaStatementSyntax(this, lambdaStatement);
            }
        }

        // Methods
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

            // Body
            if(HasBody == true)
            {
                body.GetSourceText(writer);
            }
            // Lambda
            else if(HasLambdaStatement == true)
            {
                lambdaStatement.GetSourceText(writer);
            }
        }
    }
}
