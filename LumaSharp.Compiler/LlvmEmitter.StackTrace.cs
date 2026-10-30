namespace LumaSharp.Compiler;

internal sealed partial class LlvmEmitter
{
    private void EmitNativeStackTraceRuntime()
    {
        var sizeType = compilation.NativePointerSize == 8 ? "i64" : "i32";
        if (!compilation.ClassTypes.TryGetValue("System.Exception", out var exceptionType))
            throw new InvalidOperationException("Stack trace support requires System.Exception.");
        var exceptionField = exceptionType.Fields.FirstOrDefault(field => field.Declaration.Name == "stackTrace")
            ?? throw new InvalidOperationException("Stack trace support requires the System.Exception.stackTrace field.");

        EmitUtf8CConstant("@luma_stack_prefix", "at ");
        EmitUtf8CConstant("@luma_stack_newline", "\n");
        EmitStackTraceFrameCountRuntime();
        EmitStackTraceCaptureRuntime(sizeType);

        _output.AppendLine("define internal void @luma_exception_capture_stacktrace(ptr %exception, i1 %overwrite) {");
        _output.AppendLine("entry:");
        _output.AppendLine($"  %trace.address = getelementptr %class.{Safe("System.Exception")}, ptr %exception, i32 0, i32 {exceptionField.Index + 4}");
        _output.AppendLine("  %existing.trace = load ptr, ptr %trace.address");
        _output.AppendLine("  %has.trace = icmp ne ptr %existing.trace, null");
        _output.AppendLine("  %needs.capture = xor i1 %has.trace, true");
        _output.AppendLine("  %should.capture = or i1 %overwrite, %needs.capture");
        _output.AppendLine("  br i1 %should.capture, label %capture, label %done");
        _output.AppendLine("capture:");
        _output.AppendLine("  br i1 %has.trace, label %release.old, label %create");
        _output.AppendLine("release.old:");
        _output.AppendLine("  call void @luma_string_release(ptr %existing.trace)");
        _output.AppendLine("  br label %create");
        _output.AppendLine("create:");
        _output.AppendLine("  %trace = call ptr @luma_stacktrace_capture(i32 0)");
        _output.AppendLine("  store ptr %trace, ptr %trace.address");
        _output.AppendLine("  br label %done");
        _output.AppendLine("done:");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");
    }

    private void EmitStackTraceFrameCountRuntime()
    {
        _output.AppendLine("define internal i32 @luma_stacktrace_frame_count(i32 %frames.to.skip) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %frame.slot = alloca ptr");
        _output.AppendLine("  %skip.slot = alloca i32");
        _output.AppendLine("  %initial.frame = load ptr, ptr @luma_stack_current");
        _output.AppendLine("  store ptr %initial.frame, ptr %frame.slot");
        _output.AppendLine("  store i32 %frames.to.skip, ptr %skip.slot");
        _output.AppendLine("  br label %skip.check");
        _output.AppendLine("skip.check:");
        _output.AppendLine("  %skip.frame = load ptr, ptr %frame.slot");
        _output.AppendLine("  %skip.remaining = load i32, ptr %skip.slot");
        _output.AppendLine("  %skip.has.frame = icmp ne ptr %skip.frame, null");
        _output.AppendLine("  %skip.has.count = icmp sgt i32 %skip.remaining, 0");
        _output.AppendLine("  %skip.next = and i1 %skip.has.frame, %skip.has.count");
        _output.AppendLine("  br i1 %skip.next, label %skip.body, label %count.start");
        _output.AppendLine("skip.body:");
        _output.AppendLine("  %skip.previous.address = getelementptr %luma.stackframe, ptr %skip.frame, i32 0, i32 0");
        _output.AppendLine("  %skip.previous = load ptr, ptr %skip.previous.address");
        _output.AppendLine("  store ptr %skip.previous, ptr %frame.slot");
        _output.AppendLine("  %skip.decremented = sub i32 %skip.remaining, 1");
        _output.AppendLine("  store i32 %skip.decremented, ptr %skip.slot");
        _output.AppendLine("  br label %skip.check");
        _output.AppendLine("count.start:");
        _output.AppendLine("  %count.initial.frame = load ptr, ptr %frame.slot");
        _output.AppendLine("  br label %count.loop");
        _output.AppendLine("count.loop:");
        _output.AppendLine("  %frame = phi ptr [ %count.initial.frame, %count.start ], [ %previous, %count.body ]");
        _output.AppendLine("  %count = phi i32 [ 0, %count.start ], [ %next.count, %count.body ]");
        _output.AppendLine("  %finished = icmp eq ptr %frame, null");
        _output.AppendLine("  br i1 %finished, label %done, label %count.body");
        _output.AppendLine("count.body:");
        _output.AppendLine("  %previous.address = getelementptr %luma.stackframe, ptr %frame, i32 0, i32 0");
        _output.AppendLine("  %previous = load ptr, ptr %previous.address");
        _output.AppendLine("  %next.count = add i32 %count, 1");
        _output.AppendLine("  br label %count.loop");
        _output.AppendLine("done:");
        _output.AppendLine("  ret i32 %count");
        _output.AppendLine("}");
    }

