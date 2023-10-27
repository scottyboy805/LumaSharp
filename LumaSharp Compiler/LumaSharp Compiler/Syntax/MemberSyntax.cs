using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace LumaSharp_Compiler.AST
{
    public abstract class MemberSyntax : SyntaxNode
    {
        // Protected
        protected AttributeSyntax[] attributes = null;
        protected SyntaxToken[] accessModifiers = null;
        protected SyntaxToken identifier = null;

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

                return base.StartToken;
            }
        }

        public AttributeSyntax[] Attributes
        {
            get { return attributes; }
            internal set { attributes = value; }
        }

        public SyntaxToken[] AccessModifiers
        {
            get { return accessModifiers; }
            internal set 
            { 
                accessModifiers = value;

                foreach (SyntaxToken modifier in accessModifiers)
                    modifier.WithTrailingWhitespace(" ");
            }
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
        protected MemberSyntax(string identifier, SyntaxToken start, SyntaxToken end)
            : base(start, end)
        {
            this.identifier = new SyntaxToken(identifier);
        }

        internal MemberSyntax(ITerminalNode identifier, SyntaxTree tree, SyntaxNode parent, ParserRuleContext context, LumaSharpParser.AttributeDeclarationContext[] attributes, LumaSharpParser.AccessModifierContext[] modifiers)
            : base(tree, parent, context)
        {
            this.identifier = new SyntaxToken(identifier);

            // Attributes
            if(attributes != null && attributes.Length > 0)
            {
                this.attributes = attributes.Select(a => new AttributeSyntax(tree, this, a)).ToArray();
            }

            // Access modifiers
            if (modifiers != null && modifiers.Length > 0)
            {
                this.accessModifiers = modifiers.Select(m => new SyntaxToken(m.Start)).ToArray();
            }
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

            if (HasAccessModifiers == true)
            {
                // Get access modifiers
                foreach (SyntaxToken modifier in accessModifiers)
                {
                    modifier.GetSourceText(writer);
                }
            }
        }

        internal static SyntaxNode RootElement(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.RootElementContext element)
        {
            // Check for namespace
            if (element.namespaceDeclaration() != null)
                return new NamespaceSyntax(tree, parent, element.namespaceDeclaration());

            // Get member
            return RootMember(tree, parent, element.rootMember());
        }

        internal static MemberSyntax RootMember(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.RootMemberContext member)
        {
            // Check for type declaration
            if (member.typeDeclaration() != null)
                return new TypeSyntax(tree, parent, member.typeDeclaration());

            // Check for contract declaration
            if (member.contractDeclaration() != null)
                return new ContractSyntax(tree, parent, member.contractDeclaration());

            // Check for enum declaration
            if (member.enumDeclaration() != null)
                return new EnumSyntax(tree, parent, member.enumDeclaration());

            // Not valid
            throw new NotSupportedException("Unsupported root member type: " + member);
        }

        internal static MemberSyntax Member(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.MemberDeclarationContext member)
        {
            // Check for type
            if (member.typeDeclaration() != null)
                return new TypeSyntax(tree, parent, member.typeDeclaration());

            // Check for contract
            if(member.contractDeclaration() != null)
                return new ContractSyntax(tree, parent, member.contractDeclaration());

            // Check for enum
            if (member.enumDeclaration() != null)
                return new EnumSyntax(tree, parent, member.enumDeclaration());

            // Check for field
            if (member.fieldDeclaration() != null)
                return new FieldSyntax(tree, parent, member.fieldDeclaration());

            // Check for accessor
            if (member.accessorDeclaration() != null)
                return new AccessorSyntax(tree, parent, member.accessorDeclaration());

            // Check for method
            if(member.methodDeclaration() != null)
                return new MethodSyntax(tree, parent, member.methodDeclaration());

            // Not valid
            throw new NotSupportedException("Unsupported member type: " + member);
        }
    }
}
