namespace LumaSharpVMBeef.Meta;

public struct LumaMemberReference
{
	// Private
	private LumaToken assemblyToken;
	private LumaToken nameToken;

	// Properties
	public LumaToken AssemblyToken => assemblyToken;
	public LumaToken NameToken => nameToken;


}