using LumaSharp_Compiler.Semantics.Model;

namespace LumaSharp_Compiler.Emit.Builder
{
    internal sealed class MethodBuilder : MethodBodyBuilder
    {
        // Private
        private MethodModel methodModel = null;

        // Constructor
        public MethodBuilder(MethodModel methodModel)
            : base(methodModel.HasBody == true ? methodModel.BodyStatements : null)
        {
            this.methodModel = methodModel;
        }

        // Methods
    }
}
