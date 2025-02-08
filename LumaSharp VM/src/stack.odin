package LumaVM

import "core:mem"
import "base:runtime"

LumaStackToken :: enum u8
{
    Invalid = 0,
    U32,
    U64,
    F32,
    F64,
    Addr,
    CallHead,
}


// Write
luma_stack_write_U8 :: proc(sp: ^rawptr, value: u8)
{
    // Promote to 32-bit
    (cast(^u32)sp^)^ = u32(value);

    // Append stack token
    (cast(^LumaStackToken)(uintptr(sp^) + size_of(u32)))^ = .U32;

    // Increment sp
    sp^ = cast(rawptr)(uintptr(sp^) + size_of(u32) + 1);   
}

luma_stack_write_U16 :: proc(sp: ^rawptr, value: u16)
{
    // Promote to 32-bit
    (cast(^u32)sp^)^ = u32(value);

    // Append stack token
    (cast(^LumaStackToken)(uintptr(sp^) + size_of(u32)))^ = .U32;

    // Increment sp
    sp^ = cast(rawptr)(uintptr(sp^) + size_of(u32) + 1);   
}

luma_stack_write_U32 :: proc(sp: ^rawptr, value: u32)
{
    // Write to 32-bit
    (cast(^u32)sp^)^ = value;

    // Append stack token
    (cast(^LumaStackToken)(uintptr(sp^) + size_of(u32)))^ = .U32;

    // Increment sp
    sp^ = cast(rawptr)(uintptr(sp^) + size_of(u32) + 1);   
}

luma_stack_write_U64 :: proc(sp: ^rawptr, value: u64)
{
    // Write to 32-bit
    (cast(^u64)sp^)^ = value;

    // Append stack token
    (cast(^LumaStackToken)(uintptr(sp^) + size_of(u64)))^ = .U64;

    // Increment sp
    sp^ = cast(rawptr)(uintptr(sp^) + size_of(u64) + 1);   
}

luma_stack_write_F32 :: proc(sp: ^rawptr, value: f32)
{
    // Write to 32-bit
    (cast(^f32)sp^)^ = value;

    // Append stack token
    (cast(^LumaStackToken)(uintptr(sp^) + size_of(f32)))^ = .F32;

    // Increment sp
    sp^ = cast(rawptr)(uintptr(sp^) + size_of(f32) + 1);   
}

