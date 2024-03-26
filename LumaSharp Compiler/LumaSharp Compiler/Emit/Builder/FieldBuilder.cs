using LumaSharp.Runtime.Handle;
using LumaSharp.Runtime.Reflection;
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
        public int EmitMetaModel(BinaryWriter writer = null)
        {
            // Check for writer
            if (writer == null)
            {
                // Create memory
                Stream executableStream = new MemoryStream();

                // Create writer
                writer = new BinaryWriter(executableStream);
            }

            // Get field flags
            FieldFlags fieldFlags = fieldModel.FieldFlags;

            // Write metadata
            writer.Write(fieldModel.SymbolToken);
            writer.Write(fieldModel.FieldName);
            writer.Write((uint)fieldFlags);

            // Get size required for this field metadata
            writer.Flush();
            return (int)writer.BaseStream.Position;
        }

        public int EmitExecutableModel(BinaryWriter writer = null)
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
            handle.Write(writer);

            // Get size required for this method image
            writer.Flush();
            return (int)writer.BaseStream.Position;
        }
    }
}
