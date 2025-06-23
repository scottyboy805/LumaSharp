using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class ForModel : StatementModel
    {
        // Private
        private readonly VariableModel variableModel = null;
        private readonly ExpressionModel conditionModel = null;
        private readonly ExpressionModel[] incrementModels = null;
        private readonly ScopeModel scopeModel;
        private ILocalIdentifierReferenceSymbol[] localsInScope = null;

        // Properties
        public VariableModel Variable
        {
            get { return variableModel; }
        }

        public ExpressionModel Condition
        {
            get { return conditionModel; }
        }

        public ExpressionModel[] IncrementModels
        {
            get { return incrementModels; }
        }

        public ScopeModel Scope
        {
            get { return scopeModel; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get
            {
                if (variableModel != null)
                    yield return variableModel;

                if (conditionModel != null)
                    yield return conditionModel;

                if(incrementModels != null)
                {
                    foreach(ExpressionModel model in incrementModels)
                        yield return model;
                }

                if (scopeModel != null)
                    yield return scopeModel;
            }
        }

        // Constructor
        public ForModel(ForStatementSyntax forSyntax)
            : base(forSyntax != null ? forSyntax.GetSpan() : null)
        {
            // Check for null
            if(forSyntax == null)
                throw new ArgumentNullException(nameof(forSyntax));

            // Create variable
            if (forSyntax.HasVariable == true)
            {
                this.variableModel = new VariableModel(forSyntax.Variable);
                this.variableModel.parent = this;
            }

            // Create condition
            if (forSyntax.HasCondition == true)
                this.conditionModel = ExpressionModel.Any(forSyntax.Condition, this);

            // Create increments
            if (forSyntax.HasIncrements == true)
                this.incrementModels = forSyntax.Increments.Select(i => ExpressionModel.Any(i, this)).ToArray();

            // Create scope
            if (forSyntax.Statement != null)
            {
                this.scopeModel = new ScopeModel("For Body", forSyntax.Statement);
                this.scopeModel.parent = this;
            }
        }

        public ForModel(VariableModel variable, ExpressionModel condition, ExpressionModel[] increments, ScopeModel scope, SyntaxSpan? span)
            : base(span)
        {
            this.variableModel = variable;
            this.conditionModel = condition;
            this.incrementModels = increments;
            this.scopeModel = scope;

            // Set parent
            if (this.variableModel != null)
                this.variableModel.parent = this;

            if(this.conditionModel != null)
                this.conditionModel.parent = this;

            if(this.incrementModels != null)
            {
                foreach (ExpressionModel increment in incrementModels)
                    increment.parent = this;
            }

            if(this.scopeModel != null)
                this.scopeModel.parent = this;
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitFor(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve variable symbols
            if(variableModel != null)
            {
                variableModel.ResolveSymbols(provider, report);
            }

            // Resolve condition symbols
            if(conditionModel != null)
            {
                conditionModel.ResolveSymbols(provider, report);
            }

            // Resolve increments
            if(incrementModels != null)
            {
                foreach(ExpressionModel model in incrementModels)
                {
                    model.ResolveSymbols(provider, report);
                }
            }

            // Resolve statements
            if(scopeModel != null)
            {
                scopeModel.ResolveSymbols(provider, report);
            }
        }
    }
}
