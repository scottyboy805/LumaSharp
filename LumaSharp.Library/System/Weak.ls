namespace System;

public class Weak<T> where T : class
{
	private int control;

	public this(T target)
	{
		control = CreateControlRuntime(target);
	}

	public bool IsAlive
	{
		get => IsAliveRuntime(control);
	}

	public T? Target
	{
		get => GetTargetRuntime(control);
	}

	~this()
	{
		ReleaseControlRuntime(control);
	}

	private static int CreateControlRuntime(T target)
	{
		return 0;
	}

	private static bool IsAliveRuntime(int handle)
	{
		return false;
	}

	private static T? GetTargetRuntime(int handle)
	{
		return null;
	}

	private static void ReleaseControlRuntime(int handle)
	{
	}
}
