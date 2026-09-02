namespace LumaSharpVMBeef.Meta;

public struct LumaTypeReference : LumaMemberReference
{
	// Private
	private LumaToken namespaceToken;

	// Properties
	public LumaToken NamespaceToken => namespaceToken;
}