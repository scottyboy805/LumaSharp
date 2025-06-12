
namespace LumaSharp.Compiler.AST
{
    public enum SyntaxTokenKind
    {
        Invalid = 0,
        EOF = 1,

        Identifier,
        Literal,
        LiteralDescriptor,

        AssignSymbol,
        AssignInferSymbol,
        AssignPlusSymbol,
        AssignMinusSymbol,
        AssignMultiplySymbol,
        AssignDivideSymbol,

        OrSymbol,
        AndSymbol,
        BitwiseOrSymbol,
        BitwiseXOrSymbol,
        BitwiseAndSymbol,
        EqualitySymbol,
        NonEqualitySymbol,
        LessSymbol,
        LessEqualSymbol,
        GreaterSymbol,
        GreaterEqualSymbol,
        BitShiftLeftSymbol,
        BitShiftRightSymbol,
        AddSymbol,
        SubtractSymbol,
        MultiplySymbol,
        DivideSymbol,
        ModulusSymbol,
        PlusPlusSymbol,
        MinusMinusSymbol,
        NotSymbol,

        LArraySymbol,
        RArraySymbol,
        LBlockSymbol,
        RBlockSymbol,
        LParenSymbol,
        RParenSymbol,

        DotSymbol,
        CommaSymbol,
        ColonSymbol,
        SemicolonSymbol,
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
        ElifKeyword,
        ElseKeyword,
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
        TypeofKeyword,
        SizeofKeyword,
        ReadKeyword,
        WriteKeyword,
        ThisKeyword,
        BaseKeyword,
        NewKeyword,
    }

