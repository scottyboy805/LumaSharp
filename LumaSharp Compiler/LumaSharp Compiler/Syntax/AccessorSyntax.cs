
using Antlr4.Runtime.Tree;

namespace LumaSharp_Compiler.Syntax
{
    public class AccessorSyntax : MemberSyntax
    {
        // Private
        private TypeReferenceSyntax accessorType = null;
        private ExpressionSyntax assignValue = null;
        private SyntaxToken assignIdentifier = null;
        private AccessorBodySyntax readBody = null;
        private AccessorBodySyntax writeBody = null;

        public override SyntaxToken StartToken => throw new NotImplementedException();

        public override SyntaxToken EndToken => throw new NotImplementedException();

        public TypeReferenceSyntax AccessorType
        {
            get { return accessorType; }
        }

        public ExpressionSyntax AssignValue
        {
            get { return assignValue; }
        }

        public SyntaxToken AssignIdentifier
        {
            get { return assignIdentifier; }
        }

        public AccessorBodySyntax ReadBody
        {
            get { return readBody; }
        }

        public AccessorBodySyntax WriteBody
        {
            get { return writeBody; }
        }

        public bool HasAssignValue
        {
            get { return assignValue != null; }
        }

        public bool HasAssignIdentifier
        {
            get { return assignIdentifier != null; }
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
        internal AccessorSyntax(LumaSharpParser.AccessorDeclarationContext accessorDef, SyntaxTree tree, SyntaxNode parent)
            : base(accessorDef.IDENTIFIER(), tree, parent, accessorDef.attributeDeclaration(), accessorDef.accessModifier())
        {
            // Get the body
            LumaSharpParser.AccessorBodyContext body = accessorDef.accessorBody();

            // Check for assigned value
            LumaSharpParser.EndExpressionContext expression = body.endExpression();

            if (expression != null)
                ;//this.assignValue = new ExpressionSyntax(expression);

            // Check for assigned identifier
            ITerminalNode identifier = body.IDENTIFIER();

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
