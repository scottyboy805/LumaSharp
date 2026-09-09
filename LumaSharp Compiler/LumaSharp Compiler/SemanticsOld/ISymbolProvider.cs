using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.SemanticsOld
{
    public sealed class ParentTypeSymbol
    {
        // Private
        private readonly string name;
        private readonly ITypeReferenceSymbol[] genericArguments;

        // Properties
        public string Name
        {
            get { return name; }
        }

        public ITypeReferenceSymbol[] GenericArguments
        {
            get { return genericArguments; }
        }

        public bool IsGenericType
        {
            get { return genericArguments != null; }
        }

        // Constructor
        internal ParentTypeSymbol(string name, ITypeReferenceSymbol[] genericArguments)
        {
            // Check for null or empty
            if (string.IsNullOrEmpty(name) == true)
                throw new ArgumentException(nameof(name) + " cannot be null or empty");

            this.name = name;
            this.genericArguments = genericArguments;
        }
    }

    public interface ISymbolProvider
    {
        // Methods
        INamespaceReferenceSymbol ResolveNamespaceSymbol(string[] separatedName, SyntaxSpan? span);

        ITypeReferenceSymbol ResolveTypeSymbol(ReservedType primitiveType, int? arrayRank, SyntaxSpan? span);

        ITypeReferenceSymbol ResolveTypeSymbol(IReferenceSymbol context, string typeName, string[] separatedNamespace, ParentTypeSymbol[] parentTypes, ITypeReferenceSymbol[] genericArguments, int? arrayRank, SyntaxSpan? span);

        IIdentifierReferenceSymbol ResolveIdentifierSymbol(IReferenceSymbol context, string identifier, SyntaxSpan? span);

        IIdentifierReferenceSymbol ResolveFieldAccessorIdentifierSymbol(IReferenceSymbol context, string fieldOrAccessorName, SyntaxSpan? span);

        IIdentifierReferenceSymbol ResolveMethodIdentifierSymbol(IReferenceSymbol context, string methodName, ITypeReferenceSymbol[] genericArguments, ITypeReferenceSymbol[] arguments, SyntaxSpan? span);
    }
}
