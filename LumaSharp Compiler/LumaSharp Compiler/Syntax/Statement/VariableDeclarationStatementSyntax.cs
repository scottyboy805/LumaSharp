
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class VariableDeclarationStatementSyntax : StatementSyntax
    {
        // Private
        private readonly VariableDeclarationSyntax variable;
        private readonly SyntaxToken semicolon;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return variable.StartToken; }
        }

        public override SyntaxToken EndToken
        {
            get { return semicolon; }
        }

        public SyntaxToken Semicolon
        {
            get { return semicolon; }
        }

        public TypeReferenceSyntax VariableType
        {
            get { return variable.VariableType; }
        }

        public SyntaxToken[] IdentifierNames
        {
            get { return variable.Identifiers.ToArray(); }
        }

        public SeparatedTokenList Identifiers
        {
            get { return variable.Identifiers; }
        }

        public VariableAssignmentExpressionSyntax Assignment
        {
            get { return variable.Assignment; }
        }

        public bool HasAssignment
        {
            get { return variable.Assignment != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield return variable; }
        }

        // Constructor
        internal VariableDeclarationStatementSyntax(VariableDeclarationSyntax variable)
            : this(
                  variable,
                  Syntax.Token(SyntaxTokenKind.SemicolonSymbol))
        {
        }

        internal VariableDeclarationStatementSyntax(VariableDeclarationSyntax variable, SyntaxToken semicolon)
        {
            // Check for null
            if (variable == null)
                throw new ArgumentNullException(nameof(variable));

            this.variable = variable;
            this.semicolon = semicolon;

            // Set parent
            variable.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitVariableDeclarationStatement(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Write variable 
            variable.GetSourceText(writer);

            // Semicolon
            semicolon.GetSourceText(writer);
        }
    }
}
