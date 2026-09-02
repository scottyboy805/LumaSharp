using System;

namespace LumaSharpVMBeef;

public static class LumaArithmetic
{
	// Methods
	[Inline]
	public static void Add(LumaStackContext* stack)
	{
		// Make sure we have 2 values on the stack
		stack.RequireStackFixedValue(2);

		// Pop 1 value
		stack.sp -= sizeof(LumaStackValue);

		// Pop stack manually for best performance
		LumaStackValue* b = (LumaStackValue*)stack.sp;
		LumaStackValue* a = b - 1;

		switch(a.type)
		{
		case .I32: *a.I32 += *b.I32;
		case .U32: *a.U32 += *b.U32;
		case .I64: *a.I64 += *b.I64;
		case .U64: *a.U64 += *b.U64;
		case .F32: *a.F32 += *b.F32;
		case .F64: *a.F64 += *b.F64;
		case .Address: *a.Address += *b.Address;
		default: Runtime.FatalError(scope $"Unsupported arithmetic type: {a.type}");
		}
	}

	[Inline]
	public static void Sub(LumaStackContext* stack)
	{
		// Make sure we have 2 values on the stack
		stack.RequireStackFixedValue(2);

		// Pop 1 value
		stack.sp -= sizeof(LumaStackValue);

		// Pop stack manually for best performance
		LumaStackValue* b = (LumaStackValue*)stack.sp;
		LumaStackValue* a = b - 1;

		switch(a.type)
		{
		case .I32: *a.I32 -= *b.I32;
		case .U32: *a.U32 -= *b.U32;
		case .I64: *a.I64 -= *b.I64;
		case .U64: *a.U64 -= *b.U64;
		case .F32: *a.F32 -= *b.F32;
		case .F64: *a.F64 -= *b.F64;
		case .Address: *a.Address -= *b.Address;
		default: Runtime.FatalError(scope $"Unsupported arithmetic type: {a.type}");
		}
	}

	[Inline]
	public static void Mul(LumaStackContext* stack)
	{
		// Make sure we have 2 values on the stack
		stack.RequireStackFixedValue(2);

		// Pop 1 value
		stack.sp -= sizeof(LumaStackValue);

		// Pop stack manually for best performance
		LumaStackValue* b = (LumaStackValue*)stack.sp;
		LumaStackValue* a = b - 1;

		switch(a.type)
		{
		case .I32: *a.I32 *= *b.I32;
		case .U32: *a.U32 *= *b.U32;
		case .I64: *a.I64 *= *b.I64;
		case .U64: *a.U64 *= *b.U64;
		case .F32: *a.F32 *= *b.F32;
		case .F64: *a.F64 *= *b.F64;
		case .Address: *a.Address *= *b.Address;
		default: Runtime.FatalError(scope $"Unsupported arithmetic type: {a.type}");
		}
	}

	[Inline]
	public static void Div(LumaStackContext* stack)
	{
		// Make sure we have 2 values on the stack
		stack.RequireStackFixedValue(2);

		// Pop 1 value
		stack.sp -= sizeof(LumaStackValue);

		// Pop stack manually for best performance
		LumaStackValue* b = (LumaStackValue*)stack.sp;
		LumaStackValue* a = b - 1;

		switch(a.type)
		{
		case .I32: *a.I32 /= *b.I32;
		case .U32: *a.U32 /= *b.U32;
		case .I64: *a.I64 /= *b.I64;
		case .U64: *a.U64 /= *b.U64;
		case .F32: *a.F32 /= *b.F32;
		case .F64: *a.F64 /= *b.F64;
		case .Address: *a.Address /= *b.Address;
		default: Runtime.FatalError(scope $"Unsupported arithmetic type: {a.type}");
		}
	}

	[Inline]
	public static void Mod(LumaStackContext* stack)
	{
		// Make sure we have 2 values on the stack
		stack.RequireStackFixedValue(2);

		// Pop 1 value
		stack.sp -= sizeof(LumaStackValue);

		// Pop stack manually for best performance
		LumaStackValue* b = (LumaStackValue*)stack.sp;
		LumaStackValue* a = b - 1;

		switch(a.type)
		{
		case .I32: *a.I32 %= *b.I32;
		case .U32: *a.U32 %= *b.U32;
		case .I64: *a.I64 %= *b.I64;
		case .U64: *a.U64 %= *b.U64;
		case .F32: *a.F32 %= *b.F32;
		case .F64: *a.F64 %= *b.F64;
		case .Address: *a.Address %= *b.Address;
		default: Runtime.FatalError(scope $"Unsupported arithmetic type: {a.type}");
		}
	}

