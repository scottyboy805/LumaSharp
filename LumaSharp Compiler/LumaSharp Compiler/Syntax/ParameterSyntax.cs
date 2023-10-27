
namespace LumaSharp_Compiler.AST
{
    public sealed class ParameterSyntax : SyntaxNode
    {
        // Private
        private TypeReferenceSyntax parameterType = null;
        private SyntaxToken identifier = null;
        private ExpressionSyntax assignExpression = null;
        private SyntaxToken reference = null;
        private SyntaxToken variableParameterList = null;
        private int index = 0;
        private bool byReference = false;
        private bool variableSizedList = false;

        // Properties
        public TypeReferenceSyntax ParameterType
        {
            get { return parameterType; }
        }

        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public ExpressionSyntax AssignExpression
        {
            get { return assignExpression; }
        }

        public int Index
        {
            get { return index; }
        }

        public bool IsByReference
        {
            get { return byReference; }
        }

        public bool IsVariableSizedList
        {
            get { return variableSizedList; }
        }

        public bool HasAssignExpression
        {
            get { return assignExpression != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                // Get type
                yield return parameterType;

                // Check for expression
                if (HasAssignExpression == true)
                    yield return assignExpression;
            }
        }

        // Constructor
        internal ParameterSyntax(TypeReferenceSyntax parameterType, string identifier, bool byReference = false, bool variableSizedList = false)
            : base(parameterType.StartToken, new SyntaxToken(identifier))
        {
            // Param type
            this.parameterType = parameterType;

            // Identifier
            this.identifier = new SyntaxToken(identifier)
                .WithLeadingWhitespace(" ");

            // Variable sized
            this.byReference = byReference;
            this.reference = SyntaxToken.Reference();
            this.variableSizedList = variableSizedList;
            this.variableParameterList = SyntaxToken.VariableParam();
        }

        internal ParameterSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.MethodParameterContext paramDef, int index)
            : base(tree, parent, paramDef)
        {
            // Index
            this.index = index;

            // Param type
            this.parameterType = new TypeReferenceSyntax(tree, this, paramDef.typeReference());

            // Identifier
            this.identifier = new SyntaxToken(paramDef.IDENTIFIER());

            // Check for variable sized
            this.variableSizedList = paramDef.Stop.Text == "...";
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Parameter type
            parameterType.GetSourceText(writer);

            // Check for reference
            if(IsByReference == true)
            {
                reference.GetSourceText(writer);
            }

            // Identifier
            identifier.GetSourceText(writer);

            // Variable sized list
            if(variableSizedList == true)
            {
                variableParameterList.GetSourceText(writer);
            }
        }
    }
}
