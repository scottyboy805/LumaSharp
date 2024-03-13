package LumaVM

LumaMetaVersionInfo :: struct
{
    major: u32,
    minor: u32,
    build: u32,
}

LumaMetaNameInfo :: struct
{
    name: string,
    version: LumaMetaVersionInfo,
}

LumaMetaLibrary :: struct
{
    appContext: ^LumaAppContext,
    libPath: string,

    token: u32,
    nameInfo: LumaMetaNameInfo,
    referencesInfo: [dynamic]LumaMetaNameInfo,
    types: [dynamic]LumaMetaType,
}

luma_library_load :: proc(appContext: ^LumaAppContext, libPath: string) -> ^LumaMetaLibrary
{
    // Create memory
    lumaLib := new(LumaMetaLibrary) 

    // Context and path
    lumaLib.appContext = appContext;
    lumaLib.libPath = libPath

    return lumaLib
}

luma_library_unload :: proc(library: ^LumaMetaLibrary)
{

}