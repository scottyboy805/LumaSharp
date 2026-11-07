namespace System.Reflection;

/// <summary>Metadata for an invokable method.</summary>
public class Method : MemberInfo
{
	public bool IsMutating { get => (Flags & 128) != 0; }

	public object? Invoke(object? instance, object? ... arguments)
	{
		return __memberinvoke(this, instance, arguments);
	}
}
