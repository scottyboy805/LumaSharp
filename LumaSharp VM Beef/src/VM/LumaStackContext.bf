using System;
using System.Interop;

namespace LumaSharpVMBeef;

public struct LumaStackContext
{
	// Public
	public readonly LumaStackContext* parent;	// The parent method frame used to build the call stack
	public readonly LumaMethodHandle* method;	// The method being called
	public readonly uint8* spa;					// Stack pointer arguments
	public readonly uint8* spl;					// Stack pointer locals
	public readonly uint8* spmin;				// Stack pointer min value - less than would be an invalid pop
	public readonly uint8* spmax;				// Stack pointer max value - greater or equal should cause stack overflow
	public readonly uint8* pcmax;				// Program counter max value - greater or equal should cause execution error

	public uint8* sp;							// Stack pointer
	public uint8* spalloc;						// Stack pointer where allocations for the current frame can be made
	public uint8* pc;							// Program counter

	public readonly LumaStackType type;

	// Constructor
	public this(LumaMethodHandle* method, uint8* sp, uint8* spMin, uint8* spMax)
	{
		uint8* baseSp = sp + method.signature.parameterSize + method.localSize;

		this.parent = null;
		this.method = method;
		this.sp = (uint8*)baseSp;
		this.spa = sp;
		this.spl = sp + method.signature.parameterSize;
		this.spalloc = baseSp + method.maxStack;
		this.spmin = spMin;
		this.spmax = spMax;
		this.pc = method.rva;
		this.pcmax = method.rva + method.size;
		this.type = .CallFrame;
	}

	// Methods
	public void RequireStackFixedValue(uint32 count)
	{
		// Get offset after 
		uint8* spResult = sp - (sizeof(LumaStackValue) * count);

		// Check underflow
		Runtime.Assert(spResult >= spmin, "Stack underflow");
	}

	[Inline]
	public LumaStackValue* Peek()
	{
		// Try to peek meta
		return ((LumaStackValue*)sp) - 1;
	}

	[Inline]
	public LumaStackType PeekType()
	{
		// Try to peek meta
		return (((LumaStackValue*)sp) - 1).type;
	}

	[Inline]
	public void PushI32(int32 val) mut
	{
		// Store fixed value on stack
		*(LumaStackValue*)sp = .(val);

		// Increment ptr
		sp += sizeof(LumaStackValue);

		// Check overflow
		Runtime.Assert(sp < spmax, "Stack overflow");
	}

	[Inline]
	public void PushU32(uint32 val) mut
	{
		// Store fixed value on stack
		*(LumaStackValue*)sp = .(val);

		// Increment ptr
		sp += sizeof(LumaStackValue);

		// Check overflow
		Runtime.Assert(sp < spmax, "Stack overflow");
	}

	[Inline]
	public void PushI64(int64 val) mut
	{
		// Store fixed value on stack
		*(LumaStackValue*)sp = .(val);

		// Increment ptr
		sp += sizeof(LumaStackValue);

		// Check overflow
		Runtime.Assert(sp < spmax, "Stack overflow");
	}

	[Inline]
	public void PushU64(uint64 val) mut
	{
		// Store fixed value on stack
		*(LumaStackValue*)sp = .(val);

		// Increment ptr
		sp += sizeof(LumaStackValue);

		// Check overflow
		Runtime.Assert(sp < spmax, "Stack overflow");
	}

	[Inline]
	public void PushF32(float val) mut
	{
		// Store fixed value on stack
		*(LumaStackValue*)sp = .(val);

		// Increment ptr
		sp += sizeof(LumaStackValue);

		// Check overflow
		Runtime.Assert(sp < spmax, "Stack overflow");
	}

	[Inline]
	public void PushF64(double val) mut
	{
		// Store fixed value on stack
		*(LumaStackValue*)sp = .(val);

		// Increment ptr
		sp += sizeof(LumaStackValue);

		// Check overflow
		Runtime.Assert(sp < spmax, "Stack overflow");
	}

	[Inline]
	public void PushPtr(void* val) mut
	{
		// Store fixed value on stack
		*(LumaStackValue*)sp = .(val);

		// Increment ptr
		sp += sizeof(LumaStackValue);

		// Check overflow
		Runtime.Assert(sp < spmax, "Stack overflow");
	}

