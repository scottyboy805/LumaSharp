using LumaSharp_Compiler.AST.Statement;
using LumaSharp_Compiler.Reporting;
using LumaSharp_Compiler.Semantics.Model.Expression;

namespace LumaSharp_Compiler.Semantics.Model.Statement
{
    public sealed class AssignModel : StatementModel
    {
        // Private
        private AssignStatementSyntax syntax = null;
        private ExpressionModel left = null;
        private ExpressionModel right = null;

        // Properties
        public ExpressionModel Left
        {
            get { return left; }
        }

        public ExpressionModel Right
        {
            get { return right; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get
            {
                yield return left;
                yield return right;
            }
        }

        // Constructor
        public AssignModel(SemanticModel model, SymbolModel parent, AssignStatementSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.left = ExpressionModel.Any(model, this, syntax.Left);
            this.right = ExpressionModel.Any(model, this, syntax.Right);
        }

        // Methods
        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve left
            left.ResolveSymbols(provider, report);

            // Resolve right
            right.ResolveSymbols(provider, report);

            // Check for assignment
        }
    }
}