    private void EmitStackTraceCaptureRuntime(string sizeType)
    {
        const int bufferCapacity = 65536;
        var capacityConstant = sizeType == "i64" ? $"i64 {bufferCapacity}" : $"i32 {bufferCapacity}";
        var pointerOffsetType = sizeType;
        _output.AppendLine($"define internal ptr @luma_stacktrace_capture(i32 %frames.to.skip) {{");
        _output.AppendLine("entry:");
        _output.AppendLine($"  %buffer = call ptr @malloc({capacityConstant})");
        _output.AppendLine("  %buffer.failed = icmp eq ptr %buffer, null");
        _output.AppendLine("  br i1 %buffer.failed, label %allocation.failed, label %initialize");
        _output.AppendLine("initialize:");
        _output.AppendLine("  %frame.slot = alloca ptr");
        _output.AppendLine("  %skip.slot = alloca i32");
        _output.AppendLine("  %initial.frame = load ptr, ptr @luma_stack_current");
        _output.AppendLine("  store ptr %initial.frame, ptr %frame.slot");
        _output.AppendLine("  store i32 %frames.to.skip, ptr %skip.slot");
        _output.AppendLine("  br label %skip.check");
        _output.AppendLine("skip.check:");
        _output.AppendLine("  %skip.frame = load ptr, ptr %frame.slot");
        _output.AppendLine("  %skip.remaining = load i32, ptr %skip.slot");
        _output.AppendLine("  %skip.has.frame = icmp ne ptr %skip.frame, null");
        _output.AppendLine("  %skip.has.count = icmp sgt i32 %skip.remaining, 0");
        _output.AppendLine("  %skip.next = and i1 %skip.has.frame, %skip.has.count");
        _output.AppendLine("  br i1 %skip.next, label %skip.body, label %copy.start");
        _output.AppendLine("skip.body:");
        _output.AppendLine("  %skip.previous.address = getelementptr %luma.stackframe, ptr %skip.frame, i32 0, i32 0");
        _output.AppendLine("  %skip.previous = load ptr, ptr %skip.previous.address");
        _output.AppendLine("  store ptr %skip.previous, ptr %frame.slot");
        _output.AppendLine("  %skip.decremented = sub i32 %skip.remaining, 1");
        _output.AppendLine("  store i32 %skip.decremented, ptr %skip.slot");
        _output.AppendLine("  br label %skip.check");
        _output.AppendLine("copy.start:");
        _output.AppendLine("  %copy.initial.frame = load ptr, ptr %frame.slot");
        _output.AppendLine("  %prefix = getelementptr [4 x i8], ptr @luma_stack_prefix, i32 0, i32 0");
        _output.AppendLine("  %newline = getelementptr [2 x i8], ptr @luma_stack_newline, i32 0, i32 0");
        _output.AppendLine("  br label %copy.loop");
        _output.AppendLine("copy.loop:");
        _output.AppendLine("  %frame = phi ptr [ %copy.initial.frame, %copy.start ], [ %next.frame, %copy.body ]");
        _output.AppendLine("  %offset = phi i32 [ 0, %copy.start ], [ %next.offset, %copy.body ]");
        _output.AppendLine("  %finished = icmp eq ptr %frame, null");
        _output.AppendLine("  br i1 %finished, label %copy.done, label %copy.check");
        _output.AppendLine("copy.check:");
        _output.AppendLine("  %name.address = getelementptr %luma.stackframe, ptr %frame, i32 0, i32 1");
        _output.AppendLine("  %name = load ptr, ptr %name.address");
        _output.AppendLine("  %name.length.address = getelementptr %luma.stackframe, ptr %frame, i32 0, i32 2");
        _output.AppendLine("  %name.length = load i32, ptr %name.length.address");
        _output.AppendLine("  %record.length = add i32 %name.length, 4");
        _output.AppendLine("  %next.offset = add i32 %offset, %record.length");
        _output.AppendLine($"  %fits = icmp ule i32 %next.offset, {bufferCapacity - 1}");
        _output.AppendLine("  br i1 %fits, label %copy.body, label %copy.done");
        _output.AppendLine("copy.body:");
        _output.AppendLine("  %prefix.destination = getelementptr i8, ptr %buffer, i32 %offset");
        _output.AppendLine($"  %prefix.copy = call ptr @memcpy(ptr %prefix.destination, ptr %prefix, {pointerOffsetType} 3)");
        _output.AppendLine("  %name.offset = add i32 %offset, 3");
        _output.AppendLine("  %name.destination = getelementptr i8, ptr %buffer, i32 %name.offset");
        _output.AppendLine($"  %name.length.native = {(sizeType == "i64" ? "zext i32 %name.length to i64" : "add i32 %name.length, 0")}");
        _output.AppendLine($"  %name.copy = call ptr @memcpy(ptr %name.destination, ptr %name, {pointerOffsetType} %name.length.native)");
        _output.AppendLine("  %newline.offset = sub i32 %next.offset, 1");
        _output.AppendLine("  %newline.destination = getelementptr i8, ptr %buffer, i32 %newline.offset");
        _output.AppendLine("  store i8 10, ptr %newline.destination");
        _output.AppendLine("  %next.frame.address = getelementptr %luma.stackframe, ptr %frame, i32 0, i32 0");
        _output.AppendLine("  %next.frame = load ptr, ptr %next.frame.address");
        _output.AppendLine("  br label %copy.loop");
        _output.AppendLine("copy.done:");
        _output.AppendLine("  %terminator = getelementptr i8, ptr %buffer, i32 %offset");
        _output.AppendLine("  store i8 0, ptr %terminator");
        _output.AppendLine("  %trace = call ptr @luma_string_from_utf8z(ptr %buffer)");
        _output.AppendLine("  call void @free(ptr %buffer)");
        _output.AppendLine("  ret ptr %trace");
        _output.AppendLine("allocation.failed:");
        _output.AppendLine("  ret ptr @luma_string_empty");
        _output.AppendLine("}");
    }
}
