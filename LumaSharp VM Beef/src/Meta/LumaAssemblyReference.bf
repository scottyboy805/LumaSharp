namespace LumaSharpVMBeef.Meta;

public struct LumaAssemblyReference
{
	// Private
	private LumaToken nameToken;
	private int32 majorVersion;
	private int32 minorVersion;
	private int32 buildVersion;

	// Properties
	public LumaToken NameToken => nameToken;
	public int32 MajorVersion => majorVersion;
	public int32 MinorVersion => minorVersion;
	public int32 BuildVersion => buildVersion;
}