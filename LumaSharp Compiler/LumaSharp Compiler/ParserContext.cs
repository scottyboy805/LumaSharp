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
        public SyntaxTree ParseCompilationUnit()
        {
            // Create the parser
            return new SyntaxTree(CreateParser().compilationUnit());
        }

        //public StatementSyntax ParseStatement()
        //{
        //    return 
        //}

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
