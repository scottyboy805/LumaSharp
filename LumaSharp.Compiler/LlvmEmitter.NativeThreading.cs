namespace LumaSharp.Compiler;

internal sealed partial class LlvmEmitter
{
	private void EmitNativeThreadRuntime()
	{
		if (!compilation.DelegateTypes.ContainsKey("System.Threading.ThreadStart"))
			throw new InvalidOperationException("Threading intrinsics require System.Threading.ThreadStart.");

		var pointerBits = compilation.NativePointerSize * 8;
		var sizeType = $"i{pointerBits}";
		_output.AppendLine($"%luma.thread_state = type {{ ptr, ptr, ptr, ptr, {sizeType}, i32, i32, i32 }}");
		var windows = UsesWindowsCommandLineApi();
		EmitThreadEntryPoint(windows);
		foreach (var (name, type) in compilation.DelegateTypes.OrderBy(pair => pair.Key, StringComparer.Ordinal))
			if (type.DelegateReturnType == "void" && (type.DelegateParameters?.Count ?? 0) <= 1 &&
				(type.DelegateParameters?.All(parameter => IsSupportedThreadArgument(parameter.TypeName)) ?? true))
				EmitThreadInvoker(name, type);

		if (windows)
			EmitWindowsThreadRuntime(sizeType);
		else
			EmitPosixThreadRuntime(sizeType);
		EmitThreadCurrentRuntime(sizeType);
	}

	private void EmitThreadEntryPoint(bool windows)
	{
		var callingConvention = windows && compilation.NativePointerSize == 4 ? "x86_stdcallcc " : "";
		var returnType = windows ? "i32" : "ptr";
		_output.AppendLine($"define internal {callingConvention}{returnType} @luma_thread_entry(ptr %state) {{");
		_output.AppendLine("entry:");
		if (!windows)
			_output.AppendLine("  call i32 @pthread_setcanceltype(i32 1, ptr null)");
		_output.AppendLine("  %invoke.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 2");
		_output.AppendLine("  %invoke = load ptr, ptr %invoke.address");
		_output.AppendLine("  call void %invoke(ptr %state)");
		_output.AppendLine("  %callback.returned.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 7");
		_output.AppendLine("  store atomic i32 1, ptr %callback.returned.address release, align 4");
		_output.AppendLine("  call void @luma_thread_current_release()");
		_output.AppendLine("  %delegate.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 0");
		_output.AppendLine("  %delegate = load ptr, ptr %delegate.address");
		_output.AppendLine("  call void @luma_class_release_dynamic(ptr %delegate)");
		_output.AppendLine("  %argument.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 1");
		_output.AppendLine("  %argument = load ptr, ptr %argument.address");
		_output.AppendLine("  call void @luma_class_release_dynamic(ptr %argument)");
		_output.AppendLine("  %completed.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 5");
		_output.AppendLine("  store atomic i32 1, ptr %completed.address release, align 4");
		_output.AppendLine(windows ? "  ret i32 0" : "  ret ptr null");
		_output.AppendLine("}");
	}

	private void EmitThreadInvoker(string name, TypeDeclaration delegateType)
	{
		_output.AppendLine($"define internal void @luma_thread_invoke_{Safe(name)}(ptr %state) {{");
		_output.AppendLine("entry:");
		_output.AppendLine("  %delegate.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 0");
		_output.AppendLine("  %delegate = load ptr, ptr %delegate.address");
		_output.AppendLine($"  %function.address = getelementptr %delegate.{Safe(name)}, ptr %delegate, i32 0, i32 4");
		_output.AppendLine("  %function = load ptr, ptr %function.address");
		_output.AppendLine($"  %target.address = getelementptr %delegate.{Safe(name)}, ptr %delegate, i32 0, i32 5");
		_output.AppendLine("  %target = load ptr, ptr %target.address");
		_output.AppendLine("  %is.static = icmp eq ptr %target, null");
		if ((delegateType.DelegateParameters?.Count ?? 0) == 0)
		{
			_output.AppendLine("  br i1 %is.static, label %static, label %instance");
			_output.AppendLine("static:");
			_output.AppendLine("  call void %function()");
			_output.AppendLine("  br label %done");
			_output.AppendLine("instance:");
			_output.AppendLine("  call void %function(ptr %target)");
			_output.AppendLine("  br label %done");
		}
		else
		{
			var parameterType = delegateType.DelegateParameters![0].TypeName;
			var argumentValue = EmitThreadArgumentValue(parameterType);
			var argumentType = LlvmType(parameterType);
			_output.AppendLine("  br i1 %is.static, label %static, label %instance");
			_output.AppendLine("static:");
			_output.AppendLine($"  call void %function({argumentType} {argumentValue})");
			_output.AppendLine("  br label %done");
			_output.AppendLine("instance:");
			_output.AppendLine($"  call void %function(ptr %target, {argumentType} {argumentValue})");
			_output.AppendLine("  br label %done");
		}
		_output.AppendLine("done:");
		_output.AppendLine("  ret void");
		_output.AppendLine("}");
	}

