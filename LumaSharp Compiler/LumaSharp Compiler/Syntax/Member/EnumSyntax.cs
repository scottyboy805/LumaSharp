using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class EnumSyntax : MemberSyntax
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly BaseTypeListSyntax underlyingType;
        private readonly EnumBlockSyntax body;

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
            get { return body.EndToken; }
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

        public BaseTypeListSyntax UnderlyingType
        {
            get { return underlyingType; }
        }

        public EnumBlockSyntax Body
        {
            get { return body; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield return body; }
        }

        // Constructor
        internal EnumSyntax(AttributeListSyntax attributes, SyntaxToken[] accessModifiers, SyntaxToken keyword, SyntaxToken identifier, BaseTypeListSyntax underlyingType, EnumBlockSyntax body)
            : base(identifier, attributes, accessModifiers)
        {
            // Check kind
            if (keyword.Kind != SyntaxTokenKind.EnumKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.EnumKeyword);

            // Check null
            if (body == null)
                body = new EnumBlockSyntax(null);

            this.keyword = keyword;
            this.underlyingType = underlyingType;
            this.body = body;

            // Set parent
            if (attributes != null)
            {
                foreach (AttributeSyntax a in attributes)
                    a.parent = this;
            }
            if (underlyingType != null) underlyingType.parent = this;
            body.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitEnum(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitEnum(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Generate attributes
            base.GetSourceText(writer);

            // Keyword
            keyword.GetSourceText(writer);

            // Identifier
            identifier.GetSourceText(writer);

            // Underlying type
            if(underlyingType != null)
            {
                // Underlying type
                underlyingType.GetSourceText(writer);
            }

            // Member block
            body.GetSourceText(writer);
        }
    }

    public static partial class Syntax
    {
        // Methods
        public static EnumSyntax Enum(SyntaxToken identifier, TypeReferenceSyntax underlyingType = null) 
            => new (null, null, Token(SyntaxTokenKind.EnumKeyword), identifier, underlyingType != null ? new(new(SyntaxTokenKind.CommaSymbol, underlyingType)) : null, null);


        public static EnumSyntax WithAttribute(this EnumSyntax _enum, AttributeSyntax attribute)
            => new(AttributeList(attribute), _enum.Modifiers, _enum.Keyword, _enum.Identifier, _enum.UnderlyingType, _enum.Body);

        public static EnumSyntax WithAttributes(this EnumSyntax _enum, params AttributeSyntax[] attributes)
            => new(AttributeList(attributes), _enum.Modifiers, _enum.Keyword, _enum.Identifier, _enum.UnderlyingType, _enum.Body);

        public static EnumSyntax WithAttributes(this EnumSyntax _enum, AttributeListSyntax attributes)
            => new (attributes, _enum.Modifiers, _enum.Keyword, _enum.Identifier, _enum.UnderlyingType, _enum.Body);

        public static EnumSyntax WithAccessModifiers(this EnumSyntax _enum, params SyntaxToken[] modifiers)
            => new (_enum.Attributes, modifiers, _enum.Keyword, _enum.Identifier, _enum.UnderlyingType, _enum.Body);

        public static EnumSyntax WithMembers(this EnumSyntax _enum, params EnumFieldSyntax[] fields)
            => new (_enum.Attributes, _enum.Modifiers, _enum.Keyword, _enum.Identifier, _enum.UnderlyingType, new EnumBlockSyntax(new SeparatedSyntaxList<EnumFieldSyntax>(SyntaxTokenKind.CommaSymbol, fields)));

        public static EnumSyntax WithUnderlyingType(this EnumSyntax _enum, TypeReferenceSyntax underlyingType)
            => new (_enum.Attributes, _enum.Modifiers, _enum.Keyword, _enum.Identifier, new(new(SyntaxTokenKind.CommaSymbol, underlyingType)), _enum.Body);
    }
}
