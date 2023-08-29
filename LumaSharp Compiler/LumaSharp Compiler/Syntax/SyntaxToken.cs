using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace LumaSharp_Compiler.Syntax
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
    }
}
