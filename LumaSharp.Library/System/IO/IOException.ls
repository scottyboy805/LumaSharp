namespace System.IO;

public class IOException : System.Exception
{
	public this() { }
	public this(string message) : base(message) { }
	public this(string message, System.Exception? innerException) : base(message, innerException) { }
}
