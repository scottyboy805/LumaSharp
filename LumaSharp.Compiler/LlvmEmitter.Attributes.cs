namespace LumaSharp.Compiler;

internal sealed partial class LlvmEmitter
{
    private void EmitTypeAttributeRuntime()
    {
        var typeIdField = compilation.ClassTypes["System.Type"].Fields
            .Single(field => field.Declaration.Name == "typeId").Index + 4;
        var attributedTypes = new List<(int Id, string TypeName, IReadOnlyList<RuntimeAttributeMetadata> Attributes)>();
        var typeId = 1;
        foreach (var pair in compilation.RuntimeTypes.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            if (pair.Value.Attributes.Count > 0)
                attributedTypes.Add((typeId, pair.Key, pair.Value.Attributes));
            typeId++;
        }

        foreach (var owner in attributedTypes)
        for (var attributeIndex = 0; attributeIndex < owner.Attributes.Count; attributeIndex++)
            EmitTypeAttributeFactory(owner.Id, attributeIndex, owner.Attributes[attributeIndex]);

        _output.AppendLine("define internal i32 @luma_type_attribute_count(ptr %type) {");
        _output.AppendLine("entry:");
        EmitTypeMetadataIdLoad(typeIdField, "count");
        if (attributedTypes.Count == 0)
        {
            _output.AppendLine("  ret i32 0");
        }
        else
        {
            var countCases = attributedTypes.Select((item, index) => $"i32 {item.Id}, label %count.{index}").ToArray();
            _output.AppendLine($"  switch i32 %type.id, label %none [{string.Join(" ", countCases)}]");
            for (var index = 0; index < attributedTypes.Count; index++)
            {
                _output.AppendLine($"count.{index}:");
                _output.AppendLine($"  ret i32 {attributedTypes[index].Attributes.Count}");
            }
            _output.AppendLine("none:");
            _output.AppendLine("  ret i32 0");
        }
        _output.AppendLine("}");

        _output.AppendLine("define internal ptr @luma_type_attribute_type(ptr %type, i32 %index) {");
        _output.AppendLine("entry:");
        EmitTypeMetadataIdLoad(typeIdField, "attribute.type");
        if (attributedTypes.Count > 0)
        {
            var typeCases = attributedTypes.Select((item, index) => $"i32 {item.Id}, label %owner.{index}").ToArray();
            _output.AppendLine($"  switch i32 %type.id, label %none [{string.Join(" ", typeCases)}]");
            for (var ownerIndex = 0; ownerIndex < attributedTypes.Count; ownerIndex++)
            {
                var item = attributedTypes[ownerIndex];
                var attributeCases = item.Attributes.Select((_, index) => $"i32 {index}, label %attribute.{ownerIndex}.{index}").ToArray();
                _output.AppendLine($"owner.{ownerIndex}:");
                _output.AppendLine($"  switch i32 %index, label %none [{string.Join(" ", attributeCases)}]");
                for (var attributeIndex = 0; attributeIndex < item.Attributes.Count; attributeIndex++)
                {
                    _output.AppendLine($"attribute.{ownerIndex}.{attributeIndex}:");
                    _output.AppendLine($"  ret ptr @luma_type_handle_{Safe(item.Attributes[attributeIndex].TypeName)}");
                }
            }
        }
        else _output.AppendLine("  br label %none");
        _output.AppendLine("none:");
        _output.AppendLine("  ret ptr null");
        _output.AppendLine("}");

        _output.AppendLine("define internal ptr @luma_type_attribute(ptr %type, i32 %index) {");
        _output.AppendLine("entry:");
        EmitTypeMetadataIdLoad(typeIdField, "attribute.value");
        if (attributedTypes.Count > 0)
        {
            var typeCases = attributedTypes.Select((item, index) => $"i32 {item.Id}, label %attribute.owner.{index}").ToArray();
            _output.AppendLine($"  switch i32 %type.id, label %none [{string.Join(" ", typeCases)}]");
            for (var ownerIndex = 0; ownerIndex < attributedTypes.Count; ownerIndex++)
            {
                var item = attributedTypes[ownerIndex];
                var attributeCases = item.Attributes.Select((_, index) => $"i32 {index}, label %attribute.create.{ownerIndex}.{index}").ToArray();
                _output.AppendLine($"attribute.owner.{ownerIndex}:");
                _output.AppendLine($"  switch i32 %index, label %none [{string.Join(" ", attributeCases)}]");
                for (var attributeIndex = 0; attributeIndex < item.Attributes.Count; attributeIndex++)
                {
                    _output.AppendLine($"attribute.create.{ownerIndex}.{attributeIndex}:");
                    _output.AppendLine($"  %attribute.instance.{ownerIndex}.{attributeIndex} = call ptr @luma_type_attribute_create_{item.Id}_{attributeIndex}()");
                    _output.AppendLine($"  ret ptr %attribute.instance.{ownerIndex}.{attributeIndex}");
                }
            }
        }
        else _output.AppendLine("  br label %none");
        _output.AppendLine("none:");
        _output.AppendLine("  ret ptr null");
        _output.AppendLine("}");

        _output.AppendLine("define internal i1 @luma_type_has_attribute(ptr %type, ptr %attribute.type) {");
        _output.AppendLine("entry:");
        EmitTypeMetadataIdLoad(typeIdField, "has.attribute");
        _output.AppendLine("  %query.is.null = icmp eq ptr %attribute.type, null");
        _output.AppendLine("  br i1 %query.is.null, label %has.none, label %has.lookup");
        _output.AppendLine("has.lookup:");
        if (attributedTypes.Count > 0)
        {
            var typeCases = attributedTypes.Select((item, index) => $"i32 {item.Id}, label %has.owner.{index}").ToArray();
            _output.AppendLine($"  switch i32 %type.id, label %has.none [{string.Join(" ", typeCases)}]");
            for (var ownerIndex = 0; ownerIndex < attributedTypes.Count; ownerIndex++)
            {
                var attributes = attributedTypes[ownerIndex].Attributes;
                _output.AppendLine($"has.owner.{ownerIndex}:");
                for (var attributeIndex = 0; attributeIndex < attributes.Count; attributeIndex++)
                    _output.AppendLine($"  %has.match.{ownerIndex}.{attributeIndex} = icmp eq ptr %attribute.type, @luma_type_handle_{Safe(attributes[attributeIndex].TypeName)}");
                var matches = Enumerable.Range(0, attributes.Count).Select(index => $"%has.match.{ownerIndex}.{index}").ToArray();
                var combined = matches[0];
                for (var index = 1; index < matches.Length; index++)
                {
                    var next = $"%has.match.or.{ownerIndex}.{index}";
                    _output.AppendLine($"  {next} = or i1 {combined}, {matches[index]}");
                    combined = next;
                }
                _output.AppendLine($"  ret i1 {combined}");
            }
        }
        else _output.AppendLine("  br label %has.none");
        _output.AppendLine("has.none:");
        _output.AppendLine("  ret i1 false");
        _output.AppendLine("}");
    }

