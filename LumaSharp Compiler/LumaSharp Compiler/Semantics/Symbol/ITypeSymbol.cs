
using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Semantics
{
    public interface ITypeSymbol : IMemberSymbol
    {
        // Properties
        bool IsReserved { get; }
        ReservedType ReservedType { get; }
        bool IsClass { get; }
        bool IsStruct { get; }
        bool IsInterfalce { get; }
        bool IsEnum { get; }
        bool IsVirtual { get; }
        bool IsNested { get; }
        bool IsGeneric { get; }
        bool IsGenericInstantiated { get; }

        // Methods
        ITypeSymbol GetBaseType();
        IReadOnlyList<ITypeSymbol> GetInterfaceImplementations();
        IReadOnlyList<IMethodSymbol> GetConstructors();
        IMethodSymbol GetStaticConstructor();
        IReadOnlyList<IMemberSymbol> GetMembers();
        IReadOnlyList<IMemberSymbol> GetMembers(string name);
    }
}
