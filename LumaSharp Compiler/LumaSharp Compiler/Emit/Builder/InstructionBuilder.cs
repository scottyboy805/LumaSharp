using LumaSharp.Runtime;
using LumaSharp_Compiler.Semantics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LumaSharp_Compiler.Emit.Builder
{
    internal sealed class Instruction
    {
        // Public
        public int index;
        public OpCode opCode;
        public object data0;
        public object data1;

        // Constructor
        public Instruction(int index, OpCode opCode)
        {
            this.index = index;
            this.opCode = opCode;
        }

        public Instruction(int index, OpCode opCode, object data)
        {
            this.index = index;
            this.opCode = opCode;
            this.data0 = data;
        }

        public Instruction(int index, OpCode opCode, object data0, object data1)
        {
            this.index = index;
            this.opCode = opCode;
            this.data0 = data0;
            this.data1 = data1;
        }

        // Methods
        public override string ToString()
        {
            if(data0 != null && data1 != null)
            {
                return string.Format("{0}: {1}: {2}, {3}", index, data0, data1);
            }
            else if(data0 != null)
            {
                return string.Format("{0}: {1}: {2}", index, data0);
            }
            else
            {
                return string.Format("{0}: {1}", index, opCode);
            }    
        }
    }

    internal sealed class InstructionBuilder
    {
        // Private
        private BinaryWriter writer = null;
        private List<Instruction> instructions = null;
        private int instructionIndex = 0;

        // Properties
        public int InstructionIndex
        {
            get { return instructionIndex; }
        }

        public Instruction this[int index]
        {
            get { return instructions[index]; }
        }        

        // Constructor
        public InstructionBuilder(BinaryWriter writer)
        {
            this.writer = writer;
            this.instructions = new List<Instruction>();
        }

        // Methods
        public void EmitOpCode(OpCode code)
        {
            // Add op code
            instructions.Add(new Instruction(instructionIndex, code));

            writer.Write((byte)code);
            instructionIndex++;
        }

        public void EmitOpCode(OpCode code, byte data)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(byte))
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, code, data));

            writer.Write((byte)code);
            writer.Write(data);
            instructionIndex++;
        }

        public void EmitOpCode(OpCode code, ushort data)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(ushort))
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, code, data));

            writer.Write((byte)code);
            writer.Write(data);
            instructionIndex++;
        }

        public void EmitOpCode(OpCode code,  int data)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(int))
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, code, data));

            writer.Write((byte)code);
            writer.Write(data);
            instructionIndex++;
        }

        public void EmitOpCode(OpCode code, uint data)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(uint))
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, code, data));

            writer.Write((byte)code);
            writer.Write(data);
            instructionIndex++;
        }

        public void EmitOpCode(OpCode code, long data)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(long))
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, code, data));

            writer.Write((byte)code);
            writer.Write(data);
            instructionIndex++;
        }

        public void EmitOpCode(OpCode code, ulong data)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(ulong))
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, code, data));

            writer.Write((byte)code);
            writer.Write(data);
            instructionIndex++;
        }

        public void EmitOpCode(OpCode code, float data)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(float))
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, code, data));

            writer.Write((byte)code);
            writer.Write(data);
            instructionIndex++;
        }

        public void EmitOpCode(OpCode code, double data)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(double))
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, code, data));

            writer.Write((byte)code);
            writer.Write(data);
            instructionIndex++;
        }

        public void EmitOpCode(OpCode code, byte data0, int data1)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(byte) + sizeof(int))
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, code, data0, data1));

            writer.Write((byte)code);
            writer.Write(data0);
            writer.Write(data1);
            instructionIndex++;
        }

        public void EmitOpCode(OpCode code, IReferenceSymbol symbol)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(int))
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, code, symbol));

            writer.Write((byte)code);
            writer.Write(symbol.SymbolToken);
            instructionIndex++;
        }
    }
}
