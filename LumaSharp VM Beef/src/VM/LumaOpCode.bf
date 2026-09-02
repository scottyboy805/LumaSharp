namespace LumaSharpVMBeef;

public enum LumaOpCode : uint8
{
	Nop = 0,

	Pop = 0x1,
	Dup = 0x2,

	// Const
	Ld_I1 = 0x10,           // 1 byte data
	Ld_I2 = 0x11,           // 2 byte data
	Ld_I4 = 0x12,           // 4 byte data
	Ld_I8 = 0x13,           // 8 byte data
	Ld_F4 = 0x14,           // 4 byte data
	Ld_F8 = 0x15,           // 8 byte data
	Ld_Str = 0x16,          // 4 byte data - offset into string table
	Ld_Null = 0x17,         // No data

	Ld_I4_0 = 0x18,         // 4 byte data - Load 32 bit int with value of '0'
	Ld_I4_1 = 0x19,         // 4 byte data - Load 32 bit int with value of '1'
	Ld_I4_M1 = 0x1A,        // 4 byte data -  Load 32 bit int with value of '-1'
	Ld_F4_0 = 0x1B,         // 4 byte data - Load 32 bit float with value 0f '0'

	// Locals
	Ld_Loc_0 = 0x20,        // No data
	Ld_Loc_1 = 0x21,        // No data
	Ld_Loc_2 = 0x22,        // No data
	Ld_Loc_3 = 0x23,        // No data
	Ld_Loc_4 = 0x24,        // No data
	Ld_Loc = 0x25,          // 1 byte data
	Ld_Loc_E = 0x26,        // 2 byte data
	Ld_Loc_A = 0x27,        // 1 byte data
	Ld_Loc_EA = 0x28,       // 2 byte data
	St_Loc_0 = 0x29,        // No data
	St_Loc_1 = 0x2A,        // No data
	St_Loc_2 = 0x2B,        // No data
	St_Loc_3 = 0x2C,        // No data
	St_Loc_4 = 0x2D,        // No data
	St_Loc = 0x2E,          // 1 byte data
	St_Loc_E = 0x2F,        // 2 byte data

	// Arguments
	Ld_Arg_0 = 0x30,        // No data
	Ld_Arg_1 = 0x31,        // No data
	Ld_Arg_2 = 0x32,        // No data
	Ld_Arg_3 = 0x33,        // No data
	Ld_Arg_4 = 0x34,        // No data
	Ld_Arg = 0x35,          // 1 byte data
	Ld_Arg_E = 0x36,        // 2 byte data
	Ld_Arg_A = 0x37,        // 1 byte data
	Ld_Arg_EA = 0x38,       // 2 byte data
	St_Arg_0 = 0x39,        // No data
	St_Arg_1 = 0x3A,        // No data
	St_Arg_2 = 0x3B,        // No data
	St_Arg_3 = 0x3C,        // No data
	St_Arg_4 = 0x3D,        // No data
	St_Arg = 0x3E,          // 1 byte data
	St_Arg_E = 0x3F,        // 2 byte data

	// Fields
	Ld_Fld = 0x40,          // 4 byte data - field token
	Ld_Fld_A = 0x41,        // 4 byte data - field token
	St_Fld = 0x42,          // 4 byte data - field token

	// Arrays
	NewArr = 0x50,          // 4 byte data - type token
	NewArr_S = 0x51,        // 4 byte data - type token
	Ld_Elem = 0x52,         // No data
	Ld_Elem_A = 0x53,       // No data
	St_Elem = 0x54,         // No data

	// Indirect
	Ld_Addr_I1 = 0x61,      // No data
	Ld_Addr_I2 = 0x62,      // No data
	Ld_Addr_I4 = 0x63,      // No data
	Ld_Addr_I8 = 0x64,      // No data
	Ld_Addr_F4 = 0x65,      // No data
	Ld_Addr_F8 = 0x66,      // No data
	Ld_Addr_Any = 0x67,     // 4 byte data - type token
	St_Addr_I1 = 0x68,      // No data
	St_Addr_I2 = 0x69,      // No data
	St_Addr_I4 = 0x6A,      // No data
	St_Addr_I8 = 0x6B,      // No data
	St_Addr_F4 = 0x6C,      // No data
	St_Addr_F8 = 0x6D,      // No data
	St_Addr_Any = 0x6E,     // 4 byte data - type token

