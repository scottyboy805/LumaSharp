using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class NewModel : ExpressionModel
    {
        // Private
        private readonly TypeReferenceModel newTypeModel = null;
        private readonly ExpressionModel[] argumentModels = null;

        // Properties
        public override bool IsStaticallyEvaluated
        {
            get { return false; }
        }

        public override ITypeReferenceSymbol EvaluatedTypeSymbol
        {
            get { return newTypeModel.EvaluatedTypeSymbol; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield return newTypeModel; }
        }

        // Constructor
        public NewModel(NewExpressionSyntax newSyntax)
            : base(newSyntax != null ? newSyntax.GetSpan() : null)
        {
            // Check for null
            if (newSyntax == null)
                throw new ArgumentNullException(nameof(newSyntax));

            this.newTypeModel = new TypeReferenceModel(newSyntax.NewType);
            this.argumentModels = newSyntax.HasArguments == true
                ? newSyntax.ArgumentList.Select(a => ExpressionModel.Any(a, this)).ToArray()
                : null;

            // Set parent
            newTypeModel.parent = this;
            if (argumentModels != null)
            {
                foreach (ExpressionModel argumentModel in argumentModels)
                    argumentModel.parent = this;
            }
        }

        public NewModel(TypeReferenceModel newTypeModel, ExpressionModel[] argumentModels, SyntaxSpan? span)
            : base(span)
        {
            // Check for null
            if(newTypeModel == null)
                throw new ArgumentNullException(nameof(newTypeModel));

            this.newTypeModel = newTypeModel;
            this.argumentModels = argumentModels;

            // Set parent
            newTypeModel.parent = this;
            if(argumentModels != null)
            {
                foreach (ExpressionModel argumentModel in argumentModels)
                    argumentModel.parent = this;
            }
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitNew(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve type
            newTypeModel.ResolveSymbols(provider, report);

            // Check for resolved
            if (newTypeModel.IsResolved == true)
            {
                // TODO - resolve constructor from arguments

                //newTypeModel.EvaluatedTypeSymbol.me
            }
        }
    }
}
