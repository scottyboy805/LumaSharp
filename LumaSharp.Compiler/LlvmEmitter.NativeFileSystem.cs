namespace LumaSharp.Compiler;

internal sealed partial class LlvmEmitter
{
    private static bool IsNativeFileSystemShim(MethodSymbol method) =>
        method.Declaration.IsExtern &&
        method.Owner.Namespace == "System.IO" &&
        method.Owner.Name == "NativeFileSystem" &&
        method.Declaration.Name.StartsWith("luma_fs_", StringComparison.Ordinal);

    private void EmitNativeFileSystemRuntime()
    {
        if (UsesWindowsCommandLineApi()) EmitWindowsFileSystemRuntime();
        else EmitPosixFileSystemRuntime();
    }

    private void EmitPosixFileSystemRuntime()
    {
        var sizeType = compilation.NativePointerSize == 8 ? "i64" : "i32";
        _output.AppendLine("@luma_fs_mode_read = private unnamed_addr constant [3 x i8] c\"rb\\00\"");
        _output.AppendLine("@luma_fs_mode_write = private unnamed_addr constant [3 x i8] c\"wb\\00\"");
        _output.AppendLine("@luma_fs_mode_write_new = private unnamed_addr constant [4 x i8] c\"wbx\\00\"");
        _output.AppendLine("@luma_fs_mode_create = private unnamed_addr constant [4 x i8] c\"w+b\\00\"");
        _output.AppendLine("@luma_fs_mode_create_new = private unnamed_addr constant [5 x i8] c\"w+bx\\00\"");
        _output.AppendLine("@luma_fs_mode_append = private unnamed_addr constant [3 x i8] c\"ab\\00\"");
        _output.AppendLine("@luma_fs_mode_readwrite = private unnamed_addr constant [4 x i8] c\"r+b\\00\"");
        _output.AppendLine("@luma_fs_mode_readwrite_create = private unnamed_addr constant [4 x i8] c\"w+b\\00\"");
        _output.AppendLine("declare ptr @fopen(ptr, ptr)");
        _output.AppendLine($"declare {sizeType} @fread(ptr, {sizeType}, {sizeType}, ptr)");
        _output.AppendLine($"declare {sizeType} @fwrite(ptr, {sizeType}, {sizeType}, ptr)");
        _output.AppendLine("declare i32 @fseek(ptr, i32, i32)");
        _output.AppendLine("declare i32 @ftell(ptr)");
        _output.AppendLine("declare i32 @fflush(ptr)");
        _output.AppendLine("declare i32 @fclose(ptr)");
        _output.AppendLine("declare i32 @fileno(ptr)");
        _output.AppendLine($"declare i32 @ftruncate(i32, {sizeType})");
        _output.AppendLine("declare i32 @access(ptr, i32)");
        _output.AppendLine("declare ptr @opendir(ptr)");
        _output.AppendLine("declare i32 @closedir(ptr)");
        _output.AppendLine("declare i32 @mkdir(ptr, i32)");
        _output.AppendLine("declare i32 @unlink(ptr)");
        _output.AppendLine("declare i32 @rmdir(ptr)");
        _output.AppendLine("declare i32 @rename(ptr, ptr)");
        _output.AppendLine("declare i32 @remove(ptr)");
        _output.AppendLine($"declare ptr @getcwd(ptr, {sizeType})");
        _output.AppendLine("declare ptr @getenv(ptr)");
        _output.AppendLine("@luma_fs_temp_variable = private unnamed_addr constant [7 x i8] c\"TMPDIR\\00\"");
        _output.AppendLine("@luma_fs_temp_default = private unnamed_addr constant [6 x i8] c\"/tmp/\\00\"");
        _output.AppendLine("define internal i32 @luma_fs_open(ptr %path, i32 %mode, ptr %handle) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %read = icmp eq i32 %mode, 0");
        _output.AppendLine("  %write = icmp eq i32 %mode, 1");
        _output.AppendLine("  %append = icmp eq i32 %mode, 2");
        _output.AppendLine("  %readwrite = icmp eq i32 %mode, 3");
        _output.AppendLine("  %create.new = icmp eq i32 %mode, 5");
        _output.AppendLine("  %valid.first = or i1 %read, %write");
        _output.AppendLine("  %valid.second = or i1 %append, %readwrite");
        _output.AppendLine("  %valid.basic = or i1 %valid.first, %valid.second");
        _output.AppendLine("  %valid = or i1 %valid.basic, %create.new");
        _output.AppendLine("  br i1 %valid, label %select.mode, label %invalid");
        _output.AppendLine("select.mode:");
        _output.AppendLine("  %mode.ptr.0 = getelementptr [3 x i8], ptr @luma_fs_mode_read, i32 0, i32 0");
        _output.AppendLine("  %mode.ptr.1 = getelementptr [3 x i8], ptr @luma_fs_mode_write, i32 0, i32 0");
        _output.AppendLine("  %mode.ptr.2 = getelementptr [3 x i8], ptr @luma_fs_mode_append, i32 0, i32 0");
        _output.AppendLine("  %mode.ptr.3 = getelementptr [4 x i8], ptr @luma_fs_mode_readwrite, i32 0, i32 0");
        _output.AppendLine("  %mode.ptr.4 = getelementptr [4 x i8], ptr @luma_fs_mode_readwrite_create, i32 0, i32 0");
        _output.AppendLine("  %mode.ptr.5 = getelementptr [5 x i8], ptr @luma_fs_mode_create_new, i32 0, i32 0");
        _output.AppendLine("  %mode.create = getelementptr [4 x i8], ptr @luma_fs_mode_create, i32 0, i32 0");
        _output.AppendLine("  %selected.0 = select i1 %read, ptr %mode.ptr.0, ptr %mode.create");
        _output.AppendLine("  %selected.1 = select i1 %append, ptr %mode.ptr.2, ptr %mode.ptr.3");
        _output.AppendLine("  %selected.base = select i1 %valid.first, ptr %selected.0, ptr %selected.1");
        _output.AppendLine("  %existing = call i32 @access(ptr %path, i32 0)");
        _output.AppendLine("  %does.not.exist = icmp ne i32 %existing, 0");
        _output.AppendLine("  %readwrite.new = select i1 %does.not.exist, ptr %mode.ptr.4, ptr %mode.ptr.3");
        _output.AppendLine("  %is.open.or.create = icmp eq i32 %mode, 3");
        _output.AppendLine("  %selected = select i1 %is.open.or.create, ptr %readwrite.new, ptr %selected.base");
        _output.AppendLine("  %selected.final = select i1 %create.new, ptr %mode.ptr.5, ptr %selected");
        _output.AppendLine("  %file = call ptr @fopen(ptr %path, ptr %selected.final)");
        _output.AppendLine("  %failed = icmp eq ptr %file, null");
        _output.AppendLine("  br i1 %failed, label %failure, label %success");
        _output.AppendLine("success:");
        _output.AppendLine("  store ptr %file, ptr %handle");
        _output.AppendLine("  ret i32 0");
        _output.AppendLine("invalid:");
        _output.AppendLine("  ret i32 -1");
        _output.AppendLine("failure:");
        _output.AppendLine("  ret i32 -1");
        _output.AppendLine("}");
        _output.AppendLine($"define internal i32 @luma_fs_read(ptr %handle, ptr %buffer, i32 %count, ptr %bytesRead) {{");
        _output.AppendLine("entry:");
        _output.AppendLine($"  %count.native = {(sizeType == "i64" ? "zext i32 %count to i64" : "add i32 %count, 0")}");
        _output.AppendLine($"  %read.native = call {sizeType} @fread(ptr %buffer, {sizeType} 1, {sizeType} %count.native, ptr %handle)");
        _output.AppendLine(sizeType == "i64"
            ? "  %read = trunc i64 %read.native to i32"
            : "  %read = add i32 %read.native, 0");
        _output.AppendLine("  store i32 %read, ptr %bytesRead");
        _output.AppendLine($"  %error = call i32 @ferror(ptr %handle)");
        _output.AppendLine("  %failed = icmp ne i32 %error, 0");
        _output.AppendLine("  %status = select i1 %failed, i32 -1, i32 0");
        _output.AppendLine("  ret i32 %status");
        _output.AppendLine("}");
        _output.AppendLine($"define internal i32 @luma_fs_write(ptr %handle, ptr %buffer, i32 %count, ptr %bytesWritten) {{");
        _output.AppendLine("entry:");
        _output.AppendLine($"  %count.native = {(sizeType == "i64" ? "zext i32 %count to i64" : "add i32 %count, 0")}");
        _output.AppendLine($"  %written.native = call {sizeType} @fwrite(ptr %buffer, {sizeType} 1, {sizeType} %count.native, ptr %handle)");
        _output.AppendLine(sizeType == "i64"
            ? "  %written = trunc i64 %written.native to i32"
            : "  %written = add i32 %written.native, 0");
        _output.AppendLine("  store i32 %written, ptr %bytesWritten");
        _output.AppendLine("  %complete = icmp eq i32 %written, %count");
        _output.AppendLine("  %status = select i1 %complete, i32 0, i32 -1");
        _output.AppendLine("  ret i32 %status");
        _output.AppendLine("}");
        _output.AppendLine("declare i32 @ferror(ptr)");
        _output.AppendLine("define internal i32 @luma_fs_seek(ptr %handle, i32 %offset, i32 %origin, ptr %position) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %failed = call i32 @fseek(ptr %handle, i32 %offset, i32 %origin)");
        _output.AppendLine("  %has.failed = icmp ne i32 %failed, 0");
        _output.AppendLine("  br i1 %has.failed, label %error, label %tell");
        _output.AppendLine("tell:");
        _output.AppendLine("  %current = call i32 @ftell(ptr %handle)");
        _output.AppendLine("  %position.failed = icmp slt i32 %current, 0");
        _output.AppendLine("  br i1 %position.failed, label %error, label %success");
        _output.AppendLine("success:");
        _output.AppendLine("  store i32 %current, ptr %position");
        _output.AppendLine("  ret i32 0");
        _output.AppendLine("error:");
        _output.AppendLine("  ret i32 -1");
        _output.AppendLine("}");
        _output.AppendLine("define internal i32 @luma_fs_set_length(ptr %handle, i32 %length) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %flush = call i32 @fflush(ptr %handle)");
        _output.AppendLine("  %flush.failed = icmp ne i32 %flush, 0");
        _output.AppendLine("  br i1 %flush.failed, label %error, label %truncate");
        _output.AppendLine("truncate:");
        _output.AppendLine("  %descriptor = call i32 @fileno(ptr %handle)");
        _output.AppendLine($"  %length.native = {(sizeType == "i64" ? "zext i32 %length to i64" : "add i32 %length, 0")}");
        _output.AppendLine($"  %result = call i32 @ftruncate(i32 %descriptor, {sizeType} %length.native)");
        _output.AppendLine("  ret i32 %result");
        _output.AppendLine("error:");
        _output.AppendLine("  ret i32 -1");
        _output.AppendLine("}");
        EmitSimpleFileStatusFunction("flush", "i32", "call i32 @fflush(ptr %handle)", "%result");
        EmitSimpleFileStatusFunction("close", "i32", "call i32 @fclose(ptr %handle)", "%result");
        _output.AppendLine("define internal i1 @luma_fs_directory_exists(ptr %path) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %directory = call ptr @opendir(ptr %path)");
        _output.AppendLine("  %missing = icmp eq ptr %directory, null");
        _output.AppendLine("  br i1 %missing, label %no, label %yes");
        _output.AppendLine("yes:");
        _output.AppendLine("  %closed = call i32 @closedir(ptr %directory)");
        _output.AppendLine("  ret i1 true");
        _output.AppendLine("no:");
        _output.AppendLine("  ret i1 false");
        _output.AppendLine("}");
        _output.AppendLine("define internal i1 @luma_fs_file_exists(ptr %path) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %available = call i32 @access(ptr %path, i32 0)");
        _output.AppendLine("  %exists = icmp eq i32 %available, 0");
        _output.AppendLine("  %directory = call i1 @luma_fs_directory_exists(ptr %path)");
        _output.AppendLine("  %is.file = xor i1 %directory, true");
        _output.AppendLine("  %result = and i1 %exists, %is.file");
        _output.AppendLine("  ret i1 %result");
        _output.AppendLine("}");
        _output.AppendLine("define internal i32 @luma_fs_create_directory(ptr %path) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %result = call i32 @mkdir(ptr %path, i32 511)");
        _output.AppendLine("  ret i32 %result");
        _output.AppendLine("}");
        EmitSimplePathStatusFunction("delete_file", "unlink", "ptr %path");
        EmitSimplePathStatusFunction("delete_directory", "rmdir", "ptr %path");
        _output.AppendLine("define internal i32 @luma_fs_move(ptr %source, ptr %destination, i1 %overwrite) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %destination.exists = call i32 @access(ptr %destination, i32 0)");
        _output.AppendLine("  %exists = icmp eq i32 %destination.exists, 0");
        _output.AppendLine("  %overwrite.disabled = xor i1 %overwrite, true");
        _output.AppendLine("  %must.fail = and i1 %exists, %overwrite.disabled");
        _output.AppendLine("  br i1 %must.fail, label %error, label %move");
        _output.AppendLine("move:");
        _output.AppendLine("  %result = call i32 @rename(ptr %source, ptr %destination)");
        _output.AppendLine("  ret i32 %result");
        _output.AppendLine("error:");
        _output.AppendLine("  ret i32 -1");
        _output.AppendLine("}");
        _output.AppendLine("define internal i32 @luma_fs_copy(ptr %source, ptr %destination, i1 %overwrite) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %buffer = alloca [8192 x i8], align 16");
        _output.AppendLine("  %buffer.ptr = getelementptr [8192 x i8], ptr %buffer, i32 0, i32 0");
        _output.AppendLine("  %source.file = call ptr @fopen(ptr %source, ptr @luma_fs_mode_read)");
        _output.AppendLine("  %source.failed = icmp eq ptr %source.file, null");
        _output.AppendLine("  br i1 %source.failed, label %error, label %destination.open");
        _output.AppendLine("destination.open:");
        _output.AppendLine("  %destination.exists = call i32 @access(ptr %destination, i32 0)");
        _output.AppendLine("  %exists = icmp eq i32 %destination.exists, 0");
        _output.AppendLine("  %overwrite.disabled = xor i1 %overwrite, true");
        _output.AppendLine("  %cannot.overwrite = and i1 %exists, %overwrite.disabled");
        _output.AppendLine("  br i1 %cannot.overwrite, label %close.source.error, label %create.destination");
        _output.AppendLine("create.destination:");
        _output.AppendLine("  %write.mode = getelementptr [3 x i8], ptr @luma_fs_mode_write, i32 0, i32 0");
        _output.AppendLine("  %new.mode = getelementptr [4 x i8], ptr @luma_fs_mode_write_new, i32 0, i32 0");
        _output.AppendLine("  %selected.mode = select i1 %overwrite, ptr %write.mode, ptr %new.mode");
        _output.AppendLine("  %destination.file = call ptr @fopen(ptr %destination, ptr %selected.mode)");
        _output.AppendLine("  %destination.failed = icmp eq ptr %destination.file, null");
        _output.AppendLine("  br i1 %destination.failed, label %close.source.error, label %copy.loop");
        _output.AppendLine("copy.loop:");
        _output.AppendLine($"  %read = call {sizeType} @fread(ptr %buffer.ptr, {sizeType} 1, {sizeType} 8192, ptr %source.file)");
        _output.AppendLine($"  %written = call {sizeType} @fwrite(ptr %buffer.ptr, {sizeType} 1, {sizeType} %read, ptr %destination.file)");
        _output.AppendLine($"  %write.failed = icmp ne {sizeType} %read, %written");
        _output.AppendLine("  br i1 %write.failed, label %close.error, label %check.eof");
        _output.AppendLine("check.eof:");
        _output.AppendLine($"  %eof = icmp ult {sizeType} %read, 8192");
        _output.AppendLine("  br i1 %eof, label %check.read.error, label %copy.loop");
        _output.AppendLine("check.read.error:");
        _output.AppendLine("  %read.error = call i32 @ferror(ptr %source.file)");
        _output.AppendLine("  %has.read.error = icmp ne i32 %read.error, 0");
        _output.AppendLine("  br i1 %has.read.error, label %close.error, label %finish");
        _output.AppendLine("finish:");
        _output.AppendLine("  %destination.close = call i32 @fclose(ptr %destination.file)");
        _output.AppendLine("  %source.close = call i32 @fclose(ptr %source.file)");
        _output.AppendLine("  %failed = icmp ne i32 %destination.close, 0");
        _output.AppendLine("  %status = select i1 %failed, i32 -1, i32 0");
        _output.AppendLine("  ret i32 %status");
        _output.AppendLine("close.error:");
        _output.AppendLine("  %failed.destination.close = call i32 @fclose(ptr %destination.file)");
        _output.AppendLine("  br label %close.source.error");
        _output.AppendLine("close.source.error:");
        _output.AppendLine("  %failed.source.close = call i32 @fclose(ptr %source.file)");
        _output.AppendLine("  br label %error");
        _output.AppendLine("error:");
        _output.AppendLine("  ret i32 -1");
        _output.AppendLine("}");
        _output.AppendLine("define internal ptr @luma_fs_current_directory() {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %buffer = alloca [32768 x i8], align 16");
        _output.AppendLine("  %buffer.ptr = getelementptr [32768 x i8], ptr %buffer, i32 0, i32 0");
        _output.AppendLine($"  %cwd = call ptr @getcwd(ptr %buffer.ptr, {sizeType} 32768)");
        _output.AppendLine("  %failed = icmp eq ptr %cwd, null");
        _output.AppendLine("  br i1 %failed, label %error, label %convert");
        _output.AppendLine("convert:");
        _output.AppendLine("  %result = call ptr @luma_string_from_utf8z(ptr %cwd)");
        _output.AppendLine("  ret ptr %result");
        _output.AppendLine("error:");
        _output.AppendLine("  ret ptr @luma_string_empty");
        _output.AppendLine("}");
        _output.AppendLine("define internal ptr @luma_fs_temporary_directory() {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %variable = getelementptr [7 x i8], ptr @luma_fs_temp_variable, i32 0, i32 0");
        _output.AppendLine("  %default = getelementptr [6 x i8], ptr @luma_fs_temp_default, i32 0, i32 0");
        _output.AppendLine("  %environment = call ptr @getenv(ptr %variable)");
        _output.AppendLine("  %missing = icmp eq ptr %environment, null");
        _output.AppendLine("  %path = select i1 %missing, ptr %default, ptr %environment");
        _output.AppendLine("  %result = call ptr @luma_string_from_utf8z(ptr %path)");
        _output.AppendLine("  ret ptr %result");
        _output.AppendLine("}");
    }

