
using LumaSharp.Compiler.AST.Visitor;

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
        ObjectKeyword,
        BoolKeyword,
        CharKeyword,
        Int8Keyword,
        UInt8Keyword,
        Int16Keyword,
        UInt16Keyword,
        Int32Keyword,
        UInt32Keyword,
        Int64Keyword,
        UInt64Keyword,
        Intkeyword,
        UIntKeyword,
        FloatKeyword,
        DoubleKeyword,
        StringKeyword,

        UsingKeyword,
        NamespaceKeyword,
        ClassKeyword,
        StructKeyword,
        InterfaceKeyword,
        EnumKeyword,
        StaticKeyword,
        PublicKeyword,
        InternalKeyword,
        ProtectedKeyword,
        PrivateKeyword,
        ConstKeyword,
        ReadonlyKeyword,
        MutKeyword,
        AsKeyword,
        ContinueKeyword,
        BreakKeyword,
        ReturnKeyword,
        VirtualKeyword,
        OverrideKeyword,
        IfKeyword,
        ElseKeyword,
        TrueKeyword,
        FalseKeyword,
        NullKeyword,
        InKeyword,
        ForKeyword,
        WhileKeyword,
        SelectKeyword,
        MatchKeyword,
        DefaultKeyword,
        TryKeyword,
        CatchKeyword,
        FinallyKeyword,
        TypeofKeyword,
        SizeofKeyword,
        GetKeyword,
        SetKeyword,        
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
            { SyntaxTokenKind.ObjectKeyword, "object" },
            { SyntaxTokenKind.BoolKeyword, "bool" },
            { SyntaxTokenKind.CharKeyword, "char" },
            { SyntaxTokenKind.Int8Keyword, "int8" },
            { SyntaxTokenKind.UInt8Keyword, "uint8" },
            { SyntaxTokenKind.Int16Keyword, "int16" },
            { SyntaxTokenKind.UInt16Keyword, "uint16" },
            { SyntaxTokenKind.Int32Keyword, "int32" },
            { SyntaxTokenKind.UInt32Keyword, "uint32" },
            { SyntaxTokenKind.Int64Keyword, "int64" },
            { SyntaxTokenKind.UInt64Keyword, "uint64" },
            { SyntaxTokenKind.Intkeyword, "int" },
            { SyntaxTokenKind.UIntKeyword, "uint" },
            { SyntaxTokenKind.FloatKeyword, "float" },
            { SyntaxTokenKind.DoubleKeyword, "double" },
            { SyntaxTokenKind.StringKeyword, "string" },

            { SyntaxTokenKind.UsingKeyword, "using" },
            { SyntaxTokenKind.NamespaceKeyword, "namespace" },
            { SyntaxTokenKind.ClassKeyword, "class" },
            { SyntaxTokenKind.StructKeyword, "struct" },
            { SyntaxTokenKind.InterfaceKeyword, "interface" },
            { SyntaxTokenKind.EnumKeyword, "enum" },
            { SyntaxTokenKind.StaticKeyword, "static" },
            { SyntaxTokenKind.PublicKeyword, "public" },
            { SyntaxTokenKind.InternalKeyword, "internal" },
            { SyntaxTokenKind.ProtectedKeyword, "protected" },
            { SyntaxTokenKind.PrivateKeyword, "private" },
            { SyntaxTokenKind.AsKeyword, "as" },
            { SyntaxTokenKind.ContinueKeyword, "continue" },
            { SyntaxTokenKind.BreakKeyword, "break" },
            { SyntaxTokenKind.ReturnKeyword, "return" },
            { SyntaxTokenKind.VirtualKeyword, "virtual" },
            { SyntaxTokenKind.OverrideKeyword, "override" },
            { SyntaxTokenKind.ConstKeyword, "const" },
            { SyntaxTokenKind.ReadonlyKeyword, "readonly" },
            { SyntaxTokenKind.MutKeyword, "mut" },
            { SyntaxTokenKind.IfKeyword, "if" },
            { SyntaxTokenKind.ElseKeyword, "else" },
            { SyntaxTokenKind.TrueKeyword, "true" },
            { SyntaxTokenKind.FalseKeyword, "false" },
            { SyntaxTokenKind.NullKeyword, "null" },
            { SyntaxTokenKind.InKeyword, "in" },
            { SyntaxTokenKind.ForKeyword, "for" },
            { SyntaxTokenKind.WhileKeyword, "while" },
            { SyntaxTokenKind.SelectKeyword, "select" },
            { SyntaxTokenKind.MatchKeyword, "match" },
            { SyntaxTokenKind.DefaultKeyword, "default" },
            { SyntaxTokenKind.TryKeyword, "try" },
            { SyntaxTokenKind.CatchKeyword, "catch" },
            { SyntaxTokenKind.FinallyKeyword, "finally" },
            { SyntaxTokenKind.TypeofKeyword, "typeof" },
            { SyntaxTokenKind.SizeofKeyword, "sizeof" },
            { SyntaxTokenKind.GetKeyword, "get" },
            { SyntaxTokenKind.SetKeyword, "set" },            
            { SyntaxTokenKind.ThisKeyword, "this" },
            { SyntaxTokenKind.BaseKeyword, "base" },
            { SyntaxTokenKind.NewKeyword, "new" },
        };

        // Private
        private readonly SyntaxTrivia[] leadingTrivia;
        private readonly SyntaxTrivia[] trailingTrivia;

        // Public
        public static readonly SyntaxToken Invalid = Syntax.Token(SyntaxTokenKind.Invalid);

        public readonly SyntaxTokenKind Kind;
        public readonly string Text;
        public readonly SyntaxSpan Span;

        // Properties
        public bool IsKeyword => IsKeywordKind(Kind);
        public bool IsSymbol => IsSymbolKind(Kind);
        public bool IsLiteral => IsLiteralKind(Kind);
        public bool IsReservedType => IsReservedTypeKind(Kind);
        public bool IsModifier => IsModifierKind(Kind);
        public bool IsRange => IsRangeKind(Kind);
        public bool IsBinaryOperand => IsBinaryOperandKind(Kind);
        public bool IsUnaryOperand => IsUnaryOperandKind(Kind);
        public bool IsAssign => IsAssignKind(Kind);

        public IEnumerable<SyntaxTrivia> Trivia
        {
            get
            {
                // Get leading
                if(leadingTrivia != null)
                {
                    foreach (SyntaxTrivia trivia in leadingTrivia)
                        yield return trivia;
                }

                // Get trailing
                if(trailingTrivia != null)
                {
                    foreach(SyntaxTrivia trivia in trailingTrivia)
                        yield return trivia;
                }
            }
        }

        public IEnumerable<SyntaxTrivia> LeadingTrivia => leadingTrivia != null ? leadingTrivia : Enumerable.Empty<SyntaxTrivia>();
        public IEnumerable<SyntaxTrivia> TrailingTrivia => trailingTrivia != null ? trailingTrivia : Enumerable.Empty<SyntaxTrivia>();

        // Constructor
        private SyntaxToken(SyntaxTokenKind kind, string text, SyntaxSpan source, SyntaxTrivia[] leadingTrivia, SyntaxTrivia[] trailingTrivia)
        {
            this.Kind = kind;
            this.Text = text;
            this.Span = source;
            this.leadingTrivia = leadingTrivia;
            this.trailingTrivia = trailingTrivia;
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
        public SyntaxToken WithLeadingTrivia(SyntaxTrivia trivia)
        {
            return new SyntaxToken(Kind, Text, Span, new[] { trivia }, trailingTrivia);
        }

        public SyntaxToken WithLeadingTrivia(IEnumerable<SyntaxTrivia> trivia)
        {
            return new SyntaxToken(Kind, Text, Span, trivia.ToArray(), trailingTrivia);
        }

        public SyntaxToken WithTrailingTrivia(SyntaxTrivia trivia)
        {
            return new SyntaxToken(Kind, Text, Span, leadingTrivia, new[] { trivia });
        }

        public SyntaxToken WithTrailingTrivia(IEnumerable<SyntaxTrivia> trivia)
        {
            return new SyntaxToken(Kind, Text, Span, leadingTrivia, trivia.ToArray());
        }

        public void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitToken(this);
        }

        public T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitToken(this);
        }

        public void GetSourceText(TextWriter writer)
        {
            // Write leading trivia
            if (leadingTrivia != null)
            {
                foreach (SyntaxTrivia t in LeadingTrivia)
                    t.GetSourceText(writer);
            }

            // Write symbol
            writer.Write(Text);

            // Write trailing trivia
            if (trailingTrivia != null)
            {
                foreach (SyntaxTrivia t in TrailingTrivia)
                    t.GetSourceText(writer);
            }
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
            if(leadingTrivia == null && trailingTrivia == null)
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

        public static bool IsReservedTypeKind(SyntaxTokenKind kind)
        {
            switch(kind)
            {
                case SyntaxTokenKind.ObjectKeyword:
                case SyntaxTokenKind.BoolKeyword:
                case SyntaxTokenKind.Int8Keyword:
                case SyntaxTokenKind.UInt8Keyword:
                case SyntaxTokenKind.Int16Keyword:
                case SyntaxTokenKind.UInt16Keyword:
                case SyntaxTokenKind.Int32Keyword:
                case SyntaxTokenKind.UInt32Keyword:
                case SyntaxTokenKind.Int64Keyword:
                case SyntaxTokenKind.UInt64Keyword:
                case SyntaxTokenKind.FloatKeyword:
                case SyntaxTokenKind.DoubleKeyword:
                case SyntaxTokenKind.StringKeyword:
                case SyntaxTokenKind.VoidKeyword:
                    return true;
            }
            return false;
        }

        public static bool IsModifierKind(SyntaxTokenKind kind)
        {
            switch(kind)
            {
                // Static
                case SyntaxTokenKind.StaticKeyword:

                // Visibility
                case SyntaxTokenKind.PublicKeyword:                
                case SyntaxTokenKind.ProtectedKeyword:
                case SyntaxTokenKind.PrivateKeyword:
                case SyntaxTokenKind.InternalKeyword:

                // Inheritance
                case SyntaxTokenKind.VirtualKeyword:
                case SyntaxTokenKind.OverrideKeyword:

                // Accessible
                case SyntaxTokenKind.ConstKeyword:
                case SyntaxTokenKind.ReadonlyKeyword:
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
