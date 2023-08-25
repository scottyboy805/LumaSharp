
using LumaSharp_Compiler.Syntax.Factory;

namespace LumaSharp_Compiler.Syntax
{
    public interface IMemberSyntaxContainer : ISyntaxFactory
    {
        // Properties
        IEnumerable<MemberSyntax> Members { get; }

        // Methods
        //void AddMember(MemberSyntax member);
    }
}
