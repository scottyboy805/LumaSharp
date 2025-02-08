package LumaVM

LumaMethodSignature :: struct
{
    returnCount: u16,
    returnTypes: []LumaMethodLocalHandle,
    parameterCount: u16,
    parameterTypes: []LumaMethodLocalHandle,
    localsCount: u16,
    localTypes: []LumaMethodLocalHandle,
}

LumaMethodLocalHandle :: struct
{
    offset: u16,
    size: u16,
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