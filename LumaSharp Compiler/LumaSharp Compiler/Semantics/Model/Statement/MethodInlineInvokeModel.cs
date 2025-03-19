using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class MethodInlineInvokeModel : StatementModel
    {
        // Private
        private MethodInvokeModel invokeModel = null;

        // Properties
        public MethodInvokeModel InvokeModel
        {
            get { return invokeModel; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield return invokeModel; }
        }

        // Constructor
        public MethodInlineInvokeModel(SemanticModel model, SymbolModel parent, MethodInvokeStatementSyntax syntax, int statementIndex)
            : base(model, parent, syntax, statementIndex)
        {
            this.invokeModel = new MethodInvokeModel(model, this, syntax.InvokeExpression);
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitMethodInvoke(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve symbols
            invokeModel.ResolveSymbols(provider, report);
        }
    }
}
