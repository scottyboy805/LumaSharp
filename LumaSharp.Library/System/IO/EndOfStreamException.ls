namespace System.IO;

public class EndOfStreamException : IOException
{
	public this()
	{
	}

	public this(string message) : base(message)
	{
	}
}
