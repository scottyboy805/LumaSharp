using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class ForModel : StatementModel, IScopeModel, IScopedReferenceSymbol, IReferenceSymbol
    {
        // Private
        private ForStatementSyntax syntax = null;
        private VariableModel variableModel = null;
        private ExpressionModel conditionModel = null;
        private ExpressionModel[] incrementModels = null;
        private StatementModel[] statements = null;
        private ILocalIdentifierReferenceSymbol[] localsInScope = null;

        // Properties
        public string ScopeName
        {
            get { return "For Statement"; }
        }

        public ILocalIdentifierReferenceSymbol[] LocalsInScope
        {
            get { return localsInScope; }
        }

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

        public ILibraryReferenceSymbol LibrarySymbol
        {
            get { return null; }
        }

        public _TokenHandle SymbolToken
        {
            get { return default; }
        }

        // Constructor
        public ForModel(SemanticModel model, SymbolModel parent, ForStatementSyntax syntax, int index)
            : base(model, parent, syntax, index)
        {
            this.syntax = syntax;

            // Variable
            if(syntax.HasVariable == true)
                this.variableModel = StatementModel.Any(model, this, syntax.Variable, StatementIndex, this) as VariableModel;

            // Condition
            if (syntax.HasCondition == true)
                this.conditionModel = ExpressionModel.Any(model, this, syntax.Condition);

            // Increment
            if (syntax.HasIncrements == true)
                this.incrementModels = syntax.Increments.Select(i => ExpressionModel.Any(model, this, i)).ToArray();

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
            // Create the variable
            VariableModel variableModel = new VariableModel(model, this, syntax, index);

            // Register locals
            LocalOrParameterModel[] localModels = variableModel.VariableModels.Where(m => m != null).ToArray();

            // Declare all
            if (localModels.Length > 0)
            {
                // Make sure array is allocated
                if (localsInScope == null)
                    localsInScope = Array.Empty<LocalOrParameterModel>();

                int startOffset = localsInScope.Length;

                // Resize the array
                Array.Resize(ref localsInScope, localsInScope.Length + localModels.Length);

                // Append elements
                for (int i = 0; i < localModels.Length; i++)
                {
                    localsInScope[startOffset + i] = localModels[i];
                }
            }
            return variableModel;
        }

        private void BuildSyntaxBlock(ForStatementSyntax syntax)
        {
            // Check for block
            if(syntax.Statement is StatementBlockSyntax statementBlock)
            {
                this.statements = statementBlock.Statements.Select((s, i) => StatementModel.Any(Model, this, s, StatementIndex + 1 + i, this)).ToArray();
            }
            else
            {
                this.statements = new StatementModel[] { StatementModel.Any(Model, this, syntax.Statement, StatementIndex + 1, this) };
            }
        }
    }
}
