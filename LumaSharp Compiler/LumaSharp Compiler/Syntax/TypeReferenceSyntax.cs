
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace LumaSharp_Compiler.AST
{
    public enum PrimitiveType
    {
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
        Float,
        Double,
        String
    }

    public class TypeReferenceSyntax : ExpressionSyntax
    {
        // Private
        private SyntaxToken identifier = null;
        private NamespaceName namespaceName = null;
        private TypeReferenceSyntax[] parentTypes = null;
        private GenericArgumentsSyntax genericArguments = null;
        private ArrayParametersSyntax arrayParameters = null;
        private SyntaxToken colon = null;
        private SyntaxToken dot = null;
        private SyntaxToken reference = null;

        // Properties
        public override SyntaxToken StartToken
        {
            get
            {
                if(HasNamespace == true)
                {
                    return namespaceName.StartToken;
                }

                if(HasParentTypeIdentifiers == true)
                {
                    return parentTypes[0].StartToken;
                }
                return base.StartToken;
            }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                if(IsByReference == true)
                {
                    return reference;
                }

                if(IsArrayType == true)
                {
                    return arrayParameters.EndToken;
                }

                if(IsGenericType == true)
                {
                    return genericArguments.EndToken;
                }
                return base.EndToken;
            }
        }

        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public NamespaceName Namespace
        {
            get { return namespaceName; }
            internal set { namespaceName = value; }
        }

        public TypeReferenceSyntax[] ParentTypeIdentifiers
        {
            get { return parentTypes; }
            internal set {  parentTypes = value; }
        }

        public GenericArgumentsSyntax GenericArguments
        {
            get { return genericArguments; }
            internal set { genericArguments = value; }
        }

        public ArrayParametersSyntax ArrayParameters
        {
            get { return arrayParameters; }
            internal set { arrayParameters = value; }
        }

        public SyntaxToken Reference
        {
            get { return reference; }
            internal set { reference = value; }
        }

        public int NamespaceDepth
        {
            get { return HasNamespace ? namespaceName.Identifiers.Length : 0; }
        }

        public int NestedDepth
        {
            get { return IsNested ? parentTypes.Length : 0; }
        }

        public int GenericArgumentCount
        {
            get { return IsGenericType ? genericArguments.GenericTypeCount : 0; }
        }

        public int ArrayParameterRank
        {
            get { return IsArrayType ? arrayParameters.Rank : 0; }
        }

        public bool HasNamespace
        {
            get { return namespaceName != null; }
        }

        public bool HasParentTypeIdentifiers
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

        public bool IsByReference
        {
            get { return reference != null; }
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
        internal TypeReferenceSyntax(PrimitiveType primitive)
            : base(new SyntaxToken(primitive.ToString().ToLower()))
        {
            // Identifier
            this.identifier = base.StartToken;
        }

        internal TypeReferenceSyntax(string identifier)
            : base(new SyntaxToken(identifier))
        {
            // Identifier
            this.identifier = base.StartToken;
            this.colon = SyntaxToken.Colon();
            this.dot = SyntaxToken.Dot();
        }

        internal TypeReferenceSyntax(MemberSyntax fromMember)
            : base(fromMember.Identifier)
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

        internal TypeReferenceSyntax(TypeSyntax fromType)
            : base(fromType.Identifier)
        {
            this.identifier = fromType.Identifier;
            this.namespaceName = fromType.Namespace;
        }

        internal TypeReferenceSyntax(SyntaxTree tree, SyntaxNode parent, PrimitiveType primitive)
            : base(new SyntaxToken(primitive.ToString().ToLower()))
        {
            this.tree = tree;
            this.parent = parent;

            // Identifier
            this.identifier = base.StartToken;
        }

        internal TypeReferenceSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.PrimitiveTypeContext typeRef)
            : base(tree, parent, typeRef)
        {
            // Identifier
            this.identifier = new SyntaxToken(typeRef.Start);
        }

        internal TypeReferenceSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.TypeReferenceContext typeRef)
            : base(tree, parent, typeRef)
        {
            // Check for primitive
            LumaSharpParser.PrimitiveTypeContext primitive = typeRef.primitiveType();            
            LumaSharpParser.ArrayParametersContext array = typeRef.arrayParameters();

            if (primitive != null)
            {
                // Create primitive identifier
                this.identifier = new SyntaxToken(primitive.Start);
            }
            else
            {
                // Get identifiers
                ITerminalNode[] identifiers = typeRef.IDENTIFIER();

                // Build namespace
                if(identifiers.Length > 1)
                {
                    // Create namespace
                    this.namespaceName = new NamespaceName(tree, parent, identifiers);
                }

                // Get generics
                LumaSharpParser.GenericArgumentsContext generics = typeRef.genericArguments();

                if(generics != null)
                    this.genericArguments = new GenericArgumentsSyntax(tree, this, generics);

                // Create identifier
                this.identifier = new SyntaxToken(identifiers[identifiers.Length - 1]);
            }
            
            // Check for array
            if(array != null)
            {
                this.arrayParameters = new ArrayParametersSyntax(tree, this, array);
            }

            // Check for reference
            if (typeRef.Stop.Text == "&")
                this.reference = new SyntaxToken(typeRef.Stop);
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
            if(IsNested == true)
            {
                for(int i = 0; i < parentTypes.Length; i++)
                {
                    parentTypes[i].GetSourceText(writer);
                    dot.GetSourceText(writer);
                }
            }

            // Write identifier
            identifier.GetSourceText(writer);

            // Write generics
            if(IsGenericType == true)
                genericArguments.GetSourceText(writer);

            // Write array
            if(IsArrayType == true)
                arrayParameters.GetSourceText(writer);

            // Write reference
            if(IsByReference == true)
                reference.GetSourceText(writer);
        }

        internal bool GetPrimitiveType(out PrimitiveType primitiveType)
        {
            // Get all available primitives
            PrimitiveType[] primitiveTypes = Enum.GetValues<PrimitiveType>();

            // Check for all 
            foreach(PrimitiveType type in primitiveTypes)
            {
                // Check for matched - lower case enum
                if(type.ToString().ToLower() == identifier.Text)
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
