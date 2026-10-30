namespace System;

public virtual class Exception : Object
{
	public string Message { get; private set; } = "";
	public Exception? InnerException { get; private set; }
	private string? stackTrace;
	public string? StackTrace
	{
		get => stackTrace;
		private set => stackTrace = value;
	}

	public this() { }
	public this(string message) { this.Message = message; }
	public this(string message, Exception? innerException)
	{
		this.Message = message;
		this.InnerException = innerException;
	}
}
