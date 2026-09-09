
namespace LumaSharp.Compiler.Semantics
{ 
    public interface IMethodSymbol : IMemberSymbol
    {
        // Properties
        bool IsInitializer { get; }
        bool IsAsync { get; }
        bool IsVirtual { get; }
        bool IsOverride { get; }
        bool IsImplemented { get; }
        bool IsGeneric { get; }
        bool IsGenericInstantiated { get; }
        bool HasReturnValue { get; }
        int ParameterCount { get; }

        // Methods
        IMethodSymbol GetOverrideMethod();
        ITypeSymbol GetImplicitInterfaceImplementation();
        ITypeSymbol GetExplicitInterfaceImplementation();
        ITypeSymbol GetReturnType();
        IReadOnlyList<IParameterSymbol> GetParameters();
    }
}
