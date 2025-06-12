
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class AccessorSyntax : MemberSyntax
    {
        // Private
        private readonly TypeReferenceSyntax accessorType;
        private readonly AccessorBodySyntax[] accessorBodies;
        private readonly AccessorLambdaSyntax lambda;
        private readonly SyntaxToken? overrideKeyword;

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

        public SyntaxToken? Override
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

        public AccessorLambdaSyntax Lambda
        {
            get { return lambda; }
        }

        public bool HasLambdaBody
        {
            get { return lambda != null; }
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

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        internal AccessorSyntax(SyntaxToken identifier, AttributeSyntax[] attributes, SyntaxToken[] modifiers, TypeReferenceSyntax type, SyntaxToken? overrideToken, AccessorBodySyntax[] accessorBodies, AccessorLambdaSyntax lambda)
            : base(identifier, attributes, modifiers)
        {
            // Accessor type
            this.accessorType = type;
            this.overrideKeyword = overrideToken;
            this.accessorBodies = accessorBodies;
            this.lambda = lambda;            
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitAccessor(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitAccessor(this);
        }

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
