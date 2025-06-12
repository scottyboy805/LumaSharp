using LumaSharp.Runtime;
using LumaSharp.Compiler.AST;
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Semantics.Reference
{
    internal sealed class EnumTypeSymbol : ITypeReferenceSymbol
    {
        // Private
        private ReferenceLibrary library = null;
        private ITypeReferenceSymbol[] baseTypes = null;

        private _TypeHandle typeHandle = default;

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

        public _TypeHandle TypeHandle => typeHandle;

        public _TokenHandle SymbolToken => default;// new _TokenHandle(TokenKind.TypeReference, 0, 1);

        // Constructor
        internal EnumTypeSymbol(ReferenceLibrary runtimeLibrary, ITypeReferenceSymbol baseType)
        {
            this.library = runtimeLibrary;
            this.baseTypes = new ITypeReferenceSymbol[] { baseType };
        }
    }
}
