
namespace LumaSharp.Compiler.AST
{
    public sealed class TypeSyntax : MemberSyntax, IMemberSyntaxContainer
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly GenericParameterListSyntax genericParameters;
        private readonly BaseTypeListSyntax baseTypes;
        private readonly SyntaxToken? overrideKeyword;
        private readonly MemberBlockSyntax members;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

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
                while(current != null && (current is NamespaceSyntax) == false)
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

        public SyntaxToken? Override
        {
            get { return overrideKeyword; }
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
            get { return HasMembers ? members.Count: 0; }
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
        internal TypeSyntax(SyntaxToken identifier, AttributeReferenceSyntax[] attributes, SyntaxToken[] accessModifiers, GenericParameterListSyntax genericParameters, SyntaxToken? overrideToken, BaseTypeListSyntax baseTypes, MemberBlockSyntax members)
            : this(
                  new SyntaxToken(SyntaxTokenKind.TypeKeyword),
                  identifier,
                  attributes,
                  accessModifiers,
                  genericParameters,
                  overrideToken,
                  baseTypes,
                  members)
        {
        }

        internal TypeSyntax(SyntaxToken keyword, SyntaxToken identifier, AttributeReferenceSyntax[] attributes, SyntaxToken[] accessModifiers, GenericParameterListSyntax genericParameters, SyntaxToken? overrideToken, BaseTypeListSyntax baseTypes, MemberBlockSyntax members)
            : base(identifier, attributes, accessModifiers)
        {
            // Check kind
            if (keyword.Kind != SyntaxTokenKind.TypeKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.TypeKeyword);

            // Check null
            if (members == null)
                throw new ArgumentNullException(nameof(members));

            // Members
            this.keyword = keyword;
            this.genericParameters = genericParameters;
            this.overrideKeyword = overrideToken;
            this.baseTypes = baseTypes;
            this.members = members;

            // Set parent
            if (attributes != null)
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
                // Generic parameters
                genericParameters.GetSourceText(writer);
            }

            // Check for override
            if(overrideKeyword != null)
                overrideKeyword?.GetSourceText(writer);

            // Check for base types
            if(HasBaseTypes == true)
            {
                // Base types
                baseTypes.GetSourceText(writer);
            }

            // Write block
            members.GetSourceText(writer);
        }

        void IMemberSyntaxContainer.AddMember(MemberSyntax member)
        {
            //((IMemberSyntaxContainer)members).AddMember(member);

            // Update hierarchy
            member.parent = this;
        }
    }
}
