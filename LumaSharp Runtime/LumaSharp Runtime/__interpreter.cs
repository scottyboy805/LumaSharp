
using LumaSharp.Runtime.Handle;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LumaSharp Compiler")]
[assembly: InternalsVisibleTo("LumaSharp RuntimeTests")]
[assembly: InternalsVisibleTo("LumaSharp CompilerTests")]

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

        internal static IntPtr ExecuteBytecode(_MethodHandle method, byte[] instructions)
        {
            // Get instruction mem
            fixed (byte* instructionPtr = instructions)
            {
                // Get stack ptr
                byte* stackBasePtr = (byte*)__memory.stackBasePtr + method.stackPtrOffset;

                // Setup instructions
                method.instructionPtr = instructionPtr;

                // Run bytecode
                return (IntPtr)ExecuteBytecode(method, stackBasePtr);
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

            // Get stack ptr where dynamic stack allocations can be made - after this method frame
            byte* stackAllocPtr = stackPtr + method.maxStack;

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

                            int _32Val = *((int*)(stackBasePtr + locHandle.offset));

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

                            int _32Val = *((int*)(stackBasePtr + locHandle.offset));

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

                            int _32Val = *((int*)(stackBasePtr + locHandle.offset));

                            // Decrement stack ptr
                            stackPtr -= locHandle.typeHandle.size;
                            break;
                        }
                    case OpCode.St_Loc:
                        {
                            // Get local
                            _StackHandle locHandle = method.argLocals[method.localHandleOffset + *((byte*)instructionPtr++)];

                            // Move from stack
                            __memory.Copy(stackPtr - locHandle.typeHandle.size, stackBasePtr + locHandle.offset, locHandle.typeHandle.size);

                            // Decrement stack ptr
                            stackPtr -= locHandle.typeHandle.size;
                            break;
                        }
                    case OpCode.St_Loc_E:
                        {
                            // Get local
                            _StackHandle locHandle = method.argLocals[method.localHandleOffset + *((ushort*)instructionPtr)];
                            instructionPtr += 2;

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

                            int _32Val = *((int*)(stackBasePtr + locHandle.offset));

                            // Move from stack
                            __memory.Copy(stackBasePtr + locHandle.offset, stackPtr, locHandle.typeHandle.size);

                            int stackTop = *((int*)stackPtr - 1);

                            // Increment stack ptr
                            stackPtr += locHandle.typeHandle.size;
                            break;
                        }
                    case OpCode.Ld_Loc_1:
                        {
                            // Get local
                            _StackHandle locHandle = method.argLocals[method.localHandleOffset + 1];

                            int _32Val = *((int*)(stackBasePtr + locHandle.offset));

                            // Move from stack
                            __memory.Copy(stackBasePtr + locHandle.offset, stackPtr, locHandle.typeHandle.size);

                            int stackTop = *((int*)stackPtr - 1);

                            // Increment stack ptr
                            stackPtr += locHandle.typeHandle.size;
                            break;
                        }
                    case OpCode.Ld_Loc_2:
                        {
                            // Get local
                            _StackHandle locHandle = method.argLocals[method.localHandleOffset + 2];

                            int _32Val = *((int*)(stackBasePtr + locHandle.offset));

                            // Move from stack
                            __memory.Copy(stackBasePtr + locHandle.offset, stackPtr, locHandle.typeHandle.size);

                            // Increment stack ptr
                            stackPtr += locHandle.typeHandle.size;
                            break;
                        }
                    case OpCode.Ld_Loc:
                        {
                            // Get local
                            _StackHandle locHandle = method.argLocals[method.localHandleOffset + *((byte*)instructionPtr++)];

                            int _32Val = *((int*)(stackBasePtr + locHandle.offset));

                            // Move from stack
                            __memory.Copy(stackBasePtr + locHandle.offset, stackPtr, locHandle.typeHandle.size);

                            // Increment stack ptr
                            stackPtr += locHandle.typeHandle.size;
                            break;
                        }
                    case OpCode.Ld_Loc_E:
                        {
                            // Get local
                            _StackHandle locHandle = method.argLocals[method.localHandleOffset + *((ushort*)instructionPtr)];
                            instructionPtr += 2;

                            int _32Val = *((int*)(stackBasePtr + locHandle.offset));

                            // Move from stack
                            __memory.Copy(stackBasePtr + locHandle.offset, stackPtr, locHandle.typeHandle.size);

                            // Increment stack ptr
                            stackPtr += locHandle.typeHandle.size;
                            break;
                        }
                    case OpCode.Ld_Loc_A:
                        {
                            // Get local
                            _StackHandle locHandle = method.argLocals[method.localHandleOffset + *((byte*)instructionPtr++)];

                            // Push address
                            *((IntPtr*)stackPtr) = (IntPtr)(stackBasePtr + locHandle.offset);

                            // Increment points
                            stackPtr += sizeof(IntPtr);
                            break;
                        }
                    case OpCode.Ld_Loc_EA:
                        {
                            // Get local
                            _StackHandle locHandle = method.argLocals[method.localHandleOffset + *((ushort*)instructionPtr)];
                            instructionPtr++;

                            // Push address
                            *((IntPtr*)stackPtr) = (IntPtr)(stackBasePtr + locHandle.offset);

                            // Increment points
                            stackPtr += sizeof(IntPtr);
                            break;
                        }
                    #endregion

                    #region Argument
                    case OpCode.St_Arg_0:
                        {
                            // Get local
                            _StackHandle argHandle = method.argLocals[0];

                            // Move from stack
                            __memory.Copy(stackPtr - argHandle.typeHandle.size, stackBasePtr + argHandle.offset, argHandle.typeHandle.size);

                            // Decrement stack ptr
                            stackPtr -= argHandle.typeHandle.size;
                            break;
                        }
                    case OpCode.St_Arg_1:
                        {
                            // Get local
                            _StackHandle argHandle = method.argLocals[1];

                            // Move from stack
                            __memory.Copy(stackPtr - argHandle.typeHandle.size, stackBasePtr + argHandle.offset, argHandle.typeHandle.size);

                            // Decrement stack ptr
                            stackPtr -= argHandle.typeHandle.size;
                            break;
                        }
                    case OpCode.St_Arg_2:
                        {
                            // Get local
                            _StackHandle argHandle = method.argLocals[2];

                            // Move from stack
                            __memory.Copy(stackPtr - argHandle.typeHandle.size, stackBasePtr + argHandle.offset, argHandle.typeHandle.size);

                            // Decrement stack ptr
                            stackPtr -= argHandle.typeHandle.size;
                            break;
                        }
                    case OpCode.Ld_Arg_0:
                        {
                            // Get local
                            _StackHandle argHandle = method.argLocals[0];

                            // Move from stack
                            __memory.Copy(stackBasePtr + argHandle.offset, stackPtr - argHandle.typeHandle.size, argHandle.typeHandle.size);

                            // Increment stack ptr
                            stackPtr += argHandle.typeHandle.size;
                            break;
                        }
                    case OpCode.Ld_Arg_1:
                        {
                            // Get local
                            _StackHandle argHandle = method.argLocals[1];

                            // Move from stack
                            __memory.Copy(stackBasePtr + argHandle.offset, stackPtr - argHandle.typeHandle.size, argHandle.typeHandle.size);

                            // Increment stack ptr
                            stackPtr += argHandle.typeHandle.size;
                            break;
                        }
                    case OpCode.Ld_Arg_2:
                        {
                            // Get local
                            _StackHandle argHandle = method.argLocals[2];

                            // Move from stack
                            __memory.Copy(stackBasePtr + argHandle.offset, stackPtr - argHandle.typeHandle.size, argHandle.typeHandle.size);

                            // Increment stack ptr
                            stackPtr += argHandle.typeHandle.size;
                            break;
                        }
                    #endregion

                    #region Indirect
                    case OpCode.Ld_Addr_I1:
                        {
                            // Dereference address and push as I32
                            *((int*)(stackPtr - sizeof(IntPtr))) = *((byte*)*((IntPtr*)stackPtr - 1));

                            // Decrement stack ptr
                            stackPtr -= sizeof(IntPtr) - sizeof(int);
                            break;
                        }
                    case OpCode.Ld_Addr_I2:
                        {
                            // Dereference address and push as I32
                            *((int*)(stackPtr - sizeof(IntPtr))) = *((short*)*((IntPtr*)stackPtr - 1));

                            // Decrement stack ptr
                            stackPtr -= sizeof(IntPtr) - sizeof(int);
                            break;
                        }
                    case OpCode.Ld_Addr_I4:
                        {
                            // Dereference address and push as I32
                            *((int*)(stackPtr - sizeof(IntPtr))) = *((int*)*((IntPtr*)stackPtr - 1));

                            // Decrement stack ptr
                            stackPtr -= sizeof(IntPtr) - sizeof(int);
                            break;
                        }
                    case OpCode.Ld_Addr_I8:
                        {
                            // Dereference address and push as I32
                            *((long*)(stackPtr - sizeof(IntPtr))) = *((long*)*((IntPtr*)stackPtr - 1));

                            // Increment stack ptr - in case of 32 bit pointer
                            stackPtr += sizeof(IntPtr) - sizeof(long);
                            break;
                        }
                    case OpCode.Ld_Addr_F4:
                        {
                            // Dereference address and push as I32
                            *((float*)(stackPtr - sizeof(IntPtr))) = *((float*)*((IntPtr*)stackPtr - 1));

                            // Decrement stack ptr
                            stackPtr -= sizeof(IntPtr) - sizeof(float);
                            break;
                        }
                    case OpCode.Ld_Addr_F8:
                        {
                            // Dereference address and push as I32
                            *((double*)(stackPtr - sizeof(IntPtr))) = *((double*)*((IntPtr*)stackPtr - 1));

                            // Increment stack ptr - in case of 32 bit pointer
                            stackPtr += sizeof(IntPtr) - sizeof(double);
                            break;
                        }
                    case OpCode.St_Addr_I1:
                        {
                            // Store as byte
                            *((byte*)*((IntPtr*)stackPtr - 1)) = (byte)*((int*)(stackPtr - sizeof(IntPtr)) - 1);

                            // Pop address and value
                            stackPtr -= sizeof(IntPtr) + sizeof(int);
                            break;
                        }
                    case OpCode.St_Addr_I2:
                        {
                            // Store as byte
                            *((short*)*((IntPtr*)stackPtr - 1)) = (short)*((int*)(stackPtr - sizeof(IntPtr)) - 1);

                            // Pop address and value
                            stackPtr -= sizeof(IntPtr) + sizeof(int);
                            break;
                        }
                    case OpCode.St_Addr_I4:
                        {
                            // Store as byte
                            *((int*)*((IntPtr*)stackPtr - 1)) = (int)*((int*)(stackPtr - sizeof(IntPtr)) - 1);

                            // Pop address and value
                            stackPtr -= sizeof(IntPtr) + sizeof(int);
                            break;
                        }
                    case OpCode.St_Addr_I8:
                        {
                            // Store as byte
                            *((long*)*((IntPtr*)stackPtr - 1)) = (int)*((long*)(stackPtr - sizeof(IntPtr)) - 1);

                            // Pop address and value
                            stackPtr -= sizeof(IntPtr) + sizeof(long);
                            break;
                        }
                    case OpCode.St_Addr_F4:
                        {
                            // Store as byte
                            *((float*)*((IntPtr*)stackPtr - 1)) = *((float*)(stackPtr - sizeof(IntPtr)) - 1);

                            // Pop address and value
                            stackPtr -= sizeof(IntPtr) + sizeof(float);
                            break;
                        }
                    case OpCode.St_Addr_F8:
                        {
                            // Store as byte
                            *((double*)*((IntPtr*)stackPtr - 1)) = *((double*)(stackPtr - sizeof(IntPtr)) - 1);

                            // Pop address and value
                            stackPtr -= sizeof(IntPtr) + sizeof(double);
                            break;
                        }
                    #endregion

                    #region Field
                    #endregion

                    #region Array
                    case OpCode.St_Elem:
                        {
                            // Get element size
                            uint elemSize = *((uint*)(stackPtr - sizeof(IntPtr) + sizeof(int) + sizeof(uint)));

                            // Get array ptr
                            byte* arr = (byte*)*((IntPtr*)stackPtr - 1);

                            // Get array ptr with element offset
                            arr += (elemSize * *((uint*)(stackPtr - sizeof(IntPtr) + sizeof(int))));

                            // Assign at index
                            __memory.Copy(stackPtr - elemSize, arr, elemSize);

                            // Pop stack
                            stackPtr -= elemSize + sizeof(IntPtr) + sizeof(int);
                            break;
                        }
                    case OpCode.Ld_Elem:
                        {
                            break;
                        }
                    #endregion

                    #region Jump
                    case OpCode.Jmp:
                        {
                            // Jump to new instruction
                            instructionPtr += *((int*)instructionPtr);
                            break;
                        }
                    case OpCode.Jmp_0:
                        {
                            // Check zero
                            if (*((int*)stackPtr - 1) == 0)
                            {
                                // Jump to new instruction
                                instructionPtr += *((int*)instructionPtr);
                            }
                            else
                            {
                                // Advance instruction ptr
                                instructionPtr += sizeof(int);
                            }
                            // Pop from stack
                            stackPtr -= sizeof(int);
                            break;
                        }
                    case OpCode.Jmp_1:
                        {
                            // Check zero
                            if (*((int*)stackPtr - 1) == 1)
                            {
                                // Jump to new instruction
                                instructionPtr += *((int*)instructionPtr);
                            }
                            else
                            {
                                // Advance instruction ptr
                                instructionPtr += sizeof(int);
                            }
                            // Pop from stack
                            stackPtr -= sizeof(int);
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
                    case OpCode.NewArr:
                        {
                            // Get size
                            bool stackAlloc = true; // is type token have by ref qualifier - allocate on heap

                            // Get type handle
                            _TypeHandle type = new _TypeHandle
                            {
                                typeToken = *((int*)instructionPtr),
                                size = 4,
                            };

                            // Allocate memory - pop size from stack
                            void* arr = __memory.StackAlloc(ref stackAllocPtr, type, *((uint*)stackPtr - 1));

                            // Push array ptr to stack
                            *((IntPtr*)(stackPtr - sizeof(uint))) = (IntPtr)arr;

                            // Increment instruction ptr
                            instructionPtr += sizeof(int);

                            // Increment stack ptr - in case of 64 bit ptr
                            stackPtr += sizeof(IntPtr) - sizeof(uint);
                            break;
                        }
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
