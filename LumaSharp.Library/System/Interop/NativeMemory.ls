namespace System.Interop;

public static unsafe class NativeMemory
{
	public static void* Allocate(int byteCount)
	{
		if (byteCount < 0)
			throw new System.ArgumentOutOfRangeException();

		return __memalloc(byteCount);
	}

	public static void* AllocateZeroed(int byteCount)
	{
		if (byteCount < 0)
			throw new System.ArgumentOutOfRangeException();

		return __memalloczero(byteCount);
	}

	public static void Free(void* memory)
	{
		__memfree(memory);
	}

	public static void* Copy(void* destination, void* source, int byteCount)
	{
		if (byteCount < 0)
			throw new System.ArgumentOutOfRangeException();

		return __memcpy(destination, source, byteCount);
	}

	public static void* Move(void* destination, void* source, int byteCount)
	{
		if (byteCount < 0)
			throw new System.ArgumentOutOfRangeException();

		return __memmove(destination, source, byteCount);
	}

	public static void* Zero(void* memory, int byteCount)
	{
		if (byteCount < 0)
			throw new System.ArgumentOutOfRangeException();

		return __memzero(memory, byteCount);
	}
}
