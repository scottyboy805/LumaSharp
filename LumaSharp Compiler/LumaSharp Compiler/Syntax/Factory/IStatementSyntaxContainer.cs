
namespace LumaSharp_Compiler.AST.Factory
{
    public interface IStatementSyntaxContainer : ISyntaxFactory
    {
        // Methods
        void AddStatement(StatementSyntax statement);
    }
}
