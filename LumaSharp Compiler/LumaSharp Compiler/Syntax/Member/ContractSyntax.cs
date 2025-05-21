
namespace LumaSharp.Compiler.AST
{
    public sealed class ContractSyntax : MemberSyntax, IMemberSyntaxContainer
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly GenericParameterListSyntax genericParameters;
        private readonly BaseTypeListSyntax baseTypes;
        private readonly MemberBlockSyntax members;

        // Properties
        public override SyntaxToken EndToken
        {
            get { return members.EndToken; }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public SeparatedTokenList Namespace
        {
            get
            {
                SyntaxNode current = Parent;

                // Move up until end or namespace
                while (current != null && (current is NamespaceSyntax) == false)
                    current = current.Parent;

                // Try to get namespace
                NamespaceSyntax ns = current as NamespaceSyntax;

                // Get the name
                if (ns != null)
                    return ns.Name;

                return null;
            }
        }

        public GenericParameterListSyntax GenericParameters
        {
            get { return genericParameters; }
        }

        public BaseTypeListSyntax BaseTypes
        {
            get { return baseTypes; }
        }

        public MemberBlockSyntax Members
        {
            get { return members; }
        }

        public int GenericParameterCount
        {
            get { return HasGenericParameters ? genericParameters.Count : 0; }
        }

        public int BaseTypeCount
        {
            get { return HasBaseTypes ? baseTypes.Count : 0; }
        }

        public int MemberCount
        {
            get { return HasMembers ? members.Count : 0; }
        }

        public bool HasGenericParameters
        {
            get { return genericParameters != null; }
        }

        public bool HasBaseTypes
        {
            get { return baseTypes != null; }
        }

        public bool HasMembers
        {
            get { return members.Count > 0; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { return members; }
        }

        // Constructor
        internal ContractSyntax(SyntaxToken identifier, AttributeReferenceSyntax[] attributes, SyntaxToken[] accessModifiers, GenericParameterListSyntax genericParameter, BaseTypeListSyntax baseTypes, MemberBlockSyntax members)
            : this(
                  new SyntaxToken(SyntaxTokenKind.ContractKeyword),
                  identifier,
                  attributes,
                  accessModifiers,
                  genericParameter,
                  baseTypes, 
                  members)
        {
        }

        internal ContractSyntax(SyntaxToken keyword, SyntaxToken identifier, AttributeReferenceSyntax[] attributes, SyntaxToken[] accessModifiers, GenericParameterListSyntax genericParameters, BaseTypeListSyntax baseTypes, MemberBlockSyntax members)
            : base(identifier, attributes, accessModifiers)
        {
            // Check kind
            if (keyword.Kind != SyntaxTokenKind.ContractKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.ContractKeyword);

            // Check null
            if (members == null)
                throw new ArgumentNullException(nameof(members));

            this.keyword = keyword;
            this.genericParameters = genericParameters;
            this.baseTypes = baseTypes;
            this.members = members;

            // Set parent
            if(attributes != null)
            {
                foreach (AttributeReferenceSyntax a in attributes)
                    a.parent = this;
            }
            if (genericParameters != null) genericParameters.parent = this;
            if (baseTypes != null) baseTypes.parent = this;
            members.parent = this;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Generate attributes
            base.GetSourceText(writer);

            // Keyword
            keyword.GetSourceText(writer);

            // Identifier
            identifier.GetSourceText(writer);

            // Generics
            if(HasGenericParameters == true)
            {
                genericParameters.GetSourceText(writer);
            }

            // Check for base types
            if(HasBaseTypes == true)
            {
                // Base types
                baseTypes.GetSourceText(writer);
            }

            // Write block
            members.GetSourceText(writer);
        }

        public void AddMember(MemberSyntax member)
        {
            //((IMemberSyntaxContainer)memberBlock).AddMember(member);

            // Update hierarchy
            member.parent = this;
        }
    }
}
