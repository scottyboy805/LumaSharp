package LumaVM

LumaMethodSignature :: struct
{
    returnCount: u32,
    returnTypes: []LumaTypeHandle,
    parameterCount: u32,
    parameterTypes: []LumaTypeHandle,
}

LumaMethodHandle :: struct
{
    meta: ^LumaMetaMethod,
    signature: ^LumaMethodSignature,
    maxStack: u16,
    localPtrOffset: u16,
    stackPtrOffset: u16,
    instructions: ^u8,
}

luma_method_invoke :: proc(method: LumaMethodHandle)
{

}