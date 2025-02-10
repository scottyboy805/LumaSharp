package LumaVM

import "core:fmt"

main :: proc() {
    
    // Initialize luma
    state := luma_init();
    defer luma_cleanup(state);

    callHandle := LumaMethodHandle{};
    callHandle.localsCount = 2;
    callHandle.localTypes = make([]LumaStackHandle, 2);
    callHandle.localTypes[0].type.size = 4;
    callHandle.localTypes[1].offset = 1;
    callHandle.localTypes[1].type.size = 4;

    bytecode: []u8 = 
    {
        // Locals
        0x00, 0x00, 0x00, 0x00,

        0x00,       // NOP
        0x10, 128,  // Ld_I1 128
        0x27,       // St_Loc_1
        0x20,       // Ld_Loc_0
        0x10, 22,   // Ld_I1 22
        0x71, 8,    // Add I32
        0x10, 5,    // Ld_I1 5
        //0xF4, 8, 0, 0, 0,    // NewArr_S I32
    }
    state.call = &callHandle;
    state.pc = &bytecode[4];


    fibCall := LumaMethodHandle{};
    // Arg
    fibCall.signature.parameterTypes = make([]LumaStackHandle, 1);
    fibCall.signature.parameterCount = 1;
    fibCall.signature.parameterTypes[0].type.code = .I32;
    fibCall.signature.parameterTypes[0].type.size = 4;
    fibCall.signature.parameterTypes[0].offset = 4;

    // Loc
    fibCall.localTypes = make([]LumaStackHandle, 6);
    fibCall.localsCount = 6;
    fibCall.localTypes[0].type.code = .I32;
    fibCall.localTypes[0].type.size = 4;
    fibCall.localTypes[1].offset = 4;
    fibCall.localTypes[1].type.code = .I32;
    fibCall.localTypes[1].type.size = 4;
    fibCall.localTypes[2].offset = 8;
    fibCall.localTypes[2].type.code = .I32;
    fibCall.localTypes[2].type.size = 4;
    fibCall.localTypes[3].offset = 12;
    fibCall.localTypes[3].type.code = .I32;
    fibCall.localTypes[3].type.size = 4;
    fibCall.localTypes[4].offset = 16;
    fibCall.localTypes[4].type.code = .I32;
    fibCall.localTypes[4].type.size = 4;
    fibCall.localTypes[5].offset = 20;
    fibCall.localTypes[5].type.code = .I32;
    fibCall.localTypes[5].type.size = 4;


    off: i8 = -33;
    fibBytecode: []u8 =
    {
        0x00,               // Nop
        0x18,               // Ld_I4_0
        0x27,               // St_Loc_0
        0x19,               // Ld_I4_1
        0x28,               // St_Loc_1
        0x18,               // Ld_I4_0
        0x29,               // St_Loc_2
        0xB9, 20, 0,        // Jmp

        // Jump target:
        0x00,               // Nop
        0x20,               // Ld_Loc_0
        0x2A, 3, 0,         // St_Loc, 3 
        0x21,               // Ld_Loc_1
        0x27,               // St_Loc_0
        0x23, 3, 0,         // Ld_Loc, 3
        0x21,               // Ld_Loc_1
        0x71, 0,            // Add, 0
        0x28,               // St_Loc_1
        0x00,               // Nop
        0x22,               // Ld_Loc_2
        0x19,               // Ld_I4_1
        0x71, 0,            // Add, 0
        0x29,               // St_Loc_2

        // Jump target
        0x22,               // Ld_Loc_2
        0x30,               // Ld_Arg_0
        0x95, 0,            // Cmp_L, 0
        0x2A, 4, 0,         // St_loc, 4
        0x23, 4, 0,         // Ld_Loc, 4
        0xB3, 0xDF, 0xFF,         // Jmp_1 -31

        0x20,               // Ld_Loc_0
        0x2A, 5, 0,         // St_Loc, 5
        0x23, 5, 0,         // Ld_Loc, 5
        0xFD,               // ret
    }

    fibRecursiveBytecode: []u8 =
    {
        0x00,               // Nop
        0x30,               // Ld_Arg_0
        0x18,               // Ld_I4_0
        0x99, 8,            // Cmp_Eq, I32       
        0x27,               // St_Loc_0
        0x20,               // Ld_Loc_0       
        0xB4, 5, 0x00,      // Jmp_0, 5
        0x18,               // Ld_I4_0
        0x28,               // St_Loc_1
        0xB9, 37, 0x00,     // Jmp, 37

        // Jump target
        0x30,               // Ld_Arg_0
        0x19,               // Ld_I4_1
        0x99, 8,            // Cmp_Eq, I32
        0x29,               // St_Loc_2
        0x22,               // Ld_Loc_2
        0xB4, 5, 0x00,      // Jmp_0, 5
        0x19,               // Ld_I4_1
        0x28,               // St_Loc_1
        0xB9, 23, 0x00,     // Jmp, 23

        // Jump target
        0x00,               // Nop
        0x30,               // Ld_Arg_0,
        0x12, 2, 0, 0, 0,   // Ld_I4, 2
        0x75, 8,            // Sub, I32
        0xF5, 0x00, 0x00, 0x00, 0x00,   // Call, token
        0x30,               // Ld_Arg_0
        0x19,               // Ld_I4_1
        0x75, 8,            // Sub, I32
        0xF5, 0x00, 0x00, 0x00, 0x00,   // Call, token    
        0x71, 8,            // Add, I32
        0x28,               // St_Loc_1
        0x21,               // Ld_Loc_1
        0xFD,               // Ret
    }

    // Pass arg
    state.stack[96] = 9;

    state.call = &fibCall;
    // state.pc = &fibBytecode[0]; //&fibRecursiveBytecode[0]; //&fibBytecode[0];
    // state.callAddr = &fibRecursiveBytecode[0];

    fmt.println("__Start__");
    luma_execute_bytecode(state, &fibBytecode[0]);


    fmt.println("Stack size: ", len(state.stack));

    //luma_execute_bytecode(nil, nil, 0)
}