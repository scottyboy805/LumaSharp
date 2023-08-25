using LumaSharp_Compiler.Syntax;

namespace LumaSharp_Compiler.Semantics
{
    public sealed class GenericParameterModel
    {
        // Private
        private SyntaxToken identifier = null;
        private TypeReferenceModel[] genericConstraints = null;

        // Properties
        public string GenericParameterName
        {
            get { return identifier.Text; }
        }

        public TypeReferenceModel[] GenericConstraints
        {
            get { return genericConstraints; }
        }

        public int GenericConstraintCount
        {
            get { return HasGenericConstraints ? genericConstraints.Length : 0; }
        }

        public bool HasGenericConstraints
        {
            get { return genericConstraints != null; }
        }

        // Constructor
        internal GenericParameterModel(SyntaxToken identifier, TypeReferenceModel[] genericConstraints = null)
        {
            this.identifier = identifier;
            this.genericConstraints = genericConstraints;
        }
    }
}
