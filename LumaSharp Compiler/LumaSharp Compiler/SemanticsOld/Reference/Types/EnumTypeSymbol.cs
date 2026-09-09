using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.SemanticsOld.Reference
{
    internal sealed class EnumTypeSymbol : ITypeReferenceSymbol
    {
        // Private
        private ReferenceLibrary library = null;
        private ITypeReferenceSymbol[] baseTypes = null;

        // Properties
        public string TypeName => "enum";

        public string[] NamespaceName => null;

        public ReservedType PrimitiveType => ReservedType.Object;

        public bool IsPrimitive => false;

        public bool IsClass => false;
        public bool IsStruct => false;

        public bool IsInterface => false;

        public bool IsEnum => true;

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

        // Constructor
        internal EnumTypeSymbol(ReferenceLibrary runtimeLibrary, ITypeReferenceSymbol baseType)
        {
            this.library = runtimeLibrary;
            this.baseTypes = new ITypeReferenceSymbol[] { baseType };
        }
    }
}
