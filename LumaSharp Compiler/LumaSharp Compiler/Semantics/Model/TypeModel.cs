using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Semantics.Model
{
    internal sealed class TypeModel : ITypeSymbol
    {
        // Private
        private readonly SemanticModel model;
        private readonly MemberSyntax type;     // Must be class, struct, interface or enum
        private readonly ReservedType? reservedType = null;

        private readonly Lazy<IMemberSymbol[]> members = null;

        // Properties
        public bool IsReserved => reservedType != null;
        public ReservedType ReservedType => reservedType != null ? reservedType.Value : ReservedType.Object;
        public bool IsClass => type is ClassSyntax;
        public bool IsStruct => type is StructSyntax;
        public bool IsInterface => type is InterfaceSyntax;
        public bool IsEnum => type is EnumSyntax;
        public bool IsVirtual => type.HasModifier(SyntaxTokenKind.VirtualKeyword);
        public bool IsNested => type?.Parent is MemberSyntax;
        public bool IsGeneric => (type is ClassSyntax _class && _class.HasGenericParameters == true)
            || (type is StructSyntax _struct && _struct.HasGenericParameters == true)
            || (type is InterfaceSyntax _interface &&  _interface.HasGenericParameters == true);
        public bool IsGenericInstantiated => false;
        public bool IsPublic => type.HasModifier(SyntaxTokenKind.PublicKeyword);
        public bool IsProtected => type.HasModifier(SyntaxTokenKind.ProtectedKeyword);
        public bool IsPrivate => type.HasModifier(SyntaxTokenKind.PrivateKeyword);
        public bool IsInternal => type.HasModifier(SyntaxTokenKind.InternalKeyword);
        public bool IsStatic => type.HasModifier(SyntaxTokenKind.StaticKeyword);
        public string Name => type.Identifier.Text;
        public SyntaxSpan? Source => type.GetSpan();

        // Constructor
        public TypeModel(SemanticModel model, ClassSyntax _class)
            : this()
        {
            this.model = model;
            this.type = _class;
        }

        public TypeModel(SemanticModel model, StructSyntax _struct)
            : this()
        {
            this.model = model;
            this.type = _struct;
        }

        public TypeModel(SemanticModel model, InterfaceSyntax _interface)
            : this()
        {
            this.model = model;
            this.type = _interface;
        }

        public TypeModel(SemanticModel model, EnumSyntax _enum)
            : this()
        {
            this.model = model;
            this.type = _enum;
        }

        public TypeModel(SemanticModel model, MemberSyntax definition, ReservedType reservedType)
            : this()
        {
            this.model = model;
            this.type = definition;
            this.reservedType = reservedType;
        }

        private TypeModel()
        {
            members = new(InitMembers);
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
            return members.Value;
        }

        public IMethodSymbol GetStaticConstructor()
        {
            throw new NotImplementedException();
        }

        private IMemberSymbol[] InitMembers()
        {
            return type.DescendantsOfType<MemberSyntax>()
                .Select(m => model.GetSymbol(m).Symbol as IMemberSymbol)
                .Where(m => m != null)
                .ToArray();
        }
    }
}
