using Antlr4.Runtime;
using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Parser;
using LumaSharp.Compiler.Reporting;
using System.Diagnostics;

namespace LumaSharp_CompilerTests
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
            return CreateStringParser(input).ParseStatement();
        }

        public static ExpressionSyntax ParseInputStringExpression(string input)
        {
            return CreateStringParser(input).ParseExpression();
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
            return CreateStringParser(input).ParseMember();
        }

        public static TypeSyntax ParseTypeDeclaration(string input)
        {
            return CreateStringParser(input).ParseTypeDeclaration();
        }

        public static ContractSyntax ParseContractDeclaration(string input)
        {
            return CreateStringParser(input).ParseContractDeclaration();
        }

        public static EnumSyntax ParseEnumDeclaration(string input)
        {
            return CreateStringParser(input).ParseEnumDeclaration();
        }

        public static FieldSyntax ParseFieldDeclaration(string input)
        {
            return CreateStringParser(input).ParseFieldDeclaration();
        }

        public static AccessorSyntax ParseAccessorDeclaration(string input)
        {
            return CreateStringParser(input).ParseAccessorDeclaration();
        }

        public static TypeReferenceSyntax ParseTypeReference(string input)
        {
            return CreateStringParser(input).ParseTypeReference();
        }

        private static ASTParser CreateStringParser(string input)
        {
            // Create the report
            CompileReport report = new CompileReport();

            // Create the text view
            TextView textView = new TextView(new StringReader(input));
            
            // Create the token parser
            TokenParser tokenParser = new TokenParser(textView);

            // Create parser
            ASTParser parser = new ASTParser(tokenParser.GetEnumerator(), report);

            // Check for errors
            foreach(ICompileMessage message in report.Messages)
            {
                Debug.WriteLine(message);
            }

            return parser;
        }
    }
}
