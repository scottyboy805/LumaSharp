using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class InterfaceSyntax : MemberSyntax
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
        internal InterfaceSyntax(AttributeListSyntax attributes, SyntaxToken[] accessModifiers, SyntaxToken keyword, SyntaxToken identifier, GenericParameterListSyntax genericParameters, BaseTypeListSyntax baseTypes, MemberBlockSyntax members)
            : base(identifier, attributes, accessModifiers)
        {
            // Check kind
            if (keyword.Kind != SyntaxTokenKind.InterfaceKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.InterfaceKeyword);

            // Check null
            if (members == null)
                members = new MemberBlockSyntax(Enumerable.Empty<MemberSyntax>());

            this.keyword = keyword;
            this.genericParameters = genericParameters;
            this.baseTypes = baseTypes;
            this.members = members;

            // Set parent
            if(attributes != null)
            {
                foreach (AttributeSyntax a in attributes)
                    a.parent = this;
            }
            if (genericParameters != null) genericParameters.parent = this;
            if (baseTypes != null) baseTypes.parent = this;
            members.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitInterface(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitInterface(this);
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
        public static InterfaceSyntax Interface(SyntaxToken identifier) 
            => new (null, null, Token(SyntaxTokenKind.InterfaceKeyword), identifier, null, null, null);


        public static InterfaceSyntax WithAttribute(this InterfaceSyntax _interface, AttributeSyntax attribute)
            => new(AttributeList(attribute), _interface.Modifiers, _interface.Keyword, _interface.Identifier, _interface.GenericParameters, _interface.BaseTypes, _interface.Members);

        public static InterfaceSyntax WithAttributes(this InterfaceSyntax _interface, params AttributeSyntax[] attributes)
            => new(AttributeList(attributes), _interface.Modifiers, _interface.Keyword, _interface.Identifier, _interface.GenericParameters, _interface.BaseTypes, _interface.Members);

        public static InterfaceSyntax WithAttributes(this InterfaceSyntax _interface, AttributeListSyntax attributes)
            => new (attributes, _interface.Modifiers, _interface.Keyword, _interface.Identifier, _interface.GenericParameters, _interface.BaseTypes, _interface.Members);

        public static InterfaceSyntax WithAccessModifiers(this InterfaceSyntax _interface, params SyntaxToken[] modifiers)
            => new (_interface.Attributes, modifiers, _interface.Keyword, _interface.Identifier, _interface.GenericParameters, _interface.BaseTypes, _interface.Members);

        public static InterfaceSyntax WithGenericParameters(this InterfaceSyntax _interface, GenericParameterListSyntax genericParameters)
            => new (_interface.Attributes, _interface.Modifiers, _interface.Keyword, _interface.Identifier, genericParameters, _interface.BaseTypes, _interface.Members);

        public static InterfaceSyntax WithGenericParameters(this InterfaceSyntax contract, params GenericParameterSyntax[] genericParameters)
            => contract.WithGenericParameters(new GenericParameterListSyntax(new(SyntaxTokenKind.CommaSymbol, genericParameters)));

        public static InterfaceSyntax WithBaseTypes(this InterfaceSyntax _interface, params TypeReferenceSyntax[] baseTypes)
            => new (_interface.Attributes, _interface.Modifiers, _interface.Keyword, _interface.Identifier, _interface.GenericParameters, new(new(SyntaxTokenKind.CommaSymbol, baseTypes)), _interface.Members);

        public static InterfaceSyntax WithMembers(this InterfaceSyntax _interface, params MemberSyntax[] members)
            => new(_interface.Attributes, _interface.Modifiers, _interface.Keyword, _interface.Identifier, _interface.GenericParameters, _interface.BaseTypes, new MemberBlockSyntax(members));

    }
}
