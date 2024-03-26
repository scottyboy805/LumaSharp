
namespace LumaSharp_Compiler.AST.Expression
{
    public sealed class MethodInvokeExpressionSyntax : ExpressionSyntax
    {
        // Private
        private ExpressionSyntax accessExpression = null;
        private SyntaxToken identifier = null;
        private TypeReferenceSyntax[] genericArguments = null;
        private ExpressionSyntax[] arguments = null;
        private SyntaxToken dot = null;
        private SyntaxToken lgen = null;
        private SyntaxToken rgen = null;
        private SyntaxToken lparen = null;
        private SyntaxToken rparen = null;

        // Properties
        public ExpressionSyntax AccessExpression
        {
            get { return accessExpression; }
        }

        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public TypeReferenceSyntax[] GenericArguments
        {
            get { return genericArguments; }
            internal set { genericArguments = value; }
        }

        public ExpressionSyntax[] Arguments
        {
            get { return arguments; }
            internal set { arguments = value; }
        }

        public int GenericArgumentCount
        {
            get { return HasGenericArguments ? genericArguments.Length : 0; }
        }

        public int ArgumentCount
        {
            get { return HasArguments ? arguments.Length : 0; }
        }

        public bool HasGenericArguments
        {
            get { return genericArguments != null; }
        }

        public bool HasArguments
        {
            get { return arguments != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                yield return accessExpression;

                // Get generics
                if(HasGenericArguments == true)
                {
                    foreach (SyntaxNode node in GenericArguments)
                        yield return node;
                }

                // Get arguments
                if(HasArguments == true)
                {
                    foreach(SyntaxNode node in Arguments)
                        yield return node;
                }
            }
        }

        // Constructor
        internal MethodInvokeExpressionSyntax(string identifier, ExpressionSyntax accessExpression)
            : base(accessExpression.StartToken, SyntaxToken.RParen())
        {
            this.identifier = new SyntaxToken(identifier);
            this.accessExpression = accessExpression;

            dot = SyntaxToken.Dot();
            lgen = SyntaxToken.LGeneric();
            rgen = SyntaxToken.RGeneric();
            lparen = SyntaxToken.LParen();
            rparen = base.EndToken;
        }

        internal MethodInvokeExpressionSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(tree, parent, expression)
        {
            // Create access expression
            if (expression.typeReference() != null)
            {
                this.accessExpression = new TypeReferenceSyntax(tree, this, expression.typeReference());
            }
            else if(expression.expression(0) != null)
            {
                this.accessExpression = Any(tree, this, expression.expression(0));
            }

            // Get the method
            LumaSharpParser.MethodInvokeExpressionContext method = expression.methodInvokeExpression();

            // Identifier
            this.identifier = new SyntaxToken(method.IDENTIFIER());

            // Get dot token
            if (accessExpression != null)
            {
                dot = new SyntaxToken(method.dot);
            }
            
            // Generic arguments
            LumaSharpParser.GenericArgumentsContext generics = method.genericArguments();

            if (generics != null)
            {
                this.genericArguments = generics.typeReference().Select(t => new TypeReferenceSyntax(tree, this, t)).ToArray();

                lgen = new SyntaxToken(generics.lgen);
                rgen = new SyntaxToken(generics.rgen);
            }

            lparen = new SyntaxToken(method.lparen);
            rparen = new SyntaxToken(method.rparen);


            // Method arguments
            LumaSharpParser.MethodArgumentsContext argumentList = method.methodArguments();

            if(argumentList != null)
            {
                // Get arguments
                this.arguments = argumentList.methodArgument().Select(a => ExpressionSyntax.Any(tree, this, a.expression())).ToArray();
            }
        }

        internal MethodInvokeExpressionSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ExpressionContext expression, LumaSharpParser.MethodInvokeExpressionContext method)
            : base(tree, parent, method)
        {
            // Identifier
            this.identifier = new SyntaxToken(method.IDENTIFIER());

            dot = new SyntaxToken(method.dot);

            // Generic arguments
            LumaSharpParser.GenericArgumentsContext generics = method.genericArguments();

            if (generics != null)
            {
                this.genericArguments = generics.typeReference().Select(t => new TypeReferenceSyntax(tree, this, t)).ToArray();

                lgen = new SyntaxToken(generics.lgen);
                rgen = new SyntaxToken(generics.rgen);
            }

            lparen = new SyntaxToken(method.lparen);
            rparen = new SyntaxToken(method.rparen);

            if (expression != null)
            {
                // Create access expression
                if (expression.typeReference() != null)
                {
                    this.accessExpression = new TypeReferenceSyntax(tree, this, expression.typeReference());
                }
                else
                {
                    this.accessExpression = Any(tree, this, expression.expression(0));
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
            }

            if (dot != null)
            {
                // Write dot 
                dot.GetSourceText(writer);
            }

            // Write method name
            writer.Write(identifier.ToString());

            // Write generics
            if(HasGenericArguments == true)
            {
                // Write lgen
                lgen.GetSourceText(writer);

                // Write all generics
                for(int i = 0; i < genericArguments.Length; i++)
                {
                    genericArguments[i].GetSourceText(writer);

                    // Write comma
                    if(i < genericArguments.Length - 1)
                        writer.Write(",");
                }

                // Write rgen
                rgen.GetSourceText(writer);
            }

            // Write lparen
            lparen.GetSourceText(writer);

            // Write all arguments
            if (HasArguments == true)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    arguments[i].GetSourceText(writer);

                    // Write comma
                    if(i < arguments.Length - 1)
                        writer.Write(",");
                }
            }

            // Write rparen
            rparen.GetSourceText(writer);
        }
    }
}
