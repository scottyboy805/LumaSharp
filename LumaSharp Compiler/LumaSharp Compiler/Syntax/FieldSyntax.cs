
namespace LumaSharp_Compiler.AST
{
    public class FieldSyntax : MemberSyntax
    {
        // Private
        private TypeReferenceSyntax fieldType = null;
        private SyntaxToken assign = null;
        private ExpressionSyntax fieldAssignment = null;

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
        internal FieldSyntax(string identifier, TypeReferenceSyntax fieldType)
            : base(identifier, fieldType.StartToken, SyntaxToken.Semi())
        {
            this.fieldType = fieldType;
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
            // Write attributes
            //if(HasAt)

            // Write access modifiers
            if (HasAccessModifiers == true)
            {
                for(int i = 0; i < accessModifiers.Length; i++)
                {
                    writer.Write(accessModifiers[i].ToString());
                    writer.Write(' ');
                }
            }
        }
    }
}
