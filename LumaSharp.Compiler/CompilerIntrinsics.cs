namespace LumaSharp.Compiler;

/// <summary>
/// Compiler reserved runtime entry points. Keep the names, source signatures and LLVM
/// lowering targets in one extensible table so adding a runtime intrinsic does not
/// require another owner/name special case in semantic analysis or code generation.
/// </summary>
internal sealed record CompilerIntrinsic(string Name, string ReturnType, IReadOnlyList<string> ParameterTypes,
    IReadOnlyList<string>? PassingModes, string Lowering, string Family)
{
    public string PassingMode(int index) => PassingModes is not null && index < PassingModes.Count ? PassingModes[index] : "";
}

internal static class CompilerIntrinsics
{
    private static readonly IReadOnlyDictionary<string, CompilerIntrinsic> Entries = Build();

    public static bool TryGet(string name, out CompilerIntrinsic intrinsic) => Entries.TryGetValue(name, out intrinsic!);
    public static IReadOnlyCollection<CompilerIntrinsic> All => Entries.Values.ToArray();

    public static MethodSymbol CreateMethod(CompilerIntrinsic intrinsic, SourceLocation location)
    {
        var parameters = intrinsic.ParameterTypes.Select((type, index) => new ParameterDeclaration(type,
            "arg" + index.ToString(System.Globalization.CultureInfo.InvariantCulture), location,
            PassingMode: intrinsic.PassingMode(index))).ToArray();
        var declaration = new MethodDeclaration("__CompilerIntrinsics", intrinsic.Name, intrinsic.ReturnType,
            parameters, null, IsStatic: true, IsPublic: false, IsMutating: false, HasTypeParameters: false,
            Location: location, Accessibility: "private");
        return new MethodSymbol(new TypeDeclaration("System", "class", "__CompilerIntrinsics", [], location), declaration,
            Intrinsic: intrinsic);
    }

    private static IReadOnlyDictionary<string, CompilerIntrinsic> Build()
    {
        var entries = new List<CompilerIntrinsic>();
        void Add(string name, string result, string family, params string[] parameters) =>
            entries.Add(new(name, result, parameters, null, LoweringFor(name), family));
        void AddOut(string name, string result, string family, string outputType, params string[] parameters) =>
            entries.Add(new(name, result, parameters.Append(outputType).ToArray(),
                Enumerable.Repeat("", parameters.Length).Append("out").ToArray(), LoweringFor(name), family));

        Add("__memalloc", "void*", "memory", "int");
        Add("__memalloczero", "void*", "memory", "int");
        Add("__memfree", "void", "memory", "void*");
        Add("__memcpy", "void*", "memory", "void*", "void*", "int");
        Add("__memmove", "void*", "memory", "void*", "void*", "int");
        Add("__memzero", "void*", "memory", "void*", "int");

        entries.Add(new("__fileopen", "int32", ["string", "int32", "void*"], ["", "", "out"], LoweringFor("__fileopen"), "filesystem"));
        Add("__conwrite", "void", "filesystem", "string?", "int32", "bool");
        AddOut("__fileread", "int32", "filesystem", "int32", "void*", "uint8*", "int32");
        AddOut("__filewrite", "int32", "filesystem", "int32", "void*", "uint8*", "int32");
        AddOut("__fileseek", "int32", "filesystem", "int32", "void*", "int32", "int32");
        Add("__filesetlen", "int32", "filesystem", "void*", "int32");
        Add("__fileflush", "int32", "filesystem", "void*");
        Add("__fileclose", "int32", "filesystem", "void*");
        Add("__fileexists", "bool", "filesystem", "string");
        Add("__direxists", "bool", "filesystem", "string");
        Add("__dircreate", "int32", "filesystem", "string");
        Add("__filedelete", "int32", "filesystem", "string");
        Add("__dirdelete", "int32", "filesystem", "string");
        Add("__filemove", "int32", "filesystem", "string", "string", "bool");
        Add("__filecopy", "int32", "filesystem", "string", "string", "bool");
        Add("__curdir", "string", "filesystem");
        Add("__tempdir", "string", "filesystem");

        Add("__libopen", "void*", "interop", "string");
        Add("__libsym", "void*", "interop", "void*", "string");
        Add("__libclose", "bool", "interop", "void*");
        Add("__timestamp", "int64", "clock");
        Add("__frequency", "int64", "clock");
        Add("__threadstart", "int", "threading", "System.Threading.ThreadStart");
        Add("__threadstartarg", "int", "threading", "System.Delegate", "System.Object?");
        Add("__threadjoin", "bool", "threading", "int", "int32");
        Add("__threadclose", "void", "threading", "int");
        Add("__threadrequeststop", "void", "threading", "int");
        Add("__threadstoprequested", "bool", "threading", "int");
        Add("__threadsleep", "void", "threading", "int32");
        Add("__threadcurrent", "System.Threading.Thread", "threading");
        Add("__stacktrace", "string", "stacktrace", "int32");
        Add("__stackframes", "int32", "stacktrace", "int32");
        Add("__weakcreate", "int", "weak", "System.Object");
        Add("__weakalive", "bool", "weak", "int");
        Add("__weaktarget", "System.Object?", "weak", "int");
        Add("__weakrelease", "void", "weak", "int");
        Add("__execpath", "string", "environment");
        Add("__execdir", "string", "environment");
        Add("__execname", "string", "environment");
        Add("__processors", "int32", "environment");
        Add("__endianness", "bool", "environment");
        Add("__platform", "int32", "environment");
        Add("__architecture", "int32", "environment");
        Add("__arrlen", "int32", "array", "System.Array");
        Add("__arrrank", "int32", "array", "System.Array");
        Add("__arrdim", "int32", "array", "System.Array", "int32");
        Add("__arrptr", "void*", "array", "System.Array");

        return entries.ToDictionary(entry => entry.Name, StringComparer.Ordinal);
    }

