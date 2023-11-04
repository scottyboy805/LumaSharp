
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
    }

    public class TypeReferenceSyntax : ExpressionSyntax
    {
        // Private
        private SyntaxToken identifier = null;
        private NamespaceName namespaceName = null;
        private TypeReferenceSyntax parentType = null;
        private GenericArgumentsSyntax genericArguments = null;
        private ArrayParametersSyntax arrayParameters = null;
        private SyntaxToken colon = null;
        private SyntaxToken dot = null;

        // Properties
        public override SyntaxToken StartToken
        {
            get
            {
                if(HasNamespace == true)
                {
                    return namespaceName.StartToken;
                }

                if(HasParentTypeIdentifier == true)
                {
                    return parentType.StartToken;
                }
                return base.StartToken;
            }
        }

        public override SyntaxToken EndToken
        {
            get
            {
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

        public TypeReferenceSyntax ParentTypeIdentifier
        {
            get { return parentType; }
            internal set {  parentType = value; }
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

        public int NamespaceDepth
        {
            get { return HasNamespace ? namespaceName.Identifiers.Length : 0; }
        }

        public int NestedDepth
        {
            get { return IsNested ? parentType.NestedDepth + 1 : 0; }
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

        public bool HasParentTypeIdentifier
        {
            get { return parentType != null; }
        }

        public bool IsNested
        {
            get { return parentType != null; }
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
        internal TypeReferenceSyntax(PrimitiveType primitive)
            : base(new SyntaxToken(primitive.ToString().ToLower()))
        {
            // Identifier
            this.identifier = base.StartToken;
        }

        internal TypeReferenceSyntax(string identifier, TypeReferenceSyntax parentType = null)
            : base(new SyntaxToken(identifier))
        {
            // Identifier
            this.identifier = base.StartToken;
            this.parentType = parentType;
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

            if(primitive != null)
            {
                // Create primitive identifier
                this.identifier = new SyntaxToken(primitive.Start);
            }
            else
            {
                // Get parent type
                LumaSharpParser.ParentTypeReferenceContext parentTypeRef = typeRef.parentTypeReference();

                // Get identifiers
                ITerminalNode[] identifiers = typeRef.IDENTIFIER();
                ITerminalNode[] parentIdentifiers = (parentTypeRef != null) ? parentTypeRef.IDENTIFIER() : null;

                // Build namespace
                if(identifiers.Length > 1)
                {
                    // Create namespace
                    this.namespaceName = new NamespaceName(tree, parent, parentTypeRef == null 
                        ? identifiers.Take(identifiers.Length - 1).ToArray()
                        : identifiers);
                }

                // Get parent
                if(parentIdentifiers != null)
                {
                    //this.parentTypes
                }

                // Get generics
                LumaSharpParser.GenericArgumentsContext generics = typeRef.genericArguments();

                if(generics != null)
                    this.genericArguments = new GenericArgumentsSyntax(tree, this, generics);

                if (parentTypeRef == null)
                {
                    // Create identifier
                    this.identifier = new SyntaxToken(identifiers[identifiers.Length - 1]);
                }
                else
                {
                    // Create identifier
                    this.identifier = new SyntaxToken(parentIdentifiers[parentIdentifiers.Length - 1]);
                }
            }
            
            // Check for array
            if(array != null)
            {
                this.arrayParameters = new ArrayParametersSyntax(tree, this, array);
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
            if(IsNested == true)
            {
                // Write parent type
                parentType.GetSourceText(writer);
            }

            // Write identifier
            identifier.GetSourceText(writer);

            // Write generics
            if(IsGenericType == true)
                genericArguments.GetSourceText(writer);

            // Write array
            if(IsArrayType == true)
                arrayParameters.GetSourceText(writer);
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
