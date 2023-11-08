
using Antlr4.Runtime;
using LumaSharp_Compiler.AST.Statement;

namespace LumaSharp_Compiler.AST
{
    public abstract class StatementSyntax : SyntaxNode
    {
        // Protected
        protected SyntaxToken statementEnd = null;     // Semicolon or block

        // Properties
        public SyntaxToken StatementEnd
        {
            get { return statementEnd; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal StatementSyntax(SyntaxToken start)
            : base(start, SyntaxToken.Semi())
        {
            statementEnd = base.EndToken;
        }

        internal StatementSyntax(SyntaxTree tree, SyntaxNode parent, ParserRuleContext context)
            : base(tree, parent, context)
        {
            this.statementEnd = new SyntaxToken(context.Stop);
        }

        // Methods
        public static StatementSyntax Any(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.StatementContext statement)
        {
            // Check for condition
            if (statement.ifStatement() != null)
                return new ConditionStatementSyntax(tree, parent, statement.ifStatement());

            // Check for for loop
            if(statement.forStatement() != null)
                return new ForStatementSyntax(tree, parent, statement.forStatement());

            // Check for return
            if (statement.returnStatement() != null)
                return new ReturnStatementSyntax(tree, parent, statement.returnStatement());

            // Check for variable
            if (statement.localVariableStatement() != null)
                return new VariableDeclarationStatementSyntax(tree, parent, statement.localVariableStatement());

            // Check for assign
            if(statement.assignStatement() != null)
                return new AssignStatementSyntax(tree, parent, statement.assignStatement());

            throw new NotSupportedException("Statement is not supported");
        }
    }
}
