namespace System.Reflection;

/// <summary>Metadata for a computed property.</summary>
public class Property : MemberInfo
{
	public bool CanRead { get => (Flags & 32) != 0; }
	public bool CanWrite { get => (Flags & 64) != 0; }

	public object? GetValue(object? instance)
	{
		return __memberget(this, instance);
	}

	public void SetValue(object? instance, object? value)
	{
		__memberset(this, instance, value);
	}
}
