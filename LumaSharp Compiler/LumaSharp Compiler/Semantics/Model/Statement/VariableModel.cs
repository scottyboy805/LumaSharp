using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
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
            this.variableModels = new LocalOrParameterModel[syntax.Identifiers.Count];

            // Get assign expression models
            for(int i = 0; i < syntax.Identifiers.Count; i++)
            {
                variableModels[i] = new LocalOrParameterModel(syntax, parent as IReferenceSymbol, index, i, index);
            }

            if (syntax.HasAssignment == true)
            {
                // Get assigns
                this.assignModels = new AssignModel[syntax.Assignment.AssignExpressions.Count];

                // Get assign models
                for (int i = 0; i < syntax.Assignment.AssignExpressions.Count; i++)
                {
                    assignModels[i] = new AssignModel(model, this, syntax, new VariableReferenceModel(model, this, 
                        new VariableReferenceExpressionSyntax(syntax.Identifiers[i])), 
                        ExpressionModel.Any(model, this, syntax.Assignment.AssignExpressions[i]), index);                    
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

            if (assignModels != null)
            {
                // Resolve all assign models
                for (int i = 0; i < assignModels.Length; i++)
                {
                    // Resolve symbols
                    if (assignModels[i] != null)
                        assignModels[i].ResolveSymbols(provider, report);
                }
            }
        }
    }
}
