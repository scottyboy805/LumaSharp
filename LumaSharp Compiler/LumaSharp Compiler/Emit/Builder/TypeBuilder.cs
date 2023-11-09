using LumaSharp.Runtime;
using LumaSharp_Compiler.Semantics.Model;

namespace LumaSharp_Compiler.Emit.Builder
{
    internal class TypeBuilder
    {
        // Private
        private TypeModel typeModel = null;
        private List<MethodBuilder> methodBuilders = new List<MethodBuilder>();

        private MemoryStream executableStream = null;

        // Constructor
        public TypeBuilder(TypeModel typeModel)
        {
            this.typeModel = typeModel;
        }

        // Methods
        public int BuildEmitModel()
        {
            // Create memory
            executableStream = new MemoryStream();

            // Create writer
            BinaryWriter writer = new BinaryWriter(executableStream);

            // Get type handle
            _TypeHandle handle = typeModel.TypeHandle;

            // Write type handle
            EmitUtil.WriteStruct(writer, handle);


            // Write all fields


            // Write all methods
            foreach(MethodBuilder methodBuilder in methodBuilders)
            {
                methodBuilder.BuildEmitModel(writer);
            }

            // Get size required for this type image
            writer.Flush();
            return (int)executableStream.Position;
        }

    }
}
