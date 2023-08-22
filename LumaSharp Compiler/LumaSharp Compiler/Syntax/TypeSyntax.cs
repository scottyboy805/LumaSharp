
namespace LumaSharp_Compiler.Syntax
{
    public sealed class TypeSyntax : MemberSyntax
    {
        // Private
        private SyntaxToken[] accessModifiers = null;
        private SyntaxToken keyword = null;
        private SyntaxToken start = null;
        private SyntaxToken end = null;
        private GenericParametersSyntax genericParameters = null;
        private TypeReferenceSyntax baseTypeReference = null;
        private TypeReferenceSyntax[] contractTypeReferences = null;

        private MemberSyntax[] members = null;

        // Properties
        public SyntaxToken[] AccessModifiers
        {
            get { return accessModifiers; }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public override SyntaxToken StartToken
        {
            get { return start; }
        }

        public override SyntaxToken EndToken
        {
            get { return end; }
        }

        public GenericParametersSyntax GenericParameters
        {
            get { return genericParameters; }
        }

        public TypeReferenceSyntax BaseTypeReference
        {
            get { return baseTypeReference; }
        }

        public TypeReferenceSyntax[] ContractTypeReferences
        {
            get { return contractTypeReferences; }
        }

        public MemberSyntax[] Members
        {
            get { return members; }
        }

        public bool HasAccessModifiers
        {
            get { return accessModifiers != null; }
        }

        public bool HasGenericParameters
        {
            get { return genericParameters != null; }
        }

        public bool HasBaseType
        {
            get { return baseTypeReference != null; }
        }

        public bool HasContractTypes
        {
            get { return contractTypeReferences != null; }
        }

        public bool HasMembers
        {
            get { return members != null; }
        }

        // Constructor
        internal TypeSyntax(string identifier)
            : base(identifier)
        {
            this.keyword = new SyntaxToken("type");

            this.start = this.keyword;
            this.end = this.identifier;
        }


        // Methods
        public override void GetSourceText(TextWriter writer)
        {

        }
    }
}
