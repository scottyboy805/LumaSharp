
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

        // Properties
        public TypeReferenceSyntax AccessorType
        {
            get { return accessorType; }
        }

        public ExpressionSyntax AssignExpression
        {
            get { return assignExpression; }
        }

        public AccessorBodySyntax ReadBody
        {
            get { return readBody; }
        }

        public AccessorBodySyntax WriteBody
        {
            get { return writeBody; }
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
        internal AccessorSyntax(string identifier, TypeReferenceSyntax type)
            : base(identifier)
        {
            // Accessor type
            this.accessorType = type;
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
            throw new NotImplementedException();
        }
    }
}
