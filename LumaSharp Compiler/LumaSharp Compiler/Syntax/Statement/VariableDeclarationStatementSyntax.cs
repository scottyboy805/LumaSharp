
namespace LumaSharp_Compiler.Syntax
{
    public sealed class VariableDeclarationStatementSyntax : StatementSyntax
    {
        // Private
        private TypeReferenceSyntax variableType = null;
        private SyntaxToken[] identifiers = null;
        private SyntaxToken assign = null;
        private ExpressionSyntax[] assignExpressions = null;

        // Properties
        public TypeReferenceSyntax VariableType
        {
            get { return variableType; }
        }

        public SyntaxToken[] Identifiers
        {
            get { return identifiers; }
        }

        public SyntaxToken Assign
        {
            get { return assign; }
        }

        public ExpressionSyntax[] AssignExpressions
        {
            get { return assignExpressions; }
        }

        public int IdentifierCount
        {
            get { return identifiers.Length; }
        }

        public int AssignExpressionCount
        {
            get { return HasAssignExpressions ? identifiers.Length : 0; }
        }

        public bool HasAssignExpressions
        {
            get { return assignExpressions != null; }
        }

        // Constructor
        //internal VariableDeclarationStatementSyntax(SyntaxTree tree, SyntaxNode parent, TypeReferenceSyntax variableType, string[] variableNames, ExpressionSyntax[] assignExpressions = null)
        //    : base(tree, parent, new SyntaxToken(";"))
        //{
        //    // Check for incompatible
        //    if (variableNames != null && variableNames.Length > 0 && assignExpressions != null && variableNames.Length != assignExpressions.Length)
        //        throw new ArgumentException("Assign expression length must match variable names length");

        //    this.variableType = variableType;
        //    this.assignExpressions = assignExpressions;

        //    // Setup identifiers
        //    this.identifiers = new SyntaxToken[variableNames.Length];

        //    for(int i = 0; i < variableNames.Length; i++)
        //    {
        //        this.identifiers[i] = new SyntaxToken(variableNames[i]);
        //    }
        //}

        internal VariableDeclarationStatementSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.LocalVariableStatementContext local)
            : base(tree, parent, local)
        {
            // Variable type
            this.variableType = new TypeReferenceSyntax(tree, this, local.typeReference());

            // Identifiers
            this.identifiers = local.IDENTIFIER().Select(i =>  new SyntaxToken(i)).ToArray();

            // Assign expressions
            this.assignExpressions = local.expression().Select(e => ExpressionSyntax.Any(tree, this, e)).ToArray();
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
