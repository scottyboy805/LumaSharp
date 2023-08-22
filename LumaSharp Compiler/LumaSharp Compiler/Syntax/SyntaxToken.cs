using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace LumaSharp_Compiler.Syntax
{
    public sealed class SyntaxToken
    {
        // Private
        private string text = "";
        private SyntaxSource source = null;

        // Properties
        public string Text
        {
            get { return text; }
        }

        public SyntaxSource Source
        {
            get { return source; }
        }

        // Constructor
        internal SyntaxToken(string identifier)
        {
            this.text = identifier;
            this.source = new SyntaxSource();
        }

        internal SyntaxToken(ITerminalNode node)
        {
            this.text = node.Symbol.Text;
            this.source = new SyntaxSource(node.Symbol);
        }

        internal SyntaxToken(IToken node)
        {
            this.text = node.Text;
            this.source = new SyntaxSource(node);
        }

        // Methods
        public override string ToString()
        {
            return text;
        }
    }
}
