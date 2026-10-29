namespace System.IO;

public unsafe class FileStream : Stream
{
	private int handle;
	private bool isOpen;
	private bool canRead;
	private bool canWrite;

	public this(string path, FileMode mode)
	{
		int32 nativeMode;
		if (mode == FileMode.CreateNew)
			nativeMode = NativeFileSystem.CreateNewMode;
		else if (mode == FileMode.Open)
			nativeMode = NativeFileSystem.OpenMode;
		else if (mode == FileMode.Create || mode == FileMode.Truncate)
		{
			if (mode == FileMode.Truncate && !File.Exists(path))
				throw new IOException("The file to truncate does not exist.");
			nativeMode = NativeFileSystem.CreateMode;
		}
		else if (mode == FileMode.Append)
			nativeMode = NativeFileSystem.AppendMode;
		else if (mode == FileMode.OpenOrCreate)
			nativeMode = NativeFileSystem.OpenOrCreateMode;
		else
			throw new System.ArgumentOutOfRangeException();

		void* openedHandle = null;
		if (NativeFileSystem.luma_fs_open(path.Ptr, nativeMode, &openedHandle) != 0)
			throw new IOException("Unable to open the file.");
		handle = (int)openedHandle;

		isOpen = true;
		canRead = mode != FileMode.Append;
		canWrite = mode != FileMode.Open;
	}

	~this()
	{
		if (isOpen)
			NativeFileSystem.luma_fs_close((void*)handle);
	}

	public override bool CanRead
	{
		get => isOpen && canRead;
	}

	public override bool CanWrite
	{
		get => isOpen && canWrite;
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
			int32 initialPosition = Position;
			int32 endPosition = 0;
			int32 end = NativeFileSystem.luma_fs_seek((void*)handle, 0, (int32)SeekOrigin.End, &endPosition);
			if (end != 0)
				throw new IOException("Unable to get the file length.");
			int32 length = endPosition;
			if (NativeFileSystem.luma_fs_seek((void*)handle, initialPosition, (int32)SeekOrigin.Begin, &endPosition) != 0)
				throw new IOException("Unable to restore the file position.");
			return length;
		}
	}

	public override int32 Position
	{
		get
		{
			EnsureOpen();
			int32 current = 0;
			if (NativeFileSystem.luma_fs_seek((void*)handle, 0, (int32)SeekOrigin.Current, &current) != 0)
				throw new IOException("Unable to get the file position.");
			return current;
		}
		set
		{
			EnsureOpen();
			if (value < 0)
				throw new System.ArgumentOutOfRangeException();
			int32 position = 0;
			if (NativeFileSystem.luma_fs_seek((void*)handle, value, (int32)SeekOrigin.Begin, &position) != 0)
				throw new IOException("Unable to set the file position.");
		}
	}

	public override int32 Read(scope uint8[] buffer, int32 offset, int32 count) mut
	{
		EnsureOpen();
		ValidateBufferArguments(buffer, offset, count);
		if (!canRead)
			throw new System.NotSupportedException("This stream does not support reading.");
		if (count == 0)
			return 0;
		int32 bytesRead = 0;
		uint8* start = (uint8*)buffer.Ptr + offset;
		if (NativeFileSystem.luma_fs_read((void*)handle, start, count, &bytesRead) != 0)
			throw new IOException("Unable to read from the file.");
		return bytesRead;
	}

	public override void Write(scope uint8[] buffer, int32 offset, int32 count) mut
	{
		EnsureOpen();
		ValidateBufferArguments(buffer, offset, count);
		if (!canWrite)
			throw new System.NotSupportedException("This stream does not support writing.");
		if (count == 0)
			return;
		int32 bytesWritten = 0;
		uint8* start = (uint8*)buffer.Ptr + offset;
		if (NativeFileSystem.luma_fs_write((void*)handle, start, count, &bytesWritten) != 0 || bytesWritten != count)
			throw new IOException("Unable to write to the file.");
	}

	public override int32 Seek(int32 offset, SeekOrigin origin) mut
	{
		EnsureOpen();
		int32 position = 0;
		if (NativeFileSystem.luma_fs_seek((void*)handle, offset, (int32)origin, &position) != 0)
			throw new System.ArgumentOutOfRangeException();
		return position;
	}

	public override void SetLength(int32 value) mut
	{
		EnsureOpen();
		if (!canWrite)
			throw new System.NotSupportedException("This stream does not support changing its length.");
		if (value < 0)
			throw new System.ArgumentOutOfRangeException();
		if (NativeFileSystem.luma_fs_set_length((void*)handle, value) != 0)
			throw new IOException("Unable to change the file length.");
	}

	public override void Flush() mut
	{
		EnsureOpen();
		if (NativeFileSystem.luma_fs_flush((void*)handle) != 0)
			throw new IOException("Unable to flush the file.");
	}

	public override void Close() mut
	{
		if (!isOpen)
			return;
		isOpen = false;
		canRead = false;
		canWrite = false;
		if (NativeFileSystem.luma_fs_close((void*)handle) != 0)
			throw new IOException("Unable to close the file.");
		handle = 0;
	}

	private void EnsureOpen()
	{
		if (!isOpen)
			throw new System.InvalidOperationException("The stream is closed.");
	}

	private void ValidateBufferArguments(scope uint8[] buffer, int32 offset, int32 count)
	{
		if (offset < 0 || count < 0 || offset > buffer.Length - count)
			throw new System.ArgumentOutOfRangeException();
	}
}
