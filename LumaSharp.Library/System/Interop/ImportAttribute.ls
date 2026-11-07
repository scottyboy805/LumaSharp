using System;
namespace System.Interop;

/// <summary>Describes the native module and symbol associated with an extern method.</summary>
public class ImportAttribute : Attribute
{
	public readonly string ModuleName;
	public readonly string EntryPointName;

	public this(string moduleName, string entryPointName)
	{
		ModuleName = moduleName;
		EntryPointName = entryPointName;
	}
}
