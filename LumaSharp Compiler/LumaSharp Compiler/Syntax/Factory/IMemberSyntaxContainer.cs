
using LumaSharp_Compiler.AST.Factory;

namespace LumaSharp_Compiler.AST
{
    public interface IMemberSyntaxContainer : ISyntaxFactory
    {
        // Methods
        void AddMember(MemberSyntax member);
    }
}
