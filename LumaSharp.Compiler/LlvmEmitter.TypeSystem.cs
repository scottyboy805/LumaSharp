using System.Text;

namespace LumaSharp.Compiler;

internal sealed partial class LlvmEmitter
{
    private void EmitDynamicTypeRuntime()
    {
        if (!compilation.CallTargets.Values.Any(method => method.Intrinsic?.Name == "__typenew")) return;

        EmitTypeClassDistanceRuntime();
        EmitTypeNewFailureRuntime();

        var factories = new List<(int Id, string TypeName, string FactoryName)>();
        var id = 1;
        foreach (var typeName in compilation.RuntimeTypes.Keys.Order(StringComparer.Ordinal))
        {
            if (compilation.ClassTypes.ContainsKey(typeName))
            {
                var type = compilation.ClassTypes[typeName].Declaration;
                if (!type.IsStatic && type.Kind == "class" && (compilation.RuntimeTypes[typeName].Flags & 256) == 0)
                {
                    var constructors = compilation.Methods.Where(method => method.IsClassConstructor &&
                        QualifiedTypeName(method.Owner) == typeName &&
                        (method.SourceConstructor?.IsPublic ?? method.Declaration.IsPublic) &&
                        method.Declaration.Parameters.All(parameter => parameter.PassingMode.Length == 0 && parameter.DefaultValue is null && !parameter.IsVariadic))
                        .ToArray();
                    if (constructors.Length > 0)
                    {
                        EmitTypeNewClassFactory(typeName, constructors);
                        factories.Add((id, typeName, "luma_type_new_for_" + Safe(typeName)));
                    }
                }
            }
            else if (compilation.StructTypes.TryGetValue(typeName, out var structure) && !structure.Declaration.IsRefStruct)
            {
                var constructors = compilation.Methods.Where(method => method.IsStructConstructor &&
                    QualifiedTypeName(method.Owner) == typeName &&
                    (method.SourceConstructor?.IsPublic ?? method.Declaration.IsPublic) &&
                    method.Declaration.Parameters.All(parameter => parameter.PassingMode.Length == 0 && parameter.DefaultValue is null && !parameter.IsVariadic))
                    .ToArray();
                EmitTypeNewStructFactory(typeName, constructors);
                factories.Add((id, typeName, "luma_type_new_for_" + Safe(typeName)));
            }
            else if (IsTypeNewScalarOrEnum(typeName))
            {
                EmitTypeNewScalarFactory(typeName);
                factories.Add((id, typeName, "luma_type_new_for_" + Safe(typeName)));
            }
            id++;
        }

        var typeIdField = compilation.ClassTypes["System.Type"].Fields
            .Single(field => field.Declaration.Name == "typeId").Index + 4;
        _output.AppendLine("define internal ptr @luma_type_new(ptr %type, ptr %arguments) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %type.is.null = icmp eq ptr %type, null");
        _output.AppendLine("  br i1 %type.is.null, label %null.type, label %lookup");
        _output.AppendLine("null.type:");
        _output.AppendLine("  call void @luma_throw_null()");
        _output.AppendLine("  unreachable");
        _output.AppendLine("lookup:");
        _output.AppendLine($"  %type.id.address = getelementptr %class.System_Type, ptr %type, i32 0, i32 {typeIdField}");
        _output.AppendLine("  %type.id = load i32, ptr %type.id.address");
        var cases = factories.Select(factory => $"i32 {factory.Id}, label %type.{factory.Id}").ToArray();
        _output.AppendLine($"  switch i32 %type.id, label %missing [{string.Join(" ", cases)}]");
        foreach (var factory in factories)
        {
            _output.AppendLine($"type.{factory.Id}:");
            _output.AppendLine($"  %created.{factory.Id} = call ptr @{factory.FactoryName}(ptr %arguments)");
            _output.AppendLine($"  ret ptr %created.{factory.Id}");
        }
        _output.AppendLine("missing:");
        _output.AppendLine("  call void @luma_raise_type_new_failure()");
        _output.AppendLine("  ret ptr null");
        _output.AppendLine("}");
    }

    private void EmitTypeClassDistanceRuntime()
    {
        _output.AppendLine("define internal i32 @luma_type_class_distance(ptr %object, ptr %target) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %is.null = icmp eq ptr %object, null");
        _output.AppendLine("  br i1 %is.null, label %missing, label %start");
        _output.AppendLine("start:");
        _output.AppendLine("  %dynamic.type = load ptr, ptr %object");
        _output.AppendLine("  br label %walk");
        _output.AppendLine("walk:");
        _output.AppendLine("  %current = phi ptr [ %dynamic.type, %start ], [ %base, %next ]");
        _output.AppendLine("  %distance = phi i32 [ 0, %start ], [ %next.distance, %next ]");
        _output.AppendLine("  %matches = icmp eq ptr %current, %target");
        _output.AppendLine("  br i1 %matches, label %found, label %advance");
        _output.AppendLine("advance:");
        _output.AppendLine("  %base.address = getelementptr %luma.typedesc, ptr %current, i32 0, i32 1");
        _output.AppendLine("  %base = load ptr, ptr %base.address");
        _output.AppendLine("  %no.base = icmp eq ptr %base, null");
        _output.AppendLine("  br i1 %no.base, label %missing, label %next");
        _output.AppendLine("next:");
        _output.AppendLine("  %next.distance = add i32 %distance, 1");
        _output.AppendLine("  br label %walk");
        _output.AppendLine("found:");
        _output.AppendLine("  ret i32 %distance");
        _output.AppendLine("missing:");
        _output.AppendLine("  ret i32 -1");
        _output.AppendLine("}");
    }

