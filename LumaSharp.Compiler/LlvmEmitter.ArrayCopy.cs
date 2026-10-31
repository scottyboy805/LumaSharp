using System.Text;

namespace LumaSharp.Compiler;

internal sealed partial class LlvmEmitter
{
    private string ArrayCopyHelperName(string arrayType, bool clear) =>
        $"luma_array_{(clear ? "clear" : "copy")}_{Convert.ToHexString(Encoding.UTF8.GetBytes(arrayType))}";

    private IReadOnlyList<(string ArrayType, string ElementType)> GetConcreteArrayCopyTypes()
    {
        var genericParameters = compilation.Methods
            .SelectMany(method => (method.Owner.TypeParameters ?? []).Concat(method.Declaration.TypeParameters ?? []))
            .ToHashSet(StringComparer.Ordinal);
        return GetManagedArrayTypes()
            .Where(arrayType => SemanticAnalyzer.TryParseManagedArrayType(arrayType, out _, out var rank) && rank is >= 1 and <= 3)
            .Select(arrayType =>
            {
                SemanticAnalyzer.TryParseManagedArrayType(arrayType, out var elementType, out _);
                return (ArrayType: arrayType, ElementType: elementType);
            })
            .Where(pair => !ContainsUnboundArrayElementParameter(pair.ElementType, genericParameters))
            .OrderBy(pair => pair.ArrayType, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool ContainsUnboundArrayElementParameter(string type, IReadOnlySet<string> genericParameters)
    {
        if (genericParameters.Contains(type)) return true;
        return SemanticAnalyzer.TryParseManagedArrayType(type, out var nestedElementType, out _) &&
            ContainsUnboundArrayElementParameter(nestedElementType, genericParameters);
    }

    private void EmitArrayCopyRuntime()
    {
        var sizeType = compilation.NativePointerSize == 8 ? "i64" : "i32";
        var intrinsicBits = compilation.NativePointerSize * 8;
        var nativeMemoryRuntimeWillDeclareIntrinsics = compilation.CallTargets.Values.Any(method => method.Intrinsic?.Family == "memory");
        if (!nativeMemoryRuntimeWillDeclareIntrinsics)
        {
            AppendDeclarationIfMissing($"declare void @llvm.memmove.p0.p0.i{intrinsicBits}(ptr, ptr, {sizeType}, i1)");
            AppendDeclarationIfMissing($"declare void @llvm.memset.p0.i{intrinsicBits}(ptr, i8, {sizeType}, i1)");
        }

        _output.AppendLine("define internal i1 @luma_intrinsic_arraysametype(ptr %source, ptr %destination) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %source.type.address = getelementptr %luma.array, ptr %source, i32 0, i32 0");
        _output.AppendLine("  %source.type = load ptr, ptr %source.type.address");
        _output.AppendLine("  %destination.type.address = getelementptr %luma.array, ptr %destination, i32 0, i32 0");
        _output.AppendLine("  %destination.type = load ptr, ptr %destination.type.address");
        _output.AppendLine("  %same.type = icmp eq ptr %source.type, %destination.type");
        _output.AppendLine("  ret i1 %same.type");
        _output.AppendLine("}");

        var arrayTypes = GetConcreteArrayCopyTypes();
        foreach (var (arrayType, elementType) in arrayTypes)
        {
            EmitArrayCopyTypeHelper(arrayType, elementType);
            EmitArrayClearTypeHelper(arrayType, elementType);
        }
        EmitArrayCopyDispatcher(arrayTypes);
        EmitArrayClearDispatcher(arrayTypes);
    }

    private void EmitArrayCopyDispatcher(IReadOnlyList<(string ArrayType, string ElementType)> arrayTypes)
    {
        _output.AppendLine("define internal void @luma_intrinsic_arraycopy(ptr %source, i32 %source.index, ptr %destination, i32 %destination.index, i32 %length) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %type.address = getelementptr %luma.array, ptr %source, i32 0, i32 0");
        _output.AppendLine("  %type = load ptr, ptr %type.address");
        if (arrayTypes.Count == 0)
        {
            _output.AppendLine("  call void @luma_throw_bounds()");
            _output.AppendLine("  unreachable");
        }
        else
        {
            _output.AppendLine("  br label %check.0");
            for (var index = 0; index < arrayTypes.Count; index++)
            {
                var next = index + 1 == arrayTypes.Count ? "invalid" : $"check.{index + 1}";
                _output.AppendLine(index == 0 ? "check.0:" : $"check.{index}:");
                _output.AppendLine($"  %matches.{index} = icmp eq ptr %type, {ArrayDescriptorPointer(arrayTypes[index].ArrayType)}");
                _output.AppendLine($"  br i1 %matches.{index}, label %copy.{index}, label %{next}");
                _output.AppendLine($"copy.{index}:");
                _output.AppendLine($"  call void @{ArrayCopyHelperName(arrayTypes[index].ArrayType, clear: false)}(ptr %source, i32 %source.index, ptr %destination, i32 %destination.index, i32 %length)");
                _output.AppendLine("  ret void");
            }
            _output.AppendLine("invalid:");
            _output.AppendLine("  call void @luma_throw_bounds()");
            _output.AppendLine("  unreachable");
        }
        _output.AppendLine("}");
    }

    private void EmitArrayClearDispatcher(IReadOnlyList<(string ArrayType, string ElementType)> arrayTypes)
    {
        _output.AppendLine("define internal void @luma_intrinsic_arrayclear(ptr %array, i32 %index, i32 %length) {");
        _output.AppendLine("entry:");
        _output.AppendLine("  %type.address = getelementptr %luma.array, ptr %array, i32 0, i32 0");
        _output.AppendLine("  %type = load ptr, ptr %type.address");
        if (arrayTypes.Count == 0)
        {
            _output.AppendLine("  call void @luma_throw_bounds()");
            _output.AppendLine("  unreachable");
        }
        else
        {
            _output.AppendLine("  br label %check.0");
            for (var index = 0; index < arrayTypes.Count; index++)
            {
                var next = index + 1 == arrayTypes.Count ? "invalid" : $"check.{index + 1}";
                _output.AppendLine(index == 0 ? "check.0:" : $"check.{index}:");
                _output.AppendLine($"  %matches.{index} = icmp eq ptr %type, {ArrayDescriptorPointer(arrayTypes[index].ArrayType)}");
                _output.AppendLine($"  br i1 %matches.{index}, label %clear.{index}, label %{next}");
                _output.AppendLine($"clear.{index}:");
                _output.AppendLine($"  call void @{ArrayCopyHelperName(arrayTypes[index].ArrayType, clear: true)}(ptr %array, i32 %index, i32 %length)");
                _output.AppendLine("  ret void");
            }
            _output.AppendLine("invalid:");
            _output.AppendLine("  call void @luma_throw_bounds()");
            _output.AppendLine("  unreachable");
        }
        _output.AppendLine("}");
    }

    private bool TryGetArrayReferenceOperations(string elementType, out string retain, out string release, out bool aggregate)
    {
        aggregate = AggregateTypeContainsManagedReferences(elementType);
        if (aggregate)
        {
            retain = $"@{AggregateOwnershipFunctionName(elementType, retain: true)}";
            release = $"@{AggregateOwnershipFunctionName(elementType, retain: false)}";
            return true;
        }

        if (elementType is "string" or "System.String" or "string?" or "System.String?")
        {
            retain = "@luma_string_retain";
            release = "@luma_string_release";
            return true;
        }

        if (SemanticAnalyzer.TryParseManagedArrayType(elementType, out _, out _))
        {
            retain = "@luma_array_retain";
            release = "@luma_array_release";
            return true;
        }

        var referenceType = elementType.EndsWith("?", StringComparison.Ordinal) ? elementType[..^1] : elementType;
        if (compilation.ClassTypes.ContainsKey(referenceType) || compilation.InterfaceTypes.ContainsKey(referenceType) ||
            compilation.DelegateTypes.ContainsKey(referenceType) || referenceType is "object" or "System.Object")
        {
            retain = "@luma_class_retain_dynamic";
            release = "@luma_class_release_dynamic";
            return true;
        }

        retain = release = "";
        return false;
    }

    private void EmitArrayCopyTypeHelper(string arrayType, string elementType)
    {
        var name = ArrayCopyHelperName(arrayType, clear: false);
        var elementLlvmType = LlvmType(elementType);
        var sizeType = compilation.NativePointerSize == 8 ? "i64" : "i32";
        var intrinsicBits = compilation.NativePointerSize * 8;
        _output.AppendLine($"define internal void @{name}(ptr %source, i32 %source.index, ptr %destination, i32 %destination.index, i32 %length) {{");
        _output.AppendLine("entry:");
        _output.AppendLine("  %source.data = getelementptr %luma.array, ptr %source, i32 0, i32 7");
        _output.AppendLine("  %destination.data = getelementptr %luma.array, ptr %destination, i32 0, i32 7");
        _output.AppendLine("  %empty = icmp sle i32 %length, 0");
        _output.AppendLine("  br i1 %empty, label %done, label %copy.start");
        _output.AppendLine("copy.start:");
        if (!TryGetArrayReferenceOperations(elementType, out var retain, out var release, out var aggregate))
        {
            _output.AppendLine($"  %element.end = getelementptr {elementLlvmType}, ptr null, i32 1");
            _output.AppendLine($"  %element.size = ptrtoint ptr %element.end to {sizeType}");
            _output.AppendLine($"  %length.native = zext i32 %length to {sizeType}");
            _output.AppendLine($"  %byte.count = mul {sizeType} %length.native, %element.size");
            _output.AppendLine($"  %source.index.native = zext i32 %source.index to {sizeType}");
            _output.AppendLine($"  %source.offset = mul {sizeType} %source.index.native, %element.size");
            _output.AppendLine($"  %source.start = getelementptr i8, ptr %source.data, {sizeType} %source.offset");
            _output.AppendLine($"  %destination.index.native = zext i32 %destination.index to {sizeType}");
            _output.AppendLine($"  %destination.offset = mul {sizeType} %destination.index.native, %element.size");
            _output.AppendLine($"  %destination.start = getelementptr i8, ptr %destination.data, {sizeType} %destination.offset");
            _output.AppendLine($"  call void @llvm.memmove.p0.p0.i{intrinsicBits}(ptr %destination.start, ptr %source.start, {sizeType} %byte.count, i1 false)");
            _output.AppendLine("  br label %done");
        }
        else
        {
            _output.AppendLine("  %same.array = icmp eq ptr %source, %destination");
            _output.AppendLine("  %destination.after.source = icmp ugt i32 %destination.index, %source.index");
            _output.AppendLine("  %source.end = add i32 %source.index, %length");
            _output.AppendLine("  %destination.before.end = icmp ult i32 %destination.index, %source.end");
            _output.AppendLine("  %overlap = and i1 %same.array, %destination.after.source");
            _output.AppendLine("  %copy.backwards = and i1 %overlap, %destination.before.end");
            _output.AppendLine("  br i1 %copy.backwards, label %backward.start, label %forward.start");
            _output.AppendLine("forward.start:");
            _output.AppendLine("  br label %forward.loop");
            _output.AppendLine("forward.loop:");
            _output.AppendLine("  %forward.index = phi i32 [ 0, %forward.start ], [ %forward.next, %forward.body ]");
            _output.AppendLine("  %forward.done = icmp uge i32 %forward.index, %length");
            _output.AppendLine("  br i1 %forward.done, label %done, label %forward.body");
            _output.AppendLine("forward.body:");
            _output.AppendLine("  %forward.source.index = add i32 %source.index, %forward.index");
            _output.AppendLine("  %forward.destination.index = add i32 %destination.index, %forward.index");
            EmitArrayReferenceElementCopy(elementLlvmType, retain, release, aggregate, "%forward.source.index", "%forward.destination.index");
            _output.AppendLine("  %forward.next = add i32 %forward.index, 1");
            _output.AppendLine("  br label %forward.loop");
            _output.AppendLine("backward.start:");
            _output.AppendLine("  %backward.initial = sub i32 %length, 1");
            _output.AppendLine("  br label %backward.loop");
            _output.AppendLine("backward.loop:");
            _output.AppendLine("  %backward.index = phi i32 [ %backward.initial, %backward.start ], [ %backward.next, %backward.body ]");
            _output.AppendLine("  %backward.done = icmp slt i32 %backward.index, 0");
            _output.AppendLine("  br i1 %backward.done, label %done, label %backward.body");
            _output.AppendLine("backward.body:");
            _output.AppendLine("  %backward.source.index = add i32 %source.index, %backward.index");
            _output.AppendLine("  %backward.destination.index = add i32 %destination.index, %backward.index");
            EmitArrayReferenceElementCopy(elementLlvmType, retain, release, aggregate, "%backward.source.index", "%backward.destination.index");
            _output.AppendLine("  %backward.next = sub i32 %backward.index, 1");
            _output.AppendLine("  br label %backward.loop");
        }
        _output.AppendLine("done:");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");
    }

    private void EmitArrayReferenceElementCopy(string elementLlvmType, string retain, string release, bool aggregate,
        string sourceIndex, string destinationIndex)
    {
        _output.AppendLine($"  %source.element.{sourceIndex.Trim('%')} = getelementptr {elementLlvmType}, ptr %source.data, i32 {sourceIndex}");
        _output.AppendLine($"  %destination.element.{destinationIndex.Trim('%')} = getelementptr {elementLlvmType}, ptr %destination.data, i32 {destinationIndex}");
        var sourceAddress = $"%source.element.{sourceIndex.Trim('%')}";
        var destinationAddress = $"%destination.element.{destinationIndex.Trim('%')}";
        if (aggregate)
        {
            _output.AppendLine($"  call void {retain}(ptr {sourceAddress})");
            _output.AppendLine($"  %source.value.{sourceIndex.Trim('%')} = load {elementLlvmType}, ptr {sourceAddress}");
            _output.AppendLine($"  call void {release}(ptr {destinationAddress})");
            _output.AppendLine($"  store {elementLlvmType} %source.value.{sourceIndex.Trim('%')}, ptr {destinationAddress}");
        }
        else
        {
            _output.AppendLine($"  %source.value.{sourceIndex.Trim('%')} = load {elementLlvmType}, ptr {sourceAddress}");
            _output.AppendLine($"  call void {retain}(ptr %source.value.{sourceIndex.Trim('%')})");
            _output.AppendLine($"  %destination.old.{destinationIndex.Trim('%')} = load {elementLlvmType}, ptr {destinationAddress}");
            _output.AppendLine($"  call void {release}(ptr %destination.old.{destinationIndex.Trim('%')})");
            _output.AppendLine($"  store {elementLlvmType} %source.value.{sourceIndex.Trim('%')}, ptr {destinationAddress}");
        }
    }

    private void EmitArrayClearTypeHelper(string arrayType, string elementType)
    {
        var name = ArrayCopyHelperName(arrayType, clear: true);
        var elementLlvmType = LlvmType(elementType);
        var sizeType = compilation.NativePointerSize == 8 ? "i64" : "i32";
        var intrinsicBits = compilation.NativePointerSize * 8;
        _output.AppendLine($"define internal void @{name}(ptr %array, i32 %index, i32 %length) {{");
        _output.AppendLine("entry:");
        _output.AppendLine("  %data = getelementptr %luma.array, ptr %array, i32 0, i32 7");
        _output.AppendLine("  %empty = icmp sle i32 %length, 0");
        _output.AppendLine("  br i1 %empty, label %done, label %clear.start");
        _output.AppendLine("clear.start:");
        if (!TryGetArrayReferenceOperations(elementType, out _, out var release, out var aggregate))
        {
            _output.AppendLine($"  %element.end = getelementptr {elementLlvmType}, ptr null, i32 1");
            _output.AppendLine($"  %element.size = ptrtoint ptr %element.end to {sizeType}");
            _output.AppendLine($"  %length.native = zext i32 %length to {sizeType}");
            _output.AppendLine($"  %byte.count = mul {sizeType} %length.native, %element.size");
            _output.AppendLine($"  %index.native = zext i32 %index to {sizeType}");
            _output.AppendLine($"  %byte.offset = mul {sizeType} %index.native, %element.size");
            _output.AppendLine($"  %start = getelementptr i8, ptr %data, {sizeType} %byte.offset");
            _output.AppendLine($"  call void @llvm.memset.p0.i{intrinsicBits}(ptr %start, i8 0, {sizeType} %byte.count, i1 false)");
            _output.AppendLine("  br label %done");
        }
        else
        {
            _output.AppendLine("  br label %loop");
            _output.AppendLine("loop:");
            _output.AppendLine("  %offset = phi i32 [ 0, %clear.start ], [ %next, %body ]");
            _output.AppendLine("  %finished = icmp uge i32 %offset, %length");
            _output.AppendLine("  br i1 %finished, label %done, label %body");
            _output.AppendLine("body:");
            _output.AppendLine("  %element.index = add i32 %index, %offset");
            _output.AppendLine($"  %element.address = getelementptr {elementLlvmType}, ptr %data, i32 %element.index");
            if (aggregate)
            {
                _output.AppendLine($"  call void {release}(ptr %element.address)");
                _output.AppendLine($"  store {elementLlvmType} zeroinitializer, ptr %element.address");
            }
            else
            {
                _output.AppendLine($"  %old.value = load {elementLlvmType}, ptr %element.address");
                _output.AppendLine($"  call void {release}(ptr %old.value)");
                var defaultValue = elementType is "string" or "System.String" ? "@luma_string_empty" : "null";
                _output.AppendLine($"  store {elementLlvmType} {defaultValue}, ptr %element.address");
            }
            _output.AppendLine("  %next = add i32 %offset, 1");
            _output.AppendLine("  br label %loop");
        }
        _output.AppendLine("done:");
        _output.AppendLine("  ret void");
        _output.AppendLine("}");
    }
}
