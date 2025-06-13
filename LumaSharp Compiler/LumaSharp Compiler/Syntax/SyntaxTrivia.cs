
using LumaSharp.Compiler.AST.Visitor;

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
        // Public
        public const string LineComment = "//";
        public const string BlockCommentStart = "/*";
        public const string BlockCommentEnd = "*/";

        public readonly SyntaxTriviaKind Kind;
        public readonly string Text;
        public readonly SyntaxSpan Span;

        // Properties
        public bool IsWhitespace => IsWhitespaceKind(Kind);
        public bool IsNewline => IsNewlineKind(Kind);

        // Constructor
        internal SyntaxTrivia(SyntaxTriviaKind kind, string text, SyntaxSpan span)
        {
            // Check for null
            if (text == null)
                text = string.Empty;

            this.Kind = kind;
            this.Text = text;
            this.Span = span;
        }

        // Methods
        public void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitTrivia(this);
        }

        public T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitTrivia(this);
        }

        public void GetSourceText(TextWriter writer)
        {
            // Write text
            writer.Write(Text);
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
