using LumaSharp.Compiler.Parser;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.AST
{
    public sealed class SyntaxTree
    {
        // Private
        private readonly InputSource source;
        private readonly List<string> defineSymbols = new List<string>();
        private readonly SyntaxNode root = null;
        private readonly CompilerDiagnosticCollection diagnostics = null;

        // Properties
        public InputSource Source => source;
        public IReadOnlyList<string> DefineSymbols => defineSymbols;
        public SyntaxNode Root => root;
        public IReadOnlyList<CompilerDiagnostic> Diagnostics => diagnostics.AllDiagnostics;

        // Constructor
        private SyntaxTree(InputSource source, SyntaxNode root, CompilerDiagnosticCollection diagnostics, IEnumerable<string> defines = null)
        {
            this.source = source;
            this.root = root;
            this.diagnostics = diagnostics;

            if(defines != null)
                this.defineSymbols.AddRange(defines);
        }

        // Methods
        public void GetSourceText(TextWriter writer)
        {
            if(root != null)
                root.GetSourceText(writer);
        }

        public string GetSourceText()
        {
            using (StringWriter sw = new StringWriter())
            {
                GetSourceText(sw);
                return sw.ToString();
            }
        }

        public static SyntaxTree Create(SyntaxNode root)
        {
            // Check for compilation unit
            if(root is not CompilationUnitSyntax)
                root = new CompilationUnitSyntax(null, new[] { root });

            return new SyntaxTree(null, root, CompilerDiagnosticCollection.Empty);
        }

        public static SyntaxTree Create(params SyntaxNode[] rootNodes)
        {
            CompilationUnitSyntax unit = new CompilationUnitSyntax(null, rootNodes);

            // Create from unit
            return Create(unit);
        }

        public static SyntaxTree Parse(string source)
        {
            using (InputSource inputSource = InputSource.FromSourceText(source))
                return Parse(inputSource);
        }

        public static SyntaxTree Parse(InputSource source)
        {
            // Check for null
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // Create text view
            using(TextView textView = new TextView(source.Reader))
            {
                // Create token parser
                TokenParser tokenParser = new TokenParser(textView, source.Document);

                // Create the diagnostics
                CompilerDiagnosticCollection diagnostics = new ();

                // Create the syntax parser
                ASTParser syntaxParser = new ASTParser(tokenParser.GetEnumerator(), diagnostics);

                // Parse the compilation unit
                CompilationUnitSyntax unit = syntaxParser.ParseCompilationUnit();

                // Create the tree
                return new SyntaxTree(source, unit, diagnostics);
            }
        }

        public static SyntaxTree ParseStatement(string source)
        {
            using (InputSource inputSource = InputSource.FromSourceText(source))
                return ParseStatement(inputSource);
        }

        public static SyntaxTree ParseStatement(InputSource source)
        {
            // Check for null
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // Create text view
            using (TextView textView = new TextView(source.Reader))
            {
                // Create token parser
                TokenParser tokenParser = new TokenParser(textView, source.Document);

                // Create the diagnostics
                CompilerDiagnosticCollection diagnostics = new();

                // Create the syntax parser
                ASTParser syntaxParser = new ASTParser(tokenParser.GetEnumerator(), diagnostics);

                // Parse the compilation unit
                StatementSyntax statement = syntaxParser.ParseStatement();

                // Create the tree
                return new SyntaxTree(source, statement, diagnostics);
            }
        }

        public static SyntaxTree ParseExpression(string source)
        {
            using (InputSource inputSource = InputSource.FromSourceText(source))
                return ParseExpression(inputSource);
        }

        public static SyntaxTree ParseExpression(InputSource source)
        {
            // Check for null
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // Create text view
            using (TextView textView = new TextView(source.Reader))
            {
                // Create token parser
                TokenParser tokenParser = new TokenParser(textView, source.Document);

                // Create the diagnostics
                CompilerDiagnosticCollection diagnostics = new();

                // Create the syntax parser
                ASTParser syntaxParser = new ASTParser(tokenParser.GetEnumerator(), diagnostics);

                // Parse the compilation unit
                ExpressionSyntax expression = syntaxParser.ParseExpression();

                // Create the tree
                return new SyntaxTree(source, expression, diagnostics);
            }

            //return new ASTParser(new TokenParser(new TextView(source.Reader), source.Document).GetEnumerator(), new CompileReport())
            //    .ParseExpression();

            //// Create parser
            //ParserContext context = new ParserContext(source);
            
            //// Parse expression only
            //return context.ParseExpression();
        }
    }
}
