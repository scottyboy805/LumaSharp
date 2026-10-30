namespace LumaSharp.Compiler;

internal sealed partial class LlvmEmitter
{
	private static bool IsNativeEnvironmentShim(MethodSymbol method) =>
		method.Declaration.IsExtern &&
		method.Owner.Namespace == "System" &&
		method.Owner.Name == "Environment" &&
		method.Declaration.Name.StartsWith("luma_env_", StringComparison.Ordinal);

	private void EmitNativeEnvironmentRuntime()
	{
		var targetTriple = compilation.TargetTriple == "host"
			? LlvmToolchain.GetHostTargetTriple() ?? string.Empty
			: compilation.TargetTriple;
		var sizeType = compilation.NativePointerSize == 8 ? "i64" : "i32";
		var platform = GetEnvironmentPlatform(targetTriple);
		var architecture = GetEnvironmentArchitecture(targetTriple);

		if (UsesWindowsCommandLineApi())
		{
			EmitWindowsEnvironmentRuntime();
		}
		else if (targetTriple.Contains("darwin", StringComparison.OrdinalIgnoreCase) ||
			targetTriple.Contains("apple", StringComparison.OrdinalIgnoreCase))
		{
			EmitMacOSEnvironmentRuntime(sizeType);
		}
		else
		{
			EmitPosixEnvironmentRuntime(sizeType, targetTriple);
		}

		_output.AppendLine("define internal i1 @luma_env_is_little_endian() {");
		_output.AppendLine("entry:");
		_output.AppendLine("  %value = alloca i32, align 4");
		_output.AppendLine("  store i32 1, ptr %value, align 4");
		_output.AppendLine("  %first.byte = load i8, ptr %value, align 1");
		_output.AppendLine("  %little.endian = icmp eq i8 %first.byte, 1");
		_output.AppendLine("  ret i1 %little.endian");
		_output.AppendLine("}");
		EmitEnvironmentConstant("platform", platform);
		EmitEnvironmentConstant("architecture", architecture);
	}

	private void EmitWindowsEnvironmentRuntime()
	{
		_output.AppendLine("declare i32 @GetModuleFileNameW(ptr, ptr, i32)");
		_output.AppendLine("declare i32 @GetActiveProcessorCount(i16)");
		_output.AppendLine("define internal ptr @luma_env_executable_path() {");
		_output.AppendLine("entry:");
		_output.AppendLine("  %buffer = alloca [32768 x i16], align 2");
		_output.AppendLine("  %buffer.ptr = getelementptr [32768 x i16], ptr %buffer, i32 0, i32 0");
		_output.AppendLine("  %length = call i32 @GetModuleFileNameW(ptr null, ptr %buffer.ptr, i32 32768)");
		_output.AppendLine("  %failed = icmp eq i32 %length, 0");
		_output.AppendLine("  br i1 %failed, label %error, label %convert");
		_output.AppendLine("convert:");
		_output.AppendLine("  %result = call ptr @luma_string_from_winwide(ptr %buffer.ptr)");
		_output.AppendLine("  ret ptr %result");
		_output.AppendLine("error:");
		_output.AppendLine("  ret ptr @luma_string_empty");
		_output.AppendLine("}");
		_output.AppendLine("define internal i32 @luma_env_processor_count() {");
		_output.AppendLine("entry:");
		_output.AppendLine("  %count = call i32 @GetActiveProcessorCount(i16 -1)");
		_output.AppendLine("  ret i32 %count");
		_output.AppendLine("}");
		EmitWindowsExecutableSegment("directory", isDirectory: true);
		EmitWindowsExecutableSegment("name", isDirectory: false);
	}

