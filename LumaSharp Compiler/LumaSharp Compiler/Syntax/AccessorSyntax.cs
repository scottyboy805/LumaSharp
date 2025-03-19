
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
        internal AccessorSyntax(SyntaxNode parent, string identifier, AttributeReferenceSyntax[] attributes, SyntaxToken[] modifiers, TypeReferenceSyntax type, AccessorBodySyntax[] accessorBodies, bool isOverride)
            : base(parent, identifier, attributes, modifiers)
        {
            // Accessor type
            this.accessorType = type;
            this.accessorBodies = accessorBodies;

            // Check for override
            if (isOverride == true)
                this.overrideKeyword = Syntax.KeywordOrSymbol(SyntaxTokenKind.OverrideKeyword);
        }

        internal AccessorSyntax(SyntaxNode parent, LumaSharpParser.AccessorDeclarationContext accessorDef)
            : base(accessorDef.IDENTIFIER(), parent, accessorDef.attributeReference(), accessorDef.accessModifier())
        {
            // Accessor type
            this.accessorType = new TypeReferenceSyntax(this, null, accessorDef.typeReference());

            // Override
            if (accessorDef.OVERRIDE() != null)
                this.overrideKeyword = new SyntaxToken(SyntaxTokenKind.OverrideKeyword, accessorDef.OVERRIDE());

            // Get the body
            LumaSharpParser.AccessorBodyContext body = accessorDef.accessorBody();

            // Check for body
            if(body != null)
            {
                // Check for expression lambda
                if(body.expressionLambda() != null)
                {
                    // Create new lambda body
                    this.accessorBodies = new [] { new AccessorBodySyntax(parent, body.expressionLambda()) };
                }
                else
                {
                    // Create all bodies
                    this.accessorBodies = body.accessorReadWrite().Select(a =>
                        new AccessorBodySyntax(parent, a)).ToArray();
                }
            }
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
