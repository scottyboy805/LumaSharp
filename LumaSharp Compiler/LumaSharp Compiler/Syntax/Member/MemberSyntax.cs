
namespace LumaSharp.Compiler.AST
{
    public enum AccessModifier
    {
        Export,
        Internal,
        Hidden,
        Global,
    }

    public abstract class MemberSyntax : SyntaxNode
    {
        // Protected
        protected readonly AttributeReferenceSyntax[] attributes;
        protected readonly SyntaxToken[] accessModifiers;
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
                if (HasAccessModifiers == true)
                    return accessModifiers[0];

                return identifier;
            }
        }

        public AttributeReferenceSyntax[] Attributes
        {
            get { return attributes; }
        }

        public SyntaxToken[] AccessModifiers
        {
            get { return accessModifiers; }
        }

        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public int AttributeCount
        {
            get { return HasAttributes ? attributes.Length : 0; }
        }

        public int AccessModifierCount
        {
            get { return HasAccessModifiers ? accessModifiers.Length : 0; }
        }

        public bool HasAttributes
        {
            get { return attributes != null; }
        }

        public bool HasAccessModifiers
        {
            get { return accessModifiers != null; }
        }

        // Constructor
        protected MemberSyntax(SyntaxToken identifier, AttributeReferenceSyntax[] attributes, SyntaxToken[] accessModifiers)
        {
            // Check for identifier
            if(identifier.Kind != SyntaxTokenKind.Identifier)
                throw new ArgumentException(nameof(identifier) + " must be of kind: " + SyntaxTokenKind.Identifier);

            this.identifier = identifier;
            this.attributes = attributes;
            this.accessModifiers = accessModifiers;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            if (HasAttributes == true)
            {
                // Get custom attributes
                foreach (AttributeReferenceSyntax attribute in attributes)
                {
                    attribute.GetSourceText(writer);
                }
            }

            if (HasAccessModifiers == true)
            {
                // Get access modifiers
                foreach (SyntaxToken modifier in accessModifiers)
                {
                    modifier.GetSourceText(writer);
                }
            }
        }
    }
}
