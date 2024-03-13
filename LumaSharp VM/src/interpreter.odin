package LumaVM

import "core:fmt"

@private
luma_execute_bytecode :: proc(instructions: []u8, stack: []u8, stackOffset: u32)
{
    // Reset prt
    instuctionPtr := 0
    stackPtr := stackOffset

    // Execution loop
    for
    {
        // Get op code
        op: LumaOpCode = cast(LumaOpCode)instructions[instuctionPtr]
        instuctionPtr += 1

        // Evaluate op code
        #partial switch op
        {
            case .Nop:
            case: fmt.printf("Op code not implemented: %v", op)

            // Constants
            case .Ld_I1:
                {
                    // Get stack insert pointer
                    stackDestPtr := cast(^i32)&stack[stackPtr]

                    // Move 8 bit and push as 32 bit onto the stack
                    stackDestPtr^ = cast(i32)instructions[instuctionPtr]

                    // Increment offsets
                    instuctionPtr += size_of(i8)
                    stackPtr += size_of(i32)
                }

            case .Ld_I2:
                {
                    // Get stack insert pointer
                    stackDestPtr: ^i32 = cast(^i32)&stack[stackPtr]

                    // Move 16 bit and push as 32 bit onto the stack
                    stackDestPtr^ = cast(i32)instructions[instuctionPtr]

                    // Increment offsets
                    instuctionPtr += size_of(i16)
                    stackPtr += size_of(i32)
                }

            case .Ld_I4:
                {
                    // Get stack insert pointer
                    stackDestPtr: ^i32 = cast(^i32)&stack[stackPtr]

                    // Move 32 bit and pust as 32 bit onto the stack
                    stackDestPtr^ = cast(i32)instructions[instuctionPtr]

                    // Increment offsets
                    instuctionPtr += size_of(i32)
                    stackPtr += size_of(i32)
                }
        } // End switch
    }
}