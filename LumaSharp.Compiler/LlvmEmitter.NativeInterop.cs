namespace LumaSharp.Compiler;

internal sealed partial class LlvmEmitter
{
    private static bool IsNativeInteropShim(MethodSymbol method) =>
        method.Declaration.IsExtern &&
        method.Owner.Namespace == "System.Interop" &&
        method.Owner.Name is "NativeMemoryRuntime" or "NativeLibraryRuntime" &&
        method.Declaration.Name.StartsWith("luma_interop_", StringComparison.Ordinal);

    private void EmitNativeInteropRuntime()
    {
        if (!compilation.Methods.Any(IsNativeFileSystemShim))
            EmitUtf16ToUtf8Shim();

        EmitNativeMemoryRuntime();
        EmitNativeLibraryRuntime();
    }

    private void EmitNativeMemoryRuntime()
    {
        var sizeType = compilation.NativePointerSize == 8 ? "i64" : "i32";
        AppendDeclarationIfMissing($"declare ptr @malloc({sizeType})");
        _output.AppendLine($"declare ptr @calloc({sizeType}, {sizeType})");
        AppendDeclarationIfMissing("declare void @free(ptr)");
        _output.AppendLine($"declare void @llvm.memcpy.p0.p0.i{compilation.NativePointerSize * 8}(ptr, ptr, {sizeType}, i1)");
        _output.AppendLine($"declare void @llvm.memmove.p0.p0.i{compilation.NativePointerSize * 8}(ptr, ptr, {sizeType}, i1)");
        _output.AppendLine($"declare void @llvm.memset.p0.i{compilation.NativePointerSize * 8}(ptr, i8, {sizeType}, i1)");
        _output.AppendLine($"define internal ptr @luma_interop_memory_allocate({sizeType} %size) {{");
        _output.AppendLine("entry:");
        _output.AppendLine("  %result = call ptr @malloc(" + sizeType + " %size)");
        _output.AppendLine("  ret ptr %result");
        _output.AppendLine("}");
        _output.AppendLine($"define internal ptr @luma_interop_memory_allocate_zeroed({sizeType} %size) {{");
        _output.AppendLine("entry:");
        _output.AppendLine($"  %result = call ptr @calloc({sizeType} 1, {sizeType} %size)");
        _output.AppendLine("  ret ptr %result");
        _output.AppendLine("}");
        _output.AppendLine($"define internal void @luma_interop_memory_free(ptr %memory) {{");
        _output.AppendLine("entry:");
        _output.AppendLine("  call void @free(ptr %memory)");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");
        EmitMemoryCopyFunction("copy", "memcpy");
        EmitMemoryCopyFunction("move", "memmove");
        EmitMemoryZeroFunction();
    }

    private void EmitMemoryCopyFunction(string operation, string intrinsic)
    {
        var sizeType = compilation.NativePointerSize == 8 ? "i64" : "i32";
        var intrinsicBits = compilation.NativePointerSize * 8;
        _output.AppendLine($"define internal ptr @luma_interop_memory_{operation}(ptr %destination, ptr %source, {sizeType} %count) {{");
        _output.AppendLine("entry:");
        _output.AppendLine($"  %empty = icmp sle {sizeType} %count, 0");
        _output.AppendLine("  br i1 %empty, label %done, label %copy");
        _output.AppendLine("copy:");
        _output.AppendLine($"  call void @llvm.{intrinsic}.p0.p0.i{intrinsicBits}(ptr %destination, ptr %source, {sizeType} %count, i1 false)");
        _output.AppendLine("  br label %done");
        _output.AppendLine("done:");
        _output.AppendLine("  ret ptr %destination");
        _output.AppendLine("}");
    }

    private void EmitMemoryZeroFunction()
    {
        var sizeType = compilation.NativePointerSize == 8 ? "i64" : "i32";
        var intrinsicBits = compilation.NativePointerSize * 8;
        _output.AppendLine($"define internal ptr @luma_interop_memory_zero(ptr %memory, {sizeType} %count) {{");
        _output.AppendLine("entry:");
        _output.AppendLine($"  %empty = icmp sle {sizeType} %count, 0");
        _output.AppendLine("  br i1 %empty, label %done, label %clear");
        _output.AppendLine("clear:");
        _output.AppendLine($"  call void @llvm.memset.p0.i{intrinsicBits}(ptr %memory, i8 0, {sizeType} %count, i1 false)");
        _output.AppendLine("  br label %done");
        _output.AppendLine("done:");
        _output.AppendLine("  ret ptr %memory");
        _output.AppendLine("}");
    }

