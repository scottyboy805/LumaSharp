package LumaVM

@private

// Add
luma_add_U32 :: proc(a: u32, b: u32, opFlags: LumaOpCodeFlags) -> (result: u32, overflow: bool)
{
    // Check op flags
    switch                             
    {
        // Signed, no overflow
        case:
            {
                return u32(i32(a) + i32(b)), false;
            }
        // Unsigned, no overflow
        case LumaOpCodeFlags.Unsigned in opFlags:
            {
                return a + b, false;
            }
        // Unsigned, with overflow
        case LumaOpCodeFlags.Unsigned | LumaOpCodeFlags.Overflow in opFlags:
            {
                // Perform add and detect overflow
                sum := a + b;
                overflow := sum < a;

                // Get result with overflow flag
                return sum, overflow;
            }
        // Signed, with overflow
        case LumaOpCodeFlags.Overflow in opFlags:
            {
                // Perform add and detect overflow
                sum := i32(a) + i32(b);
                overflow := (b > 0 && sum < i32(a)) || (b < 0 && sum > i32(a));

                // Get result with overflow flag
                return u32(sum), overflow;
            }
    }
}

luma_add_U64 :: proc(a: u64, b: u64, opFlags: LumaOpCodeFlags) -> (result: u64, overflow: bool)
{
    // Check op flags
    switch                             
    {
        // Signed, no overflow
        case:
            {
                return u64(i64(a) + i64(b)), false;
            }
        // Unsigned, no overflow
        case LumaOpCodeFlags.Unsigned in opFlags:
            {
                return a + b, false;
            }
        // Unsigned, with overflow
        case LumaOpCodeFlags.Unsigned | LumaOpCodeFlags.Overflow in opFlags:
            {
                // Perform add and detect overflow
                sum := a + b;
                overflow := sum < a;

                // Get result with overflow flag
                return sum, overflow;
            }
        // Signed, with overflow
        case LumaOpCodeFlags.Overflow in opFlags:
            {
                // Perform add and detect overflow
                sum := i64(a) + i64(b);
                overflow := (b > 0 && sum < i64(a)) || (b < 0 && sum > i64(a));

                // Get result with overflow flag
                return u64(sum), overflow;
            }
    }
}



// Subtract
luma_sub_U32 :: proc(a: u32, b: u32, opFlags: LumaOpCodeFlags) -> (result: u32, overflow: bool)
{
    // Check op flags
    switch                             
    {
        // Signed, no overflow
        case:
            {
                return u32(i32(a) - i32(b)), false;
            }
        // Unsigned, no overflow
        case LumaOpCodeFlags.Unsigned in opFlags:
            {
                return a - b, false;
            }
        // Unsigned, with overflow
        case LumaOpCodeFlags.Unsigned | LumaOpCodeFlags.Overflow in opFlags:
            {
                // Perform sub and detect overflow
                sum := a - b;
                overflow := sum > a;

                // Get result with overflow flag
                return sum, overflow;
            }
        // Signed, with overflow
        case LumaOpCodeFlags.Overflow in opFlags:
            {
                // Perform sub and detect overflow
                sum := i32(a) - i32(b);
                overflow := (b > 0 && sum > i32(a)) || (b < 0 && sum < i32(a));

                // Get result with overflow flag
                return u32(sum), overflow;
            }
    }
}