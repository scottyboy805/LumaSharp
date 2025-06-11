using LumaSharp.Runtime.Emit;
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Runtime.Reader
{
    public sealed class BytecodeReader : IDisposable
    {
        // Type
        private struct BytecodeOperation
        {
            // Public
            public int Offset;
            public OpCode OpCode;
            public OperandType OperandType;
            public object Operand;
            public int OperandSize;

            // Properties
            public int EndOffset => Offset + 1 + OperandSize;
        }

        private struct BytecodeLabel
        {
            // Public
            public string Label;
            public int Offset;
        }

        private struct BytecodeToken
        {
            // Public
            public string Identifier;
            public _TokenHandle ResolvedToken;
        }

        // Private
        private readonly TextReader reader;

        private List<BytecodeOperation> operations = new();
        private List<BytecodeLabel> labels = new();
        private RuntimeTypeCode[] parameterTypes = new RuntimeTypeCode[0];
        private RuntimeTypeCode[] localTypes = new RuntimeTypeCode[0];
        private int maxStack;

        // Constructor
        public BytecodeReader(TextReader reader)
        {
            // Check for null
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            this.reader = reader;

            // Begin reading
            ReadBytecode();
        }

        // Methods
        public void Dispose()
        {
            reader.Dispose();
        }

        public unsafe _MethodHandle* GenerateMethod(Func<string, _TokenHandle> resolveToken = null)
        {
            // Create the generator
            BytecodeGenerator generator = new BytecodeGenerator();

            // Emit all instructions
            for (int i = 0; i < operations.Count; i++)
            {
                // Get the operation
                BytecodeOperation operation = operations[i];

                // Check for jump
                if (operation.OpCode.IsJump() == true)
                {
                    // Try to resolve jump label
                    string jumpLabel = (string)operation.Operand;

                    // Find the label offset
                    int index = labels.FindIndex(l => l.Label == jumpLabel);

                    // Check for invalid index
                    if (index == -1)
                        throw new Exception("Could not resolve jump target: " + jumpLabel);

                    // Get offset of next instructions
                    int nextOffset = operation.EndOffset;

                    // Get the jump offset
                    int jumpOffset = labels[index].Offset;

                    // Calculate offset to target instruction
                    int relativeJumpOffset = jumpOffset - nextOffset;

                    // Emit
                    generator.Emit(operation.OpCode, relativeJumpOffset);
                }
                // Check for token
                else if(operation.OpCode.IsToken() == true)
                {
                    // Get the token
                    BytecodeToken token = (BytecodeToken)operation.Operand;

                    // Check for resolve required
                    if(token.ResolvedToken.IsNil == true)
                    {
                        // Check for absolute meta token
                        if(int.TryParse(token.Identifier, out int intToken) == true)
                        {
                            // Simply set the token direct
                            token.ResolvedToken = intToken;
                        }
                        else
                        {
                            // We need the function to resolve the token
                            if (resolveToken == null)
                                throw new InvalidOperationException("Cannot resolve metadata token because a token resolver is required: " + token.Identifier);

                            // Try to resolve
                            token.ResolvedToken = resolveToken(token.Identifier);
                        }
                    }

                    // Emit
                    generator.EmitToken(operation.OpCode, token.ResolvedToken.MetaToken);
                }
                else
                {
                    // Select type
                    switch (operation.OperandType)
                    {
                        case OperandType.InlineNone:
                            {
                                generator.Emit(operation.OpCode);
                                break;
                            }
                        case OperandType.InlineI1:
                            {
                                generator.Emit(operation.OpCode, (sbyte)operation.Operand);
                                break;
                            }
                        case OperandType.InlineI2:
                            {
                                generator.Emit(operation.OpCode, (short)operation.Operand);
                                break;
                            }
                        case OperandType.InlineI4:
                            {
                                generator.Emit(operation.OpCode, (int)operation.Operand);
                                break;
                            }
                        case OperandType.InlineI8:
                            {
                                generator.Emit(operation.OpCode, (long)operation.Operand);
                                break;
                            }
                        case OperandType.InlineF4:
                            {
                                generator.Emit(operation.OpCode, (float)operation.Operand);
                                break;
                            }
                        case OperandType.InlineF8:
                            {
                                generator.Emit(operation.OpCode, (double)operation.Operand);
                                break;
                            }
                        case OperandType.InlineToken:
                            {
                                generator.EmitToken(operation.OpCode, (int)operation.Operand);
                                break;
                            }
                    }
                }
            }

            // Build the executable method
            return generator.GenerateMethod(
                parameterTypes,
                localTypes,
                maxStack);
        }

        private void ReadBytecode()
        {
            // Read all lines
            while (reader.Peek() != -1)
            {
                // Read the line
                string line = reader.ReadLine()
                    .Trim();

                // Check for empty - ignore whitespace
                if (string.IsNullOrEmpty(line) == true)
                    continue;

                // Check for method executable info
                if (line.StartsWith(".") == true)
                {
                    // Read method info
                    ReadBytecodeMethodInfo(line);
                }
                else
                {
                    // Read operation or label
                    ReadBytecodeOperationOrLabel(line);
                }
            }
        }

        private void ReadBytecodeMethodInfo(string line)
        {
            // Check for colon
            if (line.IndexOf(':') == -1)
                throw new FormatException("Expected ':' when parsing method info: " + line);

            // Split by colon
            string[] split = line.Split(':');

            // Check for identifiers
            switch (split[0].Trim())
            {
                case ".parameters":
                    {
                        // Parse all types
                        parameterTypes = ReadTypeCodes(split[1]);
                        break;
                    }
                case ".locals":
                    {
                        // Parse all types
                        localTypes = ReadTypeCodes(split[1]);
                        break;
                    }
                case ".maxstack":
                    {
                        // Parse int
                        if (int.TryParse(split[1], out maxStack) == false)
                            throw new FormatException("Expected integer: " + split[1]);
                        break;
                    }

                default:
                    throw new FormatException("Unexpected method info: " + line);
            }
        }

        private void ReadBytecodeOperationOrLabel(string line)
        {
            // Check for label
            if (line.IndexOf(':') != -1)
            {
                // We have a label
                string labelName = line.Replace(":", "")
                    .Trim();

                // Get the current offset
                int offset = operations.Count > 0
                    ? operations[operations.Count - 1].EndOffset
                    : 0;

                // Append the label
                labels.Add(new BytecodeLabel
                {
                    Label = labelName,
                    Offset = offset
                });
            }
            else
            {
                // Split by comma
                string[] split = line.Split(',');

                // Try to parse opcode
                if (Enum.TryParse(split[0], true, out OpCode opCode) == false)
                    throw new FormatException("Invalid Op Code: " + split[0]);

                // Get operand type
                OperandType operandType = opCode.GetOperandType();

                string operandText = split.Length > 1 ? split[1].Trim() : null;
                object operand = null;

                // Check for operand required
                if (operandType != OperandType.InlineNone && string.IsNullOrEmpty(operandText) == true)
                    throw new FormatException("Expected operand: " + line);

                // Jump instructions use labels for ease of use
                if (opCode.IsJump() == true)
                {
                    // Get the label to jump to
                    operand = operandText;
                }
                else if(opCode.IsToken() == true)
                {
                    // Try to parse
                    _TokenHandle resolvedToken = default;

                    // We can resolve runtime type codes here
                    if(operandText.All(char.IsDigit) == false && Enum.TryParse(operandText, true, out RuntimeTypeCode typeCode) == true)
                        resolvedToken = typeCode;

                    // Create token
                    operand = new BytecodeToken
                    {
                        Identifier = operandText,
                        ResolvedToken = resolvedToken,
                    };
                }
                else
                {
                    // Try to parse operand
                    switch (operandType)
                    {
                        case OperandType.InlineI1:
                            {
                                // Parse the byte
                                if (sbyte.TryParse(operandText, out sbyte i1) == false)
                                    throw new FormatException("Expected operand I1: " + operandText);

                                operand = i1;
                                break;
                            }
                        case OperandType.InlineI2:
                            {
                                // Parse the short
                                if (short.TryParse(operandText, out short i2) == false)
                                    throw new FormatException("Expected operand I2: " + operandText);

                                operand = i2;
                                break;
                            }
                        case OperandType.InlineI4:
                        case OperandType.InlineToken:
                            {
                                // Parse the int
                                if (int.TryParse(operandText, out int i4) == false)
                                    throw new FormatException("Expected operand I4: " + operandText);

                                operand = i4;
                                break;
                            }
                        case OperandType.InlineI8:
                            {
                                // Parse the byte
                                if (long.TryParse(operandText, out long i8) == false)
                                    throw new FormatException("Expected operand I8: " + operandText);

                                operand = i8;
                                break;
                            }
                        case OperandType.InlineF4:
                            {
                                // Parse the byte
                                if (float.TryParse(operandText, out float f4) == false)
                                    throw new FormatException("Expected operand F4: " + operandText);

                                operand = f4;
                                break;
                            }
                        case OperandType.InlineF8:
                            {
                                // Parse the byte
                                if (double.TryParse(operandText, out double f8) == false)
                                    throw new FormatException("Expected operand F8: " + operandText);

                                operand = f8;
                                break;
                            }
                    }
                }

                // Get the current offset
                int offset = operations.Count > 0
                    ? operations[operations.Count - 1].EndOffset
                    : 0;

                // Get the op code size
                int size = opCode.GetOperandSize();

                // Create the instruction
                operations.Add(new BytecodeOperation
                {
                    Offset = offset,
                    OpCode = opCode,
                    OperandType = operandType,
                    Operand = operand,
                    OperandSize = size
                });
            }
        }

        private RuntimeTypeCode[] ReadTypeCodes(string source)
        {
            // Args is a comma separated list of types
            string[] types = source.Split(',');

            // Parse all
            RuntimeTypeCode[] typeCodes = new RuntimeTypeCode[types.Length];

            // Process all
            for (int i = 0; i < types.Length; i++)
            {
                // Get the trimmed string
                string type = types[i].Trim();

                // Try to parse
                if (Enum.TryParse(type, true, out typeCodes[i]) == false)
                    throw new FormatException("Unknown type code: " + type);
            }
            return typeCodes;
        }
    }
}
