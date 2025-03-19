
namespace LumaSharp.Compiler.AST
{
    public class ArgumentListSyntax : SeparatedListSyntax<ExpressionSyntax>
    {
        // Private
        private readonly SyntaxToken lParen;
        private readonly SyntaxToken rParen;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return lParen; }
        }

        public override SyntaxToken EndToken
        {
            get { return rParen; }
        }

        public SyntaxToken LParen
        {
            get { return lParen; }
        }

        public SyntaxToken RParen
        {
            get { return rParen; }
        }

        public bool HasArgumentExpressions
        {
            get { return Count > 0; }
        }

        // Constructor
        internal ArgumentListSyntax(SyntaxNode parent, ExpressionSyntax[] argumentExpressions)
            : base(parent, SyntaxTokenKind.CommaSymbol)
        {
            lParen = Syntax.KeywordOrSymbol(SyntaxTokenKind.LParenSymbol);
            rParen = Syntax.KeywordOrSymbol(SyntaxTokenKind.RParenSymbol);

            foreach (ExpressionSyntax expression in argumentExpressions)
                AddElement(expression, Syntax.KeywordOrSymbol(SyntaxTokenKind.CommaSymbol));
        }

        internal ArgumentListSyntax(SyntaxNode parent, LumaSharpParser.ArgumentListContext arguments)
            : base(parent, SyntaxTokenKind.CommaSymbol)
        {
            lParen = new SyntaxToken(SyntaxTokenKind.LParenSymbol, arguments.LPAREN());
            rParen = new SyntaxToken(SyntaxTokenKind.RParenSymbol, arguments.RPAREN());

            // Get expression reference list
            LumaSharpParser.ExpressionListContext expressionList = arguments.expressionList();

            if(expressionList != null)
            {
                // Add primary element
                LumaSharpParser.ExpressionContext primaryExpressionArgument = expressionList.expression();

                // Check for primary
                if (primaryExpressionArgument != null)
                    AddElement(ExpressionSyntax.Any(this, primaryExpressionArgument), null);


                // Add secondary elements
                LumaSharpParser.ExpressionSecondaryContext[] secondaryExpressionArguments = expressionList.expressionSecondary();

                // Check for secondary
                if(secondaryExpressionArguments != null)
                {
                    // Process all additional expressions
                    foreach(LumaSharpParser.ExpressionSecondaryContext secondaryExpressionArgument in secondaryExpressionArguments)
                    {
                        AddElement(
                            ExpressionSyntax.Any(this, secondaryExpressionArgument.expression()),
                            new SyntaxToken(SyntaxTokenKind.CommaSymbol, secondaryExpressionArgument.COMMA()));
                    }
                }
            }
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Argument start
            lParen.GetSourceText(writer);

            // Arguments
            base.GetSourceText(writer);

            // Argument end
            rParen.GetSourceText(writer);
        }
    }
}
