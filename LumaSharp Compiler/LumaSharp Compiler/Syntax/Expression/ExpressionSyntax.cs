
namespace LumaSharp.Compiler.AST
{
    public abstract class ExpressionSyntax : SyntaxNode
    {
        // Private
        private readonly SyntaxToken lParen;
        private readonly SyntaxToken rParen;

        // Properties
        public SyntaxToken LParen
        {
            get { return lParen; }
        }

        public SyntaxToken RParen
        {
            get { return rParen; }
        }

        public bool IsEnclosed
        {
            get { return lParen.Kind != SyntaxTokenKind.Invalid && rParen.Kind != SyntaxTokenKind.Invalid; }
        }

        // Constructor
        protected ExpressionSyntax(SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
            : base(parent) 
        {
            if (expression != null)
            {
                // Check for parent
                LumaSharpParser.ParenExpressionContext parenExpression = expression.parenExpression();

                if (parenExpression != null)
                {
                    lParen = new SyntaxToken(SyntaxTokenKind.LParenSymbol, parenExpression.LPAREN());
                    rParen = new SyntaxToken(SyntaxTokenKind.RParenSymbol, parenExpression.RPAREN());
                }
            }
        }

        // Methods
        public static SeparatedListSyntax<ExpressionSyntax> List(SyntaxNode parent, LumaSharpParser.ExpressionListContext expressionList)
        {
            // Check for null
            if (expressionList == null)
                return null;

            // Create separated list
            SeparatedListSyntax<ExpressionSyntax> list = new SeparatedListSyntax<ExpressionSyntax>(parent, SyntaxTokenKind.CommaSymbol);

            // Get primary
            LumaSharpParser.ExpressionContext primaryExpression = expressionList.expression();

            // Add primary
            list.AddElement(Any(list, primaryExpression), null);


            // Get secondary
            LumaSharpParser.ExpressionSecondaryContext[] secondaryExpressions = expressionList.expressionSecondary();

            // Check for any
            if(secondaryExpressions != null)
            {
                // Add all
                foreach(LumaSharpParser.ExpressionSecondaryContext secondaryExpression in secondaryExpressions)
                {
                    list.AddElement(
                        Any(list, secondaryExpression.expression()),
                        new SyntaxToken(SyntaxTokenKind.CommaSymbol, secondaryExpression.COMMA()));
                }
            }

            return list;
        }

        public static SeparatedListSyntax<TypeReferenceSyntax> List(SyntaxNode parent, LumaSharpParser.TypeReferenceListContext typeReferenceList)
        {
            // Create separated list
            SeparatedListSyntax<TypeReferenceSyntax> list = new SeparatedListSyntax<TypeReferenceSyntax>(parent, SyntaxTokenKind.CommaSymbol);

            // Get primary
            LumaSharpParser.TypeReferenceContext primaryTypeReference = typeReferenceList.typeReference();

            // Add primary
            list.AddElement(new TypeReferenceSyntax(list, null, primaryTypeReference), null);


            // Get secondary
            LumaSharpParser.TypeReferenceSecondaryContext[] secondaryTypeReferences = typeReferenceList.typeReferenceSecondary();

            // Check for any
            if (secondaryTypeReferences != null)
            {
                // Add all
                foreach (LumaSharpParser.TypeReferenceSecondaryContext secondaryTypeReference in secondaryTypeReferences)
                {
                    list.AddElement(
                        new TypeReferenceSyntax(list, null, secondaryTypeReference.typeReference()),
                        new SyntaxToken(SyntaxTokenKind.CommaSymbol, secondaryTypeReference.COMMA()));
                }
            }

            return list;
        }

        public static ExpressionSyntax Any(SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
        {
            // Check for index first of all
            if (expression.indexExpression() != null)
                return new ArrayIndexExpressionSyntax(parent, expression);

            // Check for base
            if (expression.BASE() != null)
                return new BaseExpressionSyntax(parent, expression);

            // Check for this
            if (expression.THIS() != null)
                return new ThisExpressionSyntax(parent, expression);

            // Check for size
            if (expression.sizeExpression() != null)
                return new SizeExpressionSyntax(parent, expression);

            // Check for type
            if (expression.typeExpression() != null)
                return new TypeExpressionSyntax(parent, expression);            

            // Check for method
            if (expression.methodInvokeExpression() != null)
                return new MethodInvokeExpressionSyntax(parent, expression);

            // Check for field
            if(expression.fieldAccessExpression() != null)
                return new FieldReferenceExpressionSyntax(parent, expression);

            // Check for new
            if(expression.newExpression() != null)
                return new NewExpressionSyntax(parent, expression);

            // Check for end
            if (expression.endExpression() != null)
                return new LiteralExpressionSyntax(parent, expression);

            // Check for bracketed
            if (expression.parenExpression() != null)
                return Any(parent, expression.expression(0));

            // Check for identifier
            if(expression.IDENTIFIER() != null)
                return new VariableReferenceExpressionSyntax(parent, expression);

            // Check for ternary
            if (expression.TERNARY() != null)
                return new TernaryExpressionSyntax(parent, expression);

            // Check for unary
            if(expression.unaryPrefix != null || expression.unaryPostfix != null)
                return new UnaryExpressionSyntax(parent, expression);

            // Check for binary
            if (expression.binary != null)
                return new BinaryExpressionSyntax(parent, expression);

            
            // No expression matched
            return null;
        }
    }
}
