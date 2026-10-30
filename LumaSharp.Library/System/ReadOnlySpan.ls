namespace System;

public unsafe ref struct ReadOnlySpan<T>
{
	private T* pointer;
	private int32 length;
	private object? owner;

	public static ReadOnlySpan<T> Empty
	{
		get => default;
	}

	public this(T* pointer, int32 length)
	{
		if (length < 0)
			throw new System.ArgumentOutOfRangeException();
		this.pointer = pointer;
		this.length = length;
		this.owner = null;
	}

	public this(T* pointer, int32 length, object? owner)
	{
		if (length < 0)
			throw new System.ArgumentOutOfRangeException();
		this.pointer = pointer;
		this.length = length;
		this.owner = owner;
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
			if (index < 0 || index >= length)
				throw new System.ArgumentOutOfRangeException();
			return unchecked(pointer[index]);
		}
	}

	public ReadOnlySpan<T> Slice(int32 start)
	{
		return Slice(start, length - start);
	}

	public ReadOnlySpan<T> Slice(int32 start, int32 sliceLength)
	{
		if (start < 0 || start > length || sliceLength < 0 || sliceLength > length - start)
			throw new System.ArgumentOutOfRangeException();
		return ReadOnlySpan<T>(pointer + start, sliceLength, owner);
	}
}
