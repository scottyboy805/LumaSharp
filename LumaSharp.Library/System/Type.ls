namespace System;

/// <summary>Runtime metadata for a LumaSharp type.</summary>
public class Type : Object
{
	private string name = "";
	private string namespaceName = "";
	private int32 flags;
	private System.Type? elementType = null;
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
	public bool IsNullable { get => (flags & 512) != 0; }
	public bool HasDefault { get => (flags & 1024) != 0; }
	public System.Type? ElementType { get => elementType; }
	public ReadOnlySpan<System.Attribute> Attributes
	{
		get
		{
			System.Type? underlyingType = ElementType;
			if (IsNullable && underlyingType != null)
				return underlyingType.Attributes;
			return __attrib(this);
		}
	}
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
		System.Type? underlyingType = ElementType;
		if (IsNullable && underlyingType != null)
			return underlyingType.GetFields();
		return __typefields(this);
	}

	public ReadOnlySpan<System.Reflection.Property> GetProperties()
	{
		System.Type? underlyingType = ElementType;
		if (IsNullable && underlyingType != null)
			return underlyingType.GetProperties();
		return __typeproperties(this);
	}

	public ReadOnlySpan<System.Reflection.Method> GetMethods()
	{
		System.Type? underlyingType = ElementType;
		if (IsNullable && underlyingType != null)
			return underlyingType.GetMethods();
		return __typemethods(this);
	}

	public ReadOnlySpan<System.Reflection.Constructor> GetConstructors()
	{
		System.Type? underlyingType = ElementType;
		if (IsNullable && underlyingType != null)
			return underlyingType.GetConstructors();
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
		System.Type? underlyingType = ElementType;
		if (IsNullable && !IsValueType && underlyingType != null)
			return __typenew(underlyingType, arguments);
		return __typenew(this, arguments);
	}

	/// <summary>Returns this type's default value boxed as an object.</summary>
	public object? Default()
	{
		if (!HasDefault)
			throw new InvalidOperationException("The represented type has no default value.");
		return __typedefault(this);
	}

	/// <summary>Returns this type's default value in its concrete type without boxing.</summary>
	public T Default<T>() where T : default
	{
		// The default constraint guarantees that the closed T has a default. Nullable
		// reference annotations intentionally share their runtime Type handle.
		if (this != typeof(T))
			throw new InvalidOperationException("The requested type does not match a type with a default value.");
		return default(T);
	}
}
