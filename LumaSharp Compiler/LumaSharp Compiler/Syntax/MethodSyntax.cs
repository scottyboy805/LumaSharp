
namespace LumaSharp_Compiler.Syntax
{
    public sealed class MethodSyntax : MemberSyntax
    {
        // Private
        private TypeReferenceSyntax returnType = null;
        private GenericParameterListSyntax genericParameters = null;
        private ParameterListSyntax parameters = null;
        private BlockSyntax<StatementSyntax> body = null;

        // Properties
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

        public BlockSyntax<StatementSyntax> Body
        {
            get { return body; }
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
        internal MethodSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.MethodDeclarationContext methodDef)
            : base(methodDef.IDENTIFIER(), tree, parent, methodDef, methodDef.attributeDeclaration(), methodDef.accessModifier())
        {
            // Return type
            this.returnType = new TypeReferenceSyntax(tree, this, methodDef.typeReference());

            // Generics
            this.genericParameters = new GenericParameterListSyntax(tree, this, methodDef.genericParameterList());

            // Parameters
            this.parameters = new ParameterListSyntax(tree, this, methodDef.methodParameterList());

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
            throw new NotImplementedException();
        }
    }
}
