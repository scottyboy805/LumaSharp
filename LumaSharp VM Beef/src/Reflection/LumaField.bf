using System;
using LumaSharpVMBeef.Meta;

namespace LumaSharpVMBeef.Reflection;

public sealed class LumaField : LumaMember
{
	// Private
	private LumaFieldDefinition* fieldDefinition;

	// Properties
	public LumaType FieldType => assemblyContext.GetMetaType(fieldDefinition.FieldTypeToken);

	// Constructor
	internal this(LumaAssemblyContext assemblyContext, LumaFieldDefinition* fieldDefinition)
		: base(assemblyContext, fieldDefinition)
	{
		this.fieldDefinition = fieldDefinition;
	}
}