using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class StringModel : SymbolModel, IReferenceSymbol
    {
        // Private
        private readonly string text;
        private _TokenHandle token = default;

        // Properties
        public string Text
        {
            get { return text; }
        }

        public ILibraryReferenceSymbol LibrarySymbol
        {
            get { return Model.LibrarySymbol; }
        }

        public _TokenHandle Token
        {
            get { return token; }
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
            // Try to get string token
            token = provider.GetSymbolToken(this);
        }
    }
}
