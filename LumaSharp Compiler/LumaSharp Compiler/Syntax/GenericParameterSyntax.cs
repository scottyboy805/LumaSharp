
namespace LumaSharp_Compiler.AST
{
    public sealed class GenericParameterSyntax : SyntaxNode
    {
        // Private
        private SyntaxToken identifier = null;
        private TypeReferenceSyntax[] constraintTypes = null;
        private int index = 0;

        // Properties
        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public TypeReferenceSyntax[] ConstraintTypes
        {
            get { return constraintTypes; }
        }

        public int Index
        {
            get { return index; }
        }

        public int ConstraintTypeCount
        {
            get { return HasConstraintTypes ? constraintTypes.Length : 0; }
        }

        public bool HasConstraintTypes
        {
            get { return constraintTypes != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { return constraintTypes; }
        }

        // Constructor
        internal GenericParameterSyntax(string identifier, TypeReferenceSyntax[] constraintTypes, int index = 0)
            : base(new SyntaxToken(identifier))
        {
            // Identifier
            this.identifier = new SyntaxToken(identifier);

            // Constrain types
            this.constraintTypes = constraintTypes;

            this.index = index;
        }

        internal GenericParameterSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.GenericParameterContext paramDef, int index)
            : base(tree, parent, paramDef)
        {
            // Identifier
            this.identifier = new SyntaxToken(paramDef.IDENTIFIER());

            // Assigned index in list
            this.index = index;

            // Constraint types
            LumaSharpParser.TypeReferenceContext[] constraints = paramDef.typeReference();

            if(constraints != null)
            {
                this.constraintTypes = constraints.Select(c => new TypeReferenceSyntax(tree, this, c)).ToArray();
            }
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
