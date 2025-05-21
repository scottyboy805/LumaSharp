using Antlr4.Runtime;
using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.AST.Visitor;
using LumaSharp.Compiler.Parser;
using LumaSharp.Compiler.Reporting;
using System.Diagnostics;

namespace CompilerTests
{
    public static class TestUtils
    {
        // Type
        private class SyntaxErrorHandler : BaseErrorListener
        {
            public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                throw new Exception($"Syntax error at line {line}, position {charPositionInLine}: {msg}");
            }
        }

        // Methods
        public static LumaSharpParser.CompilationUnitContext ParseInputString(string input)
        {
            // Create the parser
            AntlrInputStream inputStream = new AntlrInputStream(input);
            LumaSharpLexer lexer = new LumaSharpLexer(inputStream);
            LumaSharpParser parser = new LumaSharpParser(new CommonTokenStream(lexer));

            // Add error handler
            parser.AddErrorListener(new  SyntaxErrorHandler());

            // Run program
            LumaSharpParser.CompilationUnitContext context = parser.compilationUnit();

            // Log errors
            if(parser.NumberOfSyntaxErrors > 0)
            {
                Debug.WriteLine("Syntax errors: " + parser.NumberOfSyntaxErrors);
            }

            return context;
        }

        public static StatementSyntax ParseInputStringStatement(string input)
        {
            return CreateParser(input, p => p.ParseStatement());
        }

        public static ExpressionSyntax ParseInputStringExpression(string input)
        {
            return CreateParser(input, p => p.ParseExpression());
        }

        public static LumaSharpParser.NamespaceDeclarationContext ParseNamespaceDeclaration(string input)
        {
            // Create the parser
            AntlrInputStream inputStream = new AntlrInputStream(input);
            LumaSharpLexer lexer = new LumaSharpLexer(inputStream);
            LumaSharpParser parser = new LumaSharpParser(new CommonTokenStream(lexer));

            // Add error handler
            parser.AddErrorListener(new SyntaxErrorHandler());

            // Run program
            LumaSharpParser.NamespaceDeclarationContext context = parser.namespaceDeclaration();

            // Log errors
            if (parser.NumberOfSyntaxErrors > 0)
            {
                Debug.WriteLine("Syntax errors: " + parser.NumberOfSyntaxErrors);
            }

            return context;
        }

        public static MemberSyntax ParseMemberDeclaration(string input)
        {
            return CreateParser(input, p => p.ParseMember());
        }

        public static TypeSyntax ParseTypeDeclaration(string input)
        {
            return CreateParser(input, p => p.ParseTypeDeclaration());
        }

        public static ContractSyntax ParseContractDeclaration(string input)
        {
            return CreateParser(input, p => p.ParseContractDeclaration());
        }

        public static EnumSyntax ParseEnumDeclaration(string input)
        {
            return CreateParser(input, p => p.ParseEnumDeclaration());
        }

        public static FieldSyntax ParseFieldDeclaration(string input)
        {
            return CreateParser(input, p => p.ParseFieldDeclaration());
        }

        public static AccessorSyntax ParseAccessorDeclaration(string input)
        {
            return CreateParser(input, p => p.ParseAccessorDeclaration());
        }

        public static TypeReferenceSyntax ParseTypeReference(string input)
        {
            return CreateParser(input, p => p.ParseTypeReference());
        }

        private static T CreateParser<T>(string input, Func<ASTParser, T> parse) where T : SyntaxNode
        {
            // Create the report
            CompileReport report = new CompileReport();

            // Create the text view
            TextView textView = new TextView(new StringReader(input));
            
            // Create the token parser
            TokenParser tokenParser = new TokenParser(textView);

            // Create parser
            ASTParser parser = new ASTParser(tokenParser.GetEnumerator(), report);

            // Try to parse
            T result = parse(parser);

            // Check for errors
            foreach(ICompileDiagnostic message in report.Diagnostics)
            {
                Debug.WriteLine(message);
            }

            // Write syntax
            if(result is ExpressionSyntax e)
            {
                PrintSyntaxTreeWalker print = new();
                e.Accept(print);
                Debug.WriteLine(print.Text);
            }

            return result;
        }
    }
}
