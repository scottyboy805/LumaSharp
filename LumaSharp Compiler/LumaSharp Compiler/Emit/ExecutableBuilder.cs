using LumaSharp.Runtime;
using LumaSharp.Runtime.Handle;
using System.Runtime.InteropServices;

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

        public unsafe int WriteMethodExecutable(_MethodHandle methodHandle, IList<_VariableHandle> returnParameterHandles, IList<_VariableHandle> parameterHandles, IList<_VariableHandle> localHandles, MethodBodyBuilder body)
        {
            // Get the rva - relative virtual address of this method call
            int rva = (int)buffer.Position;

            // Tokens
            writer.Write(methodHandle.MethodToken.MetaToken);
            writer.Write(methodHandle.DeclaringTypeToken.MetaToken);

            // Signature
            writer.Write((ushort)methodHandle.Signature.Flags);
            writer.Write((ushort)methodHandle.Signature.ParameterCount);
            writer.Write((ushort)methodHandle.Signature.ReturnCount);

            long bodyOffset = buffer.Position;

            // Body
            writer.Write((ushort)methodHandle.Body.MaxStack);
            writer.Write((ushort)methodHandle.Body.VariableCount);
            writer.Write((uint)methodHandle.Body.InstructionsSize);


            if (body != null)
            {
                // Create bytecode builder
                BytecodeBuilder builder = new BytecodeBuilder(buffer);

                // Emit the body
                body.EmitExecutionObject(builder);

                // Flush
                buffer.Flush();

                long currentOffset = buffer.Position;

                // Return to update max stack
                buffer.Seek(bodyOffset, SeekOrigin.Begin);

                // Overwrite body
                writer.Write((ushort)body.MaxStack);
                writer.Write((ushort)methodHandle.Body.VariableCount);
                writer.Write((uint)builder.Size);

                // Return to current
                buffer.Seek(currentOffset, SeekOrigin.Begin);
            }

            // Note that these come after the instructions because it means the rva + methodHandle size is the start of bytecode
            // Write return variables
            for(int i = 0; i < methodHandle.Signature.ReturnCount; i++)
            {
                WriteVariableExecutable(returnParameterHandles[i]);
            }

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
            writer.Write((uint)variableHandle.StackOffset);
        }

        private void WriteTypeReferenceExecutable(_TypeHandle typeHandle)
        {
            // Write type token and size - Note that size can be recalculated by the runtime
            writer.Write((int)typeHandle.TypeToken.MetaToken);
            writer.Write((uint)typeHandle.TypeSize);
        }
    }
}
