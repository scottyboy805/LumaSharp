using System;
using LumaSharpVMBeef.Meta;

namespace LumaSharpVMBeef.Reflection;

public abstract class LumaMember
{
	// Protected
	protected LumaAssemblyContext assemblyContext;
	protected LumaMemberDefinition* memberDefinition;

	// Properties
	public StringView Name => assemblyContext.GetString(memberDefinition.NameToken);
	public LumaType DeclaringType => assemblyContext.GetMetaType(memberDefinition.DeclaringTypeToken);

	// Constructor
	protected this(LumaAssemblyContext assemblyContext, LumaMemberDefinition* memberDefinition)
	{
		this.assemblyContext = assemblyContext;
		this.memberDefinition = memberDefinition;
	}
}