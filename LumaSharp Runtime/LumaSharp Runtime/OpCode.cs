
namespace LumaSharp.Runtime
{
    public enum OpCode : byte
    {
        Nop = 0x00,

        // Const
        Ld_I1 = 0x11,           // 1 byte data
        Ld_I2 = 0x12,           // 2 byte data
        Ld_I4 = 0x13,           // 4 byte data
        Ld_I8 = 0x14,           // 8 byte data
        Ld_F4 = 0x15,           // 4 byte data
        Ld_F8 = 0x16,           // 8 byte data
        Ld_Str = 0x17,          // 4 byte data - offset into string table
        Ld_Null = 0x18,         // No data

        Ld_I4_0 = 0x19,         // 4 byte data - Load 32 bit int with value of '0'
        Ld_I4_1 = 0x1A,         // 4 byte data - Load 32 bit int with value of '1'
        Ld_I4_M1 = 0x1B,        // 4 byte data -  Load 32 bit int with value of '-1'
        Ld_F4_0 = 0x1C,         // 4 byte data - Load 32 bit float with value 0f '0'

        // Var
        Ld_Var_0 = 0x21,       // No data
        Ld_Var_1 = 0x22,       // No data
        Ld_Var_2 = 0x23,       // No data
        Ld_Var_3 = 0x24,       // No data
        Ld_Var = 0x25,         // 1 byte data
        Ld_Var_E = 0x26,       // 2 byte data
        Ld_Var_A = 0x27,       // 1 byte data
        Ld_Var_EA = 0x28,      // 2 byte data
        St_Var_0 = 0x29,       // No data
        St_Var_1 = 0x2A,       // No data
        St_Var_2 = 0x2B,       // No data
        St_Var_3 = 0x2C,       // No data
        St_Var = 0x2D,         // 1 byte data
        St_Var_E = 0x2E,       // 2 byte data

        // Fields
        Ld_Fld = 0x31,          // 4 byte data - field token
        Ld_Fld_S = 0x32,        // 4 byte data - field token
        Ld_Fld_A = 0x33,        // 4 byte data - field token
        Ld_Fls_SA = 0x34,       // 4 byte data - field token
        St_Fld = 0x35,          // 4 byte data - field token
        St_Fld_S = 0x36,        // 4 byte data - field token

        // Arrays
        Ld_Len = 0x41,          // No data
        Ld_Elem = 0x42,         // No data
        Ld_Elem_A = 0x43,       // No data
        St_Elem = 0x44,         // No data

        // Address
        Ld_Addr = 0x51,         // No data
        St_Addr = 0x52,         // No data

        // Arithmetic
        Add = 0x71,             // No data
        Sub = 0x72,             // No data
        Mul= 0x73,              // No data
        Div = 0x74,             // No data
        Neg = 0x81,             // No data
        And = 0x82,             // No data
        Or = 0x83,              // No data
        XOr = 0x84,             // No data
        Bit_Shl = 0x86,         // No data
        Bit_Shr = 0x87,         // No data
        Mod = 0x88,             // No data

        // Compare
        Cmp_L = 0x92,           // No data
        Cmp_Le = 0x93,          // No data
        Cmp_G = 0x94,           // No data
        Cmp_Ge = 0x95,          // No data
        Cmp_Eq = 0x96,          // No data
        Cmp_NEq = 0x97,         // No data

        // Convert      
        Cast_I4 = 0xA1,         
        Cast_I8 = 0XA2,     
        Cast_U4 = 0xA3,        
        Cast_U8 = 0xA4,
        Cast_F4 = 0xA5,
        Cast_F8 = 0xA6,
        Cast_Any = 0xA7,        // 4 byte data - type token

        // Jump
        Jmp_1 = 0xB1,           // 4 byte instruction offset
        Jmp_0 = 0xB2,           // 4 byte instruction offset
        Jmp = 0xB3,             // 4 byte instruction offset

        // Obj
        New = 0xF1,             // 4 byte data - type token
        NewArr = 0xF2,          // 4 byte data - type token
        Call = 0xF4,            // 4 byte data - method token
        Call_Virt = 0xF5,        // 4 byte data - method token
        Call_Addr = 0xF6,       // No data
        Is_Any = 0xF7,          // 4 byte data - type token - Is obj of specified type
        As_Any = 0xF8,          // 4 byte data - type token - Convert object or primitive to object of type
        From_Any = 0xF9,        // 4 byte data - type token - Convert primitive boxed value to primitive
        Ld_Size = 0xFA,         // No data
        Ld_Type = 0xFB,			// 4 byte data - type token
        Ld_Func = 0xFC,         // 4 byte data - type token
        Ret = 0xFD,             // No data
        Throw = 0xFE,           // No data
    }

    public enum OperandType
    {
        InlineNone,
        InlineI1,
        InlineI2,
        InlineI4,
        InlineI8,
        InlineF4,
        InlineF8,
        InlineToken,
    }

    public static class OpCodeExtensions
    {
        // Methods
        public static bool IsJump(this OpCode code)
        {
            switch(code)
            {
                case OpCode.Jmp:
                case OpCode.Jmp_0:
                case OpCode.Jmp_1:
                    return true;
            }
            return false;
        }

        public static int GetOperandSize(this OpCode code)
        {
            return code.GetOperandType() switch
            { 
                OperandType.InlineI1 => 1,
                OperandType.InlineI2 => 2,
                OperandType.InlineI4 => 4,
                OperandType.InlineI8 => 8,
                OperandType.InlineF4 => 4,
                OperandType.InlineF8 => 8,
                OperandType.InlineToken => 4,
                _ => 0,
            };
        }

        public static OperandType GetOperandType(this OpCode code)
        {
            switch(code)
            {
                // Constant
                case OpCode.Ld_I1: return OperandType.InlineI1;
                case OpCode.Ld_I2: return OperandType.InlineI2;
                case OpCode.Ld_I4: return OperandType.InlineI4;
                case OpCode.Ld_I8: return OperandType.InlineI8;
                case OpCode.Ld_F4: return OperandType.InlineF4;
                case OpCode.Ld_F8: return OperandType.InlineF8;

                // Variable
                case OpCode.St_Var:
                case OpCode.Ld_Var:
                case OpCode.Ld_Var_A: return OperandType.InlineI1;
                case OpCode.St_Var_E:
                case OpCode.Ld_Var_EA:
                case OpCode.Ld_Var_E: return OperandType.InlineI2;

                // Fields
                case OpCode.Ld_Fld:
                case OpCode.Ld_Fld_S:
                case OpCode.Ld_Fld_A:
                case OpCode.Ld_Fls_SA:
                case OpCode.St_Fld:
                case OpCode.St_Fld_S: return OperandType.InlineToken;

                // Cast
                case OpCode.Cast_Any: return OperandType.InlineToken;

                // Jump
                case OpCode.Jmp_1:
                case OpCode.Jmp_0:
                case OpCode.Jmp: return OperandType.InlineI4;

                // Object
                case OpCode.New:
                case OpCode.NewArr:
                case OpCode.Call:
                case OpCode.Call_Virt:
                case OpCode.Is_Any:
                case OpCode.As_Any:
                case OpCode.From_Any:
                case OpCode.Ld_Type:
                case OpCode.Ld_Func:  return OperandType.InlineToken;
            }
            return OperandType.InlineNone;
        }
    }
}
