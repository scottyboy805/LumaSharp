using Antlr4.Runtime;

namespace LumaSharp_Compiler.Syntax
{
    public sealed class SyntaxSource
    {
        // Private
        private string sourceName = "";
        private int line = -1;
        private int column = -1;

        // Properties
        public string SourceName
        {
            get { return sourceName; }
        }

        public int Line
        {
            get { return line; }
        }

        public int Column
        {
            get { return column; }
        }

        // Constructor
        internal SyntaxSource() { }

        internal SyntaxSource(IToken token)
        {
            this.sourceName = token.TokenSource.SourceName;
            this.line = token.Line;
            this.column = token.Column;
        }
    }
}
