using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Parser
{
    internal sealed partial class ASTParser
    {
        // Methods
        /// <summary>
        /// This is the root parse expression entry point.
        /// it simply hands off to parse in order of precedence.
        /// </summary>
        /// <returns></returns>
        internal ExpressionSyntax ParseExpression() => ParseLogicalOrExpression();
        private ExpressionSyntax ParseLogicalOrExpression() => ParseBinaryExpression(o => o == SyntaxTokenKind.OrSymbol, ParseLogicalAndExpression);
        private ExpressionSyntax ParseLogicalAndExpression() => ParseBinaryExpression(o => o == SyntaxTokenKind.AndSymbol, ParseBitwiseOrExpression);
        private ExpressionSyntax ParseBitwiseOrExpression() => ParseBinaryExpression(o => o == SyntaxTokenKind.BitwiseOrSymbol, ParseBitwiseXOrExpression);
        private ExpressionSyntax ParseBitwiseXOrExpression() => ParseBinaryExpression(o => o == SyntaxTokenKind.BitwiseXOrSymbol, ParseBitwiseAndExpression);
        private ExpressionSyntax ParseBitwiseAndExpression() => ParseBinaryExpression(o => o == SyntaxTokenKind.BitwiseAndSymbol, ParseEqualityExpression);
        private ExpressionSyntax ParseEqualityExpression() => ParseBinaryExpression(o => o == SyntaxTokenKind.EqualitySymbol || o == SyntaxTokenKind.NonEqualitySymbol, ParseRelationalExpression);
        private ExpressionSyntax ParseRelationalExpression() => ParseBinaryExpression(o => o == SyntaxTokenKind.LessSymbol || o == SyntaxTokenKind.LessEqualSymbol || o == SyntaxTokenKind.GreaterSymbol || o == SyntaxTokenKind.GreaterEqualSymbol, ParseBitShiftExpression);
        private ExpressionSyntax ParseBitShiftExpression() => ParseBinaryExpression(o => o == SyntaxTokenKind.BitShiftLeftSymbol || o == SyntaxTokenKind.BitShiftRightSymbol, ParseAdditiveExpression);
        private ExpressionSyntax ParseAdditiveExpression() => ParseBinaryExpression(o => o == SyntaxTokenKind.AddSymbol || o == SyntaxTokenKind.SubtractSymbol, ParseMultiplicativeExpression);
        private ExpressionSyntax ParseMultiplicativeExpression() => ParseBinaryExpression(o => o == SyntaxTokenKind.MultiplySymbol || o == SyntaxTokenKind.DivideSymbol || o == SyntaxTokenKind.ModulusSymbol, ParseUnaryExpression);

        private ExpressionSyntax ParseBinaryExpression(Func<SyntaxTokenKind, bool> isOperand, Func<ExpressionSyntax> parseChildExpression)
        {
            // Parse and
            ExpressionSyntax expr = parseChildExpression();
            
            // Check for or operator
            while (isOperand(tokens.PeekKind()) == true)
            {
                // Consume the operator
                SyntaxToken operandToken = tokens.Consume();

                // Parse and
                ExpressionSyntax right = parseChildExpression();

                // Create binary
                expr = new BinaryExpressionSyntax(expr, operandToken, right);
            }
            return expr;
        }

        private ExpressionSyntax ParseUnaryExpression()
        {
            // Peek next
            SyntaxTokenKind kind = tokens.PeekKind();

            // Check for unary prefix
            if(kind == SyntaxTokenKind.SubtractSymbol || kind == SyntaxTokenKind.NotSymbol || kind == SyntaxTokenKind.PlusPlusSymbol || kind == SyntaxTokenKind.MinusMinusSymbol)
            {
                // Consume the operator
                SyntaxToken op = tokens.Consume();

                // Parse the expression
                ExpressionSyntax expr = ParseUnaryExpression();

                // Create the unary prefix
                return new UnaryExpressionSyntax(expr, op, true);
            }
            // Handle postfix expressions
            return ParsePostfixExpression();
        }

        private ExpressionSyntax ParsePostfixExpression()
        {
            // Parse the primary
            ExpressionSyntax expr = ParsePrimaryExpression();

            // Check for postfix operators
            while(tokens.PeekKind() == SyntaxTokenKind.PlusPlusSymbol || tokens.PeekKind() == SyntaxTokenKind.MinusMinusSymbol)
            {
                // Consume the token
                SyntaxToken op = tokens.Consume();

                // Create the unary postfix
                return new UnaryExpressionSyntax(expr, op, false);
            }
            // Just get the plain expression
            return expr;
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            ExpressionSyntax expr = null;

            // First handle parenthesis properly
            if(tokens.PeekKind() == SyntaxTokenKind.LParenSymbol)
            {
                // Consume the paren
                SyntaxToken lParen = tokens.Consume();

                // Begin parsing from top down once again
                expr = ParseLogicalOrExpression();

                // Expect closing paren
                if(tokens.ConsumeExpect(SyntaxTokenKind.RParenSymbol, out SyntaxToken rParen) == false)
                {
                    // Expected ')'
                    report.ReportMessage(Code.ExpectedToken, MessageSeverity.Error, rParen.Source, SyntaxToken.GetText(SyntaxTokenKind.RParenSymbol));
                    return RecoverFromExpressionError();
                }

                // Create a bracketed expression
                expr = new ParenthesizedExpressionSyntax(lParen, expr, rParen);
            }

            // Leaf expression nodes
            // ### Clearly defined rules - begin with an explicit keyword or symbol that uniquely identifies the expression syntax
            {
                // Check for base expression
                if (expr == null)
                    expr = ParseBaseExpression();

                // Check for this expression
                if (expr == null)
                    expr = ParseThisExpression();

                // Check for typeof
                if (expr == null)
                    expr = ParseTypeofExpression();

                // Check for sizeof
                if (expr == null)
                    expr = ParseSizeofExpression();
            }

            // ### Loosely defined rules - Can be matched in multiple cases so order of evaluation is important
            {
                // Literal
                if (expr == null)
                    expr = ParseLiteralExpression();

                // Identifier
                if(expr == null)
                    expr = ParseVariableReferenceExpression();

                // Type reference
                if (expr == null)
                    expr = ParseTypeReference();
            }

            // Array indexing which can apply to any leaf expression - will be checked semantically later
            // Check for valid expression
            if(expr != null)
            {
                // Handle array index for expression
                if(tokens.PeekKind() == SyntaxTokenKind.LArraySymbol)
                {
                    // Consume the array start
                    SyntaxToken lArray = tokens.Consume();

                    // Parse expression list
                    SeparatedListSyntax<ExpressionSyntax> indexExpressions = ParseSeparatedSyntaxList(ParseExpression, SyntaxTokenKind.CommaSymbol);

                    // Expect array end
                    if(tokens.ConsumeExpect(SyntaxTokenKind.RArraySymbol, out SyntaxToken rArray) == false)
                    {
                        // Expected ']'
                        report.ReportMessage(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Source, SyntaxToken.GetText(SyntaxTokenKind.RArraySymbol));
                        return RecoverFromExpressionError();
                    }

                    // Apply indexing
                    expr = new ArrayIndexExpressionSyntax(expr, lArray, indexExpressions, rArray);
                }

                // Finally get the expression
                return expr;
            }

            // Unexpected token - no valid expression was found
            report.ReportMessage(Code.UnexpectedToken, MessageSeverity.Error, tokens.Peek().Source, tokens.PeekKind());
            return RecoverFromExpressionError();
        }

        internal ThisExpressionSyntax ParseThisExpression()
        {
            // Check for this keyword
            if(tokens.PeekKind() == SyntaxTokenKind.ThisKeyword)
            {
                // Consume the token
                SyntaxToken thisToken = tokens.Consume();

                // Create the expression
                return new ThisExpressionSyntax(thisToken);
            }
            return null;
        }

        internal BaseExpressionSyntax ParseBaseExpression()
        {
            // Check for base keyword
            if(tokens.PeekKind() == SyntaxTokenKind.BaseKeyword)
            {
                // Consume the token
                SyntaxToken baseToken = tokens.Consume();

                // Create the expression
                return new BaseExpressionSyntax(baseToken);
            }
            return null;
        }

        internal ExpressionSyntax ParseTypeofExpression()
        {
            // Check for typeof keyword
            if(tokens.PeekKind() == SyntaxTokenKind.TypeofKeyword)
            {
                // Consume the token
                SyntaxToken typeofToken = tokens.Consume();

                // Expect lParen
                if(tokens.ConsumeExpect(SyntaxTokenKind.LParenSymbol, out SyntaxToken lParen) == false)
                {
                    // Expected '('
                    report.ReportMessage(Code.ExpectedToken, MessageSeverity.Error, lParen.Source, SyntaxToken.GetText(SyntaxTokenKind.LParenSymbol));
                    return RecoverFromExpressionError();
                }

                // Expect type reference
                TypeReferenceSyntax typeReference = ParseTypeReference();

                // Check for type
                if(typeReference == null)
                {
                    // Expected type
                    report.ReportMessage(Code.ExpectedType, MessageSeverity.Error, tokens.Peek().Source);
                    return RecoverFromExpressionError();
                }

                // Expect rParen
                if (tokens.ConsumeExpect(SyntaxTokenKind.RParenSymbol, out SyntaxToken rParen) == false)
                {
                    // Expected ')'
                    report.ReportMessage(Code.ExpectedToken, MessageSeverity.Error, rParen.Source, SyntaxToken.GetText(SyntaxTokenKind.RParenSymbol));
                    return RecoverFromExpressionError();
                }

                // Build typeof
                return new TypeofExpressionSyntax(typeofToken, lParen, typeReference, rParen);
            }
            // Not a typeof expression
            return null;
        }

        internal ExpressionSyntax ParseSizeofExpression()
        {
            // Check for typeof keyword
            if (tokens.PeekKind() == SyntaxTokenKind.SizeofKeyword)
            {
                // Consume the token
                SyntaxToken sizeofToken = tokens.Consume();

                // Expect lParen
                if (tokens.ConsumeExpect(SyntaxTokenKind.LParenSymbol, out SyntaxToken lParen) == false)
                {
                    // Expected '('
                    report.ReportMessage(Code.ExpectedToken, MessageSeverity.Error, lParen.Source, SyntaxToken.GetText(SyntaxTokenKind.LParenSymbol));
                    return RecoverFromExpressionError();
                }

                // Expect type reference
                TypeReferenceSyntax typeReference = ParseTypeReference();

                // Check for type
                if (typeReference == null)
                {
                    // Expected type
                    report.ReportMessage(Code.ExpectedType, MessageSeverity.Error, tokens.Peek().Source);
                    return RecoverFromExpressionError();
                }

                // Expect rParen
                if (tokens.ConsumeExpect(SyntaxTokenKind.RParenSymbol, out SyntaxToken rParen) == false)
                {
                    // Expected ')'
                    report.ReportMessage(Code.ExpectedToken, MessageSeverity.Error, rParen.Source, SyntaxToken.GetText(SyntaxTokenKind.RParenSymbol));
                    return RecoverFromExpressionError();
                }

                // Build typeof
                return new SizeofExpressionSyntax(sizeofToken, lParen, typeReference, rParen);
            }
            // Not a sizeof expression
            return null;
        }

        private ExpressionSyntax ParseNewExpression()
        {
            // Check for new
            if(tokens.PeekKind() == SyntaxTokenKind.NewKeyword)
            {
                // Get keyword
                SyntaxToken newToken = tokens.Consume();

                // Parse optional type reference
                TypeReferenceSyntax newTypeReference = ParseTypeReference();

                // Parse argument list
                ArgumentListSyntax argumentList = ParseArgumentList();

                // Check for argument list
                if (argumentList == null) 
                {
                    // Expected '('
                    report.ReportMessage(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Source, SyntaxToken.GetText(SyntaxTokenKind.LParenSymbol));
                    return RecoverFromExpressionError();
                }

                // Create new expression
                return new NewExpressionSyntax(newToken, newTypeReference, argumentList);
            }
            return null;
        }

        private ExpressionSyntax ParseLiteralExpression()
        {
            // Check for literal
            if(tokens.PeekKind() == SyntaxTokenKind.Literal)
            {
                // Consume the token
                SyntaxToken literal = tokens.Consume();

                // Check for optional descriptor
                SyntaxToken? descriptor = null;

                if(tokens.PeekKind() == SyntaxTokenKind.LiteralDescriptor)
                    descriptor = tokens.Consume();

                // Create the literal
                return new LiteralExpressionSyntax(literal, descriptor);
            }
            return null;
        }

        private ExpressionSyntax ParseVariableReferenceExpression()
        {
            // Check for identifier
            if(tokens.PeekKind() == SyntaxTokenKind.Identifier)
            {
                // Consume the token
                SyntaxToken identifier = tokens.Consume();

                // Create the variable reference
                return new VariableReferenceExpressionSyntax(identifier);
            }
            return null;
        }

        private ExpressionSyntax RecoverFromExpressionError()
        {
            // An error occurred while parsing an expression - To recover we should consume all tokens until we reach a semi colon or end of stream
            while (tokens.EOF == false && tokens.PeekKind() != SyntaxTokenKind.SemicolonSymbol)
                tokens.Consume();

            // Always null - just means we can return from calling control easily
            return null;
        }
    }
}
