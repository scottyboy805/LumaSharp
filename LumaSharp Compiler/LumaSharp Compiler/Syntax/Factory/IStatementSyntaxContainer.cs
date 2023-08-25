
namespace LumaSharp_Compiler.Syntax.Factory
{
    public interface IStatementSyntaxContainer : ISyntaxFactory
    {
        // Properties
        IEnumerable<StatementSyntax> Statements { get; }

        // Methods
        //void AddStatement(StatementSyntax statement);
    }
}
