using LumaSharp_Compiler.Semantics.Model.Statement;
using LumaSharp_Compiler.Semantics.Visitor;

namespace LumaSharp_Compiler.Emit.Builder
{
    internal class MethodBodyBuilder : SemanticVisitor
    {
        // Private
        private StatementModel[] statements = null;

        // Constructor
        public MethodBodyBuilder(StatementModel[] statements)
        {
            this.statements = statements;
        }

        // Methods

    }
}
