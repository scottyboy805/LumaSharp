
namespace LumaSharp_Compiler.Syntax
{
    public sealed class AttributeSyntax : SyntaxNode
    {
        // Private
        private TypeReferenceSyntax attributeType = null;
        private ExpressionSyntax[] attributeExpressions = null;

        // Properties
        public TypeReferenceSyntax AttributeType
        {
            get { return attributeType; }
        }

        public ExpressionSyntax[] AttributeExpressions
        {
            get { return attributeExpressions; }
        }

        public int AttributeExpressionCount
        {
            get { return HasAttributeExpressions ? attributeExpressions.Length : 0; }
        }

        public bool HasAttributeExpressions
        {
            get { return attributeExpressions != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                // Type
                yield return attributeType;

                // Expressions
                if(HasAttributeExpressions == true)
                {
                    foreach (ExpressionSyntax expression in attributeExpressions)
                        yield return expression;
                }
            }
        }

        // Constructor
        internal AttributeSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.AttributeDeclarationContext attribDef)
            : base(tree, parent, attribDef)
        {            
            // Create attribute type
            this.attributeType = new TypeReferenceSyntax(tree, this, attribDef.typeReference());
        }

        // Methods

        public override void GetSourceText(TextWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
