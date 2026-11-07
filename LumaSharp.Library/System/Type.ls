namespace System;

/// <summary>Runtime metadata for a LumaSharp type.</summary>
public class Type : Object
{
	private string name = "";
	private string namespaceName = "";
	private int32 flags;
	private System.Type? elementType = null;
	private int32 typeId;
	private System.Type? baseType = null;
	private System.Type? enumUnderlyingType = null;
	public static readonly System.Binder Binder = new System.Binder();

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
	internal System.Type? EnumUnderlyingType { get => enumUnderlyingType; }
	internal bool Is64BitNativeInteger { get => (flags & 2048) != 0; }
	public System.Type? BaseType
	{
		get
		{
			System.Type? underlyingType = ElementType;
			if (IsNullable && underlyingType != null)
				return underlyingType.BaseType;
			return baseType;
		}
	}
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

	public ReadOnlySpan<System.Type> GetInterfaces()
	{
		System.Type? underlyingType = ElementType;
		if (IsNullable && underlyingType != null)
			return underlyingType.GetInterfaces();
		return __typeinterfaces(this);
	}

	/// <summary>Returns whether source can be implicitly assigned to this type.</summary>
	public bool IsAssignableFrom(System.Type? source)
	{
		if (source == null)
			return false;
		if (this == source)
			return true;

		if (IsNullable)
		{
			System.Type? destinationElement = ElementType;
			System.Type? sourceElement = source.IsNullable ? source.ElementType : source;
			return destinationElement != null && sourceElement != null && destinationElement.IsAssignableFrom(sourceElement);
		}
		if (source.IsNullable)
		{
			System.Type? sourceElement = source.ElementType;
			if (sourceElement == null)
				return false;

			// Nullable value types box their underlying value when assigned to
			// object or an interface implemented by that value type. A nullable
			// reference cannot be assigned to a non-nullable destination without
			// a flow proof, which is not available to this type-only query.
			if (source.IsValueType)
			{
				if (Namespace == "System" && Name == "Object")
					return true;
				return IsInterface && IsAssignableFrom(sourceElement);
			}
			return false;
		}

		// All reference types and boxable value types have an implicit conversion to object.
		if (Namespace == "System" && Name == "Object")
			return source.IsClass || source.IsInterface || source.IsArray || source.IsDelegate ||
				(source.IsValueType && source.HasDefault);

		System.Type? currentBase = source.BaseType;
		while (currentBase != null)
		{
			if (currentBase == this)
				return true;
			currentBase = currentBase.BaseType;
		}

		var interfaces = source.GetInterfaces();
		for (int32 index = 0; index < interfaces.Length; index++)
			if (interfaces[index] == this)
				return true;
		return false;
	}

	/// <summary>Returns whether this type can be implicitly assigned to destination.</summary>
	public bool IsAssignableTo(System.Type? destination)
	{
		if (destination == null)
			return false;
		return destination.IsAssignableFrom(this);
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

	public System.Reflection.Method? GetMethod(string name, System.Type? ... parameterTypes)
	{
		return GetMethod(name, System.Type.Binder, parameterTypes);
	}

	public System.Reflection.Method? GetMethod(string name, System.Binder? binder, System.Type? ... parameterTypes)
	{
		System.Binder selectedBinder = binder ?? System.Type.Binder;
		return selectedBinder.BindToMethod(this, name, parameterTypes);
	}

	public System.Reflection.Constructor? GetConstructor(System.Type? ... parameterTypes)
	{
		return GetConstructor(System.Type.Binder, parameterTypes);
	}

	public System.Reflection.Constructor? GetConstructor(System.Binder? binder, System.Type? ... parameterTypes)
	{
		System.Binder selectedBinder = binder ?? System.Type.Binder;
		return selectedBinder.BindToConstructor(this, parameterTypes);
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

}
