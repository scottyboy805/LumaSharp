package LumaVM

LumaStackToken :: enum u8
{
    Invalid = 0,
    I32,
    U32,
    I64,
    U64,
    F32,
    F64,
    Addr,
    CallHead,
}

luma_stack_write_8 :: proc(sp: ^rawptr, value: u8)
{
    // Promote to 32-bit
    (cast(^i32)sp^)^ = i32(value);

    // Append stack token
    (cast(^LumaStackToken)(uintptr(sp^) + uintptr(4)))^ = .I32;

    // Increment sp
    sp^ = cast(rawptr)(uintptr(sp^) + size_of(u32) + 1);   
}

luma_stack_write_16 :: proc(sp: ^rawptr, value: u16)
{
    // Promote to 32-bit
    (cast(^i32)sp^)^ = i32(value);

    // Append stack token
    (cast(^LumaStackToken)(uintptr(sp^) + uintptr(4)))^ = .I32;

    // Increment sp
    sp^ = cast(rawptr)(uintptr(sp^) + size_of(u32) + 1);   
}

luma_stack_write_32 :: proc(sp: ^rawptr, value: u32)
{
    // Write to 32-bit
    (cast(^i32)sp^)^ = i32(value);

    // Append stack token
    (cast(^LumaStackToken)(uintptr(sp^) + uintptr(4)))^ = .I32;

    // Increment sp
    sp^ = cast(rawptr)(uintptr(sp^) + size_of(u32) + 1);   
}

luma_stack_write_64 :: proc(sp: ^rawptr, value: u64)
{
    // Write to 32-bit
    (cast(^i64)sp^)^ = i64(value);

    // Append stack token
    (cast(^LumaStackToken)(uintptr(sp^) + size_of(u64)))^ = .I64;

    // Increment sp
    sp^ = cast(rawptr)(uintptr(sp^) + size_of(u64) + 1);   
}

luma_stack_write_ptr :: proc(sp: ^rawptr, value: rawptr)
{
    // Write to 32-bit
    (cast(^rawptr)sp^)^ = rawptr(value);

    // Append stack token
    (cast(^LumaStackToken)(uintptr(sp^) + size_of(rawptr)))^ = .Addr;

    // Increment sp
    sp^ = cast(rawptr)(uintptr(sp^) + size_of(rawptr) + 1);   
}