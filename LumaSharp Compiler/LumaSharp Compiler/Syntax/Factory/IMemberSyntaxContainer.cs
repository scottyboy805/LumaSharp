
namespace LumaSharp.Compiler.AST
{
    public interface IMemberSyntaxContainer
    {
        // Properties
        SyntaxTree SyntaxTree { get; }

        SyntaxNode Parent { get; }

        // Methods
        void AddMember(MemberSyntax member);
    }
}
