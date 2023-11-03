using LumaSharp_Compiler.AST;

namespace LumaSharp_Compiler.Semantics.Reference
{
    internal sealed class EnumTypeSymbol : ITypeReferenceSymbol
    {
        // Private
        private ReferenceLibrary library = null;
        private ITypeReferenceSymbol[] baseTypes = null;

        // Properties
        public string TypeName => "enum";

        public string[] NamespaceName => null;

        public PrimitiveType PrimitiveType => PrimitiveType.Any;

        public bool IsPrimitive => false;

        public bool IsType => false;

        public bool IsContract => false;

        public bool IsEnum => true;

        public INamespaceReferenceSymbol NamespaceSymbol => null;

        public ITypeReferenceSymbol DeclaringTypeSymbol => null;

        public IGenericParameterIdentifierReferenceSymbol[] GenericParameterSymbols => null;

        public ITypeReferenceSymbol[] BaseTypeSymbols => baseTypes;

        public ITypeReferenceSymbol[] TypeMemberSymbols => null;

        public IFieldReferenceSymbol[] FieldMemberSymbols => null;

        public IAccessorReferenceSymbol[] AccessorMemberSymbols => null;

        public IMethodReferenceSymbol[] MethodMemberSymbols => null;

        public IMethodReferenceSymbol[] OperatorMemberSymbols => null;

        public ILibraryReferenceSymbol LibrarySymbol => library;

        public int SymbolToken => 15;

        // Constructor
        internal EnumTypeSymbol(ReferenceLibrary runtimeLibrary, ITypeReferenceSymbol baseType)
        {
            this.library = runtimeLibrary;
            this.baseTypes = new ITypeReferenceSymbol[] { baseType };
        }
    }
}
