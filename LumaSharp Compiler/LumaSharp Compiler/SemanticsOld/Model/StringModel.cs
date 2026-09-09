using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.SemanticsOld.Model
{
    public sealed class StringModel : SymbolModel, IReferenceSymbol
    {
        // Private
        private readonly string text;

        // Properties
        public string Text
        {
            get { return text; }
        }

        public ILibraryReferenceSymbol LibrarySymbol
        {
            get { return Model.LibrarySymbol; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield break; }
        }

        // Constructor
        public StringModel(SyntaxToken stringToken)
            : base(stringToken.Span)
        {
            // Check for kind
            if (stringToken.Kind != SyntaxTokenKind.Identifier && stringToken.Kind != SyntaxTokenKind.Literal)
                throw new ArgumentException(nameof(stringToken) + " must be of kind: " + SyntaxTokenKind.Identifier + " or " + SyntaxTokenKind.Literal);

            this.text = stringToken.Text;
        }

        public StringModel(string text, SyntaxSpan? span)
            : base(span)
        {
            // Check for invalid
            if(string.IsNullOrEmpty(text) == true)
                throw new ArgumentNullException(nameof(text));

            this.text = text;
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
        }
    }
}
