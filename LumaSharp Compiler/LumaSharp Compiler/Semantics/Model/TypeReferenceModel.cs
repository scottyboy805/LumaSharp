//using LumaSharp_Compiler.Syntax;

//namespace LumaSharp_Compiler.Semantics.Model
//{
//    public sealed class TypeReferenceModel : SymbolModel
//    {
//        // Private
//        private TypeReferenceSyntax syntax = null;
//        private ITypeReferenceSymbol resolvedTypeSymbol = null;

//        // Properties
//        public override IReferenceSymbol ResolvedSymbol
//        {
//            get { return resolvedTypeSymbol; }
//        }

//        public bool IsResolved
//        {
//            get { return resolvedTypeSymbol != null; }
//        }

//        // Constructor
//        internal TypeReferenceModel(SemanticModel model, TypeReferenceSyntax syntax)
//            : base(model)
//        {
//            this.syntax = syntax;
//        }

//        // Methods
//        public override ITypeReferenceSymbol ResolveSymbols(ISymbolProvider provider)
//        {
//            // Check for primitive
//            if (syntax.IsPrimitiveType == true)
//                return provider.ResolveTypeSymbol(Enum.Parse<PrimitiveType>(syntax.Identifier.Text));

//            return null;
//        }
//    }
//}
