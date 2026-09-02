using System;
using internal LumaSharpVMBeef;

namespace LumaSharpVMBeef;

class Program
{
	public static void Main()
	{
		// Create reader
		LumaBytecodeReader reader = scope .();
		reader.ReadFile("bytecode.txt");

		//uint8[] bytecode = scope .
		//(
		//	0x10, 11,	// Ld I1
		//	0x10, 16,
		//	0x71,		// Add
		//	0xfb,		// ret
		//);

		LumaMethodHandle method = .
		{
			maxStack = 2,
			rva = reader.Rva,
			size = reader.Size,
		};


		// Create thread
		LumaThreadContext threadContext = scope .(512);

		// Execute method
		threadContext.PrepareCall(&method);
		threadContext.Execute();

		// Get return value
		int32 result = threadContext.PopReturn<int32>();

		Console.WriteLine($"Result = {result}");

		Console.WriteLine("Hello");
		Console.Read();
	}
}