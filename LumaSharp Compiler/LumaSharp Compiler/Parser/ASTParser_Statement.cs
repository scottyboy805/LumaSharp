using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Parser
{
    internal sealed partial class ASTParser
    {
        // Methods
        public StatementSyntax ParseStatement()
        {
            StatementSyntax statement = null;

            // Check for empty
            if (statement == null)
                statement = ParseEmptyStatement();

            // Check for block
            if (statement == null)
                statement = ParseBlockStatement();

            // General statements
            // ### Clearly defined rules - begin with an explicit keyword or symbol that uniquely identifies the expression syntax
            {
                // Check for break statement
                if (statement == null)
                    statement = ParseBreakStatement();

                // Check for continue statement
                if(statement == null)
                    statement = ParseContinueStatement();

                // Check for condition
                if (statement == null)
                    statement = ParseIfStatement();

                // Check for return
                if (statement == null)
                    statement = ParseReturnStatement();

                // Check for foreach
                if (statement == null)
                    statement = ParseForeachStatement();

                // Check for for
                if (statement == null)
                    statement = ParseForStatement();

                // Check for while
                if (statement == null)
                    statement = ParseWhileStatement();
            }

            // Check for assignment
            if (statement == null)
                statement = ParseAssignStatement();

            // Check for variable declaration
            if(statement == null)
                statement = ParseVariableDeclarationStatement();

            return statement;
        }

        internal StatementSyntax ParseEmptyStatement()
        {
            // Check for semicolon
            if(tokens.PeekKind() == SyntaxTokenKind.SemicolonSymbol)
            {
                // Consume the token
                SyntaxToken semicolon = tokens.Consume();

                // Create the empty statement
                return new EmptyStatementSyntax(semicolon);
            }
            return null;
        }

        internal StatementBlockSyntax ParseBlockStatement()
        {
            // Check for block
            if(tokens.PeekKind() == SyntaxTokenKind.LBlockSymbol)
            {
                // Consume the token
                SyntaxToken lBlock = tokens.Consume();

                // Get statements
                List<StatementSyntax> statements = null;

                // Repeat until closing block
                while(tokens.PeekKind() != SyntaxTokenKind.RBlockSymbol)
                {
                    // Try to parse a statement
                    StatementSyntax statement = ParseStatement();

                    // Check for error
                    if(statement == null)
                    {
                        // Expected statement
                        report.Report(Code.ExpectedStatement, DiagnosticSeverity.Error, tokens.Peek().Span);
                        return RecoverFromStatementBlockError();
                    }

                    // Create list on demand
                    if (statements == null)
                        statements = new();

                    // Push the statement
                    statements.Add(statement);
                }

                // Expect closing block
                SyntaxToken rBlock = tokens.Consume();

                // Create block
                return new StatementBlockSyntax(lBlock, statements, rBlock);
            }
            return null;
        }

        internal StatementSyntax ParseIfStatement()
        {
            // Check for if keyword
            if(tokens.PeekKind() == SyntaxTokenKind.IfKeyword)
            {
                // Consume the token
                SyntaxToken ifToken = tokens.Consume();

                // Expect opening brace
                if (tokens.ConsumeExpect(SyntaxTokenKind.LParenSymbol, out SyntaxToken lParen) == false)
                {
                    // Expected '('
                    report.Report(Code.ExpectedToken, DiagnosticSeverity.Error, tokens.Peek().Span, SyntaxToken.GetText(SyntaxTokenKind.LParenSymbol));
                    return RecoverFromStatementError();
                }

                // Parse the condition expression
                ExpressionSyntax conditionExpression = ParseExpression();

                // Check for condition
                if(conditionExpression == null)
                {
                    // Expected expression
                    report.Report(Code.ExpectedExpression, DiagnosticSeverity.Error, tokens.Peek().Span);
                    return RecoverFromStatementError();
                }

                // Expect closing brace
                if (tokens.ConsumeExpect(SyntaxTokenKind.RParenSymbol, out SyntaxToken rParen) == false)
                {
                    // Expected ')'
                    report.Report(Code.ExpectedToken, DiagnosticSeverity.Error, tokens.Peek().Span, SyntaxToken.GetText(SyntaxTokenKind.RParenSymbol));
                    return RecoverFromStatementError();
                }

                // Parse the statement
                StatementSyntax statement = ParseStatement();

                // Check for statement
                if(statement == null)
                {
                    // Expected statement
                    report.Report(Code.ExpectedStatement, DiagnosticSeverity.Error, tokens.Peek().Span);
                    return RecoverFromStatementError();
                }

                // Parse optional alternate recursively
                ElseClauseSyntax alternate = ParseElseClause();

                // Create if
                return new IfStatementSyntax(ifToken, lParen, conditionExpression, rParen, statement, alternate);
            }
            return null;
        }

        private ElseClauseSyntax ParseElseClause()
        {
            // Check for else
            if(tokens.PeekKind() == SyntaxTokenKind.ElseKeyword)
            {
                // Consume token
                SyntaxToken elseToken = tokens.Consume();

                // Must be just an else clause
                // Parse the statement
                StatementSyntax statement = ParseStatement();

                // Check for statement
                if (statement == null)
                {
                    // Expected statement
                    report.Report(Code.ExpectedStatement, DiagnosticSeverity.Error, tokens.Peek().Span);
                    RecoverFromStatementError();
                    return null;                    
                }

                // Create the alternate condition
                return new ElseClauseSyntax(elseToken, statement);
            }
            return null;
        }

        internal StatementSyntax ParseForeachStatement()
        {
            // Store current position
            int initialPosition = tokens.Position;

            // Check for
            if(tokens.PeekKind() == SyntaxTokenKind.ForKeyword)
            {
                // Consume token
                SyntaxToken forToken = tokens.Consume();

                // Parse optional type reference
                TypeReferenceSyntax typeReference = null;

                // Parse identifier - Type is implicit??
                if(tokens.PeekKind() == SyntaxTokenKind.Identifier && tokens.PeekKind(1) == SyntaxTokenKind.InKeyword)
                {
                    // Now we know that we are dealing with a foreach loop for certain
                    SyntaxToken identifier = tokens.Consume();

                    // Get in keyword
                    SyntaxToken inToken = tokens.Consume();

                    // Get expression
                    ExpressionSyntax expression = ParseExpression();

                    // Parse the statement
                    StatementSyntax statement = ParseStatement();

                    // Create the foreach syntax
                    return new ForeachStatementSyntax(forToken, typeReference, identifier, inToken, expression, statement);
                }
                // Parse with explicit type
                else if((typeReference = ParseTypeReference()) != null && tokens.PeekKind() == SyntaxTokenKind.Identifier && tokens.PeekKind(1) == SyntaxTokenKind.InKeyword)
                {
                    // Now we know that we are dealing with a foreach loop for certain
                    SyntaxToken identifier = tokens.Consume();

                    // Get in keyword
                    SyntaxToken inToken = tokens.Consume();

                    // Get expression
                    ExpressionSyntax expression = ParseExpression();

                    // Parse the statement
                    StatementSyntax statement = ParseStatement();

                    // Create the foreach syntax
                    return new ForeachStatementSyntax(forToken, typeReference, identifier, inToken, expression, statement);
                }
                //else if(tokens.PeekKind() == SyntaxTokenKind.Identifier)
                //{
                //    // Expected 'in' - we can give an error here after we encounter 'for identifier' - this pattern can only be a foreach
                //    report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Source, SyntaxToken.GetText(SyntaxTokenKind.InKeyword));
                //    return RecoverFromStatementError();
                //}
            }

            // Retrace position
            tokens.Retrace(initialPosition);
            return null;
        }

        internal StatementSyntax ParseForStatement()
        {
            // Store current position
            int initialPosition = tokens.Position;

            // Check for
            if(tokens.PeekKind() == SyntaxTokenKind.ForKeyword)
            {
                // Consume token
                SyntaxToken forToken = tokens.Consume();

                // Expect opening brace
                if(tokens.ConsumeExpect(SyntaxTokenKind.LParenSymbol, out SyntaxToken lParen) == false)
                {
                    // Expected '('
                    report.Report(Code.ExpectedToken, DiagnosticSeverity.Error, tokens.Peek().Span, SyntaxToken.GetText(SyntaxTokenKind.LParenSymbol));
                    return RecoverFromStatementError();
                }

                // Parse variable
                VariableDeclarationSyntax variable = ParseVariableDeclaration();

                // Expect semicolon
                if(tokens.ConsumeExpect(SyntaxTokenKind.SemicolonSymbol, out SyntaxToken variableSemicolon) == false)
                {
                    // Expected ';'
                    report.Report(Code.ExpectedToken, DiagnosticSeverity.Error, tokens.Peek().Span, SyntaxToken.GetText(SyntaxTokenKind.SemicolonSymbol));
                    return RecoverFromStatementError();
                }

                // Parse condition expression
                ExpressionSyntax condition = ParseOptionalExpression();

                // Expect semicolon
                if (tokens.ConsumeExpect(SyntaxTokenKind.SemicolonSymbol, out SyntaxToken conditionSemicolon) == false)
                {
                    // Expected ';'
                    report.Report(Code.ExpectedToken, DiagnosticSeverity.Error, tokens.Peek().Span, SyntaxToken.GetText(SyntaxTokenKind.SemicolonSymbol));
                    return RecoverFromStatementError();
                }

                // Parse increment expressions
                SeparatedSyntaxList<ExpressionSyntax> increments = ParseSeparatedSyntaxList(ParseOptionalExpression, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Invalid);

                // Expect closing brace
                if (tokens.ConsumeExpect(SyntaxTokenKind.RParenSymbol, out SyntaxToken rParen) == false)
                {
                    // Expected ')'
                    report.Report(Code.ExpectedToken, DiagnosticSeverity.Error, tokens.Peek().Span, SyntaxToken.GetText(SyntaxTokenKind.RParenSymbol));
                    return RecoverFromStatementError();
                }

                // Parse statement
                StatementSyntax statement = ParseStatement();

                // Create for
                return new ForStatementSyntax(forToken, lParen, variable, variableSemicolon, condition, conditionSemicolon, increments, rParen, statement);
            }
            return null;
        }

        internal StatementSyntax ParseWhileStatement()
        {
            // Check for keyword
            if(tokens.PeekKind() == SyntaxTokenKind.WhileKeyword)
            {
                // Consume token
                SyntaxToken whileToken = tokens.Consume();

                // Expect opening brace
                if (tokens.ConsumeExpect(SyntaxTokenKind.LParenSymbol, out SyntaxToken lParen) == false)
                {
                    // Expected '('
                    report.Report(Code.ExpectedToken, DiagnosticSeverity.Error, tokens.Peek().Span, SyntaxToken.GetText(SyntaxTokenKind.LParenSymbol));
                    return RecoverFromStatementError();
                }

                // Parse condition expression
                ExpressionSyntax conditionExpression = ParseExpression();

                // Expect closing brace
                if (tokens.ConsumeExpect(SyntaxTokenKind.RParenSymbol, out SyntaxToken rParen) == false)
                {
                    // Expected ')'
                    report.Report(Code.ExpectedToken, DiagnosticSeverity.Error, tokens.Peek().Span, SyntaxToken.GetText(SyntaxTokenKind.RParenSymbol));
                    return RecoverFromStatementError();
                }

                // Parse statement
                StatementSyntax statement = ParseStatement();

                // Create while
                return new WhileStatement(whileToken, lParen, conditionExpression, rParen, statement);
            }
            return null;
        }

        internal StatementSyntax ParseAssignStatement()
        {
            int startPosition = tokens.Position;

            // Parse the left side - be sure to only parse lhs expressions to avoid ambiguity and over parsing
            SeparatedSyntaxList<ExpressionSyntax> left = ParseSeparatedSyntaxList(ParseLHSExpression, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Invalid);

            // Check for valid assign
            if(tokens.Peek().IsAssign == true)
            {
                // Parse the assignment
                VariableAssignmentExpressionSyntax assignment = ParseVariableAssignExpression();

                // Expect semicolon
                if(tokens.ConsumeExpect(SyntaxTokenKind.SemicolonSymbol, out SyntaxToken semicolon) == false)
                {
                    // Expected ';'
                    report.Report(Code.ExpectedToken, DiagnosticSeverity.Error, tokens.Peek().Span, SyntaxToken.GetText(SyntaxTokenKind.SemicolonSymbol));
                    return RecoverFromStatementError();
                }

                // Create the assign
                return new AssignStatementSyntax(left, assignment, semicolon);
            }

            // Retrace back to the start
            tokens.Retrace(startPosition);
            return null;
        }

        internal StatementSyntax ParseVariableDeclarationStatement()
        {
            // Try to parse variable
            VariableDeclarationSyntax variable = ParseVariableDeclaration();

            // Check for valid
            if(variable != null)
            {
                // Expect semicolon
                if(tokens.ConsumeExpect(SyntaxTokenKind.SemicolonSymbol, out SyntaxToken semicolon) == false)
                {
                    // Expected ';'
                    report.Report(Code.ExpectedToken, DiagnosticSeverity.Error, tokens.Peek().Span, SyntaxToken.GetText(SyntaxTokenKind.SemicolonSymbol));
                    return RecoverFromStatementError();
                }

                // Create the variable
                return new VariableDeclarationStatementSyntax(variable, semicolon);
            }
            return null;
        }

        internal VariableDeclarationSyntax ParseVariableDeclaration()
        {
            // Store current position
            int initialPosition = tokens.Position;

            // Expect type
            TypeReferenceSyntax variableType = ParseTypeReference();

            // Check for identifier
            SeparatedTokenList identifierList = ParseSeparatedTokenList(SyntaxTokenKind.CommaSymbol);

            // Check for valid
            if ((variableType != null || tokens.PeekKind() == SyntaxTokenKind.AssignInferSymbol) && identifierList != null)
            {
                // Parse optional assignment
                VariableAssignmentExpressionSyntax assignExpression = ParseVariableAssignExpression();

                // Expected type if non-infer operator
                if(variableType == null && (assignExpression == null || assignExpression.Assign.Kind != SyntaxTokenKind.AssignInferSymbol))
                {
                    // TODO - Type must be provided, or did you mean ':='?
                    //report.ReportDiagnostic(Code.)
                    //return RecoverFromStatementBlockError
                }

                // Create the declaration
                return new VariableDeclarationSyntax(variableType, identifierList, assignExpression);
            }
            tokens.Retrace(initialPosition);
            return null;
        }

        internal StatementSyntax ParseReturnStatement()
        {
            // Check for return
            if(tokens.PeekKind() == SyntaxTokenKind.ReturnKeyword)
            {
                // Consume token
                SyntaxToken returnToken = tokens.Consume();

                // Check for optional expression list
                ExpressionSyntax returnExpression = null;
                
                // Check for semicolon
                if(tokens.PeekKind() != SyntaxTokenKind.SemicolonSymbol)
                    returnExpression = ParseExpression();

                // Require semicolon
                if(tokens.ConsumeExpect(SyntaxTokenKind.SemicolonSymbol, out SyntaxToken semicolon) == false)
                {
                    // Expected ';'
                    report.Report(Code.ExpectedToken, DiagnosticSeverity.Error, semicolon.Span, SyntaxToken.GetText(SyntaxTokenKind.SemicolonSymbol));
                    return RecoverFromStatementError();
                }

                // Create return
                return new ReturnStatementSyntax(returnToken, returnExpression, semicolon);
            }
            return null;
        }

        internal StatementSyntax ParseBreakStatement()
        {
            // Check for break
            if(tokens.PeekKind() == SyntaxTokenKind.BreakKeyword)
            {
                // Consume token
                SyntaxToken breakToken = tokens.Consume();

                // Optional label
                SyntaxToken? label = null;
                if (tokens.PeekKind() == SyntaxTokenKind.Identifier)
                    label = tokens.Consume();

                // Require semicolon
                if (tokens.ConsumeExpect(SyntaxTokenKind.SemicolonSymbol, out SyntaxToken semicolon) == false)
                {
                    // Expected ';'
                    report.Report(Code.ExpectedToken, DiagnosticSeverity.Error, semicolon.Span, SyntaxToken.GetText(SyntaxTokenKind.SemicolonSymbol));
                    return RecoverFromStatementError();
                }

                // Create break statement
                return new BreakStatementSyntax(breakToken, label, semicolon);
            }
            return null;
        }

        internal StatementSyntax ParseContinueStatement()
        {
            // Check for continue
            if (tokens.PeekKind() == SyntaxTokenKind.ContinueKeyword)
            {
                // Consume token
                SyntaxToken continueToken = tokens.Consume();

                // Require semicolon
                if (tokens.ConsumeExpect(SyntaxTokenKind.SemicolonSymbol, out SyntaxToken semicolon) == false)
                {
                    // Expected ';'
                    report.Report(Code.ExpectedToken, DiagnosticSeverity.Error, semicolon.Span, SyntaxToken.GetText(SyntaxTokenKind.SemicolonSymbol));
                    return RecoverFromStatementError();
                }

                // Create continue statement
                return new ContinueStatementSyntax(continueToken, semicolon);
            }
            return null;
        }

        private LambdaSyntax ParseLambdaStatement()
        {
            // Check for lambda
            if(tokens.PeekKind() == SyntaxTokenKind.LambdaSymbol)
            {
                // Consume the token
                SyntaxToken lambdaToken = tokens.Consume();

                // Parse the statement
                StatementSyntax statement = ParseStatement();

                // Create the lambda
                return new LambdaSyntax(lambdaToken, statement);
            }
            return null;
        }

        private StatementSyntax RecoverFromStatementError()
        {
            // An error occurred while parsing an expression - To recover we should consume all tokens until we reach a semi colon or end of stream
            while (tokens.EOF == false && tokens.PeekKind() != SyntaxTokenKind.SemicolonSymbol)
                tokens.Consume();

            // Consume the semicolon also which demotes the end of a statement
            if (tokens.EOF == false)
                tokens.Consume();

            // Always null - just means we can return from calling control easily
            return null;
        }

        private StatementBlockSyntax RecoverFromStatementBlockError()
        {
            // An error occurred while parsing a statement block - To recover we should consume all tokens until we reach a closing block or end of stream
            while (tokens.EOF == false && tokens.PeekKind() != SyntaxTokenKind.RBlockSymbol)
                tokens.Consume();

            // Consume the end block token also which demotes the end of a statement block
            if (tokens.EOF == false)
                tokens.Consume();

            // Always null - just means we can return from calling control easily
            return null;
        }
    }
}
