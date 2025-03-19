
namespace LumaSharp.Compiler.AST
{
    public sealed class FieldSyntax : MemberSyntax
    {
        // Private
        private readonly TypeReferenceSyntax fieldType;
        private readonly VariableAssignExpressionSyntax fieldAssignment;
        private readonly SyntaxToken comma;                 // Required if there is another member following immediately

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
            get
            {
                // Check for comma
                if (HasComma == true)
                    return comma;

                // Check for assignment
                if (HasFieldAssignment == true)
                    return fieldAssignment.EndToken;

                return identifier;
            }
        }

        public TypeReferenceSyntax FieldType
        {
            get { return fieldType; }
        }

        public VariableAssignExpressionSyntax FieldAssignment
        {
            get { return fieldAssignment; }
        }

        public SyntaxToken Comma
        {
            get { return comma; }
        }

        public bool HasFieldAssignment
        {
            get { return fieldAssignment != null; }
        }

        public bool HasComma
        {
            get { return comma.Kind != SyntaxTokenKind.Invalid; }
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
        internal FieldSyntax(SyntaxNode parent, string identifier, AttributeReferenceSyntax[] attributes, SyntaxToken[] accessModifiers, TypeReferenceSyntax fieldType, VariableAssignExpressionSyntax fieldAssignment)
            : base(parent, identifier, attributes, accessModifiers)
        {
            this.fieldType = fieldType;
            this.fieldAssignment = fieldAssignment;
        }

        internal FieldSyntax(SyntaxNode node, LumaSharpParser.FieldDeclarationContext fieldDef)
            : base(fieldDef.IDENTIFIER(), node, fieldDef.attributeReference(), fieldDef.accessModifier())
        {
            // Create type reference
            this.fieldType = new TypeReferenceSyntax(this, null, fieldDef.typeReference());

            // Check for assign
            LumaSharpParser.VariableAssignmentContext assignment = fieldDef.variableAssignment();

            if(assignment != null)
            {
                this.fieldAssignment = new VariableAssignExpressionSyntax(this, assignment);
            }

            // Comma
            if(fieldDef.COMMA() != null)
                this.comma = new SyntaxToken(SyntaxTokenKind.CommaSymbol, fieldDef.COMMA());
        }

        internal FieldSyntax(SyntaxNode parent, TypeReferenceSyntax enumType, LumaSharpParser.EnumFieldContext enumField)
            : base(enumField.IDENTIFIER(), parent, enumField.attributeReference(), null)
        {
            // Create field type
            this.fieldType = enumType;

            // Check for assignment
            LumaSharpParser.VariableAssignmentContext assignment = enumField.variableAssignment();

            if(assignment != null)
            {
                this.fieldAssignment = new VariableAssignExpressionSyntax(this, assignment);
            }

            // Comma
            this.comma = new SyntaxToken(SyntaxTokenKind.CommaSymbol, enumField.COMMA());
        }

        // Methods
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

            // Comma
            if (HasComma == true)
                comma.GetSourceText(writer);
        }
    }
}
