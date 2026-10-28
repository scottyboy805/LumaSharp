namespace System;

public unsafe struct Span<T>
{
	private T* pointer;
	private int32 length;

	public this(T* pointer, int32 length)
	{
		if (length < 0)
			throw new System.ArgumentOutOfRangeException();
		this.pointer = pointer;
		this.length = length;
	}

	public int32 Length
	{
		get => length;
	}

	public bool IsEmpty
	{
		get => length == 0;
	}

	public T* Ptr
	{
		get => pointer;
	}

	public T this[int32 index]
	{
		get
		{
			return pointer[index];
		}
		set
		{
			pointer[index] = value;
		}
	}

	public Span<T> Slice(int32 start)
	{
		return Slice(start, length - start);
	}

	public Span<T> Slice(int32 start, int32 sliceLength)
	{
		if (start < 0 || start > length || sliceLength < 0 || sliceLength > length - start)
			throw new System.ArgumentOutOfRangeException();
		return Span<T>(pointer + start, sliceLength);
	}

	public ReadOnlySpan<T> AsReadOnlySpan()
	{
		return ReadOnlySpan<T>(pointer, length);
	}
}
