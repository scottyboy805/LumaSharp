using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Semantics.Model
{
    internal sealed class ParameterModel : IParameterSymbol
    {
        // Private
        private readonly SemanticModel model;
        private readonly ParameterSyntax parameter;

        // Properties
        public bool IsOptional => parameter.HasAssignment;
        public bool IsEnumerable => parameter.Enumerable != null;
        public bool IsIn => throw new NotImplementedException();
        public bool IsOut => throw new NotImplementedException();
        public bool IsRef => throw new NotImplementedException();
        public string Name => parameter.Identifier.Text;
        public SyntaxSpan? Source => parameter.GetSpan();

        // Constructor
        public ParameterModel(SemanticModel model, ParameterSyntax parameter)
        {
            this.model = model;
            this.parameter = parameter;
        }

        // Methods
        public IConstantSymbol GetDefaultValueSymbol()
        {
            throw new NotImplementedException();
        }

        public ITypeSymbol GetParameterType()
        {
            // Try to get type
            return model.GetTypeSymbol(parameter.ParameterType);
        }

        public IMethodSymbol GetDeclaringMethod()
        {
            // Try to declaring method symbol
            return model.GetSymbol(parameter.ParentOfType<MethodSyntax>()).Symbol as IMethodSymbol;
        }

        public IConstantSymbol GetDefaultValue()
        {
            throw new NotImplementedException();
        }
    }
}
