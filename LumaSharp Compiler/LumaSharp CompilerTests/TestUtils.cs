using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static LumaSharpParser.StatementContext ParseInputStringStatement(string input)
        {
            // Create the parser
            AntlrInputStream inputStream = new AntlrInputStream(input);
            LumaSharpLexer lexer = new LumaSharpLexer(inputStream);
            LumaSharpParser parser = new LumaSharpParser(new CommonTokenStream(lexer));

            // Add error handler
            parser.AddErrorListener(new SyntaxErrorHandler());

            // Run program
            LumaSharpParser.StatementContext context = parser.statement();

            // Log errors
            if (parser.NumberOfSyntaxErrors > 0)
            {
                Debug.WriteLine("Syntax errors: " + parser.NumberOfSyntaxErrors);
            }

            return context;
        }

        public static LumaSharpParser.ExpressionContext ParseInputStringExpression(string input)
        {
            // Create the parser
            AntlrInputStream inputStream = new AntlrInputStream(input);
            LumaSharpLexer lexer = new LumaSharpLexer(inputStream);
            LumaSharpParser parser = new LumaSharpParser(new CommonTokenStream(lexer));

            // Add error handler
            parser.AddErrorListener(new SyntaxErrorHandler());

            // Run program
            LumaSharpParser.ExpressionContext context = parser.expression();

            // Log errors
            if (parser.NumberOfSyntaxErrors > 0)
            {
                Debug.WriteLine("Syntax errors: " + parser.NumberOfSyntaxErrors);
            }

            return context;
        }

        public static LumaSharpParser.TypeDeclarationContext ParseTypeDeclaration(string input)
        {
            // Create the parser
            AntlrInputStream inputStream = new AntlrInputStream(input);
            LumaSharpLexer lexer = new LumaSharpLexer(inputStream);
            LumaSharpParser parser = new LumaSharpParser(new CommonTokenStream(lexer));

            // Add error handler
            parser.AddErrorListener(new SyntaxErrorHandler());

            // Run program
            LumaSharpParser.TypeDeclarationContext context = parser.typeDeclaration();

            // Log errors
            if (parser.NumberOfSyntaxErrors > 0)
            {
                Debug.WriteLine("Syntax errors: " + parser.NumberOfSyntaxErrors);
            }

            return context;
        }

        public static LumaSharpParser.FieldDeclarationContext ParseFieldDeclaration(string input)
        {
            // Create the parser
            AntlrInputStream inputStream = new AntlrInputStream(input);
            LumaSharpLexer lexer = new LumaSharpLexer(inputStream);
            LumaSharpParser parser = new LumaSharpParser(new CommonTokenStream(lexer));

            // Add error handler
            parser.AddErrorListener(new SyntaxErrorHandler());

            // Run program
            LumaSharpParser.FieldDeclarationContext context = parser.fieldDeclaration();

            // Log errors
            if (parser.NumberOfSyntaxErrors > 0)
            {
                Debug.WriteLine("Syntax errors: " + parser.NumberOfSyntaxErrors);
            }

            return context;
        }

        public static LumaSharpParser.AccessorDeclarationContext ParseAccessorDeclaration(string input)
        {
            // Create the parser
            AntlrInputStream inputStream = new AntlrInputStream(input);
            LumaSharpLexer lexer = new LumaSharpLexer(inputStream);
            LumaSharpParser parser = new LumaSharpParser(new CommonTokenStream(lexer));

            // Add error handler
            parser.AddErrorListener(new SyntaxErrorHandler());

            // Run program
            LumaSharpParser.AccessorDeclarationContext context = parser.accessorDeclaration();

            // Log errors
            if (parser.NumberOfSyntaxErrors > 0)
            {
                Debug.WriteLine("Syntax errors: " + parser.NumberOfSyntaxErrors);
            }

            return context;
        }

        public static LumaSharpParser.TypeReferenceContext ParseTypeReference(string input)
        {
            // Create the parser
            AntlrInputStream inputStream = new AntlrInputStream(input);
            LumaSharpLexer lexer = new LumaSharpLexer(inputStream);
            LumaSharpParser parser = new LumaSharpParser(new CommonTokenStream(lexer));

            // Add error handler
            parser.AddErrorListener(new SyntaxErrorHandler());

            // Run program
            LumaSharpParser.TypeReferenceContext context = parser.typeReference();

            // Log errors
            if (parser.NumberOfSyntaxErrors > 0)
            {
                Debug.WriteLine("Syntax errors: " + parser.NumberOfSyntaxErrors);
            }

            return context;
        }
    }
}
