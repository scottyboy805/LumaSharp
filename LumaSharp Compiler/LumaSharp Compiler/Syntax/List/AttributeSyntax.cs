
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class AttributeSyntax : SyntaxNode
    {
        // Private
        private readonly TypeReferenceSyntax attributeType;
        private readonly ArgumentListSyntax argumentList;

        // Properties
        public override SyntaxToken StartToken => attributeType.StartToken;
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
        {
            // Check for null
            if(attributeType == null)
                throw new ArgumentNullException(nameof(attributeType));

            this.attributeType = attributeType;
            this.argumentList = argumentList;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitAttribute(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitAttribute(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Write type
            attributeType.GetSourceText(writer);

            // Check for argument list
            if(HasArgumentList == true)
                argumentList.GetSourceText(writer);
        }
    }

    public static partial class Syntax
    {
        // Methods
        public static AttributeSyntax Attribute(TypeReferenceSyntax attributeType, ArgumentListSyntax arguments = null) 
            => new (attributeType, arguments);
    }
}