	private void EmitPosixEnvironmentRuntime(string sizeType, string targetTriple)
	{
		if (targetTriple.Contains("linux", StringComparison.OrdinalIgnoreCase))
		{
			_output.AppendLine("@luma_env_executable_link = private unnamed_addr constant [15 x i8] c\"/proc/self/exe\\00\"");
			_output.AppendLine($"declare {sizeType} @readlink(ptr, ptr, {sizeType})");
			_output.AppendLine("declare i32 @get_nprocs()");
			_output.AppendLine("define internal ptr @luma_env_executable_path() {");
			_output.AppendLine("entry:");
			_output.AppendLine("  %buffer = alloca [32768 x i8], align 16");
			_output.AppendLine("  %buffer.ptr = getelementptr [32768 x i8], ptr %buffer, i32 0, i32 0");
			_output.AppendLine("  %path = getelementptr [15 x i8], ptr @luma_env_executable_link, i32 0, i32 0");
			_output.AppendLine($"  %length = call {sizeType} @readlink(ptr %path, ptr %buffer.ptr, {sizeType} 32767)");
			_output.AppendLine($"  %failed = icmp slt {sizeType} %length, 0");
			_output.AppendLine("  br i1 %failed, label %error, label %convert");
			_output.AppendLine("convert:");
			_output.AppendLine($"  %terminator = getelementptr i8, ptr %buffer.ptr, {sizeType} %length");
			_output.AppendLine("  store i8 0, ptr %terminator, align 1");
			_output.AppendLine("  %result = call ptr @luma_string_from_utf8z(ptr %buffer.ptr)");
			_output.AppendLine("  ret ptr %result");
			_output.AppendLine("error:");
			_output.AppendLine("  ret ptr @luma_string_empty");
			_output.AppendLine("}");
			_output.AppendLine("define internal i32 @luma_env_processor_count() {");
			_output.AppendLine("entry:");
			_output.AppendLine("  %count = call i32 @get_nprocs()");
			_output.AppendLine("  ret i32 %count");
			_output.AppendLine("}");
			EmitPosixExecutableSegment("directory", sizeType);
			EmitPosixExecutableSegment("name", sizeType);
			return;
		}

		_output.AppendLine("define internal ptr @luma_env_executable_path() {");
		_output.AppendLine("entry:");
		_output.AppendLine("  ret ptr @luma_string_empty");
		_output.AppendLine("}");
		_output.AppendLine("define internal i32 @luma_env_processor_count() {");
		_output.AppendLine("entry:");
		_output.AppendLine("  ret i32 1");
		_output.AppendLine("}");
		EmitFallbackExecutableSegment("directory");
		EmitFallbackExecutableSegment("name");
	}

	private void EmitMacOSEnvironmentRuntime(string sizeType)
	{
		_output.AppendLine("@luma_env_logical_cpu_name = private unnamed_addr constant [14 x i8] c\"hw.logicalcpu\\00\"");
		_output.AppendLine("declare i32 @_NSGetExecutablePath(ptr, ptr)");
		_output.AppendLine($"declare i32 @sysctlbyname(ptr, ptr, ptr, ptr, {sizeType})");
		_output.AppendLine($"declare {sizeType} @strlen(ptr)");
		_output.AppendLine("define internal ptr @luma_env_executable_path() {");
		_output.AppendLine("entry:");
		_output.AppendLine("  %buffer = alloca [32768 x i8], align 16");
		_output.AppendLine("  %buffer.ptr = getelementptr [32768 x i8], ptr %buffer, i32 0, i32 0");
		_output.AppendLine("  %buffer.length = alloca i32, align 4");
		_output.AppendLine("  store i32 32768, ptr %buffer.length, align 4");
		_output.AppendLine("  %status = call i32 @_NSGetExecutablePath(ptr %buffer.ptr, ptr %buffer.length)");
		_output.AppendLine("  %failed = icmp ne i32 %status, 0");
		_output.AppendLine("  br i1 %failed, label %error, label %convert");
		_output.AppendLine("convert:");
		_output.AppendLine("  %result = call ptr @luma_string_from_utf8z(ptr %buffer.ptr)");
		_output.AppendLine("  ret ptr %result");
		_output.AppendLine("error:");
		_output.AppendLine("  ret ptr @luma_string_empty");
		_output.AppendLine("}");
		_output.AppendLine("define internal i32 @luma_env_processor_count() {");
		_output.AppendLine("entry:");
		_output.AppendLine("  %name = getelementptr [14 x i8], ptr @luma_env_logical_cpu_name, i32 0, i32 0");
		_output.AppendLine("  %count = alloca i32, align 4");
		_output.AppendLine($"  %length = alloca {sizeType}, align {compilation.NativePointerSize}");
		_output.AppendLine($"  store {sizeType} 4, ptr %length, align {compilation.NativePointerSize}");
		_output.AppendLine($"  %status = call i32 @sysctlbyname(ptr %name, ptr %count, ptr %length, ptr null, {sizeType} 0)");
		_output.AppendLine("  %failed = icmp ne i32 %status, 0");
		_output.AppendLine("  br i1 %failed, label %error, label %success");
		_output.AppendLine("success:");
		_output.AppendLine("  %result = load i32, ptr %count, align 4");
		_output.AppendLine("  ret i32 %result");
		_output.AppendLine("error:");
		_output.AppendLine("  ret i32 1");
		_output.AppendLine("}");
		EmitMacOSExecutableSegment("directory", sizeType);
		EmitMacOSExecutableSegment("name", sizeType);
	}

