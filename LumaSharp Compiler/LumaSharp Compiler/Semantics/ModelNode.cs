
namespace LumaSharp_Compiler.Semantics
{
    public abstract class ModelNode
    {
        // Private
        private SemanticModel model = null;

        // Properties
        public SemanticModel Model
        {
            get { return model; }
        }

        // Constructor
        internal ModelNode(SemanticModel model)
        {
            this.model = model;
        }
    }
}
