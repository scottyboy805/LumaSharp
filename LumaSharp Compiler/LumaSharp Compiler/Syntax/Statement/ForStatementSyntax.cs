
namespace LumaSharp_Compiler.AST.Statement
{
    public sealed class ForStatementSyntax : StatementSyntax
    {
        // Private
        private SyntaxToken keyword = null;
        private SyntaxToken lparen = null;
        private SyntaxToken rparen = null;
        private VariableDeclarationStatementSyntax[] forVariables = null;
        private ExpressionSyntax forCondition = null;
        private ExpressionSyntax[] forIncrements = null;
        private StatementSyntax inlineStatement = null;
        private BlockSyntax<StatementSyntax> blockStatement = null;
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

        public VariableDeclarationStatementSyntax[] ForVariables
        {
            get { return forVariables; }
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
        }

        public BlockSyntax<StatementSyntax> BlockStatement
        {
            get { return blockStatement; }
        }

        public SyntaxToken Semicolon
        {
            get { return semicolon; }
        }

        public int ForVariableCount
        {
            get { return HasForVariables ? forVariables.Length : 0; }
        }

        public int ForIncrementCount
        {
            get { return HasForIncrements ? forIncrements.Length : 0; }
        }

        public bool HasForVariables
        {
            get { return forVariables != null; }
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
        internal ForStatementSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ForStatementContext statement)
            : base(tree, parent, statement)
        {

        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
