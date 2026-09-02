namespace LumaSharpVMBeef;

public struct LumaMethodSignature
{
	// Public
	public int32 parameterCount;			// The number of arguments
	public int32 parameterSize;				// Size in bytes required for all arguments
	public LumaStackHandle* parameters;		// The parameters of this signature
}

public struct LumaStackHandle
{
	// Public
	public LumaTypeHandle* type;
	public uint32 offset;
}

public struct LumaMethodHandle
{
	public LumaToken token;
	public LumaAssemblyContext assembly;	// The assembly context where this method is loaded
	public int32 maxStack;					// The max stack value required by this method
	public int32 localCount;				// Number of local variables
	public int32 localSize;					// Size in bytes required for all locals
	public LumaStackHandle* locals;			// The local variables of this method body
	public LumaMethodSignature signature;	// The signature for this method
	public uint8* rva;						// Byte code instructions memory (Relative virtual address)
	public uint32 size;						// Size of bytecode in bytes
}