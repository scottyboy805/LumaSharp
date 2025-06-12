
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class FieldSyntax : MemberSyntax
    {
        // Private
        private readonly TypeReferenceSyntax fieldType;
        private readonly VariableAssignmentExpressionSyntax fieldAssignment;
        private readonly SyntaxToken semicolon;

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

                return fieldType.StartToken;
            }
        }

        public override SyntaxToken EndToken
        {
            get { return semicolon; }
        }

        public TypeReferenceSyntax FieldType
        {
            get { return fieldType; }
        }

        public VariableAssignmentExpressionSyntax FieldAssignment
        {
            get { return fieldAssignment; }
        }

        public SyntaxToken Semicolon
        {
            get { return semicolon; }
        }

        public bool HasFieldAssignment
        {
            get { return fieldAssignment != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                // Field type
                yield return fieldType;

                // Field assignment
                if (HasFieldAssignment == true)
                    yield return fieldAssignment;
            }
        }

        // Constructor
        internal FieldSyntax(SyntaxToken identifier, AttributeSyntax[] attributes, SyntaxToken[] accessModifiers, TypeReferenceSyntax fieldType, VariableAssignmentExpressionSyntax fieldAssignment)
            : this(
                  identifier,
                  attributes,
                  accessModifiers,
                  fieldType,
                  fieldAssignment,
                  Syntax.Token(SyntaxTokenKind.SemicolonSymbol))
        {
        }

        internal FieldSyntax(SyntaxToken identifier, AttributeSyntax[] attributes, SyntaxToken[] accessModifiers, TypeReferenceSyntax fieldType, VariableAssignmentExpressionSyntax fieldAssignment, SyntaxToken semicolon)
            : base(identifier, attributes, accessModifiers)
        {
            // Check kind
            if(semicolon.Kind != SyntaxTokenKind.SemicolonSymbol)
                throw new ArgumentException(nameof(semicolon) + " must be of kind: " + SyntaxTokenKind.SemicolonSymbol);

            this.fieldType = fieldType;
            this.fieldAssignment = fieldAssignment;
            this.semicolon = semicolon;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitField(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Attributes
            base.GetSourceText(writer);

            // Field type
            fieldType.GetSourceText(writer);

            // Identifier
            identifier.GetSourceText(writer);

            // Assign
            if(HasFieldAssignment == true)
            {
                // Assign expression
                fieldAssignment.GetSourceText(writer);
            }

            // Semicolon
            semicolon.GetSourceText(writer);
        }
    }
}
