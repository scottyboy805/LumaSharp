using System;

namespace LumaSharpVMBeef;

public struct LumaClass
{
	// Public
	public LumaTypeHandle* type;
	public void* fieldData;
}