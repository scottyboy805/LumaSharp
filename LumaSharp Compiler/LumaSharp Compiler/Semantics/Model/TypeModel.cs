using LumaSharp.Runtime;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;
using LumaSharp_Compiler.Semantics.Model.Expression;
using LumaSharp_Compiler.Semantics.Reference;

namespace LumaSharp_Compiler.Semantics.Model
{
    public sealed class TypeModel : MemberModel, ITypeReferenceSymbol
    {
        // Private
        private MemberSyntax syntax = null;
        private NamespaceName namespaceName = null;

        private TypeModel parentType = null;
        private IReadOnlyList<INamespaceReferenceSymbol> importSymbols = null;
        private INamespaceReferenceSymbol namespaceSymbol = null;
        private GenericParameterModel[] genericParameterIdentifierSymbols = null;
        private TypeReferenceModel[] baseTypesSymbols = null;

        private TypeModel[] memberTypes = null;
        private FieldModel[] memberFields = null;
        private AccessorModel[] memberAccessors = null;
        private MethodModel[] memberMethods = null;
        private MethodModel[] operatorMethods = null;

        private _TypeHandle typeHandle = default;

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
            get { return baseTypesSymbols.Select(b => b.EvaluatedTypeSymbol).ToArray(); }
        }

        public ITypeReferenceSymbol[] TypeMemberSymbols
        {
            get { return memberTypes; }
        }

        public IFieldReferenceSymbol[] FieldMemberSymbols
        {
            get { return memberFields; }
        }

        public IAccessorReferenceSymbol[] AccessorMemberSymbols
        {
            get { return memberAccessors; }
        }

        public IMethodReferenceSymbol[] MethodMemberSymbols
        {
            get { return memberMethods; }
        }

        public IMethodReferenceSymbol[] OperatorMemberSymbols
        {
            get { return operatorMethods; }
        }

        public bool HasGenericParameters
        {
            get 
            { 
                // Check for type
                if(syntax is TypeSyntax)
                    return ((TypeSyntax)syntax).HasGenericParameters;

                // Check for contract
                if (syntax is ContractSyntax)
                    return ((ContractSyntax)syntax).HasGenericParameters;

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

                // Check for contract
                if(syntax is ContractSyntax)
                    return ((ContractSyntax)syntax).HasBaseTypes;

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

        public _TypeHandle TypeHandle
        {
            get { return typeHandle; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get
            {
                // Types
                foreach (SymbolModel type in memberTypes)
                    yield return type;

                // Fields
                foreach(SymbolModel field in memberFields)
                    yield return field;

                // Accessors
                foreach (SymbolModel accessor in memberAccessors)
                    yield return accessor;

                // Methods
                foreach (SymbolModel method in memberMethods)
                    yield return method;
            }
        }

        // Constructor
        internal TypeModel(SemanticModel model, SymbolModel parent, TypeSyntax syntax, IReadOnlyList<INamespaceReferenceSymbol> importSymbols)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.namespaceName = syntax.Namespace;
            this.importSymbols = importSymbols;

            // Update parent type
            if (parent is TypeModel)
                this.parentType = (TypeModel)parent;

            // Create generics
            if (syntax.HasGenericParameters == true)
            {
                // Create symbol array
                genericParameterIdentifierSymbols = new GenericParameterModel[syntax.GenericParameters.GenericParameterCount];

                // Build all
                for (int i = 0; i < genericParameterIdentifierSymbols.Length; i++)
                {
                    // Add to type model
                    genericParameterIdentifierSymbols[i] = new GenericParameterModel(syntax.GenericParameters.GenericParameters[i], this);
                }
            }

            // Update base types
            this.baseTypesSymbols = syntax.HasBaseTypes == true
                ? syntax.BaseTypeReferences.Select(t => new TypeReferenceModel(model, this, t)).ToArray()
                : null;


            // Build model
            BuildMembersModel(model, syntax.Members);
        }

        internal TypeModel(SemanticModel model, SymbolModel parent, ContractSyntax syntax, IReadOnlyList<INamespaceReferenceSymbol> importSymbols)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.namespaceName = syntax.Namespace;
            this.importSymbols = importSymbols;

            // Update parent
            if (parent is TypeModel)
                this.parentType = (TypeModel)parent;

            // Create generics
            if (syntax.HasGenericParameters == true)
            {
                // Create symbol array
                genericParameterIdentifierSymbols = new GenericParameterModel[syntax.GenericParameters.GenericParameterCount];

                // Build all
                for (int i = 0; i < genericParameterIdentifierSymbols.Length; i++)
                {
                    // Add to type model
                    genericParameterIdentifierSymbols[i] = new GenericParameterModel(syntax.GenericParameters.GenericParameters[i], this);
                }
            }

            // Update base types
            this.baseTypesSymbols = syntax.HasBaseTypes == true
                ? syntax.BaseTypeReferences.Select(t => new TypeReferenceModel(model, this, t)).ToArray()
                : null;

            // Build model
            BuildMembersModel(model, syntax.Members);
        }

        internal TypeModel(SemanticModel model, SymbolModel parent, EnumSyntax syntax, IReadOnlyList<INamespaceReferenceSymbol> importSymbols)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.namespaceName = syntax.Namespace;
            this.importSymbols = importSymbols;

            // Update parent
            if (parent is TypeModel)
                this.parentType = (TypeModel)parent;

            // Build model
            BuildMembersModel(model, syntax.Fields);
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitType(this);
        }

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

