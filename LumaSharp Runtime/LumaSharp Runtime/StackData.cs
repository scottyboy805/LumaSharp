using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LumaSharp.Runtime
{
    internal enum StackTypeCode : uint
    {
        Invalid = 0,
        I32,
        U32,
        I64,
        U64,
        F32,
        F64,
        Address,
    };

    [StructLayout(LayoutKind.Explicit)]
    internal struct StackData       // 16 bytes with padding
    {
        // Access to primitive data for common operations
        [FieldOffset(0)]
        public int I32;
        [FieldOffset(0)]
        public long I64;
        [FieldOffset(0)]
        public float F32;
        [FieldOffset(0)]
        public double F64;

        [FieldOffset(0)]
        public IntPtr Ptr;              // 8 bytes

        // The type of data stored on the stack
        [FieldOffset(8)]
        public StackTypeCode Type;      // 12 bytes

        [FieldOffset(12)]
        public RuntimeTypeCode TypeCode;       // 16 bytes

        // Methods
        public override string ToString()
        {
            return Type switch
            { 
                StackTypeCode.I32 => string.Format("I32 {0}", I32),
                StackTypeCode.U32 => string.Format("U32 {0}", (uint)I32),
                StackTypeCode.I64 => string.Format("I64 {0}", I64),
                StackTypeCode.U64 => string.Format("U64 {0}", (ulong)I64),
                StackTypeCode.F32 => string.Format("F32 {0}", F32),
                StackTypeCode.F64 => string.Format("F64 {0}", F64),
                StackTypeCode.Address => Ptr != IntPtr.Zero 
                    ? string.Format("Address {0:X4}", Ptr)
                    : "Address Null",
                _ => "Invalid",
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void CopyStack(StackData* src, StackData* dst)
        {
            // Copy to dst
            *dst = *src;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void CopyToMemory(StackData* src, byte* mem, RuntimeTypeCode typeCode)
        {
            switch(typeCode)
            {
                default: throw new NotSupportedException(typeCode.ToString());
                case RuntimeTypeCode.Bool:
                case RuntimeTypeCode.I8: *(sbyte*)mem = (sbyte)src->I32; break;
                case RuntimeTypeCode.U8: *(byte*)mem = (byte)src->I32; break;
                case RuntimeTypeCode.Char:
                case RuntimeTypeCode.I16: *(short*)mem = (short)src->I32; break;
                case RuntimeTypeCode.U16: *(ushort*)mem = (ushort)src->I32; break;
                case RuntimeTypeCode.I32: *(int*)mem = (int)src->I32; break;
                case RuntimeTypeCode.U32: *(uint*)mem = (uint)src->I32; break;
                case RuntimeTypeCode.I64: *(long*)mem = (long)src->I64; break;
                case RuntimeTypeCode.U64: *(ulong*)mem = (ulong)src->I64; break;
                case RuntimeTypeCode.F32: *(float*)mem = (float)src->F32; break;
                case RuntimeTypeCode.F64: *(double*)mem = (double)src->F64; break;
                case RuntimeTypeCode.Any:
                case RuntimeTypeCode.Ptr: *(IntPtr*)mem = src->Ptr; break;
                case RuntimeTypeCode.UPtr: *(UIntPtr*)mem = (UIntPtr)src->Ptr; break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void CopyFromMemory(StackData* dst, byte* mem, RuntimeTypeCode typeCode)
        {
            switch(typeCode)
            {
                default: throw new NotSupportedException(typeCode.ToString());
                case RuntimeTypeCode.Bool:
                case RuntimeTypeCode.I8:
                    {
                        dst->Type = StackTypeCode.I32;
                        dst->I32 = *(sbyte*)mem;
                        break;
                    }
                case RuntimeTypeCode.U8:
                    {
                        dst->Type = StackTypeCode.I32;
                        dst->I32 = *(byte*)mem;
                        break;
                    }
                case RuntimeTypeCode.Char:
                case RuntimeTypeCode.I16:
                    {
                        dst->Type = StackTypeCode.I32;
                        dst->I32 = *(short*)mem;
                        break;
                    }
                case RuntimeTypeCode.U16:
                    {
                        dst->Type = StackTypeCode.I32;
                        dst->I32 = *(ushort*)mem;
                        break;
                    }
                case RuntimeTypeCode.I32:
                    {
                        dst->Type = StackTypeCode.I32;
                        dst->I32 = *(int*)mem;
                        break;
                    }
                case RuntimeTypeCode.U32:
                    {
                        dst->Type = StackTypeCode.U32;
                        dst->I32 = (int)*(uint*)mem;
                        break;
                    }
                case RuntimeTypeCode.I64:
                    {
                        dst->Type = StackTypeCode.I64;
                        dst->I64 = *(long*)mem;
                        break;
                    }
                case RuntimeTypeCode.U64:
                    {
                        dst->Type = StackTypeCode.U64;
                        dst->I64 = (long)*(ulong*)mem;
                        break;
                    }
                case RuntimeTypeCode.F32:
                    {
                        dst->Type = StackTypeCode.F32;
                        dst->F32 = *(float*)mem;
                        break;
                    }
                case RuntimeTypeCode.F64:
                    {
                        dst->Type = StackTypeCode.F64;
                        dst->F64 = *(double*)mem;
                        break;
                    }
                case RuntimeTypeCode.Any:
                case RuntimeTypeCode.Ptr:
                    {
                        dst->Type = StackTypeCode.Address;
                        dst->Ptr = *(IntPtr*)mem;
                        break;
                    }
                case RuntimeTypeCode.UPtr:
                    {
                        dst->Type = StackTypeCode.Address;
                        dst->Ptr = (IntPtr)(*(UIntPtr*)mem);
                        break;
                    }
            }
        }
    }
}