	[Inline]
	public void PushCopy(void* srcAddress, LumaTypeHandle* type) mut
	{
		// Check value type
		if((type.flags & .ValueType) != 0)
		{
			PushValueTypeCopy(srcAddress, type.size);
		}
		else
		{
			switch(type.code)
			{
			case .Bool: PushI32(*(bool*)srcAddress ? 1 : 0);
			case .Char: PushI32((int32)*(char8*)srcAddress);
			case .I8: PushI32(*(int8*)srcAddress);
			case .U8: PushI32(*(uint8*)srcAddress);
			case .I16: PushI32(*(int16*)srcAddress);
			case .U16: PushI32(*(uint16*)srcAddress);
			case .I32: PushI32(*(int32*)srcAddress);
			case .U32: PushU32(*(uint32*)srcAddress);
			case .I64: PushI64(*(int64*)srcAddress);
			case .U64: PushU64(*(uint64*)srcAddress);
			case .F32: PushF32(*(float*)srcAddress);
			case .F64: PushF64(*(double*)srcAddress);
			case .Any: PushPtr(*(void**)srcAddress);

			default: Runtime.FatalError(scope $"Attempting to push invalid type: {type.code}");
			}
		}
	}

	[Inline]
	public void PushValueTypeCopy(void* srcValueTypePtr, uint32 valueTypeSize) mut
	{
		// Copy memory to stack
		LumaMemory.Copy(srcValueTypePtr, sp, valueTypeSize);

		// Increment ptr the size of the value type
		sp += valueTypeSize;

		// Store int on stack
		*(LumaStackValue*)sp = .(srcValueTypePtr, valueTypeSize);

		// Increment ptr
		sp += sizeof(LumaStackValue);

		// Check overflow
		Runtime.Assert(sp < spmax, "Stack overflow");
	}


	[Inline]
	public int32 PopI32() mut
	{
		// Pop
		sp -= sizeof(LumaStackValue);

		// Check underflow
		Runtime.Assert(sp >= spmin, "Stack underflow");

		// Get value from stack
		LumaStackValue* val = (LumaStackValue*)sp;

		// Check type
		Runtime.Assert(val.type == .I32, "Stack value must be I32");

		// Get value
		return *val.I32;
	}

	[Inline]
	public uint32 PopU32() mut
	{
		// Pop
		sp -= sizeof(LumaStackValue);

		// Check underflow
		Runtime.Assert(sp >= spmin, "Stack underflow");

		// Get value from stack
		LumaStackValue* val = (LumaStackValue*)sp;

		// Check type
		Runtime.Assert(val.type == .U32, "Stack value must be U32");

		// Get value
		return *val.U32;
	}

	[Inline]
	public int64 PopI64() mut
	{
		// Pop
		sp -= sizeof(LumaStackValue);

		// Check underflow
		Runtime.Assert(sp >= spmin, "Stack underflow");

		// Get value from stack
		LumaStackValue* val = (LumaStackValue*)sp;

		// Check type
		Runtime.Assert(val.type == .I64, "Stack value must be I64");

		// Get value int
		return *val.I64;
	}

	[Inline]
	public uint64 PopU64() mut
	{
		// Pop
		sp -= sizeof(LumaStackValue);

		// Check underflow
		Runtime.Assert(sp >= spmin, "Stack underflow");

		// Get value from stack
		LumaStackValue* val = (LumaStackValue*)sp;

		// Check type
		Runtime.Assert(val.type == .U64, "Stack value must be U64");

		// Get value int
		return *val.U64;
	}

	[Inline]
	public float PopF32() mut
	{
		// Pop
		sp -= sizeof(LumaStackValue);

		// Check underflow
		Runtime.Assert(sp >= spmin, "Stack underflow");

		// Get value from stack
		LumaStackValue* val = (LumaStackValue*)sp;

		// Check type
		Runtime.Assert(val.type == .F32, "Stack value must be F32");

		// Get value int
		return *val.F32;
	}

	[Inline]
	public double PopF64() mut
	{
		// Pop
		sp -= sizeof(LumaStackValue);

		// Check underflow
		Runtime.Assert(sp >= spmin, "Stack underflow");

		// Get value from stack
		LumaStackValue* val = (LumaStackValue*)sp;

		// Check type
		Runtime.Assert(val.type == .F64, "Stack value must be F64");

		// Get value int
		return *val.F64;
	}

	[Inline]
	public void* PopPtr() mut
	{
		// Pop
		sp -= sizeof(LumaStackValue);

		// Check underflow
		Runtime.Assert(sp >= spmin, "Stack underflow");

		// Get value from stack
		LumaStackValue* val = (LumaStackValue*)sp;

		// Check type
		Runtime.Assert(val.type == .Address, "Stack value must be an address");

		// Get value int
		return val.Ptr;
	}

