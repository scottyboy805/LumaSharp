
namespace LumaSharp_Compiler.AST.Expression
{
    public sealed class NewExpressionSyntax : ExpressionSyntax
    {
        // Private
        private SyntaxToken keyword = null;
        private SyntaxToken lparen = null;
        private SyntaxToken rparen = null;
        private TypeReferenceSyntax newType = null;
        private ExpressionSyntax[] argumentExpressions = null;

        // Properties
        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public TypeReferenceSyntax NewType
        {
            get { return newType; }
        }

        public ExpressionSyntax[] ArgumentExpressions
        {
            get { return argumentExpressions; }
            internal set { argumentExpressions = value; }
        }

        public int ArgumentExpressionCount
        {
            get { return HasArgumentExpressions ? argumentExpressions.Length : 0; }
        }

        public bool IsByReference
        {
            get { return keyword != null && keyword.Text == "new"; }
        }

        public bool HasKeyword
        {
            get { return keyword != null; }
        }

        public bool HasArgumentExpressions
        {
            get { return argumentExpressions != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                yield return newType;

                foreach (SyntaxNode node in argumentExpressions)
                    yield return node;
            }
        }

        // Constructor
        internal NewExpressionSyntax(TypeReferenceSyntax newType, bool stackAlloc)
            : base(newType.Identifier)
        {
            this.newType = newType;
            this.keyword = stackAlloc == false ? new SyntaxToken("new").WithTrailingWhitespace(" ") : null;
            
            lparen = new SyntaxToken("(");
            rparen = new SyntaxToken(")");
        }

        internal NewExpressionSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.NewExpressionContext expression)
            : base(tree, parent, expression)
        {
            // Keyword
            if (expression.NEW() != null)
            {
                this.keyword = new SyntaxToken(expression.NEW());
            }
            else if (expression.STACKNEW() != null)
            {
                this.keyword = new SyntaxToken(expression.STACKNEW());
            }

            // Get initializer
            LumaSharpParser.InitializerInvokeExpressionContext initializer = expression.initializerInvokeExpression();

            // New type
            this.newType = new TypeReferenceSyntax(tree, this, initializer.typeReference());

            // LR paren
            this.lparen = new SyntaxToken(initializer.lparen);
            this.rparen = new SyntaxToken(initializer.rparen);

            // Init expressions
            this.argumentExpressions = initializer.expression().Select(e => ExpressionSyntax.Any(tree, this, e)).ToArray();
        }

        internal NewExpressionSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.InitializerInvokeExpressionContext initializer)
            : base(tree, parent, initializer)
        {
            // New type
            this.newType = new TypeReferenceSyntax(tree, this, initializer.typeReference());

            // LR paren
            this.lparen = new SyntaxToken(initializer.lparen);
            this.rparen = new SyntaxToken(initializer.rparen);

            // Init expressions
            this.argumentExpressions = initializer.expression().Select(e => ExpressionSyntax.Any(tree, this, e)).ToArray();
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            if (HasKeyword == true)
                keyword.GetSourceText(writer);

            // Type reference
            newType.GetSourceText(writer);

            // L paren
            lparen.GetSourceText(writer);

            // Arguments
            if(HasArgumentExpressions == true)
            {
                for(int i = 0; i < argumentExpressions.Length; i++)
                {
                    // Expression
                    argumentExpressions[i].GetSourceText(writer);

                    // Separator
                    if (i < argumentExpressions.Length - 1)
                        writer.Write(",");
                }
            }

            // R paren
            rparen.GetSourceText(writer);
        }
    }
}
