package LumaVM

LumaMetaFieldOption :: enum u32
{
    Export,
}

LumaMetaFieldFlags :: distinct bit_set[LumaMetaFieldOption; u32]

LumaMetaField :: struct
{
    token : u32,
    name: string,
    flags: LumaMetaFieldFlags,
    fieldType: LumaMetaType,
}

luma_meta_field_isexported :: proc(field: LumaMetaField) -> bool
{
    return (field.flags & {.Export}) == {.Export}
}