using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public enum PrimitiveType : byte
    {
        Void,
        Any = 1,
        Bool,
        Char,
        String,
        I8,
        U8,
        I16,
        U16,
        I32,
        U32,
        I64,
        U64,
        F32,
        F64,
        Ptr,
        UPtr,
    }

    public sealed class ParentTypeReferenceSyntax : SyntaxNode
    {
        // Private
        private readonly SyntaxToken identifier;
        private readonly GenericArgumentListSyntax genericArguments;
        private readonly SyntaxToken dot;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return identifier; }
        }

        public override SyntaxToken EndToken
        {
            get { return dot; }
        }

        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public GenericArgumentListSyntax GenericArguments
        {
            get { return genericArguments; }
        }

        public SyntaxToken Dot
        {
            get { return dot; }
        }

        public int GenericArgumentCount
        {
            get { return IsGenericType ? genericArguments.Count : 0; }
        }

        public bool IsGenericType
        {
            get { return genericArguments != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                if (IsGenericType == true)
                    return genericArguments.Descendants;

                return Enumerable.Empty<SyntaxNode>();
            }
        }

        // Constructor
        internal ParentTypeReferenceSyntax(SyntaxToken identifier, GenericArgumentListSyntax genericArguments)
            : this(
                  identifier,
                  genericArguments,
                  new SyntaxToken(SyntaxTokenKind.DotSymbol))
        {
        }

        internal ParentTypeReferenceSyntax(SyntaxToken identifier, GenericArgumentListSyntax genericArguments, SyntaxToken dot)
        {
            // Check kind
            if (identifier.Kind != SyntaxTokenKind.Identifier)
                throw new ArgumentException(nameof(identifier) + " must be of kind: " + SyntaxTokenKind.Identifier);

            if (dot.Kind != SyntaxTokenKind.DotSymbol)
                throw new ArgumentException(nameof(dot) + " must be of kind: " + SyntaxTokenKind.DotSymbol);

            this.identifier = identifier;
            this.genericArguments = genericArguments;
            this.dot = dot;

            // Set parent
            if (genericArguments != null) genericArguments.parent = this;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Identifier
            identifier.GetSourceText(writer);

            // Check for generic
            if(IsGenericType == true)
                genericArguments.GetSourceText(writer);

            // Dot
            dot.GetSourceText(writer);
        }
    }

    public sealed class TypeReferenceSyntax : ExpressionSyntax
    {
        // Private
        private readonly SyntaxToken identifier;
        private readonly SeparatedTokenList namespaceName;
        private readonly ParentTypeReferenceSyntax[] parentTypes;
        private readonly GenericArgumentListSyntax genericArguments;
        private readonly ArrayParametersSyntax arrayParameters;
        private readonly SyntaxToken colon;

        // Properties
        public override SyntaxToken StartToken
        {
            get
            {
                if (HasNamespace == true)
                {
                    return namespaceName.StartToken;
                }

                if (HasParentType == true)
                {
                    return parentTypes[0].StartToken;
                }
                return identifier;
            }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                if (IsArrayType == true)
                {
                    return arrayParameters.EndToken;
                }

                if (IsGenericType == true)
                {
                    return genericArguments.EndToken;
                }
                return identifier;
            }
        }

        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public SyntaxToken NamespaceSeparator
        {
            get { return colon; }
        }

        public SeparatedTokenList Namespace
        {
            get { return namespaceName; }
        }

        public ParentTypeReferenceSyntax[] ParentTypes
        {
            get { return parentTypes; }
        }

        public GenericArgumentListSyntax GenericArguments
        {
            get { return genericArguments; }
        }

        public ArrayParametersSyntax ArrayParameters
        {
            get { return arrayParameters; }
        }

        public int NamespaceDepth
        {
            get { return HasNamespace ? namespaceName.Count : 0; }
        }

        public int NestedDepth
        {
            get { return IsNested ? parentTypes.Length : 0; }
        }

        public int GenericArgumentCount
        {
            get { return IsGenericType ? genericArguments.Count : 0; }
        }

        public int ArrayParameterRank
        {
            get { return IsArrayType ? arrayParameters.Rank : 0; }
        }

        public bool HasNamespace
        {
            get { return namespaceName != null; }
        }

        public bool HasParentType
        {
            get { return parentTypes != null; }
        }

        public bool IsNested
        {
            get { return parentTypes != null; }
        }

        public bool IsGenericType
        {
            get { return genericArguments != null; }
        }

        public bool IsArrayType
        {
            get { return arrayParameters != null; }
        }

        public bool IsPrimitiveType
        {
            get { return GetPrimitiveType(out _); }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal TypeReferenceSyntax(MemberSyntax fromMember)
        {
            // Check for type
            if (fromMember is TypeSyntax)
            {
                TypeSyntax fromType = (TypeSyntax)fromMember;
                this.identifier = fromType.Identifier;
                this.namespaceName = fromType.Namespace;
            }
            // Check for contract
            else if (fromMember is ContractSyntax)
            {
                ContractSyntax fromContract = (ContractSyntax)fromMember;
                this.identifier = fromContract.Identifier;
                this.namespaceName = fromContract.Namespace;
            }
            // Check for enum
            else if (fromMember is EnumSyntax)
            {
                EnumSyntax fromEnum = (EnumSyntax)fromMember;
                this.identifier = fromEnum.Identifier;
                this.namespaceName = fromEnum.Namespace;
            }
            else
                throw new NotSupportedException("Cannot create type reference from non-type member: " + fromMember);
        }

        internal TypeReferenceSyntax(PrimitiveType primitive, ArrayParametersSyntax arrayParameters)
        {
            this.identifier = new SyntaxToken(primitive switch
            {
                PrimitiveType.Void => SyntaxTokenKind.VoidKeyword,
                PrimitiveType.Bool => SyntaxTokenKind.BoolKeyword,
                PrimitiveType.Char => SyntaxTokenKind.CharKeyword,
                PrimitiveType.String => SyntaxTokenKind.StringKeyword,
                PrimitiveType.I8 => SyntaxTokenKind.I8Keyword,
                PrimitiveType.U8 => SyntaxTokenKind.U8Keyword,
                PrimitiveType.I16 => SyntaxTokenKind.I16Keyword,
                PrimitiveType.U16 => SyntaxTokenKind.U16Keyword,
                PrimitiveType.I32 => SyntaxTokenKind.I32Keyword,
                PrimitiveType.U32 => SyntaxTokenKind.U32Keyword,
                PrimitiveType.I64 => SyntaxTokenKind.I64Keyword,
                PrimitiveType.U64 => SyntaxTokenKind.U64Keyword,
                PrimitiveType.F32 => SyntaxTokenKind.F32Keyword,
                PrimitiveType.F64 => SyntaxTokenKind.F64Keyword,
                PrimitiveType.Any => SyntaxTokenKind.AnyKeyword,

                _ => throw new NotSupportedException(primitive.ToString()),
            });
            this.arrayParameters = arrayParameters;
        }

        internal TypeReferenceSyntax(SyntaxToken primitiveToken, ArrayParametersSyntax arrayParameters)
        {
            this.identifier = primitiveToken;
            this.arrayParameters = arrayParameters;
        }

        internal TypeReferenceSyntax(SeparatedTokenList namespaceName, ParentTypeReferenceSyntax[] parentTypes, SyntaxToken identifier, GenericArgumentListSyntax genericArguments, ArrayParametersSyntax arrayParameters)
        {
            // Check kind
            if(identifier.Kind != SyntaxTokenKind.Identifier)
                throw new ArgumentException(nameof(identifier) + " must be of kind: " + SyntaxTokenKind.Identifier);

            this.identifier = identifier;
            this.namespaceName = namespaceName;
            this.parentTypes = parentTypes;
            this.genericArguments = genericArguments;
            this.arrayParameters = arrayParameters;

            // Set parent
            if (namespaceName != null) namespaceName.parent = this;
            if(parentTypes != null)
            {
                foreach (ParentTypeReferenceSyntax p in parentTypes)
                    p.parent = this;
            }
            if (genericArguments != null) genericArguments.parent = this;
            if(arrayParameters != null) arrayParameters.parent = this;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write namespace
            if (HasNamespace == true)
            {
                namespaceName.GetSourceText(writer);

                // Write trailing :
                colon.GetSourceText(writer);
            }

            // Write parent types
            if (IsNested == true)
            {
                // Write parent type
                foreach(ParentTypeReferenceSyntax parentType in parentTypes)
                    parentType.GetSourceText(writer);
            }

            // Write identifier
            identifier.GetSourceText(writer);

            // Write generics
            if (IsGenericType == true)
                genericArguments.GetSourceText(writer);

            // Write array
            if (IsArrayType == true)
                arrayParameters.GetSourceText(writer);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitTypeReference(this);
        }

        internal bool GetPrimitiveType(out PrimitiveType primitiveType)
        {
            // Get all available primitives
            PrimitiveType[] primitiveTypes = Enum.GetValues<PrimitiveType>();

            // Check for all 
            foreach (PrimitiveType type in primitiveTypes)
            {
                // Check for matched - lower case enum
                if (type.ToString().ToLower() == identifier.Text)
                {
                    primitiveType = type;
                    return true;
                }
            }
            primitiveType = 0;
            return false;
        }
    }
}
