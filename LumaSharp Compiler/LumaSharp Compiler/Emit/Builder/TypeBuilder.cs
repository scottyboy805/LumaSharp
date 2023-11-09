using LumaSharp.Runtime;
using LumaSharp_Compiler.Semantics.Model;
using System.Linq;

namespace LumaSharp_Compiler.Emit.Builder
{
    internal class TypeBuilder
    {
        // Private
        private TypeModel typeModel = null;
        private List<FieldBuilder> fieldBuilders = new List<FieldBuilder>();
        private List<MethodBuilder> methodBuilders = new List<MethodBuilder>();

        private MemoryStream executableStream = null;

        // Properties
        public Stream ExecutableStream
        {
            get
            {
                // Return to read position
                if (executableStream != null)
                    executableStream.Position = 0;

                return executableStream;
            }
        }

        // Constructor
        public TypeBuilder(TypeModel typeModel)
        {
            this.typeModel = typeModel;

            // Add fields
            if (typeModel.MemberFields != null)
                fieldBuilders.AddRange(typeModel.MemberFields.Select(f => new FieldBuilder(f)));

            // Add methods
            if (typeModel.MemberMethods != null)
                methodBuilders.AddRange(typeModel.MemberMethods.Select(m => new MethodBuilder(m)));
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
            foreach(FieldBuilder fieldBuilder in fieldBuilders)
            {
                fieldBuilder.BuildEmitModel(writer);
            }


            // Write all methods
            foreach(MethodBuilder methodBuilder in methodBuilders)
            {
                methodBuilder.BuildEmitModel(writer);
            }

            // Get size required for this type image
            writer.Flush();
            return (int)writer.BaseStream.Position;
        }
    }
}