	private string EmitThreadArgumentValue(string type)
	{
		_output.AppendLine("  %boxed.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 1");
		_output.AppendLine("  %boxed = load ptr, ptr %boxed.address");
		if (compilation.StructTypes.ContainsKey(type))
		{
			_output.AppendLine($"  %payload.address = getelementptr %luma.structbox.{Safe(type)}, ptr %boxed, i32 0, i32 4");
			_output.AppendLine($"  %argument.value = load %struct.{Safe(type)}, ptr %payload.address");
			return "%argument.value";
		}
		if (type == "bool")
		{
			EmitThreadBoxPayload();
			_output.AppendLine("  %argument.value = trunc i64 %payload to i1");
			return "%argument.value";
		}
		if (type is "float" or "double")
		{
			EmitThreadBoxPayload();
			if (type == "float")
			{
				_output.AppendLine("  %float.bits = trunc i64 %payload to i32");
				_output.AppendLine("  %argument.value = bitcast i32 %float.bits to float");
			}
			else _output.AppendLine("  %argument.value = bitcast i64 %payload to double");
			return "%argument.value";
		}
		if (IsThreadIntegerType(type))
		{
			EmitThreadBoxPayload();
			var llvmType = LlvmType(compilation.EnumUnderlyingTypes.GetValueOrDefault(type, type));
			if (llvmType == "i64") return "%payload";
			_output.AppendLine($"  %argument.value = trunc i64 %payload to {llvmType}");
			return "%argument.value";
		}
		return "%boxed";
	}

	private void EmitThreadBoxPayload()
	{
		_output.AppendLine("  %payload.address = getelementptr %luma.box, ptr %boxed, i32 0, i32 4");
		_output.AppendLine("  %payload = load i64, ptr %payload.address");
	}

	private bool IsThreadIntegerType(string type) => type is "char" or "int8" or "uint8" or "int16" or "uint16" or "int32" or "uint32" or "int64" or "uint64" or "int" or "uint" || compilation.EnumUnderlyingTypes.ContainsKey(type);

	private bool IsSupportedThreadArgument(string type)
	{
		if (IsThreadIntegerType(type) || type is "bool" or "float" or "double" or "string" or "object" or "System.Object")
			return true;
		return compilation.StructTypes.ContainsKey(type) || compilation.ClassTypes.ContainsKey(type) ||
			compilation.InterfaceTypes.ContainsKey(type) || compilation.DelegateTypes.ContainsKey(type) ||
			SemanticAnalyzer.TryParseManagedArrayType(type, out _, out _);
	}

