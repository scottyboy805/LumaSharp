namespace System;

public virtual class Array : Object
{
	public int32 Length { get => __arrlen(this); }
	public int32 Rank { get => __arrrank(this); }
	public int32 GetLength(int32 dimension) { return __arrdim(this, dimension); }
	public unsafe void* Ptr { get => __arrptr(this); }

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
