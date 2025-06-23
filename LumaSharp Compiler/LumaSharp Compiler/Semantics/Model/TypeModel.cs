using LumaSharp.Runtime;
using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Compiler.Semantics.Reference;
using LumaSharp.Runtime.Reflection;
using PrimitiveType = LumaSharp.Compiler.AST.PrimitiveType;
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Semantics.Model
{
    public enum TypeKind
    {
        Type,
        Contract,
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
        private readonly AccessorModel[] memberAccessors;
        private readonly MethodModel[] memberMethods;
        private readonly MethodModel[] memberOperators;

        private INamespaceReferenceSymbol namespaceSymbol = null;
        private MetaTypeFlags typeFlags = default;
        private _TokenHandle typeToken = default;
        private _TypeHandle typeHandle = default;

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
            get { return kind == TypeKind.Type; }
        }

        public bool IsContract
        {
            get { return kind == TypeKind.Contract; }
        }

        public bool IsEnum
        {
            get { return kind == TypeKind.Enum; }
        }

        public bool IsCopy
        {
            get { return typeSyntax.Attributes != null ? typeSyntax.Attributes.Any(a => a.AttributeType.Identifier.Text == "Copy") : false; }
        }

        public MetaTypeFlags TypeFlags
        {
            get { return typeFlags; }
        }

        public override _TokenHandle Token
        {
            get { return typeToken; }
        }

        public _TypeHandle TypeHandle
        {
            get { return typeHandle; }
        }

        public IReadOnlyList<TypeModel> MemberTypes
        {
            get { return memberTypes; }
        }

        public IReadOnlyList<FieldModel> MemberFields
        {
            get { return memberFields; }
        }

        public IReadOnlyList<AccessorModel> MemberAccessors
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
        internal TypeModel(TypeSyntax typeSyntax, TypeModel declaringType)
            : base(typeSyntax != null ? typeSyntax.Identifier : default,
                  typeSyntax != null ? typeSyntax.AccessModifiers : null,
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
            this.kind = TypeKind.Type;


            // Build model
            BuildMembersModel(typeSyntax.Members, out memberTypes, out memberFields, out memberAccessors, out memberMethods, out memberOperators);

            // Create flags
            this.typeFlags = BuildTypeFlags(); 
        }

        internal TypeModel(ContractSyntax contractSyntax, TypeModel declaringType)
            : base(contractSyntax != null ? contractSyntax.Identifier : default,
                  contractSyntax != null ? contractSyntax.AccessModifiers : null,
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
            this.kind = TypeKind.Contract;


            // Build model
            BuildMembersModel(contractSyntax.Members, out memberTypes, out memberFields, out memberAccessors, out memberMethods, out memberOperators);

            // Create flags
            this.typeFlags = BuildTypeFlags();
        }

        internal TypeModel(EnumSyntax enumSyntax, TypeModel declaringType)
            : base(enumSyntax != null ? enumSyntax.Identifier : default,
                  enumSyntax != null ? enumSyntax.AccessModifiers : null,
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

            // Create flags
            this.typeFlags = BuildTypeFlags();
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


            // Get the type token
            typeToken = provider.GetSymbolToken(this);
        }

        //public override void StaticallyEvaluateMember(ISymbolProvider provider)
        //{
        //    // Evaluate all child members
        //    foreach(MemberModel member in Descendants)
        //    {
        //        member.StaticallyEvaluateMember(provider);
        //    }
        //}

        private void BuildMembersModel(IEnumerable<MemberSyntax> members, out TypeModel[] memberTypes, out FieldModel[] memberFields, out AccessorModel[] memberAccessors, out MethodModel[] memberMethods, out MethodModel[] memberOperators)
        {
            IEnumerable<TypeModel> nestedTypes = members.Where(t => t is TypeSyntax).Select(t => new TypeModel(t as TypeSyntax, this));
            IEnumerable<TypeModel> nestedContracts = members.Where(c => c is ContractSyntax).Select(c => new TypeModel(c as ContractSyntax, this));
            IEnumerable<TypeModel> nestedEnums = members.Where(e => e is EnumSyntax).Select(e => new TypeModel(e as EnumSyntax, this));

            // Create member types
            memberTypes = Enumerable.Concat(nestedTypes, Enumerable.Concat(nestedContracts, nestedEnums)).ToArray();

            // Create member fields
            memberFields = members.Where(f => f is FieldSyntax).Select(f => new FieldModel(f as FieldSyntax, this)).ToArray();

            // Create member accessors
            memberAccessors = members.Where(a => a is AccessorSyntax).Select(a => new AccessorModel(a as AccessorSyntax, this)).ToArray();

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
            if (baseTypesModels == null || (allResolved == true && baseTypesModels.Any(t => t.EvaluatedTypeSymbol.IsType == true) == false))
            {
                // Select the new implicit base type
                string newBaseName = (IsType == true) ? "any" : (IsEnum == true)
                    ? "enum" : null;

                // Apply the base type
                if (newBaseName != null)
                {
                    if (baseTypesModels == null)
                    {
                        baseTypesSymbols = new TypeReferenceModel[] { new TypeReferenceModel(Model, this, Syntax.TypeReference(Syntax.Identifier(newBaseName))) };
                    }
                    else
                    {
                        List<TypeReferenceModel> baseModels = new List<TypeReferenceModel>(baseTypesModels);
                        baseModels.Insert(0, new TypeReferenceModel(Model, this, Syntax.TypeReference(Syntax.Identifier(newBaseName))));

                        baseTypesSymbols = baseModels.ToArray();
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
                    if (baseModels[i].EvaluatedTypeSymbol.IsType == true)
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
            if(IsContract == true && typeSymbols.Count > 0)
            {
                for (int i = 0; i < typeSymbols.Count; i++)
                {
                    report.ReportDiagnostic(Code.InvalidTypeBaseContract, MessageSeverity.Error, typeSymbols[i].Span, typeSymbols[i].EvaluatedTypeSymbol);
                }
            }

            // Check for contract before type symbols
            if(typeSymbols.Count > 0 && baseModels[0].EvaluatedTypeSymbol != null && baseModels[0].EvaluatedTypeSymbol.IsContract == true)
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
                Code reportCode = (IsType == true) ? Code.InvalidSelfBaseType : Code.InvalidSelfBaseContract;
                report.ReportDiagnostic(reportCode, MessageSeverity.Error, baseModel.Span, baseType);
            }

            // Check for enum
            if(baseType.IsEnum == true)
            {
                Code reportCode = (IsType == true) ? Code.InvalidEnumBaseType : Code.InvalidEnumBaseContract;
                report.ReportDiagnostic(reportCode, MessageSeverity.Error, baseModel.Span, baseType);
            }
        }

        private MetaTypeFlags BuildTypeFlags()
        {
            MetaTypeFlags flags = 0;

            // Check for export
            if (HasAccessModifier(AccessModifier.Export) == true) flags |= MetaTypeFlags.Export;

            // Check for internal
            if (HasAccessModifier(AccessModifier.Internal) == true) flags |= MetaTypeFlags.Internal;

            // Check for hidden
            if (HasAccessModifier(AccessModifier.Hidden) == true) flags |= MetaTypeFlags.Hidden;

            // Check for global
            if (HasAccessModifier(AccessModifier.Global) == true) flags |= MetaTypeFlags.Global;

            // Check for type
            if (IsType == true) flags |= MetaTypeFlags.Type;

            // Check for contract
            if (IsContract == true) flags |= MetaTypeFlags.Contract;

            // Check for enum
            if (IsEnum == true) flags |= MetaTypeFlags.Enum;

            // Check for array generic
            if (HasGenericParameters == true) flags |= MetaTypeFlags.Generic;

            // Check for copy
            if(IsCopy == true) flags |= MetaTypeFlags.Copy;

            return flags;
        }
    }
}
