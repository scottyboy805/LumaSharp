namespace System;

public virtual class Array : Object
{
	// These signatures are compiler intrinsics over managed array descriptors.
	public int32 Length { get => 0; }
	public int32 Rank { get => 0; }
	public int32 GetLength(int32 dimension) { return 0; }
}
