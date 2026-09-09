using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Semantics.Model
{
    internal sealed class TypeModel : ITypeSymbol
    {
        // Private
        private readonly SemanticModel model;
        private readonly MemberSyntax type;     // Must be class, struct, interface or enum
        private readonly ReservedType? reservedType = null;

        // Properties
        public bool IsReserved => reservedType != null;
        public ReservedType ReservedType => reservedType != null ? reservedType.Value : ReservedType.Object;
        public bool IsClass => type is ClassSyntax;
        public bool IsStruct => type is StructSyntax;
        public bool IsInterfalce => type is InterfaceSyntax;
        public bool IsEnum => type is EnumSyntax;
        public bool IsVirtual => type.Modifiers.Any(m => m.Kind == SyntaxTokenKind.VirtualKeyword);
        public bool IsNested => type?.Parent is MemberSyntax;
        public bool IsGeneric => (type is ClassSyntax _class && _class.HasGenericParameters == true)
            || (type is StructSyntax _struct && _struct.HasGenericParameters == true)
            || (type is InterfaceSyntax _interface &&  _interface.HasGenericParameters == true);
        public bool IsGenericInstantiated => false;
        public bool IsPublic => type.Modifiers.Any(m => m.Kind == SyntaxTokenKind.PublicKeyword);
        public bool IsProtected => type.Modifiers.Any(m => m.Kind == SyntaxTokenKind.ProtectedKeyword);
        public bool IsPrivate => type.Modifiers.Any(m => m.Kind == SyntaxTokenKind.PrivateKeyword);
        public bool IsInternal => type.Modifiers.Any(m => m.Kind == SyntaxTokenKind.InternalKeyword);
        public bool IsStatic => type.Modifiers.Any(m => m.Kind == SyntaxTokenKind.StaticKeyword);
        public string Name => type.Identifier.Text;
        public SyntaxSpan? Source => type.GetSpan();

        // Constructor
        public TypeModel(SemanticModel model, ClassSyntax _class)
        {
            this.model = model;
            this.type = _class;
        }

        public TypeModel(SemanticModel model, StructSyntax _struct)
        {
            this.model = model;
            this.type = _struct;
        }

        public TypeModel(SemanticModel model, InterfaceSyntax _interface)
        {
            this.model = model;
            this.type = _interface;
        }

        public TypeModel(SemanticModel model, EnumSyntax _enum)
        {
            this.model = model;
            this.type = _enum;
        }

        public TypeModel(SemanticModel model, MemberSyntax definition, ReservedType reservedType)
        {
            this.model = model;
            this.type = definition;
            this.reservedType = reservedType;
        }

        // Methods
        public ITypeSymbol GetBaseType()
        {
            // Only applicable to class
            if(type is ClassSyntax _class && _class.HasBaseTypes == true)
            {
                // Get the base type reference
                return model.GetTypeSymbol(_class.BaseTypes.First());
            }
            return null;
        }

        public ITypeSymbol GetDeclaringType()
        {
            // Try to declaring type symbol
            return model.GetSymbol(type.ParentOfType<MemberSyntax>()).Symbol as ITypeSymbol;
        }

        public IReadOnlyList<IMethodSymbol> GetConstructors()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ITypeSymbol> GetInterfaceImplementations()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IMemberSymbol> GetMembers()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IMemberSymbol> GetMembers(string name)
        {
            throw new NotImplementedException();
        }

        public IMethodSymbol GetStaticConstructor()
        {
            throw new NotImplementedException();
        }
    }
}
