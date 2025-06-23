using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class VariableModel : StatementModel
    {
        // Private
        private readonly TypeReferenceModel variableTypeModel;
        private readonly LocalOrParameterModel[] variableModels;

        // Properties
        public TypeReferenceModel VariableTypeModel
        {
            get { return variableTypeModel; }
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
        public VariableModel(VariableDeclarationStatementSyntax variableSyntax)
            : base(variableSyntax != null ? variableSyntax.GetSpan() : null)
        {
            // Check for null
            if(variableSyntax == null)
                throw new ArgumentNullException(nameof(variableSyntax));

            this.variableTypeModel = new TypeReferenceModel(variableSyntax.VariableType);
            this.variableModels = LocalOrParameterModel.CreateLocalVariables(variableSyntax.Variable, this);
        }

        public VariableModel(VariableDeclarationSyntax variableSyntax)
            : base(variableSyntax != null ? variableSyntax.GetSpan() : null)
        {
            // Check for null
            if (variableSyntax == null)
                throw new ArgumentNullException(nameof(variableSyntax));

            this.variableTypeModel = new TypeReferenceModel(variableSyntax.VariableType);
            this.variableModels = LocalOrParameterModel.CreateLocalVariables(variableSyntax, this);
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitVariable(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve type
            variableTypeModel.ResolveSymbols(provider, report);

            // Resolve all variable models
            for(int i = 0; i <  variableModels.Length; i++)
            {
                // Resolve symbols
                variableModels[i].ResolveSymbols(provider, report);
            }
        }
    }
}
