using System;
using LumaSharpVMBeef.Meta;

namespace LumaSharpVMBeef;

public struct LumaFieldHandle
{
	// Public
	public LumaTypeHandle* declaringType;
	public LumaTypeHandle* fieldType;
	public LumaFieldFlags flags;

	// Methods
	[Inline]
	public void* FieldGetAddress(void* fieldBaseAddr) mut
	{
		// Get the field offset
		uint32 offset = declaringType.TypeGetFieldOffset(&this);

		// Get the address into the provided memory
		return ((uint8*)fieldBaseAddr) + offset;
	}
}