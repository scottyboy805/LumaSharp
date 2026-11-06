namespace System.IO;

public virtual class AsyncStream : Stream
{
	public virtual async int32 ReadAsync(uint8[] buffer, int32 offset, int32 count);

	public virtual async void WriteAsync(uint8[] buffer, int32 offset, int32 count);
}
