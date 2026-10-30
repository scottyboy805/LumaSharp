namespace System.IO;

public enum SeekOrigin : int32
{
	Begin = 0,
	Current = 1,
	End = 2
}

public virtual class Stream : System.Object
{
	public virtual bool CanRead
	{
		get => true;
	}

	public virtual bool CanWrite
	{
		get => true;
	}

	public virtual bool CanSeek
	{
		get => false;
	}

	public virtual int32 Length
	{
		get
		{
			throw new System.NotSupportedException("This stream does not support length.");
		}
	}

	public virtual int32 Position
	{
		get
		{
			throw new System.NotSupportedException("This stream does not support seeking.");
		}
		set
		{
			throw new System.NotSupportedException("This stream does not support seeking.");
		}
	}

	public virtual int32 Read(scope uint8[] buffer, int32 offset, int32 count);

	public int32 Read(scope uint8[] buffer) mut
	{
		return Read(buffer, 0, buffer.Length);
	}

	public uint8 Read() mut
	{
		let buffer = scope uint8[1];
		int32 read = Read(buffer, 0, 1);
		if (read == 0)
			throw new EndOfStreamException();
		return buffer[0];
	}

	public virtual void Write(scope uint8[] buffer, int32 offset, int32 count);

	public void Write(scope uint8[] buffer) mut
	{
		Write(buffer, 0, buffer.Length);
	}

	public void Write(uint8 value) mut
	{
		let buffer = scope uint8[1];
		buffer[0] = value;
		Write(buffer, 0, 1);
	}

	public virtual void Flush()
	{
	}

	public virtual int32 Seek(int32 offset, SeekOrigin origin)
	{
		throw new System.NotSupportedException("This stream does not support seeking.");
	}

	public virtual void SetLength(int32 value)
	{
		throw new System.NotSupportedException("This stream does not support changing its length.");
	}

	public void CopyTo(Stream destination) mut
	{
		CopyTo(destination, 81920);
	}

	public void CopyTo(Stream destination, int32 bufferSize) mut
	{
		if (destination == this)
			throw new System.ArgumentException("A stream cannot copy to itself.");
		if (bufferSize <= 0)
			throw new System.ArgumentOutOfRangeException();
		uint8[] buffer = new uint8[bufferSize];
		int32 read;
		while ((read = Read(buffer, 0, buffer.Length)) != 0)
			destination.Write(buffer, 0, read);
	}

	public virtual void Close()
	{
	}
}
