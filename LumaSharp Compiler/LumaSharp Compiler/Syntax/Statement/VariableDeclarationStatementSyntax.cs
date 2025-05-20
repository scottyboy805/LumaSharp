
namespace LumaSharp.Compiler.AST
{
    public sealed class VariableDeclarationStatementSyntax : StatementSyntax
    {
        // Private
        private readonly TypeReferenceSyntax variableType;
        private readonly SeparatedTokenList identifiers;
        private readonly VariableAssignExpressionSyntax assignment;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return variableType.StartToken; }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                // Check for assignment
                if (HasAssignment == true)
                    return assignment.EndToken;

                // Identifier
                return identifiers.EndToken;
            }
        }

        public TypeReferenceSyntax VariableType
        {
            get { return variableType; }
        }

        public SyntaxToken[] IdentifierNames
        {
            get { return identifiers.ToArray(); }
        }

        public SeparatedTokenList Identifiers
        {
            get { return identifiers; }
        }

        public VariableAssignExpressionSyntax Assignment
        {
            get { return assignment; }
        }

        public bool HasAssignment
        {
            get { return assignment != null; }
        }

        // Constructor
        internal VariableDeclarationStatementSyntax(TypeReferenceSyntax variableType, SeparatedTokenList identifiers, VariableAssignExpressionSyntax assignment)
        {
            // Check for null
            if (variableType == null)
                throw new ArgumentNullException(nameof(variableType));

            if(identifiers == null)
                throw new ArgumentNullException(nameof(identifiers));

            this.variableType = variableType;
            this.identifiers = identifiers;
            this.assignment = assignment;

            // Set parent
            variableType.parent = this;
            identifiers.parent = this;
            assignment.parent = this;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write type 
            variableType.GetSourceText(writer);

            // Write identifiers
            identifiers.GetSourceText(writer);

            // Check for assignment
            if (HasAssignment == true)
            {
                // Write assign
                assignment.GetSourceText(writer);
            }
        }
    }
}
