using System;

namespace LumaSharpVMBeef;

internal static class LumaConvert
{
	// Methods
	[Inline]
	public static void ConvertI8(uint8* sp, LumaPrimitiveTypeCode type)
	{
		// Get i8 val
		int8 val = *(int8*)sp;

		switch(type)
		{
		case .I8: *(int32*)sp = (int8)val;
		case .U8: *(int32*)sp = (uint8)val;
		case .I16: *(int32*)sp = (int16)val;
		case .U16: *(int32*)sp = (uint16)val;
		case .I32: *(int32*)sp = (int32)val;
		case .U32: *(uint32*)sp = (uint32)val;
		case .I64: *(int64*)sp = (int64)val;
		case .U64: *(uint16*)sp = (uint16)val;
		case .F32: *(float*)sp = (float)val;
		case .F64: *(double*)sp = (double)val;
		case .Ptr: *(int*)sp = (int)val;
		}
	}

	[Inline]
	public static void ConvertU8(uint8* sp, LumaPrimitiveTypeCode type)
	{
		// Get i8 val
		uint8 val = *(uint8*)sp;

		switch(type)
		{
		case .I8: *(int32*)sp = (int8)val;
		case .U8: *(int32*)sp = (uint8)val;
		case .I16: *(int32*)sp = (int16)val;
		case .U16: *(int32*)sp = (uint16)val;
		case .I32: *(int32*)sp = (int32)val;
		case .U32: *(uint32*)sp = (uint32)val;
		case .I64: *(int64*)sp = (int64)val;
		case .U64: *(uint16*)sp = (uint16)val;
		case .F32: *(float*)sp = (float)val;
		case .F64: *(double*)sp = (double)val;
		case .Ptr: *(int*)sp = (int)val;
		}
	}

	[Inline]
	public static void ConvertI16(uint8* sp, LumaPrimitiveTypeCode type)
	{
		// Get i8 val
		int16 val = *(int16*)sp;

		switch(type)
		{
		case .I8: *(int32*)sp = (int8)val;
		case .U8: *(int32*)sp = (uint8)val;
		case .I16: *(int32*)sp = (int16)val;
		case .U16: *(int32*)sp = (uint16)val;
		case .I32: *(int32*)sp = (int32)val;
		case .U32: *(uint32*)sp = (uint32)val;
		case .I64: *(int64*)sp = (int64)val;
		case .U64: *(uint16*)sp = (uint16)val;
		case .F32: *(float*)sp = (float)val;
		case .F64: *(double*)sp = (double)val;
		case .Ptr: *(int*)sp = (int)val;
		}
	}

	[Inline]
	public static void ConvertU16(uint8* sp, LumaPrimitiveTypeCode type)
	{
		// Get i8 val
		uint16 val = *(uint16*)sp;

		switch(type)
		{
		case .I8: *(int32*)sp = (int8)val;
		case .U8: *(int32*)sp = (uint8)val;
		case .I16: *(int32*)sp = (int16)val;
		case .U16: *(int32*)sp = (uint16)val;
		case .I32: *(int32*)sp = (int32)val;
		case .U32: *(uint32*)sp = (uint32)val;
		case .I64: *(int64*)sp = (int64)val;
		case .U64: *(uint16*)sp = (uint16)val;
		case .F32: *(float*)sp = (float)val;
		case .F64: *(double*)sp = (double)val;
		case .Ptr: *(int*)sp = (int)val;
		}
	}

	[Inline]
	public static void ConvertI32(uint8* sp, LumaPrimitiveTypeCode type)
	{
		// Get i8 val
		int32 val = *(int32*)sp;

		switch(type)
		{
		case .I8: *(int32*)sp = (int8)val;
		case .U8: *(int32*)sp = (uint8)val;
		case .I16: *(int32*)sp = (int16)val;
		case .U16: *(int32*)sp = (uint16)val;
		case .I32: *(int32*)sp = (int32)val;
		case .U32: *(uint32*)sp = (uint32)val;
		case .I64: *(int64*)sp = (int64)val;
		case .U64: *(uint16*)sp = (uint16)val;
		case .F32: *(float*)sp = (float)val;
		case .F64: *(double*)sp = (double)val;
		case .Ptr: *(int*)sp = (int)val;
		}
	}

