namespace System;

public virtual class Array : Object
{
	// These signatures are compiler intrinsics over managed array descriptors.
	public int32 Length { get => 0; }
	public int32 Rank { get => 0; }
	public int32 GetLength(int32 dimension) { return 0; }

	// The compiler binds these helpers for implicit one-dimensional array conversions.
	internal static unsafe Span<T> ToSpan<T>(T[] source)
	{
		if (source.Length == 0)
			return Span<T>(null, 0);
		return Span<T>(&source[0], source.Length);
	}

	internal static unsafe ReadOnlySpan<T> ToReadOnlySpan<T>(T[] source)
	{
		if (source.Length == 0)
			return ReadOnlySpan<T>(null, 0);
		return ReadOnlySpan<T>(&source[0], source.Length);
	}
}
