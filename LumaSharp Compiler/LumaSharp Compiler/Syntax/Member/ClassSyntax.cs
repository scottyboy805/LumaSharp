
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class ClassSyntax : MemberSyntax
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly GenericParameterListSyntax genericParameters;
        private readonly BaseTypeListSyntax baseTypes;
        private readonly MemberBlockSyntax members;

        // Properties
        public override SyntaxToken StartToken
        {
            get 
            {
                // Check for attributes
                if (attributes != null)
                    return attributes[0].StartToken;

                // Check for modifiers
                if (modifiers != null)
                    return modifiers[0];

                // Fallback to keyword
                return keyword; 
            }
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
        internal ClassSyntax(AttributeListSyntax attributes, SyntaxToken[] accessModifiers, SyntaxToken keyword, SyntaxToken identifier, GenericParameterListSyntax genericParameters, BaseTypeListSyntax baseTypes, MemberBlockSyntax members)
            : base(identifier, attributes, accessModifiers)
        {
            // Check kind
            if (keyword.Kind != SyntaxTokenKind.ClassKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.ClassKeyword);

            // Check null
            if (members == null)
                members = new MemberBlockSyntax(Enumerable.Empty<MemberSyntax>());

            // Members
            this.keyword = keyword;
            this.genericParameters = genericParameters;
            this.baseTypes = baseTypes;
            this.members = members;

            // Set parent
            if (attributes != null)
            {
                foreach (AttributeSyntax a in this.attributes)
                    a.parent = this;
            }
            if (genericParameters != null) this.genericParameters.parent = this;
            if (baseTypes != null) this.baseTypes.parent = this;
            this.members.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitClass(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitClass(this);
        }

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

            // Check for base types
            if(HasBaseTypes == true)
            {
                // Base types
                baseTypes.GetSourceText(writer);
            }

            // Write block
            members.GetSourceText(writer);
        }
    }

    public static partial class Syntax
    {
        // Methods
        public static ClassSyntax Class(SyntaxToken identifier) 
            => new (null, null, Token(SyntaxTokenKind.ClassKeyword), identifier,  null, null, null);

        public static ClassSyntax WithMembers(this ClassSyntax _class, params MemberSyntax[] members)
            => new (_class.Attributes, _class.Modifiers, _class.Keyword, _class.Identifier, _class.GenericParameters, _class.BaseTypes, new MemberBlockSyntax(members));

        public static ClassSyntax WithAttribute(this ClassSyntax _class, AttributeSyntax attribute)
            => new(AttributeList(attribute), _class.Modifiers, _class.Keyword, _class.Identifier, _class.GenericParameters, _class.BaseTypes, _class.Members);

        public static ClassSyntax WithAttributes(this ClassSyntax _class, params AttributeSyntax[] attributes)
            => new(AttributeList(attributes), _class.Modifiers, _class.Keyword, _class.Identifier, _class.GenericParameters, _class.BaseTypes, _class.Members);

        public static ClassSyntax WithAttributes(this ClassSyntax _class, AttributeListSyntax attributes)
            => new (attributes, _class.Modifiers, _class.Keyword, _class.Identifier, _class.GenericParameters, _class.BaseTypes, _class.Members);

        public static ClassSyntax WithAccessModifiers(this ClassSyntax _class, params SyntaxToken[] modifiers)
            => new (_class.Attributes, modifiers, _class.Keyword, _class.Identifier, _class.GenericParameters, _class.BaseTypes, _class.Members);

        public static ClassSyntax WithGenericParameters(this ClassSyntax _class, params GenericParameterSyntax[] genericParameters)
            => _class.WithGenericParameters(new GenericParameterListSyntax(new(SyntaxTokenKind.CommaSymbol, genericParameters)));

        public static ClassSyntax WithGenericParameters(this ClassSyntax _class, GenericParameterListSyntax genericParameters)
            => new (_class.Attributes, _class.Modifiers, _class.Keyword, _class.Identifier, genericParameters, _class.BaseTypes, _class.Members);

        public static ClassSyntax WithBaseTypes(this ClassSyntax _class, params TypeReferenceSyntax[] baseTypes)
            => new (_class.Attributes, _class.Modifiers, _class.Keyword, _class.Identifier, _class.GenericParameters, new(new(SyntaxTokenKind.CommaSymbol, baseTypes)), _class.Members);

    }
}
