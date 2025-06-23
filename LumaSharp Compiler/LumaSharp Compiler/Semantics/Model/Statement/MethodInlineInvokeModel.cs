using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class MethodInlineInvokeModel : StatementModel
    {
        // Private
        private readonly MethodInvokeModel invokeModel;

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
        public MethodInlineInvokeModel(MethodInvokeStatementSyntax invokeSyntax)
            : base(invokeSyntax != null ? invokeSyntax.GetSpan() : null)
        {
            // Check for null
            if(invokeSyntax == null)
                throw new ArgumentNullException(nameof(invokeSyntax));

            this.invokeModel = new MethodInvokeModel(invokeSyntax.InvokeExpression);

            // Set parent
            invokeModel.parent = this;
        }

        public MethodInlineInvokeModel(MethodInvokeModel invokeModel, SyntaxSpan? span)
            : base(span)
        {
            // Check for null
            if(invokeModel == null)
                throw new ArgumentNullException(nameof(invokeModel));

            this.invokeModel = invokeModel;

            // Set parent
            invokeModel.parent = this;
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
