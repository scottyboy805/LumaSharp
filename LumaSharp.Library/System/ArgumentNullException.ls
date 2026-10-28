namespace System;

public class ArgumentNullException : ArgumentException
{
	public this()
	{
	}

	public this(string message) : base(message)
	{
	}
}
