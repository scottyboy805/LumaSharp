using LumaSharpVMBeef.Meta;

namespace LumaSharpVMBeef.Reflection;

public class LumaType : LumaMember
{
	// Private
	private LumaTypeDefinition* typeDefinition;

	// Properties
	public bool IsVirtual => (typeDefinition.Flags & .Virtual) != 0;
	public bool IsAbstract => (typeDefinition.Flags & .Abstract) != 0;
	public bool IsReferenceType => (typeDefinition.Flags & .ValueType) == 0 && (typeDefinition.Flags & .EnumType) == 0;
	public bool IsValueType => (typeDefinition.Flags & .ValueType) != 0;
	public bool IsEnumType => (typeDefinition.Flags & .EnumType) != 0;

	public LumaType BaseType => assemblyContext.GetMetaType(typeDefinition.BaseTypeToken);

	// Constructor
	internal this(LumaAssemblyContext assemblyContext, LumaTypeDefinition* typeDefnition)
		: base(assemblyContext, typeDefinition)
	{
		this.typeDefinition = typeDefinition;
	}
}