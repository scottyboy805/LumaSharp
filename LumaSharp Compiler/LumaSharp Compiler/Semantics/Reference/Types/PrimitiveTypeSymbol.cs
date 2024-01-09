using LumaSharp.Runtime;
using LumaSharp_Compiler.AST;

namespace LumaSharp_Compiler.Semantics.Reference
{
    internal sealed class PrimitiveTypeSymbol : ITypeReferenceSymbol
    {
        // Private
        private string typeName = "";
        private AST.PrimitiveType primitiveType = 0;
        private ReferenceLibrary library = null;
        private ITypeReferenceSymbol[] baseTypes = null;

        private _TypeHandle typeHandle = default;

        // Properties
        public string TypeName => typeName;

        public string[] NamespaceName => null;

        public PrimitiveType PrimitiveType => primitiveType;

        public bool IsPrimitive => true;

        public bool IsType => false;

        public bool IsContract => false;

        public bool IsEnum => false;

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

        public int SymbolToken => (int)primitiveType;

        public string LibraryName => "runtime";

        public _TypeHandle TypeHandle => typeHandle;

        // Constructor
        internal PrimitiveTypeSymbol(ReferenceLibrary runtimeLibrary, PrimitiveType primitiveType, ITypeReferenceSymbol baseType = null)
        {
            this.library = runtimeLibrary;
            this.typeName = primitiveType.ToString().ToLower();
            this.primitiveType = primitiveType;

            if (baseType != null)
                this.baseTypes = new ITypeReferenceSymbol[] { baseType };

            // Create handle
            this.typeHandle = new _TypeHandle
            {
                TypeToken = (int)primitiveType,
                TypeSize = __runtime.Size((int)primitiveType),
            };

            // Load members from reference library
        }

        internal PrimitiveTypeSymbol(ReferenceLibrary runtimeLibrary, string typeName, PrimitiveType primitiveType, ITypeReferenceSymbol baseType = null)
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
