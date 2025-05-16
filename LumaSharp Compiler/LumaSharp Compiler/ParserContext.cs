//using Antlr4.Runtime;
//using LumaSharp.Compiler.AST;

//namespace LumaSharp.Compiler
//{
//    internal sealed class ParserContext : BaseErrorListener, IDisposable
//    {
//        // Private
//        private static ParserContext currentContext = null;

//        private InputSource inputSource = null;
//        private CommonTokenStream tokenStream = null;
//        private HashSet<IToken> consumedHiddenTokens = new HashSet<IToken>();

//        // Properties
//        public static ParserContext Current
//        {
//            get { return currentContext; }
//        }

//        // Constructor
//        public ParserContext(InputSource input)
//        {
//            this.inputSource = input;
//        }

//        // Methods
//        public void Dispose()
//        {
//            if (currentContext == this)
//                currentContext = null;
//        }

//        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
//        {
//            base.SyntaxError(output, recognizer, offendingSymbol, line, charPositionInLine, msg, e);
//            throw new Exception($"Syntax error at line {line}, position {charPositionInLine}: {msg}");
//        }

//        public IEnumerable<IToken> GetLeadingHiddenTokens(int tokenIndex)
//        {
//            IList<IToken> tokens = tokenStream.GetHiddenTokensToLeft(tokenIndex);

//            if (tokens != null)
//            {
//                // Process all
//                foreach (IToken token in tokens)
//                {
//                    //if (consumedHiddenTokens.Contains(token) == false)
//                        yield return token;

//                    // Mark as consumed
//                    //consumedHiddenTokens.Add(token);
//                }
//            }
//        }

//        public IEnumerable<IToken> GetTrailingHiddenTokens(int tokenIndex)
//        {
//            IList<IToken> tokens = tokenStream.GetHiddenTokensToRight(tokenIndex);

//            if(tokens != null)
//            {
//                // Process all
//                foreach(IToken token in tokens)
//                {
//                    //if(consumedHiddenTokens.Contains(token) == false)
//                        yield return token;

//                    // Mark as consumed
//                    //consumedHiddenTokens.Add(token);
//                }
//            }
//        }

//        public SyntaxTree ParseCompilationUnit()
//        {
//            // Create the parser
//            return new SyntaxTree(CreateParser().compilationUnit());
//        }

//        //public StatementSyntax ParseStatement()
//        //{
//        //    return 
//        //}

//        public ExpressionSyntax ParseExpression()
//        {
//            return ExpressionSyntax.Any(null, CreateParser().expression());
//        }

//        private LumaSharpParser CreateParser()
//        {
//            // Create lexer
//            LumaSharpLexer lexer = new LumaSharpLexer(inputSource.GetInputStream());

//            // Create token stream
//            tokenStream = new CommonTokenStream(lexer);
//            currentContext = this;

//            // Create parser
//            LumaSharpParser parser = new LumaSharpParser(tokenStream);

//            // Register error listener
//            parser.AddErrorListener(this);

//            return parser;
//        }
//    }
//}
