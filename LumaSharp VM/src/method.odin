package LumaVM

LumaMethodSignature :: struct
{
    returnCount: u16,
    returnTypes: []LumaStackHandle,
    parameterCount: u16,
    parameterTypes: []LumaStackHandle,
}

LumaMethodHandle :: struct
{
    meta: ^LumaMetaMethod,
    signature: LumaMethodSignature,
    maxStack: u16, 
    localsCount: u16,
    localsSize: u32,
    localTypes: []LumaStackHandle,
}

luma_method_invoke :: proc(method: ^LumaMethodHandle)
{

}