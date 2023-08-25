
namespace LumaSharp_Compiler.Syntax.Factory
{
    public interface IRootMemberSyntaxContainer : ISyntaxFactory
    {
        // Properties
        IEnumerable<MemberSyntax> RootMembers { get; }

        // Methods
        //void AddMember(MemberSyntax member);
    }
}
