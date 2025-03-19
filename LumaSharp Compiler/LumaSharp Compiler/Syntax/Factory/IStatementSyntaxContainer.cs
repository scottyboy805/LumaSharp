
namespace LumaSharp.Compiler.AST
{
    public interface IStatementSyntaxContainer
    {
        // Properties
        SyntaxTree SyntaxTree { get; }

        SyntaxNode Parent { get; }

        // Methods
        void AddStatement(StatementSyntax statement);
    }
}
