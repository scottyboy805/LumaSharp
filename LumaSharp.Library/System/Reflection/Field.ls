namespace System.Reflection;

/// <summary>Metadata for a field declaration.</summary>
public class Field : MemberInfo
{
	public object? GetValue(object? instance)
	{
		return __memberget(this, instance);
	}

	public void SetValue(object? instance, object? value)
	{
		__memberset(this, instance, value);
	}
}