    private void EmitNativeLibraryRuntime()
    {
        if (UsesWindowsCommandLineApi())
        {
            _output.AppendLine("declare ptr @LoadLibraryW(ptr)");
            _output.AppendLine("declare ptr @GetProcAddress(ptr, ptr)");
            _output.AppendLine("declare i32 @FreeLibrary(ptr)");
            _output.AppendLine("define internal ptr @luma_interop_library_load(ptr %path) {");
            _output.AppendLine("entry:");
            _output.AppendLine("  %handle = call ptr @LoadLibraryW(ptr %path)");
            _output.AppendLine("  ret ptr %handle");
            _output.AppendLine("}");
            _output.AppendLine("define internal ptr @luma_interop_library_get_proc_address(ptr %handle, ptr %name) {");
            _output.AppendLine("entry:");
            _output.AppendLine("  %name.length.address = alloca i32, align 4");
            _output.AppendLine("  %utf8.name = call ptr @luma_utf16_to_utf8(ptr %name, i32 -1, ptr %name.length.address)");
            _output.AppendLine("  %name.failed = icmp eq ptr %utf8.name, null");
            _output.AppendLine("  br i1 %name.failed, label %failure, label %lookup");
            _output.AppendLine("lookup:");
            _output.AppendLine("  %address = call ptr @GetProcAddress(ptr %handle, ptr %utf8.name)");
            _output.AppendLine("  call void @free(ptr %utf8.name)");
            _output.AppendLine("  ret ptr %address");
            _output.AppendLine("failure:");
            _output.AppendLine("  ret ptr null");
            _output.AppendLine("}");
            _output.AppendLine("define internal i1 @luma_interop_library_unload(ptr %handle) {");
            _output.AppendLine("entry:");
            _output.AppendLine("  %is.null = icmp eq ptr %handle, null");
            _output.AppendLine("  br i1 %is.null, label %success, label %unload");
            _output.AppendLine("unload:");
            _output.AppendLine("  %result = call i32 @FreeLibrary(ptr %handle)");
            _output.AppendLine("  %unloaded = icmp ne i32 %result, 0");
            _output.AppendLine("  ret i1 %unloaded");
            _output.AppendLine("success:");
            _output.AppendLine("  ret i1 true");
            _output.AppendLine("}");
            return;
        }

        _output.AppendLine("declare ptr @dlopen(ptr, i32)");
        _output.AppendLine("declare ptr @dlsym(ptr, ptr)");
        _output.AppendLine("declare i32 @dlclose(ptr)");
        AppendDeclarationIfMissing("declare void @free(ptr)");
        EmitPosixLibraryLoad();
        _output.AppendLine("define internal ptr @luma_interop_library_get_proc_address(ptr %handle, ptr %name) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %name.length.address = alloca i32, align 4");
        _output.AppendLine("  %utf8.name = call ptr @luma_utf16_to_utf8(ptr %name, i32 -1, ptr %name.length.address)");
        _output.AppendLine("  %name.failed = icmp eq ptr %utf8.name, null");
        _output.AppendLine("  br i1 %name.failed, label %failure, label %lookup");
        _output.AppendLine("lookup:");
        _output.AppendLine("  %address = call ptr @dlsym(ptr %handle, ptr %utf8.name)");
        _output.AppendLine("  call void @free(ptr %utf8.name)");
        _output.AppendLine("  ret ptr %address");
        _output.AppendLine("failure:");
        _output.AppendLine("  ret ptr null");
        _output.AppendLine("}");
        _output.AppendLine("define internal i1 @luma_interop_library_unload(ptr %handle) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %is.null = icmp eq ptr %handle, null");
        _output.AppendLine("  br i1 %is.null, label %success, label %unload");
        _output.AppendLine("unload:");
        _output.AppendLine("  %result = call i32 @dlclose(ptr %handle)");
        _output.AppendLine("  %unloaded = icmp eq i32 %result, 0");
        _output.AppendLine("  ret i1 %unloaded");
        _output.AppendLine("success:");
        _output.AppendLine("  ret i1 true");
        _output.AppendLine("}");
    }

    private void EmitPosixLibraryLoad()
    {
        _output.AppendLine("define internal ptr @luma_interop_library_load(ptr %path) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %path.length.address = alloca i32, align 4");
        _output.AppendLine("  %utf8.path = call ptr @luma_utf16_to_utf8(ptr %path, i32 -1, ptr %path.length.address)");
        _output.AppendLine("  %path.failed = icmp eq ptr %utf8.path, null");
        _output.AppendLine("  br i1 %path.failed, label %failure, label %load");
        _output.AppendLine("load:");
        _output.AppendLine("  %handle = call ptr @dlopen(ptr %utf8.path, i32 2)");
        _output.AppendLine("  call void @free(ptr %utf8.path)");
        _output.AppendLine("  ret ptr %handle");
        _output.AppendLine("failure:");
        _output.AppendLine("  ret ptr null");
        _output.AppendLine("}");
    }

    private void AppendDeclarationIfMissing(string declaration)
    {
        if (!_output.ToString().Contains(declaration, StringComparison.Ordinal))
            _output.AppendLine(declaration);
    }
}
