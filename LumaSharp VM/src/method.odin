package LumaVM

LumaMethodSignature :: struct
{
    returnCount: u16,
    returnTypes: []LumaStackHandle,
    parameterCount: u16,
    parameterTypes: []LumaStackHandle,
    localsCount: u16,
    localTypes: []LumaStackHandle,
}

LumaMethodHandle :: struct
{
    meta: ^LumaMetaMethod,
    signature: LumaMethodSignature,
    maxStack: u16,
    localPtrOffset: u16,
    stackPtrOffset: u16,
    instructions: ^u8,
}

luma_method_invoke :: proc(method: LumaMethodHandle)
{

}