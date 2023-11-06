
using LumaSharp.Runtime.Handle;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LumaSharp RuntimeTests")]

namespace LumaSharp.Runtime
{
    internal static unsafe class __interpreter
    {
        // Methods
        internal static T FetchValue<T>(int offset = 0) where T : struct
        {
            // Get address
            byte* valPtr = (byte*)__memory.stackBasePtr + offset;

            // Get as T
            return *((T*)valPtr);
        }

        internal static T FetchValue<T>(byte* stackPtr, int offset = 0) where T : struct
        {
            // Get address
            byte* valPtr = stackPtr + offset;

            // Get as T
            return *((T*)valPtr);
        }

        internal static IntPtr ExecuteBytecode(byte[] instructions, int argOffset = 0, int localOffset = 0)
        {
            // Get instruction mem
            fixed(byte* instructionPtr  = instructions)
            {
                // Get stack ptr
                byte* stackBasePtr = (byte*)__memory.stackBasePtr;

                // Create method handle
                _MethodHandle method = new _MethodHandle
                {
                    argPtrOffset = (ushort)argOffset,
                    localPtrOffset = (ushort)localOffset,
                    instructionPtr = instructionPtr,
                };

                // Run bytecode
                return (IntPtr)ExecuteBytecode(new _MethodHandle { instructionPtr = instructionPtr}, stackBasePtr);
            }
        }

