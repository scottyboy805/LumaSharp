using System.Runtime.InteropServices;
using System.Text;
using LLVMSharp.Interop;

namespace LumaSharp.Compiler;

public sealed record LlvmToolchainResult(bool Succeeded, string? TargetTriple, string? Error);

/// <summary>Loads, verifies, optionally optimizes, and emits LLVM IR using LLVM's C API.</summary>
public static unsafe class LlvmToolchain
{
    private static readonly object InitializationLock = new();
    private static readonly HashSet<string> InitializedTargets = new(StringComparer.Ordinal);

    public static string? GetHostTargetTriple()
    {
        try { var value = LLVM.GetDefaultTargetTriple(); try { return Utf8(value); } finally { LLVM.DisposeMessage(value); } }
        catch (DllNotFoundException) { return null; }
        catch (EntryPointNotFoundException) { return null; }
        catch (Exception) { return null; }
    }

    /// <summary>Resolves a target triple and verifies that LLVM can create a target machine for it.</summary>
    public static LlvmToolchainResult ValidateTarget(string targetTriple = "host")
    {
        LLVMOpaqueTargetMachine* machine = null;
        IntPtr triplePtr = IntPtr.Zero; IntPtr cpuPtr = IntPtr.Zero; IntPtr featuresPtr = IntPtr.Zero;
        string? requestedTriple = null;
        try
        {
            requestedTriple = targetTriple == "host" ? GetHostTripleUnsafe() : targetTriple;
            if (string.IsNullOrWhiteSpace(requestedTriple)) return new(false, null, "LLVM did not provide a host target triple.");
            InitializeTarget(requestedTriple);
            triplePtr = Utf8Ptr(requestedTriple); cpuPtr = Utf8Ptr(""); featuresPtr = Utf8Ptr("");
            LLVMTarget* target = null; sbyte* targetError = null;
            if (LLVM.GetTargetFromTriple((sbyte*)triplePtr, &target, &targetError) != 0)
            {
                var message = Utf8(targetError); if (targetError != null) LLVM.DisposeMessage(targetError);
                return new(false, requestedTriple, $"LLVM target '{requestedTriple}' is unavailable: {message}");
            }
            machine = LLVM.CreateTargetMachine(target, (sbyte*)triplePtr, (sbyte*)cpuPtr, (sbyte*)featuresPtr,
                LLVMCodeGenOptLevel.LLVMCodeGenLevelDefault, LLVMRelocMode.LLVMRelocDefault, LLVMCodeModel.LLVMCodeModelDefault);
            return machine == null
                ? new(false, requestedTriple, $"LLVM could not create a target machine for '{requestedTriple}'.")
                : new(true, requestedTriple, null);
        }
        catch (DllNotFoundException ex) { return new(false, requestedTriple, $"LLVM native library is unavailable: {ex.Message}"); }
        catch (EntryPointNotFoundException ex) { return new(false, requestedTriple, $"The loaded LLVM library is incompatible: {ex.Message}"); }
        catch (NotSupportedException ex) { return new(false, requestedTriple, ex.Message); }
        catch (Exception ex) { return new(false, requestedTriple, $"LLVM target validation failed: {ex.Message}"); }
        finally
        {
            if (machine != null) LLVM.DisposeTargetMachine(machine);
            foreach (var pointer in new[] { triplePtr, cpuPtr, featuresPtr }) if (pointer != IntPtr.Zero) Marshal.FreeCoTaskMem(pointer);
        }
    }

    public static LlvmToolchainResult VerifyAndEmitObject(string llvmIr, string outputPath, string targetTriple = "host", bool optimize = false,
        CancellationToken cancellationToken = default) =>
        VerifyAndEmitObjectCore(llvmIr, outputPath, targetTriple, optimize, cancellationToken, eliminateDeadCode: true);

    internal static LlvmToolchainResult VerifyAndEmitObject(string llvmIr, string outputPath, string targetTriple,
        bool optimize, bool eliminateDeadCode, CancellationToken cancellationToken = default, CompilationProfiler? profiler = null) =>
        VerifyAndEmitObjectCore(llvmIr, outputPath, targetTriple, optimize, cancellationToken, eliminateDeadCode, profiler);

