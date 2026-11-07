namespace System.Reflection;

/// <summary>Metadata for a class or struct constructor.</summary>
public class Constructor : MemberInfo
{
	public object? Invoke(object? ... arguments)
	{
		return __memberinvoke(this, null, arguments);
	}
}
