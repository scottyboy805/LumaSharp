using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class ConditionModel : StatementModel
    {
        // Private
        private readonly ExpressionModel conditionModel;
        private readonly ScopeModel scopeModel;        
        private readonly ConditionModel alternateModel;
        private ConditionModel alternateParentModel = null;

        // Properties
        public ExpressionModel Condition
        {
            get { return conditionModel; }
        }

        public ScopeModel Scope
        {
            get { return scopeModel; }
        }

        public ConditionModel Alternate
        {
            get { return alternateModel; }
        }

        public bool IsAlternate
        {
            get { return alternateParentModel != null; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get
            {
                if (conditionModel != null)
                    yield return conditionModel;

                // Get scope
                if(scopeModel != null)
                    yield return scopeModel;

                if (alternateModel != null)
                    yield return alternateModel;
            }
        }

        // Constructor
        public ConditionModel(ConditionStatementSyntax conditionSyntax)
            : base(conditionSyntax != null ? conditionSyntax.GetSpan() : null)
        {
            // Check for null
            if (conditionSyntax == null)
                throw new ArgumentNullException(nameof(conditionSyntax));

            // Create condition
            if(conditionSyntax.Condition != null)
                this.conditionModel = ExpressionModel.Any(conditionSyntax.Condition, this);

            // Create scope
            if (conditionSyntax.Statement != null)
            {
                this.scopeModel = new ScopeModel("Condition Body", conditionSyntax.Statement);
                this.scopeModel.parent = this;
            }

            // Check for alternate
            if(conditionSyntax.Alternate != null)
            {
                this.alternateModel = new ConditionModel(conditionSyntax.Alternate);
                this.alternateModel.alternateParentModel = this;
                this.alternateModel.parent = this;
            }
        }

        private ConditionModel(ExpressionModel condition, ScopeModel scope, ConditionModel alternate, SyntaxSpan? span)
            : base(span)
        {
            this.conditionModel = condition;
            this.scopeModel = scope;
            this.alternateModel = alternate;

            // Set alternate
            if (alternate != null)
                alternate.alternateParentModel = this;

            // Set parent
            if (this.conditionModel != null)
                this.conditionModel.parent = this;

            if (this.scopeModel != null)
                this.scopeModel.parent = this;

            if (this.alternateModel != null)
                this.alternateModel.parent = this;
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitCondition(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve condition symbols
            if(conditionModel != null)
            {
                conditionModel.ResolveSymbols(provider, report);
            }

            // Resolve statements
            if(scopeModel != null)
            {
                scopeModel.ResolveSymbols(provider, report);
            }

            // Resolve alternate
            if(alternateModel != null)
            {
                alternateModel.ResolveSymbols(provider, report);
            }
        }
    }
}