    private void EmitTypeNewFailureRuntime()
    {
        _output.AppendLine("define internal void @luma_raise_type_new_failure() {");
        _output.AppendLine("entry:");
        if (!compilation.UsesExceptionHandling || !compilation.ClassTypes.ContainsKey("System.InvalidOperationException"))
        {
            _output.AppendLine("  call void @llvm.trap()");
            _output.AppendLine("  unreachable");
        }
        else
        {
            _output.AppendLine("  %previous = load ptr, ptr @luma_pending_exception");
            _output.AppendLine("  %has.previous = icmp ne ptr %previous, null");
            _output.AppendLine("  br i1 %has.previous, label %release.previous, label %set.failure");
            _output.AppendLine("release.previous:");
            _output.AppendLine("  call void @luma_class_release_dynamic(ptr %previous)");
            _output.AppendLine("  br label %set.failure");
            _output.AppendLine("set.failure:");
            _output.AppendLine("  store ptr @luma_type_new_failure, ptr @luma_pending_exception");
            _output.AppendLine("  %handler = call ptr @luma_eh_find_handler(ptr @luma_type_new_failure)");
            _output.AppendLine("  store ptr %handler, ptr @luma_pending_handler");
            _output.AppendLine("  ret void");
        }
        _output.AppendLine("}");
    }

    private void EmitTypeNewClassFactory(string typeName, IReadOnlyList<MethodSymbol> constructors)
    {
        var safe = Safe(typeName);
        foreach (var (constructor, index) in constructors.Select((method, index) => (method, index)))
        {
            EmitTypeNewConstructorMatcher(typeName, constructor, index);
            EmitTypeNewConstructorInvoker(typeName, constructor, index);
        }

        _output.AppendLine($"define internal ptr @luma_type_new_for_{safe}(ptr %arguments) {{");
        _output.AppendLine("entry:");
        _output.AppendLine("  %best.address = alloca i32");
        _output.AppendLine("  store i32 -1, ptr %best.address");
        _output.AppendLine("  %score.address = alloca i32");
        _output.AppendLine("  store i32 2147483647, ptr %score.address");
        _output.AppendLine("  %ambiguous.address = alloca i1");
        _output.AppendLine("  store i1 false, ptr %ambiguous.address");
        for (var index = 0; index < constructors.Count; index++)
        {
            _output.AppendLine($"  %candidate.{index} = call i32 @luma_type_new_match_{safe}_{index}(ptr %arguments)");
            _output.AppendLine($"  %valid.{index} = icmp sge i32 %candidate.{index}, 0");
            _output.AppendLine($"  %old.score.{index} = load i32, ptr %score.address");
            _output.AppendLine($"  %better.{index} = icmp slt i32 %candidate.{index}, %old.score.{index}");
            _output.AppendLine($"  %same.{index} = icmp eq i32 %candidate.{index}, %old.score.{index}");
            _output.AppendLine($"  %select.{index} = and i1 %valid.{index}, %better.{index}");
            _output.AppendLine($"  %new.score.{index} = select i1 %select.{index}, i32 %candidate.{index}, i32 %old.score.{index}");
            _output.AppendLine($"  store i32 %new.score.{index}, ptr %score.address");
            _output.AppendLine($"  %old.ambiguous.{index} = load i1, ptr %ambiguous.address");
            _output.AppendLine($"  %equal.valid.{index} = and i1 %valid.{index}, %same.{index}");
            _output.AppendLine($"  %ambiguous.with.equal.{index} = or i1 %old.ambiguous.{index}, %equal.valid.{index}");
            _output.AppendLine($"  %ambiguous.new.{index} = select i1 %select.{index}, i1 false, i1 %ambiguous.with.equal.{index}");
            _output.AppendLine($"  store i1 %ambiguous.new.{index}, ptr %ambiguous.address");
            _output.AppendLine($"  %old.best.{index} = load i32, ptr %best.address");
            _output.AppendLine($"  %new.best.{index} = select i1 %select.{index}, i32 {index}, i32 %old.best.{index}");
            _output.AppendLine($"  store i32 %new.best.{index}, ptr %best.address");
        }
        _output.AppendLine("  %best = load i32, ptr %best.address");
        _output.AppendLine("  %no.match = icmp slt i32 %best, 0");
        _output.AppendLine("  %ambiguous = load i1, ptr %ambiguous.address");
        _output.AppendLine("  %invalid = or i1 %no.match, %ambiguous");
        _output.AppendLine("  br i1 %invalid, label %failure, label %invoke");
        _output.AppendLine("failure:");
        _output.AppendLine("  call void @luma_raise_type_new_failure()");
        _output.AppendLine("  ret ptr null");
        _output.AppendLine("invoke:");
        var cases = constructors.Select((_, index) => $"i32 {index}, label %invoke.{index}").ToArray();
        _output.AppendLine($"  switch i32 %best, label %failure [{string.Join(" ", cases)}]");
        for (var index = 0; index < constructors.Count; index++)
        {
            _output.AppendLine($"invoke.{index}:");
            _output.AppendLine($"  %instance.{index} = call ptr @luma_type_new_invoke_{safe}_{index}(ptr %arguments)");
            _output.AppendLine($"  ret ptr %instance.{index}");
        }
        _output.AppendLine("}");
    }

