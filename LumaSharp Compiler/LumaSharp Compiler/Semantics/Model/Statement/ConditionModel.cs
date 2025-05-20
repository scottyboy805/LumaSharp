using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class ConditionModel : StatementModel, IScopeModel
    {
        // Private
        private ConditionStatementSyntax syntax = null;
        private ConditionModel parentConditionModel = null;
        private ExpressionModel conditionModel = null;
        private StatementModel[] statements = null;
        private ConditionModel alternateModel = null;

        // Properties
        public ConditionModel ParentCondition
        {
            get { return parentConditionModel; }
        }

        public ExpressionModel Condition
        {
            get { return conditionModel; }
        }

        public StatementModel[] Statements
        {
            get { return statements; }
        }

        public ConditionModel Alternate
        {
            get { return alternateModel; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get
            {
                if (conditionModel != null)
                    yield return conditionModel;

                foreach (StatementModel statement in statements)
                    yield return statement;

                if (alternateModel != null)
                    yield return alternateModel;
            }
        }

        // Constructor
        public ConditionModel(SemanticModel model, SymbolModel parent, ConditionStatementSyntax syntax, int index)
            : base(model, parent, syntax, index)
        {
            this.syntax = syntax;

            // Condition
            if(syntax.Condition != null)
                this.conditionModel = ExpressionModel.Any(model, this, syntax.Condition);

            // Statements
            BuildSyntaxBlock(syntax);

            // Alternate
            if (syntax.Alternate != null)
                this.alternateModel = new ConditionModel(model, this, this, syntax.Alternate, StatementIndex + 1);
        }

        private ConditionModel(SemanticModel model, SymbolModel parent, ConditionModel parentCondition, ConditionStatementSyntax alternate, int index)
            : base(model, parent, alternate, index)
        {
            this.syntax = alternate;
            this.parentConditionModel = parentCondition;

            // Condition
            if(alternate.Condition != null)
                this.conditionModel = ExpressionModel.Any(model, this, alternate.Condition);

            // Statements
            BuildSyntaxBlock(syntax);

            // Alternate
            if(syntax.Alternate != null)
                this.alternateModel = new ConditionModel(model, this, this, alternate.Alternate, StatementIndex + 1);
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
            if(statements != null)
            {
                foreach(StatementModel statement in statements)
                {
                    statement.ResolveSymbols(provider, report);
                }
            }

            // Resolve alternate
            if(alternateModel != null)
            {
                alternateModel.ResolveSymbols(provider, report);
            }
        }

        public VariableModel DeclareScopedLocal(SemanticModel model, VariableDeclarationStatementSyntax syntax, int index)
        {
            throw new NotImplementedException();
        }

        private void BuildSyntaxBlock(ConditionStatementSyntax syntax)
        {
            // Check for inline
            if(syntax.Statement is StatementBlockSyntax statementBlock)
            {
                this.statements = statementBlock.Statements.Select((s, i) => StatementModel.Any(Model, this, s, StatementIndex + 1 + i, this)).ToArray();
                
            }
            // Check for body
            else
            {
                this.statements = new StatementModel[] { StatementModel.Any(Model, this, syntax.Statement, StatementIndex + 1, this) };
            }
        }
    }
}
