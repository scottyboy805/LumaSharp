using LumaSharp_Compiler.Syntax;

namespace LumaSharp_Compiler.Semantics
{
    public sealed class TypeModel : MemberModel, ITypeReferenceSymbol
    {
        // Private
        private TypeSyntax syntax = null;
        private GenericParameterModel[] genericParameters = null;
        private TypeReferenceModel[] baseTypes = null;

        // Properties
        public GenericParameterModel[] GenericParameters
        {
            get { return genericParameters; }
        }

        public TypeReferenceModel[] BaseTypes
        {
            get { return baseTypes; }
        }

        public int GenericParameterCount
        {
            get { return HasGenericParameters ? genericParameters.Length : 0; }
        }

        public int BaseTypeCount
        {
            get { return HasBaseTypes ? baseTypes.Length : 0; }
        }

        public bool HasGenericParameters
        {
            get { return genericParameters != null; }
        }

        public bool HasBaseTypes
        {
            get { return baseTypes != null; }
        }

        public int TypeToken => throw new NotImplementedException();

        public ILibraryReferenceSymbol LibrarySymbol => throw new NotImplementedException();

        public string TypeName
        {
            get { return MemberName; }
        }

        // Constructor
        internal TypeModel(TypeSyntax syntax, SemanticModel model, MemberModel parent = null)
            : base(syntax, model, parent)
        {
            this.syntax = syntax;

            // Create generics
            this.genericParameters = syntax.HasGenericParameters
                ? syntax.GenericParameters.GenericTypes.Select(t => new GenericParameterModel(t)).ToArray()
                : null;

            // Create base types
            this.baseTypes = syntax.HasBaseTypes 
                ? syntax.BaseTypeReferences.Select(t => new TypeReferenceModel(t)).ToArray() 
                : null;
        }
    }
}
