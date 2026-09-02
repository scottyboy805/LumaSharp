using System;
using System.Collections;
using System.IO;

namespace LumaSharpVMBeef;

public class LumaBytecodeReader
{
	// Private
	private readonly List<uint8> bytecode = new .() ~delete _;

	// Properties
	public uint8* Rva => bytecode.Ptr;
	public uint32 Size => (uint32)bytecode.Count;

	// Methods
	public void ReadFile(StringView path)
	{
		// Create file stream
		FileStream fs = scope .();
		fs.Open(path);

		// Create reader
		StreamReader reader = scope .(fs);

		// Read contents
		Read(reader);

		// Close stream
		fs.Close();
	}

	public void Read(StreamReader reader)
	{
		// Clear bytes
		bytecode.Clear();

		// Open text
		String line = scope .();
		while(reader.EndOfStream == false)
		{
			// Read a line
			line.Clear();
			reader.ReadLine(line);

			// Check for empty
			if(line.IsEmpty == true)
				continue;

			// Check for colon
			if(line.Contains(':') == true)
			{
				// Read tag
				
			}

			// Split by space
			var split = line.Split(' ');

			// Parse op code
			split.MoveNext();
			let opCode = ReadOpCode(split.Current);

			// Get the operand type
			LumaOperandType operandType = opCode.GetOperandType();

			if(operandType != .None)
			{
				// Check for more
				if(split.HasMore == false)
					Runtime.FatalError(scope $"Expected operand after op code: {opCode}");

				// Move to operand
				split.MoveNext();
		
				// Parse operand
				ReadOperand(split.Current, opCode, operandType);
			}
			else
			{
				// Check for too many
				if(split.HasMore == true)
					Runtime.FatalError(scope $"Operand was provided when none is expected following op code: {opCode}");
			}
		}

	}

	private LumaOpCode ReadOpCode(StringView str)
	{
		// Try to parse op code
		let result = Enum.Parse<LumaOpCode>(str, true);

		// Check for error
		if(result case .Err)
			Runtime.FatalError(scope $"Failed to parse op code: {str}");

		// Push to bytecode
		bytecode.Add((uint8)result);

		// Get the result
		return result;
	}

	private void ReadOperand(StringView str, LumaOpCode opCode, LumaOperandType operandType)
	{
		switch(operandType)
		{
		case .InlineI1:
			{
				// Try to parse
				let result = int8.Parse(str);

				if(result case .Err)
					Runtime.FatalError(scope $"Expected I1 operand following op code {opCode}, but got: {str}");

				// Add to bytecode
				bytecode.Add((uint8)result);
			}
		case .InlineI2:
			{
				// Try to parse
				let result = int16.Parse(str);
	
				if(result case .Err)
					Runtime.FatalError(scope $"Expected I2 operand following op code {opCode}, but got: {str}");

				// Get the value
				var val = result.Value;

				// Add to bytecode
				bytecode.AddRange(*(uint8[2]*)&val);
			}
		case .InlineI4:
			{
				// Try to parse
				let result = int32.Parse(str);
	
				if(result case .Err)
					Runtime.FatalError(scope $"Expected I4 operand following op code {opCode}, but got: {str}");
	
				// Get the value
				var val = result.Value;
	
				// Add to bytecode
				bytecode.AddRange(*(uint8[4]*)&val);
			}
		case .InlineI8:
			{
				// Try to parse
				let result = int64.Parse(str);
	
				if(result case .Err)
					Runtime.FatalError(scope $"Expected I8 operand following op code {opCode}, but got: {str}");
	
				// Get the value
				var val = result.Value;
	
				// Add to bytecode
				bytecode.AddRange(*(uint8[8]*)&val);
			}
		case .InlineF4:
			{
				// Try to parse
				let result = float.Parse(str);
	
				if(result case .Err)
					Runtime.FatalError(scope $"Expected F4 operand following op code {opCode}, but got: {str}");
	
				// Get the value
				var val = result.Value;
	
				// Add to bytecode
				bytecode.AddRange(*(uint8[4]*)&val);
			}
		case .InlineF8:
			{
				// Try to parse
				let result = double.Parse(str);
	
				if(result case .Err)
					Runtime.FatalError(scope $"Expected I8 operand following op code {opCode}, but got: {str}");
	
				// Get the value
				var val = result.Value;
	
				// Add to bytecode
				bytecode.AddRange(*(uint8[8]*)&val);
			}

		default: Runtime.FatalError(scope $"Not supported operand: {operandType}");
		}
	}
}