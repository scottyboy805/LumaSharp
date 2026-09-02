using System;

namespace LumaSharpVMBeef.Meta;

public struct LumaMemberDefinition
{
	// Private
	private LumaToken nameToken;
	private LumaToken declaringTypeToken;

	// Properties
	public LumaToken NameToken => nameToken;
	public LumaToken DeclaringTypeToken => declaringTypeToken;

	// Constructor
	protected this(LumaToken nameToken, LumaToken declaringTypeToken)
	{
		this.nameToken = nameToken;
		this.declaringTypeToken = declaringTypeToken;
	}
}