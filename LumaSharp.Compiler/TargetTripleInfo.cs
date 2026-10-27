namespace LumaSharp.Compiler;

/// <summary>Shared target-triple architecture classification for ABI and LLVM setup.</summary>
internal static class TargetTripleInfo
{
    public static string GetLlvmBackend(string targetTriple)
    {
        var architecture = GetArchitecture(targetTriple);
        if (architecture is "x86_64" or "x86_64h" or "amd64" or "i386" or "i486" or "i586" or "i686" or "x86") return "X86";
        if (architecture is "aarch64" or "aarch64_be" or "aarch64_32" or "arm64" or "arm64e" or "arm64ec" or "arm64_32") return "AArch64";
        if (architecture.StartsWith("arm", StringComparison.Ordinal) || architecture.StartsWith("thumb", StringComparison.Ordinal)) return "ARM";
        if (architecture is "riscv64" or "riscv32") return "RISCV";
        if (architecture is "wasm32" or "wasm64") return "WebAssembly";
        throw new NotSupportedException($"LLVM target architecture '{architecture}' is not enabled by the bundled backend.");
    }

    public static int GetPointerSize(string targetTriple, int hostPointerSize)
    {
        if (targetTriple == "host") return hostPointerSize;
        var architecture = GetArchitecture(targetTriple);
        if (architecture is "x86_64" or "x86_64h" or "amd64" or "aarch64" or "aarch64_be" or "arm64" or "arm64e" or "arm64ec" or "riscv64" or "wasm64" or "ppc64" or "ppc64le" or "mips64" or "s390x") return 8;
        if (architecture is "i386" or "i486" or "i586" or "i686" or "x86" or "aarch64_32" or "arm64_32" or "riscv32" or "wasm32" or "mips" or "ppc" ||
            architecture.StartsWith("arm", StringComparison.Ordinal) || architecture.StartsWith("thumb", StringComparison.Ordinal)) return 4;
        return hostPointerSize;
    }

    private static string GetArchitecture(string targetTriple) =>
        targetTriple.Split('-', 2)[0].Trim().ToLowerInvariant();
}
