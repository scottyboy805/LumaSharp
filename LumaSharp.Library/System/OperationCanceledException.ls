namespace System;

public class OperationCanceledException : Exception
{
	public this() : base("The operation was canceled.") { }

	public this(string message) : base(message) { }
}
