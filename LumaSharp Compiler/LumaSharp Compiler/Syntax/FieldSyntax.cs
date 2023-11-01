
namespace LumaSharp_Compiler.AST
{
    public class FieldSyntax : MemberSyntax
    {
        // Private
        private TypeReferenceSyntax fieldType = null;        
        private ExpressionSyntax fieldAssignment = null;
        private SyntaxToken reference = null;
        private SyntaxToken assign = null;
        private SyntaxToken semicolon = null;

        // Properties
        public TypeReferenceSyntax FieldType
        {
            get { return fieldType; }
        }

        public ExpressionSyntax FieldAssignment
        {
            get { return fieldAssignment; }
        }

        public bool HasFieldAssignment
        {
            get { return fieldAssignment != null; }
        }

        public bool IsByReference
        {
            get { return reference != null; }
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
        internal FieldSyntax(string identifier, TypeReferenceSyntax fieldType, ExpressionSyntax fieldAssign, bool byReference)
            : base(identifier, fieldType.StartToken, SyntaxToken.Semi())
        {
            this.fieldType = fieldType;
            this.fieldAssignment = fieldAssign;
            this.identifier.WithLeadingWhitespace(" ");
            this.reference = (byReference != null) ? SyntaxToken.Reference() : null;
            this.assign = SyntaxToken.Assign();
            this.semicolon = base.EndToken;
        }

        internal FieldSyntax(SyntaxTree tree, SyntaxNode node, LumaSharpParser.FieldDeclarationContext fieldDef)
            : base(fieldDef.IDENTIFIER(), tree, node, fieldDef, fieldDef.attributeDeclaration(), fieldDef.accessModifier())
        {
            // Create type reference
            this.fieldType = new TypeReferenceSyntax(tree, this, fieldDef.typeReference());

            // Check for assign
            LumaSharpParser.FieldAssignmentContext assignment = fieldDef.fieldAssignment();

            if(assignment != null)
            {
                // Get assign
                this.assign = new SyntaxToken(assignment.assign);

                // Get expression
                this.fieldAssignment = ExpressionSyntax.Any(tree, this, assignment.expression());
            }
        }

        internal FieldSyntax(SyntaxTree tree, SyntaxNode parent, TypeReferenceSyntax enumType, LumaSharpParser.EnumFieldContext enumField)
            : base(enumField.IDENTIFIER(), tree, parent, enumField, enumField.attributeDeclaration(), null)
        {
            // Create field type
            this.fieldType = enumType;

            // Check for assignment
            LumaSharpParser.FieldAssignmentContext assignment = enumField.fieldAssignment();

            if(assignment != null)
            {
                // Get assign
                this.assign = new SyntaxToken(assignment.assign);

                // Get expression
                this.fieldAssignment = ExpressionSyntax.Any(tree, this, assignment.expression());
            }
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Attributes
            base.GetSourceText(writer);

            // Field type
            fieldType.GetSourceText(writer);

            // Check for by reference
            if(IsByReference == true)
            {
                reference.GetSourceText(writer);
            }

            // Identifier
            identifier.GetSourceText(writer);

            // Assign
            if(HasFieldAssignment == true)
            {
                // Assign op
                assign.GetSourceText(writer);

                // Assign expression
                fieldAssignment.GetSourceText(writer);
            }

            // End
            semicolon.GetSourceText(writer);
        }
    }
}
