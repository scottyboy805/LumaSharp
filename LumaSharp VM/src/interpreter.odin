package LumaVM

import "core:fmt"
import "core:strings"
import "core:mem"
import "core:math"

@private
luma_execute_bytecode :: proc(luma: ^LumaState, callAddr: rawptr) // instructions: []u8, stack: []u8, stackOffset: u32)
{
    // Set current call
    luma.pc = callAddr;

    // Create base call
    callBase := LumaStackCall {
        callAddr = callAddr,
        pcAddr = callAddr,
        sp0Addr = luma.sp,
        spAddr = luma_mem_ptr_offset(&luma.sp, int(size_of(LumaStackCall))),
    };

    // Push call to stack
    luma_stack_write_call(&luma.sp, callBase);

    // Execution loop
    for //i := 0; i < 50; i += 1
    {
        //fmt.printf("Raw bytecode: %x\n", (cast(^u8)luma.pc)^);

        // Get op code
        op: LumaOpCode = cast(LumaOpCode)luma_fetchdecode_8(&luma.pc);

        // Evaluate op code
        #partial switch op
        {
            case .Nop: fmt.println("NOP");
            case: 
            {
                fmt.printfln("Op code not implemented: %v", op);
                break;
            }

            // Constants
            case .Ld_I1:
                {
                    // Fetch value from instruction
                    constVal := luma_fetchdecode_8(&luma.pc);

                    // Write to stack as U8
                    luma_stack_write_U8(&luma.sp, constVal);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_32("Execute Ld_I1", luma.sp, -5);
                    }
                }
            case .Ld_I2:
                {
                    // Fetch value from instruction
                    constVal := luma_fetchdecode_16(&luma.pc);

                    // Write to stack as U16
                    luma_stack_write_U16(&luma.sp, constVal);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_32("Execute Ld_I2", luma.sp, -5);
                    }
                }
            case .Ld_I4:
                {
                    // Fetch value from instruction
                    constVal := luma_fetchdecode_32(&luma.pc);

                    // Write to stack
                    luma_stack_write_U32(&luma.sp, constVal);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_32("Execute Ld_I4", luma.sp, -5);
                    }
                }
            case .Ld_I8:
                {
                        // Fetch value from instruction
                    constVal := luma_fetchdecode_64(&luma.pc);

                    // Write to stack
                    luma_stack_write_U64(&luma.sp, constVal);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_64("Execute Ld_I8", luma.sp, -9);
                    }
                }

            // Fixed Constants
            case .Ld_I4_0:
                {
                    // Write to stack
                    luma_stack_write_U32(&luma.sp, 0);                    

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_32("Execute Ld_I4_0", luma.sp, -5);
                    }
                }
            case .Ld_I4_1:
                {
                    // Write to stack
                    luma_stack_write_U32(&luma.sp, 1);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_32("Execute Ld_I4_1", luma.sp, -5);
                    }
                }
            case .Ld_I4_M1:
                {
                    // Write to stack
                    m1: i32 = -1;
                    luma_stack_write_U32(&luma.sp, u32(m1));

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_32("Execute Ld_I4_M1", luma.sp, -5);
                    }
                }

            // Locals
            case .Ld_Loc_0:
                {
                    // Get method signature
                    local := (cast(^LumaMethodHandle)luma.call).localTypes[0];

                    // Get local size
                    size := u32(local.type.size);

                    // Get local stack token
                    token := luma_stacktoken_from_typecode(local.type.code);

                    // Copy to stack ptr
                    luma_stack_copy_from(&luma.sp, luma.sp0, size, token);                    

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_x("Execute Ld_Loc_0", luma.sp, size, -int(size + 1));
                    }
                }
            case .Ld_Loc_1:
                {
                    // Get method signature
                    local := (cast(^LumaMethodHandle)luma.call).localTypes[1];

                    // Get local size
                    size := u32(local.type.size);

                    // Get local stack token
                    token := luma_stacktoken_from_typecode(local.type.code);

                    // Get local offset ptr
                    localPtr := luma_mem_ptr_offset(luma.sp0, int(local.offset));

                    // Copy to stack ptr
                    luma_stack_copy_from(&luma.sp, localPtr, size, token);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_x("Execute Ld_Loc_1", luma.sp, size, -int(size + 1));
                    }
                }
            case .Ld_Loc_2:
                {
                    // Get method signature
                    local := (cast(^LumaMethodHandle)luma.call).localTypes[2];

                    // Get local size
                    size := u32(local.type.size);

                    // Get local stack token
                    token := luma_stacktoken_from_typecode(local.type.code);

                    // Get local offset ptr
                    localPtr := luma_mem_ptr_offset(luma.sp0, int(local.offset));

                    // Copy to stack ptr
                    luma_stack_copy_from(&luma.sp, localPtr, size, token);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_x("Execute Ld_Loc_2", luma.sp, size, -int(size + 1));
                    }
                }
            case .Ld_Loc:
                {
                    // Fetch index
                    index := luma_fetchdecode_16(&luma.pc);

                    // Get method signature
                    local := (cast(^LumaMethodHandle)luma.call).localTypes[index];

                    // Get local size
                    size := u32(local.type.size);

                    // Get local stack token
                    token := luma_stacktoken_from_typecode(local.type.code);

                    // Get local offset ptr
                    localPtr := luma_mem_ptr_offset(luma.sp0, int(local.offset));

                    // Copy to stack ptr
                    luma_stack_copy_from(&luma.sp, localPtr, size, token);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        builder: strings.Builder;
                        luma_debug_x(fmt.sbprintf(&builder, "Execute Ld_Loc [%d]", index), luma.sp, size, -int(size + 1));
                    }
                }
            case .St_Loc_0:
                {
                    // Get method signature
                    local := (cast(^LumaMethodHandle)luma.call).localTypes[0];

                    // Get local size
                    size := u32(local.type.size);

                    // Copy from stack ptr
                    luma_stack_copy_to(&luma.sp, luma.sp0, size);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_x("Execute St_Loc_0", luma.sp0, size, 0);
                    }
                }
            case .St_Loc_1:
                {
                    // Get method signature
                    local := (cast(^LumaMethodHandle)luma.call).localTypes[1];

                    // Get local size
                    size := u32(local.type.size);

                    // Get local offset ptr
                    localPtr := luma_mem_ptr_offset(luma.sp0, int(local.offset));

                    // Copy from stack ptr
                    luma_stack_copy_to(&luma.sp, localPtr, size);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_x("Execute St_Loc_1", localPtr, size, 0);
                    }
                }
            case .St_Loc_2:
                {
                    // Get method signature
                    local := (cast(^LumaMethodHandle)luma.call).localTypes[2];

                    // Get local size
                    size := u32(local.type.size);

                    // Get local offset ptr
                    localPtr := luma_mem_ptr_offset(luma.sp0, int(local.offset));

                    // Copy from stack ptr
                    luma_stack_copy_to(&luma.sp, localPtr, size);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_x("Execute St_Loc_2", localPtr, size, 0);
                    }
                }
            case .St_Loc:
                {
                    // Fetch index
                    index := luma_fetchdecode_16(&luma.pc);

                    // Get method signature
                    local := (cast(^LumaMethodHandle)luma.call).localTypes[index];

                    // Get local size
                    size := u32(local.type.size);

                    // Get local offset ptr
                    localPtr := luma_mem_ptr_offset(luma.sp0, int(local.offset));

                    // Copy from stack ptr
                    luma_stack_copy_to(&luma.sp, localPtr, size);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        builder: strings.Builder;
                        luma_debug_x(fmt.sbprintf(&builder, "Execute St_Loc [%d]", index), localPtr, size, 0);
                    }
                }

            // Argument
            case .Ld_Arg_0:
                {
                    // Get method signature
                    param := (cast(^LumaMethodHandle)luma.call).signature.parameterTypes[0];

                    // Get local size
                    size := u32(param.type.size);
                    
                    // Get stack token
                    token := luma_stacktoken_from_typecode(param.type.code);

                    // Get local offset ptr
                    localPtr := luma.sp0;
                    luma_mem_ptr_sub(&localPtr, uint(param.offset));

                    // Copy to stack ptr
                    luma_stack_copy_from(&luma.sp, localPtr, size, token);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_x("Execute Ld_Arg_0", luma.sp, size, -int(size + 1));
                    }
                }

            // Arithmetic
            case .Add:
                {
                    // Fetch op code extra flags
                    opFlags := transmute(LumaOpCodeFlags)luma_fetchdecode_8(&luma.pc);

                    // Fetch stack token
                    token := luma_stacktoken_peek(luma.sp);

                    #partial switch token
                    {
                        // I32
                        case .U32: 
                        {
                            // Fetch a and b values from stack - pop in reverse
                            b := luma_stack_fetch_U32(&luma.sp);
                            a := luma_stack_fetch_U32(&luma.sp);

                            // Perform add
                            sum, overflow := luma_add_U32(a, b, opFlags);

                            // Push back to stack
                            luma_stack_write_U32(&luma.sp, sum);

                            // Debug exec
                            when ODIN_DEBUG == true
                            {
                                luma_debug_32("Execute Add [U32]", luma.sp, -5);
                            }
                        }
                    }
                }
            case .Sub:
                {
                    // Fetch op code extra flags
                    opFlags := transmute(LumaOpCodeFlags)luma_fetchdecode_8(&luma.pc);

                    // Fetch stack token
                    token := luma_stacktoken_peek(luma.sp);

                    #partial switch token
                    {
                        // I32
                        case .U32: 
                        {
                            // Fetch a and b values from stack - pop in reverse
                            b := luma_stack_fetch_U32(&luma.sp);
                            a := luma_stack_fetch_U32(&luma.sp);

                            // Perform subtract
                            sum, overflow := luma_sub_U32(a, b, opFlags);

                            // Check for overflow
                            if overflow == true
                            {
                                // Raise exception
                                break;
                            }

                            // Push to stack
                            luma_stack_write_U32(&luma.sp, sum);

                            // Debug exec
                            when ODIN_DEBUG == true
                            {
                                luma_debug_32("Execute Sub [U32]", luma.sp, -5);
                            }
                        }
                    }
                }
            case .Cmp_L:
                {
                    // Fetch op code extra flags
                    opFlags := transmute(LumaOpCodeFlags)luma_fetchdecode_8(&luma.pc);

                    // Fetch stack token
                    token := luma_stacktoken_peek(luma.sp);

                    #partial switch token
                    {
                        // I32
                        case .U32:
                            {
                                // Fetch a and b values from stack - pop in reverse
                                b := luma_stack_fetch_U32(&luma.sp);
                                a := luma_stack_fetch_U32(&luma.sp);

                                // Check for unsigned
                                if LumaOpCodeFlags.Unsigned in opFlags
                                {
                                    // Perform unsigned comparison and push result
                                    luma_stack_write_U32(&luma.sp, a < b ? 1 : 0);
                                }
                                else
                                {
                                    // Perform signed comparison and push result
                                    luma_stack_write_U32(&luma.sp, u32(i32(i32(a) < i32(b) ? 1 : 0)));
                                }                                

                                // Debug exec
                                when ODIN_DEBUG == true
                                {
                                    luma_debug_32("Execute Cmp_L [U32]", luma.sp, -5);
                                }
                            }
                    }
                }
            case .Cmp_Eq:
                {
                    // Fetch op code extra flags
                    opFlags := transmute(LumaOpCodeFlags)luma_fetchdecode_8(&luma.pc);

                    // Fetch stack token
                    token := luma_stacktoken_peek(luma.sp);

                    #partial switch token
                    {
                        // I32
                        case .U32:
                            {
                                // Fetch a and b values from stack - pop in reverse
                                b := luma_stack_fetch_U32(&luma.sp);
                                a := luma_stack_fetch_U32(&luma.sp);

                                // Check for unsigned
                                if LumaOpCodeFlags.Unsigned in opFlags
                                {
                                    // Perform unsigned comparison and push result
                                    luma_stack_write_U32(&luma.sp, a == b ? 1 : 0);
                                }
                                else
                                {
                                    // Perform unsigned comparison and push result
                                    luma_stack_write_U32(&luma.sp, u32(i32(i32(a) == i32(b) ? 1 : 0)));
                                }

                                // Debug exec
                                when ODIN_DEBUG == true
                                {
                                    luma_debug_32("Execute Cmp_Eq [U32]", luma.sp, -5);
                                }
                            }
                    }
                }

            // Arrays
            // case .Ld_Elem:
            //     {
            //         // Pop index
            //         index := luma_fetchd_32(&luma.sp);

            //         // Pop array address
            //         arrAddr := luma_fetchd_ptr(&luma.sp);

            //         // Get element handle
            //         memHandle := luma_mem_gethandle(arrAddr);

            //         // Get element size
            //         size := memHandle.type.size;

            //         // Get element address
            //         elemAddr := &(cast(^[]u8)arrAddr)[index * size];

            //         // Copy to stack
            //         luma_copyi_x(elemAddr, &luma.sp, size);

            //         fmt.println("Executed Ld_Elem: ", luma_fetch_32(cast(rawptr)(uintptr(luma.sp) - 4)));
            //     }

            // Jump
            case .Jmp:
                {
                    // Fetch offset
                    offset := int(luma_fetchdecode_16(&luma.pc));

                    // Jump to address
                    luma.pc = luma_mem_ptr_offset(luma.pc, offset);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        fmt.println("Execute Jmp: ", offset);
                    }
                }
            case .Jmp_1:
                {
                    // Fetch offset
                    offset := int(i16(luma_fetchdecode_16(&luma.pc)));

                    // Fetch value
                    val := luma_stack_fetch_U32(&luma.sp);

                    // Check condition
                    if val == 1
                    {
                        // Jump to address
                        luma.pc = luma_mem_ptr_offset(luma.pc, offset);
                    }

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        fmt.println("Execute Jmp_1: [", val == 1, "] ", offset);
                    }
                }
            case .Jmp_0:
                {
                    // Fetch offset
                    offset := int(i16(luma_fetchdecode_16(&luma.pc)));

                    // Fetch value
                    val := luma_stack_fetch_U32(&luma.sp);

                    // Check condition
                    if val == 0
                    {
                        // Jump to address
                        luma.pc = luma_mem_ptr_offset(luma.pc, offset);
                    }

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        fmt.println("Execute Jmp_0: [", val == 0, "] ", offset);
                    }
                }

            // Obj
            // case .NewArr_S:
            //     {
            //         // Fetch type code
            //         typeToken := luma_fetchi_32(&luma.pc);
            //         fmt.println("Type Token: ", typeToken);

            //         // Fetch size
            //         size := luma_fetchd_32(&luma.sp);
            //         fmt.println("Array Size: ", size);
                    
            //         // Get the type handle
            //         typeHandle := luma_type_getHandle(luma, typeToken);
            //         fmt.println("Type Code: ", typeHandle.code);
            //         fmt.println("Type Size: ", typeHandle.size);

            //         // Allocate the memory
            //         arr := luma_mem_allocarray(typeHandle, u64(size));

            //         // Push address to stack
            //         luma_writei_ptr(&luma.sp, &arr[0]);

            //         fmt.println("Executed NewArr_S: ", cast(^[]u8)arr);
            //     }
            case .Call:
                {
                    // Fetch method token
                    token := luma_fetchdecode_32(&luma.pc);

                    // Get method address
                    callAddr := luma.callAddr;

                    // Push stack base and pc
                    luma_stack_write_ptr(&luma.sp, luma.sp0);
                    luma_stack_write_ptr(&luma.sp, callAddr);

                    // Update stack pointer and pc
                    luma.sp0 = luma.sp;
                    luma.pc = callAddr;

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        fmt.println("Execute Call");
                    }
                }
            case .Ret:
                {
                    // // Unwind stack
                    // returnAddr := luma_stack_fetch_ptr(&luma.sp);
                    // returnS0 := luma_stack_fetch_ptr(&luma.sp);

                    // // Return to prior state
                    // luma.sp0 = returnS0;
                    // luma.pc = returnAddr;

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        fmt.println("Execute Ret");
                    }
                    return;
                }
        } // End switch
    }
}