	private void EmitThreadCurrentRuntime(string sizeType)
	{
		const string typeName = "System.Threading.Thread";
		if (!compilation.ClassTypes.TryGetValue(typeName, out var threadType))
			throw new InvalidOperationException("The Thread.CurrentThread intrinsic requires System.Threading.Thread.");

		var safeType = Safe(typeName);
		_output.AppendLine("@luma_thread_current_instance = internal thread_local global ptr null");
		_output.AppendLine("define internal ptr @luma_thread_current() {");
		_output.AppendLine("entry:");
		_output.AppendLine("  %cached = load ptr, ptr @luma_thread_current_instance");
		_output.AppendLine("  %is.cached = icmp ne ptr %cached, null");
		_output.AppendLine("  br i1 %is.cached, label %retain, label %allocate");
		_output.AppendLine("allocate:");
		_output.AppendLine($"  %size.address = getelementptr %class.{safeType}, ptr null, i32 1");
		_output.AppendLine($"  %size = ptrtoint ptr %size.address to {sizeType}");
		_output.AppendLine($"  %instance = call ptr @malloc({sizeType} %size)");
		_output.AppendLine("  %allocation.failed = icmp eq ptr %instance, null");
		_output.AppendLine("  br i1 %allocation.failed, label %out.of.memory, label %initialize");
		_output.AppendLine("initialize:");
		_output.AppendLine($"  %descriptor.address = getelementptr %class.{safeType}, ptr %instance, i32 0, i32 0");
		_output.AppendLine($"  store ptr @luma_classdesc_{safeType}, ptr %descriptor.address");
		_output.AppendLine($"  %count.address = getelementptr %class.{safeType}, ptr %instance, i32 0, i32 1");
		_output.AppendLine("  store i32 1, ptr %count.address");
		_output.AppendLine($"  %flags.address = getelementptr %class.{safeType}, ptr %instance, i32 0, i32 2");
		_output.AppendLine("  store i32 0, ptr %flags.address");
		_output.AppendLine($"  %weak.address = getelementptr %class.{safeType}, ptr %instance, i32 0, i32 3");
		_output.AppendLine("  store ptr null, ptr %weak.address");
		foreach (var field in threadType.Fields.OrderBy(field => field.Index))
		{
			var fieldAddress = $"%field.address.{field.Index}";
			_output.AppendLine($"  {fieldAddress} = getelementptr %class.{safeType}, ptr %instance, i32 0, i32 {field.Index + 4}");
			var value = field.Declaration.Name switch
			{
				"isCurrentThread" => "true",
				"hasStarted" => "true",
				_ => DefaultStaticStorageValue(field.TypeName)
			};
			_output.AppendLine($"  store {LlvmType(field.TypeName)} {value}, ptr {fieldAddress}");
		}
		_output.AppendLine("  store ptr %instance, ptr @luma_thread_current_instance");
		_output.AppendLine("  call void @luma_class_retain_dynamic(ptr %instance)");
		_output.AppendLine("  ret ptr %instance");
		_output.AppendLine("retain:");
		_output.AppendLine("  call void @luma_class_retain_dynamic(ptr %cached)");
		_output.AppendLine("  ret ptr %cached");
		_output.AppendLine("out.of.memory:");
		_output.AppendLine("  call void @luma_raise_out_of_memory()");
		_output.AppendLine("  ret ptr null");
		_output.AppendLine("}");
		_output.AppendLine("define internal void @luma_thread_current_release() {");
		_output.AppendLine("entry:");
		_output.AppendLine("  %current = load ptr, ptr @luma_thread_current_instance");
		_output.AppendLine("  store ptr null, ptr @luma_thread_current_instance");
		_output.AppendLine("  call void @luma_class_release_dynamic(ptr %current)");
		_output.AppendLine("  ret void");
		_output.AppendLine("}");
	}

