using System;

namespace LumaSharpVMBeef;

public enum LumaStackType : uint32
{
	Invalid = 0,
	I32,
	U32,
	I64,
	U64,
	F32,
	F64,
	Address,
	ValueType,
	CallFrame,
}

// 16 byte fixed size
// Value = 8, type = 4, size = 4
public struct LumaStackValue
{
	// Private
	private uint64 val;

	// Public
	public readonly LumaStackType type;
	public readonly uint32 size;

	public const LumaStackValue Invalid = default;

	// Properties
	public int32* I32
	{
		[Inline]
		get mut => (int32*)&val;
	}

	public uint32* U32
	{
		[Inline]
		get mut => (uint32*)&val;
	}

	public int64* I64
	{
		[Inline]
		get mut => (int64*)&val;
	}

	public uint64* U64
	{
		[Inline]
		get mut => (uint64*)&val;
	}

	public float* F32
	{
		[Inline]
		get mut => (float*)&val;
	}

	public double* F64
	{
		[Inline]
		get mut => (double*)&val;
	}

	public int* Address
	{
		[Inline]
		get mut => (int*)&val;
	}

	public void* Ptr
	{
		[Inline]
		get mut => (void*)(int)val;
	}

	// Constructor
	[Inline]
	public this(int32 val)
	{
		this.val = (uint64)val;
		this.type = .I32;
		this.size = 0;
	}

	[Inline]
	public this(uint32 val)
	{
		this.val = (uint32)val;
		this.type = .U32;
		this.size = 0;
	}

	[Inline]
	public this(int64 val)
	{
		this.val = (uint64)val;
		this.type = .I64;
		this.size = 0;
	}

	[Inline]
	public this(uint64 val)
	{
		this.val = val;
		this.type = .U64;
		this.size = 0;
	}

	[Inline]
	public this(float val)
	{
		var val;
		this.val = *(uint32*)&val;
		this.type = .F32;
		this.size = 0;
	}

	[Inline]
	public this(double val)
	{
		var val;
		this.val = *(uint64*)&val;
		this.type = .F64;
		this.size = 0;
	}

	[Inline]
	public this(void* ptr)
	{
		this.val = (uint64)(int)ptr;
		this.type = .Address;
		this.size = 0;
	}

	[Inline]
	public this(void* valueTypePtr, uint32 valueTypeSize)
	{
		this.val = (uint64)(int)valueTypePtr;
		this.type = .ValueType;
		this.size = valueTypeSize;
	}

	// Methods
	public override void ToString(String strBuffer) mut
	{
		switch(type)
		{
		case .I32: strBuffer.Set(scope $"I32 ({*I32})");
		case .U32: strBuffer.Set(scope $"U32 ({*U32})");
		case .I64: strBuffer.Set(scope $"I64 ({*I64})");
		case .U64: strBuffer.Set(scope $"U64 ({*U64})");
		case .F32: strBuffer.Set(scope $"F32 ({*F32})");
		case .F64: strBuffer.Set(scope $"F64 ({*F64})");
		case .Address: strBuffer.Set(scope $"Addr ({*Ptr})");

		case .ValueType:
			{
				strBuffer.Set(scope $"ValueType (Size = {size})");
			}

		case .CallFrame:
			{

				LumaStackContext context = *(LumaStackContext*)Ptr;
				strBuffer.Set(scope $"CallFrame (Method = {context.method})");
			}

		default: strBuffer.Set("Invalid");
		}
	}
}