namespace System.IO;

public class MemoryStream : Stream
{
	private uint8[] buffer;
	private int32 length;
	private int32 position;
	private bool isOpen;
	private bool isWritable;
	private bool isExpandable;

	public this()
	{
		buffer = new uint8[0];
		length = 0;
		position = 0;
		isOpen = true;
		isWritable = true;
		isExpandable = true;
	}

	public this(int32 capacity)
	{
		if (capacity < 0)
			throw new System.ArgumentOutOfRangeException();
		buffer = new uint8[capacity];
		length = 0;
		position = 0;
		isOpen = true;
		isWritable = true;
		isExpandable = true;
	}

	public this(uint8[] buffer)
	{
		this.buffer = buffer;
		length = buffer.Length;
		position = 0;
		isOpen = true;
		isWritable = false;
		isExpandable = false;
	}

	public this(uint8[] buffer, bool writable)
	{
		this.buffer = buffer;
		length = buffer.Length;
		position = 0;
		isOpen = true;
		isWritable = writable;
		isExpandable = false;
	}

	public unsafe uint8* Ptr
	{
		get => (uint8*)buffer.Ptr;
	}

	public override bool CanRead
	{
		get => isOpen;
	}

	public override bool CanWrite
	{
		get => isOpen && isWritable;
	}

	public override bool CanSeek
	{
		get => isOpen;
	}

	public override int32 Length
	{
		get
		{
			EnsureOpen();
			return length;
		}
	}

	public override int32 Position
	{
		get
		{
			EnsureOpen();
			return position;
		}
		set
		{
			EnsureOpen();
			if (value < 0)
				throw new System.ArgumentOutOfRangeException();
			position = value;
		}
	}

	public override int32 Read(uint8[] destination, int32 offset, int32 count) mut
	{
		EnsureOpen();
		ValidateBufferArguments(destination, offset, count);
		if (position >= length || count == 0)
			return 0;
		int32 available = length - position;
		int32 read = count < available ? count : available;
		for (int32 index = 0; index < read; index++)
			destination[offset + index] = buffer[position + index];
		position += read;
		return read;
	}

	public override void Write(uint8[] source, int32 offset, int32 count) mut
	{
		EnsureOpen();
		ValidateBufferArguments(source, offset, count);
		if (!isWritable)
			throw new System.NotSupportedException("This stream does not support writing.");
		if (count == 0)
			return;
		if (position > 2147483647 - count)
			throw new System.OutOfMemoryException();
		int32 end = position + count;
		EnsureCapacity(end);
		if (position > length)
			ClearRange(length, position);
		for (int32 index = 0; index < count; index++)
			buffer[position + index] = source[offset + index];
		position = end;
		if (position > length)
			length = position;
	}

	public override int32 Seek(int32 offset, SeekOrigin origin) mut
	{
		EnsureOpen();
		int32 basePosition;
		if (origin == SeekOrigin.Begin)
			basePosition = 0;
		else if (origin == SeekOrigin.Current)
			basePosition = position;
		else if (origin == SeekOrigin.End)
			basePosition = length;
		else
			throw new System.ArgumentOutOfRangeException();
		if (offset < -basePosition || offset > 2147483647 - basePosition)
			throw new System.ArgumentOutOfRangeException();
		position = basePosition + offset;
		return position;
	}

	public override void SetLength(int32 value) mut
	{
		EnsureOpen();
		if (!isWritable)
			throw new System.NotSupportedException("This stream does not support changing its length.");
		if (value < 0)
			throw new System.ArgumentOutOfRangeException();
		EnsureCapacity(value);
		if (value > length)
			ClearRange(length, value);
		else if (value < length)
			ClearRange(value, length);
		length = value;
		if (position > length)
			position = length;
	}

	public uint8[] ToArray()
	{
		uint8[] result = new uint8[length];
		for (int32 index = 0; index < length; index++)
			result[index] = buffer[index];
		return result;
	}

	public override void Flush()
	{
		EnsureOpen();
	}

	public override void Close() mut
	{
		isOpen = false;
	}

	private void ValidateBufferArguments(uint8[] value, int32 offset, int32 count)
	{
		if (offset < 0 || count < 0 || offset > value.Length - count)
			throw new System.ArgumentOutOfRangeException();
	}

	private void EnsureOpen()
	{
		if (!isOpen)
			throw new System.InvalidOperationException("The stream is closed.");
	}

	private void EnsureCapacity(int32 required) mut
	{
		if (required <= buffer.Length)
			return;
		if (!isExpandable)
			throw new System.NotSupportedException("This stream cannot be expanded.");
		int32 capacity = buffer.Length == 0 ? 256 : buffer.Length;
		while (capacity < required)
		{
			if (capacity > 1073741823)
			{
				capacity = required;
				break;
			}
			capacity *= 2;
		}
		uint8[] replacement = new uint8[capacity];
		for (int32 index = 0; index < length; index++)
			replacement[index] = buffer[index];
		buffer = replacement;
	}

	private void ClearRange(int32 start, int32 end) mut
	{
		for (int32 index = start; index < end; index++)
			buffer[index] = 0;
	}
}