	private void EmitWindowsThreadRuntime(string sizeType)
	{
		AppendDeclarationIfMissing($"declare ptr @malloc({sizeType})");
		AppendDeclarationIfMissing("declare void @free(ptr)");
		var cc = compilation.NativePointerSize == 4 ? "x86_stdcallcc " : "";
		var closeHandleCc = cc;
		if (_output.ToString().Contains("@CloseHandle(", StringComparison.Ordinal))
			closeHandleCc = "";
		_output.AppendLine($"declare {cc}ptr @CreateThread(ptr, {sizeType}, ptr, ptr, i32, ptr)");
		_output.AppendLine($"declare {cc}i32 @WaitForSingleObject(ptr, i32)");
		_output.AppendLine($"declare {cc}i32 @TerminateThread(ptr, i32)");
		AppendDeclarationIfMissing($"declare {closeHandleCc}i32 @CloseHandle(ptr)");
		_output.AppendLine($"declare {cc}void @Sleep(i32)");
		EmitThreadStartFunction(sizeType, true);
		_output.AppendLine($"define internal i1 @luma_thread_join({sizeType} %handle, i32 %timeout) {{");
		_output.AppendLine("entry:");
		_output.AppendLine($"  %state = inttoptr {sizeType} %handle to ptr");
		_output.AppendLine("  %native.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 3");
		_output.AppendLine("  %native = load ptr, ptr %native.address");
		_output.AppendLine($"  %wait = call {cc}i32 @WaitForSingleObject(ptr %native, i32 %timeout)");
		_output.AppendLine("  %success = icmp eq i32 %wait, 0");
		_output.AppendLine("  ret i1 %success");
		_output.AppendLine("}");
		_output.AppendLine($"define internal void @luma_thread_close({sizeType} %handle) {{");
		_output.AppendLine("entry:");
		_output.AppendLine($"  %state = inttoptr {sizeType} %handle to ptr");
		_output.AppendLine("  %native.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 3");
		_output.AppendLine("  %native = load ptr, ptr %native.address");
		_output.AppendLine($"  %wait = call {cc}i32 @WaitForSingleObject(ptr %native, i32 -1)");
		_output.AppendLine($"  %closed = call {closeHandleCc}i32 @CloseHandle(ptr %native)");
		_output.AppendLine("  call void @free(ptr %state)");
		_output.AppendLine("  ret void");
		_output.AppendLine("}");
		_output.AppendLine($"define internal void @luma_thread_abort({sizeType} %handle) {{");
		_output.AppendLine("entry:");
		_output.AppendLine($"  %state = inttoptr {sizeType} %handle to ptr");
		_output.AppendLine("  %native.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 3");
		_output.AppendLine("  %native = load ptr, ptr %native.address");
		_output.AppendLine($"  %terminate.status = call {cc}i32 @TerminateThread(ptr %native, i32 1)");
		_output.AppendLine($"  %wait.status = call {cc}i32 @WaitForSingleObject(ptr %native, i32 -1)");
		_output.AppendLine("  %callback.returned.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 7");
		_output.AppendLine("  %callback.returned = load atomic i32, ptr %callback.returned.address acquire, align 4");
		_output.AppendLine("  %callback.finished = icmp ne i32 %callback.returned, 0");
		_output.AppendLine("  br i1 %callback.finished, label %release.state, label %release.retained.references");
		_output.AppendLine("release.retained.references:");
		_output.AppendLine("  %delegate.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 0");
		_output.AppendLine("  %delegate = load ptr, ptr %delegate.address");
		_output.AppendLine("  call void @luma_class_release_dynamic(ptr %delegate)");
		_output.AppendLine("  %argument.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 1");
		_output.AppendLine("  %argument = load ptr, ptr %argument.address");
		_output.AppendLine("  call void @luma_class_release_dynamic(ptr %argument)");
		_output.AppendLine("  br label %release.state");
		_output.AppendLine("release.state:");
		_output.AppendLine($"  %closed = call {closeHandleCc}i32 @CloseHandle(ptr %native)");
		_output.AppendLine("  call void @free(ptr %state)");
		_output.AppendLine("  ret void");
		_output.AppendLine("}");
		_output.AppendLine("define internal void @luma_thread_sleep(i32 %milliseconds) {");
		_output.AppendLine("entry:");
		_output.AppendLine($"  call {cc}void @Sleep(i32 %milliseconds)");
		_output.AppendLine("  ret void");
		_output.AppendLine("}");
	}

