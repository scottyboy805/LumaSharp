using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.SemanticsOld.Reference
{
    internal sealed class PrimitiveTypeSymbol : ITypeReferenceSymbol
    {
        // Private
        private string typeName = "";
        private ReservedType primitiveType = 0;
        private ReferenceLibrary library = null;
        private ITypeReferenceSymbol[] baseTypes = null;

        // Properties
        public string TypeName => typeName;

        public string[] NamespaceName => null;

        public ReservedType PrimitiveType => primitiveType;

        public bool IsPrimitive => true;

        public bool IsClass => false;
        public bool IsStruct => primitiveType != ReservedType.String && primitiveType != ReservedType.Object;

        public bool IsInterface => false;

        public bool IsEnum => false;

        public INamespaceReferenceSymbol NamespaceSymbol => null;

        public ITypeReferenceSymbol DeclaringTypeSymbol => null;

        public IGenericParameterIdentifierReferenceSymbol[] GenericParameterSymbols => null;

        public ITypeReferenceSymbol[] BaseTypeSymbols => baseTypes;

        public ITypeReferenceSymbol[] TypeMemberSymbols => null;

        public IFieldReferenceSymbol[] FieldMemberSymbols => null;

        public IPropertyReferenceSymbol[] AccessorMemberSymbols => null;

        public IMethodReferenceSymbol[] MethodMemberSymbols => null;

        public IMethodReferenceSymbol[] OperatorMemberSymbols => null;

        public ILibraryReferenceSymbol LibrarySymbol => library;

        public string LibraryName => "runtime";

        // Constructor
        internal PrimitiveTypeSymbol(ReferenceLibrary runtimeLibrary, ReservedType primitiveType, ITypeReferenceSymbol baseType = null)
        {
            this.library = runtimeLibrary;
            this.typeName = primitiveType.ToString().ToLower();
            this.primitiveType = primitiveType;

            if (baseType != null)
                this.baseTypes = new ITypeReferenceSymbol[] { baseType };

            // Load members from reference library
        }

        internal PrimitiveTypeSymbol(ReferenceLibrary runtimeLibrary, string typeName, ReservedType primitiveType, ITypeReferenceSymbol baseType = null)
        {
            this.library = runtimeLibrary;
            this.typeName = typeName;
            this.primitiveType = primitiveType;

            if (baseType != null)
                this.baseTypes = new ITypeReferenceSymbol[] { baseType };

            // Load members from reference library
        }

        // Methods
        public override string ToString()
        {
            return TypeName;
        }
    }
}
