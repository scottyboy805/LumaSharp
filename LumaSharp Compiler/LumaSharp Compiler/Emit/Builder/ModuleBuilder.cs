using LumaSharp.Runtime.Reflection;

namespace LumaSharp_Compiler.Emit.Builder
{
    internal sealed class ModuleBuilder
    {
        // Private
        private string moduleName = "";
        private List<TypeBuilder> typeBuilders = new List<TypeBuilder>();

        // Methods
        public void EmitLibrary(Stream writeStream)
        {
            // Check for null
            if(writeStream == null)
                throw new ArgumentNullException(nameof(writeStream));

            // Check for write
            if (writeStream.CanWrite == false)
                throw new ArgumentException("Stream must be writable");

            // Open for writing
            using(BinaryWriter writer = new BinaryWriter(writeStream))
            {
                // Write magic
                writer.Write(Module.magic);

                // Write library meta
                writer.Write(moduleName);

                // Write all type metas
                EmitMetaTypes(writer);

                // Write all executable segments
                EmitExecutableTypes(writer);
            }
        }

        private void EmitMetaTypes(BinaryWriter writer)
        {
            // Write number of types
            writer.Write(typeBuilders.Count);

            // Process all types
            foreach(TypeBuilder typeBuilder in typeBuilders)
            {
                // Emit the meta
                typeBuilder.EmitMetaModel(writer);
            }
        }

        private void EmitExecutableTypes(BinaryWriter writer)
        {
            // Process all types
            foreach (TypeBuilder typeBuilder in typeBuilders)
            {
                // Emit the executable
                typeBuilder.EmitExecutableModel(writer);
            }
        }
    }
}
