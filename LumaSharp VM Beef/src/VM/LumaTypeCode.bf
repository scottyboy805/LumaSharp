namespace LumaSharpVMBeef;

public enum LumaTypeCode : uint8
{
	Void = 0,
	Any = 1,
	Bool,
	Char,
	I8 = 4,
	U8,
	I16,
	U16,
	I32,
	U32,
	I64,
	U64,
	F32,
	F64,
}

public enum LumaPrimitiveTypeCode : uint8
{
	I8 = 4,
	U8,
	I16,
	U16,
	I32,
	U32,
	I64,
	U64,
	F32,
	F64,
	Ptr,
}

extension LumaPrimitiveTypeCode
{
	public uint32 GetTypeByteSize()
	{
		switch(this)
		{
		case .I8: return 1;
		case .U8: return 1; 
		case .I16: return 2;
		case .U16: return 2;
		case .I32: return 4;
		case .U32: return 4;
		case .I64: return 8;
		case .U64: return 8;
		case .F32: return 4;
		case .F64: return 8;
		case .Ptr: return sizeof(int);
		}
	}
}