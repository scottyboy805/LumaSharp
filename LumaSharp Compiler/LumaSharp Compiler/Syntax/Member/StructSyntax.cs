
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class StructSyntax : MemberSyntax
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
        internal StructSyntax(AttributeListSyntax attributes, SyntaxToken[] accessModifiers, SyntaxToken keyword, SyntaxToken identifier, GenericParameterListSyntax genericParameters, BaseTypeListSyntax baseTypes, MemberBlockSyntax members)
            : base(identifier, attributes, accessModifiers)
        {
            // Check kind
            if (keyword.Kind != SyntaxTokenKind.StructKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.StructKeyword);

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
            visitor.VisitStruct(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitStruct(this);
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
            if (HasGenericParameters == true)
            {
                // Generic parameters
                genericParameters.GetSourceText(writer);
            }

            // Check for base types
            if (HasBaseTypes == true)
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
        public static StructSyntax Struct(SyntaxToken identifier)
            => new(null, null, Token(SyntaxTokenKind.StructKeyword), identifier, null, null, null);

        public static StructSyntax WithMembers(this StructSyntax _struct, params MemberSyntax[] members)
            => new(_struct.Attributes, _struct.Modifiers, _struct.Keyword, _struct.Identifier, _struct.GenericParameters, _struct.BaseTypes, new MemberBlockSyntax(members));

        public static StructSyntax WithAttribute(this StructSyntax _struct, AttributeSyntax attribute)
            => new(AttributeList(attribute), _struct.Modifiers, _struct.Keyword, _struct.Identifier, _struct.GenericParameters, _struct.BaseTypes, _struct.Members);

        public static StructSyntax WithAttributes(this StructSyntax _struct, params AttributeSyntax[] attributes)
            => new(AttributeList(attributes), _struct.Modifiers, _struct.Keyword, _struct.Identifier, _struct.GenericParameters, _struct.BaseTypes, _struct.Members);

        public static StructSyntax WithAttributes(this StructSyntax _struct, AttributeListSyntax attributes)
            => new(attributes, _struct.Modifiers, _struct.Keyword, _struct.Identifier, _struct.GenericParameters, _struct.BaseTypes, _struct.Members);

        public static StructSyntax WithAccessModifiers(this StructSyntax _struct, params SyntaxToken[] modifiers)
            => new(_struct.Attributes, modifiers, _struct.Keyword, _struct.Identifier, _struct.GenericParameters, _struct.BaseTypes, _struct.Members);

        public static StructSyntax WithGenericParameters(this StructSyntax _struct, params GenericParameterSyntax[] genericParameters)
            => _struct.WithGenericParameters(new GenericParameterListSyntax(new(SyntaxTokenKind.CommaSymbol, genericParameters)));

        public static StructSyntax WithGenericParameters(this StructSyntax _struct, GenericParameterListSyntax genericParameters)
            => new(_struct.Attributes, _struct.Modifiers, _struct.Keyword, _struct.Identifier, genericParameters, _struct.BaseTypes, _struct.Members);

        public static StructSyntax WithBaseTypes(this StructSyntax _struct, params TypeReferenceSyntax[] baseTypes)
            => new(_struct.Attributes, _struct.Modifiers, _struct.Keyword, _struct.Identifier, _struct.GenericParameters, new(new(SyntaxTokenKind.CommaSymbol, baseTypes)), _struct.Members);

    }
}