    private void EmitTypeAttributeFactory(int ownerId, int attributeIndex, RuntimeAttributeMetadata attribute)
    {
        var typeName = attribute.TypeName;
        if (!compilation.ClassTypes.TryGetValue(typeName, out var classType))
            throw new InvalidOperationException($"Attribute type '{typeName}' is not an emitted class.");
        var safeType = Safe(typeName);
        var factoryName = $"luma_type_attribute_create_{ownerId}_{attributeIndex}";
        var sizeType = compilation.NativePointerSize == 8 ? "i64" : "i32";

        _output.AppendLine($"define internal ptr @{factoryName}() {{");
        _output.AppendLine("entry:");
        if (_typeInitializationFunctions.TryGetValue(typeName, out var typeInitializer))
            _output.AppendLine($"  call void @{typeInitializer}()");
        _output.AppendLine($"  %size.address = getelementptr %class.{safeType}, ptr null, i32 1");
        _output.AppendLine($"  %size = ptrtoint ptr %size.address to {sizeType}");
        _output.AppendLine($"  %instance = call ptr @malloc({sizeType} %size)");
        _output.AppendLine("  %out.of.memory = icmp eq ptr %instance, null");
        _output.AppendLine("  br i1 %out.of.memory, label %oom, label %initialize");
        _output.AppendLine("oom:");
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
        _output.AppendLine("initialize:");
        _output.AppendLine($"  %descriptor = getelementptr %class.{safeType}, ptr %instance, i32 0, i32 0");
        _output.AppendLine($"  store ptr @luma_classdesc_{safeType}, ptr %descriptor");
        _output.AppendLine($"  %reference.count = getelementptr %class.{safeType}, ptr %instance, i32 0, i32 1");
        _output.AppendLine("  store i32 1, ptr %reference.count");
        _output.AppendLine($"  %reference.flags = getelementptr %class.{safeType}, ptr %instance, i32 0, i32 2");
        _output.AppendLine("  store i32 0, ptr %reference.flags");
        _output.AppendLine($"  %weak.control = getelementptr %class.{safeType}, ptr %instance, i32 0, i32 3");
        _output.AppendLine("  store ptr null, ptr %weak.control");
        foreach (var field in classType.Fields)
        {
            _output.AppendLine($"  %field.{field.Index} = getelementptr %class.{safeType}, ptr %instance, i32 0, i32 {field.Index + 4}");
            _output.AppendLine($"  store {LlvmType(field.TypeName)} {DefaultStaticStorageValue(field.TypeName)}, ptr %field.{field.Index}");
        }

        var callArguments = new List<string> { "ptr %instance" };
        if (attribute.Arguments.Count != attribute.Constructor.Declaration.Parameters.Count)
            throw new InvalidOperationException($"Attribute constructor metadata for '{typeName}' has inconsistent argument count.");
        callArguments.AddRange(attribute.Arguments.Select(argument => $"{LlvmType(argument.TypeName)} {argument.LlvmLiteral}"));
        callArguments.Add("i1 true");
        _output.AppendLine($"  call void @{FunctionName(attribute.Constructor, compilation.Methods)}({string.Join(", ", callArguments)})");

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
    }

    private void EmitTypeMetadataIdLoad(int typeIdField, string labelPrefix)
    {
        _output.AppendLine("  %is.null = icmp eq ptr %type, null");
        _output.AppendLine($"  br i1 %is.null, label %{labelPrefix}.null, label %{labelPrefix}.lookup");
        _output.AppendLine($"{labelPrefix}.null:");
        _output.AppendLine("  call void @luma_throw_null()");
        _output.AppendLine("  unreachable");
        _output.AppendLine($"{labelPrefix}.lookup:");
        _output.AppendLine($"  %type.id.address = getelementptr %class.System_Type, ptr %type, i32 0, i32 {typeIdField}");
        _output.AppendLine("  %type.id = load i32, ptr %type.id.address");
    }
}
