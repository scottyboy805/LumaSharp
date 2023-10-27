using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace LumaSharp_Compiler.AST
{
    public sealed class SyntaxToken
    {
        // Private
        private string text = "";
        private SyntaxSource source = null;

        private string leadingWhitespace = "";
        private string trailingWhitespace = "";

        // Properties
        public string Text
        {
            get { return text; }
        }

        public SyntaxSource Source
        {
            get { return source; }
        }

        public string LeadingWhitespace
        {
            get { return leadingWhitespace; }
        }

        public string TrailingWhitespace
        {
            get { return trailingWhitespace; }
        }

        public bool HasLeadingWhitespace
        {
            get { return leadingWhitespace != null; }
        }

        public bool HasTrailingWhitespace
        {
            get { return trailingWhitespace != null; }
        }

        // Constructor
        internal SyntaxToken(string identifier)
        {
            this.text = identifier;
            this.source = new SyntaxSource();
        }

        internal SyntaxToken(ITerminalNode node)
            : this(node.Symbol)
        {
        }

        internal SyntaxToken(IToken node)
        {
            this.text = node.Text;
            this.source = new SyntaxSource(node);

            // Get the whitespace
            GetWhitespace(node.TokenIndex);
        }

        // Methods
        public void GetSourceText(TextWriter writer)
        {
            // Write leading whitespace
            if(HasLeadingWhitespace == true)
                writer.Write(leadingWhitespace);

            // Write symbol
            writer.Write(text);

            // Write trailing whitespace
            if(HasTrailingWhitespace == true)
                writer.Write(trailingWhitespace);
        }

        public override string ToString()
        {
            return text;
        }

        private void GetWhitespace(int tokenIndex)
        {
            // Get whitespace
            if (ParserContext.Current != null)
            {
                // Get leading
                foreach (IToken token in ParserContext.Current.GetLeadingHiddenTokens(tokenIndex))
                    leadingWhitespace += token.Text;

                // Get trailing
                foreach(IToken token in ParserContext.Current.GetTrailingHiddenTokens(tokenIndex))
                    trailingWhitespace += token.Text;
            }
        }

        public SyntaxToken WithLeadingWhitespace(string whitespace)
        {
            leadingWhitespace = whitespace;
            return this;
        }

        public SyntaxToken WithTrailingWhitespace(string whitespace)
        {
            trailingWhitespace = whitespace;
            return this;
        }


        // Internal
        internal static SyntaxToken Import() => new SyntaxToken("import");
        internal static SyntaxToken Namespace() => new SyntaxToken("namespace");
        internal static SyntaxToken As() => new SyntaxToken("as");
        internal static SyntaxToken Type() => new SyntaxToken("type");
        internal static SyntaxToken Contract() => new SyntaxToken("contract");
        internal static SyntaxToken Enum() => new SyntaxToken("enum");
        internal static SyntaxToken Read() => new SyntaxToken("read");
        internal static SyntaxToken Write() => new SyntaxToken("write");
        internal static SyntaxToken Size() => new SyntaxToken("size");
        internal static SyntaxToken This() => new SyntaxToken("this");
        internal static SyntaxToken Base() => new SyntaxToken("base");
        internal static SyntaxToken New() => new SyntaxToken("new");
        internal static SyntaxToken If() => new SyntaxToken("if");
        internal static SyntaxToken Elif() => new SyntaxToken("elif");
        internal static SyntaxToken Else() => new SyntaxToken("else");
        internal static SyntaxToken Foreach() => new SyntaxToken("foreach");
        internal static SyntaxToken In() => new SyntaxToken("in");
        internal static SyntaxToken For() => new SyntaxToken("for");
        internal static SyntaxToken Return() => new SyntaxToken("return");

        internal static SyntaxToken LParen() => new SyntaxToken("(");
        internal static SyntaxToken RParen() => new SyntaxToken(")");
        internal static SyntaxToken LBlock() => new SyntaxToken("{");
        internal static SyntaxToken RBlock() => new SyntaxToken("}");
        internal static SyntaxToken LArray() => new SyntaxToken("[");
        internal static SyntaxToken RArray() => new SyntaxToken("]");
        internal static SyntaxToken LGeneric() => new SyntaxToken("<");
        internal static SyntaxToken RGeneric() => new SyntaxToken(">");
        internal static SyntaxToken Dot() => new SyntaxToken(".");
        internal static SyntaxToken Comma() => new SyntaxToken(",");
        internal static SyntaxToken Colon() => new SyntaxToken(":");
        internal static SyntaxToken Semi() => new SyntaxToken(";");
        internal static SyntaxToken Hash() => new SyntaxToken("#");
        internal static SyntaxToken Reference() => new SyntaxToken("&");

        internal static SyntaxToken Lambda() => new SyntaxToken("=>");
        internal static SyntaxToken Assign() => new SyntaxToken("=");
    }
}
