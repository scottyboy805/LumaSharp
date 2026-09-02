using System;
using LumaSharpVMBeef.Meta;

namespace LumaSharpVMBeef;

public sealed class LumaAssemblyReader
{
	// Private
	private String[] strings;
	private LumaAssemblyReference assemblyName;
	private LumaAssemblyReference[] assemblyReferences;

	private LumaTypeDefinition[] typeDefinitions;
	private LumaTypeReference[] typeReferences;
	private LumaTypeSpecification[] typeSpecifications;

	private LumaMethodDefinition[] methodDefinitions;

	// Properties
	public LumaAssemblyReference AssemblyName => assemblyName;
	public LumaAssemblyReference[] AssemblyReferences => assemblyReferences;

	// Methods
	[Inline]
	public String GetString(LumaToken token)
	{
		// Check valid token
		if(token.Kind != .StringReference || token.Index >= strings.Count)
			return null;

		// Get the value
		return strings[token.Index];
	}

	public LumaAssemblyReference* GetAssemblyReference(LumaToken token)
	{
		// Check valid token
		if(token.Kind != .AssemblyReference || token.Index >= assemblyReferences.Count)
			return null;

		// Get the value
		return &assemblyReferences[token.Index];
	}

	public LumaTypeDefinition* GetTypeDefinition(LumaToken token)
	{
		// Check valid token
		if(token.Kind != .TypeDefinition || token.Index >= strings.Count)
			return null;

		// Get the value
		return &typeDefinitions[token.Index];
	}

	public LumaTypeReference* GetTypeReference(LumaToken token)
	{
		// Check valid token
		if(token.Kind != .TypeReference || token.Index >= typeReferences.Count)
			return null;

		// Get the value
		return &typeReferences[token.Index];
	}

	public LumaTypeSpecification* GetTypeSpecification(LumaToken token)
	{
		// Check valid token
		if(token.Kind != .TypeSpecification || token.Index >= typeSpecifications.Count)
			return null;

		// Get the value
		return &typeSpecifications[token.Index];
	}

	public LumaMethodDefinition* GetMethodDefinition(LumaToken token)
	{
		// Check valid token
		if(token.Kind != .MethodDefinition || token.Index >= strings.Count)
			return null;

		// Get the value
		return &methodDefinitions[token.Index];
	}
}