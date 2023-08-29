
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Diagnostics;

namespace LumaSharp_Compiler.Syntax
{
    public abstract class MemberSyntax : SyntaxNode
    {
        // Protected
        protected AttributeSyntax[] attributes = null;
        protected SyntaxToken[] accessModifiers = null;
        protected SyntaxToken identifier = null;

        // Properties
        public AttributeSyntax[] Attributes
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
        //protected MemberSyntax(string identifier, SyntaxTree tree, SyntaxNode parent)
        //    : base(tree, parent)
        //{
        //    this.identifier = new SyntaxToken(identifier);
        //}

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
            if (accessModifiers != null && modifiers.Length > 0)
            {
                this.accessModifiers = modifiers.Select(m => new SyntaxToken(m.Start)).ToArray();
            }
        }

        // Methods
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
