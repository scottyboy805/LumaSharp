using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.AST.Statement;
using LumaSharp_Compiler.Reporting;
using LumaSharp_Compiler.Semantics.Model.Expression;

namespace LumaSharp_Compiler.Semantics.Model.Statement
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
            this.conditionModel = ExpressionModel.Any(model, this, syntax.ConditionExpression);

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
            if(alternate.ConditionExpression != null)
                this.conditionModel = ExpressionModel.Any(model, this, alternate.ConditionExpression);

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

        private void BuildSyntaxBlock(ConditionStatementSyntax syntax)
        {
            // Check for inline
            if(syntax.HasInlineStatement == true)
            {
                this.statements = new StatementModel[] { StatementModel.Any(Model, this, syntax.InlineStatement, StatementIndex + 1, this) };
            }
            // Check for body
            else if(syntax.HasBlockStatement == true)
            {
                this.statements = syntax.BlockStatement.Elements.Select((s, i) => StatementModel.Any(Model, this, s, StatementIndex + 1 + i, this)).ToArray();
            }
        }

        public VariableModel DeclareScopedLocal(SemanticModel model, VariableDeclarationStatementSyntax syntax, int index)
        {
            throw new NotImplementedException();
        }
    }
}
