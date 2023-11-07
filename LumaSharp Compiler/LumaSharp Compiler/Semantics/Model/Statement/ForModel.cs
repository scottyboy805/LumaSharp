using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.AST.Statement;
using LumaSharp_Compiler.Reporting;
using LumaSharp_Compiler.Semantics.Model.Expression;

namespace LumaSharp_Compiler.Semantics.Model.Statement
{
    public sealed class ForModel : StatementModel, IScopeModel
    {
        // Private
        private ForStatementSyntax syntax = null;
        private VariableModel variableModel = null;
        private ExpressionModel conditionModel = null;
        private ExpressionModel[] incrementModels = null;
        private StatementModel[] statements = null;

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

        public StatementModel[] Statements
        {
            get { return statements; }
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

                if(statements != null)
                {
                    foreach(StatementModel model in statements)
                        yield return model;
                }
            }
        }

        // Constructor
        public ForModel(SemanticModel model, SymbolModel parent, ForStatementSyntax syntax, int index)
            : base(model, parent, syntax, index)
        {
            this.syntax = syntax;

            // Variable
            if(syntax.HasForVariables == true)
                this.variableModel = StatementModel.Any(model, this, syntax.ForVariable, StatementIndex + 1) as VariableModel;

            // Condition
            if (syntax.HasForCondition == true)
                this.conditionModel = ExpressionModel.Any(model, this, syntax.ForCondition);

            // Increment
            if (syntax.HasForIncrements == true)
                this.incrementModels = syntax.ForIncrements.Select(i => ExpressionModel.Any(model, this, i)).ToArray();

            // Statements
            BuildSyntaxBlock(syntax);
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
            if(statements != null)
            {
                foreach(StatementModel statement in statements)
                {
                    statement.ResolveSymbols(provider, report);
                }
            }
        }

        public VariableModel DeclareScopedLocal(SemanticModel model, VariableDeclarationStatementSyntax syntax, int index)
        {
            throw new NotImplementedException();
        }

        private void BuildSyntaxBlock(ForStatementSyntax syntax)
        {
            // Check for inline
            if (syntax.HasInlineStatement == true)
            {
                this.statements = new StatementModel[] { StatementModel.Any(Model, this, syntax.InlineStatement, StatementIndex + 1, this) };
            }
            // Check for body
            else if (syntax.HasBlockStatement == true)
            {
                this.statements = syntax.BlockStatement.Elements.Select((s, i) => StatementModel.Any(Model, this, s, StatementIndex + 1 + i, this)).ToArray();
            }
        }
    }
}
