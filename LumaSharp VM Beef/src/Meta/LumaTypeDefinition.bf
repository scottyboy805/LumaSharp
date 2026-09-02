namespace LumaSharpVMBeef.Meta;

public enum LumaTypeFlags : uint32
{
	None = 0,

	// Not an instance member
	Static = 1 << 0,
	// Can be used by referencing assemblies
	Export = 1 << 1,
	// Can be used by referencing assembly, from a derived type
	Inherit = 1 << 2,
	// Type can be derived from
	Virtual = 1 << 3,
	// Type must be derived from
	Abstract = 1 << 4,

	// Type is a struct
	ValueType = 1 << 5,
	// Type is an enum
	EnumType = 1 << 6,
}

public struct LumaTypeDefinition : LumaMemberDefinition
{
	// Private
	private LumaTypeFlags flags;
	private LumaToken baseTypeToken;

	// Properties
	public LumaTypeFlags Flags => flags;
	public LumaToken BaseTypeToken => baseTypeToken;

	// Constructor
	internal this(LumaToken nameToken, LumaToken declaringTypeToken, LumaTypeFlags flags, LumaToken baseTypeToken)
		: base(nameToken, declaringTypeToken)
	{
		this.flags = flags;
		this.baseTypeToken = baseTypeToken;
	}
}