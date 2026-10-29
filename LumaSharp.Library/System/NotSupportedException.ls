namespace System;

public class NotSupportedException : Exception
{
	public this()
	{
	}

	public this(string message) : base(message)
	{
	}
}
