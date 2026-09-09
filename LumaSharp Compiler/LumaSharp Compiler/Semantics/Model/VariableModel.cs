using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Semantics.Model
{
    internal sealed class VariableModel : IVariableSymbol
    {
        // Private
        private readonly SemanticModel model;
        private readonly VariableDeclarationSyntax variable;
        private readonly int variableIndex;

        // Properties
        public bool IsConst => false;
        public bool IsAssigned => variable.HasAssignment;
        public string Name => variable.IdentifierNames[variableIndex].Text;
        public SyntaxSpan? Source => variable.GetSpan();

        // Constructor
        public VariableModel(SemanticModel model, VariableDeclarationSyntax variable, int variableIndex)
        {
            this.model = model;
            this.variable = variable;
            this.variableIndex = variableIndex;
        }

        // Methods
        public IMethodSymbol GetDeclaringMethod()
        {
            // Try to declaring method symbol
            return model.GetSymbol(variable.ParentOfType<MethodSyntax>()).Symbol as IMethodSymbol;
        }

        public ITypeSymbol GetVariableType()
        {
            // Get the type symbol
            return model.GetTypeSymbol(variable.VariableType);
        }
    }
}