    private void EmitTypeNewStructFactory(string typeName, IReadOnlyList<MethodSymbol> constructors)
    {
        var safe = Safe(typeName);
        EmitTypeNewDefaultValueFactory(typeName);
        foreach (var (constructor, index) in constructors.Select((method, index) => (method, index)))
        {
            EmitTypeNewConstructorMatcher(typeName, constructor, index);
            EmitTypeNewStructConstructorInvoker(typeName, constructor, index);
        }

        _output.AppendLine($"define internal ptr @luma_type_new_for_{safe}(ptr %arguments) {{");
        _output.AppendLine("entry:");
        EmitTypeNewEmptyArgumentsCheck("default", "match");
        _output.AppendLine("default:");
        _output.AppendLine($"  %default.value = call ptr @luma_type_new_default_{safe}()");
        _output.AppendLine("  ret ptr %default.value");
        _output.AppendLine("match:");
        if (constructors.Count == 0)
        {
            _output.AppendLine("  call void @luma_raise_type_new_failure()");
            _output.AppendLine("  ret ptr null");
            _output.AppendLine("}");
            return;
        }
        _output.AppendLine("  %best.address = alloca i32");
        _output.AppendLine("  store i32 -1, ptr %best.address");
        _output.AppendLine("  %score.address = alloca i32");
        _output.AppendLine("  store i32 2147483647, ptr %score.address");
        _output.AppendLine("  %ambiguous.address = alloca i1");
        _output.AppendLine("  store i1 false, ptr %ambiguous.address");
        for (var index = 0; index < constructors.Count; index++)
        {
            _output.AppendLine($"  %candidate.{index} = call i32 @luma_type_new_match_{safe}_{index}(ptr %arguments)");
            _output.AppendLine($"  %valid.{index} = icmp sge i32 %candidate.{index}, 0");
            _output.AppendLine($"  %old.score.{index} = load i32, ptr %score.address");
            _output.AppendLine($"  %better.{index} = icmp slt i32 %candidate.{index}, %old.score.{index}");
            _output.AppendLine($"  %same.{index} = icmp eq i32 %candidate.{index}, %old.score.{index}");
            _output.AppendLine($"  %select.{index} = and i1 %valid.{index}, %better.{index}");
            _output.AppendLine($"  %new.score.{index} = select i1 %select.{index}, i32 %candidate.{index}, i32 %old.score.{index}");
            _output.AppendLine($"  store i32 %new.score.{index}, ptr %score.address");
            _output.AppendLine($"  %old.ambiguous.{index} = load i1, ptr %ambiguous.address");
            _output.AppendLine($"  %equal.valid.{index} = and i1 %valid.{index}, %same.{index}");
            _output.AppendLine($"  %ambiguous.with.equal.{index} = or i1 %old.ambiguous.{index}, %equal.valid.{index}");
            _output.AppendLine($"  %ambiguous.new.{index} = select i1 %select.{index}, i1 false, i1 %ambiguous.with.equal.{index}");
            _output.AppendLine($"  store i1 %ambiguous.new.{index}, ptr %ambiguous.address");
            _output.AppendLine($"  %old.best.{index} = load i32, ptr %best.address");
            _output.AppendLine($"  %new.best.{index} = select i1 %select.{index}, i32 {index}, i32 %old.best.{index}");
            _output.AppendLine($"  store i32 %new.best.{index}, ptr %best.address");
        }
        _output.AppendLine("  %best = load i32, ptr %best.address");
        _output.AppendLine("  %no.match = icmp slt i32 %best, 0");
        _output.AppendLine("  %ambiguous = load i1, ptr %ambiguous.address");
        _output.AppendLine("  %invalid = or i1 %no.match, %ambiguous");
        _output.AppendLine("  br i1 %invalid, label %failure, label %invoke");
        _output.AppendLine("failure:");
        _output.AppendLine("  call void @luma_raise_type_new_failure()");
        _output.AppendLine("  ret ptr null");
        _output.AppendLine("invoke:");
        var cases = constructors.Select((_, index) => $"i32 {index}, label %invoke.{index}").ToArray();
        _output.AppendLine($"  switch i32 %best, label %failure [{string.Join(" ", cases)}]");
        for (var index = 0; index < constructors.Count; index++)
        {
            _output.AppendLine($"invoke.{index}:");
            _output.AppendLine($"  %instance.{index} = call ptr @luma_type_new_invoke_{safe}_{index}(ptr %arguments)");
            _output.AppendLine($"  ret ptr %instance.{index}");
        }
        _output.AppendLine("}");
    }

    private void EmitTypeNewScalarFactory(string typeName)
    {
        var safe = Safe(typeName);
        EmitTypeNewDefaultValueFactory(typeName);
        _output.AppendLine($"define internal ptr @luma_type_new_for_{safe}(ptr %arguments) {{");
        _output.AppendLine("entry:");
        EmitTypeNewEmptyArgumentsCheck("default", "failure");
        _output.AppendLine("default:");
        _output.AppendLine($"  %default.value = call ptr @luma_type_new_default_{safe}()");
        _output.AppendLine("  ret ptr %default.value");
        _output.AppendLine("failure:");
        _output.AppendLine("  call void @luma_raise_type_new_failure()");
        _output.AppendLine("  ret ptr null");
        _output.AppendLine("}");
    }

