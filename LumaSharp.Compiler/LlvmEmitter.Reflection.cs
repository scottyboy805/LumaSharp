using System.Globalization;

namespace LumaSharp.Compiler;

internal sealed partial class LlvmEmitter
{
    private void EmitRuntimeReflectionMetadata()
    {
        EmitAttributeInstanceHandles();
        EmitAttributePointerArrays();
        EmitReflectionMemberHandles();
        EmitAttributeSpanIntrinsic();
        EmitTypeMemberSpanIntrinsic("luma_type_fields", "fields", "System.Reflection.Field");
        EmitTypeMemberSpanIntrinsic("luma_type_properties", "properties", "System.Reflection.Property");
        EmitTypeMemberSpanIntrinsic("luma_type_methods", "methods", "System.Reflection.Method");
        EmitTypeMemberSpanIntrinsic("luma_type_constructors", "constructors", "System.Reflection.Constructor");
        EmitMemberOperationIntrinsics();
    }

    private IEnumerable<(int Id, string TypeName, IReadOnlyList<RuntimeAttributeMetadata> Attributes)> AttributeOwners()
    {
        var typeId = 1;
        foreach (var pair in compilation.RuntimeTypes.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            yield return (typeId++, pair.Key, pair.Value.Attributes);
        }
        foreach (var member in compilation.RuntimeMembers.OrderBy(member => member.Id))
            yield return (member.Id, member.Kind + ":" + member.DeclaringType + "." + member.Name, member.Attributes ?? []);
    }

    private void EmitAttributeInstanceHandles()
    {
        var owners = AttributeOwners().ToArray();
        foreach (var owner in owners)
        {
            var ordered = owner.Attributes.Select((attribute, sourceIndex) => (Attribute: attribute, SourceIndex: sourceIndex))
                .OrderBy(item => item.Attribute.TypeName, StringComparer.Ordinal)
                .ThenBy(item => item.SourceIndex)
                .ToArray();
            for (var index = 0; index < ordered.Length; index++)
            {
                var attribute = ordered[index].Attribute;
                if (!compilation.ClassTypes.TryGetValue(attribute.TypeName, out var classType))
                    throw new InvalidOperationException($"Attribute type '{attribute.TypeName}' is not an emitted class.");
                var fieldValues = GetAttributeFieldValues(attribute);
                var values = classType.Fields.OrderBy(field => field.Index).Select(field =>
                {
                    var llvmType = LlvmType(field.TypeName);
                    var value = fieldValues.TryGetValue(field.Declaration.Name, out var assigned)
                        ? assigned
                        : DefaultStaticStorageValue(field.TypeName);
                    return $"{llvmType} {value}";
                }).ToArray();
                var suffix = values.Length == 0 ? "" : ", " + string.Join(", ", values);
                _output.AppendLine($"@luma_attribute_instance_{owner.Id}_{index} = internal constant %class.{Safe(attribute.TypeName)} {{ ptr @luma_classdesc_{Safe(attribute.TypeName)}, i32 -1, i32 1, ptr null{suffix} }}");
            }
        }
    }

