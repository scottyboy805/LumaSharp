namespace System;

public virtual class Array : Object
{
	// These signatures are compiler intrinsics over managed array descriptors.
	public int32 Length { get => 0; }
	public int32 Rank { get => 0; }
	public int32 GetLength(int32 dimension) { return 0; }
	public unsafe void* Ptr { get => null; }

	// The compiler binds these helpers for implicit one-dimensional array conversions.
	internal static unsafe Span<T> ToSpan<T>(T[] source)
	{
		if (source.Length == 0)
			return default(Span<T>);
		return Span<T>(source.Ptr, source.Length);
	}

	internal static unsafe ReadOnlySpan<T> ToReadOnlySpan<T>(T[] source)
	{
		if (source.Length == 0)
			return default(ReadOnlySpan<T>);
		return ReadOnlySpan<T>(source.Ptr, source.Length);
	}
}
