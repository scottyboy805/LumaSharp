namespace System.Interop;

public static unsafe class NativeMemory
{
	public static void* Allocate(int byteCount)
	{
		if (byteCount < 0)
			throw new System.ArgumentOutOfRangeException();

		return NativeMemoryRuntime.luma_interop_memory_allocate(byteCount);
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
}

internal static unsafe class NativeMemoryRuntime
{
	public static extern void* luma_interop_memory_allocate(int byteCount);
	public static extern void luma_interop_memory_free(void* memory);
	public static extern void* luma_interop_memory_copy(void* destination, void* source, int byteCount);
	public static extern void* luma_interop_memory_move(void* destination, void* source, int byteCount);
}
