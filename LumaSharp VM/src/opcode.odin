package LumaVM

LumaOpCode :: enum u8
{
    Nop = 0,

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
    Ld_Loc = 0x23,          // 1 byte data
    Ld_Loc_E = 0x24,        // 2 byte data
    Ld_Loc_A = 0x25,        // 1 byte data
    Ld_Loc_EA = 0x26,       // 2 byte data
    St_Loc_0 = 0x27,        // No data
    St_Loc_1 = 0x28,        // No data
    St_Loc_2 = 0x29,        // No data
    St_Loc = 0x2A,          // 1 byte data
    St_Loc_E = 0x2B,        // 2 byte data

    // Arguments
    Ld_Arg_0 = 0x30,        // No data
    Ld_Arg_1 = 0x31,        // No data
    Ld_Arg_2 = 0x32,        // No data
    Ld_Arg_3 = 0x33,        // No data
    Ld_Arg = 0x34,          // 1 byte data
    Ld_Arg_E = 0x35,        // 2 byte data
    Ld_Arg_A = 0x36,        // 1 byte data
    Ld_Arg_EA = 0x37,       // 2 byte data
    St_Arg_0 = 0x38,        // No data
    St_Arg_1 = 0x39,        // No data
    St_Arg_2 = 0x3A,        // No data
    St_Arg_3 = 0x3B,        // No data
    St_Arg = 0x3C,          // 1 byte data
    St_Arg_E = 0x3D,        // 2 byte data

    // Fields
    Ld_Fld = 0x40,          // 4 byte data - field token
    Ld_Fld_A = 0x41,        // 4 byte data - field token
    St_Fld = 0x42,          // 4 byte data - field token

    // Arrays
    Ld_Elem = 0x50,         // No data
    Ld_Elem_A = 0x51,       // No data
    St_Elem = 0x52,         // No data

    // Indirect
    Ld_Addr_I1 = 0x61,      // No data
    Ld_Addr_I2 = 0x62,      // No data
    Ld_Addr_I4 = 0x63,      // No data
    Ld_Addr_I8 = 0x64,      // No data
    Ld_Addr_F4 = 0x65,      // No data
    Ld_Addr_F8 = 0x66,      // No data
    Ld_Addr_Any = 0x67,     // No data
    St_Addr_I1 = 0x68,      // No data
    St_Addr_I2 = 0x69,      // No data
    St_Addr_I4 = 0x6A,      // No data
    St_Addr_I8 = 0x6B,      // No data
    St_Addr_F4 = 0x6C,      // No data
    St_Addr_F8 = 0x6D,      // No data
    St_Addr_Any = 0x6E,     // No data

    // Arithmetic
    Add = 0x71,             // 1 byte type code
    Sub = 0x75,
    Mul= 0x79,
    Div = 0x7D,
    Neg = 0x81,
    And = 0x85,
    Or = 0x87,
    XOr = 0x89,
    Not = 0x8B,
    Bit_Shl = 0x8D,
    Bit_Shr = 0x8F,
    Mod = 0x91,
    Cmp_L = 0x95,           // 1 byte type code
    Cmp_Le = 0x96,          // 1 byte type code
    Cmp_G = 0x97,           // 1 byte type code
    Cmp_Ge = 0x98,          // 1 byte type code
    Cmp_Eq = 0x99,          // 1 byte type code
    Cmp_NEq = 0x9A,         // 1 byte type code

    // Convert
    Cast_I1 = 0xA1,         // 1 byte data - primitive type token - cast 8 bit integer on top of stack to type specified by token
    Cast_I2 = 0xA2,         // 1 byte data - primitive type token
    Cast_I4 = 0xA3,         // 1 byte data - primitive type token
    // Cast_I8 = 0XA4,         // 1 byte data - primitive type token
    // Cast_F4 = 0xA5,         // 1 byte data - primitive type token
    // Cast_F8 = 0xA6,         // 1 byte data - primitive type token
    // Cast_UI1 = 0xA7,        // 1 byte data - primitive type token
    // Cast_UI2 = 0xA8,        // 1 byte data - primitive type token
    // Cast_UI4 = 0xA9,        // 1 byte data - primitive type token
    // Cast_UI8 = 0xAA,        // 1 byte data - primitive type token
    // Cast_Any = 0xAB,        // 4 byte data - type token

    // Jump
    // Jmp_Eq = 0xB1,          // 1 byte type code, 4 byte instruction offset
    // Jmp_NEq = 0xB2,         // 1 byte type code, 4 byte instruction offset
    Jmp_1 = 0xB3,           // 4 byte instruction offset
    Jmp_0 = 0xB4,           // 4 byte instruction offset
    // Jmp_L = 0xB5,           // 1 byte type code, 4 byte instruction offset
    // Jmp_Le = 0xB6,          // 1 byte type code, 4 byte instruction offset
    // Jmp_G = 0xB7,           // 1 byte type code, 4 byte instruction offset
    // Jmp_Ge = 0xB8,          // 1 byte type code, 4 byte instruction offset
    Jmp = 0xB9,             // 4 byte instruction offset

    // Obj
    New = 0xF1,             // 4 byte data - type token
    New_S = 0xF2,           // 4 byte data - type token
    NewArr = 0xF3,          // 4 byte data - type token
    NewArr_S = 0xF4,        // 4 byte data - type token
    Call = 0xF5,            // 4 byte data - method token
    Call_Addr = 0xF6,
    Is_Any = 0xF7,          // 4 byte data - type token - Is obj of specified type
    As_Any = 0xF8,          // 4 byte data - type token - Convert object or primitive to object of type
    From_Any = 0xF9,        // 4 byte data - type token - Convert primitive boxed value to primitive
    Ld_Size = 0xFA,         // No data
    Ld_Type = 0xFB,			// No data
    Ld_Func = 0xFC,         // 4 byte data - type token
    Ret = 0xFD,             // No data
    Throw = 0xFE,           // No data
}