    private void EmitWindowsFileSystemRuntime()
    {
        // The Windows backend is implemented with kernel32 wide-character APIs.
        // Path conversion uses a per-call stack buffer and rejects invalid UTF-8.
        _output.AppendLine("declare i32 @MultiByteToWideChar(i32, i32, ptr, i32, ptr, i32)");
        _output.AppendLine("declare i32 @GetCurrentDirectoryW(i32, ptr)");
        _output.AppendLine("declare i32 @GetTempPathW(i32, ptr)");
        _output.AppendLine("declare i32 @GetFileAttributesW(ptr)");
        _output.AppendLine("declare i32 @CreateDirectoryW(ptr, ptr)");
        _output.AppendLine("declare i32 @DeleteFileW(ptr)");
        _output.AppendLine("declare i32 @RemoveDirectoryW(ptr)");
        _output.AppendLine("declare i32 @MoveFileExW(ptr, ptr, i32)");
        _output.AppendLine("declare i32 @CopyFileW(ptr, ptr, i32)");
        _output.AppendLine("declare ptr @CreateFileW(ptr, i32, i32, ptr, i32, i32, ptr)");
        _output.AppendLine("declare i32 @ReadFile(ptr, ptr, i32, ptr, ptr)");
        _output.AppendLine("declare i32 @WriteFile(ptr, ptr, i32, ptr, ptr)");
        _output.AppendLine("declare i32 @FlushFileBuffers(ptr)");
        _output.AppendLine("declare i32 @CloseHandle(ptr)");
        _output.AppendLine("declare i32 @SetFilePointerEx(ptr, i64, ptr, i32)");
        _output.AppendLine("declare i32 @SetEndOfFile(ptr)");
        _output.AppendLine("define internal i1 @luma_fs_to_wide(ptr %source, ptr %destination) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %count = call i32 @MultiByteToWideChar(i32 65001, i32 8, ptr %source, i32 -1, ptr %destination, i32 32768)");
        _output.AppendLine("  %valid = icmp sgt i32 %count, 0");
        _output.AppendLine("  ret i1 %valid");
        _output.AppendLine("}");
        _output.AppendLine("; Windows file-system operations are emitted by the runtime intrinsic backend.");
        EmitWindowsFileSystemOperations();
        EmitWindowsSpecialDirectories();
    }

