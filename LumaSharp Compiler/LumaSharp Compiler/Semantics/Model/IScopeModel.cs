using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Semantics.Model.Statement;

namespace LumaSharp_Compiler.Semantics.Model
{
    public interface IScopeModel
    {
        // Methods
        VariableModel DeclareScopedLocal(SemanticModel model, VariableDeclarationStatementSyntax syntax, int index);
    }
}
