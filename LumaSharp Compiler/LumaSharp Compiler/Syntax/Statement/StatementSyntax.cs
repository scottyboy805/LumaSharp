﻿
namespace LumaSharp.Compiler.AST
{
    public abstract class StatementSyntax : SyntaxNode
    {
        // Properties
        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal StatementSyntax(SyntaxNode parent)
            : base(parent)
        {
        }

        // Methods
        public static StatementSyntax Any(SyntaxNode parent, LumaSharpParser.StatementContext statement)
        {
            // Check for condition
            if (statement.ifStatement() != null)
                return new ConditionStatementSyntax(parent, statement.ifStatement());

            // Check for for loop
            if(statement.forStatement() != null)
                return new ForStatementSyntax(parent, statement.forStatement());

            // Check for return
            if (statement.returnStatement() != null)
                return new ReturnStatementSyntax(parent, statement.returnStatement());

            // Check for variable
            if (statement.localVariableStatement() != null)
                return new VariableDeclarationStatementSyntax(parent, statement.localVariableStatement());

            // Check for assign
            if(statement.assignStatement() != null)
                return new AssignStatementSyntax(parent, statement.assignStatement());

            throw new NotSupportedException("Statement is not supported");
        }
    }
}
