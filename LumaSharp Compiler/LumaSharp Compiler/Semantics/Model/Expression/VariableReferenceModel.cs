using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class VariableReferenceModel : ExpressionModel
    {
        // Private
        private readonly StringModel identifier;
        private IIdentifierReferenceSymbol identifierSymbol = null;

        // Properties
        public StringModel Identifier
        {
            get { return identifier; }
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
        internal VariableReferenceModel(VariableReferenceExpressionSyntax variableReferenceSyntax)
            : base(variableReferenceSyntax != null ? variableReferenceSyntax.GetSpan() : null)
        {
            // Check for null
            if(variableReferenceSyntax == null)
                throw new ArgumentNullException(nameof(variableReferenceSyntax));

            this.identifier = new StringModel(variableReferenceSyntax.Identifier);
        }

        internal VariableReferenceModel(string identifier, SyntaxSpan? span)
            : base(span)
        {
            // Check for null or empty
            if (string.IsNullOrEmpty(identifier) == true)
                throw new ArgumentException(nameof(identifier) + " cannot be null or empty");

            this.identifier = new StringModel(identifier, span);
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitVariableReference(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Try to resolve symbol
            this.identifierSymbol = provider.ResolveIdentifierSymbol(ParentSymbol, identifier.Text, Span);

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
                    report.ReportDiagnostic(Code.IdentifierUsedBeforeDeclared, MessageSeverity.Error, Span, identifier);
                }
            }
        }
    }
}
