using LumaSharp.Runtime.Handle;
using LumaSharp_Compiler.Semantics.Model;

namespace LumaSharp_Compiler.Emit.Builder
{
    internal sealed class FieldBuilder
    {
        // Private
        private FieldModel fieldModel = null;

        private MemoryStream executableStream = null;

        // Properties
        public Stream ExecutableStream
        {
            get 
            { 
                // Return to read position
                if(executableStream != null)
                    executableStream.Position = 0;

                return executableStream; 
            }
        }

        // Constructor
        public FieldBuilder(FieldModel fieldModel)
        {
            this.fieldModel = fieldModel;
        }

        // Methods
        public int BuildEmitModel(BinaryWriter writer = null)
        {
            // Check for writer
            if (writer == null)
            {
                // Create memory
                executableStream = new MemoryStream();

                // Create writer
                writer = new BinaryWriter(executableStream);
            }

            // Get field handle
            _FieldHandle handle = fieldModel.FieldHandle;

            // Write field handle
            EmitUtil.WriteStruct(writer, handle);

            // Get size required for this method image
            writer.Flush();
            return (int)writer.BaseStream.Position;
        }
    }
}