    private static LlvmToolchainResult VerifyAndEmitObjectCore(string llvmIr, string outputPath, string targetTriple, bool optimize,
        CancellationToken cancellationToken, bool eliminateDeadCode, CompilationProfiler? profiler = null)
    {
        LLVMOpaqueContext* context = null; LLVMOpaqueMemoryBuffer* buffer = null; LLVMOpaqueModule* module = null;
        LLVMOpaqueTargetMachine* machine = null; LLVMOpaqueTargetData* dataLayout = null; LLVMOpaquePassBuilderOptions* passOptions = null;
        IntPtr triplePtr = IntPtr.Zero; IntPtr sourceNamePtr = IntPtr.Zero; IntPtr pathPtr = IntPtr.Zero; IntPtr cpuPtr = IntPtr.Zero; IntPtr featuresPtr = IntPtr.Zero;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var requestedTriple = targetTriple == "host" ? GetHostTripleUnsafe() : targetTriple;
            if (string.IsNullOrWhiteSpace(requestedTriple)) return new(false, null, "LLVM did not provide a host target triple.");
            if (profiler is null) InitializeTarget(requestedTriple);
            else profiler.Measure("LLVM target initialization", () => InitializeTarget(requestedTriple));
            triplePtr = Utf8Ptr(requestedTriple); sourceNamePtr = Utf8Ptr("lumasharp.module.ll"); pathPtr = Utf8Ptr(Path.GetFullPath(outputPath)); cpuPtr = Utf8Ptr(""); featuresPtr = Utf8Ptr("");
            var irBytes = Encoding.UTF8.GetBytes(llvmIr);
            using (profiler?.Measure("LLVM IR parsing"))
            {
                fixed (byte* irPointer = irBytes)
                {
                    context = LLVM.ContextCreate();
                    buffer = LLVM.CreateMemoryBufferWithMemoryRangeCopy((sbyte*)irPointer, (nuint)irBytes.Length, (sbyte*)sourceNamePtr);
                    sbyte* parseError = null;
                    var parseResult = LLVM.ParseIRInContext(context, buffer, &module, &parseError);
                    // LLVMParseIRInContext takes ownership of the memory buffer.
                    buffer = null;
                    if (parseResult != 0)
                    {
                        var message = Utf8(parseError); if (parseError != null) LLVM.DisposeMessage(parseError);
                        return new(false, requestedTriple, $"LLVM IR parse failed: {message}");
                    }
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
            using (profiler?.Measure("LLVM target machine setup"))
            {
                LLVM.SetTarget(module, (sbyte*)triplePtr);
                LLVMTarget* target = null; sbyte* targetError = null;
                if (LLVM.GetTargetFromTriple((sbyte*)triplePtr, &target, &targetError) != 0)
                {
                    var message = Utf8(targetError); if (targetError != null) LLVM.DisposeMessage(targetError);
                    return new(false, requestedTriple, $"LLVM target '{requestedTriple}' is unavailable: {message}");
                }
                machine = LLVM.CreateTargetMachine(target, (sbyte*)triplePtr, (sbyte*)cpuPtr, (sbyte*)featuresPtr,
                    LLVMCodeGenOptLevel.LLVMCodeGenLevelDefault, LLVMRelocMode.LLVMRelocDefault, LLVMCodeModel.LLVMCodeModelDefault);
                if (machine == null) return new(false, requestedTriple, $"LLVM could not create a target machine for '{requestedTriple}'.");
                dataLayout = LLVM.CreateTargetDataLayout(machine); LLVM.SetModuleDataLayout(module, dataLayout);
            }
            if (optimize || eliminateDeadCode)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // Verify the completed pipeline once below. Verifying after every
                // individual LLVM pass substantially slows optimized builds.
                passOptions = LLVM.CreatePassBuilderOptions();
                // Executables internalize compiler-managed definitions, so even a
                // debug build can safely discard functions and globals unreachable
                // from main without optimizing the remaining function bodies.
                IntPtr passPtr = Utf8Ptr(optimize ? "default<O2>" : "globaldce");
                try
                {
                    using (profiler?.Measure(optimize ? "LLVM optimization passes" : "LLVM dead-code elimination"))
                    {
                        var passError = LLVM.RunPasses(module, (sbyte*)passPtr, machine, passOptions);
                        if (passError != null)
                        {
                            var message = Utf8(LLVM.GetErrorMessage(passError)); LLVM.ConsumeError(passError);
                            return new(false, requestedTriple, $"LLVM {(optimize ? "optimization" : "dead-code elimination")} failed: {message}");
                        }
                    }
                }
                finally { Marshal.FreeCoTaskMem(passPtr); }
                cancellationToken.ThrowIfCancellationRequested();
            }
            cancellationToken.ThrowIfCancellationRequested();
            sbyte* verifyError = null;
            var verificationResult = 0;
            using (profiler?.Measure("LLVM final verification"))
                verificationResult = LLVM.VerifyModule(module, LLVMVerifierFailureAction.LLVMReturnStatusAction, &verifyError);
            if (verificationResult != 0)
            {
                var message = Utf8(verifyError); if (verifyError != null) LLVM.DisposeMessage(verifyError);
                return new(false, requestedTriple, $"LLVM IR verification failed: {message}");
            }
            cancellationToken.ThrowIfCancellationRequested();
            sbyte* emitError = null;
            var emitResult = 0;
            using (profiler?.Measure("LLVM object code generation"))
                emitResult = LLVM.TargetMachineEmitToFile(machine, module, (sbyte*)pathPtr, LLVMCodeGenFileType.LLVMObjectFile, &emitError);
            if (emitResult != 0)
            {
                var message = Utf8(emitError); if (emitError != null) LLVM.DisposeMessage(emitError);
                return new(false, requestedTriple, $"LLVM object emission failed: {message}");
            }
            cancellationToken.ThrowIfCancellationRequested();
            return new(true, requestedTriple, null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (DllNotFoundException ex) { return new(false, null, $"LLVM native library is unavailable: {ex.Message}"); }
        catch (EntryPointNotFoundException ex) { return new(false, null, $"The loaded LLVM library is incompatible: {ex.Message}"); }
        catch (Exception ex) { return new(false, null, ex.Message); }
        finally
        {
            if (buffer != null) LLVM.DisposeMemoryBuffer(buffer);
            if (module != null) LLVM.DisposeModule(module);
            if (dataLayout != null) LLVM.DisposeTargetData(dataLayout);
            if (machine != null) LLVM.DisposeTargetMachine(machine);
            if (passOptions != null) LLVM.DisposePassBuilderOptions(passOptions);
            if (context != null) LLVM.ContextDispose(context);
            foreach (var pointer in new[] { triplePtr, sourceNamePtr, pathPtr, cpuPtr, featuresPtr }) if (pointer != IntPtr.Zero) Marshal.FreeCoTaskMem(pointer);
        }
    }

    private static void InitializeTarget(string triple)
    {
        var prefix = TargetTripleInfo.GetLlvmBackend(triple);
        lock (InitializationLock)
        {
            if (!InitializedTargets.Add(prefix)) return;
            switch (prefix)
            {
                case "X86": LLVM.InitializeX86TargetInfo(); LLVM.InitializeX86Target(); LLVM.InitializeX86TargetMC(); LLVM.InitializeX86AsmPrinter(); LLVM.InitializeX86AsmParser(); break;
                case "AArch64": LLVM.InitializeAArch64TargetInfo(); LLVM.InitializeAArch64Target(); LLVM.InitializeAArch64TargetMC(); LLVM.InitializeAArch64AsmPrinter(); LLVM.InitializeAArch64AsmParser(); break;
                case "ARM": LLVM.InitializeARMTargetInfo(); LLVM.InitializeARMTarget(); LLVM.InitializeARMTargetMC(); LLVM.InitializeARMAsmPrinter(); LLVM.InitializeARMAsmParser(); break;
                case "RISCV": LLVM.InitializeRISCVTargetInfo(); LLVM.InitializeRISCVTarget(); LLVM.InitializeRISCVTargetMC(); LLVM.InitializeRISCVAsmPrinter(); LLVM.InitializeRISCVAsmParser(); break;
                case "WebAssembly": LLVM.InitializeWebAssemblyTargetInfo(); LLVM.InitializeWebAssemblyTarget(); LLVM.InitializeWebAssemblyTargetMC(); LLVM.InitializeWebAssemblyAsmPrinter(); LLVM.InitializeWebAssemblyAsmParser(); break;
            }
        }
    }
    private static string GetHostTripleUnsafe() { var value = LLVM.GetDefaultTargetTriple(); try { return Utf8(value); } finally { LLVM.DisposeMessage(value); } }
    private static IntPtr Utf8Ptr(string value) => Marshal.StringToCoTaskMemUTF8(value);
    private static string Utf8(sbyte* value) => value == null ? "" : Marshal.PtrToStringUTF8((IntPtr)value) ?? "";
}
