using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class MethodInvokeModel : ExpressionModel
    {
        // Private
        private readonly ExpressionModel accessModel = null;
        private readonly ExpressionModel[] genericArgumentModels = null;
        private readonly ExpressionModel[] argumentModels = null;
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

        public ExpressionModel AccessModel
        {
            get { return accessModel; }
        }

        public ExpressionModel[] GenericArgumentModels
        {
            get { return argumentModels; }
        }

        public ExpressionModel[] ArgumentModels
        {
            get { return argumentModels; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield return accessModel; }
        }

        // Constructor
        public MethodInvokeModel(MethodInvokeExpressionSyntax invokeSyntax)
            : base(invokeSyntax != null ? invokeSyntax.GetSpan() : null)
        {
            // Check for null
            if(invokeSyntax == null)
                throw new ArgumentNullException(nameof(invokeSyntax));

            this.accessModel = ExpressionModel.Any(invokeSyntax.AccessExpression, this);
            this.genericArgumentModels = invokeSyntax.HasGenericArguments == true
                ? invokeSyntax.GenericArgumentList.Select(a => new TypeReferenceModel(a)).ToArray() 
                : Array.Empty<TypeReferenceModel>();
            this.argumentModels = invokeSyntax.HasArguments == true
                ? invokeSyntax.ArgumentList.Select(a => ExpressionModel.Any(a, this)).ToArray()
                : Array.Empty<ExpressionModel>();
        }

        public MethodInvokeModel(ExpressionModel accessModel, ExpressionModel[] genericArguments, ExpressionModel[] arguments, SyntaxSpan? span)
            : base(span)
        {
            // Check for null
            if(accessModel == null)
                throw new ArgumentNullException(nameof(accessModel));

            this.accessModel = accessModel;
            this.genericArgumentModels = genericArguments != null ? genericArguments : Array.Empty<TypeReferenceModel>();
            this.argumentModels = arguments != null ? arguments : Array.Empty<ExpressionModel>();

            // Set parent
            this.accessModel.parent = this;

            if(genericArguments != null)
            {
                foreach (ExpressionModel genericArgument in genericArguments)
                    genericArgument.parent = this;
            }

            if(arguments != null)
            {
                foreach(ExpressionModel argument in arguments)
                    argument.parent = this;
            }
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
                // Select generic argument evaluated types used to infer method overloads
                ITypeReferenceSymbol[] genericArgumentTypes = (genericArgumentModels != null && genericArgumentModels.Length > 0)
                    ? genericArgumentModels.Select(g => g.EvaluatedTypeSymbol).ToArray()
                    : null;

                // Select argument evaluated types used to infer method overloads
                ITypeReferenceSymbol[] argumentTypes = (argumentModels != null && argumentModels.Length > 0)
                    ? argumentModels.Select(a => a.EvaluatedTypeSymbol).ToArray()
                    : null;
                
                // Try to resolve the method with arguments
                methodIdentifierSymbol = provider.ResolveMethodIdentifierSymbol(accessModel.EvaluatedTypeSymbol, GetMethodIdentifier(), genericArgumentTypes, argumentTypes, Span) as IMethodReferenceSymbol;                
            }
        }

        private string GetMethodIdentifier()
        {
            // Check for variable
            if(accessModel is VariableReferenceModel variable)
            {
                return variable.Identifier.Text;
            }
            // Check for field
            else if(accessModel is FieldAccessorReferenceModel fieldAccessor)
            {
                return fieldAccessor.Identifier.Text;
            }
            throw new NotSupportedException();
        }
    }
}
