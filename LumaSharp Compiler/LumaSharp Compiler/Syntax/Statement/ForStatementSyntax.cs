﻿
using System.Diagnostics;

namespace LumaSharp_Compiler.AST.Statement
{
    public sealed class ForStatementSyntax : StatementSyntax
    {
        // Private
        private SyntaxToken keyword = null;
        private SyntaxToken lparen = null;
        private SyntaxToken rparen = null;
        private VariableDeclarationStatementSyntax forVariable = null;
        private ExpressionSyntax forCondition = null;
        private ExpressionSyntax[] forIncrements = null;
        private StatementSyntax inlineStatement = null;
        private BlockSyntax<StatementSyntax> blockStatement = null;
        private SyntaxToken comma = null;
        private SyntaxToken semicolon = null;

        // Properties
        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public SyntaxToken LParen
        {
            get { return lparen; }
        }

        public SyntaxToken RParen
        {
            get { return rparen; }
        }

        public VariableDeclarationStatementSyntax ForVariable
        {
            get { return forVariable; }
        }

        public ExpressionSyntax ForCondition
        {
            get { return forCondition; }
        }

        public ExpressionSyntax[] ForIncrements
        {
            get { return forIncrements; }
        }

        public StatementSyntax InlineStatement
        {
            get { return inlineStatement; }
            internal set { inlineStatement = value; }
        }

        public BlockSyntax<StatementSyntax> BlockStatement
        {
            get { return blockStatement; }
            internal set { blockStatement = value; }
        }

        public SyntaxToken Semicolon
        {
            get { return semicolon; }
        }

        public int ForVariableCount
        {
            get { return HasForVariables ? forVariable.IdentifierCount : 0; }
        }

        public int ForIncrementCount
        {
            get { return HasForIncrements ? forIncrements.Length : 0; }
        }

        public bool HasForVariables
        {
            get { return forVariable != null; }
        }

        public bool HasForCondition
        {
            get { return forCondition != null; }
        }

        public bool HasForIncrements
        {
            get { return forIncrements != null; }
        }

        public bool HasInlineStatement
        {
            get { return inlineStatement != null; }
        }

        public bool HasBlockStatement
        {
            get { return blockStatement != null; }
        }

        // Constructor
        internal ForStatementSyntax(VariableDeclarationStatementSyntax variable, ExpressionSyntax condition, ExpressionSyntax[] increment)
            : base(SyntaxToken.For())
        {
            this.keyword = base.StartToken;
            this.forVariable = variable;
            this.forCondition = condition;
            this.forIncrements = increment;

            lparen = SyntaxToken.LParen();
            rparen = SyntaxToken.RParen();
            comma = SyntaxToken.Comma();
            semicolon = SyntaxToken.Semi();
        }

        internal ForStatementSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ForStatementContext forStatement)
            : base(tree, parent, forStatement)
        {
            // Keyword
            this.keyword = new SyntaxToken(forStatement.FOR());

            // LR paren
            this.lparen = new SyntaxToken(forStatement.lparen);
            this.rparen = new SyntaxToken(forStatement.rparen);

            // Variables
            if (forStatement.forVariableStatement() != null)
                this.forVariable = new VariableDeclarationStatementSyntax(tree, this, forStatement.forVariableStatement(), forStatement.semiVar);

            // Conditions
            if (forStatement.expression() != null)
                this.forCondition = ExpressionSyntax.Any(tree, this, forStatement.expression());

            // Increments
            LumaSharpParser.ForIncrementExpressionContext[] increments = forStatement.forIncrementExpression();

            if(increments != null && increments.Length > 0)
            {
                this.forIncrements = increments.Select(e => ExpressionSyntax.Any(tree, this, e.expression())).ToArray();
            }

            // Statement inline
            if(forStatement.statement() != null)
            {
                this.inlineStatement = Any(tree, this, forStatement.statement());
            }

            // Statement block
            if(forStatement.statementBlock() != null)
            {
                this.blockStatement = new BlockSyntax<StatementSyntax>(tree, this, forStatement.statementBlock());
            }

            // Semi
            if (forStatement.semi != null)
                this.statementEnd = new SyntaxToken(forStatement.semi);
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            keyword.GetSourceText(writer);

            // LParen
            lparen.GetSourceText(writer);

            // Variables
            if (forVariable != null)
            {
                forVariable.GetSourceText(writer);
            }
            else
            {
                // Semi colon
                semicolon.GetSourceText(writer);
            }

            // Condition
            if(forCondition != null)
            {
                forCondition.GetSourceText(writer);
            }

            // Semi colon
            semicolon.GetSourceText(writer);

            // Increments
            if(forIncrements != null)
            {
                for(int i = 0; i < forIncrements.Length; i++)
                {
                    forIncrements[i].GetSourceText(writer);

                    if(i < forIncrements.Length - 1)
                        comma.GetSourceText(writer);
                }
            }

            // RParen
            rparen.GetSourceText(writer);


            // Check for inline
            if(HasInlineStatement == true)
            {
                inlineStatement.GetSourceText(writer);
            }
            // Check for block
            else if(HasBlockStatement == true)
            {
                blockStatement.GetSourceText(writer);
            }
            // Fall back to empty statement
            else if(semicolon != null)
            {
                semicolon.GetSourceText(writer);
            }
        }
    }
}
