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

	internal void AppendAsyncStackTrace(string parentTrace) mut
	{
		if (parentTrace.Length == 0)
			return;

		if (stackTrace != null && stackTrace!.Length > 0)
		{
			int32 firstFrameEnd = parentTrace.IndexOf('\n');
			if (firstFrameEnd >= 0)
			{
				string firstFrame = parentTrace.Substring(0, firstFrameEnd + 1);
				bool duplicateFrame = firstFrame.Length <= stackTrace!.Length;
				int32 frameOffset = stackTrace!.Length - firstFrame.Length;
				for (int32 index = 0; duplicateFrame && index < firstFrame.Length; index++)
					if (stackTrace![frameOffset + index] != firstFrame[index])
						duplicateFrame = false;
				if (duplicateFrame)
					parentTrace = parentTrace.Substring(firstFrame.Length);
			}
		}

		if (parentTrace.Length == 0)
			return;

		if (stackTrace == null || stackTrace.Length == 0)
			stackTrace = parentTrace;
		else
			stackTrace += "\n--- End of stack trace from previous location ---\n" + parentTrace;
	}
}
