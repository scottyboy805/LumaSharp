using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class VariableReferenceModel : ExpressionModel
    {
        // Private
        private VariableReferenceExpressionSyntax syntax = null;
        private IIdentifierReferenceSymbol identifierSymbol = null;

        // Properties
        public string Identifier
        {
            get { return syntax.Identifier.Text; }
        }

        public override bool IsStaticallyEvaluated
        {
            get { return false; }
        }

        public IIdentifierReferenceSymbol IdentifierSymbol
        {
            get { return identifierSymbol; }
        }

        public override ITypeReferenceSymbol EvaluatedTypeSymbol
        {
            get
            {
                if (identifierSymbol != null)
                    return identifierSymbol.TypeSymbol;

                return null;
            }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal VariableReferenceModel(SemanticModel model, SymbolModel parent, VariableReferenceExpressionSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitVariableReference(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Try to resolve symbol
            this.identifierSymbol = provider.ResolveIdentifierSymbol(ParentSymbol, syntax);

            // Check for symbol resolved
            if(identifierSymbol != null && identifierSymbol is ILocalIdentifierReferenceSymbol localIdentifier)
            {
                // Get base statements
                SymbolModel current = this;

                while ((current is StatementModel) == false && current.Parent != null)
                    current = current.Parent;

                // Check for local
                if(current != null && localIdentifier.IsLocal == true && localIdentifier.DeclareIndex > ((StatementModel)current).StatementIndex)
                {
                    report.ReportDiagnostic(Code.IdentifierUsedBeforeDeclared, MessageSeverity.Error, syntax.StartToken.Source, syntax.Identifier.Text);
                }
            }
        }
    }
}
