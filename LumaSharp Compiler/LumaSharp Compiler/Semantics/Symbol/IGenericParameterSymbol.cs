
namespace LumaSharp.Compiler.Semantics
{
    public interface IGenericParameterSymbol : ITypeSymbol
    {
        // Properties
        bool HasTypeConstraint { get; }
        bool HasStructConstraint { get; }
        bool HasClassConstraint { get; }
        bool HasEnumConstraint { get; }
        
        // Methods
        IMethodSymbol GetGenericDeclaringMethod();
        IMethodSymbol GetGenericDeclaringType();
        IReadOnlyList<ITypeSymbol> GetTypeConstraints();
    }
}