    private void EmitTypeNewEmptyArgumentsCheck(string emptyLabel, string nonEmptyLabel)
    {
        _output.AppendLine("  %arguments.is.null = icmp eq ptr %arguments, null");
        _output.AppendLine($"  br i1 %arguments.is.null, label %{emptyLabel}, label %arguments.count");
        _output.AppendLine("arguments.count:");
        _output.AppendLine("  %arguments.length.address = getelementptr %luma.array, ptr %arguments, i32 0, i32 4");
        _output.AppendLine("  %arguments.length = load i32, ptr %arguments.length.address");
        _output.AppendLine("  %arguments.empty = icmp eq i32 %arguments.length, 0");
        _output.AppendLine($"  br i1 %arguments.empty, label %{emptyLabel}, label %{nonEmptyLabel}");
    }

    private void EmitTypeNewDefaultValueFactory(string typeName)
    {
        var safe = Safe(typeName);
        var sizeType = compilation.NativePointerSize == 8 ? "i64" : "i32";
        var isStruct = compilation.StructTypes.ContainsKey(typeName);
        var boxType = isStruct ? $"%luma.structbox.{safe}" : "%luma.box";
        _output.AppendLine($"define internal ptr @luma_type_new_default_{safe}() {{");
        _output.AppendLine("entry:");
        _output.AppendLine($"  %box.size.address = getelementptr {boxType}, ptr null, i32 1");
        _output.AppendLine($"  %box.size = ptrtoint ptr %box.size.address to {sizeType}");
        _output.AppendLine($"  %box = call ptr @malloc({sizeType} %box.size)");
        _output.AppendLine("  %out.of.memory = icmp eq ptr %box, null");
        _output.AppendLine("  br i1 %out.of.memory, label %oom, label %initialize");
        _output.AppendLine("oom:");
        _output.AppendLine(compilation.UsesExceptionHandling
            ? "  call void @luma_raise_out_of_memory()"
            : "  call void @luma_throw_out_of_memory()");
        _output.AppendLine(compilation.UsesExceptionHandling ? "  ret ptr null" : "  unreachable");
        _output.AppendLine("initialize:");
        var descriptorType = isStruct ? boxType : "%luma.box";
        _output.AppendLine($"  %descriptor = getelementptr {descriptorType}, ptr %box, i32 0, i32 0");
        _output.AppendLine($"  store ptr @luma_boxdesc_{safe}, ptr %descriptor");
        _output.AppendLine($"  %reference.count = getelementptr {descriptorType}, ptr %box, i32 0, i32 1");
        _output.AppendLine("  store i32 1, ptr %reference.count");
        _output.AppendLine($"  %reference.flags = getelementptr {descriptorType}, ptr %box, i32 0, i32 2");
        _output.AppendLine("  store i32 0, ptr %reference.flags");
        _output.AppendLine($"  %weak.control = getelementptr {descriptorType}, ptr %box, i32 0, i32 3");
        _output.AppendLine("  store ptr null, ptr %weak.control");
        _output.AppendLine($"  %payload = getelementptr {descriptorType}, ptr %box, i32 0, i32 4");
        if (isStruct)
            _output.AppendLine($"  store %struct.{safe} zeroinitializer, ptr %payload");
        else
            _output.AppendLine("  store i64 0, ptr %payload");
        _output.AppendLine("  ret ptr %box");
        _output.AppendLine("}");
    }

    private bool IsTypeNewScalarOrEnum(string typeName) => compilation.EnumUnderlyingTypes.ContainsKey(typeName) ||
        typeName is "bool" or "char" or "int" or "uint" or "int8" or "uint8" or "int16" or "uint16" or
            "int32" or "uint32" or "int64" or "uint64" or "float" or "double";

