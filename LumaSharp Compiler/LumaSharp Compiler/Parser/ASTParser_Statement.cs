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
                    statement = ParseConditionStatement();

                // Check for return
                if (statement == null)
                    statement = ParseReturnStatement();
            }

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

        internal StatementSyntax ParseBlockStatement()
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
                        report.ReportDiagnostic(Code.ExprectedStatement, MessageSeverity.Error, tokens.Peek().Source);
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

        internal StatementSyntax ParseConditionStatement()
        {
            // Check for if keyword
            if(tokens.PeekKind() == SyntaxTokenKind.IfKeyword)
            {
                // Consume the token
                SyntaxToken ifToken = tokens.Consume();

                // Parse the condition expression
                ExpressionSyntax conditionExpression = ParseExpression();

                // Check for condition
                if(conditionExpression == null)
                {
                    // Expected expression
                    report.ReportDiagnostic(Code.ExpectedExpression, MessageSeverity.Error, tokens.Peek().Source);
                    return RecoverFromStatementError();
                }

                // Parse the statement
                StatementSyntax statement = ParseStatement();

                // Check for statement
                if(statement == null)
                {
                    // Expected statement
                    report.ReportDiagnostic(Code.ExprectedStatement, MessageSeverity.Error, tokens.Peek().Source);
                    return RecoverFromStatementError();
                }

                // Parse optional alternate recursively
                ConditionStatementSyntax alternate = ParseConditionAlternateStatement() as ConditionStatementSyntax;

                // Create if
                return new ConditionStatementSyntax(ifToken, conditionExpression, alternate, statement);
            }
            return null;
        }

        private StatementSyntax ParseConditionAlternateStatement()
        {
            // Check for else
            if(tokens.PeekKind() == SyntaxTokenKind.ElseKeyword || tokens.PeekKind() == SyntaxTokenKind.ElifKeyword)
            {
                // Consume token
                SyntaxToken elseOrElifToken = tokens.Consume();

                // Get condition
                ExpressionSyntax conditionExpression = null;

                // Check for conditioned if
                if (tokens.PeekKind() == SyntaxTokenKind.ElifKeyword)
                    conditionExpression = ParseExpression();

                // Must be just an else clause
                // Parse the statement
                StatementSyntax statement = ParseStatement();

                // Check for statement
                if (statement == null)
                {
                    // Expected statement
                    report.ReportDiagnostic(Code.ExprectedStatement, MessageSeverity.Error, tokens.Peek().Source);
                    return RecoverFromStatementError();
                }

                // Parse optional alternate recursively
                ConditionStatementSyntax alternate = ParseConditionAlternateStatement() as ConditionStatementSyntax;

                // Create the alternate condition
                return new ConditionStatementSyntax(elseOrElifToken, conditionExpression, alternate, statement);
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
                    //return new Foreach
                }
                else if(tokens.PeekKind() == SyntaxTokenKind.Identifier)
                {
                    // Expected 'in' - we can give an error here after we encounter 'for identifier' - this pattern can only be a foreach
                    report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Source, SyntaxToken.GetText(SyntaxTokenKind.InKeyword));
                    return RecoverFromStatementError();
                }
            }

            // Retrace position
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
                SeparatedSyntaxList<ExpressionSyntax> returnExpressions = ParseSeparatedSyntaxList(ParseExpression, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.SemicolonSymbol);

                // Require semicolon
                if(tokens.ConsumeExpect(SyntaxTokenKind.SemicolonSymbol, out SyntaxToken semicolon) == false)
                {
                    // Expected ';'
                    report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, semicolon.Source, SyntaxToken.GetText(SyntaxTokenKind.SemicolonSymbol));
                    return RecoverFromStatementError();
                }

                // Create return
                return new ReturnStatementSyntax(returnToken, returnExpressions, semicolon);
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

                // Require semicolon
                if (tokens.ConsumeExpect(SyntaxTokenKind.SemicolonSymbol, out SyntaxToken semicolon) == false)
                {
                    // Expected ';'
                    report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, semicolon.Source, SyntaxToken.GetText(SyntaxTokenKind.SemicolonSymbol));
                    return RecoverFromStatementError();
                }

                // Create break statement
                return new BreakStatementSyntax(breakToken, semicolon);
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
                    report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, semicolon.Source, SyntaxToken.GetText(SyntaxTokenKind.SemicolonSymbol));
                    return RecoverFromStatementError();
                }

                // Create continue statement
                return new ContinueStatementSyntax(continueToken, semicolon);
            }
            return null;
        }

        private LambdaStatementSyntax ParseLambdaStatement()
        {
            // Check for lambda
            if(tokens.PeekKind() == SyntaxTokenKind.LambdaSymbol)
            {
                // Consume the token
                SyntaxToken lambdaToken = tokens.Consume();

                // Parse the statement
                StatementSyntax statement = ParseStatement();

                // Create the lambda
                return new LambdaStatementSyntax(lambdaToken, statement);
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

        private StatementSyntax RecoverFromStatementBlockError()
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