    private static string LoweringFor(string name) => name switch
    {
        "__memalloc" => "luma_interop_memory_allocate",
        "__memalloczero" => "luma_interop_memory_allocate_zeroed",
        "__memfree" => "luma_interop_memory_free",
        "__memcpy" => "luma_interop_memory_copy",
        "__memmove" => "luma_interop_memory_move",
        "__memzero" => "luma_interop_memory_zero",
        "__fileopen" => "luma_fs_open",
        "__conwrite" => "luma_fs_console_write",
        "__fileread" => "luma_fs_read",
        "__filewrite" => "luma_fs_write",
        "__fileseek" => "luma_fs_seek",
        "__filesetlen" => "luma_fs_set_length",
        "__fileflush" => "luma_fs_flush",
        "__fileclose" => "luma_fs_close",
        "__fileexists" => "luma_fs_file_exists",
        "__direxists" => "luma_fs_directory_exists",
        "__dircreate" => "luma_fs_create_directory",
        "__filedelete" => "luma_fs_delete_file",
        "__dirdelete" => "luma_fs_delete_directory",
        "__filemove" => "luma_fs_move",
        "__filecopy" => "luma_fs_copy",
        "__curdir" => "luma_fs_current_directory",
        "__tempdir" => "luma_fs_temporary_directory",
        "__libopen" => "luma_interop_library_load",
        "__libsym" => "luma_interop_library_get_proc_address",
        "__libclose" => "luma_interop_library_unload",
        "__timestamp" => "luma_diag_timestamp",
        "__frequency" => "luma_diag_frequency",
        "__threadstart" => "luma_thread_start",
        "__threadstartarg" => "luma_thread_start",
        "__threadjoin" => "luma_thread_join",
        "__threadclose" => "luma_thread_close",
        "__threadrequeststop" => "luma_thread_request_stop",
        "__threadstoprequested" => "luma_thread_stop_requested",
        "__threadsleep" => "luma_thread_sleep",
        "__threadcurrent" => "luma_thread_current",
        "__stacktrace" => "luma_stacktrace_capture",
        "__stackframes" => "luma_stacktrace_frame_count",
        "__weakcreate" => "luma_weak_create",
        "__weakalive" => "luma_weak_is_alive",
        "__weaktarget" => "luma_weak_get_target",
        "__weakrelease" => "luma_weak_release",
        "__execpath" => "luma_env_executable_path",
        "__execdir" => "luma_env_executable_directory",
        "__execname" => "luma_env_executable_name",
        "__processors" => "luma_env_processor_count",
        "__endianness" => "luma_env_is_little_endian",
        "__platform" => "luma_env_platform",
        "__architecture" => "luma_env_architecture",
        "__arrlen" => "luma_intrinsic_arrlen",
        "__arrrank" => "luma_intrinsic_arrrank",
        "__arrdim" => "luma_intrinsic_arrdim",
        "__arrptr" => "luma_intrinsic_arrptr",
        _ => throw new InvalidOperationException($"No lowering is registered for compiler intrinsic '{name}'.")
    };
}
