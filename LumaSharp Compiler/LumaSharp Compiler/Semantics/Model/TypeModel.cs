using LumaSharp_Compiler.Syntax;

namespace LumaSharp_Compiler.Semantics.Model
{
    public sealed class TypeModel : MemberModel, ITypeReferenceSymbol
    {
        // Private
        private TypeSyntax syntax = null;
        //private GenericParameterModel[] genericParameters = null;
        
        private ITypeReferenceSymbol[] baseTypesSymbols = null;

        private TypeModel[] memberTypes = null;
        private FieldModel[] memberFields = null;

        // Properties
        //public GenericParameterModel[] GenericParameters
        //{
        //    get { return genericParameters; }
        //}

        public ITypeReferenceSymbol[] BaseTypesSymbols
        {
            get { return baseTypesSymbols; }
        }

        public bool HasGenericParameters
        {
            get { return syntax.HasGenericParameters; }
        }

        public bool HasBaseTypes
        {
            get { return syntax.HasBaseTypes; }
        }

        public string TypeName
        {
            get { return MemberName; }
        }

        public PrimitiveType PrimitiveType
        {
            get { return PrimitiveType.Any; }
        }

        public bool IsPrimitive
        {
            get { return false; }
        }

        public bool IsType
        {
            get { return true; }
        }

        public bool IsContract
        {
            get { return false; }
        }

        public bool IsEnum
        {
            get { return false; }
        }

        public ITypeReferenceSymbol[] GenericTypeSymbols => throw new NotImplementedException();

        public ITypeReferenceSymbol[] BaseTypeSymbols => throw new NotImplementedException();

        // Constructor
        internal TypeModel(TypeSyntax syntax, SemanticModel model, MemberModel parent = null)
            : base(syntax, model, parent)
        {
            this.syntax = syntax;
            // Create generics
            //genericParameters = syntax.HasGenericParameters
            //    ? syntax.GenericParameters.GenericTypes.Select(t => new GenericParameterModel(t)).ToArray()
            //    : null;

            //// Create base types
            //baseTypes = syntax.HasBaseTypes
            //    ? syntax.BaseTypeReferences.Select(t => new TypeReferenceModel(model, t)).ToArray()
            //    : null;


            // Create member types
            memberTypes = syntax.HasMembers
                ? syntax.Members.Where(t => t is TypeSyntax).Select(t => new TypeModel(t as TypeSyntax, model, this)).ToArray()
                : null;

            // Create member fields
            memberFields = syntax.HasMembers
                ? syntax.Members.Where(f => f is FieldSyntax).Select(f => new FieldModel(f as FieldSyntax, model, this)).ToArray()
                : null;
        }

        // Methods
        public override bool ResolveSymbols(ISymbolProvider provider)
        {
            return false;
        }
    }
}
