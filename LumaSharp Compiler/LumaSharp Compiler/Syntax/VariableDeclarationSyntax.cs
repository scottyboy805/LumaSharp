
namespace LumaSharp.Compiler.AST
{
    public sealed class VariableDeclarationSyntax : SyntaxNode
    {
        // Private
        private readonly TypeReferenceSyntax variableType;
        private readonly SeparatedTokenList identifiers;
        private readonly VariableAssignmentExpressionSyntax assignment;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return variableType.StartToken; }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                if (assignment != null)
                    return assignment.EndToken;

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

        public VariableAssignmentExpressionSyntax Assignment
        {
            get { return assignment; }
        }

        public bool HasAssignment
        {
            get { return assignment != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                yield return variableType;
                yield return identifiers;

                if (assignment != null)
                    yield return assignment;
            }
        }

        // Constructor
        internal VariableDeclarationSyntax(TypeReferenceSyntax variableType, SeparatedTokenList identifiers, VariableAssignmentExpressionSyntax assignment)
        {
            // Check for null
            if (variableType == null)
                throw new ArgumentNullException(nameof(variableType));

            if (identifiers == null)
                throw new ArgumentNullException(nameof(identifiers));

            this.variableType = variableType;
            this.identifiers = identifiers;
            this.assignment = assignment;

            // Set parent
            variableType.parent = this;
            identifiers.parent = this;
            if (assignment != null) assignment.parent = this;
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
