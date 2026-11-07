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
	public ReadOnlySpan<System.Attribute> Attributes { get => __attrib(this); }
	public int32 AttributeCount { get => Attributes.Length; }

	public Type? GetAttributeType(int32 index)
	{
		if (index < 0 || index >= AttributeCount)
			return null;
		return Attributes[index].GetType();
	}

	public Attribute? GetAttribute(int32 index)
	{
		if (index < 0 || index >= AttributeCount)
			return null;
		return Attributes[index];
	}

	public Attribute? GetAttribute(Type? attributeType)
	{
		if (attributeType == null)
			return null;
		var attributes = Attributes;
		for (int32 index = 0; index < attributes.Length; index++)
		{
			let attribute = attributes[index];
			if (attribute.GetType() == attributeType)
				return attribute;
		}
		return null;
	}

	public bool HasAttribute(Type? attributeType)
	{
		return GetAttribute(attributeType) != null;
	}

	public ReadOnlySpan<System.Reflection.Field> GetFields()
	{
		return __typefields(this);
	}

	public ReadOnlySpan<System.Reflection.Property> GetProperties()
	{
		return __typeproperties(this);
	}

	public ReadOnlySpan<System.Reflection.Method> GetMethods()
	{
		return __typemethods(this);
	}

	public ReadOnlySpan<System.Reflection.Constructor> GetConstructors()
	{
		return __typeconstructors(this);
	}

	public System.Reflection.Field? GetField(string name)
	{
		var fields = GetFields();
		for (int32 index = fields.Length - 1; index >= 0; index--)
			if (fields[index].Name == name)
				return fields[index];
		return null;
	}

	public System.Reflection.Property? GetProperty(string name)
	{
		var properties = GetProperties();
		System.Reflection.Property? found = null;
		for (int32 index = 0; index < properties.Length; index++)
			if (properties[index].Name == name)
			{
				if (found != null)
					throw new InvalidOperationException("More than one reflected property has the requested name.");
				found = properties[index];
			}
		return found;
	}

	public System.Reflection.Method? GetMethod(string name)
	{
		var methods = GetMethods();
		System.Reflection.Method? found = null;
		for (int32 index = 0; index < methods.Length; index++)
			if (methods[index].Name == name)
			{
				if (found != null)
					throw new InvalidOperationException("More than one reflected method has the requested name.");
				found = methods[index];
			}
		return found;
	}

	public override string ToString()
	{
		return Namespace.Length == 0 ? Name : Namespace + "." + Name;
	}

	public object New(object? ... arguments)
	{
		return __typenew(this, arguments);
	}
}
