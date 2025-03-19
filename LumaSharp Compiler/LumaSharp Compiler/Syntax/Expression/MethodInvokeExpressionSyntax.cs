
namespace LumaSharp.Compiler.AST
{
    public sealed class MethodInvokeExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly ExpressionSyntax accessExpression;
        private readonly SyntaxToken identifier;
        private readonly SyntaxToken dot;
        private readonly GenericArgumentListSyntax genericArgumentList;
        private readonly ArgumentListSyntax argumentList;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return accessExpression.StartToken; }
        }

        public override SyntaxToken EndToken
        {
            get { return argumentList.EndToken; }
        }

        public ExpressionSyntax AccessExpression
        {
            get { return accessExpression; }
        }

        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public SyntaxToken Dot
        {
            get { return dot; }
        }

        public GenericArgumentListSyntax GenericArgumentList
        {
            get { return genericArgumentList; }
        }

        public ArgumentListSyntax ArgumentList
        {
            get { return argumentList; }
        }

        public int GenericArgumentCount
        {
            get { return HasGenericArguments ? genericArgumentList.Count : 0; }
        }

        public int ArgumentCount
        {
            get { return argumentList.Count; }
        }

        public bool HasGenericArguments
        {
            get { return genericArgumentList != null; }         // Possible to have empty generics Type<>
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                yield return accessExpression;

                // Get generics
                if(HasGenericArguments == true)
                {
                    foreach (SyntaxNode node in genericArgumentList.Descendants)
                        yield return node;
                }

                // Get arguments
                foreach(SyntaxNode node in argumentList.Descendants)
                    yield return node;
            }
        }

        // Constructor
        internal MethodInvokeExpressionSyntax(SyntaxNode parent, string identifier, ExpressionSyntax accessExpression, GenericArgumentListSyntax genericArguments, ArgumentListSyntax arguments)
            : base(parent, null)
        {
            this.identifier = Syntax.Identifier(identifier);
            this.accessExpression = accessExpression;
            this.argumentList = arguments;

            if(genericArgumentList != null)
            {
                this.dot = Syntax.KeywordOrSymbol(SyntaxTokenKind.DotSymbol);
                this.genericArgumentList = genericArguments;
            }
        }

        internal MethodInvokeExpressionSyntax(SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(parent, expression)
        {
            // Get invoke
            LumaSharpParser.MethodInvokeExpressionContext method = expression.methodInvokeExpression();

            // Identifier
            this.identifier = new SyntaxToken(SyntaxTokenKind.Identifier, method.IDENTIFIER());

            // Generic arguments
            LumaSharpParser.GenericArgumentListContext generics = method.genericArgumentList();

            if (generics != null)
            {
                this.genericArgumentList = new GenericArgumentListSyntax(this, generics);
            }

            // Create arguments
            this.argumentList = new ArgumentListSyntax(this, method.argumentList());

            if (expression != null)
            {
                // Create dot
                dot = new SyntaxToken(SyntaxTokenKind.DotSymbol, method.DOT());

                // Create access expression
                if (expression.typeReference() != null)
                {
                    this.accessExpression = new TypeReferenceSyntax(this, null, expression.typeReference());
                }
                else
                {
                    this.accessExpression = Any(this, expression.expression(0));
                }
            }
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            if (accessExpression != null)
            {
                // Write access
                accessExpression.GetSourceText(writer);

                // Write dot 
                dot.GetSourceText(writer);
            }

            // Write method name
            writer.Write(identifier.ToString());

            // Write generics
            if(HasGenericArguments == true)
            {
                // Build generic arguments
                genericArgumentList.GetSourceText(writer);
            }

            // Build arguments
            argumentList.GetSourceText(writer);
        }
    }
}
