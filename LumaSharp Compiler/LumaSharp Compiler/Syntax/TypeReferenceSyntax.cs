using Antlr4.Runtime.Tree;
using LumaSharp.Runtime.Reflection;

namespace LumaSharp.Compiler.AST
{
    public enum PrimitiveType : byte
    {
        Void,
        Any = 1,
        Bool,
        Char,
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
        internal ParentTypeReferenceSyntax(SyntaxNode parent, string identifier, GenericArgumentListSyntax genericArguments)
            : base(parent)
        {
            this.identifier = Syntax.Identifier(identifier);
            this.genericArguments = genericArguments;
            this.dot = Syntax.KeywordOrSymbol(SyntaxTokenKind.DotSymbol);
        }

        internal ParentTypeReferenceSyntax(SyntaxNode parent, LumaSharpParser.ParentTypeReferenceContext parentType)
            : base(parent)
        {
            // Identifier
            this.identifier = new SyntaxToken(SyntaxTokenKind.Identifier, parentType.IDENTIFIER());

            // Generic
            if (parentType.genericArgumentList() != null)
                this.genericArguments = new GenericArgumentListSyntax(this, parentType.genericArgumentList());

            // Dot
            this.dot = new SyntaxToken(SyntaxTokenKind.DotSymbol, parentType.DOT());
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
            : base(null, null)
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

        internal TypeReferenceSyntax(SyntaxNode parent, PrimitiveType primitive)
            : base(parent, null)
        {
            this.identifier = Syntax.Identifier(primitive.ToString().ToLower());
        }

        internal TypeReferenceSyntax(SyntaxNode parent, SeparatedTokenList namespaceName, ParentTypeReferenceSyntax[] parentTypes, string identifier, GenericArgumentListSyntax genericArguments, ArrayParametersSyntax arrayParameters)
            : base(parent, null)
        {
            this.identifier = Syntax.Identifier(identifier);
            this.namespaceName = namespaceName;
            this.parentTypes = parentTypes;
            this.genericArguments = genericArguments;
            this.arrayParameters = arrayParameters;
        }

        internal TypeReferenceSyntax(SyntaxNode parent, LumaSharpParser.PrimitiveTypeContext typeRef)
            : base(parent, null)
        {
            // Identifier
            this.identifier = GetPrimitiveType(typeRef);
        }

        internal TypeReferenceSyntax(SyntaxNode parent, LumaSharpParser.ExpressionContext expression, LumaSharpParser.TypeReferenceContext typeRef)
            : base(parent, expression)
        {
            // Check for primitive
            LumaSharpParser.PrimitiveTypeContext primitive = typeRef.primitiveType();
            LumaSharpParser.ArrayParametersContext array = typeRef.arrayParameters();

            if (primitive != null)
            {
                // Create primitive identifier
                this.identifier = GetPrimitiveType(primitive);
            }
            else
            {
                // Namespace
                if(typeRef.namespaceName() != null)
                {
                    this.namespaceName = new SeparatedTokenList(this, typeRef.namespaceName());
                    this.colon = new SyntaxToken(SyntaxTokenKind.ColonSymbol, typeRef.COLON());
                }

                // Get parent type
                if(typeRef.parentTypeReference() != null && typeRef.parentTypeReference().Length > 0)
                {
                    this.parentTypes = typeRef.parentTypeReference().Select(p => new ParentTypeReferenceSyntax(this, p)).ToArray();
                }

                // Identifier
                this.identifier = new SyntaxToken(SyntaxTokenKind.Identifier, typeRef.IDENTIFIER());


                // Get generics
                if (typeRef.genericArgumentList() != null)
                {
                    this.genericArguments = new GenericArgumentListSyntax(this, typeRef.genericArgumentList());
                }
            }

            // Check for array
            if (array != null)
            {
                this.arrayParameters = new ArrayParametersSyntax(this, array);
            }
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

        private SyntaxToken GetPrimitiveType(LumaSharpParser.PrimitiveTypeContext primitive)
        {
            // Check for any
            if (primitive.ANY() != null)
            {
                return new SyntaxToken(SyntaxTokenKind.AnyKeyword, primitive.ANY());
            }
            // Check for bool
            else if (primitive.BOOL() != null)
            {
                return new SyntaxToken(SyntaxTokenKind.BoolKeyword, primitive.BOOL());
            }
            // Check for char
            else if (primitive.CHAR() != null)
            {
                return new SyntaxToken(SyntaxTokenKind.CharKeyword, primitive.CHAR());
            }
            // Check for I8
            else if (primitive.I8() != null)
            {
                return new SyntaxToken(SyntaxTokenKind.I8Keyword, primitive.I8());
            }
            // Check for U8
            else if (primitive.U8() != null)
            {
                return new SyntaxToken(SyntaxTokenKind.U8Keyword, primitive.U8());
            }
            // Check for I16
            else if (primitive.I16() != null)
            {
                return new SyntaxToken(SyntaxTokenKind.I16Keyword, primitive.I16());
            }
            // Check for U16
            else if (primitive.U16() != null)
            {
                return new SyntaxToken(SyntaxTokenKind.U16Keyword, primitive.U16());
            }
            // Check for I32
            else if (primitive.I32() != null)
            {
                return new SyntaxToken(SyntaxTokenKind.I32Keyword, primitive.I32());
            }
            // Check for U32
            else if (primitive.U32() != null)
            {
                return new SyntaxToken(SyntaxTokenKind.U32Keyword, primitive.U32());
            }
            // Check for I64
            else if (primitive.I64() != null)
            {
                return new SyntaxToken(SyntaxTokenKind.I64Keyword, primitive.I64());
            }
            // Check for U64
            else if (primitive.U64() != null)
            {
                return new SyntaxToken(SyntaxTokenKind.U64Keyword, primitive.U64());
            }
            // Check for F32
            else if (primitive.F32() != null)
            {
                return new SyntaxToken(SyntaxTokenKind.F32Keyword, primitive.F32());
            }
            // Check for F64
            else if (primitive.F64() != null)
            {
                return new SyntaxToken(SyntaxTokenKind.F64Keyword, primitive.F64());
            }
            // Check for string
            else if (primitive.STRING() != null)
            {
                return new SyntaxToken(SyntaxTokenKind.StringKeyword, primitive.STRING());
            }
            // Check for void
            else if (primitive.VOID() != null)
            {
                return new SyntaxToken(SyntaxTokenKind.VoidKeyword, primitive.VOID());
            }

            // Should never happen
            return SyntaxToken.Invalid;
        }
    }
}
