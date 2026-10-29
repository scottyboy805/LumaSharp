namespace System.IO;

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
			return ThrowUnsupportedLength();
		}
	}

	public virtual int32 Position
	{
		get
		{
			return ThrowUnsupportedPosition();
		}
		set
		{
			ThrowUnsupportedOperation();
		}
	}

	public virtual int32 Read(uint8[] buffer, int32 offset, int32 count) mut;

	public int32 Read(uint8[] buffer) mut
	{
		return Read(buffer, 0, buffer.Length);
	}

	public int32 ReadByte() mut
	{
		uint8[] buffer = new uint8[1];
		int32 read = Read(buffer, 0, 1);
		if (read == 0)
			return -1;
		return buffer[0];
	}

	public virtual void Write(uint8[] buffer, int32 offset, int32 count) mut;

	public void Write(uint8[] buffer) mut
	{
		Write(buffer, 0, buffer.Length);
	}

	public void WriteByte(uint8 value) mut
	{
		uint8[] buffer = new uint8[1];
		buffer[0] = value;
		Write(buffer, 0, 1);
	}

	public virtual void Flush()
	{
	}

	public virtual int32 Seek(int32 offset, SeekOrigin origin) mut
	{
		ThrowUnsupportedOperation();
		return 0;
	}

	public virtual void SetLength(int32 value) mut
	{
		ThrowUnsupportedOperation();
	}

	public void ReadExactly(uint8[] buffer, int32 offset, int32 count) mut
	{
		if (offset < 0 || count < 0 || offset > buffer.Length - count)
			throw new System.ArgumentOutOfRangeException();
		int32 totalRead = 0;
		while (totalRead < count)
		{
			int32 read = Read(buffer, offset + totalRead, count - totalRead);
			if (read == 0)
				throw new EndOfStreamException();
			totalRead += read;
		}
	}

	public void CopyTo(Stream destination) mut
	{
		CopyTo(destination, 81920);
	}

	public void CopyTo(Stream destination, int32 bufferSize) mut
	{
		if (destination == null)
			throw new System.ArgumentNullException();
		if (destination == this)
			throw new System.ArgumentException("A stream cannot copy to itself.");
		if (bufferSize <= 0)
			throw new System.ArgumentOutOfRangeException();
		uint8[] buffer = new uint8[bufferSize];
		int32 read;
		while ((read = Read(buffer, 0, buffer.Length)) != 0)
			destination.Write(buffer, 0, read);
	}

	public virtual void Close() mut
	{
		Dispose();
	}

	public virtual void Dispose() mut
	{
	}

	private int32 ThrowUnsupportedLength()
	{
		throw new System.NotSupportedException("This stream does not support length.");
	}

	private int32 ThrowUnsupportedPosition()
	{
		throw new System.NotSupportedException("This stream does not support seeking.");
	}

	private void ThrowUnsupportedOperation()
	{
		throw new System.NotSupportedException("This stream does not support this operation.");
	}
}