    private void EmitWindowsSpecialDirectories()
    {
        foreach (var (name, api) in new[] { ("current_directory", "GetCurrentDirectoryW"), ("temporary_directory", "GetTempPathW") })
        {
            _output.AppendLine($"define internal ptr @luma_fs_{name}() {{");
            _output.AppendLine("entry:");
            _output.AppendLine("  %wide = alloca [32768 x i16], align 2");
            _output.AppendLine("  %wide.ptr = getelementptr [32768 x i16], ptr %wide, i32 0, i32 0");
            _output.AppendLine($"  %length = call i32 @{api}(i32 32768, ptr %wide.ptr)");
            _output.AppendLine("  %failed = icmp eq i32 %length, 0");
            _output.AppendLine("  br i1 %failed, label %error, label %convert");
            _output.AppendLine("convert:");
            _output.AppendLine("  %result = call ptr @luma_string_from_winwide(ptr %wide.ptr)");
            _output.AppendLine("  ret ptr %result");
            _output.AppendLine("error:");
            _output.AppendLine("  ret ptr @luma_string_empty");
            _output.AppendLine("}");
        }
    }

    private void EmitWindowsFileSystemOperations()
    {
        // Keep the native surface explicit; unsupported operations return failure.
        // File/path CRUD and stream primitives are emitted below using Win32 APIs.
        _output.AppendLine("define internal i32 @luma_fs_open(ptr %path, i32 %mode, ptr %handle) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %wide = alloca [32768 x i16], align 2");
        _output.AppendLine("  %wide.ptr = getelementptr [32768 x i16], ptr %wide, i32 0, i32 0");
        _output.AppendLine("  %converted = call i1 @luma_fs_to_wide(ptr %path, ptr %wide.ptr)");
        _output.AppendLine("  br i1 %converted, label %open, label %error");
        _output.AppendLine("open:");
        _output.AppendLine("  %read = icmp eq i32 %mode, 0");
        _output.AppendLine("  %write = icmp eq i32 %mode, 1");
        _output.AppendLine("  %append = icmp eq i32 %mode, 2");
        _output.AppendLine("  %create.new = icmp eq i32 %mode, 5");
        _output.AppendLine("  %access.first = select i1 %read, i32 2147483648, i32 -1073741824");
        _output.AppendLine("  %access.default = select i1 %append, i32 4, i32 %access.first");
        _output.AppendLine("  %creation.base = select i1 %write, i32 2, i32 3");
        _output.AppendLine("  %open.or.create = icmp eq i32 %mode, 3");
        _output.AppendLine("  %creation = select i1 %open.or.create, i32 4, i32 %creation.base");
        _output.AppendLine("  %create.for.append = select i1 %append, i32 4, i32 %creation");
        _output.AppendLine("  %both = icmp eq i32 %mode, 3");
        _output.AppendLine("  %access = select i1 %both, i32 -1073741824, i32 %access.default");
        _output.AppendLine("  %creation.new = select i1 %create.new, i32 1, i32 %create.for.append");
        _output.AppendLine("  %file = call ptr @CreateFileW(ptr %wide.ptr, i32 %access, i32 3, ptr null, i32 %creation.new, i32 128, ptr null)");
        _output.AppendLine($"  %failed = icmp eq ptr %file, inttoptr (i{compilation.NativePointerSize * 8} -1 to ptr)");
        _output.AppendLine("  br i1 %failed, label %error, label %success");
        _output.AppendLine("success:");
        _output.AppendLine("  store ptr %file, ptr %handle");
        _output.AppendLine("  ret i32 0");
        _output.AppendLine("error:");
        _output.AppendLine("  ret i32 -1");
        _output.AppendLine("}");
        _output.AppendLine("define internal i32 @luma_fs_read(ptr %handle, ptr %buffer, i32 %count, ptr %bytesRead) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %result = call i32 @ReadFile(ptr %handle, ptr %buffer, i32 %count, ptr %bytesRead, ptr null)");
        _output.AppendLine("  %ok = icmp ne i32 %result, 0");
        _output.AppendLine("  %status = select i1 %ok, i32 0, i32 -1");
        _output.AppendLine("  ret i32 %status");
        _output.AppendLine("}");
        _output.AppendLine("define internal i32 @luma_fs_write(ptr %handle, ptr %buffer, i32 %count, ptr %bytesWritten) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %result = call i32 @WriteFile(ptr %handle, ptr %buffer, i32 %count, ptr %bytesWritten, ptr null)");
        _output.AppendLine("  %ok = icmp ne i32 %result, 0");
        _output.AppendLine("  %status = select i1 %ok, i32 0, i32 -1");
        _output.AppendLine("  ret i32 %status");
        _output.AppendLine("}");
        _output.AppendLine("define internal i32 @luma_fs_seek(ptr %handle, i32 %offset, i32 %origin, ptr %position) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %offset.wide = sext i32 %offset to i64");
        _output.AppendLine("  %wide.position = alloca i64, align 8");
        _output.AppendLine("  %ok.native = call i32 @SetFilePointerEx(ptr %handle, i64 %offset.wide, ptr %wide.position, i32 %origin)");
        _output.AppendLine("  %ok = icmp ne i32 %ok.native, 0");
        _output.AppendLine("  br i1 %ok, label %save.position, label %error");
        _output.AppendLine("save.position:");
        _output.AppendLine("  %position.wide = load i64, ptr %wide.position, align 8");
        _output.AppendLine("  %position.narrow = trunc i64 %position.wide to i32");
        _output.AppendLine("  store i32 %position.narrow, ptr %position");
        _output.AppendLine("  ret i32 0");
        _output.AppendLine("error:");
        _output.AppendLine("  ret i32 -1");
        _output.AppendLine("}");
        _output.AppendLine("define internal i32 @luma_fs_set_length(ptr %handle, i32 %length) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %old.position = alloca i64, align 8");
        _output.AppendLine("  %new.position = alloca i64, align 8");
        _output.AppendLine("  %length.wide = zext i32 %length to i64");
        _output.AppendLine("  %read.position = call i32 @SetFilePointerEx(ptr %handle, i64 0, ptr %old.position, i32 1)");
        _output.AppendLine("  %read.ok = icmp ne i32 %read.position, 0");
        _output.AppendLine("  br i1 %read.ok, label %seek.end, label %error");
        _output.AppendLine("seek.end:");
        _output.AppendLine("  %seek.result = call i32 @SetFilePointerEx(ptr %handle, i64 %length.wide, ptr %new.position, i32 0)");
        _output.AppendLine("  %seek.ok = icmp ne i32 %seek.result, 0");
        _output.AppendLine("  br i1 %seek.ok, label %truncate, label %error");
        _output.AppendLine("truncate:");
        _output.AppendLine("  %truncated = call i32 @SetEndOfFile(ptr %handle)");
        _output.AppendLine("  %truncate.ok = icmp ne i32 %truncated, 0");
        _output.AppendLine("  %saved.position = load i64, ptr %old.position, align 8");
        _output.AppendLine("  %restored = call i32 @SetFilePointerEx(ptr %handle, i64 %saved.position, ptr null, i32 0)");
        _output.AppendLine("  %restore.ok = icmp ne i32 %restored, 0");
        _output.AppendLine("  %success = and i1 %truncate.ok, %restore.ok");
        _output.AppendLine("  %status = select i1 %success, i32 0, i32 -1");
        _output.AppendLine("  ret i32 %status");
        _output.AppendLine("error:");
        _output.AppendLine("  ret i32 -1");
        _output.AppendLine("}");
        _output.AppendLine("define internal i32 @luma_fs_flush(ptr %handle) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %result = call i32 @FlushFileBuffers(ptr %handle)");
        _output.AppendLine("  %ok = icmp ne i32 %result, 0");
        _output.AppendLine("  %status = select i1 %ok, i32 0, i32 -1");
        _output.AppendLine("  ret i32 %status");
        _output.AppendLine("}");
        _output.AppendLine("define internal i32 @luma_fs_close(ptr %handle) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %result = call i32 @CloseHandle(ptr %handle)");
        _output.AppendLine("  %ok = icmp ne i32 %result, 0");
        _output.AppendLine("  %status = select i1 %ok, i32 0, i32 -1");
        _output.AppendLine("  ret i32 %status");
        _output.AppendLine("}");
        EmitWindowsPathStatusFunctions();
    }

