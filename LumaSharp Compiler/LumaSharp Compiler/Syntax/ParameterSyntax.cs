
namespace LumaSharp.Compiler.AST
{
    public sealed class ParameterSyntax : SyntaxNode
    {
        // Private
        private readonly AttributeReferenceSyntax[] attributes;
        private readonly TypeReferenceSyntax parameterType;
        private readonly VariableAssignExpressionSyntax assignment;
        private readonly SyntaxToken identifier;
        private readonly SyntaxToken enumerable;
        private readonly int index;

        // Properties
        public override SyntaxToken StartToken
        {
            get
            {
                // Check for attributes
                if (HasAttributes == true)
                    return attributes[0].StartToken;

                // Type
                return parameterType.StartToken;
            }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                // Check for enumerable
                if (HasVariableSizedList == true)
                    return enumerable;

                // Check for assign
                if (HasAssignment == true)
                    return assignment.EndToken;

                return identifier;
            }
        }

        public TypeReferenceSyntax ParameterType
        {
            get { return parameterType; }
        }

        public VariableAssignExpressionSyntax Assignment
        {
            get { return assignment; }
        }

        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public SyntaxToken Enumerable
        {
            get { return enumerable; }
        }

        public int Index
        {
            get { return index; }
        }

        public bool HasVariableSizedList
        {
            get { return enumerable.Kind != SyntaxTokenKind.Invalid; }
        }

        public bool HasAttributes
        {
            get { return attributes != null; }
        }

        public bool HasAssignment
        {
            get { return assignment != null; }
        }

        public bool IsByReference
        {
            get { return attributes.Any(a => a.AttributeType.Identifier.Text == "in" || a.AttributeType.Identifier.Text == "ref"); }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                // Get type
                yield return parameterType;

                // Check for expression
                if (HasAssignment == true)
                    yield return assignment;
            }
        }

        // Constructor
        internal ParameterSyntax(SyntaxNode parent, AttributeReferenceSyntax[] attributes, TypeReferenceSyntax parameterType, string identifier, VariableAssignExpressionSyntax assignment, bool isVariableSizedList)
            : base(parent)
        {
            this.attributes = attributes;
            this.parameterType = parameterType;
            this.identifier = Syntax.Identifier(identifier);
            this.assignment = assignment;

            // Variable sized
            if (isVariableSizedList == true)
                this.enumerable = Syntax.KeywordOrSymbol(SyntaxTokenKind.EnumerableSymbol);
        }

        internal ParameterSyntax(SyntaxNode parent, LumaSharpParser.MethodParameterContext paramDef, int index)
            : base(parent)
        {
            // Index
            this.index = index;

            // Param type
            this.parameterType = new TypeReferenceSyntax(this, null, paramDef.typeReference());

            // Identifier
            this.identifier = new SyntaxToken(SyntaxTokenKind.Identifier, paramDef.IDENTIFIER());

            // Check for variable sized
            if(paramDef.ENUMERABLE() != null)
                this.enumerable = new SyntaxToken(SyntaxTokenKind.EnumerableSymbol, paramDef.ENUMERABLE());
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write attributes
            if(HasAttributes == true)
            {
                foreach(AttributeReferenceSyntax attribute in attributes)
                    attribute.GetSourceText(writer);
            }

            // Parameter type
            parameterType.GetSourceText(writer);

            // Identifier
            identifier.GetSourceText(writer);

            // Variable sized list
            if(HasVariableSizedList == true)
            {
                enumerable.GetSourceText(writer);
            }
        }
    }
}
