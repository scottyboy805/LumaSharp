namespace LumaSharp.Compiler;

internal sealed partial class LlvmEmitter
{
	private void EmitNativeStopWatchRuntime()
	{
		var targetTriple = compilation.TargetTriple == "host"
			? LlvmToolchain.GetHostTargetTriple() ?? string.Empty
			: compilation.TargetTriple;

		if (UsesWindowsCommandLineApi())
		{
			EmitWindowsStopWatchRuntime();
			return;
		}

		if (targetTriple.Contains("linux", StringComparison.OrdinalIgnoreCase) ||
			targetTriple.Contains("darwin", StringComparison.OrdinalIgnoreCase) ||
			targetTriple.Contains("apple", StringComparison.OrdinalIgnoreCase))
		{
			EmitPosixStopWatchRuntime(targetTriple);
			return;
		}

		_output.AppendLine("define internal i64 @luma_diag_timestamp() {");
		_output.AppendLine("entry:");
		_output.AppendLine("  ret i64 0");
		_output.AppendLine("}");
		_output.AppendLine("define internal i64 @luma_diag_frequency() {");
		_output.AppendLine("entry:");
		_output.AppendLine("  ret i64 1");
		_output.AppendLine("}");
	}

	private void EmitWindowsStopWatchRuntime()
	{
		_output.AppendLine("declare i32 @QueryPerformanceCounter(ptr)");
		_output.AppendLine("declare i32 @QueryPerformanceFrequency(ptr)");
		_output.AppendLine("define internal i64 @luma_diag_timestamp() {");
		_output.AppendLine("entry:");
		_output.AppendLine("  %counter.address = alloca i64, align 8");
		_output.AppendLine("  %counter.status = call i32 @QueryPerformanceCounter(ptr %counter.address)");
		_output.AppendLine("  %counter.failed = icmp eq i32 %counter.status, 0");
		_output.AppendLine("  br i1 %counter.failed, label %error, label %success");
		_output.AppendLine("success:");
		_output.AppendLine("  %counter = load i64, ptr %counter.address, align 8");
		_output.AppendLine("  ret i64 %counter");
		_output.AppendLine("error:");
		_output.AppendLine("  ret i64 0");
		_output.AppendLine("}");
		_output.AppendLine("define internal i64 @luma_diag_frequency() {");
		_output.AppendLine("entry:");
		_output.AppendLine("  %frequency.address = alloca i64, align 8");
		_output.AppendLine("  %status = call i32 @QueryPerformanceFrequency(ptr %frequency.address)");
		_output.AppendLine("  %failed = icmp eq i32 %status, 0");
		_output.AppendLine("  br i1 %failed, label %error, label %success");
		_output.AppendLine("success:");
		_output.AppendLine("  %frequency = load i64, ptr %frequency.address, align 8");
		_output.AppendLine("  %invalid = icmp sle i64 %frequency, 0");
		_output.AppendLine("  br i1 %invalid, label %error, label %return.frequency");
		_output.AppendLine("return.frequency:");
		_output.AppendLine("  ret i64 %frequency");
		_output.AppendLine("error:");
		_output.AppendLine("  ret i64 1");
		_output.AppendLine("}");
	}

	private void EmitPosixStopWatchRuntime(string targetTriple)
	{
		var clockId = targetTriple.Contains("darwin", StringComparison.OrdinalIgnoreCase) ||
			targetTriple.Contains("apple", StringComparison.OrdinalIgnoreCase) ? 6 : 1;
		var timeType = compilation.NativePointerSize == 8 ? "i64" : "i32";
		var alignment = compilation.NativePointerSize;
		_output.AppendLine("declare i32 @clock_gettime(i32, ptr)");
		_output.AppendLine("define internal i64 @luma_diag_timestamp() {");
		_output.AppendLine("entry:");
		_output.AppendLine($"  %time.address = alloca {{ {timeType}, {timeType} }}, align {alignment}");
		_output.AppendLine($"  %status = call i32 @clock_gettime(i32 {clockId}, ptr %time.address)");
		_output.AppendLine("  %failed = icmp ne i32 %status, 0");
		_output.AppendLine("  br i1 %failed, label %error, label %convert");
		_output.AppendLine("convert:");
		_output.AppendLine($"  %seconds.address = getelementptr {{ {timeType}, {timeType} }}, ptr %time.address, i32 0, i32 0");
		_output.AppendLine($"  %nanoseconds.address = getelementptr {{ {timeType}, {timeType} }}, ptr %time.address, i32 0, i32 1");
		_output.AppendLine($"  %seconds.native = load {timeType}, ptr %seconds.address, align {alignment}");
		_output.AppendLine($"  %nanoseconds.native = load {timeType}, ptr %nanoseconds.address, align {alignment}");
		if (timeType == "i32")
		{
			_output.AppendLine("  %seconds = sext i32 %seconds.native to i64");
			_output.AppendLine("  %nanoseconds = sext i32 %nanoseconds.native to i64");
		}
		else
		{
			_output.AppendLine("  %seconds = add i64 %seconds.native, 0");
			_output.AppendLine("  %nanoseconds = add i64 %nanoseconds.native, 0");
		}
		_output.AppendLine("  %whole.nanoseconds = mul i64 %seconds, 1000000000");
		_output.AppendLine("  %timestamp = add i64 %whole.nanoseconds, %nanoseconds");
		_output.AppendLine("  ret i64 %timestamp");
		_output.AppendLine("error:");
		_output.AppendLine("  ret i64 0");
		_output.AppendLine("}");
		_output.AppendLine("define internal i64 @luma_diag_frequency() {");
		_output.AppendLine("entry:");
		_output.AppendLine("  ret i64 1000000000");
		_output.AppendLine("}");
	}
}