    private void EmitTypeNewStructConstructorInvoker(string typeName, MethodSymbol constructor, int index)
    {
        var safe = Safe(typeName);
        var structType = compilation.StructTypes[typeName];
        var parameters = constructor.Declaration.Parameters;
        var sizeType = compilation.NativePointerSize == 8 ? "i64" : "i32";
        _output.AppendLine($"define internal ptr @luma_type_new_invoke_{safe}_{index}(ptr %arguments) {{");
        _output.AppendLine("entry:");
        if (_typeInitializationFunctions.TryGetValue(typeName, out var typeInitializer))
            _output.AppendLine($"  call void @{typeInitializer}()");
        if (parameters.Count > 0) _output.AppendLine("  %data = getelementptr %luma.array, ptr %arguments, i32 0, i32 7");
        var callArguments = new List<string>();
        for (var parameterIndex = 0; parameterIndex < parameters.Count; parameterIndex++)
        {
            var parameterType = parameters[parameterIndex].TypeName;
            var argument = $"%argument.{parameterIndex}";
            _output.AppendLine($"  {argument}.address = getelementptr ptr, ptr %data, i32 {parameterIndex}");
            _output.AppendLine($"  {argument} = load ptr, ptr {argument}.address");
            callArguments.Add($"{LlvmType(parameterType)} {UnboxArgument(parameterType, argument, parameterIndex)}");
        }
        var resultType = LlvmType(QualifiedTypeName(structType.Declaration));
        _output.AppendLine($"  %constructed = call {resultType} @{FunctionName(constructor, compilation.Methods)}({string.Join(", ", callArguments)})");
        if (compilation.UsesExceptionHandling)
        {
            _output.AppendLine("  %exception = load ptr, ptr @luma_pending_exception");
            _output.AppendLine("  %constructor.failed = icmp ne ptr %exception, null");
            _output.AppendLine("  br i1 %constructor.failed, label %failed, label %allocate");
            _output.AppendLine("failed:");
            _output.AppendLine("  ret ptr null");
            _output.AppendLine("allocate:");
        }
        _output.AppendLine($"  %box.size.address = getelementptr %luma.structbox.{safe}, ptr null, i32 1");
        _output.AppendLine($"  %box.size = ptrtoint ptr %box.size.address to {sizeType}");
        _output.AppendLine($"  %box = call ptr @malloc({sizeType} %box.size)");
        _output.AppendLine("  %out.of.memory = icmp eq ptr %box, null");
        if (compilation.UsesExceptionHandling)
        {
            _output.AppendLine("  br i1 %out.of.memory, label %oom, label %initialize");
            _output.AppendLine("oom:");
            _output.AppendLine("  call void @luma_raise_out_of_memory()");
            _output.AppendLine("  ret ptr null");
            _output.AppendLine("initialize:");
        }
        else
        {
            _output.AppendLine("  br i1 %out.of.memory, label %oom, label %initialize");
            _output.AppendLine("oom:");
            _output.AppendLine("  call void @luma_throw_out_of_memory()");
            _output.AppendLine("  unreachable");
            _output.AppendLine("initialize:");
        }
        _output.AppendLine($"  %descriptor = getelementptr %luma.structbox.{safe}, ptr %box, i32 0, i32 0");
        _output.AppendLine($"  store ptr @luma_boxdesc_{safe}, ptr %descriptor");
        _output.AppendLine($"  %reference.count = getelementptr %luma.structbox.{safe}, ptr %box, i32 0, i32 1");
        _output.AppendLine("  store i32 1, ptr %reference.count");
        _output.AppendLine($"  %reference.flags = getelementptr %luma.structbox.{safe}, ptr %box, i32 0, i32 2");
        _output.AppendLine("  store i32 0, ptr %reference.flags");
        _output.AppendLine($"  %weak.control = getelementptr %luma.structbox.{safe}, ptr %box, i32 0, i32 3");
        _output.AppendLine("  store ptr null, ptr %weak.control");
        _output.AppendLine($"  %payload = getelementptr %luma.structbox.{safe}, ptr %box, i32 0, i32 4");
        _output.AppendLine($"  store {resultType} %constructed, ptr %payload");
        _output.AppendLine("  ret ptr %box");
        _output.AppendLine("}");

        string UnboxArgument(string parameterType, string argument, int parameterIndex)
        {
            if (parameterType.EndsWith("?", StringComparison.Ordinal) && IsNullableValueRuntimeType(parameterType[..^1]))
            {
                var underlying = parameterType[..^1];
                var isNull = $"%argument.{parameterIndex}.nullable";
                _output.AppendLine($"  {isNull} = icmp eq ptr {argument}, null");
                var nullLabel = $"argument.{parameterIndex}.nullable.null";
                var valueLabel = $"argument.{parameterIndex}.nullable.present";
                var endLabel = $"argument.{parameterIndex}.nullable.end";
                _output.AppendLine($"  br i1 {isNull}, label %{nullLabel}, label %{valueLabel}");
                _output.AppendLine(valueLabel + ":");
                var decoded = UnboxArgument(underlying, argument, parameterIndex);
                var withTag = $"%argument.{parameterIndex}.nullable.with.tag";
                var inner = $"%argument.{parameterIndex}.nullable.with.value";
                _output.AppendLine($"  {withTag} = insertvalue {LlvmType(parameterType)} zeroinitializer, i1 true, 0");
                _output.AppendLine($"  {inner} = insertvalue {LlvmType(parameterType)} {withTag}, {LlvmType(underlying)} {decoded}, 1");
                _output.AppendLine($"  br label %{endLabel}");
                _output.AppendLine(nullLabel + ":");
                _output.AppendLine($"  br label %{endLabel}");
                _output.AppendLine(endLabel + ":");
                var result = $"%argument.{parameterIndex}.nullable.value";
                _output.AppendLine($"  {result} = phi {LlvmType(parameterType)} [ zeroinitializer, %{nullLabel} ], [ {inner}, %{valueLabel} ]");
                return result;
            }
            if (IsRuntimeReferenceType(parameterType) || parameterType.EndsWith("?", StringComparison.Ordinal) && IsRuntimeReferenceType(parameterType[..^1])) return argument;
            if (compilation.StructTypes.ContainsKey(parameterType))
            {
                var payload = $"%argument.{parameterIndex}.payload.address";
                var result = $"%argument.{parameterIndex}.payload";
                _output.AppendLine($"  {payload} = getelementptr %luma.structbox.{Safe(parameterType)}, ptr {argument}, i32 0, i32 4");
                _output.AppendLine($"  {result} = load %struct.{Safe(parameterType)}, ptr {payload}");
                return result;
            }
            var scalarPayload = $"%argument.{parameterIndex}.scalar.address";
            var scalar = $"%argument.{parameterIndex}.scalar";
            _output.AppendLine($"  {scalarPayload} = getelementptr %luma.box, ptr {argument}, i32 0, i32 4");
            _output.AppendLine($"  {scalar} = load i64, ptr {scalarPayload}");
            if (parameterType == "bool")
            {
                var result = $"%argument.{parameterIndex}.bool";
                _output.AppendLine($"  {result} = trunc i64 {scalar} to i1");
                return result;
            }
            if (parameterType == "float")
            {
                var bits = $"%argument.{parameterIndex}.float.bits";
                var result = $"%argument.{parameterIndex}.float";
                _output.AppendLine($"  {bits} = trunc i64 {scalar} to i32");
                _output.AppendLine($"  {result} = bitcast i32 {bits} to float");
                return result;
            }
            if (parameterType == "double")
            {
                var result = $"%argument.{parameterIndex}.double";
                _output.AppendLine($"  {result} = bitcast i64 {scalar} to double");
                return result;
            }
            var llvmType = LlvmType(parameterType);
            if (llvmType == "i64") return scalar;
            var converted = $"%argument.{parameterIndex}.converted";
            _output.AppendLine($"  {converted} = trunc i64 {scalar} to {llvmType}");
            return converted;
        }
    }

