namespace System.IO;

public virtual class TextReader : System.Object
{
	public virtual int32 Read();

	public virtual int32 Peek()
	{
		throw new System.NotSupportedException("This reader does not support peeking.");
	}

	public virtual void Close()
	{
	}
}