	private void EmitPosixThreadRuntime(string sizeType)
	{
		AppendDeclarationIfMissing($"declare ptr @malloc({sizeType})");
		AppendDeclarationIfMissing("declare void @free(ptr)");
		_output.AppendLine("declare i32 @pthread_create(ptr, ptr, ptr, ptr)");
		_output.AppendLine($"declare i32 @pthread_join({sizeType}, ptr)");
		_output.AppendLine($"declare i32 @pthread_cancel({sizeType})");
		_output.AppendLine("declare i32 @pthread_setcanceltype(i32, ptr)");
		_output.AppendLine("declare i32 @nanosleep(ptr, ptr)");
		EmitThreadStartFunction(sizeType, false);
		EmitPosixThreadJoin(sizeType);
		_output.AppendLine($"define internal void @luma_thread_close({sizeType} %handle) {{");
		_output.AppendLine("entry:");
		_output.AppendLine($"  %state = inttoptr {sizeType} %handle to ptr");
		_output.AppendLine("  %joined.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 6");
		_output.AppendLine("  %joined = load i32, ptr %joined.address");
		_output.AppendLine("  %already.joined = icmp ne i32 %joined, 0");
		_output.AppendLine("  br i1 %already.joined, label %release.state, label %join.thread");
		_output.AppendLine("join.thread:");
		_output.AppendLine("  %id.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 4");
		_output.AppendLine($"  %id = load {sizeType}, ptr %id.address");
		_output.AppendLine($"  %joined.status = call i32 @pthread_join({sizeType} %id, ptr null)");
		_output.AppendLine("  br label %release.state");
		_output.AppendLine("release.state:");
		_output.AppendLine("  call void @free(ptr %state)");
		_output.AppendLine("  ret void");
		_output.AppendLine("}");
		_output.AppendLine($"define internal void @luma_thread_abort({sizeType} %handle) {{");
		_output.AppendLine("entry:");
		_output.AppendLine($"  %state = inttoptr {sizeType} %handle to ptr");
		_output.AppendLine("  %joined.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 6");
		_output.AppendLine("  %joined = load i32, ptr %joined.address");
		_output.AppendLine("  %already.joined = icmp ne i32 %joined, 0");
		_output.AppendLine("  br i1 %already.joined, label %release.state, label %cancel.thread");
		_output.AppendLine("cancel.thread:");
		_output.AppendLine("  %id.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 4");
		_output.AppendLine($"  %id = load {sizeType}, ptr %id.address");
		_output.AppendLine($"  %cancel.status = call i32 @pthread_cancel({sizeType} %id)");
		_output.AppendLine($"  %join.status = call i32 @pthread_join({sizeType} %id, ptr null)");
		_output.AppendLine("  store i32 1, ptr %joined.address");
		_output.AppendLine("  %callback.returned.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 7");
		_output.AppendLine("  %callback.returned = load atomic i32, ptr %callback.returned.address acquire, align 4");
		_output.AppendLine("  %callback.finished = icmp ne i32 %callback.returned, 0");
		_output.AppendLine("  br i1 %callback.finished, label %release.state, label %release.retained.references");
		_output.AppendLine("release.retained.references:");
		_output.AppendLine("  %delegate.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 0");
		_output.AppendLine("  %delegate = load ptr, ptr %delegate.address");
		_output.AppendLine("  call void @luma_class_release_dynamic(ptr %delegate)");
		_output.AppendLine("  %argument.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 1");
		_output.AppendLine("  %argument = load ptr, ptr %argument.address");
		_output.AppendLine("  call void @luma_class_release_dynamic(ptr %argument)");
		_output.AppendLine("  br label %release.state");
		_output.AppendLine("release.state:");
		_output.AppendLine("  call void @free(ptr %state)");
		_output.AppendLine("  ret void");
		_output.AppendLine("}");
		_output.AppendLine("define internal void @luma_thread_sleep(i32 %milliseconds) {");
		_output.AppendLine("entry:");
		_output.AppendLine("  %seconds = sdiv i32 %milliseconds, 1000");
		_output.AppendLine("  %remainder = srem i32 %milliseconds, 1000");
		_output.AppendLine("  %nanoseconds.32 = mul i32 %remainder, 1000000");
		if (sizeType == "i64")
		{
			_output.AppendLine("  %seconds.wide = sext i32 %seconds to i64");
			_output.AppendLine("  %nanoseconds = sext i32 %nanoseconds.32 to i64");
		}
		else
		{
			_output.AppendLine("  %seconds.wide = add i32 %seconds, 0");
			_output.AppendLine("  %nanoseconds = add i32 %nanoseconds.32, 0");
		}
		_output.AppendLine($"  %time.address = alloca {{ {sizeType}, {sizeType} }}, align {compilation.NativePointerSize}");
		_output.AppendLine($"  %seconds.address = getelementptr {{ {sizeType}, {sizeType} }}, ptr %time.address, i32 0, i32 0");
		_output.AppendLine($"  %nanoseconds.address = getelementptr {{ {sizeType}, {sizeType} }}, ptr %time.address, i32 0, i32 1");
		_output.AppendLine($"  store {sizeType} %seconds.wide, ptr %seconds.address");
		_output.AppendLine($"  store {sizeType} %nanoseconds, ptr %nanoseconds.address");
		_output.AppendLine("  call i32 @nanosleep(ptr %time.address, ptr null)");
		_output.AppendLine("  ret void");
		_output.AppendLine("}");
	}

