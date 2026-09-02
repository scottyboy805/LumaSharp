using System;
using LumaSharpVMBeef.Meta;

namespace LumaSharpVMBeef.Reflection;

public sealed class LumaMethod : LumaMember
{
	// Private
	private LumaMethodDefinition* methodDefinition;

	// Properties
	public LumaType ReturnType => assemblyContext.GetMetaType(methodDefinition.ReturnTypeToken);

	// Constructor
	internal this(LumaAssemblyContext assemblyContext, LumaMethodDefinition* methodDefinition)
		: base(assemblyContext, methodDefinition)
	{
		this.methodDefinition = methodDefinition;
	}
}