    private void EmitTypeNewConstructorMatcher(string typeName, MethodSymbol constructor, int index)
    {
        var safe = Safe(typeName);
        var parameters = constructor.Declaration.Parameters;
        _output.AppendLine($"define internal i32 @luma_type_new_match_{safe}_{index}(ptr %arguments) {{");
        _output.AppendLine("entry:");
        _output.AppendLine("  %is.null = icmp eq ptr %arguments, null");
        _output.AppendLine("  br i1 %is.null, label %no, label %count");
        _output.AppendLine("count:");
        _output.AppendLine("  %length.address = getelementptr %luma.array, ptr %arguments, i32 0, i32 4");
        _output.AppendLine("  %length = load i32, ptr %length.address");
        _output.AppendLine($"  %count.matches = icmp eq i32 %length, {parameters.Count}");
        _output.AppendLine("  br i1 %count.matches, label %check.arguments, label %no");
        _output.AppendLine("check.arguments:");
        _output.AppendLine("  %data = getelementptr %luma.array, ptr %arguments, i32 0, i32 7");
        var scoreTerms = new List<string>();
        for (var parameterIndex = 0; parameterIndex < parameters.Count; parameterIndex++)
        {
            var parameter = parameters[parameterIndex];
            var normalized = parameter.TypeName.EndsWith("?", StringComparison.Ordinal) ? parameter.TypeName[..^1] : parameter.TypeName;
            var argumentStartLabel = parameterIndex == 0 ? "check.arguments" : $"arguments.{parameterIndex - 1}.continue";
            var argument = $"%argument.{parameterIndex}";
            _output.AppendLine($"  {argument}.address = getelementptr ptr, ptr %data, i32 {parameterIndex}");
            _output.AppendLine($"  {argument} = load ptr, ptr {argument}.address");
            var nullAllowed = IsRuntimeReferenceType(normalized) || parameter.TypeName.EndsWith("?", StringComparison.Ordinal);
            var isNull = $"%argument.{parameterIndex}.null";
            _output.AppendLine($"  {isNull} = icmp eq ptr {argument}, null");
            var validLabel = $"arguments.{parameterIndex}.valid";
            var checkLabel = $"arguments.{parameterIndex}.check";
            if (nullAllowed) _output.AppendLine($"  br i1 {isNull}, label %{validLabel}, label %{checkLabel}");
            else _output.AppendLine($"  br i1 {isNull}, label %no, label %{checkLabel}");
            _output.AppendLine(checkLabel + ":");
            string distance;
            if (compilation.InterfaceTypes.ContainsKey(normalized))
            {
                distance = $"%argument.{parameterIndex}.distance";
                var implements = $"%argument.{parameterIndex}.implements";
                _output.AppendLine($"  {implements} = call i1 @luma_class_implements(ptr {argument}, ptr @luma_interfacedesc_{Safe(normalized)})");
                _output.AppendLine($"  {distance} = select i1 {implements}, i32 1, i32 -1");
            }
            else if (RuntimeTypeDescriptor(normalized) is { } descriptor &&
                     (compilation.ClassTypes.ContainsKey(normalized) || SemanticAnalyzer.TryParseManagedArrayType(normalized, out _, out _) || normalized is "string" or "object" or "System.Object"))
            {
                distance = $"%argument.{parameterIndex}.distance";
                var classDistance = $"%argument.{parameterIndex}.class.distance";
                _output.AppendLine($"  {classDistance} = call i32 @luma_type_class_distance(ptr {argument}, ptr {descriptor})");
                _output.AppendLine($"  {distance} = mul i32 {classDistance}, 2");
            }
            else
            {
                distance = $"%argument.{parameterIndex}.distance";
                var exactDescriptor = RuntimeTypeDescriptor(normalized);
                if (exactDescriptor is null)
                {
                    _output.AppendLine($"  br label %no");
                    _output.AppendLine(validLabel + ":");
                    _output.AppendLine($"  br label %no");
                    returnNoFunction();
                    return;
                }
                var descriptorAddress = $"%argument.{parameterIndex}.descriptor.address";
                var descriptorValue = $"%argument.{parameterIndex}.descriptor";
                var exact = $"%argument.{parameterIndex}.exact";
                _output.AppendLine($"  {descriptorAddress} = getelementptr ptr, ptr {argument}, i32 0");
                _output.AppendLine($"  {descriptorValue} = load ptr, ptr {descriptorAddress}");
                _output.AppendLine($"  {exact} = icmp eq ptr {descriptorValue}, {exactDescriptor}");
                _output.AppendLine($"  {distance} = select i1 {exact}, i32 0, i32 -1");
            }
            _output.AppendLine($"  br label %{validLabel}");
            _output.AppendLine(validLabel + ":");
            if (nullAllowed)
            {
                var selected = $"%argument.{parameterIndex}.selected.distance";
                _output.AppendLine($"  {selected} = phi i32 [ 0, %{argumentStartLabel} ], [ {distance}, %{checkLabel} ]");
                distance = selected;
            }
            var valid = $"%argument.{parameterIndex}.valid.match";
            _output.AppendLine($"  {valid} = icmp sge i32 {distance}, 0");
            var continueLabel = $"arguments.{parameterIndex}.continue";
            _output.AppendLine($"  br i1 {valid}, label %{continueLabel}, label %no");
            _output.AppendLine(continueLabel + ":");
            scoreTerms.Add(distance);
        }
        var sum = "0";
        for (var i = 0; i < scoreTerms.Count; i++)
        {
            var next = $"%score.{i}";
            _output.AppendLine($"  {next} = add i32 {sum}, {scoreTerms[i]}");
            sum = next;
        }
        _output.AppendLine($"  ret i32 {sum}");
        _output.AppendLine("no:");
        _output.AppendLine("  ret i32 -1");
        _output.AppendLine("}");

        void returnNoFunction()
        {
            _output.AppendLine("no:");
            _output.AppendLine("  ret i32 -1");
            _output.AppendLine("}");
        }
    }

