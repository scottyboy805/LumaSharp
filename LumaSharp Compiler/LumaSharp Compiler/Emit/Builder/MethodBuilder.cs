using LumaSharp.Runtime;
using LumaSharp.Runtime.Emit;
using LumaSharp.Runtime.Handle;
using LumaSharp_Compiler.Semantics.Model;
using System.Runtime.CompilerServices;

namespace LumaSharp_Compiler.Emit.Builder
{
    internal unsafe sealed class MethodBuilder
    {
        // Private
        private MethodModel methodModel = null;
        private MethodBodyBuilder bodyBuilder = null;

        private MemoryStream executableStream = null;

        // Constructor
        public MethodBuilder(MethodModel methodModel)
        {
            this.methodModel = methodModel;
            this.bodyBuilder = new MethodBodyBuilder(methodModel.HasBody == true ? methodModel.BodyStatements : null);
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

            // Create instruction builder
            InstructionBuilder builder = new InstructionBuilder(writer);

            // Get method handle
            _MethodHandle handle = methodModel.MethodHandle;

            _StackHandle[] argLocalHandles = handle.argLocals;
            handle.instructionPtr = null;

            // Write method handle
            EmitUtil.WriteStruct(writer, handle);

            // Emit locals
            for(int i = 0; i < argLocalHandles.Length; i++)
                EmitUtil.WriteStruct(writer, argLocalHandles[i]);


            // Emit instructions
            bodyBuilder.BuildEmitObject(builder);

            // Get size
            writer.Flush();
            return (int)executableStream.Length;
        }

        public void WriteToStream(Stream stream)
        {
            // Copy to target stream
            if(executableStream != null)
                executableStream.CopyTo(stream);
        }
    }
}
