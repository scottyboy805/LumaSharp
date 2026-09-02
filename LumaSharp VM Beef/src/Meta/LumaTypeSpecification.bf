namespace LumaSharpVMBeef.Meta;

public enum LumaTypeSpecificationFlags : uint32
{
	None = 0,

	Pointer = 1 << 0,
	ByRef = 1 << 1,
	Array = 1 << 2,
	Generic = 1 << 3,
}

public struct LumaTypeSpecification
{
	// Private
	private LumaToken typeToken;
	private LumaTypeSpecificationFlags flags;
	private uint16 arrayRank;
	private uint16 genericCount;

	// Properties
	public LumaToken TypeToken => typeToken;
	public LumaTypeSpecificationFlags Flags => flags;
}