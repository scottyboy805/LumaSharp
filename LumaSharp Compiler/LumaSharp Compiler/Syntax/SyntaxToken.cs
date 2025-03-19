using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace LumaSharp.Compiler.AST
{
    public enum SyntaxTokenKind
    {
        Invalid = 0,

        Identifier,
        Literal,
        LiteralDescriptor,
        UnaryOperator,
        BinaryOperator,
        AssignOperator,

        LGenericSymbol,
        RGenericSymbol,
        LArraySymbol,
        RArraySymbol,
        LBlockSymbol,
        RBlockSymbol,
        LParenSymbol,
        RParenSymbol,

        DotSymbol,
        CommaSymbol,
        ColonSymbol,
        HashSymbol,
        LambdaSymbol,
        EnumerableSymbol,
        RangeInclusiveSymbol,
        RangeExclusiveSymbol,
        TernarySymbol,

        VoidKeyword,
        AnyKeyword,
        BoolKeyword,
        CharKeyword,
        I8Keyword,
        U8Keyword,
        I16Keyword,
        U16Keyword,
        I32Keyword,
        U32Keyword,
        I64Keyword,
        U64Keyword,
        F32Keyword,
        F64Keyword,
        StringKeyword,

        ImportKeyword,
        NamespaceKeyword,
        TypeKeyword,
        AttributeKeyword,
        ContractKeyword,
        EnumKeyword,
        GlobalKeyword,
        ExportKeyword,
        InternalKeyword,
        HiddenKeyword,
        AsKeyword,
        ContinueKeyword,
        BreakKeyword,
        ReturnKeyword,
        OverrideKeyword,
        IfKeyword,
        ElseKeyword,
        ElseifKeyword,
        TrueKeyword,
        FalseKeyword,
        NullKeyword,
        InKeyword,
        ForKeyword,
        SelectKeyword,
        MatchKeyword,
        DefaultKeyword,
        TryKeyword,
        CatchKeyword,
        FinallyKeyword,
        SizeKeyword,
        ReadKeyword,
        WriteKeyword,
        ThisKeyword,
        BaseKeyword,
        NewKeyword,
    }

    public readonly struct SyntaxToken
    {
        // Private
        private static readonly Dictionary<SyntaxTokenKind, string> syntaxTokenStrings = new()
        {
            { SyntaxTokenKind.LGenericSymbol, "<" },
            { SyntaxTokenKind.RGenericSymbol, ">" },
            { SyntaxTokenKind.LArraySymbol, "[" },
            { SyntaxTokenKind.RArraySymbol, "]" },
            { SyntaxTokenKind.LBlockSymbol, "{" },
            { SyntaxTokenKind.RBlockSymbol, "}" },
            { SyntaxTokenKind.LParenSymbol, "(" },
            { SyntaxTokenKind.RParenSymbol, ")" },

            { SyntaxTokenKind.DotSymbol, "." },
            { SyntaxTokenKind.CommaSymbol, "," },
            { SyntaxTokenKind.ColonSymbol, ":" },
            { SyntaxTokenKind.HashSymbol, "#" },
            { SyntaxTokenKind.LambdaSymbol, "=>" },
            { SyntaxTokenKind.EnumerableSymbol, "..." },
            { SyntaxTokenKind.RangeInclusiveSymbol, "..=" },
            { SyntaxTokenKind.RangeExclusiveSymbol, "..<" },
            { SyntaxTokenKind.TernarySymbol, "?" },

            { SyntaxTokenKind.VoidKeyword, "void" },
            { SyntaxTokenKind.AnyKeyword, "any" },
            { SyntaxTokenKind.BoolKeyword, "bool" },
            { SyntaxTokenKind.CharKeyword, "char" },
            { SyntaxTokenKind.I8Keyword, "i8" },
            { SyntaxTokenKind.U8Keyword, "u8" },
            { SyntaxTokenKind.I16Keyword, "i16" },
            { SyntaxTokenKind.U16Keyword, "u16" },
            { SyntaxTokenKind.I32Keyword, "i32" },
            { SyntaxTokenKind.U32Keyword, "u32" },
            { SyntaxTokenKind.I64Keyword, "i64" },
            { SyntaxTokenKind.U64Keyword, "u64" },
            { SyntaxTokenKind.F32Keyword, "f32" },
            { SyntaxTokenKind.F64Keyword, "f64" },
            { SyntaxTokenKind.StringKeyword, "string" },

            { SyntaxTokenKind.ImportKeyword, "import" },
            { SyntaxTokenKind.NamespaceKeyword, "namespace" },
            { SyntaxTokenKind.TypeKeyword, "type" },
            { SyntaxTokenKind.AttributeKeyword, "attribute" },
            { SyntaxTokenKind.ContractKeyword, "contract" },
            { SyntaxTokenKind.EnumKeyword, "enum" },
            { SyntaxTokenKind.GlobalKeyword, "global" },
            { SyntaxTokenKind.ExportKeyword, "export" },
            { SyntaxTokenKind.InternalKeyword, "internal" },
            { SyntaxTokenKind.HiddenKeyword, "hidden" },
            { SyntaxTokenKind.AsKeyword, "as" },
            { SyntaxTokenKind.ContinueKeyword, "continue" },
            { SyntaxTokenKind.BreakKeyword, "break" },
            { SyntaxTokenKind.ReturnKeyword, "return" },
            { SyntaxTokenKind.OverrideKeyword, "override" },
            { SyntaxTokenKind.IfKeyword, "if" },
            { SyntaxTokenKind.ElseKeyword, "else" },
            { SyntaxTokenKind.ElseifKeyword, "elseif" },
            { SyntaxTokenKind.TrueKeyword, "true" },
            { SyntaxTokenKind.FalseKeyword, "false" },
            { SyntaxTokenKind.NullKeyword, "null" },
            { SyntaxTokenKind.InKeyword, "in" },
            { SyntaxTokenKind.ForKeyword, "for" },
            { SyntaxTokenKind.SelectKeyword, "select" },
            { SyntaxTokenKind.MatchKeyword, "match" },
            { SyntaxTokenKind.DefaultKeyword, "default" },
            { SyntaxTokenKind.TryKeyword, "try" },
            { SyntaxTokenKind.CatchKeyword, "catch" },
            { SyntaxTokenKind.FinallyKeyword, "finally" },
            { SyntaxTokenKind.SizeKeyword, "size" },
            { SyntaxTokenKind.ReadKeyword, "read" },
            { SyntaxTokenKind.WriteKeyword, "write" },
            { SyntaxTokenKind.ThisKeyword, "this" },
            { SyntaxTokenKind.BaseKeyword, "base" },
            { SyntaxTokenKind.NewKeyword, "new" },
        };

        private readonly SyntaxTokenKind kind;
        private readonly string text;
        private readonly SyntaxSource source;

        // Public
        public static readonly SyntaxToken Invalid = new SyntaxToken(SyntaxTokenKind.Invalid, (string)null);

        // Properties
        public SyntaxTokenKind Kind
        {
            get { return kind; }
        }

        public string Text
        {
            get { return text; }
        }

        public SyntaxSource Source
        {
            get { return source; }
        }

        // Constructor
        internal SyntaxToken(SyntaxTokenKind kind, SyntaxSource source = default)
        {
            // Get text
            string text;
            if (syntaxTokenStrings.TryGetValue(kind, out text) == false)
                throw new ArgumentException(string.Format("Syntax kind '{0}' requires text to be specified", kind));

            this.kind = kind;
            this.text = text;
            this.source = source;
        }

        internal SyntaxToken(SyntaxTokenKind kind, string text, SyntaxSource source = default)
        {
            // Ensure text is correct
            string expected;
            if (syntaxTokenStrings.TryGetValue(kind, out expected) == true && expected != text)
                throw new ArgumentException(string.Format("Expected text '{0}' for syntax kind: '{1}'", expected, kind));

            this.kind = kind;
            this.text = text;
            this.source = source;
        }

        internal SyntaxToken(SyntaxTokenKind kind, ITerminalNode node)
            : this(kind, node != null ? node.Symbol : null)
        {
        }

        internal SyntaxToken(SyntaxTokenKind kind, IToken node)
        {
            this.kind = kind;
            this.text = node.Text;
            this.source = new SyntaxSource(node);
        }

        // Methods
        public void GetSourceText(TextWriter writer)
        {
            // Write symbol
            writer.Write(text);
        }

        public override string ToString()
        {
            return text;
        }
    }
}
