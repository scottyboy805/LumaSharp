namespace System;

public class Weak<T> where T : class
{
	private int control;

	public this(T target)
	{
		control = __weakcreate(target);
	}

	public bool IsAlive
	{
		get => __weakalive(control);
	}

	public T? Target
	{
		get => (T?)__weaktarget(control);
	}

	~this()
	{
		__weakrelease(control);
	}
}