	[Inline]
	public static void ConvertU32(uint8* sp, LumaPrimitiveTypeCode type)
	{
		// Get i8 val
		uint32 val = *(uint32*)sp;

		switch(type)
		{
		case .I8: *(int32*)sp = (int8)val;
		case .U8: *(int32*)sp = (uint8)val;
		case .I16: *(int32*)sp = (int16)val;
		case .U16: *(int32*)sp = (uint16)val;
		case .I32: *(int32*)sp = (int32)val;
		case .U32: *(uint32*)sp = (uint32)val;
		case .I64: *(int64*)sp = (int64)val;
		case .U64: *(uint16*)sp = (uint16)val;
		case .F32: *(float*)sp = (float)val;
		case .F64: *(double*)sp = (double)val;
		case .Ptr: *(int*)sp = (int)val;
		}
	}

	[Inline]
	public static void ConvertI64(uint8* sp, LumaPrimitiveTypeCode type)
	{
		// Get i8 val
		int64 val = *(int64*)sp;

		switch(type)
		{
		case .I8: *(int32*)sp = (int8)val;
		case .U8: *(int32*)sp = (uint8)val;
		case .I16: *(int32*)sp = (int16)val;
		case .U16: *(int32*)sp = (uint16)val;
		case .I32: *(int32*)sp = (int32)val;
		case .U32: *(uint32*)sp = (uint32)val;
		case .I64: *(int64*)sp = (int64)val;
		case .U64: *(uint16*)sp = (uint16)val;
		case .F32: *(float*)sp = (float)val;
		case .F64: *(double*)sp = (double)val;
		case .Ptr: *(int*)sp = (int)val;
		}
	}

	[Inline]
	public static void ConvertU64(uint8* sp, LumaPrimitiveTypeCode type)
	{
		// Get i8 val
		uint64 val = *(uint64*)sp;

		switch(type)
		{
		case .I8: *(int32*)sp = (int8)val;
		case .U8: *(int32*)sp = (uint8)val;
		case .I16: *(int32*)sp = (int16)val;
		case .U16: *(int32*)sp = (uint16)val;
		case .I32: *(int32*)sp = (int32)val;
		case .U32: *(uint32*)sp = (uint32)val;
		case .I64: *(int64*)sp = (int64)val;
		case .U64: *(uint16*)sp = (uint16)val;
		case .F32: *(float*)sp = (float)val;
		case .F64: *(double*)sp = (double)val;
		case .Ptr: *(int*)sp = (int)val;
		}
	}

	[Inline]
	public static void ConvertF32(uint8* sp, LumaPrimitiveTypeCode type)
	{
		// Get i8 val
		float val = *(float*)sp;

		switch(type)
		{
		case .I8: *(int32*)sp = (int8)val;
		case .U8: *(int32*)sp = (uint8)val;
		case .I16: *(int32*)sp = (int16)val;
		case .U16: *(int32*)sp = (uint16)val;
		case .I32: *(int32*)sp = (int32)val;
		case .U32: *(uint32*)sp = (uint32)val;
		case .I64: *(int64*)sp = (int64)val;
		case .U64: *(uint16*)sp = (uint16)val;
		case .F32: *(float*)sp = (float)val;
		case .F64: *(double*)sp = (double)val;
		case .Ptr: *(int*)sp = (int)val;
		}
	}

	[Inline]
	public static void ConvertF64(uint8* sp, LumaPrimitiveTypeCode type)
	{
		// Get i8 val
		double val = *(double*)sp;

		switch(type)
		{
		case .I8: *(int32*)sp = (int8)val;
		case .U8: *(int32*)sp = (uint8)val;
		case .I16: *(int32*)sp = (int16)val;
		case .U16: *(int32*)sp = (uint16)val;
		case .I32: *(int32*)sp = (int32)val;
		case .U32: *(uint32*)sp = (uint32)val;
		case .I64: *(int64*)sp = (int64)val;
		case .U64: *(uint16*)sp = (uint16)val;
		case .F32: *(float*)sp = (float)val;
		case .F64: *(double*)sp = (double)val;
		case .Ptr: *(int*)sp = (int)val;
		}
	}

	[Inline]
	public static void ConvertPtr(uint8* sp, LumaPrimitiveTypeCode type)
	{
		// Get i8 val
		int val = *(int*)sp;

		switch(type)
		{
		case .I8: *(int32*)sp = (int8)val;
		case .U8: *(int32*)sp = (uint8)val;
		case .I16: *(int32*)sp = (int16)val;
		case .U16: *(int32*)sp = (uint16)val;
		case .I32: *(int32*)sp = (int32)val;
		case .U32: *(uint32*)sp = (uint32)val;
		case .I64: *(int64*)sp = (int64)val;
		case .U64: *(uint16*)sp = (uint16)val;
		case .F32: *(float*)sp = (float)val;
		case .F64: *(double*)sp = (double)val;
		case .Ptr: *(int*)sp = (int)val;
		}
	}
}