	private void EmitWindowsExecutableSegment(string name, bool isDirectory)
	{
		_output.AppendLine($"define internal ptr @luma_env_executable_{name}() {{");
		_output.AppendLine("entry:");
		_output.AppendLine("  %buffer = alloca [32768 x i16], align 2");
		_output.AppendLine("  %buffer.ptr = getelementptr [32768 x i16], ptr %buffer, i32 0, i32 0");
		_output.AppendLine("  %length = call i32 @GetModuleFileNameW(ptr null, ptr %buffer.ptr, i32 32768)");
		_output.AppendLine("  %invalid.low = icmp eq i32 %length, 0");
		_output.AppendLine("  %invalid.high = icmp uge i32 %length, 32768");
		_output.AppendLine("  %invalid = or i1 %invalid.low, %invalid.high");
		_output.AppendLine("  br i1 %invalid, label %error, label %scan");
		EmitExecutablePathScan(isDirectory, isWide: true, "entry");
		_output.AppendLine("}");
	}

	private void EmitPosixExecutableSegment(string name, string sizeType)
	{
		var isDirectory = name == "directory";
		_output.AppendLine($"define internal ptr @luma_env_executable_{name}() {{");
		_output.AppendLine("entry:");
		_output.AppendLine("  %buffer = alloca [32768 x i8], align 16");
		_output.AppendLine("  %buffer.ptr = getelementptr [32768 x i8], ptr %buffer, i32 0, i32 0");
		_output.AppendLine("  %path = getelementptr [15 x i8], ptr @luma_env_executable_link, i32 0, i32 0");
		_output.AppendLine($"  %length.native = call {sizeType} @readlink(ptr %path, ptr %buffer.ptr, {sizeType} 32767)");
		_output.AppendLine($"  %failed = icmp slt {sizeType} %length.native, 0");
		_output.AppendLine("  br i1 %failed, label %error, label %convert");
		_output.AppendLine("convert:");
		if (sizeType == "i32")
			_output.AppendLine("  %length = add i32 %length.native, 0");
		else
			_output.AppendLine("  %length = trunc i64 %length.native to i32");
		_output.AppendLine("  %invalid = icmp uge i32 %length, 32767");
		_output.AppendLine("  br i1 %invalid, label %error, label %terminate");
		_output.AppendLine("terminate:");
		_output.AppendLine($"  %terminator = getelementptr i8, ptr %buffer.ptr, {sizeType} %length.native");
		_output.AppendLine("  store i8 0, ptr %terminator, align 1");
		_output.AppendLine("  br label %scan");
		EmitExecutablePathScan(isDirectory, isWide: false, "terminate");
		_output.AppendLine("}");
	}

	private void EmitMacOSExecutableSegment(string name, string sizeType)
	{
		var isDirectory = name == "directory";
		_output.AppendLine($"define internal ptr @luma_env_executable_{name}() {{");
		_output.AppendLine("entry:");
		_output.AppendLine("  %buffer = alloca [32768 x i8], align 16");
		_output.AppendLine("  %buffer.ptr = getelementptr [32768 x i8], ptr %buffer, i32 0, i32 0");
		_output.AppendLine("  %buffer.length = alloca i32, align 4");
		_output.AppendLine("  store i32 32768, ptr %buffer.length, align 4");
		_output.AppendLine("  %status = call i32 @_NSGetExecutablePath(ptr %buffer.ptr, ptr %buffer.length)");
		_output.AppendLine("  %failed = icmp ne i32 %status, 0");
		_output.AppendLine("  br i1 %failed, label %error, label %measure");
		_output.AppendLine("measure:");
		_output.AppendLine($"  %length.native = call {sizeType} @strlen(ptr %buffer.ptr)");
		if (sizeType == "i32")
			_output.AppendLine("  %length = add i32 %length.native, 0");
		else
			_output.AppendLine("  %length = trunc i64 %length.native to i32");
		_output.AppendLine("  br label %scan");
		EmitExecutablePathScan(isDirectory, isWide: false, "measure");
		_output.AppendLine("}");
	}

	private void EmitFallbackExecutableSegment(string name)
	{
		_output.AppendLine($"define internal ptr @luma_env_executable_{name}() {{");
		_output.AppendLine("entry:");
		_output.AppendLine("  ret ptr @luma_string_empty");
		_output.AppendLine("}");
	}

