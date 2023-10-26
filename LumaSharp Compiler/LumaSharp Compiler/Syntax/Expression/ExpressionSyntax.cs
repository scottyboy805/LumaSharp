using Antlr4.Runtime;
using LumaSharp_Compiler.AST.Expression;

namespace LumaSharp_Compiler.AST
{
    public abstract class ExpressionSyntax : SyntaxNode
    {
        // Constructor
        protected ExpressionSyntax(SyntaxToken singleToken)
            : base(singleToken)
        {
        }

        protected ExpressionSyntax(SyntaxToken start, SyntaxToken end)
            : base(start, end)
        {
        }

        protected ExpressionSyntax(SyntaxTree tree, SyntaxNode node, ParserRuleContext context)
            : base(tree, node, context) 
        {
        }

        // Methods
        public static ExpressionSyntax Any(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ExpressionContext expression)
        {
            // Check for index first of all
            if (expression.indexExpression() != null && expression.indexExpression().Length > 0)
                return new ArrayIndexExpressionSyntax(tree, parent, expression);

            // Check for base
            if (expression.BASE() != null)
                return new BaseExpressionSyntax(tree, parent, expression);

            // Check for this
            if (expression.THIS() != null)
                return new ThisExpressionSyntax(tree, parent, expression);

            // Check for size
            if (expression.sizeExpression() != null)
                return new SizeExpressionSyntax(tree, parent, expression.sizeExpression());

            // Check for type
            if (expression.typeExpression() != null)
                return new TypeExpressionSyntax(tree, parent, expression.typeExpression());            

            // Check for method
            if (expression.methodInvokeExpression() != null)
                return new MethodInvokeExpressionSyntax(tree, parent, expression);

            // Check for field
            if(expression.fieldAccessExpression() != null)
                return new FieldAccessorReferenceExpressionSyntax(tree, parent, expression);

            // Check for new
            if(expression.newExpression() != null)
                return new NewExpressionSyntax(tree, parent, expression.newExpression());

            // Check for stack initializer
            if (expression.initializerInvokeExpression() != null)
                return new NewExpressionSyntax(tree, parent, expression.initializerInvokeExpression());

            // Check for end
            if (expression.endExpression() != null)
                return new LiteralExpressionSyntax(tree, parent, expression.endExpression());

            // Check for bracketed
            if (expression.GetChild(0).GetText() == "(")
                return Any(tree, parent, expression.expression(0));

            // Check for identifier
            if(expression.IDENTIFIER() != null)
                return new VariableReferenceExpressionSyntax(tree, parent, expression);

            // Check for ternary
            if (expression.ternary != null)
                return new TernaryExpressionSyntax(tree, parent, expression);

            // Check for binary
            if (expression.binary != null)
                return new BinaryExpressionSyntax(tree, parent, expression);

            
            // No expression matched
            return null;
        }
    }
}
