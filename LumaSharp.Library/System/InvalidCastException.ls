namespace System;

public class InvalidCastException : Exception
{
	public this() { }
	public this(string message) : base(message) { }
	public this(string message, Exception? innerException) : base(message, innerException) { }
}
