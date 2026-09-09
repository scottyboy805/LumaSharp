
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public class GenericArgumentListSyntax : SeparatedSyntaxList<TypeReferenceSyntax>
    {
        // Private
        private readonly SyntaxToken lGeneric;
        private readonly SyntaxToken rGeneric;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return lGeneric; }
        }

        public override SyntaxToken EndToken
        {
            get { return rGeneric; }
        }

        public SyntaxToken LGeneric
        {
            get { return lGeneric; }
        }

        public SyntaxToken RGeneric
        {
            get { return rGeneric; }
        }

        public bool HasGenericArguments
        {
            get { return Count > 0; }
        }

        // Constructor
        internal GenericArgumentListSyntax(SyntaxToken lGeneric, SeparatedSyntaxList<TypeReferenceSyntax> genericArguments, SyntaxToken rGeneric)
            : base(genericArguments)
        {
            // Check kind
            if(lGeneric.Kind != SyntaxTokenKind.LessSymbol)
                throw new ArgumentException(nameof(lGeneric) + " must be of kind: " + SyntaxTokenKind.LessSymbol);

            if(rGeneric.Kind != SyntaxTokenKind.GreaterSymbol)
                throw new ArgumentException(nameof(rGeneric) + " must be of kind: " + SyntaxTokenKind.GreaterSymbol);

            this.lGeneric = lGeneric;
            this.rGeneric = rGeneric;

            // Set parent
            if (genericArguments != null) genericArguments.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitGenericArgumentList(this);
        }

        public override J Accept<J>(SyntaxVisitor<J> visitor)
        {
            return visitor.VisitGenericArgumentList(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Generic start
            lGeneric.GetSourceText(writer);

            // Generic arguments
            base.GetSourceText(writer);

            // Generic end
            rGeneric.GetSourceText(writer);
        }
    }

    public static partial class Syntax
    {
        // Methods
        internal static GenericArgumentListSyntax EmptyGenericArgumentList()
            => new(Token(SyntaxTokenKind.LessSymbol), null, Token(SyntaxTokenKind.GreaterSymbol));

        public static GenericArgumentListSyntax GenericArgumentList(params TypeReferenceSyntax[] genericTypeArguments)
            => new(Token(SyntaxTokenKind.LessSymbol), new(SyntaxTokenKind.CommaSymbol, genericTypeArguments), Token(SyntaxTokenKind.GreaterSymbol));
    }
}