    private void EmitWindowsPathStatusFunctions()
    {
        _output.AppendLine("define internal i1 @luma_fs_directory_exists(ptr %path) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %wide = alloca [32768 x i16], align 2");
        _output.AppendLine("  %wide.ptr = getelementptr [32768 x i16], ptr %wide, i32 0, i32 0");
        _output.AppendLine("  %converted = call i1 @luma_fs_to_wide(ptr %path, ptr %wide.ptr)");
        _output.AppendLine("  br i1 %converted, label %check, label %no");
        _output.AppendLine("check:");
        _output.AppendLine("  %attributes = call i32 @GetFileAttributesW(ptr %wide.ptr)");
        _output.AppendLine("  %missing = icmp eq i32 %attributes, -1");
        _output.AppendLine("  %directory.bits = and i32 %attributes, 16");
        _output.AppendLine("  %is.directory = icmp ne i32 %directory.bits, 0");
        _output.AppendLine("  %exists = xor i1 %missing, true");
        _output.AppendLine("  %result = and i1 %exists, %is.directory");
        _output.AppendLine("  ret i1 %result");
        _output.AppendLine("no:");
        _output.AppendLine("  ret i1 false");
        _output.AppendLine("}");
        _output.AppendLine("define internal i1 @luma_fs_file_exists(ptr %path) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %wide = alloca [32768 x i16], align 2");
        _output.AppendLine("  %wide.ptr = getelementptr [32768 x i16], ptr %wide, i32 0, i32 0");
        _output.AppendLine("  %converted = call i1 @luma_fs_to_wide(ptr %path, ptr %wide.ptr)");
        _output.AppendLine("  br i1 %converted, label %check, label %no");
        _output.AppendLine("check:");
        _output.AppendLine("  %attributes = call i32 @GetFileAttributesW(ptr %wide.ptr)");
        _output.AppendLine("  %exists = icmp ne i32 %attributes, -1");
        _output.AppendLine("  %directory.bits = and i32 %attributes, 16");
        _output.AppendLine("  %is.directory = icmp ne i32 %directory.bits, 0");
        _output.AppendLine("  %is.file = xor i1 %is.directory, true");
        _output.AppendLine("  %result = and i1 %exists, %is.file");
        _output.AppendLine("  ret i1 %result");
        _output.AppendLine("no:");
        _output.AppendLine("  ret i1 false");
        _output.AppendLine("}");
        _output.AppendLine("define internal i32 @luma_fs_create_directory(ptr %path) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %wide = alloca [32768 x i16], align 2");
        _output.AppendLine("  %wide.ptr = getelementptr [32768 x i16], ptr %wide, i32 0, i32 0");
        _output.AppendLine("  %converted = call i1 @luma_fs_to_wide(ptr %path, ptr %wide.ptr)");
        _output.AppendLine("  br i1 %converted, label %create, label %error");
        _output.AppendLine("create:");
        _output.AppendLine("  %created.native = call i32 @CreateDirectoryW(ptr %wide.ptr, ptr null)");
        _output.AppendLine("  %created = icmp ne i32 %created.native, 0");
        _output.AppendLine("  br i1 %created, label %success, label %error");
        _output.AppendLine("success:");
        _output.AppendLine("  ret i32 0");
        _output.AppendLine("error:");
        _output.AppendLine("  ret i32 -1");
        _output.AppendLine("}");
        EmitWindowsOnePathFunction("delete_file", "DeleteFileW");
        EmitWindowsOnePathFunction("delete_directory", "RemoveDirectoryW");
        _output.AppendLine("define internal i32 @luma_fs_move(ptr %source, ptr %destination, i1 %overwrite) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %source.wide = alloca [32768 x i16], align 2");
        _output.AppendLine("  %destination.wide = alloca [32768 x i16], align 2");
        _output.AppendLine("  %source.ptr = getelementptr [32768 x i16], ptr %source.wide, i32 0, i32 0");
        _output.AppendLine("  %destination.ptr = getelementptr [32768 x i16], ptr %destination.wide, i32 0, i32 0");
        _output.AppendLine("  %source.ok = call i1 @luma_fs_to_wide(ptr %source, ptr %source.ptr)");
        _output.AppendLine("  %destination.ok = call i1 @luma_fs_to_wide(ptr %destination, ptr %destination.ptr)");
        _output.AppendLine("  %valid = and i1 %source.ok, %destination.ok");
        _output.AppendLine("  br i1 %valid, label %move, label %error");
        _output.AppendLine("move:");
        _output.AppendLine("  %flags = select i1 %overwrite, i32 1, i32 0");
        _output.AppendLine("  %moved.native = call i32 @MoveFileExW(ptr %source.ptr, ptr %destination.ptr, i32 %flags)");
        _output.AppendLine("  %moved = icmp ne i32 %moved.native, 0");
        _output.AppendLine("  %status = select i1 %moved, i32 0, i32 -1");
        _output.AppendLine("  ret i32 %status");
        _output.AppendLine("error:");
        _output.AppendLine("  ret i32 -1");
        _output.AppendLine("}");
        _output.AppendLine("define internal i32 @luma_fs_copy(ptr %source, ptr %destination, i1 %overwrite) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %source.wide = alloca [32768 x i16], align 2");
        _output.AppendLine("  %destination.wide = alloca [32768 x i16], align 2");
        _output.AppendLine("  %source.ptr = getelementptr [32768 x i16], ptr %source.wide, i32 0, i32 0");
        _output.AppendLine("  %destination.ptr = getelementptr [32768 x i16], ptr %destination.wide, i32 0, i32 0");
        _output.AppendLine("  %source.ok = call i1 @luma_fs_to_wide(ptr %source, ptr %source.ptr)");
        _output.AppendLine("  %destination.ok = call i1 @luma_fs_to_wide(ptr %destination, ptr %destination.ptr)");
        _output.AppendLine("  %valid = and i1 %source.ok, %destination.ok");
        _output.AppendLine("  br i1 %valid, label %copy, label %error");
        _output.AppendLine("copy:");
        _output.AppendLine("  %fail.if.exists = xor i1 %overwrite, true");
        _output.AppendLine("  %fail.if.exists.native = zext i1 %fail.if.exists to i32");
        _output.AppendLine("  %copied.native = call i32 @CopyFileW(ptr %source.ptr, ptr %destination.ptr, i32 %fail.if.exists.native)");
        _output.AppendLine("  %copied = icmp ne i32 %copied.native, 0");
        _output.AppendLine("  %status = select i1 %copied, i32 0, i32 -1");
        _output.AppendLine("  ret i32 %status");
        _output.AppendLine("error:");
        _output.AppendLine("  ret i32 -1");
        _output.AppendLine("}");
    }

