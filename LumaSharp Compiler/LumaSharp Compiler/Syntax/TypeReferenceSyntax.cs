
namespace LumaSharp_Compiler.Syntax
{
    public enum PrimitiveType
    {
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
        Single,
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

        public GenericArgumentsSyntax GenericArguments
        {
            get { return genericArguments; }
        }

        public ArrayParametersSyntax ArrayParameters
        {
            get { return arrayParameters; }
        }

        public int GenericArgumentCount
        {
            get { return IsGenericType ? genericArguments.GenericTypeCount : 0; }
        }

        public int ArrayParameterRank
        {
            get { return IsArrayType ? arrayParameters.Rank : 0; }
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
            get { return Enum.TryParse(typeof(PrimitiveType), identifier.Text, true, out _); }
        }

        // Constructor
        internal TypeReferenceSyntax(PrimitiveType type, int arrayRank = 0)
        {
            this.identifier = new SyntaxToken(type.ToString().ToLower());
            this.start = identifier;

            // Create array type
            if (arrayRank > 0)
            {
                this.arrayParameters = new ArrayParametersSyntax(arrayRank);
                this.end = this.arrayParameters.EndToken;
            }
        }

        internal TypeReferenceSyntax(string typeName, TypeReferenceSyntax[] genericArguments, int arrayRank = 0)
        {
            this.identifier = new SyntaxToken(typeName);
            this.start = identifier;

            // Create generics
            if (genericArguments != null && genericArguments.Length > 0)
            {
                this.genericArguments = new GenericArgumentsSyntax(genericArguments);
                this.end = this.genericArguments.EndToken;
            }

            // Create array type
            if (arrayRank > 0)
            {
                this.arrayParameters = new ArrayParametersSyntax(arrayRank);
                this.end = this.arrayParameters.EndToken;
            }
        }

        internal TypeReferenceSyntax(LumaSharpParser.TypeReferenceContext typeRef)
        {
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write identifier
            writer.Write(identifier.ToString());

            // Write generics
            if(IsGenericType == true)
                genericArguments.GetSourceText(writer);

            // Write array
            if(IsArrayType == true)
                arrayParameters.GetSourceText(writer);
        }
    }
}
