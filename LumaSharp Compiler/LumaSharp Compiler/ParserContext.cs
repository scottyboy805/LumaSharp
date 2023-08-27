using Antlr4.Runtime;
using LumaSharp_Compiler.Syntax;

namespace LumaSharp_Compiler
{
    internal sealed class ParserContext : BaseErrorListener
    {
        // Private
        private InputSource inputSource = null;

        // Constructor
        public ParserContext(InputSource input)
        {
            this.inputSource = input;
        }

        // Methods
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            base.SyntaxError(output, recognizer, offendingSymbol, line, charPositionInLine, msg, e);
            throw new Exception($"Syntax error at line {line}, position {charPositionInLine}: {msg}");
        }

        public SyntaxTree ParseCompilationUnit()
        {
            // Create the parser
            return new SyntaxTree(CreateParser().compilationUnit());
        }

        //public StatementSyntax ParseStatement()
        //{
        //    return 
        //}

        public ExpressionSyntax ParseExpression()
        {
            return ExpressionSyntax.Any(null, null, CreateParser().expression());
        }

        private LumaSharpParser CreateParser()
        {
            // Create lexer
            LumaSharpLexer lexer = new LumaSharpLexer(inputSource.GetInputStream());

            // Create parser
            LumaSharpParser parser = new LumaSharpParser(new CommonTokenStream(lexer));

            // Register error listener
            parser.AddErrorListener(this);

            return parser;
        }
    }
}