    private void EmitWindowsOnePathFunction(string name, string api)
    {
        _output.AppendLine($"define internal i32 @luma_fs_{name}(ptr %path) {{");
        _output.AppendLine("entry:");
        _output.AppendLine("  %wide = alloca [32768 x i16], align 2");
        _output.AppendLine("  %wide.ptr = getelementptr [32768 x i16], ptr %wide, i32 0, i32 0");
        _output.AppendLine("  %converted = call i1 @luma_fs_to_wide(ptr %path, ptr %wide.ptr)");
        _output.AppendLine("  br i1 %converted, label %change, label %error");
        _output.AppendLine("change:");
        _output.AppendLine($"  %changed.native = call i32 @{api}(ptr %wide.ptr)");
        _output.AppendLine("  %changed = icmp ne i32 %changed.native, 0");
        _output.AppendLine("  %status = select i1 %changed, i32 0, i32 -1");
        _output.AppendLine("  ret i32 %status");
        _output.AppendLine("error:");
        _output.AppendLine("  ret i32 -1");
        _output.AppendLine("}");
    }

    private void EmitSimpleFileStatusFunction(string name, string returnType, string expression, string result)
    {
        _output.AppendLine($"define internal {returnType} @luma_fs_{name}(ptr %handle) {{");
        _output.AppendLine("entry:");
        _output.AppendLine($"  {result} = {expression}");
        _output.AppendLine($"  ret {returnType} {result}");
        _output.AppendLine("}");
    }

    private void EmitSimplePathStatusFunction(string name, string api, string parameters)
    {
        _output.AppendLine($"define internal i32 @luma_fs_{name}({parameters}) {{");
        _output.AppendLine("entry:");
        _output.AppendLine($"  %result = call i32 @{api}(ptr %path)");
        _output.AppendLine("  ret i32 %result");
        _output.AppendLine("}");
    }

}
