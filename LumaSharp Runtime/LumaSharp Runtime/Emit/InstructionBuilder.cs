using LumaSharp.Runtime;
using System.Runtime.InteropServices;

namespace LumaSharp.Runtime.Emit
{
    internal struct Instruction
    {
        // Public
        public int index;
        public int offset;
        public OpCode opCode;
        public object data0;
        public object data1;
        public int dataSize = 0;

        // Constructor
        public Instruction(int index, int offset, OpCode opCode)
        {
            this.index = index;
            this.offset = offset;
            this.opCode = opCode;
        }

        public Instruction(int index, int offset, OpCode opCode, object data)
        {
            this.index = index;
            this.offset = offset;
            this.opCode = opCode;
            this.data0 = data;
            this.dataSize = Marshal.SizeOf(data);
        }

        public Instruction(int index, int offset, OpCode opCode, object data0, object data1)
        {
            this.index = index;
            this.offset = offset;
            this.opCode = opCode;
            this.data0 = data0;
            this.data1 = data1;
            this.dataSize = Marshal.SizeOf(data0) + Marshal.SizeOf(data1);
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

    internal class InstructionBuilder
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

        public Instruction Last
        {
            get
            {
                if(instructions.Count > 0)
                    return instructions[instructions.Count - 1];

                return default;
            }
        }

        // Constructor
        public InstructionBuilder(BinaryWriter writer)
        {
            this.writer = writer;
            this.instructions = new List<Instruction>();
        }

        // Methods
        public Instruction EmitOpCode(OpCode code)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != 0)
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, (int)writer.BaseStream.Position, code));

            writer.Write((byte)code);
            instructionIndex++;

            // Get last instruction
            return instructions[instructions.Count - 1];
        }

        public Instruction EmitOpCode(OpCode code, sbyte data)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(sbyte))
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, (int)writer.BaseStream.Position, code, data));

            writer.Write((byte)code);
            writer.Write(data);
            instructionIndex++;

            // Get last instruction
            return instructions[instructions.Count - 1];
        }

        public Instruction EmitOpCode(OpCode code, byte data)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(byte))
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, (int)writer.BaseStream.Position, code, data));

            writer.Write((byte)code);
            writer.Write(data);
            instructionIndex++;

            // Get last instruction
            return instructions[instructions.Count - 1];
        }

        public Instruction EmitOpCode(OpCode code, short data)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(short))
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, (int)writer.BaseStream.Position, code, data));

            writer.Write((byte)code);
            writer.Write(data);
            instructionIndex++;

            // Get last instruction
            return instructions[instructions.Count - 1];
        }

        public Instruction EmitOpCode(OpCode code, ushort data)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(ushort))
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, (int)writer.BaseStream.Position, code, data));

            writer.Write((byte)code);
            writer.Write(data);
            instructionIndex++;

            // Get last instruction
            return instructions[instructions.Count - 1];
        }

        public Instruction EmitOpCode(OpCode code,  int data)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(int))
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, (int)writer.BaseStream.Position, code, data));

            writer.Write((byte)code);
            writer.Write(data);
            instructionIndex++;

            // Get last instruction
            return instructions[instructions.Count - 1];
        }

        public Instruction EmitOpCode(OpCode code, uint data)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(uint))
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, (int)writer.BaseStream.Position, code, data));

            writer.Write((byte)code);
            writer.Write(data);
            instructionIndex++;

            // Get last instruction
            return instructions[instructions.Count - 1];
        }

        public Instruction EmitOpCode(OpCode code, long data)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(long))
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, (int)writer.BaseStream.Position, code, data));

            writer.Write((byte)code);
            writer.Write(data);
            instructionIndex++;

            // Get last instruction
            return instructions[instructions.Count - 1];
        }

        public Instruction EmitOpCode(OpCode code, ulong data)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(ulong))
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, (int)writer.BaseStream.Position, code, data));

            writer.Write((byte)code);
            writer.Write(data);
            instructionIndex++;

            // Get last instruction
            return instructions[instructions.Count - 1];
        }

        public Instruction EmitOpCode(OpCode code, float data)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(float))
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, (int)writer.BaseStream.Position, code, data));

            writer.Write((byte)code);
            writer.Write(data);
            instructionIndex++;

            // Get last instruction
            return instructions[instructions.Count - 1];
        }

        public Instruction EmitOpCode(OpCode code, double data)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(double))
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, (int)writer.BaseStream.Position, code, data));

            writer.Write((byte)code);
            writer.Write(data);
            instructionIndex++;

            // Get last instruction
            return instructions[instructions.Count - 1];
        }

        public Instruction EmitOpCode(OpCode code, byte data0, int data1)
        {
            if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(byte) + sizeof(int))
                throw new InvalidOperationException("Invalid instruction");

            // Add op code
            instructions.Add(new Instruction(instructionIndex, (int)writer.BaseStream.Position, code, data0, data1));

            writer.Write((byte)code);
            writer.Write(data0);
            writer.Write(data1);
            instructionIndex++;

            // Get last instruction
            return instructions[instructions.Count - 1];
        }

        //public Instruction EmitOpCode(OpCode code, IReferenceSymbol symbol)
        //{
        //    if (OpCodeCheck.GetOpCodeDataSize(code) != sizeof(int))
        //        throw new InvalidOperationException("Invalid instruction");

        //    // Add op code
        //    instructions.Add(new Instruction(instructionIndex, (int)writer.BaseStream.Position, code, symbol));

        //    writer.Write((byte)code);
        //    writer.Write(symbol.SymbolToken);
        //    instructionIndex++;

        //    // Get last instruction
        //    return instructions[instructions.Count - 1];
        //}

        public void ModifyOpCode(Instruction instruction, int data)
        {
            // Check size
            if (instruction.dataSize != sizeof(int))
                throw new InvalidOperationException("invalid modification");

            // Get current position
            long position = writer.BaseStream.Position;

            // Return to op code
            writer.BaseStream.Seek(instruction.offset + 1, SeekOrigin.Begin);
            writer.Write(data);

            // Return to last
            writer.BaseStream.Seek(position, SeekOrigin.Begin);

            // Update instruction
            instruction.data0 = data;

            // Update instruction
            instructions[instruction.index] = instruction;
        }
    }
}