	[Inline]
	public static void Neg(LumaStackContext* stack)
	{
		// Make sure we have 1 value on the stack
		stack.RequireStackFixedValue(1);

		// Pop stack manually for best performance
		LumaStackValue* val = ((LumaStackValue*)stack.sp) - 1;

		switch(val.type)
		{
		case .I32: *val.I32 = -*val.I32;
		case .I64: *val.I64 = -*val.I64;
		case .F32: *val.F32 = -*val.F32;
		case .F64: *val.F64 = -*val.F64;
		case .Address: *val.Address += *val.Address;
		default: Runtime.FatalError(scope $"Unsupported arithmetic type: {val.type}");
		}
	}

	[Inline]
	public static void Bitwise_And(LumaStackContext* stack)
	{
		// Make sure we have 2 values on the stack
		stack.RequireStackFixedValue(2);

		// Pop 1 value
		stack.sp -= sizeof(LumaStackValue);

		// Pop stack manually for best performance
		LumaStackValue* b = (LumaStackValue*)stack.sp;
		LumaStackValue* a = b - 1;

		switch(a.type)
		{
		case .I32: *a.I32 &= *b.I32;
		case .U32: *a.U32 &= *b.U32;
		case .I64: *a.I64 &= *b.I64;
		case .U64: *a.U64 &= *b.U64;
		case .Address: *a.Address &= *b.Address;
		default: Runtime.FatalError(scope $"Unsupported arithmetic type: {a.type}");
		}
	}

	[Inline]
	public static void Bitwise_Or(LumaStackContext* stack)
	{
		// Make sure we have 2 values on the stack
		stack.RequireStackFixedValue(2);

		// Pop 1 value
		stack.sp -= sizeof(LumaStackValue);

		// Pop stack manually for best performance
		LumaStackValue* b = (LumaStackValue*)stack.sp;
		LumaStackValue* a = b - 1;

		switch(a.type)
		{
		case .I32: *a.I32 |= *b.I32;
		case .U32: *a.U32 |= *b.U32;
		case .I64: *a.I64 |= *b.I64;
		case .U64: *a.U64 |= *b.U64;
		case .Address: *a.Address |= *b.Address;
		default: Runtime.FatalError(scope $"Unsupported arithmetic type: {a.type}");
		}
	}

	[Inline]
	public static void Bitwise_Xor(LumaStackContext* stack)
	{
		// Make sure we have 2 values on the stack
		stack.RequireStackFixedValue(2);

		// Pop 1 value
		stack.sp -= sizeof(LumaStackValue);

		// Pop stack manually for best performance
		LumaStackValue* b = (LumaStackValue*)stack.sp;
		LumaStackValue* a = b - 1;

		switch(a.type)
		{
		case .I32: *a.I32 ^= *b.I32;
		case .U32: *a.U32 ^= *b.U32;
		case .I64: *a.I64 ^= *b.I64;
		case .U64: *a.U64 ^= *b.U64;
		case .Address: *a.Address ^= *b.Address;
		default: Runtime.FatalError(scope $"Unsupported arithmetic type: {a.type}");
		}
	}

	[Inline]
	public static void Bitwise_Not(LumaStackContext* stack)
	{
		// Make sure we have 1 value on the stack
		stack.RequireStackFixedValue(1);

		// Pop stack manually for best performance
		LumaStackValue* val = ((LumaStackValue*)stack.sp) - 1;

		switch(val.type)
		{
		case .I32: *val.I32 = *val.I32 == 1 ? 0 : 1;
		default: Runtime.FatalError(scope $"Unsupported arithmetic type: {val.type}");
		}
	}

	[Inline]
	public static void Bitwise_Shl(LumaStackContext* stack)
	{
		// Make sure we have 2 values on the stack
		stack.RequireStackFixedValue(2);

		// Pop 1 value
		stack.sp -= sizeof(LumaStackValue);

		// Pop stack manually for best performance
		LumaStackValue* b = (LumaStackValue*)stack.sp;
		LumaStackValue* a = b - 1;

		switch(a.type)
		{
		case .I32: *a.I32 <<= *b.I32;
		case .U32: *a.U32 <<= *b.U32;
		case .I64: *a.I64 <<= *b.I64;
		case .U64: *a.U64 <<= *b.U64;
		case .Address: *a.Address <<= *b.Address;
		default: Runtime.FatalError(scope $"Unsupported arithmetic type: {a.type}");
		}
	}

