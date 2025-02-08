package LumaVM

import "core:fmt"
import "core:strings"
import "core:mem"

@private
luma_execute_bytecode :: proc(luma: ^LumaState) // instructions: []u8, stack: []u8, stackOffset: u32)
{
    // Execution loop
    for i := 0; i < 50; i += 1
    {
        //fmt.printf("Raw bytecode: %x\n", (cast(^u8)luma.pc)^);

        // Get op code
        op: LumaOpCode = cast(LumaOpCode)luma_fetchi_8(&luma.pc);

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
                    constVal := luma_fetchi_8(&luma.pc);

                    // Write to stack as 32-bit
                    luma_writei_32(&luma.sp, u32(constVal));

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_32("Execute Ld_I1", luma.sp, -4);
                    }
                }
            case .Ld_I2:
                {
                    // Fetch value from instruction
                    constVal := luma_fetchi_16(&luma.pc);

                    // Write to stack as 32-bit
                    luma_writei_32(&luma.sp, u32(constVal));

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_32("Execute Ld_I2", luma.sp, -4);
                    }
                }
            case .Ld_I4:
                {
                    // Fetch value from instruction
                    constVal := luma_fetchi_32(&luma.pc);

                    // Write to stack
                    luma_writei_32(&luma.sp, constVal);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_32("Execute Ld_I4", luma.sp, -4);
                    }
                }
            case .Ld_I8:
                {
                        // Fetch value from instruction
                    constVal := luma_fetchi_64(&luma.pc);

                    // Write to stack
                    luma_writei_64(&luma.sp, constVal);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_64("Execute Ld_I8", luma.sp, -8);
                    }
                }

            // Fixed Constants
            case .Ld_I4_0:
                {
                    // Write to stack
                    luma_writei_32(&luma.sp, 0);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_32("Execute Ld_I4_0", luma.sp, -4);
                    }
                }
            case .Ld_I4_1:
                {
                    // Write to stack
                    luma_writei_32(&luma.sp, 1);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_32("Execute Ld_I4_1", luma.sp, -4);
                    }
                }
            case .Ld_I4_M1:
                {
                    // Write to stack
                    m1: i32 = -1;
                    luma_writei_32(&luma.sp, u32(m1));

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_32("Execute Ld_I4_M1", luma.sp, -4);
                    }
                }

            // Locals
            case .Ld_Loc_0:
                {
                    // Get method signature
                    signature := (cast(^LumaMethodHandle)luma.call).signature;

                    // Get local size
                    size := u32(signature.localTypes[0].size);

                    // Copy to stack ptr
                    luma_copyi_x(luma.sp0, &luma.sp, size);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_x("Execute Ld_Loc_0", luma.sp, size, -int(size));
                    }
                }
            case .Ld_Loc_1:
                {
                    // Get method signature
                    signature := (cast(^LumaMethodHandle)luma.call).signature;

                    // Get local size
                    size := u32(signature.localTypes[1].size);

                    // Get local offset ptr
                    localPtr := luma_mem_ptr_offset(luma.sp0, int(signature.localTypes[1].offset));

                    // Copy to stack ptr
                    luma_copyi_x(localPtr, &luma.sp, size);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_x("Execute Ld_Loc_1", luma.sp, size, -int(size));
                    }
                }
            case .Ld_Loc_2:
                {
                    // Get method signature
                    signature := (cast(^LumaMethodHandle)luma.call).signature;

                    // Get local size
                    size := u32(signature.localTypes[2].size);

                    // Get local offset ptr
                    localPtr := luma_mem_ptr_offset(luma.sp0, int(signature.localTypes[2].offset));

                    // Copy to stack ptr
                    luma_copyi_x(localPtr, &luma.sp, size);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_x("Execute Ld_Loc_2", luma.sp, size, -int(size));
                    }
                }
            case .Ld_Loc:
                {
                    // Fetch index
                    index := luma_fetchi_16(&luma.pc);

                    // Get method signature
                    signature := (cast(^LumaMethodHandle)luma.call).signature;

                    // Get local size
                    size := u32(signature.localTypes[index].size);

                    // Get local offset ptr
                    localPtr := luma_mem_ptr_offset(luma.sp0, int(signature.localTypes[index].offset));

                    // Copy to stack ptr
                    luma_copyi_x(localPtr, &luma.sp, size);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        builder: strings.Builder;
                        luma_debug_x(fmt.sbprintf(&builder, "Execute Ld_Loc [%d]", index), luma.sp, size, -int(size));
                    }
                }
            case .St_Loc_0:
                {
                    // Get method signature
                    signature := (cast(^LumaMethodHandle)luma.call).signature;

                    // Get local size
                    size := u32(signature.localTypes[0].size);

                    // Copy from stack ptr
                    luma_copyd_x(&luma.sp, luma.sp0, size);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_x("Execute St_Loc_0", luma.sp0, size, 0);
                    }
                }
            case .St_Loc_1:
                {
                    // Get method signature
                    signature := (cast(^LumaMethodHandle)luma.call).signature;

                    // Get local size
                    size := u32(signature.localTypes[1].size);

                    // Get local offset ptr
                    localPtr := luma_mem_ptr_offset(luma.sp0, int(signature.localTypes[1].offset));

                    // Copy from stack ptr
                    luma_copyd_x(&luma.sp, localPtr, size);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_x("Execute St_Loc_1", localPtr, size, 0);
                    }
                }
            case .St_Loc_2:
                {
                    // Get method signature
                    signature := (cast(^LumaMethodHandle)luma.call).signature;

                    // Get local size
                    size := u32(signature.localTypes[2].size);

                    // Get local offset ptr
                    localPtr := luma_mem_ptr_offset(luma.sp0, int(signature.localTypes[2].offset));

                    // Copy from stack ptr
                    luma_copyd_x(&luma.sp, localPtr, size);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_x("Execute St_Loc_2", localPtr, size, 0);
                    }
                }
            case .St_Loc:
                {
                    // Fetch index
                    index := luma_fetchi_16(&luma.pc);

                    // Get method signature
                    signature := (cast(^LumaMethodHandle)luma.call).signature;

                    // Get local size
                    size := u32(signature.localTypes[index].size);

                    // Get local offset ptr
                    localPtr := luma_mem_ptr_offset(luma.sp0, int(signature.localTypes[index].offset));

                    // Copy from stack ptr
                    luma_copyd_x(&luma.sp, localPtr, size);

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
                    signature := (cast(^LumaMethodHandle)luma.call).signature;

                    // Get local size
                    size := u32(signature.parameterTypes[0].size);

                    // Get local offset ptr
                    localPtr := luma.sp0;// luma_mem_ptr_offset(luma.sp0, -int(signature.localTypes[1].offset));
                    luma_mem_ptr_sub(&localPtr, uint(signature.parameterTypes[0].offset));

                    // Copy to stack ptr
                    luma_copyi_x(localPtr, &luma.sp, size);

                    // Debug exec
                    when ODIN_DEBUG == true
                    {
                        luma_debug_x("Execute Ld_Arg_0", luma.sp, size, -int(size));
                    }
                }

            // Arithmetic
            case .Add:
                {
                    // Fetch type code
                    typeCode := LumaMetaTypeCode(luma_fetchi_8(&luma.pc));

                    #partial switch typeCode
                    {
                        // I32
                        case .I32: 
                        {
                            // Fetch a and v values from stack
                            a := i32(luma_fetchd_32(&luma.sp));
                            b := i32(luma_fetchd_32(&luma.sp));

                            // Perform add and push back to stack
                            luma_writei_32(&luma.sp, u32(b + a));

                            fmt.println("Execute Add: ", luma_fetch_32(cast(rawptr)(uintptr(luma.sp) - 4)));
                        }
                    }
                }
            case .Sub:
                {
                    // Fetch type code
                    typeCode := LumaMetaTypeCode(luma_fetchi_8(&luma.pc));

                    #partial switch typeCode
                    {
                        // I32
                        case .I32: 
                        {
                            // Fetch a and v values from stack
                            a := i32(luma_fetchd_32(&luma.sp));
                            b := i32(luma_fetchd_32(&luma.sp));

                            // Perform add and push back to stack
                            luma_writei_32(&luma.sp, u32(b - a));

                            fmt.println("Execute Sub: ", luma_fetch_32(cast(rawptr)(uintptr(luma.sp) - 4)));
                        }
                    }
                }
            case .Cmp_L:
                {
                    // Fetch type code
                    typeCode := LumaMetaTypeCode(luma_fetchi_8(&luma.pc));

                    #partial switch typeCode
                    {
                        // I32
                        case .I32:
                            {
                                // Fetch a and v values from stack
                                a := i32(luma_fetchd_32(&luma.sp));
                                b := i32(luma_fetchd_32(&luma.sp));

                                // Perform comparison and push result
                                luma_writei_32(&luma.sp, u32(b < a ? 1 : 0));

                                // Debug exec
                                when ODIN_DEBUG == true
                                {
                                    luma_debug_32("Execute Cmp_L [I32]", luma.sp, -4);
                                }
                            }
                    }
                }
            case .Cmp_Eq:
                {
                    // Fetch type code
                    typeCode := LumaMetaTypeCode(luma_fetchi_8(&luma.pc));

                    #partial switch typeCode
                    {
                        // I32
                        case .I32:
                            {
                                // Fetch a and v values from stack
                                a := i32(luma_fetchd_32(&luma.sp));
                                b := i32(luma_fetchd_32(&luma.sp));

                                // Perform comparison and push result
                                luma_writei_32(&luma.sp, u32(b == a ? 1 : 0));

                                // Debug exec
                                when ODIN_DEBUG == true
                                {
                                    luma_debug_32("Execute Cmp_Eq [I32]", luma.sp, -4);
                                }
                            }
                    }
                }

            // Arrays
            case .Ld_Elem:
                {
                    // Pop index
                    index := luma_fetchd_32(&luma.sp);

                    // Pop array address
                    arrAddr := luma_fetchd_ptr(&luma.sp);

                    // Get element handle
                    memHandle := luma_mem_gethandle(arrAddr);

                    // Get element size
                    size := memHandle.type.size;

                    // Get element address
                    elemAddr := &(cast(^[]u8)arrAddr)[index * size];

                    // Copy to stack
                    luma_copyi_x(elemAddr, &luma.sp, size);

                    fmt.println("Executed Ld_Elem: ", luma_fetch_32(cast(rawptr)(uintptr(luma.sp) - 4)));
                }

            // Jump
            case .Jmp:
                {
                    // Fetch offset
                    offset := int(luma_fetchi_16(&luma.pc));

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
                    offset := int(i16(luma_fetchi_16(&luma.pc)));

                    // Fetch value
                    val := luma_fetchd_32(&luma.sp);

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
                    offset := int(i16(luma_fetchi_16(&luma.pc)));

                    // Fetch value
                    val := luma_fetchd_32(&luma.sp);

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
            case .NewArr_S:
                {
                    // Fetch type code
                    typeToken := luma_fetchi_32(&luma.pc);
                    fmt.println("Type Token: ", typeToken);

                    // Fetch size
                    size := luma_fetchd_32(&luma.sp);
                    fmt.println("Array Size: ", size);
                    
                    // Get the type handle
                    typeHandle := luma_type_getHandle(luma, typeToken);
                    fmt.println("Type Code: ", typeHandle.code);
                    fmt.println("Type Size: ", typeHandle.size);

                    // Allocate the memory
                    arr := luma_mem_allocarray(typeHandle, u64(size));

                    // Push address to stack
                    luma_writei_ptr(&luma.sp, &arr[0]);

                    fmt.println("Executed NewArr_S: ", cast(^[]u8)arr);
                }
            case .Call:
                {
                    // Fetch method token
                    token := luma_fetchd_32(&luma.pc);

                    // Get method address
                    callAddr := luma.callAddr;

                    // Push stack base and pc
                    luma_writei_ptr(&luma.sp, luma.sp0);
                    luma_writei_ptr(&luma.sp, callAddr);

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
                    // Unwind stack
                    returnAddr := luma_fetchd_ptr(&luma.sp);
                    returnS0 := luma_fetchd_ptr(&luma.sp);

                    // Return to prior state
                    luma.sp0 = returnS0;
                    luma.pc = returnAddr;

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