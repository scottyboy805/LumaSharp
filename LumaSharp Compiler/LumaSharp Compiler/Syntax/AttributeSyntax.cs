
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class AttributeSyntax : SyntaxNode
    {
        // Private
        private readonly SyntaxToken hash;
        private readonly TypeReferenceSyntax attributeType;
        private readonly ArgumentListSyntax argumentList;

        // Properties
        public override SyntaxToken StartToken => hash;
        public override SyntaxToken EndToken
        {
            get
            {
                // Get arg end token
                if (HasArgumentList == true)
                    return argumentList.EndToken;

                // Get type end token
                return attributeType.EndToken;
            }
        }

        public SyntaxToken Hash => hash;
        public TypeReferenceSyntax AttributeType => attributeType;
        public ArgumentListSyntax ArgumentList => argumentList;

        public bool HasArgumentList
        {
            get { return argumentList != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                // Type
                yield return attributeType;

                // Expressions
                if(HasArgumentList == true)
                {
                    foreach (SyntaxNode argument in argumentList.Descendants)
                        yield return argument;
                }
            }
        }

        // Constructor
        internal AttributeSyntax(TypeReferenceSyntax attributeType, ArgumentListSyntax argumentList)
            : this(
                  Syntax.Token(SyntaxTokenKind.HashSymbol),
                  attributeType,
                  argumentList)
        {
        }

        internal AttributeSyntax(SyntaxToken hashToken, TypeReferenceSyntax attributeType, ArgumentListSyntax argumentList)
        {
            // Check token
            if(hashToken.Kind != SyntaxTokenKind.HashSymbol)
                throw new ArgumentException(nameof(hashToken) + " must be of kind: " + SyntaxTokenKind.HashSymbol);

            this.hash = hashToken;
            this.attributeType = attributeType;
            this.argumentList = argumentList;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitAttribute(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Write hash
            hash.GetSourceText(writer);

            // Write type
            attributeType.GetSourceText(writer);

            // Check for argument list
            if(HasArgumentList == true)
                argumentList.GetSourceText(writer);
        }
    }
}
