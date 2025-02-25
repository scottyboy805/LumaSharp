package LumaVM

import "core:mem"

LumaState :: struct
{
    types: []LumaTypeHandle,    // Built-in types

    call: rawptr,               // Address of the method being called
    pc: rawptr,                 // Program counter
    sp: rawptr,                 // Stack pointer
    spa: rawptr,                // Stack pointer args
    spl: rawptr,                // Stack pointer locals

    stack: []u8,                // Stack    

    callAddr: rawptr,   // TESTING
}

luma_init :: proc(stackSize: u32 = 4096) -> ^LumaState
{
    // Create state
    state := new(LumaState);

    //state := new(LumaState{        
        // Initialize types
        state.types = []LumaTypeHandle{
            LumaTypeHandle{ code = .Void, size = 0 },                       // Void
            LumaTypeHandle{ code = .Any, size = size_of(rawptr) },          // Any
            LumaTypeHandle{ code = .Bool, size = size_of(bool) },           // Bool
            LumaTypeHandle{ code = .Char, size = size_of(rune) },           // Char
            LumaTypeHandle{ code = .I8, size = size_of(i8) },               // I8
            LumaTypeHandle{ code = .U8, size = size_of(u8) },               // U8
            LumaTypeHandle{ code = .I16, size = size_of(i16) },             // I16
            LumaTypeHandle{ code = .U16, size = size_of(u16) },             // U16
            LumaTypeHandle{ code = .I32, size = size_of(i32) },             // I32
            LumaTypeHandle{ code = .U32, size = size_of(u32) },             // U32
            LumaTypeHandle{ code = .I64, size = size_of(i64) },             // I64
            LumaTypeHandle{ code = .U64, size = size_of(u64) },             // U64
            LumaTypeHandle{ code = .F32, size = size_of(f32) },           // Float
            LumaTypeHandle{ code = .F64, size = size_of(f64) },          // Double
        }
    //});

    // Initialize stack
    state.stack = make([]u8, stackSize);

    // Init stack pointer
    
    //state.spa = &state.stack[100];
    state.sp = &state.stack[100];

    return state;
}

luma_cleanup :: proc(luma: ^LumaState)
{
    // Free stack

    // Free the memory
    //mem.free(luma);
}