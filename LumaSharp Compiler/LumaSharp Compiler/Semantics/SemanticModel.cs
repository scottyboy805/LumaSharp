using LumaSharp_Compiler.Syntax;

namespace LumaSharp_Compiler.Semantics
{
    public sealed class SemanticModel : ILibraryReferenceSymbol
    {
        // Private
        private string libraryName = "";
        private TypeModel[] typeModels = null;

        // Properties
        public int LibraryToken
        {
            get { return -1; }  // -1 = current library
        }

        public string LibraryName
        {
            get { return libraryName; }
        }

        // Constructor
        internal SemanticModel(string libraryName, TypeSyntax[] types)
            : base()
        {
            this.libraryName = libraryName;
            this.typeModels = types != null 
                ? types.Select(t => new TypeModel(t, this)).ToArray() 
                : null;
        }
    }
}
