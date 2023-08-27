
namespace LumaSharp_Compiler.Syntax
{
    public class FieldSyntax : MemberSyntax
    {
        // Private
        private TypeReferenceSyntax fieldType = null;
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
        internal FieldSyntax(LumaSharpParser.FieldDeclarationContext fieldDef, SyntaxTree tree, SyntaxNode node)
            : base(fieldDef.IDENTIFIER(), tree, node, fieldDef, fieldDef.attributeDeclaration(), fieldDef.accessModifier())
        {
            // Create type reference
            this.fieldType = new TypeReferenceSyntax(tree, this, fieldDef.typeReference());
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
