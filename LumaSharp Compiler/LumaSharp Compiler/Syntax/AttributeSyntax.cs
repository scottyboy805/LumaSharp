
namespace LumaSharp_Compiler.AST
{
    public sealed class AttributeSyntax : SyntaxNode
    {
        // Private
        private SyntaxToken hash = null;
        private TypeReferenceSyntax attributeType = null;
        private ExpressionSyntax[] attributeExpressions = null;
        private SyntaxToken lparen = null;
        private SyntaxToken rparen = null;
        private SyntaxToken comma = null;

        // Properties
        public override SyntaxToken EndToken
        {
            get
            {
                if(HasAttributeExpressions == true)
                {
                    return attributeExpressions[attributeExpressions.Length - 1].EndToken;
                }
                return base.EndToken;
            }
        }

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
            get { return attributeExpressions != null && attributeExpressions.Length > 0; }
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
        internal AttributeSyntax(TypeReferenceSyntax attributeType, ExpressionSyntax[] expressions)
            : base(SyntaxToken.Hash(), attributeType.EndToken)
        {
            this.hash = base.StartToken;
            this.attributeType = attributeType;
            this.attributeExpressions = expressions;

            if (expressions.Length > 0)
            {
                this.lparen = SyntaxToken.LParen();
                this.rparen = SyntaxToken.RParen();
                this.comma = SyntaxToken.Comma();
                expressions[expressions.Length - 1].EndToken.WithTrailingWhitespace(" ");
            }
            else
            {
                attributeType.EndToken.WithTrailingWhitespace(" ");
            }
        }

        internal AttributeSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.AttributeDeclarationContext attribDef)
            : base(tree, parent, attribDef)
        {            
            // Create attribute type
            this.attributeType = new TypeReferenceSyntax(tree, this, attribDef.typeReference());
        }

        // Methods

        public override void GetSourceText(TextWriter writer)
        {
            // Write hash
            hash.GetSourceText(writer);

            // Write type
            attributeType.GetSourceText(writer);

            // Check for expressions
            if(HasAttributeExpressions == true)
            {
                // lparen
                lparen.GetSourceText(writer);

                // Expressions
                for(int i = 0; i < attributeExpressions.Length; i++)
                {
                    // Write expression
                    attributeExpressions[i].GetSourceText(writer);

                    // Write comma
                    if (i < attributeExpressions.Length - 1)
                        comma.GetSourceText(writer);
                }

                // rparen
                rparen.GetSourceText(writer);
            }
        }
    }
}
