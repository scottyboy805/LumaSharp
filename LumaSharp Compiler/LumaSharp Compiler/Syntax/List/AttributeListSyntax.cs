
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class AttributeListSyntax : SeparatedSyntaxList<AttributeSyntax>
    {
        // Private
        private readonly SyntaxToken lBracket;
        private readonly SyntaxToken rBracket;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return lBracket; }
        }

        public override SyntaxToken EndToken
        {
            get { return rBracket; }
        }

        public SyntaxToken LBracket
        {
            get { return lBracket; }
        }

        public SyntaxToken RBracket
        {
            get { return rBracket; }
        }

        // Constructor
        internal AttributeListSyntax(SyntaxToken lBracket, SeparatedSyntaxList<AttributeSyntax> attributes, SyntaxToken rBracket)
            : base(attributes)
        {
            // Check kind
            if (lBracket.Kind != SyntaxTokenKind.LArraySymbol)
                throw new ArgumentException(nameof(lBracket) + " must be of kind: " + SyntaxTokenKind.LArraySymbol);

            if(rBracket.Kind != SyntaxTokenKind.RArraySymbol)
                throw new ArgumentException(nameof(rBracket) + " must be of kind: " + SyntaxTokenKind.RArraySymbol);

            this.lBracket = lBracket;
            this.rBracket = rBracket;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitAttributeList(this);
        }

        public override J Accept<J>(SyntaxVisitor<J> visitor)
        {
            return visitor.VisitAttributeList(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // LBracket
            lBracket.GetSourceText(writer);

            // Attribute list
            base.GetSourceText(writer);

            // RBracket
            rBracket.GetSourceText(writer);
        }
    }

    public static partial class Syntax
    {
        // Methods
        public static AttributeListSyntax EmptyAttributeList()
            => new(Token(SyntaxTokenKind.LArraySymbol), null, Token(SyntaxTokenKind.RArraySymbol));

        public static AttributeListSyntax AttributeList(AttributeSyntax singleAttribute)
            => new(Token(SyntaxTokenKind.LArraySymbol), new (SyntaxTokenKind.CommaSymbol, singleAttribute), Token(SyntaxTokenKind.RArraySymbol));

        public static AttributeListSyntax AttributeList(SeparatedSyntaxList<AttributeSyntax> attributes)
            => new(Token(SyntaxTokenKind.LArraySymbol), attributes, Token(SyntaxTokenKind.RArraySymbol));

        public static AttributeListSyntax AttributeList(params AttributeSyntax[] attributes)
            => new(Token(SyntaxTokenKind.LArraySymbol), new (SyntaxTokenKind.CommaSymbol, attributes), Token(SyntaxTokenKind.RArraySymbol));
    }
}
