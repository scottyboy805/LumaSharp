using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Compiler.SemanticsOld.Reference;

namespace LumaSharp.Compiler.SemanticsOld.Model
{
    public enum TypeKind
    {
        Class,
        Struct,
        Interface,
        Enum,
    }

    public sealed class TypeModel : MemberModel, ITypeReferenceSymbol
    {
        // Private
        private readonly TypeModel declaringType;
        private readonly StringModel namespaceName;
        private readonly GenericParameterModel[] genericParameterModels;
        private readonly TypeReferenceModel[] baseTypesModels;
        private readonly TypeKind kind;

        private readonly TypeModel[] memberTypes;
        private readonly FieldModel[] memberFields;
        private readonly PropertyModel[] memberAccessors;
        private readonly MethodModel[] memberMethods;
        private readonly MethodModel[] memberOperators;

        private INamespaceReferenceSymbol namespaceSymbol = null;

        // Properties
        public INamespaceReferenceSymbol NamespaceSymbol
        {
            get { return namespaceSymbol; }
        }

        public ITypeReferenceSymbol DeclaringTypeSymbol
        {
            get { return declaringType; }
        }

        public IGenericParameterIdentifierReferenceSymbol[] GenericParameterSymbols
        {
            get { return genericParameterModels; }
        }

        public ITypeReferenceSymbol[] BaseTypeSymbols
        {
            get { return baseTypesModels.Select(b => b.EvaluatedTypeSymbol).ToArray(); }
        }

        public ITypeReferenceSymbol[] TypeMemberSymbols
        {
            get { return memberTypes; }
        }

        public IFieldReferenceSymbol[] FieldMemberSymbols
        {
            get { return memberFields; }
        }

        public IPropertyReferenceSymbol[] AccessorMemberSymbols
        {
            get { return memberAccessors; }
        }

        public IMethodReferenceSymbol[] MethodMemberSymbols
        {
            get { return memberMethods; }
        }

        public IMethodReferenceSymbol[] OperatorMemberSymbols
        {
            get { return memberOperators; }
        }

        public bool HasGenericParameters
        {
            get { return genericParameterModels != null;  }
        }

        public bool HasBaseTypes
        {
            get { return baseTypesModels != null; }
        }

        public string TypeName
        {
            get { return MemberName; }
        }

        public string[] NamespaceName
        {
            get 
            {
                return namespaceName != null
                    ? namespaceName.Text.Split(SyntaxToken.GetText(SyntaxTokenKind.ColonSymbol))
                    : null;
            }
        }

        public ReservedType PrimitiveType
        {
            get { return ReservedType.Object; }
        }

        public bool IsPrimitive
        {
            get { return false; }
        }

        public bool IsClass
        {
            get { return kind == TypeKind.Class; }
        }

        public bool IsStruct
        {
            get { return kind == TypeKind.Struct; }
        }

        public bool IsInterface
        {
            get { return kind == TypeKind.Interface; }
        }

        public bool IsEnum
        {
            get { return kind == TypeKind.Enum; }
        }

        public IReadOnlyList<TypeModel> MemberTypes
        {
            get { return memberTypes; }
        }

        public IReadOnlyList<FieldModel> MemberFields
        {
            get { return memberFields; }
        }

        public IReadOnlyList<PropertyModel> MemberAccessors
        {
            get { return memberAccessors; }
        }

