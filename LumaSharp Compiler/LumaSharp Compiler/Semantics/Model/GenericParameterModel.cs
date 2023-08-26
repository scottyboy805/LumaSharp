//using LumaSharp_Compiler.Syntax;

//namespace LumaSharp_Compiler.Semantics.Model
//{
//    public sealed class GenericParameterModel
//    {
//        // Private
//        private SyntaxToken identifier = null;
//        private int genericIndex = 0;
//        private ITypeReferenceSymbol[] genericConstraintsSymbols = null;

//        // Properties
//        public string GenericParameterName
//        {
//            get { return identifier.Text; }
//        }

//        public TypeReferenceModel[] GenericConstraints
//        {
//            get { return genericConstraintsSymbols; }
//        }

//        public int GenericConstraintCount
//        {
//            get { return HasGenericConstraints ? genericConstraintsSymbols.Length : 0; }
//        }

//        public bool HasGenericConstraints
//        {
//            get { return genericConstraintsSymbols != null; }
//        }

//        // Constructor
//        internal GenericParameterModel(SyntaxToken identifier, TypeReferenceModel[] genericConstraints = null)
//        {
//            this.identifier = identifier;
//            this.genericConstraints = genericConstraints;
//        }
//    }
//}
