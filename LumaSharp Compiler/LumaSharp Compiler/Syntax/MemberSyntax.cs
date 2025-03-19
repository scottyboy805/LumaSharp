using Antlr4.Runtime.Tree;
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
        protected MemberSyntax(SyntaxNode parent, string identifier, AttributeReferenceSyntax[] attributes, SyntaxToken[] accessModifiers)
            : base(parent)
        {
            this.identifier = Syntax.Identifier(identifier);
            this.attributes = attributes;
            this.accessModifiers = accessModifiers;
        }

        internal MemberSyntax(ITerminalNode identifier, SyntaxNode parent, LumaSharpParser.AttributeReferenceContext[] attributes, LumaSharpParser.AccessModifierContext[] modifiers)
            : base(parent)
        {
            this.identifier = new SyntaxToken(SyntaxTokenKind.Identifier, identifier);

            // Attributes
            if(attributes != null && attributes.Length > 0)
            {
                this.attributes = attributes.Select(a => new AttributeReferenceSyntax(this, a)).ToArray();
            }

            // Access modifiers
            if (modifiers != null && modifiers.Length > 0)
            {
                this.accessModifiers = GetModifiers(modifiers);
            }
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

        private SyntaxToken[] GetModifiers(LumaSharpParser.AccessModifierContext[] modifiers)
        {
            SyntaxToken[] tokens = new SyntaxToken[modifiers.Length];

            // Process all
            for (int i = 0; i < tokens.Length; i++)
            {
                // Check for hidden
                if (modifiers[i].SPECIALHIDDEN() != null)
                {
                    tokens[i] = new SyntaxToken(SyntaxTokenKind.HiddenKeyword, modifiers[i].SPECIALHIDDEN());
                }
                // Check for internal
                else if (modifiers[i].INTERNAL() != null)
                {
                    tokens[i] = new SyntaxToken(SyntaxTokenKind.InternalKeyword, modifiers[i].INTERNAL());
                }
                // Check for export
                else if (modifiers[i].EXPORT() != null)
                {
                    tokens[i] = new SyntaxToken(SyntaxTokenKind.ExportKeyword, modifiers[i].EXPORT());
                }
                // Check for global
                else if (modifiers[i].GLOBAL() != null)
                {
                    tokens[i] = new SyntaxToken(SyntaxTokenKind.GlobalKeyword, modifiers[i].GLOBAL());
                }
                else
                {
                    tokens[i] = SyntaxToken.Invalid;
                }
            }
            return tokens;
        }

        internal static SyntaxNode RootElement(SyntaxNode parent, LumaSharpParser.RootElementContext element)
        {
            // Check for namespace
            if (element.namespaceDeclaration() != null)
                return new NamespaceSyntax(parent, element.namespaceDeclaration());

            // Get member
            return RootMember(parent, element.rootMember());
        }

        internal static MemberSyntax RootMember(SyntaxNode parent, LumaSharpParser.RootMemberContext member)
        {
            // Check for type declaration
            if (member.typeDeclaration() != null)
                return new TypeSyntax(parent, member.typeDeclaration());

            // Check for contract declaration
            if (member.contractDeclaration() != null)
                return new ContractSyntax(parent, member.contractDeclaration());

            // Check for enum declaration
            if (member.enumDeclaration() != null)
                return new EnumSyntax(parent, member.enumDeclaration());

            // Not valid
            throw new NotSupportedException("Unsupported root member type: " + member);
        }

        internal static MemberSyntax Member(SyntaxNode parent, LumaSharpParser.MemberDeclarationContext member)
        {
            // Check for type
            if (member.typeDeclaration() != null)
                return new TypeSyntax(parent, member.typeDeclaration());

            // Check for contract
            if(member.contractDeclaration() != null)
                return new ContractSyntax(parent, member.contractDeclaration());

            // Check for enum
            if (member.enumDeclaration() != null)
                return new EnumSyntax(parent, member.enumDeclaration());

            // Check for field
            if (member.fieldDeclaration() != null)
                return new FieldSyntax(parent, member.fieldDeclaration());

            // Check for accessor
            if (member.accessorDeclaration() != null)
                return new AccessorSyntax(parent, member.accessorDeclaration());

            // Check for method
            if(member.methodDeclaration() != null)
                return new MethodSyntax(parent, member.methodDeclaration());

            // Not valid
            throw new NotSupportedException("Unsupported member type: " + member);
        }
    }
}