        public IReadOnlyList<MethodModel> MemberMethods
        {
            get { return memberMethods; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get
            {
                // Types
                foreach (SymbolModel type in memberTypes)
                    yield return type;

                // Fields
                foreach(SymbolModel memberField in memberFields)
                    yield return memberField;

                // Accessors
                foreach (SymbolModel accessor in memberAccessors)
                    yield return accessor;

                // Methods
                foreach (SymbolModel method in memberMethods)
                    yield return method;
            }
        }

        // Constructor
        internal TypeModel(ClassSyntax typeSyntax, TypeModel declaringType)
            : base(typeSyntax != null ? typeSyntax.Identifier : default,
                  typeSyntax != null ? typeSyntax.Modifiers : null,
                  typeSyntax != null ? typeSyntax.GetSpan() : null)
        {
            // Check for null
            if(typeSyntax == null)
                throw new ArgumentNullException(nameof(typeSyntax));

            this.declaringType = declaringType;
            this.namespaceName = typeSyntax.Namespace != null
                ? new StringModel(typeSyntax.Namespace.NormalizeWhitespace().GetSourceText())
                : null;
            this.genericParameterModels = typeSyntax.HasGenericParameters == true
                ? typeSyntax.GenericParameters.Select(g => new GenericParameterModel(g)).ToArray() 
                : null;
            this.baseTypesModels = typeSyntax.HasBaseTypes == true
                ? typeSyntax.BaseTypes.Select(b => new TypeReferenceModel(b)).ToArray()
                : null;
            this.kind = TypeKind.Class;


            // Build model
            BuildMembersModel(typeSyntax.Members, out memberTypes, out memberFields, out memberAccessors, out memberMethods, out memberOperators);
        }

        internal TypeModel(InterfaceSyntax contractSyntax, TypeModel declaringType)
            : base(contractSyntax != null ? contractSyntax.Identifier : default,
                  contractSyntax != null ? contractSyntax.Modifiers : null,
                  contractSyntax != null ? contractSyntax.GetSpan() : null)
        {
            // Check for null
            if(contractSyntax == null)
                throw new ArgumentNullException(nameof(contractSyntax));

            this.declaringType = declaringType;
            this.namespaceName = contractSyntax.Namespace != null
                ? new StringModel(contractSyntax.Namespace.NormalizeWhitespace().GetSourceText())
                : null;
            this.genericParameterModels = contractSyntax.HasGenericParameters == true
                ? contractSyntax.GenericParameters.Select(g => new GenericParameterModel(g)).ToArray()
                : null;
            this.baseTypesModels = contractSyntax.HasBaseTypes == true
                ? contractSyntax.BaseTypes.Select(b => new TypeReferenceModel(b)).ToArray()
                : null;
            this.kind = TypeKind.Interface;


            // Build model
            BuildMembersModel(contractSyntax.Members, out memberTypes, out memberFields, out memberAccessors, out memberMethods, out memberOperators);
        }

        internal TypeModel(EnumSyntax enumSyntax, TypeModel declaringType)
            : base(enumSyntax != null ? enumSyntax.Identifier : default,
                  enumSyntax != null ? enumSyntax.Modifiers : null,
                  enumSyntax != null ? enumSyntax.GetSpan() : null)
        {
            // Check for null
            if(enumSyntax == null)
                throw new ArgumentNullException(nameof(enumSyntax));

            this.declaringType = declaringType;
            this.namespaceName = enumSyntax.Namespace != null
                ? new StringModel(enumSyntax.Namespace.NormalizeWhitespace().GetSourceText())
                : null;
            this.baseTypesModels = enumSyntax.UnderlyingType != null
                ? new[] { new TypeReferenceModel(enumSyntax.UnderlyingType.First()) }
                : null;
            this.kind = TypeKind.Enum;


            // Build model
            BuildMembersModel(enumSyntax.Body?.ToArray(), out memberTypes, out memberFields, out memberAccessors, out memberMethods, out memberOperators);
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitType(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve base
            base.ResolveSymbols(provider, report);

            // Resolve namespace
            if(namespaceName != null)
            {
                namespaceSymbol = provider.ResolveNamespaceSymbol(NamespaceName, null);
            }

            // Resolve generics
            if(HasGenericParameters == true)
            {
                for(int i = 0; i < genericParameterModels.Length; i++)
                {
                    genericParameterModels[i].ResolveSymbols(provider, report);
                }
            }

            // Resolve base types
            if (HasBaseTypes == true)
            {
                for(int i = 0; i < baseTypesModels.Length; i++)
                {
                    // Resolve the base type
                    baseTypesModels[i].ResolveSymbols(provider, report);
                }
    
                // Check base type usage
                CheckBaseTypes(baseTypesModels, report);
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
            foreach(MethodModel operatorMethod in memberOperators)
            {
                operatorMethod.ResolveSymbols(provider, report);

                // Check usage
                OpTable.CheckSpecialOpUsage(operatorMethod, provider, report);
            }
        }

        //public override void StaticallyEvaluateMember(ISymbolProvider provider)
        //{
        //    // Evaluate all child members
        //    foreach(MemberModel member in Descendants)
        //    {
        //        member.StaticallyEvaluateMember(provider);
        //    }
        //}

        private void BuildMembersModel(IEnumerable<MemberSyntax> members, out TypeModel[] memberTypes, out FieldModel[] memberFields, out PropertyModel[] memberAccessors, out MethodModel[] memberMethods, out MethodModel[] memberOperators)
        {
            IEnumerable<TypeModel> nestedTypes = members.Where(t => t is ClassSyntax).Select(t => new TypeModel(t as ClassSyntax, this));
            IEnumerable<TypeModel> nestedContracts = members.Where(c => c is InterfaceSyntax).Select(c => new TypeModel(c as InterfaceSyntax, this));
            IEnumerable<TypeModel> nestedEnums = members.Where(e => e is EnumSyntax).Select(e => new TypeModel(e as EnumSyntax, this));

            // Create member types
            memberTypes = Enumerable.Concat(nestedTypes, Enumerable.Concat(nestedContracts, nestedEnums)).ToArray();

            // Create member fields
            memberFields = members.Where(f => f is FieldSyntax).Select(f => new FieldModel(f as FieldSyntax, this)).ToArray();

            // Create member accessors
            //memberAccessors = members.Where(a => a is AccessorSyntax).Select(a => new AccessorModel(a as AccessorSyntax, this)).ToArray();
            memberAccessors = null;

            // Create member methods
            memberMethods = members.Where(m => m is MethodSyntax && OpTable.specialOpMethods.Contains(m.Identifier.Text) == false)
                .Select(m => new MethodModel(m as MethodSyntax, this)).ToArray();

            // Create operator methods
            memberOperators = members.Where(m => m is MethodSyntax && OpTable.specialOpMethods.Contains(m.Identifier.Text) == true)
                .Select(m => new MethodModel(m as MethodSyntax, this)).ToArray();
        }

        private void ImplementDefaultBaseTypes(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Check for all resolved
            bool allResolved = baseTypesModels == null || baseTypesModels.Any(t => t.EvaluatedTypeSymbol == null) == false;

            // Update base type
            if (baseTypesModels == null || (allResolved == true && baseTypesModels.Any(t => t.EvaluatedTypeSymbol.IsClass == true) == false))
            {
                // Select the new implicit base type
                string newBaseName = (IsClass == true) ? "any" : (IsEnum == true)
                    ? "enum" : null;

                // Apply the base type
                if (newBaseName != null)
                {
                    if (baseTypesModels == null)
                    {
                        //baseTypesSymbols = new TypeReferenceModel[] { new TypeReferenceModel(Model, this, Syntax.TypeReference(Syntax.Identifier(newBaseName))) };
                    }
                    else
                    {
                        List<TypeReferenceModel> baseModels = new List<TypeReferenceModel>(baseTypesModels);
                        //baseModels.Insert(0, new TypeReferenceModel(Model, this, Syntax.TypeReference(Syntax.Identifier(newBaseName))));

                        //baseTypesSymbols = baseModels.ToArray();
                    }

                    // Resolve the symbols
                    for (int i = 0; i < baseTypesModels.Length; i++)
                    {
                        baseTypesModels[i].ResolveSymbols(provider, report);
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
                    if (baseModels[i].EvaluatedTypeSymbol.IsClass == true)
                        typeSymbols.Add(baseModels[i]);
                }
            }

            // Check for multiple types
            if(typeSymbols.Count > 1)
            {
                for(int i = 0; i < typeSymbols.Count - 1; i++)
                {
                    report.ReportDiagnostic(Code.MultipleBaseTypes, MessageSeverity.Error, typeSymbols[i].Span, typeSymbols[0].EvaluatedTypeSymbol, typeSymbols[i + 1].EvaluatedTypeSymbol);
                }
            }

            // Check for contract with any types
            if(IsInterface == true && typeSymbols.Count > 0)
            {
                for (int i = 0; i < typeSymbols.Count; i++)
                {
                    report.ReportDiagnostic(Code.InvalidTypeBaseContract, MessageSeverity.Error, typeSymbols[i].Span, typeSymbols[i].EvaluatedTypeSymbol);
                }
            }

            // Check for contract before type symbols
            if(typeSymbols.Count > 0 && baseModels[0].EvaluatedTypeSymbol != null && baseModels[0].EvaluatedTypeSymbol.IsInterface == true)
            {
                for(int i = 0; i < typeSymbols.Count; i++)
                {
                    report.ReportDiagnostic(Code.FirstBaseType, MessageSeverity.Error, typeSymbols[i].Span, typeSymbols[i].EvaluatedTypeSymbol);
                }
            }
        }

        private void CheckBaseType(TypeReferenceModel baseModel, ICompileReportProvider report)
        {
            ITypeReferenceSymbol baseType = baseModel.EvaluatedTypeSymbol;

            // Check for inheritance cycle
            if(baseType == this)
            {
                Code reportCode = (IsClass == true) ? Code.InvalidSelfBaseType : Code.InvalidSelfBaseContract;
                report.ReportDiagnostic(reportCode, MessageSeverity.Error, baseModel.Span, baseType);
            }

            // Check for enum
            if(baseType.IsEnum == true)
            {
                Code reportCode = (IsClass == true) ? Code.InvalidEnumBaseType : Code.InvalidEnumBaseContract;
                report.ReportDiagnostic(reportCode, MessageSeverity.Error, baseModel.Span, baseType);
            }
        }
    }
}
