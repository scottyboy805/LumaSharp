using LumaSharp.Runtime;
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Emit
{
    internal sealed class ExecutableBuilder : IDisposable
    {
        // Private
        private MemoryStream buffer = null;
        private BinaryWriter writer = null;

        // Constructor
        internal ExecutableBuilder(MemoryStream buffer = null)
        {
            // Check for null
            if (buffer == null)
                buffer = new MemoryStream();

            this.buffer = buffer;
            this.writer = new BinaryWriter(buffer);
        }

        // Methods
        public void Dispose()
        {
            writer.Dispose();
        }

        public int WriteTypeExecutable(_TypeHandle typeHandle)
        {
            // Get the rva - relative virtual address of this method call
            int rva = (int)buffer.Position;

            // Type reference
            WriteTypeReferenceExecutable(typeHandle);

            return rva;
        }

        public int WriteMethodExecutable(_MethodHandle methodHandle, _VariableHandle[] parameterHandles, _VariableHandle[] localHandles, MethodBodyBuilder body)
        {
            // Get the rva - relative virtual address of this method call
            int rva = (int)buffer.Position;

            // Tokens
            writer.Write(methodHandle.MethodToken.MetaToken);
            writer.Write(methodHandle.DeclaringTypeToken.MetaToken);

            // Signature
            writer.Write((uint)methodHandle.Signature.Flags);
            writer.Write((ushort)methodHandle.Signature.ParameterCount);
            writer.Write((ushort)methodHandle.Signature.ReturnCount);

            // Body
            writer.Write((ushort)methodHandle.Body.MaxStack);
            writer.Write((ushort)methodHandle.Body.VariableCount);


            // Write method body
            writer.Write(body != null);

            if(body != null)
            {
                // Create bytecode builder
                BytecodeBuilder builder = new BytecodeBuilder(buffer);

                // Emit the body
                body.EmitExecutionObject(builder);

                // Flush
                buffer.Flush();
            }

            // Note that these come after the instructions because it means the rva + methodHandle size is the start of bytecode
            // Write parameter variables
            for(int i = 0; i < methodHandle.Signature.ParameterCount; i++)
            {
                WriteVariableExecutable(parameterHandles[i]);
            }

            // Write local variables
            for(int i = 0; i < methodHandle.Body.VariableCount; i++)
            {
                WriteVariableExecutable(localHandles[i]);
            }
            return rva;
        }

        private void WriteVariableExecutable(_VariableHandle variableHandle)
        {
            // Write the variable type
            WriteTypeReferenceExecutable(variableHandle.TypeHandle);

            // Write the offset - Note that the offset can be recalculated by the runtime
            writer.Write(variableHandle.StackOffset);
        }

        private void WriteTypeReferenceExecutable(_TypeHandle typeHandle)
        {
            // Write type token and size - Note that size can be recalculated by the runtime
            writer.Write(typeHandle.TypeToken.MetaToken);
            writer.Write(typeHandle.TypeSize);
        }
    }
}