	[Inline]
	public void PopCopy(void* dstAddress, LumaTypeHandle* type) mut
	{
		// Check value type
		if((type.flags & .ValueType) != 0)
		{
			PopValueTypeCopy(dstAddress, type.size);
		}
		else
		{
			switch(type.code)
			{
			case .Bool: *(bool*)dstAddress = PopI32() == 1;
			case .Char: *(char8*)dstAddress = (char8)PopI32();
			case .I8: *(int8*)dstAddress = (int8)PopI32();
			case .U8: *(uint8*)dstAddress = (uint8)PopI32();
			case .I16: *(int16*)dstAddress = (int16)PopI32();
			case .U16: *(uint16*)dstAddress = (uint16)PopI32();
			case .I32: *(int32*)dstAddress = PopI32();
			case .U32: *(uint32*)dstAddress = PopU32();
			case .I64: *(int64*)dstAddress = PopI64();
			case .U64: *(uint64*)dstAddress = PopU64();
			case .F32: *(float*)dstAddress = PopF32();
			case .F64: *(double*)dstAddress = PopF64();
			case .Any: *(void**)dstAddress = PopPtr();

			default: Runtime.FatalError(scope $"Attempting to pop invalid type: {type.code}");
			}
		}
	}

	[Inline]
	public void PopValueTypeCopy(void* dstValueTypePtr, uint32 expectedValueTypeSize) mut
	{
		// Pop
		sp -= sizeof(LumaStackValue);

		// Check underflow
		Runtime.Assert(sp - expectedValueTypeSize >= spmin, "Stack underflow");

		// Get value from stack
		LumaStackValue* val = (LumaStackValue*)sp;

		// Check type and size
		Runtime.Assert(val.type == .ValueType, "Stack value must be a value type");
		Runtime.Assert(val.size == expectedValueTypeSize, scope $"Stack value type size expected: {expectedValueTypeSize}, but got: {val.size}");

		// Perform copy
		LumaMemory.Copy(val.Ptr, dstValueTypePtr, expectedValueTypeSize);

		// Pop value type
		sp -= expectedValueTypeSize;
	}

	[Inline]
	public LumaStackValue* Pop() mut
	{
		// Peek top value
		LumaStackValue* value = Peek();

		// Pop meta
		sp -= sizeof(LumaStackValue);

		// Check for larger sized items
		if(value.size > 0)
		{
			// Account for inline size on the stack
			sp -= value.size;
		}

		return value;
	}

	[Inline]
	public void Dup() mut
	{
		// Peek top value
		LumaStackValue* value = Peek();

		// Check for inline sized items
		if(value.size > 0)
		{
			// Copy item to sp
			LumaMemory.Copy(value.Ptr, sp, value.size);

			// Increment sp
			sp += value.size;
		}

		// Push meta
		*(LumaStackValue*)sp = *value;
		sp += sizeof(LumaStackValue);
	}


//	[Inline]
//	public void StackPush<T>(T val) mut where T : struct
//	{
//		*((T*)sp) = val;
//		sp += sizeof(T);
//	}
//
//	[Inline]
//	public void StackPushPointer(void* val) mut
//	{
//		*(void**)sp = val;
//		sp += sizeof(void*);
//	}
//
//	[Inline]
//	public void StackPushCopy(void* src, uint32 size) mut
//	{
//		LumaMemory.Copy(src, sp, size);
//		sp += size;
//	}
//
//	[Inline]
//	public void* StackPush(uint32 size) mut
//	{
//		void* ptr = sp;
//		sp += size;
//		return ptr;
//	}
//
//	[Inline]
//	public T StackPop<T>() mut where T : struct
//	{
//		sp -= sizeof(T);
//		return *((T*)sp);
//	}
//
//	[Inline]
//	public void* StackPop(uint32 size) mut
//	{
//		sp -= size;
//		return sp;
//	}
//
//	[Inline]
//	public void* StackPopPointer() mut
//	{
//		sp -= sizeof(void*);
//		return *(void**)sp;
//	}
//
//	[Inline]
//	public void StackPopCopy(void* dst, uint32 size) mut
//	{
//		sp -= size;
//		LumaMemory.Copy(sp, dst, size);
//	}
//
//	[Inline]
//	public void* StackPeek(uint32 offset)
//	{
//		return sp - offset;
//	}
//
//	public T StackPeek<T>() where T : struct
//	{
//		uint8* spPeek = sp - sizeof(T);
//		return *(T*)spPeek;
//	}

	[Inline]
	public T InstructionRead<T>() mut where T : struct
	{
		T op = *((T*)pc);
		pc += sizeof(T);
		return op;
	}

	[Inline]
	public void* StackGetArgAddress(uint32 offsetInBytes)
	{
		return spa + offsetInBytes;
	}

	[Inline]
	public void* StackGetLocalAddress(uint32 offsetInBytes)
	{
		return spl + offsetInBytes;
	}
}