luma_fetchdecode_8 :: proc(pc: ^rawptr) -> u8
{
    // Fetch val at ptr
    val := (cast(^u8)pc^)^;

    // Increment addr
    pc^ = cast(rawptr)(uintptr(pc^) + 1)
    return val;
}

luma_fetchdecode_16 :: proc(pc: ^rawptr) -> u16
{
    // Fetch val at ptr
    val := (cast(^u16)pc^)^;

    // Increment addr
    pc^ = cast(rawptr)(uintptr(pc^) + 2)
    return val;
}

luma_fetchdecode_32 :: proc(pc: ^rawptr) -> u32
{
    // Fetch val at ptr
    val := (cast(^u32)pc^)^;

    // Increment addr
    pc^ = cast(rawptr)(uintptr(pc^) + 4)
    return val;
}

luma_fetchdecode_64 :: proc(pc: ^rawptr) -> u64
{
    // Fetch val at ptr
    val := (cast(^u64)pc^)^;

    // Increment addr
    pc^ = cast(rawptr)(uintptr(pc^) + 8)
    return val;
}

// Debug helper
when ODIN_DEBUG == true
{
    // DEBUG
    luma_debug_8 :: proc(label: string, addr: rawptr, offset: int = 0)
    {
        fmt.println(label, " :", luma_fetch_8(cast(rawptr)(uintptr(addr) + uintptr(offset))));
    }

    luma_debug_16 :: proc(label: string, addr: rawptr, offset: int = 0)
    {
        fmt.println(label, " :", luma_fetch_16(cast(rawptr)(uintptr(addr) + uintptr(offset))));
    }

    luma_debug_32 :: proc(label: string, addr: rawptr, offset: int = 0)
    {
        fmt.println(label, " :", luma_fetch_32(cast(rawptr)(uintptr(addr) + uintptr(offset))));
    }

    luma_debug_64 :: proc(label: string, addr: rawptr, offset: int = 0)
    {
        fmt.println(label, " :", luma_fetch_64(cast(rawptr)(uintptr(addr) + uintptr(offset))));
    }

    luma_debug_x :: proc(label: string, addr: rawptr, size: u32, offset: int = 0)
    {
        switch size
        {
            case 1: luma_debug_8(label, addr, offset);
            case 2: luma_debug_16(label, addr, offset);
            case 4: luma_debug_32(label, addr, offset);
            case 8: luma_debug_64(label, addr, offset);
        }
    }
}