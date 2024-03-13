package LumaVM

LumaStackHandle :: struct
{
    type: LumaTypeHandle,
    offset: u32,
}

LumaMemoryHandle :: struct
{
    referenceCount: u32,
    type: LumaTypeHandle,
}

luma_stack_allocate :: proc(stackSize: u32) -> [dynamic]u8
{
    // Check for empty stack
    if stackSize == 0 do return nil

    // Allocate stack buffer
    return make([dynamic]u8, stackSize)
}

luma_memory_allocate :: proc(type: LumaTypeHandle) -> ^u8
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

luma_memory_stackallocate :: proc(type: LumaTypeHandle, stack: [dynamic]u8, stackOffset: u32) -> (^u8, u32)
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