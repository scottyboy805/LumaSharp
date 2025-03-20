using LumaSharp.Compiler.Semantics;
using LumaSharp.Runtime;
using LumaSharp.Runtime.Emit;

namespace LumaSharp.Compiler.Emit
{
    internal struct BytecodeOperation
    {
        // Public
        public int Offset;
        public OpCode OpCode;
        public OperandType OperandType;
        public int OperandSize;
        public object Operand;

        // Properties
        public int EndOffset
        {
            get { return Offset + OperandSize; }
        }

        // Constructor
        public BytecodeOperation(int offset, OpCode opCode, OperandType operandType, int operandSize, object operand = null)
        {
            this.Offset = offset;
            this.OpCode = opCode;
            this.OperandType = operandType;
            this.OperandSize = operandSize;
            this.Operand = operand;
        }
        // Methods
        public override string ToString()
        {
            if(OperandType != OperandType.InlineNone)
            {
                return string.Format("{0}: {1}: {2}", Offset, OpCode, Operand);
            }

            return string.Format("{0}: {1}", Offset, Operand);
        }
    }

    internal sealed class BytecodeBuilder
    {
        // Private
        private MemoryStream buffer = null;
        private BytecodeGenerator generator = null;
        private List<BytecodeOperation> operations = null;
        private int baseOffset = 0;

        // Properties
        public int CurrentOffset
        {
            get { return generator.CurrentOffset - baseOffset; }
        }

        public BytecodeOperation this[int index]
        {
            get { return operations[index]; }
        }

        public int Count
        {
            get { return operations.Count; }
        }

        public int Size
        {
            get { return generator.Size; }
        }

        public BytecodeOperation First
        {
            get 
            {
                // Check for none
                if (operations.Count == 0)
                    return new BytecodeOperation(0, OpCode.Nop, OperandType.InlineNone, 0);

                return operations[0]; 
            }
        }

        public BytecodeOperation Last
        {
            get 
            {
                // Check for none
                if (operations.Count == 0)
                    return new BytecodeOperation(0, OpCode.Nop, OperandType.InlineNone, 0);

                return operations[operations.Count - 1]; 
            }
        }

        public int MaxStack
        {
            get { return generator.MaxStack; }
        }

        // Constructor
        public BytecodeBuilder(MemoryStream buffer = null)
        {
            // Check for null
            if (buffer == null)
                buffer = new MemoryStream();

            this.buffer = buffer;
            this.generator = new BytecodeGenerator(buffer);
            this.operations = new List<BytecodeOperation>();

            // Get base offset
            this.baseOffset = (int)buffer.Position;
        }

        // Methods
        public BytecodeOperation Emit(OpCode code)
        {
            // Get the offset
            int offset = CurrentOffset;

            // Emit the opcode
            generator.Emit(code);

            // Get the operation
            BytecodeOperation op = new BytecodeOperation(offset, code, OperandType.InlineNone, 0);

            // Add operation
            operations.Add(op);
            return op;
        }

        public BytecodeOperation Emit(OpCode code, sbyte operand)
        {
            // Get the offset
            int offset = CurrentOffset;

            // Emit the opcode with operand
            generator.Emit(code, operand);

            // Get the operation
            BytecodeOperation op = new BytecodeOperation(offset, code, OperandType.InlineI1, 1, operand);

            // Add operation
            operations.Add(op);
            return op;
        }

        public BytecodeOperation Emit(OpCode code, short operand)
        {
            // Get the offset
            int offset = CurrentOffset;

            // Emit the opcode with operand
            generator.Emit(code, operand);

            // Get the operation
            BytecodeOperation op = new BytecodeOperation(offset, code, OperandType.InlineI2, 2, operand);

            // Add operation
            operations.Add(op);
            return op;
        }

        public BytecodeOperation Emit(OpCode code, int operand)
        {
            // Get the offset
            int offset = CurrentOffset;

            // Emit the opcode with operand
            generator.Emit(code, operand);

            // Get the operation
            BytecodeOperation op = new BytecodeOperation(offset, code, OperandType.InlineI4, 4, operand);

            // Add operation
            operations.Add(op);
            return op;
        }

        public BytecodeOperation Emit(OpCode code, long operand)
        {
            // Get the offset
            int offset = CurrentOffset;

            // Emit the opcode with operand
            generator.Emit(code, operand);

            // Get the operation
            BytecodeOperation op = new BytecodeOperation(offset, code, OperandType.InlineI8, 8, operand);

            // Add operation
            operations.Add(op);
            return op;
        }

        public BytecodeOperation Emit(OpCode code, float operand)
        {
            // Get the offset
            int offset = CurrentOffset;

            // Emit the opcode with operand
            generator.Emit(code, operand);

            // Get the operation
            BytecodeOperation op = new BytecodeOperation(offset, code, OperandType.InlineF4, 4, operand);

            // Add operation
            operations.Add(op);
            return op;
        }

        public BytecodeOperation Emit(OpCode code, double operand)
        {
            // Get the offset
            int offset = CurrentOffset;

            // Emit the opcode with operand
            generator.Emit(code, operand);

            // Get the operation
            BytecodeOperation op = new BytecodeOperation(offset, code, OperandType.InlineF8, 8, operand);

            // Add operation
            operations.Add(op);
            return op;
        }

        public BytecodeOperation EmitToken(OpCode code, IReferenceSymbol symbol)
        {
            // Check for null
            if(symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            // Get the offset
            int offset = CurrentOffset;

            // Emit the opcode with operand
            generator.EmitToken(code, symbol.SymbolToken.MetaToken);

            // Get the operation
            BytecodeOperation op = new BytecodeOperation(offset, code, OperandType.InlineToken, 4, symbol.SymbolToken);

            // Add operation
            operations.Add(op);
            return op;
        }

        public BytecodeOperation ModifyOperand(BytecodeOperation operation, int operand)
        {
            // Get the current buffer
            byte[] memory = buffer.GetBuffer();

            // Get the bytes
            byte[] bytes = BitConverter.GetBytes(operand);

            // Insert bytes
            for (int i = operation.Offset + 1, j = 0; i < bytes.Length; i++, j++)
            {
                // Copy into buffer
                memory[i] = bytes[j];
            }

            // Update operation
            operation.Operand = operand;
            return operation;
        }
    }
}
