
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace LumaSharp_Compiler.Syntax
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

    public class TypeReferenceSyntax : SyntaxNode
    {
        // Private
        private SyntaxToken identifier = null;
        private SyntaxToken start = null;
        private SyntaxToken end = null;
        private SyntaxToken[] namespaceIdentifiers = null;
        private TypeReferenceSyntax[] parentTypes = null;
        private GenericArgumentsSyntax genericArguments = null;
        private ArrayParametersSyntax arrayParameters = null;
        private SyntaxToken reference = null;

        // Properties
        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public override SyntaxToken StartToken
        {
            get { return start; }
        }

        public override SyntaxToken EndToken
        {
            get { return end; }
        }

        public SyntaxToken[] NamespaceIdentifiers
        {
            get { return namespaceIdentifiers; }
        }

        public TypeReferenceSyntax[] ParentTypeIdentifiers
        {
            get { return parentTypes; }
        }

        public GenericArgumentsSyntax GenericArguments
        {
            get { return genericArguments; }
        }

        public ArrayParametersSyntax ArrayParameters
        {
            get { return arrayParameters; }
        }

        public SyntaxToken Reference
        {
            get { return reference; }
        }

        public int NamespaceDepth
        {
            get { return HasNamespace ? namespaceIdentifiers.Length : 0; }
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
            get { return namespaceIdentifiers != null; }
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
            get { return Enum.TryParse(typeof(PrimitiveType), identifier.Text, true, out _); }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal TypeReferenceSyntax(SyntaxTree tree, SyntaxNode parent, PrimitiveType type, int arrayRank = 0)
            : base(tree, parent)
        {
            this.identifier = new SyntaxToken(type.ToString().ToLower());
            this.start = identifier;

            // Create array type
            if (arrayRank > 0)
            {
                this.arrayParameters = new ArrayParametersSyntax(tree, this, arrayRank);
                this.end = this.arrayParameters.EndToken;
            }
        }

        internal TypeReferenceSyntax(SyntaxTree tree, SyntaxNode parent, string typeName, TypeReferenceSyntax[] genericArguments, int arrayRank = 0)
            : base(tree, parent)
        {
            this.identifier = new SyntaxToken(typeName);
            this.start = identifier;

            // Create generics
            if (genericArguments != null && genericArguments.Length > 0)
            {
                this.genericArguments = new GenericArgumentsSyntax(tree, this, genericArguments);
                this.end = this.genericArguments.EndToken;
            }

            // Create array type
            if (arrayRank > 0)
            {
                this.arrayParameters = new ArrayParametersSyntax(tree, this, arrayRank);
                this.end = this.arrayParameters.EndToken;
            }
        }

        internal TypeReferenceSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.TypeReferenceContext typeRef)
            : base(tree, parent)
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
                    // Create array
                    this.namespaceIdentifiers = new SyntaxToken[identifiers.Length - 1];

                    for (int i = 0; i < identifiers.Length - 1; i++)
                    {
                        this.namespaceIdentifiers[i] = new SyntaxToken(identifiers[i]);
                    }
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
            if(HasNamespace == true)
            {
                for(int i = 0; i < namespaceIdentifiers.Length; i++)
                {
                    writer.Write(namespaceIdentifiers[i].ToString());
                    writer.Write('.');
                }
            }

            // Write parent types
            if(IsNested == true)
            {
                for(int i = 0; i < parentTypes.Length; i++)
                {
                    parentTypes[i].GetSourceText(writer);

                    if(i < parentTypes.Length - 1)
                        writer.Write('.');
                }
            }

            // Write identifier
            writer.Write(identifier.ToString());

            // Write generics
            if(IsGenericType == true)
                genericArguments.GetSourceText(writer);

            // Write array
            if(IsArrayType == true)
                arrayParameters.GetSourceText(writer);

            // Write reference
            if(IsByReference == true)
                writer.Write(reference.ToString());
        }
    }
}
