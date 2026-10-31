namespace System;

public virtual class Array : Object
{
	public int32 Length { get => __arrlen(this); }
	public int32 Rank { get => __arrrank(this); }
	public int32 GetLength(int32 dimension) { return __arrdim(this, dimension); }
	public unsafe void* Ptr { get => __arrptr(this); }

	/// <summary>Copies all elements from source to destination.</summary>
	public static void Copy(Array source, Array destination)
	{
		CopyInternal(source, destination);
	}

	/// <summary>Copies a row-major linear section between arrays of the same element type and rank.</summary>
	public static void Copy(Array source, int32 sourceIndex, Array destination, int32 destinationIndex, int32 length)
	{
		CopyInternal(source, sourceIndex, destination, destinationIndex, length);
	}

	/// <summary>Fills the array with the default value of its element type.</summary>
	public static void Clear(Array array)
	{
		ClearInternal(array);
	}

	/// <summary>Clears a row-major linear section of an array.</summary>
	public static void Clear(Array array, int32 index, int32 length)
	{
		ClearInternal(array, index, length);
	}

	private static void CopyInternal(Array? source, Array? destination)
	{
		if (source == null) throw new ArgumentNullException();
		CopyInternal(source, 0, destination, 0, source.Length);
	}

	private static void CopyInternal(Array? source, int32 sourceIndex, Array? destination, int32 destinationIndex, int32 length)
	{
		if (source == null) throw new ArgumentNullException();
		if (destination == null) throw new ArgumentNullException();
		if (sourceIndex < 0) throw new ArgumentOutOfRangeException();
		if (destinationIndex < 0) throw new ArgumentOutOfRangeException();
		if (length < 0) throw new ArgumentOutOfRangeException();
		if (sourceIndex > source.Length || destinationIndex > destination.Length)
			throw new ArgumentException("An array index is outside the array bounds.");
		if (length > source.Length - sourceIndex || length > destination.Length - destinationIndex)
			throw new ArgumentException("The requested array section does not fit in the source or destination.");
		if (!__arraysametype(source, destination))
			throw new ArrayTypeMismatchException();
		__arraycopy(source, sourceIndex, destination, destinationIndex, length);
	}

	private static void ClearInternal(Array? array)
	{
		if (array == null) throw new ArgumentNullException();
		ClearInternal(array, 0, array.Length);
	}

	private static void ClearInternal(Array? array, int32 index, int32 length)
	{
		if (array == null) throw new ArgumentNullException();
		if (index < 0) throw new ArgumentOutOfRangeException();
		if (length < 0) throw new ArgumentOutOfRangeException();
		if (index > array.Length || length > array.Length - index)
			throw new ArgumentException("The requested array section is outside the array bounds.");
		__arrayclear(array, index, length);
	}

	// The compiler binds these helpers for implicit one-dimensional array conversions.
	internal static unsafe Span<T> ToSpan<T>(T[] source)
	{
		if (source.Length == 0)
			return Span<T>.Empty;
		return Span<T>(source.Ptr, source.Length, source);
	}

	internal static unsafe ReadOnlySpan<T> ToReadOnlySpan<T>(T[] source)
	{
		if (source.Length == 0)
			return ReadOnlySpan<T>.Empty;
		return ReadOnlySpan<T>(source.Ptr, source.Length, source);
	}
}