	[Inline]
	public static void Bitwise_Shr(LumaStackContext* stack)
	{
		// Make sure we have 2 values on the stack
		stack.RequireStackFixedValue(2);

		// Pop 1 value
		stack.sp -= sizeof(LumaStackValue);

		// Pop stack manually for best performance
		LumaStackValue* b = (LumaStackValue*)stack.sp;
		LumaStackValue* a = b - 1;

		switch(a.type)
		{
		case .I32: *a.I32 >>= *b.I32;
		case .U32: *a.U32 >>= *b.U32;
		case .I64: *a.I64 >>= *b.I64;
		case .U64: *a.U64 >>= *b.U64;
		case .Address: *a.Address ^= *b.Address;
		default: Runtime.FatalError(scope $"Unsupported arithmetic type: {a.type}");
		}
	}



	[Inline]
	public static void Cmp_Less(ref uint8* sp, LumaPrimitiveTypeCode type)
	{
		switch(type)
		{
		case .I32:
			{
				int32* b = ((int32*)sp) - 1;
				int32* a = b - 1;

				*a = *a < *b ? 1 : 0;
				sp = (uint8*)(a + 1);
			}
		case .U32:
			{
				uint32* b = ((uint32*)sp) - 1;
				uint32* a = b - 1;

				*a = *a < *b ? 1 : 0;
				sp = (uint8*)(a + 1);
			}
		case .I64:
			{
				int64* b = ((int64*)sp) - 1;
				int64* a = b - 1;

				*a = *a < *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .U64:
			{
				uint64* b = ((uint64*)sp) - 1;
				uint64* a = b - 1;

				*a = *a < *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .F32:
			{
				float* b = ((float*)sp) - 1;
				float* a = b - 1;

				*a = *a < *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .F64:
			{
				double* b = ((double*)sp) - 1;
				double* a = b - 1;

				*a = *a < *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .Ptr:
			{
				int* b = ((int*)sp) - 1;
				int* a = b - 1;

				*a = *a < *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		default: Runtime.FatalError(scope $"Unsupported arithmetic type: {type}");
		}
	}

	[Inline]
	public static void Cmp_LessEqual(ref uint8* sp, LumaPrimitiveTypeCode type)
	{
		switch(type)
		{
		case .I32:
			{
				int32* b = ((int32*)sp) - 1;
				int32* a = b - 1;

				*a = *a <= *b ? 1 : 0;
				sp = (uint8*)(a + 1);
			}
		case .U32:
			{
				uint32* b = ((uint32*)sp) - 1;
				uint32* a = b - 1;

				*a = *a <= *b ? 1 : 0;
				sp = (uint8*)(a + 1);
			}
		case .I64:
			{
				int64* b = ((int64*)sp) - 1;
				int64* a = b - 1;

				*a = *a <= *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .U64:
			{
				uint64* b = ((uint64*)sp) - 1;
				uint64* a = b - 1;

				*a = *a <= *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .F32:
			{
				float* b = ((float*)sp) - 1;
				float* a = b - 1;

				*a = *a <= *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .F64:
			{
				double* b = ((double*)sp) - 1;
				double* a = b - 1;

				*a = *a <= *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .Ptr:
			{
				int* b = ((int*)sp) - 1;
				int* a = b - 1;

				*a = *a <= *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		default: Runtime.FatalError(scope $"Unsupported arithmetic type: {type}");
		}
	}

	[Inline]
	public static void Cmp_Greater(ref uint8* sp, LumaPrimitiveTypeCode type)
	{
		switch(type)
		{
		case .I32:
			{
				int32* b = ((int32*)sp) - 1;
				int32* a = b - 1;

				*a = *a > *b ? 1 : 0;
				sp = (uint8*)(a + 1);
			}
		case .U32:
			{
				uint32* b = ((uint32*)sp) - 1;
				uint32* a = b - 1;

				*a = *a > *b ? 1 : 0;
				sp = (uint8*)(a + 1);
			}
		case .I64:
			{
				int64* b = ((int64*)sp) - 1;
				int64* a = b - 1;

				*a = *a > *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .U64:
			{
				uint64* b = ((uint64*)sp) - 1;
				uint64* a = b - 1;

				*a = *a > *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .F32:
			{
				float* b = ((float*)sp) - 1;
				float* a = b - 1;

				*a = *a > *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .F64:
			{
				double* b = ((double*)sp) - 1;
				double* a = b - 1;

				*a = *a > *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .Ptr:
			{
				int* b = ((int*)sp) - 1;
				int* a = b - 1;

				*a = *a > *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		default: Runtime.FatalError(scope $"Unsupported arithmetic type: {type}");
		}
	}

	[Inline]
	public static void Cmp_GreaterEqual(ref uint8* sp, LumaPrimitiveTypeCode type)
	{
		switch(type)
		{
		case .I32:
			{
				int32* b = ((int32*)sp) - 1;
				int32* a = b - 1;

				*a = *a >= *b ? 1 : 0;
				sp = (uint8*)(a + 1);
			}
		case .U32:
			{
				uint32* b = ((uint32*)sp) - 1;
				uint32* a = b - 1;

				*a = *a >= *b ? 1 : 0;
				sp = (uint8*)(a + 1);
			}
		case .I64:
			{
				int64* b = ((int64*)sp) - 1;
				int64* a = b - 1;

				*a = *a >= *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .U64:
			{
				uint64* b = ((uint64*)sp) - 1;
				uint64* a = b - 1;

				*a = *a >= *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .F32:
			{
				float* b = ((float*)sp) - 1;
				float* a = b - 1;

				*a = *a >= *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .F64:
			{
				double* b = ((double*)sp) - 1;
				double* a = b - 1;

				*a = *a >= *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .Ptr:
			{
				int* b = ((int*)sp) - 1;
				int* a = b - 1;

				*a = *a >= *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		default: Runtime.FatalError(scope $"Unsupported arithmetic type: {type}");
		}
	}

	[Inline]
	public static void Cmp_Equal(ref uint8* sp, LumaPrimitiveTypeCode type)
	{
		switch(type)
		{
		case .I32:
			{
				int32* b = ((int32*)sp) - 1;
				int32* a = b - 1;

				*a = *a == *b ? 1 : 0;
				sp = (uint8*)(a + 1);
			}
		case .U32:
			{
				uint32* b = ((uint32*)sp) - 1;
				uint32* a = b - 1;

				*a = *a == *b ? 1 : 0;
				sp = (uint8*)(a + 1);
			}
		case .I64:
			{
				int64* b = ((int64*)sp) - 1;
				int64* a = b - 1;

				*a = *a == *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .U64:
			{
				uint64* b = ((uint64*)sp) - 1;
				uint64* a = b - 1;

				*a = *a == *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .F32:
			{
				float* b = ((float*)sp) - 1;
				float* a = b - 1;

				*a = *a == *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .F64:
			{
				double* b = ((double*)sp) - 1;
				double* a = b - 1;

				*a = *a == *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .Ptr:
			{
				int* b = ((int*)sp) - 1;
				int* a = b - 1;

				*a = *a == *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		default: Runtime.FatalError(scope $"Unsupported arithmetic type: {type}");
		}
	}

	[Inline]
	public static void Cmp_NotEqual(ref uint8* sp, LumaPrimitiveTypeCode type)
	{
		switch(type)
		{
		case .I32:
			{
				int32* b = ((int32*)sp) - 1;
				int32* a = b - 1;

				*a = *a != *b ? 1 : 0;
				sp = (uint8*)(a + 1);
			}
		case .U32:
			{
				uint32* b = ((uint32*)sp) - 1;
				uint32* a = b - 1;

				*a = *a != *b ? 1 : 0;
				sp = (uint8*)(a + 1);
			}
		case .I64:
			{
				int64* b = ((int64*)sp) - 1;
				int64* a = b - 1;

				*a = *a != *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .U64:
			{
				uint64* b = ((uint64*)sp) - 1;
				uint64* a = b - 1;

				*a = *a != *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .F32:
			{
				float* b = ((float*)sp) - 1;
				float* a = b - 1;

				*a = *a != *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .F64:
			{
				double* b = ((double*)sp) - 1;
				double* a = b - 1;

				*a = *a != *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		case .Ptr:
			{
				int* b = ((int*)sp) - 1;
				int* a = b - 1;

				*a = *a != *b ? 1 : 0;
				sp = (uint8*)(((int32*)a) + 1);
			}
		default: Runtime.FatalError(scope $"Unsupported arithmetic type: {type}");
		}
	}
}