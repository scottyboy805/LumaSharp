
namespace LumaSharp.Compiler.AST
{
    public enum SyntaxTriviaKind
    {
        Invalid = 0,

        Whitespace,
        Newline,
        LineComment,
        BlockComment,
    }

    public readonly struct SyntaxTrivia
    {
        // Private
        private readonly int position;

        // Public
        public const string LineComment = "//";
        public const string BlockCommentStart = "/*";
        public const string BlockCommentEnd = "*/";

        public readonly SyntaxTriviaKind Kind;
        public readonly string Text;
        public readonly SyntaxSpan Span;

        // Properties
        public bool IsLeading => position == -1;
        public bool IsTrailing => position == 1;
        public bool IsWhitespace => IsWhitespaceKind(Kind);
        public bool IsNewline => IsNewlineKind(Kind);

        // Constructor
        private SyntaxTrivia(SyntaxTriviaKind kind, string text, SyntaxSpan span, int position)
        {
            // Check for null
            if (text == null)
                text = string.Empty;

            // Check position
            if (position != -1 && position != 1)
                throw new ArgumentException(nameof(position) + " must be '-1' or '1'");

            this.Kind = kind;
            this.Text = text;
            this.Span = span;
            this.position = position;
        }

        // Methods
        public void GetSourceText(TextWriter writer)
        {
            // Write text
            writer.Write(Text);
        }

        internal static SyntaxTrivia Leading(SyntaxTriviaKind kind, string text, SyntaxSpan source)
        {
            return new SyntaxTrivia(kind, text, source, -1);
        }

        internal static SyntaxTrivia Trailing(SyntaxTriviaKind kind, string text, SyntaxSpan source)
        {
            return new SyntaxTrivia(kind, text, source, 1);
        }

        public static bool IsWhitespaceKind(SyntaxTriviaKind kind)
        {
            return kind == SyntaxTriviaKind.Whitespace;
        }

        public static bool IsNewlineKind(SyntaxTriviaKind kind) 
        {
            return kind == SyntaxTriviaKind.Newline;
        }
    }
}
