
using LumaSharp.Compiler.AST.Visitor;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

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
            get { return keyword; }
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
        internal EnumSyntax(SyntaxToken identifier, AttributeSyntax[] attributes, SyntaxToken[] accessModifiers, BaseTypeListSyntax underlyingType, EnumBlockSyntax body)
            : this(
                  Syntax.Token(SyntaxTokenKind.EnumKeyword),
                  identifier,
                  attributes,
                  accessModifiers,
                  underlyingType,
                  body)
        {
        }

        internal EnumSyntax(SyntaxToken keyword, SyntaxToken identifier, AttributeSyntax[] attributes, SyntaxToken[] accessModifiers, BaseTypeListSyntax underlyingType, EnumBlockSyntax body)
            : base(identifier, attributes, accessModifiers)
        {
            // Check kind
            if (keyword.Kind != SyntaxTokenKind.EnumKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.EnumKeyword);

            // Check null
            if (body == null)
                body = new EnumBlockSyntax(new SeparatedSyntaxList<EnumFieldSyntax>(SyntaxTokenKind.CommaSymbol));

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
}
