namespace LumaSharpVMBeef.Meta;

public enum LumaFieldFlags: uint32
{
	None = 0,

	// Not an instance method
	Static = 1 << 0,
	// Can be used by referencing assemblies
	Export = 1 << 1,
	// Can be used by referencing assembly, from a derived type
	Inherit = 1 << 2,

	Const = 1 << 3,
	Readonly = 1 << 4,
}

public struct LumaFieldDefinition : LumaMemberDefinition
{
	// Private
	private LumaFieldFlags flags;
	private LumaToken fieldTypeToken;
	private void* constantRva;

	// Properties
	public LumaFieldFlags Flags => flags;
	public LumaToken FieldTypeToken => fieldTypeToken;
	public void* ConstantRva => constantRva;

	// Constructor
	internal this(LumaToken nameToken, LumaToken declaringTypeToken, LumaFieldFlags flags, LumaToken fieldTypeToken, void* constantRva)
		: base(nameToken, declaringTypeToken)
	{
		this.flags = flags;
		this.fieldTypeToken = fieldTypeToken;
		this.constantRva = constantRva;
	}
}