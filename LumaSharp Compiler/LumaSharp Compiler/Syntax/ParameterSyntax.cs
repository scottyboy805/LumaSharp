
namespace LumaSharp_Compiler.AST
{
    public sealed class ParameterSyntax : SyntaxNode
    {
        // Private
        private TypeReferenceSyntax parameterType = null;
        private SyntaxToken identifier = null;
        private ExpressionSyntax assignExpression = null;
        private int index = 0;
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
        internal ParameterSyntax(TypeReferenceSyntax parameterType, string identifier, bool variableSizedList = false)
            : base((SyntaxToken)null)
        {
            // Param type
            this.parameterType = parameterType;

            // Identifier
            this.identifier = new SyntaxToken(identifier);

            // Variable sized
            this.variableSizedList = variableSizedList;
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
            throw new NotImplementedException();
        }
    }
}
