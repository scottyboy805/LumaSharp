
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class ParameterSyntax : SyntaxNode
    {
        // Private
        private readonly AttributeSyntax[] attributes;
        private readonly TypeReferenceSyntax parameterType;
        private readonly VariableAssignmentExpressionSyntax assignment;
        private readonly SyntaxToken identifier;
        private readonly SyntaxToken? enumerable;

        // Internal
        internal static readonly ParameterSyntax Error = new();

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
                if (enumerable != null)
                    return enumerable.Value;

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

        public VariableAssignmentExpressionSyntax Assignment
        {
            get { return assignment; }
        }

        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public SyntaxToken? Enumerable
        {
            get { return enumerable; }
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
            get { return attributes != null && attributes.Any(a => a.AttributeType.Identifier.Text == "in" || a.AttributeType.Identifier.Text == "ref"); }
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
        private ParameterSyntax()
        {
            this.identifier = Syntax.Identifier("Error");
        }

        internal ParameterSyntax(AttributeSyntax[] attributes, TypeReferenceSyntax parameterType, SyntaxToken identifier, VariableAssignmentExpressionSyntax assignment, SyntaxToken? enumerable)
        {
            // Check null
            if (parameterType == null)
                throw new ArgumentNullException(nameof(parameterType));

            // Check kind
            if(identifier.Kind != SyntaxTokenKind.Identifier)
                throw new ArgumentException(nameof(identifier) + " must be of kind: " + SyntaxTokenKind.Identifier);

            if(enumerable != null && enumerable.Value.Kind != SyntaxTokenKind.EnumerableSymbol)
                throw new ArgumentException(nameof(enumerable) + " must be of kind: " + SyntaxTokenKind.EnumerableSymbol.ToString());

            this.attributes = attributes;
            this.parameterType = parameterType;
            this.identifier = identifier;
            this.assignment = assignment;
            this.enumerable = enumerable;

            // Set parent
            if(attributes != null)
            {
                foreach (AttributeSyntax a in attributes)
                    a.parent = this;
            }
            parameterType.parent = this;
            if (assignment != null) assignment.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitParameter(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitParameter(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Write attributes
            if(HasAttributes == true)
            {
                foreach(AttributeSyntax attribute in attributes)
                    attribute.GetSourceText(writer);
            }

            // Parameter type
            parameterType.GetSourceText(writer);

            // Identifier
            identifier.GetSourceText(writer);

            // Variable sized list
            if(enumerable != null)
            {
                enumerable.Value.GetSourceText(writer);
            }
        }

        public int GetPositionIndex()
        {
            // Try to find index
            if (parent is ParameterListSyntax parameterList)
                return parameterList.IndexOf(this);

            // Invalid index
            return -1;
        }
    }
}
