
using Antlr4.Runtime.Tree;

namespace LumaSharp_Compiler.Syntax
{
    public abstract class MemberSyntax : SyntaxNode
    {
        // Protected
        protected SyntaxToken[] accessModifiers = null;
        protected SyntaxToken identifier = null;

        // Properties
        public SyntaxToken[] AccessModifiers
        {
            get { return accessModifiers; }
        }

        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public int AccessModifierCount
        {
            get { return HasAccessModifiers ? accessModifiers.Length : 0; }
        }

        public bool HasAccessModifiers
        {
            get { return accessModifiers != null; }
        }

        // Constructor
        protected MemberSyntax(string identifier)
        {
            this.identifier = new SyntaxToken(identifier);
        }

        internal MemberSyntax(ITerminalNode identifier, LumaSharpParser.AccessModifierContext[] modifiers)
        {
            this.identifier = new SyntaxToken(identifier);

            if (modifiers.Length > 0)
            {
                this.accessModifiers = new SyntaxToken[modifiers.Length];

                for (int i = 0; i < modifiers.Length; i++)
                {
                    this.accessModifiers[i] = new SyntaxToken(modifiers[i].Start);
                }
            }
        }
    }
}
