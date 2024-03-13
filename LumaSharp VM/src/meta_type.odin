package LumaVM

LumaMetaTypeCode :: enum u32
{
    Any = 1,
    Bool,
    Char,
    I8,
    U8,
    I16,
    U16,
    I32,
    U32,
    I64,
    U64,
    Float,
    Double,
}

LumaMetaTypeOption :: enum u32
{
    Export,
    Type,
    Contract,
    Enum,
    Array,
    Generic
}

LumaMetaTypeFlags :: distinct bit_set[LumaMetaTypeOption; u32]

LumaMetaType :: struct
{
    token: u32,
    name: string,
    flags: LumaMetaTypeFlags,
    code: LumaMetaTypeCode,

    runtimeType: LumaTypeHandle,
}

luma_meta_type_isexported :: proc(type: LumaMetaType) -> bool
{
    return (type.flags & {.Export}) == {.Export}
}

luma_meta_type_istype :: proc(type: LumaMetaType) -> bool
{
    return (type.flags & {.Type}) == {.Type}
}

luma_meta_type_iscontract :: proc(type: LumaMetaType) -> bool
{
    return (type.flags & {.Contract}) == {.Contract}
}

luma_meta_type_isenum :: proc(type: LumaMetaType) -> bool
{
    return (type.flags & {.Enum}) == {.Enum}
}

luma_meta_type_isarray :: proc(type: LumaMetaType) -> bool
{
    return (type.flags & {.Array}) == {.Array}
}

luma_meta_type_isgeneric :: proc(type: LumaMetaType) -> bool
{
    return (type.flags & {.Generic}) == {.Generic}
}