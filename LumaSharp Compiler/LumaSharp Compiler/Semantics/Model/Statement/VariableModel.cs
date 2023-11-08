using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;
using LumaSharp_Compiler.Semantics.Model.Expression;

namespace LumaSharp_Compiler.Semantics.Model.Statement
{
    public sealed class VariableModel : StatementModel
    {
        // Private
        private VariableDeclarationStatementSyntax syntax = null;
        private ITypeReferenceSymbol variableTypeSymbol = null;
        private LocalOrParameterModel[] variableModels = null;
        private AssignModel[] assignModels = null;

        // Properties
        public ITypeReferenceSymbol VariableTypeSymbol
        {
            get { return variableTypeSymbol; }
        }

        public LocalOrParameterModel[] VariableModels
        {
            get { return variableModels; }
        }

        public AssignModel[] AssignModels
        {
            get { return assignModels; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield break; }
        }

        // Constructor
        public VariableModel(SemanticModel model, SymbolModel parent, VariableDeclarationStatementSyntax syntax, int index)
            : base(model, parent, syntax, index)
        {
            this.syntax = syntax;

            // Get models
            this.variableModels = new LocalOrParameterModel[syntax.IdentifierCount];

            // Get assign expression models
            for(int i = 0; i < syntax.IdentifierCount; i++)
            {
                variableModels[i] = new LocalOrParameterModel(syntax, parent as IReferenceSymbol, index, i, index);
            }

            if (syntax.HasAssignExpressions == true)
            {
                // Get assigns
                this.assignModels = new AssignModel[syntax.AssignExpressionCount];

                // Get assign models
                for (int i = 0; i < syntax.AssignExpressionCount; i++)
                {
                    assignModels[i] = new AssignModel(model, this, syntax, new VariableReferenceModel(model, this, new VariableReferenceExpressionSyntax(syntax.Identifiers[i].Text)), ExpressionModel.Any(model, this, syntax.AssignExpressions[i]), index);                    
                }
            }
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitVariable(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve type
            variableTypeSymbol = provider.ResolveTypeSymbol(ParentSymbol, syntax.VariableType);

            // Resolve all variable models
            for(int i = 0; i <  variableModels.Length; i++)
            {
                // Resolve symbols
                if (variableModels[i] != null)
                    variableModels[i].ResolveSymbols(provider, report);
            }

            // Resolve all assign models
            for(int i = 0; i < assignModels.Length; i++)
            {
                // Resolve symbols
                if (assignModels[i] != null)
                    assignModels[i].ResolveSymbols(provider, report);
            }
        }
    }
}
