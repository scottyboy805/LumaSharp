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
        private readonly CompileReport report = null;

        // Properties
        public InputSource Source => source;
        public IReadOnlyList<string> DefineSymbols => defineSymbols;
        public SyntaxNode Root => root;
        public ICompileReport Report => report;

        // Constructor
        private SyntaxTree(InputSource source, SyntaxNode root, CompileReport report, IEnumerable<string> defines = null)
        {
            this.source = source;
            this.root = root;
            this.report = report;

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
            return new SyntaxTree(null, root, new CompileReport());
        }

        public static SyntaxTree Create(params SyntaxNode[] rootNodes)
        {
            CompilationUnitSyntax unit = new CompilationUnitSyntax(null, rootNodes);

            // Create from unit
            return Create(unit);
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

                // Create the report
                CompileReport report = new CompileReport();

                // Create the syntax parser
                ASTParser syntaxParser = new ASTParser(tokenParser.GetEnumerator(), report);

                // Parse the compilation unit
                CompilationUnitSyntax unit = syntaxParser.ParseCompilationUnit();

                // Create the tree
                return new SyntaxTree(source, unit, report);
            }
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

                // Create the report
                CompileReport report = new CompileReport();

                // Create the syntax parser
                ASTParser syntaxParser = new ASTParser(tokenParser.GetEnumerator(), report);

                // Parse the compilation unit
                StatementSyntax statement = syntaxParser.ParseStatement();

                // Create the tree
                return new SyntaxTree(source, statement, report);
            }
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

                // Create the report
                CompileReport report = new CompileReport();

                // Create the syntax parser
                ASTParser syntaxParser = new ASTParser(tokenParser.GetEnumerator(), report);

                // Parse the compilation unit
                ExpressionSyntax expression = syntaxParser.ParseExpression();

                // Create the tree
                return new SyntaxTree(source, expression, report);
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