            // Resolve base types
            if (HasBaseTypes == true)
            {
                for(int i = 0; i < baseTypesSymbols.Length; i++)
                {
                    // Resolve the base type
                    baseTypesSymbols[i].ResolveSymbols(provider, report);
                }
    
                // Check base type usage
                CheckBaseTypes(baseTypesSymbols, report);
            }

            // Make sure that default bases are implemented (Any type must derive from `any` for example)
            ImplementDefaultBaseTypes(provider, report);

            // Resolve all sub type symbols
            foreach (TypeModel subType in memberTypes)
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

            // Resolve all operator symbols
            foreach(MethodModel operatorMethod in operatorMethods)
            {
                operatorMethod.ResolveSymbols(provider, report);

                // Check usage
                OpTable.CheckSpecialOpUsage(operatorMethod, provider, report);
            }
        }

        public override void StaticallyEvaluateMember(ISymbolProvider provider)
        {
            // Evaluate all child members
            foreach(MemberModel member in Descendants)
            {
                member.StaticallyEvaluateMember(provider);
            }
        }

        private void BuildMembersModel(SemanticModel model, IEnumerable<MemberSyntax> members)
        {
            // Create member types
            memberTypes = members.Where(t => t is TypeSyntax).Select(t => new TypeModel(model, this, t as TypeSyntax, importSymbols)).ToArray();

            // Create member fields
            memberFields = members.Where(f => f is FieldSyntax).Select(f => new FieldModel(model, this, f as FieldSyntax)).ToArray();

            // Create member accessors
            memberAccessors = members.Where(a => a is AccessorSyntax).Select(a => new AccessorModel(model, this, a as AccessorSyntax)).ToArray();

            // Create member methods
            memberMethods = members.Where(m => m is MethodSyntax && OpTable.specialOpMethods.Contains(m.Identifier.Text) == false)
                .Select(m => new MethodModel(model, this, m as MethodSyntax)).ToArray();

            // Create operator methods
            operatorMethods = members.Where(m => m is MethodSyntax && OpTable.specialOpMethods.Contains(m.Identifier.Text) == true)
                .Select(m => new MethodModel(model, this, m as MethodSyntax)).ToArray();
        }

        private void ImplementDefaultBaseTypes(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Check for all resolved
            bool allResolved = baseTypesSymbols == null || baseTypesSymbols.Any(t => t.EvaluatedTypeSymbol == null) == false;

            // Update base type
            if (baseTypesSymbols == null || (allResolved == true && baseTypesSymbols.Any(t => t.EvaluatedTypeSymbol.IsType == true) == false))
            {
                // Select the new implicit base type
                string newBaseName = (IsType == true) ? "any" : (IsEnum == true)
                    ? "enum" : null;

                // Apply the base type
                if (newBaseName != null)
                {
                    if (baseTypesSymbols == null)
                    {
                        baseTypesSymbols = new TypeReferenceModel[] { new TypeReferenceModel(Model, this, new TypeReferenceSyntax(newBaseName)) };
                    }
                    else
                    {
                        List<TypeReferenceModel> baseModels = new List<TypeReferenceModel>(baseTypesSymbols);
                        baseModels.Insert(0, new TypeReferenceModel(Model, this, new TypeReferenceSyntax(newBaseName)));

                        baseTypesSymbols = baseModels.ToArray();
                    }

                    // Resolve the symbols
                    for (int i = 0; i < baseTypesSymbols.Length; i++)
                    {
                        baseTypesSymbols[i].ResolveSymbols(provider, report);
                    }
                }
            }
        }

        private void CheckBaseTypes(TypeReferenceModel[] baseModels, ICompileReportProvider report)
        {
            List<TypeReferenceModel> typeSymbols = new List<TypeReferenceModel>();

            // Check each type
            for(int i = 0; i < baseModels.Length; i++) 
            {
                if (baseModels[i].EvaluatedTypeSymbol != null)
                {
                    CheckBaseType(baseModels[i], report);

                    // Check for type
                    if (baseModels[i].EvaluatedTypeSymbol.IsType == true)
                        typeSymbols.Add(baseModels[i]);
                }
            }

            // Check for multiple types
            if(typeSymbols.Count > 1)
            {
                for(int i = 0; i < typeSymbols.Count - 1; i++)
                {
                    report.ReportMessage(Code.MultipleBaseTypes, MessageSeverity.Error, typeSymbols[i].Syntax.StartToken.Source, typeSymbols[0], typeSymbols[i + 1].EvaluatedTypeSymbol);
                }
            }

            // Check for contract with any types
            if(IsContract == true && typeSymbols.Count > 0)
            {
                for (int i = 0; i < typeSymbols.Count; i++)
                {
                    report.ReportMessage(Code.InvalidTypeBaseContract, MessageSeverity.Error, typeSymbols[i].Syntax.StartToken.Source, typeSymbols[i]);
                }
            }

            // Check for contract before type symbols
            if(typeSymbols.Count > 0 && baseModels[0].EvaluatedTypeSymbol != null && baseModels[0].EvaluatedTypeSymbol.IsContract == true)
            {
                for(int i = 0; i < typeSymbols.Count; i++)
                {
                    report.ReportMessage(Code.FirstBaseType, MessageSeverity.Error, typeSymbols[i].Syntax.StartToken.Source, typeSymbols[i].EvaluatedTypeSymbol);
                }
            }
        }

        private void CheckBaseType(TypeReferenceModel baseModel, ICompileReportProvider report)
        {
            ITypeReferenceSymbol baseType = baseModel.EvaluatedTypeSymbol;

            // Check for inheritance cycle
            if(baseType == this)
            {
                Code reportCode = (IsType == true) ? Code.InvalidSelfBaseType : Code.InvalidSelfBaseContract;
                report.ReportMessage(reportCode, MessageSeverity.Error, baseModel.Syntax.StartToken.Source, baseType);
            }

            // Check for enum
            if(baseType.IsEnum == true)
            {
                Code reportCode = (IsType == true) ? Code.InvalidEnumBaseType : Code.InvalidEnumBaseContract;
                report.ReportMessage(reportCode, MessageSeverity.Error, baseModel.Syntax.StartToken.Source, baseType);
            }
        }
    }
}
