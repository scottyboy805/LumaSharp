using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Semantics.Model
{
    public interface IScopeModel
    {
        // Methods
        VariableModel DeclareScopedLocal(SemanticModel model, VariableDeclarationStatementSyntax syntax, int index);
    }
}
