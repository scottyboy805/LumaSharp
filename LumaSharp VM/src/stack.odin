package LumaVM



luma_stack_new :: proc(stackSize: u32) -> []u8
{
    stack: [dynamic]u8 = make([dynamic]u8, stackSize)
    return nil
    //return make([stackSize]u32)
}