namespace LumaSharp.Compiler;

/// <summary>
/// Version of the compiler-emitted runtime contract. Increment this when generated
/// object layouts, helper signatures, ownership rules, or runtime state cease to be
/// link-compatible with earlier generated modules.
/// </summary>
public static class RuntimeAbi
{
    /// <summary>The first coordinated LumaSharp compiler/runtime ABI.</summary>
    public const int Version = 1;

    /// <summary>The LLVM module-flag key used to reject incompatible LTO inputs.</summary>
    public const string ModuleFlag = "LumaSharpRuntimeABI";
}