    private Dictionary<string, string> GetAttributeFieldValues(RuntimeAttributeMetadata attribute)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        var parameters = attribute.Constructor.Declaration.Parameters;
        var parameterValues = parameters.Select((parameter, index) => (parameter.Name, Value: attribute.Arguments[index].LlvmLiteral))
            .ToDictionary(item => item.Name, item => item.Value, StringComparer.Ordinal);
        if (attribute.Constructor.Declaration.Body is not { } body) return values;
        foreach (var statement in body.Statements)
        {
            if (statement is not ExpressionStatement { Value: BinaryExpression { Operator: "=" } assignment }) continue;
            var fieldName = assignment.Left switch
            {
                NameExpression name => name.Name,
                MemberAccessExpression { Receiver: ThisExpression, Member: var member } => member,
                _ => null
            };
            if (fieldName is null || assignment.Right is not NameExpression sourceName ||
                !parameterValues.TryGetValue(sourceName.Name, out var fieldValue)) continue;
            values[fieldName] = fieldValue;
        }
        return values;
    }

    private void EmitAttributePointerArrays()
    {
        foreach (var owner in AttributeOwners())
        {
            var ordered = owner.Attributes.Select((attribute, sourceIndex) => (Attribute: attribute, SourceIndex: sourceIndex))
                .OrderBy(item => item.Attribute.TypeName, StringComparer.Ordinal)
                .ThenBy(item => item.SourceIndex)
                .ToArray();
            if (ordered.Length == 0) continue;
            var values = ordered.Select((item, index) => $"ptr @luma_attribute_instance_{owner.Id}_{index}");
            _output.AppendLine($"@luma_attribute_array_{owner.Id} = private constant [{ordered.Length} x ptr] [{string.Join(", ", values)}]");
        }
    }

    private void EmitReflectionMemberHandles()
    {
        foreach (var member in compilation.RuntimeMembers)
        {
            var memberType = "System.Reflection." + member.Kind;
            if (!compilation.ClassTypes.TryGetValue(memberType, out var memberClass))
                throw new InvalidOperationException($"Reflection metadata class '{memberType}' is unavailable.");
            if (!compilation.RuntimeTypes.ContainsKey(member.DeclaringType))
                throw new InvalidOperationException($"Declaring type metadata '{member.DeclaringType}' is unavailable.");
            var nameLiteral = compilation.StringLiteralPool.TryGetValue(member.Name, out var nameString)
                ? "@" + nameString.Symbol
                : "@luma_string_empty";
            var fields = memberClass.Fields.OrderBy(field => field.Index).Select(field =>
            {
                var value = field.Declaration.Name switch
                {
                    "name" => nameLiteral,
                    "declaringType" => "@luma_type_handle_" + Safe(member.DeclaringType),
                    "flags" => member.Flags.ToString(CultureInfo.InvariantCulture),
                    "memberId" => member.Id.ToString(CultureInfo.InvariantCulture),
                    _ => DefaultStaticStorageValue(field.TypeName)
                };
                return $"{LlvmType(field.TypeName)} {value}";
            }).ToArray();
            var suffix = fields.Length == 0 ? "" : ", " + string.Join(", ", fields);
            _output.AppendLine($"@luma_reflection_member_{member.Id} = internal constant %class.{Safe(memberType)} {{ ptr @luma_classdesc_{Safe(memberType)}, i32 -1, i32 1, ptr null{suffix} }}");
        }
    }

    private void EmitTypeMemberPointerArrays(string kind, string arrayKind, string elementType)
    {
        foreach (var typeName in compilation.RuntimeTypes.Keys.Order(StringComparer.Ordinal))
        {
            var members = GetTypeMembersForReflection(typeName, kind);
            if (members.Count == 0) continue;
            var name = ReflectionMemberArrayName(typeName, arrayKind);
            var handles = members.Select(member => $"ptr @luma_reflection_member_{member.Id}");
            _output.AppendLine($"@{name} = private constant [{members.Count} x ptr] [{string.Join(", ", handles)}]");
        }
    }

    private IReadOnlyList<RuntimeMemberMetadata> GetTypeMembersForReflection(string typeName, string kind)
    {
        var chain = new List<string>();
        var current = typeName;
        var visited = new HashSet<string>(StringComparer.Ordinal);
        while (visited.Add(current) && compilation.ClassTypes.TryGetValue(current, out var classType) && classType.BaseTypeName is { } baseType)
        {
            chain.Add(current);
            current = baseType;
        }
        chain.Add(current);
        chain.Reverse();
        if (!compilation.ClassTypes.ContainsKey(typeName)) chain = [typeName];
        var result = new List<RuntimeMemberMetadata>();
        foreach (var owner in chain)
        foreach (var member in compilation.RuntimeMembers.Where(member => member.Kind == kind && member.DeclaringType == owner))
            if (kind != "Constructor" || owner == typeName)
            {
                if (kind == "Field")
                    result.RemoveAll(existing => existing.Kind == kind && existing.Name == member.Name);
                else if (kind == "Property")
                    result.RemoveAll(existing => existing.Kind == kind && existing.Name == member.Name);
                else if (kind == "Method" && member.Method is { } method)
                    result.RemoveAll(existing => existing.Kind == kind && existing.Method is { } existingMethod &&
                        existingMethod.Declaration.Name == method.Declaration.Name &&
                        existingMethod.Declaration.Parameters.Select(parameter => parameter.TypeName)
                            .SequenceEqual(method.Declaration.Parameters.Select(parameter => parameter.TypeName), StringComparer.Ordinal));
                result.Add(member);
            }
        return result;
    }

    private static string ReflectionMemberArrayName(string typeName, string kind) =>
        "luma_reflection_" + kind + "_" + Safe(typeName);

    private void EmitAttributeSpanIntrinsic()
    {
        var spanName = ClosedReadOnlySpanName("System.Attribute");
        var spanType = LlvmType(spanName);
        var spanSymbol = compilation.StructTypes[spanName];
        var pointerIndex = spanSymbol.Fields.Single(field => field.Declaration.Name == "pointer").Index;
        var lengthIndex = spanSymbol.Fields.Single(field => field.Declaration.Name == "length").Index;
        var ownerIndex = spanSymbol.Fields.Single(field => field.Declaration.Name == "owner").Index;
        var typeClass = compilation.ClassTypes["System.Type"];
        var typeIdIndex = typeClass.Fields.Single(field => field.Declaration.Name == "typeId").Index + 4;
        var memberClass = compilation.ClassTypes["System.Reflection.MemberInfo"];
        var memberIdIndex = memberClass.Fields.Single(field => field.Declaration.Name == "memberId").Index + 4;
        var owners = AttributeOwners().Where(owner => owner.Attributes.Count > 0).ToArray();
        _output.AppendLine($"define internal {spanType} @luma_attribute_span(ptr %owner) {{");
        _output.AppendLine("entry:");
        _output.AppendLine("  %descriptor = load ptr, ptr %owner");
        _output.AppendLine("  %type.distance = call i32 @luma_type_class_distance(ptr %owner, ptr @luma_classdesc_System_Type)");
        _output.AppendLine("  %is.type = icmp sge i32 %type.distance, 0");
        _output.AppendLine("  br i1 %is.type, label %type.owner, label %check.member");
        _output.AppendLine("check.member:");
        _output.AppendLine("  %member.distance = call i32 @luma_type_class_distance(ptr %owner, ptr @luma_classdesc_System_Reflection_MemberInfo)");
        _output.AppendLine("  %is.member = icmp sge i32 %member.distance, 0");
        _output.AppendLine("  br i1 %is.member, label %member.owner, label %none");
        _output.AppendLine("type.owner:");
        _output.AppendLine($"  %type.id.address = getelementptr %class.System_Type, ptr %owner, i32 0, i32 {typeIdIndex}");
        _output.AppendLine("  %owner.id = load i32, ptr %type.id.address");
        _output.AppendLine("  br label %lookup");
        _output.AppendLine("member.owner:");
        _output.AppendLine($"  %member.id.address = getelementptr %class.System_Reflection_MemberInfo, ptr %owner, i32 0, i32 {memberIdIndex}");
        _output.AppendLine("  %member.owner.id = load i32, ptr %member.id.address");
        _output.AppendLine("  br label %lookup");
        _output.AppendLine("lookup:");
        _output.AppendLine("  %id = phi i32 [ %owner.id, %type.owner ], [ %member.owner.id, %member.owner ]");
        var cases = owners.Select((owner, index) => $"i32 {owner.Id}, label %attributes.{index}").ToArray();
        _output.AppendLine(cases.Length == 0
            ? "  br label %none"
            : $"  switch i32 %id, label %none [{string.Join(" ", cases)}]");
        for (var index = 0; index < owners.Length; index++)
        {
            var ordered = owners[index].Attributes.Select((attribute, sourceIndex) => (Attribute: attribute, SourceIndex: sourceIndex))
                .OrderBy(item => item.Attribute.TypeName, StringComparer.Ordinal)
                .ThenBy(item => item.SourceIndex)
                .ToArray();
            _output.AppendLine($"attributes.{index}:");
            EmitReadOnlySpanReturn(spanType, pointerIndex, lengthIndex, ownerIndex,
                $"@luma_attribute_array_{owners[index].Id}", ordered.Length, $"attribute.{index}");
        }
        _output.AppendLine("none:");
        _output.AppendLine($"  ret {spanType} zeroinitializer");
        _output.AppendLine("}");
    }

    private void EmitTypeMemberSpanIntrinsic(string functionName, string arrayKind, string elementType)
    {
        var spanSourceName = ClosedReadOnlySpanName(elementType);
        var spanType = LlvmType(spanSourceName);
        var spanSymbol = compilation.StructTypes[spanSourceName];
        var pointerIndex = spanSymbol.Fields.Single(field => field.Declaration.Name == "pointer").Index;
        var lengthIndex = spanSymbol.Fields.Single(field => field.Declaration.Name == "length").Index;
        var ownerIndex = spanSymbol.Fields.Single(field => field.Declaration.Name == "owner").Index;
        var typeIdField = compilation.ClassTypes["System.Type"].Fields.Single(field => field.Declaration.Name == "typeId").Index + 4;
        var arrayElements = compilation.RuntimeTypes.Keys.Order(StringComparer.Ordinal)
            .Select((typeName, index) => (TypeName: typeName, Id: index + 1, Members: GetTypeMembersForReflection(typeName, arrayKind switch
            {
                "fields" => "Field",
                "properties" => "Property",
                "methods" => "Method",
                "constructors" => "Constructor",
                _ => throw new InvalidOperationException($"Unknown reflection member collection '{arrayKind}'.")
            })))
            .Where(item => item.Members.Count > 0)
            .ToArray();
        var kind = arrayKind switch
        {
            "fields" => "Field",
            "properties" => "Property",
            "methods" => "Method",
            "constructors" => "Constructor",
            _ => throw new InvalidOperationException($"Unknown reflection member collection '{arrayKind}'.")
        };
        foreach (var item in arrayElements)
        {
            var name = ReflectionMemberArrayName(item.TypeName, arrayKind);
            var values = item.Members.Select(member => $"ptr @luma_reflection_member_{member.Id}");
            _output.AppendLine($"@{name} = private constant [{item.Members.Count} x ptr] [{string.Join(", ", values)}]");
        }
        _output.AppendLine($"define internal {spanType} @{functionName}(ptr %type) {{");
        _output.AppendLine("entry:");
        _output.AppendLine("  %type.is.null = icmp eq ptr %type, null");
        _output.AppendLine("  br i1 %type.is.null, label %null, label %lookup");
        _output.AppendLine("null:");
        _output.AppendLine("  call void @luma_throw_null()");
        _output.AppendLine("  unreachable");
        _output.AppendLine("lookup:");
        _output.AppendLine($"  %type.id.address = getelementptr %class.System_Type, ptr %type, i32 0, i32 {typeIdField}");
        _output.AppendLine("  %type.id = load i32, ptr %type.id.address");
        var cases = arrayElements.Select((item, index) => $"i32 {item.Id}, label %members.{index}").ToArray();
        _output.AppendLine(cases.Length == 0
            ? "  br label %none"
            : $"  switch i32 %type.id, label %none [{string.Join(" ", cases)}]");
        for (var index = 0; index < arrayElements.Length; index++)
        {
            var item = arrayElements[index];
            _output.AppendLine($"members.{index}:");
            EmitReadOnlySpanReturn(spanType, pointerIndex, lengthIndex, ownerIndex,
                "@" + ReflectionMemberArrayName(item.TypeName, arrayKind), item.Members.Count, $"{kind}.{index}");
        }
        _output.AppendLine("none:");
        _output.AppendLine($"  ret {spanType} zeroinitializer");
        _output.AppendLine("}");
    }

    private string ClosedReadOnlySpanName(string elementType)
    {
        var qualifiedName = "System." + GenericTypeSpecializer.GetSpecializedName("ReadOnlySpan", [elementType]);
        if (!compilation.StructTypes.ContainsKey(qualifiedName))
            throw new InvalidOperationException($"Closed System.ReadOnlySpan<{elementType}> specialization '{qualifiedName}' was not emitted.");
        return qualifiedName;
    }

    private void EmitReadOnlySpanReturn(string spanType, int pointerIndex, int lengthIndex, int ownerIndex,
        string arrayGlobal, int length, string suffix)
    {
        var arrayType = $"[{length} x ptr]";
        var pointer = $"%span.pointer.{suffix}";
        _output.AppendLine($"  {pointer} = getelementptr inbounds {arrayType}, ptr {arrayGlobal}, i32 0, i32 0");
        var withPointer = $"%span.with.pointer.{suffix}";
        var withLength = $"%span.with.length.{suffix}";
        var result = $"%span.result.{suffix}";
        _output.AppendLine($"  {withPointer} = insertvalue {spanType} zeroinitializer, ptr {pointer}, {pointerIndex}");
        _output.AppendLine($"  {withLength} = insertvalue {spanType} {withPointer}, i32 {length}, {lengthIndex}");
        _output.AppendLine($"  {result} = insertvalue {spanType} {withLength}, ptr null, {ownerIndex}");
        _output.AppendLine($"  ret {spanType} {result}");
    }

    private void EmitMemberOperationIntrinsics()
    {
        EmitReflectionBoxHelpers();
        EmitReflectionUnboxHelpers();
        foreach (var member in compilation.RuntimeMembers)
        {
            switch (member.Kind)
            {
                case "Field":
                    EmitReflectionFieldGetter(member);
                    if ((member.Flags & 64) != 0) EmitReflectionFieldSetter(member);
                    break;
                case "Property":
                    if (member.Property?.Getter is { } getter) EmitReflectionPropertyGetter(member, getter);
                    if ((member.Flags & 64) != 0 && member.Property?.Setter is { } setter) EmitReflectionPropertySetter(member, setter);
                    break;
                case "Method":
                case "Constructor":
                    EmitReflectionMethodInvoker(member);
                    break;
            }
        }
        EmitReflectionMemberDispatch("get", "ptr", "luma_reflection_member_get", compilation.RuntimeMembers
            .Where(member => member.Kind == "Field" || member.Kind == "Property" && (member.Flags & 32) != 0)
            .Select(member => (member, $"luma_reflection_get_{member.Id}")));
        EmitReflectionMemberDispatch("set", "void", "luma_reflection_member_set", compilation.RuntimeMembers
            .Where(member => member.Kind == "Field" && (member.Flags & 64) != 0 || member.Kind == "Property" && (member.Flags & 64) != 0)
            .Select(member => (member, $"luma_reflection_set_{member.Id}")));
        EmitReflectionMemberDispatch("invoke", "ptr", "luma_reflection_member_invoke", compilation.RuntimeMembers
            .Where(member => member.Kind is "Method" or "Constructor")
            .Select(member => (member, $"luma_reflection_invoke_{member.Id}")));
    }

    private void EmitReflectionMemberDispatch(string operation, string returnType, string name,
        IEnumerable<(RuntimeMemberMetadata Member, string Adapter)> adapters)
    {
        var entries = adapters.ToArray();
        var memberIdIndex = compilation.ClassTypes["System.Reflection.MemberInfo"].Fields
            .Single(field => field.Declaration.Name == "memberId").Index + 4;
        var parameters = operation switch
        {
            "get" => "ptr %instance",
            "set" => "ptr %instance, ptr %value",
            _ => "ptr %instance, ptr %arguments"
        };
        _output.AppendLine($"define internal {returnType} @{name}(ptr %member, {parameters}) {{");
        _output.AppendLine("entry:");
        _output.AppendLine("  %member.is.null = icmp eq ptr %member, null");
        _output.AppendLine("  br i1 %member.is.null, label %invalid, label %lookup");
        _output.AppendLine("lookup:");
        _output.AppendLine("  %member.distance = call i32 @luma_type_class_distance(ptr %member, ptr @luma_classdesc_System_Reflection_MemberInfo)");
        _output.AppendLine("  %member.valid = icmp sge i32 %member.distance, 0");
        _output.AppendLine("  br i1 %member.valid, label %member.lookup, label %invalid");
        _output.AppendLine("member.lookup:");
        _output.AppendLine($"  %member.id.address = getelementptr %class.System_Reflection_MemberInfo, ptr %member, i32 0, i32 {memberIdIndex}");
        _output.AppendLine("  %member.id = load i32, ptr %member.id.address");
        var cases = entries.Select((entry, index) => $"i32 {entry.Member.Id}, label %member.{index}").ToArray();
        _output.AppendLine(cases.Length == 0 ? "  br label %invalid" : $"  switch i32 %member.id, label %invalid [{string.Join(" ", cases)}]");
        for (var index = 0; index < entries.Length; index++)
        {
            _output.AppendLine($"member.{index}:");
            var callArguments = operation switch
            {
                "get" => "ptr %instance, ptr %member",
                "set" => "ptr %instance, ptr %value, ptr %member",
                _ => "ptr %instance, ptr %arguments, ptr %member"
            };
            var resultName = $"%result.{index}";
            _output.AppendLine($"  {(returnType == "void" ? "" : resultName + " = ")}call {returnType} @{entries[index].Adapter}({callArguments})");
            if (returnType != "void") _output.AppendLine($"  ret {returnType} {resultName}");
            else _output.AppendLine("  ret void");
        }
        _output.AppendLine("invalid:");
        _output.AppendLine("  call void @luma_raise_type_new_failure()");
        _output.AppendLine(returnType == "void" ? "  ret void" : $"  ret {returnType} null");
        _output.AppendLine("}");
    }

    private void EmitReflectionFieldGetter(RuntimeMemberMetadata member)
    {
        var type = member.ValueType;
        var owner = member.DeclaringType;
        var safe = Safe(owner);
        _output.AppendLine($"define internal ptr @luma_reflection_get_{member.Id}(ptr %instance, ptr %member) {{");
        _output.AppendLine("entry:");
        if (!IsReflectionSupportedType(type))
        {
            EmitReflectionFailureReturn("ptr");
            _output.AppendLine("}");
            return;
        }
        string address;
        if (member.StaticField is { } staticField)
        {
            EmitReflectionTypeInitialization(owner, $"field.{member.Id}.initializer", "ptr");
            address = "@" + staticField.GlobalName;
        }
        else if (member.Field is { } field)
        {
            EmitReflectionInstanceCheck(owner, "%instance", $"field.{member.Id}.access", "ptr");
            if (compilation.ClassTypes.ContainsKey(owner))
            {
                address = $"%field.{member.Id}.address";
                _output.AppendLine($"field.{member.Id}.access:");
                _output.AppendLine($"  {address} = getelementptr %class.{safe}, ptr %instance, i32 0, i32 {field.Index + 4}");
            }
            else
            {
                _output.AppendLine($"field.{member.Id}.access:");
                _output.AppendLine($"  %field.{member.Id}.payload = getelementptr %luma.structbox.{safe}, ptr %instance, i32 0, i32 4");
                address = $"%field.{member.Id}.address";
                _output.AppendLine($"  {address} = getelementptr %struct.{safe}, ptr %field.{member.Id}.payload, i32 0, i32 {field.Index}");
            }
        }
        else
        {
            EmitReflectionFailureReturn("ptr");
            _output.AppendLine("}");
            return;
        }
        var value = $"%field.{member.Id}.value";
        _output.AppendLine($"  {value} = load {LlvmType(type)}, ptr {address}");
        var boxed = EmitReflectionBoxValue(type, value, $"field.{member.Id}");
        _output.AppendLine($"  ret ptr {boxed}");
        _output.AppendLine("}");
    }

    private void EmitReflectionFieldSetter(RuntimeMemberMetadata member)
    {
        var owner = member.DeclaringType;
        _output.AppendLine($"define internal void @luma_reflection_set_{member.Id}(ptr %instance, ptr %value, ptr %member) {{");
        _output.AppendLine("entry:");
        if (!IsReflectionSupportedType(member.ValueType))
        {
            EmitReflectionFailureReturn("void");
            _output.AppendLine("}");
            return;
        }
        string address;
        if (member.StaticField is { } staticField)
        {
            EmitReflectionTypeInitialization(owner, $"set.{member.Id}.initializer", "void");
            address = "@" + staticField.GlobalName;
        }
        else if (member.Field is { } field)
        {
            EmitReflectionInstanceCheck(owner, "%instance", $"set.{member.Id}.access", "void");
            if (compilation.ClassTypes.ContainsKey(owner))
            {
                address = $"%field.{member.Id}.address";
                _output.AppendLine($"set.{member.Id}.access:");
                _output.AppendLine($"  {address} = getelementptr %class.{Safe(owner)}, ptr %instance, i32 0, i32 {field.Index + 4}");
            }
            else
            {
                _output.AppendLine($"set.{member.Id}.access:");
                _output.AppendLine($"  %field.{member.Id}.payload = getelementptr %luma.structbox.{Safe(owner)}, ptr %instance, i32 0, i32 4");
                address = $"%field.{member.Id}.address";
                _output.AppendLine($"  {address} = getelementptr %struct.{Safe(owner)}, ptr %field.{member.Id}.payload, i32 0, i32 {field.Index}");
            }
        }
        else
        {
            EmitReflectionFailureReturn("void");
            _output.AppendLine("}");
            return;
        }
        var unboxed = EmitReflectionUnboxValue(member.ValueType, "%value", $"set.{member.Id}");
        EmitReflectionPendingCheck("void", $"set.{member.Id}.valid");
        EmitReflectionStoreValue(member.ValueType, address, unboxed, $"set.{member.Id}");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");
    }

    private void EmitReflectionPropertyGetter(RuntimeMemberMetadata member, MethodSymbol getter)
    {
        _output.AppendLine($"define internal ptr @luma_reflection_get_{member.Id}(ptr %instance, ptr %member) {{");
        _output.AppendLine("entry:");
        if (!IsReflectionSupportedType(member.ValueType))
        {
            EmitReflectionFailureReturn("ptr");
            _output.AppendLine("}");
            return;
        }
        var receiver = EmitReflectionReceiver(member, getter, $"property.{member.Id}", "ptr");
        var value = EmitReflectionCall(getter, receiver, [], $"property.{member.Id}", "ptr");
        if (value is null)
        {
            EmitReflectionFailureReturn("ptr");
        }
        else
        {
            EmitReflectionPendingCheck("ptr", $"property.{member.Id}.result");
            var boxed = EmitReflectionBoxValue(member.ValueType, value, $"property.{member.Id}");
            _output.AppendLine($"  ret ptr {boxed}");
        }
        _output.AppendLine("}");
    }

    private void EmitReflectionPropertySetter(RuntimeMemberMetadata member, MethodSymbol setter)
    {
        _output.AppendLine($"define internal void @luma_reflection_set_{member.Id}(ptr %instance, ptr %value, ptr %member) {{");
        _output.AppendLine("entry:");
        if (!IsReflectionSupportedType(member.ValueType))
        {
            EmitReflectionFailureReturn("void");
            _output.AppendLine("}");
            return;
        }
        var argument = EmitReflectionUnboxValue(member.ValueType, "%value", $"property.set.{member.Id}");
        EmitReflectionPendingCheck("void", $"property.set.{member.Id}.valid");
        var receiver = EmitReflectionReceiver(member, setter, $"property.set.{member.Id}", "void");
        _ = EmitReflectionCall(setter, receiver, [$"{LlvmType(member.ValueType)} {argument}"], $"property.set.{member.Id}", "void");
        EmitReflectionPendingCheck("void", $"property.set.{member.Id}.result");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");
    }

    private void EmitReflectionMethodInvoker(RuntimeMemberMetadata member)
    {
        _output.AppendLine($"define internal ptr @luma_reflection_invoke_{member.Id}(ptr %instance, ptr %arguments, ptr %member) {{");
        _output.AppendLine("entry:");
        if (member.Kind == "Constructor")
        {
            var constructor = member.Method;
            if (constructor is null || !CanInvokeMethod(constructor))
            {
                EmitReflectionFailureReturn("ptr");
                _output.AppendLine("}");
                return;
            }
            EmitReflectionArgumentCountCheck(constructor.Declaration.Parameters.Count, $"constructor.{member.Id}");
            _output.AppendLine($"  %created = call ptr @luma_type_new(ptr @luma_type_handle_{Safe(member.DeclaringType)}, ptr %arguments)");
            EmitReflectionPendingCheck("ptr", $"constructor.{member.Id}.created");
            _output.AppendLine("  ret ptr %created");
            _output.AppendLine("}");
            return;
        }
        if (member.Method is not { } method || !CanInvokeMethod(method))
        {
            EmitReflectionFailureReturn("ptr");
            _output.AppendLine("}");
            return;
        }
        EmitReflectionArgumentCountCheck(method.Declaration.Parameters.Count, $"method.{member.Id}");
        var unboxed = EmitReflectionUnboxedArguments(method, member.Id);
        EmitReflectionPendingCheck("ptr", $"method.{member.Id}.arguments");
        var receiver = EmitReflectionReceiver(member, method, $"method.{member.Id}", "ptr");
        var callArguments = unboxed.Select((value, index) => $"{LlvmType(method.Declaration.Parameters[index].TypeName)} {value}").ToArray();
        var result = EmitReflectionCall(method, receiver, callArguments, $"method.{member.Id}", "ptr");
        if (method.Declaration.ReturnType == "void")
        {
            EmitReflectionPendingCheck("ptr", $"method.{member.Id}.result");
            _output.AppendLine("  ret ptr null");
            _output.AppendLine("}");
            return;
        }
        if (result is null)
        {
            _output.AppendLine("  ret ptr null");
            _output.AppendLine("}");
            return;
        }
        EmitReflectionPendingCheck("ptr", $"method.{member.Id}.result");
        var boxed = EmitReflectionBoxValue(method.Declaration.ReturnType, result, $"method.{member.Id}");
        _output.AppendLine($"  ret ptr {boxed}");
        _output.AppendLine("}");
    }

    private bool CanInvokeMethod(MethodSymbol method) =>
        (method.Declaration.Body is not null ||
            method.Owner.Kind == "interface" && FindInterfaceMethodSlot(method) is not null ||
            method.Declaration.IsVirtual && FindVirtualMethodSlot(method) is not null) &&
        !method.Declaration.ReturnsByRef && !method.Declaration.IsOperator && !method.Declaration.HasTypeParameters &&
        (method.Declaration.ReturnType == "void" || IsReflectionSupportedType(method.Declaration.ReturnType)) &&
        method.Declaration.Parameters.All(parameter => parameter.PassingMode.Length == 0 && !parameter.IsVariadic &&
            !parameter.IsScoped && IsReflectionSupportedType(parameter.TypeName)) &&
        (!(method.IsClassConstructor || method.IsStructConstructor) || method.Declaration.Parameters.All(parameter => parameter.DefaultValue is null));

    private bool IsReflectionSupportedType(string type)
    {
        if (!IsClosedReflectionType(type)) return false;
        var baseType = type.EndsWith("?", StringComparison.Ordinal) ? type[..^1] : type;
        return IsRuntimeReferenceType(baseType) || IsReflectionBoxableValueType(baseType);
    }

    private bool IsClosedReflectionType(string type)
    {
        var baseType = type.EndsWith("?", StringComparison.Ordinal) ? type[..^1] : type;
        if (SemanticAnalyzer.TryParseManagedArrayType(baseType, out var elementType, out _))
            return IsClosedReflectionType(elementType);
        if (baseType.EndsWith('*')) return false;
        return baseType is "void" or "bool" or "char" or "int" or "uint" or "int8" or "uint8" or
            "int16" or "uint16" or "int32" or "uint32" or "int64" or "uint64" or "float" or "double" or
            "string" or "object" or "System.Object" || compilation.ClassTypes.ContainsKey(baseType) ||
            compilation.StructTypes.ContainsKey(baseType) || compilation.InterfaceTypes.ContainsKey(baseType) ||
            compilation.DelegateTypes.ContainsKey(baseType) || compilation.EnumUnderlyingTypes.ContainsKey(baseType);
    }

    private List<string> EmitReflectionUnboxedArguments(MethodSymbol method, int memberId)
    {
        var result = new List<string>(method.Declaration.Parameters.Count);
        if (method.Declaration.Parameters.Count == 0) return result;
        _output.AppendLine($"  %arguments.data.{memberId} = getelementptr %luma.array, ptr %arguments, i32 0, i32 7");
        for (var index = 0; index < method.Declaration.Parameters.Count; index++)
        {
            var type = method.Declaration.Parameters[index].TypeName;
            _output.AppendLine($"  %argument.address.{memberId}.{index} = getelementptr ptr, ptr %arguments.data.{memberId}, i32 {index}");
            _output.AppendLine($"  %argument.object.{memberId}.{index} = load ptr, ptr %argument.address.{memberId}.{index}");
            var value = $"%argument.value.{memberId}.{index}";
            _output.AppendLine($"  {value} = call {LlvmType(type)} @luma_reflection_unbox_{Safe(type)}(ptr %argument.object.{memberId}.{index})");
            result.Add(value);
        }
        return result;
    }

    private void EmitReflectionArgumentCountCheck(int count, string prefix)
    {
        _output.AppendLine("  %arguments.is.null = icmp eq ptr %arguments, null");
        _output.AppendLine("  br i1 %arguments.is.null, label %arguments.none, label %arguments.present");
        _output.AppendLine("arguments.none:");
        _output.AppendLine("  br label %arguments.count.ready");
        _output.AppendLine("arguments.present:");
        _output.AppendLine("  %arguments.length.address = getelementptr %luma.array, ptr %arguments, i32 0, i32 4");
        _output.AppendLine("  %arguments.length = load i32, ptr %arguments.length.address");
        _output.AppendLine("  br label %arguments.count.ready");
        _output.AppendLine("arguments.count.ready:");
        _output.AppendLine("  %actual.argument.count = phi i32 [ 0, %arguments.none ], [ %arguments.length, %arguments.present ]");
        _output.AppendLine($"  %argument.count.matches = icmp eq i32 %actual.argument.count, {count}");
        _output.AppendLine("  br i1 %argument.count.matches, label %arguments.valid, label %arguments.invalid");
        _output.AppendLine("arguments.invalid:");
        _output.AppendLine("  call void @luma_raise_type_new_failure()");
        _output.AppendLine("  ret ptr null");
        _output.AppendLine("arguments.valid:");
        if (count > 0)
        {
            _output.AppendLine("  %arguments.is.missing = icmp eq ptr %arguments, null");
            _output.AppendLine("  br i1 %arguments.is.missing, label %arguments.invalid, label %arguments.data.ready");
            _output.AppendLine("arguments.data.ready:");
        }
    }

    private string? EmitReflectionReceiver(RuntimeMemberMetadata member, MethodSymbol method, string prefix, string adapterReturnType)
    {
        if (method.Declaration.IsStatic)
        {
            EmitReflectionTypeInitialization(member.DeclaringType, $"{prefix}.initializer", adapterReturnType);
            return null;
        }
        EmitReflectionInstanceCheck(member.DeclaringType, "%instance", $"{prefix}.receiver", adapterReturnType);
        if (method.IsStructInstanceMethod)
        {
            var payload = $"%{Safe(prefix)}.receiver.payload";
            _output.AppendLine($"{prefix}.receiver:");
            _output.AppendLine($"  {payload} = getelementptr %luma.structbox.{Safe(member.DeclaringType)}, ptr %instance, i32 0, i32 4");
            return payload;
        }
        _output.AppendLine($"{prefix}.receiver:");
        return "%instance";
    }

    private string? EmitReflectionCall(MethodSymbol method, string? receiver, IReadOnlyList<string> arguments, string prefix, string adapterReturnType)
    {
        var callArguments = new List<string>();
        if (receiver is not null) callArguments.Add($"ptr {receiver}");
        callArguments.AddRange(arguments);
        var returnType = method.Declaration.ReturnsByRef ? "ptr" : LlvmType(method.Declaration.ReturnType);
        var result = method.Declaration.ReturnType == "void" ? null : $"%{Safe(prefix)}.call.result";
        var callTarget = "@" + FunctionName(method, compilation.Methods);
        var ownerName = (string.IsNullOrEmpty(method.Owner.Namespace) ? method.Owner.Name : method.Owner.Namespace + "." + method.Owner.Name);
        if (receiver is not null && method.IsClassInstanceMethod &&
            (method.Declaration.IsVirtual || method.Declaration.IsOverride) && FindVirtualMethodSlot(method) is { } virtualSlot)
        {
            _output.AppendLine($"  %{Safe(prefix)}.descriptor = load ptr, ptr {receiver}");
            _output.AppendLine($"  %{Safe(prefix)}.vtable.address = getelementptr %luma.typedesc, ptr %{Safe(prefix)}.descriptor, i32 0, i32 2");
            _output.AppendLine($"  %{Safe(prefix)}.vtable = load ptr, ptr %{Safe(prefix)}.vtable.address");
            _output.AppendLine($"  %{Safe(prefix)}.entry.address = getelementptr ptr, ptr %{Safe(prefix)}.vtable, i32 {virtualSlot}");
            _output.AppendLine($"  %{Safe(prefix)}.function = load ptr, ptr %{Safe(prefix)}.entry.address");
            _output.AppendLine($"  %{Safe(prefix)}.function.null = icmp eq ptr %{Safe(prefix)}.function, null");
            _output.AppendLine($"  br i1 %{Safe(prefix)}.function.null, label %{Safe(prefix)}.failure, label %{Safe(prefix)}.call");
            _output.AppendLine($"{Safe(prefix)}.failure:");
            _output.AppendLine("  call void @luma_raise_type_new_failure()");
            _output.AppendLine(adapterReturnType == "void" ? "  ret void" : $"  ret {adapterReturnType} null");
            _output.AppendLine($"{Safe(prefix)}.call:");
            callTarget = $"%{Safe(prefix)}.function";
        }
        else if (receiver is not null && method.Owner.Kind == "interface" && FindInterfaceMethodSlot(method) is { } interfaceSlot)
        {
            _output.AppendLine($"  %{Safe(prefix)}.descriptor = load ptr, ptr {receiver}");
            _output.AppendLine($"  %{Safe(prefix)}.itable.address = getelementptr %luma.typedesc, ptr %{Safe(prefix)}.descriptor, i32 0, i32 6");
            _output.AppendLine($"  %{Safe(prefix)}.itable = load ptr, ptr %{Safe(prefix)}.itable.address");
            _output.AppendLine($"  %{Safe(prefix)}.entry.address = getelementptr ptr, ptr %{Safe(prefix)}.itable, i32 {interfaceSlot}");
            _output.AppendLine($"  %{Safe(prefix)}.function = load ptr, ptr %{Safe(prefix)}.entry.address");
            _output.AppendLine($"  %{Safe(prefix)}.function.null = icmp eq ptr %{Safe(prefix)}.function, null");
            _output.AppendLine($"  br i1 %{Safe(prefix)}.function.null, label %{Safe(prefix)}.failure, label %{Safe(prefix)}.call");
            _output.AppendLine($"{Safe(prefix)}.failure:");
            _output.AppendLine("  call void @luma_raise_type_new_failure()");
            _output.AppendLine(adapterReturnType == "void" ? "  ret void" : $"  ret {adapterReturnType} null");
            _output.AppendLine($"{Safe(prefix)}.call:");
            callTarget = $"%{Safe(prefix)}.function";
        }
        _output.AppendLine($"  {(result is null ? "" : result + " = ")}call {returnType} {callTarget}({string.Join(", ", callArguments)})");
        return result;
    }

    private int? FindVirtualMethodSlot(MethodSymbol method)
    {
        var ownerName = (string.IsNullOrEmpty(method.Owner.Namespace) ? method.Owner.Name : method.Owner.Namespace + "." + method.Owner.Name);
        if (!compilation.ClassTypes.ContainsKey(ownerName)) return null;
        var methods = VirtualDispatchLayout.GetMethods(ownerName, compilation.ClassTypes, compilation.Methods);
        for (var index = 0; index < methods.Count; index++)
            if (SameMethodSignature(methods[index], method)) return index;
        return null;
    }

    private int? FindInterfaceMethodSlot(MethodSymbol method)
    {
        var ownerName = (string.IsNullOrEmpty(method.Owner.Namespace) ? method.Owner.Name : method.Owner.Namespace + "." + method.Owner.Name);
        return compilation.InterfaceMethodSlots.FirstOrDefault(slot =>
            (string.IsNullOrEmpty(slot.InterfaceType.Namespace) ? slot.InterfaceType.Name : slot.InterfaceType.Namespace + "." + slot.InterfaceType.Name) == ownerName &&
            SameMethodSignature(slot.Contract, method))?.Index;
    }

    private static bool SameMethodSignature(MethodSymbol left, MethodSymbol right) =>
        left.Declaration.Name == right.Declaration.Name && left.Declaration.ReturnType == right.Declaration.ReturnType &&
        left.Declaration.Parameters.Select(parameter => parameter.TypeName).SequenceEqual(
            right.Declaration.Parameters.Select(parameter => parameter.TypeName), StringComparer.Ordinal);

    private void EmitReflectionInstanceCheck(string owner, string instance, string successLabel, string resultType)
    {
        var ownerBase = owner.EndsWith("?", StringComparison.Ordinal) ? owner[..^1] : owner;
        var prefix = Safe(successLabel);
        _output.AppendLine($"  %{prefix}.is.null = icmp eq ptr {instance}, null");
        _output.AppendLine($"  br i1 %{prefix}.is.null, label %{successLabel}.failure, label %{successLabel}.check");
        _output.AppendLine($"{successLabel}.check:");
        if (compilation.StructTypes.ContainsKey(ownerBase))
        {
            _output.AppendLine($"  %{prefix}.descriptor = load ptr, ptr {instance}");
            _output.AppendLine($"  %{prefix}.valid = icmp eq ptr %{prefix}.descriptor, @luma_boxdesc_{Safe(ownerBase)}");
        }
        else if (compilation.InterfaceTypes.ContainsKey(ownerBase))
        {
            _output.AppendLine($"  %{prefix}.valid = call i1 @luma_class_implements(ptr {instance}, ptr @luma_interfacedesc_{Safe(ownerBase)})");
        }
        else
        {
            _output.AppendLine($"  %{prefix}.distance = call i32 @luma_type_class_distance(ptr {instance}, ptr @luma_classdesc_{Safe(ownerBase)})");
            _output.AppendLine($"  %{prefix}.valid = icmp sge i32 %{prefix}.distance, 0");
        }
        _output.AppendLine($"  br i1 %{prefix}.valid, label %{successLabel}, label %{successLabel}.failure");
        _output.AppendLine($"{successLabel}.failure:");
        _output.AppendLine("  call void @luma_raise_type_new_failure()");
        _output.AppendLine(resultType == "void" ? "  ret void" : $"  ret {resultType} null");
    }

    private void EmitReflectionTypeInitialization(string typeName, string prefix, string adapterReturnType)
    {
        if (_typeInitializationFunctions.TryGetValue(typeName, out var initializer))
        {
            _output.AppendLine($"  call void @{initializer}()");
            EmitReflectionPendingCheck(adapterReturnType, prefix);
        }
    }

    private void EmitReflectionPendingCheck(string resultType, string successLabel)
    {
        if (!compilation.UsesExceptionHandling) return;
        var prefix = Safe(successLabel);
        _output.AppendLine($"  %{prefix}.pending.exception = load ptr, ptr @luma_pending_exception");
        _output.AppendLine($"  %{prefix}.has.exception = icmp ne ptr %{prefix}.pending.exception, null");
        _output.AppendLine($"  br i1 %{prefix}.has.exception, label %{prefix}.failure, label %{prefix}");
        _output.AppendLine($"{Safe(successLabel)}.failure:");
        _output.AppendLine(resultType == "void" ? "  ret void" : $"  ret {resultType} null");
        _output.AppendLine($"{Safe(successLabel)}:");
    }

    private string EmitReflectionUnboxValue(string type, string value, string prefix)
    {
        var result = $"%{Safe(prefix)}.unboxed";
        _output.AppendLine($"  {result} = call {LlvmType(type)} @luma_reflection_unbox_{Safe(type)}(ptr {value})");
        return result;
    }

    private void EmitReflectionFailureReturn(string resultType)
    {
        _output.AppendLine("  call void @luma_raise_type_new_failure()");
        _output.AppendLine(resultType == "void" ? "  ret void" : $"  ret {resultType} null");
    }

    private void EmitReflectionStoreValue(string type, string address, string value, string prefix)
    {
        var baseType = type.EndsWith("?", StringComparison.Ordinal) ? type[..^1] : type;
        if (IsRuntimeReferenceType(baseType))
        {
            EmitReflectionRetain(baseType, value);
            var old = $"%{Safe(prefix)}.old";
            _output.AppendLine($"  {old} = load ptr, ptr {address}");
            EmitReflectionRelease(baseType, old);
            _output.AppendLine($"  store ptr {value}, ptr {address}");
        }
        else if (AggregateTypeContainsManagedReferences(baseType))
        {
            var newAddress = $"%{Safe(prefix)}.new.value.address";
            _output.AppendLine($"  {newAddress} = alloca {LlvmType(type)}");
            _output.AppendLine($"  store {LlvmType(type)} {value}, ptr {newAddress}");
            _output.AppendLine($"  call void @{AggregateOwnershipFunctionName(type, retain: true)}(ptr {newAddress})");
            _output.AppendLine($"  call void @{AggregateOwnershipFunctionName(type, retain: false)}(ptr {address})");
            _output.AppendLine($"  store {LlvmType(type)} {value}, ptr {address}");
        }
        else _output.AppendLine($"  store {LlvmType(type)} {value}, ptr {address}");
    }

    private void EmitReflectionRetain(string type, string value)
    {
        var function = type == "string" || type == "System.String" ? "luma_string_retain" :
            SemanticAnalyzer.TryParseManagedArrayType(type, out _, out _) ? "luma_array_retain" : "luma_class_retain_dynamic";
        _output.AppendLine($"  call void @{function}(ptr {value})");
    }

    private void EmitReflectionRelease(string type, string value)
    {
        var function = type == "string" || type == "System.String" ? "luma_string_release" :
            SemanticAnalyzer.TryParseManagedArrayType(type, out _, out _) ? "luma_array_release" : "luma_class_release_dynamic";
        _output.AppendLine($"  call void @{function}(ptr {value})");
    }

    private string EmitReflectionBoxValue(string type, string value, string prefix)
    {
        var baseType = type.EndsWith("?", StringComparison.Ordinal) ? type[..^1] : type;
        if (IsRuntimeReferenceType(baseType))
        {
            EmitReflectionRetain(baseType, value);
            return value;
        }
        var result = $"%{Safe(prefix)}.boxed";
        _output.AppendLine($"  {result} = call ptr @luma_reflection_box_{Safe(type)}({LlvmType(type)} {value})");
        return result;
    }

    private IEnumerable<string> ReflectionValueTypes()
    {
        var types = compilation.RuntimeMembers.Select(member => member.ValueType)
            .Concat(compilation.RuntimeMembers.SelectMany(member => member.Method?.Declaration.Parameters.Select(parameter => parameter.TypeName) ?? []))
            .Where(type => IsReflectionBoxableValueType(type))
            .Concat(compilation.RuntimeMembers.Select(member => member.ValueType)
                .Concat(compilation.RuntimeMembers.SelectMany(member => member.Method?.Declaration.Parameters.Select(parameter => parameter.TypeName) ?? []))
                .Select(type => type.EndsWith("?", StringComparison.Ordinal) ? type[..^1] : type)
                .Where(IsReflectionBoxableValueType))
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal);
        return types;
    }

    private bool IsReflectionBoxableValueType(string type)
    {
        var baseType = type.EndsWith("?", StringComparison.Ordinal) ? type[..^1] : type;
        return compilation.StructTypes.TryGetValue(baseType, out var structure) && !structure.Declaration.IsRefStruct || compilation.EnumUnderlyingTypes.ContainsKey(baseType) ||
            baseType is "bool" or "char" or "int" or "uint" or "int8" or "uint8" or "int16" or "uint16" or "int32" or "uint32" or "int64" or "uint64" or "float" or "double";
    }

    private void EmitReflectionBoxHelpers()
    {
        foreach (var type in ReflectionValueTypes())
        {
            if (compilation.StructTypes.ContainsKey(type) && compilation.StructTypes[type].Declaration.IsRefStruct) continue;
            var llvmType = LlvmType(type);
            var sizeType = compilation.NativePointerSize == 8 ? "i64" : "i32";
            var structValue = compilation.StructTypes.ContainsKey(type);
            var boxType = structValue ? $"%luma.structbox.{Safe(type)}" : "%luma.box";
            _output.AppendLine($"define internal ptr @luma_reflection_box_{Safe(type)}({llvmType} %value) {{");
            _output.AppendLine("entry:");
            if (type.EndsWith("?", StringComparison.Ordinal))
            {
                var inner = type[..^1];
                _output.AppendLine("  %has.value = extractvalue " + llvmType + " %value, 0");
                _output.AppendLine("  br i1 %has.value, label %present, label %absent");
                _output.AppendLine("present:");
                _output.AppendLine($"  %inner.value = extractvalue {llvmType} %value, 1");
                _output.AppendLine($"  %boxed.inner = call ptr @luma_reflection_box_{Safe(inner)}({LlvmType(inner)} %inner.value)");
                _output.AppendLine("  br label %complete");
                _output.AppendLine("absent:");
                _output.AppendLine("  br label %complete");
                _output.AppendLine("complete:");
                _output.AppendLine("  %boxed = phi ptr [ %boxed.inner, %present ], [ null, %absent ]");
                _output.AppendLine("  ret ptr %boxed");
                _output.AppendLine("}");
                continue;
            }
            _output.AppendLine($"  %size.address = getelementptr {boxType}, ptr null, i32 1");
            _output.AppendLine($"  %size = ptrtoint ptr %size.address to {sizeType}");
            _output.AppendLine($"  %box = call ptr @malloc({sizeType} %size)");
            _output.AppendLine("  %allocation.failed = icmp eq ptr %box, null");
            _output.AppendLine("  br i1 %allocation.failed, label %out.of.memory, label %initialize");
            _output.AppendLine("out.of.memory:");
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
            _output.AppendLine($"  %descriptor.address = getelementptr {boxType}, ptr %box, i32 0, i32 0");
            _output.AppendLine($"  store ptr @luma_boxdesc_{Safe(type)}, ptr %descriptor.address");
            _output.AppendLine($"  %count.address = getelementptr {boxType}, ptr %box, i32 0, i32 1");
            _output.AppendLine("  store i32 1, ptr %count.address");
            _output.AppendLine($"  %flags.address = getelementptr {boxType}, ptr %box, i32 0, i32 2");
            _output.AppendLine("  store i32 0, ptr %flags.address");
            _output.AppendLine($"  %weak.address = getelementptr {boxType}, ptr %box, i32 0, i32 3");
            _output.AppendLine("  store ptr null, ptr %weak.address");
            _output.AppendLine($"  %payload = getelementptr {boxType}, ptr %box, i32 0, i32 4");
            if (structValue)
            {
                if (AggregateTypeContainsManagedReferences(type))
                {
                    _output.AppendLine("  %value.address = alloca " + llvmType);
                    _output.AppendLine($"  store {llvmType} %value, ptr %value.address");
                    _output.AppendLine($"  call void @{AggregateOwnershipFunctionName(type, retain: true)}(ptr %value.address)");
                }
                _output.AppendLine($"  store {llvmType} %value, ptr %payload");
            }
            else
            {
                var scalarType = compilation.EnumUnderlyingTypes.GetValueOrDefault(type, type);
                var encoded = "%encoded";
                if (scalarType == "bool") _output.AppendLine($"  {encoded} = zext {llvmType} %value to i64");
                else if (scalarType == "float")
                {
                    _output.AppendLine("  %encoded.bits = bitcast float %value to i32");
                    _output.AppendLine("  %encoded = zext i32 %encoded.bits to i64");
                }
                else if (scalarType == "double") _output.AppendLine("  %encoded = bitcast double %value to i64");
                else if (llvmType == "i64") _output.AppendLine("  %encoded = add i64 %value, 0");
                else _output.AppendLine($"  {encoded} = {(scalarType.StartsWith("u", StringComparison.Ordinal) || scalarType == "char" ? "zext" : "sext")} {llvmType} %value to i64");
                _output.AppendLine("  store i64 %encoded, ptr %payload");
            }
            _output.AppendLine("  ret ptr %box");
            _output.AppendLine("}");
        }
    }

    private void EmitReflectionUnboxHelpers()
    {
        foreach (var type in ReflectionValueTypes())
        {
            var llvmType = LlvmType(type);
            var nullable = type.EndsWith("?", StringComparison.Ordinal);
            var baseType = nullable ? type[..^1] : type;
            _output.AppendLine($"define internal {llvmType} @luma_reflection_unbox_{Safe(type)}(ptr %object) {{");
            _output.AppendLine("entry:");
            _output.AppendLine("  %is.null = icmp eq ptr %object, null");
            if (nullable)
            {
                _output.AppendLine("  br i1 %is.null, label %null.value, label %validate");
                _output.AppendLine("validate:");
            }
            else
            {
                _output.AppendLine("  br i1 %is.null, label %invalid, label %validate");
                _output.AppendLine("validate:");
            }
            var descriptor = RuntimeTypeDescriptor(baseType);
            _output.AppendLine("  %descriptor = load ptr, ptr %object");
            _output.AppendLine($"  %type.matches = icmp eq ptr %descriptor, {descriptor ?? "null"}");
            _output.AppendLine("  br i1 %type.matches, label %load.value, label %invalid");
            _output.AppendLine("load.value:");
            string value;
            if (compilation.StructTypes.ContainsKey(baseType))
            {
                _output.AppendLine($"  %payload.address = getelementptr %luma.structbox.{Safe(baseType)}, ptr %object, i32 0, i32 4");
                _output.AppendLine($"  %payload = load %struct.{Safe(baseType)}, ptr %payload.address");
                value = "%payload";
            }
            else
            {
                _output.AppendLine("  %payload.address = getelementptr %luma.box, ptr %object, i32 0, i32 4");
                _output.AppendLine("  %payload = load i64, ptr %payload.address");
                var scalarType = compilation.EnumUnderlyingTypes.GetValueOrDefault(baseType, baseType);
                if (scalarType == "bool") _output.AppendLine("  %decoded = trunc i64 %payload to i1");
                else if (scalarType == "float")
                {
                    _output.AppendLine("  %decoded.bits = trunc i64 %payload to i32");
                    _output.AppendLine("  %decoded = bitcast i32 %decoded.bits to float");
                }
                else if (scalarType == "double") _output.AppendLine("  %decoded = bitcast i64 %payload to double");
                else if (LlvmType(baseType) == "i64") _output.AppendLine("  %decoded = add i64 %payload, 0");
                else _output.AppendLine($"  %decoded = trunc i64 %payload to {LlvmType(baseType)}");
                value = "%decoded";
            }
            if (nullable)
            {
                _output.AppendLine($"  %present.tag = insertvalue {llvmType} zeroinitializer, i1 true, 0");
                _output.AppendLine($"  %present.value = insertvalue {llvmType} %present.tag, {LlvmType(baseType)} {value}, 1");
                _output.AppendLine("  br label %complete");
                _output.AppendLine("null.value:");
                _output.AppendLine("  br label %complete");
                _output.AppendLine("complete:");
                _output.AppendLine($"  %result = phi {llvmType} [ %present.value, %load.value ], [ zeroinitializer, %null.value ]");
                _output.AppendLine($"  ret {llvmType} %result");
            }
            else _output.AppendLine($"  ret {llvmType} {value}");
            _output.AppendLine("invalid:");
            _output.AppendLine("  call void @luma_raise_type_new_failure()");
            _output.AppendLine($"  ret {llvmType} zeroinitializer");
            _output.AppendLine("}");
        }
        foreach (var type in ReflectionReferenceTypes()) EmitReflectionReferenceUnboxHelper(type);
    }

    private IEnumerable<string> ReflectionReferenceTypes() => compilation.RuntimeMembers.Select(member => member.ValueType)
        .Concat(compilation.RuntimeMembers.SelectMany(member => member.Method?.Declaration.Parameters.Select(parameter => parameter.TypeName) ?? []))
        .Where(type => type != "void" && IsClosedReflectionType(type) && IsRuntimeReferenceType(type.EndsWith("?", StringComparison.Ordinal) ? type[..^1] : type))
        .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal);

    private void EmitReflectionReferenceUnboxHelper(string type)
    {
        var nullable = type.EndsWith("?", StringComparison.Ordinal);
        var baseType = nullable ? type[..^1] : type;
        _output.AppendLine($"define internal ptr @luma_reflection_unbox_{Safe(type)}(ptr %object) {{");
        _output.AppendLine("entry:");
        _output.AppendLine("  %is.null = icmp eq ptr %object, null");
        if (nullable)
            _output.AppendLine("  br i1 %is.null, label %valid, label %check");
        else _output.AppendLine("  br i1 %is.null, label %invalid, label %check");
        _output.AppendLine("check:");
        if (baseType is "System.Object" or "object")
            _output.AppendLine("  br label %valid");
        else if (compilation.InterfaceTypes.ContainsKey(baseType))
        {
            _output.AppendLine($"  %matches = call i1 @luma_class_implements(ptr %object, ptr @luma_interfacedesc_{Safe(baseType)})");
            _output.AppendLine("  br i1 %matches, label %valid, label %invalid");
        }
        else if (compilation.ClassTypes.ContainsKey(baseType))
        {
            _output.AppendLine($"  %distance = call i32 @luma_type_class_distance(ptr %object, ptr @luma_classdesc_{Safe(baseType)})");
            _output.AppendLine("  %matches = icmp sge i32 %distance, 0");
            _output.AppendLine("  br i1 %matches, label %valid, label %invalid");
        }
        else
        {
            var descriptor = RuntimeTypeDescriptor(baseType);
            _output.AppendLine("  %descriptor = load ptr, ptr %object");
            _output.AppendLine($"  %matches = icmp eq ptr %descriptor, {descriptor ?? "null"}");
            _output.AppendLine("  br i1 %matches, label %valid, label %invalid");
        }
        _output.AppendLine("valid:");
        _output.AppendLine("  ret ptr %object");
        _output.AppendLine("invalid:");
        _output.AppendLine("  call void @luma_raise_type_new_failure()");
        _output.AppendLine("  ret ptr null");
        _output.AppendLine("}");
    }
}
