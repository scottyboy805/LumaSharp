
namespace LumaSharp.Compiler.Semantics
{
    public interface IMemberSymbol : ISymbol
    {
        // Properties
        bool IsPublic { get; }
        bool IsProtected { get; }
        bool IsPrivate { get; }
        bool IsInternal { get; }
        bool IsStatic { get; }

        // Methods
        public ITypeSymbol GetDeclaringType();
    }
}
