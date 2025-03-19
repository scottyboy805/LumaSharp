using Antlr4.Runtime;

namespace LumaSharp.Compiler.AST
{
    public readonly struct SyntaxSource
    {
        // Private
        private readonly string sourceName = "";
        private readonly int line = -1;
        private readonly int column = -1;

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
        internal SyntaxSource(IToken token)
        {
            this.sourceName = token.TokenSource.SourceName;
            this.line = token.Line;
            this.column = token.Column;
        }
    }
}
