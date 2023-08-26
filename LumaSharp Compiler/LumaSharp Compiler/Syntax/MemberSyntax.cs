
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
        protected MemberSyntax(string identifier, SyntaxTree tree, SyntaxNode parent)
            : base(tree, parent)
        {
            this.identifier = new SyntaxToken(identifier);
        }

        internal MemberSyntax(ITerminalNode identifier, SyntaxTree tree, SyntaxNode parent, LumaSharpParser.AttributeDeclarationContext[] attributes, LumaSharpParser.AccessModifierContext[] modifiers)
            : base(tree, parent)
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
    }
}
