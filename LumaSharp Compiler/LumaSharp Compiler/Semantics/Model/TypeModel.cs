using LumaSharp_Compiler.Syntax;

namespace LumaSharp_Compiler.Semantics.Model
{
    public sealed class TypeModel : MemberModel, ITypeReferenceSymbol
    {
        // Private
        private MemberSyntax syntax = null;
        //private GenericParameterModel[] genericParameters = null;

        private ITypeReferenceSymbol[] baseTypesSymbols = null;

        private TypeModel[] memberTypes = null;
        private FieldModel[] memberFields = null;

        // Properties
        public string[] Namespace
        {
            get 
            { 
                // Check for type
                if(syntax is TypeSyntax)
                    return ((TypeSyntax)syntax).Namespace.Identifiers.Select(i => i.Text).ToArray();

                return null;
            }
        }

        //public GenericParameterModel[] GenericParameters
        //{
        //    get { return genericParameters; }
        //}

        public ITypeReferenceSymbol[] BaseTypesSymbols
        {
            get { return baseTypesSymbols; }
        }

        public IFieldReferenceSymbol[] FieldMemberSymbols
        {
            get { return memberFields; }
        }

        public IFieldReferenceSymbol[] AccessorMemberSymbols
        {
            get { throw new NotImplementedException(); }
        }

        public IMethodReferenceSymbol[] MethodMemberSymbols
        {
            get { throw new NotImplementedException(); }
        }

        public bool HasGenericParameters
        {
            get 
            { 
                if(syntax is TypeSyntax)
                    return ((TypeSyntax)syntax).HasGenericParameters;

                return false;
            }
        }

        public bool HasBaseTypes
        {
            get 
            { 
                // Check for type
                if(syntax is TypeSyntax)
                    return ((TypeSyntax)syntax).HasBaseTypes;

                return false;
            }
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
            get { return syntax is TypeSyntax; }
        }

        public bool IsContract
        {
            get { return syntax is ContractSyntax; }
        }

        public bool IsEnum
        {
            get { return syntax is EnumSyntax; }
        }

        public ITypeReferenceSymbol[] GenericTypeSymbols => throw new NotImplementedException();

        public ITypeReferenceSymbol[] BaseTypeSymbols => throw new NotImplementedException();

        // Constructor
        internal TypeModel(SemanticModel model, MemberModel parent, TypeSyntax syntax)
            : base(model, parent, syntax)
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
                ? syntax.Members.Where(t => t is TypeSyntax).Select(t => new TypeModel(model, this, t as TypeSyntax)).ToArray()
                : null;

            // 

            // Create member fields
            memberFields = syntax.HasMembers
                ? syntax.Members.Where(f => f is FieldSyntax).Select(f => new FieldModel(model, this, f as FieldSyntax)).ToArray()
                : null;
        }

        internal TypeModel(SemanticModel model, MemberModel parent, ContractSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            
            // Create types
            memberTypes
        }

        internal TypeModel(SemanticModel model, MemberModel parent, EnumSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;

            // Create fields
            memberFields = syntax.HasFields
                ? syntax.Fields.Select(f => new FieldModel(model, this, f)).ToArray()
                : null;
        }

        // Methods
        public override void ResolveSymbols(ISymbolProvider provider)
        {
        }
    }
}
