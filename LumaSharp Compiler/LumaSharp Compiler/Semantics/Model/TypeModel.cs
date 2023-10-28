using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;
using System.Diagnostics.SymbolStore;
using System.Reflection;

namespace LumaSharp_Compiler.Semantics.Model
{
    public sealed class TypeModel : MemberModel, ITypeReferenceSymbol
    {
        // Private
        private MemberSyntax syntax = null;
        //private GenericParameterModel[] genericParameters = null;

        private TypeModel parentType = null;
        private ITypeReferenceSymbol[] baseTypesSymbols = null;

        private TypeModel[] memberTypes = null;
        private FieldModel[] memberFields = null;
        private MethodModel[] memberMethods = null;

        // Properties
        internal MemberSyntax Syntax
        {
            get { return syntax; }
        }

        public string[] Namespace
        {
            get 
            { 
                // Check for type
                if(syntax is TypeSyntax && ((TypeSyntax)syntax).Namespace != null)
                    return ((TypeSyntax)syntax).Namespace.Identifiers.Select(i => i.Text).ToArray();

                return null;
            }
        }

        //public GenericParameterModel[] GenericParameters
        //{
        //    get { return genericParameters; }
        //}

        public ITypeReferenceSymbol DeclaringTypeSymbol
        {
            get { return parentType; }
        }

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

            // Update parent
            if(parent is TypeModel)
                this.parentType = (TypeModel)parent;

            // Create generics
            //genericParameters = syntax.HasGenericParameters
            //    ? syntax.GenericParameters.GenericTypes.Select(t => new GenericParameterModel(t)).ToArray()
            //    : null;

            //// Create base types
            //baseTypes = syntax.HasBaseTypes
            //    ? syntax.BaseTypeReferences.Select(t => new TypeReferenceModel(model, t)).ToArray()
            //    : null;


            // Build model
            BuildMembersModel(model, syntax.Members);
        }

        internal TypeModel(SemanticModel model, MemberModel parent, ContractSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;

            // Update parent
            if (parent is TypeModel)
                this.parentType = (TypeModel)parent;

            // Build model
            BuildMembersModel(model, syntax.Members);
        }

        internal TypeModel(SemanticModel model, MemberModel parent, EnumSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;

            // Update parent
            if (parent is TypeModel)
                this.parentType = (TypeModel)parent;

            // Build model
            BuildMembersModel(model, syntax.Fields);
        }

        // Methods
        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve all sub type symbols
            foreach(TypeModel subType in memberTypes)
            {
                subType.ResolveSymbols(provider, report);
            }

            // Resolve all field symbols
            foreach(FieldModel field in memberFields)
            {
                field.ResolveSymbols(provider, report);
            }

            // Resolve all method symbols
            foreach(MethodModel method in memberMethods)
            {
                method.ResolveSymbols(provider, report);
            }
        }

        private void BuildMembersModel(SemanticModel model, IEnumerable<MemberSyntax> members)
        {
            // Create member types
            memberTypes = members.Where(t => t is TypeSyntax).Select(t => new TypeModel(model, this, t as TypeSyntax)).ToArray();

            // Create member fields
            memberFields = members.Where(f => f is FieldSyntax).Select(f => new FieldModel(model, this, f as FieldSyntax)).ToArray();

            // Create member methods
            memberMethods = members.Where(m => m is MethodSyntax).Select(m => new MethodModel(model, this, m as MethodSyntax)).ToArray();
        }
    }
}
