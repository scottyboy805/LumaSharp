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
        internal static StackData* ExecuteBytecode(ThreadContext context, _MethodHandle* method)
        {
            // Get sp max
            byte* spMax = context.ThreadStackPtr + context.ThreadStackSize;

            // Get pointers
            byte* pc = (byte*)method + sizeof(_MethodHandle);
            StackData* spVar = (StackData*)context.ThreadStackPtr;
            StackData* sp = spVar + (method->Signature.ParameterCount + method->Body.VariableCount);

            // Check overflow
            if (sp + method->Body.MaxStack >= spMax)
                context.Throw<StackOverflowException>();

            // Set instruction ptr
            context.DebugInstructionPtr(pc);

            // Push call stack
            context.PushCall(method, pc, spVar, sp);

            bool halt = false;

            // Loop until halt command
            while(halt == false)
            {
                // Get code
                OpCode code = *(OpCode*)pc++;

                // Evaluate code
                switch(code)
                {
                    default: throw new NotImplementedException("Instruction is not implemented: " + code);

                    // Nop
                    case OpCode.Nop:
                        {
                            context.DebugInstruction(code, pc - 1);
                            break;
                        }

                    // Constants
                    #region Constant
                    case OpCode.Ld_I1:
                        {
                            // Push I32 to stack
                            sp->Type = StackTypeCode.I32;
                            sp->I32 = *(sbyte*)pc++;
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 2, sp - 1);
                            break;
                        }
                    case OpCode.Ld_I2:
                        {
                            sp->Type = StackTypeCode.I32;
                            sp->I32 = *(short*)pc;
                            pc += sizeof(short);
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 3, sp - 1);
                            break;
                        }
                    case OpCode.Ld_I4:
                        {
                            sp->Type = StackTypeCode.I32;
                            sp->I32 = *(int*)pc;
                            pc += sizeof(int);
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 5, sp - 1);
                            break;
                        }
                    case OpCode.Ld_I8:
                        {
                            sp->Type = StackTypeCode.I64;
                            sp->I64 = *(long*)pc;
                            pc += sizeof(long);
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 9, sp - 1);
                            break;
                        }
                    case OpCode.Ld_F4:
                        {
                            sp->Type = StackTypeCode.F32;
                            sp->F32 = *(float*)pc;
                            pc += sizeof(float);
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 5, sp - 1);
                            break;
                        }
                    case OpCode.Ld_F8:
                        {
                            sp->Type = StackTypeCode.F64;
                            sp->F64 = *(double*)pc;
                            pc += sizeof(double);
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 9, sp - 1);
                            break;
                        }
                    case OpCode.Ld_Str:
                        {
                            // Get the string token
                            int token = *(int*)pc;
                            pc += sizeof(int);

                            // Lookup the string
                            sp->Type = StackTypeCode.Address;
                            sp->Ptr = default; // String address from token
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Ld_Null:
                        {
                            // Push null address to stack
                            sp->Type = StackTypeCode.Address;
                            sp->Ptr = IntPtr.Zero;
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Ld_I4_0:
                        {
                            sp->Type = StackTypeCode.I32;
                            sp->I32 = 0;
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Ld_I4_1:
                        {
                            sp->Type = StackTypeCode.I32;
                            sp->I32 = 1;
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Ld_I4_M1:
                        {
                            sp->Type = StackTypeCode.I32;
                            sp->I32 = -1;
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Ld_F4_0:
                        {
                            sp->Type = StackTypeCode.F32;
                            sp->F32 = 0f;
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    #endregion
                        
                    // Variables
                    #region Variable
                    case OpCode.Ld_Var_0:
                        {
                            // Copy from variable to stack
                            *sp = *spVar;
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Ld_Var_1:
                        {
                            // Copy from variable to stack
                            *sp = *(spVar + 1);
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Ld_Var_2:
                        {
                            // Copy variable to stack
                            *sp = *(spVar + 2);
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Ld_Var_3:
                        {
                            // Copy variable to stack
                            *sp = *(spVar + 3);
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Ld_Var:
                        {
                            // Get offset I1
                            byte offset = *(byte*)pc++;

                            // Copy variable to stack
                            *sp = *(spVar + offset);
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 2, sp - 1);
                            break;
                        }
                    case OpCode.Ld_Var_E:
                        {
                            // Get offset I2
                            ushort offset = *(ushort*)pc;
                            pc += sizeof(ushort);

                            // Copy variable to stack
                            *sp = *(spVar + offset);
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 3, sp - 1);
                            break;
                        }
                    case OpCode.Ld_Var_A:
                        {
                            // Get offset I1
                            byte offset = *(byte*)pc++;

                            // Push address of variable
                            sp->Type = StackTypeCode.Address;
                            sp->TypeCode = method->Body.Variables[offset].TypeHandle.TypeCode;
                            sp->Ptr = (IntPtr)(spVar + offset);
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 2, sp - 1);
                            break;
                        }
                    case OpCode.Ld_Var_EA:
                        {
                            // Get offset I2
                            ushort offset = *(ushort*)pc;
                            pc += sizeof(ushort);

                            // Push address of variable
                            sp->Type = StackTypeCode.Address;
                            sp->TypeCode = method->Body.Variables[offset].TypeHandle.TypeCode;
                            sp->Ptr = (IntPtr)(spVar + offset);                            
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 3, sp - 1);
                            break;
                        }
                    case OpCode.St_Var_0:
                        {
                            // Pop and copy to variable
                            sp--;
                            *spVar = *sp;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, spVar);
                            break;
                        }
                    case OpCode.St_Var_1:
                        {
                            // Pop and copy to variable
                            sp--;
                            *(spVar + 1) = *sp;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, spVar + 1);
                            break;
                        }
                    case OpCode.St_Var_2:
                        {
                            // Pop and copy to variable
                            sp--;
                            *(spVar + 2) = *sp;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, spVar + 2);
                            break;
                        }
                    case OpCode.St_Var_3:
                        {
                            // Pop and copy to variable
                            sp--;
                            *(spVar + 3) = *sp;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, spVar + 3);
                            break;
                        }
                    case OpCode.St_Var:
                        {
                            // Get offset I1
                            byte offset = *(byte*)pc++;

                            // Pop and copy to variable
                            sp--;
                            *(spVar + offset) = *sp;

                            // Debug execution
                            context.DebugInstruction(code, pc - 2, spVar + offset);
                            break;
                        }
                    case OpCode.St_Var_E:
                        {
                            // Get offset I2
                            ushort offset = *(ushort*)pc;
                            pc += sizeof(ushort);

                            // Pop and copy to variable
                            sp--;
                            *(spVar + offset) = *sp;

                            // Debug execution
                            context.DebugInstruction(code, pc - 3, spVar + offset);
                            break;
                        }
                    #endregion

                    // Fields
                    #region Field
                    case OpCode.Ld_Fld:
                        {
                            // Get field token
                            int token = *(int*)pc;
                            pc += sizeof(int);

                            // Get field handle
                            _FieldHandle* field = (_FieldHandle*)context.AppContext.fieldHandles[token];

                            // Pop instance
                            sp--;

                            // Check for null
                            if (sp->Ptr == IntPtr.Zero)
                                context.Throw<NullReferenceException>();

                            // Get field address
                            byte* fieldMem = field->GetFieldAddress((byte*)sp->Ptr);

                            // Copy to stack
                            StackData.CopyFromMemory(sp, fieldMem, field->TypeHandle.TypeCode);
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 5, sp - 1);
                            break;
                        }
                    case OpCode.Ld_Fld_A:
                        {
                            // Get field token
                            int token = *(int*)pc;
                            pc += sizeof(int);

                            // Get field handle
                            _FieldHandle* field = (_FieldHandle*)context.AppContext.fieldHandles[token];

                            // Pop instance
                            sp--;

                            // Check for null
                            if (sp->Ptr == IntPtr.Zero)
                                context.Throw<NullReferenceException>();

                            // Get field address
                            byte* fieldMem = field->GetFieldAddress((byte*)sp->Ptr);

                            // Push address of field
                            sp->Type = StackTypeCode.Address;
                            sp->TypeCode = field->TypeHandle.TypeCode;
                            sp->Ptr = (IntPtr)fieldMem;
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 5, sp - 1);
                            break;
                        }
                    case OpCode.St_Fld:
                        {
                            // Get field token
                            int token = *(int*)pc;
                            pc += sizeof(int);

                            // Get field handle
                            _FieldHandle* field = (_FieldHandle*)context.AppContext.fieldHandles[token];

                            // Pop value then instance
                            sp -= 2;

                            // Check for null
                            if (sp->Ptr == IntPtr.Zero)
                                context.Throw<NullReferenceException>();

                            // Get field address
                            byte* fieldMem = field->GetFieldAddress((byte*)sp->Ptr);

                            // Copy to memory
                            StackData.CopyToMemory(sp + 1, fieldMem, field->TypeHandle.TypeCode);

                            // Debug execution
                            context.DebugInstruction(code, pc - 5, fieldMem, field->TypeHandle.TypeCode);
                            break;
                        }
                    #endregion

                    // Arrays
                    #region Array
                    case OpCode.Ld_Len:
                        {
                            // Pop instance
                            sp--;

                            // Check for null
                            if (sp->Ptr == IntPtr.Zero)
                                context.Throw<NullReferenceException>();

                            // Load array instance
                            _ArrayHandle arr = *(_ArrayHandle*)(sp->Ptr - _ArrayHandle.Size);

                            // Push length
                            sp->Type = StackTypeCode.I64;
                            sp->I64 = arr.ElementCount;
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Ld_Elem:
                        {
                            // Pop index
                            sp--;
                            long index = sp->Type == StackTypeCode.I64 ? sp->I64 : sp->I32;

                            // Pop instance
                            sp--;

                            // Check for null
                            if(sp->Ptr == IntPtr.Zero)
                                context.Throw<NullReferenceException>();

                            // Load array instance
                            _ArrayHandle arr = *(_ArrayHandle*)(sp->Ptr - _ArrayHandle.Size);

                            // Check bounds
                            if (index < 0 || index >= arr.ElementCount)
                                context.Throw<IndexOutOfRangeException>();

                            // Get element ptr
                            byte* elementMem = arr.GetElementAddress((byte*)sp->Ptr, index);

                            // Copy to stack
                            StackData.CopyFromMemory(sp, elementMem, arr.MemoryHandle.TypeHandle->TypeCode);
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Ld_Elem_A:
                        {
                            // Pop index
                            sp--;
                            long index = sp->Type == StackTypeCode.I64 ? sp->I64 : sp->I32;

                            // Pop instance
                            sp--;

                            // Check for null
                            if (sp->Ptr == IntPtr.Zero)
                                context.Throw<NullReferenceException>();

                            // Load array instance
                            _ArrayHandle arr = *(_ArrayHandle*)(sp->Ptr - _ArrayHandle.Size);

                            // Check bounds
                            if (index < 0 || index >= arr.ElementCount)
                                context.Throw<IndexOutOfRangeException>();

                            // Get element ptr
                            byte* elementMem = arr.GetElementAddress((byte*)sp->Ptr, index);

                            // Push address of element
                            sp->Type = StackTypeCode.Address;
                            sp->TypeCode = arr.MemoryHandle.TypeHandle->TypeCode;
                            sp->Ptr = (IntPtr)elementMem;                            
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.St_Elem:
                        {
                            // Pop value
                            sp--;

                            // Pop index
                            sp--;
                            long index = sp->Type == StackTypeCode.I64 ? sp->I64 : sp->I32;

                            // Pop instance
                            sp--;

                            // Check for null
                            if (sp->Ptr == IntPtr.Zero)
                                context.Throw<NullReferenceException>();

                            // Load array instance
                            _ArrayHandle arr = *(_ArrayHandle*)(sp->Ptr - _ArrayHandle.Size);

                            // Check bounds
                            if (index < 0 || index >= arr.ElementCount)
                                context.Throw<IndexOutOfRangeException>();

                            // Get element ptr
                            byte* elementMem = arr.GetElementAddress((byte*)sp->Ptr, index);

                            // Copy to memory
                            StackData.CopyToMemory(sp + 2, elementMem, arr.MemoryHandle.TypeHandle->TypeCode);

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, elementMem, arr.MemoryHandle.TypeHandle->TypeCode);
                            break;
                        }
                    #endregion

                    // Address
                    #region Address
                    case OpCode.Ld_Addr:
                        {
                            // Pop address
                            sp--;

                            // Copy from address stored on stack
                            StackData.CopyFromMemory(sp, (byte*)sp->Ptr, sp->TypeCode);
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.St_Addr:
                        {
                            // Pop value
                            sp--;

                            // Pop address
                            sp--;

                            // Copy to address stored on stack
                            StackData.CopyToMemory(sp + 1, (byte*)sp->Ptr, sp->TypeCode);

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, (byte*)sp->Ptr, sp->TypeCode);
                            break;
                        }
                    #endregion

                    // Arithmetic
                    #region Arithmetic
                    case OpCode.Add:
                        {
                            // Decrement ptr
                            sp--;

                            // Allow overflow
                            unchecked
                            {
                                switch(sp->Type)
                                {
                                    default: throw new NotSupportedException(sp->Type.ToString());
                                    case StackTypeCode.I32:
                                        {
                                            // Perform add I32 : I32
                                            (sp - 1)->I32 = (sp - 1)->I32 + sp->I32;
                                            break;
                                        }
                                    case StackTypeCode.U32:
                                        {
                                            // Perform add U32 : U32
                                            (sp - 1)->I32 = (int)((uint)(sp - 1)->I32 + (uint)sp->I32);
                                            break;
                                        }
                                    case StackTypeCode.I64:
                                        {
                                            // Perform add I64 : I64
                                            (sp - 1)->I64 = (sp - 1)->I64 + sp->I64;
                                            break;
                                        }
                                    case StackTypeCode.U64:
                                        {
                                            // Perform add U64 : U64
                                            (sp - 1)->I64 = (long)((ulong)(sp - 1)->I64 + (ulong)sp->I64);
                                            break;
                                        }
                                    case StackTypeCode.Address:
                                        {
                                            // Perform add IntPtr : IntPtr
                                            (sp - 1)->Ptr = (nint)(sp - 1)->Ptr + (nint)sp->Ptr;
                                            break;
                                        }
                                    case StackTypeCode.F32:
                                        {
                                            // Perform add F32 : F32
                                            (sp - 1)->F32 = (sp - 1)->F32 + sp->F32;
                                            break;
                                        }
                                    case StackTypeCode.F64:
                                        {
                                            // Perform add F64 : F64
                                            (sp - 1)->F64 = (sp - 1)->F64 + sp->F64;
                                            break;
                                        }
                                }
                            }

                            // Debug instruction
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Sub:
                        {
                            // Decrement ptr
                            sp--;

                            // Allow overflow
                            unchecked
                            {
                                switch (sp->Type)
                                {
                                    default: throw new NotSupportedException(sp->Type.ToString());
                                    case StackTypeCode.I32:
                                        {
                                            // Perform subtract I32 : I32
                                            (sp - 1)->I32 = (sp - 1)->I32 - sp->I32;
                                            break;
                                        }
                                    case StackTypeCode.U32:
                                        {
                                            // Perform subtract U32 : U32
                                            (sp - 1)->I32 = (int)((uint)(sp - 1)->I32 - (uint)sp->I32);
                                            break;
                                        }
                                    case StackTypeCode.I64:
                                        {
                                            // Perform subtract I64 : I64
                                            (sp - 1)->I64 = (sp - 1)->I64 - sp->I64;
                                            break;
                                        }
                                    case StackTypeCode.U64:
                                        {
                                            // Perform subtract U64 : U64
                                            (sp - 1)->I64 = (long)((ulong)(sp - 1)->I64 - (ulong)sp->I64);
                                            break;
                                        }
                                    case StackTypeCode.Address:
                                        {
                                            // Perform subtract IntPtr : IntPtr
                                            (sp - 1)->Ptr = (nint)(sp - 1)->Ptr - (nint)sp->Ptr;
                                            break;
                                        }
                                    case StackTypeCode.F32:
                                        {
                                            // Perform subtract F32 : F32
                                            (sp - 1)->F32 = (sp - 1)->F32 - sp->F32;
                                            break;
                                        }
                                    case StackTypeCode.F64:
                                        {
                                            // Perform subtract F64 : F64
                                            (sp - 1)->F64 = (sp - 1)->F64 - sp->F64;
                                            break;
                                        }
                                }
                            }

                            // Debug instruction
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Mul:
                        {
                            // Decrement ptr
                            sp--;

                            // Allow overflow
                            unchecked
                            {
                                switch (sp->Type)
                                {
                                    default: throw new NotSupportedException(sp->Type.ToString());
                                    case StackTypeCode.I32:
                                        {
                                            // Perform multiply I32 : I32
                                            (sp - 1)->I32 = (sp - 1)->I32 * sp->I32;
                                            break;
                                        }
                                    case StackTypeCode.U32:
                                        {
                                            // Perform multiply U32 : U32
                                            (sp - 1)->I32 = (int)((uint)(sp - 1)->I32 * (uint)sp->I32);
                                            break;
                                        }
                                    case StackTypeCode.I64:
                                        {
                                            // Perform multiply I64 : I64
                                            (sp - 1)->I64 = (sp - 1)->I64 * sp->I64;
                                            break;
                                        }
                                    case StackTypeCode.U64:
                                        {
                                            // Perform multiply U64 : U64
                                            (sp - 1)->I64 = (long)((ulong)(sp - 1)->I64 * (ulong)sp->I64);
                                            break;
                                        }
                                    case StackTypeCode.Address:
                                        {
                                            // Perform multiply IntPtr : IntPtr
                                            (sp - 1)->Ptr = (nint)(sp - 1)->Ptr * (nint)sp->Ptr;
                                            break;
                                        }
                                    case StackTypeCode.F32:
                                        {
                                            // Perform multiply F32 : F32
                                            (sp - 1)->F32 = (sp - 1)->F32 * sp->F32;
                                            break;
                                        }
                                    case StackTypeCode.F64:
                                        {
                                            // Perform multiply F64 : F64
                                            (sp - 1)->F64 = (sp - 1)->F64 * sp->F64;
                                            break;
                                        }
                                }
                            }

                            // Debug instruction
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Div:
                        {
                            // Decrement ptr
                            sp--;

                            // Allow overflow
                            unchecked
                            {
                                switch (sp->Type)
                                {
                                    default: throw new NotSupportedException(sp->Type.ToString());
                                    case StackTypeCode.I32:
                                        {
                                            // Perform divide I32 : I32
                                            (sp - 1)->I32 = (sp - 1)->I32 / sp->I32;
                                            break;
                                        }
                                    case StackTypeCode.U32:
                                        {
                                            // Perform divide U32 : U32
                                            (sp - 1)->I32 = (int)((uint)(sp - 1)->I32 / (uint)sp->I32);
                                            break;
                                        }
                                    case StackTypeCode.I64:
                                        {
                                            // Perform divide I64 : I64
                                            (sp - 1)->I64 = (sp - 1)->I64 + sp->I64;
                                            break;
                                        }
                                    case StackTypeCode.U64:
                                        {
                                            // Perform divide U64 : U64
                                            (sp - 1)->I64 = (long)((ulong)(sp - 1)->I64 / (ulong)sp->I64);
                                            break;
                                        }
                                    case StackTypeCode.Address:
                                        {
                                            // Perform divide IntPtr : IntPtr
                                            (sp - 1)->Ptr = (nint)(sp - 1)->Ptr / (nint)sp->Ptr;
                                            break;
                                        }
                                    case StackTypeCode.F32:
                                        {
                                            // Perform divide F32 : F32
                                            (sp - 1)->F32 = (sp - 1)->F32 / sp->F32;
                                            break;
                                        }
                                    case StackTypeCode.F64:
                                        {
                                            // Perform divide F64 : F64
                                            (sp - 1)->F64 = (sp - 1)->F64 / sp->F64;
                                            break;
                                        }
                                }
                            }

                            // Debug instruction
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Neg:
                        {
                            // Allow overflow
                            unchecked
                            {
                                switch (sp->Type)
                                {
                                    default: throw new NotSupportedException(sp->Type.ToString());
                                    case StackTypeCode.I32:
                                        {
                                            // Perform negate I32 : I32
                                            (sp - 1)->I32 = -(sp - 1)->I32;
                                            break;
                                        }
                                    case StackTypeCode.I64:
                                        {
                                            // Perform negate I64 : I64
                                            (sp - 1)->I64 = -(sp - 1)->I64;
                                            break;
                                        }
                                    case StackTypeCode.Address:
                                        {
                                            // Perform negate IntPtr : IntPtr
                                            (sp - 1)->Ptr = -(nint)(sp - 1)->Ptr;
                                            break;
                                        }
                                    case StackTypeCode.F32:
                                        {
                                            // Perform negate F32 : F32
                                            (sp - 1)->F32 = -(sp - 1)->F32;
                                            break;
                                        }
                                    case StackTypeCode.F64:
                                        {
                                            // Perform negate F64 : F64
                                            (sp - 1)->F64 = -(sp - 1)->F64;
                                            break;
                                        }
                                }
                            }

                            // Debug instruction
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.And:
                        {
                            // Decrement ptr
                            sp--;

                            // Allow overflow
                            unchecked
                            {
                                switch (sp->Type)
                                {
                                    default: throw new NotSupportedException(sp->Type.ToString());
                                    case StackTypeCode.I32:
                                        {
                                            // Perform bitwise and I32 : I32
                                            (sp - 1)->I32 = (sp - 1)->I32 & sp->I32;
                                            break;
                                        }
                                    case StackTypeCode.U32:
                                        {
                                            // Perform bitwise and U32 : U32
                                            (sp - 1)->I32 = (int)((uint)(sp - 1)->I32 & (uint)sp->I32);
                                            break;
                                        }
                                    case StackTypeCode.I64:
                                        {
                                            // Perform bitwise and I64 : I64
                                            (sp - 1)->I64 = (sp - 1)->I64 & sp->I64;
                                            break;
                                        }
                                    case StackTypeCode.U64:
                                        {
                                            // Perform bitwise and U64 : U64
                                            (sp - 1)->I64 = (long)((ulong)(sp - 1)->I64 & (ulong)sp->I64);
                                            break;
                                        }
                                    case StackTypeCode.Address:
                                        {
                                            // Perform bitwise and IntPtr : IntPtr
                                            (sp - 1)->Ptr = (nint)(sp - 1)->Ptr & (nint)sp->Ptr;
                                            break;
                                        }
                                }
                            }

                            // Debug instruction
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Or:
                        {
                            // Decrement ptr
                            sp--;

                            // Allow overflow
                            unchecked
                            {
                                switch (sp->Type)
                                {
                                    default: throw new NotSupportedException(sp->Type.ToString());
                                    case StackTypeCode.I32:
                                        {
                                            // Perform bitwise or I32 : I32
                                            (sp - 1)->I32 = (sp - 1)->I32 | sp->I32;
                                            break;
                                        }
                                    case StackTypeCode.U32:
                                        {
                                            // Perform bitwise or U32 : U32
                                            (sp - 1)->I32 = (int)((uint)(sp - 1)->I32 | (uint)sp->I32);
                                            break;
                                        }
                                    case StackTypeCode.I64:
                                        {
                                            // Perform bitwise or I64 : I64
                                            (sp - 1)->I64 = (sp - 1)->I64 | sp->I64;
                                            break;
                                        }
                                    case StackTypeCode.U64:
                                        {
                                            // Perform bitwise or U64 : U64
                                            (sp - 1)->I64 = (long)((ulong)(sp - 1)->I64 | (ulong)sp->I64);
                                            break;
                                        }
                                    case StackTypeCode.Address:
                                        {
                                            // Perform bitwise or IntPtr : IntPtr
                                            (sp - 1)->Ptr = (nint)(sp - 1)->Ptr | (nint)sp->Ptr;
                                            break;
                                        }
                                }
                            }

                            // Debug instruction
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.XOr:
                        {
                            // Decrement ptr
                            sp--;

                            // Allow overflow
                            unchecked
                            {
                                switch (sp->Type)
                                {
                                    default: throw new NotSupportedException(sp->Type.ToString());
                                    case StackTypeCode.I32:
                                        {
                                            // Perform bitwise xor I32 : I32
                                            (sp - 1)->I32 = (sp - 1)->I32 ^ sp->I32;
                                            break;
                                        }
                                    case StackTypeCode.U32:
                                        {
                                            // Perform bitwise xor U32 : U32
                                            (sp - 1)->I32 = (int)((uint)(sp - 1)->I32 ^ (uint)sp->I32);
                                            break;
                                        }
                                    case StackTypeCode.I64:
                                        {
                                            // Perform bitwise xor I64 : I64
                                            (sp - 1)->I64 = (sp - 1)->I64 ^ sp->I64;
                                            break;
                                        }
                                    case StackTypeCode.U64:
                                        {
                                            // Perform bitwise xor U64 : U64
                                            (sp - 1)->I64 = (long)((ulong)(sp - 1)->I64 ^ (ulong)sp->I64);
                                            break;
                                        }
                                    case StackTypeCode.Address:
                                        {
                                            // Perform bitwise xor IntPtr : IntPtr
                                            (sp - 1)->Ptr = (nint)(sp - 1)->Ptr ^ (nint)sp->Ptr;
                                            break;
                                        }
                                }
                            }

                            // Debug instruction
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Bit_Shl:
                        {
                            // Decrement ptr
                            sp--;

                            // Allow overflow
                            unchecked
                            {
                                switch (sp->Type)
                                {
                                    default: throw new NotSupportedException(sp->Type.ToString());
                                    case StackTypeCode.I32:
                                        {
                                            // Perform bitwise shift left I32 : I32
                                            (sp - 1)->I32 = (sp - 1)->I32 << sp->I32;
                                            break;
                                        }
                                    case StackTypeCode.U32:
                                        {
                                            // Perform bitwise shift left U32 : I32
                                            (sp - 1)->I32 = (int)((uint)(sp - 1)->I32 << sp->I32);
                                            break;
                                        }
                                    case StackTypeCode.I64:
                                        {
                                            // Perform bitwise shift left I64 : I32
                                            (sp - 1)->I64 = (sp - 1)->I64 << sp->I32;
                                            break;
                                        }
                                    case StackTypeCode.U64:
                                        {
                                            // Perform bitwise shift left U64 : I32
                                            (sp - 1)->I64 = (long)((ulong)(sp - 1)->I64 << sp->I32);
                                            break;
                                        }
                                    case StackTypeCode.Address:
                                        {
                                            // Perform bitwise shift left IntPtr : I32
                                            (sp - 1)->Ptr = (nint)(sp - 1)->Ptr << sp->I32;
                                            break;
                                        }
                                }
                            }

                            // Debug instruction
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Bit_Shr:
                        {
                            // Decrement ptr
                            sp--;

                            // Allow overflow
                            unchecked
                            {
                                switch (sp->Type)
                                {
                                    default: throw new NotSupportedException(sp->Type.ToString());
                                    case StackTypeCode.I32:
                                        {
                                            // Perform bitwise shift right I32 : I32
                                            (sp - 1)->I32 = (sp - 1)->I32 >> sp->I32;
                                            break;
                                        }
                                    case StackTypeCode.U32:
                                        {
                                            // Perform bitwise shift right U32 : I32
                                            (sp - 1)->I32 = (int)((uint)(sp - 1)->I32 >> sp->I32);
                                            break;
                                        }
                                    case StackTypeCode.I64:
                                        {
                                            // Perform bitwise shift right I64 : I32
                                            (sp - 1)->I64 = (sp - 1)->I64 >> sp->I32;
                                            break;
                                        }
                                    case StackTypeCode.U64:
                                        {
                                            // Perform bitwise shift right U64 : I32
                                            (sp - 1)->I64 = (long)((ulong)(sp - 1)->I64 >> sp->I32);
                                            break;
                                        }
                                    case StackTypeCode.Address:
                                        {
                                            // Perform bitwise shift right IntPtr : I32
                                            (sp - 1)->Ptr = (nint)(sp - 1)->Ptr >> sp->I32;
                                            break;
                                        }
                                }
                            }

                            // Debug instruction
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Mod:
                        {
                            // Decrement ptr
                            sp--;

                            // Allow overflow
                            unchecked
                            {
                                switch (sp->Type)
                                {
                                    default: throw new NotSupportedException(sp->Type.ToString());
                                    case StackTypeCode.I32:
                                        {
                                            // Perform mod I32 : I32
                                            (sp - 1)->I32 = (sp - 1)->I32 % sp->I32;
                                            break;
                                        }
                                    case StackTypeCode.U32:
                                        {
                                            // Perform mod U32 : U32
                                            (sp - 1)->I32 = (int)((uint)(sp - 1)->I32 % (uint)sp->I32);
                                            break;
                                        }
                                    case StackTypeCode.I64:
                                        {
                                            // Perform mod I64 : I64
                                            (sp - 1)->I64 = (sp - 1)->I64 % sp->I64;
                                            break;
                                        }
                                    case StackTypeCode.U64:
                                        {
                                            // Perform mod U64 : U64
                                            (sp - 1)->I64 = (long)((ulong)(sp - 1)->I64 % (ulong)sp->I64);
                                            break;
                                        }
                                    case StackTypeCode.Address:
                                        {
                                            // Perform mod IntPtr : IntPtr
                                            (sp - 1)->Ptr = (nint)(sp - 1)->Ptr % (nint)sp->Ptr;
                                            break;
                                        }
                                    case StackTypeCode.F32:
                                        {
                                            // Perform mod F32 : F32
                                            (sp - 1)->F32 = (sp - 1)->F32 % sp->F32;
                                            break;
                                        }
                                    case StackTypeCode.F64:
                                        {
                                            // Perform mod F64 : F64
                                            (sp - 1)->F64 = (sp - 1)->F64 % sp->F64;
                                            break;
                                        }
                                }
                            }

                            // Debug instruction
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    #endregion

                    // Compare
                    #region Compare
                    case OpCode.Cmp_L:
                        {
                            // Decrement ptr
                            sp--;

                            switch (sp->Type)
                            {
                                default: throw new NotSupportedException(sp->Type.ToString());
                                case StackTypeCode.I32:
                                    {
                                        // Perform compare I32 : I32
                                        (sp - 1)->I32 = (sp - 1)->I32 < sp->I32 ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.U32:
                                    {
                                        // Perform compare U32 : U32
                                        (sp - 1)->I32 = ((uint)(sp - 1)->I32 < (uint)sp->I32) ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.I64:
                                    {
                                        // Perform compare I64 : I64
                                        (sp - 1)->I32 = (sp - 1)->I64 < sp->I64 ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.U64:
                                    {
                                        // Perform compare U64 : U64
                                        (sp - 1)->I32 = ((ulong)(sp - 1)->I64 < (ulong)sp->I64) ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.Address:
                                    {
                                        // Perform compare IntPtr : IntPtr
                                        (sp - 1)->I32 = (sp - 1)->Ptr < (nint)sp->Ptr ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.F32:
                                    {
                                        // Perform compare F32 : F32
                                        (sp - 1)->I32 = (sp - 1)->F32 < sp->F32 ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.F64:
                                    {
                                        // Perform compare F64 : F64
                                        (sp - 1)->I32 = (sp - 1)->F64 < sp->F64 ? 1 : 0;
                                        break;
                                    }
                            }
                            // Set type
                            (sp - 1)->Type = StackTypeCode.I32;

                            // Debug instruction
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Cmp_Le:
                        {
                            // Decrement ptr
                            sp--;

                            switch (sp->Type)
                            {
                                default: throw new NotSupportedException(sp->Type.ToString());
                                case StackTypeCode.I32:
                                    {
                                        // Perform compare I32 : I32
                                        (sp - 1)->I32 = (sp - 1)->I32 <= sp->I32 ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.U32:
                                    {
                                        // Perform compare U32 : U32
                                        (sp - 1)->I32 = ((uint)(sp - 1)->I32 <= (uint)sp->I32) ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.I64:
                                    {
                                        // Perform compare I64 : I64
                                        (sp - 1)->I32 = (sp - 1)->I64 <= sp->I64 ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.U64:
                                    {
                                        // Perform compare U64 : U64
                                        (sp - 1)->I32 = ((ulong)(sp - 1)->I64 <= (ulong)sp->I64) ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.Address:
                                    {
                                        // Perform compare IntPtr : IntPtr
                                        (sp - 1)->I32 = (sp - 1)->Ptr <= (nint)sp->Ptr ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.F32:
                                    {
                                        // Perform compare F32 : F32
                                        (sp - 1)->I32 = (sp - 1)->F32 <= sp->F32 ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.F64:
                                    {
                                        // Perform compare F64 : F64
                                        (sp - 1)->I32 = (sp - 1)->F64 <= sp->F64 ? 1 : 0;
                                        break;
                                    }
                            }
                            // Set type
                            (sp - 1)->Type = StackTypeCode.I32;

                            // Debug instruction
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Cmp_G:
                        {
                            // Decrement ptr
                            sp--;

                            switch (sp->Type)
                            {
                                default: throw new NotSupportedException(sp->Type.ToString());
                                case StackTypeCode.I32:
                                    {
                                        // Perform compare I32 : I32
                                        (sp - 1)->I32 = (sp - 1)->I32 > sp->I32 ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.U32:
                                    {
                                        // Perform compare U32 : U32
                                        (sp - 1)->I32 = ((uint)(sp - 1)->I32 > (uint)sp->I32) ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.I64:
                                    {
                                        // Perform compare I64 : I64
                                        (sp - 1)->I32 = (sp - 1)->I64 > sp->I64 ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.U64:
                                    {
                                        // Perform compare U64 : U64
                                        (sp - 1)->I32 = ((ulong)(sp - 1)->I64 > (ulong)sp->I64) ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.Address:
                                    {
                                        // Perform compare IntPtr : IntPtr
                                        (sp - 1)->I32 = (sp - 1)->Ptr > (nint)sp->Ptr ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.F32:
                                    {
                                        // Perform compare F32 : F32
                                        (sp - 1)->I32 = (sp - 1)->F32 > sp->F32 ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.F64:
                                    {
                                        // Perform compare F64 : F64
                                        (sp - 1)->I32 = (sp - 1)->F64 > sp->F64 ? 1 : 0;
                                        break;
                                    }
                            }
                            // Set type
                            (sp - 1)->Type = StackTypeCode.I32;

                            // Debug instruction
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Cmp_Ge:
                        {
                            // Decrement ptr
                            sp--;

                            switch (sp->Type)
                            {
                                default: throw new NotSupportedException(sp->Type.ToString());
                                case StackTypeCode.I32:
                                    {
                                        // Perform compare I32 : I32
                                        (sp - 1)->I32 = (sp - 1)->I32 >= sp->I32 ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.U32:
                                    {
                                        // Perform compare U32 : U32
                                        (sp - 1)->I32 = ((uint)(sp - 1)->I32 >= (uint)sp->I32) ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.I64:
                                    {
                                        // Perform compare I64 : I64
                                        (sp - 1)->I32 = (sp - 1)->I64 >= sp->I64 ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.U64:
                                    {
                                        // Perform compare U64 : U64
                                        (sp - 1)->I32 = ((ulong)(sp - 1)->I64 >= (ulong)sp->I64) ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.Address:
                                    {
                                        // Perform compare IntPtr : IntPtr
                                        (sp - 1)->I32 = (sp - 1)->Ptr >= (nint)sp->Ptr ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.F32:
                                    {
                                        // Perform compare F32 : F32
                                        (sp - 1)->I32 = (sp - 1)->F32 >= sp->F32 ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.F64:
                                    {
                                        // Perform compare F64 : F64
                                        (sp - 1)->I32 = (sp - 1)->F64 >= sp->F64 ? 1 : 0;
                                        break;
                                    }
                            }
                            // Set type
                            (sp - 1)->Type = StackTypeCode.I32;

                            // Debug instruction
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Cmp_Eq:
                        {
                            // Decrement ptr
                            sp--;

                            switch (sp->Type)
                            {
                                default: throw new NotSupportedException(sp->Type.ToString());
                                case StackTypeCode.I32:
                                    {
                                        // Perform compare I32 : I32
                                        (sp - 1)->I32 = (sp - 1)->I32 == sp->I32 ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.U32:
                                    {
                                        // Perform compare U32 : U32
                                        (sp - 1)->I32 = ((uint)(sp - 1)->I32 == (uint)sp->I32) ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.I64:
                                    {
                                        // Perform compare I64 : I64
                                        (sp - 1)->I32 = (sp - 1)->I64 == sp->I64 ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.U64:
                                    {
                                        // Perform compare U64 : U64
                                        (sp - 1)->I32 = ((ulong)(sp - 1)->I64 == (ulong)sp->I64) ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.Address:
                                    {
                                        // Perform compare IntPtr : IntPtr
                                        (sp - 1)->I32 = (sp - 1)->Ptr == (nint)sp->Ptr ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.F32:
                                    {
                                        // Perform compare F32 : F32
                                        (sp - 1)->I32 = (sp - 1)->F32 == sp->F32 ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.F64:
                                    {
                                        // Perform compare F64 : F64
                                        (sp - 1)->I32 = (sp - 1)->F64 == sp->F64 ? 1 : 0;
                                        break;
                                    }
                            }
                            // Set type
                            (sp - 1)->Type = StackTypeCode.I32;

                            // Debug instruction
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Cmp_NEq:
                        {
                            // Decrement ptr
                            sp--;

                            switch (sp->Type)
                            {
                                default: throw new NotSupportedException(sp->Type.ToString());
                                case StackTypeCode.I32:
                                    {
                                        // Perform compare I32 : I32
                                        (sp - 1)->I32 = (sp - 1)->I32 != sp->I32 ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.U32:
                                    {
                                        // Perform compare U32 : U32
                                        (sp - 1)->I32 = ((uint)(sp - 1)->I32 != (uint)sp->I32) ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.I64:
                                    {
                                        // Perform compare I64 : I64
                                        (sp - 1)->I32 = (sp - 1)->I64 != sp->I64 ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.U64:
                                    {
                                        // Perform compare U64 : U64
                                        (sp - 1)->I32 = ((ulong)(sp - 1)->I64 != (ulong)sp->I64) ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.Address:
                                    {
                                        // Perform compare IntPtr : IntPtr
                                        (sp - 1)->I32 = (sp - 1)->Ptr != (nint)sp->Ptr ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.F32:
                                    {
                                        // Perform compare F32 : F32
                                        (sp - 1)->I32 = (sp - 1)->F32 != sp->F32 ? 1 : 0;
                                        break;
                                    }
                                case StackTypeCode.F64:
                                    {
                                        // Perform compare F64 : F64
                                        (sp - 1)->I32 = (sp - 1)->F64 != sp->F64 ? 1 : 0;
                                        break;
                                    }
                            }
                            // Set type
                            (sp - 1)->Type = StackTypeCode.I32;

                            // Debug instruction
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    #endregion

                    // Cast
                    #region Cast
                    case OpCode.Cast_I4:
                        {
                            // Pop value
                            sp--;

                            // Check type
                            switch(sp->Type)
                            {
                                case StackTypeCode.I32: break;
                                case StackTypeCode.U32:
                                    {
                                        sp->I32 = (int)(uint)sp->I32;
                                        break;
                                    }
                                case StackTypeCode.I64:
                                    {
                                        sp->I32 = (int)sp->I64;
                                        break;
                                    }
                                case StackTypeCode.U64:
                                    {
                                        sp->I32 = (int)(ulong)sp->I64;
                                        break;
                                    }
                                case StackTypeCode.F32:
                                    {
                                        sp->I32 = (int)sp->F32;
                                        break;
                                    }
                                case StackTypeCode.F64:
                                    {
                                        sp->I32 = (int)sp->F64;
                                        break;
                                    }
                                case StackTypeCode.Address:
                                    {
                                        sp->I32 = (int)sp->Ptr;
                                        break;
                                    }
                            }

                            // Push value
                            sp->Type = StackTypeCode.I32;
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Cast_I8:
                        {
                            // Pop value
                            sp--;

                            // Check type
                            switch (sp->Type)
                            {
                                case StackTypeCode.I64: break;
                                case StackTypeCode.I32:
                                    {
                                        sp->I64 = (long)sp->I32;
                                        break;
                                    }
                                case StackTypeCode.U32:
                                    {
                                        sp->I64 = (long)(uint)sp->I32;
                                        break;
                                    }
                                case StackTypeCode.U64:
                                    {
                                        sp->I64 = (long)(ulong)sp->I64;
                                        break;
                                    }
                                case StackTypeCode.F32:
                                    {
                                        sp->I64 = (long)sp->F32;
                                        break;
                                    }
                                case StackTypeCode.F64:
                                    {
                                        sp->I64 = (long)sp->F64;
                                        break;
                                    }
                                case StackTypeCode.Address:
                                    {
                                        sp->I64 = (long)sp->Ptr;
                                        break;
                                    }
                            }

                            // Push value
                            sp->Type = StackTypeCode.I64;
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Cast_U4:
                        {
                            // Pop value
                            sp--;

                            // Check type
                            switch (sp->Type)
                            {
                                case StackTypeCode.U32: break;
                                case StackTypeCode.I32:
                                    {
                                        sp->I32 = (int)(uint)sp->I32;
                                        break;
                                    }
                                case StackTypeCode.I64:
                                    {
                                        sp->I32 = (int)(uint)sp->I64;
                                        break;
                                    }
                                case StackTypeCode.U64:
                                    {
                                        sp->I32 = (int)(uint)(ulong)sp->I64;
                                        break;
                                    }
                                case StackTypeCode.F32:
                                    {
                                        sp->I32 = (int)(uint)sp->F32;
                                        break;
                                    }
                                case StackTypeCode.F64:
                                    {
                                        sp->I32 = (int)(uint)sp->F64;
                                        break;
                                    }
                                case StackTypeCode.Address:
                                    {
                                        sp->I32 = (int)(uint)sp->Ptr;
                                        break;
                                    }
                            }

                            // Push value
                            sp->Type = StackTypeCode.U32;
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Cast_U8:
                        {
                            // Pop value
                            sp--;

                            // Check type
                            switch (sp->Type)
                            {
                                case StackTypeCode.U64: break;
                                case StackTypeCode.I32:
                                    {
                                        sp->I64 = (long)(ulong)sp->I32;
                                        break;
                                    }
                                case StackTypeCode.U32:
                                    {
                                        sp->I64 = (long)(ulong)(uint)sp->I32;
                                        break;
                                    }
                                case StackTypeCode.I64:
                                    {
                                        sp->I64 = (long)(ulong)sp->I64;
                                        break;
                                    }
                                case StackTypeCode.F32:
                                    {
                                        sp->I64 = (long)(ulong)sp->F32;
                                        break;
                                    }
                                case StackTypeCode.F64:
                                    {
                                        sp->I64 = (long)(ulong)sp->F64;
                                        break;
                                    }
                                case StackTypeCode.Address:
                                    {
                                        sp->I64 = (long)(ulong)sp->Ptr;
                                        break;
                                    }
                            }

                            // Push value
                            sp->Type = StackTypeCode.U64;
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Cast_F4:
                        {
                            // Pop value
                            sp--;

                            // Check type
                            switch (sp->Type)
                            {
                                case StackTypeCode.F32: break;
                                case StackTypeCode.I32:
                                    {
                                        sp->F32 = (float)sp->I32;
                                        break;
                                    }
                                case StackTypeCode.U32:
                                    {
                                        sp->F32 = (float)(uint)sp->I32;
                                        break;
                                    }
                                case StackTypeCode.I64:
                                    {
                                        sp->F32 = (float)sp->I64;
                                        break;
                                    }
                                case StackTypeCode.U64:
                                    {
                                        sp->F32 = (float)(ulong)sp->I64;
                                        break;
                                    }
                                case StackTypeCode.F64:
                                    {
                                        sp->F32 = (float)sp->F64;
                                        break;
                                    }
                                case StackTypeCode.Address:
                                    {
                                        sp->F32 = (float)sp->Ptr;
                                        break;
                                    }
                            }

                            // Push value
                            sp->Type = StackTypeCode.F32;
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Cast_F8:
                        {
                            // Pop value
                            sp--;

                            // Check type
                            switch (sp->Type)
                            {
                                case StackTypeCode.F64: break;
                                case StackTypeCode.I32:
                                    {
                                        sp->F64 = (double)sp->I32;
                                        break;
                                    }
                                case StackTypeCode.U32:
                                    {
                                        sp->F64 = (double)(uint)sp->I32;
                                        break;
                                    }
                                case StackTypeCode.I64:
                                    {
                                        sp->F64 = (double)sp->I64;
                                        break;
                                    }
                                case StackTypeCode.U64:
                                    {
                                        sp->F64 = (double)(ulong)sp->I64;
                                        break;
                                    }
                                case StackTypeCode.Address:
                                    {
                                        sp->F64 = (double)sp->Ptr;
                                        break;
                                    }
                            }

                            // Push value
                            sp->Type = StackTypeCode.F64;
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    #endregion

                    // Jump
                    #region Jump
                    case OpCode.Jmp_1:
                        {
                            // Get offset
                            int offset = *(int*)pc;
                            pc += sizeof(int);

                            // Debug execution
                            context.DebugInstruction(code, pc - 5, sp - 1, offset);

                            // Check condition
                            sp--;
                            if(sp->I32 == 1)
                            {
                                // Jump to offset
                                pc += offset;
                            }
                            break;
                        }
                    case OpCode.Jmp_0:
                        {
                            // Get offset
                            int offset = *(int*)pc;
                            pc += sizeof(int);

                            // Debug execution
                            context.DebugInstruction(code, pc - 5, sp - 1, offset);

                            // Check condition
                            sp--;
                            if (sp->I32 == 0)
                            {
                                // Jump to offset
                                pc += offset;
                            }
                            break;
                        }
                    case OpCode.Jmp:
                        {
                            // Get offset
                            int offset = *(int*)pc;
                            pc += sizeof(int);

                            // Debug execution
                            context.DebugInstruction(code, pc - 5, offset);

                            // Jump to offset
                            pc += offset;
                            break;
                        }
                    #endregion

                    // Object
                    #region Object
                    case OpCode.New:
                        {
                            // Get token
                            int token = *(int*)pc;
                            pc += sizeof(int);

                            // Get type handle
                            _TypeHandle* typeHandle = (_TypeHandle*)context.AppContext.typeHandles[token];

                            // Create new instance and push to stack
                            sp->Type = StackTypeCode.Address;
                            sp->Ptr = (IntPtr)__memory.Alloc(typeHandle);
                            sp++;

                            // TODO - call constructor?

                            // Debug execution
                            context.DebugInstruction(code, pc - 5, sp - 1);
                            break;
                        }
                    case OpCode.Call:
                    case OpCode.Call_Virt:
                        {
                            // Get token from instruction
                            int token = *(int*)pc;
                            pc += sizeof(int);

                            // Get method handle
                            _MethodHandle* callHandle = (_MethodHandle*)context.AppContext.methodHandles[token];

                            // Get call ptr
                            StackData* spCall = sp;

                            // Decrement stack ptr
                            sp -= callHandle->Signature.ParameterCount;

                            // Copy arguments
                            for(int i = 0; i < callHandle->Signature.ParameterCount; i++)
                            {
                                // Copy arg
                                StackData.CopyStack(sp + i, spCall + i);
                            }

                            // Check for virtual call - need to resolve the correct method via late binding
                            if(code == OpCode.Call_Virt)
                            {
                                // TODO
                            }

                            // Debug execution
                            context.DebugInstruction(code, pc - 5);

                            // Push call
                            context.PushCall(method, pc, spVar, sp);

                            // Update ptrs to jump to call
                            pc = (byte*)callHandle + sizeof(_MethodHandle);
                            spVar = spCall;
                            sp = spVar + (callHandle->Signature.ParameterCount + callHandle->Body.VariableCount);

                            // Check overflow
                            if (sp + callHandle->Body.MaxStack >= spMax)
                                context.Throw<StackOverflowException>();

                            // Set instruction ptr
                            context.DebugInstructionPtr(pc);
                            break;
                        }
                    case OpCode.Call_Addr:
                        {
                            // Pop handle
                            sp--;

                            // Get method handle from stack
                            _MethodHandle* callHandle = (_MethodHandle*)sp->Ptr;

                            // Get call ptr
                            StackData* spCall = sp;

                            // Decrement stack ptr
                            sp -= callHandle->Signature.ParameterCount;

                            // Copy arguments
                            for (int i = 0; i < callHandle->Signature.ParameterCount; i++)
                            {
                                // Copy arg
                                StackData.CopyStack(sp + i, spCall + i);
                            }

                            // Debug execution
                            context.DebugInstruction(code, pc - 5);

                            // Push call
                            context.PushCall(method, pc, spVar, sp);

                            // Update ptrs to jump to call
                            pc = (byte*)callHandle + sizeof(_MethodHandle);
                            spVar = spCall;
                            sp = spVar + (callHandle->Signature.ParameterCount + callHandle->Body.VariableCount);

                            // Check overflow
                            if (sp + callHandle->Body.MaxStack >= spMax)
                                context.Throw<StackOverflowException>();

                            // Set instruction ptr
                            context.DebugInstructionPtr(pc);
                            break;
                        }
                    case OpCode.Ret:
                        {
                            // Debug execution
                            context.DebugInstruction(code, pc - 1);

                            // Get return address
                            StackData* spReturn = sp;

                            // Pop call stack
                            context.PopCall(out method, out pc, out spVar, out sp);

                            // Copy return value
                            // if HasReturnValue
                            StackData.CopyStack(spReturn - 1, sp);
                            sp++;

                            // Check for end of call stack
                            if(context.CallDepth == 0)
                                halt = true;

                            break;
                        }

                    case OpCode.Ld_Size:
                        {
                            // Pop handle
                            sp--;

                            // Push size of type
                            sp->Type = StackTypeCode.I32;
                            sp->I32 = (int)((_TypeHandle*)sp->Ptr)->TypeSize;
                            sp++;
                            
                            // Debug execution
                            context.DebugInstruction(code, pc - 1, sp - 1);
                            break;
                        }
                    case OpCode.Ld_Type:
                        {
                            // Get type token
                            int token = *(int*)pc;
                            pc += sizeof(int);

                            // Get type handle ptr
                            _TypeHandle* typePtr = (_TypeHandle*)context.AppContext.typeHandles[token];

                            // Push address of type to stack
                            sp->Type = StackTypeCode.Address;
                            sp->Ptr = (IntPtr)typePtr;
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 5, sp - 1);
                            break;
                        }
                    case OpCode.Ld_Func:
                        {
                            // Get method token
                            int token = *(int*)pc;
                            pc += sizeof(int);

                            // Get method handle ptr
                            _MethodHandle* methodPtr = (_MethodHandle*)context.AppContext.methodHandles[token];

                            // Push address of method to stack
                            sp->Type = StackTypeCode.Address;
                            sp->Ptr = (IntPtr)methodPtr;
                            sp++;

                            // Debug execution
                            context.DebugInstruction(code, pc - 5, sp - 1);
                            break;
                        }
                    #endregion
                }
            }

            return sp - 1;
        }
    }
}
