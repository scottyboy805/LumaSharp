
namespace LumaSharp.Compiler.AST
{
    public sealed class AccessorSyntax : MemberSyntax
    {
        // Private
        private readonly TypeReferenceSyntax accessorType;
        private readonly AccessorBodySyntax[] accessorBodies;
        private readonly SyntaxToken overrideKeyword;

        // Properties
        public override SyntaxToken EndToken
        {
            get
            {
                // Check for accessor bodies
                if (HasAccessorBodies == true)
                    return accessorBodies[accessorBodies.Length - 1].EndToken;

                return Identifier;
            }
        }

        public SyntaxToken Override
        {
            get { return overrideKeyword; }
        }

        public TypeReferenceSyntax AccessorType
        {
            get { return accessorType; }
        }

        public AccessorBodySyntax[] AccessorBodies
        {
            get { return accessorBodies; }
        }

        public bool HasLambdaBody
        {
            get { return accessorBodies != null && accessorBodies.Any(b => b.Lambda.Kind != SyntaxTokenKind.Invalid); }
        }

        public bool HasReadBody
        {
            get { return accessorBodies != null && accessorBodies.Any(b => b.IsReadBody); }
        }

        public bool HasWriteBody
        {
            get { return accessorBodies != null && accessorBodies.Any(b => b.IsWriteBody); }
        }

        public bool HasAccessorBodies
        {
            get { return accessorBodies != null; }
        }

        public bool IsOverride
        {
            get { return overrideKeyword.Kind != SyntaxTokenKind.Invalid; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal AccessorSyntax(SyntaxToken identifier, AttributeReferenceSyntax[] attributes, SyntaxToken[] modifiers, TypeReferenceSyntax type, AccessorBodySyntax[] accessorBodies, bool isOverride)
            : base(identifier, attributes, modifiers)
        {
            // Accessor type
            this.accessorType = type;
            this.accessorBodies = accessorBodies;

            // Check for override
            if (isOverride == true)
                this.overrideKeyword = Syntax.Token(SyntaxTokenKind.OverrideKeyword);
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Attributes
            base.GetSourceText(writer);

            // Type
            accessorType.GetSourceText(writer);

            // Identifier
            identifier.GetSourceText(writer);

            // Check for bodies
            if(accessorBodies != null)
            {
                foreach(AccessorBodySyntax accessorBody in accessorBodies)
                    accessorBody.GetSourceText(writer);
            }   
        }
    }
}