	private void EmitThreadStartFunction(string sizeType, bool windows)
	{
		var callingConvention = windows && compilation.NativePointerSize == 4 ? "x86_stdcallcc " : "";
		_output.AppendLine($"define internal {sizeType} @luma_thread_start(ptr %delegate, ptr %argument, ptr %invoke) {{");
		_output.AppendLine("entry:");
		_output.AppendLine("  %state.size.address = getelementptr %luma.thread_state, ptr null, i32 1");
		_output.AppendLine($"  %state.size = ptrtoint ptr %state.size.address to {sizeType}");
		_output.AppendLine($"  %state = call ptr @malloc({sizeType} %state.size)");
		_output.AppendLine("  %failed = icmp eq ptr %state, null");
		_output.AppendLine("  br i1 %failed, label %failure, label %initialize");
		_output.AppendLine("initialize:");
		_output.AppendLine("  %delegate.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 0");
		_output.AppendLine("  store ptr %delegate, ptr %delegate.address");
		_output.AppendLine("  %argument.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 1");
		_output.AppendLine("  store ptr %argument, ptr %argument.address");
		_output.AppendLine("  %invoke.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 2");
		_output.AppendLine("  store ptr %invoke, ptr %invoke.address");
		_output.AppendLine("  %native.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 3");
		_output.AppendLine("  store ptr null, ptr %native.address");
		_output.AppendLine("  %id.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 4");
		_output.AppendLine($"  store {sizeType} 0, ptr %id.address");
		_output.AppendLine("  %completed.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 5");
		_output.AppendLine("  store i32 0, ptr %completed.address");
		_output.AppendLine("  %joined.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 6");
		_output.AppendLine("  store i32 0, ptr %joined.address");
		_output.AppendLine("  %callback.returned.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 7");
		_output.AppendLine("  store i32 0, ptr %callback.returned.address");
		_output.AppendLine("  call void @luma_class_retain_dynamic(ptr %delegate)");
		_output.AppendLine("  call void @luma_class_retain_dynamic(ptr %argument)");
		if (windows)
		{
			_output.AppendLine($"  %native = call {callingConvention}ptr @CreateThread(ptr null, {sizeType} 0, ptr @luma_thread_entry, ptr %state, i32 0, ptr null)");
			_output.AppendLine("  %create.failed = icmp eq ptr %native, null");
		}
		else
		{
			_output.AppendLine("  %create.status = call i32 @pthread_create(ptr %id.address, ptr null, ptr @luma_thread_entry, ptr %state)");
			_output.AppendLine("  %create.failed = icmp ne i32 %create.status, 0");
		}
		_output.AppendLine("  br i1 %create.failed, label %start.failure, label %started");
		_output.AppendLine("started:");
		if (windows) _output.AppendLine("  store ptr %native, ptr %native.address");
		_output.AppendLine($"  %handle = ptrtoint ptr %state to {sizeType}");
		_output.AppendLine($"  ret {sizeType} %handle");
		_output.AppendLine("start.failure:");
		_output.AppendLine("  call void @luma_class_release_dynamic(ptr %delegate)");
		_output.AppendLine("  call void @luma_class_release_dynamic(ptr %argument)");
		_output.AppendLine("  call void @free(ptr %state)");
		_output.AppendLine($"  ret {sizeType} 0");
		_output.AppendLine("failure:");
		_output.AppendLine($"  ret {sizeType} 0");
		_output.AppendLine("}");
	}

