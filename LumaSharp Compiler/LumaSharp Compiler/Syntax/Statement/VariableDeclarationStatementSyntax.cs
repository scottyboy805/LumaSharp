
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
        internal VariableDeclarationStatementSyntax(SyntaxNode parent, TypeReferenceSyntax variableType, string[] identifiers, VariableAssignExpressionSyntax assignment)
            : base(parent)
        {
            this.variableType = variableType;
            this.identifiers = new SeparatedTokenList(this, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Identifier);
            this.assignment = assignment;

            // Add identifiers
            foreach (string identifier in identifiers)
                this.identifiers.AddElement(Syntax.Identifier(identifier), Syntax.KeywordOrSymbol(SyntaxTokenKind.CommaSymbol));
        }

        internal VariableDeclarationStatementSyntax(SyntaxNode parent, LumaSharpParser.LocalVariableStatementContext local)
            : base(parent)
        {
            // Variable type
            this.variableType = new TypeReferenceSyntax(this, null, local.typeReference());

            // Identifiers
            this.identifiers = new SeparatedTokenList(this, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Identifier);

            // Add primary identifier
            this.identifiers.AddElement(new SyntaxToken(SyntaxTokenKind.Identifier, local.IDENTIFIER()), null);

            // Add secondary identifiers
            LumaSharpParser.LocalVariableSecondaryContext[] secondaryIdentifiers = local.localVariableSecondary();

            if(secondaryIdentifiers != null)
            {
                foreach(LumaSharpParser.LocalVariableSecondaryContext secondaryIdentifier in secondaryIdentifiers)
                {
                    this.identifiers.AddElement(
                        new SyntaxToken(SyntaxTokenKind.Identifier, secondaryIdentifier.IDENTIFIER()),
                        new SyntaxToken(SyntaxTokenKind.CommaSymbol, secondaryIdentifier.COMMA()));
                }
            }


            // Get assignment
            LumaSharpParser.VariableAssignmentContext assignment = local.variableAssignment();

            if (assignment != null)
            {
                // Create assignment
                this.assignment = new VariableAssignExpressionSyntax(this, assignment);
            }
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