	// Arithmetic
	Add = 0x71,             // No data
	Sub = 0x72,             // No data
	Mul = 0x73,             // No data
	Div = 0x74,             // No data
	Neg = 0x75,             // No data
	Mod = 0x76,
	Bit_And = 0x81,
	Bit_Or = 0x82,
	Bit_XOr = 0x83,
	Bit_Not = 0x84,
	Bit_Shl = 0x85,         // No data
	Bit_Shr = 0x86,         // No data
	Cmp_L = 0x95,           // No data
	Cmp_Le = 0x96,          // No data
	Cmp_G = 0x97,           // No data
	Cmp_Ge = 0x98,          // No data
	Cmp_Eq = 0x99,          // No data
	Cmp_NEq = 0x9A,         // No data

	// Convert
	Cast_I1 = 0xA1,         // No data
	Cast_I2 = 0xA2,         // No data
	Cast_I4 = 0xA3,         // No data
	Cast_I8 = 0XA4,         // No data
	Cast_F4 = 0xA5,         // No data
	Cast_F8 = 0xA6,         // No data
	Cast_UI1 = 0xA7,        // No data
	Cast_UI2 = 0xA8,        // No data
	Cast_UI4 = 0xA9,        // No data
	Cast_UI8 = 0xAA,        // No data
	Cast_Addr = 0xAB,       // No data

	// Jump
	Jmp = 0xB1,             // 4 byte instruction offset
	Jmp_1 = 0xB2,           // 4 byte instruction offset
	Jmp_0 = 0xB3,           // 4 byte instruction offset
	Jmp_Eq = 0xB4,          // 1 byte type code, 4 byte instruction offset
	Jmp_NEq = 0xB5,         // 1 byte type code, 4 byte instruction offset
	Jmp_L = 0xB6,           // 1 byte type code, 4 byte instruction offset
	Jmp_Le = 0xB7,          // 1 byte type code, 4 byte instruction offset
	Jmp_G = 0xB8,           // 1 byte type code, 4 byte instruction offset
	Jmp_Ge = 0xB9,          // 1 byte type code, 4 byte instruction offset

	// Obj
	New = 0xF1,             // 4 byte data - type token
	New_S = 0xF2,           // 4 byte data - type token
	Call = 0xF3,            // 4 byte data - method token
	Call_Addr = 0xF4,
	Is_Any = 0xF5,          // 4 byte data - type token - Is obj of specified type
	As_Any = 0xF6,          // 4 byte data - type token - Convert object or primitive to object of type
	From_Any = 0xF7,        // 4 byte data - type token - Convert primitive boxed value to primitive
	Ld_Size = 0xF8,         // No data
	Ld_Type = 0xF9,			// No data
	Ld_Func = 0xFA,         // 4 byte data - type token
	Ret = 0xFB,             // No data
	Throw = 0xFC,           // No data
}

public enum LumaOperandType : uint8
{
	None = 0,
	InlineI1,
	InlineI2,
	InlineI4,
	InlineI8,
	InlineF4,
	InlineF8,
	InlineOffset,
	InlineOffsetE,
	InlineBranch,
	InlineBranchE,
	InlineToken,
}

extension LumaOpCode
{
	// Methods
	public LumaOperandType GetOperandType()
	{
		switch(this)
		{
		case .Ld_I1: return .InlineI1;
		case .Ld_I2: return .InlineI2;
		case .Ld_I4, .Ld_I4_0, .Ld_I4_1, .Ld_I4_M1: return .InlineI4;
		case .Ld_I8: return .InlineI8;
		case .Ld_F4, .Ld_F4_0: return .InlineF4;
		case .Ld_F8: return .InlineF8;
		case .Ld_Str: return .InlineToken;

		case .Ld_Loc, .Ld_Loc_A, .St_Loc, .Ld_Arg, .Ld_Arg_A, .St_Arg: return .InlineOffset;
		case .Ld_Loc_E, .Ld_Loc_EA, .St_Loc_E, .Ld_Arg_E, .Ld_Arg_EA, .St_Arg_E: return .InlineOffsetE;

		case .Ld_Fld, .Ld_Fld_A, .St_Fld: return .InlineToken;
		case .New, .New_S, .NewArr, .NewArr_S: return .InlineToken;

		case .Ld_Addr_Any, .St_Addr_Any: return .InlineToken;
		case .Call, .Is_Any, .As_Any, .From_Any, .Ld_Func: return .InlineToken;

		case .Jmp, .Jmp_0, .Jmp_1, .Jmp_Eq, .Jmp_G, .Jmp_Ge, .Jmp_L, .Jmp_Le, .Jmp_NEq: return .InlineBranch;

		default: return .None;
		}
	}
}