	private void EmitPosixThreadJoin(string sizeType)
	{
		_output.AppendLine($"define internal i1 @luma_thread_join({sizeType} %handle, i32 %timeout) {{");
		_output.AppendLine("entry:");
		_output.AppendLine($"  %state = inttoptr {sizeType} %handle to ptr");
		_output.AppendLine("  %joined.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 6");
		_output.AppendLine("  %joined = load i32, ptr %joined.address");
		_output.AppendLine("  %already.joined = icmp ne i32 %joined, 0");
		_output.AppendLine("  br i1 %already.joined, label %success, label %check.timeout");
		_output.AppendLine("check.timeout:");
		_output.AppendLine("  %infinite = icmp slt i32 %timeout, 0");
		_output.AppendLine("  br i1 %infinite, label %wait.forever, label %poll.start");
		_output.AppendLine("wait.forever:");
		_output.AppendLine("  %id.address.forever = getelementptr %luma.thread_state, ptr %state, i32 0, i32 4");
		_output.AppendLine($"  %id.forever = load {sizeType}, ptr %id.address.forever");
		_output.AppendLine($"  %join.status.forever = call i32 @pthread_join({sizeType} %id.forever, ptr null)");
		_output.AppendLine("  %join.failed.forever = icmp ne i32 %join.status.forever, 0");
		_output.AppendLine("  br i1 %join.failed.forever, label %failure, label %mark.joined");
		_output.AppendLine("poll.start:");
		_output.AppendLine("  br label %poll");
		_output.AppendLine("poll:");
		_output.AppendLine("  %remaining = phi i32 [ %timeout, %poll.start ], [ %remaining.next, %poll.sleep ]");
		_output.AppendLine("  %complete.address = getelementptr %luma.thread_state, ptr %state, i32 0, i32 5");
		_output.AppendLine("  %complete = load atomic i32, ptr %complete.address acquire, align 4");
		_output.AppendLine("  %is.complete = icmp ne i32 %complete, 0");
		_output.AppendLine("  %no.time = icmp sle i32 %remaining, 0");
		_output.AppendLine("  br i1 %is.complete, label %join.complete, label %timeout.check");
		_output.AppendLine("timeout.check:");
		_output.AppendLine("  br i1 %no.time, label %failure, label %poll.sleep");
		_output.AppendLine("poll.sleep:");
		_output.AppendLine("  %sleep.address = alloca { i64, i64 }, align 8");
		_output.AppendLine("  %sleep.seconds = getelementptr { i64, i64 }, ptr %sleep.address, i32 0, i32 0");
		_output.AppendLine("  %sleep.nanoseconds = getelementptr { i64, i64 }, ptr %sleep.address, i32 0, i32 1");
		_output.AppendLine("  store i64 0, ptr %sleep.seconds");
		_output.AppendLine("  store i64 1000000, ptr %sleep.nanoseconds");
		_output.AppendLine("  call i32 @nanosleep(ptr %sleep.address, ptr null)");
		_output.AppendLine("  %remaining.next = sub i32 %remaining, 1");
		_output.AppendLine("  br label %poll");
		_output.AppendLine("join.complete:");
		_output.AppendLine("  %id.address.complete = getelementptr %luma.thread_state, ptr %state, i32 0, i32 4");
		_output.AppendLine($"  %id.complete = load {sizeType}, ptr %id.address.complete");
		_output.AppendLine($"  %join.status.complete = call i32 @pthread_join({sizeType} %id.complete, ptr null)");
		_output.AppendLine("  %join.failed.complete = icmp ne i32 %join.status.complete, 0");
		_output.AppendLine("  br i1 %join.failed.complete, label %failure, label %mark.joined");
		_output.AppendLine("mark.joined:");
		_output.AppendLine("  store i32 1, ptr %joined.address");
		_output.AppendLine("  br label %success");
		_output.AppendLine("success:");
		_output.AppendLine("  ret i1 true");
		_output.AppendLine("failure:");
		_output.AppendLine("  ret i1 false");
		_output.AppendLine("}");
	}
}
