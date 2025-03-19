
namespace LumaSharp.Compiler.AST
{
    public sealed class ParameterListSyntax : SeparatedListSyntax<ParameterSyntax>
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

        public bool HasParameters
        {
            get { return Count > 0; }
        }

        // Constructor
        internal ParameterListSyntax(SyntaxNode parent, ParameterSyntax[] parameters)
            : base(parent, SyntaxTokenKind.CommaSymbol)
        {
            this.lParen = Syntax.KeywordOrSymbol(SyntaxTokenKind.LParenSymbol);
            this.rParen = Syntax.KeywordOrSymbol(SyntaxTokenKind.RParenSymbol);

            if (parameters != null)
            {
                // Add all parameters
                foreach (ParameterSyntax parameter in parameters)
                    AddElement(parameter, Syntax.KeywordOrSymbol(SyntaxTokenKind.CommaSymbol));
            }
        }

        internal ParameterListSyntax(SyntaxNode parent, LumaSharpParser.MethodParameterListContext paramsDef)
            : base(parent, SyntaxTokenKind.CommaSymbol)
        {
            this.lParen = new SyntaxToken(SyntaxTokenKind.CommaSymbol, paramsDef.LPAREN());
            this.rParen = new SyntaxToken(SyntaxTokenKind.CommaSymbol, paramsDef.RPAREN());

            // Add primate element
            LumaSharpParser.MethodParameterContext primaryParameter = paramsDef.methodParameter();

            // Check for primary
            if (primaryParameter != null)
                AddElement(new ParameterSyntax(this, primaryParameter, 0), null);


            // Add secondary elements
            LumaSharpParser.MethodParameterSecondaryContext[] secondaryParameters = paramsDef.methodParameterSecondary();

            // Check for secondary
            if(secondaryParameters != null)
            {
                int index = 1;

                // Process all additional parameters
                foreach(LumaSharpParser.MethodParameterSecondaryContext secondaryParameter in secondaryParameters)
                {
                    AddElement(
                        new ParameterSyntax(this, secondaryParameter.methodParameter(), index++),
                        new SyntaxToken(SyntaxTokenKind.CommaSymbol, secondaryParameter.COMMA()));
                }
            }
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Parameter start
            lParen.GetSourceText(writer);

            // Parameters
            base.GetSourceText(writer);

            // Parameter end
            rParen.GetSourceText(writer);
        }
    }
}
