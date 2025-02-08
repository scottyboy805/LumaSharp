package LumaVM

LumaTypeHandle :: struct
{
    code: LumaMetaTypeCode,
    size: u32,
}

luma_type_getHandle :: proc(luma: ^LumaState, token: u32) -> ^LumaTypeHandle
{
    // Check for built in
    if luma_type_isprimitive(token) == true
    {
        // Get type from state
        return &luma.types[token];
    }
    return nil;
}

luma_type_isprimitive :: proc(token: u32) -> bool
{
    // Get max value
    max := u32(LumaMetaTypeCode.Max);

    // Check for built int
    return token < max;
}