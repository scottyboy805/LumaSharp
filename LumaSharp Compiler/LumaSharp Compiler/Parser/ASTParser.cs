using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Parser
{
    internal sealed partial class ASTParser
    {
        // Private
        private readonly TokenView tokens;
        private readonly ICompileReportProvider report;

        // Constructor
        public ASTParser(IEnumerator<SyntaxToken> tokenProvider, ICompileReportProvider report)
        {
            // Check for null
            if (tokenProvider == null)
                throw new ArgumentNullException(nameof(tokenProvider));

            // Create token view
            this.tokens = new TokenView(tokenProvider);
            this.report = report;
        }

        // Methods
        internal SeparatedTokenList ParseNamespaceReference()
        {
            SeparatedTokenList namespaceName = null;

            // Expect identifier followed by colon
            if(tokens.PeekKind() == SyntaxTokenKind.Identifier && tokens.PeekKind(1) == SyntaxTokenKind.ColonSymbol)
            {
                // Consume the first tokens
                SyntaxToken identifierToken = tokens.Consume();
                SyntaxToken colonToken = tokens.Consume();

                // We have a valid namespace - create the list
                namespaceName = new(null, SyntaxTokenKind.ColonSymbol, SyntaxTokenKind.Identifier);

                // Add the first name group
                namespaceName.AddElement(identifierToken, colonToken);

                // Repeat for following namespace names - Note look ahead further this time because we should not consume the last colon
                while(tokens.PeekKind() == SyntaxTokenKind.Identifier && tokens.PeekKind(1) == SyntaxTokenKind.ColonSymbol)
                {
                    // Consume the last color
                    tokens.Consume();

                    // Consume the additional tokens
                    identifierToken = tokens.Consume();
                    colonToken = tokens.Consume();

                    // Add the additional name group
                    namespaceName.AddElement(identifierToken, colonToken);
                }
            }
            return namespaceName;
        }

        internal AttributeReferenceSyntax[] ParseAttributes()
        {
            // Parse an attribute
            AttributeReferenceSyntax attribute = ParseAttribute();

            // Create array
            AttributeReferenceSyntax[] attributes = null;

            // Check for not null
            while (attribute != null)
            {
                // Update the array
                if (attributes != null)
                {
                    Array.Resize(ref attributes, attributes.Length + 1);
                }
                else
                {
                    attributes = new AttributeReferenceSyntax[1];
                }

                // Update element
                attributes[attributes.Length - 1] = attribute;

                // Parse next
                attribute = ParseAttribute();
            }
            return attributes;
        }

        internal AttributeReferenceSyntax ParseAttribute()
        {
            // Check for hash
            if(tokens.PeekKind() == SyntaxTokenKind.HashSymbol)
            {
                // Get the hash token
                SyntaxToken hashToken = tokens.Consume();

                // Get the type reference
                TypeReferenceSyntax attributeTypeReference = ParseFullTypeReference();

                // Check for type
                if (attributeTypeReference == null)
                {
                    // Expected type reference
                    report.ReportDiagnostic(Code.ExpectedType, MessageSeverity.Error, tokens.Peek().Source);
                    return null; // Cannot recover from here - need to process each following token manually
                }

                // Get the argument list
                ArgumentListSyntax argumentList = ParseArgumentList();

                // Create attribute
                return new AttributeReferenceSyntax(hashToken, attributeTypeReference, argumentList);
            }
            return null;
        }

        internal ArrayParametersSyntax ParseArrayParameters()
        {
            // Check for open array
            if (tokens.PeekKind() == SyntaxTokenKind.LArraySymbol)
            {
                // Get the open array
                SyntaxToken lArray = tokens.Consume();

                // Parse separators
                SyntaxToken[] separators = null;

                // Check for separators
                while (tokens.PeekKind() == SyntaxTokenKind.CommaSymbol)
                {
                    // Update array
                    if (separators != null)
                    {
                        // resize existing
                        Array.Resize(ref separators, separators.Length + 1);
                    }
                    else
                    {
                        // Create new
                        separators = new SyntaxToken[1];
                    }

                    // Append token
                    separators[separators.Length - 1] = tokens.Consume();
                }

                // Expect comma or end array
                if (tokens.ConsumeExpect(SyntaxTokenKind.RArraySymbol, out SyntaxToken rArray) == false)
                {
                    // Expected ']'
                    report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Source, SyntaxToken.GetText(SyntaxTokenKind.RArraySymbol));
                    return RecoverFromArrayParameterError();
                }

                // Create array parameters
                return new ArrayParametersSyntax(lArray, separators, rArray);
            }
            return null;
        }

        internal GenericParameterListSyntax ParseGenericParameterList()
        {
            // Check for generic
            if(tokens.PeekKind() == SyntaxTokenKind.LessSymbol)
            {
                // Consume the generic
                SyntaxToken lGeneric = tokens.Consume();

                // Parse generic parameters
                SeparatedSyntaxList<GenericParameterSyntax> genericParameters = ParseSeparatedSyntaxList(ParseGenericParameter, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.GreaterSymbol);

                // Consume the r generic
                if (tokens.ConsumeExpect(SyntaxTokenKind.GreaterSymbol, out SyntaxToken rGeneric) == false)
                {
                    // Expected '>'
                    report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Source, SyntaxToken.GetText(SyntaxTokenKind.GreaterSymbol));
                    return RecoverFromGenericParameterListError();
                }

                // Get parameter list
                return new GenericParameterListSyntax(lGeneric, genericParameters, rGeneric);
            }
            return null;
        }

        internal GenericParameterSyntax ParseGenericParameter(int index)
        {
            // Require identifier
            if (tokens.ConsumeExpect(SyntaxTokenKind.Identifier, out SyntaxToken identifier) == false)
            {
                // Expected identifier
                report.ReportDiagnostic(Code.ExpectedIdentifier, MessageSeverity.Error, tokens.Peek().Source);
                return RecoverFromGenericParameterError();
            }

            SyntaxToken colonToken = default;
            SeparatedSyntaxList<TypeReferenceSyntax> genericConstraints = null;

            // Parse generic constraints
            if(tokens.PeekKind() == SyntaxTokenKind.ColonSymbol)
            {
                // Consume the token
                colonToken = tokens.Consume();

                // Expect generic constraint list
                genericConstraints = ParseSeparatedSyntaxList(ParseTypeReference, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.CommaSymbol);

                // Check for null
                if(genericConstraints == null)
                {
                    // Expected type
                    report.ReportDiagnostic(Code.ExpectedType, MessageSeverity.Error, tokens.Peek().Source);
                    return RecoverFromGenericParameterError();
                }

            }
            return new GenericParameterSyntax(identifier, index, colonToken, genericConstraints);
        }

        internal bool LooksLikeGenericArgumentList()
        {
            int initialPos = tokens.Position;
            int depth = 0;

            // Look ahead 16 tokens
            while(tokens.EOF == false && tokens.Position < initialPos + 16)
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
                            if(depth == 0)
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

        internal GenericArgumentListSyntax ParseGenericArgumentList()
        {
            // Check for generic
            if(tokens.PeekKind() == SyntaxTokenKind.LessSymbol)
            {
                // Consume the generic
                SyntaxToken lGeneric = tokens.Consume();

                // Parse generic type references
                SeparatedSyntaxList<TypeReferenceSyntax> genericTypeArguments = ParseSeparatedSyntaxList(ParseTypeReference, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.GreaterSymbol);

                // Expect r generic
                if (tokens.ConsumeExpect(SyntaxTokenKind.GreaterSymbol, out SyntaxToken rGeneric) == false)
                {
                    // Expected '>'
                    report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Source, SyntaxToken.GetText(SyntaxTokenKind.GreaterSymbol));
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
            if(tokens.PeekKind() == SyntaxTokenKind.LParenSymbol)
            {
                // Consume the lParen
                SyntaxToken lParen = tokens.Consume();

                // Parse the parameter list
                SeparatedSyntaxList<ParameterSyntax> parameters = ParseSeparatedSyntaxList(ParseParameter, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.RParenSymbol);

                // Expect rParen
                if (tokens.ConsumeExpect(SyntaxTokenKind.RParenSymbol, out SyntaxToken rParen) == false)
                {
                    // Expected ')'
                    report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Source, SyntaxToken.GetText(SyntaxTokenKind.RParenSymbol));
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
            AttributeReferenceSyntax[] attributes = ParseAttributes();

            // Parse type reference
            TypeReferenceSyntax parameterType = ParseTypeReference();

            // Check for null
            if (parameterType == null)
            {
                // Expected type
                report.ReportDiagnostic(Code.ExpectedType, MessageSeverity.Error, tokens.Peek().Source);
                return RecoverFromParameterError();
            }

            // Expect identifier
            if (tokens.ConsumeExpect(SyntaxTokenKind.Identifier, out SyntaxToken identifier) == false)
            {
                // Expected identifier
                report.ReportDiagnostic(Code.ExpectedIdentifier, MessageSeverity.Error, tokens.Peek().Source);
                return RecoverFromParameterError();
            }

            // Optional enumerable token
            SyntaxToken? enumerable = null;

            if(tokens.PeekKind() == SyntaxTokenKind.EnumerableSymbol)
                enumerable = tokens.Consume();

            // Optional assign
            VariableAssignExpressionSyntax assignment = null;

            // Create parameter
            return new ParameterSyntax(attributes, parameterType, identifier, assignment, enumerable);
        }

        internal ArgumentListSyntax ParseArgumentList()
        {
            // Check for arg start
            if(tokens.PeekKind() == SyntaxTokenKind.LParenSymbol)
            {
                // Consume the lParen
                SyntaxToken lParen = tokens.Consume();

                // Parse the arguments
                SeparatedSyntaxList<ExpressionSyntax> expressionArguments = ParseSeparatedSyntaxList(ParseExpression, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.RParenSymbol);

                // Expect rParen
                if (tokens.ConsumeExpect(SyntaxTokenKind.RParenSymbol, out SyntaxToken rParen) == false)
                {
                    // Expected ')'
                    report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Source, SyntaxToken.GetText(SyntaxTokenKind.RParenSymbol));
                    return RecoverFromArgumentListError();
                }

                // Create argument list
                return new ArgumentListSyntax(lParen, expressionArguments, rParen);
            }
            return null;
        }

        internal TypeReferenceSyntax ParseTypeReference()
        {
            // Check for primitive
            TypeReferenceSyntax typeReference = ParsePrimitiveTypeReference();

            // Check for null
            if (typeReference != null)
                return typeReference;

            // Check for identifier
            return ParseFullTypeReference();
        }

        internal TypeReferenceSyntax ParsePrimitiveTypeReference()
        {
            // Check for any primitive type
            SyntaxTokenKind kind = tokens.PeekKind();

            // Check for primitive token
            switch (kind)
            {
                case SyntaxTokenKind.AnyKeyword:
                case SyntaxTokenKind.BoolKeyword:
                case SyntaxTokenKind.CharKeyword:
                case SyntaxTokenKind.I8Keyword:
                case SyntaxTokenKind.U8Keyword:
                case SyntaxTokenKind.I16Keyword:
                case SyntaxTokenKind.U16Keyword:
                case SyntaxTokenKind.I32Keyword:
                case SyntaxTokenKind.U32Keyword:
                case SyntaxTokenKind.I64Keyword:
                case SyntaxTokenKind.U64Keyword:
                case SyntaxTokenKind.F32Keyword:
                case SyntaxTokenKind.F64Keyword:
                case SyntaxTokenKind.StringKeyword:
                case SyntaxTokenKind.VoidKeyword:
                    {
                        // Get the keyword token
                        SyntaxToken primitiveToken = tokens.Consume();

                        // Create reference
                        return new TypeReferenceSyntax(primitiveToken,
                            ParseArrayParameters());
                    }
            }
            return null;
        }

        internal TypeReferenceSyntax ParseFullTypeReference()
        {
            // Parse the namespace name first
            SeparatedTokenList namespaceName = ParseNamespaceReference();

            // Parse parent name?
            ParentTypeReferenceSyntax[] parentTypes = null;

            // Parse identifier
            if (tokens.ConsumeExpect(SyntaxTokenKind.Identifier, out SyntaxToken identifier) == false)
            {
                // Expected identifier
                report.ReportDiagnostic(Code.ExpectedIdentifier, MessageSeverity.Error, tokens.Peek().Source);
                return null;
            }

            // Parse the generic arguments
            GenericArgumentListSyntax genericArguments = ParseGenericArgumentList();

            // Parse array parameters
            ArrayParametersSyntax arrayParameters = ParseArrayParameters();

            // Create type reference
            return new TypeReferenceSyntax(namespaceName, parentTypes, identifier, genericArguments, arrayParameters);
        }

        internal SeparatedSyntaxList<T> ParseSeparatedSyntaxList<T>(Func<T> parseSyntaxElement, SyntaxTokenKind separatorKind, SyntaxTokenKind endTokenKind) where T : SyntaxNode
        {
            return ParseSeparatedSyntaxList((i) => parseSyntaxElement(), separatorKind, endTokenKind);
        }

        internal SeparatedSyntaxList<T> ParseSeparatedSyntaxList<T>(Func<int, T> parseSyntaxElement, SyntaxTokenKind separatorKind, SyntaxTokenKind endTokenKind) where T : SyntaxNode
        {
            // Check for end
            if (tokens.PeekKind() == endTokenKind)
                return null;

            // Store index
            int index = 0;

            // Parse first reference
            T syntaxReference = parseSyntaxElement(index++);

            // Check for valid
            if (syntaxReference != null)
            {
                // Get the separator
                SyntaxToken? separator = tokens.PeekKind() == separatorKind
                    ? tokens.Consume()
                    : (SyntaxToken?)null; // Nasty cast required otherwise separator is initialized to some weird corrupt value

                // Create the syntax list
                SeparatedSyntaxList<T> syntaxReferenceList = new(separatorKind);

                // Add the reference
                syntaxReferenceList.AddElement(syntaxReference, separator);

                // Repeat for all other elements
                while (separator != null && tokens.PeekKind() != endTokenKind && (syntaxReference = parseSyntaxElement(index++)) != null)
                {
                    // Get the separator
                    separator = tokens.PeekKind() == separatorKind
                        ? tokens.Consume()
                        : (SyntaxToken?)null;

                    // Add the additional reference
                    syntaxReferenceList.AddElement(syntaxReference, separator);
                }
                return syntaxReferenceList;
            }
            return null;
        }

        private ArrayParametersSyntax RecoverFromArrayParameterError()
        {
            // An error occurred while parsing a array parameters - To recover we should consume all tokens until we reach a rArray or end of stream
            while (tokens.EOF == false && tokens.PeekKind() != SyntaxTokenKind.GreaterSymbol)
                tokens.Consume();

            // Consume the rArray
            if (tokens.EOF == false)
                tokens.Consume();

            // Create error
            return new ArrayParametersSyntax(null);
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
