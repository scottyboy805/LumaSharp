using System;

namespace LumaSharpVMBeef;

public static class LumaMemory
{
	// Methods
	public static LumaArray* AllocArray(LumaTypeHandle* elementType, uint64 length)
	{
		// Get total size
		uint64 sizeInbytes = (elementType.size * length) + sizeof(LumaArray);

		// Allocate some memory
		uint8[] arr = new uint8[sizeInbytes];

		// Get ptr
		uint8* basePtr = &arr[0];
		uint8* dataPtr = basePtr + sizeof(LumaArray);

		// Create handle
		*((LumaArray*)basePtr) = LumaArray
		{
			elementType = elementType,
		  	length = length,
			arrayData = dataPtr,
		};

		// Get the handle
		return (LumaArray*)basePtr;
	}

	public static LumaArray* StackAllocArray(LumaStackContext* stack, LumaTypeHandle* elementType, uint64 length)
	{
		// Get total size
		uint64 sizeInbytes = (elementType.size * length) + sizeof(LumaArray);

		// Allocate memory on the stack
		uint8* basePtr = (uint8*)stack.spalloc;
		stack.spalloc += sizeInbytes;

		// Get ptr
		uint8* dataPtr = basePtr + sizeof(LumaArray);

		// Create handle
		*((LumaArray*)basePtr) = LumaArray
		{
			elementType = elementType,
			length = length,
			arrayData = dataPtr,
		};

		// Get the handle
		return (LumaArray*)basePtr;
	}

	[Inline]
	public static void Clear(void* mem, uint32 size)
	{
		Internal.MemSet(mem, 0, size);
	}

	[Inline]
	public static bool Compare(void* a, void* b, uint32 size)
	{
		return Internal.MemCmp(a, b, size) == 0;
	}

	[Inline]
	public static void Copy(void* src, void* dst, uint32 size)
	{
		Internal.MemCpy(src, dst, size);
	}
}