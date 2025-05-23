
namespace LumaSharp.Compiler.AST
{
    public sealed class ConstructorInvokeSyntax : SyntaxNode
    {
        // Private
        private readonly SyntaxToken colon;
        private readonly SyntaxToken baseOrThisKeyword;        
        private readonly ArgumentListSyntax arguments;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return colon; }
        }

        public override SyntaxToken EndToken
        {
            get { return arguments.EndToken; }
        }

        public SyntaxToken Colon
        {
            get { return colon; }
        }

        public SyntaxToken BaseOrThisKeyword
        {
            get { return baseOrThisKeyword; }
        }

        public ArgumentListSyntax Arguments
        {
            get { return arguments; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield return arguments; }
        }

        // Constructor
        internal ConstructorInvokeSyntax(SyntaxToken colon, SyntaxToken baseOrThisKeyword, ArgumentListSyntax arguments)
        {
            // Check kind
            if (colon.Kind != SyntaxTokenKind.ColonSymbol)
                throw new ArgumentException(nameof(colon) + " must be of kind: " + SyntaxTokenKind.ColonSymbol);

            if (baseOrThisKeyword.Kind != SyntaxTokenKind.BaseKeyword && baseOrThisKeyword.Kind != SyntaxTokenKind.ThisKeyword)
                throw new ArgumentException(nameof(baseOrThisKeyword) + " must be of kind: " + SyntaxTokenKind.BaseKeyword + " or" + SyntaxTokenKind.ThisKeyword);

            // Check for null
            if(arguments == null)
                throw new ArgumentNullException(nameof(arguments));

            this.colon = colon;
            this.baseOrThisKeyword = baseOrThisKeyword;
            this.arguments = arguments;

            // Set parent
            arguments.parent = this;
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write colon
            colon.GetSourceText(writer);

            // Write keyword
            baseOrThisKeyword.GetSourceText(writer);

            // Write arguments
            arguments.GetSourceText(writer);
        }
    }
}
