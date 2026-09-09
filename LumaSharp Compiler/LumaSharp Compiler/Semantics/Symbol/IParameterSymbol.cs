
namespace LumaSharp.Compiler.Semantics
{
    public interface IParameterSymbol : ISymbol
    {
        // Properties
        bool IsOptional { get; }
        bool IsEnumerable { get; }
        bool IsIn { get; }
        bool IsOut { get; }
        bool IsRef { get; }

        // Methods
        IMethodSymbol GetDeclaringMethod();
        ITypeSymbol GetParameterType();
        IConstantSymbol GetDefaultValue();
    }
}
