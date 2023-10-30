using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;

namespace LumaSharp_Compiler.Semantics.Model.Statement
{
    public sealed class VariableModel : StatementModel
    {
        // Private
        private VariableDeclarationStatementSyntax syntax = null;
        private ITypeReferenceSymbol variableTypeSymbol = null;
        private LocalOrParameterModel[] variableModels = null;

        // Properties
        public ITypeReferenceSymbol VariableTypeSymbol
        {
            get { return variableTypeSymbol; }
        }

        public LocalOrParameterModel[] VariableModels
        {
            get { return variableModels; }
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
            for(int i = 0; i < syntax.AssignExpressionCount; i++)
            {
                if (i < variableModels.Length)
                    variableModels[i] = new LocalOrParameterModel(syntax, parent as IReferenceSymbol, i, i);
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

            // Resolve all assign models
            for(int i = 0; i <  variableModels.Length; i++)
            {
                // Resolve symbols
                if (variableModels[i] != null)
                    variableModels[i].ResolveSymbols(provider, report);
            }
        }
    }
}
