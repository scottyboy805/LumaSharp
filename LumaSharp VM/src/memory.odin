package LumaVM

import "core:mem"

LumaStackHandle :: struct
{
    type: LumaTypeHandle,
    offset: u32,
}

LumaMemoryHandle :: struct
{
    referenceCount: u32,
    type: ^LumaTypeHandle,
}

luma_stack_allocate :: proc(stackSize: u32) -> [dynamic]u8
{
    // Check for empty stack
    if stackSize == 0 do return nil

    // Allocate stack buffer
    return make([dynamic]u8, stackSize)
}

luma_mem_alloc :: proc(type: ^LumaTypeHandle) -> ^u8
{
    // Check for empty type
    if type.size == 0 do return nil  

    // Calcualte full mem size required
    allocSize := size_of(LumaMemoryHandle) + type.size
    
    // Create the memory
    allocMemory := make([dynamic]u8, allocSize)

    // Get memory handle ptr
    memoryHandlePtr := cast(^LumaMemoryHandle)&allocMemory[0]

    // Write memory info
    memoryHandlePtr.type = type

    // Get actual instance ptr
    return cast(^u8)&allocMemory[size_of(LumaMemoryHandle)]
}

luma_mem_stackalloc :: proc(type: ^LumaTypeHandle, stack: [dynamic]u8, stackOffset: u32) -> (^u8, u32)
{
    // Check for empty type
    if type.size == 0 do return nil, stackOffset  

    // Calcualte full mem size required
    allocSize := size_of(LumaMemoryHandle) + type.size

    // Check stack overflow
    if stackOffset + allocSize >= cast(u32)len(stack) do return nil, stackOffset

    // Get memory handle ptr
    memoryHandlePtr := cast(^LumaMemoryHandle)&stack[stackOffset]

    // Write memory info
    memoryHandlePtr.type = type

    // Increment stack offset
    updatedStackOffset := stackOffset + allocSize

    // Get actual instance pointer and updated stack ptr
    return cast(^u8)&stack[stackOffset + size_of(LumaMemoryHandle)], updatedStackOffset
}

luma_mem_allocarray :: proc(elementType: ^LumaTypeHandle, elementCount: u64) -> ^[]u8
{
    // Check for empty type
    if elementType.size == 0 do return nil  

    // Calcualte full mem size required
    allocSize := size_of(LumaMemoryHandle) + (u64(elementType.size) * elementCount)

    // Create the memory
    allocMemory := make([dynamic]u8, allocSize)

    // Get memory handle ptr
    memoryHandlePtr := cast(^LumaMemoryHandle)&allocMemory[0]

    // Write memory info
    memoryHandlePtr.type = elementType

    // Get actual instance ptr
    return cast(^[]u8)&allocMemory[size_of(LumaMemoryHandle)]
}

luma_mem_gethandle :: proc(addr: rawptr) -> ^LumaMemoryHandle
{
    // Get base address
    basePtr := cast(rawptr)(uintptr(addr) - uintptr(size_of(LumaMemoryHandle)));

    // Get as memory handle
    return cast(^LumaMemoryHandle)basePtr;
}

luma_mem_ptr_offset :: proc(addr: rawptr, offset: int) -> rawptr
{
    return cast(rawptr)(uintptr(addr) + uintptr(offset));
}

luma_mem_ptr_add :: proc(addr: ^rawptr, offset: uint)
{
    addr^ = cast(rawptr)(uintptr(addr^) + uintptr(offset));
}

luma_mem_ptr_sub :: proc(addr: ^rawptr, offset: uint)
{
    addr^ = cast(rawptr)(uintptr(addr^) - uintptr(offset));
}


// // Read / Write proc
// luma_fetchi_8 :: proc(addr: ^rawptr) -> u8
// {
//     // Fetch val at ptr
//     val := (cast(^u8)addr^)^;

//     // Increment addr
//     addr^ = cast(rawptr)(uintptr(addr^) + 1)
//     return val;
// }

// luma_fetchi_16 :: proc(addr: ^rawptr) -> u16
// {
//     // Fetch val at ptr
//     val := (cast(^u16)addr^)^;

//     // Increment addr
//     addr^ = cast(rawptr)(uintptr(addr^) + 2)
//     return val;
// }

// luma_fetchi_32 :: proc(addr: ^rawptr) -> u32
// {
//     // Fetch val at ptr
//     val := (cast(^u32)addr^)^;

//     // Increment addr
//     addr^ = cast(rawptr)(uintptr(addr^) + 4)
//     return val;
// }

// luma_fetchi_64 :: proc(addr: ^rawptr) -> u64
// {
//     // Fetch val at ptr
//     val := (cast(^u64)addr^)^;

//     // Increment addr
//     addr^ = cast(rawptr)(uintptr(addr^) + 8)
//     return val;
// }

