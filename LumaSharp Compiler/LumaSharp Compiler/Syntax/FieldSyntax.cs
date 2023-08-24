
namespace LumaSharp_Compiler.Syntax
{
    public class FieldSyntax : MemberSyntax
    {
        // Private
        private TypeReferenceSyntax fieldType = null;
        //private Expression fieldAssignment = null;

        public override SyntaxToken StartToken => throw new NotImplementedException();

        public override SyntaxToken EndToken => throw new NotImplementedException();

        // Constructor
        internal FieldSyntax(LumaSharpParser.FieldDeclarationContext fieldDef)
            : base(fieldDef.IDENTIFIER(), fieldDef.accessModifier())
        {

        }

        public override void GetSourceText(TextWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
