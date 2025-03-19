using LumaSharp.Runtime.Handle;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LumaSharp.Runtime.Emit
{
    public sealed class BytecodeGenerator
    {
        // Private
        private MemoryStream instructionBuffer = null;
        private BinaryWriter instructionWriter = null;
        private bool validateInstructions = true;
        private int maxStack = 0;

        // Properties
        public int CurrentOffset
        {
            get { return (int)instructionBuffer.Position; }
        }

        public int MaxStack
        {
            get { return maxStack; }
        }

        // Constructor
        public BytecodeGenerator(MemoryStream buffer = null, bool validateInstructions = true)
        {
            this.instructionBuffer = buffer;
            this.validateInstructions = validateInstructions;

            // Check for null
            if (buffer == null)
                this.instructionBuffer = new MemoryStream(4096);

            // Create writer
            this.instructionWriter = new BinaryWriter(instructionBuffer);
        }

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            instructionBuffer.SetLength(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe byte* GenerateBytecode()
        {
            // Allocate the native memory where the instructions will be emitted
            void* mem = NativeMemory.Alloc((nuint)instructionBuffer.Length);

            // Copy to mem
            fixed(void* buffer = instructionBuffer.GetBuffer())
            {
                // Copy instructions
                NativeMemory.Copy(buffer, mem, (nuint)instructionBuffer.Length);
            }

            // Get ptr
            return (byte*)mem;
        }

        public unsafe _MethodHandle* GenerateMethod(RuntimeTypeCode[] parameterTypes, RuntimeTypeCode[] localTypes, int maxStack)
        {
            // Allocate the native memory
            void* mem = NativeMemory.Alloc((nuint)instructionBuffer.Length + (uint)sizeof(_MethodHandle));

            // Create signature
            _MethodSignatureHandle signature = new _MethodSignatureHandle((ushort)parameterTypes.Length, (ushort)localTypes.Length, null, null);

            // Create body
            _MethodBodyHandle body = new _MethodBodyHandle((ushort)maxStack, null, (ushort)localTypes.Length);

            // Create method handle
            *(_MethodHandle*)mem = new _MethodHandle(new _TokenHandle(0), new _TokenHandle(0), signature, body);

            // Append instructions
            fixed(void* buffer = instructionBuffer.GetBuffer())
            {
                // Copy instructions
                NativeMemory.Copy(buffer, (byte*)mem + sizeof(_MethodHandle), (nuint)instructionBuffer.Length);
            }

            // Get method handle ptr
            return (_MethodHandle*)mem;
        }

        #region Emit
        public void Emit(OpCode code)
        {
            // Check for argument type
            if (validateInstructions == true && code.GetOperandType() != OperandType.InlineNone)
                throw new InvalidOperationException(string.Format("Op code {0} expects {1}", code, code.GetOperandType()));

            // Emit the code
            instructionWriter.Write((byte)code);
        }

        public void Emit(OpCode code, sbyte operand)
        {
            // Check for argument type
            if (validateInstructions == true && code.GetOperandType() != OperandType.InlineI1)
                throw new InvalidOperationException(string.Format("Op code {0} expects {1} but got InlineI1", code, code.GetOperandType()));

            // Emit the code and operand
            instructionWriter.Write((byte)code);
            instructionWriter.Write(operand);
        }

        public void Emit(OpCode code, short operand)
        {
            // Check for argument type
            if (validateInstructions == true && code.GetOperandType() != OperandType.InlineI2)
                throw new InvalidOperationException(string.Format("Op code {0} expects {1} but got InlineI2", code, code.GetOperandType()));

            // Emit the code and operand
            instructionWriter.Write((byte)code);
            instructionWriter.Write(operand);
        }

        public void Emit(OpCode code, int operand)
        {
            // Check for argument type
            if (validateInstructions == true && code.GetOperandType() != OperandType.InlineI4)
                throw new InvalidOperationException(string.Format("Op code {0} expects {1} but got InlineI4", code, code.GetOperandType()));

            // Emit the code and operand
            instructionWriter.Write((byte)code);
            instructionWriter.Write(operand);
        }

        public void Emit(OpCode code, long operand)
        {
            // Check for argument type
            if (validateInstructions == true && code.GetOperandType() != OperandType.InlineI8)
                throw new InvalidOperationException(string.Format("Op code {0} expects {1} but got InlineI8", code, code.GetOperandType()));

            // Emit the code and operand
            instructionWriter.Write((byte)code);
            instructionWriter.Write(operand);
        }

        public void Emit(OpCode code, float operand)
        {
            // Check for argument type
            if (validateInstructions == true && code.GetOperandType() != OperandType.InlineF4)
                throw new InvalidOperationException(string.Format("Op code {0} expects {1} but got InlineF4", code, code.GetOperandType()));

            // Emit the code and operand
            instructionWriter.Write((byte)code);
            instructionWriter.Write(operand);
        }

        public void Emit(OpCode code, double operand)
        {
            // Check for argument type
            if (validateInstructions == true && code.GetOperandType() != OperandType.InlineF8)
                throw new InvalidOperationException(string.Format("Op code {0} expects {1} but got InlineF8", code, code.GetOperandType()));

            // Emit the code and operand
            instructionWriter.Write((byte)code);
            instructionWriter.Write(operand);
        }

        public void EmitToken(OpCode code, int token)
        {
            // Check for argument type
            if (validateInstructions == true && code.GetOperandType() != OperandType.InlineToken)
                throw new InvalidOperationException(string.Format("Op code {0} expects {1} but got InlineToken", code, code.GetOperandType()));

            // Emit the code and operand
            instructionWriter.Write((byte)code);
            instructionWriter.Write(token);
        }
        #endregion
    }
}