    private void EmitTypeNewConstructorInvoker(string typeName, MethodSymbol constructor, int index)
    {
        var safe = Safe(typeName);
        var classType = compilation.ClassTypes[typeName];
        var args = constructor.Declaration.Parameters;
        var sizeType = compilation.NativePointerSize == 8 ? "i64" : "i32";
        _output.AppendLine($"define internal ptr @luma_type_new_invoke_{safe}_{index}(ptr %arguments) {{");
        _output.AppendLine("entry:");
        if (_typeInitializationFunctions.TryGetValue(typeName, out var typeInitializer))
            _output.AppendLine($"  call void @{typeInitializer}()");
        _output.AppendLine($"  %size.address = getelementptr %class.{safe}, ptr null, i32 1");
        _output.AppendLine($"  %size = ptrtoint ptr %size.address to {sizeType}");
        _output.AppendLine($"  %instance = call ptr @malloc({sizeType} %size)");
        _output.AppendLine("  %out.of.memory = icmp eq ptr %instance, null");
        if (compilation.UsesExceptionHandling)
        {
            _output.AppendLine("  br i1 %out.of.memory, label %oom, label %initialize");
            _output.AppendLine("oom:");
            _output.AppendLine("  call void @luma_raise_out_of_memory()");
            _output.AppendLine("  ret ptr null");
            _output.AppendLine("initialize:");
        }
        else
        {
            _output.AppendLine("  br i1 %out.of.memory, label %oom, label %initialize");
            _output.AppendLine("oom:");
            _output.AppendLine("  call void @luma_throw_out_of_memory()");
            _output.AppendLine("  unreachable");
            _output.AppendLine("initialize:");
        }
        _output.AppendLine($"  %descriptor = getelementptr %class.{safe}, ptr %instance, i32 0, i32 0");
        _output.AppendLine($"  store ptr @luma_classdesc_{safe}, ptr %descriptor");
        _output.AppendLine($"  %reference.count = getelementptr %class.{safe}, ptr %instance, i32 0, i32 1");
        _output.AppendLine("  store i32 1, ptr %reference.count");
        _output.AppendLine($"  %reference.flags = getelementptr %class.{safe}, ptr %instance, i32 0, i32 2");
        _output.AppendLine("  store i32 0, ptr %reference.flags");
        _output.AppendLine($"  %weak.control = getelementptr %class.{safe}, ptr %instance, i32 0, i32 3");
        _output.AppendLine("  store ptr null, ptr %weak.control");
        foreach (var field in classType.Fields)
        {
            _output.AppendLine($"  %field.{field.Index} = getelementptr %class.{safe}, ptr %instance, i32 0, i32 {field.Index + 4}");
            _output.AppendLine($"  store {LlvmType(field.TypeName)} {DefaultStaticStorageValue(field.TypeName)}, ptr %field.{field.Index}");
        }
        var dataIndex = args.Count == 0 ? null : "";
        if (args.Count > 0) _output.AppendLine("  %data = getelementptr %luma.array, ptr %arguments, i32 0, i32 7");
        var callArguments = new List<string> { $"ptr %instance" };
        for (var parameterIndex = 0; parameterIndex < args.Count; parameterIndex++)
        {
            var parameterType = args[parameterIndex].TypeName;
            var argument = $"%argument.{parameterIndex}";
            _output.AppendLine($"  {argument}.address = getelementptr ptr, ptr %data, i32 {parameterIndex}");
            _output.AppendLine($"  {argument} = load ptr, ptr {argument}.address");
            var value = UnboxArgument(parameterType, argument, parameterIndex);
            callArguments.Add($"{LlvmType(parameterType)} {value}");
        }
        callArguments.Add("i1 true");
        _output.AppendLine($"  call void @{FunctionName(constructor, compilation.Methods)}({string.Join(", ", callArguments)})");
        if (compilation.UsesExceptionHandling)
        {
            _output.AppendLine("  %exception = load ptr, ptr @luma_pending_exception");
            _output.AppendLine("  %constructor.failed = icmp ne ptr %exception, null");
            _output.AppendLine("  br i1 %constructor.failed, label %failed, label %ready");
            _output.AppendLine("failed:");
            _output.AppendLine("  call void @luma_class_release_dynamic(ptr %instance)");
            _output.AppendLine("  ret ptr null");
            _output.AppendLine("ready:");
        }
        _output.AppendLine("  ret ptr %instance");
        _output.AppendLine("}");

        string UnboxArgument(string parameterType, string argument, int parameterIndex)
        {
            if (parameterType.EndsWith("?", StringComparison.Ordinal) && IsNullableValueRuntimeType(parameterType[..^1]))
            {
                var underlying = parameterType[..^1];
                var isNull = $"%argument.{parameterIndex}.nullable";
                _output.AppendLine($"  {isNull} = icmp eq ptr {argument}, null");
                var nullLabel = $"argument.{parameterIndex}.nullable.null";
                var valueLabel = $"argument.{parameterIndex}.nullable.present";
                var endLabel = $"argument.{parameterIndex}.nullable.end";
                _output.AppendLine($"  br i1 {isNull}, label %{nullLabel}, label %{valueLabel}");
                _output.AppendLine(valueLabel + ":");
                var decoded = UnboxArgument(underlying, argument, parameterIndex);
                var inner = $"%argument.{parameterIndex}.nullable.with.value";
                var withTag = $"%argument.{parameterIndex}.nullable.with.tag";
                _output.AppendLine($"  {withTag} = insertvalue {LlvmType(parameterType)} zeroinitializer, i1 true, 0");
                _output.AppendLine($"  {inner} = insertvalue {LlvmType(parameterType)} {withTag}, {LlvmType(underlying)} {decoded}, 1");
                _output.AppendLine($"  br label %{endLabel}");
                _output.AppendLine(nullLabel + ":");
                _output.AppendLine($"  br label %{endLabel}");
                _output.AppendLine(endLabel + ":");
                var result = $"%argument.{parameterIndex}.nullable.value";
                _output.AppendLine($"  {result} = phi {LlvmType(parameterType)} [ zeroinitializer, %{nullLabel} ], [ {inner}, %{valueLabel} ]");
                return result;
            }
            if (IsRuntimeReferenceType(parameterType) || parameterType.EndsWith("?", StringComparison.Ordinal) && IsRuntimeReferenceType(parameterType[..^1])) return argument;
            if (compilation.StructTypes.ContainsKey(parameterType))
            {
                var payload = $"%argument.{parameterIndex}.payload.address";
                var result = $"%argument.{parameterIndex}.payload";
                _output.AppendLine($"  {payload} = getelementptr %luma.structbox.{Safe(parameterType)}, ptr {argument}, i32 0, i32 4");
                _output.AppendLine($"  {result} = load %struct.{Safe(parameterType)}, ptr {payload}");
                return result;
            }
            var scalarPayload = $"%argument.{parameterIndex}.scalar.address";
            var scalar = $"%argument.{parameterIndex}.scalar";
            _output.AppendLine($"  {scalarPayload} = getelementptr %luma.box, ptr {argument}, i32 0, i32 4");
            _output.AppendLine($"  {scalar} = load i64, ptr {scalarPayload}");
            if (parameterType == "bool")
            {
                var result = $"%argument.{parameterIndex}.bool";
                _output.AppendLine($"  {result} = trunc i64 {scalar} to i1");
                return result;
            }
            if (parameterType == "float")
            {
                var bits = $"%argument.{parameterIndex}.float.bits";
                var result = $"%argument.{parameterIndex}.float";
                _output.AppendLine($"  {bits} = trunc i64 {scalar} to i32");
                _output.AppendLine($"  {result} = bitcast i32 {bits} to float");
                return result;
            }
            if (parameterType == "double")
            {
                var result = $"%argument.{parameterIndex}.double";
                _output.AppendLine($"  {result} = bitcast i64 {scalar} to double");
                return result;
            }
            var llvmType = LlvmType(parameterType);
            if (llvmType == "i64") return scalar;
            var converted = $"%argument.{parameterIndex}.converted";
            _output.AppendLine($"  {converted} = trunc i64 {scalar} to {llvmType}");
            return converted;
        }
    }

    private bool IsRuntimeReferenceType(string typeName) => typeName is "object" or "System.Object" or "string" ||
        compilation.ClassTypes.ContainsKey(typeName) || compilation.InterfaceTypes.ContainsKey(typeName) ||
        compilation.DelegateTypes.ContainsKey(typeName) || SemanticAnalyzer.TryParseManagedArrayType(typeName, out _, out _);

    private bool IsNullableValueRuntimeType(string typeName) => compilation.StructTypes.ContainsKey(typeName) ||
        compilation.EnumUnderlyingTypes.ContainsKey(typeName) ||
        typeName is "bool" or "char" or "int" or "uint" or "int8" or "uint8" or "int16" or "uint16" or "int32" or "uint32" or "int64" or "uint64" or "float" or "double";
}
