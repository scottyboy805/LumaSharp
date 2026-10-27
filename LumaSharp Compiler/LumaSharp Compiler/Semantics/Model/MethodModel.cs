
using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Semantics.Model
{
    internal sealed class MethodModel : IMethodSymbol
    {
        // Private
        private readonly SemanticModel model;
        private readonly MemberSyntax methodOrInitializer;

        private readonly Lazy<IParameterSymbol[]> parameterSymbols;

        // Properties
        public bool IsConstructor => methodOrInitializer is ConstructorSyntax;
        public bool IsAsync => false;
        public bool IsVirtual => methodOrInitializer.HasModifier(SyntaxTokenKind.VirtualKeyword);
        public bool IsOverride => methodOrInitializer.HasModifier(SyntaxTokenKind.OverrideKeyword);
        public bool IsImplemented => throw new NotImplementedException();
        public bool IsGeneric => false;
        public bool IsGenericInstantiated => false;
        public bool HasReturnValue => methodOrInitializer is MethodSyntax method && method.HasReturnType;
        public int ParameterCount => methodOrInitializer is MethodSyntax method ? method.Parameters.Count : 0;
        public bool IsPublic => methodOrInitializer.HasModifier(SyntaxTokenKind.PublicKeyword);
        public bool IsProtected => methodOrInitializer.HasModifier(SyntaxTokenKind.ProtectedKeyword);
        public bool IsPrivate => methodOrInitializer.HasModifier(SyntaxTokenKind.PrivateKeyword);
        public bool IsInternal => methodOrInitializer.HasModifier(SyntaxTokenKind.InternalKeyword);
        public bool IsStatic => methodOrInitializer.HasModifier(SyntaxTokenKind.StaticKeyword);
        public string Name => methodOrInitializer.Identifier.Text;
        public SyntaxSpan? Source => methodOrInitializer.GetSpan();

        // Constructor
        public MethodModel(SemanticModel model, MethodSyntax method)
            : this()
        {
            this.model = model;
            this.methodOrInitializer = method;
        }

        public MethodModel(SemanticModel model, ConstructorSyntax constructor)
            : this()
        {
            this.model = model;
            this.methodOrInitializer = constructor;
        }

        private MethodModel()
        {
            this.parameterSymbols = new(CreateParameters);
        }

        // Methods
        public ITypeSymbol GetDeclaringType()
        {
            throw new NotImplementedException();
        }

        public ITypeSymbol GetExplicitInterfaceImplementation()
        {
            throw new NotImplementedException();
        }

        public ITypeSymbol GetImplicitInterfaceImplementation()
        {
            throw new NotImplementedException();
        }

        public IMethodSymbol GetOverrideMethod()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IParameterSymbol> GetParameters()
        {
            // Get parameters
            return parameterSymbols.Value;
        }

        public ITypeSymbol GetReturnType()
        {
            // Get return type
            if (methodOrInitializer is MethodSyntax method)
                return model.GetTypeSymbol(method.ReturnType);

            // No return type
            return null;
        }

        private IParameterSymbol[] CreateParameters()
        {
            // Get parameter list syntax
            ParameterListSyntax parameters = methodOrInitializer is MethodSyntax method ? method.Parameters
                : methodOrInitializer is ConstructorSyntax initializer ? initializer.Parameters : null;

            // Check for null
            if (parameters == null)
                return Array.Empty<IParameterSymbol>();

            // Create array
            IParameterSymbol[] parameterSymbols = new IParameterSymbol[parameters.Count];

            // Create parameter symbols
            for(int i = 0; i < parameters.Count; i++)
            {
                // Try to get parameter symbol
                parameterSymbols[i] = model.GetSymbol(parameters[i]).Symbol as IParameterSymbol;
            }
            return parameterSymbols;
        }
    }
}
