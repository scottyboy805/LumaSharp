package LumaVM

// Parameter
LumaMetaParameterOption :: enum u32
{
    Reference,
    Optional,
}

LumaMetaParameterFlags :: distinct bit_set[LumaMetaParameterOption; u32]

LumaMetaParameter :: struct
{
    index: u32,
    name: string,
    flags: LumaMetaParameterFlags,
    type: LumaMetaType,
}

// Method
LumaMetaMethodOption :: enum u32
{
    Export,
    Initializer,
    Abstract,
    Override,
    Generic
}

LumaMetaMethodFlags :: distinct bit_set[LumaMetaMethodOption; u32]

LumaMetaMethod :: struct
{
    token: u32,
    name: string,
    flags: LumaMetaMethodFlags,
    returnTypes: []LumaMetaType,
    parameterTypes: []LumaMetaParameter,
}

luma_meta_method_isexported :: proc(method: LumaMetaMethod) -> bool
{
    return (method.flags & {.Export}) == {.Export}
}

luma_meta_method_isinitializer :: proc(method: LumaMetaMethod) -> bool
{
    return (method.flags & {.Initializer}) == {.Initializer}
}

luma_meta_method_isabstract :: proc(method: LumaMetaMethod) -> bool
{
    return (method.flags & {.Abstract}) == {.Abstract}
}

luma_meta_method_isoverride :: proc(method: LumaMetaMethod) -> bool
{
    return (method.flags & {.Override}) == {.Override}
}

