using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using LLVMSharp.Interop;

namespace LumaSharp.Tests;

/// <summary>Runs a generated module through the bundled host LLVM JIT with a tracked allocator.</summary>
internal static unsafe class LlvmJit
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int EntryPoint();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int EntryPointWithArguments(int argumentCount, IntPtr arguments);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr AllocateFunction(UIntPtr size);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void FreeFunction(IntPtr pointer);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr CopyFunction(IntPtr destination, IntPtr source, UIntPtr size);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int WindowsWriteFunction(int descriptor, IntPtr buffer, uint count);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nint PosixWriteFunction(int descriptor, IntPtr buffer, UIntPtr count);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr WindowsGetCommandLineFunction();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr WindowsCommandLineToArgvFunction(IntPtr commandLine, IntPtr argumentCount);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int WindowsWideCharToMultiByteFunction(uint codePage, uint flags, IntPtr wideString, int wideCharacterCount,
        IntPtr multiByteString, int multiByteCount, IntPtr defaultCharacter, IntPtr usedDefaultCharacter);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr WindowsLocalFreeFunction(IntPtr memory);

    private static readonly object Gate = new();
    private static readonly ConcurrentDictionary<IntPtr, byte> Allocations = new();
    private static readonly AllocateFunction AllocateCallback = Allocate;
    private static readonly FreeFunction FreeCallback = Release;
    private static readonly CopyFunction CopyCallback = Copy;
    private static readonly WindowsWriteFunction WindowsWriteCallback = CaptureWindowsWrite;
    private static readonly PosixWriteFunction PosixWriteCallback = CapturePosixWrite;
    private static readonly WindowsGetCommandLineFunction WindowsGetCommandLineCallback = GetSimulatedWindowsCommandLine;
    private static readonly WindowsCommandLineToArgvFunction WindowsCommandLineToArgvCallback = SplitSimulatedWindowsCommandLine;
    private static readonly WindowsWideCharToMultiByteFunction WindowsWideCharToMultiByteCallback = ConvertWindowsWideStringToUtf8;
    private static readonly WindowsLocalFreeFunction WindowsLocalFreeCallback = ReleaseSimulatedWindowsArguments;
    private static readonly object InteropErrorLock = new();
    private static readonly object StderrLock = new();
    private static readonly StringBuilder StderrOutput = new();
    private static string? _interopError;
    private static bool _initialized;
    private static IntPtr _cRuntimeHandle;
    private static int? _allocationFailureCountdown;
    private static string[] _simulatedWindowsArguments = [];
    private static IntPtr _simulatedWindowsCommandLine;
    private static readonly ConcurrentDictionary<IntPtr, IntPtr[]> SimulatedWindowsArgumentBlocks = new();

    public static (int ExitCode, int OutstandingAllocations, string? InteropError) RunMain(
        string llvmIr,
        int? allocationFailureAfter = null,
        string? forceStringRuntimeOverflow = null,
        string[]? arguments = null,
        byte[][]? rawUtf8Arguments = null)
    {
        lock (Gate)
        {
            Initialize();
            ClearInteropError();
            lock (StderrLock) StderrOutput.Clear();
            LLVMOpaqueContext* context = null;
            LLVMOpaqueMemoryBuffer* buffer = null;
            LLVMOpaqueModule* module = null;
            LLVMOpaqueExecutionEngine* engine = null;
            IntPtr triplePointer = IntPtr.Zero;
            IntPtr sourceNamePointer = IntPtr.Zero;
            IntPtr entryNamePointer = IntPtr.Zero;
            IntPtr nativeArgumentsPointer = IntPtr.Zero;
            var nativeArgumentStrings = new List<IntPtr>();
            var allocationsBefore = Allocations.Count;
            try
            {
                triplePointer = (IntPtr)LLVM.GetDefaultTargetTriple();
                var triple = Marshal.PtrToStringUTF8(triplePointer) ?? throw new InvalidOperationException("LLVM returned an empty host target triple.");
                sourceNamePointer = Marshal.StringToCoTaskMemUTF8("lumasharp-jit.ll");
                entryNamePointer = Marshal.StringToCoTaskMemUTF8("main");
                // MCJIT does not implement the host TLS relocation model. Each JIT run
                // invokes one entry point on one thread, so lower TLS globals to ordinary
                // globals only in this single-threaded execution harness.
                var executableIr = llvmIr.Replace("internal thread_local global", "internal global", StringComparison.Ordinal);
                if (forceStringRuntimeOverflow is not null)
                {
                    var operation = forceStringRuntimeOverflow switch
                    {
                        "length" => "  %length.pair = call { i32, i1 } @llvm.uadd.with.overflow.i32(i32 %left.length, i32 %right.length)",
                        "bytes" => "  %bytes.pair = call { i32, i1 } @llvm.uadd.with.overflow.i32(i32 %left.bytes, i32 %right.bytes)",
                        _ => throw new ArgumentOutOfRangeException(nameof(forceStringRuntimeOverflow), forceStringRuntimeOverflow, "Use 'length' or 'bytes'.")
                    };
                    if (!executableIr.Contains(operation, StringComparison.Ordinal))
                        throw new InvalidOperationException($"The generated string runtime does not contain the '{forceStringRuntimeOverflow}' overflow check.");
                    var forcedOperation = forceStringRuntimeOverflow == "length"
                        ? "  %length.pair = call { i32, i1 } @luma_test_overflow_pair()"
                        : "  %bytes.pair = call { i32, i1 } @luma_test_overflow_pair()";
                    executableIr = executableIr.Replace(operation, forcedOperation, StringComparison.Ordinal);
                    const string testFunction = """
                        define internal { i32, i1 } @luma_test_overflow_pair() {
                        entry:
                          %value = insertvalue { i32, i1 } poison, i32 0, 0
                          %result = insertvalue { i32, i1 } %value, i1 true, 1
                          ret { i32, i1 } %result
                        }
                        """;
                    const string entryPoint = "define i32 @main() {";
                    if (!executableIr.Contains(entryPoint, StringComparison.Ordinal))
                        throw new InvalidOperationException("The generated module has no main entry point for overflow injection.");
                    executableIr = executableIr.Replace(entryPoint, testFunction + Environment.NewLine + entryPoint, StringComparison.Ordinal);
                }
                var bytes = Encoding.UTF8.GetBytes(executableIr);
                fixed (byte* bytesPointer = bytes)
                {
                    context = LLVM.ContextCreate();
                    buffer = LLVM.CreateMemoryBufferWithMemoryRangeCopy((sbyte*)bytesPointer, (UIntPtr)bytes.Length, (sbyte*)sourceNamePointer);
                    sbyte* parseError = null;
                    var parseStatus = LLVM.ParseIRInContext(context, buffer, &module, &parseError);
                    buffer = null; // ParseIRInContext consumes the memory buffer.
                    if (parseStatus != 0) throw new InvalidOperationException(TakeMessage(parseError, "LLVM failed to parse the generated IR."));
                }

                LLVM.SetTarget(module, (sbyte*)triplePointer);
                sbyte* engineError = null;
                if (LLVM.CreateExecutionEngineForModule(&engine, module, &engineError) != 0)
                    throw new InvalidOperationException(TakeMessage(engineError, "LLVM failed to create the host execution engine."));
                module = null; // The execution engine owns the module after successful creation.
                var address = LLVM.GetFunctionAddress(engine, (sbyte*)entryNamePointer);
                if (address == 0) throw new InvalidOperationException("LLVM JIT did not expose the generated main function.");
                _allocationFailureCountdown = allocationFailureAfter;
                int exitCode;
                if (llvmIr.Contains("define i32 @main(i32 %argc, ptr %argv)", StringComparison.Ordinal))
                {
                    if (arguments is not null && rawUtf8Arguments is not null)
                        throw new InvalidOperationException("Choose either managed strings or raw UTF-8 argument bytes.");
                    var userArguments = rawUtf8Arguments ?? (arguments ?? []).Select(Encoding.UTF8.GetBytes).ToArray();
                    var processArguments = new[] { Encoding.UTF8.GetBytes("luma-test") }.Concat(userArguments).ToArray();
                    if (OperatingSystem.IsWindows())
                        _simulatedWindowsArguments = processArguments.Select(Encoding.UTF8.GetString).ToArray();
                    nativeArgumentsPointer = Marshal.AllocHGlobal(checked(IntPtr.Size * processArguments.Length));
                    for (var index = 0; index < processArguments.Length; index++)
                    {
                        var nativeArgument = Marshal.AllocHGlobal(checked(processArguments[index].Length + 1));
                        Marshal.Copy(processArguments[index], 0, nativeArgument, processArguments[index].Length);
                        Marshal.WriteByte(nativeArgument, processArguments[index].Length, 0);
                        nativeArgumentStrings.Add(nativeArgument);
                        Marshal.WriteIntPtr(nativeArgumentsPointer, checked(index * IntPtr.Size), nativeArgument);
                    }
                    var entry = Marshal.GetDelegateForFunctionPointer<EntryPointWithArguments>((IntPtr)(long)address);
                    exitCode = entry(processArguments.Length, nativeArgumentsPointer);
                }
                else
                {
                    if (arguments is { Length: > 0 }) throw new InvalidOperationException("Arguments were supplied to a Main method without a string[] parameter.");
                    if (rawUtf8Arguments is { Length: > 0 }) throw new InvalidOperationException("Arguments were supplied to a Main method without a string[] parameter.");
                    var entry = Marshal.GetDelegateForFunctionPointer<EntryPoint>((IntPtr)(long)address);
                    exitCode = entry();
                }
                return (exitCode, Allocations.Count - allocationsBefore, GetInteropError());
            }
            finally
            {
                _allocationFailureCountdown = null;
                if (engine != null) LLVM.DisposeExecutionEngine(engine);
                if (module != null) LLVM.DisposeModule(module);
                if (buffer != null) LLVM.DisposeMemoryBuffer(buffer);
                if (context != null) LLVM.ContextDispose(context);
                foreach (var nativeArgument in nativeArgumentStrings) Marshal.FreeHGlobal(nativeArgument);
                if (nativeArgumentsPointer != IntPtr.Zero) Marshal.FreeHGlobal(nativeArgumentsPointer);
                if (_simulatedWindowsCommandLine != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_simulatedWindowsCommandLine);
                    _simulatedWindowsCommandLine = IntPtr.Zero;
                }
                _simulatedWindowsArguments = [];
                foreach (var block in SimulatedWindowsArgumentBlocks.Keys)
                    ReleaseSimulatedWindowsArguments(block);
                if (triplePointer != IntPtr.Zero) LLVM.DisposeMessage((sbyte*)triplePointer);
                if (sourceNamePointer != IntPtr.Zero) Marshal.FreeCoTaskMem(sourceNamePointer);
                if (entryNamePointer != IntPtr.Zero) Marshal.FreeCoTaskMem(entryNamePointer);
            }
        }
    }

    public static string CapturedStderr
    {
        get { lock (StderrLock) return StderrOutput.ToString(); }
    }

    private static void Initialize()
    {
        if (_initialized) return;
        LLVM.InitializeNativeTarget();
        LLVM.InitializeNativeAsmPrinter();
        LLVM.LinkInMCJIT();
        AddSymbol("malloc", Marshal.GetFunctionPointerForDelegate(AllocateCallback));
        AddSymbol("free", Marshal.GetFunctionPointerForDelegate(FreeCallback));
        AddSymbol("memcpy", Marshal.GetFunctionPointerForDelegate(CopyCallback));
        AddSymbol("_write", Marshal.GetFunctionPointerForDelegate(WindowsWriteCallback));
        AddSymbol("write", Marshal.GetFunctionPointerForDelegate(PosixWriteCallback));
        if (OperatingSystem.IsWindows())
        {
            AddSymbol("GetCommandLineW", Marshal.GetFunctionPointerForDelegate(WindowsGetCommandLineCallback));
            AddSymbol("CommandLineToArgvW", Marshal.GetFunctionPointerForDelegate(WindowsCommandLineToArgvCallback));
            AddSymbol("WideCharToMultiByte", Marshal.GetFunctionPointerForDelegate(WindowsWideCharToMultiByteCallback));
            AddSymbol("LocalFree", Marshal.GetFunctionPointerForDelegate(WindowsLocalFreeCallback));
        }
        RegisterCrtSymbol("snprintf");
        _initialized = true;
    }

    private static IntPtr GetSimulatedWindowsCommandLine()
    {
        if (_simulatedWindowsCommandLine != IntPtr.Zero)
            Marshal.FreeHGlobal(_simulatedWindowsCommandLine);
        var escapedArguments = _simulatedWindowsArguments.Select(argument => "\"" + argument.Replace("\"", "\\\"", StringComparison.Ordinal) + "\"");
        _simulatedWindowsCommandLine = Marshal.StringToHGlobalUni(string.Join(" ", escapedArguments));
        return _simulatedWindowsCommandLine;
    }

    private static IntPtr SplitSimulatedWindowsCommandLine(IntPtr commandLine, IntPtr argumentCount)
    {
        var arguments = _simulatedWindowsArguments;
        Marshal.WriteInt32(argumentCount, arguments.Length);
        var block = Marshal.AllocHGlobal(checked(IntPtr.Size * arguments.Length));
        var strings = new IntPtr[arguments.Length];
        try
        {
            for (var index = 0; index < arguments.Length; index++)
            {
                strings[index] = Marshal.StringToHGlobalUni(arguments[index]);
                Marshal.WriteIntPtr(block, checked(index * IntPtr.Size), strings[index]);
            }
            if (!SimulatedWindowsArgumentBlocks.TryAdd(block, strings))
                throw new InvalidOperationException("Could not track a simulated Windows argument block.");
            return block;
        }
        catch
        {
            foreach (var value in strings)
                if (value != IntPtr.Zero) Marshal.FreeHGlobal(value);
            Marshal.FreeHGlobal(block);
            throw;
        }
    }

    private static int ConvertWindowsWideStringToUtf8(uint codePage, uint flags, IntPtr wideString, int wideCharacterCount,
        IntPtr multiByteString, int multiByteCount, IntPtr defaultCharacter, IntPtr usedDefaultCharacter)
    {
        if (codePage != 65001 || wideCharacterCount != -1 || wideString == IntPtr.Zero)
            return 0;
        var value = Marshal.PtrToStringUni(wideString) ?? string.Empty;
        var bytes = Encoding.UTF8.GetBytes(value + "\0");
        if (multiByteString == IntPtr.Zero)
            return bytes.Length;
        if (multiByteCount < bytes.Length)
            return 0;
        Marshal.Copy(bytes, 0, multiByteString, bytes.Length);
        return bytes.Length;
    }

    private static IntPtr ReleaseSimulatedWindowsArguments(IntPtr memory)
    {
        if (memory == IntPtr.Zero)
            return IntPtr.Zero;
        if (!SimulatedWindowsArgumentBlocks.TryRemove(memory, out var strings))
            return IntPtr.Zero;
        foreach (var value in strings) Marshal.FreeHGlobal(value);
        Marshal.FreeHGlobal(memory);
        return IntPtr.Zero;
    }

    private static int CaptureWindowsWrite(int descriptor, IntPtr buffer, uint count)
    {
        if (descriptor == 2) AppendStderr(buffer, checked((int)count));
        return checked((int)count);
    }

    private static nint CapturePosixWrite(int descriptor, IntPtr buffer, UIntPtr count)
    {
        var length = checked((int)count.ToUInt64());
        if (descriptor == 2) AppendStderr(buffer, length);
        return length;
    }

    private static void AppendStderr(IntPtr buffer, int count)
    {
        if (count <= 0) return;
        var bytes = new byte[count];
        Marshal.Copy(buffer, bytes, 0, count);
        lock (StderrLock) StderrOutput.Append(Encoding.UTF8.GetString(bytes));
    }

    private static void RegisterCrtSymbol(string name)
    {
        var libraries = OperatingSystem.IsWindows()
            ? new[] { "ucrtbase.dll", "msvcrt.dll" }
            : OperatingSystem.IsMacOS() ? new[] { "/usr/lib/libSystem.B.dylib" } : new[] { "libc.so.6", "libc.so" };
        foreach (var library in libraries)
        {
            var loaded = NativeLibrary.TryLoad(library, out _cRuntimeHandle);
            if (!loaded) continue;
            var found = NativeLibrary.TryGetExport(_cRuntimeHandle, name, out var address);
            if (!found && OperatingSystem.IsWindows()) found = NativeLibrary.TryGetExport(_cRuntimeHandle, "_snprintf", out address);
            if (found)
            {
                AddSymbol(name, address);
                return;
            }
            NativeLibrary.Free(_cRuntimeHandle);
            _cRuntimeHandle = IntPtr.Zero;
        }
    }

    private static void AddSymbol(string name, IntPtr address)
    {
        var namePointer = Marshal.StringToCoTaskMemUTF8(name);
        try { LLVM.AddSymbol((sbyte*)namePointer, (void*)address); }
        finally { Marshal.FreeCoTaskMem(namePointer); }
    }

    private static IntPtr Allocate(UIntPtr size)
    {
        if (_allocationFailureCountdown is { } remaining)
        {
            if (remaining == 0)
            {
                _allocationFailureCountdown = null;
                return IntPtr.Zero;
            }
            _allocationFailureCountdown = remaining - 1;
        }
        try
        {
            var address = Marshal.AllocHGlobal(checked((nint)size.ToUInt64()));
            Allocations.TryAdd(address, 0);
            return address;
        }
        catch (Exception ex)
        {
            SetInteropError($"malloc callback failed: {ex.Message}");
            return IntPtr.Zero;
        }
    }

    private static void Release(IntPtr address)
    {
        if (address == IntPtr.Zero) return;
        if (!Allocations.TryRemove(address, out _)) { SetInteropError("free callback received an untracked address."); return; }
        try { Marshal.FreeHGlobal(address); }
        catch (Exception ex) { SetInteropError($"free callback failed: {ex.Message}"); }
    }

    private static IntPtr Copy(IntPtr destination, IntPtr source, UIntPtr size)
    {
        try
        {
            var length = checked((long)size.ToUInt64());
            Buffer.MemoryCopy(source.ToPointer(), destination.ToPointer(), length, length);
        }
        catch (Exception ex) { SetInteropError($"memcpy callback failed: {ex.Message}"); }
        return destination;
    }

    private static string TakeMessage(sbyte* message, string fallback)
    {
        if (message == null) return fallback;
        var result = Marshal.PtrToStringUTF8((IntPtr)message) ?? fallback;
        LLVM.DisposeMessage(message);
        return result;
    }

    private static void ClearInteropError() { lock (InteropErrorLock) _interopError = null; }
    private static void SetInteropError(string value) { lock (InteropErrorLock) _interopError ??= value; }
    private static string? GetInteropError() { lock (InteropErrorLock) return _interopError; }
}
