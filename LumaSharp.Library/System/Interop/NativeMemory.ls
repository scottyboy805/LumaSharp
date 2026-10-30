namespace System.Interop;

public static unsafe class NativeMemory
{
	public static void* Allocate(int byteCount)
	{
		if (byteCount < 0)
			throw new System.ArgumentOutOfRangeException();

		return NativeMemoryRuntime.luma_interop_memory_allocate(byteCount);
	}

	public static void* AllocateZeroed(int byteCount)
	{
		if (byteCount < 0)
			throw new System.ArgumentOutOfRangeException();

		return NativeMemoryRuntime.luma_interop_memory_allocate_zeroed(byteCount);
	}

	public static void Free(void* memory)
	{
		NativeMemoryRuntime.luma_interop_memory_free(memory);
	}

	public static void* Copy(void* destination, void* source, int byteCount)
	{
		if (byteCount < 0)
			throw new System.ArgumentOutOfRangeException();

		return NativeMemoryRuntime.luma_interop_memory_copy(destination, source, byteCount);
	}

	public static void* Move(void* destination, void* source, int byteCount)
	{
		if (byteCount < 0)
			throw new System.ArgumentOutOfRangeException();

		return NativeMemoryRuntime.luma_interop_memory_move(destination, source, byteCount);
	}

	public static void* Zero(void* memory, int byteCount)
	{
		if (byteCount < 0)
			throw new System.ArgumentOutOfRangeException();

		return NativeMemoryRuntime.luma_interop_memory_zero(memory, byteCount);
	}
}

internal static unsafe class NativeMemoryRuntime
{
	public static extern void* luma_interop_memory_allocate(int byteCount);
	public static extern void* luma_interop_memory_allocate_zeroed(int byteCount);
	public static extern void luma_interop_memory_free(void* memory);
	public static extern void* luma_interop_memory_copy(void* destination, void* source, int byteCount);
	public static extern void* luma_interop_memory_move(void* destination, void* source, int byteCount);
	public static extern void* luma_interop_memory_zero(void* memory, int byteCount);
}
