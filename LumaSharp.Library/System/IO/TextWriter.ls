namespace System.IO;

public virtual class TextWriter : System.Object
{
	public virtual void Write(string? value);

	public virtual void Write(char value)
	{
		Write(value.ToString());
	}

	public virtual void Write(int32 value)
	{
		Write(value.ToString());
	}

	public virtual void Write(bool value)
	{
		Write(value.ToString());
	}

	public virtual void WriteLine()
	{
		Write("\n");
	}

	public virtual void WriteLine(string? value)
	{
		Write(value);
		WriteLine();
	}

	public virtual void WriteLine(char value)
	{
		Write(value);
		WriteLine();
	}

	public virtual void WriteLine(int32 value)
	{
		Write(value);
		WriteLine();
	}

	public virtual void WriteLine(bool value)
	{
		Write(value);
		WriteLine();
	}

	public virtual void Flush()
	{
	}

	public virtual void Close()
	{
		Flush();
	}
}
