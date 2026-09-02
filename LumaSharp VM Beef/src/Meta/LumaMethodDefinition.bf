using System;

namespace LumaSharpVMBeef.Meta;

public enum LumaMethodFlags : uint32
{
	None = 0,

	// Not an instance method
	Static = 1 << 0,
	// Can be used by referencing assemblies
	Export = 1 << 1,
	// Can be used by referencing assembly, from a derived type
	Inherit = 1 << 2,
	// Method can be derived from
	Virtual = 1 << 3,
	// Method must be derived from
	Abstract = 1 << 4,

	// Returns a value
	Return = 1 << 5,
	// Method accepts 1 or more parameters
	ParamList = 1 << 6,
	// Method is a constructor
	Initializer = 1 << 7,
}

public struct LumaMethodDefinition : LumaMemberDefinition
{
	// Private
	private LumaMethodFlags flags;
	private LumaToken returnTypeToken;
	private void* rva;

	// Properties
	public LumaMethodFlags Flags => flags;
	public LumaToken ReturnTypeToken => returnTypeToken;
	public void* Rva => rva;

	// Constructor
	internal this(LumaToken nameToken, LumaToken declaringtypeToken, LumaMethodFlags flags, LumaToken returnTypeToken, void* rva)
		: base(nameToken, declaringtypeToken)
	{
		this.flags = flags;
		this.returnTypeToken = returnTypeToken;
		this.rva = rva;
	}
}