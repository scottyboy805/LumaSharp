using System;
namespace System.Interop;

/// <summary>Describes the native module and symbol associated with an extern method.</summary>
public class ImportAttribute : Attribute
{
	public string ModuleName;
	public string EntryPointName;

	public this(string moduleName, string entryPointName)
	{
		ModuleName = moduleName;
		EntryPointName = entryPointName;
	}
}
