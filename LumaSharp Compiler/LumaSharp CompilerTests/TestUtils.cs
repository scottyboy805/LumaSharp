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
    }
}
