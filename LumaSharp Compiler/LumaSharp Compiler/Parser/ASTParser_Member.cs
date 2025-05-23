using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Parser
{
    internal sealed partial class ASTParser
    {
        // Methods
        internal MemberBlockSyntax ParseMemberBlock()
        {
            // Expect opening
            if(tokens.PeekKind() == SyntaxTokenKind.LBlockSymbol)
            {
                // Consume block
                SyntaxToken lBlock = tokens.Consume();

                MemberSyntax[] members = null;

                // Parse all members
                MemberSyntax member = ParseMember();

                // Check for valid
                while(member != null)
                {
                    if(members != null)
                    {
                        Array.Resize(ref members, members.Length + 1);
                    }
                    else
                    {
                        members = new MemberSyntax[1];
                    }

                    // Add to members
                    members[members.Length - 1] = member;

                    // Try to parse next member
                    member = ParseMember();
                }

                // Expect closing
                if (tokens.ConsumeExpect(SyntaxTokenKind.RBlockSymbol, out SyntaxToken rBlock) == false)
                {
                    // Expected '}'
                    report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Source, SyntaxToken.GetText(SyntaxTokenKind.RBlockSymbol));
                    return null;
                }

                // Create block
                return new MemberBlockSyntax(lBlock, members, rBlock);
            }
            return null;
        }

        internal EnumBlockSyntax ParseEnumMemberBlock()
        {
            // Expect opening
            if (tokens.PeekKind() == SyntaxTokenKind.LBlockSymbol)
            {
                // Consume block
                SyntaxToken lBlock = tokens.Consume();

                // Parse separated list
                SeparatedSyntaxList<EnumFieldSyntax> enumFields = ParseSeparatedSyntaxList(ParseEnumFieldDeclaration, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.RBlockSymbol);

                // Expect closing
                if (tokens.ConsumeExpect(SyntaxTokenKind.RBlockSymbol, out SyntaxToken rBlock) == false)
                {
                    // Expected '}'
                    report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Source, SyntaxToken.GetText(SyntaxTokenKind.RBlockSymbol));
                    return null;
                }

                // Create block
                return new EnumBlockSyntax(lBlock, enumFields, rBlock);
            }
            return null;
        }

        internal MemberSyntax ParseRootMember()
        {
            // Parse member
            MemberSyntax member = null;

            // Parse type
            if (member == null)
                member = ParseTypeDeclaration();

            // Parse contract
            if (member == null)
                member = ParseContractDeclaration();

            // Parse enum
            if(member == null)
                member = ParseEnumDeclaration();

            return member;
        }

        internal MemberSyntax ParseMember()
        {
            // Any root can be nested
            MemberSyntax member = ParseRootMember();

            // Parse constructor
            if(member == null)
                member = ParseConstructorDeclaration();
                        
            // Parse method
            if (member == null)
                member = ParseMethodDeclaration();

            // Parse accessor
            if (member == null)
                member = ParseAccessorDeclaration();

            // Parse field
            if (member == null)
                member = ParseFieldDeclaration();

            return member;
        }

        internal TypeSyntax ParseTypeDeclaration()
        {
            // Store position
            int position = tokens.Position;

            // Parse attributes
            AttributeReferenceSyntax[] attributes = ParseAttributes();

            // Parse access modifiers
            SyntaxToken[] modifiers = ParseAccessModifiers();

            // Check for type
            if (tokens.PeekKind() == SyntaxTokenKind.TypeKeyword)
            {
                // Consume keyword
                SyntaxToken typeToken = tokens.Consume();

                // Parse identifier
                if (tokens.ConsumeExpect(SyntaxTokenKind.Identifier, out SyntaxToken identifier) == false)
                {
                    // Expected identifier
                    report.ReportDiagnostic(Code.ExpectedIdentifier, MessageSeverity.Error, tokens.Peek().Source);
                }

                // Parse generic parameters
                GenericParameterListSyntax genericParameters = ParseGenericParameterList();

                // Override keyword
                SyntaxToken? overrideToken = null;

                if(tokens.PeekKind() == SyntaxTokenKind.OverrideKeyword)
                    overrideToken = tokens.Consume();

                // Base types
                BaseTypeListSyntax baseTypes = ParseBaseTypes();

                // Parse members
                MemberBlockSyntax members = ParseMemberBlock();

                // Create type declaration
                return new TypeSyntax(identifier, attributes, modifiers, genericParameters, overrideToken, baseTypes, members);
            }

            // Retrace to original position because we are not working with a type
            tokens.Retrace(position);
            return null;
        }

        internal ContractSyntax ParseContractDeclaration()
        {
            // Store position
            int position = tokens.Position;

            // Parse attributes
            AttributeReferenceSyntax[] attributes = ParseAttributes();

            // Parse access modifiers
            SyntaxToken[] modifiers = ParseAccessModifiers();

            // Check for contract
            if (tokens.PeekKind() == SyntaxTokenKind.ContractKeyword)
            {
                // Consume keyword
                SyntaxToken contractToken = tokens.Consume();

                // Parse identifier
                if (tokens.ConsumeExpect(SyntaxTokenKind.Identifier, out SyntaxToken identifier) == false)
                {
                    // Expected identifier
                    report.ReportDiagnostic(Code.ExpectedIdentifier, MessageSeverity.Error, tokens.Peek().Source);
                }

                // Parse generic parameters
                GenericParameterListSyntax genericParameters = ParseGenericParameterList();

                // Base types
                BaseTypeListSyntax baseTypes = ParseBaseTypes();

                // Parse members
                MemberBlockSyntax members = ParseMemberBlock();

                // Create contract declaration
                return new ContractSyntax(identifier, attributes, modifiers, genericParameters, baseTypes, members);
            }

            // Retrace to original position because we are not working with a type
            tokens.Retrace(position);
            return null;
        }

        internal EnumSyntax ParseEnumDeclaration()
        {
            // Store position
            int position = tokens.Position;

            // Parse attributes
            AttributeReferenceSyntax[] attributes = ParseAttributes();

            // Parse access modifiers
            SyntaxToken[] modifiers = ParseAccessModifiers();

            // Check for enum
            if (tokens.PeekKind() == SyntaxTokenKind.EnumKeyword)
            {
                // Consume keyword
                SyntaxToken contractToken = tokens.Consume();

                // Parse identifier
                if (tokens.ConsumeExpect(SyntaxTokenKind.Identifier, out SyntaxToken identifier) == false)
                {
                    // Expected identifier
                    report.ReportDiagnostic(Code.ExpectedIdentifier, MessageSeverity.Error, tokens.Peek().Source);
                }

                // Base types
                BaseTypeListSyntax baseTypes = ParseBaseTypes();

                // Parse members
                EnumBlockSyntax members = ParseEnumMemberBlock();

                // Create enum declaration
                return new EnumSyntax(identifier, attributes, modifiers, baseTypes, members);
            }

            // Retrace to original position because we are not working with a type
            tokens.Retrace(position);
            return null;
        }

        internal FieldSyntax ParseFieldDeclaration()
        {
            // Store current position
            int initialPosition = tokens.Position;

            // Parse optional attributes
            AttributeReferenceSyntax[] attributes = ParseAttributes();

            // Parse access modifiers
            SyntaxToken[] modifiers = ParseAccessModifiers();

            // Parse the field type
            TypeReferenceSyntax fieldType = ParseTypeReference();

            // Require that field type is valid at this point
            if(fieldType != null)
            {
                // Expect identifier next for the field name
                if(tokens.ConsumeExpect(SyntaxTokenKind.Identifier, out SyntaxToken identifier) == false)
                {
                    // Expected identifier
                    report.ReportDiagnostic(Code.ExpectedIdentifier, MessageSeverity.Error, tokens.Peek().Source);
                    return RecoverFromFieldError();
                }

                // Parse optional assignment
                VariableAssignmentExpressionSyntax assignExpression = ParseVariableAssignExpression();

                // Expect semicolon
                if(tokens.ConsumeExpect(SyntaxTokenKind.SemicolonSymbol, out SyntaxToken semicolon) == false)
                {
                    // Expected ';'
                    report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Source, SyntaxToken.GetText(SyntaxTokenKind.SemicolonSymbol));
                    return RecoverFromFieldError();
                }

                // Create the field
                return new FieldSyntax(identifier, attributes, modifiers, fieldType, assignExpression);
            }

            // Retrace to starting position
            tokens.Retrace(initialPosition);
            return null;
        }

        internal EnumFieldSyntax ParseEnumFieldDeclaration()
        {
            // Parse identifier
            if(tokens.ConsumeExpect(SyntaxTokenKind.Identifier, out SyntaxToken identifier) == false)
            {
                report.ReportDiagnostic(Code.ExpectedIdentifier, MessageSeverity.Error, tokens.Peek().Source);
                return RecoverFromEnumFieldError();
            }

            // Parse optional assign
            VariableAssignmentExpressionSyntax assignExpression = ParseVariableAssignExpression();

            // Create the enum field
            return new EnumFieldSyntax(identifier, assignExpression);
        }

        internal AccessorSyntax ParseAccessorDeclaration()
        {
            // Store current position
            int initialPosition = tokens.Position;

            // Parse optional attributes
            AttributeReferenceSyntax[] attributes = ParseAttributes();

            // Parse access modifiers
            SyntaxToken[] modifiers = ParseAccessModifiers();

            // Parse the field type
            TypeReferenceSyntax accessorType = ParseTypeReference();

            // Require that field type is valid at this point
            if (accessorType != null)
            {
                // Expect identifier next for the field name
                if (tokens.ConsumeExpect(SyntaxTokenKind.Identifier, out SyntaxToken identifier) == false)
                {
                    return null;
                    //// Expected identifier
                    //report.ReportDiagnostic(Code.ExpectedIdentifier, MessageSeverity.Error, tokens.Peek().Source);
                    //return RecoverFromFieldError();
                }

                // Optional override
                SyntaxToken? overrideToken = null;

                if (tokens.PeekKind() == SyntaxTokenKind.OverrideKeyword)
                    overrideToken = tokens.Consume();

                // Expect lambda - Only now we can be sure that we are dealing with an accessor
                if (tokens.PeekKind() == SyntaxTokenKind.LambdaSymbol)
                {
                    // Now we know we are definitely dealing with an accessor
                    AccessorBodySyntax[] bodies = ParseAccessorBodies();
                    AccessorLambdaSyntax lambda = null;

                    // Check for lambda
                    if (bodies == null)
                        lambda = ParseAccessorLambda();

                    // Create the field
                    return new AccessorSyntax(identifier, attributes, modifiers, accessorType, overrideToken, bodies, lambda);
                }
            }

            // Retrace to starting position
            tokens.Retrace(initialPosition);
            return null;
        }

        internal AccessorBodySyntax[] ParseAccessorBodies()
        {
            // Create array
            AccessorBodySyntax[] bodies = null;

            // Check for lambda followed by read or write
            while(tokens.PeekKind() == SyntaxTokenKind.LambdaSymbol && (tokens.PeekKind(1) == SyntaxTokenKind.ReadKeyword || tokens.PeekKind(1) == SyntaxTokenKind.WriteKeyword))
            {
                // Consume lambda
                SyntaxToken lambda = tokens.Consume();

                // Read keyword
                SyntaxToken keyword = tokens.Consume();

                // Expect colon separator
                if(tokens.ConsumeExpect(SyntaxTokenKind.ColonSymbol, out SyntaxToken colon) == false)
                {
                    // Expected ':'
                    report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Source, SyntaxToken.GetText(SyntaxTokenKind.ColonSymbol));
                    RecoverFromAccessorBodyError();
                    continue;
                }

                // Parse the statement
                StatementSyntax statement = ParseStatement();

                // Check for statement
                if(statement == null)
                {
                    // Expected statement
                    report.ReportDiagnostic(Code.ExpectedStatement, MessageSeverity.Error, tokens.Peek().Source);
                    RecoverFromAccessorBodyError();
                    continue;
                }

                // Create the array
                if(bodies != null)
                {
                    Array.Resize(ref bodies, bodies.Length + 1);
                }
                else
                {
                    bodies = new AccessorBodySyntax[1];
                }

                // Add body
                bodies[bodies.Length - 1] = new AccessorBodySyntax(lambda, keyword, colon, statement);
            }
            return bodies;
        }

        internal AccessorLambdaSyntax ParseAccessorLambda()
        {
            // Check for lambda
            if(tokens.PeekKind() == SyntaxTokenKind.LambdaSymbol)
            {
                // Consume the token
                SyntaxToken lambda = tokens.Consume();

                // Parse expression
                ExpressionSyntax expression = ParseExpression();

                // Expect expression
                if(expression == null)
                {
                    // Expected expression
                    report.ReportDiagnostic(Code.ExpectedExpression, MessageSeverity.Error, tokens.Peek().Source);
                    return RecoverFromAccessorLambdaError();
                }

                // Expect semicolon
                if(tokens.ConsumeExpect(SyntaxTokenKind.SemicolonSymbol, out SyntaxToken semicolon) == false)
                {
                    // Expected ';'
                    report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Source, SyntaxToken.GetText(SyntaxTokenKind.SemicolonSymbol));
                    return RecoverFromAccessorLambdaError();
                }

                // Create lambda
                return new AccessorLambdaSyntax(lambda, expression, semicolon);
            }
            return null;
        }

        internal ConstructorSyntax ParseConstructorDeclaration()
        {
            // Store current position
            int initialPosition = tokens.Position;

            // Parse optional attributes
            AttributeReferenceSyntax[] attributes = ParseAttributes();

            // Parse access modifiers
            SyntaxToken[] modifiers = ParseAccessModifiers();

            // Expect this keyword
            if(tokens.PeekKind() == SyntaxTokenKind.ThisKeyword)
            {
                // Consume the token
                SyntaxToken thisKeyword = tokens.Consume();

                // Parse parameters
                ParameterListSyntax parameters = ParseParameterList();

                // Parse optional constructor invoke
                ConstructorInvokeSyntax constructorInvoke = ParseConstructorInvoke();

                // Now parse the body or lambda
                LambdaStatementSyntax lambda = ParseLambdaStatement();
                StatementBlockSyntax body = null;

                // Check for lambda parsed
                if (lambda != null)
                {
                    // Check for following lBlock
                    if (tokens.PeekKind() == SyntaxTokenKind.LBlockSymbol)
                    {
                        // Expected member??
                    }
                }
                else
                {
                    // Parse the statement
                    body = ParseBlockStatement();
                }

                // Create constructor
                return new ConstructorSyntax(thisKeyword, attributes, modifiers, parameters, constructorInvoke, body, lambda);
            }
            tokens.Retrace(initialPosition);
            return null;
        }

        private ConstructorInvokeSyntax ParseConstructorInvoke()
        {
            // Expect colon
            if(tokens.PeekKind() == SyntaxTokenKind.ColonSymbol)
            {
                // Consume the colon
                SyntaxToken colon = tokens.Consume();

                // Expect base or this
                if(tokens.PeekKind() != SyntaxTokenKind.BaseKeyword && tokens.PeekKind() != SyntaxTokenKind.ThisKeyword)
                {
                    // Expected base or this
                    report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Source, SyntaxToken.GetText(SyntaxTokenKind.BaseKeyword) + " or " + SyntaxToken.GetText(SyntaxTokenKind.ThisKeyword));
                    return null; // TODO - recover
                }

                // Consume the token
                SyntaxToken baseOrThisKeyword = tokens.Consume();

                // Parse arguments
                ArgumentListSyntax arguments = ParseArgumentList();

                // Create invoke
                return new ConstructorInvokeSyntax(colon, baseOrThisKeyword, arguments);
            }
            return null;
        }

        internal MethodSyntax ParseMethodDeclaration()
        {
            // Store current position
            int initialPosition = tokens.Position;

            // Parse optional attributes
            AttributeReferenceSyntax[] attributes = ParseAttributes();

            // Parse access modifiers
            SyntaxToken[] modifiers = ParseAccessModifiers();

            // Parse method return values
            SeparatedSyntaxList<TypeReferenceSyntax> returnTypes = ParseSeparatedSyntaxList(ParseTypeReference, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Invalid);

            // Check for return types available
            if(returnTypes != null)
            {
                // Expect identifier at this point
                if(tokens.ConsumeExpect(SyntaxTokenKind.Identifier, out SyntaxToken identifier) == true)
                {
                    // Optional generic parameters
                    GenericParameterListSyntax genericParameters = ParseGenericParameterList();

                    // Now require arg list if we are dealing with a method
                    if (tokens.PeekKind() == SyntaxTokenKind.LParenSymbol)
                    {
                        // Now we are certain that we are parsing a method
                        ParameterListSyntax parameters = ParseParameterList();

                        // Optional override
                        SyntaxToken? overrideKeyword = null;

                        if (tokens.PeekKind() == SyntaxTokenKind.OverrideKeyword)
                            overrideKeyword = tokens.Consume();

                        // Now parse the body or lambda
                        LambdaStatementSyntax lambda = ParseLambdaStatement();
                        StatementBlockSyntax body = null;

                        // Check for lambda parsed
                        if (lambda != null)
                        {
                            // Check for following lBlock
                            if(tokens.PeekKind() == SyntaxTokenKind.LBlockSymbol)
                            {
                                // Expected member??
                            }
                        }
                        else
                        {
                            // Parse the statement
                            body = ParseBlockStatement();
                        }

                        // Build the method
                        return new MethodSyntax(identifier, attributes, modifiers, returnTypes, genericParameters, parameters, overrideKeyword, body, lambda);
                    }
                }
            }

            // Retrace to starting position
            tokens.Retrace(initialPosition);
            return null;
        }        

        internal SyntaxToken[] ParseAccessModifiers()
        {
            // Check for modifier
            SyntaxToken modifierToken = tokens.Peek();

            // Create modifiers
            SyntaxToken[] modifierList = null;

            // Check for modifier
            while(modifierToken.IsAccessModifier == true)
            {
                // Consume the token
                tokens.Consume();

                // Create array
                if(modifierList != null)
                {
                    Array.Resize(ref modifierList, modifierList.Length + 1);
                }
                else
                {
                    modifierList = new SyntaxToken[1];
                }

                // Add the modifier
                modifierList[modifierList.Length - 1] = modifierToken;

                // Check next token
                modifierToken = tokens.Peek();
            }
            return modifierList;
        }

        internal BaseTypeListSyntax ParseBaseTypes()
        {
            // Check for colon
            if(tokens.PeekKind() == SyntaxTokenKind.ColonSymbol)
            {
                // Consume token
                SyntaxToken colon = tokens.Consume();

                // Parse type references
                SeparatedSyntaxList<TypeReferenceSyntax> baseTypes = ParseSeparatedSyntaxList(ParseTypeReference, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.LBlockSymbol);

                // Create base types
                return new BaseTypeListSyntax(colon, baseTypes);
            }
            return null;
        }

        private FieldSyntax RecoverFromFieldError()
        {
            // An error occurred while parsing a field - To recover we should consume all tokens until we reach a semicolon or end of stream
            while (tokens.EOF == false && tokens.PeekKind() != SyntaxTokenKind.SemicolonSymbol)
                tokens.Consume();

            // Consume the semicolon so we can parse the next member
            if (tokens.EOF == false)
                tokens.Consume();

            // Always null - just means we can return from calling control easily
            return null;
        }

        private EnumFieldSyntax RecoverFromEnumFieldError()
        {
            // An error occurred while parsing an enum field - To recover we should consume all tokens until we reach a comma, closing block or end of stream
            while (tokens.EOF == false && tokens.PeekKind() != SyntaxTokenKind.CommaSymbol && tokens.PeekKind() != SyntaxTokenKind.RBlockSymbol)
                tokens.Consume();

            // Always null - just means we can return from calling control easily
            return null;
        }

        private AccessorBodySyntax RecoverFromAccessorBodyError()
        {
            // An error occurred while parsing a body - To recover we should consume all tokens until we reach a semicolon or rBlock or end of stream
            while (tokens.EOF == false && tokens.PeekKind() != SyntaxTokenKind.SemicolonSymbol && tokens.PeekKind() != SyntaxTokenKind.RBlockSymbol)
                tokens.Consume();

            // Consume the semicolon or RBlock so we can parse the next member
            if (tokens.EOF == false)
                tokens.Consume();

            // Always null - just means we can return from calling control easily
            return null;
        }

        private AccessorLambdaSyntax RecoverFromAccessorLambdaError()
        {
            // An error occurred while parsing a lambda - To recover we should consume all tokens until we reach a semicolon or end of stream
            while (tokens.EOF == false && tokens.PeekKind() != SyntaxTokenKind.SemicolonSymbol)
                tokens.Consume();

            // Consume the semicolon so we can parse the next member
            if (tokens.EOF == false)
                tokens.Consume();

            // Always null - just means we can return from calling control easily
            return null;
        }
    }
}