// luma_fetchd_8 :: proc(addr: ^rawptr) -> u8
// {
//     // Decrement addr
//     addr^ = cast(rawptr)(uintptr(addr^) - 1)

//     // Fetch val at ptr
//     return (cast(^u8)addr^)^;
// }

// luma_fetchd_16 :: proc(addr: ^rawptr) -> u16
// {
//     // Decrement addr
//     addr^ = cast(rawptr)(uintptr(addr^) - 2)

//     // Fetch val at ptr
//     return (cast(^u16)addr^)^;
// }

// luma_fetchd_32 :: proc(addr: ^rawptr) -> u32
// {
//     // Decrement addr
//     addr^ = cast(rawptr)(uintptr(addr^) - 4)

//     // Fetch val at ptr
//     return (cast(^u32)addr^)^;
// }

// luma_fetchd_64 :: proc(addr: ^rawptr) -> u64
// {
//     // Decrement addr
//     addr^ = cast(rawptr)(uintptr(addr^) - 8)

//     // Fetch val at ptr
//     return (cast(^u64)addr^)^;
// }

// luma_fetchd_ptr :: proc(addr: ^rawptr) -> rawptr
// {
//     // Decrement addr
//     addr^ = cast(rawptr)(uintptr(addr^) - uintptr(size_of(rawptr)))

//     // Fetch val at ptr
//     return (cast(^rawptr)addr^)^;
// }

luma_fetch_8 :: proc(addr: rawptr) -> u8
{
    // Fetch val at ptr
    return (cast(^u8)addr)^;
}

luma_fetch_16 :: proc(addr: rawptr) -> u16
{
    // Fetch val at ptr
    return (cast(^u16)addr)^;
}

luma_fetch_32 :: proc(addr: rawptr) -> u32
{
    // Fetch val at ptr
    return (cast(^u32)addr)^;
}

luma_fetch_64 :: proc(addr: rawptr) -> u64
{
    // Fetch val at ptr
    return (cast(^u64)addr)^;
}

luma_fetch_ptr :: proc(addr: rawptr) -> rawptr
{
    // Fetch val at ptr
    return (cast(^rawptr)addr)^;
}

// luma_writei_8 :: proc(addr: ^rawptr, val: u8)
// {
//     // Write to addr
//     (cast(^u8)addr^)^ = val;

//     // Increment addr
//     addr^ = cast(rawptr)(uintptr(addr^) + 1);    
// }

// luma_writei_16 :: proc(addr: ^rawptr, val: u16)
// {
//     // Write to addr
//     (cast(^u16)addr^)^ = val;

//     // Increment addr
//     addr^ = cast(rawptr)(uintptr(addr^) + 2);   
// }

// luma_writei_32 :: proc(addr: ^rawptr, val: u32)
// {
//     // Write to addr
//     (cast(^u32)addr^)^ = val;

//     // Increment addr
//     addr^ = cast(rawptr)(uintptr(addr^) + 4);   
// }

// luma_writei_64 :: proc(addr: ^rawptr, val: u64)
// {
//     // Write to addr
//     (cast(^u64)addr^)^ = val;

//     // Increment addr
//     addr^ = cast(rawptr)(uintptr(addr^) + 8);   
// }

// luma_writei_ptr :: proc(addr: ^rawptr, val: rawptr)
// {
//     // Write to addr
//     (cast(^rawptr)addr^)^ = val;

//     // Increment addr
//     addr^ = cast(rawptr)(uintptr(addr^) + uintptr(size_of(rawptr)));   
// }

// luma_write_8 :: proc(addr: rawptr, val: u8)
// {
//     // Write to addr
//     (cast(^u8)addr)^ = val; 
// }

// luma_write_16 :: proc(addr: rawptr, val: u16)
// {
//     // Write to addr
//     (cast(^u16)addr)^ = val; 
// }

// luma_write_32 :: proc(addr: rawptr, val: u32)
// {
//     // Write to addr
//     (cast(^u32)addr)^ = val;
// }

// luma_write_64 :: proc(addr: rawptr, val: u64)
// {
//     // Write to addr
//     (cast(^u64)addr)^ = val;
// }

// luma_copyi_x :: proc(src: rawptr, dest: ^rawptr, size: u32)
// {
//     // Copy memory
//     mem.copy(dest^, src, int(size));

//     // Increment dest
//     dest^ = cast(rawptr)(uintptr(dest^) + uintptr(size));  
// }

// luma_copyd_x :: proc(src: ^rawptr, dest: rawptr, size: u32)
// {
//     // Decrement dest
//     src^ = cast(rawptr)(uintptr(src^) - uintptr(size)); 

//     // Copy memory
//     mem.copy(dest, src^, int(size));
// }

// luma_copy_x :: proc(src: rawptr, dest: rawptr, size: u32)
// {
//     // Copy memory
//     mem.copy(dest, src, int(size));
// }