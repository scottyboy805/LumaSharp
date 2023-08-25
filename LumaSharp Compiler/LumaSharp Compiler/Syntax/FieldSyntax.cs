
namespace LumaSharp_Compiler.Syntax
{
    public class FieldSyntax : MemberSyntax
    {
        // Private
        private TypeReferenceSyntax fieldType = null;
        //private Expression fieldAssignment = null;

        public override SyntaxToken StartToken => throw new NotImplementedException();

        public override SyntaxToken EndToken => throw new NotImplementedException();

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal FieldSyntax(LumaSharpParser.FieldDeclarationContext fieldDef, SyntaxTree tree, SyntaxNode node)
            : base(fieldDef.IDENTIFIER(), tree, node, fieldDef.accessModifier())
        {

        }

        public override void GetSourceText(TextWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