luma_stack_write_F64 :: proc(sp: ^rawptr, value: f64)
{
    // Write to 32-bit
    (cast(^f64)sp^)^ = value;

    // Append stack token
    (cast(^LumaStackToken)(uintptr(sp^) + size_of(f64)))^ = .F64;

    // Increment sp
    sp^ = cast(rawptr)(uintptr(sp^) + size_of(f64) + 1);   
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


// Fetch
luma_stack_fetch_U8 :: proc(sp: ^rawptr) -> u8
{
    // Check stack token
    assert((cast(^LumaStackToken)(uintptr(sp^) - size_of(LumaStackToken)))^ == .U32);

    // Decrement sp
    sp^ = cast(rawptr)(uintptr(sp^) - size_of(u32) - 1);

    // Fetch value with demote
    return cast(u8)(cast(^u32)sp^)^;
}

luma_stack_fetch_U16 :: proc(sp: ^rawptr) -> u16
{
    // Check stack token
    assert((cast(^LumaStackToken)(uintptr(sp^) - size_of(LumaStackToken)))^ == .U32);

    // Decrement sp
    sp^ = cast(rawptr)(uintptr(sp^) - size_of(u32) - 1);

    // Fetch value with demote
    return cast(u16)(cast(^u32)sp^)^;
}

luma_stack_fetch_U32 :: proc(sp: ^rawptr) -> u32
{
    // Check stack token
    assert((cast(^LumaStackToken)(uintptr(sp^) - size_of(LumaStackToken)))^ == .U32);

    // Decrement sp
    sp^ = cast(rawptr)(uintptr(sp^) - size_of(u32) - 1);

    // Fetch value with demote
    return (cast(^u32)sp^)^;
}

luma_stack_fetch_U64 :: proc(sp: ^rawptr) -> u64
{
    // Check stack token
    assert((cast(^LumaStackToken)(uintptr(sp^) - size_of(LumaStackToken)))^ == .U64);

    // Decrement sp
    sp^ = cast(rawptr)(uintptr(sp^) - size_of(u64) - 1);

    // Fetch value with demote
    return (cast(^u64)sp^)^;
}

luma_stack_fetch_F32 :: proc(sp: ^rawptr) -> f32
{
    // Check stack token
    assert((cast(^LumaStackToken)(uintptr(sp^) - size_of(LumaStackToken)))^ == .F32);

    // Decrement sp
    sp^ = cast(rawptr)(uintptr(sp^) - size_of(f32) - 1);

    // Fetch value with demote
    return (cast(^f32)sp^)^;
}

luma_stack_fetch_F64 :: proc(sp: ^rawptr) -> f64
{
    // Check stack token
    assert((cast(^LumaStackToken)(uintptr(sp^) - size_of(LumaStackToken)))^ == .F64);

    // Decrement sp
    sp^ = cast(rawptr)(uintptr(sp^) - size_of(f64) - 1);

    // Fetch value with demote
    return (cast(^f64)sp^)^;
}

luma_stack_fetch_ptr :: proc(sp: ^rawptr) -> rawptr
{
    // Check stack token
    assert((cast(^LumaStackToken)(uintptr(sp^) - size_of(LumaStackToken)))^ == .Addr);

    // Decrement sp
    sp^ = cast(rawptr)(uintptr(sp^) - size_of(rawptr) - 1);

    // Fetch value with demote
    return (cast(^rawptr)sp^)^;
}

// Copy
/// Copy a value of specified size from the stack to the target address.
/// Handles demoting values less than 32-bit to fit in the correct size slot.
luma_stack_copy_to :: proc(sp: ^rawptr, dstAddr: rawptr, size: u32)
{
    // Decrement sp
    sp^ = cast(rawptr)(uintptr(sp^) - uintptr(size) - 1);

    // Check for demote
    switch size
    {
        // U8 demote
        case 1:
            {
                // Copy 32-bit value from stack and demote 8-bit
                (cast(^u8)dstAddr)^ = cast(u8)(cast(^u32)sp)^;
            }
        // U16 demote
        case 2:
            {
                // Copy 32-bit value from stack and demote 16-bit
                (cast(^u16)dstAddr)^ = cast(u16)(cast(^u32)sp)^;
            }
        // All other types
        case:
            {
                // Copy memory
                mem.copy(dstAddr, sp^, int(size));
            }
    }
}

luma_stack_copy_from :: proc(sp: ^rawptr, srcAddr: rawptr, size: u32, token: LumaStackToken)
{
    // Check for promote
    switch size
    {
        // U8 promote
        case 1:
            {
                // Copy 8-bit value from address and promote to 32-bit
                (cast(^u32)sp)^ = cast(u32)(cast(^u8)srcAddr)^;
            }
        // U16 promote
        case 2:
            {
                // Copy 16-bit value from address and promote to 32-bit
                (cast(^u32)sp)^ = cast(u32)(cast(^u16)srcAddr)^;
            }
        // All other types
        case:
            {
                // Copy memory
                mem.copy(sp^, srcAddr, int(size));
            }
    }

    // Append stack token
    (cast(^LumaStackToken)(uintptr(sp^) + uintptr(size)))^ = token;

    // Increment sp
    sp^ = cast(rawptr)(uintptr(sp^) + uintptr(size) + 1);
}


// Util
luma_stacktoken_peek :: proc(sp: rawptr) -> LumaStackToken
{
    return (cast(^LumaStackToken)(uintptr(sp) - size_of(LumaStackToken)))^;
}

/// Map a type code to the stack token used to represent the data on the stack
luma_stacktoken_from_typecode :: proc(code: LumaMetaTypeCode) -> LumaStackToken
{
    #partial switch code
    {
        case .Any:                                      return .Addr;
        case .Bool, .Char, .I8, .U8, .I16, .U16:        return .U32;
        case .I32, .U32:                                return .U32;
        case .I64, .U64:                                return .U64;
        case .F32:                                      return .F32;
        case .F64:                                      return .F64;
    }
    return .Invalid;
}