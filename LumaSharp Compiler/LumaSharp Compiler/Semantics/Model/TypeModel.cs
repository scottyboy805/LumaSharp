using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;

namespace LumaSharp_Compiler.Semantics.Model
{
    public sealed class TypeModel : MemberModel, ITypeReferenceSymbol
    {
        // Private
        private MemberSyntax syntax = null;
        private NamespaceName namespaceName = null;

        private TypeModel parentType = null;
        private INamespaceReferenceSymbol namespaceSymbol = null;
        private GenericParameterModel[] genericParameterIdentifierSymbols = null;
        private ITypeReferenceSymbol[] baseTypesSymbols = null;

        private TypeModel[] memberTypes = null;
        private FieldModel[] memberFields = null;
        private MethodModel[] memberMethods = null;

        // Properties
        internal MemberSyntax Syntax
        {
            get { return syntax; }
        }

        public INamespaceReferenceSymbol NamespaceSymbol
        {
            get { return namespaceSymbol; }
        }

        public ITypeReferenceSymbol DeclaringTypeSymbol
        {
            get { return parentType; }
        }

        public IGenericParameterIdentifierReferenceSymbol[] GenericParameterSymbols
        {
            get { return genericParameterIdentifierSymbols; }
        }

        public ITypeReferenceSymbol[] BaseTypeSymbols
        {
            get { return baseTypesSymbols; }
        }

        public ITypeReferenceSymbol[] TypeMemberSymbols
        {
            get { return memberTypes; }
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
            get { return memberMethods; }
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

        public string[] NamespaceName
        {
            get 
            { 
                if(namespaceName != null)
                    return namespaceName.Identifiers.Select(i => i.Text).ToArray();

                return null;
            }
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

        // Constructor
        internal TypeModel(SemanticModel model, SymbolModel parent, TypeSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.namespaceName = syntax.Namespace;

            // Update parent type
            if(parent is TypeModel)
                this.parentType = (TypeModel)parent;

            // Create generics
            if(syntax.HasGenericParameters == true)
            {
                // Create symbol array
                genericParameterIdentifierSymbols = new GenericParameterModel[syntax.GenericParameters.GenericParameterCount];

                // Build all
                for(int i = 0; i < genericParameterIdentifierSymbols.Length; i++)
                {
                    // Add to type model
                    genericParameterIdentifierSymbols[i] = new GenericParameterModel(syntax.GenericParameters.GenericParameters[i], this);
                }
            }

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

        internal TypeModel(SemanticModel model, SymbolModel parent, ContractSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.namespaceName = syntax.Namespace;

            // Update parent
            if (parent is TypeModel)
                this.parentType = (TypeModel)parent;

            // Build model
            BuildMembersModel(model, syntax.Members);
        }

        internal TypeModel(SemanticModel model, SymbolModel parent, EnumSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.namespaceName = syntax.Namespace;

            // Update parent
            if (parent is TypeModel)
                this.parentType = (TypeModel)parent;

            // Build model
            BuildMembersModel(model, syntax.Fields);
        }

        // Methods
        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve namespace
            if(namespaceName != null)
            {
                namespaceSymbol = provider.ResolveNamespaceSymbol(namespaceName);
            }

            // Resolve generics
            if(HasGenericParameters == true)
            {
                for(int i = 0; i < genericParameterIdentifierSymbols.Length; i++)
                {
                    genericParameterIdentifierSymbols[i].ResolveSymbols(provider, report);
                }
            }

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
