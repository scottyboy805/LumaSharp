
using Antlr4.Runtime.Tree;

namespace LumaSharp_Compiler.AST
{
    public class AccessorSyntax : MemberSyntax
    {
        // Private
        private TypeReferenceSyntax accessorType = null;
        private ExpressionSyntax assignExpression = null;
        private AccessorBodySyntax readBody = null;
        private AccessorBodySyntax writeBody = null;
        private SyntaxToken lambda = null;
        private SyntaxToken colon = null;
        private SyntaxToken semicolon = null;

        // Properties
        public override SyntaxToken EndToken
        {
            get
            {
                // Check for read
                if(HasReadBody == true)
                    return readBody.EndToken;

                // Check for write
                if(HasWriteBody == true)
                    return writeBody.EndToken;

                return base.EndToken;
            }
        }

        public TypeReferenceSyntax AccessorType
        {
            get { return accessorType; }
        }

        public ExpressionSyntax AssignExpression
        {
            get { return assignExpression; }
            internal set { assignExpression = value; }
        }

        public AccessorBodySyntax ReadBody
        {
            get { return readBody; }
            internal set { readBody = value; }
        }

        public AccessorBodySyntax WriteBody
        {
            get { return writeBody; }
            internal set { writeBody = value; }
        }

        public bool HasAssignExpression
        {
            get { return assignExpression != null; }
        }

        public bool HasReadBody
        {
            get { return readBody != null; }
        }

        public bool HasWriteBody
        {
            get { return writeBody != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal AccessorSyntax(string identifier, TypeReferenceSyntax type, ExpressionSyntax assignExpression)
            : base(identifier, type.StartToken, SyntaxToken.Semi())
        {
            // Accessor type
            this.accessorType = type;
            this.assignExpression = assignExpression;
            this.identifier.WithLeadingWhitespace(" ");
            this.lambda = SyntaxToken.Lambda();
            this.colon = SyntaxToken.Colon();
            this.semicolon = base.EndToken;
        }

        internal AccessorSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.AccessorDeclarationContext accessorDef)
            : base(accessorDef.IDENTIFIER(), tree, parent, accessorDef, accessorDef.attributeDeclaration(), accessorDef.accessModifier())
        {
            // Accessor type
            this.accessorType = new TypeReferenceSyntax(tree, this, accessorDef.typeReference());

            // Get the body
            LumaSharpParser.AccessorBodyContext body = accessorDef.accessorBody();

            // Check for assigned value
            LumaSharpParser.ExpressionContext expression = body.expression();

            if (expression != null)
                this.assignExpression = ExpressionSyntax.Any(tree, this, expression);

            // Check for assigned identifier
            ITerminalNode identifier = accessorDef.IDENTIFIER();

            if (identifier != null)
                this.identifier = new SyntaxToken(identifier);

            // Check for read body
            LumaSharpParser.AccessorReadContext read = body.accessorRead();

            if(read != null)
            {
                this.readBody = new AccessorBodySyntax(tree, this, read);
            }

            // Check for write body
            LumaSharpParser.AccessorWriteContext write = body.accessorWrite();

            if(write != null)
            {
                this.writeBody = new AccessorBodySyntax(tree, this, write);
            }

        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Attributes
            base.GetSourceText(writer);

            // Type
            accessorType.GetSourceText(writer);

            // Identifier
            identifier.GetSourceText(writer);

            // Check for assignments
            if (HasAssignExpression == true)
            {
                // Lambda
                lambda.GetSourceText(writer);

                // Expression
                assignExpression.GetSourceText(writer);

                // Semi
                semicolon.GetSourceText(writer);
            }
            else if (HasReadBody == true || HasWriteBody == true)
            {
                // Check for read
                if(HasReadBody == true)
                {
                    // Lambda
                    lambda.GetSourceText(writer);

                    // Read
                    readBody.GetSourceText(writer);
                }

                // Check for write
                if(HasWriteBody == true)
                {
                    // Lambda
                    lambda.GetSourceText(writer);

                    // Write
                    writeBody.GetSourceText(writer);
                }
            }
            else
            {
                // No inline or body - empty accessor
                semicolon.GetSourceText(writer);
            }            
        }
    }
}
