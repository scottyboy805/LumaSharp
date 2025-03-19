using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class MethodInvokeModel : ExpressionModel
    {
        // Private
        private MethodInvokeExpressionSyntax syntax = null;
        private ExpressionModel accessModel = null;
        private ExpressionModel[] argumentModels = null;
        private IMethodReferenceSymbol methodIdentifierSymbol = null;

        // Properties
        public override bool IsStaticallyEvaluated
        {
            get { return false; }
        }

        public override ITypeReferenceSymbol EvaluatedTypeSymbol
        {
            get
            {
                // Get return type
                if (methodIdentifierSymbol != null)
                    return methodIdentifierSymbol.ReturnTypeSymbols[0];

                return null;
            }
        }

        public IMethodReferenceSymbol MethodIdentifier
        {
            get { return methodIdentifierSymbol; }
        }

        public ExpressionModel AccessModelExpression
        {
            get { return accessModel; }
        }

        public ExpressionModel[] ArgumentModelExpressions
        {
            get { return argumentModels; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield return accessModel; }
        }

        // Constructor
        public MethodInvokeModel(SemanticModel model, SymbolModel parent, MethodInvokeExpressionSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.accessModel = syntax.AccessExpression != null
                ? ExpressionModel.Any(model, this, syntax.AccessExpression)
                : new ThisModel(model, this, Syntax.This());
            this.argumentModels = (syntax.ArgumentCount > 0)
                ? syntax.ArgumentList.Select(a => ExpressionModel.Any(model, this, a)).ToArray()
                : null;
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitMethodInvoke(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve accessor
            if(accessModel != null)
                accessModel.ResolveSymbols(provider, report);

            bool argumentsResolved = true;

            // Resolve arguments
            if (argumentModels != null)
            {
                for(int i = 0; i < argumentModels.Length; i++)
                {
                    // Resolve the symbols
                    argumentModels[i].ResolveSymbols(provider, report);

                    // Check for resolved
                    if (argumentModels[i].EvaluatedTypeSymbol == null)
                        argumentsResolved = false;
                }
            }

            // Resolve method if accessor is valid - require that arguments are resolved because we will use them to resolve method overloading
            if (accessModel.EvaluatedTypeSymbol != null && argumentsResolved == true)
            {
                // Select argument evaluated types used to infer method overloads
                ITypeReferenceSymbol[] argumentTypes = (argumentModels != null && argumentModels.Length > 0)
                    ? argumentModels.Select(a => a.EvaluatedTypeSymbol).ToArray()
                    : null;
                
                // Try to resolve the method with arguments
                methodIdentifierSymbol = provider.ResolveMethodIdentifierSymbol(accessModel.EvaluatedTypeSymbol, syntax, argumentTypes) as IMethodReferenceSymbol;                
            }
        }
    }
}
