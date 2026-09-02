using System;
using LumaSharpVMBeef.Meta;

namespace LumaSharpVMBeef;

public struct LumaTypeHandle
{
	// Public
	public LumaTypeCode code;
	public uint32 size;
	public LumaTypeFlags flags;
	public int fieldCount;
	public LumaFieldHandle* fields;

	// Methods
	[Inline]
	public uint32 TypeGetFieldOffset(LumaFieldHandle* field)
	{
		return 0;
	}
}