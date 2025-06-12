using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Parser
{
    internal sealed partial class ASTParser
    {
        // Methods
        internal SeparatedSyntaxList<T> ParseSeparatedSyntaxList<T>(Func<T> parseSyntaxElement, SyntaxTokenKind separatorKind, SyntaxTokenKind endTokenKind) where T : SyntaxNode
        {
            // Check for end
            if (tokens.PeekKind() == endTokenKind)
                return null;

            // Store list of syntax
            SeparatedSyntaxList<T> list = null;
            T syntax = null;

            // Check for value
            while ((syntax = parseSyntaxElement()) != null)
            {
                // Check for separator
                SyntaxToken? separator = tokens.PeekKind() == separatorKind
                    ? tokens.Consume()
                    : (SyntaxToken?)null;

                // Create list
                if (list == null)
                    list = new(separatorKind);

                // Add item
                list.AddElement(syntax, separator);

                // Check for end
                if (separator == null)
                    break;
            }

            //// Parse first reference
            //T syntaxReference = parseSyntaxElement();

            //// Check for valid
            //if (syntaxReference != null)
            //{
            //    // Get the separator
            //    SyntaxToken? separator = tokens.PeekKind() == separatorKind
            //        ? tokens.Consume()
            //        : (SyntaxToken?)null; // Nasty cast required otherwise separator is initialized to some weird corrupt value

            //    // Create the syntax list
            //    SeparatedSyntaxList<T> syntaxReferenceList = new(separatorKind);

            //    // Add the reference
            //    syntaxReferenceList.AddElement(syntaxReference, separator);

            //    // Repeat for all other elements
            //    while (separator != null && tokens.PeekKind() != endTokenKind && (syntaxReference = parseSyntaxElement()) != null)
            //    {
            //        // Get the separator
            //        separator = tokens.PeekKind() == separatorKind
            //            ? tokens.Consume()
            //            : (SyntaxToken?)null;

            //        // Add the additional reference
            //        syntaxReferenceList.AddElement(syntaxReference, separator);
            //    }
            //    return syntaxReferenceList;
            //}
            return list;
        }

        internal SeparatedTokenList ParseSeparatedTokenList(SyntaxTokenKind separatorKind, SyntaxTokenKind valueKind = SyntaxTokenKind.Identifier, bool requireTrailingSeparator = false)
        {
            SeparatedTokenList tokenList = null;

            // Check for trailing required
            if (requireTrailingSeparator == true && (tokens.PeekKind() != valueKind || tokens.PeekKind(1) != separatorKind))
                return null;

            // Check for value
            while(tokens.PeekKind() == valueKind && (requireTrailingSeparator == false || tokens.PeekKind(1) == separatorKind))
            {
                // Consume the 
                SyntaxToken value = tokens.Consume();

                // Check for separator
                SyntaxToken? separator = tokens.PeekKind() == separatorKind
                    ? tokens.Consume()
                    : (SyntaxToken?)null;

                // Create list
                if (tokenList == null)
                    tokenList = new(separatorKind, valueKind);

                // Add item
                tokenList.AddElement(value, separator);
            }

            // Check for trailing separator
            if(tokenList != null && requireTrailingSeparator == true && tokenList.HasTrailingSeparator == false)
            {
                // Expected separator
                report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Span, SyntaxToken.GetText(separatorKind));
            }

            // Expect identifier followed by colon
            //if (tokens.PeekKind() == valueKind && tokens.PeekKind(1) == separatorKind)
            //{
            //    // Consume the first tokens
            //    SyntaxToken valueToken = tokens.Consume();
            //    SyntaxToken separatorToken = tokens.Consume();

            //    // We have a valid namespace - create the list
            //    tokenList = new(separatorKind, valueKind);

            //    // Add the first name group
            //    tokenList.AddElement(valueToken, separatorToken);

            //    // Repeat for following tokens - Note look ahead further this time because we should not consume the last colon
            //    while (tokens.PeekKind() == valueKind && tokens.PeekKind(1) == separatorKind)
            //    {
            //        // Consume the last color
            //        tokens.Consume();

            //        // Consume the additional tokens
            //        valueToken = tokens.Consume();
            //        separatorToken = tokens.Consume();

            //        // Add the additional name group
            //        tokenList.AddElement(valueToken, separatorToken);
            //    }
            //}
            return tokenList;
        }

        internal GenericParameterListSyntax ParseGenericParameterList()
        {
            // Check for generic
            if (tokens.PeekKind() == SyntaxTokenKind.LessSymbol)
            {
                // Consume the generic
                SyntaxToken lGeneric = tokens.Consume();

                // Parse generic parameters
                SeparatedSyntaxList<GenericParameterSyntax> genericParameters = ParseSeparatedSyntaxList(ParseGenericParameter, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.GreaterSymbol);

                // Consume the r generic
                if (tokens.ConsumeExpect(SyntaxTokenKind.GreaterSymbol, out SyntaxToken rGeneric) == false)
                {
                    // Expected '>'
                    report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Span, SyntaxToken.GetText(SyntaxTokenKind.GreaterSymbol));
                    return RecoverFromGenericParameterListError();
                }

                // Get parameter list
                return new GenericParameterListSyntax(lGeneric, genericParameters, rGeneric);
            }
            return null;
        }

        internal GenericParameterSyntax ParseGenericParameter()
        {
            // Require identifier
            if (tokens.ConsumeExpect(SyntaxTokenKind.Identifier, out SyntaxToken identifier) == false)
            {
                // Expected identifier
                report.ReportDiagnostic(Code.ExpectedIdentifier, MessageSeverity.Error, tokens.Peek().Span);
                return RecoverFromGenericParameterError();
            }

            SyntaxToken colonToken = default;
            SeparatedSyntaxList<TypeReferenceSyntax> genericConstraints = null;

            // Parse generic constraints
            if (tokens.PeekKind() == SyntaxTokenKind.ColonSymbol)
            {
                // Consume the token
                colonToken = tokens.Consume();

                // Expect generic constraint list
                genericConstraints = ParseSeparatedSyntaxList(ParseTypeReference, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.CommaSymbol);

                // Check for null
                if (genericConstraints == null)
                {
                    // Expected type
                    report.ReportDiagnostic(Code.ExpectedType, MessageSeverity.Error, tokens.Peek().Span);
                    return RecoverFromGenericParameterError();
                }

            }
            return new GenericParameterSyntax(identifier, colonToken, genericConstraints);
        }

        internal GenericArgumentListSyntax ParseGenericArgumentList()
        {
            // Check for generic
            if (tokens.PeekKind() == SyntaxTokenKind.LessSymbol)
            {
                // Consume the generic
                SyntaxToken lGeneric = tokens.Consume();

                // Parse generic type references
                SeparatedSyntaxList<TypeReferenceSyntax> genericTypeArguments = ParseSeparatedSyntaxList(ParseTypeReference, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.GreaterSymbol);

                // Expect r generic
                if (tokens.ConsumeExpect(SyntaxTokenKind.GreaterSymbol, out SyntaxToken rGeneric) == false)
                {
                    // Expected '>'
                    report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Span, SyntaxToken.GetText(SyntaxTokenKind.GreaterSymbol));
                    return RecoverFromGenericArgumentListError();
                }

                // Create generic arguments
                return new GenericArgumentListSyntax(lGeneric, genericTypeArguments, rGeneric);
            }
            return null;
        }

        internal ParameterListSyntax ParseParameterList()
        {
            // Check for parameter start
            if (tokens.PeekKind() == SyntaxTokenKind.LParenSymbol)
            {
                // Consume the lParen
                SyntaxToken lParen = tokens.Consume();

                // Parse the parameter list
                SeparatedSyntaxList<ParameterSyntax> parameters = ParseSeparatedSyntaxList(ParseParameter, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.RParenSymbol);

                // Expect rParen
                if (tokens.ConsumeExpect(SyntaxTokenKind.RParenSymbol, out SyntaxToken rParen) == false)
                {
                    // Expected ')'
                    report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Span, SyntaxToken.GetText(SyntaxTokenKind.RParenSymbol));
                    return RecoverFromParameterListError();
                }

                // Create the parameter list
                return new ParameterListSyntax(lParen, parameters, rParen);
            }
            return null;
        }

        internal ParameterSyntax ParseParameter()
        {
            // Parse attributes
            AttributeSyntax[] attributes = ParseAttributes();

            // Parse type reference
            TypeReferenceSyntax parameterType = ParseTypeReference();

            // Check for null
            if (parameterType == null)
            {
                // Expected type
                report.ReportDiagnostic(Code.ExpectedType, MessageSeverity.Error, tokens.Peek().Span);
                return RecoverFromParameterError();
            }

            // Expect identifier
            if (tokens.ConsumeExpect(SyntaxTokenKind.Identifier, out SyntaxToken identifier) == false)
            {
                // Expected identifier
                report.ReportDiagnostic(Code.ExpectedIdentifier, MessageSeverity.Error, tokens.Peek().Span);
                return RecoverFromParameterError();
            }

            // Optional enumerable token
            SyntaxToken? enumerable = null;

            if (tokens.PeekKind() == SyntaxTokenKind.EnumerableSymbol)
                enumerable = tokens.Consume();

            // Optional assign
            VariableAssignmentExpressionSyntax assignment = null;

            // Create parameter
            return new ParameterSyntax(attributes, parameterType, identifier, assignment, enumerable);
        }

        internal ArgumentListSyntax ParseArgumentList()
        {
            // Check for arg start
            if (tokens.PeekKind() == SyntaxTokenKind.LParenSymbol)
            {
                // Consume the lParen
                SyntaxToken lParen = tokens.Consume();

                // Parse the arguments
                SeparatedSyntaxList<ExpressionSyntax> expressionArguments = ParseSeparatedSyntaxList(ParseExpression, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.RParenSymbol);

                // Expect rParen
                if (tokens.ConsumeExpect(SyntaxTokenKind.RParenSymbol, out SyntaxToken rParen) == false)
                {
                    // Expected ')'
                    report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Span, SyntaxToken.GetText(SyntaxTokenKind.RParenSymbol));
                    return RecoverFromArgumentListError();
                }

                // Create argument list
                return new ArgumentListSyntax(lParen, expressionArguments, rParen);
            }
            return null;
        }

        private bool LooksLikeGenericArgumentList()
        {
            int initialPos = tokens.Position;
            int depth = 0;

            // Look ahead 16 tokens
            while (tokens.EOF == false && tokens.Position < initialPos + 16)
            {
                // Get the next token
                SyntaxToken token = tokens.Consume();

                // Check kind
                switch (token.Kind)
                {
                    case SyntaxTokenKind.LessSymbol:
                        {
                            depth++;
                            break;
                        }
                    case SyntaxTokenKind.GreaterSymbol:
                        {
                            depth--;

                            // Check for balanced
                            if (depth == 0)
                            {
                                // Check for common tokens following generic arguments closing
                                bool isLikelyGeneric = (tokens.PeekKind() == SyntaxTokenKind.DotSymbol || tokens.PeekKind() == SyntaxTokenKind.LParenSymbol || tokens.PeekKind() == SyntaxTokenKind.Identifier);

                                // Retrace and then return
                                tokens.Retrace(initialPos);
                                return isLikelyGeneric;
                            }
                            break;
                        }
                    case SyntaxTokenKind.CommaSymbol:
                        {
                            continue;
                        }

                    default:
                        {
                            // Check for binary or unary operator
                            if (token.IsBinaryOperand == true || token.IsUnaryOperand == true || token.IsLiteral == true)
                            {
                                tokens.Retrace(initialPos);
                                return false;
                            }
                            break;
                        }
                }
            }

            tokens.Retrace(initialPos);
            return false;
        }

        private GenericParameterListSyntax RecoverFromGenericParameterListError()
        {
            // An error occurred while parsing a generic parameter list - To recover we should consume all tokens until we reach a rgeneric or end of stream
            while (tokens.EOF == false && tokens.PeekKind() != SyntaxTokenKind.GreaterSymbol)
                tokens.Consume();

            // Consume the rParen
            if (tokens.EOF == false)
                tokens.Consume();

            // Always null - just means we can return from calling control easily
            return new GenericParameterListSyntax(null);
        }

        private GenericParameterSyntax RecoverFromGenericParameterError()
        {
            // An error occurred while parsing a generic parameter - To recover we should consume all tokens until we reach a comma, closing generic or end of stream
            while (tokens.EOF == false && tokens.PeekKind() != SyntaxTokenKind.CommaSymbol && tokens.PeekKind() != SyntaxTokenKind.GreaterSymbol)
                tokens.Consume();

            // Always null - just means we can return from calling control easily
            return GenericParameterSyntax.Error;
        }

        private ParameterListSyntax RecoverFromParameterListError()
        {
            // An error occurred while parsing a parameter list - To recover we should consume all tokens until we reach a rParen or end of stream
            while (tokens.EOF == false && tokens.PeekKind() != SyntaxTokenKind.RParenSymbol)
                tokens.Consume();

            // Consume the rParen
            if (tokens.EOF == false)
                tokens.Consume();

            // Always null - just means we can return from calling control easily
            return new ParameterListSyntax(null);
        }

        private ParameterSyntax RecoverFromParameterError()
        {
            // An error occurred while parsing a parameter list - To recover we should consume all tokens until we reach a comma, rParen or end of stream
            while (tokens.EOF == false && tokens.PeekKind() != SyntaxTokenKind.CommaSymbol && tokens.PeekKind() != SyntaxTokenKind.RParenSymbol)
                tokens.Consume();

            // Always null - just means we can return from calling control easily
            return ParameterSyntax.Error;
        }

        private GenericArgumentListSyntax RecoverFromGenericArgumentListError()
        {
            // An error occurred while parsing a generic argument list - To recover we should consume all tokens until we reach a rGeneric or end of stream
            while (tokens.EOF == false && tokens.PeekKind() != SyntaxTokenKind.GreaterSymbol)
                tokens.Consume();

            // Consume the rParen
            if (tokens.EOF == false)
                tokens.Consume();

            // Always null - just means we can return from calling control easily
            return new GenericArgumentListSyntax(null);
        }

        private ArgumentListSyntax RecoverFromArgumentListError()
        {
            // An error occurred while parsing a argument list - To recover we should consume all tokens until we reach a rParen or end of stream
            while (tokens.EOF == false && tokens.PeekKind() != SyntaxTokenKind.RParenSymbol)
                tokens.Consume();

            // Consume the rParen
            if (tokens.EOF == false)
                tokens.Consume();

            // Always null - just means we can return from calling control easily
            return new ArgumentListSyntax(null);
        }
    }
}
