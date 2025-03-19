
namespace LumaSharp.Compiler.AST
{
    public sealed class GenericParameterSyntax : SyntaxNode
    {
        // Private
        private readonly SyntaxToken identifier;
        private readonly SeparatedListSyntax<TypeReferenceSyntax> genericConstraints;
        private readonly SyntaxToken colon;
        private readonly int index;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return identifier; }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                // Check for constraint
                if (HasConstraintTypes == true)
                    return genericConstraints.EndToken;

                return identifier;
            }
        }

        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public SeparatedListSyntax<TypeReferenceSyntax> ConstraintTypes
        {
            get { return genericConstraints; }
        }

        public int Index
        {
            get { return index; }
        }

        public int ConstraintTypeCount
        {
            get { return HasConstraintTypes ? genericConstraints.Count : 0; }
        }

        public bool HasConstraintTypes
        {
            get { return genericConstraints != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { return genericConstraints.Descendants; }
        }

        // Constructor
        internal GenericParameterSyntax(SyntaxNode parent, string identifier, int index, TypeReferenceSyntax[] constraintTypes)
            : base(parent)
        {
            // Identifier
            this.identifier = Syntax.Identifier(identifier);
            this.index = index;

            if (genericConstraints != null)
            {
                this.colon = Syntax.KeywordOrSymbol(SyntaxTokenKind.ColonSymbol);
                this.genericConstraints = new SeparatedListSyntax<TypeReferenceSyntax>(this, SyntaxTokenKind.CommaSymbol);

                // Constrain types
                foreach (TypeReferenceSyntax constraintType in constraintTypes)
                    this.genericConstraints.AddElement(constraintType, Syntax.KeywordOrSymbol(SyntaxTokenKind.CommaSymbol));
            }
        }

        internal GenericParameterSyntax(SyntaxNode parent, LumaSharpParser.GenericParameterContext paramDef, int index)
            : base(parent)
        {
            // Identifier
            this.identifier = new SyntaxToken(SyntaxTokenKind.Identifier, paramDef.IDENTIFIER());

            // Assigned index in list
            this.index = index;

            // Constraint types
            LumaSharpParser.GenericConstraintListContext constraints = paramDef.genericConstraintList();

            if(constraints != null)
            {
                // Create constraints
                this.genericConstraints = new SeparatedListSyntax<TypeReferenceSyntax>(this, SyntaxTokenKind.CommaSymbol);

                // Get colon
                this.colon = new SyntaxToken(SyntaxTokenKind.ColonSymbol, constraints.COLON());


                // Get primary constraint
                LumaSharpParser.GenericConstraintContext primaryConstraint = constraints.genericConstraint();

                this.genericConstraints.AddElement(new TypeReferenceSyntax(genericConstraints, null, primaryConstraint.typeReference()), null);


                // Get secondary constraints
                LumaSharpParser.GenericConstraintSecondaryContext[] secondaryConstraints = constraints.genericConstraintSecondary();

                if(secondaryConstraints != null)
                {
                    // Process all constrains
                    foreach(LumaSharpParser.GenericConstraintSecondaryContext secondaryConstraint in secondaryConstraints)
                    {
                        this.genericConstraints.AddElement(
                            new TypeReferenceSyntax(genericConstraints, null, secondaryConstraint.genericConstraint().typeReference()),
                            new SyntaxToken(SyntaxTokenKind.CommaSymbol, secondaryConstraint.COMMA()));
                    }
                }
            }
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write identifier
            identifier.GetSourceText(writer);

            if (HasConstraintTypes == true)
            {
                // Colon
                colon.GetSourceText(writer);

                // Get constrains types
                genericConstraints.GetSourceText(writer);
            }
        }
    }
}
