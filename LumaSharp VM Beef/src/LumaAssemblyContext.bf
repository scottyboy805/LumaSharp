using System;
using LumaSharpVMBeef.Reflection;

namespace LumaSharpVMBeef;

public sealed class LumaAssemblyContext
{
	// Private
	private LumaAssemblyReader reader;

	private LumaType[] typeDefinitionMetas;
	private LumaMethod[] methodDefinitionMetas;

	private LumaTypeHandle[] typeHandles;

	// Methods
	[Inline]
	public String GetString(LumaToken token)
	{
		// Try to get the string
		return reader.GetString(token);
	}

	[Inline]
	public LumaTypeHandle* GetTypeHandle(LumaToken token)
	{
		switch(token.Kind)
		{
		case .TypeDefinition:
			{
				// Get type declaration handle
				return &typeHandles[token.Index];
			}

			// Invalid token
		default: return null;
		}
	}

	public LumaType GetMetaType(LumaToken token)
	{
		switch(token.Kind)
		{
		case .TypeDefinition: return typeDefinitionMetas[token.Index];

			// Invalid token
		default: return null;
		}
	}

	public LumaMethod GetMetaMethod(LumaToken token)
	{
		switch(token.Kind)
		{
		case .MethodDefinition: return methodDefinitionMetas[token.Index];

			// Invalid token
		default: return null;
		}
	}
}