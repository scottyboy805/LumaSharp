using System.Numerics;
using System.Text;

namespace LumaSharp.Compiler;

internal sealed partial class LlvmEmitter(CompilationAnalysis compilation, CancellationToken cancellationToken = default)
{
    private readonly StringBuilder _output = new();
    private readonly IReadOnlyDictionary<TryStatement, string> _exceptionCatchTableNames = BuildExceptionCatchTableNames(compilation);
    private readonly IReadOnlyDictionary<string, string> _typeInitializationFunctions = compilation.Methods
        .Where(method => method.IsTypeInitializer)
        .ToDictionary(method => QualifiedTypeName(method.Owner), method => TypeInitializationFunctionName(QualifiedTypeName(method.Owner)), StringComparer.Ordinal);

    private static string QualifiedTypeName(TypeDeclaration type) => string.IsNullOrEmpty(type.Namespace) ? type.Name : type.Namespace + "." + type.Name;
    private static string TypeInitializationFunctionName(string typeName) => "luma_type_init_" + Safe(typeName);
    private static string TypeInitializationFailureMessage(string typeName) => $"The type initializer for '{typeName}' threw an exception.";
    private static string TypeInitializationCircularMessage(string typeName) => $"circular static initialization of {typeName}";
    private static string StringLiteralSymbol(string value) => StringLiteralCodec.TryCreate(value, out var literal, out _)
        ? literal.Symbol : throw new InvalidOperationException("A compiler-generated runtime message is not valid Unicode.");

    private static IReadOnlyDictionary<TryStatement, string> BuildExceptionCatchTableNames(CompilationAnalysis analysis)
    {
        var result = new Dictionary<TryStatement, string>();
        var index = 0;
        foreach (var method in analysis.Methods)
            if (method.Declaration.Body is { } body)
                foreach (var statement in EnumerateTryStatements(body))
                    if (statement.Catches.Count > 0)
                        result.Add(statement, $"@luma_eh_catches_{index++}");
        return result;
    }

    private static IEnumerable<TryStatement> EnumerateTryStatements(Statement statement)
    {
        switch (statement)
        {
            case TryStatement tryStatement:
                yield return tryStatement;
                foreach (var nested in EnumerateTryStatements(tryStatement.Body)) yield return nested;
                foreach (var clause in tryStatement.Catches)
                    foreach (var nested in EnumerateTryStatements(clause.Body)) yield return nested;
                if (tryStatement.Finally is not null)
                    foreach (var nested in EnumerateTryStatements(tryStatement.Finally)) yield return nested;
                break;
            case BlockStatement block:
                foreach (var child in block.Statements)
                    foreach (var nested in EnumerateTryStatements(child)) yield return nested;
                break;
            case IfStatement conditional:
                foreach (var nested in EnumerateTryStatements(conditional.Then)) yield return nested;
                if (conditional.Else is not null)
                    foreach (var nested in EnumerateTryStatements(conditional.Else)) yield return nested;
                break;
            case WhileStatement loop:
                foreach (var nested in EnumerateTryStatements(loop.Body)) yield return nested;
                break;
            case DoWhileStatement loop:
                foreach (var nested in EnumerateTryStatements(loop.Body)) yield return nested;
                break;
            case ForStatement loop:
                if (loop.Initializer is not null)
                    foreach (var nested in EnumerateTryStatements(loop.Initializer)) yield return nested;
                foreach (var nested in EnumerateTryStatements(loop.Body)) yield return nested;
                break;
            case ForInStatement loop:
                foreach (var nested in EnumerateTryStatements(loop.Body)) yield return nested;
                break;
            case SwitchStatement selection:
                foreach (var section in selection.Sections)
                    foreach (var child in section.Statements)
                        foreach (var nested in EnumerateTryStatements(child)) yield return nested;
                break;
            case OverflowContextStatement overflow:
                foreach (var nested in EnumerateTryStatements(overflow.Body)) yield return nested;
                break;
        }
    }

    private void EmitExceptionCatchTables()
    {
        foreach (var (tryStatement, tableName) in _exceptionCatchTableNames)
        {
            var entries = tryStatement.Catches.Select(clause =>
            {
                if (clause.IsCatchAll) return "ptr null";
                if (!compilation.ExceptionCatchTypes.TryGetValue(clause, out var typeName))
                    throw new InvalidOperationException("A validated catch clause is missing its bound exception type.");
                return $"ptr @luma_classdesc_{Safe(typeName)}";
            });
            var catchTypes = string.Join(", ", entries);
            _output.AppendLine($"{tableName} = private constant [{tryStatement.Catches.Count} x ptr] [{catchTypes}]");
        }
    }
    public string Emit()
    {
        cancellationToken.ThrowIfCancellationRequested();
        _output.AppendLine("; LumaSharp native LLVM IR");
        _output.AppendLine("declare void @llvm.trap()");
        _output.AppendLine("declare float @llvm.trunc.f32(float)");
        _output.AppendLine("declare double @llvm.trunc.f64(double)");
        foreach (var width in new[] { 8, 16, 32, 64 }) foreach (var prefix in new[] { "s", "u" }) foreach (var op in new[] { "add", "sub", "mul" })
            _output.AppendLine($"declare {{ i{width}, i1 }} @llvm.{prefix}{op}.with.overflow.i{width}(i{width}, i{width})");
        if (HasStringSupport() || compilation.UsesExceptionHandling || HasManagedClassSupport())
        {
            EmitStderrWriterRuntime();
            if (HasStringSupport() || HasManagedClassSupport()) EmitFatalRuntime();
        }
        _output.AppendLine("define internal void @luma_throw_overflow() noreturn {"); _output.AppendLine("entry:"); _output.AppendLine("  call void @llvm.trap()"); _output.AppendLine("  unreachable"); _output.AppendLine("}");
        _output.AppendLine("define internal void @luma_throw_divide_by_zero() noreturn {"); _output.AppendLine("entry:"); _output.AppendLine("  call void @llvm.trap()"); _output.AppendLine("  unreachable"); _output.AppendLine("}");
        _output.AppendLine("define internal void @luma_throw_bounds() noreturn {"); _output.AppendLine("entry:"); _output.AppendLine("  call void @llvm.trap()"); _output.AppendLine("  unreachable"); _output.AppendLine("}");
        _output.AppendLine("define internal void @luma_throw_out_of_memory() noreturn {"); _output.AppendLine("entry:"); _output.AppendLine("  call void @llvm.trap()"); _output.AppendLine("  unreachable"); _output.AppendLine("}");
        _output.AppendLine("define internal void @luma_throw_null() noreturn {"); _output.AppendLine("entry:"); _output.AppendLine("  call void @llvm.trap()"); _output.AppendLine("  unreachable"); _output.AppendLine("}");
        foreach (var (name, structure) in compilation.StructTypes)
        {
            var fields = structure.Fields.Count == 0 ? "i8" : string.Join(", ", structure.Fields.Select(field => LlvmType(field.TypeName)));
            _output.AppendLine($"%struct.{Safe(name)} = type {{ {fields} }}");
        }
        if (HasManagedClassSupport())
            _output.AppendLine("%luma.weak.control = type { ptr, i32, i32, i32 }");
        if (HasManagedClassSupport())
            _output.AppendLine("%luma.box = type { ptr, i32, i32, ptr, i64 }");
        foreach (var type in BoxableStructTypeNames())
            _output.AppendLine($"%luma.structbox.{Safe(type)} = type {{ ptr, i32, i32, ptr, %struct.{Safe(type)} }}");
        foreach (var (name, classType) in compilation.ClassTypes)
        {
            var fields = classType.Fields.Count == 0 ? "" : ", " + string.Join(", ", classType.Fields.Select(field => LlvmType(field.TypeName)));
            _output.AppendLine($"%class.{Safe(name)} = type {{ ptr, i32, i32, ptr{fields} }}");
        }
        if (HasManagedClassSupport() || HasStringSupport())
            _output.AppendLine("%luma.typedesc = type { ptr, ptr, ptr, i32, ptr, ptr, ptr, ptr, i32, ptr }");
        foreach (var name in compilation.DelegateTypes.Keys.Order(StringComparer.Ordinal))
            _output.AppendLine($"%delegate.{Safe(name)} = type {{ ptr, i32, i32, ptr, ptr, ptr }}");
        if (HasStringSupport())
        {
            EmitStringLiterals();
            EmitStringRuntime();
        }
        if (HasManagedArraySupport())
        {
            _output.AppendLine("%luma.array = type { ptr, i32, i32, ptr, i32, i32, [3 x i32], [0 x i64] }");
            if (!HasStringSupport()) EmitArrayMemoryDeclarations();
            EmitManagedArrayRuntime();
        }
        if (HasManagedClassSupport())
        {
            if (!HasStringSupport() && !HasManagedArraySupport()) EmitArrayMemoryDeclarations();
            foreach (var interfaceName in compilation.InterfaceTypes.Keys.Order(StringComparer.Ordinal))
                _output.AppendLine($"@luma_interfacedesc_{Safe(interfaceName)} = private constant i8 0");
            EmitManagedClassRuntime();
            if (compilation.UsesExceptionHandling)
            {
                EmitRuntimeExceptionInstance("System.OutOfMemoryException", "luma_oom_instance");
                EmitRuntimeExceptionInstance("System.OverflowException", "luma_overflow_instance");
            }
        }
        EmitAggregateOwnershipRuntime();
        if (compilation.UsesExceptionHandling)
        {
            _output.AppendLine("%luma.ehnode = type { ptr, ptr, ptr, i32 }");
            _output.AppendLine("@luma_eh_top = internal thread_local global ptr null");
            _output.AppendLine("@luma_pending_exception = internal thread_local global ptr null");
            _output.AppendLine("@luma_pending_handler = internal thread_local global ptr null");
            EmitExceptionRuntime();
            EmitExceptionCatchTables();
        }
        foreach (var field in compilation.StaticFields)
            _output.AppendLine($"@{field.GlobalName} = {(field.Declaration.IsReadOnly && !field.RequiresInitialization ? "constant" : "global")} {LlvmType(field.Declaration.TypeName)} {field.InitialValue}");
        foreach (var typeName in _typeInitializationFunctions.Keys.Order(StringComparer.Ordinal))
        {
            var safeType = Safe(typeName);
            _output.AppendLine($"@luma_type_init_state_{safeType} = internal global i8 0");
            _output.AppendLine($"@luma_type_init_active_{safeType} = internal thread_local global i1 false");
            if (compilation.UsesExceptionHandling)
                _output.AppendLine($"@luma_type_init_failure_{safeType} = internal global ptr null");
        }
        EmitTypeInitializationFunctions();
        if (EntryPointHasStringArguments() || HasStringSupport()) EmitUtf8ArgumentRuntime();
        var nativeFileSystemMethods = compilation.Methods.Where(IsNativeFileSystemShim).ToArray();
        if (nativeFileSystemMethods.Length > 0) EmitNativeFileSystemRuntime();
        foreach (var method in compilation.Methods.Where(method => method.Declaration.IsExtern))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!IsNativeFileSystemShim(method)) EmitExternDeclaration(method);
        }
        foreach (var method in compilation.Methods)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (method.Declaration.Body is not null && (method.Declaration.IsStatic || method.IsStructConstructor || method.IsStructInstanceMethod || method.IsClassConstructor || method.IsClassInstanceMethod || method.IsClassFinalizer)) EmitMethod(method);
        }
        EmitStructDelegateThunks();
        EmitStructInterfaceThunks();
        if (compilation.EntryPoint is { } entry) EmitEntryPoint(entry);
        _output.AppendLine("!llvm.module.flags = !{!0}");
        _output.AppendLine($"!0 = !{{i32 1, !\"{RuntimeAbi.ModuleFlag}\", i32 {RuntimeAbi.Version}}}");
        return _output.ToString();
    }
    private void EmitTypeInitializationFunctions()
    {
        foreach (var initializer in compilation.Methods.Where(method => method.IsTypeInitializer))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var typeName = QualifiedTypeName(initializer.Owner);
            var safeType = Safe(typeName);
            var state = $"@luma_type_init_state_{safeType}";
            var active = $"@luma_type_init_active_{safeType}";
            var function = _typeInitializationFunctions[typeName];
            _output.AppendLine($"define internal void @{function}() {{");
            _output.AppendLine("entry:");
            _output.AppendLine($"  %claim = cmpxchg ptr {state}, i8 0, i8 1 acq_rel acquire");
            _output.AppendLine("  %previous = extractvalue { i8, i1 } %claim, 0");
            _output.AppendLine("  %claimed = extractvalue { i8, i1 } %claim, 1");
            _output.AppendLine("  br i1 %claimed, label %initialize, label %observe");
            _output.AppendLine("observe:");
            _output.AppendLine("  %already.done = icmp eq i8 %previous, 2");
            _output.AppendLine("  br i1 %already.done, label %done, label %check.state");
            _output.AppendLine("check.state:");
            _output.AppendLine("  %already.failed = icmp eq i8 %previous, 3");
            _output.AppendLine("  br i1 %already.failed, label %failed, label %reentrant.check");
            _output.AppendLine("reentrant.check:");
            _output.AppendLine($"  %is.reentrant = load i1, ptr {active}");
            _output.AppendLine("  br i1 %is.reentrant, label %circular, label %wait");
            if (compilation.UsesExceptionHandling)
            {
                _output.AppendLine("circular:");
                var circularMessage = StringLiteralSymbol(TypeInitializationCircularMessage(typeName));
                var ctor = FindTypeInitializationExceptionConstructor();
                _output.AppendLine("  store ptr null, ptr @luma_pending_exception");
                var circularWrapper = EmitTypeInitializationExceptionObject("circular.wrapper", circularMessage, "null", ctor);
                _output.AppendLine("  %circular.previous.exception = load ptr, ptr @luma_pending_exception");
                _output.AppendLine("  call void @luma_class_release_dynamic(ptr %circular.previous.exception)");
                _output.AppendLine($"  store ptr {circularWrapper}, ptr @luma_pending_exception");
                _output.AppendLine($"  %circular.handler = call ptr @luma_eh_find_handler(ptr {circularWrapper})");
                _output.AppendLine("  store ptr %circular.handler, ptr @luma_pending_handler");
                _output.AppendLine("  br label %done");
            }
            else
            {
                _output.AppendLine("circular:");
                _output.AppendLine("  br label %done");
            }
            _output.AppendLine("wait:");
            _output.AppendLine($"  %current.state = load atomic i8, ptr {state} acquire, align 1");
            _output.AppendLine("  %still.running = icmp eq i8 %current.state, 1");
            _output.AppendLine("  br i1 %still.running, label %wait, label %after.wait");
            _output.AppendLine("after.wait:");
            _output.AppendLine("  %finished.after.wait = icmp eq i8 %current.state, 2");
            _output.AppendLine("  br i1 %finished.after.wait, label %done, label %failed");
            _output.AppendLine("initialize:");
            _output.AppendLine($"  store i1 true, ptr {active}");
            var baseInitializer = compilation.ClassTypes.TryGetValue(typeName, out var classType) &&
                classType.BaseTypeName is { } baseTypeName && _typeInitializationFunctions.TryGetValue(baseTypeName, out var baseFunction)
                ? baseFunction : null;
            if (baseInitializer is not null)
            {
                _output.AppendLine($"  call void @{baseInitializer}()");
                _output.AppendLine("  %base.initialization.exception = load ptr, ptr @luma_pending_exception");
                _output.AppendLine("  %base.initialization.failed = icmp ne ptr %base.initialization.exception, null");
                _output.AppendLine("  br i1 %base.initialization.failed, label %base.failed, label %initialize.body");
                _output.AppendLine("base.failed:");
                _output.AppendLine("  br label %record.failure");
                _output.AppendLine("initialize.body:");
            }
            _output.AppendLine($"  call void @{FunctionName(initializer, compilation.Methods)}()");
            if (compilation.UsesExceptionHandling)
            {
                _output.AppendLine("  %own.initialization.exception = load ptr, ptr @luma_pending_exception");
                _output.AppendLine("  %own.initialization.failed = icmp ne ptr %own.initialization.exception, null");
                _output.AppendLine("  br i1 %own.initialization.failed, label %record.failure, label %complete");
                _output.AppendLine("record.failure:");
                _output.AppendLine("  %initialization.exception = load ptr, ptr @luma_pending_exception");
                var failureMessage = StringLiteralSymbol(TypeInitializationFailureMessage(typeName));
                var ctor = FindTypeInitializationExceptionConstructor();
                _output.AppendLine("  store ptr null, ptr @luma_pending_exception");
                var failureWrapper = EmitTypeInitializationExceptionObject("failure.wrapper", failureMessage, "%initialization.exception", ctor);
                _output.AppendLine("  call void @luma_class_release_dynamic(ptr %initialization.exception)");
                _output.AppendLine($"  call void @luma_class_retain_dynamic(ptr {failureWrapper})");
                _output.AppendLine($"  store ptr {failureWrapper}, ptr @luma_type_init_failure_{safeType}");
                _output.AppendLine($"  store ptr {failureWrapper}, ptr @luma_pending_exception");
                _output.AppendLine($"  %failure.handler = call ptr @luma_eh_find_handler(ptr {failureWrapper})");
                _output.AppendLine("  store ptr %failure.handler, ptr @luma_pending_handler");
                _output.AppendLine($"  store i1 false, ptr {active}");
                _output.AppendLine($"  store atomic i8 3, ptr {state} release, align 1");
                _output.AppendLine("  ret void");
            }
            else _output.AppendLine("  br label %complete");
            _output.AppendLine("complete:");
            _output.AppendLine($"  store i1 false, ptr {active}");
            _output.AppendLine($"  store atomic i8 2, ptr {state} release, align 1");
            _output.AppendLine("  br label %done");
            _output.AppendLine("failed:");
            if (compilation.UsesExceptionHandling)
            {
                _output.AppendLine($"  %saved.exception = load ptr, ptr @luma_type_init_failure_{safeType}");
                _output.AppendLine("  call void @luma_class_retain_dynamic(ptr %saved.exception)");
                _output.AppendLine("  %previous.exception = load ptr, ptr @luma_pending_exception");
                _output.AppendLine("  call void @luma_class_release_dynamic(ptr %previous.exception)");
                _output.AppendLine("  store ptr %saved.exception, ptr @luma_pending_exception");
                _output.AppendLine("  %saved.handler = call ptr @luma_eh_find_handler(ptr %saved.exception)");
                _output.AppendLine("  store ptr %saved.handler, ptr @luma_pending_handler");
            }
            _output.AppendLine("  br label %done");
            _output.AppendLine("done:");
            _output.AppendLine("  ret void");
            _output.AppendLine("}");
        }
    }

    private bool EntryPointHasStringArguments() =>
        compilation.EntryPoint?.Declaration.Parameters is [{ TypeName: "string[]" }];

    private void EmitUtf8ArgumentRuntime()
    {
        var sizeType = compilation.NativePointerSize == 8 ? "i64" : "i32";
        if (UsesWindowsCommandLineApi())
        {
            _output.AppendLine("declare ptr @GetCommandLineW()");
            _output.AppendLine("declare ptr @CommandLineToArgvW(ptr, ptr)");
            _output.AppendLine("declare ptr @LocalFree(ptr)");
        }
        _output.AppendLine("define internal { i32, i32 } @luma_utf8_decode(ptr %source, i32 %index) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %lead.address = getelementptr i8, ptr %source, i32 %index");
        _output.AppendLine("  %lead.byte = load i8, ptr %lead.address");
        _output.AppendLine("  %lead = zext i8 %lead.byte to i32");
        _output.AppendLine("  %is.ascii = icmp ult i32 %lead, 128");
        _output.AppendLine("  br i1 %is.ascii, label %single, label %classify");
        _output.AppendLine("classify:");
        _output.AppendLine("  %is.two.high = icmp ult i32 %lead, 224");
        _output.AppendLine("  %is.two.low = icmp uge i32 %lead, 194");
        _output.AppendLine("  %is.two = and i1 %is.two.high, %is.two.low");
        _output.AppendLine("  br i1 %is.two, label %two, label %classify.three");
        _output.AppendLine("classify.three:");
        _output.AppendLine("  %is.three.high = icmp ult i32 %lead, 240");
        _output.AppendLine("  %is.three.low = icmp uge i32 %lead, 224");
        _output.AppendLine("  %is.three = and i1 %is.three.high, %is.three.low");
        _output.AppendLine("  br i1 %is.three, label %three, label %classify.four");
        _output.AppendLine("classify.four:");
        _output.AppendLine("  %is.four.high = icmp ule i32 %lead, 244");
        _output.AppendLine("  %is.four.low = icmp uge i32 %lead, 240");
        _output.AppendLine("  %is.four = and i1 %is.four.high, %is.four.low");
        _output.AppendLine("  br i1 %is.four, label %four, label %replacement");
        _output.AppendLine("single:");
        _output.AppendLine("  br label %decoded");
        _output.AppendLine("two:");
        _output.AppendLine("  %two.second.address = getelementptr i8, ptr %source, i32 %index");
        _output.AppendLine("  %two.second.offset = getelementptr i8, ptr %two.second.address, i32 1");
        _output.AppendLine("  %two.second.byte = load i8, ptr %two.second.offset");
        _output.AppendLine("  %two.second = zext i8 %two.second.byte to i32");
        _output.AppendLine("  %two.mask = and i32 %two.second, 192");
        _output.AppendLine("  %two.valid = icmp eq i32 %two.mask, 128");
        _output.AppendLine("  br i1 %two.valid, label %two.decode, label %replacement");
        _output.AppendLine("two.decode:");
        _output.AppendLine("  %two.lead.bits = and i32 %lead, 31");
        _output.AppendLine("  %two.second.bits = and i32 %two.second, 63");
        _output.AppendLine("  %two.lead.shifted = shl i32 %two.lead.bits, 6");
        _output.AppendLine("  %two.codepoint = or i32 %two.lead.shifted, %two.second.bits");
        _output.AppendLine("  br label %decoded");
        _output.AppendLine("three:");
        _output.AppendLine("  %three.second.address = getelementptr i8, ptr %source, i32 %index");
        _output.AppendLine("  %three.second.offset = getelementptr i8, ptr %three.second.address, i32 1");
        _output.AppendLine("  %three.second.byte = load i8, ptr %three.second.offset");
        _output.AppendLine("  %three.second = zext i8 %three.second.byte to i32");
        _output.AppendLine("  %three.second.mask = and i32 %three.second, 192");
        _output.AppendLine("  %three.second.valid = icmp eq i32 %three.second.mask, 128");
        _output.AppendLine("  br i1 %three.second.valid, label %three.third.load, label %replacement");
        _output.AppendLine("three.third.load:");
        _output.AppendLine("  %three.third.index = add i32 %index, 2");
        _output.AppendLine("  %three.third.address = getelementptr i8, ptr %source, i32 %three.third.index");
        _output.AppendLine("  %three.third.byte = load i8, ptr %three.third.address");
        _output.AppendLine("  %three.third = zext i8 %three.third.byte to i32");
        _output.AppendLine("  %three.third.mask = and i32 %three.third, 192");
        _output.AppendLine("  %three.third.valid = icmp eq i32 %three.third.mask, 128");
        _output.AppendLine("  br i1 %three.third.valid, label %three.decode, label %replacement");
        _output.AppendLine("three.decode:");
        _output.AppendLine("  %three.lead.bits = and i32 %lead, 15");
        _output.AppendLine("  %three.second.bits = and i32 %three.second, 63");
        _output.AppendLine("  %three.third.bits = and i32 %three.third, 63");
        _output.AppendLine("  %three.lead.shifted = shl i32 %three.lead.bits, 12");
        _output.AppendLine("  %three.second.shifted = shl i32 %three.second.bits, 6");
        _output.AppendLine("  %three.high = or i32 %three.lead.shifted, %three.second.shifted");
        _output.AppendLine("  %three.codepoint = or i32 %three.high, %three.third.bits");
        _output.AppendLine("  %three.overlong = icmp ult i32 %three.codepoint, 2048");
        _output.AppendLine("  %three.surrogate.low = icmp uge i32 %three.codepoint, 55296");
        _output.AppendLine("  %three.surrogate.high = icmp ule i32 %three.codepoint, 57343");
        _output.AppendLine("  %three.surrogate = and i1 %three.surrogate.low, %three.surrogate.high");
        _output.AppendLine("  %three.invalid = or i1 %three.overlong, %three.surrogate");
        _output.AppendLine("  br i1 %three.invalid, label %replacement, label %three.valid");
        _output.AppendLine("three.valid:");
        _output.AppendLine("  br label %decoded");
        _output.AppendLine("four:");
        _output.AppendLine("  %four.second.address = getelementptr i8, ptr %source, i32 %index");
        _output.AppendLine("  %four.second.offset = getelementptr i8, ptr %four.second.address, i32 1");
        _output.AppendLine("  %four.second.byte = load i8, ptr %four.second.offset");
        _output.AppendLine("  %four.second = zext i8 %four.second.byte to i32");
        _output.AppendLine("  %four.second.mask = and i32 %four.second, 192");
        _output.AppendLine("  %four.second.valid = icmp eq i32 %four.second.mask, 128");
        _output.AppendLine("  br i1 %four.second.valid, label %four.third.load, label %replacement");
        _output.AppendLine("four.third.load:");
        _output.AppendLine("  %four.third.index = add i32 %index, 2");
        _output.AppendLine("  %four.third.address = getelementptr i8, ptr %source, i32 %four.third.index");
        _output.AppendLine("  %four.third.byte = load i8, ptr %four.third.address");
        _output.AppendLine("  %four.third = zext i8 %four.third.byte to i32");
        _output.AppendLine("  %four.third.mask = and i32 %four.third, 192");
        _output.AppendLine("  %four.third.valid = icmp eq i32 %four.third.mask, 128");
        _output.AppendLine("  br i1 %four.third.valid, label %four.fourth.load, label %replacement");
        _output.AppendLine("four.fourth.load:");
        _output.AppendLine("  %four.fourth.index = add i32 %index, 3");
        _output.AppendLine("  %four.fourth.address = getelementptr i8, ptr %source, i32 %four.fourth.index");
        _output.AppendLine("  %four.fourth.byte = load i8, ptr %four.fourth.address");
        _output.AppendLine("  %four.fourth = zext i8 %four.fourth.byte to i32");
        _output.AppendLine("  %four.fourth.mask = and i32 %four.fourth, 192");
        _output.AppendLine("  %four.fourth.valid = icmp eq i32 %four.fourth.mask, 128");
        _output.AppendLine("  br i1 %four.fourth.valid, label %four.decode, label %replacement");
        _output.AppendLine("four.decode:");
        _output.AppendLine("  %four.lead.bits = and i32 %lead, 7");
        _output.AppendLine("  %four.second.bits = and i32 %four.second, 63");
        _output.AppendLine("  %four.third.bits = and i32 %four.third, 63");
        _output.AppendLine("  %four.fourth.bits = and i32 %four.fourth, 63");
        _output.AppendLine("  %four.lead.shifted = shl i32 %four.lead.bits, 18");
        _output.AppendLine("  %four.second.shifted = shl i32 %four.second.bits, 12");
        _output.AppendLine("  %four.third.shifted = shl i32 %four.third.bits, 6");
        _output.AppendLine("  %four.high = or i32 %four.lead.shifted, %four.second.shifted");
        _output.AppendLine("  %four.middle = or i32 %four.high, %four.third.shifted");
        _output.AppendLine("  %four.codepoint = or i32 %four.middle, %four.fourth.bits");
        _output.AppendLine("  %four.overlong = icmp ult i32 %four.codepoint, 65536");
        _output.AppendLine("  %four.out.of.range = icmp ugt i32 %four.codepoint, 1114111");
        _output.AppendLine("  %four.invalid = or i1 %four.overlong, %four.out.of.range");
        _output.AppendLine("  br i1 %four.invalid, label %replacement, label %four.valid");
        _output.AppendLine("four.valid:");
        _output.AppendLine("  br label %decoded");
        _output.AppendLine("replacement:");
        _output.AppendLine("  br label %decoded");
        _output.AppendLine("decoded:");
        _output.AppendLine("  %codepoint = phi i32 [ %lead, %single ], [ %two.codepoint, %two.decode ], [ %three.codepoint, %three.valid ], [ %four.codepoint, %four.valid ], [ 65533, %replacement ]");
        _output.AppendLine("  %width = phi i32 [ 1, %single ], [ 2, %two.decode ], [ 3, %three.valid ], [ 4, %four.valid ], [ 1, %replacement ]");
        _output.AppendLine("  %with.codepoint = insertvalue { i32, i32 } undef, i32 %codepoint, 0");
        _output.AppendLine("  %result = insertvalue { i32, i32 } %with.codepoint, i32 %width, 1");
        _output.AppendLine("  ret { i32, i32 } %result");
        _output.AppendLine("}");

        _output.AppendLine("define internal ptr @luma_string_from_utf8z(ptr %source) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  br label %length.loop");
        _output.AppendLine("length.loop:");
        _output.AppendLine("  %length.index = phi i32 [ 0, %entry ], [ %length.next, %length.advance ]");
        _output.AppendLine("  %length.address = getelementptr i8, ptr %source, i32 %length.index");
        _output.AppendLine("  %length.byte = load i8, ptr %length.address");
        _output.AppendLine("  %length.done = icmp eq i8 %length.byte, 0");
        _output.AppendLine("  br i1 %length.done, label %count.start, label %length.advance");
        _output.AppendLine("length.advance:");
        _output.AppendLine("  %length.next = add i32 %length.index, 1");
        _output.AppendLine("  br label %length.loop");
        _output.AppendLine("count.start:");
        _output.AppendLine("  %empty = icmp eq i32 %length.index, 0");
        _output.AppendLine("  br i1 %empty, label %empty.string, label %count.loop");
        _output.AppendLine("empty.string:");
        _output.AppendLine("  ret ptr @luma_string_empty");
        _output.AppendLine("count.loop:");
        _output.AppendLine("  %count.byte.index = phi i32 [ 0, %count.start ], [ %count.next.byte, %count.advance ]");
        _output.AppendLine("  %unit.count = phi i32 [ 0, %count.start ], [ %count.next.unit, %count.advance ]");
        _output.AppendLine("  %utf8.byte.count = phi i32 [ 0, %count.start ], [ %count.next.utf8.byte, %count.advance ]");
        _output.AppendLine("  %ascii.only = phi i1 [ true, %count.start ], [ %count.next.ascii, %count.advance ]");
        _output.AppendLine("  %count.done = icmp uge i32 %count.byte.index, %length.index");
        _output.AppendLine("  br i1 %count.done, label %allocate.units, label %count.decode");
        _output.AppendLine("count.decode:");
        _output.AppendLine("  %count.decoded = call { i32, i32 } @luma_utf8_decode(ptr %source, i32 %count.byte.index)");
        _output.AppendLine("  %count.codepoint = extractvalue { i32, i32 } %count.decoded, 0");
        _output.AppendLine("  %count.width = extractvalue { i32, i32 } %count.decoded, 1");
        _output.AppendLine("  %count.non.bmp = icmp ugt i32 %count.codepoint, 65535");
        _output.AppendLine("  %count.units = select i1 %count.non.bmp, i32 2, i32 1");
        _output.AppendLine("  %count.next.unit = add i32 %unit.count, %count.units");
        _output.AppendLine("  %count.utf8.ascii = icmp ult i32 %count.codepoint, 128");
        _output.AppendLine("  %count.utf8.two = icmp ult i32 %count.codepoint, 2048");
        _output.AppendLine("  %count.utf8.three = icmp ule i32 %count.codepoint, 65535");
        _output.AppendLine("  %count.utf8.width.two.or.more = select i1 %count.utf8.two, i32 2, i32 3");
        _output.AppendLine("  %count.utf8.width.three.or.four = select i1 %count.utf8.three, i32 3, i32 4");
        _output.AppendLine("  %count.utf8.width.non.ascii = select i1 %count.utf8.three, i32 %count.utf8.width.two.or.more, i32 4");
        _output.AppendLine("  %count.utf8.width = select i1 %count.utf8.ascii, i32 1, i32 %count.utf8.width.non.ascii");
        _output.AppendLine("  %count.next.utf8.byte = add i32 %utf8.byte.count, %count.utf8.width");
        _output.AppendLine("  %count.is.ascii = icmp ult i32 %count.codepoint, 128");
        _output.AppendLine("  %count.next.ascii = and i1 %ascii.only, %count.is.ascii");
        _output.AppendLine("  %count.next.byte = add i32 %count.byte.index, %count.width");
        _output.AppendLine("  br label %count.advance");
        _output.AppendLine("count.advance:");
        _output.AppendLine("  br label %count.loop");
        _output.AppendLine("allocate.units:");
        _output.AppendLine($"  %unit.count.native = {(sizeType == "i64" ? "zext i32 %unit.count to i64" : "add i32 %unit.count, 0")}");
        _output.AppendLine($"  %unit.bytes.pair = call {{ {sizeType}, i1 }} @llvm.umul.with.overflow.{sizeType}({sizeType} %unit.count.native, {sizeType} 2)");
        _output.AppendLine($"  %unit.bytes = extractvalue {{ {sizeType}, i1 }} %unit.bytes.pair, 0");
        _output.AppendLine($"  %unit.bytes.overflow = extractvalue {{ {sizeType}, i1 }} %unit.bytes.pair, 1");
        _output.AppendLine("  br i1 %unit.bytes.overflow, label %overflow, label %allocate");
        _output.AppendLine("overflow:");
        if (compilation.UsesExceptionHandling)
        {
            _output.AppendLine("  call void @luma_raise_overflow()");
            _output.AppendLine("  ret ptr null");
        }
        else
        {
            _output.AppendLine("  call void @luma_throw_overflow()");
            _output.AppendLine("  unreachable");
        }
        _output.AppendLine("allocate:");
        _output.AppendLine($"  %units = call ptr @malloc({sizeType} %unit.bytes)");
        _output.AppendLine("  %units.failed = icmp eq ptr %units, null");
        _output.AppendLine("  br i1 %units.failed, label %out.of.memory, label %allocate.normalized");
        _output.AppendLine("out.of.memory:");
        _output.AppendLine(StringRuntimeOutOfMemoryBody("  "));
        _output.AppendLine("allocate.normalized:");
        _output.AppendLine($"  %utf8.byte.count.native = {(sizeType == "i64" ? "zext i32 %utf8.byte.count to i64" : "add i32 %utf8.byte.count, 0")}");
        _output.AppendLine($"  %normalized = call ptr @malloc({sizeType} %utf8.byte.count.native)");
        _output.AppendLine("  %normalized.failed = icmp eq ptr %normalized, null");
        _output.AppendLine("  br i1 %normalized.failed, label %normalized.out.of.memory, label %fill.loop");
        _output.AppendLine("normalized.out.of.memory:");
        _output.AppendLine("  call void @free(ptr %units)");
        _output.AppendLine(StringRuntimeOutOfMemoryBody("  "));
        _output.AppendLine("fill.loop:");
        _output.AppendLine("  %fill.byte.index = phi i32 [ 0, %allocate.normalized ], [ %fill.next.byte, %fill.advance ]");
        _output.AppendLine("  %fill.unit.index = phi i32 [ 0, %allocate.normalized ], [ %fill.next.unit, %fill.advance ]");
        _output.AppendLine("  %fill.output.index = phi i32 [ 0, %allocate.normalized ], [ %fill.next.output.index, %fill.advance ]");
        _output.AppendLine("  %fill.done = icmp uge i32 %fill.byte.index, %length.index");
        _output.AppendLine("  br i1 %fill.done, label %copy.string, label %fill.decode");
        _output.AppendLine("fill.decode:");
        _output.AppendLine("  %fill.decoded = call { i32, i32 } @luma_utf8_decode(ptr %source, i32 %fill.byte.index)");
        _output.AppendLine("  %fill.codepoint = extractvalue { i32, i32 } %fill.decoded, 0");
        _output.AppendLine("  %fill.width = extractvalue { i32, i32 } %fill.decoded, 1");
        _output.AppendLine("  %fill.next.byte = add i32 %fill.byte.index, %fill.width");
        _output.AppendLine("  %fill.supplementary = icmp ugt i32 %fill.codepoint, 65535");
        _output.AppendLine("  br i1 %fill.supplementary, label %write.surrogate.pair, label %write.basic.unit");
        _output.AppendLine("write.basic.unit:");
        _output.AppendLine("  %basic.unit = trunc i32 %fill.codepoint to i16");
        _output.AppendLine("  %basic.address = getelementptr i16, ptr %units, i32 %fill.unit.index");
        _output.AppendLine("  store i16 %basic.unit, ptr %basic.address");
        _output.AppendLine("  %basic.next.unit = add i32 %fill.unit.index, 1");
        _output.AppendLine("  %basic.ascii = icmp ult i32 %fill.codepoint, 128");
        _output.AppendLine("  br i1 %basic.ascii, label %encode.ascii, label %encode.two.check");
        _output.AppendLine("encode.ascii:");
        _output.AppendLine("  %encoded.ascii = trunc i32 %fill.codepoint to i8");
        _output.AppendLine("  %encoded.ascii.address = getelementptr i8, ptr %normalized, i32 %fill.output.index");
        _output.AppendLine("  store i8 %encoded.ascii, ptr %encoded.ascii.address");
        _output.AppendLine("  %encoded.ascii.next = add i32 %fill.output.index, 1");
        _output.AppendLine("  br label %fill.advance");
        _output.AppendLine("encode.two.check:");
        _output.AppendLine("  %basic.two.byte = icmp ult i32 %fill.codepoint, 2048");
        _output.AppendLine("  br i1 %basic.two.byte, label %encode.two, label %encode.three");
        _output.AppendLine("encode.two:");
        _output.AppendLine("  %encoded.two.first.bits = lshr i32 %fill.codepoint, 6");
        _output.AppendLine("  %encoded.two.first = or i32 %encoded.two.first.bits, 192");
        _output.AppendLine("  %encoded.two.second.bits = and i32 %fill.codepoint, 63");
        _output.AppendLine("  %encoded.two.second = or i32 %encoded.two.second.bits, 128");
        _output.AppendLine("  %encoded.two.first.byte = trunc i32 %encoded.two.first to i8");
        _output.AppendLine("  %encoded.two.second.byte = trunc i32 %encoded.two.second to i8");
        _output.AppendLine("  %encoded.two.first.address = getelementptr i8, ptr %normalized, i32 %fill.output.index");
        _output.AppendLine("  store i8 %encoded.two.first.byte, ptr %encoded.two.first.address");
        _output.AppendLine("  %encoded.two.second.index = add i32 %fill.output.index, 1");
        _output.AppendLine("  %encoded.two.second.address = getelementptr i8, ptr %normalized, i32 %encoded.two.second.index");
        _output.AppendLine("  store i8 %encoded.two.second.byte, ptr %encoded.two.second.address");
        _output.AppendLine("  %encoded.two.next = add i32 %fill.output.index, 2");
        _output.AppendLine("  br label %fill.advance");
        _output.AppendLine("encode.three:");
        _output.AppendLine("  %encoded.three.first.bits = lshr i32 %fill.codepoint, 12");
        _output.AppendLine("  %encoded.three.first = or i32 %encoded.three.first.bits, 224");
        _output.AppendLine("  %encoded.three.second.bits = lshr i32 %fill.codepoint, 6");
        _output.AppendLine("  %encoded.three.second.masked = and i32 %encoded.three.second.bits, 63");
        _output.AppendLine("  %encoded.three.second = or i32 %encoded.three.second.masked, 128");
        _output.AppendLine("  %encoded.three.third.bits = and i32 %fill.codepoint, 63");
        _output.AppendLine("  %encoded.three.third = or i32 %encoded.three.third.bits, 128");
        _output.AppendLine("  %encoded.three.first.byte = trunc i32 %encoded.three.first to i8");
        _output.AppendLine("  %encoded.three.second.byte = trunc i32 %encoded.three.second to i8");
        _output.AppendLine("  %encoded.three.third.byte = trunc i32 %encoded.three.third to i8");
        _output.AppendLine("  %encoded.three.first.address = getelementptr i8, ptr %normalized, i32 %fill.output.index");
        _output.AppendLine("  store i8 %encoded.three.first.byte, ptr %encoded.three.first.address");
        _output.AppendLine("  %encoded.three.second.index = add i32 %fill.output.index, 1");
        _output.AppendLine("  %encoded.three.second.address = getelementptr i8, ptr %normalized, i32 %encoded.three.second.index");
        _output.AppendLine("  store i8 %encoded.three.second.byte, ptr %encoded.three.second.address");
        _output.AppendLine("  %encoded.three.third.index = add i32 %fill.output.index, 2");
        _output.AppendLine("  %encoded.three.third.address = getelementptr i8, ptr %normalized, i32 %encoded.three.third.index");
        _output.AppendLine("  store i8 %encoded.three.third.byte, ptr %encoded.three.third.address");
        _output.AppendLine("  %encoded.three.next = add i32 %fill.output.index, 3");
        _output.AppendLine("  br label %fill.advance");
        _output.AppendLine("write.surrogate.pair:");
        _output.AppendLine("  %surrogate.adjusted = sub i32 %fill.codepoint, 65536");
        _output.AppendLine("  %surrogate.high.bits = lshr i32 %surrogate.adjusted, 10");
        _output.AppendLine("  %surrogate.high = add i32 %surrogate.high.bits, 55296");
        _output.AppendLine("  %surrogate.low.bits = and i32 %surrogate.adjusted, 1023");
        _output.AppendLine("  %surrogate.low = add i32 %surrogate.low.bits, 56320");
        _output.AppendLine("  %surrogate.high.unit = trunc i32 %surrogate.high to i16");
        _output.AppendLine("  %surrogate.low.unit = trunc i32 %surrogate.low to i16");
        _output.AppendLine("  %surrogate.high.address = getelementptr i16, ptr %units, i32 %fill.unit.index");
        _output.AppendLine("  store i16 %surrogate.high.unit, ptr %surrogate.high.address");
        _output.AppendLine("  %surrogate.low.index = add i32 %fill.unit.index, 1");
        _output.AppendLine("  %surrogate.low.address = getelementptr i16, ptr %units, i32 %surrogate.low.index");
        _output.AppendLine("  store i16 %surrogate.low.unit, ptr %surrogate.low.address");
        _output.AppendLine("  %surrogate.next.unit = add i32 %fill.unit.index, 2");
        _output.AppendLine("  %encoded.four.first.bits = lshr i32 %fill.codepoint, 18");
        _output.AppendLine("  %encoded.four.first = or i32 %encoded.four.first.bits, 240");
        _output.AppendLine("  %encoded.four.second.bits = lshr i32 %fill.codepoint, 12");
        _output.AppendLine("  %encoded.four.second.masked = and i32 %encoded.four.second.bits, 63");
        _output.AppendLine("  %encoded.four.second = or i32 %encoded.four.second.masked, 128");
        _output.AppendLine("  %encoded.four.third.bits = lshr i32 %fill.codepoint, 6");
        _output.AppendLine("  %encoded.four.third.masked = and i32 %encoded.four.third.bits, 63");
        _output.AppendLine("  %encoded.four.third = or i32 %encoded.four.third.masked, 128");
        _output.AppendLine("  %encoded.four.fourth.bits = and i32 %fill.codepoint, 63");
        _output.AppendLine("  %encoded.four.fourth = or i32 %encoded.four.fourth.bits, 128");
        _output.AppendLine("  %encoded.four.first.byte = trunc i32 %encoded.four.first to i8");
        _output.AppendLine("  %encoded.four.second.byte = trunc i32 %encoded.four.second to i8");
        _output.AppendLine("  %encoded.four.third.byte = trunc i32 %encoded.four.third to i8");
        _output.AppendLine("  %encoded.four.fourth.byte = trunc i32 %encoded.four.fourth to i8");
        _output.AppendLine("  %encoded.four.first.address = getelementptr i8, ptr %normalized, i32 %fill.output.index");
        _output.AppendLine("  store i8 %encoded.four.first.byte, ptr %encoded.four.first.address");
        _output.AppendLine("  %encoded.four.second.index = add i32 %fill.output.index, 1");
        _output.AppendLine("  %encoded.four.second.address = getelementptr i8, ptr %normalized, i32 %encoded.four.second.index");
        _output.AppendLine("  store i8 %encoded.four.second.byte, ptr %encoded.four.second.address");
        _output.AppendLine("  %encoded.four.third.index = add i32 %fill.output.index, 2");
        _output.AppendLine("  %encoded.four.third.address = getelementptr i8, ptr %normalized, i32 %encoded.four.third.index");
        _output.AppendLine("  store i8 %encoded.four.third.byte, ptr %encoded.four.third.address");
        _output.AppendLine("  %encoded.four.fourth.index = add i32 %fill.output.index, 3");
        _output.AppendLine("  %encoded.four.fourth.address = getelementptr i8, ptr %normalized, i32 %encoded.four.fourth.index");
        _output.AppendLine("  store i8 %encoded.four.fourth.byte, ptr %encoded.four.fourth.address");
        _output.AppendLine("  %encoded.four.next = add i32 %fill.output.index, 4");
        _output.AppendLine("  br label %fill.advance");
        _output.AppendLine("fill.advance:");
        _output.AppendLine("  %fill.next.unit = phi i32 [ %basic.next.unit, %encode.ascii ], [ %basic.next.unit, %encode.two ], [ %basic.next.unit, %encode.three ], [ %surrogate.next.unit, %write.surrogate.pair ]");
        _output.AppendLine("  %fill.next.output.index = phi i32 [ %encoded.ascii.next, %encode.ascii ], [ %encoded.two.next, %encode.two ], [ %encoded.three.next, %encode.three ], [ %encoded.four.next, %write.surrogate.pair ]");
        _output.AppendLine("  br label %fill.loop");
        _output.AppendLine("copy.string:");
        _output.AppendLine("  %string = call ptr @luma_string_from_utf8(ptr %normalized, i32 %utf8.byte.count, i32 %unit.count, ptr %units, i1 %ascii.only)");
        _output.AppendLine("  call void @free(ptr %units)");
        _output.AppendLine("  call void @free(ptr %normalized)");
        _output.AppendLine("  ret ptr %string");
        _output.AppendLine("}");

    }

    private void EmitWindowsWideStringRuntime(string sizeType)
    {
        _output.AppendLine("declare i32 @WideCharToMultiByte(i32, i32, ptr, i32, ptr, i32, ptr, ptr)");
        _output.AppendLine("define internal ptr @luma_string_from_winwide(ptr %source) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %needed = call i32 @WideCharToMultiByte(i32 65001, i32 0, ptr %source, i32 -1, ptr null, i32 0, ptr null, ptr null)");
        _output.AppendLine("  %conversion.failed = icmp sle i32 %needed, 0");
        _output.AppendLine("  br i1 %conversion.failed, label %out.of.memory, label %allocate");
        _output.AppendLine("allocate:");
        _output.AppendLine($"  %needed.native = {(sizeType == "i64" ? "zext i32 %needed to i64" : "add i32 %needed, 0")}");
        _output.AppendLine($"  %buffer = call ptr @malloc({sizeType} %needed.native)");
        _output.AppendLine("  %buffer.failed = icmp eq ptr %buffer, null");
        _output.AppendLine("  br i1 %buffer.failed, label %out.of.memory, label %convert");
        _output.AppendLine("convert:");
        _output.AppendLine("  %written = call i32 @WideCharToMultiByte(i32 65001, i32 0, ptr %source, i32 -1, ptr %buffer, i32 %needed, ptr null, ptr null)");
        _output.AppendLine("  %write.failed = icmp sle i32 %written, 0");
        _output.AppendLine("  br i1 %write.failed, label %conversion.cleanup, label %create.string");
        _output.AppendLine("conversion.cleanup:");
        _output.AppendLine("  call void @free(ptr %buffer)");
        _output.AppendLine("  br label %out.of.memory");
        _output.AppendLine("create.string:");
        _output.AppendLine("  %string = call ptr @luma_string_from_utf8z(ptr %buffer)");
        _output.AppendLine("  call void @free(ptr %buffer)");
        _output.AppendLine("  ret ptr %string");
        _output.AppendLine("out.of.memory:");
        _output.AppendLine(StringRuntimeOutOfMemoryBody("  "));
        _output.AppendLine("}");
    }

    private bool UsesWindowsCommandLineApi()
    {
        if (compilation.TargetTriple == "host") return OperatingSystem.IsWindows();
        return compilation.TargetTriple.Contains("windows", StringComparison.OrdinalIgnoreCase) ||
               compilation.TargetTriple.Contains("win32", StringComparison.OrdinalIgnoreCase) ||
               compilation.TargetTriple.Contains("mingw", StringComparison.OrdinalIgnoreCase);
    }

    private void EmitEntryPoint(MethodSymbol entry)
    {
        var takesArguments = entry.Declaration.Parameters.Count == 1;
        _output.AppendLine(takesArguments ? "define i32 @main(i32 %argc, ptr %argv) {" : "define i32 @main() {");
        _output.AppendLine("entry:");
        if (takesArguments) EmitProcessArgumentsInEntryPoint();

        if (_typeInitializationFunctions.TryGetValue(QualifiedTypeName(entry.Owner), out var entryTypeInitializer))
        {
            _output.AppendLine($"  call void @{entryTypeInitializer}()");
            if (compilation.UsesExceptionHandling)
            {
                _output.AppendLine("  %entry.initialization.exception = load ptr, ptr @luma_pending_exception");
                _output.AppendLine("  %entry.initialization.failed = icmp ne ptr %entry.initialization.exception, null");
                _output.AppendLine("  br i1 %entry.initialization.failed, label %entry.initialization.failure, label %entry.call");
                _output.AppendLine("entry.initialization.failure:");
                if (takesArguments) _output.AppendLine("  call void @luma_array_release(ptr %luma.arguments)");
                _output.AppendLine("  call void @luma_report_unhandled_exception(ptr %entry.initialization.exception)");
                _output.AppendLine("  ret i32 70");
                _output.AppendLine("entry.call:");
            }
        }

        var arguments = takesArguments ? "(ptr %luma.arguments)" : "()";
        var call = $"call {LlvmType(entry.Declaration.ReturnType)} @{FunctionName(entry, compilation.Methods)}{arguments}";
        if (entry.Declaration.ReturnType == "void") _output.AppendLine($"  {call}");
        else _output.AppendLine($"  %exit = {call}");
        if (takesArguments) _output.AppendLine("  call void @luma_array_release(ptr %luma.arguments)");
        if (compilation.UsesExceptionHandling)
        {
            _output.AppendLine("  %unhandled.exception = load ptr, ptr @luma_pending_exception");
            _output.AppendLine("  %has.unhandled.exception = icmp ne ptr %unhandled.exception, null");
            _output.AppendLine("  br i1 %has.unhandled.exception, label %unhandled, label %normal.exit");
            _output.AppendLine("unhandled:");
            _output.AppendLine("  call void @luma_report_unhandled_exception(ptr %unhandled.exception)");
            _output.AppendLine("  ret i32 70");
            _output.AppendLine("normal.exit:");
        }
        _output.AppendLine(entry.Declaration.ReturnType == "void" ? "  ret i32 0" : "  ret i32 %exit");
        _output.AppendLine("}");
    }

    private void EmitProcessArgumentsInEntryPoint()
    {
        var useWindowsWideArguments = UsesWindowsCommandLineApi();
        if (useWindowsWideArguments)
        {
            _output.AppendLine("  %luma.wide.argc.address = alloca i32, align 4");
            _output.AppendLine("  %luma.command.line = call ptr @GetCommandLineW()");
            _output.AppendLine("  %luma.wide.arguments = call ptr @CommandLineToArgvW(ptr %luma.command.line, ptr %luma.wide.argc.address)");
            _output.AppendLine("  %luma.wide.arguments.failed = icmp eq ptr %luma.wide.arguments, null");
            _output.AppendLine("  br i1 %luma.wide.arguments.failed, label %entry.arguments.parse.failure, label %entry.arguments.parse.ready");
            _output.AppendLine("entry.arguments.parse.failure:");
            if (compilation.UsesExceptionHandling)
            {
                _output.AppendLine("  call void @luma_raise_out_of_memory()");
                _output.AppendLine("  %luma.parse.exception = load ptr, ptr @luma_pending_exception");
                _output.AppendLine("  call void @luma_report_unhandled_exception(ptr %luma.parse.exception)");
                _output.AppendLine("  ret i32 70");
            }
            else
            {
                _output.AppendLine("  call void @luma_throw_out_of_memory()");
                _output.AppendLine("  unreachable");
            }
            _output.AppendLine("entry.arguments.parse.ready:");
            _output.AppendLine("  %luma.native.argc = load i32, ptr %luma.wide.argc.address");
            _output.AppendLine("  %luma.native.arguments = getelementptr ptr, ptr %luma.wide.arguments, i32 0");
            _output.AppendLine("  br label %entry.arguments.process");
        }
        else
        {
            _output.AppendLine("  %luma.native.argc = add i32 %argc, 0");
            _output.AppendLine("  %luma.native.arguments = getelementptr ptr, ptr %argv, i32 0");
            _output.AppendLine("  br label %entry.arguments.process");
        }
        _output.AppendLine("entry.arguments.process:");
        _output.AppendLine("  %luma.has.arguments = icmp sgt i32 %luma.native.argc, 1");
        _output.AppendLine("  %luma.argument.count.raw = sub i32 %luma.native.argc, 1");
        _output.AppendLine("  %luma.argument.count = select i1 %luma.has.arguments, i32 %luma.argument.count.raw, i32 0");
        _output.AppendLine("  %luma.arguments.pair = call { ptr, i32 } @luma_array_new(i32 %luma.argument.count, i32 1, i32 %luma.argument.count, i32 0, i32 0, i32 " + compilation.NativePointerSize + ", i1 true, i1 true, i1 false, i1 false, i1 false)");
        _output.AppendLine("  %luma.arguments = extractvalue { ptr, i32 } %luma.arguments.pair, 0");
        _output.AppendLine("  %luma.arguments.status = extractvalue { ptr, i32 } %luma.arguments.pair, 1");
        _output.AppendLine("  %luma.arguments.allocated = icmp eq i32 %luma.arguments.status, 0");
        _output.AppendLine("  br i1 %luma.arguments.allocated, label %entry.arguments.initialize, label %entry.arguments.allocation.failure");
        _output.AppendLine("entry.arguments.allocation.failure:");
        _output.AppendLine("  %luma.arguments.overflowed = icmp eq i32 %luma.arguments.status, 1");
        _output.AppendLine("  br i1 %luma.arguments.overflowed, label %entry.arguments.overflow, label %entry.arguments.out.of.memory");
        _output.AppendLine("entry.arguments.overflow:");
        if (compilation.UsesExceptionHandling)
        {
            _output.AppendLine("  call void @luma_raise_overflow()");
            _output.AppendLine("  br label %entry.arguments.unhandled");
        }
        else
        {
            _output.AppendLine("  %luma.arguments.local.free.overflow = call ptr @LocalFree(ptr %luma.wide.arguments)");
            _output.AppendLine("  call void @luma_throw_overflow()");
            _output.AppendLine("  unreachable");
        }
        _output.AppendLine("entry.arguments.out.of.memory:");
        if (compilation.UsesExceptionHandling)
        {
            _output.AppendLine("  call void @luma_raise_out_of_memory()");
            _output.AppendLine("  br label %entry.arguments.unhandled");
        }
        else
        {
            _output.AppendLine("  %luma.arguments.local.free.oom = call ptr @LocalFree(ptr %luma.wide.arguments)");
            _output.AppendLine("  call void @luma_throw_out_of_memory()");
            _output.AppendLine("  unreachable");
        }
        if (compilation.UsesExceptionHandling)
        {
            _output.AppendLine("entry.arguments.unhandled:");
            _output.AppendLine("  %luma.arguments.exception = load ptr, ptr @luma_pending_exception");
            _output.AppendLine("  call void @luma_report_unhandled_exception(ptr %luma.arguments.exception)");
            if (useWindowsWideArguments) _output.AppendLine("  %luma.arguments.local.free.unhandled = call ptr @LocalFree(ptr %luma.wide.arguments)");
            _output.AppendLine("  ret i32 70");
        }
        _output.AppendLine("entry.arguments.initialize:");
        _output.AppendLine($"  %luma.arguments.descriptor.address = getelementptr %luma.array, ptr %luma.arguments, i32 0, i32 0");
        _output.AppendLine($"  store ptr {ArrayDescriptorPointer("string[]")}, ptr %luma.arguments.descriptor.address");
        _output.AppendLine("  %luma.arguments.data = getelementptr %luma.array, ptr %luma.arguments, i32 0, i32 7");
        _output.AppendLine("  br label %entry.arguments.loop");
        _output.AppendLine("entry.arguments.loop:");
        _output.AppendLine("  %luma.arguments.index = phi i32 [ 0, %entry.arguments.initialize ], [ %luma.arguments.next, %entry.arguments.store ]");
        _output.AppendLine("  %luma.arguments.more = icmp ult i32 %luma.arguments.index, %luma.argument.count");
        _output.AppendLine("  br i1 %luma.arguments.more, label %entry.arguments.read, label %entry.arguments.ready");
        _output.AppendLine("entry.arguments.read:");
        _output.AppendLine("  %luma.native.argument.index = add i32 %luma.arguments.index, 1");
        _output.AppendLine("  %luma.native.argument.address = getelementptr ptr, ptr %luma.native.arguments, i32 %luma.native.argument.index");
        _output.AppendLine("  %luma.native.argument = load ptr, ptr %luma.native.argument.address");
        _output.AppendLine(useWindowsWideArguments
            ? "  %luma.argument.string = call ptr @luma_string_from_winwide(ptr %luma.native.argument)"
            : "  %luma.argument.string = call ptr @luma_string_from_utf8z(ptr %luma.native.argument)");
        if (compilation.UsesExceptionHandling)
        {
            _output.AppendLine("  %luma.argument.exception = load ptr, ptr @luma_pending_exception");
            _output.AppendLine("  %luma.argument.failed = icmp ne ptr %luma.argument.exception, null");
            _output.AppendLine("  br i1 %luma.argument.failed, label %entry.arguments.conversion.failure, label %entry.arguments.store.value");
            _output.AppendLine("entry.arguments.conversion.failure:");
            _output.AppendLine("  call void @luma_array_release(ptr %luma.arguments)");
            if (useWindowsWideArguments) _output.AppendLine("  %luma.arguments.local.free.failure = call ptr @LocalFree(ptr %luma.wide.arguments)");
            _output.AppendLine("  call void @luma_report_unhandled_exception(ptr %luma.argument.exception)");
            _output.AppendLine("  ret i32 70");
            _output.AppendLine("entry.arguments.store.value:");
        }
        _output.AppendLine("  %luma.argument.element.address = getelementptr ptr, ptr %luma.arguments.data, i32 %luma.arguments.index");
        _output.AppendLine("  store ptr %luma.argument.string, ptr %luma.argument.element.address");
        _output.AppendLine("  br label %entry.arguments.store");
        _output.AppendLine("entry.arguments.store:");
        _output.AppendLine("  %luma.arguments.next = add i32 %luma.arguments.index, 1");
        _output.AppendLine("  br label %entry.arguments.loop");
        _output.AppendLine("entry.arguments.ready:");
        if (useWindowsWideArguments) _output.AppendLine("  %luma.arguments.local.free = call ptr @LocalFree(ptr %luma.wide.arguments)");
    }

    private MethodSymbol FindTypeInitializationExceptionConstructor()
    {
        var constructor = compilation.Methods.FirstOrDefault(method =>
            QualifiedTypeName(method.Owner) == "System.TypeInitializationException" &&
            method.IsClassConstructor && method.Declaration.Parameters.Count == 2 &&
            method.Declaration.Parameters[0].TypeName == "string" &&
            method.Declaration.Parameters[1].TypeName == "System.Exception?");
        return constructor ?? throw new InvalidOperationException("The compiler-known TypeInitializationException(string, System.Exception?) constructor is unavailable.");
    }

    private string EmitTypeInitializationExceptionObject(string prefix, string messageSymbol, string innerException, MethodSymbol constructor)
    {
        const string typeName = "System.TypeInitializationException";
        if (!compilation.ClassTypes.TryGetValue(typeName, out var exceptionType))
            throw new InvalidOperationException("The compiler-known TypeInitializationException type is unavailable.");
        var safeType = Safe(typeName);
        var sizeType = compilation.NativePointerSize == 8 ? "i64" : "i32";
        var wrapperInstance = $"%{prefix}.instance";
        _output.AppendLine($"  %{prefix}.size.address = getelementptr %class.{safeType}, ptr null, i32 1");
        _output.AppendLine($"  %{prefix}.size = ptrtoint ptr %{prefix}.size.address to {sizeType}");
        _output.AppendLine($"  {wrapperInstance} = call ptr @malloc({sizeType} %{prefix}.size)");
        _output.AppendLine($"  %{prefix}.out.of.memory = icmp eq ptr {wrapperInstance}, null");
        _output.AppendLine($"  br i1 %{prefix}.out.of.memory, label %{prefix}.oom, label %{prefix}.initialize");
        _output.AppendLine($"{prefix}.oom:");
        _output.AppendLine($"  br label %{prefix}.ready");
        _output.AppendLine($"{prefix}.initialize:");
        _output.AppendLine($"  %{prefix}.descriptor = getelementptr %class.{safeType}, ptr {wrapperInstance}, i32 0, i32 0");
        _output.AppendLine($"  store ptr @luma_classdesc_{safeType}, ptr %{prefix}.descriptor");
        _output.AppendLine($"  %{prefix}.count = getelementptr %class.{safeType}, ptr {wrapperInstance}, i32 0, i32 1");
        _output.AppendLine($"  store i32 1, ptr %{prefix}.count");
        _output.AppendLine($"  %{prefix}.flags = getelementptr %class.{safeType}, ptr {wrapperInstance}, i32 0, i32 2");
        _output.AppendLine($"  store i32 0, ptr %{prefix}.flags");
        _output.AppendLine($"  %{prefix}.weak = getelementptr %class.{safeType}, ptr {wrapperInstance}, i32 0, i32 3");
        _output.AppendLine($"  store ptr null, ptr %{prefix}.weak");
        foreach (var field in exceptionType.Fields)
        {
            _output.AppendLine($"  %{prefix}.field.{field.Index} = getelementptr %class.{safeType}, ptr {wrapperInstance}, i32 0, i32 {field.Index + 4}");
            _output.AppendLine($"  store {LlvmType(field.TypeName)} {DefaultStaticStorageValue(field.TypeName)}, ptr %{prefix}.field.{field.Index}");
        }
        _output.AppendLine($"  call void @{FunctionName(constructor, compilation.Methods)}(ptr {wrapperInstance}, ptr @{messageSymbol}, ptr {innerException}, i1 true)");
        _output.AppendLine($"  br label %{prefix}.ready");
        _output.AppendLine($"{prefix}.ready:");
        _output.AppendLine($"  %{prefix}.result = phi ptr [ @luma_oom_instance, %{prefix}.oom ], [ {wrapperInstance}, %{prefix}.initialize ]");
        return $"%{prefix}.result";
    }

    private string DefaultStaticStorageValue(string type)
    {
        if (type == "string") return "@luma_string_empty";
        var llvmType = LlvmType(type);
        if (llvmType == "ptr") return "null";
        return llvmType is "float" or "double" ? "0.0" : llvmType.StartsWith('{') || llvmType.StartsWith('[') ? "zeroinitializer" : "0";
    }

    private void EmitExceptionRuntime()
    {
        _output.AppendLine("define internal i1 @luma_exception_is_instance(ptr %exception, ptr %target) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %null.exception = icmp eq ptr %exception, null");
        _output.AppendLine("  br i1 %null.exception, label %no, label %start");
        _output.AppendLine("start:");
        _output.AppendLine("  %dynamic.type = load ptr, ptr %exception");
        _output.AppendLine("  br label %walk");
        _output.AppendLine("walk:");
        _output.AppendLine("  %current.type = phi ptr [ %dynamic.type, %start ], [ %base.type, %advance ]");
        _output.AppendLine("  %matches = icmp eq ptr %current.type, %target");
        _output.AppendLine("  br i1 %matches, label %yes, label %advance");
        _output.AppendLine("advance:");
        _output.AppendLine("  %base.address = getelementptr %luma.typedesc, ptr %current.type, i32 0, i32 1");
        _output.AppendLine("  %base.type = load ptr, ptr %base.address");
        _output.AppendLine("  %no.base = icmp eq ptr %base.type, null");
        _output.AppendLine("  br i1 %no.base, label %no, label %walk");
        _output.AppendLine("yes:"); _output.AppendLine("  ret i1 true");
        _output.AppendLine("no:"); _output.AppendLine("  ret i1 false");
        _output.AppendLine("}");
        _output.AppendLine("define internal ptr @luma_eh_find_handler(ptr %exception) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %initial.node = load ptr, ptr @luma_eh_top");
        _output.AppendLine("  br label %node.loop");
        _output.AppendLine("node.loop:");
        _output.AppendLine("  %node = phi ptr [ %initial.node, %entry ], [ %previous.node, %next.node ]");
        _output.AppendLine("  %node.is.null = icmp eq ptr %node, null");
        _output.AppendLine("  br i1 %node.is.null, label %not.found, label %clause.start");
        _output.AppendLine("clause.start:");
        _output.AppendLine("  %catch.types.address = getelementptr %luma.ehnode, ptr %node, i32 0, i32 2");
        _output.AppendLine("  %catch.types = load ptr, ptr %catch.types.address");
        _output.AppendLine("  %catch.count.address = getelementptr %luma.ehnode, ptr %node, i32 0, i32 3");
        _output.AppendLine("  %catch.count = load i32, ptr %catch.count.address");
        _output.AppendLine("  br label %clause.loop");
        _output.AppendLine("clause.loop:");
        _output.AppendLine("  %clause.index = phi i32 [ 0, %clause.start ], [ %next.clause.index, %clause.next ]");
        _output.AppendLine("  %has.clause = icmp ult i32 %clause.index, %catch.count");
        _output.AppendLine("  br i1 %has.clause, label %clause.test, label %next.node");
        _output.AppendLine("clause.test:");
        _output.AppendLine("  %catch.type.address = getelementptr ptr, ptr %catch.types, i32 %clause.index");
        _output.AppendLine("  %catch.type = load ptr, ptr %catch.type.address");
        _output.AppendLine("  %is.catch.all = icmp eq ptr %catch.type, null");
        _output.AppendLine("  br i1 %is.catch.all, label %matched, label %typed.test");
        _output.AppendLine("typed.test:");
        _output.AppendLine("  %typed.match = call i1 @luma_exception_is_instance(ptr %exception, ptr %catch.type)");
        _output.AppendLine("  br i1 %typed.match, label %matched, label %clause.next");
        _output.AppendLine("clause.next:");
        _output.AppendLine("  %next.clause.index = add i32 %clause.index, 1");
        _output.AppendLine("  br label %clause.loop");
        _output.AppendLine("matched:"); _output.AppendLine("  ret ptr %node");
        _output.AppendLine("next.node:");
        _output.AppendLine("  %previous.address = getelementptr %luma.ehnode, ptr %node, i32 0, i32 0");
        _output.AppendLine("  %previous.node = load ptr, ptr %previous.address");
        _output.AppendLine("  br label %node.loop");
        _output.AppendLine("not.found:"); _output.AppendLine("  ret ptr null");
        _output.AppendLine("}");
        EmitRaiseSingletonException("luma_raise_out_of_memory", "luma_oom_instance");
        EmitRaiseSingletonException("luma_raise_overflow", "luma_overflow_instance");
        EmitUnhandledExceptionReporter();
    }

    private void EmitRaiseSingletonException(string functionName, string instanceName)
    {
        _output.AppendLine($"define internal void @{functionName}() {{");
        _output.AppendLine("entry:");
        _output.AppendLine("  %previous.exception = load ptr, ptr @luma_pending_exception");
        _output.AppendLine("  %has.previous.exception = icmp ne ptr %previous.exception, null");
        _output.AppendLine("  br i1 %has.previous.exception, label %release.previous, label %store.new");
        _output.AppendLine("release.previous:");
        _output.AppendLine("  call void @luma_class_release_dynamic(ptr %previous.exception)");
        _output.AppendLine("  br label %store.new");
        _output.AppendLine("store.new:");
        _output.AppendLine($"  store ptr @{instanceName}, ptr @luma_pending_exception");
        _output.AppendLine($"  %handler = call ptr @luma_eh_find_handler(ptr @{instanceName})");
        _output.AppendLine("  store ptr %handler, ptr @luma_pending_handler");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");
    }

    private void EmitStderrWriterRuntime()
    {
        var windowsTarget = compilation.TargetTriple == "host"
            ? OperatingSystem.IsWindows()
            : compilation.TargetTriple.Contains("windows", StringComparison.OrdinalIgnoreCase) ||
              compilation.TargetTriple.Contains("win32", StringComparison.OrdinalIgnoreCase) ||
              compilation.TargetTriple.Contains("mingw", StringComparison.OrdinalIgnoreCase);
        var pointerBits = compilation.NativePointerSize * 8;
        var sizeType = "i" + pointerBits;
        var writeName = windowsTarget ? "_write" : "write";
        if (windowsTarget)
            _output.AppendLine("declare i32 @_write(i32, ptr, i32)");
        else
            _output.AppendLine($"declare {sizeType} @write(i32, ptr, {sizeType})");

        _output.AppendLine("define internal void @luma_write_stderr(ptr %bytes, i32 %length) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %is.empty = icmp eq i32 %length, 0");
        _output.AppendLine("  %is.null = icmp eq ptr %bytes, null");
        _output.AppendLine("  %skip = or i1 %is.empty, %is.null");
        _output.AppendLine("  br i1 %skip, label %done, label %loop");
        _output.AppendLine("loop:");
        _output.AppendLine("  %written = phi i32 [ 0, %entry ], [ %next, %advance ]");
        _output.AppendLine("  %remaining = sub i32 %length, %written");
        _output.AppendLine("  %current = getelementptr i8, ptr %bytes, i32 %written");
        if (windowsTarget)
        {
            _output.AppendLine($"  %write.result = call i32 @{writeName}(i32 2, ptr %current, i32 %remaining)");
            _output.AppendLine("  %write.failed = icmp sle i32 %write.result, 0");
            _output.AppendLine("  %next = add i32 %written, %write.result");
        }
        else
        {
            var count = pointerBits == 64 ? "%remaining.native = zext i32 %remaining to i64" : "";
            if (count.Length > 0) _output.AppendLine("  " + count);
            var nativeCount = pointerBits == 64 ? "%remaining.native" : "%remaining";
            _output.AppendLine($"  %write.result = call {sizeType} @{writeName}(i32 2, ptr %current, {sizeType} {nativeCount})");
            _output.AppendLine($"  %write.failed = icmp sle {sizeType} %write.result, 0");
            if (pointerBits == 64) _output.AppendLine("  %write.result.i32 = trunc i64 %write.result to i32");
            var result32 = pointerBits == 64 ? "%write.result.i32" : "%write.result";
            _output.AppendLine($"  %next = add i32 %written, {result32}");
        }
        _output.AppendLine("  br i1 %write.failed, label %done, label %advance");
        _output.AppendLine("advance:");
        _output.AppendLine("  %complete = icmp uge i32 %next, %length");
        _output.AppendLine("  br i1 %complete, label %done, label %loop");
        _output.AppendLine("done:");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");
        _output.AppendLine("define internal void @luma_write_stderr_cstr(ptr %text) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %is.null = icmp eq ptr %text, null");
        _output.AppendLine("  br i1 %is.null, label %done, label %loop");
        _output.AppendLine("loop:");
        _output.AppendLine("  %length = phi i32 [ 0, %entry ], [ %next, %advance ]");
        _output.AppendLine("  %address = getelementptr i8, ptr %text, i32 %length");
        _output.AppendLine("  %byte = load i8, ptr %address");
        _output.AppendLine("  %is.end = icmp eq i8 %byte, 0");
        _output.AppendLine("  br i1 %is.end, label %write, label %advance");
        _output.AppendLine("advance:");
        _output.AppendLine("  %next = add i32 %length, 1");
        _output.AppendLine("  br label %loop");
        _output.AppendLine("write:");
        _output.AppendLine("  call void @luma_write_stderr(ptr %text, i32 %length)");
        _output.AppendLine("  br label %done");
        _output.AppendLine("done:");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");
    }

    private void EmitFatalRuntime()
    {
        const string diagnostic = "LumaSharp fatal error: reference-count overflow\n";
        EmitUtf8CConstant("@luma_fatal_refcount_message", diagnostic);
        _output.AppendLine("declare void @abort() noreturn");
        _output.AppendLine("define internal void @luma_fatal_reference_count_overflow() noreturn {");
        _output.AppendLine("entry:");
        _output.AppendLine($"  call void @luma_write_stderr(ptr {ConstantPointer("@luma_fatal_refcount_message", diagnostic)}, i32 {Encoding.UTF8.GetByteCount(diagnostic)})");
        _output.AppendLine("  call void @abort()");
        _output.AppendLine("  unreachable");
        _output.AppendLine("}");
    }

    private void EmitCheckedReferenceCountIncrement(string countAddress, string doneLabel, string prefix)
    {
        var retry = prefix + ".retry";
        var observed = "%" + prefix + ".observed";
        var overflow = "%" + prefix + ".overflow";
        var next = "%" + prefix + ".next";
        var attempt = "%" + prefix + ".attempt";
        var succeeded = "%" + prefix + ".succeeded";
        _output.AppendLine($"  br label %{retry}");
        _output.AppendLine(retry + ":");
        _output.AppendLine($"  {observed} = load atomic i32, ptr {countAddress} monotonic, align 4");
        _output.AppendLine($"  {overflow} = icmp uge i32 {observed}, 2147483647");
        _output.AppendLine($"  br i1 {overflow}, label %{prefix}.fatal, label %{prefix}.increment");
        _output.AppendLine(prefix + ".fatal:");
        _output.AppendLine("  call void @luma_fatal_reference_count_overflow()");
        _output.AppendLine("  unreachable");
        _output.AppendLine(prefix + ".increment:");
        _output.AppendLine($"  {next} = add i32 {observed}, 1");
        _output.AppendLine($"  {attempt} = cmpxchg ptr {countAddress}, i32 {observed}, i32 {next} monotonic monotonic");
        _output.AppendLine($"  {succeeded} = extractvalue {{ i32, i1 }} {attempt}, 1");
        _output.AppendLine($"  br i1 {succeeded}, label %{doneLabel}, label %{retry}");
    }

    private void EmitUnhandledExceptionReporter()
    {
        EmitUtf8CConstant("@luma_unhandled_prefix", "Unhandled exception: ");
        EmitUtf8CConstant("@luma_inner_exception_prefix", "  InnerException: ");
        EmitUtf8CConstant("@luma_exception_separator", ": ");
        EmitUtf8CConstant("@luma_exception_newline", "\n");

        var unhandledPrefix = ConstantPointer("@luma_unhandled_prefix", "Unhandled exception: ");
        var innerPrefix = ConstantPointer("@luma_inner_exception_prefix", "  InnerException: ");
        var separator = ConstantPointer("@luma_exception_separator", ": ");
        var newline = ConstantPointer("@luma_exception_newline", "\n");
        var exceptionLayout = "%class." + Safe("System.Exception");
        _output.AppendLine("define internal void @luma_report_unhandled_exception(ptr %exception) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  br label %walk");
        _output.AppendLine("walk:");
        _output.AppendLine("  %current.exception = phi ptr [ %exception, %entry ], [ %inner.exception, %next ]");
        _output.AppendLine("  %is.first = phi i1 [ true, %entry ], [ false, %next ]");
        _output.AppendLine("  %current.is.null = icmp eq ptr %current.exception, null");
        _output.AppendLine("  br i1 %current.is.null, label %done, label %report");
        _output.AppendLine("report:");
        _output.AppendLine("  br i1 %is.first, label %write.first.prefix, label %write.inner.prefix");
        _output.AppendLine("write.first.prefix:");
        _output.AppendLine($"  call void @luma_write_stderr_cstr(ptr {unhandledPrefix})");
        _output.AppendLine("  br label %write.type");
        _output.AppendLine("write.inner.prefix:");
        _output.AppendLine($"  call void @luma_write_stderr_cstr(ptr {innerPrefix})");
        _output.AppendLine("  br label %write.type");
        _output.AppendLine("write.type:");
        _output.AppendLine("  %descriptor = load ptr, ptr %current.exception");
        _output.AppendLine("  %name.address = getelementptr %luma.typedesc, ptr %descriptor, i32 0, i32 0");
        _output.AppendLine("  %name = load ptr, ptr %name.address");
        _output.AppendLine("  call void @luma_write_stderr_cstr(ptr %name)");
        _output.AppendLine($"  call void @luma_write_stderr_cstr(ptr {separator})");
        _output.AppendLine($"  %message.address = getelementptr {exceptionLayout}, ptr %current.exception, i32 0, i32 3");
        _output.AppendLine("  %message = load ptr, ptr %message.address");
        _output.AppendLine("  %message.is.null = icmp eq ptr %message, null");
        _output.AppendLine("  br i1 %message.is.null, label %newline, label %write.message");
        _output.AppendLine("write.message:");
        _output.AppendLine("  %message.bytes.address = getelementptr %luma.string, ptr %message, i32 0, i32 4");
        _output.AppendLine("  %message.bytes = load ptr, ptr %message.bytes.address");
        _output.AppendLine("  %message.length.address = getelementptr %luma.string, ptr %message, i32 0, i32 6");
        _output.AppendLine("  %message.length = load i32, ptr %message.length.address");
        _output.AppendLine("  call void @luma_write_stderr(ptr %message.bytes, i32 %message.length)");
        _output.AppendLine("  br label %newline");
        _output.AppendLine("newline:");
        _output.AppendLine($"  call void @luma_write_stderr_cstr(ptr {newline})");
        _output.AppendLine($"  %inner.address = getelementptr {exceptionLayout}, ptr %current.exception, i32 0, i32 4");
        _output.AppendLine("  %inner.exception = load ptr, ptr %inner.address");
        _output.AppendLine("  %has.inner = icmp ne ptr %inner.exception, null");
        _output.AppendLine("  br i1 %has.inner, label %next, label %done");
        _output.AppendLine("next:");
        _output.AppendLine("  br label %walk");
        _output.AppendLine("done:");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");
    }

    private void EmitUtf8CConstant(string name, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value + "\0");
        var encoded = string.Concat(bytes.Select(value => "\\" + value.ToString("X2", System.Globalization.CultureInfo.InvariantCulture)));
        _output.AppendLine($"{name} = private unnamed_addr constant [{bytes.Length} x i8] c\"{encoded}\"");
    }

    private string ConstantPointer(string name, string value)
    {
        var length = Encoding.UTF8.GetByteCount(value + "\0");
        return $"getelementptr inbounds ([{length} x i8], ptr {name}, i32 0, i32 0)";
    }
    private bool HasStringSupport() => compilation.StringLiteralPool.Count > 0 ||
        compilation.StringLengthAccesses.Count > 0 || compilation.StringIndexAccesses.Count > 0 ||
        compilation.VariableTypes.Values.Any(IsStringType) ||
        compilation.DefaultTypes.Values.Contains("string", StringComparer.Ordinal) ||
        compilation.ClassToStringCalls.Count > 0 ||
        compilation.StructToStringCalls.Count > 0 ||
        compilation.ConstantFields.Values.Any(field => IsStringType(field.TypeName)) ||
        compilation.StaticFields.Any(field => IsStringType(field.Declaration.TypeName)) ||
        compilation.ClassTypes.Values.Any(classType => classType.Fields.Any(field => IsStringType(field.TypeName))) ||
        compilation.Methods.Any(method => IsStringType(method.Declaration.ReturnType) || method.Declaration.Parameters.Any(parameter => IsStringType(parameter.TypeName))) ||
        compilation.StructTypes.Values.Any(structure => structure.Fields.Any(field => IsStringType(field.TypeName))) ||
        compilation.StaticFields.Any(field => IsStringArrayType(field.Declaration.TypeName)) ||
        compilation.VariableTypes.Values.Any(IsStringArrayType) || compilation.ManagedArrayAllocations.Values.Any(IsStringArrayType) ||
        compilation.ManagedArrayIndexAccesses.Values.Any(binding => binding.ElementType == "string" || IsStringArrayType(binding.ElementType)) ||
        compilation.Methods.Any(method => IsStringArrayType(method.Declaration.ReturnType) || method.Declaration.Parameters.Any(parameter => IsStringArrayType(parameter.TypeName)));
    private static bool IsStringArrayType(string type) => SemanticAnalyzer.TryParseManagedArrayType(type, out var elementType, out _) &&
        (elementType is "string" or "string?" || IsStringArrayType(elementType));
    private static bool IsStringType(string type) => type is "string" or "string?";
    private static bool IsNullableArrayType(string type) => type.EndsWith("?", StringComparison.Ordinal) && SemanticAnalyzer.TryParseManagedArrayType(type[..^1], out _, out _);
    private bool HasManagedArrayReferenceElements() => compilation.StaticFields.Any(field => IsArrayOfArrays(field.Declaration.TypeName)) ||
        compilation.VariableTypes.Values.Any(IsArrayOfArrays) ||
        compilation.ManagedArrayAllocations.Values.Any(IsArrayOfArrays) ||
        compilation.ManagedArrayIndexAccesses.Values.Any(binding => IsManagedArrayType(binding.ElementType)) ||
        compilation.Methods.Any(method => IsArrayOfArrays(method.Declaration.ReturnType) || method.Declaration.Parameters.Any(parameter => IsArrayOfArrays(parameter.TypeName))) ||
        GetManagedArrayTypes().Any(type => SemanticAnalyzer.TryParseManagedArrayType(type, out var elementType, out _) && AggregateTypeContainsManagedReferences(elementType));
    private bool HasManagedClassReferenceElements() => compilation.StaticFields.Any(field => IsArrayOfClasses(field.Declaration.TypeName)) ||
        compilation.VariableTypes.Values.Any(IsArrayOfClasses) ||
        compilation.ManagedArrayAllocations.Values.Any(IsArrayOfClasses) ||
        compilation.ManagedArrayIndexAccesses.Values.Any(binding => IsClassReferenceArrayElementType(binding.ElementType)) ||
        compilation.Methods.Any(method => IsArrayOfClasses(method.Declaration.ReturnType) || method.Declaration.Parameters.Any(parameter => IsArrayOfClasses(parameter.TypeName)));
    private static bool IsManagedArrayType(string type) => SemanticAnalyzer.TryParseManagedArrayType(type, out _, out _);
    private static bool IsArrayOfArrays(string type) => SemanticAnalyzer.TryParseManagedArrayType(type, out var elementType, out _) && IsManagedArrayType(elementType);
    private bool IsArrayOfClasses(string type)
    {
        if (!SemanticAnalyzer.TryParseManagedArrayType(type, out var elementType, out _)) return false;
        return IsClassReferenceArrayElementType(elementType);
    }
    private bool IsClassReferenceArrayElementType(string type)
    {
        var element = type.EndsWith("?", StringComparison.Ordinal) ? type[..^1] : type;
        return compilation.ClassTypes.ContainsKey(element) || compilation.InterfaceTypes.ContainsKey(element) || compilation.DelegateTypes.ContainsKey(element);
    }
    private bool HasManagedArraySupport() => compilation.StaticFields.Any(field => SemanticAnalyzer.TryParseManagedArrayType(field.Declaration.TypeName, out _, out _)) ||
        compilation.ManagedArrayAllocations.Count > 0 || compilation.ManagedArrayIndexAccesses.Count > 0 ||
        compilation.ManagedArrayLengthAccesses.Count > 0 || compilation.ManagedArrayPointerAccesses.Count > 0 || compilation.VariableTypes.Values.Any(type => SemanticAnalyzer.TryParseManagedArrayType(type, out _, out _)) ||
        compilation.ManagedArrayRankAccesses.Count > 0 || compilation.ManagedArrayGetLengthCalls.Count > 0 || compilation.ManagedArrayForInElementTypes.Count > 0 ||
        compilation.ClassTypes.Values.Any(classType => classType.Fields.Any(field => SemanticAnalyzer.TryParseManagedArrayType(field.TypeName, out _, out _))) ||
        compilation.StructTypes.Values.Any(structure => structure.Fields.Any(field => SemanticAnalyzer.TryParseManagedArrayType(field.TypeName, out _, out _))) ||
        compilation.Methods.Any(method => SemanticAnalyzer.TryParseManagedArrayType(method.Declaration.ReturnType, out _, out _) || method.Declaration.Parameters.Any(parameter => SemanticAnalyzer.TryParseManagedArrayType(parameter.TypeName, out _, out _)));
    private bool UsesFloatStringConversion() => compilation.StringifiedPrimitiveTypes.Contains("float") || compilation.StringifiedPrimitiveTypes.Contains("double");
    private void EmitMethod(MethodSymbol method)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var d = method.Declaration; var emitter = new FunctionEmitter(method, compilation.Methods, _typeInitializationFunctions, compilation.VariableTypes, compilation.ForInTypes, compilation.LoweredForInLoops, compilation.ScopeBoundVariables, compilation.CallTargets, compilation.EnumToStringCalls, compilation.ClassToStringCalls, compilation.StructToStringCalls, compilation.VirtualCallSlots, compilation.InterfaceCallSlots, compilation.DelegateMethodGroups, compilation.ClosureEnvironments, compilation.ForHeaderClosureEnvironments, compilation.ForInClosureEnvironments, compilation.BlockClosureEnvironments, compilation.CompositeClosureEnvironments, compilation.DelegateInvocations, compilation.DelegateTypes, compilation.CallArguments, compilation.ConstructorInitializers, compilation.OperatorTargets, compilation.UserConversionOperators, compilation.DefaultTypes, compilation.ConstantFields, compilation.UnqualifiedConstantFields, compilation.FieldAccesses, compilation.PropertyGetterCalls, compilation.PropertyGetterNameCalls, compilation.PropertySetterCalls, compilation.MultiAssignmentAccessorAssignments, compilation.IndexerGetterCalls, compilation.UnqualifiedStaticFieldAccesses, compilation.StructFieldAccesses, compilation.UnqualifiedStructFields, compilation.ClassFieldAccesses, compilation.UnqualifiedClassFields, compilation.StructConstructors, compilation.ClassAllocations, compilation.GenericConstructions, compilation.StructTypes, compilation.ClassTypes, compilation.InterfaceTypes, compilation.InterfaceConversions, GetManagedClassTypes(), compilation.EnumUnderlyingTypes, compilation.NativePointerSize, compilation.CastTypes, compilation.TypeOperatorTargets, compilation.ConditionalExpressionTypes, compilation.SwitchTypePatterns, compilation.SizeOfTypes, compilation.StringLiterals, compilation.StringLengthAccesses, compilation.StringIndexAccesses, compilation.ManagedArrayAllocations, compilation.ManagedArrayIndexAccesses, compilation.ManagedArrayLengthAccesses, compilation.ManagedArrayRankAccesses, compilation.ManagedArrayPointerAccesses, compilation.GenericSpanEmptyAccesses, compilation.ManagedArrayGetLengthCalls, compilation.ManagedArrayForInElementTypes, compilation.ExceptionCatchTypes, compilation.ThrowTypes, _exceptionCatchTableNames, compilation.UsesExceptionHandling, cancellationToken);
        var parameters = d.Parameters.Select((p, i) => $"{LlvmParameterType(p)} %arg{i}");
        var functionParameters = method.IsStructInstanceMethod || method.IsClassInstanceMethod || method.IsClassConstructor || method.IsClassFinalizer ? new[] { "ptr %this" }.Concat(parameters) : parameters;
        if (method.IsClassConstructor) functionParameters = functionParameters.Append("i1 %luma.complete.class");
        var linkage = IsRuntimeIntrinsicType(method.Owner) ? "internal " : "";
        _output.AppendLine($"define {linkage}{LlvmType(d.ReturnType)} @{FunctionName(method, compilation.Methods)}({string.Join(", ", functionParameters)}) {{");
        var body = emitter.Emit(d.Body!); _output.Append(body); _output.AppendLine("}");
    }
    private void EmitExternDeclaration(MethodSymbol method)
    {
        var declaration = method.Declaration;
        var returnAttribute = AbiAttribute(declaration.ReturnType, compilation.EnumUnderlyingTypes);
        var parameters = declaration.Parameters.Select(parameter =>
        {
            var llvmType = LlvmType(parameter.TypeName);
            var attribute = AbiAttribute(parameter.TypeName, compilation.EnumUnderlyingTypes);
            return attribute.Length == 0 ? llvmType : $"{llvmType} {attribute.Trim()}";
        });
        _output.AppendLine($"declare {returnAttribute}{LlvmType(declaration.ReturnType)} @{FunctionName(method, compilation.Methods)}({string.Join(", ", parameters)})");
    }
    private static string AbiAttribute(string type, IReadOnlyDictionary<string, string> enumTypes) => enumTypes.TryGetValue(type, out var underlying) ? AbiAttribute(underlying, enumTypes) : type switch
    {
        "bool" or "uint8" or "uint16" or "char" => "zeroext ",
        "int8" or "int16" => "signext ",
        _ => ""
    };
    private string LlvmParameterType(ParameterDeclaration parameter) => parameter.PassingMode is "ref" or "out" ? "ptr" : LlvmType(parameter.TypeName);
    private static string FunctionName(MethodSymbol method, IReadOnlyList<MethodSymbol> allMethods)
    {
        if (method.Declaration.IsExtern) return Safe(method.Declaration.Name);
        var qualifiedTypeName = string.IsNullOrEmpty(method.Owner.Namespace) ? method.Owner.Name : method.Owner.Namespace + "." + method.Owner.Name;
        var collisions = allMethods.Count(candidate => candidate.QualifiedName[..^(candidate.Declaration.Name.Length + 1)] == qualifiedTypeName && candidate.Declaration.Name == method.Declaration.Name && candidate.Declaration.Parameters.Count == method.Declaration.Parameters.Count);
        var overloadSignature = collisions > 1 ? "_" + string.Join("_", method.Declaration.Parameters.Select(p => Safe(p.TypeName))) : "";
        var explicitSuffix = method.Declaration.ExplicitInterfaceName is { } interfaceName ? "_explicit_" + Safe(interfaceName) : "";
        var methodName = method.IsStructConstructor || method.IsClassConstructor ? "ctor" : method.IsClassFinalizer ? "finalizer" : method.IsStructEqualityOperator ? "op_eq" : method.Declaration.IsConversionOperator ? "op_" + method.Declaration.Operator + "_to_" + Safe(method.Declaration.ReturnType) : method.IsStructInstanceMethod || method.IsClassInstanceMethod ? "inst_" + Safe(method.Declaration.Name) + explicitSuffix : Safe(method.Declaration.Name) + explicitSuffix;
        return $"luma_{Safe(qualifiedTypeName)}_{methodName}_{method.Declaration.Parameters.Count}{overloadSignature}";
    }

    private static bool IsRuntimeIntrinsicType(TypeDeclaration type) =>
        type.Location.File is "<builtin>" or "<LumaSharp runtime>";
    private static string StructDelegateThunkName(DelegateMethodGroupBinding binding, IReadOnlyList<MethodSymbol> allMethods) =>
        $"luma_struct_delegate_{Safe(binding.DelegateType)}_{FunctionName(binding.Method, allMethods)}";

    private void EmitStructDelegateThunks()
    {
        var bindings = compilation.DelegateMethodGroups.Values
            .Where(binding => binding.Method.IsStructInstanceMethod)
            .DistinctBy(binding => StructDelegateThunkName(binding, compilation.Methods), StringComparer.Ordinal)
            .OrderBy(binding => StructDelegateThunkName(binding, compilation.Methods), StringComparer.Ordinal);
        foreach (var binding in bindings)
        {
            var method = binding.Method;
            var structName = QualifiedTypeName(method.Owner);
            var arguments = method.Declaration.Parameters.Select((parameter, index) =>
                $"{LlvmParameterType(parameter)} %arg{index}").ToArray();
            var signatureArguments = string.Join(", ", new[] { "ptr %box" }.Concat(arguments));
            var callArguments = string.Join(", ", arguments.Select((argument, index) =>
                $"{LlvmParameterType(method.Declaration.Parameters[index])} %arg{index}"));
            var boxType = $"%luma.structbox.{Safe(structName)}";
            var resultType = LlvmType(method.Declaration.ReturnType);
            _output.AppendLine($"define internal {resultType} @{StructDelegateThunkName(binding, compilation.Methods)}({signatureArguments}) {{");
            _output.AppendLine("entry:");
            _output.AppendLine($"  %payload = getelementptr {boxType}, ptr %box, i32 0, i32 3");
            var allArguments = callArguments.Length == 0 ? "ptr %payload" : $"ptr %payload, {callArguments}";
            var call = $"call {resultType} @{FunctionName(method, compilation.Methods)}({allArguments})";
            if (method.Declaration.ReturnType == "void")
            {
                _output.AppendLine($"  {call}");
                _output.AppendLine("  ret void");
            }
            else
            {
                _output.AppendLine($"  %result = {call}");
                _output.AppendLine($"  ret {resultType} %result");
            }
            _output.AppendLine("}");
        }
    }
    private static string Safe(string value) => string.Concat(value.Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : '_'));
    private void EmitArrayMemoryDeclarations()
    {
        var sizeType = compilation.NativePointerSize == 8 ? "i64" : "i32";
        _output.AppendLine($"declare ptr @malloc({sizeType})");
        _output.AppendLine("declare void @free(ptr)");
    }
    private void EmitManagedArrayRuntime()
    {
        var sizeType = compilation.NativePointerSize == 8 ? "i64" : "i32";
        var objectSize = compilation.NativePointerSize == 8 ? 48 : 36;
        var hasStringSupport = HasStringSupport();
        var hasArrayReferences = HasManagedArrayReferenceElements();
        var hasClassReferences = HasManagedClassReferenceElements();
        var hasAggregateReferences = GetManagedArrayTypes().Any(type =>
            SemanticAnalyzer.TryParseManagedArrayType(type, out var elementType, out _) && AggregateTypeContainsManagedReferences(elementType));
        _output.AppendLine("define internal void @luma_array_retain(ptr %input) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %is.null = icmp eq ptr %input, null");
        _output.AppendLine("  br i1 %is.null, label %done, label %retain");
        _output.AppendLine("retain:");
        _output.AppendLine("  %count.address = getelementptr %luma.array, ptr %input, i32 0, i32 1");
        EmitCheckedReferenceCountIncrement("%count.address", "done", "array.retain");
        _output.AppendLine("done:");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");
        _output.AppendLine("define internal void @luma_array_release(ptr %input) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %is.null = icmp eq ptr %input, null");
        _output.AppendLine("  br i1 %is.null, label %done, label %decrement");
        _output.AppendLine("decrement:");
        _output.AppendLine("  %count.address = getelementptr %luma.array, ptr %input, i32 0, i32 1");
        _output.AppendLine("  %weak.address = getelementptr %luma.array, ptr %input, i32 0, i32 3");
        _output.AppendLine("  %weak.control = load atomic ptr, ptr %weak.address acquire, align " + compilation.NativePointerSize);
        _output.AppendLine("  %has.weak.control = icmp ne ptr %weak.control, null");
        _output.AppendLine("  br i1 %has.weak.control, label %weak.decrement, label %normal.decrement");
        _output.AppendLine("normal.decrement:");
        _output.AppendLine("  %old.count = atomicrmw sub ptr %count.address, i32 1 acq_rel");
        _output.AppendLine("  %last.reference = icmp eq i32 %old.count, 1");
        _output.AppendLine("  br i1 %last.reference, label %destroy, label %done");
        _output.AppendLine("weak.decrement:");
        _output.AppendLine("  call void @luma_weak_lock(ptr %weak.control)");
        _output.AppendLine("  %weak.old.count = atomicrmw sub ptr %count.address, i32 1 acq_rel");
        _output.AppendLine("  %weak.last.reference = icmp eq i32 %weak.old.count, 1");
        _output.AppendLine("  br i1 %weak.last.reference, label %weak.destroy, label %weak.not.last");
        _output.AppendLine("weak.not.last:");
        _output.AppendLine("  call void @luma_weak_unlock(ptr %weak.control)");
        _output.AppendLine("  br label %done");
        _output.AppendLine("weak.destroy:");
        _output.AppendLine("  %weak.target.address = getelementptr %luma.weak.control, ptr %weak.control, i32 0, i32 0");
        _output.AppendLine("  store atomic ptr null, ptr %weak.target.address release, align " + compilation.NativePointerSize);
        _output.AppendLine("  %weak.count.address = getelementptr %luma.weak.control, ptr %weak.control, i32 0, i32 2");
        _output.AppendLine("  %weak.handle.count = load atomic i32, ptr %weak.count.address acquire, align 4");
        _output.AppendLine("  %weak.has.handles = icmp ne i32 %weak.handle.count, 0");
        _output.AppendLine("  call void @luma_weak_unlock(ptr %weak.control)");
        _output.AppendLine("  br i1 %weak.has.handles, label %destroy, label %weak.free");
        _output.AppendLine("weak.free:");
        _output.AppendLine("  call void @free(ptr %weak.control)");
        _output.AppendLine("  br label %destroy");
        _output.AppendLine("destroy:");
        if (hasStringSupport || hasArrayReferences || hasClassReferences)
        {
            _output.AppendLine("  %flags.address = getelementptr %luma.array, ptr %input, i32 0, i32 2");
            _output.AppendLine("  %flags = load i32, ptr %flags.address");
            _output.AppendLine("  %has.reference.elements = icmp ne i32 %flags, 0");
            _output.AppendLine("  br i1 %has.reference.elements, label %release.elements.start, label %free.object");
            _output.AppendLine("release.elements.start:");
            _output.AppendLine("  %length.address = getelementptr %luma.array, ptr %input, i32 0, i32 4");
            _output.AppendLine("  %length = load i32, ptr %length.address");
            _output.AppendLine("  %first.index = sub i32 %length, 1");
            _output.AppendLine("  %data = getelementptr %luma.array, ptr %input, i32 0, i32 7");
            _output.AppendLine("  br label %release.elements.loop");
            _output.AppendLine("release.elements.loop:");
            _output.AppendLine("  %index = phi i32 [ %first.index, %release.elements.start ], [ %previous.index, %release.elements.next ]");
            _output.AppendLine("  %has.items = icmp sge i32 %index, 0");
            _output.AppendLine("  br i1 %has.items, label %release.element, label %free.object");
            _output.AppendLine("release.element:");
            if (hasAggregateReferences)
            {
                _output.AppendLine("  %aggregate.kind.matches = icmp eq i32 %flags, 8");
                _output.AppendLine("  br i1 %aggregate.kind.matches, label %release.aggregate.element, label %release.reference.element");
                _output.AppendLine("release.aggregate.element:");
                _output.AppendLine("  %aggregate.descriptor.address = getelementptr %luma.array, ptr %input, i32 0, i32 0");
                _output.AppendLine("  %aggregate.descriptor = load ptr, ptr %aggregate.descriptor.address");
                _output.AppendLine("  %aggregate.size.pointer.address = getelementptr %luma.typedesc, ptr %aggregate.descriptor, i32 0, i32 6");
                _output.AppendLine("  %aggregate.size.pointer = load ptr, ptr %aggregate.size.pointer.address");
                _output.AppendLine("  %aggregate.element.size = load i32, ptr %aggregate.size.pointer");
                _output.AppendLine("  %aggregate.index.native = zext i32 %index to " + sizeType);
                _output.AppendLine("  %aggregate.size.native = zext i32 %aggregate.element.size to " + sizeType);
                _output.AppendLine($"  %aggregate.offset = mul {sizeType} %aggregate.index.native, %aggregate.size.native");
                _output.AppendLine($"  %aggregate.data.address = getelementptr i8, ptr %data, {sizeType} %aggregate.offset");
                _output.AppendLine("  %aggregate.release.address = getelementptr %luma.typedesc, ptr %aggregate.descriptor, i32 0, i32 7");
                _output.AppendLine("  %aggregate.release = load ptr, ptr %aggregate.release.address");
                _output.AppendLine("  call void %aggregate.release(ptr %aggregate.data.address)");
                _output.AppendLine("  br label %release.elements.next");
                _output.AppendLine("release.reference.element:");
            }
            _output.AppendLine("  %element.address = getelementptr ptr, ptr %data, i32 %index");
            _output.AppendLine("  %element = load ptr, ptr %element.address");
            var releaseKinds = new List<(int Flag, string Label, string Call)>();
            if (hasStringSupport) releaseKinds.Add((1, "release.string.element", "call void @luma_string_release(ptr %element)"));
            if (hasArrayReferences) releaseKinds.Add((2, "release.array.element", "call void @luma_array_release(ptr %element)"));
            if (hasClassReferences) releaseKinds.Add((4, "release.class.element", "call void @luma_class_release_dynamic(ptr %element)"));
            if (releaseKinds.Count == 0)
            {
                _output.AppendLine("  br label %release.elements.next");
            }
            else if (releaseKinds.Count == 1)
            {
                _output.AppendLine("  " + releaseKinds[0].Call);
                _output.AppendLine("  br label %release.elements.next");
            }
            else
            {
                for (var kindIndex = 0; kindIndex < releaseKinds.Count - 1; kindIndex++)
                {
                    var kind = releaseKinds[kindIndex];
                    var next = $"release.kind.next.{kindIndex}";
                    _output.AppendLine($"  %kind.matches.{kindIndex} = icmp eq i32 %flags, {kind.Flag}");
                    _output.AppendLine($"  br i1 %kind.matches.{kindIndex}, label %{kind.Label}, label %{next}");
                    _output.AppendLine(kind.Label + ":");
                    _output.AppendLine("  " + kind.Call);
                    _output.AppendLine("  br label %release.elements.next");
                    _output.AppendLine(next + ":");
                }
                _output.AppendLine("  " + releaseKinds[^1].Call);
                _output.AppendLine("  br label %release.elements.next");
            }
            _output.AppendLine("release.elements.next:");
            _output.AppendLine("  %previous.index = sub i32 %index, 1");
            _output.AppendLine("  br label %release.elements.loop");
            _output.AppendLine("free.object:");
        }
        _output.AppendLine("  call void @free(ptr %input)");
        _output.AppendLine("  br label %done");
        _output.AppendLine("done:");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");
        _output.AppendLine($"define internal {{ ptr, i32 }} @luma_array_new(i32 %length, i32 %rank, i32 %dim0, i32 %dim1, i32 %dim2, i32 %element.size, i1 %string.elements, i1 %empty.string.elements, i1 %array.elements, i1 %class.elements, i1 %aggregate.elements) {{");
        _output.AppendLine("entry:");
        _output.AppendLine("  %negative = icmp slt i32 %length, 0");
        _output.AppendLine("  br i1 %negative, label %overflow, label %size");
        _output.AppendLine("size:");
        if (sizeType == "i64")
        {
            _output.AppendLine("  %length.ext = zext i32 %length to i64");
            _output.AppendLine("  %element.size.ext = zext i32 %element.size to i64");
        }
        else
        {
            _output.AppendLine("  %length.ext = add i32 0, %length");
            _output.AppendLine("  %element.size.ext = add i32 0, %element.size");
        }
        _output.AppendLine($"  %payload.pair = call {{{sizeType}, i1}} @llvm.umul.with.overflow.{sizeType}({sizeType} %length.ext, {sizeType} %element.size.ext)");
        _output.AppendLine($"  %payload = extractvalue {{{sizeType}, i1}} %payload.pair, 0");
        _output.AppendLine($"  %payload.overflow = extractvalue {{{sizeType}, i1}} %payload.pair, 1");
        _output.AppendLine("  br i1 %payload.overflow, label %overflow, label %total.size");
        _output.AppendLine("total.size:");
        _output.AppendLine($"  %total.pair = call {{{sizeType}, i1}} @llvm.uadd.with.overflow.{sizeType}({sizeType} %payload, {sizeType} {objectSize})");
        _output.AppendLine($"  %total = extractvalue {{{sizeType}, i1}} %total.pair, 0");
        _output.AppendLine($"  %total.overflow = extractvalue {{{sizeType}, i1}} %total.pair, 1");
        _output.AppendLine("  br i1 %total.overflow, label %overflow, label %allocate");
        _output.AppendLine("allocate:");
        _output.AppendLine($"  %object = call ptr @malloc({sizeType} %total)");
        _output.AppendLine("  %allocation.failed = icmp eq ptr %object, null");
        _output.AppendLine("  br i1 %allocation.failed, label %out.of.memory, label %initialize");
        _output.AppendLine("initialize:");
        _output.AppendLine("  %type.address = getelementptr %luma.array, ptr %object, i32 0, i32 0");
        _output.AppendLine("  store ptr null, ptr %type.address");
        _output.AppendLine("  %count.address = getelementptr %luma.array, ptr %object, i32 0, i32 1");
        _output.AppendLine("  store i32 1, ptr %count.address");
        _output.AppendLine("  %flags.address = getelementptr %luma.array, ptr %object, i32 0, i32 2");
        _output.AppendLine("  %weak.address = getelementptr %luma.array, ptr %object, i32 0, i32 3");
        _output.AppendLine("  store ptr null, ptr %weak.address");
        _output.AppendLine("  %string.flag = zext i1 %string.elements to i32");
        _output.AppendLine("  %array.flag = zext i1 %array.elements to i32");
        _output.AppendLine("  %array.kind = shl i32 %array.flag, 1");
        _output.AppendLine("  %class.flag = zext i1 %class.elements to i32");
        _output.AppendLine("  %class.kind = shl i32 %class.flag, 2");
        _output.AppendLine("  %aggregate.flag = zext i1 %aggregate.elements to i32");
        _output.AppendLine("  %aggregate.kind = shl i32 %aggregate.flag, 3");
        _output.AppendLine("  %reference.flags = or i32 %string.flag, %array.kind");
        _output.AppendLine("  %flags = or i32 %reference.flags, %class.kind");
        _output.AppendLine("  %all.flags = or i32 %flags, %aggregate.kind");
        _output.AppendLine("  store i32 %all.flags, ptr %flags.address");
        _output.AppendLine("  %length.address = getelementptr %luma.array, ptr %object, i32 0, i32 4");
        _output.AppendLine("  store i32 %length, ptr %length.address");
        _output.AppendLine("  %rank.address = getelementptr %luma.array, ptr %object, i32 0, i32 5");
        _output.AppendLine("  store i32 %rank, ptr %rank.address");
        _output.AppendLine("  %dims = getelementptr %luma.array, ptr %object, i32 0, i32 6, i32 0");
        _output.AppendLine("  %dim0.address = getelementptr i32, ptr %dims, i32 0");
        _output.AppendLine("  store i32 %dim0, ptr %dim0.address");
        _output.AppendLine("  %dim1.address = getelementptr i32, ptr %dims, i32 1");
        _output.AppendLine("  store i32 %dim1, ptr %dim1.address");
        _output.AppendLine("  %dim2.address = getelementptr i32, ptr %dims, i32 2");
        _output.AppendLine("  store i32 %dim2, ptr %dim2.address");
        _output.AppendLine("  %data = getelementptr %luma.array, ptr %object, i32 0, i32 7");
        _output.AppendLine("  br label %zero.loop");
        _output.AppendLine($"zero.loop:");
        _output.AppendLine($"  %offset = phi {sizeType} [ 0, %initialize ], [ %next.offset, %zero.body ]");
        _output.AppendLine($"  %zero.done = icmp uge {sizeType} %offset, %payload");
        _output.AppendLine("  br i1 %zero.done, label %after.zero, label %zero.body");
        _output.AppendLine("zero.body:");
        _output.AppendLine($"  %zero.address = getelementptr i8, ptr %data, {sizeType} %offset");
        _output.AppendLine("  store i8 0, ptr %zero.address");
        _output.AppendLine($"  %next.offset = add {sizeType} %offset, 1");
        _output.AppendLine("  br label %zero.loop");
        _output.AppendLine("after.zero:");
        if (hasStringSupport)
        {
            _output.AppendLine("  br i1 %empty.string.elements, label %fill.strings.loop, label %ready");
            _output.AppendLine("fill.strings.loop:");
            _output.AppendLine("  %string.index = phi i32 [ 0, %after.zero ], [ %next.string.index, %fill.string.item ]");
            _output.AppendLine("  %string.done = icmp uge i32 %string.index, %length");
            _output.AppendLine("  br i1 %string.done, label %ready, label %fill.string.item");
            _output.AppendLine("fill.string.item:");
            _output.AppendLine("  %string.element.address = getelementptr ptr, ptr %data, i32 %string.index");
            _output.AppendLine("  store ptr @luma_string_empty, ptr %string.element.address");
            _output.AppendLine("  %next.string.index = add i32 %string.index, 1");
            _output.AppendLine("  br label %fill.strings.loop");
        }
        else _output.AppendLine("  br label %ready");
        _output.AppendLine("ready:");
        _output.AppendLine("  %ready.with.object = insertvalue { ptr, i32 } undef, ptr %object, 0");
        _output.AppendLine("  %ready.result = insertvalue { ptr, i32 } %ready.with.object, i32 0, 1");
        _output.AppendLine("  ret { ptr, i32 } %ready.result");
        _output.AppendLine("overflow:");
        _output.AppendLine("  %overflow.with.object = insertvalue { ptr, i32 } undef, ptr null, 0");
        _output.AppendLine("  %overflow.result = insertvalue { ptr, i32 } %overflow.with.object, i32 1, 1");
        _output.AppendLine("  ret { ptr, i32 } %overflow.result");
        _output.AppendLine("out.of.memory:");
        _output.AppendLine("  %memory.with.object = insertvalue { ptr, i32 } undef, ptr null, 0");
        _output.AppendLine("  %memory.result = insertvalue { ptr, i32 } %memory.with.object, i32 2, 1");
        _output.AppendLine("  ret { ptr, i32 } %memory.result");
        _output.AppendLine("}");
    }
    private void EmitStringLiterals()
    {
        var needsFloatingStringification = UsesFloatStringConversion() ||
            HasStringSupport() && compilation.ClassToStringCalls.Count > 0;
        _output.AppendLine("%luma.string = type { ptr, i32, i32, ptr, ptr, i32, i32, i8, i8, ptr }");
        _output.AppendLine("@luma_string_type_name = private unnamed_addr constant [7 x i8] c\"\\73\\74\\72\\69\\6E\\67\\00\"");
        _output.AppendLine("@luma_string_empty_utf8 = private unnamed_addr constant [1 x i8] c\"\\00\"");
        _output.AppendLine("@luma_string_empty = private unnamed_addr constant %luma.string { ptr @luma_string_type, i32 1, i32 1, ptr null, ptr getelementptr inbounds ([1 x i8], ptr @luma_string_empty_utf8, i64 0, i64 0), i32 0, i32 0, i8 0, i8 2, ptr null }");
        _output.AppendLine("@luma_string_true_utf8 = private unnamed_addr constant [5 x i8] c\"True\\00\"");
        _output.AppendLine("@luma_string_true_utf16 = private unnamed_addr constant [4 x i16] [i16 84, i16 114, i16 117, i16 101]");
        _output.AppendLine("@luma_string_true = private unnamed_addr constant %luma.string { ptr @luma_string_type, i32 1, i32 1, ptr null, ptr getelementptr inbounds ([5 x i8], ptr @luma_string_true_utf8, i64 0, i64 0), i32 4, i32 4, i8 0, i8 2, ptr getelementptr inbounds ([4 x i16], ptr @luma_string_true_utf16, i64 0, i64 0) }");
        _output.AppendLine("@luma_string_false_utf8 = private unnamed_addr constant [6 x i8] c\"False\\00\"");
        _output.AppendLine("@luma_string_false_utf16 = private unnamed_addr constant [5 x i16] [i16 70, i16 97, i16 108, i16 115, i16 101]");
        _output.AppendLine("@luma_string_false = private unnamed_addr constant %luma.string { ptr @luma_string_type, i32 1, i32 1, ptr null, ptr getelementptr inbounds ([6 x i8], ptr @luma_string_false_utf8, i64 0, i64 0), i32 5, i32 5, i8 0, i8 2, ptr getelementptr inbounds ([5 x i16], ptr @luma_string_false_utf16, i64 0, i64 0) }");
        if (needsFloatingStringification)
        {
            _output.AppendLine("@luma_format_float = private unnamed_addr constant [5 x i8] c\"%.9g\\00\"");
            _output.AppendLine("@luma_format_double = private unnamed_addr constant [6 x i8] c\"%.17g\\00\"");
            _output.AppendLine("@luma_string_nan_utf8 = private unnamed_addr constant [4 x i8] c\"NaN\\00\"");
            _output.AppendLine("@luma_string_nan_utf16 = private unnamed_addr constant [3 x i16] [i16 78, i16 97, i16 78]");
            _output.AppendLine("@luma_string_nan = private unnamed_addr constant %luma.string { ptr @luma_string_type, i32 1, i32 1, ptr null, ptr getelementptr inbounds ([4 x i8], ptr @luma_string_nan_utf8, i64 0, i64 0), i32 3, i32 3, i8 0, i8 2, ptr getelementptr inbounds ([3 x i16], ptr @luma_string_nan_utf16, i64 0, i64 0) }");
            _output.AppendLine("@luma_string_positive_infinity_utf8 = private unnamed_addr constant [9 x i8] c\"Infinity\\00\"");
            _output.AppendLine("@luma_string_positive_infinity_utf16 = private unnamed_addr constant [8 x i16] [i16 73, i16 110, i16 102, i16 105, i16 110, i16 105, i16 116, i16 121]");
            _output.AppendLine("@luma_string_positive_infinity = private unnamed_addr constant %luma.string { ptr @luma_string_type, i32 1, i32 1, ptr null, ptr getelementptr inbounds ([9 x i8], ptr @luma_string_positive_infinity_utf8, i64 0, i64 0), i32 8, i32 8, i8 0, i8 2, ptr getelementptr inbounds ([8 x i16], ptr @luma_string_positive_infinity_utf16, i64 0, i64 0) }");
            _output.AppendLine("@luma_string_negative_infinity_utf8 = private unnamed_addr constant [10 x i8] c\"-Infinity\\00\"");
            _output.AppendLine("@luma_string_negative_infinity_utf16 = private unnamed_addr constant [9 x i16] [i16 45, i16 73, i16 110, i16 102, i16 105, i16 110, i16 105, i16 116, i16 121]");
            _output.AppendLine("@luma_string_negative_infinity = private unnamed_addr constant %luma.string { ptr @luma_string_type, i32 1, i32 1, ptr null, ptr getelementptr inbounds ([10 x i8], ptr @luma_string_negative_infinity_utf8, i64 0, i64 0), i32 9, i32 9, i8 0, i8 2, ptr getelementptr inbounds ([9 x i16], ptr @luma_string_negative_infinity_utf16, i64 0, i64 0) }");
        }
        var literalsBySymbol = compilation.StringLiteralPool.Values.ToDictionary(value => value.Symbol, StringComparer.Ordinal);
        foreach (var className in compilation.ClassTypes.Keys)
            if (StringLiteralCodec.TryCreate(className, out var classNameLiteral, out _))
                literalsBySymbol.TryAdd(classNameLiteral.Symbol, classNameLiteral);
        foreach (var arrayType in GetManagedArrayTypes())
            if (StringLiteralCodec.TryCreate(arrayType, out var arrayTypeLiteral, out _))
                literalsBySymbol.TryAdd(arrayTypeLiteral.Symbol, arrayTypeLiteral);
        foreach (var boxType in BoxableTypeNames())
            if (StringLiteralCodec.TryCreate(boxType, out var boxNameLiteral, out _))
                literalsBySymbol.TryAdd(boxNameLiteral.Symbol, boxNameLiteral);
        if (compilation.UsesExceptionHandling)
            foreach (var typeName in _typeInitializationFunctions.Keys)
                foreach (var text in new[] { TypeInitializationFailureMessage(typeName), TypeInitializationCircularMessage(typeName) })
                    if (StringLiteralCodec.TryCreate(text, out var messageLiteral, out _))
                        literalsBySymbol.TryAdd(messageLiteral.Symbol, messageLiteral);
        foreach (var literal in literalsBySymbol.Values.Where(value => value.Text.Length != 0).OrderBy(value => value.Symbol, StringComparer.Ordinal))
        {
            var utf8Name = "@" + literal.Symbol + "_utf8";
            var utf16Name = "@" + literal.Symbol + "_utf16";
            var objectName = "@" + literal.Symbol;
            var utf8Length = literal.Utf8Bytes.Length + 1;
            var literalFlags = literal.Utf16CodeUnits.All(value => value < 0x80) ? 2 : 0;
            var utf8Contents = string.Concat(literal.Utf8Bytes.Select(value => "\\" + value.ToString("X2", System.Globalization.CultureInfo.InvariantCulture))) + "\\00";
            _output.AppendLine($"{utf8Name} = private unnamed_addr constant [{utf8Length} x i8] c\"{utf8Contents}\"");
            var utf16Values = string.Join(", ", literal.Utf16CodeUnits.Select(value => "i16 " + value.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            _output.AppendLine($"{utf16Name} = private unnamed_addr constant [{literal.Length} x i16] [{utf16Values}]");
            _output.AppendLine($"{objectName} = private unnamed_addr constant %luma.string {{ ptr @luma_string_type, i32 1, i32 1, ptr null, ptr getelementptr inbounds ([{utf8Length} x i8], ptr {utf8Name}, i64 0, i64 0), i32 {literal.Length}, i32 {literal.Utf8Bytes.Length}, i8 0, i8 {literalFlags}, ptr getelementptr inbounds ([{literal.Length} x i16], ptr {utf16Name}, i64 0, i64 0) }}");
        }
    }
    private void EmitStringRuntime()
    {
        var sizeType = compilation.NativePointerSize == 8 ? "i64" : "i32";
        var stringSize = compilation.NativePointerSize == 8 ? 56 : 36;
        var needsBoxedPrimitiveStringification = HasStringSupport() && compilation.ClassToStringCalls.Count > 0;
        var needsPrimitiveStringification = compilation.StringifiedPrimitiveTypes.Count > 0 || EntryPointHasStringArguments() || needsBoxedPrimitiveStringification;
        var needsFloatingStringification = UsesFloatStringConversion() || needsBoxedPrimitiveStringification;
        string SizeCast(string result, string operand) => sizeType == "i64" ? $"  %{result} = zext i32 %{operand} to i64" : $"  %{result} = add i32 %{operand}, 0";
        _output.AppendLine($"declare ptr @malloc({sizeType})");
        _output.AppendLine("declare void @free(ptr)");
        _output.AppendLine($"declare ptr @memcpy(ptr, ptr, {sizeType})");
        if (UsesWindowsCommandLineApi()) EmitWindowsWideStringRuntime(sizeType);
        _output.AppendLine("define internal void @luma_string_retain(ptr %input) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %is.null = icmp eq ptr %input, null");
        _output.AppendLine("  br i1 %is.null, label %done, label %check.immortal");
        _output.AppendLine("check.immortal:");
        _output.AppendLine("  %header.flags.address = getelementptr %luma.string, ptr %input, i32 0, i32 2");
        _output.AppendLine("  %header.flags = load i32, ptr %header.flags.address");
        _output.AppendLine("  %immortal.bit = and i32 %header.flags, 1");
        _output.AppendLine("  %immortal = icmp ne i32 %immortal.bit, 0");
        _output.AppendLine("  br i1 %immortal, label %done, label %read.count");
        _output.AppendLine("read.count:");
        _output.AppendLine("  %count.address = getelementptr %luma.string, ptr %input, i32 0, i32 1");
        EmitCheckedReferenceCountIncrement("%count.address", "done", "string.retain");
        _output.AppendLine("done:");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");

        _output.AppendLine("define internal void @luma_string_release(ptr %input) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %is.null = icmp eq ptr %input, null");
        _output.AppendLine("  br i1 %is.null, label %done, label %check.immortal");
        _output.AppendLine("check.immortal:");
        _output.AppendLine("  %header.flags.address = getelementptr %luma.string, ptr %input, i32 0, i32 2");
        _output.AppendLine("  %header.flags = load i32, ptr %header.flags.address");
        _output.AppendLine("  %immortal.bit = and i32 %header.flags, 1");
        _output.AppendLine("  %immortal = icmp ne i32 %immortal.bit, 0");
        _output.AppendLine("  br i1 %immortal, label %done, label %read.count");
        _output.AppendLine("read.count:");
        _output.AppendLine("  %count.address = getelementptr %luma.string, ptr %input, i32 0, i32 1");
        _output.AppendLine("  %weak.address = getelementptr %luma.string, ptr %input, i32 0, i32 3");
        _output.AppendLine("  %weak.control = load atomic ptr, ptr %weak.address acquire, align " + compilation.NativePointerSize);
        _output.AppendLine("  %has.weak.control = icmp ne ptr %weak.control, null");
        _output.AppendLine("  br i1 %has.weak.control, label %weak.decrement, label %decrement");
        _output.AppendLine("decrement:");
        _output.AppendLine("  %old.count = atomicrmw sub ptr %count.address, i32 1 release");
        _output.AppendLine("  %last.reference = icmp eq i32 %old.count, 1");
        _output.AppendLine("  br i1 %last.reference, label %destroy, label %done");
        _output.AppendLine("weak.decrement:");
        _output.AppendLine("  call void @luma_weak_lock(ptr %weak.control)");
        _output.AppendLine("  %weak.old.count = atomicrmw sub ptr %count.address, i32 1 acq_rel");
        _output.AppendLine("  %weak.last.reference = icmp eq i32 %weak.old.count, 1");
        _output.AppendLine("  br i1 %weak.last.reference, label %weak.destroy, label %weak.not.last");
        _output.AppendLine("weak.not.last:");
        _output.AppendLine("  call void @luma_weak_unlock(ptr %weak.control)");
        _output.AppendLine("  br label %done");
        _output.AppendLine("weak.destroy:");
        _output.AppendLine("  %weak.target.address = getelementptr %luma.weak.control, ptr %weak.control, i32 0, i32 0");
        _output.AppendLine("  store atomic ptr null, ptr %weak.target.address release, align " + compilation.NativePointerSize);
        _output.AppendLine("  %weak.count.address = getelementptr %luma.weak.control, ptr %weak.control, i32 0, i32 2");
        _output.AppendLine("  %weak.handle.count = load atomic i32, ptr %weak.count.address acquire, align 4");
        _output.AppendLine("  %weak.has.handles = icmp ne i32 %weak.handle.count, 0");
        _output.AppendLine("  call void @luma_weak_unlock(ptr %weak.control)");
        _output.AppendLine("  br i1 %weak.has.handles, label %destroy, label %weak.free");
        _output.AppendLine("weak.free:");
        _output.AppendLine("  call void @free(ptr %weak.control)");
        _output.AppendLine("  br label %destroy");
        _output.AppendLine("destroy:");
        _output.AppendLine("  fence acquire");
        _output.AppendLine("  %data.address = getelementptr %luma.string, ptr %input, i32 0, i32 4");
        _output.AppendLine("  %data = load ptr, ptr %data.address");
        _output.AppendLine("  %units.address = getelementptr %luma.string, ptr %input, i32 0, i32 9");
        _output.AppendLine("  %units = load ptr, ptr %units.address");
        _output.AppendLine("  call void @free(ptr %data)");
        _output.AppendLine("  call void @free(ptr %units)");
        _output.AppendLine("  call void @free(ptr %input)");
        _output.AppendLine("  ret void");
        _output.AppendLine("done:");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");

        _output.AppendLine("define internal ptr @luma_string_concat(ptr %left.input, ptr %right.input) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %left.null = icmp eq ptr %left.input, null");
        _output.AppendLine("  %left = select i1 %left.null, ptr @luma_string_empty, ptr %left.input");
        _output.AppendLine("  %right.null = icmp eq ptr %right.input, null");
        _output.AppendLine("  %right = select i1 %right.null, ptr @luma_string_empty, ptr %right.input");
        _output.AppendLine("  %left.length.address = getelementptr %luma.string, ptr %left, i32 0, i32 5");
        _output.AppendLine("  %left.length = load i32, ptr %left.length.address");
        _output.AppendLine("  %right.length.address = getelementptr %luma.string, ptr %right, i32 0, i32 5");
        _output.AppendLine("  %right.length = load i32, ptr %right.length.address");
        _output.AppendLine("  %length.pair = call { i32, i1 } @llvm.uadd.with.overflow.i32(i32 %left.length, i32 %right.length)");
        _output.AppendLine("  %length = extractvalue { i32, i1 } %length.pair, 0");
        _output.AppendLine("  %length.overflow = extractvalue { i32, i1 } %length.pair, 1");
        _output.AppendLine("  br i1 %length.overflow, label %overflow, label %check.empty");
        _output.AppendLine("overflow:");
        EmitStringRuntimeOverflow();
        _output.AppendLine("check.empty:");
        _output.AppendLine("  %is.empty = icmp eq i32 %length, 0");
        _output.AppendLine("  br i1 %is.empty, label %empty, label %allocate.object");
        _output.AppendLine("empty:");
        _output.AppendLine("  ret ptr @luma_string_empty");
        _output.AppendLine("allocate.object:");
        _output.AppendLine($"  %object = call ptr @malloc({sizeType} {stringSize})");
        _output.AppendLine("  %object.failed = icmp eq ptr %object, null");
        _output.AppendLine("  br i1 %object.failed, label %out.of.memory, label %read.byte.lengths");
        _output.AppendLine("read.byte.lengths:");
        _output.AppendLine("  %left.bytes.address = getelementptr %luma.string, ptr %left, i32 0, i32 6");
        _output.AppendLine("  %left.bytes = load i32, ptr %left.bytes.address");
        _output.AppendLine("  %right.bytes.address = getelementptr %luma.string, ptr %right, i32 0, i32 6");
        _output.AppendLine("  %right.bytes = load i32, ptr %right.bytes.address");
        _output.AppendLine("  %bytes.pair = call { i32, i1 } @llvm.uadd.with.overflow.i32(i32 %left.bytes, i32 %right.bytes)");
        _output.AppendLine("  %bytes = extractvalue { i32, i1 } %bytes.pair, 0");
        _output.AppendLine("  %bytes.overflow = extractvalue { i32, i1 } %bytes.pair, 1");
        _output.AppendLine("  br i1 %bytes.overflow, label %free.object.overflow, label %allocate.data");
        _output.AppendLine("free.object.overflow:");
        _output.AppendLine("  call void @free(ptr %object)");
        EmitStringRuntimeOverflow();
        _output.AppendLine("allocate.data:");
        _output.AppendLine(SizeCast("byte.count", "bytes"));
        _output.AppendLine($"  %byte.allocation = add {sizeType} %byte.count, 1");
        _output.AppendLine($"  %data = call ptr @malloc({sizeType} %byte.allocation)");
        _output.AppendLine("  %data.failed = icmp eq ptr %data, null");
        _output.AppendLine("  br i1 %data.failed, label %free.object.oom, label %allocate.units");
        _output.AppendLine("free.object.oom:");
        _output.AppendLine("  call void @free(ptr %object)");
        EmitStringRuntimeOutOfMemory();
        _output.AppendLine("allocate.units:");
        _output.AppendLine(SizeCast("unit.count", "length"));
        _output.AppendLine($"  %unit.allocation = mul {sizeType} %unit.count, 2");
        _output.AppendLine($"  %units = call ptr @malloc({sizeType} %unit.allocation)");
        _output.AppendLine("  %units.failed = icmp eq ptr %units, null");
        _output.AppendLine("  br i1 %units.failed, label %free.data.oom, label %copy.bytes");
        _output.AppendLine("free.data.oom:");
        _output.AppendLine("  call void @free(ptr %data)");
        _output.AppendLine("  call void @free(ptr %object)");
        EmitStringRuntimeOutOfMemory();
        _output.AppendLine("copy.bytes:");
        _output.AppendLine("  %left.data.address = getelementptr %luma.string, ptr %left, i32 0, i32 4");
        _output.AppendLine("  %left.data = load ptr, ptr %left.data.address");
        _output.AppendLine("  %right.data.address = getelementptr %luma.string, ptr %right, i32 0, i32 4");
        _output.AppendLine("  %right.data = load ptr, ptr %right.data.address");
        _output.AppendLine("  %left.bytes.nonzero = icmp ne i32 %left.bytes, 0");
        _output.AppendLine("  br i1 %left.bytes.nonzero, label %copy.left.bytes, label %copy.right.bytes.check");
        _output.AppendLine("copy.left.bytes:");
        _output.AppendLine(SizeCast("left.byte.count", "left.bytes"));
        _output.AppendLine($"  %left.bytes.copy = call ptr @memcpy(ptr %data, ptr %left.data, {sizeType} %left.byte.count)");
        _output.AppendLine("  br label %copy.right.bytes.check");
        _output.AppendLine("copy.right.bytes.check:");
        _output.AppendLine("  %right.bytes.nonzero = icmp ne i32 %right.bytes, 0");
        _output.AppendLine("  br i1 %right.bytes.nonzero, label %copy.right.bytes, label %copy.units");
        _output.AppendLine("copy.right.bytes:");
        _output.AppendLine(SizeCast("left.byte.offset", "left.bytes"));
        _output.AppendLine($"  %right.byte.destination = getelementptr i8, ptr %data, {sizeType} %left.byte.offset");
        _output.AppendLine(SizeCast("right.byte.count", "right.bytes"));
        _output.AppendLine($"  %right.bytes.copy = call ptr @memcpy(ptr %right.byte.destination, ptr %right.data, {sizeType} %right.byte.count)");
        _output.AppendLine("  br label %copy.units");
        _output.AppendLine("copy.units:");
        _output.AppendLine("  %left.units.address = getelementptr %luma.string, ptr %left, i32 0, i32 9");
        _output.AppendLine("  %left.units = load ptr, ptr %left.units.address");
        _output.AppendLine("  %right.units.address = getelementptr %luma.string, ptr %right, i32 0, i32 9");
        _output.AppendLine("  %right.units = load ptr, ptr %right.units.address");
        _output.AppendLine("  %left.units.nonzero = icmp ne i32 %left.length, 0");
        _output.AppendLine("  br i1 %left.units.nonzero, label %copy.left.units, label %copy.right.units.check");
        _output.AppendLine("copy.left.units:");
        _output.AppendLine(SizeCast("left.unit.count", "left.length"));
        _output.AppendLine($"  %left.unit.bytes = mul {sizeType} %left.unit.count, 2");
        _output.AppendLine($"  %left.units.copy = call ptr @memcpy(ptr %units, ptr %left.units, {sizeType} %left.unit.bytes)");
        _output.AppendLine("  br label %copy.right.units.check");
        _output.AppendLine("copy.right.units.check:");
        _output.AppendLine("  %right.units.nonzero = icmp ne i32 %right.length, 0");
        _output.AppendLine("  br i1 %right.units.nonzero, label %copy.right.units, label %finish");
        _output.AppendLine("copy.right.units:");
        _output.AppendLine(SizeCast("left.unit.offset", "left.length"));
        _output.AppendLine($"  %right.unit.destination = getelementptr i16, ptr %units, {sizeType} %left.unit.offset");
        _output.AppendLine(SizeCast("right.unit.count", "right.length"));
        _output.AppendLine($"  %right.unit.bytes = mul {sizeType} %right.unit.count, 2");
        _output.AppendLine($"  %right.units.copy = call ptr @memcpy(ptr %right.unit.destination, ptr %right.units, {sizeType} %right.unit.bytes)");
        _output.AppendLine("  br label %finish");
        _output.AppendLine("finish:");
        _output.AppendLine($"  %terminator.address = getelementptr i8, ptr %data, {sizeType} %byte.count");
        _output.AppendLine("  store i8 0, ptr %terminator.address");
        _output.AppendLine("  %object.type.address = getelementptr %luma.string, ptr %object, i32 0, i32 0");
        _output.AppendLine("  store ptr @luma_string_type, ptr %object.type.address");
        _output.AppendLine("  %object.count.address = getelementptr %luma.string, ptr %object, i32 0, i32 1");
        _output.AppendLine("  store i32 1, ptr %object.count.address");
        _output.AppendLine("  %object.header.flags.address = getelementptr %luma.string, ptr %object, i32 0, i32 2");
        _output.AppendLine("  store i32 0, ptr %object.header.flags.address");
        _output.AppendLine("  %object.weak.address = getelementptr %luma.string, ptr %object, i32 0, i32 3");
        _output.AppendLine("  store ptr null, ptr %object.weak.address");
        _output.AppendLine("  %object.data.address = getelementptr %luma.string, ptr %object, i32 0, i32 4");
        _output.AppendLine("  store ptr %data, ptr %object.data.address");
        _output.AppendLine("  %object.length.address = getelementptr %luma.string, ptr %object, i32 0, i32 5");
        _output.AppendLine("  store i32 %length, ptr %object.length.address");
        _output.AppendLine("  %object.bytes.address = getelementptr %luma.string, ptr %object, i32 0, i32 6");
        _output.AppendLine("  store i32 %bytes, ptr %object.bytes.address");
        _output.AppendLine("  %object.charset.address = getelementptr %luma.string, ptr %object, i32 0, i32 7");
        _output.AppendLine("  store i8 0, ptr %object.charset.address");
        _output.AppendLine("  %left.string.flags.address = getelementptr %luma.string, ptr %left, i32 0, i32 8");
        _output.AppendLine("  %left.string.flags = load i8, ptr %left.string.flags.address");
        _output.AppendLine("  %right.string.flags.address = getelementptr %luma.string, ptr %right, i32 0, i32 8");
        _output.AppendLine("  %right.string.flags = load i8, ptr %right.string.flags.address");
        _output.AppendLine("  %left.ascii.bit = and i8 %left.string.flags, 2");
        _output.AppendLine("  %right.ascii.bit = and i8 %right.string.flags, 2");
        _output.AppendLine("  %left.ascii = icmp eq i8 %left.ascii.bit, 2");
        _output.AppendLine("  %right.ascii = icmp eq i8 %right.ascii.bit, 2");
        _output.AppendLine("  %all.ascii = and i1 %left.ascii, %right.ascii");
        _output.AppendLine("  %result.string.flags = select i1 %all.ascii, i8 3, i8 1");
        _output.AppendLine("  %object.string.flags.address = getelementptr %luma.string, ptr %object, i32 0, i32 8");
        _output.AppendLine("  store i8 %result.string.flags, ptr %object.string.flags.address");
        _output.AppendLine("  %object.units.address = getelementptr %luma.string, ptr %object, i32 0, i32 9");
        _output.AppendLine("  store ptr %units, ptr %object.units.address");
        _output.AppendLine("  ret ptr %object");
        _output.AppendLine("out.of.memory:");
        EmitStringRuntimeOutOfMemory();
        _output.AppendLine("}");

        _output.AppendLine("define internal i1 @luma_string_equals(ptr %left.input, ptr %right.input) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %left.null = icmp eq ptr %left.input, null");
        _output.AppendLine("  %left = select i1 %left.null, ptr @luma_string_empty, ptr %left.input");
        _output.AppendLine("  %right.null = icmp eq ptr %right.input, null");
        _output.AppendLine("  %right = select i1 %right.null, ptr @luma_string_empty, ptr %right.input");
        _output.AppendLine("  %left.length.address = getelementptr %luma.string, ptr %left, i32 0, i32 5");
        _output.AppendLine("  %left.length = load i32, ptr %left.length.address");
        _output.AppendLine("  %right.length.address = getelementptr %luma.string, ptr %right, i32 0, i32 5");
        _output.AppendLine("  %right.length = load i32, ptr %right.length.address");
        _output.AppendLine("  %lengths.equal = icmp eq i32 %left.length, %right.length");
        _output.AppendLine("  br i1 %lengths.equal, label %compare.start, label %not.equal");
        _output.AppendLine("not.equal:");
        _output.AppendLine("  ret i1 false");
        _output.AppendLine("compare.start:");
        _output.AppendLine("  %left.units.address = getelementptr %luma.string, ptr %left, i32 0, i32 9");
        _output.AppendLine("  %left.units = load ptr, ptr %left.units.address");
        _output.AppendLine("  %right.units.address = getelementptr %luma.string, ptr %right, i32 0, i32 9");
        _output.AppendLine("  %right.units = load ptr, ptr %right.units.address");
        _output.AppendLine("  br label %compare.loop");
        _output.AppendLine("compare.loop:");
        _output.AppendLine("  %index = phi i32 [ 0, %compare.start ], [ %next.index, %compare.continue ]");
        _output.AppendLine("  %done = icmp eq i32 %index, %left.length");
        _output.AppendLine("  br i1 %done, label %equal, label %compare.units");
        _output.AppendLine("compare.units:");
        _output.AppendLine("  %left.unit.address = getelementptr i16, ptr %left.units, i32 %index");
        _output.AppendLine("  %left.unit = load i16, ptr %left.unit.address");
        _output.AppendLine("  %right.unit.address = getelementptr i16, ptr %right.units, i32 %index");
        _output.AppendLine("  %right.unit = load i16, ptr %right.unit.address");
        _output.AppendLine("  %units.equal = icmp eq i16 %left.unit, %right.unit");
        _output.AppendLine("  br i1 %units.equal, label %compare.continue, label %not.equal");
        _output.AppendLine("compare.continue:");
        _output.AppendLine("  %next.index = add i32 %index, 1");
        _output.AppendLine("  br label %compare.loop");
        _output.AppendLine("equal:");
        _output.AppendLine("  ret i1 true");
        _output.AppendLine("}");

        _output.AppendLine("define internal i32 @luma_string_length(ptr %input) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %is.null = icmp eq ptr %input, null");
        _output.AppendLine("  %string = select i1 %is.null, ptr @luma_string_empty, ptr %input");
        _output.AppendLine("  %length.address = getelementptr %luma.string, ptr %string, i32 0, i32 5");
        _output.AppendLine("  %length = load i32, ptr %length.address");
        _output.AppendLine("  ret i32 %length");
        _output.AppendLine("}");

        _output.AppendLine("define internal i16 @luma_string_char(ptr %input, i32 %index) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %is.null = icmp eq ptr %input, null");
        _output.AppendLine("  %string = select i1 %is.null, ptr @luma_string_empty, ptr %input");
        _output.AppendLine("  %units.address = getelementptr %luma.string, ptr %string, i32 0, i32 9");
        _output.AppendLine("  %units = load ptr, ptr %units.address");
        _output.AppendLine("  %unit.address = getelementptr i16, ptr %units, i32 %index");
        _output.AppendLine("  %unit = load i16, ptr %unit.address");
        _output.AppendLine("  ret i16 %unit");
        _output.AppendLine("}");

        if (needsPrimitiveStringification)
        {
        _output.AppendLine($$"""
            define internal ptr @luma_string_from_utf8(ptr %source, i32 %byte.length, i32 %length, ptr %source.units, i1 %ascii) {
            entry:
              %is.empty = icmp eq i32 %length, 0
              br i1 %is.empty, label %empty, label %allocate.object
            empty:
              ret ptr @luma_string_empty
            allocate.object:
              %object = call ptr @malloc({{sizeType}} {{stringSize}})
              %object.failed = icmp eq ptr %object, null
              br i1 %object.failed, label %out.of.memory, label %allocate.data
            allocate.data:
              %byte.count = {{(sizeType == "i64" ? "zext i32 %byte.length to i64" : "add i32 %byte.length, 0")}}
              %byte.allocation = add {{sizeType}} %byte.count, 1
              %data = call ptr @malloc({{sizeType}} %byte.allocation)
              %data.failed = icmp eq ptr %data, null
              br i1 %data.failed, label %free.object, label %allocate.units
            free.object:
              call void @free(ptr %object)
              br label %out.of.memory
            allocate.units:
              %unit.count = {{(sizeType == "i64" ? "zext i32 %length to i64" : "add i32 %length, 0")}}
              %unit.bytes = mul {{sizeType}} %unit.count, 2
              %units = call ptr @malloc({{sizeType}} %unit.bytes)
              %units.failed = icmp eq ptr %units, null
              br i1 %units.failed, label %free.data, label %copy.content
            free.data:
              call void @free(ptr %data)
              call void @free(ptr %object)
              br label %out.of.memory
            copy.content:
              %data.copy = call ptr @memcpy(ptr %data, ptr %source, {{sizeType}} %byte.count)
              %units.copy = call ptr @memcpy(ptr %units, ptr %source.units, {{sizeType}} %unit.bytes)
              %terminator = getelementptr i8, ptr %data, {{sizeType}} %byte.count
              store i8 0, ptr %terminator
              %type.address = getelementptr %luma.string, ptr %object, i32 0, i32 0
              store ptr @luma_string_type, ptr %type.address
              %count.address = getelementptr %luma.string, ptr %object, i32 0, i32 1
              store i32 1, ptr %count.address
              %header.flags.address = getelementptr %luma.string, ptr %object, i32 0, i32 2
              store i32 0, ptr %header.flags.address
              %weak.address = getelementptr %luma.string, ptr %object, i32 0, i32 3
              store ptr null, ptr %weak.address
              %data.address = getelementptr %luma.string, ptr %object, i32 0, i32 4
              store ptr %data, ptr %data.address
              %length.address = getelementptr %luma.string, ptr %object, i32 0, i32 5
              store i32 %length, ptr %length.address
              %byte.length.address = getelementptr %luma.string, ptr %object, i32 0, i32 6
              store i32 %byte.length, ptr %byte.length.address
              %charset.address = getelementptr %luma.string, ptr %object, i32 0, i32 7
              store i8 0, ptr %charset.address
              %ascii.flags = select i1 %ascii, i8 3, i8 1
              %string.flags.address = getelementptr %luma.string, ptr %object, i32 0, i32 8
              store i8 %ascii.flags, ptr %string.flags.address
              %units.address = getelementptr %luma.string, ptr %object, i32 0, i32 9
              store ptr %units, ptr %units.address
              ret ptr %object
            out.of.memory:
            {{StringRuntimeOutOfMemoryBody("  ")}}
            }
            """);
        _output.AppendLine($$"""
            define internal ptr @luma_string_from_ascii(ptr %source, i32 %length) {
            entry:
              %units = alloca i16, i32 %length, align 2
              br label %loop
            loop:
              %index = phi i32 [ 0, %entry ], [ %next.index, %body ]
              %done = icmp eq i32 %index, %length
              br i1 %done, label %create, label %body
            body:
              %byte.address = getelementptr i8, ptr %source, i32 %index
              %byte = load i8, ptr %byte.address
              %unit = zext i8 %byte to i16
              %unit.address = getelementptr i16, ptr %units, i32 %index
              store i16 %unit, ptr %unit.address
              %next.index = add i32 %index, 1
              br label %loop
            create:
              %result = call ptr @luma_string_from_utf8(ptr %source, i32 %length, i32 %length, ptr %units, i1 true)
              ret ptr %result
            }
            """);
        _output.AppendLine($$"""
            define internal ptr @luma_string_from_i64(i64 %input, i1 %signed) {
            entry:
              %negative.candidate = icmp slt i64 %input, 0
              %negative = and i1 %signed, %negative.candidate
              %negated = sub i64 0, %input
              %magnitude = select i1 %negative, i64 %negated, i64 %input
              %scratch = alloca [21 x i8], align 1
              %output = alloca [21 x i8], align 1
              br label %digits
            digits:
              %number = phi i64 [ %magnitude, %entry ], [ %quotient, %digit ]
              %offset = phi i32 [ 0, %entry ], [ %next.offset, %digit ]
              %remainder = urem i64 %number, 10
              %digit.byte = trunc i64 %remainder to i8
              %ascii.digit = add i8 %digit.byte, 48
              %scratch.position = getelementptr [21 x i8], ptr %scratch, i32 0, i32 %offset
              store i8 %ascii.digit, ptr %scratch.position
              %quotient = udiv i64 %number, 10
              %next.offset = add i32 %offset, 1
              %digits.done = icmp eq i64 %quotient, 0
              br i1 %digits.done, label %sign, label %digit
            digit:
              br label %digits
            sign:
              %sign.position = getelementptr [21 x i8], ptr %scratch, i32 0, i32 %next.offset
              %sign.byte = select i1 %negative, i8 45, i8 0
              store i8 %sign.byte, ptr %sign.position
              %negative.width = zext i1 %negative to i32
              %length = add i32 %next.offset, %negative.width
              br label %reverse
            reverse:
              %index = phi i32 [ 0, %sign ], [ %next.index, %copy.digit ]
              %copy.done = icmp eq i32 %index, %length
              br i1 %copy.done, label %create, label %copy.digit
            copy.digit:
              %reverse.index = sub i32 %length, %index
              %source.index = sub i32 %reverse.index, 1
              %source.position = getelementptr [21 x i8], ptr %scratch, i32 0, i32 %source.index
              %byte = load i8, ptr %source.position
              %output.position = getelementptr [21 x i8], ptr %output, i32 0, i32 %index
              store i8 %byte, ptr %output.position
              %next.index = add i32 %index, 1
              br label %reverse
            create:
              %source = getelementptr [21 x i8], ptr %output, i32 0, i32 0
              %result = call ptr @luma_string_from_ascii(ptr %source, i32 %length)
              ret ptr %result
            }
            """);
        _output.AppendLine($$"""
            define internal ptr @luma_string_from_char(i16 %input) {
            entry:
              %code = zext i16 %input to i32
              %surrogate.low = icmp uge i32 %code, 55296
              %surrogate.high = icmp ule i32 %code, 57343
              %surrogate = and i1 %surrogate.low, %surrogate.high
              %scalar = select i1 %surrogate, i32 65533, i32 %code
              %one.byte = icmp ult i32 %scalar, 128
              %two.byte = icmp ult i32 %scalar, 2048
              %first.shift = lshr i32 %scalar, 12
              %first.prefix = or i32 %first.shift, 224
              %second.shift = lshr i32 %scalar, 6
              %second.masked = and i32 %second.shift, 63
              %second.utf8 = or i32 %second.masked, 128
              %first.two.shift = lshr i32 %scalar, 6
              %first.two.masked = and i32 %first.two.shift, 31
              %first.two = or i32 %first.two.masked, 192
              %first.one = and i32 %scalar, 127
              %first.non.ascii = select i1 %two.byte, i32 %first.two, i32 %first.prefix
              %first = select i1 %one.byte, i32 %first.one, i32 %first.non.ascii
              %second.low = and i32 %scalar, 63
              %second.three = or i32 %second.low, 128
              %second.non.ascii = select i1 %two.byte, i32 %second.utf8, i32 %second.three
              %second = select i1 %one.byte, i32 0, i32 %second.non.ascii
              %third = select i1 %two.byte, i32 0, i32 %second.three
              %first.byte = trunc i32 %first to i8
              %second.byte = trunc i32 %second to i8
              %third.byte = trunc i32 %third to i8
              %byte.count = select i1 %one.byte, i32 1, i32 3
              %byte.count.final = select i1 %two.byte, i32 2, i32 %byte.count
              %buffer = alloca [3 x i8], align 1
              %units = alloca [1 x i16], align 2
              %first.address = getelementptr [3 x i8], ptr %buffer, i32 0, i32 0
              %second.address = getelementptr [3 x i8], ptr %buffer, i32 0, i32 1
              %third.address = getelementptr [3 x i8], ptr %buffer, i32 0, i32 2
              store i8 %first.byte, ptr %first.address
              store i8 %second.byte, ptr %second.address
              store i8 %third.byte, ptr %third.address
              %unit = trunc i32 %scalar to i16
              %unit.address = getelementptr [1 x i16], ptr %units, i32 0, i32 0
              store i16 %unit, ptr %unit.address
              %result = call ptr @luma_string_from_utf8(ptr %first.address, i32 %byte.count.final, i32 1, ptr %unit.address, i1 %one.byte)
              ret ptr %result
            }
            """);
        }
        if (needsFloatingStringification)
        {
            _output.AppendLine($"declare i32 @snprintf(ptr, {sizeType}, ptr, ...)");
            _output.AppendLine($$"""
                define internal ptr @luma_string_from_float(double %input, i1 %single) {
                entry:
                  %buffer = alloca [128 x i8], align 1
                  %buffer.start = getelementptr [128 x i8], ptr %buffer, i32 0, i32 0
                  %is.nan = fcmp uno double %input, %input
                  br i1 %is.nan, label %nan, label %check.positive.infinity
                nan:
                  ret ptr @luma_string_nan
                check.positive.infinity:
                  %is.positive.infinity = fcmp oeq double %input, 0x7FF0000000000000
                  br i1 %is.positive.infinity, label %positive.infinity, label %check.negative.infinity
                positive.infinity:
                  ret ptr @luma_string_positive_infinity
                check.negative.infinity:
                  %is.negative.infinity = fcmp oeq double %input, 0xFFF0000000000000
                  br i1 %is.negative.infinity, label %negative.infinity, label %format.check
                negative.infinity:
                  ret ptr @luma_string_negative_infinity
                format.check:
                  br i1 %single, label %format.single, label %format.double
                format.single:
                  %single.length = call i32 (ptr, {{sizeType}}, ptr, ...) @snprintf(ptr %buffer.start, {{sizeType}} 128, ptr @luma_format_float, double %input)
                  br label %formatted
                format.double:
                  %double.length = call i32 (ptr, {{sizeType}}, ptr, ...) @snprintf(ptr %buffer.start, {{sizeType}} 128, ptr @luma_format_double, double %input)
                  br label %formatted
                formatted:
                  %length = phi i32 [ %single.length, %format.single ], [ %double.length, %format.double ]
                  %failed = icmp slt i32 %length, 0
                  %too.long = icmp uge i32 %length, 128
                  %invalid = or i1 %failed, %too.long
                  br i1 %invalid, label %format.error, label %create
                format.error:
                  call void @llvm.trap()
                  unreachable
                create:
                  %result = call ptr @luma_string_from_ascii(ptr %buffer.start, i32 %length)
                  ret ptr %result
                }
                """);
        }
    }
    private void EmitStringRuntimeOutOfMemory()
    {
        if (compilation.UsesExceptionHandling)
        {
            _output.AppendLine("  call void @luma_raise_out_of_memory()");
            _output.AppendLine("  ret ptr null");
        }
        else
        {
            _output.AppendLine("  call void @luma_throw_out_of_memory()");
            _output.AppendLine("  unreachable");
        }
    }

    private void EmitStringRuntimeOverflow()
    {
        if (compilation.UsesExceptionHandling)
        {
            _output.AppendLine("  call void @luma_raise_overflow()");
            _output.AppendLine("  ret ptr null");
        }
        else
        {
            _output.AppendLine("  call void @luma_throw_overflow()");
            _output.AppendLine("  unreachable");
        }
    }

    private string StringRuntimeOutOfMemoryBody(string indent) => compilation.UsesExceptionHandling
        ? $"{indent}call void @luma_raise_out_of_memory()\n{indent}ret ptr null"
        : $"{indent}call void @luma_throw_out_of_memory()\n{indent}unreachable";

    private string LlvmType(string type) => LlvmType(type, compilation.EnumUnderlyingTypes, compilation.NativePointerSize,
        compilation.ClassTypes.Keys.Concat(compilation.InterfaceTypes.Keys).Concat(compilation.DelegateTypes.Keys).ToHashSet(StringComparer.Ordinal));
    private static string LlvmType(string type, IReadOnlyDictionary<string, string> enumTypes, int nativePointerSize, IReadOnlySet<string>? classTypes = null)
    {
        if (type.EndsWith('*') || SemanticAnalyzer.TryParseManagedArrayType(type, out _, out _) || type == "string" || classTypes?.Contains(type) == true ||
            type.EndsWith("?", StringComparison.Ordinal) && (type[..^1] == "string" || SemanticAnalyzer.TryParseManagedArrayType(type[..^1], out _, out _) || classTypes?.Contains(type[..^1]) == true)) return "ptr";
        if (SemanticAnalyzer.TryParseInlineArrayType(type, out var elementType, out var count))
            return $"[{count} x {LlvmType(elementType, enumTypes, nativePointerSize, classTypes)}]";
        if (type.EndsWith("?", StringComparison.Ordinal)) return $"{{ i1, {LlvmType(type[..^1], enumTypes, nativePointerSize, classTypes)} }}";
        if (enumTypes.TryGetValue(type, out var underlying)) return LlvmType(underlying, enumTypes, nativePointerSize, classTypes);
        return type switch
        {
            "bool" => "i1", "int8" or "uint8" => "i8", "int16" or "uint16" or "char" => "i16", "int32" or "uint32" => "i32", "int64" or "uint64" => "i64", "int" or "uint" => nativePointerSize == 8 ? "i64" : "i32", "float" => "float", "double" => "double", "void" => "void", "string" => "ptr", _ => $"%struct.{Safe(type)}"
        };
    }

    private bool HasManagedClassSupport() => GetManagedClassTypes().Count > 0;

    private HashSet<string> GetManagedArrayTypes()
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        void Add(string type)
        {
            if (!SemanticAnalyzer.TryParseManagedArrayType(type, out var elementType, out _)) return;
            var nonNullable = type.EndsWith("?", StringComparison.Ordinal) ? type[..^1] : type;
            if (!result.Add(nonNullable)) return;
            Add(elementType);
        }

        foreach (var type in compilation.VariableTypes.Values) Add(type);
        foreach (var type in compilation.ManagedArrayAllocations.Values) Add(type);
        foreach (var type in compilation.TypeOperatorTargets.Values) Add(type);
        foreach (var type in compilation.SwitchTypePatterns.Values) Add(type);
        foreach (var field in compilation.StaticFields) Add(field.Declaration.TypeName);
        foreach (var type in compilation.ClassTypes.Values)
            foreach (var field in type.Fields) Add(field.TypeName);
        foreach (var type in compilation.StructTypes.Values)
            foreach (var field in type.Fields) Add(field.TypeName);
        foreach (var method in compilation.Methods)
        {
            Add(method.Declaration.ReturnType);
            foreach (var parameter in method.Declaration.Parameters) Add(parameter.TypeName);
        }
        foreach (var binding in compilation.ManagedArrayIndexAccesses.Values) Add(binding.ElementType);
        foreach (var type in compilation.ManagedArrayForInElementTypes.Values) Add(type);
        foreach (var type in compilation.ForInTypes.Values) Add(type);
        return result;
    }

    private static string ArrayDescriptorSymbol(string type) =>
        "luma_arraydesc_" + Safe(type) + "_" + Convert.ToHexString(Encoding.UTF8.GetBytes(type));
    private static string ArrayDescriptorPointer(string type) => "@" + ArrayDescriptorSymbol(type);

    private HashSet<string> GetManagedClassTypes()
    {
        var used = new HashSet<string>(StringComparer.Ordinal);
        var visitedStructs = new HashSet<string>(StringComparer.Ordinal);
        void AddType(string type)
        {
            var valueType = type.EndsWith("?", StringComparison.Ordinal) ? type[..^1] : type;
            if (compilation.StructTypes.TryGetValue(valueType, out var structure) && visitedStructs.Add(valueType))
                foreach (var field in structure.Fields) AddType(field.TypeName);
            if (SemanticAnalyzer.TryParseManagedArrayType(type, out var elementType, out _))
            {
                AddType(elementType);
                used.Add("System.Array");
                return;
            }
            var classType = compilation.ClassTypes.ContainsKey(type) ? type : type.EndsWith("?", StringComparison.Ordinal) && compilation.ClassTypes.ContainsKey(type[..^1]) ? type[..^1] : null;
            if (classType is not null) used.Add(classType);
            // Interface references carry object descriptors just like class references.
            // Keeping the root descriptor in the runtime type set is also required by
            // struct-backed interface boxing, whose dispatch path reads the descriptor
            // before selecting the interface table.
            if (compilation.InterfaceTypes.ContainsKey(valueType)) used.Add("System.Object");
            var delegateType = compilation.DelegateTypes.ContainsKey(type) ? type : type.EndsWith("?", StringComparison.Ordinal) && compilation.DelegateTypes.ContainsKey(type[..^1]) ? type[..^1] : null;
            if (delegateType is not null)
            {
                used.Add(delegateType);
                used.Add("System.Delegate");
            }
        }
        foreach (var allocation in compilation.ClassAllocations.Values) AddType(allocation.TypeName);
        foreach (var type in compilation.VariableTypes.Values) AddType(type);
        foreach (var field in compilation.StaticFields) AddType(field.Declaration.TypeName);
        foreach (var type in compilation.ManagedArrayAllocations.Values) AddType(type);
        foreach (var type in compilation.TypeOperatorTargets.Values) AddType(type);
        foreach (var type in compilation.SwitchTypePatterns.Values) AddType(type);
        foreach (var method in compilation.Methods)
        {
            AddType(method.Declaration.ReturnType);
            foreach (var parameter in method.Declaration.Parameters) AddType(parameter.TypeName);
            if (method.IsClassInstanceMethod || method.IsClassFinalizer ||
                method.IsClassConstructor && compilation.VirtualCallSlots.Count > 0)
                AddType(VirtualDispatchLayout.QualifiedTypeName(method.Owner));
        }
        foreach (var field in compilation.ClassFieldAccesses.Values) AddType(field.OwnerType);
        foreach (var field in compilation.UnqualifiedClassFields.Values) AddType(field.OwnerType);
        if (compilation.TypeOperatorTargets.Values.Concat(compilation.SwitchTypePatterns.Values)
            .Any(type => BoxableTypeNames().Contains(type, StringComparer.Ordinal)))
            used.Add("System.Object");
        if (HasStringSupport()) used.Add("System.Object");
        if (compilation.UsesExceptionHandling)
            foreach (var exceptionType in compilation.ExceptionTypes) AddType(exceptionType);
        var pending = new Queue<string>(used);
        while (pending.TryDequeue(out var current))
        {
            if (!compilation.ClassTypes.TryGetValue(current, out var classType)) continue;
            if (classType.BaseTypeName is { } baseType && used.Add(baseType)) pending.Enqueue(baseType);
            foreach (var field in classType.Fields)
            {
                var before = used.ToHashSet(StringComparer.Ordinal);
                AddType(field.TypeName);
                foreach (var added in used.Except(before, StringComparer.Ordinal))
                    if (compilation.ClassTypes.ContainsKey(added)) pending.Enqueue(added);
            }
        }
        return used;
    }

    private static long AlignTo(long value, int alignment) => (value + alignment - 1) / alignment * alignment;

    private int GetAggregateArrayElementSize(string type)
    {
        if (type.EndsWith("?", StringComparison.Ordinal) && compilation.StructTypes.TryGetValue(type[..^1], out var nullableStruct))
        {
            var alignment = nullableStruct.Alignment;
            var payloadOffset = AlignTo(1, alignment);
            return checked((int)AlignTo(payloadOffset + nullableStruct.SizeInBytes, alignment));
        }
        if (compilation.StructTypes.TryGetValue(type, out var structure)) return structure.SizeInBytes;
        throw new InvalidOperationException($"Reference-bearing array element '{type}' has no supported aggregate layout.");
    }

    private void EmitWeakReferenceRuntime(string sizeType)
    {
        var pointerBytes = compilation.NativePointerSize;
        var weakOffset = pointerBytes + 8;
        _output.AppendLine("define internal void @luma_weak_lock(ptr %control)");
        _output.AppendLine("{");
        _output.AppendLine("entry:");
        _output.AppendLine("  %lock.address = getelementptr %luma.weak.control, ptr %control, i32 0, i32 1");
        _output.AppendLine("  br label %retry");
        _output.AppendLine("retry:");
        _output.AppendLine("  %attempt = cmpxchg ptr %lock.address, i32 0, i32 1 acquire monotonic");
        _output.AppendLine("  %acquired = extractvalue { i32, i1 } %attempt, 1");
        _output.AppendLine("  br i1 %acquired, label %done, label %retry");
        _output.AppendLine("done:");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");
        _output.AppendLine("define internal void @luma_weak_unlock(ptr %control)");
        _output.AppendLine("{");
        _output.AppendLine("entry:");
        _output.AppendLine("  %lock.address = getelementptr %luma.weak.control, ptr %control, i32 0, i32 1");
        _output.AppendLine("  store atomic i32 0, ptr %lock.address release, align 4");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");
        _output.AppendLine($"define internal {sizeType} @luma_weak_create(ptr %target)");
        _output.AppendLine("{");
        _output.AppendLine("entry:");
        _output.AppendLine($"  %slot = getelementptr i8, ptr %target, {sizeType} {weakOffset}");
        if (HasStringSupport() || HasManagedArraySupport())
        {
            _output.AppendLine("  %descriptor = load ptr, ptr %target");
            _output.AppendLine("  %release.address = getelementptr %luma.typedesc, ptr %descriptor, i32 0, i32 4");
            _output.AppendLine("  %release.function = load ptr, ptr %release.address");
            var specialComparisons = new List<string>();
            if (HasStringSupport())
            {
                _output.AppendLine("  %is.string = icmp eq ptr %release.function, @luma_string_release");
                specialComparisons.Add("%is.string");
            }
            if (HasManagedArraySupport())
            {
                _output.AppendLine("  %is.array = icmp eq ptr %release.function, @luma_array_release");
                specialComparisons.Add("%is.array");
            }
            var special = specialComparisons.Count == 1 ? specialComparisons[0] : "%is.special";
            if (specialComparisons.Count > 1)
                _output.AppendLine($"  {special} = or i1 {specialComparisons[0]}, {specialComparisons[1]}");
            _output.AppendLine($"  br i1 {special}, label %check.string.immortal, label %check.class.immortal");
            _output.AppendLine("check.string.immortal:");
            if (HasStringSupport())
            {
                _output.AppendLine("  %is.string.target = icmp eq ptr %release.function, @luma_string_release");
                _output.AppendLine("  br i1 %is.string.target, label %check.immortal.flags, label %lookup");
            }
            else _output.AppendLine("  br label %lookup");
            _output.AppendLine("check.class.immortal:");
            _output.AppendLine("  br label %check.immortal.flags");
            _output.AppendLine("check.immortal.flags:");
            _output.AppendLine($"  %flags.address = getelementptr i8, ptr %target, {sizeType} {pointerBytes + 4}");
            _output.AppendLine("  %flags = load i32, ptr %flags.address");
            _output.AppendLine("  %immortal.bits = and i32 %flags, 1");
            _output.AppendLine("  %immortal = icmp ne i32 %immortal.bits, 0");
            _output.AppendLine("  %destroying.bits = and i32 %flags, 4");
            _output.AppendLine("  %destroying = icmp ne i32 %destroying.bits, 0");
            _output.AppendLine("  %needs.detached = or i1 %immortal, %destroying");
            _output.AppendLine("  %detached.target = select i1 %destroying, ptr null, ptr %target");
            _output.AppendLine("  %existing.control = load atomic ptr, ptr %slot acquire, align " + pointerBytes);
            _output.AppendLine("  %has.existing.control = icmp ne ptr %existing.control, null");
            _output.AppendLine("  %reuse.destroying.control = and i1 %destroying, %has.existing.control");
            _output.AppendLine("  %allocate.detached = xor i1 %reuse.destroying.control, %needs.detached");
            _output.AppendLine("  br i1 %allocate.detached, label %detached.allocate, label %lookup");
        }
        else
        {
            _output.AppendLine($"  %flags.address = getelementptr i8, ptr %target, {sizeType} {pointerBytes + 4}");
            _output.AppendLine("  %flags = load i32, ptr %flags.address");
            _output.AppendLine("  %immortal.bits = and i32 %flags, 1");
            _output.AppendLine("  %immortal = icmp ne i32 %immortal.bits, 0");
            _output.AppendLine("  %destroying.bits = and i32 %flags, 4");
            _output.AppendLine("  %destroying = icmp ne i32 %destroying.bits, 0");
            _output.AppendLine("  %needs.detached = or i1 %immortal, %destroying");
            _output.AppendLine("  %detached.target = select i1 %destroying, ptr null, ptr %target");
            _output.AppendLine("  %existing.control = load atomic ptr, ptr %slot acquire, align " + pointerBytes);
            _output.AppendLine("  %has.existing.control = icmp ne ptr %existing.control, null");
            _output.AppendLine("  %reuse.destroying.control = and i1 %destroying, %has.existing.control");
            _output.AppendLine("  %allocate.detached = xor i1 %reuse.destroying.control, %needs.detached");
            _output.AppendLine("  br i1 %allocate.detached, label %detached.allocate, label %lookup");
        }
        _output.AppendLine("detached.allocate:");
        _output.AppendLine("  %detached.size.address = getelementptr %luma.weak.control, ptr null, i32 1");
        _output.AppendLine($"  %detached.size = ptrtoint ptr %detached.size.address to {sizeType}");
        _output.AppendLine($"  %detached.candidate = call ptr @malloc({sizeType} %detached.size)");
        _output.AppendLine("  %detached.failed = icmp eq ptr %detached.candidate, null");
        _output.AppendLine("  br i1 %detached.failed, label %oom, label %detached.initialize");
        _output.AppendLine("detached.initialize:");
        _output.AppendLine("  %detached.target.address = getelementptr %luma.weak.control, ptr %detached.candidate, i32 0, i32 0");
        _output.AppendLine("  store ptr %detached.target, ptr %detached.target.address");
        _output.AppendLine("  %detached.lock.address = getelementptr %luma.weak.control, ptr %detached.candidate, i32 0, i32 1");
        _output.AppendLine("  store i32 0, ptr %detached.lock.address");
        _output.AppendLine("  %detached.count.address = getelementptr %luma.weak.control, ptr %detached.candidate, i32 0, i32 2");
        _output.AppendLine("  store i32 1, ptr %detached.count.address");
        _output.AppendLine("  %detached.flag.address = getelementptr %luma.weak.control, ptr %detached.candidate, i32 0, i32 3");
        _output.AppendLine("  store i32 1, ptr %detached.flag.address");
        _output.AppendLine($"  %detached.handle = ptrtoint ptr %detached.candidate to {sizeType}");
        _output.AppendLine($"  ret {sizeType} %detached.handle");
        _output.AppendLine("lookup:");
        _output.AppendLine("  %current = load atomic ptr, ptr %slot acquire, align " + pointerBytes);
        _output.AppendLine("  %missing = icmp eq ptr %current, null");
        _output.AppendLine("  br i1 %missing, label %allocate, label %activate");
        _output.AppendLine("allocate:");
        _output.AppendLine("  %size.address = getelementptr %luma.weak.control, ptr null, i32 1");
        _output.AppendLine($"  %size = ptrtoint ptr %size.address to {sizeType}");
        _output.AppendLine($"  %candidate = call ptr @malloc({sizeType} %size)");
        _output.AppendLine("  %failed = icmp eq ptr %candidate, null");
        _output.AppendLine("  br i1 %failed, label %oom, label %initialize");
        _output.AppendLine("initialize:");
        _output.AppendLine("  %target.address = getelementptr %luma.weak.control, ptr %candidate, i32 0, i32 0");
        _output.AppendLine("  store ptr %target, ptr %target.address");
        _output.AppendLine("  %lock.address = getelementptr %luma.weak.control, ptr %candidate, i32 0, i32 1");
        _output.AppendLine("  store i32 0, ptr %lock.address");
        _output.AppendLine("  %active.address = getelementptr %luma.weak.control, ptr %candidate, i32 0, i32 2");
        _output.AppendLine("  store i32 0, ptr %active.address");
        _output.AppendLine("  %detached.address = getelementptr %luma.weak.control, ptr %candidate, i32 0, i32 3");
        _output.AppendLine("  store i32 0, ptr %detached.address");
        _output.AppendLine("  %attempt = cmpxchg ptr %slot, ptr null, ptr %candidate acq_rel acquire");
        _output.AppendLine("  %prior = extractvalue { ptr, i1 } %attempt, 0");
        _output.AppendLine("  %won = extractvalue { ptr, i1 } %attempt, 1");
        _output.AppendLine("  br i1 %won, label %candidate.won, label %candidate.lost");
        _output.AppendLine("candidate.won:");
        _output.AppendLine("  br label %activate");
        _output.AppendLine("candidate.lost:");
        _output.AppendLine("  call void @free(ptr %candidate)");
        _output.AppendLine("  br label %lookup");
        _output.AppendLine("activate:");
        _output.AppendLine("  %control = phi ptr [ %current, %lookup ], [ %candidate, %candidate.won ]");
        _output.AppendLine("  call void @luma_weak_lock(ptr %control)");
        _output.AppendLine("  %active.ptr = getelementptr %luma.weak.control, ptr %control, i32 0, i32 2");
        _output.AppendLine("  %active.old = atomicrmw add ptr %active.ptr, i32 1 acq_rel");
        _output.AppendLine("  call void @luma_weak_unlock(ptr %control)");
        _output.AppendLine($"  %handle = ptrtoint ptr %control to {sizeType}");
        _output.AppendLine($"  ret {sizeType} %handle");
        _output.AppendLine("oom:");
        _output.AppendLine("  call void @luma_throw_out_of_memory()");
        _output.AppendLine("  unreachable");
        _output.AppendLine("}");
        _output.AppendLine($"define internal i1 @luma_weak_is_alive({sizeType} %handle)");
        _output.AppendLine("{");
        _output.AppendLine("entry:");
        _output.AppendLine($"  %control = inttoptr {sizeType} %handle to ptr");
        _output.AppendLine("  call void @luma_weak_lock(ptr %control)");
        _output.AppendLine("  %target.address = getelementptr %luma.weak.control, ptr %control, i32 0, i32 0");
        _output.AppendLine("  %target = load atomic ptr, ptr %target.address acquire, align " + pointerBytes);
        _output.AppendLine("  %alive = icmp ne ptr %target, null");
        _output.AppendLine("  call void @luma_weak_unlock(ptr %control)");
        _output.AppendLine("  ret i1 %alive");
        _output.AppendLine("}");
        _output.AppendLine($"define internal ptr @luma_weak_get_target({sizeType} %handle)");
        _output.AppendLine("{");
        _output.AppendLine("entry:");
        _output.AppendLine($"  %control = inttoptr {sizeType} %handle to ptr");
        _output.AppendLine("  call void @luma_weak_lock(ptr %control)");
        _output.AppendLine("  %target.address = getelementptr %luma.weak.control, ptr %control, i32 0, i32 0");
        _output.AppendLine("  %target = load atomic ptr, ptr %target.address acquire, align " + pointerBytes);
        _output.AppendLine("  %is.alive = icmp ne ptr %target, null");
        _output.AppendLine("  %detached.address = getelementptr %luma.weak.control, ptr %control, i32 0, i32 3");
        _output.AppendLine("  %detached = load i32, ptr %detached.address");
        _output.AppendLine("  %is.detached = icmp ne i32 %detached, 0");
        _output.AppendLine("  %not.detached = xor i1 %is.detached, true");
        _output.AppendLine("  %should.retain = and i1 %is.alive, %not.detached");
        _output.AppendLine("  br i1 %should.retain, label %retain, label %done");
        _output.AppendLine("retain:");
        _output.AppendLine($"  %count.address = getelementptr i8, ptr %target, {sizeType} {pointerBytes}");
        _output.AppendLine("  %old.count = atomicrmw add ptr %count.address, i32 1 acq_rel");
        _output.AppendLine("  br label %done");
        _output.AppendLine("done:");
        _output.AppendLine("  %result = select i1 %is.alive, ptr %target, ptr null");
        _output.AppendLine("  call void @luma_weak_unlock(ptr %control)");
        _output.AppendLine("  ret ptr %result");
        _output.AppendLine("}");
        _output.AppendLine($"define internal void @luma_weak_release({sizeType} %handle)");
        _output.AppendLine("{");
        _output.AppendLine("entry:");
        _output.AppendLine($"  %control = inttoptr {sizeType} %handle to ptr");
        _output.AppendLine("  call void @luma_weak_lock(ptr %control)");
        _output.AppendLine("  %active.address = getelementptr %luma.weak.control, ptr %control, i32 0, i32 2");
        _output.AppendLine("  %active.old = atomicrmw sub ptr %active.address, i32 1 acq_rel");
        _output.AppendLine("  %target.address = getelementptr %luma.weak.control, ptr %control, i32 0, i32 0");
        _output.AppendLine("  %target = load atomic ptr, ptr %target.address acquire, align " + pointerBytes);
        _output.AppendLine("  %is.dead = icmp eq ptr %target, null");
        _output.AppendLine("  %no.handles = icmp eq i32 %active.old, 1");
        _output.AppendLine("  %detached.address = getelementptr %luma.weak.control, ptr %control, i32 0, i32 3");
        _output.AppendLine("  %detached = load i32, ptr %detached.address");
        _output.AppendLine("  %is.detached = icmp ne i32 %detached, 0");
        _output.AppendLine("  %can.free = or i1 %is.dead, %is.detached");
        _output.AppendLine("  %should.free = and i1 %can.free, %no.handles");
        _output.AppendLine("  call void @luma_weak_unlock(ptr %control)");
        _output.AppendLine("  br i1 %should.free, label %free, label %done");
        _output.AppendLine("free:");
        _output.AppendLine("  call void @free(ptr %control)");
        _output.AppendLine("  br label %done");
        _output.AppendLine("done:");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");
    }

    private void EmitManagedClassRuntime()
    {
        var sizeType = compilation.NativePointerSize == 8 ? "i64" : "i32";
        EmitWeakReferenceRuntime(sizeType);
        var borrowedClosureFields = compilation.ClosureEnvironments.Values
            .Concat(compilation.ForHeaderClosureEnvironments.Values)
            .Concat(compilation.ForInClosureEnvironments.Values)
            .Concat(compilation.BlockClosureEnvironments.Values)
            .Concat(compilation.CompositeClosureEnvironments.Values)
            .SelectMany(environment => (environment.BorrowedCaptureFieldIndices ?? new HashSet<int>())
                .Select(index => (environment.TypeName, Index: index)))
            .ToHashSet();
        _output.AppendLine("define internal void @luma_class_retain_dynamic(ptr %object) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %is.null = icmp eq ptr %object, null");
        _output.AppendLine("  br i1 %is.null, label %done, label %retain");
        _output.AppendLine("retain:");
        _output.AppendLine("  %descriptor = load ptr, ptr %object");
        _output.AppendLine("  %retain.address = getelementptr %luma.typedesc, ptr %descriptor, i32 0, i32 5");
        _output.AppendLine("  %retain.function = load ptr, ptr %retain.address");
        _output.AppendLine("  call void %retain.function(ptr %object)");
        _output.AppendLine("  br label %done");
        _output.AppendLine("done:");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");
        _output.AppendLine("define internal void @luma_class_release_dynamic(ptr %object) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %is.null = icmp eq ptr %object, null");
        _output.AppendLine("  br i1 %is.null, label %done, label %release");
        _output.AppendLine("release:");
        _output.AppendLine("  %descriptor = load ptr, ptr %object");
        _output.AppendLine("  %release.address = getelementptr %luma.typedesc, ptr %descriptor, i32 0, i32 4");
        _output.AppendLine("  %release.function = load ptr, ptr %release.address");
        _output.AppendLine("  call void %release.function(ptr %object)");
        _output.AppendLine("  br label %done");
        _output.AppendLine("done:");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");
        _output.AppendLine("define internal i1 @luma_class_implements(ptr %object, ptr %target) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %is.null = icmp eq ptr %object, null");
        _output.AppendLine("  br i1 %is.null, label %no, label %lookup");
        _output.AppendLine("lookup:");
        _output.AppendLine("  %descriptor = load ptr, ptr %object");
        _output.AppendLine("  %interfaces.address = getelementptr %luma.typedesc, ptr %descriptor, i32 0, i32 7");
        _output.AppendLine("  %interfaces = load ptr, ptr %interfaces.address");
        _output.AppendLine("  %count.address = getelementptr %luma.typedesc, ptr %descriptor, i32 0, i32 8");
        _output.AppendLine("  %count = load i32, ptr %count.address");
        _output.AppendLine("  br label %loop");
        _output.AppendLine("loop:");
        _output.AppendLine("  %index = phi i32 [ 0, %lookup ], [ %next, %advance ]");
        _output.AppendLine("  %in.range = icmp ult i32 %index, %count");
        _output.AppendLine("  br i1 %in.range, label %check, label %no");
        _output.AppendLine("check:");
        _output.AppendLine("  %interface.id.address = getelementptr ptr, ptr %interfaces, i32 %index");
        _output.AppendLine("  %interface.id = load ptr, ptr %interface.id.address");
        _output.AppendLine("  %matches = icmp eq ptr %interface.id, %target");
        _output.AppendLine("  br i1 %matches, label %yes, label %advance");
        _output.AppendLine("advance:");
        _output.AppendLine("  %next = add i32 %index, 1");
        _output.AppendLine("  br label %loop");
        _output.AppendLine("yes:"); _output.AppendLine("  ret i1 true");
        _output.AppendLine("no:"); _output.AppendLine("  ret i1 false");
        _output.AppendLine("}");
        foreach (var (name, classType) in compilation.ClassTypes)
        {
            if (!HasManagedClassSupportFor(name)) continue;
            var safe = Safe(name);
            var nameBytes = Encoding.UTF8.GetBytes(name + "\0");
            var nameData = string.Join(", ", nameBytes.Select(value => $"i8 {value}"));
            _output.AppendLine($"@luma_classname_{safe} = private constant [{nameBytes.Length} x i8] [{nameData}]");
            var virtualMethods = VirtualDispatchLayout.GetMethods(name, compilation.ClassTypes, compilation.Methods);
            if (virtualMethods.Count > 0 && name != "System.String")
            {
                var entries = string.Join(", ", virtualMethods.Select(method => method.IsBuiltinObjectToString
                    ? "ptr @luma_object_tostring"
                    : method.Declaration.Body is null ? "ptr null" : $"ptr @{FunctionName(method, compilation.Methods)}"));
                _output.AppendLine($"@luma_vtable_{safe} = private constant [{virtualMethods.Count} x ptr] [{entries}]");
            }
            var namePointer = $"ptr getelementptr inbounds ([{nameBytes.Length} x i8], ptr @luma_classname_{safe}, i32 0, i32 0)";
            var basePointer = classType.BaseTypeName is { } baseName && HasManagedClassSupportFor(baseName)
                ? $"ptr @luma_classdesc_{Safe(baseName)}" : "ptr null";
            var vtablePointer = virtualMethods.Count == 0 ? "ptr null" :
                $"ptr getelementptr inbounds ([{virtualMethods.Count} x ptr], ptr @luma_vtable_{safe}, i32 0, i32 0)";
            var interfaceEntries = compilation.InterfaceDispatchTables.GetValueOrDefault(name) ?? [];
            var interfaceTablePointer = "ptr null";
            if (compilation.InterfaceMethodSlots.Count > 0)
            {
                var entries = compilation.InterfaceMethodSlots.Select(slot =>
                {
                    var implementation = slot.Index < interfaceEntries.Count ? interfaceEntries[slot.Index] : null;
                    return implementation?.Declaration.Body is not null
                        ? $"ptr @{FunctionName(implementation, compilation.Methods)}"
                        : "ptr null";
                });
                _output.AppendLine($"@luma_itable_{safe} = private constant [{compilation.InterfaceMethodSlots.Count} x ptr] [{string.Join(", ", entries)}]");
                interfaceTablePointer = $"ptr getelementptr inbounds ([{compilation.InterfaceMethodSlots.Count} x ptr], ptr @luma_itable_{safe}, i32 0, i32 0)";
            }
            var implementedInterfaces = compilation.InterfaceConversions.Where(conversion => conversion.FromType == name &&
                    compilation.InterfaceTypes.ContainsKey(conversion.ToType))
                .Select(conversion => conversion.ToType).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
            var interfaceIdPointer = "ptr null";
            if (implementedInterfaces.Length > 0)
            {
                var ids = string.Join(", ", implementedInterfaces.Select(interfaceName => $"ptr @luma_interfacedesc_{Safe(interfaceName)}"));
                _output.AppendLine($"@luma_interfaceids_{safe} = private constant [{implementedInterfaces.Length} x ptr] [{ids}]");
                interfaceIdPointer = $"ptr getelementptr inbounds ([{implementedInterfaces.Length} x ptr], ptr @luma_interfaceids_{safe}, i32 0, i32 0)";
            }
            var typeNameStringPointer = HasStringSupport() && StringLiteralCodec.TryCreate(name, out var typeNameLiteral, out _)
                ? $"ptr @{typeNameLiteral.Symbol}" : "ptr null";
            _output.AppendLine($"@luma_classdesc_{safe} = private constant %luma.typedesc {{ {namePointer}, {basePointer}, {vtablePointer}, i32 {virtualMethods.Count}, ptr @luma_class_{safe}_release, ptr @luma_class_{safe}_retain, {interfaceTablePointer}, {interfaceIdPointer}, i32 {implementedInterfaces.Length}, {typeNameStringPointer} }}");
            _output.AppendLine($"define internal void @luma_class_{safe}_retain(ptr %input) {{");
            _output.AppendLine("entry:");
            _output.AppendLine("  %is.null = icmp eq ptr %input, null");
            _output.AppendLine("  br i1 %is.null, label %done, label %check.counted");
            _output.AppendLine("check.counted:");
            _output.AppendLine($"  %counted.flags.address = getelementptr %class.{safe}, ptr %input, i32 0, i32 2");
            _output.AppendLine("  %counted.flags = load atomic i32, ptr %counted.flags.address acquire, align 4");
            _output.AppendLine("  %uncounted.bits = and i32 %counted.flags, 3");
            _output.AppendLine("  %uncounted = icmp ne i32 %uncounted.bits, 0");
            _output.AppendLine("  br i1 %uncounted, label %done, label %retain");
            _output.AppendLine("retain:");
            _output.AppendLine($"  %count.address = getelementptr %class.{safe}, ptr %input, i32 0, i32 1");
            EmitCheckedReferenceCountIncrement("%count.address", "done", "class.retain");
            _output.AppendLine("done:");
            _output.AppendLine("  ret void");
            _output.AppendLine("}");
            _output.AppendLine($"define internal void @luma_class_{safe}_release(ptr %input) {{");
            _output.AppendLine("entry:");
            _output.AppendLine("  %is.null = icmp eq ptr %input, null");
            _output.AppendLine("  br i1 %is.null, label %done, label %check.immortal");
            _output.AppendLine("check.immortal:");
            _output.AppendLine($"  %immortal.flags.address = getelementptr %class.{safe}, ptr %input, i32 0, i32 2");
            _output.AppendLine("  %immortal.flags = load atomic i32, ptr %immortal.flags.address acquire, align 4");
            _output.AppendLine("  %uncounted.bits = and i32 %immortal.flags, 3");
            _output.AppendLine("  %is.uncounted = icmp ne i32 %uncounted.bits, 0");
            _output.AppendLine("  br i1 %is.uncounted, label %done, label %decrement");
            _output.AppendLine("decrement:");
            _output.AppendLine($"  %weak.address = getelementptr %class.{safe}, ptr %input, i32 0, i32 3");
            _output.AppendLine("  %weak.control = load atomic ptr, ptr %weak.address acquire, align " + compilation.NativePointerSize);
            _output.AppendLine("  %has.weak.control = icmp ne ptr %weak.control, null");
            _output.AppendLine("  br i1 %has.weak.control, label %weak.decrement, label %normal.decrement");
            _output.AppendLine("normal.decrement:");
            _output.AppendLine($"  %count.address = getelementptr %class.{safe}, ptr %input, i32 0, i32 1");
            _output.AppendLine("  %old.count = atomicrmw sub ptr %count.address, i32 1 acq_rel");
            _output.AppendLine("  %last.reference = icmp eq i32 %old.count, 1");
            _output.AppendLine("  br i1 %last.reference, label %check.destroying, label %done");
            _output.AppendLine("weak.decrement:");
            _output.AppendLine("  call void @luma_weak_lock(ptr %weak.control)");
            _output.AppendLine($"  %weak.count.address = getelementptr %class.{safe}, ptr %input, i32 0, i32 1");
            _output.AppendLine("  %weak.old.count = atomicrmw sub ptr %weak.count.address, i32 1 acq_rel");
            _output.AppendLine("  %weak.last.reference = icmp eq i32 %weak.old.count, 1");
            _output.AppendLine("  br i1 %weak.last.reference, label %weak.last.release, label %weak.not.last");
            _output.AppendLine("weak.not.last:");
            _output.AppendLine("  call void @luma_weak_unlock(ptr %weak.control)");
            _output.AppendLine("  br label %done");
            _output.AppendLine("weak.last.release:");
            _output.AppendLine("  %weak.target.address = getelementptr %luma.weak.control, ptr %weak.control, i32 0, i32 0");
            _output.AppendLine("  store atomic ptr null, ptr %weak.target.address release, align " + compilation.NativePointerSize);
            _output.AppendLine("  %weak.active.address = getelementptr %luma.weak.control, ptr %weak.control, i32 0, i32 2");
            _output.AppendLine("  %weak.handle.count = load atomic i32, ptr %weak.active.address acquire, align 4");
            _output.AppendLine("  %weak.active = icmp ne i32 %weak.handle.count, 0");
            _output.AppendLine("  call void @luma_weak_unlock(ptr %weak.control)");
            _output.AppendLine("  br i1 %weak.active, label %check.destroying, label %weak.free");
            _output.AppendLine("weak.free:");
            _output.AppendLine("  store atomic ptr null, ptr %weak.address release, align " + compilation.NativePointerSize);
            _output.AppendLine("  call void @free(ptr %weak.control)");
            _output.AppendLine("  br label %check.destroying");
            _output.AppendLine("check.destroying:");
            _output.AppendLine($"  %flags.address = getelementptr %class.{safe}, ptr %input, i32 0, i32 2");
            _output.AppendLine("  %flags = load atomic i32, ptr %flags.address acquire, align 4");
            _output.AppendLine("  %destroying.bits = and i32 %flags, 4");
            _output.AppendLine("  %already.destroying = icmp ne i32 %destroying.bits, 0");
            _output.AppendLine("  br i1 %already.destroying, label %done, label %destroy");
            _output.AppendLine("destroy:");
            _output.AppendLine($"  %destroy.flags.address = getelementptr %class.{safe}, ptr %input, i32 0, i32 2");
            _output.AppendLine("  %prior.flags = atomicrmw or ptr %destroy.flags.address, i32 4 acq_rel");
            var classChain = GetClassBaseChain(name).ToArray();
            for (var ownerIndex = 0; ownerIndex < classChain.Length; ownerIndex++)
            {
                var ownerType = classChain[ownerIndex];
                var finalizer = compilation.Methods.SingleOrDefault(method => method.IsClassFinalizer &&
                    (string.IsNullOrEmpty(method.Owner.Namespace) ? method.Owner.Name : method.Owner.Namespace + "." + method.Owner.Name) == ownerType);
                if (finalizer is not null)
                {
                    var completionBit = GetFinalizerCompletionBits()[ownerType];
                    var completionMask = unchecked((int)(1u << (completionBit + 3)));
                    _output.AppendLine($"  %finalizer.flags.address.{ownerIndex} = getelementptr %class.{safe}, ptr %input, i32 0, i32 2");
                    _output.AppendLine($"  %finalizer.flags.{ownerIndex} = load atomic i32, ptr %finalizer.flags.address.{ownerIndex} acquire, align 4");
                    _output.AppendLine($"  %finalizer.completed.bits.{ownerIndex} = and i32 %finalizer.flags.{ownerIndex}, {completionMask}");
                    _output.AppendLine($"  %finalizer.completed.{ownerIndex} = icmp ne i32 %finalizer.completed.bits.{ownerIndex}, 0");
                    _output.AppendLine($"  br i1 %finalizer.completed.{ownerIndex}, label %finalizer.run.{ownerIndex}, label %finalizer.skip.{ownerIndex}");
                    _output.AppendLine($"finalizer.run.{ownerIndex}:");
                    _output.AppendLine($"  %finalizer.type.address.{ownerIndex} = getelementptr %class.{safe}, ptr %input, i32 0, i32 0");
                    _output.AppendLine($"  store ptr @luma_classdesc_{Safe(ownerType)}, ptr %finalizer.type.address.{ownerIndex}");
                    _output.AppendLine($"  call void @{FunctionName(finalizer, compilation.Methods)}(ptr %input)");
                    _output.AppendLine($"  br label %finalizer.skip.{ownerIndex}");
                    _output.AppendLine($"finalizer.skip.{ownerIndex}:");
                }
                foreach (var field in compilation.ClassTypes[name].Fields.Where(field =>
                             (string.IsNullOrEmpty(field.Owner.Namespace) ? field.Owner.Name : field.Owner.Namespace + "." + field.Owner.Name) == ownerType).Reverse())
                {
                    if (borrowedClosureFields.Contains((name, field.Index))) continue;
                    if (AggregateTypeContainsManagedReferences(field.TypeName))
                    {
                        _output.AppendLine($"  %field.{field.Index}.aggregate.address = getelementptr %class.{safe}, ptr %input, i32 0, i32 {field.Index + 4}");
                        _output.AppendLine($"  call void @{AggregateOwnershipFunctionName(field.TypeName, retain: false)}(ptr %field.{field.Index}.aggregate.address)");
                        continue;
                    }
                    var fieldBaseType = field.TypeName.EndsWith("?", StringComparison.Ordinal) ? field.TypeName[..^1] : field.TypeName;
                    if (fieldBaseType != "string" && !compilation.ClassTypes.ContainsKey(fieldBaseType) && !compilation.InterfaceTypes.ContainsKey(fieldBaseType) && !compilation.DelegateTypes.ContainsKey(fieldBaseType) && !SemanticAnalyzer.TryParseManagedArrayType(fieldBaseType, out _, out _)) continue;
                    _output.AppendLine($"  %field.{field.Index}.address = getelementptr %class.{safe}, ptr %input, i32 0, i32 {field.Index + 4}");
                    _output.AppendLine($"  %field.{field.Index}.value = load ptr, ptr %field.{field.Index}.address");
                    var releaseFunction = fieldBaseType == "string" ? "luma_string_release" : compilation.ClassTypes.ContainsKey(fieldBaseType) || compilation.InterfaceTypes.ContainsKey(fieldBaseType) || compilation.DelegateTypes.ContainsKey(fieldBaseType) ? "luma_class_release_dynamic" : "luma_array_release";
                    _output.AppendLine($"  call void @{releaseFunction}(ptr %field.{field.Index}.value)");
                }
            }
            _output.AppendLine("  call void @free(ptr %input)");
            _output.AppendLine("  br label %done");
            _output.AppendLine("done:");
            _output.AppendLine("  ret void");
            _output.AppendLine("}");
        }
        var objectVirtualMethods = VirtualDispatchLayout.GetMethods("System.Object", compilation.ClassTypes, compilation.Methods);
        var objectVtablePointer = objectVirtualMethods.Count == 0 ? "ptr null" :
            $"ptr getelementptr inbounds ([{objectVirtualMethods.Count} x ptr], ptr @luma_vtable_System_Object, i32 0, i32 0)";
        foreach (var arrayType in GetManagedArrayTypes().Order(StringComparer.Ordinal))
        {
            var symbol = ArrayDescriptorSymbol(arrayType);
            var nameBytes = Encoding.UTF8.GetBytes(arrayType + "\0");
            var nameData = string.Join(", ", nameBytes.Select(value => $"i8 {value}"));
            _output.AppendLine($"@{symbol}_name = private constant [{nameBytes.Length} x i8] [{nameData}]");
            var namePointer = $"ptr getelementptr inbounds ([{nameBytes.Length} x i8], ptr @{symbol}_name, i32 0, i32 0)";
            var typeNameStringPointer = HasStringSupport() && StringLiteralCodec.TryCreate(arrayType, out var arrayNameLiteral, out _)
                ? $"ptr @{arrayNameLiteral.Symbol}" : "ptr null";
            var elementMetadata = "ptr null";
            var elementRelease = "ptr null";
            if (SemanticAnalyzer.TryParseManagedArrayType(arrayType, out var elementType, out _) && AggregateTypeContainsManagedReferences(elementType))
            {
                var elementSize = GetAggregateArrayElementSize(elementType);
                _output.AppendLine($"@{symbol}_element_size = private constant i32 {elementSize}");
                elementMetadata = $"ptr @{symbol}_element_size";
                elementRelease = $"ptr @{AggregateOwnershipFunctionName(elementType, retain: false)}";
            }
            _output.AppendLine($"@{symbol} = private constant %luma.typedesc {{ {namePointer}, ptr @luma_classdesc_System_Array, {objectVtablePointer}, i32 {objectVirtualMethods.Count}, ptr @luma_array_release, ptr @luma_array_retain, {elementMetadata}, {elementRelease}, i32 0, {typeNameStringPointer} }}");
        }
        if (HasStringSupport())
        {
            if (objectVirtualMethods.Count > 0)
            {
                var stringEntries = string.Join(", ", objectVirtualMethods.Select(method => method.IsBuiltinObjectToString
                    ? "ptr @luma_string_tostring"
                    : method.Declaration.Body is null ? "ptr null" : $"ptr @{FunctionName(method, compilation.Methods)}"));
                _output.AppendLine($"@luma_vtable_System_String = private constant [{objectVirtualMethods.Count} x ptr] [{stringEntries}]");
                _output.AppendLine("define internal ptr @luma_string_tostring(ptr %this) {");
                _output.AppendLine("entry:");
                _output.AppendLine("  ret ptr %this");
                _output.AppendLine("}");
            }
            var stringNamePointer = "ptr getelementptr inbounds ([7 x i8], ptr @luma_string_type_name, i32 0, i32 0)";
            var stringVtablePointer = objectVirtualMethods.Count == 0 ? "ptr null" :
                $"ptr getelementptr inbounds ([{objectVirtualMethods.Count} x ptr], ptr @luma_vtable_System_String, i32 0, i32 0)";
            _output.AppendLine($"@luma_string_type = private constant %luma.typedesc {{ {stringNamePointer}, ptr @luma_classdesc_System_Object, {stringVtablePointer}, i32 {objectVirtualMethods.Count}, ptr @luma_string_release, ptr @luma_string_retain, ptr null, ptr null, i32 0, ptr null }}");
        }
        EmitBoxRuntime();
        EmitManagedDelegateRuntime();
        _output.AppendLine("define internal ptr @luma_object_tostring(ptr %this) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %descriptor = load ptr, ptr %this");
        _output.AppendLine("  %name.address = getelementptr %luma.typedesc, ptr %descriptor, i32 0, i32 9");
        _output.AppendLine("  %name = load ptr, ptr %name.address");
        _output.AppendLine("  ret ptr %name");
        _output.AppendLine("}");
        _output.AppendLine("define internal i1 @luma_class_is_instance(ptr %object, ptr %target) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %is.null = icmp eq ptr %object, null");
        _output.AppendLine("  br i1 %is.null, label %no, label %start");
        _output.AppendLine("start:");
        _output.AppendLine("  %dynamic.type = load ptr, ptr %object");
        _output.AppendLine("  br label %walk");
        _output.AppendLine("walk:");
        _output.AppendLine("  %current.type = phi ptr [ %dynamic.type, %start ], [ %base.type, %advance ]");
        _output.AppendLine("  %matches = icmp eq ptr %current.type, %target");
        _output.AppendLine("  br i1 %matches, label %yes, label %advance");
        _output.AppendLine("advance:");
        _output.AppendLine("  %base.address = getelementptr %luma.typedesc, ptr %current.type, i32 0, i32 1");
        _output.AppendLine("  %base.type = load ptr, ptr %base.address");
        _output.AppendLine("  %no.base = icmp eq ptr %base.type, null");
        _output.AppendLine("  br i1 %no.base, label %no, label %walk");
        _output.AppendLine("yes:"); _output.AppendLine("  ret i1 true");
        _output.AppendLine("no:"); _output.AppendLine("  ret i1 false");
        _output.AppendLine("}");
    }

    private IEnumerable<string> BoxableStructTypeNames() => compilation.StructTypes.Keys.Order(StringComparer.Ordinal);

    private bool StructContainsManagedReferences(string type) => StructContainsManagedReferences(type, new HashSet<string>(StringComparer.Ordinal));

    private bool StructContainsManagedReferences(string type, HashSet<string> visiting)
    {
        if (type is "string" or "string?" or "object" || compilation.ClassTypes.ContainsKey(type) ||
            compilation.InterfaceTypes.ContainsKey(type) || compilation.DelegateTypes.ContainsKey(type) ||
            SemanticAnalyzer.TryParseManagedArrayType(type, out _, out _)) return true;
        if (type.EndsWith("?", StringComparison.Ordinal))
        {
            var underlying = type[..^1];
            if (compilation.ClassTypes.ContainsKey(underlying) || compilation.InterfaceTypes.ContainsKey(underlying) ||
                compilation.DelegateTypes.ContainsKey(underlying) || SemanticAnalyzer.TryParseManagedArrayType(underlying, out _, out _) ||
                underlying == "string") return true;
            type = underlying;
        }
        if (SemanticAnalyzer.TryParseInlineArrayType(type, out var inlineElementType, out _))
            return StructContainsManagedReferences(inlineElementType, visiting);
        if (!compilation.StructTypes.TryGetValue(type, out var structure)) return false;
        if (!visiting.Add(type)) return true;
        var containsReferences = structure.Fields.Any(field => StructContainsManagedReferences(field.TypeName, visiting));
        visiting.Remove(type);
        return containsReferences;
    }

    private static string AggregateOwnershipFunctionName(string type, bool retain) =>
        $"luma_aggregate_{(retain ? "retain" : "release")}_{Safe(type)}_{Convert.ToHexString(Encoding.UTF8.GetBytes(type))}";

    private HashSet<string> GetAggregateOwnershipTypes()
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        void Add(string type)
        {
            if (SemanticAnalyzer.TryParseManagedArrayType(type, out var arrayElementType, out _) && result.Add(type))
            {
                Add(arrayElementType);
                return;
            }
            if (type.EndsWith("?", StringComparison.Ordinal) && compilation.StructTypes.ContainsKey(type[..^1]))
            {
                if (result.Add(type)) Add(type[..^1]);
                return;
            }
            if (SemanticAnalyzer.TryParseInlineArrayType(type, out var elementType, out _) && result.Add(type))
            {
                Add(elementType);
                return;
            }
            if (compilation.StructTypes.TryGetValue(type, out var structure) && result.Add(type))
                foreach (var field in structure.Fields) Add(field.TypeName);
        }

        foreach (var type in compilation.StructTypes.Keys) Add(type);
        foreach (var classType in compilation.ClassTypes.Values)
            foreach (var field in classType.Fields) Add(field.TypeName);
        foreach (var field in compilation.StaticFields) Add(field.Declaration.TypeName);
        foreach (var type in compilation.VariableTypes.Values) Add(type);
        foreach (var type in compilation.ManagedArrayAllocations.Values) Add(type);
        foreach (var binding in compilation.ManagedArrayIndexAccesses.Values) Add(binding.ElementType);
        foreach (var type in compilation.ManagedArrayForInElementTypes.Values) Add(type);
        foreach (var method in compilation.Methods)
        {
            Add(method.Declaration.ReturnType);
            foreach (var parameter in method.Declaration.Parameters) Add(parameter.TypeName);
        }
        return result;
    }

    private bool AggregateTypeContainsManagedReferences(string type) =>
        (compilation.StructTypes.ContainsKey(type) || type.EndsWith("?", StringComparison.Ordinal) && compilation.StructTypes.ContainsKey(type[..^1]) ||
         SemanticAnalyzer.TryParseInlineArrayType(type, out _, out _)) && StructContainsManagedReferences(type);

    private void EmitAggregateOwnershipRuntime()
    {
        foreach (var type in GetAggregateOwnershipTypes().Where(AggregateTypeContainsManagedReferences).Order(StringComparer.Ordinal))
        {
            EmitAggregateOwnershipFunction(type, retain: true);
            EmitAggregateOwnershipFunction(type, retain: false);
        }
    }

    private void EmitAggregateOwnershipFunction(string type, bool retain)
    {
        var name = AggregateOwnershipFunctionName(type, retain);
        _output.AppendLine($"define internal void @{name}(ptr %value) {{");
        _output.AppendLine("entry:");
        var temporary = 0;
        EmitAggregateOwnershipOperation(type, "%value", retain, ref temporary, 0);
        _output.AppendLine("  ret void");
        _output.AppendLine("}");
    }

    private void EmitAggregateOwnershipOperation(string type, string address, bool retain, ref int temporary, int depth)
    {
        var suffix = temporary++;
        var baseType = type.EndsWith("?", StringComparison.Ordinal) ? type[..^1] : type;
        string? referenceOperation = null;
        if (baseType == "string") referenceOperation = retain ? "luma_string_retain" : "luma_string_release";
        else if (SemanticAnalyzer.TryParseManagedArrayType(baseType, out _, out _)) referenceOperation = retain ? "luma_array_retain" : "luma_array_release";
        else if (baseType == "object" || compilation.ClassTypes.ContainsKey(baseType) || compilation.InterfaceTypes.ContainsKey(baseType) || compilation.DelegateTypes.ContainsKey(baseType))
            referenceOperation = retain ? "luma_class_retain_dynamic" : "luma_class_release_dynamic";
        if (referenceOperation is not null)
        {
            _output.AppendLine($"  %aggregate.reference.{suffix} = load ptr, ptr {address}");
            _output.AppendLine($"  call void @{referenceOperation}(ptr %aggregate.reference.{suffix})");
            return;
        }

        if (type.EndsWith("?", StringComparison.Ordinal) && compilation.StructTypes.ContainsKey(baseType) &&
            AggregateTypeContainsManagedReferences(baseType))
        {
            var presentAddress = $"%aggregate.present.address.{suffix}";
            var present = $"%aggregate.present.{suffix}";
            var payload = $"%aggregate.payload.{suffix}";
            var bodyLabel = $"aggregate.present.body.{suffix}";
            var doneLabel = $"aggregate.present.done.{suffix}";
            _output.AppendLine($"  {presentAddress} = getelementptr {LlvmType(type)}, ptr {address}, i32 0, i32 0");
            _output.AppendLine($"  {present} = load i1, ptr {presentAddress}");
            _output.AppendLine($"  br i1 {present}, label %{bodyLabel}, label %{doneLabel}");
            _output.AppendLine(bodyLabel + ":");
            _output.AppendLine($"  {payload} = getelementptr {LlvmType(type)}, ptr {address}, i32 0, i32 1");
            EmitAggregateOwnershipOperation(baseType, payload, retain, ref temporary, depth + 1);
            _output.AppendLine($"  br label %{doneLabel}");
            _output.AppendLine(doneLabel + ":");
            return;
        }

        if (SemanticAnalyzer.TryParseInlineArrayType(type, out var elementType, out var count))
        {
            var order = retain ? Enumerable.Range(0, count) : Enumerable.Range(0, count).Reverse();
            foreach (var index in order)
            {
                var elementAddress = $"%aggregate.element.{temporary++}";
                _output.AppendLine($"  {elementAddress} = getelementptr {LlvmType(type)}, ptr {address}, i32 0, i32 {index}");
                EmitAggregateOwnershipOperation(elementType, elementAddress, retain, ref temporary, depth + 1);
            }
            return;
        }

        if (depth > 0 && compilation.StructTypes.ContainsKey(type) && AggregateTypeContainsManagedReferences(type))
        {
            var operation = AggregateOwnershipFunctionName(type, retain);
            _output.AppendLine($"  call void @{operation}(ptr {address})");
            return;
        }

        if (compilation.StructTypes.TryGetValue(type, out var structure))
        {
            var fields = retain ? structure.Fields : structure.Fields.Reverse();
            foreach (var field in fields)
            {
                if (!StructContainsManagedReferences(field.TypeName)) continue;
                var fieldAddress = $"%aggregate.field.{temporary++}";
                _output.AppendLine($"  {fieldAddress} = getelementptr {LlvmType(type)}, ptr {address}, i32 0, i32 {field.Index}");
                EmitAggregateOwnershipOperation(field.TypeName, fieldAddress, retain, ref temporary, depth + 1);
            }
        }
    }

    private IEnumerable<string> BoxableTypeNames() => new[]
    {
        "bool", "char", "int8", "uint8", "int16", "uint16", "int32", "uint32", "int64", "uint64", "int", "uint", "float", "double"
    }.Concat(compilation.EnumUnderlyingTypes.Keys).Concat(BoxableStructTypeNames()).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal);

    private static bool IsPrimitiveStringType(string type) => type is "bool" or "char" or "float" or "double" or
        "int" or "uint" or "int8" or "uint8" or "int16" or "uint16" or "int32" or "uint32" or "int64" or "uint64";

    private static string PrimitiveBoxToStringThunkName(string type) => $"luma_primitive_box_tostring_{Safe(type)}";

    private void EmitPrimitiveBoxToStringThunk(string type)
    {
        _output.AppendLine($"define internal ptr @{PrimitiveBoxToStringThunkName(type)}(ptr %box) {{");
        _output.AppendLine("entry:");
        _output.AppendLine("  %payload.address = getelementptr %luma.box, ptr %box, i32 0, i32 4");
        _output.AppendLine("  %payload = load i64, ptr %payload.address");
        if (type == "bool")
        {
            _output.AppendLine("  %value = trunc i64 %payload to i1");
            _output.AppendLine("  %result = select i1 %value, ptr @luma_string_true, ptr @luma_string_false");
        }
        else if (type == "char")
        {
            _output.AppendLine("  %value = trunc i64 %payload to i16");
            _output.AppendLine("  %result = call ptr @luma_string_from_char(i16 %value)");
        }
        else if (type is "float" or "double")
        {
            if (type == "float")
            {
                _output.AppendLine("  %bits = trunc i64 %payload to i32");
                _output.AppendLine("  %single = bitcast i32 %bits to float");
                _output.AppendLine("  %value = fpext float %single to double");
                _output.AppendLine("  %result = call ptr @luma_string_from_float(double %value, i1 true)");
            }
            else
            {
                _output.AppendLine("  %value = bitcast i64 %payload to double");
                _output.AppendLine("  %result = call ptr @luma_string_from_float(double %value, i1 false)");
            }
        }
        else
        {
            var signed = type is "int" or "int8" or "int16" or "int32" or "int64";
            _output.AppendLine($"  %result = call ptr @luma_string_from_i64(i64 %payload, i1 {(signed ? "true" : "false")})");
        }
        _output.AppendLine("  ret ptr %result");
        _output.AppendLine("}");
    }

    private void EmitBoxRuntime()
    {
        var usesDynamicObjectToString = HasStringSupport() && compilation.ClassToStringCalls.Count > 0;
        var virtualMethods = VirtualDispatchLayout.GetMethods("System.Object", compilation.ClassTypes, compilation.Methods);
        var vtablePointer = virtualMethods.Count == 0 ? "ptr null" :
            $"ptr getelementptr inbounds ([{virtualMethods.Count} x ptr], ptr @luma_vtable_System_Object, i32 0, i32 0)";
        _output.AppendLine("define internal void @luma_box_retain(ptr %input) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %is.null = icmp eq ptr %input, null");
        _output.AppendLine("  br i1 %is.null, label %done, label %check.counted");
        _output.AppendLine("check.counted:");
        _output.AppendLine("  %flags.address = getelementptr %luma.box, ptr %input, i32 0, i32 2");
        _output.AppendLine("  %flags = load atomic i32, ptr %flags.address acquire, align 4");
        _output.AppendLine("  %uncounted.bits = and i32 %flags, 3");
        _output.AppendLine("  %uncounted = icmp ne i32 %uncounted.bits, 0");
        _output.AppendLine("  br i1 %uncounted, label %done, label %retain");
        _output.AppendLine("retain:");
        _output.AppendLine("  %count.address = getelementptr %luma.box, ptr %input, i32 0, i32 1");
        EmitCheckedReferenceCountIncrement("%count.address", "done", "box.retain");
        _output.AppendLine("done:");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");
        _output.AppendLine("define internal void @luma_box_release(ptr %input) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %is.null = icmp eq ptr %input, null");
        _output.AppendLine("  br i1 %is.null, label %done, label %check.counted");
        _output.AppendLine("check.counted:");
        _output.AppendLine("  %flags.address = getelementptr %luma.box, ptr %input, i32 0, i32 2");
        _output.AppendLine("  %flags = load atomic i32, ptr %flags.address acquire, align 4");
        _output.AppendLine("  %uncounted.bits = and i32 %flags, 3");
        _output.AppendLine("  %uncounted = icmp ne i32 %uncounted.bits, 0");
        _output.AppendLine("  br i1 %uncounted, label %done, label %decrement");
        _output.AppendLine("decrement:");
        _output.AppendLine("  %count.address = getelementptr %luma.box, ptr %input, i32 0, i32 1");
        _output.AppendLine("  %weak.address = getelementptr %luma.box, ptr %input, i32 0, i32 3");
        _output.AppendLine("  %weak.control = load atomic ptr, ptr %weak.address acquire, align " + compilation.NativePointerSize);
        _output.AppendLine("  %has.weak.control = icmp ne ptr %weak.control, null");
        _output.AppendLine("  br i1 %has.weak.control, label %weak.decrement, label %normal.decrement");
        _output.AppendLine("normal.decrement:");
        _output.AppendLine("  %old.count = atomicrmw sub ptr %count.address, i32 1 acq_rel");
        _output.AppendLine("  %last.reference = icmp eq i32 %old.count, 1");
        _output.AppendLine("  br i1 %last.reference, label %destroy, label %done");
        _output.AppendLine("weak.decrement:");
        _output.AppendLine("  call void @luma_weak_lock(ptr %weak.control)");
        _output.AppendLine("  %weak.old.count = atomicrmw sub ptr %count.address, i32 1 acq_rel");
        _output.AppendLine("  %weak.last.reference = icmp eq i32 %weak.old.count, 1");
        _output.AppendLine("  br i1 %weak.last.reference, label %weak.destroy, label %weak.not.last");
        _output.AppendLine("weak.not.last:");
        _output.AppendLine("  call void @luma_weak_unlock(ptr %weak.control)");
        _output.AppendLine("  br label %done");
        _output.AppendLine("weak.destroy:");
        _output.AppendLine("  %weak.target.address = getelementptr %luma.weak.control, ptr %weak.control, i32 0, i32 0");
        _output.AppendLine("  store atomic ptr null, ptr %weak.target.address release, align " + compilation.NativePointerSize);
        _output.AppendLine("  %weak.count.address = getelementptr %luma.weak.control, ptr %weak.control, i32 0, i32 2");
        _output.AppendLine("  %weak.handle.count = load atomic i32, ptr %weak.count.address acquire, align 4");
        _output.AppendLine("  %weak.has.handles = icmp ne i32 %weak.handle.count, 0");
        _output.AppendLine("  call void @luma_weak_unlock(ptr %weak.control)");
        _output.AppendLine("  br i1 %weak.has.handles, label %destroy, label %weak.free");
        _output.AppendLine("weak.free:");
        _output.AppendLine("  call void @free(ptr %weak.control)");
        _output.AppendLine("  br label %destroy");
        _output.AppendLine("destroy:");
        _output.AppendLine("  call void @free(ptr %input)");
        _output.AppendLine("  br label %done");
        _output.AppendLine("done:");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");

        foreach (var type in BoxableTypeNames())
        {
            var safe = Safe(type);
            var nameBytes = Encoding.UTF8.GetBytes(type + "\0");
            var byteData = string.Join(", ", nameBytes.Select(value => $"i8 {value}"));
            _output.AppendLine($"@luma_boxname_{safe} = private constant [{nameBytes.Length} x i8] [{byteData}]");
            var namePointer = $"ptr getelementptr inbounds ([{nameBytes.Length} x i8], ptr @luma_boxname_{safe}, i32 0, i32 0)";
            var typeNameStringPointer = "ptr null";
            if (HasStringSupport() && StringLiteralCodec.TryCreate(type, out var typeNameLiteral, out _))
                typeNameStringPointer = $"ptr @{typeNameLiteral.Symbol}";
            var boxVtablePointer = vtablePointer;
            var structToString = compilation.Methods.FirstOrDefault(method => method.IsStructInstanceMethod &&
                QualifiedTypeName(method.Owner) == type && method.Declaration.Name == "ToString" &&
                method.Declaration.ReturnType == "string" && method.Declaration.Parameters.Count == 0 &&
                method.Declaration.Body is not null);
            var primitiveToString = usesDynamicObjectToString && structToString is null && IsPrimitiveStringType(type);
            if (primitiveToString)
                EmitPrimitiveBoxToStringThunk(type);
            if ((structToString is not null || primitiveToString) && virtualMethods.Count > 0)
            {
                var entries = virtualMethods.Select(method => method.IsBuiltinObjectToString
                    ? primitiveToString
                        ? $"ptr @{PrimitiveBoxToStringThunkName(type)}"
                        : $"ptr @{StructToStringThunkName(type)}"
                    : method.Declaration.Body is null ? "ptr null" : $"ptr @{FunctionName(method, compilation.Methods)}");
                _output.AppendLine($"@luma_struct_vtable_{safe} = private constant [{virtualMethods.Count} x ptr] [{string.Join(", ", entries)}]");
                boxVtablePointer = $"ptr getelementptr inbounds ([{virtualMethods.Count} x ptr], ptr @luma_struct_vtable_{safe}, i32 0, i32 0)";
            }
            var interfaceEntries = compilation.InterfaceDispatchTables.GetValueOrDefault(type) ?? [];
            var interfaceTablePointer = "ptr null";
            if (compilation.StructTypes.ContainsKey(type) && compilation.InterfaceMethodSlots.Count > 0)
            {
                var entries = compilation.InterfaceMethodSlots.Select(slot =>
                    slot.Index < interfaceEntries.Count && interfaceEntries[slot.Index]?.Declaration.Body is not null
                        ? $"ptr @{StructInterfaceThunkName(type, slot.Index)}"
                        : "ptr null");
                _output.AppendLine($"@luma_struct_itable_{safe} = private constant [{compilation.InterfaceMethodSlots.Count} x ptr] [{string.Join(", ", entries)}]");
                interfaceTablePointer = $"ptr getelementptr inbounds ([{compilation.InterfaceMethodSlots.Count} x ptr], ptr @luma_struct_itable_{safe}, i32 0, i32 0)";
            }
            var implementedInterfaces = compilation.InterfaceConversions.Where(conversion => conversion.FromType == type &&
                    compilation.InterfaceTypes.ContainsKey(conversion.ToType))
                .Select(conversion => conversion.ToType).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
            var interfaceIdPointer = "ptr null";
            if (implementedInterfaces.Length > 0)
            {
                var ids = string.Join(", ", implementedInterfaces.Select(interfaceName => $"ptr @luma_interfacedesc_{Safe(interfaceName)}"));
                _output.AppendLine($"@luma_box_interfaceids_{safe} = private constant [{implementedInterfaces.Length} x ptr] [{ids}]");
                interfaceIdPointer = $"ptr getelementptr inbounds ([{implementedInterfaces.Length} x ptr], ptr @luma_box_interfaceids_{safe}, i32 0, i32 0)";
            }
            var releaseFunction = "luma_box_release";
            if (compilation.StructTypes.ContainsKey(type) && StructContainsManagedReferences(type))
            {
                releaseFunction = StructBoxReleaseFunctionName(type);
                EmitStructBoxReleaseFunction(type);
            }
            _output.AppendLine($"@luma_boxdesc_{safe} = private constant %luma.typedesc {{ {namePointer}, ptr @luma_classdesc_System_Object, {boxVtablePointer}, i32 {virtualMethods.Count}, ptr @{releaseFunction}, ptr @luma_box_retain, {interfaceTablePointer}, {interfaceIdPointer}, i32 {implementedInterfaces.Length}, {typeNameStringPointer} }}");
        }
    }

    private static string StructBoxReleaseFunctionName(string type) => $"luma_structbox_release_{Safe(type)}";

    private void EmitStructBoxReleaseFunction(string type)
    {
        var boxType = $"%luma.structbox.{Safe(type)}";
        _output.AppendLine($"define internal void @{StructBoxReleaseFunctionName(type)}(ptr %input) {{");
        _output.AppendLine("entry:");
        _output.AppendLine("  %is.null = icmp eq ptr %input, null");
        _output.AppendLine("  br i1 %is.null, label %done, label %check.counted");
        _output.AppendLine("check.counted:");
        _output.AppendLine($"  %flags.address = getelementptr {boxType}, ptr %input, i32 0, i32 2");
        _output.AppendLine("  %flags = load atomic i32, ptr %flags.address acquire, align 4");
        _output.AppendLine("  %uncounted.bits = and i32 %flags, 3");
        _output.AppendLine("  %uncounted = icmp ne i32 %uncounted.bits, 0");
        _output.AppendLine("  br i1 %uncounted, label %done, label %decrement");
        _output.AppendLine("decrement:");
        _output.AppendLine($"  %count.address = getelementptr {boxType}, ptr %input, i32 0, i32 1");
        _output.AppendLine($"  %weak.address = getelementptr {boxType}, ptr %input, i32 0, i32 3");
        _output.AppendLine("  %weak.control = load atomic ptr, ptr %weak.address acquire, align " + compilation.NativePointerSize);
        _output.AppendLine("  %has.weak.control = icmp ne ptr %weak.control, null");
        _output.AppendLine("  br i1 %has.weak.control, label %weak.decrement, label %normal.decrement");
        _output.AppendLine("normal.decrement:");
        _output.AppendLine("  %old.count = atomicrmw sub ptr %count.address, i32 1 acq_rel");
        _output.AppendLine("  %last.reference = icmp eq i32 %old.count, 1");
        _output.AppendLine("  br i1 %last.reference, label %destroy, label %done");
        _output.AppendLine("weak.decrement:");
        _output.AppendLine("  call void @luma_weak_lock(ptr %weak.control)");
        _output.AppendLine("  %weak.old.count = atomicrmw sub ptr %count.address, i32 1 acq_rel");
        _output.AppendLine("  %weak.last.reference = icmp eq i32 %weak.old.count, 1");
        _output.AppendLine("  br i1 %weak.last.reference, label %weak.destroy, label %weak.not.last");
        _output.AppendLine("weak.not.last:");
        _output.AppendLine("  call void @luma_weak_unlock(ptr %weak.control)");
        _output.AppendLine("  br label %done");
        _output.AppendLine("weak.destroy:");
        _output.AppendLine("  %weak.target.address = getelementptr %luma.weak.control, ptr %weak.control, i32 0, i32 0");
        _output.AppendLine("  store atomic ptr null, ptr %weak.target.address release, align " + compilation.NativePointerSize);
        _output.AppendLine("  %weak.count.address = getelementptr %luma.weak.control, ptr %weak.control, i32 0, i32 2");
        _output.AppendLine("  %weak.handle.count = load atomic i32, ptr %weak.count.address acquire, align 4");
        _output.AppendLine("  %weak.has.handles = icmp ne i32 %weak.handle.count, 0");
        _output.AppendLine("  call void @luma_weak_unlock(ptr %weak.control)");
        _output.AppendLine("  br i1 %weak.has.handles, label %destroy, label %weak.free");
        _output.AppendLine("weak.free:");
        _output.AppendLine("  call void @free(ptr %weak.control)");
        _output.AppendLine("  br label %destroy");
        _output.AppendLine("destroy:");
        _output.AppendLine($"  %payload.address = getelementptr {boxType}, ptr %input, i32 0, i32 4");
        _output.AppendLine($"  call void @{AggregateOwnershipFunctionName(type, retain: false)}(ptr %payload.address)");
        _output.AppendLine("  call void @free(ptr %input)");
        _output.AppendLine("  br label %done");
        _output.AppendLine("done:");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");
    }
    private static string StructToStringThunkName(string type) => $"luma_struct_tostring_{Safe(type)}";

    private static string StructInterfaceThunkName(string type, int slot) => $"luma_struct_iface_{Safe(type)}_{slot}";

    private void EmitStructInterfaceThunks()
    {
        foreach (var type in compilation.StructTypes.Keys.Order(StringComparer.Ordinal))
        {
            if (!compilation.InterfaceDispatchTables.TryGetValue(type, out var entries)) continue;
            foreach (var slot in compilation.InterfaceMethodSlots)
            {
                if (slot.Index >= entries.Count || entries[slot.Index] is not { Declaration.Body: not null } implementation) continue;
                var arguments = implementation.Declaration.Parameters.Select((parameter, index) =>
                    $"{LlvmParameterType(parameter)} %arg{index}").ToArray();
                var signatureArguments = string.Join(", ", new[] { "ptr %box" }.Concat(arguments));
                var callArguments = string.Join(", ", arguments.Select((_, index) =>
                    $"{LlvmParameterType(implementation.Declaration.Parameters[index])} %arg{index}"));
                var resultType = LlvmType(implementation.Declaration.ReturnType);
                _output.AppendLine($"define internal {resultType} @{StructInterfaceThunkName(type, slot.Index)}({signatureArguments}) {{");
                _output.AppendLine("entry:");
                _output.AppendLine($"  %payload = getelementptr %luma.structbox.{Safe(type)}, ptr %box, i32 0, i32 4");
                var allArguments = callArguments.Length == 0 ? "ptr %payload" : $"ptr %payload, {callArguments}";
                var call = $"call {resultType} @{FunctionName(implementation, compilation.Methods)}({allArguments})";
                if (implementation.Declaration.ReturnType == "void")
                {
                    _output.AppendLine($"  {call}");
                    _output.AppendLine("  ret void");
                }
                else
                {
                    _output.AppendLine($"  %result = {call}");
                    _output.AppendLine($"  ret {resultType} %result");
                }
                _output.AppendLine("}");
            }
        }
        foreach (var type in compilation.StructTypes.Keys.Order(StringComparer.Ordinal))
        {
            var implementation = compilation.Methods.FirstOrDefault(method => method.IsStructInstanceMethod &&
                QualifiedTypeName(method.Owner) == type && method.Declaration.Name == "ToString" &&
                method.Declaration.ReturnType == "string" && method.Declaration.Parameters.Count == 0 &&
                method.Declaration.Body is not null);
            if (implementation is null) continue;
            _output.AppendLine($"define internal ptr @{StructToStringThunkName(type)}(ptr %box) {{");
            _output.AppendLine("entry:");
            _output.AppendLine($"  %payload = getelementptr %luma.structbox.{Safe(type)}, ptr %box, i32 0, i32 4");
            _output.AppendLine($"  %result = call ptr @{FunctionName(implementation, compilation.Methods)}(ptr %payload)");
            _output.AppendLine("  ret ptr %result");
            _output.AppendLine("}");
        }
    }

    private void EmitManagedDelegateRuntime()
    {
        foreach (var name in compilation.DelegateTypes.Keys.Order(StringComparer.Ordinal))
        {
            var safe = Safe(name);
            var nameBytes = Encoding.UTF8.GetBytes(name + "\0");
            var nameData = string.Join(", ", nameBytes.Select(value => $"i8 {value}"));
            _output.AppendLine($"@luma_delegatename_{safe} = private constant [{nameBytes.Length} x i8] [{nameData}]");
            var namePointer = $"ptr getelementptr inbounds ([{nameBytes.Length} x i8], ptr @luma_delegatename_{safe}, i32 0, i32 0)";
            var basePointer = HasManagedClassSupportFor("System.Delegate") ? "ptr @luma_classdesc_System_Delegate" : "ptr null";
            var virtualMethods = VirtualDispatchLayout.GetMethods("System.Delegate", compilation.ClassTypes, compilation.Methods);
            var vtablePointer = "ptr null";
            if (virtualMethods.Count > 0)
            {
                var entries = string.Join(", ", virtualMethods.Select(method => method.IsBuiltinObjectToString
                    ? "ptr @luma_object_tostring"
                    : method.Declaration.Body is null ? "ptr null" : $"ptr @{FunctionName(method, compilation.Methods)}"));
                _output.AppendLine($"@luma_delegate_vtable_{safe} = private constant [{virtualMethods.Count} x ptr] [{entries}]");
                vtablePointer = $"ptr getelementptr inbounds ([{virtualMethods.Count} x ptr], ptr @luma_delegate_vtable_{safe}, i32 0, i32 0)";
            }
            _output.AppendLine($"@luma_classdesc_delegate_{safe} = private constant %luma.typedesc {{ {namePointer}, {basePointer}, {vtablePointer}, i32 {virtualMethods.Count}, ptr @luma_delegate_{safe}_release, ptr @luma_delegate_{safe}_retain, ptr null, ptr null, i32 0, ptr null }}");
            _output.AppendLine($"define internal void @luma_delegate_{safe}_retain(ptr %input) {{");
            _output.AppendLine("entry:");
            _output.AppendLine("  %is.null = icmp eq ptr %input, null");
            _output.AppendLine("  br i1 %is.null, label %done, label %check.counted");
            _output.AppendLine("check.counted:");
            _output.AppendLine($"  %flags.address = getelementptr %delegate.{safe}, ptr %input, i32 0, i32 2");
            _output.AppendLine("  %flags = load atomic i32, ptr %flags.address acquire, align 4");
            _output.AppendLine("  %uncounted.bits = and i32 %flags, 3");
            _output.AppendLine("  %uncounted = icmp ne i32 %uncounted.bits, 0");
            _output.AppendLine("  br i1 %uncounted, label %done, label %retain");
            _output.AppendLine("retain:");
            _output.AppendLine($"  %count.address = getelementptr %delegate.{safe}, ptr %input, i32 0, i32 1");
            EmitCheckedReferenceCountIncrement("%count.address", "done", "delegate.retain");
            _output.AppendLine("done:");
            _output.AppendLine("  ret void");
            _output.AppendLine("}");
            _output.AppendLine($"define internal void @luma_delegate_{safe}_release(ptr %input) {{");
            _output.AppendLine("entry:");
            _output.AppendLine("  %is.null = icmp eq ptr %input, null");
            _output.AppendLine("  br i1 %is.null, label %done, label %check.counted");
            _output.AppendLine("check.counted:");
            _output.AppendLine($"  %flags.address = getelementptr %delegate.{safe}, ptr %input, i32 0, i32 2");
            _output.AppendLine("  %flags = load atomic i32, ptr %flags.address acquire, align 4");
            _output.AppendLine("  %uncounted.bits = and i32 %flags, 3");
            _output.AppendLine("  %uncounted = icmp ne i32 %uncounted.bits, 0");
            _output.AppendLine("  br i1 %uncounted, label %done, label %decrement");
            _output.AppendLine("decrement:");
            _output.AppendLine($"  %count.address = getelementptr %delegate.{safe}, ptr %input, i32 0, i32 1");
            _output.AppendLine($"  %weak.address = getelementptr %delegate.{safe}, ptr %input, i32 0, i32 3");
            _output.AppendLine("  %weak.control = load atomic ptr, ptr %weak.address acquire, align " + compilation.NativePointerSize);
            _output.AppendLine("  %has.weak.control = icmp ne ptr %weak.control, null");
            _output.AppendLine("  br i1 %has.weak.control, label %weak.decrement, label %normal.decrement");
            _output.AppendLine("normal.decrement:");
            _output.AppendLine("  %old.count = atomicrmw sub ptr %count.address, i32 1 acq_rel");
            _output.AppendLine("  %last.reference = icmp eq i32 %old.count, 1");
            _output.AppendLine("  br i1 %last.reference, label %destroy, label %done");
            _output.AppendLine("weak.decrement:");
            _output.AppendLine("  call void @luma_weak_lock(ptr %weak.control)");
            _output.AppendLine("  %weak.old.count = atomicrmw sub ptr %count.address, i32 1 acq_rel");
            _output.AppendLine("  %weak.last.reference = icmp eq i32 %weak.old.count, 1");
            _output.AppendLine("  br i1 %weak.last.reference, label %weak.destroy, label %weak.not.last");
            _output.AppendLine("weak.not.last:");
            _output.AppendLine("  call void @luma_weak_unlock(ptr %weak.control)");
            _output.AppendLine("  br label %done");
            _output.AppendLine("weak.destroy:");
            _output.AppendLine("  %weak.target.address = getelementptr %luma.weak.control, ptr %weak.control, i32 0, i32 0");
            _output.AppendLine("  store atomic ptr null, ptr %weak.target.address release, align " + compilation.NativePointerSize);
            _output.AppendLine("  %weak.count.address = getelementptr %luma.weak.control, ptr %weak.control, i32 0, i32 2");
            _output.AppendLine("  %weak.handle.count = load atomic i32, ptr %weak.count.address acquire, align 4");
            _output.AppendLine("  %weak.has.handles = icmp ne i32 %weak.handle.count, 0");
            _output.AppendLine("  call void @luma_weak_unlock(ptr %weak.control)");
            _output.AppendLine("  br i1 %weak.has.handles, label %destroy, label %weak.free");
            _output.AppendLine("weak.free:");
            _output.AppendLine("  call void @free(ptr %weak.control)");
            _output.AppendLine("  br label %destroy");
            _output.AppendLine("destroy:");
            _output.AppendLine($"  %target.address = getelementptr %delegate.{safe}, ptr %input, i32 0, i32 5");
            _output.AppendLine("  %target = load ptr, ptr %target.address");
            _output.AppendLine("  call void @luma_class_release_dynamic(ptr %target)");
            _output.AppendLine("  call void @free(ptr %input)");
            _output.AppendLine("  br label %done");
            _output.AppendLine("done:");
            _output.AppendLine("  ret void");
            _output.AppendLine("}");
        }
    }

    private void EmitRuntimeExceptionInstance(string typeName, string instanceName)
    {
        if (!compilation.ClassTypes.TryGetValue(typeName, out var type))
            throw new InvalidOperationException($"The compiler-known {typeName} type is unavailable.");
        var fields = type.Fields.OrderBy(field => field.Index).Select(field =>
        {
            var value = field.TypeName switch
            {
                "string" => "@luma_string_empty",
                "string?" => "null",
                _ when compilation.ClassTypes.ContainsKey(field.TypeName) ||
                    field.TypeName.EndsWith("?", StringComparison.Ordinal) && compilation.ClassTypes.ContainsKey(field.TypeName[..^1]) ||
                    SemanticAnalyzer.TryParseManagedArrayType(field.TypeName, out _, out _) => "null",
                _ => "zeroinitializer"
            };
            return $"{LlvmType(field.TypeName)} {value}";
        });
        var suffix = type.Fields.Count == 0 ? "" : ", " + string.Join(", ", fields);
        _output.AppendLine($"@{instanceName} = internal global %class.{Safe(typeName)} {{ ptr @luma_classdesc_{Safe(typeName)}, i32 -1, i32 1, ptr null{suffix} }}");
    }

    private bool HasManagedClassSupportFor(string typeName) => GetManagedClassTypes().Contains(typeName);

    private IEnumerable<string> GetClassBaseChain(string typeName)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var current = typeName;
        while (compilation.ClassTypes.TryGetValue(current, out var classType) && seen.Add(current))
        {
            yield return current;
            if (classType.BaseTypeName is null) yield break;
            current = classType.BaseTypeName;
        }
    }

    private IReadOnlyDictionary<string, int> GetFinalizerCompletionBits()
    {
        var finalizerOwners = compilation.Methods.Where(method => method.IsClassFinalizer)
            .Select(method => VirtualDispatchLayout.QualifiedTypeName(method.Owner)).ToHashSet(StringComparer.Ordinal);
        var indices = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var className in compilation.ClassTypes.Keys.Order(StringComparer.Ordinal))
        {
            var ordinal = 0;
            foreach (var owner in GetClassBaseChain(className).Reverse())
                if (finalizerOwners.Contains(owner))
                {
                    indices.TryAdd(owner, ordinal);
                    ordinal++;
                }
        }
        return indices;
    }

    private sealed class FunctionEmitter(
        MethodSymbol method,
        IReadOnlyList<MethodSymbol> methods,
        IReadOnlyDictionary<string, string> typeInitializationFunctions,
        IReadOnlyDictionary<VariableStatement, string> variableTypes,
        IReadOnlyDictionary<ForInStatement, string> forInTypes,
        IReadOnlyDictionary<ForInStatement, Statement> loweredForInLoops,
        IReadOnlySet<VariableStatement> scopeBoundVariables,
        IReadOnlyDictionary<CallExpression, MethodSymbol> callTargets,
        IReadOnlyDictionary<CallExpression, string> enumToStringCalls,
        IReadOnlyDictionary<CallExpression, ClassToStringBinding> classToStringCalls,
        IReadOnlyDictionary<CallExpression, string> structToStringCalls,
        IReadOnlyDictionary<CallExpression, int> virtualCallSlots,
        IReadOnlyDictionary<CallExpression, int> interfaceCallSlots,
        IReadOnlyDictionary<Expression, DelegateMethodGroupBinding> delegateMethodGroups,
        IReadOnlyDictionary<MethodSymbol, ClosureEnvironmentBinding> closureEnvironments,
        IReadOnlyDictionary<ForStatement, ClosureEnvironmentBinding> forHeaderClosureEnvironments,
        IReadOnlyDictionary<ForInStatement, ClosureEnvironmentBinding> forInClosureEnvironments,
        IReadOnlyDictionary<BlockStatement, ClosureEnvironmentBinding> blockClosureEnvironments,
        IReadOnlyDictionary<LambdaExpression, ClosureEnvironmentBinding> compositeClosureEnvironments,
        IReadOnlyDictionary<CallExpression, TypeDeclaration> delegateInvocations,
        IReadOnlyDictionary<string, TypeDeclaration> delegateTypes,
        IReadOnlyDictionary<CallExpression, BoundCallArguments> callArguments,
        IReadOnlyDictionary<MethodSymbol, ConstructorInitializerBinding> constructorInitializers,
        IReadOnlyDictionary<Expression, MethodSymbol> operatorTargets,
        IReadOnlyDictionary<(string SourceType, string TargetType), MethodSymbol> userConversionOperators,
        IReadOnlyDictionary<DefaultValueExpression, string> defaultTypes,
        IReadOnlyDictionary<MemberAccessExpression, ConstantFieldBinding> constantFields,
        IReadOnlyDictionary<NameExpression, ConstantFieldBinding> unqualifiedConstantFields,
        IReadOnlyDictionary<MemberAccessExpression, StaticFieldBinding> staticFields,
        IReadOnlyDictionary<MemberAccessExpression, CallExpression> propertyGetterCalls,
        IReadOnlyDictionary<NameExpression, CallExpression> propertyGetterNameCalls,
        IReadOnlyDictionary<BinaryExpression, CallExpression> propertySetterCalls,
        IReadOnlyDictionary<MultiAssignmentStatement, IReadOnlyDictionary<int, BinaryExpression>> multiAssignmentAccessorAssignments,
        IReadOnlyDictionary<IndexExpression, CallExpression> indexerGetterCalls,
        IReadOnlyDictionary<NameExpression, StaticFieldBinding> unqualifiedStaticFields,
        IReadOnlyDictionary<MemberAccessExpression, StructFieldBinding> structFields,
        IReadOnlyDictionary<NameExpression, StructFieldBinding> unqualifiedStructFields,
        IReadOnlyDictionary<MemberAccessExpression, StructFieldBinding> classFields,
        IReadOnlyDictionary<NameExpression, StructFieldBinding> unqualifiedClassFields,
        IReadOnlyDictionary<CallExpression, string> structConstructors,
        IReadOnlyDictionary<AllocationExpression, ClassAllocationBinding> classAllocations,
        IReadOnlyDictionary<AllocationExpression, GenericConstructionBinding> genericConstructions,
        IReadOnlyDictionary<string, StructTypeSymbol> structTypes,
        IReadOnlyDictionary<string, ClassTypeSymbol> classTypes,
        IReadOnlyDictionary<string, TypeDeclaration> interfaceTypes,
        IReadOnlySet<(string FromType, string ToType)> interfaceConversions,
        IReadOnlySet<string> managedClassTypes,
        IReadOnlyDictionary<string, string> enumTypes,
        int nativePointerSize,
        IReadOnlyDictionary<CastExpression, string> castTypes,
        IReadOnlyDictionary<BinaryExpression, string> typeOperatorTargets,
        IReadOnlyDictionary<ConditionalExpression, string> conditionalExpressionTypes,
        IReadOnlyDictionary<SwitchLabel, string> switchTypePatterns,
        IReadOnlyDictionary<SizeOfExpression, string> sizeOfTypes,
        IReadOnlyDictionary<LiteralExpression, StringLiteralInfo> stringLiterals,
        IReadOnlyDictionary<MemberAccessExpression, bool> stringLengthAccesses,
        IReadOnlyDictionary<IndexExpression, bool> stringIndexAccesses,
        IReadOnlyDictionary<AllocationExpression, string> managedArrayAllocations,
        IReadOnlyDictionary<IndexExpression, ManagedArrayIndexBinding> managedArrayIndexAccesses,
        IReadOnlyDictionary<MemberAccessExpression, bool> managedArrayLengthAccesses,
        IReadOnlyDictionary<MemberAccessExpression, int> managedArrayRankAccesses,
        IReadOnlyDictionary<MemberAccessExpression, string> managedArrayPointerAccesses,
        IReadOnlyDictionary<MemberAccessExpression, string> genericSpanEmptyAccesses,
        IReadOnlyDictionary<CallExpression, int> managedArrayGetLengthCalls,
        IReadOnlyDictionary<ForInStatement, string> managedArrayForInElementTypes,
        IReadOnlyDictionary<CatchClause, string> exceptionCatchTypes,
        IReadOnlyDictionary<ThrowStatement, string> throwTypes,
        IReadOnlyDictionary<TryStatement, string> exceptionCatchTableNames,
        bool usesExceptionHandling,
        CancellationToken cancellationToken)
    {
        private readonly StringBuilder _ir = new();
        private readonly List<Dictionary<string, Local>> _scopes = [new(StringComparer.Ordinal)];
        private readonly List<List<string>> _scopeStringPointers = [[]];
        private readonly List<List<string>> _scopeArrayPointers = [[]];
        private readonly List<List<(string Pointer, string Type)>> _scopeClassPointers = [[]];
        private readonly List<List<(string Pointer, string Type)>> _scopeAggregatePointers = [[]];
        private readonly Dictionary<string, string> _closureEnvironmentSlots = new(StringComparer.Ordinal);
        private readonly List<ClosureEnvironmentBinding> _activeForClosureEnvironments = [];
        private readonly List<ClosureEnvironmentBinding> _activeBlockClosureEnvironments = [];
        private readonly List<List<(string Value, string Type, bool IsArray)>> _scopeAllocations = [[]];
        private readonly List<(string Value, string Type, bool IsArray)> _statementScopeTemps = [];
        private readonly HashSet<string> _scopeBoundArrayValues = new(StringComparer.Ordinal);
        private readonly HashSet<string> _scopeBoundClassValues = new(StringComparer.Ordinal);
        private bool _isScopeVariableInitializer;
        private readonly HashSet<string> _ownedStringValues = new(StringComparer.Ordinal);
        private readonly HashSet<string> _ownedArrayValues = new(StringComparer.Ordinal);
        private readonly HashSet<(string Value, string Type)> _ownedClassValues = [];
        private readonly HashSet<(string Value, string Type)> _ownedAggregateValues = [];
        private sealed class TypePatternBindingState(string type, string address, string sourceAddress, string retainedFlagAddress)
        {
            public string Type { get; } = type;
            public string Address { get; } = address;
            public string SourceAddress { get; } = sourceAddress;
            public string RetainedFlagAddress { get; } = retainedFlagAddress;
            public string? SourceType { get; set; }
            public string? RetainedClassType { get; set; }
            public bool RetainedString { get; set; }
            public bool Initialized { get; set; }
        }
        private readonly Dictionary<BinaryExpression, TypePatternBindingState> _activeTypePatternBindings = new(ReferenceEqualityComparer.Instance);
        private sealed record LoopTarget(string Break, string Continue, string? Label, int BreakScopeDepth, int ContinueScopeDepth,
            int BreakHandlerDepth, int ContinueHandlerDepth, int BreakFinallyDepth, int ContinueFinallyDepth, int BreakTryDepth, int ContinueTryDepth);
        private sealed record BreakTarget(string Break, string? Label, int ScopeDepth, int HandlerDepth, int FinallyDepth, int TryDepth);
        private readonly Stack<LoopTarget> _loops = new();
        private readonly Stack<BreakTarget> _breakTargets = new();
        private readonly List<ExceptionRegion> _activeExceptionHandlers = [];
        private readonly List<TryFrame> _activeTries = [];
        private readonly List<FinallyContext> _activeFinalizers = [];
        private readonly Stack<string> _currentCatchExceptions = new();
        private readonly List<SavedPropagation> _savedPropagations = [];
        private readonly HashSet<BlockStatement> _preallocatedBlockClosureScopes = new(ReferenceEqualityComparer.Instance);
        private int _temp, _label;
        private string _block = "entry";
        private bool _terminated;
        private bool _checkedContext = true;
        private string NewTemp() => $"%t{_temp++}";
        private string NewLabel(string stem) => $"{stem}.{_label++}";
        private void Label(string label) { _block = label; _terminated = false; _ir.AppendLine($"{label}:"); }
        private void Emit(string instruction) { if (!_terminated) _ir.AppendLine($"  {instruction}"); }
        private void Branch(string label) { Emit($"br label %{label}"); _terminated = true; }

        private sealed record ExceptionRegion(TryStatement Statement, string Node, string PreviousNode, string DispatchLabel, int OuterScopeDepth);
        private sealed record TryFrame(TryStatement Statement, int Depth, int OuterScopeDepth, ExceptionRegion? Handler);
        private sealed record FinallyContext(TryFrame Frame, BlockStatement Body);
        private sealed record SavedPropagation(string ExceptionSlot, string HandlerSlot, int HandlerDepth);
        public string Emit(BlockStatement body)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Label("entry");
            foreach (var environment in closureEnvironments.Values
                         .Concat(forHeaderClosureEnvironments.Values)
                         .Concat(forInClosureEnvironments.Values)
                         .Concat(blockClosureEnvironments.Values)
                         .Concat(compositeClosureEnvironments.Values)
                         .Where(environment => ReferenceEquals(environment.EnclosingMethod, method))
                         .DistinctBy(environment => environment.TypeName, StringComparer.Ordinal))
            {
                var slot = NewTemp();
                Emit($"{slot} = alloca ptr");
                Emit($"store ptr null, ptr {slot}");
                _closureEnvironmentSlots[environment.TypeName] = slot;
            }
            if (closureEnvironments.TryGetValue(method, out var closureEnvironment))
            {
                _ = AllocateClosureEnvironment(closureEnvironment);
                if (closureEnvironment.CapturedFieldIndices.TryGetValue("$this", out var capturedThisIndex))
                {
                    var thisField = classTypes[closureEnvironment.TypeName].Fields[capturedThisIndex];
                    var thisPointer = EmitClosureFieldAddress(closureEnvironment, capturedThisIndex);
                    if (structTypes.ContainsKey(thisField.TypeName))
                    {
                        var capturedValue = NewTemp();
                        Emit($"{capturedValue} = load {LlvmType(thisField.TypeName)}, ptr %this");
                        var value = (capturedValue, thisField.TypeName);
                        if (IsManagedAggregateType(thisField.TypeName)) StoreAggregateInitial(thisPointer, value, thisField.TypeName);
                        else Emit($"store {LlvmType(thisField.TypeName)} {capturedValue}, ptr {thisPointer}");
                    }
                    else StoreClassInitial(thisPointer, ("%this", thisField.TypeName), thisField.TypeName);
                }
                Declare("$luma.closure.environment", closureEnvironment.TypeName, _closureEnvironmentSlots[closureEnvironment.TypeName]);
            }
            if (method.IsStructConstructor)
            {
                var structType = LlvmType(method.Declaration.ReturnType);
                Emit($"%this = alloca {structType}");
                Emit($"store {structType} zeroinitializer, ptr %this");
                InitializeStructFields();
            }
            for (var i = 0; i < method.Declaration.Parameters.Count; i++)
            {
                var p = method.Declaration.Parameters[i];
                if (closureEnvironments.TryGetValue(method, out closureEnvironment) && closureEnvironment.CapturedFieldIndices.TryGetValue(p.Name, out var capturedParameterIndex))
                {
                    var pointer = EmitClosureFieldAddress(closureEnvironment, capturedParameterIndex);
                    var value = ($"%arg{i}", p.TypeName);
                    if (closureEnvironment.BorrowedCaptureFieldIndices?.Contains(capturedParameterIndex) == true)
                        Emit($"store {LlvmType(p.TypeName)} %arg{i}, ptr {pointer}");
                    else if (IsStringType(p.TypeName)) StoreStringInitial(pointer, value);
                    else if (SemanticAnalyzer.TryParseManagedArrayType(p.TypeName, out _, out _)) StoreArrayInitial(pointer, value, p.TypeName);
                    else if (GetClassReferenceType(p.TypeName) is { } parameterClass) StoreClassInitial(pointer, value, parameterClass);
                    else if (IsManagedAggregateType(p.TypeName)) StoreAggregateInitial(pointer, value, p.TypeName);
                    else Emit($"store {LlvmType(p.TypeName)} %arg{i}, ptr {pointer}");
                    Declare(p.Name, p.TypeName, pointer, ownsString: false, ownsArray: false, ownsClass: false, ownsAggregate: false);
                }
                else if (p.PassingMode is "ref" or "out") Declare(p.Name, p.TypeName, $"%arg{i}", ownsString: false, ownsArray: false, ownsClass: false);
                else
                {
                    var ptr = $"%p{i}"; Emit($"{ptr} = alloca {LlvmType(p.TypeName)}"); Emit($"store {LlvmType(p.TypeName)} %arg{i}, ptr {ptr}");
                    if (IsStringType(p.TypeName) && p.PassingMode != "in" && !p.IsReadOnly && !p.IsScoped) Emit($"call void @luma_string_retain(ptr %arg{i})");
                    if (SemanticAnalyzer.TryParseManagedArrayType(p.TypeName, out _, out _) && p.PassingMode != "in" && !p.IsReadOnly && !p.IsScoped) Emit($"call void @luma_array_retain(ptr %arg{i})");
                    if (GetClassReferenceType(p.TypeName) is { } parameterClass && p.PassingMode != "in" && !p.IsReadOnly && !p.IsScoped) RetainClassReference("%arg" + i, parameterClass);
                    Declare(p.Name, p.TypeName, ptr, ownsString: p.PassingMode != "in" && !p.IsReadOnly && !p.IsScoped,
                        ownsArray: p.PassingMode != "in" && !p.IsReadOnly && !p.IsScoped, ownsClass: p.PassingMode != "in" && !p.IsReadOnly && !p.IsScoped, ownsAggregate: false);
                }
            }
            if (method.IsClassConstructor)
            {
                var constructedName = QualifiedTypeName(method.Owner);
                var constructedSafeName = Safe(constructedName);
                string? descriptorAddress = null;
                if (managedClassTypes.Contains(constructedName))
                {
                    descriptorAddress = NewTemp();
                    Emit($"{descriptorAddress} = getelementptr %class.{constructedSafeName}, ptr %this, i32 0, i32 0");
                    Emit($"store ptr @luma_classdesc_{constructedSafeName}, ptr {descriptorAddress}");
                }
                if (constructorInitializers.TryGetValue(method, out var initializer) && initializer.DelegatesToThis)
                {
                    _ = EmitCall(initializer.Call, "%this", classConstructorCompletes: false);
                }
                else
                {
                    InitializeClassFields();
                    if (constructorInitializers.TryGetValue(method, out initializer))
                    {
                        _ = EmitCall(initializer.Call, "%this", classConstructorCompletes: true, checkPendingException: false);
                        if (descriptorAddress is not null)
                            Emit($"store ptr @luma_classdesc_{constructedSafeName}, ptr {descriptorAddress}");
                        EmitCheckPendingException();
                    }
                }
            }
            EmitStatement(body);
            if (!_terminated)
            {
                if (method.IsStructConstructor)
                {
                    ReleaseAllOwnedStrings(); ReleaseAllOwnedArrays(); ReleaseAllOwnedClasses(); ReleaseAllOwnedAggregates();
                    ReleaseAllScopes();
                    var structType = LlvmType(method.Declaration.ReturnType);
                    var value = NewTemp(); Emit($"{value} = load {structType}, ptr %this"); Emit($"ret {structType} {value}");
                }
                else
                {
                    ReleaseAllOwnedStrings(); ReleaseAllOwnedArrays(); ReleaseAllOwnedClasses(); ReleaseAllOwnedAggregates();
                    ReleaseAllScopes();
                    MarkClassConstructorCompleted();
                    Emit(method.Declaration.ReturnType == "void" ? "ret void" : $"ret {LlvmType(method.Declaration.ReturnType)} 0");
                }
            }
            return _ir.ToString();
        }
        private void EmitStatement(Statement statement)
        {
            cancellationToken.ThrowIfCancellationRequested();
            switch (statement)
            {
                case BlockStatement block:
                    Push();
                    var hasBlockClosure = blockClosureEnvironments.TryGetValue(block, out var blockClosureEnvironment);
                    var closureWasPreallocated = hasBlockClosure && _preallocatedBlockClosureScopes.Remove(block);
                    if (hasBlockClosure && !closureWasPreallocated)
                    {
                        _ = AllocateClosureEnvironment(blockClosureEnvironment!);
                        _scopeClassPointers[^1].Add((_closureEnvironmentSlots[blockClosureEnvironment!.TypeName], blockClosureEnvironment.TypeName));
                        _activeBlockClosureEnvironments.Add(blockClosureEnvironment);
                    }
                    foreach (var child in block.Statements) { if (_terminated) break; EmitStatement(child); }
                    Pop();
                    if (hasBlockClosure) _activeBlockClosureEnvironments.RemoveAt(_activeBlockClosureEnvironments.Count - 1);
                    break;
                case EmptyStatement: break;
                case VariableStatement variable:
                    var type = variableTypes.GetValueOrDefault(variable) ?? variable.TypeName ?? InferType(variable.Initializer!);
                    if (structTypes.ContainsKey(type)) EnsureTypeInitialized(type, force: true);
                    var closureEnvironmentForVariable = FindClosureEnvironmentForVariable(variable.Name);
                    var capturedIndex = -1;
                    var isCaptured = closureEnvironmentForVariable is not null &&
                        closureEnvironmentForVariable.CapturedFieldIndices.TryGetValue(variable.Name, out capturedIndex);
                    var ptr = isCaptured ? EmitClosureFieldAddress(closureEnvironmentForVariable!, capturedIndex) : $"%v{_temp++}";
                    if (!isCaptured) Emit($"{ptr} = alloca {LlvmType(type)}");
                    var isScopeBound = scopeBoundVariables.Contains(variable);
                    var ownsScopeBoundDelegate = false;
                    _isScopeVariableInitializer = isScopeBound && variable.Initializer is AllocationExpression { IsScope: true };
                    if (variable.Initializer is not null)
                    {
                        var value = variable.Initializer is InlineArrayInitializerExpression inlineInitializer
                            ? SemanticAnalyzer.TryParseManagedArrayType(type, out _, out _) ? EmitManagedArrayInitializer(inlineInitializer, type) : EmitInlineArrayInitializer(inlineInitializer, type)
                            : EmitExpression(variable.Initializer);
                        ownsScopeBoundDelegate = isScopeBound && IsDelegateType(type) && _ownedClassValues.Any(owned => owned.Value == value.Value);
                        if (IsStringType(type))
                        {
                            if (isScopeBound) _ = StoreScopeBoundStringAlias(ptr, value, type);
                            else StoreStringInitial(ptr, value);
                        }
                        else if (SemanticAnalyzer.TryParseManagedArrayType(type, out _, out _))
                        {
                            if (isScopeBound) _ = StoreScopeBoundArrayAlias(ptr, value, type);
                            else StoreArrayInitial(ptr, value, type);
                        }
                        else if (GetClassReferenceType(type) is { } localClass)
                        {
                            if (isScopeBound && !ownsScopeBoundDelegate) _ = StoreScopeBoundClassAlias(ptr, value, localClass);
                            else StoreClassInitial(ptr, value, localClass);
                        }
                        else if (IsManagedAggregateType(type)) StoreAggregateInitial(ptr, value, type);
                        else { var converted = CastValue(value.Value, value.Type, type); Emit($"store {LlvmType(type)} {converted}, ptr {ptr}"); }
                    }
                    else Emit($"store {LlvmType(type)} {DefaultStorageValue(type)}, ptr {ptr}");
                    _isScopeVariableInitializer = false;
                    Declare(variable.Name, type, ptr, ownsString: !isCaptured, ownsArray: !isCaptured, ownsClass: !isCaptured,
                        isScopeBound: isScopeBound, ownsScopeBoundClass: ownsScopeBoundDelegate, ownsAggregate: !isCaptured); ReleaseAllOwnedArrays(); ReleaseStatementScopeTemps(); break;
                case VariableDeclarationStatement declaration:
                    EmitVariableDeclarations(declaration.Variables); ReleaseStatementScopeTemps(); break;
                case MultiAssignmentStatement assignment:
                    EmitMultiAssignment(assignment); ReleaseStatementScopeTemps(); break;
                case ExpressionStatement expression:
                    var expressionValue = EmitExpression(expression.Value);
                    ReleaseOwnedString(expressionValue); ReleaseOwnedArray(expressionValue); ReleaseOwnedClass(expressionValue); ReleaseOwnedAggregate(expressionValue); ReleaseAllOwnedArrays(); ReleaseStatementScopeTemps(); break;
                case ReturnStatement ret:
                    if (ret.Value is null)
                    {
                        if (EmitControlTransfer(0, 0, 0, 0))
                        {
                            MarkClassConstructorCompleted();
                            Emit("ret void");
                        }
                    }
                    else
                    {
                        var value = EmitExpression(ret.Value);
                        var converted = CastValue(value.Value, value.Type, method.Declaration.ReturnType);
                        if (IsStringType(method.Declaration.ReturnType)) { EnsureOwnedString(value); _ownedStringValues.Remove(value.Value); }
                        if (SemanticAnalyzer.TryParseManagedArrayType(method.Declaration.ReturnType, out _, out _)) { EnsureOwnedArray(value); _ownedArrayValues.Remove(value.Value); }
                        if (GetClassReferenceType(method.Declaration.ReturnType) is { } returnClass)
                        {
                            var returnedValue = (converted, method.Declaration.ReturnType);
                            EnsureOwnedClass(returnedValue, returnClass);
                            _ownedClassValues.Remove((converted, returnClass));
                        }
                        if (IsManagedAggregateType(method.Declaration.ReturnType))
                        {
                            var returnedValue = (converted, method.Declaration.ReturnType);
                            if (value.Type == method.Declaration.ReturnType && _ownedAggregateValues.Remove(value)) { }
                            else EmitAggregateOwnership(returnedValue, retain: true);
                            if (value.Type != method.Declaration.ReturnType) ReleaseOwnedAggregate(value);
                        }
                        if (EmitControlTransfer(0, 0, 0, 0)) Emit($"ret {LlvmType(method.Declaration.ReturnType)} {converted}");
                    }
                    _terminated = true; break;
                case IfStatement conditional: EmitIf(conditional); break;
                case WhileStatement loop: EmitWhile(loop.Condition, loop.Body, false, loop.Label); break;
                case DoWhileStatement loop: EmitDoWhile(loop); break;
                case ForStatement loop: EmitFor(loop); break;
                case ForInStatement loop:
                    if (loweredForInLoops.TryGetValue(loop, out var loweredForIn)) EmitStatement(loweredForIn);
                    else EmitForIn(loop);
                    break;
                case SwitchStatement selection: EmitSwitch(selection); break;
                case TryStatement tryStatement: EmitTry(tryStatement); break;
                case ThrowStatement thrown: EmitThrow(thrown); break;
                case BreakStatement transfer:
                    if (transfer.IsContinue)
                    {
                        if (TryFindLoop(transfer.Label, out var continueTarget) &&
                            EmitControlTransfer(continueTarget.ContinueScopeDepth, continueTarget.ContinueHandlerDepth,
                                continueTarget.ContinueTryDepth, continueTarget.ContinueFinallyDepth)) Branch(continueTarget.Continue);
                    }
                    else if (transfer.Label is not null)
                    {
                        if (TryFindLoop(transfer.Label, out var labeledTarget) &&
                            EmitControlTransfer(labeledTarget.BreakScopeDepth, labeledTarget.BreakHandlerDepth,
                                labeledTarget.BreakTryDepth, labeledTarget.BreakFinallyDepth)) Branch(labeledTarget.Break);
                    }
                    else if (_breakTargets.TryPeek(out var breakTarget) &&
                        EmitControlTransfer(breakTarget.ScopeDepth, breakTarget.HandlerDepth, breakTarget.TryDepth, breakTarget.FinallyDepth))
                        Branch(breakTarget.Break);
                    break;
                case OverflowContextStatement overflow:
                    var previousChecked = _checkedContext; _checkedContext = overflow.Checked;
                    try { EmitStatement(overflow.Body); } finally { _checkedContext = previousChecked; }
                    break;
            }
        }

        private void EmitThrow(ThrowStatement statement)
        {
            string exception;
            string? exceptionType;
            if (statement.Value is null)
            {
                if (!_currentCatchExceptions.TryPeek(out var currentCatch))
                    throw new InvalidOperationException("A validated rethrow has no active catch exception.");
                exception = NewTemp();
                Emit($"{exception} = load ptr, ptr {currentCatch}");
                exceptionType = "System.Exception";
                EnsureOwnedClass((exception, exceptionType), exceptionType);
                _ownedClassValues.Remove((exception, exceptionType));
            }
            else
            {
                if (!throwTypes.TryGetValue(statement, out var boundType))
                    throw new InvalidOperationException("A validated throw is missing its bound exception type.");
                exceptionType = boundType;
                var value = EmitExpression(statement.Value);
                exception = value.Value;
                EnsureOwnedClass(value, exceptionType);
                _ownedClassValues.Remove((exception, GetClassReferenceType(value.Type) ?? exceptionType));
            }
            EmitRaisePreparedException(exception);
        }

        private void EmitRaisePreparedException(string exception)
        {
            var previousException = NewTemp();
            Emit($"{previousException} = load ptr, ptr @luma_pending_exception");
            var hasPreviousException = NewTemp();
            Emit($"{hasPreviousException} = icmp ne ptr {previousException}, null");
            var releasePrevious = NewLabel("eh.release.previous");
            var storeNew = NewLabel("eh.store.new");
            Emit($"br i1 {hasPreviousException}, label %{releasePrevious}, label %{storeNew}");
            _terminated = true;
            Label(releasePrevious);
            Emit($"call void @luma_class_release_dynamic(ptr {previousException})");
            Branch(storeNew);
            Label(storeNew);
            Emit($"store ptr {exception}, ptr @luma_pending_exception");
            var target = NewTemp();
            Emit($"{target} = call ptr @luma_eh_find_handler(ptr {exception})");
            Emit($"store ptr {target}, ptr @luma_pending_handler");
            EmitExceptionTransfer(target);
        }

        private void EmitRuntimeException(string runtimeFailure)
        {
            var exceptionType = runtimeFailure switch
            {
                "luma_throw_overflow" => "System.OverflowException",
                "luma_throw_divide_by_zero" => "System.DivideByZeroException",
                "luma_throw_bounds" => "System.IndexOutOfRangeException",
                "luma_throw_out_of_memory" => "System.OutOfMemoryException",
                "luma_throw_null" => "System.NullReferenceException",
                "luma_throw_invalid_cast" => "System.InvalidCastException",
                _ => throw new InvalidOperationException($"Unknown runtime exception source '{runtimeFailure}'.")
            };
            if (!usesExceptionHandling || !classTypes.TryGetValue(exceptionType, out var exceptionClass))
            {
                Emit($"call void @{runtimeFailure}()");
                Emit("unreachable");
                _terminated = true;
                return;
            }
            if (exceptionType == "System.OutOfMemoryException")
            {
                EmitRaisePreparedException("@luma_oom_instance");
                return;
            }

            var llvmClassType = $"%class.{Safe(exceptionType)}";
            var sizeAddress = NewTemp(); Emit($"{sizeAddress} = getelementptr {llvmClassType}, ptr null, i32 1");
            var sizeType = nativePointerSize == 8 ? "i64" : "i32";
            var size = NewTemp(); Emit($"{size} = ptrtoint ptr {sizeAddress} to {sizeType}");
            var instance = NewTemp(); Emit($"{instance} = call ptr @malloc({sizeType} {size})");
            var allocationFailed = NewTemp(); Emit($"{allocationFailed} = icmp eq ptr {instance}, null");
            var oom = NewLabel("eh.runtime.oom");
            var initialized = NewLabel("eh.runtime.initialized");
            Emit($"br i1 {allocationFailed}, label %{oom}, label %{initialized}");
            _terminated = true;
            Label(oom);
            EmitRaisePreparedException("@luma_oom_instance");
            Label(initialized);
            var descriptor = NewTemp(); Emit($"{descriptor} = getelementptr {llvmClassType}, ptr {instance}, i32 0, i32 0");
            Emit($"store ptr @luma_classdesc_{Safe(exceptionType)}, ptr {descriptor}");
            var count = NewTemp(); Emit($"{count} = getelementptr {llvmClassType}, ptr {instance}, i32 0, i32 1");
            Emit($"store i32 1, ptr {count}");
            var flags = NewTemp(); Emit($"{flags} = getelementptr {llvmClassType}, ptr {instance}, i32 0, i32 2");
            Emit($"store i32 0, ptr {flags}");
            foreach (var field in exceptionClass.Fields)
            {
                var address = NewTemp(); Emit($"{address} = getelementptr {llvmClassType}, ptr {instance}, i32 0, i32 {field.Index + 4}");
                Emit($"store {LlvmType(field.TypeName)} {DefaultStorageValue(field.TypeName)}, ptr {address}");
            }
            EmitRaisePreparedException(instance);
        }

        private void EmitTry(TryStatement statement)
        {
            var outerScopeDepth = _scopes.Count;
            ExceptionRegion? handler = null;
            var bodyNormal = NewLabel("try.body.normal");
            var dispatch = NewLabel("try.dispatch");
            var end = NewLabel("try.end");

            if (statement.Catches.Count > 0)
            {
                if (!exceptionCatchTableNames.TryGetValue(statement, out var catchTable))
                    throw new InvalidOperationException("A try statement with catches is missing its catch table.");
                var node = NewTemp();
                var previous = NewTemp();
                Emit($"{node} = alloca %luma.ehnode");
                Emit($"{previous} = load ptr, ptr @luma_eh_top");
                var previousAddress = NewTemp(); Emit($"{previousAddress} = getelementptr %luma.ehnode, ptr {node}, i32 0, i32 0");
                Emit($"store ptr {previous}, ptr {previousAddress}");
                var ownerAddress = NewTemp(); Emit($"{ownerAddress} = getelementptr %luma.ehnode, ptr {node}, i32 0, i32 1");
                Emit($"store ptr null, ptr {ownerAddress}");
                var typesAddress = NewTemp(); Emit($"{typesAddress} = getelementptr %luma.ehnode, ptr {node}, i32 0, i32 2");
                Emit($"store ptr getelementptr inbounds ([{statement.Catches.Count} x ptr], ptr {catchTable}, i32 0, i32 0), ptr {typesAddress}");
                var countAddress = NewTemp(); Emit($"{countAddress} = getelementptr %luma.ehnode, ptr {node}, i32 0, i32 3");
                Emit($"store i32 {statement.Catches.Count}, ptr {countAddress}");
                Emit($"store ptr {node}, ptr @luma_eh_top");
                handler = new(statement, node, previous, dispatch, outerScopeDepth);
                _activeExceptionHandlers.Add(handler);
            }

            var frame = new TryFrame(statement, _activeTries.Count + 1, outerScopeDepth, handler);
            _activeTries.Add(frame);
            if (statement.Finally is not null) _activeFinalizers.Add(new(frame, statement.Finally));
            EmitStatement(statement.Body);
            if (!_terminated)
            {
                if (handler is not null) Emit($"store ptr {handler.PreviousNode}, ptr @luma_eh_top");
                Branch(bodyNormal);
            }
            if (handler is not null) _activeExceptionHandlers.RemoveAt(_activeExceptionHandlers.Count - 1);

            Label(bodyNormal);
            if (statement.Finally is not null) EmitNormalFinally(frame);
            if (!_terminated) Branch(end);

            if (handler is not null)
            {
                Label(dispatch);
                var exceptionValue = NewTemp();
                Emit($"{exceptionValue} = load ptr, ptr @luma_pending_exception");
                var clauseLabels = statement.Catches.Select(_ => NewLabel("catch.body")).ToArray();
                for (var i = 0; i < statement.Catches.Count; i++)
                {
                    var clause = statement.Catches[i];
                    if (clause.IsCatchAll)
                    {
                        Branch(clauseLabels[i]);
                        break;
                    }
                    if (!exceptionCatchTypes.TryGetValue(clause, out var catchType))
                        throw new InvalidOperationException("A validated catch clause is missing its exception type.");
                    var matches = NewTemp();
                    Emit($"{matches} = call i1 @luma_exception_is_instance(ptr {exceptionValue}, ptr @luma_classdesc_{Safe(catchType)})");
                    var next = i == statement.Catches.Count - 1 ? NewLabel("catch.invalid") : NewLabel("catch.test");
                    Emit($"br i1 {matches}, label %{clauseLabels[i]}, label %{next}");
                    _terminated = true;
                    Label(next);
                    if (i == statement.Catches.Count - 1) Emit("unreachable");
                }

                for (var i = 0; i < statement.Catches.Count; i++)
                {
                    var clause = statement.Catches[i];
                    Label(clauseLabels[i]);
                    var catchSlot = NewTemp(); Emit($"{catchSlot} = alloca ptr");
                    Emit($"store ptr {exceptionValue}, ptr {catchSlot}");
                    Emit("store ptr null, ptr @luma_pending_exception");
                    Emit("store ptr null, ptr @luma_pending_handler");
                    Push();
                    if (clause.VariableName is { } variableName)
                    {
                        var catchType = exceptionCatchTypes[clause];
                        _scopeClassPointers[^1].Add((catchSlot, catchType));
                        if (blockClosureEnvironments.TryGetValue(clause.Body, out var catchClosureEnvironment) &&
                            catchClosureEnvironment.CapturedFieldIndices.TryGetValue(variableName, out var catchFieldIndex))
                        {
                            _ = AllocateClosureEnvironment(catchClosureEnvironment);
                            _scopeClassPointers[^1].Add((_closureEnvironmentSlots[catchClosureEnvironment.TypeName], catchClosureEnvironment.TypeName));
                            _activeBlockClosureEnvironments.Add(catchClosureEnvironment);
                            var catchValue = NewTemp(); Emit($"{catchValue} = load ptr, ptr {catchSlot}");
                            var capturedAddress = EmitClosureFieldAddress(catchClosureEnvironment, catchFieldIndex);
                            StoreClassInitial(capturedAddress, (catchValue, catchType), catchType);
                            Declare(variableName, catchType, capturedAddress,
                                ownsString: false, ownsArray: false, ownsClass: false, ownsAggregate: false);
                            _preallocatedBlockClosureScopes.Add(clause.Body);
                        }
                        else Declare(variableName, catchType, catchSlot, ownsString: false, ownsArray: false, ownsClass: false);
                    }
                    else _scopeClassPointers[^1].Add((catchSlot, "System.Exception"));
                    _currentCatchExceptions.Push(catchSlot);
                    EmitStatement(clause.Body);
                    _currentCatchExceptions.Pop();
                    Pop();
                    if (!_terminated && statement.Finally is not null) EmitNormalFinally(frame);
                    if (!_terminated) Branch(end);
                }
            }

            if (statement.Finally is not null && _activeFinalizers.Count > 0 && ReferenceEquals(_activeFinalizers[^1].Frame, frame))
                _activeFinalizers.RemoveAt(_activeFinalizers.Count - 1);
            _activeTries.RemoveAt(_activeTries.Count - 1);
            Label(end);
        }

        private void EmitNormalFinally(TryFrame frame)
        {
            var index = _activeFinalizers.FindLastIndex(item => ReferenceEquals(item.Frame, frame));
            if (index < 0) return;
            var savedFinalizers = _activeFinalizers.ToArray();
            EmitFinallyBody(_activeFinalizers[index], _activeExceptionHandlers.Count, frame.Depth - 1);
            _activeFinalizers.Clear(); _activeFinalizers.AddRange(savedFinalizers);
        }

        private void EmitExceptionTransfer(string target)
        {
            var localMatches = new List<(ExceptionRegion Region, int Index, string Label)>();
            for (var i = _activeExceptionHandlers.Count - 1; i >= 0; i--)
                localMatches.Add((_activeExceptionHandlers[i], i, NewLabel("eh.local")));
            var nonLocal = NewLabel("eh.nonlocal");
            foreach (var match in localMatches)
            {
                var matches = NewTemp(); Emit($"{matches} = icmp eq ptr {target}, {match.Region.Node}");
                var next = NewLabel("eh.search");
                Emit($"br i1 {matches}, label %{match.Label}, label %{next}");
                _terminated = true;
                Label(next);
            }
            Branch(nonLocal);

            foreach (var match in localMatches)
            {
                Label(match.Label);
                EmitReplaceSavedPropagations(match.Index);
                EmitStatementTempCleanup();
                EmitOwnedTemporaryCleanup();
                var targetTryIndex = _activeTries.FindIndex(frame => ReferenceEquals(frame.Handler, match.Region));
                var targetTryDepth = targetTryIndex >= 0 ? _activeTries[targetTryIndex].Depth : int.MaxValue;
                Emit($"store ptr {match.Region.Node}, ptr @luma_eh_top");
                var savedHandlers = _activeExceptionHandlers.ToArray();
                if (_activeExceptionHandlers.Count > match.Index + 1)
                    _activeExceptionHandlers.RemoveRange(match.Index + 1, _activeExceptionHandlers.Count - match.Index - 1);
                var savedFinalizers = _activeFinalizers.ToArray();
                var cleanupDepth = _scopes.Count;
                var unwindFinalizers = _activeFinalizers.Where(context => context.Frame.Depth > targetTryDepth).OrderByDescending(context => context.Frame.Depth).ToArray();
                foreach (var context in unwindFinalizers)
                {
                    EmitScopeRange(context.Frame.OuterScopeDepth, ref cleanupDepth);
                    EmitExceptionalFinally(context, match.Index + 1, targetTryDepth);
                    if (_terminated) break;
                }
                if (!_terminated) EmitScopeRange(match.Region.OuterScopeDepth, ref cleanupDepth);
                if (!_terminated)
                {
                    Emit($"store ptr {match.Region.PreviousNode}, ptr @luma_eh_top");
                    Branch(match.Region.DispatchLabel);
                }
                _activeExceptionHandlers.Clear(); _activeExceptionHandlers.AddRange(savedHandlers);
                _activeFinalizers.Clear(); _activeFinalizers.AddRange(savedFinalizers);
            }

            Label(nonLocal);
            var hasTarget = NewTemp(); Emit($"{hasTarget} = icmp ne ptr {target}, null");
            var externalHandler = NewLabel("eh.external");
            var fatal = NewLabel("eh.unhandled");
            Emit($"br i1 {hasTarget}, label %{externalHandler}, label %{fatal}");
            _terminated = true;
            Label(externalHandler);
            EmitReplaceSavedPropagations(localHandlerIndex: null);
            EmitStatementTempCleanup();
            EmitOwnedTemporaryCleanup();
            if (_activeExceptionHandlers.Count > 0)
                Emit($"store ptr {_activeExceptionHandlers[0].PreviousNode}, ptr @luma_eh_top");
            var savedExternalFinalizers = _activeFinalizers.ToArray();
            var externalCleanupDepth = _scopes.Count;
            foreach (var context in _activeFinalizers.OrderByDescending(context => context.Frame.Depth).ToArray())
            {
                EmitScopeRange(context.Frame.OuterScopeDepth, ref externalCleanupDepth);
                EmitExceptionalFinally(context, 0, 0);
                if (_terminated) break;
            }
            if (!_terminated)
            {
                EmitScopeRange(0, ref externalCleanupDepth);
                EmitDefaultReturn();
            }
            _activeFinalizers.Clear(); _activeFinalizers.AddRange(savedExternalFinalizers);

            Label(fatal);
            EmitDefaultReturn();
        }

        private void EmitExceptionalFinally(FinallyContext context, int keepHandlerDepth, int keepTryDepth)
        {
            EmitFinallyBody(context, keepHandlerDepth, keepTryDepth);
        }

        private void EmitFinallyBody(FinallyContext context, int keepHandlerDepth, int keepTryDepth)
        {
            var finalizerIndex = _activeFinalizers.FindLastIndex(item => ReferenceEquals(item.Frame, context.Frame));
            if (finalizerIndex >= 0) _activeFinalizers.RemoveAt(finalizerIndex);
            var savedHandlers = _activeExceptionHandlers.ToArray();
            if (_activeExceptionHandlers.Count > keepHandlerDepth)
                _activeExceptionHandlers.RemoveRange(keepHandlerDepth, _activeExceptionHandlers.Count - keepHandlerDepth);
            var savedTries = _activeTries.ToArray();
            if (_activeTries.Count > keepTryDepth)
                _activeTries.RemoveRange(keepTryDepth, _activeTries.Count - keepTryDepth);

            var savedScopes = _scopes.ToArray();
            var savedStringScopes = _scopeStringPointers.ToArray();
            var savedArrayScopes = _scopeArrayPointers.ToArray();
            var savedClassScopes = _scopeClassPointers.ToArray();
            var savedAggregateScopes = _scopeAggregatePointers.ToArray();
            var savedAllocationScopes = _scopeAllocations.ToArray();
            var scopeDepth = Math.Min(context.Frame.OuterScopeDepth, _scopes.Count);
            _scopes.RemoveRange(scopeDepth, _scopes.Count - scopeDepth);
            _scopeStringPointers.RemoveRange(scopeDepth, _scopeStringPointers.Count - scopeDepth);
            _scopeArrayPointers.RemoveRange(scopeDepth, _scopeArrayPointers.Count - scopeDepth);
            _scopeClassPointers.RemoveRange(scopeDepth, _scopeClassPointers.Count - scopeDepth);
            _scopeAggregatePointers.RemoveRange(scopeDepth, _scopeAggregatePointers.Count - scopeDepth);
            _scopeAllocations.RemoveRange(scopeDepth, _scopeAllocations.Count - scopeDepth);

            var ownedStrings = _ownedStringValues.ToArray();
            var ownedArrays = _ownedArrayValues.ToArray();
            var ownedClasses = _ownedClassValues.ToArray();
            var ownedAggregates = _ownedAggregateValues.ToArray();
            _ownedStringValues.Clear(); _ownedArrayValues.Clear(); _ownedClassValues.Clear(); _ownedAggregateValues.Clear();
            var statementTemps = _statementScopeTemps.ToArray();
            _statementScopeTemps.Clear();
            var savedExceptionSlot = NewTemp(); Emit($"{savedExceptionSlot} = alloca ptr");
            var savedHandlerSlot = NewTemp(); Emit($"{savedHandlerSlot} = alloca ptr");
            var savedException = NewTemp(); Emit($"{savedException} = load ptr, ptr @luma_pending_exception");
            var savedHandler = NewTemp(); Emit($"{savedHandler} = load ptr, ptr @luma_pending_handler");
            Emit($"store ptr {savedException}, ptr {savedExceptionSlot}");
            Emit($"store ptr {savedHandler}, ptr {savedHandlerSlot}");
            Emit("store ptr null, ptr @luma_pending_exception");
            Emit("store ptr null, ptr @luma_pending_handler");
            var savedPropagation = new SavedPropagation(savedExceptionSlot, savedHandlerSlot, keepHandlerDepth);
            _savedPropagations.Add(savedPropagation);
            EmitStatement(context.Body);
            if (!_terminated)
            {
                var pendingAfterFinally = NewTemp(); Emit($"{pendingAfterFinally} = load ptr, ptr @luma_pending_exception");
                var noReplacement = NewTemp(); Emit($"{noReplacement} = icmp eq ptr {pendingAfterFinally}, null");
                var restoreLabel = NewLabel("eh.finally.restore");
                var replacementLabel = NewLabel("eh.finally.replaced");
                var doneLabel = NewLabel("eh.finally.done");
                Emit($"br i1 {noReplacement}, label %{restoreLabel}, label %{replacementLabel}");
                _terminated = true;
                Label(restoreLabel);
                var restoreException = NewTemp(); Emit($"{restoreException} = load ptr, ptr {savedExceptionSlot}");
                var restoreHandler = NewTemp(); Emit($"{restoreHandler} = load ptr, ptr {savedHandlerSlot}");
                Emit($"store ptr {restoreException}, ptr @luma_pending_exception");
                Emit($"store ptr {restoreHandler}, ptr @luma_pending_handler");
                Branch(doneLabel);
                Label(replacementLabel);
                var stillSaved = NewTemp(); Emit($"{stillSaved} = load ptr, ptr {savedExceptionSlot}");
                Emit($"call void @luma_class_release_dynamic(ptr {stillSaved})");
                Emit($"store ptr null, ptr {savedExceptionSlot}");
                Emit($"store ptr null, ptr {savedHandlerSlot}");
                Branch(doneLabel);
                Label(doneLabel);
            }
            _savedPropagations.Remove(savedPropagation);
            _ownedStringValues.Clear(); _ownedStringValues.UnionWith(ownedStrings);
            _ownedArrayValues.Clear(); _ownedArrayValues.UnionWith(ownedArrays);
            _ownedClassValues.Clear(); _ownedClassValues.UnionWith(ownedClasses);
            _ownedAggregateValues.Clear(); _ownedAggregateValues.UnionWith(ownedAggregates);
            _statementScopeTemps.Clear(); _statementScopeTemps.AddRange(statementTemps);
            _scopes.Clear(); _scopes.AddRange(savedScopes);
            _scopeStringPointers.Clear(); _scopeStringPointers.AddRange(savedStringScopes);
            _scopeArrayPointers.Clear(); _scopeArrayPointers.AddRange(savedArrayScopes);
            _scopeClassPointers.Clear(); _scopeClassPointers.AddRange(savedClassScopes);
            _scopeAggregatePointers.Clear(); _scopeAggregatePointers.AddRange(savedAggregateScopes);
            _scopeAllocations.Clear(); _scopeAllocations.AddRange(savedAllocationScopes);
            _activeExceptionHandlers.Clear(); _activeExceptionHandlers.AddRange(savedHandlers);
            _activeTries.Clear(); _activeTries.AddRange(savedTries);
        }

        private void EmitReplaceSavedPropagations(int? localHandlerIndex)
        {
            for (var index = _savedPropagations.Count - 1; index >= 0; index--)
            {
                var saved = _savedPropagations[index];
                if (localHandlerIndex is { } handlerIndex && handlerIndex >= saved.HandlerDepth) continue;
                var exception = NewTemp(); Emit($"{exception} = load ptr, ptr {saved.ExceptionSlot}");
                Emit($"call void @luma_class_release_dynamic(ptr {exception})");
                Emit($"store ptr null, ptr {saved.ExceptionSlot}");
                Emit($"store ptr null, ptr {saved.HandlerSlot}");
            }
        }

        private bool EmitControlTransfer(int targetScopeDepth, int targetHandlerDepth, int targetTryDepth, int targetFinallyDepth)
        {
            EmitStatementTempCleanup();
            EmitOwnedTemporaryCleanup();
            targetHandlerDepth = Math.Clamp(targetHandlerDepth, 0, _activeExceptionHandlers.Count);
            targetTryDepth = Math.Clamp(targetTryDepth, 0, _activeTries.Count);
            targetFinallyDepth = Math.Clamp(targetFinallyDepth, 0, _activeFinalizers.Count);
            if (_activeExceptionHandlers.Count > targetHandlerDepth)
            {
                var boundary = targetHandlerDepth == 0
                    ? _activeExceptionHandlers[0].PreviousNode
                    : _activeExceptionHandlers[targetHandlerDepth - 1].Node;
                Emit($"store ptr {boundary}, ptr @luma_eh_top");
            }

            var savedHandlers = _activeExceptionHandlers.ToArray();
            if (_activeExceptionHandlers.Count > targetHandlerDepth)
                _activeExceptionHandlers.RemoveRange(targetHandlerDepth, _activeExceptionHandlers.Count - targetHandlerDepth);
            var savedTries = _activeTries.ToArray();
            if (_activeTries.Count > targetTryDepth)
                _activeTries.RemoveRange(targetTryDepth, _activeTries.Count - targetTryDepth);
            var savedFinalizers = _activeFinalizers.ToArray();
            var cleanupDepth = _scopes.Count;
            foreach (var context in _activeFinalizers.Skip(targetFinallyDepth).Reverse().ToArray())
            {
                EmitScopeRange(context.Frame.OuterScopeDepth, ref cleanupDepth);
                EmitFinallyBody(context, targetHandlerDepth, targetTryDepth);
                if (_terminated) break;
            }
            if (!_terminated) EmitScopeRange(targetScopeDepth, ref cleanupDepth);
            _activeExceptionHandlers.Clear(); _activeExceptionHandlers.AddRange(savedHandlers);
            _activeTries.Clear(); _activeTries.AddRange(savedTries);
            _activeFinalizers.Clear(); _activeFinalizers.AddRange(savedFinalizers);
            return !_terminated;
        }

        private void EmitScopeRange(int targetDepth, ref int currentDepth)
        {
            targetDepth = Math.Clamp(targetDepth, 0, currentDepth);
            for (var scopeIndex = currentDepth - 1; scopeIndex >= targetDepth; scopeIndex--)
            {
                ReleaseScopeStrings(scopeIndex);
                ReleaseScopeArrays(scopeIndex);
                ReleaseScopeClasses(scopeIndex);
                ReleaseScopeAggregates(scopeIndex);
                ReleaseScopeAllocations(scopeIndex);
            }
            currentDepth = targetDepth;
        }

        private void EmitStatementTempCleanup()
        {
            for (var index = _statementScopeTemps.Count - 1; index >= 0; index--)
            {
                var allocation = _statementScopeTemps[index];
                if (allocation.IsArray) Emit($"call void @luma_array_release(ptr {allocation.Value})");
                else Emit($"call void @luma_class_release_dynamic(ptr {allocation.Value})");
            }
        }

        private void EmitOwnedTemporaryCleanup()
        {
            foreach (var value in _ownedStringValues) Emit($"call void @luma_string_release(ptr {value})");
            foreach (var value in _ownedArrayValues) Emit($"call void @luma_array_release(ptr {value})");
            foreach (var value in _ownedClassValues) Emit($"call void @luma_class_release_dynamic(ptr {value.Value})");
            foreach (var value in _ownedAggregateValues) EmitAggregateOwnership(value, retain: false);
        }

        private void EmitDefaultReturn()
        {
            var returnType = method.Declaration.ReturnType;
            if (returnType == "void") Emit("ret void");
            else
            {
                var llvmType = LlvmType(returnType);
                var value = llvmType == "ptr" ? "null" :
                    llvmType.StartsWith('{') || llvmType.StartsWith("%struct.", StringComparison.Ordinal) ? "zeroinitializer" :
                    llvmType is "float" or "double" ? "0.0" : "0";
                Emit($"ret {llvmType} {value}");
            }
            _terminated = true;
        }

        private void MarkClassConstructorCompleted()
        {
            if (!method.IsClassConstructor) return;
            var className = QualifiedTypeName(method.Owner);
            var classChain = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var current = className;
            while (classTypes.TryGetValue(current, out var classType) && seen.Add(current))
            {
                classChain.Add(current);
                if (classType.BaseTypeName is null) break;
                current = classType.BaseTypeName;
            }

            var finalizerOrdinal = 0;
            int? completionBit = null;
            foreach (var owner in classChain.AsEnumerable().Reverse())
            {
                if (!methods.Any(candidate => candidate.IsClassFinalizer && QualifiedTypeName(candidate.Owner) == owner)) continue;
                if (owner == className) completionBit = finalizerOrdinal;
                finalizerOrdinal++;
            }
            if (completionBit is not { } bit) return;
            var mask = unchecked((int)(1u << (bit + 3)));
            var flagsAddress = NewTemp();
            Emit($"{flagsAddress} = getelementptr %class.{Safe(className)}, ptr %this, i32 0, i32 2");
            var completionMask = NewTemp();
            Emit($"{completionMask} = select i1 %luma.complete.class, i32 {mask}, i32 0");
            Emit($"atomicrmw or ptr {flagsAddress}, i32 {completionMask} monotonic");
        }

        private void EmitCheckPendingException()
        {
            if (!usesExceptionHandling) return;
            var exception = NewTemp(); Emit($"{exception} = load ptr, ptr @luma_pending_exception");
            var hasException = NewTemp(); Emit($"{hasException} = icmp ne ptr {exception}, null");
            var exceptional = NewLabel("eh.call.failed");
            var normal = NewLabel("eh.call.normal");
            Emit($"br i1 {hasException}, label %{exceptional}, label %{normal}");
            _terminated = true;
            Label(exceptional);
            var target = NewTemp(); Emit($"{target} = load ptr, ptr @luma_pending_handler");
            EmitExceptionTransfer(target);
            Label(normal);
        }
        private (string Value, string Type) EmitDelegateCreation(Expression expression, DelegateMethodGroupBinding binding)
        {
            var target = "null";
            if (binding.IsInstance)
            {
                if (binding.Method.IsStructInstanceMethod)
                {
                    (string Value, string Type) receiver;
                    if (expression is MemberAccessExpression access)
                        receiver = EmitExpression(access.Receiver);
                    else if (expression is NameExpression && (method.IsStructInstanceMethod || method.IsStructConstructor))
                    {
                        var receiverType = QualifiedTypeName(method.Owner);
                        var receiverValue = NewTemp();
                        Emit($"{receiverValue} = load {LlvmType(receiverType)}, ptr %this");
                        receiver = (receiverValue, receiverType);
                    }
                    else throw new InvalidOperationException("A struct method-group binding has no struct value receiver.");

                    var structType = QualifiedTypeName(binding.Method.Owner);
                    if (receiver.Type != structType)
                        throw new InvalidOperationException($"A struct method-group receiver has type '{receiver.Type}', expected '{structType}'.");
                    target = EmitBoxStruct(receiver.Value, structType);
                    // The delegate now owns the newly allocated box through its target field.
                    _ownedClassValues.Remove((target, "System.Object"));
                }
                else if (expression is MemberAccessExpression access)
                {
                    var receiver = EmitExpression(access.Receiver);
                    var receiverType = GetClassReferenceType(receiver.Type) ??
                        throw new InvalidOperationException("An instance delegate binding has a non-reference receiver.");
                    var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {receiver.Value}, null");
                    TrapIf(isNull, "luma_throw_null");
                    target = receiver.Value;
                    if (_scopeBoundClassValues.Contains(target)) RetainClassReference(target, receiverType);
                    else
                    {
                        var existingOwner = _ownedClassValues.FirstOrDefault(owned => owned.Value == target);
                        if (existingOwner.Value == target) _ownedClassValues.Remove(existingOwner);
                        else RetainClassReference(target, receiverType);
                    }
                }
                else if (expression is NameExpression)
                {
                    target = "%this";
                    RetainClassReference(target, QualifiedTypeName(method.Owner));
                }
                else if (expression is LambdaExpression && binding.ClosureEnvironment is { } closureEnvironment)
                {
                    if (closureEnvironment.IsComposite)
                    {
                        target = AllocateClosureEnvironment(closureEnvironment);
                        _ownedClassValues.Add((target, closureEnvironment.TypeName));
                        foreach (var (sourceType, parentFieldName) in closureEnvironment.ParentEnvironmentFields!)
                        {
                            var source = NewTemp();
                            Emit($"{source} = load ptr, ptr {_closureEnvironmentSlots[sourceType]}");
                            var field = classTypes[closureEnvironment.TypeName].Fields.Single(candidate => candidate.Declaration.Name == parentFieldName);
                            var address = EmitClosureFieldAddress(closureEnvironment, field.Index);
                            StoreClassInitial(address, (source, sourceType), sourceType);
                        }
                    }
                    else
                    {
                        target = NewTemp();
                        Emit($"{target} = load ptr, ptr {_closureEnvironmentSlots[closureEnvironment.TypeName]}");
                    }
                    RetainClassReference(target, closureEnvironment.TypeName);
                }
                else throw new InvalidOperationException("An instance delegate binding has no supported receiver expression.");
            }

            var delegateName = NullableUnderlyingType(binding.DelegateType);
            var safeType = Safe(delegateName);
            var sizeType = nativePointerSize == 8 ? "i64" : "i32";
            var sizeAddress = NewTemp(); Emit($"{sizeAddress} = getelementptr %delegate.{safeType}, ptr null, i32 1");
            var size = NewTemp(); Emit($"{size} = ptrtoint ptr {sizeAddress} to {sizeType}");
            var instance = NewTemp(); Emit($"{instance} = call ptr @malloc({sizeType} {size})");
            var allocationFailed = NewTemp(); Emit($"{allocationFailed} = icmp eq ptr {instance}, null");
            TrapIf(allocationFailed, "luma_throw_out_of_memory");
            var typeAddress = NewTemp(); Emit($"{typeAddress} = getelementptr %delegate.{safeType}, ptr {instance}, i32 0, i32 0");
            Emit($"store ptr @luma_classdesc_delegate_{safeType}, ptr {typeAddress}");
            var countAddress = NewTemp(); Emit($"{countAddress} = getelementptr %delegate.{safeType}, ptr {instance}, i32 0, i32 1");
            Emit($"store i32 1, ptr {countAddress}");
            var flagsAddress = NewTemp(); Emit($"{flagsAddress} = getelementptr %delegate.{safeType}, ptr {instance}, i32 0, i32 2");
            Emit($"store i32 0, ptr {flagsAddress}");
            var weakAddress = NewTemp(); Emit($"{weakAddress} = getelementptr %delegate.{safeType}, ptr {instance}, i32 0, i32 3");
            Emit($"store ptr null, ptr {weakAddress}");
            var functionAddress = NewTemp(); Emit($"{functionAddress} = getelementptr %delegate.{safeType}, ptr {instance}, i32 0, i32 4");
            var function = binding.Method.IsStructInstanceMethod
                ? $"@{LlvmEmitter.StructDelegateThunkName(binding, methods)}"
                : $"@{FunctionName(binding.Method, methods)}";
            if (binding.IsInstance && binding.VirtualSlot is { } virtualSlot)
            {
                var descriptor = NewTemp(); Emit($"{descriptor} = load ptr, ptr {target}");
                var vtableAddress = NewTemp(); Emit($"{vtableAddress} = getelementptr %luma.typedesc, ptr {descriptor}, i32 0, i32 2");
                var vtable = NewTemp(); Emit($"{vtable} = load ptr, ptr {vtableAddress}");
                var slotAddress = NewTemp(); Emit($"{slotAddress} = getelementptr ptr, ptr {vtable}, i32 {virtualSlot}");
                var virtualFunction = NewTemp(); Emit($"{virtualFunction} = load ptr, ptr {slotAddress}");
                function = virtualFunction;
            }
            Emit($"store ptr {function}, ptr {functionAddress}");
            var targetAddress = NewTemp(); Emit($"{targetAddress} = getelementptr %delegate.{safeType}, ptr {instance}, i32 0, i32 5");
            Emit($"store ptr {target}, ptr {targetAddress}");
            _ownedClassValues.Add((instance, delegateName));
            return (instance, delegateName);
        }

        private (string Value, string Type) EmitDelegateCall(CallExpression expression, TypeDeclaration delegateType)
        {
            var delegateValue = EmitExpression(expression.Target);
            if (!callArguments.TryGetValue(expression, out var binding))
                throw new InvalidOperationException("A validated delegate invocation has no argument binding.");
            var parameters = delegateType.DelegateParameters ?? [];
            var values = new (string Value, string Type, bool IsAddress)?[parameters.Count];
            var ownedStrings = new List<(string Value, string Type)>();
            var ownedArrays = new List<(string Value, string Type)>();
            var ownedClasses = new List<(string Value, string Type)>();
            for (var sourceIndex = 0; sourceIndex < expression.Arguments.Count; sourceIndex++)
            {
                var parameterIndex = binding.ParameterBySource[sourceIndex];
                var parameter = parameters[parameterIndex];
                if (parameter.PassingMode is "ref" or "out")
                {
                    var (_, address) = ResolveAssignmentTarget(expression.Arguments[sourceIndex]);
                    values[parameterIndex] = (address, parameter.TypeName, true);
                    continue;
                }
                var supplied = EmitExpression(expression.Arguments[sourceIndex]);
                if (IsStringType(supplied.Type)) ownedStrings.Add(supplied);
                if (SemanticAnalyzer.TryParseManagedArrayType(supplied.Type, out _, out _)) ownedArrays.Add(supplied);
                if (GetClassReferenceType(supplied.Type) is not null) ownedClasses.Add(supplied);
                values[parameterIndex] = (CastValue(supplied.Value, supplied.Type, parameter.TypeName), parameter.TypeName, false);
            }
            var arguments = values.Select((value, index) =>
            {
                var actual = value ?? throw new InvalidOperationException("A validated delegate invocation omitted a required argument.");
                return actual.IsAddress ? $"ptr {actual.Value}" : $"{LlvmType(actual.Type)} {actual.Value}";
            });
            var argumentList = string.Join(", ", arguments);
            var returnType = delegateType.DelegateReturnType ?? "void";
            var result = returnType == "void" ? null : NewTemp();
            var safeType = Safe(QualifiedTypeName(delegateType));
            var functionAddress = NewTemp(); Emit($"{functionAddress} = getelementptr %delegate.{safeType}, ptr {delegateValue.Value}, i32 0, i32 4");
            var function = NewTemp(); Emit($"{function} = load ptr, ptr {functionAddress}");
            var targetAddress = NewTemp(); Emit($"{targetAddress} = getelementptr %delegate.{safeType}, ptr {delegateValue.Value}, i32 0, i32 5");
            var target = NewTemp(); Emit($"{target} = load ptr, ptr {targetAddress}");
            var isStatic = NewTemp(); Emit($"{isStatic} = icmp eq ptr {target}, null");
            var staticLabel = NewLabel("delegate.static"); var instanceLabel = NewLabel("delegate.instance"); var endLabel = NewLabel("delegate.end");
            Emit($"br i1 {isStatic}, label %{staticLabel}, label %{instanceLabel}"); _terminated = true;
            string? staticResult = null; string staticBlock;
            Label(staticLabel);
            var staticCall = $"call {LlvmType(returnType)} {function}({argumentList})";
            if (result is null) Emit(staticCall); else { staticResult = NewTemp(); Emit($"{staticResult} = {staticCall}"); }
            staticBlock = _block; Branch(endLabel);
            string? instanceResult = null; string instanceBlock;
            Label(instanceLabel);
            var instanceArguments = argumentList.Length == 0 ? $"ptr {target}" : $"ptr {target}, {argumentList}";
            var instanceCall = $"call {LlvmType(returnType)} {function}({instanceArguments})";
            if (result is null) Emit(instanceCall); else { instanceResult = NewTemp(); Emit($"{instanceResult} = {instanceCall}"); }
            instanceBlock = _block; Branch(endLabel);
            Label(endLabel);
            if (result is not null) Emit($"{result} = phi {LlvmType(returnType)} [ {staticResult}, %{staticBlock} ], [ {instanceResult}, %{instanceBlock} ]");
            EmitCheckPendingException();
            foreach (var value in ownedStrings) ReleaseOwnedString(value);
            foreach (var value in ownedArrays) ReleaseOwnedArray(value);
            foreach (var value in ownedClasses) ReleaseOwnedClass(value);
            ReleaseOwnedClass(delegateValue);
            if (result is not null && IsStringType(returnType)) _ownedStringValues.Add(result);
            if (result is not null && SemanticAnalyzer.TryParseManagedArrayType(returnType, out _, out _)) _ownedArrayValues.Add(result);
            if (result is not null && GetClassReferenceType(returnType) is { } resultClass) _ownedClassValues.Add((result, resultClass));
            return (result ?? string.Empty, returnType);
        }
        private bool TryFindLoop(string? label, out LoopTarget target)
        {
            foreach (var loop in _loops)
                if (label is null || loop.Label == label) { target = loop; return true; }
            target = null!; return false;
        }
        private void EmitVariableDeclarations(IReadOnlyList<VariableStatement> variables)
        {
            var values = new (string Value, string Type)?[variables.Count];
            var ownsScopeBoundDelegates = new bool[variables.Count];
            for (var index = 0; index < variables.Count; index++)
            {
                var declaredType = variableTypes.GetValueOrDefault(variables[index]) ?? variables[index].TypeName ??
                    (variables[index].Initializer is { } initial ? InferType(initial) : "?");
                if (structTypes.ContainsKey(declaredType)) EnsureTypeInitialized(declaredType, force: true);
                if (variables[index].Initializer is { } initializer)
                {
                    var isScopeBound = scopeBoundVariables.Contains(variables[index]);
                    _isScopeVariableInitializer = isScopeBound && initializer is AllocationExpression { IsScope: true };
                    values[index] = initializer is InlineArrayInitializerExpression inlineInitializer
                        ? SemanticAnalyzer.TryParseManagedArrayType(declaredType, out _, out _) ? EmitManagedArrayInitializer(inlineInitializer, declaredType) : EmitInlineArrayInitializer(inlineInitializer, declaredType)
                        : EmitExpression(initializer);
                    _isScopeVariableInitializer = false;
                }
            }

            for (var index = 0; index < variables.Count; index++)
            {
                var variable = variables[index];
                var type = variableTypes.GetValueOrDefault(variable) ?? variable.TypeName ?? InferType(variable.Initializer!);
                var closureEnvironmentForVariable = FindClosureEnvironmentForVariable(variable.Name);
                var capturedIndex = -1;
                var isCaptured = closureEnvironmentForVariable is not null &&
                    closureEnvironmentForVariable.CapturedFieldIndices.TryGetValue(variable.Name, out capturedIndex);
                var pointer = isCaptured ? EmitClosureFieldAddress(closureEnvironmentForVariable!, capturedIndex) : $"%v{_temp++}";
                if (!isCaptured) Emit($"{pointer} = alloca {LlvmType(type)}");
                if (values[index] is { } value)
                {
                ownsScopeBoundDelegates[index] = scopeBoundVariables.Contains(variable) && IsDelegateType(type) &&
                    _ownedClassValues.Any(owned => owned.Value == value.Value);
                if (IsStringType(type))
                {
                    if (scopeBoundVariables.Contains(variable)) _ = StoreScopeBoundStringAlias(pointer, value, type);
                    else StoreStringInitial(pointer, value);
                }
                else if (SemanticAnalyzer.TryParseManagedArrayType(type, out _, out _))
                {
                    if (scopeBoundVariables.Contains(variable)) _ = StoreScopeBoundArrayAlias(pointer, value, type);
                    else StoreArrayInitial(pointer, value, type);
                }
                else if (GetClassReferenceType(type) is { } localClass)
                {
                    if (scopeBoundVariables.Contains(variable) && !ownsScopeBoundDelegates[index]) _ = StoreScopeBoundClassAlias(pointer, value, localClass);
                    else StoreClassInitial(pointer, value, localClass);
                }
                else if (IsManagedAggregateType(type)) StoreAggregateInitial(pointer, value, type);
                    else
                    {
                        var converted = CastValue(value.Value, value.Type, type);
                        Emit($"store {LlvmType(type)} {converted}, ptr {pointer}");
                    }
                }
                else Emit($"store {LlvmType(type)} {DefaultStorageValue(type)}, ptr {pointer}");
                Declare(variable.Name, type, pointer, ownsString: !isCaptured, ownsArray: !isCaptured, ownsClass: !isCaptured,
                    isScopeBound: scopeBoundVariables.Contains(variable), ownsScopeBoundClass: ownsScopeBoundDelegates[index], ownsAggregate: !isCaptured);
            }
            ReleaseAllOwnedArrays();
        }
        private void EmitMultiAssignment(MultiAssignmentStatement assignment)
        {
            var values = assignment.Values.Select(EmitExpression).ToArray();
            var targets = new List<MultiAssignmentTarget>(assignment.Targets.Count);
            for (var targetIndex = 0; targetIndex < assignment.Targets.Count; targetIndex++)
            {
                var target = assignment.Targets[targetIndex];
                if (target is NameExpression name && TryLookup(name.Name, out var local))
                {
                    targets.Add(new(local.Type, local.Pointer, local.IsScopeBound, null));
                    continue;
                }
                if (target is IndexExpression index && managedArrayIndexAccesses.ContainsKey(index))
                {
                    var array = EmitExpression(index.Receiver);
                    if (_scopeBoundArrayValues.Contains(array.Value))
                    {
                        if (array.Value != "null") Emit($"call void @luma_array_retain(ptr {array.Value})");
                        if (array.Value != "null") _ownedArrayValues.Add(array.Value);
                    }
                    else EnsureOwnedArray(array);
                    var (address, elementType) = EmitIndexAddress(index, array);
                    targets.Add(new(elementType, address, false, array));
                    continue;
                }
                if (target is MemberAccessExpression member && staticFields.TryGetValue(member, out var staticField))
                {
                    EnsureTypeInitialized(staticField.OwnerType);
                    targets.Add(new(staticField.TypeName, $"@{staticField.GlobalName}", false, null, null));
                    continue;
                }
                if (target is MemberAccessExpression structMember && structFields.TryGetValue(structMember, out var structField))
                {
                    var address = EmitStructFieldAddress(structMember, structField);
                    targets.Add(new(structField.TypeName, address, false, null, null));
                    continue;
                }
                if (target is MemberAccessExpression classMember && classFields.TryGetValue(classMember, out var classField))
                {
                    var receiver = EmitExpression(classMember.Receiver);
                    var isNonOwningPointerReceiver = receiver.Type.EndsWith('*') && classTypes.ContainsKey(receiver.Type[..^1]);
                    var receiverClass = isNonOwningPointerReceiver ? receiver.Type[..^1] :
                        GetClassReferenceType(receiver.Type) ?? throw new InvalidOperationException("A class-field multiple-assignment target has no class receiver type.");
                    if (!isNonOwningPointerReceiver && _scopeBoundClassValues.Contains(receiver.Value))
                    {
                        if (receiver.Value != "null") RetainClassReference(receiver.Value, receiverClass);
                        if (receiver.Value != "null") _ownedClassValues.Add((receiver.Value, receiverClass));
                    }
                    else if (!isNonOwningPointerReceiver) EnsureOwnedClass(receiver, receiverClass);
                    var address = EmitClassFieldAddress(classMember, classField, (receiver.Value, receiverClass));
                    targets.Add(new(classField.TypeName, address, false, null,
                        isNonOwningPointerReceiver ? null : (receiver.Value, receiverClass)));
                    continue;
                }
                if (multiAssignmentAccessorAssignments.TryGetValue(assignment, out var accessorAssignments) &&
                    accessorAssignments.TryGetValue(targetIndex, out var accessorAssignment) &&
                    propertySetterCalls.TryGetValue(accessorAssignment, out var setterCall) &&
                    callTargets.TryGetValue(setterCall, out var setterTarget))
                {
                    string? receiverOverride = null;
                    (string Value, string Type)? heldReceiver = null;
                    IReadOnlyList<(string Value, string Type)> accessorArguments = [];
                    if (setterCall.Target is MemberAccessExpression accessorTarget && accessorTarget.Receiver is not BaseExpression)
                    {
                        if (setterTarget.IsClassInstanceMethod || interfaceCallSlots.ContainsKey(setterCall))
                        {
                            var receiver = EmitExpression(accessorTarget.Receiver);
                            if (GetClassReferenceType(receiver.Type) is { } receiverClass)
                            {
                                EnsureOwnedClass(receiver, receiverClass);
                                heldReceiver = receiver;
                            }
                            receiverOverride = receiver.Value;
                        }
                        else if (setterTarget.IsStructInstanceMethod)
                            (_, receiverOverride) = ResolveNullableStructAddress(accessorTarget.Receiver);
                    }
                    if (target is IndexExpression indexTarget)
                        accessorArguments = indexTarget.Indices.Select(EmitExpression).ToArray();
                    targets.Add(new(setterTarget.Declaration.Parameters[^1].TypeName, "", false, null, heldReceiver,
                        setterCall, receiverOverride, accessorArguments));
                    continue;
                }
                var (targetType, targetPointer) = ResolveAssignmentTarget(target);
                targets.Add(new(targetType, targetPointer, false, null, null));
            }
            for (var index = 0; index < targets.Count && index < values.Length; index++)
            {
                if (targets[index].SetterCall is not null) continue;
                if (IsStringType(targets[index].Type)) EnsureOwnedString(values[index]);
                else if (SemanticAnalyzer.TryParseManagedArrayType(targets[index].Type, out _, out _))
                {
                    if (_scopeBoundArrayValues.Contains(values[index].Value))
                    {
                        if (values[index].Value != "null") Emit($"call void @luma_array_retain(ptr {values[index].Value})");
                        if (values[index].Value != "null") _ownedArrayValues.Add(values[index].Value);
                        _scopeBoundArrayValues.Remove(values[index].Value);
                    }
                    else EnsureOwnedArray(values[index]);
                }
                else if (GetClassReferenceType(targets[index].Type) is { } classType)
                {
                    if (_scopeBoundClassValues.Contains(values[index].Value))
                    {
                        if (values[index].Value != "null") RetainClassReference(values[index].Value, classType);
                        if (values[index].Value != "null") _ownedClassValues.Add((values[index].Value, classType));
                        _scopeBoundClassValues.Remove(values[index].Value);
                    }
                    else EnsureOwnedClass(values[index], classType);
                }
                else if (IsManagedAggregateType(targets[index].Type)) EnsureOwnedAggregate(values[index]);
            }
            for (var index = 0; index < targets.Count && index < values.Length; index++)
            {
                if (targets[index].SetterCall is { } setter)
                {
                    var setterArguments = (targets[index].AccessorArguments ?? []).Append(values[index]).ToArray();
                    _ = EmitCall(setter, targets[index].AccessorReceiver, sourceArgumentOverrides: setterArguments);
                    continue;
                }
                if (IsStringType(targets[index].Type))
                {
                    if (targets[index].IsScopeBound) StoreScopeBoundStringAlias(targets[index].Pointer, values[index], targets[index].Type, replaceExisting: true);
                    else StoreStringReplacing(targets[index].Pointer, values[index]);
                }
                else if (SemanticAnalyzer.TryParseManagedArrayType(targets[index].Type, out _, out _))
                {
                    if (targets[index].IsScopeBound) StoreScopeBoundArrayAlias(targets[index].Pointer, values[index], targets[index].Type, replaceExisting: true);
                    else StoreArrayReplacing(targets[index].Pointer, values[index]);
                }
                else if (GetClassReferenceType(targets[index].Type) is { } targetClass)
                {
                    if (targets[index].IsScopeBound) StoreScopeBoundClassAlias(targets[index].Pointer, values[index], targetClass, replaceExisting: true);
                    else StoreClassReplacing(targets[index].Pointer, values[index], targetClass);
                }
                else if (IsManagedAggregateType(targets[index].Type)) StoreAggregateReplacing(targets[index].Pointer, values[index], targets[index].Type);
                else
                {
                    var converted = CastValue(values[index].Value, values[index].Type, targets[index].Type, checkedConversion: _checkedContext);
                    Emit($"store {LlvmType(targets[index].Type)} {converted}, ptr {targets[index].Pointer}");
                }
            }
            for (var index = targets.Count - 1; index >= 0; index--)
            {
                if (targets[index].HeldArray is { } array) ReleaseOwnedArray(array);
                if (targets[index].HeldClass is { } owner) ReleaseOwnedClass(owner);
            }
        }
        private (string Value, string Type) EmitInlineArrayInitializer(InlineArrayInitializerExpression initializer, string arrayType)
        {
            if (!SemanticAnalyzer.TryParseInlineArrayType(arrayType, out var elementType, out _))
                throw new InvalidOperationException("An inline array initializer reached emission without a bound inline array type.");
            var aggregateType = LlvmType(arrayType);
            var aggregate = "zeroinitializer";
            for (var index = 0; index < initializer.Elements.Count; index++)
            {
                var element = EmitExpression(initializer.Elements[index]);
                var converted = CastValue(element.Value, element.Type, elementType);
                var next = NewTemp();
                Emit($"{next} = insertvalue {aggregateType} {aggregate}, {LlvmType(elementType)} {converted}, {index}");
                aggregate = next;
            }
            return (aggregate, arrayType);
        }
        private (string Value, string Type) EmitManagedArrayInitializer(InlineArrayInitializerExpression initializer, string arrayType)
        {
            if (!SemanticAnalyzer.TryParseManagedArrayType(arrayType, out var elementType, out var rank) || rank > 3)
                throw new InvalidOperationException("A managed-array initializer reached emission without a supported array type.");
            var dimensions = GetArrayInitializerShape(initializer, rank).Select(length => length.ToString(System.Globalization.CultureInfo.InvariantCulture)).ToArray();
            var length = dimensions.Aggregate(1, (product, dimension) => checked(product * int.Parse(dimension, System.Globalization.CultureInfo.InvariantCulture))).ToString(System.Globalization.CultureInfo.InvariantCulture);
            var array = NewTemp();
            EmitArrayAllocation(array, length, rank, dimensions, elementType);
            _ownedArrayValues.Add(array);
            var data = NewTemp();
            Emit($"{data} = getelementptr %luma.array, ptr {array}, i32 0, i32 7");
            EmitManagedArrayElements(data, initializer, elementType);
            return (array, arrayType);
        }
        private (string Value, string Type) EmitManagedArrayAllocation(AllocationExpression allocation)
        {
            var arrayType = managedArrayAllocations[allocation];
            if (!SemanticAnalyzer.TryParseManagedArrayType(arrayType, out var elementType, out var rank) || rank > 3)
                throw new InvalidOperationException("A managed-array allocation reached emission without a supported array type.");
            var initializerShape = allocation.Initializer is null ? null : GetArrayInitializerShape(allocation.Initializer, rank);
            var dimensions = new string[rank];
            if (allocation.Dimensions.Count > 0)
            {
                for (var dimensionIndex = 0; dimensionIndex < rank; dimensionIndex++)
                {
                    var dimension = EmitExpression(allocation.Dimensions[dimensionIndex]);
                    dimensions[dimensionIndex] = CastValue(dimension.Value, dimension.Type, "int32", checkedConversion: _checkedContext);
                    var negative = NewTemp(); Emit($"{negative} = icmp slt i32 {dimensions[dimensionIndex]}, 0");
                    TrapIf(negative, "luma_throw_overflow");
                    if (initializerShape is not null)
                    {
                        var matches = NewTemp(); Emit($"{matches} = icmp eq i32 {dimensions[dimensionIndex]}, {initializerShape[dimensionIndex]}");
                        var mismatch = NewTemp(); Emit($"{mismatch} = xor i1 {matches}, true");
                        TrapIf(mismatch, "luma_throw_bounds");
                    }
                }
            }
            else if (initializerShape is not null)
            {
                for (var dimensionIndex = 0; dimensionIndex < rank; dimensionIndex++)
                    dimensions[dimensionIndex] = initializerShape[dimensionIndex].ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            else throw new InvalidOperationException("A managed-array allocation reached emission without dimensions or an initializer.");

            foreach (var dimension in dimensions)
            {
                var nonnegative = NewTemp(); Emit($"{nonnegative} = icmp sge i32 {dimension}, 0");
                var negative = NewTemp(); Emit($"{negative} = xor i1 {nonnegative}, true");
                TrapIf(negative, "luma_throw_overflow");
            }

            var length = dimensions[0];
            for (var dimensionIndex = 1; dimensionIndex < rank; dimensionIndex++)
            {
                var pair = NewTemp(); Emit($"{pair} = call {{ i32, i1 }} @llvm.umul.with.overflow.i32(i32 {length}, i32 {dimensions[dimensionIndex]})");
                var product = NewTemp(); Emit($"{product} = extractvalue {{ i32, i1 }} {pair}, 0");
                var overflow = NewTemp(); Emit($"{overflow} = extractvalue {{ i32, i1 }} {pair}, 1");
                TrapIf(overflow, "luma_throw_overflow");
                length = product;
            }
            var array = NewTemp();
            EmitArrayAllocation(array, length, rank, dimensions, elementType);
            if (allocation.IsScope)
            {
                if (_isScopeVariableInitializer) _scopeAllocations[^1].Add((array, arrayType, true));
                else _statementScopeTemps.Add((array, arrayType, true));
                _scopeBoundArrayValues.Add(array);
            }
            else _ownedArrayValues.Add(array);
            if (allocation.Initializer is { } initializer)
            {
                var data = NewTemp(); Emit($"{data} = getelementptr %luma.array, ptr {array}, i32 0, i32 7");
                EmitManagedArrayElements(data, initializer, elementType);
            }
            return (array, arrayType);
        }

        private (string Value, string Type) EmitClassAllocation(AllocationExpression allocation)
        {
            if (!classAllocations.TryGetValue(allocation, out var binding))
                throw new InvalidOperationException("A class allocation reached emission without a bound class and constructor.");
            if (binding.TypeName == "System.String")
                return EmitCall(binding.ConstructorCall);
            if (!classTypes.TryGetValue(binding.TypeName, out var classType))
                throw new InvalidOperationException("A class allocation reached emission without a bound class and constructor.");
            EnsureTypeInitialized(binding.TypeName, force: true);
            var safeType = string.Concat(binding.TypeName.Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : '_'));
            var sizeType = nativePointerSize == 8 ? "i64" : "i32";
            var sizeAddress = NewTemp(); Emit($"{sizeAddress} = getelementptr %class.{safeType}, ptr null, i32 1");
            var size = NewTemp(); Emit($"{size} = ptrtoint ptr {sizeAddress} to {sizeType}");
            var instance = NewTemp(); Emit($"{instance} = call ptr @malloc({sizeType} {size})");
            var allocationFailed = NewTemp(); Emit($"{allocationFailed} = icmp eq ptr {instance}, null"); TrapIf(allocationFailed, "luma_throw_out_of_memory");
            var typeAddress = NewTemp(); Emit($"{typeAddress} = getelementptr %class.{safeType}, ptr {instance}, i32 0, i32 0");
            Emit($"store ptr @luma_classdesc_{safeType}, ptr {typeAddress}");
            var countAddress = NewTemp(); Emit($"{countAddress} = getelementptr %class.{safeType}, ptr {instance}, i32 0, i32 1");
            Emit($"store i32 1, ptr {countAddress}");
            var flagsAddress = NewTemp(); Emit($"{flagsAddress} = getelementptr %class.{safeType}, ptr {instance}, i32 0, i32 2");
            Emit($"store i32 0, ptr {flagsAddress}");
            var weakAddress = NewTemp(); Emit($"{weakAddress} = getelementptr %class.{safeType}, ptr {instance}, i32 0, i32 3");
            Emit($"store ptr null, ptr {weakAddress}");
            foreach (var field in classType.Fields)
            {
                var address = NewTemp(); Emit($"{address} = getelementptr %class.{safeType}, ptr {instance}, i32 0, i32 {field.Index + 4}");
                Emit($"store {LlvmType(field.TypeName)} {DefaultStorageValue(field.TypeName)}, ptr {address}");
            }
            if (allocation.IsScope)
            {
                if (_isScopeVariableInitializer) _scopeAllocations[^1].Add((instance, binding.TypeName, false));
                else _statementScopeTemps.Add((instance, binding.TypeName, false));
                _scopeBoundClassValues.Add(instance);
            }
            else _ownedClassValues.Add((instance, binding.TypeName));
            // Register ownership before running user code so a constructor failure
            // destroys the partially built object and any base classes that completed.
            _ = EmitCall(binding.ConstructorCall, instance);
            return (instance, binding.TypeName);
        }

        private void InitializeClassFields()
        {
            if (!classTypes.TryGetValue(QualifiedTypeName(method.Owner), out var classType)) return;
            foreach (var field in classType.Fields.Where(field => QualifiedTypeName(field.Owner) == QualifiedTypeName(method.Owner) && field.Declaration.Initializer is not null))
            {
                var address = EmitCurrentClassFieldAddress(new(QualifiedTypeName(method.Owner), field.TypeName, field.Index, field.Declaration.IsReadOnly));
                var value = field.Declaration.Initializer is InlineArrayInitializerExpression inlineInitializer
                    ? EmitInlineArrayInitializer(inlineInitializer, field.TypeName)
                    : EmitExpression(field.Declaration.Initializer!);
                if (IsStringType(field.TypeName)) StoreStringReplacing(address, value);
                else if (GetClassReferenceType(field.TypeName) is { } fieldClass) StoreClassReplacing(address, value, fieldClass);
                else if (SemanticAnalyzer.TryParseManagedArrayType(field.TypeName, out _, out _)) StoreArrayReplacing(address, value);
                else if (IsManagedAggregateType(field.TypeName)) StoreAggregateReplacing(address, value, field.TypeName);
                else Emit($"store {LlvmType(field.TypeName)} {CastValue(value.Value, value.Type, field.TypeName)}, ptr {address}");
            }
        }

        private void InitializeStructFields()
        {
            if (!structTypes.TryGetValue(QualifiedTypeName(method.Owner), out var structure)) return;
            foreach (var field in structure.Fields.Where(field => field.Declaration.Initializer is not null))
            {
                var initializer = field.Declaration.Initializer!;
                var address = NewTemp();
                Emit($"{address} = getelementptr {LlvmType(method.Declaration.ReturnType)}, ptr %this, i32 0, i32 {field.Index}");
                var value = initializer is InlineArrayInitializerExpression inlineInitializer
                    ? EmitInlineArrayInitializer(inlineInitializer, field.TypeName)
                    : EmitExpression(initializer);
                if (IsStringType(field.TypeName)) StoreStringReplacing(address, value);
                else if (SemanticAnalyzer.TryParseManagedArrayType(field.TypeName, out _, out _)) StoreArrayReplacing(address, value, field.TypeName);
                else if (GetClassReferenceType(field.TypeName) is { } classFieldType) StoreClassReplacing(address, value, classFieldType);
                else if (IsManagedAggregateType(field.TypeName)) StoreAggregateReplacing(address, value, field.TypeName);
                else Emit($"store {LlvmType(field.TypeName)} {CastValue(value.Value, value.Type, field.TypeName)}, ptr {address}");
            }
        }

        private void EmitManagedArrayElements(string data, InlineArrayInitializerExpression initializer, string elementType)
        {
            var isJaggedElement = SemanticAnalyzer.TryParseManagedArrayType(elementType, out _, out _);
            var elements = (isJaggedElement ? initializer.Elements : FlattenArrayInitializer(initializer)).ToArray();
            for (var index = 0; index < elements.Length; index++)
            {
                var expression = elements[index];
                var element = isJaggedElement && expression is InlineArrayInitializerExpression nested
                    ? EmitManagedArrayInitializer(nested, elementType)
                    : EmitExpression(expression);
                var converted = CastValue(element.Value, element.Type, elementType);
                var address = NewTemp();
                Emit($"{address} = getelementptr {LlvmType(elementType)}, ptr {data}, i32 {index}");
                if (IsStringType(elementType)) EnsureOwnedString(element);
                else if (isJaggedElement) EnsureOwnedArrayElement(element);
                else if (GetClassReferenceType(elementType) is { } classElementType) EnsureOwnedClassElement(element, classElementType);
                else if (IsManagedAggregateType(elementType)) EnsureOwnedAggregate((converted, elementType));
                Emit($"store {LlvmType(elementType)} {converted}, ptr {address}");
                if (IsStringType(elementType)) _ownedStringValues.Remove(element.Value);
                else if (isJaggedElement) _ownedArrayValues.Remove(element.Value);
                else if (GetClassReferenceType(elementType) is { } ownedClassElement) _ownedClassValues.Remove((element.Value, ownedClassElement));
                else if (IsManagedAggregateType(elementType))
                {
                    _ownedAggregateValues.Remove((converted, elementType));
                    if (element.Type != elementType) ReleaseOwnedAggregate(element);
                }
                else ReleaseOwnedString(element);
            }
        }
        private void EmitArrayAllocation(string target, string length, int rank, IReadOnlyList<string> dimensions, string elementType)
        {
            var paddedDimensions = Enumerable.Range(0, 3).Select(index => index < dimensions.Count ? dimensions[index] : "0");
            var isStringReference = IsStringType(elementType);
            var hasEmptyStringDefault = elementType == "string";
            var isArray = SemanticAnalyzer.TryParseManagedArrayType(elementType, out _, out _);
            var isClass = GetClassReferenceType(elementType) is not null;
            var isAggregate = IsManagedAggregateType(elementType);
            var allocation = NewTemp();
            Emit($"{allocation} = call {{ ptr, i32 }} @luma_array_new(i32 {length}, i32 {rank}, {string.Join(", ", paddedDimensions.Select(value => "i32 " + value))}, i32 {SizeOf(elementType)}, i1 {(isStringReference ? "true" : "false")}, i1 {(hasEmptyStringDefault ? "true" : "false")}, i1 {(isArray ? "true" : "false")}, i1 {(isClass ? "true" : "false")}, i1 {(isAggregate ? "true" : "false")})");
            Emit($"{target} = extractvalue {{ ptr, i32 }} {allocation}, 0");
            var status = NewTemp(); Emit($"{status} = extractvalue {{ ptr, i32 }} {allocation}, 1");
            var overflow = NewTemp(); Emit($"{overflow} = icmp eq i32 {status}, 1");
            TrapIf(overflow, "luma_throw_overflow");
            var outOfMemory = NewTemp(); Emit($"{outOfMemory} = icmp eq i32 {status}, 2");
            TrapIf(outOfMemory, "luma_throw_out_of_memory");
            var arrayType = elementType + "[" + new string(',', rank - 1) + "]";
            var descriptorAddress = NewTemp(); Emit($"{descriptorAddress} = getelementptr %luma.array, ptr {target}, i32 0, i32 0");
            Emit($"store ptr {ArrayDescriptorPointer(arrayType)}, ptr {descriptorAddress}");
        }
        private static int[] GetArrayInitializerShape(InlineArrayInitializerExpression initializer, int rank)
        {
            if (rank <= 1) return [initializer.Elements.Count];
            var first = initializer.Elements.OfType<InlineArrayInitializerExpression>().FirstOrDefault();
            var childShape = first is null ? Enumerable.Repeat(0, rank - 1).ToArray() : GetArrayInitializerShape(first, rank - 1);
            return [initializer.Elements.Count, .. childShape];
        }
        private static IEnumerable<Expression> FlattenArrayInitializer(InlineArrayInitializerExpression initializer)
        {
            foreach (var element in initializer.Elements)
            {
                if (element is InlineArrayInitializerExpression nested)
                {
                    foreach (var leaf in FlattenArrayInitializer(nested)) yield return leaf;
                }
                else yield return element;
            }
        }
        private (string Value, string Type) EmitManagedArrayLength(MemberAccessExpression access)
        {
            var receiver = EmitExpression(access.Receiver);
            if (access.IsNullConditional) return EmitNullConditionalArrayLength(access, receiver);
            return EmitManagedArrayLength(receiver.Value);
        }
        private (string Value, string Type) EmitNullConditionalArrayLength(MemberAccessExpression access, (string Value, string Type) receiver)
        {
            var releaseReceiver = _ownedArrayValues.Remove(receiver.Value);
            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {receiver.Value}, null");
            var nullLabel = NewLabel("null.array.length.null"); var valueLabel = NewLabel("null.array.length.value"); var endLabel = NewLabel("null.array.length.end");
            Emit($"br i1 {isNull}, label %{nullLabel}, label %{valueLabel}"); _terminated = true;
            Label(nullLabel);
            if (releaseReceiver) Emit($"call void @luma_array_release(ptr {receiver.Value})");
            var nullBlock = _block; Branch(endLabel);
            Label(valueLabel);
            var length = EmitManagedArrayLength(receiver.Value).Value;
            var wrapped = WrapNullableValue(length, "int32", "int32?");
            if (releaseReceiver) Emit($"call void @luma_array_release(ptr {receiver.Value})");
            var valueBlock = _block; Branch(endLabel);
            Label(endLabel);
            var result = NewTemp(); Emit($"{result} = phi {LlvmType("int32?")} [ zeroinitializer, %{nullBlock} ], [ {wrapped}, %{valueBlock} ]");
            return (result, "int32?");
        }
        private (string Value, string Type) EmitManagedArrayLength(string array)
        {
            var address = NewTemp(); Emit($"{address} = getelementptr %luma.array, ptr {array}, i32 0, i32 4");
            var length = NewTemp(); Emit($"{length} = load i32, ptr {address}");
            return (length, "int32");
        }
        private (string Value, string Type) EmitManagedArrayPointer(MemberAccessExpression access, string elementType)
        {
            var receiver = EmitExpression(access.Receiver);
            if (elementType == "$string")
            {
                var dataAddress = NewTemp();
                Emit($"{dataAddress} = getelementptr %luma.string, ptr {receiver.Value}, i32 0, i32 4");
                var stringData = NewTemp();
                Emit($"{stringData} = load ptr, ptr {dataAddress}");
                return (stringData, "uint8*");
            }
            var length = EmitManagedArrayLength(receiver.Value).Value;
            var data = NewTemp();
            Emit($"{data} = getelementptr %luma.array, ptr {receiver.Value}, i32 0, i32 7");
            var isEmpty = NewTemp();
            Emit($"{isEmpty} = icmp eq i32 {length}, 0");
            var pointer = NewTemp();
            Emit($"{pointer} = select i1 {isEmpty}, ptr null, ptr {data}");
            return (pointer, elementType + "*");
        }
        private (string Value, string Type) EmitManagedArrayRank(string array)
        {
            var address = NewTemp(); Emit($"{address} = getelementptr %luma.array, ptr {array}, i32 0, i32 5");
            var rank = NewTemp(); Emit($"{rank} = load i32, ptr {address}");
            return (rank, "int32");
        }
        private (string Value, string Type) EmitNullConditionalArrayRank(MemberAccessExpression access, int rank)
        {
            var receiver = EmitExpression(access.Receiver);
            var releaseReceiver = _ownedArrayValues.Remove(receiver.Value);
            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {receiver.Value}, null");
            var nullLabel = NewLabel("null.array.rank.null"); var valueLabel = NewLabel("null.array.rank.value"); var endLabel = NewLabel("null.array.rank.end");
            Emit($"br i1 {isNull}, label %{nullLabel}, label %{valueLabel}"); _terminated = true;
            Label(nullLabel);
            if (releaseReceiver) Emit($"call void @luma_array_release(ptr {receiver.Value})");
            var nullBlock = _block; Branch(endLabel);
            Label(valueLabel);
            var value = rank < 0 ? EmitManagedArrayRank(receiver.Value).Value : rank.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var wrapped = WrapNullableValue(value, "int32", "int32?");
            if (releaseReceiver) Emit($"call void @luma_array_release(ptr {receiver.Value})");
            var valueBlock = _block; Branch(endLabel);
            Label(endLabel);
            var result = NewTemp(); Emit($"{result} = phi {LlvmType("int32?")} [ zeroinitializer, %{nullBlock} ], [ {wrapped}, %{valueBlock} ]");
            return (result, "int32?");
        }
        private (string Value, string Type) EmitManagedArrayGetLength(CallExpression call)
        {
            if (call.Target is not MemberAccessExpression member || call.Arguments.Count != 1)
                throw new InvalidOperationException("A bound Array.GetLength call has an invalid shape.");
            var receiver = EmitExpression(member.Receiver);
            if (member.IsNullConditional)
            {
                var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {receiver.Value}, null");
                var nullLabel = NewLabel("null.array.dimension.null"); var valueLabel = NewLabel("null.array.dimension.value"); var endLabel = NewLabel("null.array.dimension.end");
                Emit($"br i1 {isNull}, label %{nullLabel}, label %{valueLabel}"); _terminated = true;
                Label(nullLabel); var nullBlock = _block; Branch(endLabel);
                Label(valueLabel);
                var nullConditionalDimension = EmitExpression(call.Arguments[0]);
                var length = EmitManagedArrayGetLength(receiver.Value, CastValue(nullConditionalDimension.Value, nullConditionalDimension.Type, "int32"));
                var wrapped = WrapNullableValue(length, "int32", "int32?");
                var valueBlock = _block; Branch(endLabel);
                Label(endLabel);
                var nullableResult = NewTemp(); Emit($"{nullableResult} = phi {LlvmType("int32?")} [ zeroinitializer, %{nullBlock} ], [ {wrapped}, %{valueBlock} ]");
                return (nullableResult, "int32?");
            }
            var dimension = EmitExpression(call.Arguments[0]);
            var index = CastValue(dimension.Value, dimension.Type, "int32");
            return (EmitManagedArrayGetLength(receiver.Value, index), "int32");
        }
        private string EmitManagedArrayGetLength(string array, string index)
        {
            var rankAddress = NewTemp(); Emit($"{rankAddress} = getelementptr %luma.array, ptr {array}, i32 0, i32 5");
            var rank = NewTemp(); Emit($"{rank} = load i32, ptr {rankAddress}");
            var nonnegative = NewTemp(); Emit($"{nonnegative} = icmp sge i32 {index}, 0");
            var inRange = NewTemp(); Emit($"{inRange} = icmp slt i32 {index}, {rank}");
            var valid = NewTemp(); Emit($"{valid} = and i1 {nonnegative}, {inRange}");
            var invalid = NewTemp(); Emit($"{invalid} = xor i1 {valid}, true");
            TrapIf(invalid, "luma_throw_bounds");
            var dimensionAddress = NewTemp(); Emit($"{dimensionAddress} = getelementptr %luma.array, ptr {array}, i32 0, i32 6, i32 {index}");
            var dimensionLength = NewTemp(); Emit($"{dimensionLength} = load i32, ptr {dimensionAddress}");
            return dimensionLength;
        }
        private void EmitSwitch(SwitchStatement statement)
        {
            Push();
            var governingValue = EmitExpression(statement.Value);
            if (IsStringType(governingValue.Type))
            {
                EnsureOwnedString(governingValue);
                _ownedStringValues.Remove(governingValue.Value);
                var governingStorage = $"%v{_temp++}";
                Emit($"{governingStorage} = alloca ptr");
                Emit($"store ptr {governingValue.Value}, ptr {governingStorage}");
                _scopeStringPointers[^1].Add(governingStorage);
            }
            else if (IsManagedArrayType(governingValue.Type) && !_scopeBoundArrayValues.Contains(governingValue.Value))
            {
                EnsureOwnedArray(governingValue);
                _ownedArrayValues.Remove(governingValue.Value);
                var governingStorage = $"%v{_temp++}";
                Emit($"{governingStorage} = alloca ptr");
                Emit($"store ptr {governingValue.Value}, ptr {governingStorage}");
                _scopeArrayPointers[^1].Add(governingStorage);
            }
            var end = NewLabel("switch.end");
            var sectionLabels = statement.Sections.Select(_ => NewLabel("switch.section")).ToArray();
            var sectionPatternVariables = new Dictionary<int, List<(string Name, string Type, string Address)>>();
            for (var sectionIndex = 0; sectionIndex < statement.Sections.Count; sectionIndex++)
            {
                var variables = statement.Sections[sectionIndex].Labels
                    .SelectMany(SwitchPatternVariableNodes)
                    .Where(label => label.PatternVariableName is not null && switchTypePatterns.ContainsKey(label))
                    .GroupBy(label => label.PatternVariableName!, StringComparer.Ordinal)
                    .Select(group => group.First())
                    .Select(variablePattern =>
                    {
                        var variableType = switchTypePatterns[variablePattern];
                        var storageType = IsBoxableValueType(variableType) ? LlvmType(variableType) : "ptr";
                        var address = NewTemp(); Emit($"{address} = alloca {storageType}");
                        return (Name: variablePattern.PatternVariableName!, Type: variableType, Address: address);
                    })
                    .ToList();
                if (variables.Count > 0) sectionPatternVariables[sectionIndex] = variables;
            }
            string? defaultSection = null;
            var cases = new List<(SwitchLabel Label, string Section, int SectionIndex)>();
            for (var sectionIndex = 0; sectionIndex < statement.Sections.Count; sectionIndex++)
                foreach (var label in statement.Sections[sectionIndex].Labels)
                    if (label.IsDefault) defaultSection ??= sectionLabels[sectionIndex];
                    else cases.Add((label, sectionLabels[sectionIndex], sectionIndex));

            foreach (var (label, section, sectionIndex) in cases)
            {
                Push();
                var labelVariables = SwitchPatternVariableNodes(label)
                    .Where(pattern => pattern.PatternVariableName is not null && switchTypePatterns.ContainsKey(pattern))
                    .GroupBy(pattern => pattern.PatternVariableName!, StringComparer.Ordinal)
                    .Select(group => group.First())
                    .Select(pattern => pattern.PatternVariableName!)
                    .ToHashSet(StringComparer.Ordinal);
                if (sectionPatternVariables.TryGetValue(sectionIndex, out var patternVariables))
                    foreach (var patternVariable in patternVariables.Where(variable => labelVariables.Contains(variable.Name)))
                        Declare(patternVariable.Name, patternVariable.Type, patternVariable.Address, ownsString: false, ownsArray: false, ownsClass: false);
                var comparison = EmitSwitchPattern(governingValue, label);
                var next = NewLabel("switch.test");
                var matched = NewLabel("switch.matched");
                Emit($"br i1 {comparison}, label %{matched}, label %{next}");
                _terminated = true;
                Label(matched);
                if (sectionPatternVariables.TryGetValue(sectionIndex, out patternVariables))
                {
                    foreach (var patternVariable in patternVariables.Where(variable => labelVariables.Contains(variable.Name)))
                    {
                        var value = EmitSwitchPatternBindingValue(governingValue, patternVariable.Type);
                        if (IsManagedAggregateType(patternVariable.Type)) EmitAggregateOwnership((value, patternVariable.Type), retain: true);
                        Emit($"store {LlvmType(patternVariable.Type)} {value}, ptr {patternVariable.Address}");
                    }
                }
                ClosureEnvironmentBinding? guardClosureEnvironment = null;
                if (label.Guard is not null && blockClosureEnvironments.TryGetValue(label.GuardCaptureScope, out var foundGuardEnvironment))
                {
                    guardClosureEnvironment = foundGuardEnvironment;
                    _ = AllocateClosureEnvironment(guardClosureEnvironment);
                    _scopeClassPointers[^1].Add((_closureEnvironmentSlots[guardClosureEnvironment.TypeName], guardClosureEnvironment.TypeName));
                    _activeBlockClosureEnvironments.Add(guardClosureEnvironment);
                    if (sectionPatternVariables.TryGetValue(sectionIndex, out patternVariables))
                    {
                        foreach (var patternVariable in patternVariables.Where(variable => labelVariables.Contains(variable.Name) &&
                                     guardClosureEnvironment.CapturedFieldIndices.ContainsKey(variable.Name)))
                        {
                            var capturedValue = NewTemp();
                            Emit($"{capturedValue} = load {LlvmType(patternVariable.Type)}, ptr {patternVariable.Address}");
                            var fieldIndex = guardClosureEnvironment.CapturedFieldIndices[patternVariable.Name];
                            var capturedAddress = EmitClosureFieldAddress(guardClosureEnvironment, fieldIndex);
                            var value = (capturedValue, patternVariable.Type);
                            if (IsStringType(patternVariable.Type)) StoreStringInitial(capturedAddress, value);
                            else if (SemanticAnalyzer.TryParseManagedArrayType(patternVariable.Type, out _, out _))
                                StoreArrayInitial(capturedAddress, value, patternVariable.Type);
                            else if (GetClassReferenceType(patternVariable.Type) is { } capturedClass)
                                StoreClassInitial(capturedAddress, value, capturedClass);
                            else if (IsManagedAggregateType(patternVariable.Type))
                                StoreAggregateInitial(capturedAddress, value, patternVariable.Type);
                            else Emit($"store {LlvmType(patternVariable.Type)} {CastValue(capturedValue, patternVariable.Type, patternVariable.Type)}, ptr {capturedAddress}");
                            Declare(patternVariable.Name, patternVariable.Type, capturedAddress,
                                ownsString: false, ownsArray: false, ownsClass: false, ownsAggregate: false);
                        }
                    }
                }
                if (label.Guard is null)
                    Branch(section);
                else
                {
                    var guard = EmitExpression(label.Guard);
                    ReleaseAllOwnedArrays();
                    ReleaseStatementScopeTemps();
                    var guardPassed = NewLabel("switch.guard.passed");
                    var guardFailed = NewLabel("switch.guard.failed");
                    Emit($"br i1 {guard.Value}, label %{guardPassed}, label %{guardFailed}");
                    _terminated = true;
                    Label(guardFailed);
                    if (guardClosureEnvironment is not null) ReleaseScopeClasses(_scopes.Count - 1);
                    ReleaseScopeAggregates(_scopes.Count - 1);
                    Branch(next);
                    Label(guardPassed);
                    if (guardClosureEnvironment is not null) ReleaseScopeClasses(_scopes.Count - 1);
                    Branch(section);
                }
                if (guardClosureEnvironment is not null) _activeBlockClosureEnvironments.RemoveAt(_activeBlockClosureEnvironments.Count - 1);
                Pop();
                Label(next);
            }
            Branch(defaultSection ?? end);

            var switchScopeDepth = _scopes.Count;
            _breakTargets.Push(new(end, null, switchScopeDepth, _activeExceptionHandlers.Count, _activeFinalizers.Count, _activeTries.Count));
            for (var sectionIndex = 0; sectionIndex < statement.Sections.Count; sectionIndex++)
            {
                Label(sectionLabels[sectionIndex]); Push();
                var hasSectionClosure = blockClosureEnvironments.TryGetValue(statement.Sections[sectionIndex].CaptureScope, out var sectionClosureEnvironment);
                if (hasSectionClosure)
                {
                    _ = AllocateClosureEnvironment(sectionClosureEnvironment!);
                    _scopeClassPointers[^1].Add((_closureEnvironmentSlots[sectionClosureEnvironment!.TypeName], sectionClosureEnvironment.TypeName));
                    _activeBlockClosureEnvironments.Add(sectionClosureEnvironment);
                }
                if (sectionPatternVariables.TryGetValue(sectionIndex, out var patternVariables))
                    foreach (var patternVariable in patternVariables)
                    {
                        if (hasSectionClosure && sectionClosureEnvironment!.CapturedFieldIndices.TryGetValue(patternVariable.Name, out var capturedFieldIndex))
                        {
                            var capturedValue = NewTemp();
                            Emit($"{capturedValue} = load {LlvmType(patternVariable.Type)}, ptr {patternVariable.Address}");
                            var capturedAddress = EmitClosureFieldAddress(sectionClosureEnvironment, capturedFieldIndex);
                            var value = (capturedValue, patternVariable.Type);
                            if (IsStringType(patternVariable.Type)) StoreStringInitial(capturedAddress, value);
                            else if (SemanticAnalyzer.TryParseManagedArrayType(patternVariable.Type, out _, out _))
                                StoreArrayInitial(capturedAddress, value, patternVariable.Type);
                            else if (GetClassReferenceType(patternVariable.Type) is { } capturedClass)
                                StoreClassInitial(capturedAddress, value, capturedClass);
                            else if (IsManagedAggregateType(patternVariable.Type))
                                StoreAggregateInitial(capturedAddress, value, patternVariable.Type);
                            else Emit($"store {LlvmType(patternVariable.Type)} {CastValue(capturedValue, patternVariable.Type, patternVariable.Type)}, ptr {capturedAddress}");
                            Declare(patternVariable.Name, patternVariable.Type, capturedAddress,
                                ownsString: false, ownsArray: false, ownsClass: false, ownsAggregate: false);
                        }
                        else Declare(patternVariable.Name, patternVariable.Type, patternVariable.Address,
                            ownsString: false, ownsArray: false, ownsClass: false);
                    }
                foreach (var child in statement.Sections[sectionIndex].Statements)
                {
                    if (_terminated) break;
                    EmitStatement(child);
                }
                Pop();
                if (hasSectionClosure) _activeBlockClosureEnvironments.RemoveAt(_activeBlockClosureEnvironments.Count - 1);
                if (!_terminated) Branch(end);
            }
            _breakTargets.Pop();
            Label(end);
            Pop();
        }
        private string EmitSwitchPattern((string Value, string Type) governingValue, SwitchLabel pattern)
        {
            string comparison;
            if (pattern.CombinedOperator is { } combinedOperator && pattern.LeftPattern is { } leftPattern && pattern.RightPattern is { } rightPattern)
            {
                var left = EmitSwitchPattern(governingValue, leftPattern);
                var right = EmitSwitchPattern(governingValue, rightPattern);
                var combined = NewTemp();
                var instruction = combinedOperator switch { "and" => "and", "or" => "or", _ => throw new InvalidOperationException($"Unknown switch pattern operator '{combinedOperator}'.") };
                Emit($"{combined} = {instruction} i1 {left}, {right}");
                comparison = combined;
            }
            else if (switchTypePatterns.TryGetValue(pattern, out var patternType))
            {
                comparison = NewTemp();
                if (patternType == "string")
                {
                    if (IsStringType(governingValue.Type) && IsNullableStringType(governingValue.Type))
                    {
                        var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {governingValue.Value}, null");
                        Emit($"{comparison} = xor i1 {isNull}, true");
                    }
                    else if (IsStringType(governingValue.Type))
                        Emit($"{comparison} = icmp ne ptr {governingValue.Value}, null");
                    else if (GetClassReferenceType(governingValue.Type) is not null || IsManagedArrayType(governingValue.Type))
                        Emit($"{comparison} = call i1 @luma_class_is_instance(ptr {governingValue.Value}, ptr @luma_string_type)");
                    else if (governingValue.Type == "null")
                        Emit($"{comparison} = icmp ne ptr null, null");
                    else throw new InvalidOperationException("A validated string type pattern requires a string or object-reference governing value.");
                }
                else if (IsBoxableValueType(patternType))
                {
                    var governingType = governingValue.Type;
                    if (IsNullableValueType(governingType))
                    {
                        var underlyingType = NullableUnderlyingType(governingType);
                        if (underlyingType != patternType) Emit($"{comparison} = icmp eq i1 false, true");
                        else
                        {
                            var hasValue = NewTemp();
                            Emit($"{hasValue} = extractvalue {LlvmType(governingType)} {governingValue.Value}, 0");
                            Emit($"{comparison} = and i1 {hasValue}, true");
                        }
                    }
                    else if (IsBoxableValueType(governingType))
                        Emit($"{comparison} = icmp eq i1 {(governingType == patternType ? "true" : "false")}, true");
                    else if (IsRuntimeObjectReference(governingType))
                        Emit($"{comparison} = call i1 @luma_class_is_instance(ptr {governingValue.Value}, ptr {BoxDescriptorPointer(patternType)})");
                    else throw new InvalidOperationException("A validated value type pattern requires a matching value or object-reference governing value.");
                }
                else if (IsManagedArrayType(patternType))
                {
                    if (governingValue.Type == "null") Emit($"{comparison} = icmp ne ptr null, null");
                    else if (IsRuntimeObjectReference(governingValue.Type))
                        Emit($"{comparison} = call i1 @luma_class_is_instance(ptr {governingValue.Value}, ptr {ArrayDescriptorPointer(patternType)})");
                    else throw new InvalidOperationException("A validated array type pattern requires an array or object-reference governing value.");
                }
                else if (!IsRuntimeObjectReference(governingValue.Type))
                    throw new InvalidOperationException("A validated type pattern requires a reference governing value.");
                else if (interfaceTypes.ContainsKey(patternType))
                    Emit($"{comparison} = call i1 @luma_class_implements(ptr {governingValue.Value}, ptr @luma_interfacedesc_{Safe(patternType)})");
                else if (classTypes.ContainsKey(patternType) || delegateTypes.ContainsKey(patternType))
                    Emit($"{comparison} = call i1 @luma_class_is_instance(ptr {governingValue.Value}, ptr {ClassDescriptorPointer(patternType)})");
                else throw new InvalidOperationException($"A validated switch type pattern references unknown reference type '{patternType}'.");
            }
            else if (pattern.LeftPattern is { } nestedPattern)
                comparison = EmitSwitchPattern(governingValue, nestedPattern);
            else if (pattern.IsDiscard) comparison = "true";
            else if (pattern.RelationalOperator is { } relationalOperator)
                comparison = EmitSwitchRelational(governingValue, pattern.Value!, relationalOperator);
            else
            {
                var caseValue = EmitExpression(pattern.Value!);
                if (caseValue.Type == "null")
                {
                    // Preserve the null literal's type so nullable aggregate equality
                    // can inspect the presence tag without comparing its payload.
                    comparison = EmitSwitchEquality(governingValue, caseValue);
                }
                else
                {
                    var converted = CastValue(caseValue.Value, caseValue.Type, governingValue.Type);
                    comparison = EmitSwitchEquality(governingValue, (converted, governingValue.Type));
                }
            }
            if (pattern.IsNegated)
            {
                var negated = NewTemp();
                Emit($"{negated} = xor i1 {comparison}, true");
                comparison = negated;
            }
            return comparison;
        }

        private string EmitSwitchPatternBindingValue((string Value, string Type) governingValue, string patternType)
        {
            if (governingValue.Type == "null") return patternType switch
            {
                "bool" => "false",
                "float" or "double" => "0.0",
                _ when IsBoxableScalarOrEnum(patternType) => "0",
                _ when IsBoxableStructType(patternType) => "zeroinitializer",
                _ => "null"
            };
            if (patternType == "string" && IsStringType(governingValue.Type)) return governingValue.Value;
            if (governingValue.Type == patternType) return governingValue.Value;
            if (IsNullableValueType(governingValue.Type) && NullableUnderlyingType(governingValue.Type) == patternType)
            {
                var value = NewTemp();
                Emit($"{value} = extractvalue {LlvmType(governingValue.Type)} {governingValue.Value}, 1");
                return value;
            }
            if (IsRuntimeObjectReference(governingValue.Type) && IsBoxableValueType(patternType))
            {
                if (IsBoxableStructType(patternType)) return LoadBoxedStructValue(governingValue.Value, patternType);
                var payloadAddress = NewTemp(); Emit($"{payloadAddress} = getelementptr %luma.box, ptr {governingValue.Value}, i32 0, i32 4");
                var payload = NewTemp(); Emit($"{payload} = load i64, ptr {payloadAddress}");
                return DecodeBoxPayload(payload, patternType);
            }
            if (IsRuntimeObjectReference(governingValue.Type)) return governingValue.Value;
            throw new InvalidOperationException($"Cannot bind switch pattern value of type '{governingValue.Type}' as '{patternType}'.");
        }

        private static SwitchLabel[] SwitchPatternVariableNodes(SwitchLabel pattern)
        {
            var variables = new List<SwitchLabel>();
            if (pattern.PatternVariableName is not null) variables.Add(pattern);
            if (pattern.LeftPattern is not null) variables.AddRange(SwitchPatternVariableNodes(pattern.LeftPattern));
            if (pattern.RightPattern is not null) variables.AddRange(SwitchPatternVariableNodes(pattern.RightPattern));
            return variables.ToArray();
        }
        private string EmitSwitchRelational((string Value, string Type) governingValue, Expression patternValue, string relationalOperator)
        {
            var comparisonType = governingValue.Type;
            var governingScalar = governingValue.Value;
            string? hasValue = null;
            if (IsNullableValueType(comparisonType))
            {
                var aggregateType = LlvmType(comparisonType);
                hasValue = NewTemp(); Emit($"{hasValue} = extractvalue {aggregateType} {governingValue.Value}, 0");
                governingScalar = NewTemp(); Emit($"{governingScalar} = extractvalue {aggregateType} {governingValue.Value}, 1");
                comparisonType = NullableUnderlyingType(comparisonType);
            }
            var pattern = EmitExpression(patternValue);
            var converted = CastValue(pattern.Value, pattern.Type, comparisonType);
            var scalarComparison = EmitBinaryOperation(relationalOperator, comparisonType, governingScalar, converted).Item1;
            if (hasValue is null) return scalarComparison;
            var comparison = NewTemp(); Emit($"{comparison} = and i1 {hasValue}, {scalarComparison}");
            return comparison;
        }
        private string EmitSwitchEquality((string Value, string Type) governingValue, (string Value, string Type) caseValue)
        {
            if (IsNullableStringType(governingValue.Type) || IsNullableStringType(caseValue.Type))
                return EmitNullableStringEquality("==", governingValue, caseValue).Value;
            if (IsNullableValueType(governingValue.Type) || IsNullableValueType(caseValue.Type))
                return EmitNullableValueEquality("==", governingValue, caseValue).Value;
            if (GetClassReferenceType(governingValue.Type) is not null || SemanticAnalyzer.TryParseManagedArrayType(governingValue.Type, out _, out _))
            {
                var equal = NewTemp();
                Emit($"{equal} = icmp eq ptr {governingValue.Value}, {caseValue.Value}");
                return equal;
            }
            return EmitBinaryOperation("==", governingValue.Type, governingValue.Value, caseValue.Value, caseValue.Type).Item1;
        }
        private void EmitIf(IfStatement statement)
        {
            var patternNodes = GetPositiveTypePatternNodes(statement.Condition);
            if (patternNodes.Length > 0 && patternNodes.All(pattern => typeOperatorTargets.ContainsKey(pattern)))
            {
                EmitIfWithTypePatterns(statement, patternNodes);
                return;
            }
            var condition = EmitExpression(statement.Condition); ReleaseAllOwnedArrays(); ReleaseStatementScopeTemps(); var thenLabel = NewLabel("if.then"); var elseLabel = NewLabel("if.else"); var endLabel = NewLabel("if.end");
            var alternateLabel = statement.Else is null ? endLabel : elseLabel;
            Emit($"br i1 {condition.Value}, label %{thenLabel}, label %{alternateLabel}"); _terminated = true;
            Label(thenLabel); Push(); EmitStatement(statement.Then); Pop(); if (!_terminated) Branch(endLabel);
            if (statement.Else is not null) { Label(elseLabel); Push(); EmitStatement(statement.Else); Pop(); if (!_terminated) Branch(endLabel); }
            Label(endLabel);
        }
        private void EmitIfWithTypePatterns(IfStatement statement, IReadOnlyList<BinaryExpression> patterns)
        {
            var states = new List<(BinaryExpression Pattern, TypePatternBindingState State)>();
            foreach (var pattern in patterns)
            {
                var patternType = typeOperatorTargets[pattern];
                var storageType = IsBoxableValueType(patternType) ? LlvmType(patternType) : "ptr";
                var address = NewTemp(); Emit($"{address} = alloca {storageType}");
                var sourceAddress = NewTemp(); Emit($"{sourceAddress} = alloca ptr");
                var retainedFlag = NewTemp(); Emit($"{retainedFlag} = alloca i1");
                var state = new TypePatternBindingState(patternType, address, sourceAddress, retainedFlag);
                _activeTypePatternBindings.Add(pattern, state);
                states.Add((pattern, state));
            }
            foreach (var (_, state) in states) Emit($"store i1 false, ptr {state.RetainedFlagAddress}");
            var condition = EmitExpression(statement.Condition);
            ReleaseAllOwnedArrays(); ReleaseStatementScopeTemps();
            var thenLabel = NewLabel("if.pattern.then"); var elseLabel = NewLabel("if.pattern.else"); var endLabel = NewLabel("if.pattern.end");
            Emit($"br i1 {condition.Value}, label %{thenLabel}, label %{elseLabel}"); _terminated = true;
            Label(thenLabel); Push();
            var capturedPatternEnvironments = new List<ClosureEnvironmentBinding>();
            foreach (var (pattern, state) in states)
            {
                var capturedEnvironment = InitializeTypePatternBinding(pattern, state);
                if (capturedEnvironment is not null) capturedPatternEnvironments.Add(capturedEnvironment);
                if (state.RetainedClassType is not null && GetClassReferenceType(state.Type) is null && !IsStringType(state.Type) &&
                    (capturedEnvironment is null || !SemanticAnalyzer.TryParseManagedArrayType(state.Type, out _, out _)))
                {
                    var source = NewTemp(); Emit($"{source} = load ptr, ptr {state.SourceAddress}");
                    Emit($"call void @luma_class_release_dynamic(ptr {source})");
                }
            }
            EmitStatement(statement.Then); Pop();
            for (var index = capturedPatternEnvironments.Count - 1; index >= 0; index--)
                _activeBlockClosureEnvironments.RemoveAt(_activeBlockClosureEnvironments.Count - 1);
            if (!_terminated) Branch(endLabel);
            Label(elseLabel);
            foreach (var (_, state) in states) ReleaseTypePatternRetentionIfPresent(state);
            if (statement.Else is not null) { Push(); EmitStatement(statement.Else); Pop(); }
            if (!_terminated) Branch(endLabel);
            Label(endLabel);
            foreach (var (pattern, _) in states) _activeTypePatternBindings.Remove(pattern);
        }
        private static BinaryExpression[] GetPositiveTypePatternNodes(Expression condition)
        {
            if (condition is BinaryExpression { Operator: "&&" } conjunction)
                return GetPositiveTypePatternNodes(conjunction.Left).Concat(GetPositiveTypePatternNodes(conjunction.Right)).ToArray();
            return condition is BinaryExpression { Operator: "is", PatternVariableName: not null } pattern ? [pattern] : [];
        }
        private void CaptureTypePatternOperand(TypePatternBindingState state, (string Value, string Type) operand)
        {
            state.SourceType = operand.Type;
            if (IsManagedArrayType(operand.Type))
            {
                Emit($"store ptr {operand.Value}, ptr {state.SourceAddress}");
                if (operand.Value != "null")
                {
                    Emit($"call void @luma_array_retain(ptr {operand.Value})");
                    Emit($"store i1 true, ptr {state.RetainedFlagAddress}");
                    state.RetainedClassType = "System.Object";
                }
                if (IsManagedArrayType(state.Type) || GetClassReferenceType(state.Type) is not null)
                {
                    Emit($"store ptr {operand.Value}, ptr {state.Address}");
                    state.Initialized = true;
                }
                return;
            }
            if (GetClassReferenceType(operand.Type) is { } sourceClass)
            {
                Emit($"store ptr {operand.Value}, ptr {state.SourceAddress}");
                RetainClassReference(operand.Value, sourceClass);
                Emit($"store i1 true, ptr {state.RetainedFlagAddress}");
                state.RetainedClassType = sourceClass;
                if (GetClassReferenceType(state.Type) is not null)
                {
                    Emit($"store ptr {operand.Value}, ptr {state.Address}");
                    state.Initialized = true;
                }
                return;
            }
            if (IsStringType(operand.Type))
            {
                Emit($"store ptr {operand.Value}, ptr {state.SourceAddress}");
                if (operand.Value != "null")
                {
                    Emit($"call void @luma_string_retain(ptr {operand.Value})");
                    Emit($"store i1 true, ptr {state.RetainedFlagAddress}");
                    state.RetainedString = true;
                }
                if (IsStringType(state.Type) || GetClassReferenceType(state.Type) is not null)
                {
                    Emit($"store ptr {operand.Value}, ptr {state.Address}");
                    state.Initialized = true;
                }
                return;
            }

            var matchesValueType = operand.Type == state.Type ||
                IsNullableValueType(operand.Type) && NullableUnderlyingType(operand.Type) == state.Type;
            var bound = matchesValueType ? EmitSwitchPatternBindingValue(operand, state.Type) : ZeroValue(state.Type);
            if (matchesValueType && IsManagedAggregateType(state.Type)) EmitAggregateOwnership((bound, state.Type), retain: true);
            Emit($"store {LlvmType(state.Type)} {bound}, ptr {state.Address}");
            state.Initialized = true;
        }
        private void InitializeTypePatternBinding(TypePatternBindingState state)
        {
            if (state.Initialized) return;
            if (state.SourceType is null || !IsRuntimeObjectReference(state.SourceType))
                throw new InvalidOperationException("A successful type test has no bound source value.");
            var source = NewTemp(); Emit($"{source} = load ptr, ptr {state.SourceAddress}");
            var boundValue = EmitSwitchPatternBindingValue((source, state.SourceType), state.Type);
            if (IsManagedAggregateType(state.Type)) EmitAggregateOwnership((boundValue, state.Type), retain: true);
            Emit($"store {LlvmType(state.Type)} {boundValue}, ptr {state.Address}");
            state.Initialized = true;
        }
        private ClosureEnvironmentBinding? InitializeTypePatternBinding(BinaryExpression pattern, TypePatternBindingState state,
            bool preservePatternStorage = false)
        {
            InitializeTypePatternBinding(state);
            var name = pattern.PatternVariableName ?? throw new InvalidOperationException("A captured type pattern has no variable name.");
            if (!blockClosureEnvironments.TryGetValue(pattern.PatternCaptureScope, out var environment) ||
                !environment.CapturedFieldIndices.TryGetValue(name, out var fieldIndex))
            {
                Declare(name, state.Type, state.Address,
                    ownsString: IsStringType(state.Type), ownsClass: GetClassReferenceType(state.Type) is not null);
                return null;
            }

            _ = AllocateClosureEnvironment(environment);
            _scopeClassPointers[^1].Add((_closureEnvironmentSlots[environment.TypeName], environment.TypeName));
            _activeBlockClosureEnvironments.Add(environment);
            var capturedValue = NewTemp();
            Emit($"{capturedValue} = load {LlvmType(state.Type)}, ptr {state.Address}");
            var capturedAddress = EmitClosureFieldAddress(environment, fieldIndex);
            if (preservePatternStorage)
            {
                if (IsStringType(state.Type))
                {
                    var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {capturedValue}, null");
                    var retainLabel = NewLabel("pattern.capture.string.retain"); var doneLabel = NewLabel("pattern.capture.string.done");
                    Emit($"br i1 {isNull}, label %{doneLabel}, label %{retainLabel}"); _terminated = true;
                    Label(retainLabel); Emit($"call void @luma_string_retain(ptr {capturedValue})"); Branch(doneLabel); Label(doneLabel);
                }
                else if (SemanticAnalyzer.TryParseManagedArrayType(state.Type, out _, out _))
                {
                    var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {capturedValue}, null");
                    var retainLabel = NewLabel("pattern.capture.array.retain"); var doneLabel = NewLabel("pattern.capture.array.done");
                    Emit($"br i1 {isNull}, label %{doneLabel}, label %{retainLabel}"); _terminated = true;
                    Label(retainLabel); Emit($"call void @luma_array_retain(ptr {capturedValue})"); Branch(doneLabel); Label(doneLabel);
                }
                else if (GetClassReferenceType(state.Type) is { } classType)
                {
                    var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {capturedValue}, null");
                    var retainLabel = NewLabel("pattern.capture.class.retain"); var doneLabel = NewLabel("pattern.capture.class.done");
                    Emit($"br i1 {isNull}, label %{doneLabel}, label %{retainLabel}"); _terminated = true;
                    Label(retainLabel); RetainClassReference(capturedValue, classType); Branch(doneLabel); Label(doneLabel);
                }
                else if (IsManagedAggregateType(state.Type)) EmitAggregateOwnership((capturedValue, state.Type), retain: true);
            }
            Emit($"store {LlvmType(state.Type)} {capturedValue}, ptr {capturedAddress}");
            if (!preservePatternStorage)
                Emit($"store {LlvmType(state.Type)} {DefaultStorageValue(state.Type)}, ptr {state.Address}");
            Declare(name, state.Type, capturedAddress,
                ownsString: false, ownsArray: false, ownsClass: false, ownsAggregate: false);
            return environment;
        }
        private string ZeroValue(string type) => type switch
        {
            "bool" => "false",
            "float" or "double" => "0.0",
            _ when IsBoxableScalarOrEnum(type) => "0",
            _ => "null"
        };
        private void ReleaseTypePatternRetentionIfPresent(TypePatternBindingState state)
        {
            if (state.RetainedClassType is null && !state.RetainedString) return;
            var retained = NewTemp(); Emit($"{retained} = load i1, ptr {state.RetainedFlagAddress}");
            var releaseLabel = NewLabel("pattern.release"); var doneLabel = NewLabel("pattern.release.done");
            Emit($"br i1 {retained}, label %{releaseLabel}, label %{doneLabel}"); _terminated = true;
            Label(releaseLabel);
            var source = NewTemp(); Emit($"{source} = load ptr, ptr {state.SourceAddress}");
            if (state.RetainedString) Emit($"call void @luma_string_release(ptr {source})");
            else Emit($"call void @luma_class_release_dynamic(ptr {source})");
            Branch(doneLabel);
            Label(doneLabel);
        }
        private void EmitWhile(Expression conditionExpression, Statement body, bool doFirst, string? label = null)
        {
            var patternNodes = doFirst ? [] : GetPositiveTypePatternNodes(conditionExpression);
            if (patternNodes.Length > 0 && patternNodes.All(pattern => typeOperatorTargets.ContainsKey(pattern)))
            {
                EmitWhileWithTypePatterns(conditionExpression, body, label, patternNodes);
                return;
            }
            var condition = NewLabel("loop.cond"); var bodyLabel = NewLabel("loop.body"); var end = NewLabel("loop.end");
            if (doFirst) Branch(bodyLabel); else Branch(condition);
            Label(condition); var cond = EmitExpression(conditionExpression); ReleaseAllOwnedArrays(); ReleaseStatementScopeTemps(); Emit($"br i1 {cond.Value}, label %{bodyLabel}, label %{end}"); _terminated = true;
            Label(bodyLabel); var scopeDepth = _scopes.Count; var handlerDepth = _activeExceptionHandlers.Count; var finallyDepth = _activeFinalizers.Count; var tryDepth = _activeTries.Count;
            _loops.Push(new(end, condition, label, scopeDepth, scopeDepth, handlerDepth, handlerDepth, finallyDepth, finallyDepth, tryDepth, tryDepth));
            _breakTargets.Push(new(end, label, scopeDepth, handlerDepth, finallyDepth, tryDepth)); Push(); EmitStatement(body); Pop(); _breakTargets.Pop(); _loops.Pop(); if (!_terminated) Branch(condition);
            Label(end);
        }
        private void EmitWhileWithTypePatterns(Expression conditionExpression, Statement body, string? label, IReadOnlyList<BinaryExpression> patterns)
        {
            var states = new List<(BinaryExpression Pattern, TypePatternBindingState State)>();
            foreach (var pattern in patterns)
            {
                var patternType = typeOperatorTargets[pattern];
                var storageType = IsBoxableValueType(patternType) ? LlvmType(patternType) : "ptr";
                var address = NewTemp(); Emit($"{address} = alloca {storageType}");
                var sourceAddress = NewTemp(); Emit($"{sourceAddress} = alloca ptr");
                var retainedFlag = NewTemp(); Emit($"{retainedFlag} = alloca i1");
                var state = new TypePatternBindingState(patternType, address, sourceAddress, retainedFlag);
                _activeTypePatternBindings.Add(pattern, state);
                states.Add((pattern, state));
            }

            var conditionLabel = NewLabel("loop.pattern.cond"); var bodyLabel = NewLabel("loop.pattern.body");
            var falseLabel = NewLabel("loop.pattern.false"); var end = NewLabel("loop.pattern.end");
            Branch(conditionLabel);
            Label(conditionLabel);
            foreach (var (_, state) in states) Emit($"store i1 false, ptr {state.RetainedFlagAddress}");
            var condition = EmitExpression(conditionExpression);
            ReleaseAllOwnedArrays(); ReleaseStatementScopeTemps();
            Emit($"br i1 {condition.Value}, label %{bodyLabel}, label %{falseLabel}"); _terminated = true;

            Label(falseLabel);
            foreach (var (_, state) in states) ReleaseTypePatternRetentionIfPresent(state);
            Branch(end);

            Label(bodyLabel);
            var scopeDepth = _scopes.Count; var handlerDepth = _activeExceptionHandlers.Count;
            var finallyDepth = _activeFinalizers.Count; var tryDepth = _activeTries.Count;
            _loops.Push(new(end, conditionLabel, label, scopeDepth, scopeDepth, handlerDepth, handlerDepth, finallyDepth, finallyDepth, tryDepth, tryDepth));
            _breakTargets.Push(new(end, label, scopeDepth, handlerDepth, finallyDepth, tryDepth));
            Push();
            var capturedPatternEnvironments = new List<ClosureEnvironmentBinding>();
            foreach (var (pattern, state) in states)
            {
                var capturedEnvironment = InitializeTypePatternBinding(pattern, state);
                if (capturedEnvironment is not null) capturedPatternEnvironments.Add(capturedEnvironment);
                if (state.RetainedClassType is not null && GetClassReferenceType(state.Type) is null && !IsStringType(state.Type) &&
                    (capturedEnvironment is null || !SemanticAnalyzer.TryParseManagedArrayType(state.Type, out _, out _)))
                {
                    var source = NewTemp(); Emit($"{source} = load ptr, ptr {state.SourceAddress}");
                    Emit($"call void @luma_class_release_dynamic(ptr {source})");
                }
            }
            EmitStatement(body); Pop();
            for (var index = capturedPatternEnvironments.Count - 1; index >= 0; index--)
                _activeBlockClosureEnvironments.RemoveAt(_activeBlockClosureEnvironments.Count - 1);
            _breakTargets.Pop(); _loops.Pop(); if (!_terminated) Branch(conditionLabel);
            Label(end);
            foreach (var (pattern, _) in states) _activeTypePatternBindings.Remove(pattern);
        }
        private void EmitDoWhile(DoWhileStatement statement)
        {
            var bodyLabel = NewLabel("do.body"); var conditionLabel = NewLabel("do.cond"); var end = NewLabel("do.end"); Branch(bodyLabel);
            Label(bodyLabel); var scopeDepth = _scopes.Count; var handlerDepth = _activeExceptionHandlers.Count; var finallyDepth = _activeFinalizers.Count; var tryDepth = _activeTries.Count;
            _loops.Push(new(end, conditionLabel, statement.Label, scopeDepth, scopeDepth, handlerDepth, handlerDepth, finallyDepth, finallyDepth, tryDepth, tryDepth));
            _breakTargets.Push(new(end, statement.Label, scopeDepth, handlerDepth, finallyDepth, tryDepth)); Push(); EmitStatement(statement.Body); Pop(); _breakTargets.Pop(); _loops.Pop(); if (!_terminated) Branch(conditionLabel);
            Label(conditionLabel); var cond = EmitExpression(statement.Condition); ReleaseAllOwnedArrays(); ReleaseStatementScopeTemps(); Emit($"br i1 {cond.Value}, label %{bodyLabel}, label %{end}"); _terminated = true; Label(end);
        }
        private void EmitFor(ForStatement statement)
        {
            var outerScopeDepth = _scopes.Count; Push(); var forScopeDepth = _scopes.Count;
            var hasClosureEnvironment = forHeaderClosureEnvironments.TryGetValue(statement, out var forClosureEnvironment);
            if (hasClosureEnvironment)
            {
                _ = AllocateClosureEnvironment(forClosureEnvironment!);
                _scopeClassPointers[^1].Add((_closureEnvironmentSlots[forClosureEnvironment!.TypeName], forClosureEnvironment.TypeName));
                _activeForClosureEnvironments.Add(forClosureEnvironment);
            }
            if (statement.Initializer is not null) EmitStatement(statement.Initializer);
            var conditionLabel = NewLabel("for.cond"); var bodyLabel = NewLabel("for.body"); var stepLabel = NewLabel("for.step"); var end = NewLabel("for.end");
            var patternNodes = statement.Condition is null ? [] : GetPositiveTypePatternNodes(statement.Condition);
            var patternStates = new List<(BinaryExpression Pattern, TypePatternBindingState State)>();
            if (patternNodes.Length > 0 && patternNodes.All(pattern => typeOperatorTargets.ContainsKey(pattern)))
            {
                foreach (var pattern in patternNodes)
                {
                    var patternType = typeOperatorTargets[pattern];
                    var storageType = IsBoxableValueType(patternType) ? LlvmType(patternType) : "ptr";
                    var address = NewTemp(); Emit($"{address} = alloca {storageType}");
                    var sourceAddress = NewTemp(); Emit($"{sourceAddress} = alloca ptr");
                    var retainedFlag = NewTemp(); Emit($"{retainedFlag} = alloca i1");
                    var state = new TypePatternBindingState(patternType, address, sourceAddress, retainedFlag);
                    _activeTypePatternBindings.Add(pattern, state);
                    patternStates.Add((pattern, state));
                }
            }
            var patternFalseLabel = patternStates.Count > 0 ? NewLabel("for.pattern.false") : end;
            Branch(conditionLabel);
            Label(conditionLabel);
            if (patternStates.Count > 0)
                foreach (var (_, state) in patternStates) Emit($"store i1 false, ptr {state.RetainedFlagAddress}");
            if (statement.Condition is null) Branch(bodyLabel);
            else
            {
                var cond = EmitExpression(statement.Condition);
                ReleaseAllOwnedArrays(); ReleaseStatementScopeTemps();
                Emit($"br i1 {cond.Value}, label %{bodyLabel}, label %{patternFalseLabel}"); _terminated = true;
            }
            if (patternStates.Count > 0)
            {
                Label(patternFalseLabel);
                foreach (var (_, state) in patternStates) ReleaseTypePatternRetentionIfPresent(state);
                Branch(end);
            }
            Label(bodyLabel); var continueScopeDepth = _scopes.Count; var handlerDepth = _activeExceptionHandlers.Count; var finallyDepth = _activeFinalizers.Count; var tryDepth = _activeTries.Count;
            _loops.Push(new(end, stepLabel, statement.Label, forScopeDepth, continueScopeDepth, handlerDepth, handlerDepth, finallyDepth, finallyDepth, tryDepth, tryDepth));
            _breakTargets.Push(new(end, statement.Label, forScopeDepth, handlerDepth, finallyDepth, tryDepth)); Push();
            var capturedPatternEnvironments = new List<ClosureEnvironmentBinding>();
            foreach (var (pattern, state) in patternStates)
            {
                var capturedEnvironment = InitializeTypePatternBinding(pattern, state);
                if (capturedEnvironment is not null) capturedPatternEnvironments.Add(capturedEnvironment);
                if (state.RetainedClassType is not null && GetClassReferenceType(state.Type) is null && !IsStringType(state.Type) &&
                    (capturedEnvironment is null || !SemanticAnalyzer.TryParseManagedArrayType(state.Type, out _, out _)))
                {
                    var source = NewTemp(); Emit($"{source} = load ptr, ptr {state.SourceAddress}");
                    Emit($"call void @luma_class_release_dynamic(ptr {source})");
                }
            }
            EmitStatement(statement.Body); Pop();
            for (var index = capturedPatternEnvironments.Count - 1; index >= 0; index--)
                _activeBlockClosureEnvironments.RemoveAt(_activeBlockClosureEnvironments.Count - 1);
            _breakTargets.Pop(); _loops.Pop(); if (!_terminated) Branch(stepLabel);
            Label(stepLabel); foreach (var iterator in statement.Iterators) _ = EmitExpression(iterator); ReleaseStatementScopeTemps(); Branch(conditionLabel); Label(end); Pop();
            foreach (var (pattern, _) in patternStates) _activeTypePatternBindings.Remove(pattern);
            if (hasClosureEnvironment) _activeForClosureEnvironments.RemoveAt(_activeForClosureEnvironments.Count - 1);
        }
        private void EmitForIn(ForInStatement statement)
        {
            var isManagedArray = managedArrayForInElementTypes.TryGetValue(statement, out var managedElementType);
            string arrayType, elementType, length, arrayAddress;
            if (isManagedArray) Push();
            if (isManagedArray)
            {
                elementType = managedElementType!;
                arrayType = "";
                var array = EmitExpression(statement.Collection);
                EnsureOwnedArray(array);
                var heldArray = $"%v{_temp++}"; Emit($"{heldArray} = alloca ptr"); Emit($"store ptr {array.Value}, ptr {heldArray}");
                _ownedArrayValues.Remove(array.Value);
                if (!_scopeBoundArrayValues.Contains(array.Value)) _scopeArrayPointers[^1].Add(heldArray);
                length = EmitManagedArrayLength(array.Value).Value;
                arrayAddress = NewTemp(); Emit($"{arrayAddress} = getelementptr %luma.array, ptr {array.Value}, i32 0, i32 7");
            }
            else if (!TryGetInlineArrayAddress(statement.Collection, out arrayType, out elementType, out var inlineLength, out arrayAddress))
                throw new InvalidOperationException("A for-in loop reached emission without a bound array.");
            else length = inlineLength.ToString(System.Globalization.CultureInfo.InvariantCulture);

            var outerScopeDepth = _scopes.Count;
            var variableType = forInTypes.GetValueOrDefault(statement) ?? elementType;
            var hasClosureEnvironment = forInClosureEnvironments.TryGetValue(statement, out var forInClosureEnvironment);
            var indexPointer = $"%v{_temp++}";
            var variablePointer = $"%v{_temp++}";
            Emit($"{indexPointer} = alloca i32");
            Emit($"store i32 0, ptr {indexPointer}");
            if (!hasClosureEnvironment) Emit($"{variablePointer} = alloca {LlvmType(variableType)}");
            Push();
            if (!hasClosureEnvironment)
            {
                Declare(statement.Name, variableType, variablePointer);
                if (IsStringType(variableType)) Emit($"store ptr {(variableType == "string" ? "@luma_string_empty" : "null")}, ptr {variablePointer}");
                else if (SemanticAnalyzer.TryParseManagedArrayType(variableType, out _, out _)) Emit($"store ptr null, ptr {variablePointer}");
                else if (GetClassReferenceType(variableType) is not null) Emit($"store ptr null, ptr {variablePointer}");
                else if (IsManagedAggregateType(variableType)) Emit($"store {LlvmType(variableType)} zeroinitializer, ptr {variablePointer}");
            }

            var conditionLabel = NewLabel("forin.cond");
            var bodyLabel = NewLabel("forin.body");
            var stepLabel = NewLabel("forin.step");
            var endLabel = NewLabel("forin.end");
            Branch(conditionLabel);
            Label(conditionLabel);
            var index = NewTemp();
            Emit($"{index} = load i32, ptr {indexPointer}");
            var withinLength = NewTemp();
            Emit($"{withinLength} = icmp ult i32 {index}, {length}");
            Emit($"br i1 {withinLength}, label %{bodyLabel}, label %{endLabel}");
            _terminated = true;

            Label(bodyLabel);
            var elementAddress = NewTemp();
            if (isManagedArray) Emit($"{elementAddress} = getelementptr {LlvmType(elementType)}, ptr {arrayAddress}, i32 {index}");
            else Emit($"{elementAddress} = getelementptr {LlvmType(arrayType)}, ptr {arrayAddress}, i32 0, i32 {index}");
            var elementValue = NewTemp();
            Emit($"{elementValue} = load {LlvmType(elementType)}, ptr {elementAddress}");
            var converted = CastValue(elementValue, elementType, variableType);
            if (!hasClosureEnvironment)
            {
                if (IsStringType(variableType)) StoreStringReplacing(variablePointer, (elementValue, elementType));
                else if (SemanticAnalyzer.TryParseManagedArrayType(variableType, out _, out _)) StoreArrayReplacing(variablePointer, (elementValue, elementType), variableType);
                else if (GetClassReferenceType(variableType) is { } loopClass) StoreClassReplacing(variablePointer, (elementValue, elementType), loopClass);
                else if (IsManagedAggregateType(variableType)) StoreAggregateReplacing(variablePointer, (elementValue, elementType), variableType);
                else Emit($"store {LlvmType(variableType)} {converted}, ptr {variablePointer}");
            }

            var continueScopeDepth = _scopes.Count;
            var handlerDepth = _activeExceptionHandlers.Count; var finallyDepth = _activeFinalizers.Count; var tryDepth = _activeTries.Count;
            _loops.Push(new(endLabel, stepLabel, statement.Label, continueScopeDepth, continueScopeDepth, handlerDepth, handlerDepth, finallyDepth, finallyDepth, tryDepth, tryDepth));
            _breakTargets.Push(new(endLabel, statement.Label, continueScopeDepth, handlerDepth, finallyDepth, tryDepth));
            Push();
            if (hasClosureEnvironment)
            {
                _ = AllocateClosureEnvironment(forInClosureEnvironment!);
                _scopeClassPointers[^1].Add((_closureEnvironmentSlots[forInClosureEnvironment!.TypeName], forInClosureEnvironment.TypeName));
                var iterationVariablePointer = EmitClosureFieldAddress(forInClosureEnvironment,
                    forInClosureEnvironment.CapturedFieldIndices[statement.Name]);
                if (IsStringType(variableType)) StoreStringInitial(iterationVariablePointer, (elementValue, elementType));
                else if (SemanticAnalyzer.TryParseManagedArrayType(variableType, out _, out _)) StoreArrayInitial(iterationVariablePointer, (elementValue, elementType), variableType);
                else if (GetClassReferenceType(variableType) is { } loopClass) StoreClassInitial(iterationVariablePointer, (elementValue, elementType), loopClass);
                else if (IsManagedAggregateType(variableType)) StoreAggregateInitial(iterationVariablePointer, (elementValue, elementType), variableType);
                else Emit($"store {LlvmType(variableType)} {converted}, ptr {iterationVariablePointer}");
                Declare(statement.Name, variableType, iterationVariablePointer,
                    ownsString: false, ownsArray: false, ownsClass: false);
            }
            EmitStatement(statement.Body);
            Pop();
            _breakTargets.Pop();
            _loops.Pop();
            if (!_terminated) Branch(stepLabel);

            Label(stepLabel);
            var currentIndex = NewTemp();
            Emit($"{currentIndex} = load i32, ptr {indexPointer}");
            var nextIndex = NewTemp();
            Emit($"{nextIndex} = add i32 {currentIndex}, 1");
            Emit($"store i32 {nextIndex}, ptr {indexPointer}");
            Branch(conditionLabel);
            Label(endLabel);
            Pop();
            if (isManagedArray) Pop();
            ReleaseStatementScopeTemps();
        }
        private (string Value, string Type) EmitExpression(Expression expression)
        {
            cancellationToken.ThrowIfCancellationRequested();
            switch (expression)
            {
                case LiteralExpression literal:
                    if (stringLiterals.TryGetValue(literal, out var stringLiteral)) return ("@" + stringLiteral.Symbol, "string");
                    if (literal.Value is "true" or "false") return (literal.Value == "true" ? "1" : "0", "bool");
                    if (literal.Value == "null") return ("null", "null");
                    if (literal.Value.StartsWith('\'') && SemanticAnalyzer.TryParseCharacter(literal.Value, out var character)) return (character.ToString(), "char");
                    if (SemanticAnalyzer.IsFloatingLiteral(literal.Value)) { var floating = SemanticAnalyzer.ParseFloating(literal.Value); return (FormatFloating(floating.Type, floating.Value), floating.Type); }
                    var parsed = SemanticAnalyzer.ParseInteger(literal.Value); return (parsed.Value?.ToString() ?? "0", parsed.Type);
                case DefaultValueExpression defaultValue:
                    if (!defaultTypes.TryGetValue(defaultValue, out var defaultType)) throw new NotSupportedException("A default expression reached emission without a bound target type.");
                    if (structTypes.ContainsKey(defaultType)) EnsureTypeInitialized(defaultType, force: true);
                    return (DefaultStorageValue(defaultType), defaultType);
                case ThisExpression:
                    if (method.IsClassConstructor || method.IsClassInstanceMethod) return ("%this", QualifiedTypeName(method.Owner));
                    if (method.IsClassFinalizer) return ("%this", QualifiedTypeName(method.Owner));
                    if (!method.IsStructConstructor && !method.IsStructInstanceMethod) throw new NotSupportedException("Only instance constructors and methods have a lowered this value.");
                    var thisType = method.Declaration.ReturnType;
                    var thisValue = NewTemp(); Emit($"{thisValue} = load {LlvmType(thisType)}, ptr %this"); return (thisValue, thisType);
                case BaseExpression:
                    return EmitBaseExpression();
                case NullAssertionExpression assertion:
                    var asserted = EmitExpression(assertion.Operand);
                    if (IsNullableValueType(asserted.Type)) return EmitNullableValueAssertion(asserted);
                    if (IsNullableStringType(asserted.Type))
                    {
                        var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {asserted.Value}, null");
                        TrapIf(isNull, "luma_throw_null");
                        return (asserted.Value, "string");
                    }
                    if (IsNullableArrayType(asserted.Type))
                    {
                        var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {asserted.Value}, null");
                        TrapIf(isNull, "luma_throw_null");
                        return (asserted.Value, NullableUnderlyingType(asserted.Type));
                    }
                    if (IsNullableDelegateType(asserted.Type))
                    {
                        var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {asserted.Value}, null");
                        TrapIf(isNull, "luma_throw_null");
                        return (asserted.Value, NullableUnderlyingType(asserted.Type));
                    }
                    if (GetClassReferenceType(asserted.Type) is not { } assertedClass) throw new NotSupportedException("Only nullable reference/value assertions are currently lowered by the backend.");
                    if (asserted.Type.EndsWith("?", StringComparison.Ordinal))
                    {
                        var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {asserted.Value}, null");
                        TrapIf(isNull, "luma_throw_null");
                    }
                    return (asserted.Value, assertedClass);
                case SizeOfExpression sizeOf:
                    var sizeType = RepresentationType(sizeOfTypes.GetValueOrDefault(sizeOf, sizeOf.TypeName));
                    return ((sizeType.EndsWith('*') ? nativePointerSize : SizeOf(sizeType)).ToString(System.Globalization.CultureInfo.InvariantCulture), "int32");
                case MemberAccessExpression access when access.Member == "Length" && access.Receiver is NameExpression arrayName && TryLookup(arrayName.Name, out var arrayLocal) && SemanticAnalyzer.TryParseInlineArrayType(arrayLocal.Type, out _, out var arrayLength):
                    return (arrayLength.ToString(System.Globalization.CultureInfo.InvariantCulture), "int32");
                case MemberAccessExpression access when access.Member == "Length" && access.Receiver is MemberAccessExpression fieldAccess && structFields.TryGetValue(fieldAccess, out var inlineField) && SemanticAnalyzer.TryParseInlineArrayType(inlineField.TypeName, out _, out var fieldLength):
                    return (fieldLength.ToString(System.Globalization.CultureInfo.InvariantCulture), "int32");
                case MemberAccessExpression access when access.Member == "Length" && access.Receiver is MemberAccessExpression classFieldAccess && classFields.TryGetValue(classFieldAccess, out var inlineClassField) && SemanticAnalyzer.TryParseInlineArrayType(inlineClassField.TypeName, out _, out var classFieldLength):
                    return (classFieldLength.ToString(System.Globalization.CultureInfo.InvariantCulture), "int32");
                case MemberAccessExpression access when access.Member == "Length" && access.Receiver is NameExpression classFieldName && unqualifiedClassFields.TryGetValue(classFieldName, out var inlineUnqualifiedClassField) && SemanticAnalyzer.TryParseInlineArrayType(inlineUnqualifiedClassField.TypeName, out _, out var unqualifiedClassFieldLength):
                    return (unqualifiedClassFieldLength.ToString(System.Globalization.CultureInfo.InvariantCulture), "int32");
                case MemberAccessExpression access when access.Member == "Length" && access.Receiver is MemberAccessExpression staticAccess && staticFields.TryGetValue(staticAccess, out var staticArray) && SemanticAnalyzer.TryParseInlineArrayType(staticArray.TypeName, out _, out var staticLength):
                    EnsureTypeInitialized(staticArray.OwnerType);
                    return (staticLength.ToString(System.Globalization.CultureInfo.InvariantCulture), "int32");
                case MemberAccessExpression access when managedArrayLengthAccesses.ContainsKey(access):
                    return EmitManagedArrayLength(access);
                case MemberAccessExpression access when managedArrayPointerAccesses.TryGetValue(access, out var pointerElementType):
                    return EmitManagedArrayPointer(access, pointerElementType);
                case MemberAccessExpression access when genericSpanEmptyAccesses.TryGetValue(access, out var emptySpanType):
                    return ("zeroinitializer", emptySpanType);
                case MemberAccessExpression access when managedArrayRankAccesses.ContainsKey(access):
                    if (access.IsNullConditional) return EmitNullConditionalArrayRank(access, managedArrayRankAccesses[access]);
                    return managedArrayRankAccesses[access] < 0
                        ? EmitManagedArrayRank(EmitExpression(access.Receiver).Value)
                        : (managedArrayRankAccesses[access].ToString(System.Globalization.CultureInfo.InvariantCulture), "int32");
                case MemberAccessExpression access when access.IsNullConditional && stringLengthAccesses.ContainsKey(access):
                    return EmitNullConditionalStringLength(access);
                case MemberAccessExpression access when stringLengthAccesses.ContainsKey(access):
                    return EmitStringLength(access);
                case MemberAccessExpression access when constantFields.TryGetValue(access, out var field):
                    return (field.Value, field.TypeName);
                case MemberAccessExpression access when access.IsNullConditional && classFields.TryGetValue(access, out var nullableField):
                    return EmitNullConditionalClassField(access, nullableField);
                case MemberAccessExpression access when staticFields.TryGetValue(access, out var field):
                    EnsureTypeInitialized(field.OwnerType);
                    var globalLoad = NewTemp(); Emit($"{globalLoad} = load {LlvmType(field.TypeName)}, ptr @{field.GlobalName}"); return NormalizeString((globalLoad, field.TypeName));
                case MemberAccessExpression access when structFields.TryGetValue(access, out var field):
                    var fieldAddress = EmitStructFieldAddress(access, field);
                    var fieldValue = NewTemp(); Emit($"{fieldValue} = load {LlvmType(field.TypeName)}, ptr {fieldAddress}"); return (fieldValue, field.TypeName);
                case MemberAccessExpression access when classFields.TryGetValue(access, out var field):
                    var classFieldAddress = EmitClassFieldAddress(access, field);
                    var classFieldValue = NewTemp(); Emit($"{classFieldValue} = load {LlvmType(field.TypeName)}, ptr {classFieldAddress}");
                    if (field.IsScopeBound && IsScopeBoundClassExpression(access.Receiver))
                    {
                        if (GetClassReferenceType(field.TypeName) is not null) _scopeBoundClassValues.Add(classFieldValue);
                        else if (SemanticAnalyzer.TryParseManagedArrayType(field.TypeName, out _, out _)) _scopeBoundArrayValues.Add(classFieldValue);
                    }
                    return NormalizeString((classFieldValue, field.TypeName));
                case MemberAccessExpression access when propertyGetterCalls.TryGetValue(access, out var getterCall):
                    return EmitExpression(getterCall);
                case CallExpression call when structConstructors.TryGetValue(call, out var structType):
                    EnsureTypeInitialized(structType, force: true);
                    return call.Arguments.Count == 0 ? ("zeroinitializer", structType) : EmitCall(call);
                case CallExpression call when enumToStringCalls.TryGetValue(call, out var enumTypeName) && StringLiteralCodec.TryCreate(enumTypeName, out var enumName, out _):
                    return ("@" + enumName.Symbol, "string");
                case CallExpression call when classToStringCalls.TryGetValue(call, out var classToStringBinding) && call.Target is MemberAccessExpression target:
                    return EmitClassToString(target.Receiver, target.IsNullConditional, classToStringBinding.CallsBaseImplementation);
                case CallExpression call when structToStringCalls.TryGetValue(call, out var structTypeName) && call.Target is MemberAccessExpression primitiveToStringTarget:
                    if (structTypeName is "bool" or "char" or "float" or "double" || IsIntegerType(structTypeName))
                        return EmitStringConversion(EmitExpression(primitiveToStringTarget.Receiver));
                    return StringLiteralCodec.TryCreate(structTypeName, out var structName, out _)
                        ? ("@" + structName.Symbol, "string")
                        : EmitStringConversion(EmitExpression(primitiveToStringTarget.Receiver));
                case CallExpression call when managedArrayGetLengthCalls.ContainsKey(call):
                    return EmitManagedArrayGetLength(call);
                case AllocationExpression allocation when genericConstructions.TryGetValue(allocation, out var genericConstruction):
                    return genericConstruction.ConstructorCall is { } genericStructConstructor
                        ? EmitExpression(genericStructConstructor)
                        : (DefaultStorageValue(genericConstruction.TypeName), genericConstruction.TypeName);
                case AllocationExpression allocation when managedArrayAllocations.ContainsKey(allocation):
                    return EmitManagedArrayAllocation(allocation);
                case AllocationExpression allocation when classAllocations.ContainsKey(allocation):
                    return EmitClassAllocation(allocation);
                case CallExpression call when call.Target is MemberAccessExpression { IsNullConditional: true }:
                    return EmitNullConditionalCall(call);
                case NameExpression name when propertyGetterNameCalls.TryGetValue(name, out var getterCall):
                    return EmitCall(getterCall);
                case NameExpression name when delegateMethodGroups.TryGetValue(name, out var methodGroup):
                    return EmitDelegateCreation(name, methodGroup);
                case MemberAccessExpression access when delegateMethodGroups.TryGetValue(access, out var methodGroup):
                    return EmitDelegateCreation(access, methodGroup);
                case LambdaExpression lambda when delegateMethodGroups.TryGetValue(lambda, out var lambdaMethodGroup):
                    return EmitDelegateCreation(lambda, lambdaMethodGroup);
                case NameExpression name:
                    if (TryLookup(name.Name, out var local))
                    {
                        var localLoad = NewTemp(); Emit($"{localLoad} = load {LlvmType(local.Type)}, ptr {local.Pointer}");
                        if (local.IsScopeBound)
                        {
                            if (GetClassReferenceType(local.Type) is not null) _scopeBoundClassValues.Add(localLoad);
                            if (SemanticAnalyzer.TryParseManagedArrayType(local.Type, out _, out _)) _scopeBoundArrayValues.Add(localLoad);
                        }
                        return NormalizeString((localLoad, local.Type));
                    }
                    if (unqualifiedConstantFields.TryGetValue(name, out var constantField))
                        return (constantField.Value, constantField.TypeName);
                    if (unqualifiedStaticFields.TryGetValue(name, out var staticField))
                    {
                        EnsureTypeInitialized(staticField.OwnerType);
                        var staticLoad = NewTemp(); Emit($"{staticLoad} = load {LlvmType(staticField.TypeName)}, ptr @{staticField.GlobalName}");
                        return NormalizeString((staticLoad, staticField.TypeName));
                    }
                    if (unqualifiedStructFields.TryGetValue(name, out var boundField))
                    {
                        var unqualifiedAddress = EmitCurrentStructFieldAddress(boundField);
                        var fieldLoad = NewTemp(); Emit($"{fieldLoad} = load {LlvmType(boundField.TypeName)}, ptr {unqualifiedAddress}"); return (fieldLoad, boundField.TypeName);
                    }
                    if (unqualifiedClassFields.TryGetValue(name, out var boundClassField))
                    {
                        var unqualifiedAddress = EmitCurrentClassFieldAddress(boundClassField);
                        var fieldLoad = NewTemp(); Emit($"{fieldLoad} = load {LlvmType(boundClassField.TypeName)}, ptr {unqualifiedAddress}"); return NormalizeString((fieldLoad, boundClassField.TypeName));
                    }
                    throw new NotSupportedException($"No bound local or instance field for '{name.Name}'.");
                case IndexExpression index when stringIndexAccesses.ContainsKey(index):
                    return index.IsNullConditional ? EmitNullConditionalStringIndex(index) : EmitStringIndex(index);
                case IndexExpression index when managedArrayIndexAccesses.ContainsKey(index):
                {
                    if (index.IsNullConditional) return EmitNullConditionalArrayIndex(index, managedArrayIndexAccesses[index]);
                    var (elementAddress, indexedElementType) = EmitIndexAddress(index);
                    var elementValue = NewTemp(); Emit($"{elementValue} = load {LlvmType(indexedElementType)}, ptr {elementAddress}");
                    if (IsScopeBoundArrayExpression(index.Receiver))
                    {
                        if (GetClassReferenceType(indexedElementType) is not null) _scopeBoundClassValues.Add(elementValue);
                        if (SemanticAnalyzer.TryParseManagedArrayType(indexedElementType, out _, out _)) _scopeBoundArrayValues.Add(elementValue);
                    }
                    return NormalizeString((elementValue, indexedElementType));
                }
                case IndexExpression index when indexerGetterCalls.TryGetValue(index, out var getterCall):
                    return EmitExpression(getterCall);
                case IndexExpression index:
                {
                    var (elementAddress, indexedElementType) = EmitIndexAddress(index);
                    var elementValue = NewTemp(); Emit($"{elementValue} = load {LlvmType(indexedElementType)}, ptr {elementAddress}"); return (elementValue, indexedElementType);
                }
                case UnaryExpression unary: return EmitUnary(unary);
                case CastExpression cast:
                    var castOperand = EmitExpression(cast.Operand);
                    var castTarget = castTypes.GetValueOrDefault(cast, cast.TypeName);
                    return (CastValue(castOperand.Item1, castOperand.Item2, castTarget, checkedConversion: _checkedContext), castTarget);
                case BinaryExpression binary: return EmitBinary(binary);
                case ConditionalExpression conditional: return EmitConditional(conditional);
                case CallExpression call: return EmitCall(call);
                case OverflowContextExpression overflow:
                    var previousChecked = _checkedContext; _checkedContext = overflow.Checked;
                    try { return EmitExpression(overflow.Value); } finally { _checkedContext = previousChecked; }
                default: throw new NotSupportedException($"Expression '{expression.GetType().Name}' is not supported.");
            }
        }
        private (string, string) EmitUnary(UnaryExpression expression)
        {
            if (operatorTargets.TryGetValue(expression, out var unaryOperator))
            {
                if (expression.Operator is "++" or "--")
                {
                    var (targetType, targetPointer) = ResolveAssignmentTarget(expression.Operand);
                    var previous = NewTemp(); Emit($"{previous} = load {LlvmType(targetType)}, ptr {targetPointer}");
                    var syntheticCall = new CallExpression(new NameExpression(unaryOperator.Declaration.Name, expression.Location), [expression.Operand], expression.Location);
                    var updated = EmitCall(syntheticCall, releaseOwnedArguments: false,
                        sourceArgumentOverrides: [(previous, targetType)], explicitTarget: unaryOperator);
                    if (GetClassReferenceType(targetType) is { } classType)
                    {
                        if (expression.IsPostfix)
                        {
                            RetainClassReference(previous, classType);
                            _ownedClassValues.Add((previous, classType));
                            _ = StoreClassReplacing(targetPointer, updated, classType);
                            return (previous, targetType);
                        }
                        return StoreClassExpressionResult(targetPointer, updated, classType);
                    }
                    Emit($"store {LlvmType(targetType)} {CastValue(updated.Value, updated.Type, targetType)}, ptr {targetPointer}");
                    return (expression.IsPostfix ? previous : updated.Value, targetType);
                }
                var call = new CallExpression(new NameExpression(unaryOperator.Declaration.Name, expression.Location), [expression.Operand], expression.Location);
                return EmitCall(call, explicitTarget: unaryOperator);
            }
            if (expression.Operator == "&")
            {
                var (addressedType, storage) = ResolveAssignmentTarget(expression.Operand);
                if (classTypes.ContainsKey(addressedType))
                {
                    var instance = NewTemp();
                    Emit($"{instance} = load ptr, ptr {storage}");
                    return (instance, addressedType + "*");
                }
                return (storage, addressedType + "*");
            }
            if (expression.Operator == "*")
            {
                var pointerValue = EmitExpression(expression.Operand); var pointeeType = pointerValue.Type[..^1];
                if (classTypes.ContainsKey(pointeeType))
                {
                    RetainClassReference(pointerValue.Value, pointeeType);
                    _ownedClassValues.Add((pointerValue.Value, pointeeType));
                    return (pointerValue.Value, pointeeType);
                }
                var loadedValue = NewTemp(); Emit($"{loadedValue} = load {LlvmType(pointeeType)}, ptr {pointerValue.Value}");
                return (loadedValue, pointeeType);
            }
            if (expression.Operator is "++" or "--")
            {
                var (type, pointer) = ResolveAssignmentTarget(expression.Operand);
                if (IsNullableValueType(type)) return EmitNullableValueIncrement(expression, type, pointer);
                var old = NewTemp(); Emit($"{old} = load {LlvmType(type)}, ptr {pointer}");
                var arithmetic = expression.Operator == "++" ? "+" : "-";
                var updated = IsPointerType(type) ? EmitPointerOffset(arithmetic, type, old, "1", "int32") :
                    EmitBinaryOperation(arithmetic, type, old, type is "float" or "double" ? "1.0" : "1", type).Item1;
                Emit($"store {LlvmType(type)} {updated}, ptr {pointer}"); return (expression.IsPostfix ? old : updated, type);
            }
            var operand = EmitExpression(expression.Operand); var llvm = LlvmType(operand.Item2);
            if (expression.Operator == "!") { var inverted = NewTemp(); Emit($"{inverted} = xor i1 {operand.Item1}, true"); return (inverted, "bool"); }
            if (IsNullableValueType(operand.Item2) && expression.Operator is "+" or "-" or "~") return EmitNullableValueUnary(expression.Operator, operand);
            var resultType = SemanticAnalyzer.PromoteSmallInteger(operand.Item2); var converted = CastValue(operand.Item1, operand.Item2, resultType); var resultLlvm = LlvmType(resultType);
            if (expression.Operator == "+") return (converted, resultType);
            if (expression.Operator == "-" && resultType is ("float" or "double")) { var negated = NewTemp(); Emit($"{negated} = fneg {resultLlvm} {converted}"); return (negated, resultType); }
            if (expression.Operator == "-") return EmitBinaryOperation("-", resultType, "0", converted, resultType);
            if (expression.Operator == "~") { var inverted = NewTemp(); Emit($"{inverted} = xor {resultLlvm} {converted}, -1"); return (inverted, resultType); }
            throw new NotSupportedException($"Unary operator '{expression.Operator}' is unsupported.");
        }
        private (string Value, string Type) EmitNullableValueUnary(string op, (string Value, string Type) operand)
        {
            var underlying = NullableUnderlyingType(operand.Type); var aggregateType = LlvmType(operand.Type);
            var hasValue = NewTemp(); Emit($"{hasValue} = extractvalue {aggregateType} {operand.Value}, 0");
            var value = NewTemp(); Emit($"{value} = extractvalue {aggregateType} {operand.Value}, 1");
            var valueLabel = NewLabel("lifted.unary.value"); var nullLabel = NewLabel("lifted.unary.null"); var endLabel = NewLabel("lifted.unary.end");
            Emit($"br i1 {hasValue}, label %{valueLabel}, label %{nullLabel}"); _terminated = true;
            Label(valueLabel);
            string computed;
            if (op == "+") computed = value;
            else if (op == "~") { computed = NewTemp(); Emit($"{computed} = xor {LlvmType(underlying)} {value}, -1"); }
            else if (underlying is "float" or "double") { computed = NewTemp(); Emit($"{computed} = fneg {LlvmType(underlying)} {value}"); }
            else computed = EmitBinaryOperation("-", underlying, "0", value).Item1;
            var wrapped = WrapNullableValue(computed, underlying, operand.Type);
            var valueBlock = _block; Branch(endLabel);
            Label(nullLabel); var nullBlock = _block; Branch(endLabel);
            Label(endLabel);
            var result = NewTemp(); Emit($"{result} = phi {aggregateType} [ {wrapped}, %{valueBlock} ], [ zeroinitializer, %{nullBlock} ]");
            return (result, operand.Type);
        }
        private (string Value, string Type) EmitNullableValueIncrement(UnaryExpression expression, string type, string pointer)
        {
            var underlying = NullableUnderlyingType(type); var aggregateType = LlvmType(type);
            var old = NewTemp(); Emit($"{old} = load {aggregateType}, ptr {pointer}");
            var hasValue = NewTemp(); Emit($"{hasValue} = extractvalue {aggregateType} {old}, 0");
            var value = NewTemp(); Emit($"{value} = extractvalue {aggregateType} {old}, 1");
            var valueLabel = NewLabel("lifted.increment.value"); var nullLabel = NewLabel("lifted.increment.null"); var endLabel = NewLabel("lifted.increment.end");
            Emit($"br i1 {hasValue}, label %{valueLabel}, label %{nullLabel}"); _terminated = true;
            Label(valueLabel);
            var updatedValue = EmitBinaryOperation(expression.Operator == "++" ? "+" : "-", underlying, value,
                underlying is "float" or "double" ? "1.0" : "1", underlying).Item1;
            var wrapped = WrapNullableValue(updatedValue, underlying, type);
            Emit($"store {aggregateType} {wrapped}, ptr {pointer}");
            var valueBlock = _block; Branch(endLabel);
            Label(nullLabel); var nullBlock = _block; Branch(endLabel);
            Label(endLabel);
            if (expression.IsPostfix) return (old, type);
            var result = NewTemp(); Emit($"{result} = phi {aggregateType} [ {wrapped}, %{valueBlock} ], [ zeroinitializer, %{nullBlock} ]");
            return (result, type);
        }
        private (string, string) EmitBinary(BinaryExpression expression)
        {
            if (expression.Operator is "is" or "is not") return EmitClassTypeTest(expression);
            if (expression.Operator == "as") return EmitClassConditionalCast(expression);
            if (propertySetterCalls.TryGetValue(expression, out var propertySetterCall))
                return expression.Operator == "=" ? EmitCall(propertySetterCall) : EmitComputedAccessorAssignment(expression, propertySetterCall);
            if (operatorTargets.TryGetValue(expression, out var overloadedOperator))
            {
                var compoundAssignment = expression.Operator is "+=" or "-=" or "*=" or "/=" or "%=" or "&=" or "|=" or "^=" or "<<=" or ">>=";
                string assignmentType = "";
                string assignmentAddress = "";
                (string Value, string Type) overloadedLeft;
                if (compoundAssignment)
                {
                    (assignmentType, assignmentAddress) = ResolveAssignmentTarget(expression.Left);
                    var currentValue = NewTemp(); Emit($"{currentValue} = load {LlvmType(assignmentType)}, ptr {assignmentAddress}");
                    overloadedLeft = (currentValue, assignmentType);
                }
                else overloadedLeft = EmitExpression(expression.Left);
                var overloadedRight = EmitExpression(expression.Right);
                if (overloadedOperator.Declaration.Operator == "==" && (IsNullableValueType(overloadedLeft.Type) || IsNullableValueType(overloadedRight.Type)))
                    return EmitNullableStructEquality(expression, overloadedOperator, overloadedLeft, overloadedRight);
                var syntheticCall = new CallExpression(new NameExpression(overloadedOperator.Declaration.Name, expression.Location), [expression.Left, expression.Right], expression.Location);
                var called = EmitCall(syntheticCall, sourceArgumentOverrides: [overloadedLeft, overloadedRight], explicitTarget: overloadedOperator);
                if (compoundAssignment)
                {
                    if (GetClassReferenceType(assignmentType) is { } classType)
                        return StoreClassExpressionResult(assignmentAddress, called, classType);
                    if (IsManagedAggregateType(assignmentType))
                        return StoreAggregateExpressionResult(assignmentAddress, called, assignmentType);
                    var storedValue = CastValue(called.Value, called.Type, assignmentType);
                    Emit($"store {LlvmType(assignmentType)} {storedValue}, ptr {assignmentAddress}");
                    return (storedValue, assignmentType);
                }
                if (expression.Operator == "!=")
                {
                    var inverted = NewTemp(); Emit($"{inverted} = xor i1 {called.Value}, true"); return (inverted, "bool");
                }
                return called;
            }
            if (expression.Operator is "&&" or "||") return EmitShortCircuit(expression);
            if (expression.Operator == "??") return EmitNullCoalescing(expression);
            if (expression.Operator == "??=")
            {
                var (targetType, pointer) = ResolveAssignmentTarget(expression.Left);
                return EmitNullCoalescingAssignment(targetType, pointer, expression.Right, expression.Left);
            }
            var op = expression.Operator;
            if (op is "=" or "+=" or "-=" or "*=" or "/=" or "%=" or "&=" or "|=" or "^=" or "<<=" or ">>=" or "??=")
            {
                var (targetType, pointer) = ResolveAssignmentTarget(expression.Left);
                if (IsNullableValueType(targetType) && op is "+=" or "-=" or "*=" or "/=" or "%=" or "&=" or "|=" or "^=" or "<<=" or ">>=")
                    return EmitNullableValueCompoundAssignment(expression, targetType, pointer);
                var old = op == "=" ? null : ReadStorage(targetType, pointer);
                var assignmentRight = expression.Right is InlineArrayInitializerExpression inlineInitializer
                    ? EmitInlineArrayInitializer(inlineInitializer, targetType)
                    : EmitExpression(expression.Right);
                if (IsStringType(targetType) && op == "+=")
                {
                    var textRight = EmitStringConversion(assignmentRight);
                    var textLeft = EmitStringConversion((old!, targetType));
                    var concatenated = EmitStringConcatenation(textLeft.Value, textRight.Value);
                    return StoreStringExpressionResult(pointer, concatenated, targetType);
                }
                if (IsStringType(targetType) && op == "=")
                {
                    if (IsScopeBoundLocalTarget(expression.Left))
                        return StoreScopeBoundStringAlias(pointer, assignmentRight, targetType, replaceExisting: true);
                    return StoreStringExpressionResult(pointer, assignmentRight, targetType);
                }
                if (SemanticAnalyzer.TryParseManagedArrayType(targetType, out _, out _) && op == "=")
                {
                    if (IsScopeBoundLocalTarget(expression.Left))
                        return StoreScopeBoundArrayAlias(pointer, assignmentRight, targetType, replaceExisting: true);
                    EnsureOwnedArrayElement(expression.Left, assignmentRight);
                    return StoreArrayExpressionResult(pointer, assignmentRight, targetType);
                }
                if (GetClassReferenceType(targetType) is { } targetClass && op == "=")
                {
                    if (IsScopeBoundLocalTarget(expression.Left))
                        return StoreScopeBoundClassAlias(pointer, assignmentRight, targetClass, replaceExisting: true);
                    EnsureOwnedClassElement(expression.Left, assignmentRight, targetClass);
                    return StoreClassExpressionResult(pointer, assignmentRight, targetClass);
                }
                if (IsManagedAggregateType(targetType) && op == "=")
                    return StoreAggregateExpressionResult(pointer, assignmentRight, targetType);
                var value = assignmentRight.Value;
                if (op == "=") value = CastValue(value, assignmentRight.Type, targetType);
                else if (IsPointerType(targetType) && (op is "+=" or "-=")) value = EmitPointerOffset(op[..^1], targetType, old!, assignmentRight.Value, assignmentRight.Type);
                else
                {
                    var simple = op[..^1];
                    var compoundType = simple is "<<" or ">>" ? SemanticAnalyzer.PromoteSmallInteger(targetType) : IsNumericType(targetType) && IsNumericType(assignmentRight.Type) ? SemanticAnalyzer.NumericPromotionType(targetType, assignmentRight.Type) : targetType;
                    var promotedLeft = CastValue(old!, targetType, compoundType);
                    var promotedRight = simple is "<<" or ">>" ? CastValue(CastValue(assignmentRight.Value, assignmentRight.Type, "int32"), "int32", compoundType) : CastValue(assignmentRight.Value, assignmentRight.Type, compoundType, checkedConversion: enumTypes.ContainsKey(targetType) && assignmentRight.Type != targetType && _checkedContext);
                    var operationResult = EmitBinaryOperation(simple, compoundType, promotedLeft, promotedRight, compoundType).Item1;
                    value = CastValue(operationResult, compoundType, targetType, checkedConversion: _checkedContext);
                }
                Emit($"store {LlvmType(targetType)} {value}, ptr {pointer}"); return (value, targetType);
            }
            var left = EmitExpression(expression.Left); var right = EmitExpression(expression.Right); var type = left.Type;
            if (op is "==" or "!=" && (IsNullableStringType(left.Type) || IsNullableStringType(right.Type)))
                return EmitNullableStringEquality(op, left, right);
            if (op is "==" or "!=" && (IsDelegateType(left.Type) || IsDelegateType(right.Type)))
                return EmitDelegateEquality(op, left, right);
            if (op is "==" or "!=" && (IsManagedArrayType(left.Type) || IsManagedArrayType(right.Type)))
            {
                var arrayType = IsManagedArrayType(left.Type) ? left.Type : right.Type;
                var equalityReferenceClass = GetClassReferenceType(left.Type) ?? GetClassReferenceType(right.Type);
                var nullableReference = left.Type.EndsWith("?", StringComparison.Ordinal) || right.Type.EndsWith("?", StringComparison.Ordinal);
                var comparisonType = equalityReferenceClass is null ? arrayType : equalityReferenceClass + (nullableReference ? "?" : "");
                return EmitBinaryOperation(op, comparisonType, CastValue(left.Value, left.Type, comparisonType), CastValue(right.Value, right.Type, comparisonType));
            }
            if (op is "==" or "!=" && (IsNullableValueType(left.Type) || IsNullableValueType(right.Type) || left.Type == "null" && IsNullableValueType(right.Type) || right.Type == "null" && IsNullableValueType(left.Type)))
                return EmitNullableValueEquality(op, left, right);
            if (IsNullableValueType(left.Type) || IsNullableValueType(right.Type))
                return EmitNullableValueBinary(expression, left, right);
            if (op == "+" && (IsStringType(left.Type) || IsStringType(right.Type)))
            {
                var textLeft = EmitStringConversion(left);
                var textRight = EmitStringConversion(right);
                return EmitStringConcatenation(textLeft.Value, textRight.Value);
            }
            if (IsStringType(left.Type) && IsStringType(right.Type))
            {
                if (op == "+") return EmitStringConcatenation(left.Value, right.Value);
                var result = EmitBinaryOperation(op, "string", left.Value, right.Value);
                ReleaseOwnedString(left);
                ReleaseOwnedString(right);
                return result;
            }
            if (SemanticAnalyzer.TryParseManagedArrayType(left.Type, out _, out _) && left.Type == right.Type && (op is "==" or "!="))
                return EmitBinaryOperation(op, left.Type, left.Value, right.Value);
            var classReferenceType = GetClassReferenceType(left.Type) ?? GetClassReferenceType(right.Type);
            if (classReferenceType is not null && (op is "==" or "!=") &&
                (left.Type == right.Type || right.Type == "null" || left.Type == "null" || GetClassReferenceType(right.Type) == classReferenceType || GetClassReferenceType(left.Type) == classReferenceType))
                return EmitBinaryOperation(op, classReferenceType, left.Value, right.Value);
            if (IsPointerType(left.Type) || IsPointerType(right.Type))
            {
                if (op is "==" or "!=" or "<" or ">" or "<=" or ">=")
                {
                    var pointerType = IsPointerType(left.Type) ? left.Type : right.Type;
                    return EmitBinaryOperation(op, pointerType, CastValue(left.Value, left.Type, pointerType), CastValue(right.Value, right.Type, pointerType));
                }
                if (op == "+" && IsPointerType(left.Type)) return (EmitPointerOffset(op, left.Type, left.Value, right.Value, right.Type), left.Type);
                if (op == "+" && IsPointerType(right.Type)) return (EmitPointerOffset(op, right.Type, right.Value, left.Value, left.Type), right.Type);
                if (op == "-" && IsPointerType(left.Type) && IsPointerType(right.Type))
                {
                    var leftAddress = NewTemp(); Emit($"{leftAddress} = ptrtoint ptr {left.Value} to i64");
                    var rightAddress = NewTemp(); Emit($"{rightAddress} = ptrtoint ptr {right.Value} to i64");
                    var difference = NewTemp(); Emit($"{difference} = sub i64 {leftAddress}, {rightAddress}");
                    var stride = SizeOf(RepresentationType(left.Type[..^1]));
                    if (stride > 1) { var elements = NewTemp(); Emit($"{elements} = sdiv i64 {difference}, {stride}"); return (elements, "int64"); }
                    return (difference, "int64");
                }
                if (op == "-") return (EmitPointerOffset(op, left.Type, left.Value, right.Value, right.Type), left.Type);
            }
            if (op is "<<" or ">>")
            {
                var shiftType = SemanticAnalyzer.PromoteSmallInteger(left.Type);
                var shiftCount = CastValue(right.Value, right.Type, "int32");
                shiftCount = CastValue(shiftCount, "int32", shiftType);
                return EmitBinaryOperation(op, shiftType, CastValue(left.Value, left.Type, shiftType), shiftCount, shiftType);
            }
            var operationType = IsNumericType(left.Type) && IsNumericType(right.Type) ? SemanticAnalyzer.NumericPromotionType(left.Type, right.Type) : type;
            var convertedLeft = CastValue(left.Value, left.Type, operationType);
            var convertedRight = CastValue(right.Value, right.Type, operationType, checkedConversion: enumTypes.ContainsKey(operationType) && right.Type != operationType && _checkedContext);
            return EmitBinaryOperation(op, operationType, convertedLeft, convertedRight, operationType);
        }

        private (string Value, string Type) EmitComputedAccessorAssignment(BinaryExpression expression, CallExpression setterCall)
        {
            var getterCall = expression.Left switch
            {
                MemberAccessExpression access when propertyGetterCalls.TryGetValue(access, out var propertyGetter) => propertyGetter,
                NameExpression name when propertyGetterNameCalls.TryGetValue(name, out var namedGetter) => namedGetter,
                IndexExpression index when indexerGetterCalls.TryGetValue(index, out var indexerGetter) => indexerGetter,
                _ => throw new NotSupportedException("Compound assignment requires a readable computed accessor.")
            };
            if (!callTargets.TryGetValue(getterCall, out var getterTarget) || !callTargets.TryGetValue(setterCall, out var setterTarget))
                throw new InvalidOperationException("Computed assignment accessors were not bound.");

            string? receiverOverride = null;
            (string Value, string Type)? ownedReceiver = null;
            if (getterTarget.IsClassInstanceMethod && getterCall.Target is MemberAccessExpression classTarget && classTarget.Receiver is not BaseExpression)
            {
                var receiver = EmitExpression(classTarget.Receiver);
                if (GetClassReferenceType(receiver.Type) is { } receiverClass)
                {
                    EnsureOwnedClass(receiver, receiverClass);
                    ownedReceiver = receiver;
                }
                receiverOverride = receiver.Value;
            }
            else if (getterTarget.IsStructInstanceMethod && getterCall.Target is MemberAccessExpression structTarget)
                (_, receiverOverride) = ResolveNullableStructAddress(structTarget.Receiver);

            var indexArguments = expression.Left is IndexExpression indexed ? indexed.Indices.Select(EmitExpression).ToArray() : [];
            var beforeGetterStrings = _ownedStringValues.ToArray();
            var beforeGetterArrays = _ownedArrayValues.ToArray();
            var beforeGetterClasses = _ownedClassValues.ToArray();
            var beforeGetterAggregates = _ownedAggregateValues.ToArray();
            var beforeGetterScopedClasses = _scopeBoundClassValues.ToArray();
            var beforeGetterScopedArrays = _scopeBoundArrayValues.ToArray();
            var oldValue = EmitCall(getterCall, receiverOverride, releaseOwnedArguments: false, sourceArgumentOverrides: indexArguments);
            if (expression.Operator == "??=")
                return EmitComputedNullCoalescingAssignment(expression, setterCall, receiverOverride, ownedReceiver, indexArguments, oldValue,
                    beforeGetterStrings, beforeGetterArrays, beforeGetterClasses, beforeGetterScopedClasses, beforeGetterScopedArrays, beforeGetterAggregates);
            var right = EmitExpression(expression.Right);
            (string Value, string Type) computed;

            if (operatorTargets.TryGetValue(expression, out var overloadedOperator))
            {
                var syntheticCall = new CallExpression(new NameExpression(overloadedOperator.Declaration.Name, expression.Location), [expression.Left, expression.Right], expression.Location);
                computed = EmitCall(syntheticCall, sourceArgumentOverrides: [oldValue, right], explicitTarget: overloadedOperator);
            }
            else if (IsNullableValueType(oldValue.Type))
            {
                var temporary = NewTemp();
                Emit($"{temporary} = alloca {LlvmType(oldValue.Type)}");
                Emit($"store {LlvmType(oldValue.Type)} {oldValue.Value}, ptr {temporary}");
                computed = EmitNullableValueCompoundAssignment(expression, oldValue.Type, temporary, right);
            }
            else if (IsStringType(oldValue.Type) && expression.Operator == "+=")
            {
                var leftText = EmitStringConversion(oldValue);
                var rightText = EmitStringConversion(right);
                computed = EmitStringConcatenation(leftText.Value, rightText.Value);
            }
            else
            {
                var simpleOperator = expression.Operator[..^1];
                if (IsPointerType(oldValue.Type) && simpleOperator is "+" or "-")
                    computed = (EmitPointerOffset(simpleOperator, oldValue.Type, oldValue.Value, right.Value, right.Type), oldValue.Type);
                else
                {
                    var operationType = simpleOperator is "<<" or ">>" ? SemanticAnalyzer.PromoteSmallInteger(oldValue.Type) :
                        IsNumericType(oldValue.Type) && IsNumericType(right.Type) ? SemanticAnalyzer.NumericPromotionType(oldValue.Type, right.Type) : oldValue.Type;
                    var left = CastValue(oldValue.Value, oldValue.Type, operationType);
                    var convertedRight = simpleOperator is "<<" or ">>"
                        ? CastValue(CastValue(right.Value, right.Type, "int32"), "int32", operationType)
                        : CastValue(right.Value, right.Type, operationType,
                            checkedConversion: enumTypes.ContainsKey(oldValue.Type) && right.Type != oldValue.Type && _checkedContext);
                    var result = EmitBinaryOperation(simpleOperator, operationType, left, convertedRight, operationType).Item1;
                    computed = (CastValue(result, operationType, oldValue.Type, checkedConversion: _checkedContext), oldValue.Type);
                }
            }

            var setterArguments = indexArguments.Append(computed).ToArray();
            var assigned = EmitCall(setterCall, receiverOverride, sourceArgumentOverrides: setterArguments);
            if (ownedReceiver is { } owner) ReleaseOwnedClass(owner);
            return assigned;
        }

        private (string Value, string Type) EmitComputedNullCoalescingAssignment(BinaryExpression expression, CallExpression setterCall,
            string? receiverOverride, (string Value, string Type)? ownedReceiver, IReadOnlyList<(string Value, string Type)> indexArguments,
            (string Value, string Type) oldValue, IReadOnlyCollection<string> beforeGetterStrings,
            IReadOnlyCollection<string> beforeGetterArrays, IReadOnlyCollection<(string Value, string Type)> beforeGetterClasses,
            IReadOnlyCollection<string> beforeGetterScopedClasses, IReadOnlyCollection<string> beforeGetterScopedArrays,
            IReadOnlyCollection<(string Value, string Type)> beforeGetterAggregates)
        {
            string isNull;
            if (IsNullableValueType(oldValue.Type))
            {
                var aggregateType = LlvmType(oldValue.Type);
                var hasValue = NewTemp(); Emit($"{hasValue} = extractvalue {aggregateType} {oldValue.Value}, 0");
                isNull = NewTemp(); Emit($"{isNull} = xor i1 {hasValue}, true");
            }
            else if (IsStringType(oldValue.Type) || SemanticAnalyzer.TryParseManagedArrayType(oldValue.Type, out _, out _) || GetClassReferenceType(oldValue.Type) is not null)
            {
                isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {oldValue.Value}, null");
            }
            else throw new NotSupportedException($"Computed '??=' is not supported for '{oldValue.Type}'.");

            var nullLabel = NewLabel("computed.coalesce.null");
            var valueLabel = NewLabel("computed.coalesce.value");
            var endLabel = NewLabel("computed.coalesce.end");
            Emit($"br i1 {isNull}, label %{nullLabel}, label %{valueLabel}"); _terminated = true;

            Label(nullLabel);
            var right = EmitExpression(expression.Right);
            var setterArguments = indexArguments.Append((CastValue(right.Value, right.Type, oldValue.Type), oldValue.Type)).ToArray();
            var assigned = EmitCall(setterCall, receiverOverride, sourceArgumentOverrides: setterArguments);
            ReleaseOwnedValue(oldValue);
            if (ownedReceiver is { } nullPathReceiver) ReleaseOwnedClass(nullPathReceiver);
            var assignedBlock = _block;
            Branch(endLabel);

            Label(valueLabel);
            RestoreOwnedTemporaryState(beforeGetterStrings, beforeGetterArrays, beforeGetterClasses,
                beforeGetterScopedClasses, beforeGetterScopedArrays, beforeGetterAggregates);
            if (IsStringType(oldValue.Type)) _ownedStringValues.Add(oldValue.Value);
            else if (SemanticAnalyzer.TryParseManagedArrayType(oldValue.Type, out _, out _)) _ownedArrayValues.Add(oldValue.Value);
            else if (GetClassReferenceType(oldValue.Type) is { } oldClass) _ownedClassValues.Add((oldValue.Value, oldClass));
            else if (IsManagedAggregateType(oldValue.Type)) _ownedAggregateValues.Add(oldValue);
            foreach (var argument in indexArguments) ReleaseOwnedValue(argument);
            if (ownedReceiver is { } valuePathReceiver) ReleaseOwnedClass(valuePathReceiver);
            var oldBlock = _block;
            Branch(endLabel);

            Label(endLabel);
            var result = NewTemp(); Emit($"{result} = phi {LlvmType(oldValue.Type)} [ {assigned.Value}, %{assignedBlock} ], [ {oldValue.Value}, %{oldBlock} ]");
            if (IsStringType(oldValue.Type))
            {
                _ownedStringValues.Remove(oldValue.Value); _ownedStringValues.Remove(assigned.Value); _ownedStringValues.Add(result);
            }
            else if (SemanticAnalyzer.TryParseManagedArrayType(oldValue.Type, out _, out _))
            {
                _ownedArrayValues.Remove(oldValue.Value); _ownedArrayValues.Remove(assigned.Value); _ownedArrayValues.Add(result);
            }
            else if (GetClassReferenceType(oldValue.Type) is { } resultClass)
            {
                _ownedClassValues.Remove((oldValue.Value, resultClass)); _ownedClassValues.Remove((assigned.Value, resultClass));
                _ownedClassValues.Add((result, resultClass));
            }
            return (result, oldValue.Type);
        }

        private void ReleaseOwnedValue((string Value, string Type) value)
        {
            if (IsStringType(value.Type)) ReleaseOwnedString(value);
            else if (SemanticAnalyzer.TryParseManagedArrayType(value.Type, out _, out _)) ReleaseOwnedArray(value);
            else if (IsManagedAggregateType(value.Type)) ReleaseOwnedAggregate(value);
            else ReleaseOwnedClass(value);
        }

        private (string Value, string Type) EmitClassTypeTest(BinaryExpression expression, (string Value, string Type)? suppliedOperand = null)
        {
            if (!typeOperatorTargets.TryGetValue(expression, out var targetType))
                throw new InvalidOperationException("A validated type test is missing its target binding.");
            var operand = suppliedOperand ?? EmitExpression(expression.Left);
            if (expression.PatternVariableName is not null && _activeTypePatternBindings.TryGetValue(expression, out var patternBinding))
                CaptureTypePatternOperand(patternBinding, operand);
            string result;
            if (targetType == "null")
            {
                if (operand.Type == "null") result = "1";
                else
                {
                    var referenceType = GetClassReferenceType(operand.Type);
                    if (referenceType is null && !IsStringType(operand.Type) && !IsManagedArrayType(operand.Type))
                        throw new NotSupportedException($"Null type tests are unsupported for '{operand.Type}'.");
                    result = NewTemp();
                    Emit($"{result} = icmp eq ptr {operand.Value}, null");
                }
            }
            else if (IsBoxableScalarOrEnum(targetType))
            {
                if (operand.Type == "null") result = "false";
                else if (IsBoxableScalarOrEnum(operand.Type)) result = operand.Type == targetType ? "true" : "false";
                else if (IsNullableValueType(operand.Type))
                {
                    if (NullableUnderlyingType(operand.Type) != targetType) result = "false";
                    else
                    {
                        var hasValue = NewTemp(); Emit($"{hasValue} = extractvalue {LlvmType(operand.Type)} {operand.Value}, 0");
                        result = hasValue;
                    }
                }
                else
                {
                    if (!IsRuntimeObjectReference(operand.Type))
                        throw new NotSupportedException($"Runtime value type tests from '{operand.Type}' were not validated.");
                    result = NewTemp();
                    Emit($"{result} = call i1 @luma_class_is_instance(ptr {operand.Value}, ptr {BoxDescriptorPointer(targetType)})");
                }
            }
            else if (IsBoxableStructType(targetType))
            {
                if (operand.Type == "null") result = "false";
                else if (operand.Type == targetType) result = "true";
                else if (IsNullableValueType(operand.Type))
                {
                    if (NullableUnderlyingType(operand.Type) != targetType) result = "false";
                    else
                    {
                        result = NewTemp();
                        Emit($"{result} = extractvalue {LlvmType(operand.Type)} {operand.Value}, 0");
                    }
                }
                else if (structTypes.ContainsKey(operand.Type) || IsBoxableScalarOrEnum(operand.Type)) result = "false";
                else if (IsRuntimeObjectReference(operand.Type))
                {
                    result = NewTemp();
                    Emit($"{result} = call i1 @luma_class_is_instance(ptr {operand.Value}, ptr {BoxDescriptorPointer(targetType)})");
                }
                else throw new NotSupportedException($"Runtime struct type tests from '{operand.Type}' were not validated.");
            }
            else if (targetType == "string")
            {
                if (operand.Type == "null") result = "false";
                else if (IsStringType(operand.Type))
                {
                    result = NewTemp();
                    Emit($"{result} = icmp ne ptr {operand.Value}, null");
                }
                else if (IsRuntimeObjectReference(operand.Type))
                {
                    result = NewTemp();
                    Emit($"{result} = call i1 @luma_class_is_instance(ptr {operand.Value}, ptr @luma_string_type)");
                }
                else throw new NotSupportedException($"Runtime string type tests from '{operand.Type}' were not validated.");
            }
            else if (IsManagedArrayType(targetType))
            {
                if (operand.Type == "null" || IsStringType(operand.Type)) result = "false";
                else if (IsRuntimeObjectReference(operand.Type))
                {
                    result = NewTemp();
                    Emit($"{result} = call i1 @luma_class_is_instance(ptr {operand.Value}, ptr {ArrayDescriptorPointer(targetType)})");
                }
                else throw new NotSupportedException($"Runtime array type tests from '{operand.Type}' were not validated.");
            }
            else
            {
                var classType = GetClassReferenceType(targetType) ?? throw new NotSupportedException("The bound type-test target is not a reference type.");
                result = NewTemp();
                if (operand.Type == "null") Emit($"{result} = icmp ne ptr null, null");
                else if (interfaceTypes.ContainsKey(classType))
                    Emit($"{result} = call i1 @luma_class_implements(ptr {operand.Value}, ptr @luma_interfacedesc_{Safe(classType)})");
                else if (IsManagedArrayType(operand.Type) && classType == "System.Object")
                    Emit($"{result} = call i1 @luma_class_is_instance(ptr {operand.Value}, ptr {ClassDescriptorPointer(classType)})");
                else Emit($"{result} = call i1 @luma_class_is_instance(ptr {operand.Value}, ptr {ClassDescriptorPointer(classType)})");
            }
            if (expression.Operator == "is not")
            {
                var inverted = NewTemp(); Emit($"{inverted} = xor i1 {result}, true"); result = inverted;
            }
            if (IsManagedArrayType(operand.Type)) ReleaseOwnedArray(operand);
            else if (IsStringType(operand.Type)) ReleaseOwnedString(operand);
            else ReleaseOwnedClass(operand);
            return (result, "bool");
        }

        private (string Value, string Type) EmitClassConditionalCast(BinaryExpression expression)
        {
            if (!typeOperatorTargets.TryGetValue(expression, out var targetType))
                throw new InvalidOperationException("A validated conditional cast is missing its class target binding.");
            if (IsBoxableScalarOrEnum(targetType)) return EmitBoxConditionalCast(expression, targetType);
            if (IsBoxableStructType(targetType)) return EmitStructConditionalCast(expression, targetType);
            if (IsManagedArrayType(targetType)) return EmitArrayConditionalCast(expression, targetType);
            if (targetType == "string") return EmitStringConditionalCast(expression);
            if (GetClassReferenceType(targetType) is not { } targetClass)
                throw new InvalidOperationException("A validated conditional cast target is not a supported reference type.");
            var operand = EmitExpression(expression.Left);
            var sourceClass = GetClassReferenceType(operand.Type) ??
                (IsManagedArrayType(operand.Type) || IsStringType(operand.Type) ? "System.Object" : null);
            var provenUpcast = sourceClass is not null && (sourceClass == targetClass || IsClassAssignable(sourceClass, targetClass) ||
                interfaceConversions.Contains((sourceClass, targetClass)));
            var resultType = provenUpcast && !operand.Type.EndsWith("?", StringComparison.Ordinal) ? targetClass : targetClass + "?";
            if (operand.Type == "null") return ("null", resultType);
            if (sourceClass is null) throw new NotSupportedException($"Conditional class casts from '{operand.Type}' were not validated.");

            EnsureOwnedClass(operand, sourceClass);
            if (provenUpcast)
            {
                _ownedClassValues.Remove((operand.Value, sourceClass));
                _ownedClassValues.Add((operand.Value, targetClass));
                return (operand.Value, resultType);
            }

            var matches = NewTemp();
            if (interfaceTypes.ContainsKey(targetClass))
                Emit($"{matches} = call i1 @luma_class_implements(ptr {operand.Value}, ptr @luma_interfacedesc_{Safe(targetClass)})");
            else
                Emit($"{matches} = call i1 @luma_class_is_instance(ptr {operand.Value}, ptr {ClassDescriptorPointer(targetClass)})");
            var successLabel = NewLabel("as.success"); var failureLabel = NewLabel("as.failure"); var endLabel = NewLabel("as.end");
            Emit($"br i1 {matches}, label %{successLabel}, label %{failureLabel}"); _terminated = true;
            Label(successLabel); var successBlock = _block; Branch(endLabel);
            Label(failureLabel); Emit($"call void @luma_class_release_dynamic(ptr {operand.Value})"); var failureBlock = _block; Branch(endLabel);
            Label(endLabel);
            var result = NewTemp(); Emit($"{result} = phi ptr [ {operand.Value}, %{successBlock} ], [ null, %{failureBlock} ]");
            _ownedClassValues.Remove((operand.Value, sourceClass));
            _ownedClassValues.Add((result, targetClass));
            return (result, resultType);
        }

        private (string Value, string Type) EmitArrayConditionalCast(BinaryExpression expression, string targetArrayType)
        {
            var operand = EmitExpression(expression.Left);
            var resultType = targetArrayType + "?";
            if (operand.Type == "null") return ("null", resultType);
            if (IsStringType(operand.Type))
            {
                ReleaseOwnedString(operand);
                return ("null", resultType);
            }
            if (IsManagedArrayType(operand.Type) && NullableUnderlyingType(operand.Type) == targetArrayType &&
                !operand.Type.EndsWith("?", StringComparison.Ordinal))
                return (operand.Value, targetArrayType);

            var sourceArray = IsManagedArrayType(operand.Type);
            var sourceClass = GetClassReferenceType(operand.Type);
            if (sourceArray) EnsureOwnedArray(operand);
            else if (sourceClass is not null) EnsureOwnedClass(operand, sourceClass);
            else throw new NotSupportedException($"Conditional array casts from '{operand.Type}' were not validated.");

            var matches = NewTemp();
            Emit($"{matches} = call i1 @luma_class_is_instance(ptr {operand.Value}, ptr {ArrayDescriptorPointer(targetArrayType)})");
            var successLabel = NewLabel("as.array.success"); var failureLabel = NewLabel("as.array.failure"); var endLabel = NewLabel("as.array.end");
            Emit($"br i1 {matches}, label %{successLabel}, label %{failureLabel}"); _terminated = true;
            Label(successLabel); var successBlock = _block; Branch(endLabel);
            Label(failureLabel);
            if (sourceArray) Emit($"call void @luma_array_release(ptr {operand.Value})");
            else Emit($"call void @luma_class_release_dynamic(ptr {operand.Value})");
            var failureBlock = _block; Branch(endLabel);
            Label(endLabel);
            var result = NewTemp(); Emit($"{result} = phi ptr [ {operand.Value}, %{successBlock} ], [ null, %{failureBlock} ]");
            _ownedArrayValues.Remove(operand.Value);
            if (sourceClass is not null) _ownedClassValues.Remove((operand.Value, sourceClass));
            _ownedArrayValues.Add(result);
            return (result, resultType);
        }

        private (string Value, string Type) EmitStringConditionalCast(BinaryExpression expression)
        {
            var operand = EmitExpression(expression.Left);
            if (operand.Type == "null") return ("null", "string?");
            if (IsStringType(operand.Type))
            {
                EnsureOwnedString(operand);
                return (operand.Value, operand.Type == "string" ? "string" : "string?");
            }
            if (IsManagedArrayType(operand.Type))
            {
                ReleaseOwnedArray(operand);
                return ("null", "string?");
            }
            var sourceClass = GetClassReferenceType(operand.Type);
            if (sourceClass is null) throw new NotSupportedException($"Conditional string casts from '{operand.Type}' were not validated.");
            EnsureOwnedClass(operand, sourceClass);
            var matches = NewTemp();
            Emit($"{matches} = call i1 @luma_class_is_instance(ptr {operand.Value}, ptr @luma_string_type)");
            var successLabel = NewLabel("as.string.success"); var failureLabel = NewLabel("as.string.failure"); var endLabel = NewLabel("as.string.end");
            Emit($"br i1 {matches}, label %{successLabel}, label %{failureLabel}"); _terminated = true;
            Label(successLabel); var successBlock = _block; Branch(endLabel);
            Label(failureLabel); Emit($"call void @luma_class_release_dynamic(ptr {operand.Value})"); var failureBlock = _block; Branch(endLabel);
            Label(endLabel);
            var result = NewTemp(); Emit($"{result} = phi ptr [ {operand.Value}, %{successBlock} ], [ null, %{failureBlock} ]");
            _ownedClassValues.Remove((operand.Value, sourceClass));
            _ownedStringValues.Add(result);
            return (result, "string?");
        }

        private (string Value, string Type) EmitBoxConditionalCast(BinaryExpression expression, string targetType)
        {
            var operand = EmitExpression(expression.Left);
            var resultType = targetType + "?";
            if (operand.Type == "null") return ("zeroinitializer", resultType);
            if (!IsRuntimeObjectReference(operand.Type))
                throw new NotSupportedException($"Conditional unboxing from '{operand.Type}' was not validated.");

            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {operand.Value}, null");
            var descriptorLabel = NewLabel("as.box.check"); var nullLabel = NewLabel("as.box.null"); var endLabel = NewLabel("as.box.end");
            Emit($"br i1 {isNull}, label %{nullLabel}, label %{descriptorLabel}"); _terminated = true;
            Label(descriptorLabel);
            var descriptor = NewTemp(); Emit($"{descriptor} = load ptr, ptr {operand.Value}");
            var matches = NewTemp(); Emit($"{matches} = icmp eq ptr {descriptor}, {BoxDescriptorPointer(targetType)}");
            var successLabel = NewLabel("as.box.success"); var failedLabel = NewLabel("as.box.failed");
            Emit($"br i1 {matches}, label %{successLabel}, label %{failedLabel}"); _terminated = true;
            Label(successLabel);
            var payloadAddress = NewTemp(); Emit($"{payloadAddress} = getelementptr %luma.box, ptr {operand.Value}, i32 0, i32 4");
            var payload = NewTemp(); Emit($"{payload} = load i64, ptr {payloadAddress}");
            var unboxed = DecodeBoxPayload(payload, targetType);
            var wrapped = WrapNullableValue(unboxed, targetType, resultType);
            var successBlock = _block; Branch(endLabel);
            Label(failedLabel); var failedBlock = _block; Branch(endLabel);
            Label(nullLabel); var nullBlock = _block; Branch(endLabel);
            Label(endLabel);
            var result = NewTemp(); Emit($"{result} = phi {LlvmType(resultType)} [ {wrapped}, %{successBlock} ], [ zeroinitializer, %{failedBlock} ], [ zeroinitializer, %{nullBlock} ]");
            if (IsManagedArrayType(operand.Type)) ReleaseOwnedArray(operand);
            else if (IsStringType(operand.Type)) ReleaseOwnedString(operand);
            else ReleaseOwnedClass(operand);
            return (result, resultType);
        }

        private (string Value, string Type) EmitStructConditionalCast(BinaryExpression expression, string targetType)
        {
            var operand = EmitExpression(expression.Left);
            var resultType = targetType + "?";
            if (operand.Type == "null") return ("zeroinitializer", resultType);
            if (!IsRuntimeObjectReference(operand.Type))
                throw new NotSupportedException($"Conditional struct unboxing from '{operand.Type}' was not validated.");
            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {operand.Value}, null");
            var checkLabel = NewLabel("as.struct.check");
            var nullLabel = NewLabel("as.struct.null");
            var endLabel = NewLabel("as.struct.end");
            Emit($"br i1 {isNull}, label %{nullLabel}, label %{checkLabel}"); _terminated = true;
            Label(checkLabel);
            var descriptor = NewTemp(); Emit($"{descriptor} = load ptr, ptr {operand.Value}");
            var matches = NewTemp(); Emit($"{matches} = icmp eq ptr {descriptor}, {BoxDescriptorPointer(targetType)}");
            var successLabel = NewLabel("as.struct.success");
            var failedLabel = NewLabel("as.struct.failed");
            Emit($"br i1 {matches}, label %{successLabel}, label %{failedLabel}"); _terminated = true;
            Label(successLabel);
            var unboxed = LoadBoxedStructValue(operand.Value, targetType);
            var wrapped = WrapNullableValue(unboxed, targetType, resultType);
            var successBlock = _block; Branch(endLabel);
            Label(failedLabel); var failedBlock = _block; Branch(endLabel);
            Label(nullLabel); var nullBlock = _block; Branch(endLabel);
            Label(endLabel);
            var result = NewTemp();
            Emit($"{result} = phi {LlvmType(resultType)} [ {wrapped}, %{successBlock} ], [ zeroinitializer, %{failedBlock} ], [ zeroinitializer, %{nullBlock} ]");
            if (IsManagedArrayType(operand.Type)) ReleaseOwnedArray(operand);
            else if (IsStringType(operand.Type)) ReleaseOwnedString(operand);
            else ReleaseOwnedClass(operand);
            return (result, resultType);
        }

        private (string Value, string Type) EmitNullableStructEquality(BinaryExpression expression, MethodSymbol equalityOperator, (string Value, string Type) left, (string Value, string Type) right)
        {
            var equalityType = equalityOperator.Declaration.Parameters[0].TypeName;
            string leftHas = "true"; string rightHas = "true"; var leftValue = left.Value; var rightValue = right.Value;
            var leftNullable = IsNullableValueType(left.Type); var rightNullable = IsNullableValueType(right.Type);
            if (leftNullable)
            {
                leftHas = NewTemp(); Emit($"{leftHas} = extractvalue {LlvmType(left.Type)} {left.Value}, 0");
                leftValue = NewTemp(); Emit($"{leftValue} = extractvalue {LlvmType(left.Type)} {left.Value}, 1");
            }
            if (rightNullable)
            {
                rightHas = NewTemp(); Emit($"{rightHas} = extractvalue {LlvmType(right.Type)} {right.Value}, 0");
                rightValue = NewTemp(); Emit($"{rightValue} = extractvalue {LlvmType(right.Type)} {right.Value}, 1");
            }
            if (!leftNullable && !rightNullable)
                throw new InvalidOperationException("Nullable struct equality lowering requires at least one nullable operand.");

            var bothPresent = NewTemp(); Emit($"{bothPresent} = and i1 {leftHas}, {rightHas}");
            var callLabel = NewLabel("nullable.struct.eq.call"); var emptyLabel = NewLabel("nullable.struct.eq.empty"); var endLabel = NewLabel("nullable.struct.eq.end");
            Emit($"br i1 {bothPresent}, label %{callLabel}, label %{emptyLabel}"); _terminated = true;

            Label(callLabel);
            var equalityValue = NewTemp();
            Emit($"{equalityValue} = call i1 @{FunctionName(equalityOperator, methods)}({LlvmType(equalityType)} {leftValue}, {LlvmType(equalityType)} {rightValue})");
            var callBlock = _block; Branch(endLabel);

            Label(emptyLabel);
            var equalPresence = NewTemp(); Emit($"{equalPresence} = icmp eq i1 {leftHas}, {rightHas}");
            var emptyBlock = _block; Branch(endLabel);

            Label(endLabel);
            var equal = NewTemp(); Emit($"{equal} = phi i1 [ {equalityValue}, %{callBlock} ], [ {equalPresence}, %{emptyBlock} ]");
            if (expression.Operator == "==") return (equal, "bool");
            var inverted = NewTemp(); Emit($"{inverted} = xor i1 {equal}, true"); return (inverted, "bool");
        }
        private (string Value, string Type) EmitNullableValueEquality(string op, (string Value, string Type) left, (string Value, string Type) right)
        {
            if (left.Type == "null" || right.Type == "null")
            {
                var nullable = left.Type == "null" ? right : left;
                var hasValue = NewTemp(); Emit($"{hasValue} = extractvalue {LlvmType(nullable.Type)} {nullable.Value}, 0");
                var isNull = NewTemp(); Emit($"{isNull} = xor i1 {hasValue}, true");
                if (op == "==") return (isNull, "bool");
                var notNull = NewTemp(); Emit($"{notNull} = xor i1 {isNull}, true"); return (notNull, "bool");
            }

            var underlying = IsNullableValueType(left.Type) ? NullableUnderlyingType(left.Type) : NullableUnderlyingType(right.Type);
            string leftValue; string rightValue; string? leftHas = null; string? rightHas = null;
            if (IsNullableValueType(left.Type))
            {
                leftHas = NewTemp(); Emit($"{leftHas} = extractvalue {LlvmType(left.Type)} {left.Value}, 0");
                leftValue = NewTemp(); Emit($"{leftValue} = extractvalue {LlvmType(left.Type)} {left.Value}, 1");
            }
            else { leftHas = "true"; leftValue = CastValue(left.Value, left.Type, underlying); }
            if (IsNullableValueType(right.Type))
            {
                rightHas = NewTemp(); Emit($"{rightHas} = extractvalue {LlvmType(right.Type)} {right.Value}, 0");
                rightValue = NewTemp(); Emit($"{rightValue} = extractvalue {LlvmType(right.Type)} {right.Value}, 1");
            }
            else { rightHas = "true"; rightValue = CastValue(right.Value, right.Type, underlying); }
            var valuesEqual = EmitBinaryOperation("==", underlying, leftValue, rightValue).Item1;
            var presenceEqual = NewTemp(); Emit($"{presenceEqual} = icmp eq i1 {leftHas}, {rightHas}");
            var bothEqual = NewTemp(); Emit($"{bothEqual} = and i1 {presenceEqual}, {valuesEqual}");
            if (op == "==") return (bothEqual, "bool");
            var inverted = NewTemp(); Emit($"{inverted} = xor i1 {bothEqual}, true"); return (inverted, "bool");
        }
        private (string Value, string Type) EmitNullableStringEquality(string op, (string Value, string Type) left, (string Value, string Type) right)
        {
            var value = left.Type == "null" ? right : left;
            if (left.Type == "null" || right.Type == "null")
            {
                var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {value.Value}, null");
                var result = op == "==" ? isNull : NewTemp();
                if (op == "!=" ) Emit($"{result} = xor i1 {isNull}, true");
                ReleaseOwnedString(value);
                return (result, "bool");
            }

            if (!IsStringType(left.Type) || !IsStringType(right.Type)) throw new InvalidOperationException("Nullable string equality reached emission with incompatible operands.");
            var leftNull = NewTemp(); Emit($"{leftNull} = icmp eq ptr {left.Value}, null");
            var rightNull = NewTemp(); Emit($"{rightNull} = icmp eq ptr {right.Value}, null");
            var bothNull = NewTemp(); Emit($"{bothNull} = and i1 {leftNull}, {rightNull}");
            var eitherNull = NewTemp(); Emit($"{eitherNull} = or i1 {leftNull}, {rightNull}");
            var bothPresent = NewTemp(); Emit($"{bothPresent} = xor i1 {eitherNull}, true");
            var contentEqual = NewTemp(); Emit($"{contentEqual} = call i1 @luma_string_equals(ptr {left.Value}, ptr {right.Value})");
            var presentEqual = NewTemp(); Emit($"{presentEqual} = and i1 {bothPresent}, {contentEqual}");
            var equal = NewTemp(); Emit($"{equal} = or i1 {bothNull}, {presentEqual}");
            ReleaseOwnedString(left); ReleaseOwnedString(right);
            if (op == "==") return (equal, "bool");
            var inverted = NewTemp(); Emit($"{inverted} = xor i1 {equal}, true"); return (inverted, "bool");
        }
        private (string Value, string Type) EmitNullableValueBinary(BinaryExpression expression, (string Value, string Type) left, (string Value, string Type) right)
        {
            var leftUnderlying = IsNullableValueType(left.Type) ? NullableUnderlyingType(left.Type) : left.Type;
            var rightUnderlying = IsNullableValueType(right.Type) ? NullableUnderlyingType(right.Type) : right.Type;
            var isShift = expression.Operator is "<<" or ">>";
            var operationType = isShift ? SemanticAnalyzer.PromoteSmallInteger(leftUnderlying) : expression.Operator is "&" or "|" or "^" && leftUnderlying == "bool" && rightUnderlying == "bool"
                ? "bool" : SemanticAnalyzer.NumericPromotionType(leftUnderlying, rightUnderlying);
            var leftHas = "true"; var rightHas = "true"; var leftValue = left.Value; var rightValue = right.Value;
            if (IsNullableValueType(left.Type))
            {
                leftHas = NewTemp(); Emit($"{leftHas} = extractvalue {LlvmType(left.Type)} {left.Value}, 0");
                leftValue = NewTemp(); Emit($"{leftValue} = extractvalue {LlvmType(left.Type)} {left.Value}, 1");
            }
            if (IsNullableValueType(right.Type))
            {
                rightHas = NewTemp(); Emit($"{rightHas} = extractvalue {LlvmType(right.Type)} {right.Value}, 0");
                rightValue = NewTemp(); Emit($"{rightValue} = extractvalue {LlvmType(right.Type)} {right.Value}, 1");
            }
            var convertedLeft = CastValue(leftValue, leftUnderlying, operationType);
            var convertedRight = isShift ? CastValue(rightValue, rightUnderlying, operationType) : CastValue(rightValue, rightUnderlying, operationType);
            var hasBoth = NewTemp(); Emit($"{hasBoth} = and i1 {leftHas}, {rightHas}");
            if (expression.Operator is "<" or ">" or "<=" or ">=")
            {
                var comparison = EmitBinaryOperation(expression.Operator, operationType, convertedLeft, convertedRight).Item1;
                var result = NewTemp(); Emit($"{result} = and i1 {hasBoth}, {comparison}");
                return (result, "bool");
            }

            var valueLabel = NewLabel("lifted.value"); var nullLabel = NewLabel("lifted.null"); var endLabel = NewLabel("lifted.end");
            Emit($"br i1 {hasBoth}, label %{valueLabel}, label %{nullLabel}"); _terminated = true;
            Label(valueLabel);
            var computed = EmitBinaryOperation(expression.Operator, operationType, convertedLeft, convertedRight, rightUnderlying);
            var resultType = operationType + "?";
            var wrapped = WrapNullableValue(computed.Item1, operationType, resultType);
            var valueBlock = _block; Branch(endLabel);
            Label(nullLabel); var nullBlock = _block; Branch(endLabel);
            Label(endLabel);
            var resultAggregate = NewTemp(); Emit($"{resultAggregate} = phi {LlvmType(resultType)} [ {wrapped}, %{valueBlock} ], [ zeroinitializer, %{nullBlock} ]");
            return (resultAggregate, resultType);
        }
        private (string Value, string Type) EmitDelegateEquality(string op, (string Value, string Type) left, (string Value, string Type) right)
        {
            var delegateType = GetClassReferenceType(left.Type) ?? GetClassReferenceType(right.Type) ??
                throw new InvalidOperationException("Delegate equality has no bound delegate type.");
            var safeType = Safe(delegateType);
            var sameObject = NewTemp(); Emit($"{sameObject} = icmp eq ptr {left.Value}, {right.Value}");
            var sameLabel = NewLabel("delegate.equal.same"); var distinctLabel = NewLabel("delegate.equal.distinct");
            var nullLabel = NewLabel("delegate.equal.null"); var compareLabel = NewLabel("delegate.equal.compare"); var endLabel = NewLabel("delegate.equal.end");
            Emit($"br i1 {sameObject}, label %{sameLabel}, label %{distinctLabel}"); _terminated = true;
            Label(sameLabel); var sameBlock = _block; Branch(endLabel);
            Label(distinctLabel);
            var leftNull = NewTemp(); Emit($"{leftNull} = icmp eq ptr {left.Value}, null");
            var rightNull = NewTemp(); Emit($"{rightNull} = icmp eq ptr {right.Value}, null");
            var eitherNull = NewTemp(); Emit($"{eitherNull} = or i1 {leftNull}, {rightNull}");
            Emit($"br i1 {eitherNull}, label %{nullLabel}, label %{compareLabel}"); _terminated = true;
            Label(nullLabel); var nullBlock = _block; Branch(endLabel);
            Label(compareLabel);
            var leftFunctionAddress = NewTemp(); Emit($"{leftFunctionAddress} = getelementptr %delegate.{safeType}, ptr {left.Value}, i32 0, i32 4");
            var rightFunctionAddress = NewTemp(); Emit($"{rightFunctionAddress} = getelementptr %delegate.{safeType}, ptr {right.Value}, i32 0, i32 4");
            var leftFunction = NewTemp(); Emit($"{leftFunction} = load ptr, ptr {leftFunctionAddress}");
            var rightFunction = NewTemp(); Emit($"{rightFunction} = load ptr, ptr {rightFunctionAddress}");
            var functionsEqual = NewTemp(); Emit($"{functionsEqual} = icmp eq ptr {leftFunction}, {rightFunction}");
            var leftTargetAddress = NewTemp(); Emit($"{leftTargetAddress} = getelementptr %delegate.{safeType}, ptr {left.Value}, i32 0, i32 5");
            var rightTargetAddress = NewTemp(); Emit($"{rightTargetAddress} = getelementptr %delegate.{safeType}, ptr {right.Value}, i32 0, i32 5");
            var leftTarget = NewTemp(); Emit($"{leftTarget} = load ptr, ptr {leftTargetAddress}");
            var rightTarget = NewTemp(); Emit($"{rightTarget} = load ptr, ptr {rightTargetAddress}");
            var targetsEqual = NewTemp(); Emit($"{targetsEqual} = icmp eq ptr {leftTarget}, {rightTarget}");
            var valuesEqual = NewTemp(); Emit($"{valuesEqual} = and i1 {functionsEqual}, {targetsEqual}");
            var compareBlock = _block; Branch(endLabel);
            Label(endLabel);
            var equal = NewTemp(); Emit($"{equal} = phi i1 [ true, %{sameBlock} ], [ false, %{nullBlock} ], [ {valuesEqual}, %{compareBlock} ]");
            var result = equal;
            if (op == "!=") { result = NewTemp(); Emit($"{result} = xor i1 {equal}, true"); }
            ReleaseOwnedClass(left); ReleaseOwnedClass(right);
            return (result, "bool");
        }

        private (string, string) EmitBinaryOperation(string op, string type, string left, string right, string? rightType = null)
        {
            var llvm = LlvmType(type);
            if (type == "string")
            {
                if (op is not ("==" or "!=")) throw new NotSupportedException($"String operator '{op}' is not supported.");
                var stringCompare = NewTemp();
                Emit($"{stringCompare} = call i1 @luma_string_equals(ptr {left}, ptr {right})");
                if (op == "!=") { var inverted = NewTemp(); Emit($"{inverted} = xor i1 {stringCompare}, true"); stringCompare = inverted; }
                return (stringCompare, "bool");
            }
            if (IsPointerType(type))
            {
                var pointerInstruction = op switch { "==" => "icmp eq", "!=" => "icmp ne", "<" => "icmp ult", ">" => "icmp ugt", "<=" => "icmp ule", ">=" => "icmp uge", _ => throw new NotSupportedException($"Pointer operator '{op}' is unsupported.") };
                var pointerCompare = NewTemp(); Emit($"{pointerCompare} = {pointerInstruction} ptr {left}, {right}"); return (pointerCompare, "bool");
            }
            if (type is "float" or "double") return EmitFloatingOperation(op, type, left, right);
            if (op is "+" or "-" or "*") return EmitCheckedArithmetic(op, type, left, right, _checkedContext);
            if (op is "/" or "%")
            {
                var divisorIsZero = NewTemp(); Emit($"{divisorIsZero} = icmp eq {llvm} {right}, 0"); TrapIf(divisorIsZero, "luma_throw_divide_by_zero");
                if (!IsUnsigned(type))
                {
                    var width = IntegerWidth(type); var minValue = -(BigInteger.One << (width - 1)); var isMin = NewTemp(); Emit($"{isMin} = icmp eq {llvm} {left}, {minValue}"); var isNegativeOne = NewTemp(); Emit($"{isNegativeOne} = icmp eq {llvm} {right}, -1"); var overflow = NewTemp(); Emit($"{overflow} = and i1 {isMin}, {isNegativeOne}"); TrapIf(overflow, "luma_throw_overflow");
                }
            }
            if (op is "<<" or ">>")
            {
                var countWidth = IntegerWidth(type); var mask = countWidth - 1; var count = right;
                var countType = rightType ?? type;
                if (countType != type) count = CastInteger(right, countType, type);
                var masked = NewTemp(); Emit($"{masked} = and {llvm} {count}, {mask}"); right = masked;
            }
            string? instruction = op switch
            {
                "/" => IsUnsigned(type) ? "udiv" : "sdiv", "%" => IsUnsigned(type) ? "urem" : "srem",
                "&" => "and", "|" => "or", "^" => "xor", "<<" => "shl", ">>" => IsUnsigned(type) ? "lshr" : "ashr",
                "==" => "icmp eq", "!=" => "icmp ne", "<" => IsUnsigned(type) ? "icmp ult" : "icmp slt", ">" => IsUnsigned(type) ? "icmp ugt" : "icmp sgt", "<=" => IsUnsigned(type) ? "icmp ule" : "icmp sle", ">=" => IsUnsigned(type) ? "icmp uge" : "icmp sge", _ => null
            };
            if (instruction is null) throw new NotSupportedException($"Binary operator '{op}' is not supported.");
            var result = NewTemp(); var isCompare = instruction.StartsWith("icmp", StringComparison.Ordinal); Emit($"{result} = {instruction} {(isCompare ? llvm : llvm)} {left}, {right}"); return (result, isCompare ? "bool" : type);
        }
        private (string, string) EmitFloatingOperation(string op, string type, string left, string right)
        {
            var llvm = LlvmType(type); var instruction = op switch
            {
                "+" => "fadd", "-" => "fsub", "*" => "fmul", "/" => "fdiv", "%" => "frem",
                "==" => "fcmp oeq", "!=" => "fcmp une", "<" => "fcmp olt", ">" => "fcmp ogt", "<=" => "fcmp ole", ">=" => "fcmp oge",
                _ => throw new NotSupportedException($"Floating operator '{op}' is not supported.")
            };
            var result = NewTemp(); var compare = instruction.StartsWith("fcmp", StringComparison.Ordinal); Emit($"{result} = {instruction} {llvm} {left}, {right}"); return (result, compare ? "bool" : type);
        }
        private static string FormatFloating(string type, double value)
        {
            var result = type == "float" ? ((float)value).ToString("R", System.Globalization.CultureInfo.InvariantCulture) : value.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
            if (!result.Contains('.') && !result.Contains('e') && !result.Contains('E')) result += ".0";
            return result.Replace("E", "e", StringComparison.Ordinal);
        }
        private int SizeOf(string type)
        {
            if (IsNullableValueType(type))
            {
                var underlying = type[..^1]; var valueAlignment = AlignmentOf(underlying);
                return (int)AlignTo(AlignTo(1, valueAlignment) + SizeOf(underlying), valueAlignment);
            }
            return type switch
            {
                "bool" or "int8" or "uint8" => 1,
                "int16" or "uint16" or "char" => 2,
                "int32" or "uint32" or "float" => 4,
                "int64" or "uint64" or "double" => 8,
                "int" or "uint" => nativePointerSize,
                "string" or "string?" => nativePointerSize,
                _ when type.EndsWith('*') => nativePointerSize,
                _ when SemanticAnalyzer.TryParseManagedArrayType(type, out _, out _) => nativePointerSize,
                _ when GetClassReferenceType(type) is not null => nativePointerSize,
                _ when enumTypes.TryGetValue(type, out var enumUnderlying) => SizeOf(enumUnderlying),
                _ when SemanticAnalyzer.TryParseInlineArrayType(type, out var elementType, out var count) => count * SizeOf(elementType),
                _ when structTypes.TryGetValue(type, out var structure) => structure.SizeInBytes,
                _ => throw new NotSupportedException($"sizeof is unsupported for type '{type}'.")
            };
        }
        private int AlignmentOf(string type)
        {
            if (enumTypes.TryGetValue(type, out var underlying)) return AlignmentOf(underlying);
            if (structTypes.TryGetValue(type, out var structure)) return structure.Alignment;
            if (type.EndsWith('?')) return AlignmentOf(type[..^1]);
            if (type is "int64" or "uint64" or "double") return nativePointerSize == 8 ? 8 : GetTargetEightByteAlignment();
            return type switch
            {
                "bool" or "int8" or "uint8" => 1,
                "int16" or "uint16" or "char" => 2,
                "int32" or "uint32" or "float" => 4,
                "int" or "uint" or "string" or "string?" => nativePointerSize,
                _ when type.EndsWith('*') || SemanticAnalyzer.TryParseManagedArrayType(type, out _, out _) || GetClassReferenceType(type) is not null => nativePointerSize,
                _ when SemanticAnalyzer.TryParseInlineArrayType(type, out var elementType, out _) => AlignmentOf(elementType),
                _ => throw new NotSupportedException($"sizeof is unsupported for type '{type}'.")
            };
        }
        private int GetTargetEightByteAlignment()
        {
            return nativePointerSize == 8 ? 8 : 4;
        }
        private static long AlignTo(long value, int alignment) => (value + alignment - 1) / alignment * alignment;
        private (string, string) EmitCheckedArithmetic(string op, string type, string left, string right, bool checkedContext)
        {
            var llvm = LlvmType(type); var prefix = IsUnsigned(type) ? "u" : "s"; var operation = op switch { "+" => "add", "-" => "sub", "*" => "mul", _ => throw new NotSupportedException() };
            if (!checkedContext) { var uncheckedValue = NewTemp(); Emit($"{uncheckedValue} = {operation} {llvm} {left}, {right}"); return (uncheckedValue, type); }
            var pair = NewTemp(); Emit($"{pair} = call {{ {llvm}, i1 }} @llvm.{prefix}{operation}.with.overflow.{llvm}({llvm} {left}, {llvm} {right})");
            var value = NewTemp(); Emit($"{value} = extractvalue {{ {llvm}, i1 }} {pair}, 0"); var overflow = NewTemp(); Emit($"{overflow} = extractvalue {{ {llvm}, i1 }} {pair}, 1"); TrapIf(overflow, "luma_throw_overflow"); return (value, type);
        }
        private void TrapIf(string condition, string function)
        {
            var trap = NewLabel("check.fail"); var success = NewLabel("check.ok"); Emit($"br i1 {condition}, label %{trap}, label %{success}"); _terminated = true;
            Label(trap); EmitRuntimeException(function); Label(success);
        }
        private (string Value, string Type) EmitNullCoalescing(BinaryExpression expression)
        {
            var left = EmitExpression(expression.Left);
            if (IsNullableValueType(left.Type)) return EmitNullableValueCoalescing(expression, left);
            if (IsNullableStringType(left.Type)) return EmitNullableStringCoalescing(expression, left);
            if (IsNullableArrayType(left.Type)) return EmitNullableArrayCoalescing(expression, left);
            if (IsNullableDelegateType(left.Type)) return EmitNullableDelegateCoalescing(expression, left);
            var classType = GetClassReferenceType(left.Type) ?? throw new NotSupportedException("Only nullable class references are currently lowered for '??'.");
            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {left.Value}, null");
            var rhsLabel = NewLabel("coalesce.rhs"); var leftLabel = NewLabel("coalesce.left"); var endLabel = NewLabel("coalesce.end");
            Emit($"br i1 {isNull}, label %{rhsLabel}, label %{leftLabel}"); _terminated = true;

            Label(leftLabel);
            EnsureOwnedClass(left, classType);
            var leftBlock = _block;
            Branch(endLabel);

            Label(rhsLabel);
            var right = EmitExpression(expression.Right);
            if (GetClassReferenceType(right.Type) is { } rightClass) EnsureOwnedClass(right, rightClass);
            var rightBlock = _block;
            Branch(endLabel);

            Label(endLabel);
            var result = NewTemp(); Emit($"{result} = phi ptr [ {left.Value}, %{leftBlock} ], [ {right.Value}, %{rightBlock} ]");
            _ownedClassValues.Remove((left.Value, classType));
            _ownedClassValues.Remove((right.Value, classType));
            _ownedClassValues.Add((result, classType));
            var resultType = right.Type == "null" || GetClassReferenceType(right.Type) is not null && right.Type.EndsWith("?", StringComparison.Ordinal)
                ? classType + "?" : classType;
            return (result, resultType);
        }
        private (string Value, string Type) EmitNullableDelegateCoalescing(BinaryExpression expression, (string Value, string Type) left)
        {
            var delegateType = NullableUnderlyingType(left.Type);
            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {left.Value}, null");
            var rhsLabel = NewLabel("coalesce.delegate.rhs"); var leftLabel = NewLabel("coalesce.delegate.left"); var endLabel = NewLabel("coalesce.delegate.end");
            Emit($"br i1 {isNull}, label %{rhsLabel}, label %{leftLabel}"); _terminated = true;
            Label(leftLabel); EnsureOwnedClass(left, delegateType); var leftBlock = _block; Branch(endLabel);
            Label(rhsLabel);
            var right = EmitExpression(expression.Right);
            var rightValue = CastValue(right.Value, right.Type, delegateType + "?");
            EnsureOwnedClass((rightValue, right.Type), delegateType);
            var rightBlock = _block; Branch(endLabel);
            Label(endLabel);
            var result = NewTemp(); Emit($"{result} = phi ptr [ {left.Value}, %{leftBlock} ], [ {rightValue}, %{rightBlock} ]");
            _ownedClassValues.Remove((left.Value, delegateType));
            _ownedClassValues.Remove((rightValue, delegateType));
            _ownedClassValues.Add((result, delegateType));
            var resultType = right.Type == "null" || IsNullableDelegateType(right.Type) ? delegateType + "?" : delegateType;
            return (result, resultType);
        }
        private (string Value, string Type) EmitNullableStringCoalescing(BinaryExpression expression, (string Value, string Type) left)
        {
            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {left.Value}, null");
            var rhsLabel = NewLabel("coalesce.string.rhs"); var leftLabel = NewLabel("coalesce.string.left"); var endLabel = NewLabel("coalesce.string.end");
            Emit($"br i1 {isNull}, label %{rhsLabel}, label %{leftLabel}"); _terminated = true;

            Label(leftLabel);
            EnsureOwnedString(left);
            var leftBlock = _block; Branch(endLabel);

            Label(rhsLabel);
            var right = EmitExpression(expression.Right);
            var resultType = right.Type == "null" || IsNullableStringType(right.Type) ? "string?" : "string";
            var rightValue = CastValue(right.Value, right.Type, resultType);
            EnsureOwnedString((rightValue, resultType));
            var rightBlock = _block; Branch(endLabel);

            Label(endLabel);
            var result = NewTemp(); Emit($"{result} = phi ptr [ {left.Value}, %{leftBlock} ], [ {rightValue}, %{rightBlock} ]");
            _ownedStringValues.Remove(left.Value);
            _ownedStringValues.Remove(rightValue);
            _ownedStringValues.Add(result);
            return (result, resultType);
        }
        private (string Value, string Type) EmitNullableArrayCoalescing(BinaryExpression expression, (string Value, string Type) left)
        {
            var underlying = NullableUnderlyingType(left.Type);
            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {left.Value}, null");
            var rhsLabel = NewLabel("coalesce.array.rhs"); var leftLabel = NewLabel("coalesce.array.left"); var endLabel = NewLabel("coalesce.array.end");
            Emit($"br i1 {isNull}, label %{rhsLabel}, label %{leftLabel}"); _terminated = true;
            Label(leftLabel);
            EnsureOwnedArray(left);
            var leftBlock = _block; Branch(endLabel);
            Label(rhsLabel);
            var right = EmitExpression(expression.Right);
            var resultType = right.Type == "null" || IsNullableArrayType(right.Type) ? underlying + "?" : underlying;
            var rightValue = CastValue(right.Value, right.Type, resultType);
            EnsureOwnedArray((rightValue, resultType));
            var rightBlock = _block; Branch(endLabel);
            Label(endLabel);
            var result = NewTemp(); Emit($"{result} = phi ptr [ {left.Value}, %{leftBlock} ], [ {rightValue}, %{rightBlock} ]");
            _ownedArrayValues.Remove(left.Value); _ownedArrayValues.Remove(rightValue); _ownedArrayValues.Add(result);
            return (result, resultType);
        }
        private (string Value, string Type) EmitNullableValueCoalescing(BinaryExpression expression, (string Value, string Type) left)
        {
            var underlying = NullableUnderlyingType(left.Type);
            var leftAggregateType = LlvmType(left.Type);
            var hasValue = NewTemp(); Emit($"{hasValue} = extractvalue {leftAggregateType} {left.Value}, 0");
            var leftValue = NewTemp(); Emit($"{leftValue} = extractvalue {leftAggregateType} {left.Value}, 1");
            var rightLabel = NewLabel("coalesce.value.rhs"); var leftLabel = NewLabel("coalesce.value.left"); var endLabel = NewLabel("coalesce.value.end");
            Emit($"br i1 {hasValue}, label %{leftLabel}, label %{rightLabel}"); _terminated = true;
            var aggregatesBeforeBranches = _ownedAggregateValues.ToArray();

            Label(rightLabel);
            var right = EmitExpression(expression.Right);
            var resultNullable = right.Type == "null" || IsNullableValueType(right.Type);
            var resultType = resultNullable ? underlying + "?" : underlying;
            // The right-hand expression is emitted only on the empty path.
            var rightValue = CastValue(right.Value, right.Type, resultType);
            var convertedRight = (rightValue, resultType);
            if (IsManagedAggregateType(resultType))
            {
                EnsureOwnedAggregate(convertedRight);
                _ownedAggregateValues.Remove(convertedRight);
                if (right.Type != resultType) ReleaseOwnedAggregate(right);
                ReleaseOwnedAggregate(left);
            }
            var rightBlock = _block; Branch(endLabel);

            _ownedAggregateValues.Clear(); _ownedAggregateValues.UnionWith(aggregatesBeforeBranches);
            Label(leftLabel);
            var leftResult = resultNullable ? left.Value : leftValue;
            var leftResultType = resultNullable ? left.Type : underlying;
            var convertedLeft = (CastValue(leftResult, leftResultType, resultType), resultType);
            if (IsManagedAggregateType(resultType))
            {
                EnsureOwnedAggregate(convertedLeft);
                _ownedAggregateValues.Remove(convertedLeft);
                if (left.Type != resultType || leftResult != left.Value) ReleaseOwnedAggregate(left);
            }
            var leftBlock = _block; Branch(endLabel);

            Label(endLabel);
            var result = NewTemp(); Emit($"{result} = phi {LlvmType(resultType)} [ {convertedLeft.Item1}, %{leftBlock} ], [ {rightValue}, %{rightBlock} ]");
            if (IsManagedAggregateType(resultType))
            {
                _ownedAggregateValues.Clear();
                _ownedAggregateValues.UnionWith(aggregatesBeforeBranches.Where(value => value != left));
                _ownedAggregateValues.Add((result, resultType));
            }
            return (result, resultType);
        }
        private (string Value, string Type) EmitNullCoalescingAssignment(string targetType, string pointer, Expression rightExpression,
            Expression targetExpression)
        {
            var classType = GetClassReferenceType(targetType);
            if (IsNullableValueType(targetType)) return EmitNullableValueCoalescingAssignment(targetType, pointer, rightExpression);
            if (IsNullableStringType(targetType)) return EmitNullableStringCoalescingAssignment(targetType, pointer, rightExpression);
            if (IsNullableArrayType(targetType)) return EmitNullableArrayCoalescingAssignment(targetType, pointer, rightExpression, targetExpression);
            if (IsNullableDelegateType(targetType)) return EmitNullableDelegateCoalescingAssignment(targetType, pointer, rightExpression);
            if (classType is null || !targetType.EndsWith("?", StringComparison.Ordinal))
                throw new NotSupportedException("Only nullable class references are currently lowered for '??='.");

            var oldValue = ReadStorage(targetType, pointer);
            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {oldValue}, null");
            var rightLabel = NewLabel("coalesce.assign.rhs"); var oldLabel = NewLabel("coalesce.assign.old"); var endLabel = NewLabel("coalesce.assign.end");
            Emit($"br i1 {isNull}, label %{rightLabel}, label %{oldLabel}"); _terminated = true;

            Label(oldLabel);
            var oldOwned = (Value: oldValue, Type: targetType);
            EnsureOwnedClass(oldOwned, classType);
            var oldBlock = _block;
            Branch(endLabel);

            Label(rightLabel);
            var right = EmitExpression(rightExpression);
            EnsureOwnedClassElement(targetExpression, right, classType);
            StoreClassReplacing(pointer, right, classType);
            var storedRightValue = ReadStorage(targetType, pointer);
            var rightOwned = (Value: storedRightValue, Type: targetType);
            EnsureOwnedClass(rightOwned, classType);
            var rightBlock = _block;
            Branch(endLabel);

            Label(endLabel);
            var result = NewTemp(); Emit($"{result} = phi ptr [ {oldValue}, %{oldBlock} ], [ {storedRightValue}, %{rightBlock} ]");
            _ownedClassValues.Remove((oldValue, classType));
            _ownedClassValues.Remove((storedRightValue, classType));
            _ownedClassValues.Add((result, classType));
            return (result, targetType);
        }
        private (string Value, string Type) EmitNullableDelegateCoalescingAssignment(string targetType, string pointer, Expression rightExpression)
        {
            var delegateType = NullableUnderlyingType(targetType);
            var oldValue = ReadStorage(targetType, pointer);
            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {oldValue}, null");
            var rightLabel = NewLabel("coalesce.delegate.assign.rhs"); var oldLabel = NewLabel("coalesce.delegate.assign.old"); var endLabel = NewLabel("coalesce.delegate.assign.end");
            Emit($"br i1 {isNull}, label %{rightLabel}, label %{oldLabel}"); _terminated = true;
            Label(oldLabel); EnsureOwnedClass((oldValue, targetType), delegateType); var oldBlock = _block; Branch(endLabel);
            Label(rightLabel);
            var right = EmitExpression(rightExpression);
            var converted = CastValue(right.Value, right.Type, targetType);
            StoreClassReplacing(pointer, (converted, right.Type), delegateType);
            var stored = ReadStorage(targetType, pointer);
            EnsureOwnedClass((stored, targetType), delegateType);
            var rightBlock = _block; Branch(endLabel);
            Label(endLabel);
            var result = NewTemp(); Emit($"{result} = phi ptr [ {oldValue}, %{oldBlock} ], [ {stored}, %{rightBlock} ]");
            _ownedClassValues.Remove((oldValue, delegateType));
            _ownedClassValues.Remove((stored, delegateType));
            _ownedClassValues.Add((result, delegateType));
            var resultType = right.Type == "null" || IsNullableDelegateType(right.Type) ? targetType : delegateType;
            return (result, resultType);
        }
        private (string Value, string Type) EmitNullableStringCoalescingAssignment(string targetType, string pointer, Expression rightExpression)
        {
            var oldValue = ReadStorage(targetType, pointer);
            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {oldValue}, null");
            var rightLabel = NewLabel("coalesce.string.assign.rhs"); var oldLabel = NewLabel("coalesce.string.assign.old"); var endLabel = NewLabel("coalesce.string.assign.end");
            Emit($"br i1 {isNull}, label %{rightLabel}, label %{oldLabel}"); _terminated = true;

            Label(oldLabel);
            EnsureOwnedString((oldValue, targetType));
            var oldBlock = _block; Branch(endLabel);

            Label(rightLabel);
            var right = EmitExpression(rightExpression);
            var converted = (CastValue(right.Value, right.Type, targetType), targetType);
            StoreStringReplacing(pointer, converted);
            var storedValue = ReadStorage(targetType, pointer);
            EnsureOwnedString((storedValue, targetType));
            var rightBlock = _block; Branch(endLabel);

            Label(endLabel);
            var result = NewTemp(); Emit($"{result} = phi ptr [ {oldValue}, %{oldBlock} ], [ {storedValue}, %{rightBlock} ]");
            _ownedStringValues.Remove(oldValue); _ownedStringValues.Remove(storedValue);
            _ownedStringValues.Add(result);
            return (result, targetType);
        }
        private (string Value, string Type) EmitNullableArrayCoalescingAssignment(string targetType, string pointer,
            Expression rightExpression, Expression targetExpression)
        {
            var oldValue = ReadStorage(targetType, pointer);
            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {oldValue}, null");
            var rightLabel = NewLabel("coalesce.array.assign.rhs"); var oldLabel = NewLabel("coalesce.array.assign.old"); var endLabel = NewLabel("coalesce.array.assign.end");
            Emit($"br i1 {isNull}, label %{rightLabel}, label %{oldLabel}"); _terminated = true;
            Label(oldLabel);
            EnsureOwnedArray((oldValue, targetType));
            var oldBlock = _block; Branch(endLabel);
            Label(rightLabel);
            var right = EmitExpression(rightExpression);
            EnsureOwnedArrayElement(targetExpression, right);
            var stored = StoreArrayReplacing(pointer, right, targetType);
            EnsureOwnedArray(stored);
            var rightBlock = _block; Branch(endLabel);
            Label(endLabel);
            var result = NewTemp(); Emit($"{result} = phi ptr [ {oldValue}, %{oldBlock} ], [ {stored.Value}, %{rightBlock} ]");
            _ownedArrayValues.Remove(oldValue); _ownedArrayValues.Remove(stored.Value); _ownedArrayValues.Add(result);
            return (result, targetType);
        }
        private (string Value, string Type) EmitNullableValueCompoundAssignment(BinaryExpression expression, string targetType, string pointer,
            (string Value, string Type)? evaluatedRight = null)
        {
            var oldValue = NewTemp(); Emit($"{oldValue} = load {LlvmType(targetType)}, ptr {pointer}");
            var right = evaluatedRight ?? EmitExpression(expression.Right);
            var operation = expression.Operator[..^1];
            var calculation = EmitNullableValueBinary(expression with { Operator = operation }, (oldValue, targetType), right);
            var converted = calculation.Type == targetType ? calculation.Value : ConvertNullableValue(calculation.Value, calculation.Type, targetType);
            Emit($"store {LlvmType(targetType)} {converted}, ptr {pointer}");
            return (converted, targetType);
        }
        private (string Value, string Type) EmitNullableValueCoalescingAssignment(string targetType, string pointer, Expression rightExpression)
        {
            var aggregateType = LlvmType(targetType);
            var oldValue = NewTemp(); Emit($"{oldValue} = load {aggregateType}, ptr {pointer}");
            var hasValue = NewTemp(); Emit($"{hasValue} = extractvalue {aggregateType} {oldValue}, 0");
            var rightLabel = NewLabel("coalesce.value.assign.rhs"); var oldLabel = NewLabel("coalesce.value.assign.old"); var endLabel = NewLabel("coalesce.value.assign.end");
            Emit($"br i1 {hasValue}, label %{oldLabel}, label %{rightLabel}"); _terminated = true;
            var aggregatesBeforeBranches = _ownedAggregateValues.ToArray();

            Label(oldLabel);
            if (IsManagedAggregateType(targetType))
            {
                var oldOwned = (oldValue, targetType);
                EnsureOwnedAggregate(oldOwned);
                _ownedAggregateValues.Remove(oldOwned);
            }
            var oldBlock = _block; Branch(endLabel);

            _ownedAggregateValues.Clear(); _ownedAggregateValues.UnionWith(aggregatesBeforeBranches);
            Label(rightLabel);
            var right = EmitExpression(rightExpression);
            var converted = CastValue(right.Value, right.Type, targetType);
            if (IsManagedAggregateType(targetType)) StoreAggregateReplacing(pointer, right, targetType);
            else Emit($"store {aggregateType} {converted}, ptr {pointer}");
            var stored = NewTemp(); Emit($"{stored} = load {aggregateType}, ptr {pointer}");
            if (IsManagedAggregateType(targetType))
            {
                var storedOwned = (stored, targetType);
                EnsureOwnedAggregate(storedOwned);
                _ownedAggregateValues.Remove(storedOwned);
            }
            var rightBlock = _block; Branch(endLabel);

            Label(endLabel);
            var result = NewTemp(); Emit($"{result} = phi {aggregateType} [ {oldValue}, %{oldBlock} ], [ {stored}, %{rightBlock} ]");
            if (IsManagedAggregateType(targetType))
            {
                _ownedAggregateValues.Clear(); _ownedAggregateValues.UnionWith(aggregatesBeforeBranches);
                _ownedAggregateValues.Add((result, targetType));
            }
            return (result, targetType);
        }
        private string RepresentationType(string type) => enumTypes.GetValueOrDefault(type, type);
        private int IntegerWidth(string type) => RepresentationType(type) switch { "int8" or "uint8" => 8, "int16" or "uint16" or "char" => 16, "int32" or "uint32" => 32, "int64" or "uint64" => 64, "int" or "uint" => nativePointerSize * 8, _ => 32 };
        private string CastInteger(string value, string fromType, string toType)
        {
            var from = LlvmType(fromType); var to = LlvmType(toType); if (from == to) return value; var result = NewTemp();
            var op = IntegerWidth(fromType) > IntegerWidth(toType) ? "trunc" : IsUnsigned(fromType) ? "zext" : "sext"; Emit($"{result} = {op} {from} {value} to {to}"); return result;
        }
        private (string, string) EmitShortCircuit(BinaryExpression expression)
        {
            var left = EmitExpression(expression.Left); var rhs = NewLabel("logic.rhs"); var end = NewLabel("logic.end"); var lhsBlock = _block;
            if (expression.Operator == "&&") Emit($"br i1 {left.Value}, label %{rhs}, label %{end}"); else Emit($"br i1 {left.Value}, label %{end}, label %{rhs}"); _terminated = true;
            Label(rhs);
            var leftPatterns = expression.Operator == "&&" ? GetPositiveTypePatternNodes(expression.Left)
                .Where(pattern => _activeTypePatternBindings.ContainsKey(pattern)).ToArray() : [];
            if (leftPatterns.Length > 0)
            {
                Push();
                var capturedPatternEnvironments = new List<ClosureEnvironmentBinding>();
                foreach (var pattern in leftPatterns)
                {
                    var state = _activeTypePatternBindings[pattern];
                    var name = pattern.PatternVariableName!;
                    if (expression.CapturedPatternNamesInRight.Contains(name))
                    {
                        var capturedEnvironment = InitializeTypePatternBinding(pattern, state, preservePatternStorage: true);
                        if (capturedEnvironment is not null) capturedPatternEnvironments.Add(capturedEnvironment);
                    }
                    else
                    {
                        InitializeTypePatternBinding(state);
                        Declare(name, state.Type, state.Address, ownsString: false, ownsArray: false,
                            ownsClass: false, ownsAggregate: false);
                    }
                }
                var rightWithPatterns = EmitExpression(expression.Right);
                Pop();
                for (var index = capturedPatternEnvironments.Count - 1; index >= 0; index--)
                    _activeBlockClosureEnvironments.RemoveAt(_activeBlockClosureEnvironments.Count - 1);
                var patternRhsBlock = _block; Branch(end);
                Label(end); var patternResult = NewTemp();
                Emit($"{patternResult} = phi i1 [ false, %{lhsBlock} ], [ {rightWithPatterns.Value}, %{patternRhsBlock} ]");
                return (patternResult, "bool");
            }
            var right = EmitExpression(expression.Right); var rhsBlock = _block; Branch(end);
            Label(end); var result = NewTemp(); var shortValue = expression.Operator == "&&" ? "false" : "true"; Emit($"{result} = phi i1 [ {shortValue}, %{lhsBlock} ], [ {right.Value}, %{rhsBlock} ]"); return (result, "bool");
        }
        private (string, string) EmitConditional(ConditionalExpression expression)
        {
            var analyzedResultType = conditionalExpressionTypes.GetValueOrDefault(expression);
            var patternNodes = GetPositiveTypePatternNodes(expression.Condition);
            var patternStates = new List<(BinaryExpression Pattern, TypePatternBindingState State)>();
            if (patternNodes.Length > 0 && patternNodes.All(pattern => typeOperatorTargets.ContainsKey(pattern)))
            {
                foreach (var pattern in patternNodes)
                {
                    var patternType = typeOperatorTargets[pattern];
                    var storageType = IsBoxableValueType(patternType) ? LlvmType(patternType) : "ptr";
                    var address = NewTemp(); Emit($"{address} = alloca {storageType}");
                    var sourceAddress = NewTemp(); Emit($"{sourceAddress} = alloca ptr");
                    var retainedFlag = NewTemp(); Emit($"{retainedFlag} = alloca i1");
                    var state = new TypePatternBindingState(patternType, address, sourceAddress, retainedFlag);
                    _activeTypePatternBindings.Add(pattern, state);
                    patternStates.Add((pattern, state));
                }
                foreach (var (_, state) in patternStates) Emit($"store i1 false, ptr {state.RetainedFlagAddress}");
            }

            var condition = EmitExpression(expression.Condition); var yes = NewLabel("cond.yes"); var no = NewLabel("cond.no"); var end = NewLabel("cond.end"); Emit($"br i1 {condition.Value}, label %{yes}, label %{no}"); _terminated = true;
            var stringsBefore = _ownedStringValues.ToArray();
            var arraysBefore = _ownedArrayValues.ToArray();
            var classesBefore = _ownedClassValues.ToArray();
            var aggregatesBefore = _ownedAggregateValues.ToArray();
            var scopedClassesBefore = _scopeBoundClassValues.ToArray();
            var scopedArraysBefore = _scopeBoundArrayValues.ToArray();

            Label(yes);
            Push();
            var capturedPatternEnvironments = new List<ClosureEnvironmentBinding>();
            foreach (var (pattern, state) in patternStates)
            {
                var capturedEnvironment = InitializeTypePatternBinding(pattern, state);
                if (capturedEnvironment is not null) capturedPatternEnvironments.Add(capturedEnvironment);
                if (state.RetainedClassType is not null && GetClassReferenceType(state.Type) is null && !IsStringType(state.Type) &&
                    (capturedEnvironment is null || !SemanticAnalyzer.TryParseManagedArrayType(state.Type, out _, out _)))
                {
                    var source = NewTemp(); Emit($"{source} = load ptr, ptr {state.SourceAddress}");
                    Emit($"call void @luma_class_release_dynamic(ptr {source})");
                }
            }
            var whenTrue = EmitExpression(expression.WhenTrue);
            if (IsStringType(whenTrue.Type)) EnsureOwnedString(whenTrue);
            else if (SemanticAnalyzer.TryParseManagedArrayType(whenTrue.Type, out _, out _)) EnsureOwnedArray(whenTrue);
            else if (GetClassReferenceType(whenTrue.Type) is { } trueClass) EnsureOwnedClass(whenTrue, trueClass);
            else if (IsManagedAggregateType(whenTrue.Type)) { EnsureOwnedAggregate(whenTrue); _ownedAggregateValues.Remove(whenTrue); }
            var trueValue = whenTrue.Value;
            if (whenTrue.Type == "null" && analyzedResultType is not null && IsNullableValueType(analyzedResultType))
                trueValue = "zeroinitializer";
            else if (analyzedResultType is not null && IsNumericType(NullableUnderlyingType(analyzedResultType)) && IsNumericType(NullableUnderlyingType(whenTrue.Type)))
                trueValue = CastValue(whenTrue.Value, whenTrue.Type, analyzedResultType);
            else if (expression.WhenFalse is LiteralExpression { Value: "null" } && IsNullableValuePayloadType(whenTrue.Type))
                trueValue = CastValue(whenTrue.Value, whenTrue.Type, whenTrue.Type + "?");
            var trueIsScopeBound = _scopeBoundClassValues.Contains(whenTrue.Value) || _scopeBoundArrayValues.Contains(whenTrue.Value);
            var yesBlock = _block;
            Pop();
            for (var index = capturedPatternEnvironments.Count - 1; index >= 0; index--)
                _activeBlockClosureEnvironments.RemoveAt(_activeBlockClosureEnvironments.Count - 1);
            Branch(end);

            // Values created in one arm do not dominate the other arm. Restore the incoming
            // ownership state before emitting the alternate arm so exceptional cleanup only
            // references values that are live on that control-flow path.
            RestoreOwnedTemporaryState(stringsBefore, arraysBefore, classesBefore, scopedClassesBefore, scopedArraysBefore, aggregatesBefore);
            _ownedAggregateValues.Clear(); _ownedAggregateValues.UnionWith(aggregatesBefore);
            Label(no);
            foreach (var (_, state) in patternStates) ReleaseTypePatternRetentionIfPresent(state);
            var whenFalse = EmitExpression(expression.WhenFalse);
            if (IsStringType(whenFalse.Type)) EnsureOwnedString(whenFalse);
            else if (SemanticAnalyzer.TryParseManagedArrayType(whenFalse.Type, out _, out _)) EnsureOwnedArray(whenFalse);
            else if (GetClassReferenceType(whenFalse.Type) is { } falseClass) EnsureOwnedClass(whenFalse, falseClass);
            else if (IsManagedAggregateType(whenFalse.Type)) { EnsureOwnedAggregate(whenFalse); _ownedAggregateValues.Remove(whenFalse); }
            var falseValue = whenFalse.Value;
            if (whenFalse.Type == "null" && analyzedResultType is not null && IsNullableValueType(analyzedResultType))
                falseValue = "zeroinitializer";
            else if (analyzedResultType is not null && IsNumericType(NullableUnderlyingType(analyzedResultType)) && IsNumericType(NullableUnderlyingType(whenFalse.Type)))
                falseValue = CastValue(whenFalse.Value, whenFalse.Type, analyzedResultType);
            else if (expression.WhenTrue is LiteralExpression { Value: "null" } && IsNullableValuePayloadType(whenFalse.Type))
                falseValue = CastValue(whenFalse.Value, whenFalse.Type, whenFalse.Type + "?");
            var noBlock = _block; Branch(end);
            var falseIsScopeBound = _scopeBoundClassValues.Contains(whenFalse.Value) || _scopeBoundArrayValues.Contains(whenFalse.Value);
            var stringResult = (IsStringType(whenTrue.Type) || whenTrue.Type == "null") && (IsStringType(whenFalse.Type) || whenFalse.Type == "null") &&
                (IsNullableStringType(whenTrue.Type) || IsNullableStringType(whenFalse.Type) || whenTrue.Type == "null" || whenFalse.Type == "null");
            var nullableValueBranchType = whenTrue.Type == "null" ? whenFalse.Type : whenFalse.Type == "null" ? whenTrue.Type : null;
            var nullableValueResult = nullableValueBranchType is not null &&
                (IsNullableValueType(nullableValueBranchType) || IsNullableValuePayloadType(nullableValueBranchType));
            var nullableValueType = nullableValueBranchType is null ? null :
                IsNullableValueType(nullableValueBranchType) ? nullableValueBranchType : nullableValueBranchType + "?";
            var numericPromotionResult = analyzedResultType is not null &&
                IsNumericType(NullableUnderlyingType(analyzedResultType)) &&
                IsNumericType(NullableUnderlyingType(whenTrue.Type)) && IsNumericType(NullableUnderlyingType(whenFalse.Type));
            var resultType = numericPromotionResult ? analyzedResultType! : stringResult ? "string?" :
                nullableValueResult ? analyzedResultType ?? nullableValueType! :
                analyzedResultType is not null && analyzedResultType != "?" ? analyzedResultType : whenTrue.Type;
            var aggregateResult = IsManagedAggregateType(resultType);
            if (nullableValueResult)
            {
                if (whenTrue.Type == "null") trueValue = "zeroinitializer";
                if (whenFalse.Type == "null") falseValue = "zeroinitializer";
            }
            else
            {
                if (stringResult || aggregateResult) trueValue = CastValue(whenTrue.Value, whenTrue.Type, resultType);
                if (stringResult || aggregateResult) falseValue = CastValue(whenFalse.Value, whenFalse.Type, resultType);
            }
            Label(end); var result = NewTemp(); Emit($"{result} = phi {LlvmType(resultType)} [ {trueValue}, %{yesBlock} ], [ {falseValue}, %{noBlock} ]");
            if (IsStringType(resultType))
            {
                _ownedStringValues.Remove(whenTrue.Value); _ownedStringValues.Remove(whenFalse.Value); _ownedStringValues.Add(result);
            }
            else if (SemanticAnalyzer.TryParseManagedArrayType(resultType, out _, out _))
            {
                var scopeBound = trueIsScopeBound || falseIsScopeBound;
                _ownedArrayValues.Remove(whenTrue.Value); _ownedArrayValues.Remove(whenFalse.Value);
                if (scopeBound) _scopeBoundArrayValues.Add(result); else _ownedArrayValues.Add(result);
            }
            else if (GetClassReferenceType(resultType) is { } conditionalClass)
            {
                var scopeBound = trueIsScopeBound || falseIsScopeBound;
                _ownedClassValues.RemoveWhere(owner => owner.Value == whenTrue.Value || owner.Value == whenFalse.Value);
                _ownedStringValues.Remove(whenTrue.Value); _ownedStringValues.Remove(whenFalse.Value);
                _ownedArrayValues.Remove(whenTrue.Value); _ownedArrayValues.Remove(whenFalse.Value);
                if (scopeBound) _scopeBoundClassValues.Add(result); else _ownedClassValues.Add((result, conditionalClass));
            }
            else if (aggregateResult) _ownedAggregateValues.Add((result, resultType));
            foreach (var (pattern, _) in patternStates) _activeTypePatternBindings.Remove(pattern);
            return (result, resultType);
        }

        private void RestoreOwnedTemporaryState(
            IReadOnlyCollection<string> strings,
            IReadOnlyCollection<string> arrays,
            IReadOnlyCollection<(string Value, string Type)> classes,
            IReadOnlyCollection<string> scopedClasses,
            IReadOnlyCollection<string> scopedArrays,
            IReadOnlyCollection<(string Value, string Type)> aggregates)
        {
            _ownedStringValues.Clear(); _ownedStringValues.UnionWith(strings);
            _ownedArrayValues.Clear(); _ownedArrayValues.UnionWith(arrays);
            _ownedClassValues.Clear(); _ownedClassValues.UnionWith(classes);
            _ownedAggregateValues.Clear(); _ownedAggregateValues.UnionWith(aggregates);
            _scopeBoundClassValues.Clear(); _scopeBoundClassValues.UnionWith(scopedClasses);
            _scopeBoundArrayValues.Clear(); _scopeBoundArrayValues.UnionWith(scopedArrays);
        }
        private (string Value, string Type) EmitNullConditionalClassField(MemberAccessExpression access, StructFieldBinding field)
        {
            var receiver = EmitExpression(access.Receiver);
            var receiverClass = GetClassReferenceType(receiver.Type) ?? throw new NotSupportedException("Null-conditional field access requires a class receiver.");
            var resultClass = GetClassReferenceType(field.TypeName);
            var nullableValueResult = IsNullableValueType(field.TypeName) || IsNullableValuePayloadType(field.TypeName);
            var nullableStringResult = IsStringType(field.TypeName);
            var nullableArrayResult = IsManagedArrayType(field.TypeName);
            if (resultClass is null && !nullableValueResult && !nullableStringResult && !nullableArrayResult) throw new NotSupportedException("Null-conditional field result type was not validated by semantic analysis.");
            var resultType = nullableValueResult ? IsNullableValueType(field.TypeName) ? field.TypeName : field.TypeName + "?" : nullableStringResult ? "string?" : nullableArrayResult ? IsNullableArrayType(field.TypeName) ? field.TypeName : field.TypeName + "?" : resultClass! + "?";
            EnsureOwnedClass(receiver, receiverClass);
            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {receiver.Value}, null");
            var nullLabel = NewLabel("null.field.null"); var valueLabel = NewLabel("null.field.value"); var endLabel = NewLabel("null.field.end");
            Emit($"br i1 {isNull}, label %{nullLabel}, label %{valueLabel}"); _terminated = true;

            Label(nullLabel);
            Emit($"call void @luma_class_release_dynamic(ptr {receiver.Value})");
            var nullBlock = _block; Branch(endLabel);

            Label(valueLabel);
            var safeType = string.Concat(field.OwnerType.Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : '_'));
            var address = NewTemp(); Emit($"{address} = getelementptr %class.{safeType}, ptr {receiver.Value}, i32 0, i32 {field.Index + 4}");
            var loaded = NewTemp(); Emit($"{loaded} = load {LlvmType(field.TypeName)}, ptr {address}");
            var scopeBoundFieldRead = field.IsScopeBound && IsScopeBoundClassExpression(access.Receiver);
            if (resultClass is not null && scopeBoundFieldRead) _scopeBoundClassValues.Add(loaded);
            else if (resultClass is not null) EnsureOwnedClass((loaded, field.TypeName), resultClass);
            if (nullableStringResult) EnsureOwnedString((loaded, field.TypeName));
            if (nullableArrayResult && scopeBoundFieldRead) _scopeBoundArrayValues.Add(loaded);
            else if (nullableArrayResult) EnsureOwnedArray((loaded, field.TypeName));
            Emit($"call void @luma_class_release_dynamic(ptr {receiver.Value})");
            var valueBlock = _block;
            var presentValue = nullableValueResult && !IsNullableValueType(field.TypeName) ? WrapNullableValue(loaded, field.TypeName, resultType) : loaded;
            Branch(endLabel);

            Label(endLabel);
            var result = NewTemp();
            var llvmResultType = LlvmType(resultType);
            var emptyValue = nullableValueResult ? "zeroinitializer" : "null";
            Emit($"{result} = phi {llvmResultType} [ {emptyValue}, %{nullBlock} ], [ {presentValue}, %{valueBlock} ]");
            _ownedClassValues.Remove((receiver.Value, receiverClass));
            if (resultClass is not null)
            {
                _ownedClassValues.Remove((loaded, resultClass));
                _ownedClassValues.Add((result, resultClass));
            }
            if (nullableStringResult)
            {
                _ownedStringValues.Remove(loaded); _ownedStringValues.Add(result);
            }
            if (nullableArrayResult)
            {
                _ownedArrayValues.Remove(loaded); _ownedArrayValues.Add(result);
            }
            return (result, resultType);
        }
        private (string Value, string Type) EmitNullConditionalCall(CallExpression expression)
        {
            if (expression.Target is not MemberAccessExpression { IsNullConditional: true } member || !callTargets.TryGetValue(expression, out var target))
                throw new NotSupportedException("The null-conditional call has no bound instance method.");
            var receiver = EmitExpression(member.Receiver);
            var receiverClass = GetClassReferenceType(receiver.Type) ?? throw new NotSupportedException("Null-conditional calls require a class receiver.");
            var returnType = target.Declaration.ReturnType;
            var resultClass = returnType == "void" ? null : GetClassReferenceType(returnType);
            var nullableValueResult = returnType != "void" && (IsNullableValueType(returnType) || IsNullableValuePayloadType(returnType));
            var nullableStringResult = returnType != "void" && IsStringType(returnType);
            var nullableArrayResult = returnType != "void" && IsManagedArrayType(returnType);
            if (returnType != "void" && resultClass is null && !nullableValueResult && !nullableStringResult && !nullableArrayResult)
                throw new NotSupportedException($"Null-conditional call result type '{returnType}' was not validated by semantic analysis.");
            var resultType = nullableValueResult ? IsNullableValueType(returnType) ? returnType : returnType + "?" : nullableStringResult ? "string?" : nullableArrayResult ? IsNullableArrayType(returnType) ? returnType : returnType + "?" : resultClass is null ? "void" : resultClass + "?";
            EnsureOwnedClass(receiver, receiverClass);
            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {receiver.Value}, null");
            var nullLabel = NewLabel("null.call.null"); var callLabel = NewLabel("null.call.invoke"); var endLabel = NewLabel("null.call.end");
            Emit($"br i1 {isNull}, label %{nullLabel}, label %{callLabel}"); _terminated = true;

            Label(nullLabel);
            Emit($"call void @luma_class_release_dynamic(ptr {receiver.Value})");
            var nullBlock = _block; Branch(endLabel);

            Label(callLabel);
            var called = EmitCall(expression, receiver.Value);
            var callValue = nullableValueResult && !IsNullableValueType(returnType) ? WrapNullableValue(called.Item1, returnType, resultType) : called.Item1;
            if (nullableStringResult) EnsureOwnedString(called);
            if (nullableArrayResult) EnsureOwnedArray(called);
            Emit($"call void @luma_class_release_dynamic(ptr {receiver.Value})");
            var callBlock = _block; Branch(endLabel);

            Label(endLabel);
            _ownedClassValues.Remove((receiver.Value, receiverClass));
            if (returnType == "void") return ("", "void");
            var result = NewTemp();
            var llvmResultType = LlvmType(resultType);
            var emptyValue = nullableValueResult ? "zeroinitializer" : "null";
            Emit($"{result} = phi {llvmResultType} [ {emptyValue}, %{nullBlock} ], [ {callValue}, %{callBlock} ]");
            if (resultClass is not null)
            {
                _ownedClassValues.Remove((called.Item1, resultClass));
                _ownedClassValues.Add((result, resultClass));
            }
            if (nullableStringResult)
            {
                _ownedStringValues.Remove(called.Item1); _ownedStringValues.Add(result);
            }
            if (nullableArrayResult)
            {
                _ownedArrayValues.Remove(called.Item1); _ownedArrayValues.Add(result);
            }
            return (result, resultType);
        }
        private (string Value, string Type) EmitCall(CallExpression expression, string? instanceReceiverOverride = null,
            bool classConstructorCompletes = true, bool checkPendingException = true, bool releaseOwnedArguments = true,
            IReadOnlyList<(string Value, string Type)>? sourceArgumentOverrides = null, MethodSymbol? explicitTarget = null)
        {
            if (delegateInvocations.TryGetValue(expression, out var invokedDelegate))
                return EmitDelegateCall(expression, invokedDelegate);
            MethodSymbol target;
            BoundCallArguments binding;
            if (explicitTarget is not null)
            {
                target = explicitTarget;
                if (expression.Arguments.Count != target.Declaration.Parameters.Count)
                    throw new InvalidOperationException("A direct compiler-generated call must provide every parameter in declaration order.");
                binding = new(Enumerable.Range(0, expression.Arguments.Count).ToArray(), Enumerable.Repeat<Expression?>(null, target.Declaration.Parameters.Count).ToArray());
            }
            else
            {
                if (!callTargets.TryGetValue(expression, out target!)) throw new NotSupportedException("No bound target was recorded for this static call.");
                if (!callArguments.TryGetValue(expression, out binding!)) throw new NotSupportedException("No bound argument mapping was recorded for this static call.");
            }
            string? instanceReceiverAddress = instanceReceiverOverride;
            (string Value, string Type)? ownedInstanceReceiver = null;
            if (target.IsStructInstanceMethod)
            {
                if (instanceReceiverOverride is not null) instanceReceiverAddress = instanceReceiverOverride;
                else if (expression.Target is MemberAccessExpression instanceTarget)
                {
                    var (receiverType, receiverAddress) = ResolveAddress(instanceTarget.Receiver);
                    if (receiverType == QualifiedTypeName(target.Owner) + "*")
                    {
                        instanceReceiverAddress = NewTemp();
                        Emit($"{instanceReceiverAddress} = load ptr, ptr {receiverAddress}");
                    }
                    else instanceReceiverAddress = receiverAddress;
                }
                else if (expression.Target is NameExpression) instanceReceiverAddress = "%this";
                else throw new InvalidOperationException("A bound struct instance method requires an explicit or implicit receiver.");
            }
            else if (target.IsClassInstanceMethod && instanceReceiverOverride is null)
            {
                if (expression.Target is MemberAccessExpression instanceTarget)
                {
                    var receiver = EmitExpression(instanceTarget.Receiver);
                    if (receiver.Type.EndsWith('*') && classTypes.ContainsKey(receiver.Type[..^1]))
                        instanceReceiverAddress = receiver.Value;
                    else
                    {
                        instanceReceiverAddress = receiver.Value;
                        if (GetClassReferenceType(receiver.Type) is not null) ownedInstanceReceiver = receiver;
                    }
                }
                else if (expression.Target is NameExpression) instanceReceiverAddress = "%this";
                else throw new InvalidOperationException("A bound class instance method requires an explicit or implicit receiver.");
            }
            else if (interfaceCallSlots.ContainsKey(expression) && instanceReceiverOverride is null)
            {
                if (expression.Target is not MemberAccessExpression instanceTarget)
                    throw new InvalidOperationException("A bound interface method requires an explicit receiver.");
                var receiver = EmitExpression(instanceTarget.Receiver);
                instanceReceiverAddress = receiver.Value;
                if (GetClassReferenceType(receiver.Type) is not null) ownedInstanceReceiver = receiver;
            }

            // Evaluate supplied arguments in source order, then arrange the values into parameter order.
            var valuesByParameter = new (string Value, string Type, bool IsAddress)?[target.Declaration.Parameters.Count];
            var ownedStringArguments = new List<(string Value, string Type)>();
            var ownedArrayArguments = new List<(string Value, string Type)>();
            var ownedClassArguments = new List<(string Value, string Type)>();
            var ownedAggregateArguments = new List<(string Value, string Type)>();
            var variadicValues = new Dictionary<int, List<(string Value, string Type)>>();
            for (var sourceIndex = 0; sourceIndex < expression.Arguments.Count; sourceIndex++)
            {
                var parameterIndex = binding.ParameterBySource[sourceIndex];
                var parameter = target.Declaration.Parameters[parameterIndex]; var parameterType = parameter.TypeName;
                if (parameter.IsVariadic && binding.UsesVariadicExpansion)
                {
                    var supplied = EmitExpression(expression.Arguments[sourceIndex]);
                    if (IsStringType(supplied.Type)) ownedStringArguments.Add(supplied);
                    if (SemanticAnalyzer.TryParseManagedArrayType(supplied.Type, out _, out _)) ownedArrayArguments.Add(supplied);
                    if (GetClassReferenceType(supplied.Type) is not null) ownedClassArguments.Add(supplied);
                    if (IsManagedAggregateType(supplied.Type)) ownedAggregateArguments.Add(supplied);
                    if (!variadicValues.TryGetValue(parameterIndex, out var values)) variadicValues[parameterIndex] = values = [];
                    values.Add(supplied);
                    continue;
                }
                if (expression.Arguments[sourceIndex] is InlineOutVariableExpression inlineOut)
                {
                    var captureEnvironment = FindClosureEnvironmentForVariable(inlineOut.Name);
                    var address = captureEnvironment is not null &&
                        captureEnvironment.CapturedFieldIndices.TryGetValue(inlineOut.Name, out var capturedFieldIndex)
                        ? EmitClosureFieldAddress(captureEnvironment, capturedFieldIndex)
                        : NewTemp();
                    if (captureEnvironment is null || !captureEnvironment.CapturedFieldIndices.ContainsKey(inlineOut.Name))
                        Emit($"{address} = alloca {LlvmType(parameterType)}");
                    if (IsStringType(parameterType)) Emit($"store ptr {DefaultStorageValue(parameterType)}, ptr {address}");
                    Declare(inlineOut.Name, parameterType, address);
                    valuesByParameter[parameterIndex] = (address, "ptr", true);
                }
                else if (parameter.PassingMode is "ref" or "out")
                {
                    var (_, address) = ResolveAssignmentTarget(expression.Arguments[sourceIndex]);
                    valuesByParameter[parameterIndex] = (address, "ptr", true);
                }
                else
                {
                    var supplied = sourceArgumentOverrides is not null
                        ? sourceArgumentOverrides[sourceIndex]
                        : EmitExpression(expression.Arguments[sourceIndex]);
                    if (IsStringType(supplied.Type)) ownedStringArguments.Add(supplied);
                    if (SemanticAnalyzer.TryParseManagedArrayType(supplied.Type, out _, out _)) ownedArrayArguments.Add(supplied);
                    if (GetClassReferenceType(supplied.Type) is not null) ownedClassArguments.Add(supplied);
                    if (IsManagedAggregateType(supplied.Type)) ownedAggregateArguments.Add(supplied);
                    var convertedArgument = CastValue(supplied.Value, supplied.Type, parameterType);
                    if (GetClassReferenceType(parameterType) is not null && GetClassReferenceType(supplied.Type) is null &&
                        (IsBoxableScalarOrEnum(supplied.Type) || IsNullableValueType(supplied.Type)))
                        ownedClassArguments.Add((convertedArgument, parameterType));
                    valuesByParameter[parameterIndex] = (convertedArgument, parameterType, false);
                }
            }
            for (var parameterIndex = 0; parameterIndex < valuesByParameter.Length; parameterIndex++)
            {
                var parameter = target.Declaration.Parameters[parameterIndex];
                if (parameter.IsVariadic && binding.UsesVariadicExpansion)
                {
                    var elementType = parameter.TypeName[..^2];
                    var elements = variadicValues.GetValueOrDefault(parameterIndex) ?? [];
                    var length = elements.Count.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    var array = NewTemp();
                    EmitArrayAllocation(array, length, 1, [length], elementType);
                    var data = NewTemp(); Emit($"{data} = getelementptr %luma.array, ptr {array}, i32 0, i32 7");
                    for (var elementIndex = 0; elementIndex < elements.Count; elementIndex++)
                    {
                        var element = elements[elementIndex];
                        var converted = CastValue(element.Value, element.Type, elementType);
                        var address = NewTemp(); Emit($"{address} = getelementptr {LlvmType(elementType)}, ptr {data}, i32 {elementIndex}");
                        if (IsStringType(elementType)) EnsureOwnedString(element);
                        else if (SemanticAnalyzer.TryParseManagedArrayType(elementType, out _, out _)) EnsureOwnedArray(element);
                        else if (GetClassReferenceType(elementType) is { } classElementType) EnsureOwnedClass(element, classElementType);
                        else if (IsManagedAggregateType(elementType)) EnsureOwnedAggregate((converted, elementType));
                        Emit($"store {LlvmType(elementType)} {converted}, ptr {address}");
                        if (IsStringType(elementType)) _ownedStringValues.Remove(element.Value);
                        else if (SemanticAnalyzer.TryParseManagedArrayType(elementType, out _, out _)) _ownedArrayValues.Remove(element.Value);
                        else if (GetClassReferenceType(elementType) is { } ownedClassType) _ownedClassValues.Remove((element.Value, ownedClassType));
                        else if (IsManagedAggregateType(elementType))
                        {
                            _ownedAggregateValues.Remove((converted, elementType));
                            if (element.Type != elementType) ReleaseOwnedAggregate(element);
                        }
                    }
                    _ownedArrayValues.Add(array);
                    ownedArrayArguments.Add((array, parameter.TypeName));
                    valuesByParameter[parameterIndex] = (array, parameter.TypeName, false);
                    continue;
                }
                if (valuesByParameter[parameterIndex] is not null) continue;
                var defaultExpression = binding.DefaultsByParameter[parameterIndex] ?? throw new InvalidOperationException("A required argument is missing from a validated call.");
                var parameterType = parameter.TypeName;
                var defaultValue = EmitExpression(defaultExpression);
                if (IsStringType(defaultValue.Type)) ownedStringArguments.Add(defaultValue);
                if (SemanticAnalyzer.TryParseManagedArrayType(defaultValue.Type, out _, out _)) ownedArrayArguments.Add(defaultValue);
                if (GetClassReferenceType(defaultValue.Type) is not null) ownedClassArguments.Add(defaultValue);
                if (IsManagedAggregateType(defaultValue.Type)) ownedAggregateArguments.Add(defaultValue);
                var convertedDefault = CastValue(defaultValue.Value, defaultValue.Type, parameterType);
                if (GetClassReferenceType(parameterType) is not null && GetClassReferenceType(defaultValue.Type) is null &&
                    (IsBoxableScalarOrEnum(defaultValue.Type) || IsNullableValueType(defaultValue.Type)))
                    ownedClassArguments.Add((convertedDefault, parameterType));
                valuesByParameter[parameterIndex] = (convertedDefault, parameterType, false);
            }

            var resultType = target.Declaration.ReturnType; var result = resultType == "void" ? null : NewTemp();
            var returnAttribute = target.Declaration.IsExtern ? AbiAttribute(target.Declaration.ReturnType, enumTypes) : "";
            var llvmCallArguments = valuesByParameter.Select((v, index) =>
            {
                var value = v!.Value;
                if (value.IsAddress) return $"ptr {value.Value}";
                var llvmType = LlvmType(value.Type);
                var attribute = target.Declaration.IsExtern ? AbiAttribute(target.Declaration.Parameters[index].TypeName, enumTypes) : "";
                return attribute.Length == 0 ? $"{llvmType} {value.Value}" : $"{llvmType} {attribute.Trim()} {value.Value}";
            });
            var allArguments = instanceReceiverAddress is null ? llvmCallArguments : new[] { $"ptr {instanceReceiverAddress}" }.Concat(llvmCallArguments);
            if (target.IsClassConstructor) allArguments = allArguments.Append($"i1 {(classConstructorCompletes ? "true" : "false")}");
            if (target.Declaration.IsStatic) EnsureTypeInitialized(QualifiedTypeName(target.Owner));
            string call;
            if (virtualCallSlots.TryGetValue(expression, out var virtualSlot) && instanceReceiverAddress is not null)
            {
                var declaringType = QualifiedTypeName(target.Owner);
                var typedClass = Safe(declaringType);
                var descriptorAddress = NewTemp(); Emit($"{descriptorAddress} = getelementptr %class.{typedClass}, ptr {instanceReceiverAddress}, i32 0, i32 0");
                var descriptor = NewTemp(); Emit($"{descriptor} = load ptr, ptr {descriptorAddress}");
                var vtableAddress = NewTemp(); Emit($"{vtableAddress} = getelementptr %luma.typedesc, ptr {descriptor}, i32 0, i32 2");
                var vtable = NewTemp(); Emit($"{vtable} = load ptr, ptr {vtableAddress}");
                var entryAddress = NewTemp(); Emit($"{entryAddress} = getelementptr ptr, ptr {vtable}, i32 {virtualSlot}");
                var function = NewTemp(); Emit($"{function} = load ptr, ptr {entryAddress}");
                call = $"call {returnAttribute}{LlvmType(resultType)} {function}({string.Join(", ", allArguments)})";
            }
            else if (interfaceCallSlots.TryGetValue(expression, out var interfaceSlot) && instanceReceiverAddress is not null)
            {
                var descriptor = NewTemp(); Emit($"{descriptor} = load ptr, ptr {instanceReceiverAddress}");
                var tableAddress = NewTemp(); Emit($"{tableAddress} = getelementptr %luma.typedesc, ptr {descriptor}, i32 0, i32 6");
                var table = NewTemp(); Emit($"{table} = load ptr, ptr {tableAddress}");
                var entryAddress = NewTemp(); Emit($"{entryAddress} = getelementptr ptr, ptr {table}, i32 {interfaceSlot}");
                var function = NewTemp(); Emit($"{function} = load ptr, ptr {entryAddress}");
                call = $"call {returnAttribute}{LlvmType(resultType)} {function}({string.Join(", ", allArguments)})";
            }
            else if (IsWeakRuntimeHelper(target))
            {
                var intrinsic = target.Declaration.Name switch
                {
                    "CreateControlRuntime" => "luma_weak_create",
                    "IsAliveRuntime" => "luma_weak_is_alive",
                    "GetTargetRuntime" => "luma_weak_get_target",
                    "ReleaseControlRuntime" => "luma_weak_release",
                    _ => throw new NotSupportedException($"Unknown weak-reference runtime helper '{target.Declaration.Name}'.")
                };
                call = $"call {LlvmType(resultType)} @{intrinsic}({string.Join(", ", allArguments)})";
            }
            else call = $"call {returnAttribute}{LlvmType(resultType)} @{FunctionName(target, methods)}({string.Join(", ", allArguments)})";
            Emit(result is null ? call : $"{result} = {call}");
            if (!target.Declaration.IsExtern && checkPendingException) EmitCheckPendingException();
            if (releaseOwnedArguments)
            {
                foreach (var argument in ownedStringArguments) ReleaseOwnedString(argument);
                foreach (var argument in ownedArrayArguments) ReleaseOwnedArray(argument);
                foreach (var argument in ownedClassArguments) ReleaseOwnedClass(argument);
                foreach (var argument in ownedAggregateArguments) ReleaseOwnedAggregate(argument);
            }
            if (ownedInstanceReceiver is { } ownedReceiver) ReleaseOwnedClass(ownedReceiver);
            if (result is not null && IsStringType(resultType)) _ownedStringValues.Add(result);
            if (result is not null && SemanticAnalyzer.TryParseManagedArrayType(resultType, out _, out _)) _ownedArrayValues.Add(result);
            if (result is not null && GetClassReferenceType(resultType) is { } resultClass) _ownedClassValues.Add((result, resultClass));
            if (result is not null && IsManagedAggregateType(resultType)) _ownedAggregateValues.Add((result, resultType));
            return (result ?? "", resultType);
        }
        private (string Address, string ElementType) EmitIndexAddress(IndexExpression expression, (string Value, string Type)? managedArrayReceiverOverride = null)
        {
            if (managedArrayIndexAccesses.TryGetValue(expression, out var managedIndex))
            {
                if (expression.Indices.Count != managedIndex.Rank) throw new NotSupportedException("A managed-array index reached emission with the wrong rank.");
                var array = managedArrayReceiverOverride ?? EmitExpression(expression.Receiver);
                string? flattenedIndex = null;
                for (var dimensionIndex = 0; dimensionIndex < managedIndex.Rank; dimensionIndex++)
                {
                    var sourceIndex = EmitExpression(expression.Indices[dimensionIndex]);
                    var index = CastValue(sourceIndex.Value, sourceIndex.Type, "int32");
                    var lengthAddress = NewTemp(); Emit($"{lengthAddress} = getelementptr %luma.array, ptr {array.Value}, i32 0, i32 6, i32 {dimensionIndex}");
                    var length = NewTemp(); Emit($"{length} = load i32, ptr {lengthAddress}");
                    var nonnegative = NewTemp(); Emit($"{nonnegative} = icmp sge i32 {index}, 0");
                    var inRange = NewTemp(); Emit($"{inRange} = icmp slt i32 {index}, {length}");
                    var valid = NewTemp(); Emit($"{valid} = and i1 {nonnegative}, {inRange}");
                    var invalid = NewTemp(); Emit($"{invalid} = xor i1 {valid}, true");
                    TrapIf(invalid, "luma_throw_bounds");
                    if (flattenedIndex is null) flattenedIndex = index;
                    else
                    {
                        var rowOffset = NewTemp(); Emit($"{rowOffset} = mul i32 {flattenedIndex}, {length}");
                        var nextOffset = NewTemp(); Emit($"{nextOffset} = add i32 {rowOffset}, {index}");
                        flattenedIndex = nextOffset;
                    }
                }
                var data = NewTemp(); Emit($"{data} = getelementptr %luma.array, ptr {array.Value}, i32 0, i32 7");
                var address = NewTemp(); Emit($"{address} = getelementptr {LlvmType(managedIndex.ElementType)}, ptr {data}, i32 {flattenedIndex}");
                return (address, managedIndex.ElementType);
            }
            if (expression.Indices.Count != 1) throw new NotSupportedException("Pointer indexing requires exactly one index.");
            if (TryGetInlineArrayAddress(expression.Receiver, out var arrayType, out var arrayElementType, out var arrayLength, out var arrayAddress))
            {
                var index = EmitExpression(expression.Indices[0]);
                if (_checkedContext) EmitInlineArrayBoundsCheck(index, arrayLength);
                var nativeInteger = nativePointerSize == 8 ? "int64" : "int32";
                var offset = CastValue(index.Value, index.Type, nativeInteger);
                var address = NewTemp();
                Emit($"{address} = getelementptr {LlvmType(arrayType)}, ptr {arrayAddress}, i32 0, {LlvmType(nativeInteger)} {offset}");
                return (address, arrayElementType);
            }

            var pointer = EmitExpression(expression.Receiver);
            if (!pointer.Type.EndsWith('*')) throw new InvalidOperationException("Validated indexing requires an inline array local or a typed pointer.");
            var pointerIndex = EmitExpression(expression.Indices[0]);
            var elementType = pointer.Type[..^1];
            var pointerInteger = nativePointerSize == 8 ? "int64" : "int32";
            var pointerOffset = CastValue(pointerIndex.Value, pointerIndex.Type, pointerInteger);
            var pointerAddress = NewTemp();
            Emit($"{pointerAddress} = getelementptr {LlvmType(elementType)}, ptr {pointer.Value}, {LlvmType(pointerInteger)} {pointerOffset}");
            return (pointerAddress, elementType);
        }
        private (string Value, string Type) EmitNullConditionalArrayIndex(IndexExpression expression, ManagedArrayIndexBinding binding)
        {
            var receiver = EmitExpression(expression.Receiver);
            var resultType = binding.ElementType switch
            {
                "string" => "string?",
                var element when IsNullableStringType(element) || IsNullableValueType(element) || IsNullableArrayType(element) ||
                    GetClassReferenceType(element) is not null && element.EndsWith("?", StringComparison.Ordinal) => element,
                var element when IsNullableValuePayloadType(element) => element + "?",
                var element when IsManagedArrayType(element) => element + "?",
                var element when GetClassReferenceType(element) is not null => element + "?",
                _ => throw new NotSupportedException($"Null-conditional array element type '{binding.ElementType}' has no nullable representation.")
            };
            var releaseReceiver = _ownedArrayValues.Remove(receiver.Value);
            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {receiver.Value}, null");
            var nullLabel = NewLabel("null.array.index.null"); var valueLabel = NewLabel("null.array.index.value"); var endLabel = NewLabel("null.array.index.end");
            Emit($"br i1 {isNull}, label %{nullLabel}, label %{valueLabel}"); _terminated = true;
            Label(nullLabel);
            if (releaseReceiver) Emit($"call void @luma_array_release(ptr {receiver.Value})");
            var nullBlock = _block; Branch(endLabel);
            Label(valueLabel);
            var (elementAddress, elementType) = EmitIndexAddress(expression, receiver);
            var loaded = NewTemp(); Emit($"{loaded} = load {LlvmType(elementType)}, ptr {elementAddress}");
            if (IsScopeBoundArrayExpression(expression.Receiver))
            {
                if (GetClassReferenceType(elementType) is not null) _scopeBoundClassValues.Add(loaded);
                if (SemanticAnalyzer.TryParseManagedArrayType(elementType, out _, out _)) _scopeBoundArrayValues.Add(loaded);
            }
            var presentValue = loaded;
            if (IsNullableValueType(resultType)) presentValue = WrapNullableValue(loaded, elementType, resultType);
            else if (IsStringType(resultType)) EnsureOwnedString((loaded, elementType));
            else if (IsManagedArrayType(resultType)) EnsureOwnedArray((loaded, elementType));
            else if (GetClassReferenceType(resultType) is { } classResultType) EnsureOwnedClassElement((loaded, elementType), classResultType);
            else if (IsManagedAggregateType(resultType))
            {
                EnsureOwnedAggregate((presentValue, resultType));
                _ownedAggregateValues.Remove((presentValue, resultType));
            }
            if (releaseReceiver) Emit($"call void @luma_array_release(ptr {receiver.Value})");
            var valueBlock = _block; Branch(endLabel);
            Label(endLabel);
            var emptyValue = IsNullableValueType(resultType) ? "zeroinitializer" : "null";
            var result = NewTemp(); Emit($"{result} = phi {LlvmType(resultType)} [ {emptyValue}, %{nullBlock} ], [ {presentValue}, %{valueBlock} ]");
            if (IsStringType(resultType)) { _ownedStringValues.Remove(loaded); _ownedStringValues.Add(result); }
            else if (IsManagedArrayType(resultType)) { _ownedArrayValues.Remove(loaded); _ownedArrayValues.Add(result); }
            else if (GetClassReferenceType(resultType) is { } resultClassType)
            {
                var ownedClass = _ownedClassValues.FirstOrDefault(owned => owned.Value == loaded);
                if (ownedClass.Value == loaded)
                {
                    _ownedClassValues.Remove(ownedClass);
                    _ownedClassValues.Add((result, resultClassType));
                }
            }
            else if (IsManagedAggregateType(resultType)) _ownedAggregateValues.Add((result, resultType));
            if (IsScopeBoundArrayExpression(expression.Receiver))
            {
                if (GetClassReferenceType(resultType) is not null) _scopeBoundClassValues.Add(result);
                if (SemanticAnalyzer.TryParseManagedArrayType(resultType, out _, out _)) _scopeBoundArrayValues.Add(result);
            }
            return (result, resultType);
        }
        private bool TryGetInlineArrayAddress(Expression expression, out string arrayType, out string elementType, out int length, out string address)
        {
            if (expression is NameExpression name && TryLookup(name.Name, out var local) &&
                SemanticAnalyzer.TryParseInlineArrayType(local.Type, out elementType, out length))
            { arrayType = local.Type; address = local.Pointer; return true; }
            if (expression is MemberAccessExpression member && structFields.TryGetValue(member, out var field) &&
                SemanticAnalyzer.TryParseInlineArrayType(field.TypeName, out elementType, out length))
            { arrayType = field.TypeName; address = EmitStructFieldAddress(member, field); return true; }
            if (expression is MemberAccessExpression classMember && classFields.TryGetValue(classMember, out var classField) &&
                SemanticAnalyzer.TryParseInlineArrayType(classField.TypeName, out elementType, out length))
            { arrayType = classField.TypeName; address = EmitClassFieldAddress(classMember, classField); return true; }
            if (expression is NameExpression classFieldName && unqualifiedClassFields.TryGetValue(classFieldName, out var unqualifiedClassField) &&
                SemanticAnalyzer.TryParseInlineArrayType(unqualifiedClassField.TypeName, out elementType, out length))
            { arrayType = unqualifiedClassField.TypeName; address = EmitCurrentClassFieldAddress(unqualifiedClassField); return true; }
            if (expression is MemberAccessExpression staticMember && staticFields.TryGetValue(staticMember, out var staticField) &&
                SemanticAnalyzer.TryParseInlineArrayType(staticField.TypeName, out elementType, out length))
            { EnsureTypeInitialized(staticField.OwnerType); arrayType = staticField.TypeName; address = $"@{staticField.GlobalName}"; return true; }
            arrayType = elementType = ""; length = 0; address = ""; return false;
        }
        private string EmitStructFieldAddress(MemberAccessExpression access, StructFieldBinding field)
        {
            if (field.ReceiverIsPointer)
            {
                var receiverValue = EmitExpression(access.Receiver);
                var pointerAddress = NewTemp();
                Emit($"{pointerAddress} = getelementptr {LlvmType(field.OwnerType)}, ptr {receiverValue.Value}, i32 0, i32 {field.Index}");
                return pointerAddress;
            }
            var (receiverType, receiverAddress) = ResolveNullableStructAddress(access.Receiver);
            var address = NewTemp();
            Emit($"{address} = getelementptr {LlvmType(receiverType)}, ptr {receiverAddress}, i32 0, i32 {field.Index}");
            return address;
        }
        private (string Type, string Pointer) ResolveNullableStructAddress(Expression expression)
        {
            var storageExpression = expression is NullAssertionExpression assertion ? assertion.Operand : expression;
            var (receiverType, receiverAddress) = ResolveAddress(storageExpression);
            if (!IsNullableValueType(receiverType) || !structTypes.ContainsKey(NullableUnderlyingType(receiverType))) return (receiverType, receiverAddress);
            var aggregate = NewTemp(); Emit($"{aggregate} = load {LlvmType(receiverType)}, ptr {receiverAddress}");
            var hasValue = NewTemp(); Emit($"{hasValue} = extractvalue {LlvmType(receiverType)} {aggregate}, 0");
            var missing = NewTemp(); Emit($"{missing} = xor i1 {hasValue}, true"); TrapIf(missing, "luma_throw_null");
            var payloadAddress = NewTemp(); Emit($"{payloadAddress} = getelementptr {LlvmType(receiverType)}, ptr {receiverAddress}, i32 0, i32 1");
            return (NullableUnderlyingType(receiverType), payloadAddress);
        }
        private string EmitCurrentStructFieldAddress(StructFieldBinding field)
        {
            var address = NewTemp();
            Emit($"{address} = getelementptr {LlvmType(field.OwnerType)}, ptr %this, i32 0, i32 {field.Index}");
            return address;
        }
        private string EmitClassFieldAddress(MemberAccessExpression access, StructFieldBinding field,
            (string Value, string Type)? receiverOverride = null)
        {
            var receiver = receiverOverride ?? EmitExpression(access.Receiver);
            var objectValue = receiver.Value;
            var safeType = string.Concat(field.OwnerType.Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : '_'));
            var address = NewTemp();
            Emit($"{address} = getelementptr %class.{safeType}, ptr {objectValue}, i32 0, i32 {field.Index + 4}");
            return address;
        }
        private string EmitCurrentClassFieldAddress(StructFieldBinding field)
        {
            var safeType = string.Concat(field.OwnerType.Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : '_'));
            var address = NewTemp();
            Emit($"{address} = getelementptr %class.{safeType}, ptr %this, i32 0, i32 {field.Index + 4}");
            return address;
        }
        private string AllocateClosureEnvironment(ClosureEnvironmentBinding environment)
        {
            var safeType = Safe(environment.TypeName);
            var sizeType = nativePointerSize == 8 ? "i64" : "i32";
            var sizeAddress = NewTemp(); Emit($"{sizeAddress} = getelementptr %class.{safeType}, ptr null, i32 1");
            var size = NewTemp(); Emit($"{size} = ptrtoint ptr {sizeAddress} to {sizeType}");
            var instance = NewTemp(); Emit($"{instance} = call ptr @malloc({sizeType} {size})");
            var allocationFailed = NewTemp(); Emit($"{allocationFailed} = icmp eq ptr {instance}, null");
            TrapIf(allocationFailed, "luma_throw_out_of_memory");
            var descriptorAddress = NewTemp(); Emit($"{descriptorAddress} = getelementptr %class.{safeType}, ptr {instance}, i32 0, i32 0");
            Emit($"store ptr @luma_classdesc_{safeType}, ptr {descriptorAddress}");
            var countAddress = NewTemp(); Emit($"{countAddress} = getelementptr %class.{safeType}, ptr {instance}, i32 0, i32 1");
            Emit($"store i32 1, ptr {countAddress}");
            var flagsAddress = NewTemp(); Emit($"{flagsAddress} = getelementptr %class.{safeType}, ptr {instance}, i32 0, i32 2");
            Emit($"store i32 0, ptr {flagsAddress}");
            var weakAddress = NewTemp(); Emit($"{weakAddress} = getelementptr %class.{safeType}, ptr {instance}, i32 0, i32 3");
            Emit($"store ptr null, ptr {weakAddress}");
            foreach (var field in classTypes[environment.TypeName].Fields)
            {
                var address = NewTemp(); Emit($"{address} = getelementptr %class.{safeType}, ptr {instance}, i32 0, i32 {field.Index + 4}");
                Emit($"store {LlvmType(field.TypeName)} {DefaultStorageValue(field.TypeName)}, ptr {address}");
            }
            Emit($"store ptr {instance}, ptr {_closureEnvironmentSlots[environment.TypeName]}");
            return instance;
        }
        private string EmitClosureFieldAddress(ClosureEnvironmentBinding environment, int fieldIndex)
        {
            var instance = NewTemp();
            Emit($"{instance} = load ptr, ptr {_closureEnvironmentSlots[environment.TypeName]}");
            var address = NewTemp();
            Emit($"{address} = getelementptr %class.{Safe(environment.TypeName)}, ptr {instance}, i32 0, i32 {fieldIndex + 4}");
            return address;
        }
        private (string Value, string Type) EmitBaseExpression()
        {
            var ownerType = QualifiedTypeName(method.Owner);
            if (!classTypes.TryGetValue(ownerType, out var ownerClass))
                throw new NotSupportedException("A validated base expression has no class owner.");

            var path = FindCapturedThisPath(ownerType, new HashSet<string>(StringComparer.Ordinal));
            if (path is null)
            {
                if (ownerClass.BaseTypeName is null)
                    throw new NotSupportedException("A validated base expression is missing its base class type.");
                return ("%this", ownerClass.BaseTypeName);
            }

            var instance = "%this";
            var currentType = ownerType;
            foreach (var field in path)
            {
                var address = NewTemp();
                Emit($"{address} = getelementptr %class.{Safe(currentType)}, ptr {instance}, i32 0, i32 {field.Index + 4}");
                var nextInstance = NewTemp();
                Emit($"{nextInstance} = load ptr, ptr {address}");
                instance = nextInstance;
                currentType = field.TypeName;
            }

            if (!classTypes.TryGetValue(currentType, out var enclosingClass) || enclosingClass.BaseTypeName is null)
                throw new NotSupportedException("A captured base expression has no enclosing base class.");
            return (instance, enclosingClass.BaseTypeName);
        }

        private IReadOnlyList<StructFieldSymbol>? FindCapturedThisPath(string typeName, HashSet<string> visited)
        {
            if (!visited.Add(typeName) || !classTypes.TryGetValue(typeName, out var type)) return null;
            var receiverField = type.Fields.FirstOrDefault(field => field.Declaration.Name == "$this");
            if (receiverField is not null)
            {
                var parentPath = FindCapturedThisPath(receiverField.TypeName, visited);
                return parentPath is null ? [receiverField] : [receiverField, .. parentPath];
            }
            foreach (var scopeField in type.Fields.Where(field => field.Declaration.Name.StartsWith("$scope", StringComparison.Ordinal)))
            {
                var parentPath = FindCapturedThisPath(scopeField.TypeName, visited);
                if (parentPath is not null) return [scopeField, .. parentPath];
            }
            return null;
        }
        private ClosureEnvironmentBinding? FindClosureEnvironmentForVariable(string name)
        {
            for (var index = _activeBlockClosureEnvironments.Count - 1; index >= 0; index--)
                if (_activeBlockClosureEnvironments[index].CapturedFieldIndices.ContainsKey(name))
                    return _activeBlockClosureEnvironments[index];
            for (var index = _activeForClosureEnvironments.Count - 1; index >= 0; index--)
                if (_activeForClosureEnvironments[index].CapturedFieldIndices.ContainsKey(name))
                    return _activeForClosureEnvironments[index];
            if (closureEnvironments.TryGetValue(method, out var methodEnvironment) &&
                methodEnvironment.CapturedFieldIndices.ContainsKey(name))
                return methodEnvironment;
            return null;
        }
        private void EmitInlineArrayBoundsCheck((string Value, string Type) index, int length)
        {
            var llvmType = LlvmType(index.Type);
            var upper = NewTemp();
            Emit($"{upper} = icmp {(IsUnsigned(index.Type) ? "ult" : "slt")} {llvmType} {index.Value}, {length}");
            var valid = upper;
            if (!IsUnsigned(index.Type))
            {
                var lower = NewTemp();
                Emit($"{lower} = icmp sge {llvmType} {index.Value}, 0");
                valid = NewTemp();
                Emit($"{valid} = and i1 {lower}, {upper}");
            }
            var invalid = NewTemp();
            Emit($"{invalid} = xor i1 {valid}, true");
            TrapIf(invalid, "luma_throw_bounds");
        }
        private string InferType(Expression expression) => expression switch { LiteralExpression l when l.Value is "true" or "false" => "bool", LiteralExpression l when l.Value.StartsWith('\'') => "char", LiteralExpression l when SemanticAnalyzer.IsFloatingLiteral(l.Value) => SemanticAnalyzer.ParseFloating(l.Value).Type, LiteralExpression l => SemanticAnalyzer.ParseInteger(l.Value).Type, NameExpression n when TryLookup(n.Name, out var local) => local.Type, _ => "int32" };
        private static bool IsNumericType(string type) => type is "int" or "uint" or "int8" or "uint8" or "int16" or "uint16" or "int32" or "uint32" or "int64" or "uint64" or "float" or "double" or "char";
        private bool IsBoxableScalarOrEnum(string type) => IsNumericType(type) || type == "bool" || enumTypes.ContainsKey(type);
        private bool IsBoxableStructType(string type) => structTypes.ContainsKey(type);
        private bool IsBoxableValueType(string type) => IsBoxableScalarOrEnum(type) || IsBoxableStructType(type);
        private bool StructContainsManagedReferences(string type) => StructContainsManagedReferences(type, new HashSet<string>(StringComparer.Ordinal));
        private bool StructContainsManagedReferences(string type, HashSet<string> visiting)
        {
            if (IsStringType(type) || GetClassReferenceType(type) is not null || IsManagedArrayType(type)) return true;
            if (type.EndsWith("?", StringComparison.Ordinal))
            {
                var underlying = type[..^1];
                if (IsStringType(underlying) || GetClassReferenceType(underlying) is not null || IsManagedArrayType(underlying)) return true;
                type = underlying;
            }
            if (SemanticAnalyzer.TryParseInlineArrayType(type, out var inlineElementType, out _))
                return StructContainsManagedReferences(inlineElementType, visiting);
            if (!structTypes.ContainsKey(type)) return false;
            if (!visiting.Add(type)) return true;
            var containsReferences = structTypes[type].Fields.Any(field => StructContainsManagedReferences(field.TypeName, visiting));
            visiting.Remove(type);
            return containsReferences;
        }
        private static bool IsObjectType(string type) => type is "System.Object" or "object";
        private string BoxDescriptorPointer(string type) => $"@luma_boxdesc_{Safe(type)}";

        private string EmitBoxValue(string value, string sourceType)
        {
            if (sourceType.EndsWith("?", StringComparison.Ordinal) && IsNullableValueType(sourceType))
            {
                var underlyingType = NullableUnderlyingType(sourceType);
                var aggregateType = LlvmType(sourceType);
                var hasValue = NewTemp(); Emit($"{hasValue} = extractvalue {aggregateType} {value}, 0");
                var presentLabel = NewLabel("box.present"); var absentLabel = NewLabel("box.absent"); var endLabel = NewLabel("box.end");
                Emit($"br i1 {hasValue}, label %{presentLabel}, label %{absentLabel}"); _terminated = true;
                Label(presentLabel);
                var payload = NewTemp(); Emit($"{payload} = extractvalue {aggregateType} {value}, 1");
                var boxed = EmitBoxValue(payload, underlyingType);
                var presentBlock = _block; Branch(endLabel);
                Label(absentLabel); var absentBlock = _block; Branch(endLabel);
                Label(endLabel);
                var result = NewTemp(); Emit($"{result} = phi ptr [ {boxed}, %{presentBlock} ], [ null, %{absentBlock} ]");
                _ownedClassValues.Remove((boxed, "System.Object"));
                _ownedClassValues.Add((result, "System.Object"));
                return result;
            }
            return IsBoxableStructType(sourceType) ? EmitBoxStruct(value, sourceType) : EmitBoxScalar(value, sourceType);
        }

        private string EmitBoxStruct(string value, string sourceType)
        {
            if (!IsBoxableStructType(sourceType))
                throw new NotSupportedException($"Boxing value '{sourceType}' is unsupported.");
            var boxType = $"%luma.structbox.{Safe(sourceType)}";
            var sizeType = nativePointerSize == 8 ? "i64" : "i32";
            var sizeAddress = NewTemp(); Emit($"{sizeAddress} = getelementptr {boxType}, ptr null, i32 1");
            var size = NewTemp(); Emit($"{size} = ptrtoint ptr {sizeAddress} to {sizeType}");
            var box = NewTemp(); Emit($"{box} = call ptr @malloc({sizeType} {size})");
            var allocationFailed = NewTemp(); Emit($"{allocationFailed} = icmp eq ptr {box}, null");
            TrapIf(allocationFailed, "luma_throw_out_of_memory");
            var descriptorAddress = NewTemp(); Emit($"{descriptorAddress} = getelementptr {boxType}, ptr {box}, i32 0, i32 0");
            Emit($"store ptr {BoxDescriptorPointer(sourceType)}, ptr {descriptorAddress}");
            var countAddress = NewTemp(); Emit($"{countAddress} = getelementptr {boxType}, ptr {box}, i32 0, i32 1");
            Emit($"store i32 1, ptr {countAddress}");
            var flagsAddress = NewTemp(); Emit($"{flagsAddress} = getelementptr {boxType}, ptr {box}, i32 0, i32 2");
            Emit($"store i32 0, ptr {flagsAddress}");
            var weakAddress = NewTemp(); Emit($"{weakAddress} = getelementptr {boxType}, ptr {box}, i32 0, i32 3");
            Emit($"store ptr null, ptr {weakAddress}");
            var payloadAddress = NewTemp(); Emit($"{payloadAddress} = getelementptr {boxType}, ptr {box}, i32 0, i32 4");
            if (IsManagedAggregateType(sourceType))
            {
                var ownedValue = (value, sourceType);
                if (!_ownedAggregateValues.Remove(ownedValue)) EmitAggregateOwnership(ownedValue, retain: true);
            }
            Emit($"store {LlvmType(sourceType)} {value}, ptr {payloadAddress}");
            _ownedClassValues.Add((box, "System.Object"));
            return box;
        }

        private string EmitBoxScalar(string value, string sourceType)
        {
            var encoded = value;
            if (sourceType == "bool")
            {
                encoded = NewTemp(); Emit($"{encoded} = zext i1 {value} to i64");
            }
            else if (sourceType == "float")
            {
                var bits = NewTemp(); Emit($"{bits} = bitcast float {value} to i32");
                encoded = NewTemp(); Emit($"{encoded} = zext i32 {bits} to i64");
            }
            else if (sourceType == "double")
            {
                encoded = NewTemp(); Emit($"{encoded} = bitcast double {value} to i64");
            }
            else encoded = CastValue(value, sourceType, "int64");

            var sizeType = nativePointerSize == 8 ? "i64" : "i32";
            var sizeAddress = NewTemp(); Emit($"{sizeAddress} = getelementptr %luma.box, ptr null, i32 1");
            var size = NewTemp(); Emit($"{size} = ptrtoint ptr {sizeAddress} to {sizeType}");
            var box = NewTemp(); Emit($"{box} = call ptr @malloc({sizeType} {size})");
            var allocationFailed = NewTemp(); Emit($"{allocationFailed} = icmp eq ptr {box}, null");
            TrapIf(allocationFailed, "luma_throw_out_of_memory");
            var descriptorAddress = NewTemp(); Emit($"{descriptorAddress} = getelementptr %luma.box, ptr {box}, i32 0, i32 0");
            Emit($"store ptr {BoxDescriptorPointer(sourceType)}, ptr {descriptorAddress}");
            var countAddress = NewTemp(); Emit($"{countAddress} = getelementptr %luma.box, ptr {box}, i32 0, i32 1");
            Emit($"store i32 1, ptr {countAddress}");
            var flagsAddress = NewTemp(); Emit($"{flagsAddress} = getelementptr %luma.box, ptr {box}, i32 0, i32 2");
            Emit($"store i32 0, ptr {flagsAddress}");
            var weakAddress = NewTemp(); Emit($"{weakAddress} = getelementptr %luma.box, ptr {box}, i32 0, i32 3");
            Emit($"store ptr null, ptr {weakAddress}");
            var payloadAddress = NewTemp(); Emit($"{payloadAddress} = getelementptr %luma.box, ptr {box}, i32 0, i32 4");
            Emit($"store i64 {encoded}, ptr {payloadAddress}");
            _ownedClassValues.Add((box, "System.Object"));
            return box;
        }

        private string DecodeBoxPayload(string payload, string targetType)
        {
            if (targetType == "bool")
            {
                var result = NewTemp(); Emit($"{result} = trunc i64 {payload} to i1"); return result;
            }
            if (targetType == "float")
            {
                var bits = NewTemp(); Emit($"{bits} = trunc i64 {payload} to i32");
                var result = NewTemp(); Emit($"{result} = bitcast i32 {bits} to float"); return result;
            }
            if (targetType == "double")
            {
                var result = NewTemp(); Emit($"{result} = bitcast i64 {payload} to double"); return result;
            }
            return CastValue(payload, "uint64", targetType);
        }

        private string LoadBoxedStructValue(string objectValue, string structType)
        {
            if (!IsBoxableStructType(structType))
                throw new NotSupportedException($"Unboxing struct '{structType}' with managed references is unsupported.");
            var boxType = $"%luma.structbox.{Safe(structType)}";
            var payloadAddress = NewTemp(); Emit($"{payloadAddress} = getelementptr {boxType}, ptr {objectValue}, i32 0, i32 4");
            var payload = NewTemp(); Emit($"{payload} = load {LlvmType(structType)}, ptr {payloadAddress}");
            return payload;
        }

        private string EmitUnboxValue(string value, string sourceType, string targetType)
        {
            var nullableTarget = IsNullableValueType(targetType);
            var underlyingType = nullableTarget ? NullableUnderlyingType(targetType) : targetType;
            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {value}, null");
            if (!nullableTarget) TrapIf(isNull, "luma_throw_null");
            var checkLabel = nullableTarget ? NewLabel("unbox.check") : null;
            var nullLabel = nullableTarget ? NewLabel("unbox.null") : null;
            var endLabel = nullableTarget ? NewLabel("unbox.end") : null;
            if (nullableTarget)
            {
                Emit($"br i1 {isNull}, label %{nullLabel}, label %{checkLabel}"); _terminated = true;
                Label(checkLabel!);
            }
            var descriptor = NewTemp(); Emit($"{descriptor} = load ptr, ptr {value}");
            var exact = NewTemp(); Emit($"{exact} = icmp eq ptr {descriptor}, {BoxDescriptorPointer(underlyingType)}");
            var mismatch = NewTemp(); Emit($"{mismatch} = xor i1 {exact}, true");
            TrapIf(mismatch, "luma_throw_invalid_cast");
            var payloadAddress = NewTemp(); Emit($"{payloadAddress} = getelementptr %luma.box, ptr {value}, i32 0, i32 4");
            var payload = NewTemp(); Emit($"{payload} = load i64, ptr {payloadAddress}");
            var unboxed = DecodeBoxPayload(payload, underlyingType);
            string result;
            if (nullableTarget)
            {
                var wrapped = WrapNullableValue(unboxed, underlyingType, targetType);
                var successBlock = _block; Branch(endLabel!);
                Label(nullLabel!); var nullBlock = _block; Branch(endLabel!);
                Label(endLabel!);
                result = NewTemp(); Emit($"{result} = phi {LlvmType(targetType)} [ {wrapped}, %{successBlock} ], [ zeroinitializer, %{nullBlock} ]");
            }
            else result = unboxed;
            if (IsManagedArrayType(sourceType)) ReleaseOwnedArray((value, sourceType));
            else if (IsStringType(sourceType)) ReleaseOwnedString((value, sourceType));
            else ReleaseOwnedClass((value, sourceType));
            return result;
        }

        private string EmitUnboxStruct(string value, string sourceType, string targetType)
        {
            var nullableTarget = IsNullableValueType(targetType);
            var underlyingType = nullableTarget ? NullableUnderlyingType(targetType) : targetType;
            if (!IsBoxableStructType(underlyingType))
                throw new NotSupportedException($"Unboxing struct '{underlyingType}' with managed references is unsupported.");
            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {value}, null");
            if (!nullableTarget) TrapIf(isNull, "luma_throw_null");
            var checkLabel = nullableTarget ? NewLabel("unbox.struct.check") : null;
            var nullLabel = nullableTarget ? NewLabel("unbox.struct.null") : null;
            var endLabel = nullableTarget ? NewLabel("unbox.struct.end") : null;
            if (nullableTarget)
            {
                Emit($"br i1 {isNull}, label %{nullLabel}, label %{checkLabel}"); _terminated = true;
                Label(checkLabel!);
            }
            var descriptor = NewTemp(); Emit($"{descriptor} = load ptr, ptr {value}");
            var exact = NewTemp(); Emit($"{exact} = icmp eq ptr {descriptor}, {BoxDescriptorPointer(underlyingType)}");
            var mismatch = NewTemp(); Emit($"{mismatch} = xor i1 {exact}, true");
            TrapIf(mismatch, "luma_throw_invalid_cast");
            var unboxed = LoadBoxedStructValue(value, underlyingType);
            if (IsManagedAggregateType(underlyingType)) EmitAggregateOwnership((unboxed, underlyingType), retain: true);
            string result;
            if (nullableTarget)
            {
                var wrapped = WrapNullableValue(unboxed, underlyingType, targetType);
                var successBlock = _block; Branch(endLabel!);
                Label(nullLabel!); var nullBlock = _block; Branch(endLabel!);
                Label(endLabel!);
                result = NewTemp(); Emit($"{result} = phi {LlvmType(targetType)} [ {wrapped}, %{successBlock} ], [ zeroinitializer, %{nullBlock} ]");
            }
            else result = unboxed;
            if (IsManagedAggregateType(targetType)) _ownedAggregateValues.Add((result, targetType));
            if (IsManagedArrayType(sourceType)) ReleaseOwnedArray((value, sourceType));
            else if (IsStringType(sourceType)) ReleaseOwnedString((value, sourceType));
            else ReleaseOwnedClass((value, sourceType));
            return result;
        }

        private string CastValue(string value, string fromType, string toType, bool checkedConversion = false)
        {
            if (fromType == toType) return value;
            if (userConversionOperators.TryGetValue((fromType, toType), out var userDefinedConversion))
            {
                var location = method.Declaration.Location;
                var call = new CallExpression(new NameExpression(userDefinedConversion.Declaration.Name, location),
                    [new NameExpression("$conversionSource", location)], location);
                return EmitCall(call, sourceArgumentOverrides: [(value, fromType)], explicitTarget: userDefinedConversion).Value;
            }
            if (fromType == "null" && IsNullableStringType(toType)) return "null";
            if (fromType == "null" && IsNullableArrayType(toType)) return "null";
            if (fromType == "null" && GetClassReferenceType(toType) is not null) return "null";
            if ((IsObjectType(toType) || GetClassReferenceType(toType) == "System.Object") &&
                (IsBoxableValueType(fromType) || IsNullableValueType(fromType) && IsBoxableValueType(NullableUnderlyingType(fromType))))
                return EmitBoxValue(value, fromType);
            var interfaceTargetType = GetClassReferenceType(toType);
            if (interfaceTargetType is not null && interfaceTypes.ContainsKey(interfaceTargetType) &&
                (IsBoxableStructType(fromType) || IsNullableValueType(fromType) && IsBoxableStructType(NullableUnderlyingType(fromType))) &&
                interfaceConversions.Contains((IsNullableValueType(fromType) ? NullableUnderlyingType(fromType) : fromType, interfaceTargetType)))
                return EmitBoxValue(value, fromType);
            if (IsStringType(fromType) && (IsObjectType(toType) || GetClassReferenceType(toType) == "System.Object"))
            {
                if (IsNullableStringType(fromType) && !toType.EndsWith("?", StringComparison.Ordinal))
                {
                    var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {value}, null");
                    TrapIf(isNull, "luma_throw_null");
                }
                return value;
            }
            if (IsStringType(toType) && IsRuntimeObjectReference(fromType))
                return EmitCheckedStringCast(value, fromType, toType);
            if (IsRuntimeObjectReference(fromType) && IsBoxableStructType(toType))
                return EmitUnboxStruct(value, fromType, toType);
            if (IsRuntimeObjectReference(fromType) && IsNullableValueType(toType) && IsBoxableStructType(NullableUnderlyingType(toType)))
                return EmitUnboxStruct(value, fromType, toType);
            if (IsRuntimeObjectReference(fromType) && (IsBoxableScalarOrEnum(toType) || IsNullableValueType(toType) && IsBoxableScalarOrEnum(NullableUnderlyingType(toType))))
                return EmitUnboxValue(value, fromType, toType);
            var sourceClassType = GetClassReferenceType(fromType) ?? (IsStringType(fromType) ? "System.Object" : null);
            var targetClassType = GetClassReferenceType(toType);
            if (sourceClassType is not null && targetClassType is not null)
            {
                var staticallyAssignable = sourceClassType == targetClassType || IsClassAssignable(sourceClassType, targetClassType) ||
                    interfaceConversions.Contains((sourceClassType, targetClassType));
                if (staticallyAssignable)
                {
                    if (fromType.EndsWith("?", StringComparison.Ordinal) && !toType.EndsWith("?", StringComparison.Ordinal))
                    {
                        var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {value}, null");
                        TrapIf(isNull, "luma_throw_null");
                    }
                    return value;
                }
                if (interfaceTypes.ContainsKey(targetClassType))
                {
                    var isNullableTarget = toType.EndsWith("?", StringComparison.Ordinal);
                    if (fromType.EndsWith("?", StringComparison.Ordinal) && !isNullableTarget)
                    {
                        var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {value}, null");
                        TrapIf(isNull, "luma_throw_null");
                    }
                    var matches = NewTemp();
                    Emit($"{matches} = call i1 @luma_class_implements(ptr {value}, ptr @luma_interfacedesc_{Safe(targetClassType)})");
                    var notMatches = NewTemp(); Emit($"{notMatches} = xor i1 {matches}, true");
                    string invalid = notMatches;
                    if (isNullableTarget)
                    {
                        var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {value}, null");
                        var notNull = NewTemp(); Emit($"{notNull} = xor i1 {isNull}, true");
                        invalid = NewTemp(); Emit($"{invalid} = and i1 {notMatches}, {notNull}");
                    }
                    TrapIf(invalid, "luma_throw_invalid_cast");
                    return value;
                }
                if (classTypes.ContainsKey(targetClassType) || delegateTypes.ContainsKey(targetClassType))
                {
                    var isNullableTarget = toType.EndsWith("?", StringComparison.Ordinal);
                    if (fromType.EndsWith("?", StringComparison.Ordinal) && !isNullableTarget)
                    {
                        var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {value}, null");
                        TrapIf(isNull, "luma_throw_null");
                    }
                    var matches = NewTemp();
                    Emit($"{matches} = call i1 @luma_class_is_instance(ptr {value}, ptr {ClassDescriptorPointer(targetClassType)})");
                    var notMatches = NewTemp(); Emit($"{notMatches} = xor i1 {matches}, true");
                    string invalid = notMatches;
                    if (isNullableTarget)
                    {
                        var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {value}, null");
                        var notNull = NewTemp(); Emit($"{notNull} = xor i1 {isNull}, true");
                        invalid = NewTemp();
                        Emit($"{invalid} = and i1 {notMatches}, {notNull}");
                    }
                    TrapIf(invalid, "luma_throw_invalid_cast");
                    return value;
                }
            }
            if (IsManagedArrayType(fromType) && IsManagedArrayType(toType))
            {
                if (IsNullableArrayType(fromType) && !IsNullableArrayType(toType))
                {
                    var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {value}, null");
                    TrapIf(isNull, "luma_throw_null");
                }
                return value;
            }
            if (IsStringType(fromType) && IsStringType(toType))
            {
                if (IsNullableStringType(fromType) && toType == "string")
                {
                    var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {value}, null");
                    TrapIf(isNull, "luma_throw_null");
                }
                return value;
            }
            if (IsNullableValueType(toType))
            {
                var underlying = toType[..^1];
                if (fromType == "null") return "zeroinitializer";
                if (IsNullableValueType(fromType)) return ConvertNullableValue(value, fromType, toType);
                if (IsNumericType(fromType) || fromType == "bool" || enumTypes.ContainsKey(fromType) || structTypes.ContainsKey(fromType))
                {
                    var converted = CastValue(value, fromType, underlying, checkedConversion);
                    return WrapNullableValue(converted, underlying, toType);
                }
                throw new NotSupportedException($"Conversion from '{fromType}' to nullable value type '{toType}' was not validated.");
            }
            if (IsNullableValueType(fromType) && (IsNumericType(toType) || toType == "bool" || enumTypes.ContainsKey(toType) || structTypes.ContainsKey(toType)))
            {
                var nullable = LlvmType(fromType);
                var hasValue = NewTemp(); Emit($"{hasValue} = extractvalue {nullable} {value}, 0");
                var noValue = NewTemp(); Emit($"{noValue} = xor i1 {hasValue}, true"); TrapIf(noValue, "luma_throw_null");
                var unwrapped = NewTemp(); Emit($"{unwrapped} = extractvalue {nullable} {value}, 1");
                return CastValue(unwrapped, NullableUnderlyingType(fromType), toType, checkedConversion);
            }
            if (fromType is "?" or "null" || toType == "?") return value;
            if (IsPointerType(fromType) && IsPointerType(toType)) return value;
            if (IsPointerType(fromType) && IsNumericType(toType))
            {
                var convertedPointer = NewTemp(); Emit($"{convertedPointer} = ptrtoint ptr {value} to {LlvmType(toType)}"); return convertedPointer;
            }
            if (IsNumericType(fromType) && IsPointerType(toType))
            {
                var nativeWidth = nativePointerSize * 8; var pointerSourceWidth = IntegerWidth(fromType); var nativeType = nativePointerSize == 8 ? "int64" : "int32";
                var addressBits = value;
                if (pointerSourceWidth != nativeWidth)
                {
                    var pointerConversion = pointerSourceWidth > nativeWidth ? "trunc" : "zext";
                    addressBits = NewTemp(); Emit($"{addressBits} = {pointerConversion} {LlvmType(fromType)} {value} to {LlvmType(nativeType)}");
                }
                var convertedInteger = NewTemp(); Emit($"{convertedInteger} = inttoptr {LlvmType(nativeType)} {addressBits} to ptr"); return convertedInteger;
            }
            fromType = RepresentationType(fromType); toType = RepresentationType(toType);
            if (fromType == toType) return value;
            var fromFloating = fromType is "float" or "double"; var toFloating = toType is "float" or "double";
            if (fromFloating && toFloating)
            {
                if (fromType == "float" && toType == "double") { var widened = NewTemp(); Emit($"{widened} = fpext float {value} to double"); return widened; }
                if (fromType == "double" && toType == "float") { var narrowed = NewTemp(); Emit($"{narrowed} = fptrunc double {value} to float"); return narrowed; }
                throw new NotSupportedException($"Floating conversion from '{fromType}' to '{toType}' is unsupported.");
            }
            if (toFloating && !fromFloating)
            {
                var converted = NewTemp(); var operation = IsUnsigned(fromType) ? "uitofp" : "sitofp"; Emit($"{converted} = {operation} {LlvmType(fromType)} {value} to {LlvmType(toType)}"); return converted;
            }
            if (fromFloating && !toFloating)
            {
                var sourceLlvm = LlvmType(fromType); var width = IntegerWidth(toType); var signed = !IsUnsigned(toType);
                var truncated = NewTemp(); Emit($"{truncated} = call {sourceLlvm} @llvm.trunc.{(fromType == "float" ? "f32" : "f64")}({sourceLlvm} {value})");
                var lower = signed ? -Math.Pow(2.0, width - 1) : 0.0;
                var upper = Math.Pow(2.0, signed ? width - 1 : width);
                var lowerBound = FormatFloating(fromType, lower); var upperBound = FormatFloating(fromType, upper);
                var aboveLower = NewTemp(); Emit($"{aboveLower} = fcmp oge {sourceLlvm} {truncated}, {lowerBound}");
                var belowUpper = NewTemp(); Emit($"{belowUpper} = fcmp olt {sourceLlvm} {truncated}, {upperBound}");
                var valid = NewTemp(); Emit($"{valid} = and i1 {aboveLower}, {belowUpper}");
                if (checkedConversion) { var invalid = NewTemp(); Emit($"{invalid} = xor i1 {valid}, true"); TrapIf(invalid, "luma_throw_overflow"); }
                var safeValue = NewTemp(); Emit($"{safeValue} = select i1 {valid}, {sourceLlvm} {truncated}, {sourceLlvm} 0.0");
                var floatResult = NewTemp(); Emit($"{floatResult} = {(signed ? "fptosi" : "fptoui")} {sourceLlvm} {safeValue} to {LlvmType(toType)}"); return floatResult;
            }
            if (fromFloating || toFloating) throw new NotSupportedException($"Numeric conversion from '{fromType}' to '{toType}' is unsupported.");
            var sourceWidth = IntegerWidth(fromType); var targetWidth = IntegerWidth(toType);
            if (checkedConversion && (sourceWidth > targetWidth || IsUnsigned(fromType) != IsUnsigned(toType)))
            {
                var wideValue = NewTemp(); Emit($"{wideValue} = {(IsUnsigned(fromType) ? "zext" : "sext")} {LlvmType(fromType)} {value} to i128");
                var lower = IsUnsigned(toType) ? BigInteger.Zero : -(BigInteger.One << (targetWidth - 1));
                var upper = IsUnsigned(toType) ? (BigInteger.One << targetWidth) - 1 : (BigInteger.One << (targetWidth - 1)) - 1;
                var atLeastLower = NewTemp(); Emit($"{atLeastLower} = icmp sge i128 {wideValue}, {lower}");
                var atMostUpper = NewTemp(); Emit($"{atMostUpper} = icmp sle i128 {wideValue}, {upper}");
                var fits = NewTemp(); Emit($"{fits} = and i1 {atLeastLower}, {atMostUpper}"); var overflows = NewTemp(); Emit($"{overflows} = xor i1 {fits}, true"); TrapIf(overflows, "luma_throw_overflow");
            }
            if (sourceWidth == targetWidth) return value;
            var result = NewTemp(); var conversion = sourceWidth > targetWidth ? "trunc" : IsUnsigned(fromType) ? "zext" : "sext";
            Emit($"{result} = {conversion} {LlvmType(fromType)} {value} to {LlvmType(toType)}");
            return result;
        }
        private string EmitCheckedStringCast(string value, string sourceType, string targetType)
        {
            var isNullableTarget = IsNullableStringType(targetType);
            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {value}, null");
            if (!isNullableTarget) TrapIf(isNull, "luma_throw_null");
            var matches = NewTemp(); Emit($"{matches} = call i1 @luma_class_is_instance(ptr {value}, ptr @luma_string_type)");
            var notMatches = NewTemp(); Emit($"{notMatches} = xor i1 {matches}, true");
            string invalid = notMatches;
            if (isNullableTarget)
            {
                var notNull = NewTemp(); Emit($"{notNull} = xor i1 {isNull}, true");
                invalid = NewTemp(); Emit($"{invalid} = and i1 {notNull}, {notMatches}");
            }
            TrapIf(invalid, "luma_throw_invalid_cast");
            EnsureOwnedRuntimeReference((value, sourceType));
            TransferRuntimeReferenceToString(value, sourceType);
            return value;
        }
        private void EnsureOwnedRuntimeReference((string Value, string Type) value)
        {
            if (IsManagedArrayType(value.Type)) EnsureOwnedArray(value);
            else if (IsStringType(value.Type)) EnsureOwnedString(value);
            else if (GetClassReferenceType(value.Type) is { } classType) EnsureOwnedClass(value, classType);
            else throw new NotSupportedException($"'{value.Type}' is not a managed object reference.");
        }
        private void TransferRuntimeReferenceToString(string value, string sourceType)
        {
            if (IsManagedArrayType(sourceType)) _ownedArrayValues.Remove(value);
            else if (IsStringType(sourceType)) _ownedStringValues.Remove(value);
            else if (GetClassReferenceType(sourceType) is { } classType) _ownedClassValues.Remove((value, classType));
            _ownedStringValues.Add(value);
        }
        private bool IsUnsigned(string type) => RepresentationType(type).StartsWith('u') || RepresentationType(type) == "char";
        private static bool IsPointerType(string type) => type.EndsWith('*');
        private string LlvmType(string type) => LlvmEmitter.LlvmType(type, enumTypes, nativePointerSize,
            classTypes.Keys.Concat(interfaceTypes.Keys).Concat(delegateTypes.Keys).ToHashSet(StringComparer.Ordinal));
        private string? GetClassReferenceType(string type)
        {
            if (classTypes.ContainsKey(type)) return type;
            if (interfaceTypes.ContainsKey(type)) return type;
            if (delegateTypes.ContainsKey(type)) return type;
            if (type.EndsWith("?", StringComparison.Ordinal) && classTypes.ContainsKey(type[..^1])) return type[..^1];
            if (type.EndsWith("?", StringComparison.Ordinal) && interfaceTypes.ContainsKey(type[..^1])) return type[..^1];
            if (type.EndsWith("?", StringComparison.Ordinal) && delegateTypes.ContainsKey(type[..^1])) return type[..^1];
            return null;
        }
        private bool IsRuntimeObjectReference(string type) =>
            GetClassReferenceType(type) is not null || IsManagedArrayType(type) || IsStringType(type);
        private bool IsClassAssignable(string fromType, string toType)
        {
            if (delegateTypes.ContainsKey(fromType) && toType is ("System.Delegate" or "System.Object")) return true;
            if (fromType == "System.Delegate" && toType == "System.Object") return true;
            var visited = new HashSet<string>(StringComparer.Ordinal);
            var current = fromType;
            while (classTypes.TryGetValue(current, out var symbol) && visited.Add(current))
            {
                if (current == toType) return true;
                if (symbol.BaseTypeName is null) return false;
                current = symbol.BaseTypeName;
            }
            return false;
        }
        private string ClassDescriptorPointer(string type) => delegateTypes.ContainsKey(type)
            ? $"@luma_classdesc_delegate_{Safe(type)}" : $"@luma_classdesc_{Safe(type)}";
        private static bool IsWeakRuntimeHelper(MethodSymbol method) => method.Owner.Namespace == "System" &&
            method.Owner.Name.StartsWith("Weak", StringComparison.Ordinal) &&
            method.Declaration.Name is "CreateControlRuntime" or "IsAliveRuntime" or "GetTargetRuntime" or "ReleaseControlRuntime";

        private static bool IsStringType(string type) => type is "string" or "string?";
        private static bool IsNullableStringType(string type) => type == "string?";
        private bool IsDelegateType(string type) => delegateTypes.ContainsKey(type.EndsWith("?", StringComparison.Ordinal) ? type[..^1] : type);
        private bool IsNullableDelegateType(string type) => type.EndsWith("?", StringComparison.Ordinal) && delegateTypes.ContainsKey(type[..^1]);
        private bool IsNullableValueType(string type)
        {
            if (!type.EndsWith("?", StringComparison.Ordinal)) return false;
            var underlying = type[..^1];
            return IsNumericType(underlying) || underlying == "bool" || enumTypes.ContainsKey(underlying) || structTypes.ContainsKey(underlying);
        }
        private bool IsNullableValuePayloadType(string type) => IsNumericType(type) || type == "bool" || enumTypes.ContainsKey(type) || structTypes.ContainsKey(type);
        private static string NullableUnderlyingType(string type) => type.EndsWith("?", StringComparison.Ordinal) ? type[..^1] : type;
        private string WrapNullableValue(string value, string underlying, string nullableType)
        {
            var aggregateType = LlvmType(nullableType);
            var tagged = NewTemp(); Emit($"{tagged} = insertvalue {aggregateType} zeroinitializer, i1 true, 0");
            var result = NewTemp(); Emit($"{result} = insertvalue {aggregateType} {tagged}, {LlvmType(underlying)} {value}, 1");
            return result;
        }
        private string ConvertNullableValue(string value, string fromType, string toType)
        {
            if (!IsNullableValueType(fromType) || !IsNullableValueType(toType)) throw new NotSupportedException("Nullable value conversion requires nullable value types.");
            var sourceUnderlying = NullableUnderlyingType(fromType); var targetUnderlying = NullableUnderlyingType(toType);
            var sourceLlvm = LlvmType(fromType); var targetLlvm = LlvmType(toType);
            var hasValue = NewTemp(); Emit($"{hasValue} = extractvalue {sourceLlvm} {value}, 0");
            var payload = NewTemp(); Emit($"{payload} = extractvalue {sourceLlvm} {value}, 1");
            var convertedPayload = CastValue(payload, sourceUnderlying, targetUnderlying);
            var tagged = NewTemp(); Emit($"{tagged} = insertvalue {targetLlvm} zeroinitializer, i1 {hasValue}, 0");
            var result = NewTemp(); Emit($"{result} = insertvalue {targetLlvm} {tagged}, {LlvmType(targetUnderlying)} {convertedPayload}, 1");
            return result;
        }
        private (string Value, string Type) EmitNullableValueAssertion((string Value, string Type) nullable)
        {
            var underlying = NullableUnderlyingType(nullable.Type);
            var aggregateType = LlvmType(nullable.Type);
            var hasValue = NewTemp(); Emit($"{hasValue} = extractvalue {aggregateType} {nullable.Value}, 0");
            var noValue = NewTemp(); Emit($"{noValue} = xor i1 {hasValue}, true"); TrapIf(noValue, "luma_throw_null");
            var value = NewTemp(); Emit($"{value} = extractvalue {aggregateType} {nullable.Value}, 1");
            return (value, underlying);
        }
        private string DefaultStorageValue(string type) => type == "string" ? "@luma_string_empty" : IsNullableStringType(type) || type.EndsWith('*') || IsDelegateType(type) || GetClassReferenceType(type) is not null || SemanticAnalyzer.TryParseManagedArrayType(type, out _, out _) ? "null" : "zeroinitializer";
        private (string Value, string Type) EmitStringLength(MemberAccessExpression access)
        {
            var receiver = EmitExpression(access.Receiver);
            var length = NewTemp();
            Emit($"{length} = call i32 @luma_string_length(ptr {receiver.Value})");
            ReleaseOwnedString(receiver);
            return (length, "int32");
        }
        private (string Value, string Type) EmitNullConditionalStringLength(MemberAccessExpression access)
        {
            var receiver = EmitExpression(access.Receiver);
            var releaseReceiver = _ownedStringValues.Remove(receiver.Value);
            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {receiver.Value}, null");
            var nullLabel = NewLabel("null.string.length.null"); var valueLabel = NewLabel("null.string.length.value"); var endLabel = NewLabel("null.string.length.end");
            Emit($"br i1 {isNull}, label %{nullLabel}, label %{valueLabel}"); _terminated = true;

            Label(nullLabel);
            if (releaseReceiver) Emit($"call void @luma_string_release(ptr {receiver.Value})");
            var nullBlock = _block; Branch(endLabel);

            Label(valueLabel);
            var length = NewTemp(); Emit($"{length} = call i32 @luma_string_length(ptr {receiver.Value})");
            var wrapped = WrapNullableValue(length, "int32", "int32?");
            if (releaseReceiver) Emit($"call void @luma_string_release(ptr {receiver.Value})");
            var valueBlock = _block; Branch(endLabel);

            Label(endLabel);
            var result = NewTemp(); Emit($"{result} = phi {LlvmType("int32?")} [ zeroinitializer, %{nullBlock} ], [ {wrapped}, %{valueBlock} ]");
            return (result, "int32?");
        }
        private (string Value, string Type) EmitStringIndex(IndexExpression index)
        {
            var receiver = EmitExpression(index.Receiver);
            if (IsNullableStringType(receiver.Type))
            {
                var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {receiver.Value}, null");
                TrapIf(isNull, "luma_throw_null");
            }
            var sourceIndex = EmitExpression(index.Indices[0]);
            var convertedIndex = CastValue(sourceIndex.Value, sourceIndex.Type, "int32");
            var length = NewTemp(); Emit($"{length} = call i32 @luma_string_length(ptr {receiver.Value})");
            var nonnegative = NewTemp(); Emit($"{nonnegative} = icmp sge i32 {convertedIndex}, 0");
            var belowLength = NewTemp(); Emit($"{belowLength} = icmp slt i32 {convertedIndex}, {length}");
            var valid = NewTemp(); Emit($"{valid} = and i1 {nonnegative}, {belowLength}");
            var invalid = NewTemp(); Emit($"{invalid} = xor i1 {valid}, true");
            TrapIf(invalid, "luma_throw_bounds");
            var codeUnit = NewTemp();
            Emit($"{codeUnit} = call i16 @luma_string_char(ptr {receiver.Value}, i32 {convertedIndex})");
            ReleaseOwnedString(receiver);
            return (codeUnit, "char");
        }
        private (string Value, string Type) EmitNullConditionalStringIndex(IndexExpression index)
        {
            if (index.Indices.Count != 1) throw new NotSupportedException("Null-conditional string indexing requires exactly one index.");
            var receiver = EmitExpression(index.Receiver);
            var releaseReceiver = _ownedStringValues.Contains(receiver.Value);
            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {receiver.Value}, null");
            var nullLabel = NewLabel("null.string.index.null"); var valueLabel = NewLabel("null.string.index.value"); var endLabel = NewLabel("null.string.index.end");
            Emit($"br i1 {isNull}, label %{nullLabel}, label %{valueLabel}"); _terminated = true;

            Label(nullLabel);
            if (releaseReceiver) Emit($"call void @luma_string_release(ptr {receiver.Value})");
            var nullBlock = _block; Branch(endLabel);

            Label(valueLabel);
            var sourceIndex = EmitExpression(index.Indices[0]);
            var convertedIndex = CastValue(sourceIndex.Value, sourceIndex.Type, "int32");
            var length = NewTemp(); Emit($"{length} = call i32 @luma_string_length(ptr {receiver.Value})");
            var nonnegative = NewTemp(); Emit($"{nonnegative} = icmp sge i32 {convertedIndex}, 0");
            var belowLength = NewTemp(); Emit($"{belowLength} = icmp slt i32 {convertedIndex}, {length}");
            var valid = NewTemp(); Emit($"{valid} = and i1 {nonnegative}, {belowLength}");
            var invalid = NewTemp(); Emit($"{invalid} = xor i1 {valid}, true");
            TrapIf(invalid, "luma_throw_bounds");
            var codeUnit = NewTemp(); Emit($"{codeUnit} = call i16 @luma_string_char(ptr {receiver.Value}, i32 {convertedIndex})");
            var wrapped = WrapNullableValue(codeUnit, "char", "char?");
            ReleaseOwnedString(receiver);
            var valueBlock = _block; Branch(endLabel);

            Label(endLabel);
            var result = NewTemp(); Emit($"{result} = phi {LlvmType("char?")} [ zeroinitializer, %{nullBlock} ], [ {wrapped}, %{valueBlock} ]");
            return (result, "char?");
        }
        private (string Value, string Type) NormalizeString((string Value, string Type) value)
        {
            if (IsNullableStringType(value.Type)) return value;
            if (value.Type != "string") return value;
            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {value.Value}, null");
            var normalized = NewTemp(); Emit($"{normalized} = select i1 {isNull}, ptr @luma_string_empty, ptr {value.Value}");
            return (normalized, "string");
        }
        private static string QualifiedTypeName(TypeDeclaration owner) => string.IsNullOrEmpty(owner.Namespace) ? owner.Name : owner.Namespace + "." + owner.Name;
        private void Push() { _scopes.Add(new(StringComparer.Ordinal)); _scopeStringPointers.Add([]); _scopeArrayPointers.Add([]); _scopeClassPointers.Add([]); _scopeAggregatePointers.Add([]); _scopeAllocations.Add([]); }
        private void Pop()
        {
            if (_scopes.Count <= 1) return;
            if (!_terminated) { ReleaseScopeStrings(_scopes.Count - 1); ReleaseScopeArrays(_scopes.Count - 1); ReleaseScopeClasses(_scopes.Count - 1); ReleaseScopeAggregates(_scopes.Count - 1); ReleaseScopeAllocations(_scopes.Count - 1); }
            _scopes.RemoveAt(_scopes.Count - 1); _scopeStringPointers.RemoveAt(_scopeStringPointers.Count - 1); _scopeArrayPointers.RemoveAt(_scopeArrayPointers.Count - 1); _scopeClassPointers.RemoveAt(_scopeClassPointers.Count - 1); _scopeAggregatePointers.RemoveAt(_scopeAggregatePointers.Count - 1); _scopeAllocations.RemoveAt(_scopeAllocations.Count - 1);
        }

        private void StoreStringInitial(string pointer, (string Value, string Type) value)
        {
            EnsureOwnedString(value);
            Emit($"store ptr {value.Value}, ptr {pointer}");
            _ownedStringValues.Remove(value.Value);
        }

        private (string Value, string Type) StoreStringReplacing(string pointer, (string Value, string Type) value)
        {
            var oldValue = ReadStorage("string", pointer);
            EnsureOwnedString(value);
            Emit($"store ptr {value.Value}, ptr {pointer}");
            _ownedStringValues.Remove(value.Value);
            Emit($"call void @luma_string_release(ptr {oldValue})");
            return value;
        }

        private (string Value, string Type) StoreStringExpressionResult(string pointer, (string Value, string Type) value, string resultType = "string")
        {
            StoreStringReplacing(pointer, value);
            Emit($"call void @luma_string_retain(ptr {value.Value})");
            _ownedStringValues.Add(value.Value);
            return (value.Value, resultType);
        }

        private void StoreArrayInitial(string pointer, (string Value, string Type) value, string? targetType = null)
        {
            var storageType = targetType ?? value.Type;
            var converted = (CastValue(value.Value, value.Type, storageType), storageType);
            EnsureOwnedArray(converted);
            Emit($"store ptr {converted.Item1}, ptr {pointer}");
            _ownedArrayValues.Remove(converted.Item1);
        }

        private (string Value, string Type) StoreArrayReplacing(string pointer, (string Value, string Type) value, string? targetType = null)
        {
            var storageType = targetType ?? value.Type;
            var oldValue = ReadStorage(storageType, pointer);
            var converted = (CastValue(value.Value, value.Type, storageType), storageType);
            EnsureOwnedArray(converted);
            Emit($"store ptr {converted.Item1}, ptr {pointer}");
            _ownedArrayValues.Remove(converted.Item1);
            Emit($"call void @luma_array_release(ptr {oldValue})");
            return converted;
        }

        private (string Value, string Type) StoreArrayExpressionResult(string pointer, (string Value, string Type) value, string targetType)
        {
            var stored = StoreArrayReplacing(pointer, value, targetType);
            Emit($"call void @luma_array_retain(ptr {stored.Value})");
            _ownedArrayValues.Add(stored.Value);
            return stored;
        }

        private (string Value, string Type) StoreScopeBoundArrayAlias(string pointer, (string Value, string Type) value, string targetType, bool replaceExisting = false)
        {
            var stored = (value.Value, targetType);
            var oldValue = replaceExisting ? ReadStorage(targetType, pointer) : null;
            var hasOwnership = _ownedArrayValues.Remove(stored.Value);
            if (stored.Value != "null" && !hasOwnership) Emit($"call void @luma_array_retain(ptr {stored.Value})");
            Emit($"store ptr {stored.Value}, ptr {pointer}");
            if (oldValue is not null) Emit($"call void @luma_array_release(ptr {oldValue})");
            if (stored.Value != "null") _scopeBoundArrayValues.Add(stored.Value);
            return stored;
        }

        private (string Value, string Type) StoreScopeBoundStringAlias(string pointer, (string Value, string Type) value, string targetType, bool replaceExisting = false)
        {
            var stored = (CastValue(value.Value, value.Type, targetType), targetType);
            var oldValue = replaceExisting ? ReadStorage(targetType, pointer) : null;
            var hasOwnership = _ownedStringValues.Remove(stored.Item1);
            if (stored.Item1 != "null" && !hasOwnership) Emit($"call void @luma_string_retain(ptr {stored.Item1})");
            Emit($"store ptr {stored.Item1}, ptr {pointer}");
            if (oldValue is not null) Emit($"call void @luma_string_release(ptr {oldValue})");
            return stored;
        }

        private void StoreClassInitial(string pointer, (string Value, string Type) value, string classType)
        {
            var converted = GetClassReferenceType(value.Type) is not null ? value.Value : CastValue(value.Value, value.Type, classType);
            var stored = (converted, classType);
            EnsureOwnedClass(stored, classType);
            Emit($"store ptr {stored.converted}, ptr {pointer}");
            _ownedClassValues.Remove((stored.converted, classType));
        }

        private (string Value, string Type) StoreClassReplacing(string pointer, (string Value, string Type) value, string classType)
        {
            var oldValue = ReadStorage(classType, pointer);
            var converted = GetClassReferenceType(value.Type) is not null ? value.Value : CastValue(value.Value, value.Type, classType);
            var stored = (converted, classType);
            EnsureOwnedClass(stored, classType);
            Emit($"store ptr {stored.converted}, ptr {pointer}");
            _ownedClassValues.Remove((stored.converted, classType));
            Emit($"call void @luma_class_release_dynamic(ptr {oldValue})");
            return stored;
        }

        private (string Value, string Type) StoreClassExpressionResult(string pointer, (string Value, string Type) value, string classType)
        {
            var stored = StoreClassReplacing(pointer, value, classType);
            RetainClassReference(stored.Value, classType);
            _ownedClassValues.Add((stored.Value, classType));
            return stored;
        }

        private (string Value, string Type) StoreScopeBoundClassAlias(string pointer, (string Value, string Type) value, string classType, bool replaceExisting = false)
        {
            var converted = GetClassReferenceType(value.Type) is not null ? value.Value : CastValue(value.Value, value.Type, classType);
            var oldValue = replaceExisting ? ReadStorage(classType, pointer) : null;
            Emit($"store ptr {converted}, ptr {pointer}");
            var owned = _ownedClassValues.FirstOrDefault(candidate => candidate.Value == converted);
            if (owned.Value is not null) _ownedClassValues.Remove(owned);
            else if (converted != "null") RetainClassReference(converted, classType);
            if (oldValue is not null) Emit($"call void @luma_class_release_dynamic(ptr {oldValue})");
            if (converted != "null") _scopeBoundClassValues.Add(converted);
            return (converted, classType);
        }

        private bool IsScopeBoundLocalTarget(Expression target) => target is NameExpression name &&
            TryLookup(name.Name, out var local) && local.IsScopeBound;

        private bool IsManagedAggregateType(string type) =>
            (structTypes.ContainsKey(type) || IsNullableValueType(type) && structTypes.ContainsKey(NullableUnderlyingType(type)) ||
             SemanticAnalyzer.TryParseInlineArrayType(type, out _, out _)) && StructContainsManagedReferences(type);

        private void EmitAggregateOwnership((string Value, string Type) value, bool retain)
        {
            if (!IsManagedAggregateType(value.Type)) return;
            var address = NewTemp();
            Emit($"{address} = alloca {LlvmType(value.Type)}");
            Emit($"store {LlvmType(value.Type)} {value.Value}, ptr {address}");
            Emit($"call void @{AggregateOwnershipFunctionName(value.Type, retain)}(ptr {address})");
        }

        private void EnsureOwnedAggregate((string Value, string Type) value)
        {
            if (!IsManagedAggregateType(value.Type)) return;
            if (!_ownedAggregateValues.Remove(value)) EmitAggregateOwnership(value, retain: true);
            _ownedAggregateValues.Add(value);
        }

        private void ReleaseOwnedAggregate((string Value, string Type) value)
        {
            if (!IsManagedAggregateType(value.Type) || !_ownedAggregateValues.Remove(value)) return;
            EmitAggregateOwnership(value, retain: false);
        }

        private void ReleaseAllOwnedAggregates()
        {
            foreach (var value in _ownedAggregateValues.ToArray()) ReleaseOwnedAggregate(value);
        }

        private void StoreAggregateInitial(string pointer, (string Value, string Type) value, string aggregateType)
        {
            var converted = CastValue(value.Value, value.Type, aggregateType);
            var sameOwnedValue = value.Type == aggregateType && _ownedAggregateValues.Remove(value);
            if (!sameOwnedValue) EmitAggregateOwnership((converted, aggregateType), retain: true);
            if (value.Type != aggregateType) ReleaseOwnedAggregate(value);
            Emit($"store {LlvmType(aggregateType)} {converted}, ptr {pointer}");
        }

        private (string Value, string Type) StoreAggregateReplacing(string pointer, (string Value, string Type) value, string aggregateType)
        {
            var converted = CastValue(value.Value, value.Type, aggregateType);
            var sameOwnedValue = value.Type == aggregateType && _ownedAggregateValues.Remove(value);
            if (!sameOwnedValue) EmitAggregateOwnership((converted, aggregateType), retain: true);
            if (value.Type != aggregateType) ReleaseOwnedAggregate(value);
            Emit($"call void @{AggregateOwnershipFunctionName(aggregateType, retain: false)}(ptr {pointer})");
            Emit($"store {LlvmType(aggregateType)} {converted}, ptr {pointer}");
            return (converted, aggregateType);
        }

        private (string Value, string Type) StoreAggregateExpressionResult(string pointer, (string Value, string Type) value, string aggregateType)
        {
            var stored = StoreAggregateReplacing(pointer, value, aggregateType);
            EmitAggregateOwnership(stored, retain: true);
            _ownedAggregateValues.Add(stored);
            return stored;
        }

        private (string Value, string Type) EmitStringConcatenation(string left, string right)
        {
            var result = NewTemp();
            Emit($"{result} = call ptr @luma_string_concat(ptr {left}, ptr {right})");
            EmitCheckPendingException();
            ReleaseOwnedString((left, "string"));
            ReleaseOwnedString((right, "string"));
            _ownedStringValues.Add(result);
            return (result, "string");
        }

        private (string Value, string Type) EmitStringConversion((string Value, string Type) value)
        {
            if (value.Type == "string") return value;
            if (IsNullableStringType(value.Type))
            {
                var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {value.Value}, null");
                var normalized = NewTemp(); Emit($"{normalized} = select i1 {isNull}, ptr @luma_string_empty, ptr {value.Value}");
                ReleaseOwnedString(value);
                return (normalized, "string");
            }
            if (value.Type == "null") return ("@luma_string_empty", "string");
            if (value.Type == "bool")
            {
                var result = NewTemp();
                Emit($"{result} = select i1 {value.Value}, ptr @luma_string_true, ptr @luma_string_false");
                return (result, "string");
            }
            if (enumTypes.ContainsKey(value.Type) && StringLiteralCodec.TryCreate(value.Type, out var enumName, out _))
                return ("@" + enumName.Symbol, "string");
            if (structTypes.ContainsKey(value.Type))
            {
                var customToString = methods.FirstOrDefault(candidate => candidate.IsStructInstanceMethod &&
                    QualifiedTypeName(candidate.Owner) == value.Type && candidate.Declaration.Name == "ToString" &&
                    candidate.Declaration.IsPublic && !candidate.Declaration.IsMutating &&
                    candidate.Declaration.ReturnType == "string" && candidate.Declaration.Parameters.Count == 0);
                if (customToString is not null)
                {
                    var receiver = NewTemp();
                    Emit($"{receiver} = alloca {LlvmType(value.Type)}");
                    Emit($"store {LlvmType(value.Type)} {value.Value}, ptr {receiver}");
                    var customString = NewTemp();
                    Emit($"{customString} = call ptr @{FunctionName(customToString, methods)}(ptr {receiver})");
                    EmitCheckPendingException();
                    _ownedStringValues.Add(customString);
                    return (customString, "string");
                }
                if (StringLiteralCodec.TryCreate(value.Type, out var structName, out _))
                    return ("@" + structName.Symbol, "string");
            }
            if (GetClassReferenceType(value.Type) is { } referenceType && classTypes.ContainsKey(referenceType))
                return EmitDynamicClassTypeName(value, returnNullWhenNull: false, releaseReceiver: false);

            var converted = NewTemp();
            if (value.Type == "char")
                Emit($"{converted} = call ptr @luma_string_from_char(i16 {value.Value})");
            else if (IsIntegerType(value.Type))
            {
                var integer = CastValue(value.Value, value.Type, "int64");
                Emit($"{converted} = call ptr @luma_string_from_i64(i64 {integer}, i1 {(IsUnsigned(value.Type) ? "false" : "true")})");
            }
            else if (value.Type is "float" or "double")
            {
                var number = value.Value;
                if (value.Type == "float")
                {
                    number = NewTemp();
                    Emit($"{number} = fpext float {value.Value} to double");
                }
                Emit($"{converted} = call ptr @luma_string_from_float(double {number}, i1 {(value.Type == "float" ? "true" : "false")})");
            }
            else throw new NotSupportedException($"Conversion from '{value.Type}' to string was not validated before emission.");

            EmitCheckPendingException();
            _ownedStringValues.Add(converted);
            return (converted, "string");
        }

        private (string Value, string Type) EmitClassToString(Expression receiverExpression, bool isNullConditional, bool callsBaseImplementation)
        {
            var receiver = EmitExpression(receiverExpression);
            return EmitDynamicClassTypeName(receiver, returnNullWhenNull: isNullConditional, releaseReceiver: isNullConditional,
                dispatchVirtual: !callsBaseImplementation);
        }

        private (string Value, string Type) EmitDynamicClassTypeName((string Value, string Type) receiver, bool returnNullWhenNull,
            bool releaseReceiver, bool dispatchVirtual = true)
        {
            var classType = GetClassReferenceType(receiver.Type);
            if (classType is null || !classTypes.ContainsKey(classType) && !interfaceTypes.ContainsKey(classType))
                throw new NotSupportedException($"Default ToString requires a class-backed receiver, not '{receiver.Type}'.");
            releaseReceiver &= !_scopeBoundClassValues.Contains(receiver.Value);
            if (releaseReceiver) EnsureOwnedClass(receiver, classType);

            var isNull = NewTemp(); Emit($"{isNull} = icmp eq ptr {receiver.Value}, null");
            var nullLabel = NewLabel("tostring.null");
            var valueLabel = NewLabel("tostring.value");
            var endLabel = NewLabel("tostring.end");
            Emit($"br i1 {isNull}, label %{nullLabel}, label %{valueLabel}"); _terminated = true;

            Label(nullLabel);
            if (releaseReceiver) Emit($"call void @luma_class_release_dynamic(ptr {receiver.Value})");
            var nullBlock = _block;
            Branch(endLabel);

            Label(valueLabel);
            var methodPointer = "@luma_object_tostring";
            if (dispatchVirtual)
            {
                var descriptor = NewTemp(); Emit($"{descriptor} = load ptr, ptr {receiver.Value}");
                var vtableAddress = NewTemp(); Emit($"{vtableAddress} = getelementptr %luma.typedesc, ptr {descriptor}, i32 0, i32 2");
                var vtable = NewTemp(); Emit($"{vtable} = load ptr, ptr {vtableAddress}");
                var methodAddress = NewTemp(); Emit($"{methodAddress} = getelementptr ptr, ptr {vtable}, i32 0");
                methodPointer = NewTemp(); Emit($"{methodPointer} = load ptr, ptr {methodAddress}");
            }
            var typeName = NewTemp(); Emit($"{typeName} = call ptr {methodPointer}(ptr {receiver.Value})");
            if (releaseReceiver) Emit($"call void @luma_class_release_dynamic(ptr {receiver.Value})");
            var valueBlock = _block;
            Branch(endLabel);

            Label(endLabel);
            var result = NewTemp();
            var nullValue = returnNullWhenNull ? "null" : "@luma_string_empty";
            Emit($"{result} = phi ptr [ {nullValue}, %{nullBlock} ], [ {typeName}, %{valueBlock} ]");
            _ownedStringValues.Add(result);
            if (releaseReceiver) _ownedClassValues.Remove((receiver.Value, classType));
            return (result, returnNullWhenNull ? "string?" : "string");
        }

        private static bool IsIntegerType(string type) => type is "int" or "uint" or "int8" or "uint8" or "int16" or "uint16" or "int32" or "uint32" or "int64" or "uint64";

        private static string ClassRetainName(string type) => "luma_class_" + string.Concat(type.Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : '_')) + "_retain";
        private static string ClassReleaseName(string type) => "luma_class_" + string.Concat(type.Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : '_')) + "_release";

        private void RetainClassReference(string value, string referenceType)
        {
            if (referenceType == "System.Object" || interfaceTypes.ContainsKey(referenceType) || delegateTypes.ContainsKey(referenceType)) Emit($"call void @luma_class_retain_dynamic(ptr {value})");
            else Emit($"call void @{ClassRetainName(referenceType)}(ptr {value})");
        }

        private void EnsureOwnedString((string Value, string Type) value)
        {
            if (!IsStringType(value.Type) || value.Value == "null") return;
            if (!_ownedStringValues.Remove(value.Value)) Emit($"call void @luma_string_retain(ptr {value.Value})");
            _ownedStringValues.Add(value.Value);
        }

        private void EnsureOwnedArray((string Value, string Type) value)
        {
            if (!SemanticAnalyzer.TryParseManagedArrayType(value.Type, out _, out _)) return;
            if (_scopeBoundArrayValues.Contains(value.Value)) return;
            if (!_ownedArrayValues.Remove(value.Value)) Emit($"call void @luma_array_retain(ptr {value.Value})");
            _ownedArrayValues.Add(value.Value);
        }

        private void EnsureOwnedArrayElement((string Value, string Type) value)
        {
            if (!SemanticAnalyzer.TryParseManagedArrayType(value.Type, out _, out _)) return;
            if (_scopeBoundArrayValues.Contains(value.Value)) Emit($"call void @luma_array_retain(ptr {value.Value})");
            else EnsureOwnedArray(value);
        }

        private void EnsureOwnedArrayElement(Expression target, (string Value, string Type) value)
        {
            if (target is IndexExpression index && managedArrayIndexAccesses.ContainsKey(index)) EnsureOwnedArrayElement(value);
        }

        private void EnsureOwnedClass((string Value, string Type) value, string classType)
        {
            classType = GetClassReferenceType(classType) ?? classType;
            if (GetClassReferenceType(classType) is null || value.Value == "null") return;
            if (_scopeBoundClassValues.Contains(value.Value)) return;
            var existingOwner = _ownedClassValues.FirstOrDefault(owned => owned.Value == value.Value);
            if (existingOwner.Value == value.Value) _ownedClassValues.Remove(existingOwner);
            else RetainClassReference(value.Value, classType);
            _ownedClassValues.Add((value.Value, classType));
        }

        private void EnsureOwnedClassElement((string Value, string Type) value, string classType)
        {
            classType = GetClassReferenceType(classType) ?? classType;
            if (GetClassReferenceType(classType) is null || value.Value == "null") return;
            if (_scopeBoundClassValues.Contains(value.Value)) RetainClassReference(value.Value, classType);
            else EnsureOwnedClass(value, classType);
        }

        private void EnsureOwnedClassElement(Expression target, (string Value, string Type) value, string classType)
        {
            if (target is IndexExpression index && managedArrayIndexAccesses.ContainsKey(index)) EnsureOwnedClassElement(value, classType);
            else if (target is MemberAccessExpression member && classFields.TryGetValue(member, out var field) && field.IsScopeBound ||
                     target is NameExpression name && unqualifiedClassFields.TryGetValue(name, out field) && field.IsScopeBound)
                EnsureOwnedClassElement(value, classType);
        }

        private bool IsScopeBoundClassExpression(Expression expression) => expression switch
        {
            NameExpression name when TryLookup(name.Name, out var local) => local.IsScopeBound,
            MemberAccessExpression member when classFields.TryGetValue(member, out var field) && field.IsScopeBound =>
                IsScopeBoundClassExpression(member.Receiver),
            NullAssertionExpression assertion => IsScopeBoundClassExpression(assertion.Operand),
            CastExpression cast => IsScopeBoundClassExpression(cast.Operand),
            OverflowContextExpression overflow => IsScopeBoundClassExpression(overflow.Value),
            ConditionalExpression conditional => IsScopeBoundClassExpression(conditional.WhenTrue) || IsScopeBoundClassExpression(conditional.WhenFalse),
            BinaryExpression { Operator: "??" } coalesce => IsScopeBoundClassExpression(coalesce.Left) || IsScopeBoundClassExpression(coalesce.Right),
            _ => false
        };

        private bool IsScopeBoundArrayExpression(Expression expression) => expression switch
        {
            NameExpression name when TryLookup(name.Name, out var local) => local.IsScopeBound,
            MemberAccessExpression member when classFields.TryGetValue(member, out var field) && field.IsScopeBound =>
                IsScopeBoundClassExpression(member.Receiver),
            IndexExpression index when managedArrayIndexAccesses.ContainsKey(index) => IsScopeBoundArrayExpression(index.Receiver),
            NullAssertionExpression assertion => IsScopeBoundArrayExpression(assertion.Operand),
            CastExpression cast => IsScopeBoundArrayExpression(cast.Operand),
            OverflowContextExpression overflow => IsScopeBoundArrayExpression(overflow.Value),
            ConditionalExpression conditional => IsScopeBoundArrayExpression(conditional.WhenTrue) || IsScopeBoundArrayExpression(conditional.WhenFalse),
            BinaryExpression { Operator: "??" } coalesce => IsScopeBoundArrayExpression(coalesce.Left) || IsScopeBoundArrayExpression(coalesce.Right),
            _ => false
        };

        private void ReleaseOwnedString((string Value, string Type) value)
        {
            if (IsStringType(value.Type) && _ownedStringValues.Remove(value.Value)) Emit($"call void @luma_string_release(ptr {value.Value})");
        }

        private void ReleaseOwnedArray((string Value, string Type) value)
        {
            if (SemanticAnalyzer.TryParseManagedArrayType(value.Type, out _, out _) && _ownedArrayValues.Remove(value.Value)) Emit($"call void @luma_array_release(ptr {value.Value})");
        }

        private void ReleaseOwnedClass((string Value, string Type) value)
        {
            if (GetClassReferenceType(value.Type) is null) return;
            var owned = _ownedClassValues.FirstOrDefault(candidate => candidate.Value == value.Value);
            if (owned.Value == value.Value && _ownedClassValues.Remove(owned)) Emit($"call void @luma_class_release_dynamic(ptr {value.Value})");
        }

        private void ReleaseAllOwnedStrings()
        {
            foreach (var value in _ownedStringValues.ToArray()) Emit($"call void @luma_string_release(ptr {value})");
            _ownedStringValues.Clear();
        }

        private void ReleaseAllOwnedArrays()
        {
            foreach (var value in _ownedArrayValues.ToArray()) Emit($"call void @luma_array_release(ptr {value})");
            _ownedArrayValues.Clear();
        }

        private void ReleaseAllOwnedClasses()
        {
            foreach (var value in _ownedClassValues.ToArray()) Emit($"call void @luma_class_release_dynamic(ptr {value.Value})");
            _ownedClassValues.Clear();
        }

        private void ReleaseStringPointer(string pointer)
        {
            var value = ReadStorage("string", pointer);
            Emit($"call void @luma_string_release(ptr {value})");
        }

        private void ReleaseScopeStrings(int scopeIndex)
        {
            var pointers = _scopeStringPointers[scopeIndex];
            for (var index = pointers.Count - 1; index >= 0; index--) ReleaseStringPointer(pointers[index]);
        }

        private void ReleaseScopeArrays(int scopeIndex)
        {
            var pointers = _scopeArrayPointers[scopeIndex];
            for (var index = pointers.Count - 1; index >= 0; index--)
            {
                var value = NewTemp(); Emit($"{value} = load ptr, ptr {pointers[index]}");
                Emit($"call void @luma_array_release(ptr {value})");
            }
        }

        private void ReleaseScopeClasses(int scopeIndex)
        {
            var pointers = _scopeClassPointers[scopeIndex];
            for (var index = pointers.Count - 1; index >= 0; index--)
            {
                var (pointer, type) = pointers[index];
                var value = NewTemp(); Emit($"{value} = load ptr, ptr {pointer}");
                Emit($"call void @luma_class_release_dynamic(ptr {value})");
                Emit($"store ptr null, ptr {pointer}");
            }
        }

        private void ReleaseScopeAggregates(int scopeIndex)
        {
            var pointers = _scopeAggregatePointers[scopeIndex];
            for (var index = pointers.Count - 1; index >= 0; index--)
                Emit($"call void @{AggregateOwnershipFunctionName(pointers[index].Type, retain: false)}(ptr {pointers[index].Pointer})");
        }

        private void ReleaseScopeAllocations(int scopeIndex)
        {
            var allocations = _scopeAllocations[scopeIndex];
            for (var index = allocations.Count - 1; index >= 0; index--)
            {
                var allocation = allocations[index];
                if (allocation.IsArray) Emit($"call void @luma_array_release(ptr {allocation.Value})");
                else Emit($"call void @luma_class_release_dynamic(ptr {allocation.Value})");
            }
        }

        private void ReleaseStatementScopeTemps()
        {
            for (var index = _statementScopeTemps.Count - 1; index >= 0; index--)
            {
                var allocation = _statementScopeTemps[index];
                if (allocation.IsArray) Emit("call void @luma_array_release(ptr " + allocation.Value + ")");
                else Emit($"call void @luma_class_release_dynamic(ptr {allocation.Value})");
            }
            _statementScopeTemps.Clear();
        }

        private void ReleaseAllScopes()
        {
            for (var scopeIndex = _scopeStringPointers.Count - 1; scopeIndex >= 0; scopeIndex--) { ReleaseScopeStrings(scopeIndex); ReleaseScopeArrays(scopeIndex); ReleaseScopeClasses(scopeIndex); ReleaseScopeAggregates(scopeIndex); ReleaseScopeAllocations(scopeIndex); }
        }

        private void ReleaseOwnedStringsAndScopesFrom(int scopeDepth)
        {
            ReleaseAllOwnedStrings(); ReleaseAllOwnedArrays(); ReleaseAllOwnedClasses();
            for (var scopeIndex = _scopeStringPointers.Count - 1; scopeIndex >= scopeDepth; scopeIndex--) { ReleaseScopeStrings(scopeIndex); ReleaseScopeArrays(scopeIndex); ReleaseScopeClasses(scopeIndex); ReleaseScopeAggregates(scopeIndex); ReleaseScopeAllocations(scopeIndex); }
        }
        private (string Type, string Pointer) ResolveAssignmentTarget(Expression target)
        {
            if (target is NameExpression name && TryLookup(name.Name, out var local)) return (local.Type, local.Pointer);
            if (target is NameExpression staticName && unqualifiedStaticFields.TryGetValue(staticName, out var unqualifiedStatic) && CanWriteStaticField(unqualifiedStatic))
            {
                EnsureTypeInitialized(unqualifiedStatic.OwnerType);
                return (unqualifiedStatic.TypeName, $"@{unqualifiedStatic.GlobalName}");
            }
            if (target is NameExpression fieldName && unqualifiedStructFields.TryGetValue(fieldName, out var unqualifiedField))
                return (unqualifiedField.TypeName, EmitCurrentStructFieldAddress(unqualifiedField));
            if (target is NameExpression classFieldName && unqualifiedClassFields.TryGetValue(classFieldName, out var unqualifiedClassField))
                return (unqualifiedClassField.TypeName, EmitCurrentClassFieldAddress(unqualifiedClassField));
            if (target is MemberAccessExpression access && staticFields.TryGetValue(access, out var field) && CanWriteStaticField(field))
            {
                EnsureTypeInitialized(field.OwnerType);
                return (field.TypeName, $"@{field.GlobalName}");
            }
            if (target is MemberAccessExpression structAccess && structFields.TryGetValue(structAccess, out var structField))
                return (structField.TypeName, EmitStructFieldAddress(structAccess, structField));
            if (target is MemberAccessExpression classAccess && classFields.TryGetValue(classAccess, out var classField))
                return (classField.TypeName, EmitClassFieldAddress(classAccess, classField));
            if (target is UnaryExpression { Operator: "*" } dereference)
            {
                var pointer = EmitExpression(dereference.Operand);
                return (pointer.Type[..^1], pointer.Value);
            }
            if (target is IndexExpression index)
            {
                var (address, elementType) = EmitIndexAddress(index);
                return (elementType, address);
            }
            throw new InvalidOperationException("Validated assignment target was not found during emission.");
        }

        private bool CanWriteStaticField(StaticFieldBinding field) => field.Mutable ||
            method.IsTypeInitializer && field.OwnerType == QualifiedTypeName(method.Owner);

        private void EnsureTypeInitialized(string typeName, bool force = false)
        {
            if (!force && method.IsTypeInitializer && typeName == QualifiedTypeName(method.Owner)) return;
            if (!typeInitializationFunctions.TryGetValue(typeName, out var initializerFunction)) return;
            Emit($"call void @{initializerFunction}()");
            EmitCheckPendingException();
        }

        private (string Type, string Pointer) ResolveAddress(Expression expression)
        {
            if (expression is NullAssertionExpression nullableStructAssertion && InferNullableStructType(nullableStructAssertion.Operand) is not null)
                return ResolveNullableStructAddress(nullableStructAssertion);
            if (expression is ThisExpression && method.IsStructConstructor) return (method.Declaration.ReturnType, "%this");
            if (expression is ThisExpression && method.IsStructInstanceMethod) return (QualifiedTypeName(method.Owner), "%this");
            if (expression is ThisExpression && (method.IsClassConstructor || method.IsClassInstanceMethod)) return (QualifiedTypeName(method.Owner), "%this");
            if (expression is ThisExpression && method.IsClassFinalizer) return (QualifiedTypeName(method.Owner), "%this");
            if (expression is NameExpression name && TryLookup(name.Name, out var local)) return (local.Type, local.Pointer);
            if (expression is NameExpression staticName && unqualifiedStaticFields.TryGetValue(staticName, out var unqualifiedStatic))
            {
                EnsureTypeInitialized(unqualifiedStatic.OwnerType);
                return (unqualifiedStatic.TypeName, $"@{unqualifiedStatic.GlobalName}");
            }
            if (expression is NameExpression fieldName && unqualifiedStructFields.TryGetValue(fieldName, out var unqualifiedField))
                return (unqualifiedField.TypeName, EmitCurrentStructFieldAddress(unqualifiedField));
            if (expression is NameExpression classFieldName && unqualifiedClassFields.TryGetValue(classFieldName, out var unqualifiedClassField))
                return (unqualifiedClassField.TypeName, EmitCurrentClassFieldAddress(unqualifiedClassField));
            if (expression is MemberAccessExpression access && structFields.TryGetValue(access, out var structField))
                return (structField.TypeName, EmitStructFieldAddress(access, structField));
            if (expression is MemberAccessExpression classAccess && classFields.TryGetValue(classAccess, out var classField))
                return (classField.TypeName, EmitClassFieldAddress(classAccess, classField));
            if (expression is MemberAccessExpression staticAccess && staticFields.TryGetValue(staticAccess, out var staticField))
            {
                EnsureTypeInitialized(staticField.OwnerType);
                return (staticField.TypeName, $"@{staticField.GlobalName}");
            }
            if (expression is IndexExpression index)
            {
                var (elementAddress, elementType) = EmitIndexAddress(index);
                return (elementType, elementAddress);
            }
            var value = EmitExpression(expression);
            var temporary = $"%v{_temp++}";
            Emit($"{temporary} = alloca {LlvmType(value.Type)}");
            Emit($"store {LlvmType(value.Type)} {value.Value}, ptr {temporary}");
            return (value.Type, temporary);
        }
        private string? InferNullableStructType(Expression expression)
        {
            var candidate = expression is NullAssertionExpression assertion ? assertion.Operand : expression;
            if (candidate is NameExpression name && TryLookup(name.Name, out var local) && IsNullableValueType(local.Type) && structTypes.ContainsKey(NullableUnderlyingType(local.Type)))
                return local.Type;
            if (candidate is MemberAccessExpression member && structFields.TryGetValue(member, out var field) && IsNullableValueType(field.TypeName) && structTypes.ContainsKey(NullableUnderlyingType(field.TypeName)))
                return field.TypeName;
            return null;
        }
        private string ReadStorage(string type, string pointer)
        {
            var result = NewTemp(); Emit($"{result} = load {LlvmType(type)}, ptr {pointer}"); return result;
        }
        private string EmitPointerOffset(string op, string pointerType, string pointerValue, string offsetValue, string offsetType)
        {
            var pointeeType = pointerType[..^1];
            var nativeInteger = nativePointerSize == 8 ? "int64" : "int32";
            var index = CastValue(offsetValue, offsetType, nativeInteger);
            if (op == "-") { var negative = NewTemp(); Emit($"{negative} = sub {LlvmType(nativeInteger)} 0, {index}"); index = negative; }
            var result = NewTemp(); Emit($"{result} = getelementptr {LlvmType(pointeeType)}, ptr {pointerValue}, {LlvmType(nativeInteger)} {index}"); return result;
        }
        private void Declare(string name, string type, string pointer, bool ownsString = true, bool ownsArray = true, bool ownsClass = true, bool ownsAggregate = true,
            bool isScopeBound = false, bool ownsScopeBoundClass = false)
        {
            _scopes[^1][name] = new(type, pointer, isScopeBound);
            if (isScopeBound)
            {
                if (ownsScopeBoundClass && GetClassReferenceType(type) is { } ownedScopeBoundClass)
                    _scopeClassPointers[^1].Add((pointer, ownedScopeBoundClass));
                else if (ownsClass && GetClassReferenceType(type) is { } scopeBoundClass)
                    _scopeClassPointers[^1].Add((pointer, scopeBoundClass));
                if (ownsString && IsStringType(type)) _scopeStringPointers[^1].Add(pointer);
                if (ownsArray && SemanticAnalyzer.TryParseManagedArrayType(type, out _, out _)) _scopeArrayPointers[^1].Add(pointer);
                return;
            }
            if (IsStringType(type) && ownsString) _scopeStringPointers[^1].Add(pointer);
            if (ownsArray && SemanticAnalyzer.TryParseManagedArrayType(type, out _, out _)) _scopeArrayPointers[^1].Add(pointer);
            if (ownsClass && GetClassReferenceType(type) is { } classType) _scopeClassPointers[^1].Add((pointer, classType));
            if (ownsAggregate && IsManagedAggregateType(type)) _scopeAggregatePointers[^1].Add((pointer, type));
        }
        private bool TryLookup(string name, out Local local) { for (var i = _scopes.Count - 1; i >= 0; i--) if (_scopes[i].TryGetValue(name, out local!)) return true; local = null!; return false; }
        private sealed record MultiAssignmentTarget(string Type, string Pointer, bool IsScopeBound,
            (string Value, string Type)? HeldArray, (string Value, string Type)? HeldClass = null,
            CallExpression? SetterCall = null, string? AccessorReceiver = null,
            IReadOnlyList<(string Value, string Type)>? AccessorArguments = null);
        private sealed record Local(string Type, string Pointer, bool IsScopeBound);
    }
}