    public readonly struct SyntaxToken
    {
        // Private
        private static readonly SyntaxTokenKind StartSymbol = Enum.GetValues<SyntaxTokenKind>()
                .First(e => e.ToString().Contains("Symbol"));
        private static readonly SyntaxTokenKind EndSymbol = Enum.GetValues<SyntaxTokenKind>()
                .Last(e => e.ToString().Contains("Symbol"));
        private static readonly SyntaxTokenKind StartKeyword = Enum.GetValues<SyntaxTokenKind>()
                .First(e => e.ToString().Contains("Keyword"));
        private static readonly SyntaxTokenKind EndKeyword = Enum.GetValues<SyntaxTokenKind>()
                .Last(e => e.ToString().Contains("Keyword"));

        private static readonly Dictionary<SyntaxTokenKind, string> syntaxTokenStrings = new()
        {
            {SyntaxTokenKind.Invalid, "" },
            { SyntaxTokenKind.EOF, "\0" },

            { SyntaxTokenKind.AssignSymbol, "=" },
            { SyntaxTokenKind.AssignInferSymbol, ":="  },
            { SyntaxTokenKind.AssignPlusSymbol, "+=" },
            { SyntaxTokenKind.AssignMinusSymbol, "-=" },
            { SyntaxTokenKind.AssignMultiplySymbol, "*=" },
            { SyntaxTokenKind.AssignDivideSymbol, "/=" },

            { SyntaxTokenKind.OrSymbol, "||" },
            { SyntaxTokenKind.AndSymbol, "&&" },
            { SyntaxTokenKind.BitwiseOrSymbol, "|" },
            { SyntaxTokenKind.BitwiseXOrSymbol, "^" },
            { SyntaxTokenKind.BitwiseAndSymbol, "&" },
            { SyntaxTokenKind.EqualitySymbol, "==" },
            { SyntaxTokenKind.NonEqualitySymbol, "!=" },
            { SyntaxTokenKind.LessSymbol, "<" },              // Handled below by LGeneric - don't include duplicates
            { SyntaxTokenKind.GreaterSymbol, ">" },           // Handled below by RGeneric - don't include duplicates
            { SyntaxTokenKind.LessEqualSymbol, "<=" },
            { SyntaxTokenKind.GreaterEqualSymbol, ">=" },
            { SyntaxTokenKind.BitShiftLeftSymbol, "<<" },
            { SyntaxTokenKind.BitShiftRightSymbol, ">>" },
            { SyntaxTokenKind.AddSymbol, "+" },
            { SyntaxTokenKind.SubtractSymbol, "-" },
            { SyntaxTokenKind.MultiplySymbol, "*" },
            { SyntaxTokenKind.DivideSymbol, "/" },
            { SyntaxTokenKind.ModulusSymbol, "%" },
            { SyntaxTokenKind.PlusPlusSymbol, "++" },
            { SyntaxTokenKind.MinusMinusSymbol, "--" },
            { SyntaxTokenKind.NotSymbol, "!" },

            { SyntaxTokenKind.LArraySymbol, "[" },
            { SyntaxTokenKind.RArraySymbol, "]" },
            { SyntaxTokenKind.LBlockSymbol, "{" },
            { SyntaxTokenKind.RBlockSymbol, "}" },
            { SyntaxTokenKind.LParenSymbol, "(" },
            { SyntaxTokenKind.RParenSymbol, ")" },

            { SyntaxTokenKind.DotSymbol, "." },
            { SyntaxTokenKind.CommaSymbol, "," },
            { SyntaxTokenKind.ColonSymbol, ":" },
            { SyntaxTokenKind.SemicolonSymbol, ";" },
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
            { SyntaxTokenKind.ElifKeyword, "elif" },
            { SyntaxTokenKind.ElseKeyword, "else" },
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
            { SyntaxTokenKind.TypeofKeyword, "typeof" },
            { SyntaxTokenKind.SizeofKeyword, "sizeof" },
            { SyntaxTokenKind.ReadKeyword, "read" },
            { SyntaxTokenKind.WriteKeyword, "write" },
            { SyntaxTokenKind.ThisKeyword, "this" },
            { SyntaxTokenKind.BaseKeyword, "base" },
            { SyntaxTokenKind.NewKeyword, "new" },
        };

        // Private
        private readonly List<SyntaxTrivia> trivia;

        // Public
        public static readonly SyntaxToken Invalid = Syntax.Token(SyntaxTokenKind.Invalid);

        public readonly SyntaxTokenKind Kind;
        public readonly string Text;
        public readonly SyntaxSpan Span;

        // Properties
        public bool IsKeyword => IsKeywordKind(Kind);
        public bool IsSymbol => IsSymbolKind(Kind);
        public bool IsLiteral => IsLiteralKind(Kind);
        public bool IsPrimitiveType => IsPrimitiveTypeKind(Kind);
        public bool IsAccessModifier => IsAccessModifierKind(Kind);
        public bool IsRange => IsRangeKind(Kind);
        public bool IsBinaryOperand => IsBinaryOperandKind(Kind);
        public bool IsUnaryOperand => IsUnaryOperandKind(Kind);
        public bool IsAssign => IsAssignKind(Kind);

        public IEnumerable<SyntaxTrivia> Trivia
        {
            get
            {
                // Check for none
                if (trivia == null)
                    yield break;

                // Enumerate all
                foreach(SyntaxTrivia t in trivia)
                    yield return t;
            }
        }

        public IEnumerable<SyntaxTrivia> LeadingTrivia => Trivia.Where(t => t.IsLeading);
        public IEnumerable<SyntaxTrivia> TrailingTrivia => Trivia.Where(t => t.IsTrailing);

        // Constructor
        private SyntaxToken(SyntaxTokenKind kind, string text, SyntaxSpan source, IEnumerable<SyntaxTrivia> trivia)
        {
            this.Kind = kind;
            this.Text = text;
            this.Span = source;
            this.trivia = trivia.Any() ? trivia.ToList() : null;
        }

        internal SyntaxToken(SyntaxTokenKind kind, SyntaxSpan span)
        {
            // Get text
            string text;
            if (syntaxTokenStrings.TryGetValue(kind, out text) == false)
                throw new ArgumentException(string.Format("Syntax kind '{0}' requires text to be specified", kind));

            this.Kind = kind;
            this.Text = text;
            this.Span = span;
        }

        internal SyntaxToken(SyntaxTokenKind kind, string text, SyntaxSpan span)
        {
            // Ensure text is correct
            string expected;
            if (syntaxTokenStrings.TryGetValue(kind, out expected) == true && expected != text)
                throw new ArgumentException(string.Format("Expected text '{0}' for syntax kind: '{1}'", expected, kind));

            this.Kind = kind;
            this.Text = text;
            this.Span = span;
        }

        // Methods
        public SyntaxToken WithTrivia(IEnumerable<SyntaxTrivia> trivia)
        {
            return new SyntaxToken(Kind, Text, Span, trivia);
        }

        public void GetSourceText(TextWriter writer)
        {
            // Write leading trivia
            foreach(SyntaxTrivia t in LeadingTrivia)
                t.GetSourceText(writer);

            // Write symbol
            writer.Write(Text);

            // Write trailing trivia
            foreach(SyntaxTrivia t in TrailingTrivia)
                t.GetSourceText(writer);
        }

        public string GetSourceText()
        {
            // Create the writer
            using (StringWriter writer = new StringWriter())
            {
                // Write the text
                GetSourceText(writer);

                // Get full string
                return writer.ToString();
            }
        }

        public override string ToString()
        {
            // Check for no trivia
            if(trivia == null || trivia.Count == 0)
                return Text;

            // Get the source text with trivia
            return GetSourceText();
        }

        public static string GetText(SyntaxTokenKind kind)
        {
            string val;
            syntaxTokenStrings.TryGetValue(kind, out val);
            return val;
        }

        public static IEnumerable<SyntaxTokenKind> GetSymbols()
        {
            return Enum.GetValues<SyntaxTokenKind>().Where(IsSymbolKind);
        }

        public static IEnumerable<SyntaxTokenKind> GetKeywords()
        {
            return Enum.GetValues<SyntaxTokenKind>().Where(IsKeywordKind);
        }

        public static bool IsSymbolKind(SyntaxTokenKind kind)
        {
            return kind >= StartSymbol && kind <= EndSymbol;
        }

        public static bool IsKeywordKind(SyntaxTokenKind kind)
        {
            return kind >= StartKeyword && kind <= EndKeyword;
        }

        public static bool IsLiteralKind(SyntaxTokenKind kind)
        {
            switch (kind)
            {
                case SyntaxTokenKind.Literal:
                case SyntaxTokenKind.TrueKeyword:
                case SyntaxTokenKind.FalseKeyword:
                case SyntaxTokenKind.NullKeyword:
                    return true;
            }
            return false;
        }

        public static bool IsPrimitiveTypeKind(SyntaxTokenKind kind)
        {
            switch(kind)
            {
                case SyntaxTokenKind.AnyKeyword:
                case SyntaxTokenKind.BoolKeyword:
                case SyntaxTokenKind.I8Keyword:
                case SyntaxTokenKind.U8Keyword:
                case SyntaxTokenKind.I16Keyword:
                case SyntaxTokenKind.U16Keyword:
                case SyntaxTokenKind.I32Keyword:
                case SyntaxTokenKind.U32Keyword:
                case SyntaxTokenKind.I64Keyword:
                case SyntaxTokenKind.U64Keyword:
                case SyntaxTokenKind.F32Keyword:
                case SyntaxTokenKind.F64Keyword:
                case SyntaxTokenKind.StringKeyword:
                case SyntaxTokenKind.VoidKeyword:
                    return true;
            }
            return false;
        }

        public static bool IsAccessModifierKind(SyntaxTokenKind kind)
        {
            switch(kind)
            {
                case SyntaxTokenKind.ExportKeyword:
                case SyntaxTokenKind.GlobalKeyword:
                case SyntaxTokenKind.HiddenKeyword:
                case SyntaxTokenKind.InternalKeyword:
                    return true;
            }
            return false;
        }

        public static bool IsRangeKind(SyntaxTokenKind kind)
        {
            switch (kind)
            {
                case SyntaxTokenKind.RangeInclusiveSymbol:
                case SyntaxTokenKind.RangeExclusiveSymbol:
                    return true;
            }
            return false;
        }

        public static bool IsBinaryOperandKind(SyntaxTokenKind kind)
        {
            switch(kind)
            {
                case SyntaxTokenKind.OrSymbol:
                case SyntaxTokenKind.AndSymbol:
                case SyntaxTokenKind.BitwiseOrSymbol:
                case SyntaxTokenKind.BitwiseXOrSymbol:
                case SyntaxTokenKind.BitwiseAndSymbol:
                case SyntaxTokenKind.EqualitySymbol:
                case SyntaxTokenKind.NonEqualitySymbol:
                case SyntaxTokenKind.LessSymbol:
                case SyntaxTokenKind.LessEqualSymbol:
                case SyntaxTokenKind.GreaterSymbol:
                case SyntaxTokenKind.GreaterEqualSymbol:
                case SyntaxTokenKind.BitShiftLeftSymbol:
                case SyntaxTokenKind.BitShiftRightSymbol:
                case SyntaxTokenKind.AddSymbol:
                case SyntaxTokenKind.SubtractSymbol:
                case SyntaxTokenKind.MultiplySymbol:
                case SyntaxTokenKind.DivideSymbol:
                case SyntaxTokenKind.ModulusSymbol:
                    return true;
            }
            return false;
        }

        public static bool IsUnaryOperandKind(SyntaxTokenKind kind)
        {
            switch(kind)
            {
                case SyntaxTokenKind.PlusPlusSymbol:
                case SyntaxTokenKind.MinusMinusSymbol:
                case SyntaxTokenKind.NotSymbol:
                    return true;
            }
            return false;
        }

        public static bool IsAssignKind(SyntaxTokenKind kind)
        {
            switch(kind)
            {
                case SyntaxTokenKind.AssignSymbol:
                case SyntaxTokenKind.AssignInferSymbol:
                case SyntaxTokenKind.AssignPlusSymbol:
                case SyntaxTokenKind.AssignMinusSymbol:
                case SyntaxTokenKind.AssignMultiplySymbol:
                case SyntaxTokenKind.AssignDivideSymbol:
                    return true;
            }
            return false;
        }

        // Support implicit conversion for identifiers only
        public static implicit operator SyntaxToken(string identifier)
        {
            return Syntax.Identifier(identifier);
        }
    }
}