	private void EmitExecutablePathScan(bool isDirectory, bool isWide, string scanPredecessor)
	{
		var elementType = isWide ? "i16" : "i8";
		_output.AppendLine("scan:");
		_output.AppendLine($"  %index = phi i32 [ 0, %{scanPredecessor} ], [ %next, %scan.body ]");
		_output.AppendLine($"  %last.separator = phi i32 [ -1, %{scanPredecessor} ], [ %last.separator.next, %scan.body ]");
		_output.AppendLine("  %has.more = icmp ult i32 %index, %length");
		_output.AppendLine("  br i1 %has.more, label %scan.body, label %scan.done");
		_output.AppendLine("scan.body:");
		_output.AppendLine($"  %character.ptr = getelementptr {elementType}, ptr %buffer.ptr, i32 %index");
		_output.AppendLine($"  %character = load {elementType}, ptr %character.ptr, align { (isWide ? 2 : 1) }");
		_output.AppendLine($"  %slash = icmp eq {elementType} %character, 47");
		_output.AppendLine($"  %backslash = icmp eq {elementType} %character, 92");
		_output.AppendLine("  %separator = or i1 %slash, %backslash");
		_output.AppendLine("  %last.separator.next = select i1 %separator, i32 %index, i32 %last.separator");
		_output.AppendLine("  %next = add i32 %index, 1");
		_output.AppendLine("  br label %scan");
		_output.AppendLine("scan.done:");
		if (isDirectory)
		{
			_output.AppendLine("  %has.separator = icmp sge i32 %last.separator, 0");
			_output.AppendLine("  br i1 %has.separator, label %make.segment, label %error");
			_output.AppendLine("make.segment:");
			_output.AppendLine("  %is.root.separator = icmp eq i32 %last.separator, 0");
			if (isWide)
			{
				_output.AppendLine("  %drive.character.ptr = getelementptr i16, ptr %buffer.ptr, i32 1");
				_output.AppendLine("  %drive.character = load i16, ptr %drive.character.ptr, align 2");
				_output.AppendLine("  %drive.colon = icmp eq i16 %drive.character, 58");
				_output.AppendLine("  %drive.separator = icmp eq i32 %last.separator, 2");
				_output.AppendLine("  %is.drive.root = and i1 %drive.colon, %drive.separator");
				_output.AppendLine("  %is.root = or i1 %is.root.separator, %is.drive.root");
			}
			else
			{
				_output.AppendLine("  %is.root = or i1 %is.root.separator, false");
			}
			_output.AppendLine("  %after.separator = add i32 %last.separator, 1");
			_output.AppendLine("  %terminator.index = select i1 %is.root, i32 %after.separator, i32 %last.separator");
			_output.AppendLine($"  %segment.terminator = getelementptr {elementType}, ptr %buffer.ptr, i32 %terminator.index");
			_output.AppendLine($"  store {elementType} 0, ptr %segment.terminator, align {(isWide ? 2 : 1)}");
			_output.AppendLine("  br label %create.string");
			_output.AppendLine("create.string:");
			_output.AppendLine(isWide
				? "  %result = call ptr @luma_string_from_winwide(ptr %buffer.ptr)"
				: "  %result = call ptr @luma_string_from_utf8z(ptr %buffer.ptr)");
		}
		else
		{
			_output.AppendLine("  %after.separator = add i32 %last.separator, 1");
			_output.AppendLine("  %has.name.separator = icmp sge i32 %last.separator, 0");
			_output.AppendLine("  %start = select i1 %has.name.separator, i32 %after.separator, i32 0");
			_output.AppendLine($"  %name.ptr = getelementptr {elementType}, ptr %buffer.ptr, i32 %start");
			_output.AppendLine(isWide
				? "  %result = call ptr @luma_string_from_winwide(ptr %name.ptr)"
				: "  %result = call ptr @luma_string_from_utf8z(ptr %name.ptr)");
		}
		_output.AppendLine("  ret ptr %result");
		_output.AppendLine("error:");
		_output.AppendLine("  ret ptr @luma_string_empty");
	}

	private void EmitEnvironmentConstant(string name, int value)
	{
		_output.AppendLine($"define internal i32 @luma_env_{name}() {{");
		_output.AppendLine("entry:");
		_output.AppendLine($"  ret i32 {value}");
		_output.AppendLine("}");
	}

	private static int GetEnvironmentPlatform(string targetTriple)
	{
		if (targetTriple.Contains("windows", StringComparison.OrdinalIgnoreCase) ||
			targetTriple.Contains("win32", StringComparison.OrdinalIgnoreCase) ||
			targetTriple.Contains("mingw", StringComparison.OrdinalIgnoreCase)) return 1;
		if (targetTriple.Contains("linux", StringComparison.OrdinalIgnoreCase)) return 2;
		if (targetTriple.Contains("darwin", StringComparison.OrdinalIgnoreCase) ||
			targetTriple.Contains("apple", StringComparison.OrdinalIgnoreCase)) return 3;
		return 0;
	}

	private static int GetEnvironmentArchitecture(string targetTriple)
	{
		var architecture = targetTriple.Split('-', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
		if (architecture is "x86_64" or "amd64") return 2;
		if (architecture is "i386" or "i486" or "i586" or "i686" or "x86") return 1;
		if (architecture is "aarch64" or "arm64") return 4;
		if (architecture.StartsWith("arm", StringComparison.OrdinalIgnoreCase)) return 3;
		return 0;
	}
}
