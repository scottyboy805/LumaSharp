
namespace LumaSharp.Compiler.AST
{
    public readonly struct SyntaxLocation
    {
        // Public
        public readonly int Position;
        public readonly int Line;
        public readonly int Column;

        // Constructor
        public SyntaxLocation(int position, int line, int column)
        {
            this.Position = position;
            this.Line = line;
            this.Column = column;
        }

        // Methods
        public override string ToString()
        {
            return $"(Position = {Position}, Line = {Line}, Column = {Column})";
        }
    }

    public readonly struct SyntaxSpan
    {
        // Public
        public readonly string Document;
        public readonly SyntaxLocation Start;
        public readonly SyntaxLocation End;

        // Private
        private readonly string sourceName = "";
        private readonly int line = -1;
        private readonly int column = -1;

        // Properties
        public int LineSpan => (End.Line - Start.Line) + 1;
        public int CharacterSpan => (End.Position - Start.Position) + 1;

        // Constructor
        public SyntaxSpan(string document, SyntaxLocation start, SyntaxLocation end)
        {
            // Check for null
            if (document == null)
                document = string.Empty;

            // Check for negative span
            if (start.Position > end.Position || start.Line > end.Line)
                throw new ArgumentException("Start location cannot appear later than end position");

            this.Document = document;
            this.Start = start;
            this.End = end;
        }

        // Methods
        public override string ToString()
        {
            return $"(Document = {Document}, Start = {Start}, End = {End})";
        }
    }
}
