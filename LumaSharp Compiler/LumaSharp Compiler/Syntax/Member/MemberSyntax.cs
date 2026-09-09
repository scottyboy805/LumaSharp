
namespace LumaSharp.Compiler.AST
{
    public abstract class MemberSyntax : SyntaxNode
    {
        // Protected
        protected readonly AttributeListSyntax attributes;
        protected readonly SyntaxToken[] modifiers;
        protected readonly SyntaxToken identifier;

        // Properties
        public override SyntaxToken StartToken
        {
            get
            {
                // Check for attributes
                if (HasAttributes == true)
                    return attributes[0].StartToken;

                // Check for modifiers
                if (HasModifiers == true)
                    return modifiers[0];

                return identifier;
            }
        }

        public AttributeListSyntax Attributes
        {
            get { return attributes; }
        }

        public SyntaxToken[] Modifiers
        {
            get { return modifiers; }
        }

        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public int AttributeCount
        {
            get { return HasAttributes ? attributes.Count : 0; }
        }

        public int ModifierCount
        {
            get { return HasModifiers ? modifiers.Length : 0; }
        }

        public bool HasAttributes
        {
            get { return attributes != null; }
        }

        public bool HasModifiers
        {
            get { return modifiers != null; }
        }

        // Constructor
        protected MemberSyntax(SyntaxToken identifier, AttributeListSyntax attributes, SyntaxToken[] modifiers)
        {
            // Check for identifier
            if(identifier.Kind != SyntaxTokenKind.Identifier)
                throw new ArgumentException(nameof(identifier) + " must be of kind: " + SyntaxTokenKind.Identifier);

            this.identifier = identifier;
            this.attributes = attributes;
            this.modifiers = modifiers;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            if (HasAttributes == true)
            {
                // Get custom attributes
                foreach (AttributeSyntax attribute in attributes)
                {
                    attribute.GetSourceText(writer);
                }
            }

            if (HasModifiers == true)
            {
                // Get modifiers
                foreach (SyntaxToken modifier in modifiers)
                {
                    modifier.GetSourceText(writer);
                }
            }
        }
    }
}
