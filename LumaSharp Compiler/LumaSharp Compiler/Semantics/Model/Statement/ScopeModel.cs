using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class ScopeModel : StatementModel, IScopedReferenceSymbol
    {
        // Private
        private readonly string scopeName;
        private readonly StatementModel[] statements;
        private ILocalIdentifierReferenceSymbol[] localsInScope = null;        

        // Properties
        public string ScopeName
        {
            get { return scopeName; }
        }

        public StatementModel[] Statements
        {
            get { return statements; }
        }

        public bool HasStatements
        {
            get { return statements != null && statements.Length > 0; }
        }

        public ILocalIdentifierReferenceSymbol[] LocalsInScope
        {
            get { return localsInScope; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get
            {
                if(statements != null)
                {
                    foreach (StatementModel statement in statements)
                        yield return statement;
                }
            }
        }

        // Constructor
        public ScopeModel(string scopeName, StatementSyntax statementSyntax)
            : base(statementSyntax != null ? statementSyntax.GetSpan() : null)
        {
            this.scopeName = scopeName;
            this.statements = statementSyntax != null && statementSyntax is not EmptyStatementSyntax
                ? statementSyntax is StatementBlockSyntax block
                    ? block.Statements.Select(s => StatementModel.Any(s, this)).ToArray()
                    : new[] { StatementModel.Any(statementSyntax, this) }
                : null;
        }

        public ScopeModel(string scopeName, IEnumerable<StatementModel> statements, SyntaxSpan? span)
            : base(span)
        {
            this.scopeName = scopeName;
            this.statements = statements != null
                ? statements.ToArray()
                : Array.Empty<StatementModel>();
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitScope(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Visit all statements
            if(HasStatements == true)
            {
                // Resolve statements
                foreach(StatementModel statement in statements)
                    statement.ResolveSymbols(provider, report);
            }

            // Resolve locals
            localsInScope = HasStatements == true
                ? statements.OfType<VariableModel>()
                    .SelectMany(v => v.VariableModels)
                    .ToArray()
                : Array.Empty<ILocalIdentifierReferenceSymbol>();



            // Check for any locals
            if (localsInScope != null)
            {
                // Check for multiple symbols
                HashSet<string> definedLocals = new HashSet<string>();

                for (int i = 0; i < localsInScope.Length; i++)
                {
                    // Add the local identifier
                    if (definedLocals.Contains(localsInScope[i].IdentifierName) == false)
                    {
                        definedLocals.Add(localsInScope[i].IdentifierName);
                    }
                    else
                    {
                        report.ReportDiagnostic(Code.MultipleLocalIdentifiers, MessageSeverity.Error, ((LocalOrParameterModel)localsInScope[i]).Span, localsInScope[i].IdentifierName);
                    }
                }
            }
        }
    }
}
