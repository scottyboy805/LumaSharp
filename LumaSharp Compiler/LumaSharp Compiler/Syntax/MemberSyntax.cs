
namespace LumaSharp_Compiler.Syntax
{
    public abstract class MemberSyntax : SyntaxNode
    {
        // Protected
        protected SyntaxToken identifier = null;

        // Properties
        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        // Constructor
        protected MemberSyntax(string identifier)
        {
            this.identifier = new SyntaxToken(identifier);
        }
    }
}
