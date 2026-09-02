using System;

namespace LumaSharpVMBeef;

public struct LumaArray
{
	// Public
	public LumaTypeHandle* elementType;
	public uint64 length;
	public void* arrayData;

	// Methods
	[Inline]
	public void* ArrayGetElementAddress(uint64 index)
	{
		// Get offset
		uint64 offset = (index * length);

		// Get ptr
		return ((uint8*)arrayData) + offset;
	}
}