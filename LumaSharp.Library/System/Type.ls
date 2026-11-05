namespace System;

/// <summary>Runtime metadata for a LumaSharp type.</summary>
public class Type : Object
{
	private string name = "";
	private string namespaceName = "";
	private int32 flags;
	private int32 typeId;

	private this() { }

	public string Name { get => name; }
	public string Namespace { get => namespaceName; }
	public bool IsStatic { get => (flags & 1) != 0; }
	public bool IsArray { get => (flags & 2) != 0; }
	public bool IsClass { get => (flags & 4) != 0; }
	public bool IsValueType { get => (flags & 8) != 0; }
	public bool IsPrimitive { get => (flags & 16) != 0; }
	public bool IsInterface { get => (flags & 32) != 0; }
	public bool IsEnum { get => (flags & 64) != 0; }
	public bool IsDelegate { get => (flags & 128) != 0; }
	public bool IsAbstract { get => (flags & 256) != 0; }

	public override string ToString()
	{
		return Namespace.Length == 0 ? Name : Namespace + "." + Name;
	}

	public object New(object? ... arguments)
	{
		return __typenew(this, arguments);
	}
}
