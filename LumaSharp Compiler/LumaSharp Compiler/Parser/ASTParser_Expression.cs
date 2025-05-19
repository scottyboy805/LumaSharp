using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using System.Text;

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
        internal ExpressionSyntax ParseExpression()
        {
            // Parse root expression
            ExpressionSyntax expr = ParseLogicalOrBinaryExpression();

            // Array indexing which can apply to any leaf expression - will be checked semantically later
            // Check for valid expression
            if (expr != null)
            {
                // Handle array index for expression or just return the input expression
                expr = ParseIndexExpression(expr);

                // Handle member access for expression or just return the input expression
                expr = ParseMemberAccessExpression(expr);

                // Handle ternary
                expr = ParseTernaryExpression(expr);
            }
            // Finally get the expression
            return expr;
        }

        private ExpressionSyntax ParseLogicalOrBinaryExpression()
        {
            // Parse and
            ExpressionSyntax expr = ParseLogicalAndBinaryExpression();

            // Check for or token
            while(tokens.PeekKind() == SyntaxTokenKind.OrSymbol)
            {
                // Consume the operator
                SyntaxToken operandToken = tokens.Consume();

                // Parse right
                ExpressionSyntax right = ParseLogicalAndBinaryExpression();

                // Create binary
                return new BinaryExpressionSyntax(expr, operandToken, right);
            }
            return expr;
        }

        private ExpressionSyntax ParseLogicalAndBinaryExpression()
        {
            // Parse and
            ExpressionSyntax expr = ParseBitwiseXOrBinaryExpression();

            // Check for and token
            while (tokens.PeekKind() == SyntaxTokenKind.AndSymbol)
            {
                // Consume the operator
                SyntaxToken operandToken = tokens.Consume();

                // Parse right
                ExpressionSyntax right = ParseBitwiseXOrBinaryExpression();

                // Create binary
                return new BinaryExpressionSyntax(expr, operandToken, right);
            }
            return expr;
        }

        private ExpressionSyntax ParseBitwiseOrBinaryExpression()
        {
            // Parse and
            ExpressionSyntax expr = ParseBitwiseXOrBinaryExpression();

            // Check for or token
            while (tokens.PeekKind() == SyntaxTokenKind.BitwiseOrSymbol)
            {
                // Consume the operator
                SyntaxToken operandToken = tokens.Consume();

                // Parse right
                ExpressionSyntax right = ParseBitwiseXOrBinaryExpression();

                // Create binary
                return new BinaryExpressionSyntax(expr, operandToken, right);
            }
            return expr;
        }

        private ExpressionSyntax ParseBitwiseXOrBinaryExpression()
        {
            // Parse and
            ExpressionSyntax expr = ParseBitwiseAndBinaryExpression();

            // Check for xor token
            while (tokens.PeekKind() == SyntaxTokenKind.BitwiseXOrSymbol)
            {
                // Consume the operator
                SyntaxToken operandToken = tokens.Consume();

                // Parse right
                ExpressionSyntax right = ParseBitwiseAndBinaryExpression();

                // Create binary
                return new BinaryExpressionSyntax(expr, operandToken, right);
            }
            return expr;
        }

        private ExpressionSyntax ParseBitwiseAndBinaryExpression()
        {
            // Parse and
            ExpressionSyntax expr = ParseEqualityBinaryExpression();

            // Check for and token
            while (tokens.PeekKind() == SyntaxTokenKind.BitwiseAndSymbol)
            {
                // Consume the operator
                SyntaxToken operandToken = tokens.Consume();

                // Parse right
                ExpressionSyntax right = ParseEqualityBinaryExpression();

                // Create binary
                return new BinaryExpressionSyntax(expr, operandToken, right);
            }
            return expr;
        }

        private ExpressionSyntax ParseEqualityBinaryExpression()
        {
            // Parse and
            ExpressionSyntax expr = ParseRelationalBinaryExpression();

            // Check for '==' '!=' tokens
            while (tokens.PeekKind() == SyntaxTokenKind.EqualitySymbol || tokens.PeekKind() == SyntaxTokenKind.NonEqualitySymbol)
            {
                // Consume the operator
                SyntaxToken operandToken = tokens.Consume();

                // Parse right
                ExpressionSyntax right = ParseRelationalBinaryExpression();

                // Create binary
                return new BinaryExpressionSyntax(expr, operandToken, right);
            }
            return expr;
        }

        private ExpressionSyntax ParseRelationalBinaryExpression()
        {
            int expressionStart = tokens.Position;

            // Parse and
            ExpressionSyntax expr = ParseBitShiftBinaryExpression();

            // Handle ambiguity between less than and open generics
            if(LooksLikeGenericArgumentList() == true)
            {
                // Retrace back to start of expression and now try and parse it as a generic type reference
                tokens.Retrace(expressionStart);
                expr = ParseTypeReference();
            }

            // Check for '<' '<=' '>' '>=' tokens
            while (tokens.PeekKind() == SyntaxTokenKind.LessSymbol || tokens.PeekKind() == SyntaxTokenKind.LessEqualSymbol || tokens.PeekKind() == SyntaxTokenKind.GreaterSymbol || tokens.PeekKind() == SyntaxTokenKind.GreaterEqualSymbol)
            {
                // Consume the operator
                SyntaxToken operandToken = tokens.Consume();

                // Parse right
                ExpressionSyntax right = ParseBitShiftBinaryExpression();

                // Create binary
                return new BinaryExpressionSyntax(expr, operandToken, right);
            }
            return expr;
        }

        private ExpressionSyntax ParseBitShiftBinaryExpression()
        {
            // Parse and
            ExpressionSyntax expr = ParseAdditiveBinaryExpression();

            // Check for '<<' '>>' tokens
            while (tokens.PeekKind() == SyntaxTokenKind.BitShiftLeftSymbol || tokens.PeekKind() == SyntaxTokenKind.BitShiftRightSymbol)
            {
                // Consume the operator
                SyntaxToken operandToken = tokens.Consume();

                // Parse right
                ExpressionSyntax right = ParseAdditiveBinaryExpression();

                // Create binary
                return new BinaryExpressionSyntax(expr, operandToken, right);
            }
            return expr;
        }

        private ExpressionSyntax ParseAdditiveBinaryExpression()
        {
            // Parse and
            ExpressionSyntax expr = ParseMultiplicativeBinaryExpression();

            // Check for '+' '-' tokens
            while (tokens.PeekKind() == SyntaxTokenKind.AddSymbol || tokens.PeekKind() == SyntaxTokenKind.SubtractSymbol)
            {
                // Consume the operator
                SyntaxToken operandToken = tokens.Consume();

                // Parse right
                ExpressionSyntax right = ParseMultiplicativeBinaryExpression();

                // Create binary
                return new BinaryExpressionSyntax(expr, operandToken, right);
            }
            return expr;
        }

        private ExpressionSyntax ParseMultiplicativeBinaryExpression()
        {
            // Parse and
            ExpressionSyntax expr = ParseUnaryExpression();

            // Check for '+' '-' tokens
            while (tokens.PeekKind() == SyntaxTokenKind.MultiplySymbol || tokens.PeekKind() == SyntaxTokenKind.DivideSymbol || tokens.PeekKind() == SyntaxTokenKind.ModulusSymbol)
            {
                // Consume the operator
                SyntaxToken operandToken = tokens.Consume();

                // Parse right
                ExpressionSyntax right = ParseUnaryExpression();

                // Create binary
                return new BinaryExpressionSyntax(expr, operandToken, right);
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
                expr = ParseExpression();

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

                // Check for new
                if (expr == null)
                    expr = ParseNewExpression();
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

            // Check for valid expression
            if(expr != null)
            {
                // Finally get the expression
                return expr;
            }

            // Unexpected token - no valid expression was found
            report.ReportMessage(Code.UnexpectedToken, MessageSeverity.Error, tokens.Peek().Source, SyntaxToken.GetText(tokens.PeekKind()));
            return RecoverFromExpressionError();
        }

        private ExpressionSyntax ParseIndexExpression(ExpressionSyntax expr)
        {
            // Handle array index for expression
            while (tokens.PeekKind() == SyntaxTokenKind.LArraySymbol)
            {
                // Consume the array start
                SyntaxToken lArray = tokens.Consume();

                // Parse expression list
                SeparatedListSyntax<ExpressionSyntax> indexExpressions = ParseSeparatedSyntaxList(ParseExpression, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.RArraySymbol);

                // Expect array end
                if (tokens.ConsumeExpect(SyntaxTokenKind.RArraySymbol, out SyntaxToken rArray) == false)
                {
                    // Expected ']'
                    report.ReportMessage(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Source, SyntaxToken.GetText(SyntaxTokenKind.RArraySymbol));
                    return RecoverFromExpressionError();
                }

                // Apply indexing
                expr = new IndexExpressionSyntax(expr, lArray, indexExpressions, rArray);
            }
            return expr;
        }

        private ExpressionSyntax ParseTernaryExpression(ExpressionSyntax expr)
        {
            // Handle ternary
            while(tokens.PeekKind() == SyntaxTokenKind.TernarySymbol)
            {
                // Consume the token
                SyntaxToken ternary = tokens.Consume();

                // Parse expression
                ExpressionSyntax trueExpr = ParseExpression();

                // Require ternary alternate
                if(tokens.ConsumeExpect(SyntaxTokenKind.ColonSymbol, out SyntaxToken colon) == false)
                {
                    // Expected ':'
                    report.ReportMessage(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Source, SyntaxToken.GetText(SyntaxTokenKind.ColonSymbol));
                    return RecoverFromExpressionError();
                }

                // Parse other expression
                ExpressionSyntax falseExpr = ParseExpression();

                // Create ternary
                expr = new TernaryExpressionSyntax(expr, ternary, trueExpr, colon, falseExpr);
            }
            return expr;
        }

        private ExpressionSyntax ParseMemberAccessExpression(ExpressionSyntax expr)
        {
            // Parse member access chain
            while (tokens.PeekKind() == SyntaxTokenKind.DotSymbol)
            {
                // Consume the dot
                SyntaxToken dot = tokens.Consume();

                // Expect identifier
                if (tokens.ConsumeExpect(SyntaxTokenKind.Identifier, out SyntaxToken identifier) == false)
                {
                    // Expected identifier
                    report.ReportMessage(Code.ExpectedIdentifier, MessageSeverity.Error, tokens.Peek().Source);
                    return RecoverFromExpressionError();
                }

                // Create the member access expression
                MemberAccessExpressionSyntax accessExpression = new MemberAccessExpressionSyntax(expr, dot, identifier);

                // Check for generic arguments
                GenericArgumentListSyntax genericArguments = ParseGenericArgumentList();

                // Check for arguments
                ArgumentListSyntax arguments = ParseArgumentList();

                // Check for method
                if (genericArguments != null || arguments != null)
                {
                    // Argument list is required even if it is empty
                    if (arguments == null)
                    {
                        // Expected '('
                        report.ReportMessage(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Source, SyntaxToken.GetText(SyntaxTokenKind.LParenSymbol));
                        return RecoverFromExpressionError();
                    }

                    // Create the invoke expression
                    expr = new MethodInvokeExpressionSyntax(accessExpression, genericArguments, arguments);
                }
                // Must be a non-invokable member
                else
                {
                    // We can just update to the access expression here
                    expr = accessExpression;
                }

                // Finally we can parse array indexing on the member at this point
                // Calling once again after parsing the member allows us to support syntax like variable[0].member[1][2]
                expr = ParseIndexExpression(expr);
            }
            return expr;
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
            // Check for keyword literal
            else if(tokens.PeekKind() == SyntaxTokenKind.TrueKeyword || tokens.PeekKind() == SyntaxTokenKind.FalseKeyword || tokens.PeekKind() == SyntaxTokenKind.NullKeyword)
            {
                // Consume the token
                SyntaxToken keywordLiteral = tokens.Consume();

                // Create the literal
                return new LiteralExpressionSyntax(keywordLiteral);
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
