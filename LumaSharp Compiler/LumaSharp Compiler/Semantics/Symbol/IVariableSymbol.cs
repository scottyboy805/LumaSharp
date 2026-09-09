namespace LumaSharp.Compiler.Semantics
{
    public interface IVariableSymbol : ISymbol
    {
        // Properties
        bool IsConst { get; }
        bool IsAssigned { get; }

        // Methods
        IMethodSymbol GetDeclaringMethod();
        ITypeSymbol GetVariableType();
    }
}
