using System;
using System.Diagnostics;

namespace LumaSharpVMBeef;

public static class LumaInterpreter
{
	internal static void Execute(LumaStackContext* stack)
	{
		// Get assembly context
		LumaAssemblyContext assembly = stack.method.assembly;

		// Main execution loop
		while(stack.pc < stack.pcmax)
		{
			// Fetch decode
			LumaOpCode op = stack.InstructionRead<LumaOpCode>();

			// Evaluate op code
			switch(op)
			{
			case .Nop: Debug.WriteLine("NOP");

				// Constant instructions
#region Constants
			case .Ld_I1:
				{
					// Fetch value from instruction
					int8 val = stack.InstructionRead<int8>();

					// Write to stack as i32
					stack.PushI32(val);
					DebugInstruction(op, stack);
				}
			case .Ld_I2:
				{
					// Fetch value from instruction
					int16 val = stack.InstructionRead<int16>();

					// Write to stack as i32
					stack.PushI32(val);
					DebugInstruction(op, stack);
				}
			case .Ld_I4:
				{
					// Fetch value from instruction
					int32 val = stack.InstructionRead<int32>();

					// Write to stack as i32
					stack.PushI32(val);
					DebugInstruction(op, stack);
				}
			case .Ld_I8:
				{
					// Fetch value from instruction
					int64 val = stack.InstructionRead<int64>();

					// Write to stack as i64
					stack.PushI64(val);
					DebugInstruction(op, stack);
				}
			case .Ld_F4:
				{
					// Fetch value from instruction
					float val = stack.InstructionRead<float>();

					// Write to stack as float
					stack.PushF32(val);
					DebugInstruction(op, stack);
				}
			case .Ld_F8:
				{
				// Fetch value from instruction
				double val = stack.InstructionRead<double>();

				// Write to stack as double
				stack.PushF64(val);
				DebugInstruction(op, stack);
				}
			case .Ld_Str:
				{
					// Get string token
					LumaToken token = stack.InstructionRead<LumaToken>();

					// Get the string object
					String str = assembly.GetString(token);

					// Get address of string
					void* ptr = System.Internal.UnsafeCastToPtr(str);

					// Push pointer to stack
					stack.PushPtr(ptr);
					DebugInstruction(op, stack);
				}
			case .Ld_Null:
				{
					// Write to stack as null pointer
					stack.PushPtr(null);
					DebugInstruction(op, stack);
				}

				// Fixed constants
			case .Ld_I4_0:
				{
					// Write 0 to stack
					stack.PushI32(0);
					DebugInstruction(op, stack);
				}
			case .Ld_I4_1:
				{
					// Write 1 to stack
					stack.PushI32(1);
					DebugInstruction(op, stack);
				}
			case .Ld_I4_M1:
				{
					// Write -1 to stack
					stack.PushI32(-1);
					DebugInstruction(op, stack);
				}
			case .Ld_F4_0:
				{
					// Write 0 to stack
					stack.PushF32(0f);
					DebugInstruction(op, stack);
				}
#endregion

				// Locals
#region Locals
				// Load
			case .Ld_Loc_0:
				{
					// Get the local
					LumaStackHandle* local = &stack.method.locals[0];

					// Get address of local
					void* localAddr = stack.StackGetLocalAddress(local.offset);

					// Copy to stack
					stack.PushCopy(localAddr, local.type);
					DebugInstruction(op, stack);
				}
			case .Ld_Loc_1:
				{
					// Get the local
					LumaStackHandle* local = &stack.method.locals[1];

					// Get address of local
					void* localAddr = stack.StackGetLocalAddress(local.offset);

					// Copy to stack
					stack.PushCopy(localAddr, local.type);
					DebugInstruction(op, stack);
				}
			case .Ld_Loc_2:
				{
					// Get the local
					LumaStackHandle* local = &stack.method.locals[2];

					// Get address of local
					void* localAddr = stack.StackGetLocalAddress(local.offset);

					// Copy to stack
					stack.PushCopy(localAddr, local.type);
					DebugInstruction(op, stack);
				}
			case .Ld_Loc_3:
				{
					// Get the local
					LumaStackHandle* local = &stack.method.locals[3];
	
					// Get address of local
					void* localAddr = stack.StackGetLocalAddress(local.offset);
	
					// Copy to stack
					stack.PushCopy(localAddr, local.type);
					DebugInstruction(op, stack);
				}
			case .Ld_Loc_4:
				{
					// Get the local
					LumaStackHandle* local = &stack.method.locals[4];
	
					// Get address of local
					void* localAddr = stack.StackGetLocalAddress(local.offset);
	
					// Copy to stack
					stack.PushCopy(localAddr, local.type);
					DebugInstruction(op, stack);
				}
			case .Ld_Loc:
				{
					// Fetch index
					uint8 index = stack.InstructionRead<uint8>();

					// Get the local
					LumaStackHandle* local = &stack.method.locals[index];

					// Get address of local
					void* localAddr = stack.StackGetLocalAddress(local.offset);

					// Copy to stack
					stack.PushCopy(localAddr, local.type);
					DebugInstruction(op, stack);
				}
			case .Ld_Loc_E:
				{
					// Fetch index
					uint16 index = stack.InstructionRead<uint16>();

					// Get the local
					LumaStackHandle* local = &stack.method.locals[index];

					// Get address of local
					void* localAddr = stack.StackGetLocalAddress(local.offset);

					// Copy to stack
					stack.PushCopy(localAddr, local.type);
					DebugInstruction(op, stack);
				}
			case .Ld_Loc_A:
				{
					// Fetch index
					uint8 index = stack.InstructionRead<uint8>();

					// Get the local
					LumaStackHandle* local = &stack.method.locals[index];

					// Get address of local
					void* localAddr = stack.StackGetLocalAddress(local.offset);
					
					// Write pointer to stack
					stack.PushPtr(localAddr);
					DebugInstruction(op, stack);
				}
			case .Ld_Loc_EA:
				{
					// Fetch index
					uint16 index = stack.InstructionRead<uint16>();

					// Get the local
					LumaStackHandle* local = &stack.method.locals[index];

					// Get address of local
					void* localAddr = stack.StackGetLocalAddress(local.offset);

					// Write pointer to stack
					stack.PushPtr(localAddr);
					DebugInstruction(op, stack);
				}

				// Store
			case .St_Loc_0:
				{
					// Get the local
					LumaStackHandle* local = &stack.method.locals[0];

					// Get address of local
					void* localAddr = stack.StackGetLocalAddress(local.offset);

					// Copy from stack
					stack.PopCopy(localAddr, local.type);
					DebugInstruction(op, stack);
				}
			case .St_Loc_1:
				{
					// Get the local
					LumaStackHandle* local = &stack.method.locals[1];

					// Get address of local
					void* localAddr = stack.StackGetLocalAddress(local.offset);

					// Copy from stack
					stack.PopCopy(localAddr, local.type);
					DebugInstruction(op, stack);
				}
			case .St_Loc_2:
				{
					// Get the local
					LumaStackHandle* local = &stack.method.locals[2];

					// Get address of local
					void* localAddr = stack.StackGetLocalAddress(local.offset);

					// Copy from stack
					stack.PopCopy(localAddr, local.type);
					DebugInstruction(op, stack);
				}
			case .St_Loc_3:
				{
					// Get the local
					LumaStackHandle* local = &stack.method.locals[3];
	
					// Get address of local
					void* localAddr = stack.StackGetLocalAddress(local.offset);
	
					// Copy from stack
					stack.PopCopy(localAddr, local.type);
					DebugInstruction(op, stack);
				}
			case .St_Loc_4:
				{
					// Get the local
					LumaStackHandle* local = &stack.method.locals[4];
	
					// Get address of local
					void* localAddr = stack.StackGetLocalAddress(local.offset);
	
					// Copy from stack
					stack.PopCopy(localAddr, local.type);
					DebugInstruction(op, stack);
				}
			case .St_Loc:
				{
					// Fetch index from instruction
					uint8 index = stack.InstructionRead<uint8>();

					// Get the local
					LumaStackHandle* local = &stack.method.locals[index];

					// Get address of local
					void* localAddr = stack.StackGetLocalAddress(local.offset);

					// Copy from stack
					stack.PopCopy(localAddr, local.type);
					DebugInstruction(op, stack);
				}
			case .St_Loc_E:
				{
					// Fetch index from instruction
					uint16 index = stack.InstructionRead<uint16>();

					// Get the local
					LumaStackHandle* local = &stack.method.locals[index];

					// Get address of local
					void* localAddr = stack.StackGetLocalAddress(local.offset);

					// Copy from stack
					stack.PopCopy(localAddr, local.type);
					DebugInstruction(op, stack);
				}
#endregion

				// Arguments
#region Arguments
				// Load
			case .Ld_Arg_0:
				{
					// Get the argument
					LumaStackHandle* arg = &stack.method.signature.parameters[0];
	
					// Get address of argument
					void* argAddr = stack.StackGetArgAddress(arg.offset);
	
					// Copy to stack
					stack.PushCopy(argAddr, arg.type);
					DebugInstruction(op, stack);
				}
			case .Ld_Arg_1:
				{
					// Get the argument
					LumaStackHandle* arg = &stack.method.signature.parameters[1];
	
					// Get address of argument
					void* argAddr = stack.StackGetArgAddress(arg.offset);
	
					// Copy to stack
					stack.PushCopy(argAddr, arg.type);
					DebugInstruction(op, stack);
				}
			case .Ld_Arg_2:
				{
					// Get the argument
					LumaStackHandle* arg = &stack.method.signature.parameters[2];
	
					// Get address of argument
					void* argAddr = stack.StackGetArgAddress(arg.offset);
	
					// Copy to stack
					stack.PushCopy(argAddr, arg.type);
					DebugInstruction(op, stack);
				}
			case .Ld_Arg_3:
				{
					// Get the argument
					LumaStackHandle* arg = &stack.method.signature.parameters[3];
	
					// Get address of argument
					void* argAddr = stack.StackGetArgAddress(arg.offset);
	
					// Copy to stack
					stack.PushCopy(argAddr, arg.type);
					DebugInstruction(op, stack);
				}
			case .Ld_Arg_4:
				{
					// Get the argument
					LumaStackHandle* arg = &stack.method.signature.parameters[4];
	
					// Get address of argument
					void* argAddr = stack.StackGetArgAddress(arg.offset);
	
					// Copy to stack
					stack.PushCopy(argAddr, arg.type);
					DebugInstruction(op, stack);
				}
			case .Ld_Arg:
				{
					// Fetch index
					uint8 index = stack.InstructionRead<uint8>();

					// Get the argument
					LumaStackHandle* arg = &stack.method.signature.parameters[index];
	
					// Get address of argument
					void* argAddr = stack.StackGetArgAddress(arg.offset);
	
					// Copy to stack
					stack.PushCopy(argAddr, arg.type);
					DebugInstruction(op, stack);
				}
			case .Ld_Arg_E:
				{
					// Fetch index
					uint16 index = stack.InstructionRead<uint16>();
	
					// Get the argument
					LumaStackHandle* arg = &stack.method.signature.parameters[index];
	
					// Get address of argument
					void* argAddr = stack.StackGetArgAddress(arg.offset);
	
					// Copy to stack
					stack.PushCopy(argAddr, arg.type);
					DebugInstruction(op, stack);
				}
			case .Ld_Arg_A:
				{
					// Fetch index
					uint8 index = stack.InstructionRead<uint8>();
	
					// Get the argument
					LumaStackHandle* arg = &stack.method.signature.parameters[index];
	
					// Get address of argument
					void* argAddr = stack.StackGetArgAddress(arg.offset);

					// Push address to stack
					stack.PushPtr(argAddr);
					DebugInstruction(op, stack);
				}
			case .Ld_Arg_EA:
				{
					// Fetch index
					uint16 index = stack.InstructionRead<uint16>();
	
					// Get the local
					LumaStackHandle* arg = &stack.method.signature.parameters[index];
	
					// Get address of local
					void* argAddr = stack.StackGetArgAddress(arg.offset);
	
					// Push address to stack
					stack.PushPtr(argAddr);
					DebugInstruction(op, stack);
				}

				// Store
			case .St_Arg_0:
				{
					// Get the argument
					LumaStackHandle* arg = &stack.method.signature.parameters[0];
	
					// Get address of local
					void* argAddr = stack.StackGetArgAddress(arg.offset);
	
					// Copy from stack
					stack.PopCopy(argAddr, arg.type);
					DebugInstruction(op, stack);
				}
			case .St_Arg_1:
				{
					// Get the argument
					LumaStackHandle* arg = &stack.method.signature.parameters[1];
	
					// Get address of local
					void* argAddr = stack.StackGetArgAddress(arg.offset);
	
					// Copy from stack
					stack.PopCopy(argAddr, arg.type);
					DebugInstruction(op, stack);
				}
			case .St_Arg_2:
				{
					// Get the argument
					LumaStackHandle* arg = &stack.method.signature.parameters[2];
	
					// Get address of local
					void* argAddr = stack.StackGetArgAddress(arg.offset);
	
					// Copy from stack
					stack.PopCopy(argAddr, arg.type);
					DebugInstruction(op, stack);
				}
			case .St_Arg_3:
				{
					// Get the argument
					LumaStackHandle* arg = &stack.method.signature.parameters[3];
	
					// Get address of local
					void* argAddr = stack.StackGetArgAddress(arg.offset);
	
					// Copy from stack
					stack.PopCopy(argAddr, arg.type);
					DebugInstruction(op, stack);
				}
			case .St_Arg_4:
				{
					// Get the argument
					LumaStackHandle* arg = &stack.method.signature.parameters[4];
	
					// Get address of local
					void* argAddr = stack.StackGetArgAddress(arg.offset);
	
					// Copy from stack
					stack.PopCopy(argAddr, arg.type);
					DebugInstruction(op, stack);
				}
			case .St_Arg:
				{
					// Fetch index
					uint8 index = stack.InstructionRead<uint8>();

					// Get the argument
					LumaStackHandle* arg = &stack.method.signature.parameters[index];
	
					// Get address of local
					void* argAddr = stack.StackGetArgAddress(arg.offset);
	
					// Copy from stack
					stack.PopCopy(argAddr, arg.type);
					DebugInstruction(op, stack);
				}
			case .St_Arg_E:
				{
					// Fetch index
					uint16 index = stack.InstructionRead<uint16>();
	
					// Get the argument
					LumaStackHandle* arg = &stack.method.signature.parameters[index];
	
					// Get address of local
					void* argAddr = stack.StackGetArgAddress(arg.offset);
	
					// Copy from stack
					stack.PopCopy(argAddr, arg.type);
					DebugInstruction(op, stack);
				}
#endregion

				// Address
#region Address
			case .Ld_Addr_I1:
				{
					// Get the address
					void* addr = stack.PopPtr();

					// Get value at address
					int8 val = *(int8*)addr;

					// Load onto stack as i32
					stack.PushI32(val);
					DebugInstruction(op, stack);
				}
			case .Ld_Addr_I2:
				{
					// Get the address
					void* addr = stack.PopPtr();

					// Get value at address
					int16 val = *(int16*)addr;

					// Load onto stack as i32
					stack.PushI32(val);
					DebugInstruction(op, stack);
				}
			case .Ld_Addr_I4:
				{
					// Get the address
					void* addr = stack.PopPtr();

					// Get value at address
					int32 val = *(int32*)addr;

					// Load onto stack as i32
					stack.PushI32(val);
					DebugInstruction(op, stack);
				}
			case .Ld_Addr_I8:
				{
					// Get the address
					void* addr = stack.PopPtr();

					// Get value at address
					int64 val = *(int64*)addr;

					// Load onto stack as i64
					stack.PushI64(val);
					DebugInstruction(op, stack);
				}
			case .Ld_Addr_F4:
				{
					// Get the address
					void* addr = stack.PopPtr();

					// Get value at address
					float val = *(float*)addr;

					// Load onto stack as float
					stack.PushF32(val);
					DebugInstruction(op, stack);
				}
			case .Ld_Addr_F8:
				{
					// Get the address
					void* addr = stack.PopPtr();

					// Get value at address
					double val = *(double*)addr;

					// Load onto stack as double
					stack.PushF64(val);
					DebugInstruction(op, stack);
				}
			case .Ld_Addr_Any:
				{
					// Get the type
					int32 token = stack.InstructionRead<int32>();

					// Get the type handle
					LumaTypeHandle* type = null;

					// Get the address
					void* addr = stack.PopPtr();

					// Check for value type
					if((type.flags & .ValueType) != 0)
					{
						// Value type copy
						stack.PushValueTypeCopy(addr, type.size);
					}
					else
					{
						// Get value at address - pointer to a pointer
						void* val = *(void**)addr;
		
						// Load onto stack as ref
						stack.PushPtr(val);
					}
					DebugInstruction(op, stack);
				}
			case .St_Addr_I1:
				{
					// Get the value
					int8 val = (int8)stack.PopI32();

					// Get the address
					void* addr = stack.PopPtr();

					// Store at address as i8
					*(int8*)addr = val;
					DebugInstruction(op, stack);
				}
			case .St_Addr_I2:
				{
					// Get the value
					int16 val = (int16)stack.PopI32();

					// Get the address
					void* addr = stack.PopPtr();

					// Store at address as i16
					*(int16*)addr = val;
					DebugInstruction(op, stack);
				}
			case .St_Addr_I4:
				{
					// Get the value
					int32 val = stack.PopI32();

					// Get the address
					void* addr = stack.PopPtr();

					// Store at address as i32
					*(int32*)addr = val;
					DebugInstruction(op, stack);
				}
			case .St_Addr_I8:
				{
					// Get the value
					int64 val = stack.PopI64();

					// Get the address
					void* addr = stack.PopPtr();

					// Store at address as i64
					*(int64*)addr = val;
					DebugInstruction(op, stack);
				}
			case .St_Addr_F4:
				{
					// Get the value
					float val = stack.PopF32();

					// Get the address
					void* addr = stack.PopPtr();

					// Store at address as float
					*(float*)addr = val;
					DebugInstruction(op, stack);
				}
			case .St_Addr_F8:
				{
					// Get the value
					double val = stack.PopF64();

					// Get the address
					void* addr = stack.PopPtr();

					// Store at address as double
					*(double*)addr = val;
					DebugInstruction(op, stack);
				}
			case .St_Addr_Any:
				{
					// Get the type
					int32 token = stack.InstructionRead<int32>();

					// Get the type handle
					LumaTypeHandle* type = null;

					// Check for value type
					if((type.flags & .ValueType) != 0)
					{
						// Get the value type - the size is not constant so we pop based on size and keep the address which is value until we write to the stack again
						LumaStackValue* val = stack.Pop();

						// Get the address
						void* addr = stack.PopPtr();

						// Value type copy
						LumaMemory.Copy(val.Ptr, addr, type.size);
					}
					else
					{
						// Get the value
						void* val = stack.PopPtr();

						// Get the address
						void* addr = stack.PopPtr();

						// Store at address as float
						*(void**)addr = val;
					}
					
					DebugInstruction(op, stack);
				}
#endregion

				// Arithmetic
#region Arithmetic
			case .Add:
				{
					// Perform add
					LumaArithmetic.Add(stack);
					DebugInstruction(op, stack);
				}
			case .Sub:
				{
					// Perform add
					LumaArithmetic.Sub(stack);
					DebugInstruction(op, stack);
				}
			case .Mul:
				{
					// Perform add
					LumaArithmetic.Mul(stack);
					DebugInstruction(op, stack);
				}
			case .Div:
				{
					// Perform add
					LumaArithmetic.Div(stack);
					DebugInstruction(op, stack);
				}
			case .Mod:
				{
					// Perform add
					LumaArithmetic.Mod(stack);
					DebugInstruction(op, stack);
				}
			case .Neg:
				{
					// Perform add
					LumaArithmetic.Neg(stack);
					DebugInstruction(op, stack);
				}
			case .Bit_And:
				{
					// Perform add
					LumaArithmetic.Bitwise_And(stack);
					DebugInstruction(op, stack);
				}
			case .Bit_Or:
				{
					// Perform add
					LumaArithmetic.Bitwise_Or(stack);
					DebugInstruction(op, stack);
				}
			case .Bit_XOr:
				{
					// Perform add
					LumaArithmetic.Bitwise_Xor(stack);
					DebugInstruction(op, stack);
				}
			case .Bit_Not:
				{
					// Perform add
					LumaArithmetic.Bitwise_Not(stack);
					DebugInstruction(op, stack);
				}
			case .Bit_Shl:
				{
					// Perform add
					LumaArithmetic.Bitwise_Shl(stack);
					DebugInstruction(op, stack);
				}
			case .Bit_Shr:
				{
					// Perform add
					LumaArithmetic.Bitwise_Shr(stack);
					DebugInstruction(op, stack);
				}
			case .Cmp_L:
				{
					// Get primitive type
					LumaPrimitiveTypeCode type = stack.InstructionRead<LumaPrimitiveTypeCode>();

					// Perform add
					LumaArithmetic.Cmp_Less(ref stack.sp, type);
					DebugInstruction(op, stack);
				}
			case .Cmp_Le:
				{
					// Get primitive type
					LumaPrimitiveTypeCode type = stack.InstructionRead<LumaPrimitiveTypeCode>();

					// Perform add
					LumaArithmetic.Cmp_LessEqual(ref stack.sp, type);
					DebugInstruction(op, stack);
				}
			case .Cmp_G:
				{
					// Get primitive type
					LumaPrimitiveTypeCode type = stack.InstructionRead<LumaPrimitiveTypeCode>();

					// Perform add
					LumaArithmetic.Cmp_Greater(ref stack.sp, type);
					DebugInstruction(op, stack);
				}
			case .Cmp_Ge:
				{
					// Get primitive type
					LumaPrimitiveTypeCode type = stack.InstructionRead<LumaPrimitiveTypeCode>();

					// Perform add
					LumaArithmetic.Cmp_GreaterEqual(ref stack.sp, type);
					DebugInstruction(op, stack);
				}
			case .Cmp_Eq:
				{
					// Get primitive type
					LumaPrimitiveTypeCode type = stack.InstructionRead<LumaPrimitiveTypeCode>();

					// Perform add
					LumaArithmetic.Cmp_Equal(ref stack.sp, type);
					DebugInstruction(op, stack);
				}
			case .Cmp_NEq:
				{
					// Get primitive type
					LumaPrimitiveTypeCode type = stack.InstructionRead<LumaPrimitiveTypeCode>();

					// Perform add
					LumaArithmetic.Cmp_NotEqual(ref stack.sp, type);
					DebugInstruction(op, stack);
				}
#endregion

				// Convert
#region Convert
			case .Cast_I1:
				{
					// Get primitive type
					LumaPrimitiveTypeCode type = stack.InstructionRead<LumaPrimitiveTypeCode>();

					// Perform convert
					LumaConvert.ConvertI8(stack.sp, type);
					DebugInstruction(op, stack);
				}
			case .Cast_I2:
				{
					// Get primitive type
					LumaPrimitiveTypeCode type = stack.InstructionRead<LumaPrimitiveTypeCode>();

					// Perform convert
					LumaConvert.ConvertI16(stack.sp, type);
					DebugInstruction(op, stack);
				}
			case .Cast_I4:
				{
					// Get primitive type
					LumaPrimitiveTypeCode type = stack.InstructionRead<LumaPrimitiveTypeCode>();

					// Perform convert
					LumaConvert.ConvertI32(stack.sp, type);
					DebugInstruction(op, stack);
				}
			case .Cast_I8:
				{
					// Get primitive type
					LumaPrimitiveTypeCode type = stack.InstructionRead<LumaPrimitiveTypeCode>();

					// Perform convert
					LumaConvert.ConvertI64(stack.sp, type);
					DebugInstruction(op, stack);
				}
			case .Cast_UI1:
				{
					// Get primitive type
					LumaPrimitiveTypeCode type = stack.InstructionRead<LumaPrimitiveTypeCode>();

					// Perform convert
					LumaConvert.ConvertU8(stack.sp, type);
					DebugInstruction(op, stack);
				}
			case .Cast_UI2:
				{
					// Get primitive type
					LumaPrimitiveTypeCode type = stack.InstructionRead<LumaPrimitiveTypeCode>();

					// Perform convert
					LumaConvert.ConvertU16(stack.sp, type);
					DebugInstruction(op, stack);
				}
			case .Cast_UI4:
				{
					// Get primitive type
					LumaPrimitiveTypeCode type = stack.InstructionRead<LumaPrimitiveTypeCode>();

					// Perform convert
					LumaConvert.ConvertU32(stack.sp, type);
					DebugInstruction(op, stack);
				}
			case .Cast_UI8:
				{
					// Get primitive type
					LumaPrimitiveTypeCode type = stack.InstructionRead<LumaPrimitiveTypeCode>();

					// Perform convert
					LumaConvert.ConvertU64(stack.sp, type);
					DebugInstruction(op, stack);
				}
			case .Cast_F4:
				{
					// Get primitive type
					LumaPrimitiveTypeCode type = stack.InstructionRead<LumaPrimitiveTypeCode>();

					// Perform convert
					LumaConvert.ConvertF32(stack.sp, type);
					DebugInstruction(op, stack);
				}
			case .Cast_F8:
				{
					// Get primitive type
					LumaPrimitiveTypeCode type = stack.InstructionRead<LumaPrimitiveTypeCode>();

					// Perform convert
					LumaConvert.ConvertF64(stack.sp, type);
					DebugInstruction(op, stack);
				}
			case .Cast_Addr:
				{
					// Get primitive type
					LumaPrimitiveTypeCode type = stack.InstructionRead<LumaPrimitiveTypeCode>();

					// Perform convert
					LumaConvert.ConvertPtr(stack.sp, type);
					DebugInstruction(op, stack);
				}
#endregion

#region Jump
				// Jump
			case .Jmp:
				{
					// Get jump offset
					int32 offset = stack.InstructionRead<int32>();

					// Jump to target
					stack.pc += offset;
				}
			case .Jmp_1:
				{
					// Get jump offset
					int32 offset = stack.InstructionRead<int32>();

					// Pop bool as i32
					if(stack.PopI32() == 1)
					{
						// Jump to target
						stack.pc += offset;
					}
				}
			case .Jmp_0:
				{
					// Get jump offset
					int32 offset = stack.InstructionRead<int32>();

					// Pop bool as i32
					if(stack.PopI32() == 0)
					{
						// Jump to target
						stack.pc += offset;
					}
				}
#endregion

				// Field
#region Field
			case .Ld_Fld:
				{
					// Fetch field token
//					int32 token = stack.InstructionRead<int32>();
//
//					// Fetch field
//					LumaFieldHandle* field = null;//(LumaFieldHandle*)stack.InstructionRead();
//
//					// Check for instance
//					void* fieldBaseAddress = null;
//					if((field.flags & .Instance) != 0)
//					{
//						// Check for reference
//						if((field.declaringType.flags & .ValueType) == 0)
//						{
//							// Load the class
//							LumaClass* obj = (LumaClass*)stack.PopPtr();
//
//							// Get the address of the class field
//							fieldBaseAddress = obj.fieldData;
//						}
//						// Must be value type always passed by address
//						else
//						{
//							// Follow pointer to value type memory start
//							fieldBaseAddress = *(void**)stack.StackPeek(field.declaringType.size);
//						}
//					}
//					// Must be static
//					else
//					{
//
//					}
//
//					// Get the specific field address
//					void* fieldAddr = field.FieldGetAddress(fieldBaseAddress);
//
//					// Write to stack
//					stack.PushCopy(fieldAddr, field.fieldType);
					
				}
#endregion

				// Array
#region Array
			case .NewArr:
				{
					// Fetch type token
					LumaToken token = stack.InstructionRead<LumaToken>();

					// Get the type handle
					LumaTypeHandle* type = assembly.GetTypeHandle(token);

					// Pop length
					uint64 length = stack.PopU64();

					// Alloc array
					LumaArray* arr = LumaMemory.AllocArray(type, length);

					// Write pointer
					stack.PushPtr(arr);
					DebugInstruction(op, stack);
				}
			case .NewArr_S:
				{
					// Fetch type token
					LumaToken token = stack.InstructionRead<LumaToken>();

					// Get the type handle
					LumaTypeHandle* type = assembly.GetTypeHandle(token);

					// Pop length
					uint64 length = stack.PopU64();

					// Stack alloc array
					LumaArray* arr = LumaMemory.StackAllocArray(stack, type, length);

					// Write pointer
					stack.PushPtr(arr);
					DebugInstruction(op, stack);
				}
			case .Ld_Elem:
				{
					// Get array handle
					LumaArray* arr = (LumaArray*)stack.PopPtr();

					// Get index
					uint64 index = stack.PopU64();

					// Get element address
					void* elemtAddr = arr.ArrayGetElementAddress(index);

					// Copy element to stack
					stack.PushCopy(elemtAddr, arr.elementType);
					DebugInstruction(op, stack);
				}
			case .Ld_Elem_A:
				{
					// Get array handle
					LumaArray* arr = (LumaArray*)stack.PopPtr();

					// Get index
					uint64 index = stack.PopU64();

					// Get element address
					void* elemtAddr = arr.ArrayGetElementAddress(index);

					// Write address to stack
					stack.PushPtr(elemtAddr);
					DebugInstruction(op, stack);
				}
			case .St_Elem:
				{
					// Get array handle
					LumaArray* arr = (LumaArray*)stack.PopPtr();

					// Get index
					uint64 index = stack.PopU64();

					// Get element address
					void* elemtAddr = arr.ArrayGetElementAddress(index);

					// Copy from stack to element address
					stack.PopCopy(elemtAddr, arr.elementType);
					DebugInstruction(op, stack);
				}
#endregion

				// Object
#region Object
#endregion

			case .Ret:
				{
					// Force abort
					stack.pc = stack.pcmax;
					break;
				}

			default: Runtime.FatalError(scope $"Instruction is not implemented: {op}");
			}
		}	
	}

	private static void DebugInstruction(LumaOpCode op, LumaStackContext* stack)
	{
#if DEBUG
		Debug.Write(op);
		Debug.Write(": Stack = ");

		String str = scope .();
		stack.Peek().ToString(str);
		Debug.Write(str);

		Debug.WriteLine();
#endif
	}
}