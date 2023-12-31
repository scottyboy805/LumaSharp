﻿
namespace LumaSharp_Compiler.AST
{
    public sealed class MethodSyntax : MemberSyntax
    {
        // Private
        private TypeReferenceSyntax returnType = null;
        private GenericParameterListSyntax genericParameters = null;
        private ParameterListSyntax parameters = null;
        private BlockSyntax<StatementSyntax> body = null;
        private SyntaxToken semicolon = null;

        // Properties
        public override SyntaxToken EndToken
        {
            get
            {
                if(HasBody == true)
                {
                    return body.EndToken;
                }
                return base.EndToken;
            }
        }

        public TypeReferenceSyntax ReturnType
        {
            get { return returnType; }
            internal set { returnType = value; }
        }

        public GenericParameterListSyntax GenericParameters
        {
            get { return genericParameters; }
            internal set { genericParameters = value; }
        }

        public ParameterListSyntax Parameters
        {
            get { return parameters; }
            internal set { parameters = value; }
        }

        public BlockSyntax<StatementSyntax> Body
        {
            get { return body; }
            internal set { body = value; }
        }

        public bool HasGenericParameters
        {
            get { return genericParameters.HasGenericParameters; }
        }

        public bool HasParameters
        {
            get { return parameters.HasParameters; }
        }

        public bool HasBody
        {
            get { return body != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants => throw new NotImplementedException();

        // Constructor
        internal MethodSyntax(string identifier, TypeReferenceSyntax returnType)
            : base(identifier, returnType.StartToken, SyntaxToken.Semi())
        {
            this.returnType = returnType;
            this.identifier.WithLeadingWhitespace(" ");
            this.body = null;

            this.genericParameters = new GenericParameterListSyntax(Array.Empty<GenericParameterSyntax>());
            this.parameters = new ParameterListSyntax(Array.Empty<ParameterSyntax>());
            this.semicolon = base.EndToken;
        }

        internal MethodSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.MethodDeclarationContext methodDef)
            : base(methodDef.IDENTIFIER(), tree, parent, methodDef, methodDef.attributeDeclaration(), methodDef.accessModifier())
        {
            // Return type
            this.returnType = new TypeReferenceSyntax(tree, this, methodDef.typeReference());

            // Generics
            if (methodDef.genericParameterList() != null)
            {
                this.genericParameters = new GenericParameterListSyntax(tree, this, methodDef.genericParameterList());
            }
            else
                this.genericParameters = new GenericParameterListSyntax(Array.Empty<GenericParameterSyntax>());

            // Parameters
            if (methodDef.methodParameterList() != null)
            {
                this.parameters = new ParameterListSyntax(tree, this, methodDef.methodParameterList());
            }
            else
                this.parameters = new ParameterListSyntax(Array.Empty<ParameterSyntax>());

            // Create body
            LumaSharpParser.StatementBlockContext bodyBlock = methodDef.statementBlock();

            // Check for body
            if(bodyBlock != null)
            {
                this.body = new BlockSyntax<StatementSyntax>(tree, this, bodyBlock);
            }
        }

        // Methods
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

            // Body
            if(HasBody == true)
            {
                body.GetSourceText(writer);
            }
            else
            {
                semicolon.GetSourceText(writer);
            }
        }
    }
}