        internal static byte* ExecuteBytecode(in _MethodHandle method, byte* stackBasePtr)
        {
            // Get instruction ptr
            byte* instructionPtr = method.instructionPtr;

            // Get arg and local ptr
            byte* argPtr = stackBasePtr + method.argPtrOffset;
            byte* localPtr = stackBasePtr + method.localPtrOffset;

            // Get main stack ptr
            byte* stackPtr = stackBasePtr + method.stackPtrOffset;

            bool halt = false;

            // Loop until halt command
            while(halt == false)
            {
                // Get code
                OpCode code = *(OpCode*)instructionPtr++;

                // Evaluate code
                switch(code)
                {
                    default: throw new NotImplementedException("Instruction is not implemented: " + code);

                    // Nop
                    case OpCode.Nop: break;

                    #region LoadConstant
                    case OpCode.Ld_I1:
                        {
                            // Move 8 bit and push as 32 bit onto stack
                            *((int*)stackPtr) = *((byte*)instructionPtr);

                            // Advance instruction ptr
                            instructionPtr += _I8.Size;

                            // Advance stack ptr
                            stackPtr += _I32.Size;
                            break;
                        }
                    case OpCode.Ld_I2:
                        {
                            // Move 16 bit and push as 32 bit onto stack
                            *((int*)stackPtr) = *((short*)instructionPtr);

                            // Advance instruction ptr
                            instructionPtr += _I16.Size;

                            // Advance stack ptr
                            stackPtr += _I32.Size;
                            break;
                        }
                    case OpCode.Ld_I4:
                        {
                            // Move 32 bit
                            *((int*)stackPtr) = *((int*)instructionPtr);

                            // Advance instruction ptr
                            instructionPtr += _I32.Size;

                            // Advance stack ptr
                            stackPtr += _I32.Size;
                            break;
                        }
                    case OpCode.Ld_I8:
                        {
                            // Move 64 bit
                            *((long*)stackPtr) = *((long*)instructionPtr);

                            // Advance instruction ptr
                            instructionPtr += _I64.Size;

                            // Advance stack ptr
                            stackPtr += _I64.Size;
                            break;
                        }
                    case OpCode.Ld_F4:
                        {
                            // Move 32 bit float
                            *((float*)stackPtr) = *((float*)instructionPtr);

                            // Advance instruction ptr
                            instructionPtr += _F32.Size;

                            // Advance stack ptr
                            stackPtr += _F32.Size;
                            break;
                        }
                    case OpCode.Ld_F8:
                        {
                            // Move 64 bit float
                            *((double*)stackPtr) = *((double*)instructionPtr);

                            // Advance instruction ptr
                            instructionPtr += _F64.Size;

                            // Advance stack ptr
                            stackPtr += _F64.Size;
                            break;
                        }
                    case OpCode.Ld_Null:
                        {
                            // Move null ptr
                            *((IntPtr*)stackPtr) = *((IntPtr*)instructionPtr);

                            // Advance stack ptr
                            stackPtr += IntPtr.Size;
                            break;
                        }
                    case OpCode.Ld_I4_0:
                        {
                            // Move 32 bit with value: 0
                            *((int*)stackPtr) = 0;

                            // Advance stack ptr
                            stackPtr += _I32.Size;
                            break;
                        }
                    case OpCode.Ld_I4_1:
                        {
                            // Move 32 bit with value: 1
                            *((int*)stackPtr) = 1;

                            // Advance stack ptr
                            stackPtr += _I32.Size;
                            break;
                        }
                    case OpCode.Ld_I4_M1:
                        {
                            // Move 32 bit with value: 0
                            *((int*)stackPtr) = -1;

                            // Advance stack ptr
                            stackPtr += _I32.Size;
                            break;
                        }
                    case OpCode.Ld_F4_0:
                        {
                            // Move 32 bit float with value: 0
                            *((float*)stackPtr) = 0;

                            // Advance stack ptr
                            stackPtr += _I32.Size;
                            break;
                        }
                    #endregion

                    #region Local
                    case OpCode.St_Loc_0:
                        {
                            // Get local
                            _StackHandle locHandle = method.argLocals[method.localHandleOffset + 0];

                            // Move from stack
                            __memory.Copy(stackPtr - locHandle.typeHandle.size, stackBasePtr + locHandle.offset, locHandle.typeHandle.size);

                            // Decrement stack ptr
                            stackPtr -= locHandle.typeHandle.size;
                            break;
                        }
                    case OpCode.St_Loc_1:
                        {
                            // Get local
                            _StackHandle locHandle = method.argLocals[method.localHandleOffset + 1];

                            // Move from stack
                            __memory.Copy(stackPtr - locHandle.typeHandle.size, stackBasePtr + locHandle.offset, locHandle.typeHandle.size);

                            // Decrement stack ptr
                            stackPtr -= locHandle.typeHandle.size;
                            break;
                        }
                    case OpCode.St_Loc_2:
                        {
                            // Get local
                            _StackHandle locHandle = method.argLocals[method.localHandleOffset + 2];

                            // Move from stack
                            __memory.Copy(stackPtr - locHandle.typeHandle.size, stackBasePtr + locHandle.offset, locHandle.typeHandle.size);

                            // Decrement stack ptr
                            stackPtr -= locHandle.typeHandle.size;
                            break;
                        }
                    case OpCode.Ld_Loc_0:
                        {
                            // Get local
                            _StackHandle locHandle = method.argLocals[method.localHandleOffset + 0];

                            // Move from stack
                            __memory.Copy(stackBasePtr + locHandle.offset, stackPtr - locHandle.typeHandle.size, locHandle.typeHandle.size);

                            // Increment stack ptr
                            stackPtr += locHandle.typeHandle.size;
                            break;
                        }
                    case OpCode.Ld_Loc_1:
                        {
                            // Get local
                            _StackHandle locHandle = method.argLocals[method.localHandleOffset + 1];

                            // Move from stack
                            __memory.Copy(stackBasePtr + locHandle.offset, stackPtr - locHandle.typeHandle.size, locHandle.typeHandle.size);

                            // Increment stack ptr
                            stackPtr += locHandle.typeHandle.size;
                            break;
                        }
                    case OpCode.Ld_Loc_2:
                        {
                            // Get local
                            _StackHandle locHandle = method.argLocals[method.localHandleOffset + 2];

                            // Move from stack
                            __memory.Copy(stackBasePtr + locHandle.offset, stackPtr - locHandle.typeHandle.size, locHandle.typeHandle.size);

                            // Increment stack ptr
                            stackPtr += locHandle.typeHandle.size;
                            break;
                        }
                    #endregion

                    #region Arithmetic
                    case OpCode.Add:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            // Select type
                            switch(opType)
                            {
                                case TypeCode.I32:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = *((int*)stackPtr - 2) + *((int*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I32.Size;
                                        break;
                                    }
                                case TypeCode.I64:
                                    {
                                        // Perform add op
                                        *((long*)stackPtr - 2) = *((long*)stackPtr - 2) + *((long*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size;
                                        break;
                                    }
                                case TypeCode.U64:
                                    {
                                        // Perform add op
                                        *((ulong*)stackPtr - 2) = *((ulong*)stackPtr - 2) + *((ulong*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size;
                                        break;
                                    }
                                case TypeCode.Float:
                                    {
                                        // Perform add op
                                        *((float*)stackPtr - 2) = *((float*)stackPtr - 2) + *((float*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _F32.Size;
                                        break;
                                    }
                                case TypeCode.Double:
                                    {
                                        // Perform add op
                                        *((double*)stackPtr - 2) = *((double*)stackPtr - 2) + *((double*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _F64.Size;
                                        break;
                                    }
                            }
                            break;
                        }
                    case OpCode.Sub:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            // Select type
                            switch (opType)
                            {
                                case TypeCode.I32:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = *((int*)stackPtr - 2) - *((int*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I32.Size;
                                        break;
                                    }
                                case TypeCode.I64:
                                    {
                                        // Perform add op
                                        *((long*)stackPtr - 2) = *((long*)stackPtr - 2) - *((long*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size;
                                        break;
                                    }
                                case TypeCode.U64:
                                    {
                                        // Perform add op
                                        *((ulong*)stackPtr - 2) = *((ulong*)stackPtr - 2) - *((ulong*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size;
                                        break;
                                    }
                                case TypeCode.Float:
                                    {
                                        // Perform add op
                                        *((float*)stackPtr - 2) = *((float*)stackPtr - 2) - *((float*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _F32.Size;
                                        break;
                                    }
                                case TypeCode.Double:
                                    {
                                        // Perform add op
                                        *((double*)stackPtr - 2) = *((double*)stackPtr - 2) - *((double*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _F64.Size;
                                        break;
                                    }
                            }
                            break;
                        }
                    case OpCode.Mul:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            // Select type
                            switch (opType)
                            {
                                case TypeCode.I32:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = *((int*)stackPtr - 2) * *((int*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I32.Size;
                                        break;
                                    }
                                case TypeCode.I64:
                                    {
                                        // Perform add op
                                        *((long*)stackPtr - 2) = *((long*)stackPtr - 2) * *((long*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size;
                                        break;
                                    }
                                case TypeCode.U64:
                                    {
                                        // Perform add op
                                        *((ulong*)stackPtr - 2) = *((ulong*)stackPtr - 2) * *((ulong*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size;
                                        break;
                                    }
                                case TypeCode.Float:
                                    {
                                        // Perform add op
                                        *((float*)stackPtr - 2) = *((float*)stackPtr - 2) * *((float*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _F32.Size;
                                        break;
                                    }
                                case TypeCode.Double:
                                    {
                                        // Perform add op
                                        *((double*)stackPtr - 2) = *((double*)stackPtr - 2) * *((double*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _F64.Size;
                                        break;
                                    }
                            }
                            break;
                        }
                    case OpCode.Div:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            // Select type
                            switch (opType)
                            {
                                case TypeCode.I32:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = *((int*)stackPtr - 2) / *((int*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I32.Size;
                                        break;
                                    }
                                case TypeCode.I64:
                                    {
                                        // Perform add op
                                        *((long*)stackPtr - 2) = *((long*)stackPtr - 2) / *((long*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size;
                                        break;
                                    }
                                case TypeCode.U64:
                                    {
                                        // Perform add op
                                        *((ulong*)stackPtr - 2) = *((ulong*)stackPtr - 2) / *((ulong*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size;
                                        break;
                                    }
                                case TypeCode.Float:
                                    {
                                        // Perform add op
                                        *((float*)stackPtr - 2) = *((float*)stackPtr - 2) / *((float*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _F32.Size;
                                        break;
                                    }
                                case TypeCode.Double:
                                    {
                                        // Perform add op
                                        *((double*)stackPtr - 2) = *((double*)stackPtr - 2) / *((double*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _F64.Size;
                                        break;
                                    }
                            }
                            break;
                        }
                    case OpCode.Neg:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            // Select type
                            switch (opType)
                            {
                                case TypeCode.I32:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 1) = -*((int*)stackPtr - 1);
                                        break;
                                    }
                                case TypeCode.I64:
                                    {
                                        // Perform add op
                                        *((long*)stackPtr - 1) = -*((long*)stackPtr - 1);
                                        break;
                                    }
                                case TypeCode.Float:
                                    {
                                        // Perform add op
                                        *((float*)stackPtr - 1) = -*((float*)stackPtr - 1);
                                        break;
                                    }
                                case TypeCode.Double:
                                    {
                                        // Perform add op
                                        *((double*)stackPtr - 1) = -*((double*)stackPtr - 1);
                                        break;
                                    }
                            }
                            break;
                        }
                    case OpCode.Mod:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            // Select type
                            switch (opType)
                            {
                                case TypeCode.I32:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = *((int*)stackPtr - 2) % *((int*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I32.Size;
                                        break;
                                    }
                                case TypeCode.I64:
                                    {
                                        // Perform add op
                                        *((long*)stackPtr - 2) = *((long*)stackPtr - 2) % *((long*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size;
                                        break;
                                    }
                                case TypeCode.U64:
                                    {
                                        // Perform add op
                                        *((ulong*)stackPtr - 2) = *((ulong*)stackPtr - 2) % *((ulong*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size;
                                        break;
                                    }
                                case TypeCode.Float:
                                    {
                                        // Perform add op
                                        *((float*)stackPtr - 2) = *((float*)stackPtr - 2) % *((float*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _F32.Size;
                                        break;
                                    }
                                case TypeCode.Double:
                                    {
                                        // Perform add op
                                        *((double*)stackPtr - 2) = *((double*)stackPtr - 2) % *((double*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _F64.Size;
                                        break;
                                    }
                            }
                            break;
                        }
                    case OpCode.And:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            // Select type
                            switch (opType)
                            {
                                case TypeCode.I32:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = *((int*)stackPtr - 2) & *((int*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I32.Size;
                                        break;
                                    }
                                case TypeCode.I64:
                                    {
                                        // Perform add op
                                        *((long*)stackPtr - 2) = *((long*)stackPtr - 2) & *((long*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size;
                                        break;
                                    }
                                case TypeCode.U64:
                                    {
                                        // Perform add op
                                        *((ulong*)stackPtr - 2) = *((ulong*)stackPtr - 2) & *((ulong*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size;
                                        break;
                                    }
                            }
                            break;
                        }
                    case OpCode.Or:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            // Select type
                            switch (opType)
                            {
                                case TypeCode.I32:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = *((int*)stackPtr - 2) | *((int*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I32.Size;
                                        break;
                                    }
                                case TypeCode.I64:
                                    {
                                        // Perform add op
                                        *((long*)stackPtr - 2) = *((long*)stackPtr - 2) | *((long*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size;
                                        break;
                                    }
                                case TypeCode.U64:
                                    {
                                        // Perform add op
                                        *((ulong*)stackPtr - 2) = *((ulong*)stackPtr - 2) | *((ulong*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size;
                                        break;
                                    }
                            }
                            break;
                        }
                    case OpCode.XOr:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            // Select type
                            switch (opType)
                            {
                                case TypeCode.I32:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = *((int*)stackPtr - 2) ^ *((int*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I32.Size;
                                        break;
                                    }
                                case TypeCode.I64:
                                    {
                                        // Perform add op
                                        *((long*)stackPtr - 2) = *((long*)stackPtr - 2) ^ *((long*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size;
                                        break;
                                    }
                                case TypeCode.U64:
                                    {
                                        // Perform add op
                                        *((ulong*)stackPtr - 2) = *((ulong*)stackPtr - 2) ^ *((ulong*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size;
                                        break;
                                    }
                            }
                            break;
                        }
                    case OpCode.Not:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            // Select type
                            switch (opType)
                            {
                                case TypeCode.I32:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 1) = ~*((int*)stackPtr - 1);
                                        break;
                                    }
                                case TypeCode.I64:
                                    {
                                        // Perform add op
                                        *((long*)stackPtr - 1) = ~*((long*)stackPtr - 1);
                                        break;
                                    }
                                case TypeCode.U64:
                                    {
                                        // Perform add op
                                        *((ulong*)stackPtr - 1) = ~*((ulong*)stackPtr - 1);
                                        break;
                                    }
                            }
                            break;
                        }
                    case OpCode.Bit_Shl:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            // Select type
                            switch (opType)
                            {
                                case TypeCode.I32:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = *((int*)stackPtr - 2) << *((int*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I32.Size;
                                        break;
                                    }
                                case TypeCode.I64:
                                    {
                                        // Perform add op - 12 bytes = size(long + int)
                                        *((long*)(stackPtr - 12)) = *((long*)(stackPtr - 12)) << *((int*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I32.Size;
                                        break;
                                    }
                                case TypeCode.U64:
                                    {
                                        // Perform add op - 12 bytes = size(long + int)
                                        *((ulong*)(stackPtr - 12)) = *((ulong*)(stackPtr - 12)) << *((int*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I32.Size;
                                        break;
                                    }
                            }
                            break;
                        }
                    case OpCode.Bit_Shr:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            // Select type
                            switch (opType)
                            {
                                case TypeCode.I32:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = *((int*)stackPtr - 2) >> *((int*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I32.Size;
                                        break;
                                    }
                                case TypeCode.I64:
                                    {
                                        // Perform add op - 12 bytes = size(long + int)
                                        *((long*)(stackPtr - 12)) = *((long*)(stackPtr - 12)) >> *((int*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I32.Size;
                                        break;
                                    }
                                case TypeCode.U64:
                                    {
                                        // Perform add op - 12 bytes = size(long + int)
                                        *((ulong*)(stackPtr - 12)) = *((ulong*)(stackPtr - 12)) >> *((int*)stackPtr - 1);

                                        // Decrement stack ptr
                                        stackPtr -= _I32.Size;
                                        break;
                                    }
                            }
                            break;
                        }
                    #endregion

                    #region Compare
                    case OpCode.Cmp_Eq:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            // Select type
                            switch (opType)
                            {
                                case TypeCode.I32:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = (*((int*)stackPtr - 2) == *((int*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _I32.Size;
                                        break;
                                    }
                                case TypeCode.I64:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 4) = (*((long*)stackPtr - 2) == *((long*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size + _I32.Size;
                                        break;
                                    }
                                case TypeCode.U64:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 4) = (*((ulong*)stackPtr - 2) == *((ulong*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size + _I32.Size;
                                        break;
                                    }
                                case TypeCode.Float:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = (*((float*)stackPtr - 2) == *((float*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _F32.Size;
                                        break;
                                    }
                                case TypeCode.Double:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 4) = (*((double*)stackPtr - 2) == *((double*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _F64.Size + _F32.Size;
                                        break;
                                    }
                            }
                            break;
                        }
                    case OpCode.Cmp_NEq:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            // Select type
                            switch (opType)
                            {
                                case TypeCode.I32:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = (*((int*)stackPtr - 2) != *((int*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _I32.Size;
                                        break;
                                    }
                                case TypeCode.I64:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 4) = (*((long*)stackPtr - 2) != *((long*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size + _I32.Size;
                                        break;
                                    }
                                case TypeCode.U64:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 4) = (*((ulong*)stackPtr - 2) != *((ulong*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size + _I32.Size;
                                        break;
                                    }
                                case TypeCode.Float:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = (*((float*)stackPtr - 2) != *((float*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _F32.Size;
                                        break;
                                    }
                                case TypeCode.Double:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 4) = (*((double*)stackPtr - 2) != *((double*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _F64.Size + _F32.Size;
                                        break;
                                    }
                            }
                            break;
                        }
                    case OpCode.Cmp_L:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            // Select type
                            switch (opType)
                            {
                                case TypeCode.I32:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = (*((int*)stackPtr - 2) < *((int*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _I32.Size;
                                        break;
                                    }
                                case TypeCode.I64:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 4) = (*((long*)stackPtr - 2) < *((long*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size + _I32.Size;
                                        break;
                                    }
                                case TypeCode.U64:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 4) = (*((ulong*)stackPtr - 2) < *((ulong*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size + _I32.Size;
                                        break;
                                    }
                                case TypeCode.Float:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = (*((float*)stackPtr - 2) < *((float*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _F32.Size;
                                        break;
                                    }
                                case TypeCode.Double:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 4) = (*((double*)stackPtr - 2) < *((double*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _F64.Size + _F32.Size;
                                        break;
                                    }
                            }
                            break;
                        }
                    case OpCode.Cmp_Le:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            // Select type
                            switch (opType)
                            {
                                case TypeCode.I32:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = (*((int*)stackPtr - 2) <= *((int*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _I32.Size;
                                        break;
                                    }
                                case TypeCode.I64:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 4) = (*((long*)stackPtr - 2) <= *((long*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size + _I32.Size;
                                        break;
                                    }
                                case TypeCode.U64:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 4) = (*((ulong*)stackPtr - 2) <= *((ulong*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size + _I32.Size;
                                        break;
                                    }
                                case TypeCode.Float:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = (*((float*)stackPtr - 2) <= *((float*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _F32.Size;
                                        break;
                                    }
                                case TypeCode.Double:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 4) = (*((double*)stackPtr - 2) <= *((double*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _F64.Size + _F32.Size;
                                        break;
                                    }
                            }
                            break;
                        }
                    case OpCode.Cmp_G:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            // Select type
                            switch (opType)
                            {
                                case TypeCode.I32:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = (*((int*)stackPtr - 2) > *((int*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _I32.Size;
                                        break;
                                    }
                                case TypeCode.I64:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 4) = (*((long*)stackPtr - 2) > *((long*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size + _I32.Size;
                                        break;
                                    }
                                case TypeCode.U64:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 4) = (*((ulong*)stackPtr - 2) > *((ulong*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size + _I32.Size;
                                        break;
                                    }
                                case TypeCode.Float:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = (*((float*)stackPtr - 2) > *((float*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _F32.Size;
                                        break;
                                    }
                                case TypeCode.Double:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 4) = (*((double*)stackPtr - 2) > *((double*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _F64.Size + _F32.Size;
                                        break;
                                    }
                            }
                            break;
                        }
                    case OpCode.Cmp_Ge:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            // Select type
                            switch (opType)
                            {
                                case TypeCode.I32:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = (*((int*)stackPtr - 2) >= *((int*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _I32.Size;
                                        break;
                                    }
                                case TypeCode.I64:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 4) = (*((long*)stackPtr - 2) >= *((long*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size + _I32.Size;
                                        break;
                                    }
                                case TypeCode.U64:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 4) = (*((ulong*)stackPtr - 2) >= *((ulong*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _I64.Size + _I32.Size;
                                        break;
                                    }
                                case TypeCode.Float:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 2) = (*((float*)stackPtr - 2) >= *((float*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _F32.Size;
                                        break;
                                    }
                                case TypeCode.Double:
                                    {
                                        // Perform add op
                                        *((int*)stackPtr - 4) = (*((double*)stackPtr - 2) >= *((double*)stackPtr - 1)) ? 1 : 0;

                                        // Decrement stack ptr
                                        stackPtr -= _F64.Size + _F32.Size;
                                        break;
                                    }
                            }
                            break;
                        }
                    #endregion

                    #region Cast
                    case OpCode.Cast_I1:
                    case OpCode.Cast_I2:
                    case OpCode.Cast_I4:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            switch (opType)
                            {
                                case TypeCode.I64:
                                    {
                                        *((long*)(stackPtr - 4)) = *((int*)stackPtr - 1);

                                        // Increment stack ptr by half
                                        stackPtr += _I32.Size;
                                        break;
                                    }
                                case TypeCode.U64:
                                    {
                                        *((ulong*)(stackPtr - 4)) = (ulong)*((int*)stackPtr - 1);

                                        // Increment stack ptr by half
                                        stackPtr += _I32.Size;
                                        break;
                                    }
                                case TypeCode.Float:
                                    {
                                        *((float*)stackPtr - 1) = *((int*)stackPtr - 1);
                                        break;
                                    }
                                case TypeCode.Double:
                                    {
                                        *((double*)(stackPtr - 4)) = *((int*)stackPtr - 1);

                                        // Increment stack ptr by half
                                        stackPtr += _F32.Size;
                                        break;
                                    }
                            }
                            break;
                        }


                    case OpCode.Cast_I8:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            switch(opType)
                            {
                                case TypeCode.U64:
                                    {
                                        *((ulong*)stackPtr - 1) = (ulong)*((long*)stackPtr - 1);
                                        break;
                                    }
                            }    
                            break;
                        }
                    case OpCode.Cast_F4:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            switch (opType)
                            {
                                case TypeCode.I8:
                                case TypeCode.U8:
                                case TypeCode.I16:
                                case TypeCode.U16:
                                case TypeCode.I32:
                                case TypeCode.U32:
                                    {
                                        *((int*)stackPtr - 1) = (int)*((float*)stackPtr - 1);
                                        break;
                                    }
                                case TypeCode.I64:
                                    {
                                        *((long*)(stackPtr - 4)) = (long)*((float*)stackPtr - 1);

                                        // Increment stack ptr by half
                                        stackPtr += _I32.Size;
                                        break;
                                    }
                                case TypeCode.U64:
                                    {
                                        *((ulong*)(stackPtr - 4)) = (ulong)*((float*)stackPtr - 1);

                                        // Increment stack ptr by half
                                        stackPtr += _I32.Size;
                                        break;
                                    }
                                case TypeCode.Double:
                                    {
                                        *((double*)(stackPtr - 4)) = *((float*)stackPtr - 1);

                                        // Increment stack ptr by half
                                        stackPtr += _F32.Size;
                                        break;
                                    }
                            }
                            break;
                        }
                    case OpCode.Cast_F8:
                        {
                            // Get type code
                            TypeCode opType = *(TypeCode*)instructionPtr++;

                            switch (opType)
                            {
                                case TypeCode.I8:
                                case TypeCode.U8:
                                case TypeCode.I16:
                                case TypeCode.U16:
                                case TypeCode.I32:
                                case TypeCode.U32:
                                    {
                                        *((int*)stackPtr - 2) = (int)*((double*)stackPtr - 1);

                                        // Decrement stack ptr by half
                                        stackPtr -= _I32.Size;
                                        break;
                                    }
                                case TypeCode.I64:
                                    {
                                        *((long*)stackPtr - 1) = (long)*((double*)stackPtr - 1);
                                        break;
                                    }
                                case TypeCode.U64:
                                    {
                                        *((ulong*)stackPtr - 1) = (ulong)*((double*)stackPtr - 1);
                                        break;
                                    }
                                case TypeCode.Float:
                                    {
                                        *((float*)stackPtr - 2) = (int)*((double*)stackPtr - 1);

                                        // Decrement stack ptr by half
                                        stackPtr -= _F32.Size;
                                        break;
                                    }
                                case TypeCode.Double:
                                    {
                                        *((double*)stackPtr - 1) = *((double*)stackPtr - 1);
                                        break;
                                    }
                            }
                            break;
                        }
                    #endregion

                    #region Object
                    case OpCode.Ret:
                        {
                            halt = true;
                            break;
                        }
                    #endregion
                }
            }
            return stackPtr;
        }
    }
}
