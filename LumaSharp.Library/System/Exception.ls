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

	public override string ToString()
	{
		string result = base.ToString();
		if (Message.Length > 0)
			result += ": " + Message;

		Exception? innerException = InnerException;
		if (innerException != null)
		{
			result += "\n ---> " + innerException.ToString();
			result += "\n   --- End of inner exception stack trace ---";
		}

		if (StackTrace != null)
			result += "\n" + StackTrace;

		return result;
	}

	internal void hidden_AppendAsyncStackTrace(string parentTrace) mut
	{
		if (parentTrace.Length == 0)
			return;

		if (stackTrace == null || stackTrace.Length == 0)
			stackTrace = parentTrace;
		else
			stackTrace += "\n--- End of stack trace from previous location ---\n" + parentTrace;
	}
}
