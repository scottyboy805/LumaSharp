namespace System;

public virtual class Object
{
	public this()
	{
	}

	public Type GetType()
	{
		return __typeref(this);
	}
}
