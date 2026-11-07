namespace System.Reflection;

/// <summary>Base metadata for a reflected declaration.</summary>
public virtual class MemberInfo : System.Object
{
	private string name = "";
	private System.Type declaringType = typeof(MemberInfo);
	private int32 flags;
	private int32 memberId;

	public string Name { get => name; }
	public System.Type DeclaringType { get => declaringType; }
	public int32 Flags { get => flags; }
	public bool IsPublic { get => (flags & 1) != 0; }
	public bool IsStatic { get => (flags & 2) != 0; }
	public bool IsReadOnly { get => (flags & 4) != 0; }
	public bool IsVirtual { get => (flags & 8) != 0; }
	public bool IsAbstract { get => (flags & 16) != 0; }
	public bool IsExtern { get => (flags & 256) != 0; }
	public System.ReadOnlySpan<System.Attribute> Attributes { get => __attrib(this); }

	public System.Attribute? GetAttribute(System.Type? attributeType)
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

	public bool HasAttribute(System.Type? attributeType)
	{
		return GetAttribute(attributeType) != null;
	}
}
