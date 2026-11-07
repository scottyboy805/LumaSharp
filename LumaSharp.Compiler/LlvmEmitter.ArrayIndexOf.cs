using System.Text;

namespace LumaSharp.Compiler;

internal sealed partial class LlvmEmitter
{
    private static string ArrayIndexOfHelperName(string arrayType) =>
        "luma_array_indexof_" + Convert.ToHexString(Encoding.UTF8.GetBytes(arrayType));

    private void EmitArrayIndexOfRuntime(IReadOnlyList<(string ArrayType, string ElementType)> arrayTypes)
    {
        var hasVectorizedArray = arrayTypes.Any(pair => TryGetArrayIndexOfVectorSpec(pair.ElementType, out _, out _, out _));
        var vectorReductionWidths = arrayTypes
            .Select(pair => TryGetArrayIndexOfVectorSpec(pair.ElementType, out _, out var laneCount, out _) ? laneCount : 0)
            .Where(laneCount => laneCount >= 8)
            .Distinct();
        var bigEndian = hasVectorizedArray && IsArrayIndexOfBigEndianTarget();
        if (hasVectorizedArray)
        {
            AppendDeclarationIfMissing("declare i32 @llvm.cttz.i32(i32, i1 immarg)");
            AppendDeclarationIfMissing("declare i64 @llvm.cttz.i64(i64, i1 immarg)");
            AppendDeclarationIfMissing("declare i128 @llvm.cttz.i128(i128, i1 immarg)");
            if (bigEndian)
                AppendDeclarationIfMissing("declare i32 @llvm.bitreverse.i32(i32)");
        }
        foreach (var laneCount in vectorReductionWidths)
            AppendDeclarationIfMissing($"declare i1 @llvm.vector.reduce.or.v{laneCount}i1(<{laneCount} x i1>)");
        foreach (var (arrayType, elementType) in arrayTypes)
        {
            if (!SemanticAnalyzer.TryParseManagedArrayType(arrayType, out _, out var rank) || rank != 1) continue;
            if (!CanEmitArrayIndexOfTypeHelper(elementType)) continue;
            EmitArrayIndexOfTypeHelper(arrayType, elementType, bigEndian);
        }
    }

    private bool CanEmitArrayIndexOfTypeHelper(string elementType)
    {
        var valueType = elementType.EndsWith("?", StringComparison.Ordinal) ? elementType[..^1] : elementType;
        if (!compilation.StructTypes.ContainsKey(valueType)) return true;
        var equalitySlot = FindArrayIndexOfEqualitySlot(valueType);
        return FindArrayIndexOfStructEqualityImplementation(valueType, equalitySlot) is not null;
    }

    private void EmitArrayIndexOfTypeHelper(string arrayType, string elementType, bool bigEndianTarget)
    {
        var helperName = ArrayIndexOfHelperName(arrayType);
        var llvmElementType = LlvmType(elementType);
        var hasVectorPath = TryGetArrayIndexOfVectorSpec(elementType, out var vectorType, out var laneCount, out var laneMaskType);
        var valueType = elementType.EndsWith("?", StringComparison.Ordinal) ? elementType[..^1] : elementType;
        var hasNullablePrimitivePath = IsArrayIndexOfNullableValue(elementType) &&
            (IsArrayIndexOfPrimitive(valueType) || compilation.EnumUnderlyingTypes.ContainsKey(valueType));
        var bigEndian = hasVectorPath && bigEndianTarget;
        _output.AppendLine($"define internal i32 @{helperName}(ptr %array, i32 %start, i32 %count, {llvmElementType} %needle) {{");
        _output.AppendLine("entry:");
        _output.AppendLine("  %data = getelementptr %luma.array, ptr %array, i32 0, i32 7");
        _output.AppendLine("  %end = add i32 %start, %count");
        if (llvmElementType == "ptr" && !IsStringType(elementType) && FindArrayIndexOfEqualitySlot(elementType) is not null)
        {
            EmitArrayIndexOfReferenceEqualityHelper(elementType);
            _output.AppendLine("}");
            return;
        }
        _output.AppendLine(hasVectorPath ? "  br label %vector.loop" : hasNullablePrimitivePath ? "  br label %nullable.loop" : "  br label %scalar.preheader");
        if (hasVectorPath)
            EmitArrayIndexOfVectorLoop(vectorType, laneCount, laneMaskType, elementType == "bool", bigEndian);
        else if (hasNullablePrimitivePath)
            EmitArrayIndexOfNullablePrimitiveUnrolledLoop(valueType, llvmElementType);
        _output.AppendLine("scalar.preheader:");
        if (hasVectorPath)
            _output.AppendLine("  %scalar.start = phi i32 [ %vector.index, %vector.loop ]");
        else if (hasNullablePrimitivePath)
            _output.AppendLine("  %scalar.start = phi i32 [ %nullable.index, %nullable.loop ]");
        else
            _output.AppendLine("  %scalar.start = add i32 %start, 0");
        _output.AppendLine("  br label %loop");
        _output.AppendLine("loop:");
        _output.AppendLine("  %index = phi i32 [ %scalar.start, %scalar.preheader ], [ %next, %advance ]");
        _output.AppendLine("  %finished = icmp uge i32 %index, %end");
        _output.AppendLine("  br i1 %finished, label %missing, label %body");
        _output.AppendLine("body:");
        _output.AppendLine($"  %element.address = getelementptr {llvmElementType}, ptr %data, i32 %index");
        _output.AppendLine($"  %element = load {llvmElementType}, ptr %element.address, align 1");

        if (IsArrayIndexOfNullableValue(elementType))
        {
            _output.AppendLine($"  %element.has = extractvalue {llvmElementType} %element, 0");
            _output.AppendLine($"  %needle.has = extractvalue {llvmElementType} %needle, 0");
            var llvmValueType = LlvmType(valueType);
            _output.AppendLine($"  %element.payload = extractvalue {llvmElementType} %element, 1");
            _output.AppendLine($"  %needle.payload = extractvalue {llvmElementType} %needle, 1");
            if (IsArrayIndexOfPrimitive(valueType) || compilation.EnumUnderlyingTypes.ContainsKey(valueType))
            {
                _output.AppendLine("  %element.empty = xor i1 %element.has, true");
                _output.AppendLine("  %needle.empty = xor i1 %needle.has, true");
                _output.AppendLine("  %both.empty.match = and i1 %element.empty, %needle.empty");
                _output.AppendLine("  %both.present = and i1 %element.has, %needle.has");
                if (valueType is "float" or "double")
                {
                    _output.AppendLine($"  %payload.ordered.equal = fcmp oeq {llvmValueType} %element.payload, %needle.payload");
                    _output.AppendLine($"  %payload.left.nan = fcmp uno {llvmValueType} %element.payload, %element.payload");
                    _output.AppendLine($"  %payload.needle.nan = fcmp uno {llvmValueType} %needle.payload, %needle.payload");
                    _output.AppendLine("  %payload.both.nan = and i1 %payload.left.nan, %payload.needle.nan");
                    _output.AppendLine("  %payload.equal = or i1 %payload.ordered.equal, %payload.both.nan");
                }
                else
                {
                    var compareType = LlvmType(compilation.EnumUnderlyingTypes.GetValueOrDefault(valueType) ?? valueType);
                    _output.AppendLine($"  %payload.equal = icmp eq {compareType} %element.payload, %needle.payload");
                }
                _output.AppendLine("  %both.present.match = and i1 %both.present, %payload.equal");
                _output.AppendLine("  %nullable.match = or i1 %both.empty.match, %both.present.match");
                _output.AppendLine("  br i1 %nullable.match, label %found, label %advance");
            }
            else
            {
                _output.AppendLine("  %both.empty = xor i1 %element.has, true");
                _output.AppendLine("  %needle.empty = xor i1 %needle.has, true");
                _output.AppendLine("  %both.empty.match = and i1 %both.empty, %needle.empty");
                _output.AppendLine("  br i1 %both.empty.match, label %found, label %compare.presence");
                _output.AppendLine("compare.presence:");
                _output.AppendLine("  %same.presence = icmp eq i1 %element.has, %needle.has");
                _output.AppendLine("  br i1 %same.presence, label %nullable.values, label %advance");
                _output.AppendLine("nullable.values:");
                EmitArrayIndexOfEquality(valueType, llvmValueType, "%element.payload", "%needle.payload", "found", "advance");
            }
        }
        else
        {
            EmitArrayIndexOfEquality(elementType, llvmElementType, "%element", "%needle", "found", "advance");
        }

        _output.AppendLine("advance:");
        _output.AppendLine("  %next = add i32 %index, 1");
        _output.AppendLine("  br label %loop");
        _output.AppendLine("found:");
        _output.AppendLine("  ret i32 %index");
        _output.AppendLine("missing:");
        _output.AppendLine("  ret i32 -1");
        _output.AppendLine("}");
    }

    private void EmitArrayIndexOfReferenceEqualityHelper(string elementType)
    {
        var equalitySlot = FindArrayIndexOfEqualitySlot(elementType)!;
        var valueType = elementType.EndsWith("?", StringComparison.Ordinal) ? elementType[..^1] : elementType;
        _output.AppendLine("  %needle.is.null = icmp eq ptr %needle, null");
        _output.AppendLine("  br i1 %needle.is.null, label %null.loop, label %reference.loop");
        _output.AppendLine("null.loop:");
        _output.AppendLine("  %null.index = phi i32 [ %start, %entry ], [ %null.next, %null.advance ]");
        _output.AppendLine("  %null.finished = icmp uge i32 %null.index, %end");
        _output.AppendLine("  br i1 %null.finished, label %null.missing, label %null.body");
        _output.AppendLine("null.body:");
        _output.AppendLine("  %null.address = getelementptr ptr, ptr %data, i32 %null.index");
        _output.AppendLine("  %null.element = load ptr, ptr %null.address, align 1");
        _output.AppendLine("  %null.match = icmp eq ptr %null.element, null");
        _output.AppendLine("  br i1 %null.match, label %null.found, label %null.advance");
        _output.AppendLine("null.advance:");
        _output.AppendLine("  %null.next = add i32 %null.index, 1");
        _output.AppendLine("  br label %null.loop");
        _output.AppendLine("null.found:");
        _output.AppendLine("  ret i32 %null.index");
        _output.AppendLine("null.missing:");
        _output.AppendLine("  ret i32 -1");
        _output.AppendLine("reference.loop:");
        _output.AppendLine("  %reference.index = phi i32 [ %start, %entry ], [ %reference.next, %reference.advance ]");
        _output.AppendLine("  %reference.finished = icmp uge i32 %reference.index, %end");
        _output.AppendLine("  br i1 %reference.finished, label %reference.missing, label %reference.body");
        _output.AppendLine("reference.body:");
        _output.AppendLine("  %reference.address = getelementptr ptr, ptr %data, i32 %reference.index");
        _output.AppendLine("  %reference.element = load ptr, ptr %reference.address, align 1");
        _output.AppendLine("  %reference.element.null = icmp eq ptr %reference.element, null");
        _output.AppendLine("  br i1 %reference.element.null, label %reference.advance, label %reference.dispatch");
        _output.AppendLine("reference.dispatch:");

        if (compilation.ClassTypes.TryGetValue(valueType, out var exactClass) && !exactClass.Declaration.IsVirtual &&
            !HasArrayIndexOfDerivedClass(valueType) &&
            FindArrayIndexOfReferenceEqualityImplementation(valueType, equalitySlot) is { } exactImplementation)
        {
            var argumentType = exactImplementation.Declaration.Parameters[0].TypeName;
            _output.AppendLine($"  %reference.equal = call i1 @{FunctionName(exactImplementation, compilation.Methods)}(ptr %reference.element, {LlvmType(argumentType)} %needle)");
            if (!IsProvenNonThrowingArrayIndexOfEquality(exactImplementation))
                EmitArrayIndexOfPendingExceptionCheck();
        }
        else
        {
            _output.AppendLine("  %reference.descriptor = load ptr, ptr %reference.element");
            _output.AppendLine("  %reference.table.address = getelementptr %luma.typedesc, ptr %reference.descriptor, i32 0, i32 6");
            _output.AppendLine("  %reference.table = load ptr, ptr %reference.table.address");
            _output.AppendLine($"  %reference.function.address = getelementptr ptr, ptr %reference.table, i32 {equalitySlot.Index}");
            _output.AppendLine("  %reference.function = load ptr, ptr %reference.function.address");
            _output.AppendLine("  %reference.equal = call i1 %reference.function(ptr %reference.element, ptr %needle)");
            EmitArrayIndexOfPendingExceptionCheck();
        }
        _output.AppendLine("  br i1 %reference.equal, label %reference.found, label %reference.advance");
        _output.AppendLine("reference.advance:");
        _output.AppendLine("  %reference.next = add i32 %reference.index, 1");
        _output.AppendLine("  br label %reference.loop");
        _output.AppendLine("reference.found:");
        _output.AppendLine("  ret i32 %reference.index");
        _output.AppendLine("reference.missing:");
        _output.AppendLine("  ret i32 -1");
    }

    private void EmitArrayIndexOfNullablePrimitiveUnrolledLoop(string valueType, string llvmElementType)
    {
        const int unroll = 4;
        var llvmValueType = LlvmType(valueType);
        var compareType = LlvmType(compilation.EnumUnderlyingTypes.GetValueOrDefault(valueType) ?? valueType);
        _output.AppendLine("nullable.loop:");
        _output.AppendLine("  %nullable.index = phi i32 [ %start, %entry ], [ %nullable.next, %nullable.advance ]");
        _output.AppendLine("  %nullable.remaining = sub i32 %end, %nullable.index");
        _output.AppendLine($"  %nullable.group.available = icmp uge i32 %nullable.remaining, {unroll}");
        _output.AppendLine("  br i1 %nullable.group.available, label %nullable.group, label %scalar.preheader");
        _output.AppendLine("nullable.group:");
        _output.AppendLine($"  %nullable.needle.has = extractvalue {llvmElementType} %needle, 0");
        _output.AppendLine($"  %nullable.needle.payload = extractvalue {llvmElementType} %needle, 1");
        _output.AppendLine("  %nullable.needle.empty = xor i1 %nullable.needle.has, true");
        for (var item = 0; item < unroll; item++)
        {
            var suffix = $".{item}";
            var index = item == 0 ? "%nullable.index" : $"%nullable.index{suffix}";
            if (item > 0)
                _output.AppendLine($"  {index} = add i32 %nullable.index, {item}");
            _output.AppendLine($"  %nullable.address{suffix} = getelementptr {llvmElementType}, ptr %data, i32 {index}");
            _output.AppendLine($"  %nullable.element{suffix} = load {llvmElementType}, ptr %nullable.address{suffix}, align 1");
            _output.AppendLine($"  %nullable.has{suffix} = extractvalue {llvmElementType} %nullable.element{suffix}, 0");
            _output.AppendLine($"  %nullable.payload{suffix} = extractvalue {llvmElementType} %nullable.element{suffix}, 1");
            _output.AppendLine($"  %nullable.empty{suffix} = xor i1 %nullable.has{suffix}, true");
            _output.AppendLine($"  %nullable.both.empty{suffix} = and i1 %nullable.empty{suffix}, %nullable.needle.empty");
            _output.AppendLine($"  %nullable.both.present{suffix} = and i1 %nullable.has{suffix}, %nullable.needle.has");
            if (valueType is "float" or "double")
            {
                _output.AppendLine($"  %nullable.ordered.equal{suffix} = fcmp oeq {llvmValueType} %nullable.payload{suffix}, %nullable.needle.payload");
                _output.AppendLine($"  %nullable.left.nan{suffix} = fcmp uno {llvmValueType} %nullable.payload{suffix}, %nullable.payload{suffix}");
                _output.AppendLine($"  %nullable.needle.nan{suffix} = fcmp uno {llvmValueType} %nullable.needle.payload, %nullable.needle.payload");
                _output.AppendLine($"  %nullable.both.nan{suffix} = and i1 %nullable.left.nan{suffix}, %nullable.needle.nan{suffix}");
                _output.AppendLine($"  %nullable.payload.equal{suffix} = or i1 %nullable.ordered.equal{suffix}, %nullable.both.nan{suffix}");
            }
            else
                _output.AppendLine($"  %nullable.payload.equal{suffix} = icmp eq {compareType} %nullable.payload{suffix}, %nullable.needle.payload");
            _output.AppendLine($"  %nullable.payload.match{suffix} = and i1 %nullable.both.present{suffix}, %nullable.payload.equal{suffix}");
            _output.AppendLine($"  %nullable.match{suffix} = or i1 %nullable.both.empty{suffix}, %nullable.payload.match{suffix}");
        }
        _output.AppendLine("  %nullable.any.01 = or i1 %nullable.match.0, %nullable.match.1");
        _output.AppendLine("  %nullable.any.23 = or i1 %nullable.match.2, %nullable.match.3");
        _output.AppendLine("  %nullable.any = or i1 %nullable.any.01, %nullable.any.23");
        _output.AppendLine("  br i1 %nullable.any, label %nullable.found, label %nullable.advance");
        _output.AppendLine("nullable.found:");
        for (var item = unroll - 1; item >= 0; item--)
        {
            var suffix = $".{item}";
            var index = item == 0 ? "%nullable.index" : $"%nullable.index{suffix}";
            var fallback = item == unroll - 1 ? "-1" : $"%nullable.candidate.{item + 1}";
            _output.AppendLine($"  %nullable.candidate.{item} = select i1 %nullable.match{suffix}, i32 {index}, i32 {fallback}");
        }
        _output.AppendLine("  ret i32 %nullable.candidate.0");
        _output.AppendLine("nullable.advance:");
        _output.AppendLine($"  %nullable.next = add i32 %nullable.index, {unroll}");
        _output.AppendLine("  br label %nullable.loop");
    }

    private void EmitArrayIndexOfVectorLoop(string vectorType, int laneCount, string laneMaskType, bool widenBooleanNeedle, bool bigEndian)
    {
        if (laneCount >= 8)
        {
            EmitArrayIndexOfDeferredMaskVectorLoop(vectorType, laneCount, laneMaskType, widenBooleanNeedle, bigEndian);
            return;
        }
        var vector = $"<{laneCount} x {vectorType}>";
        var laneMask = laneMaskType;
        var unroll = laneCount >= 8 ? 4 : 2;
        var groupMaskWidth = laneCount;
        // The 8-bit path combines four 32-lane masks and needs a 128-bit
        // value; narrower masks would shift by their width (poison in LLVM IR).
        var totalMaskBits = laneCount * unroll;
        var resultBits = totalMaskBits > 64 ? 128 : totalMaskBits > 32 ? 64 : 32;
        var resultMaskType = $"i{resultBits}";
        _output.AppendLine("vector.loop:");
        _output.AppendLine("  %vector.index = phi i32 [ %start, %entry ], [ %vector.next, %vector.advance ]");
        _output.AppendLine("  %vector.remaining = sub i32 %end, %vector.index");
        _output.AppendLine($"  %vector.available = icmp uge i32 %vector.remaining, {laneCount * unroll}");
        _output.AppendLine("  br i1 %vector.available, label %vector.body, label %scalar.preheader");
        _output.AppendLine("vector.body:");
        if (widenBooleanNeedle)
            _output.AppendLine("  %needle.wide = zext i1 %needle to i8");
        _output.AppendLine($"  %needle.insert = insertelement {vector} poison, {vectorType} {(widenBooleanNeedle ? "%needle.wide" : "%needle")}, i32 0");
        _output.AppendLine($"  %needle.splat = shufflevector {vector} %needle.insert, {vector} poison, <{laneCount} x i32> zeroinitializer");
        for (var group = 0; group < unroll; group++)
        {
            var offset = laneCount * group;
            var suffix = $".{group}";
            _output.AppendLine($"  %vector.offset{suffix} = add i32 %vector.index, {offset}");
            _output.AppendLine($"  %vector.address{suffix} = getelementptr {vectorType}, ptr %data, i32 %vector.offset{suffix}");
            _output.AppendLine($"  %vector.values{suffix} = load {vector}, ptr %vector.address{suffix}, align 1");
            if (vectorType is "float" or "double")
            {
                _output.AppendLine($"  %vector.ordered.equal{suffix} = fcmp oeq {vector} %vector.values{suffix}, %needle.splat");
                _output.AppendLine($"  %vector.left.nan{suffix} = fcmp uno {vector} %vector.values{suffix}, %vector.values{suffix}");
                _output.AppendLine($"  %vector.needle.nan{suffix} = fcmp uno {vector} %needle.splat, %needle.splat");
                _output.AppendLine($"  %vector.both.nan{suffix} = and <{laneCount} x i1> %vector.left.nan{suffix}, %vector.needle.nan{suffix}");
                _output.AppendLine($"  %vector.compare{suffix} = or <{laneCount} x i1> %vector.ordered.equal{suffix}, %vector.both.nan{suffix}");
            }
            else
            {
                _output.AppendLine($"  %vector.compare{suffix} = icmp eq {vector} %vector.values{suffix}, %needle.splat");
            }
            _output.AppendLine($"  %vector.mask{suffix} = bitcast <{laneCount} x i1> %vector.compare{suffix} to {laneMask}");
            if (bigEndian)
            {
                if (laneCount == 32)
                    _output.AppendLine($"  %vector.mask.i32{suffix} = add i32 %vector.mask{suffix}, 0");
                else
                    _output.AppendLine($"  %vector.mask.i32{suffix} = zext {laneMask} %vector.mask{suffix} to i32");
                _output.AppendLine($"  %vector.mask.reversed{suffix} = call i32 @llvm.bitreverse.i32(i32 %vector.mask.i32{suffix})");
                _output.AppendLine($"  %vector.mask.ordered{suffix} = lshr i32 %vector.mask.reversed{suffix}, {32 - laneCount}");
                if (resultBits > 32)
                    _output.AppendLine($"  %vector.mask.wide{suffix} = zext i32 %vector.mask.ordered{suffix} to {resultMaskType}");
                else
                    _output.AppendLine($"  %vector.mask.wide{suffix} = add i32 %vector.mask.ordered{suffix}, 0");
            }
            else
                _output.AppendLine($"  %vector.mask.wide{suffix} = zext {laneMask} %vector.mask{suffix} to {resultMaskType}");
            if (group > 0)
                _output.AppendLine($"  %vector.mask.shifted{suffix} = shl {resultMaskType} %vector.mask.wide{suffix}, {groupMaskWidth * group}");
        }
        for (var group = 1; group < unroll; group++)
        {
            var suffix = $".{group}";
            if (group == 1)
                _output.AppendLine($"  %vector.combined.1 = or {resultMaskType} %vector.mask.wide.0, %vector.mask.shifted{suffix}");
            else
                _output.AppendLine($"  %vector.combined.{group} = or {resultMaskType} %vector.combined.{group - 1}, %vector.mask.shifted{suffix}");
        }
        var combinedMask = $"%vector.combined.{unroll - 1}";
        _output.AppendLine($"  %vector.mask.empty = icmp eq {resultMaskType} {combinedMask}, 0");
        _output.AppendLine("  br i1 %vector.mask.empty, label %vector.advance, label %vector.found");
        _output.AppendLine("vector.found:");
        _output.AppendLine($"  %vector.lane = call i{resultBits} @llvm.cttz.i{resultBits}({resultMaskType} {combinedMask}, i1 false)");
        if (resultBits > 32)
            _output.AppendLine($"  %vector.lane.i32 = trunc i{resultBits} %vector.lane to i32");
        _output.AppendLine($"  %vector.result = add i32 %vector.index, {(resultBits > 32 ? "%vector.lane.i32" : "%vector.lane")}");
        _output.AppendLine("  ret i32 %vector.result");
        _output.AppendLine("vector.advance:");
        _output.AppendLine($"  %vector.next = add i32 %vector.index, {laneCount * unroll}");
        _output.AppendLine("  br label %vector.loop");
    }

    private void EmitArrayIndexOfDeferredMaskVectorLoop(string vectorType, int laneCount, string laneMaskType,
        bool widenBooleanNeedle, bool bigEndian)
    {
        var vector = $"<{laneCount} x {vectorType}>";
        var laneMask = laneMaskType;
        var unroll = laneCount >= 8 ? 4 : 2;
        var totalLanes = laneCount * unroll;
        var resultBits = totalLanes > 64 ? 128 : totalLanes > 32 ? 64 : 32;
        var resultMaskType = $"i{resultBits}";
        _output.AppendLine("vector.loop:");
        _output.AppendLine("  %vector.index = phi i32 [ %start, %entry ], [ %vector.next, %vector.advance ]");
        _output.AppendLine("  %vector.remaining = sub i32 %end, %vector.index");
        _output.AppendLine($"  %vector.available = icmp uge i32 %vector.remaining, {totalLanes}");
        _output.AppendLine("  br i1 %vector.available, label %vector.body, label %scalar.preheader");
        _output.AppendLine("vector.body:");
        if (widenBooleanNeedle)
            _output.AppendLine("  %needle.wide = zext i1 %needle to i8");
        _output.AppendLine($"  %needle.insert = insertelement {vector} poison, {vectorType} {(widenBooleanNeedle ? "%needle.wide" : "%needle")}, i32 0");
        _output.AppendLine($"  %needle.splat = shufflevector {vector} %needle.insert, {vector} poison, <{laneCount} x i32> zeroinitializer");
        for (var group = 0; group < unroll; group++)
        {
            var suffix = $".{group}";
            var offset = laneCount * group;
            _output.AppendLine($"  %vector.offset{suffix} = add i32 %vector.index, {offset}");
            _output.AppendLine($"  %vector.address{suffix} = getelementptr {vectorType}, ptr %data, i32 %vector.offset{suffix}");
            _output.AppendLine($"  %vector.values{suffix} = load {vector}, ptr %vector.address{suffix}, align 1");
            if (vectorType is "float" or "double")
            {
                _output.AppendLine($"  %vector.ordered.equal{suffix} = fcmp oeq {vector} %vector.values{suffix}, %needle.splat");
                _output.AppendLine($"  %vector.left.nan{suffix} = fcmp uno {vector} %vector.values{suffix}, %vector.values{suffix}");
                _output.AppendLine($"  %vector.needle.nan{suffix} = fcmp uno {vector} %needle.splat, %needle.splat");
                _output.AppendLine($"  %vector.both.nan{suffix} = and <{laneCount} x i1> %vector.left.nan{suffix}, %vector.needle.nan{suffix}");
                _output.AppendLine($"  %vector.compare{suffix} = or <{laneCount} x i1> %vector.ordered.equal{suffix}, %vector.both.nan{suffix}");
            }
            else
                _output.AppendLine($"  %vector.compare{suffix} = icmp eq {vector} %vector.values{suffix}, %needle.splat");
        }
        var combinedVectorMask = "%vector.compare.0";
        for (var group = 1; group < unroll; group++)
        {
            var nextMask = $"%vector.any.mask.{group}";
            _output.AppendLine($"  {nextMask} = or <{laneCount} x i1> {combinedVectorMask}, %vector.compare.{group}");
            combinedVectorMask = nextMask;
        }
        _output.AppendLine($"  %vector.any = call i1 @llvm.vector.reduce.or.v{laneCount}i1(<{laneCount} x i1> {combinedVectorMask})");
        _output.AppendLine("  br i1 %vector.any, label %vector.extract, label %vector.advance");
        _output.AppendLine("vector.extract:");
        for (var group = 0; group < unroll; group++)
        {
            var suffix = $".{group}";
            _output.AppendLine($"  %vector.mask{suffix} = bitcast <{laneCount} x i1> %vector.compare{suffix} to {laneMask}");
            if (bigEndian)
            {
                if (laneCount == 32)
                    _output.AppendLine($"  %vector.mask.i32{suffix} = add i32 %vector.mask{suffix}, 0");
                else
                    _output.AppendLine($"  %vector.mask.i32{suffix} = zext {laneMask} %vector.mask{suffix} to i32");
                _output.AppendLine($"  %vector.mask.reversed{suffix} = call i32 @llvm.bitreverse.i32(i32 %vector.mask.i32{suffix})");
                _output.AppendLine($"  %vector.mask.ordered{suffix} = lshr i32 %vector.mask.reversed{suffix}, {32 - laneCount}");
                if (resultBits > 32)
                    _output.AppendLine($"  %vector.mask.wide{suffix} = zext i32 %vector.mask.ordered{suffix} to {resultMaskType}");
                else
                    _output.AppendLine($"  %vector.mask.wide{suffix} = add i32 %vector.mask.ordered{suffix}, 0");
            }
            else
                _output.AppendLine($"  %vector.mask.wide{suffix} = zext {laneMask} %vector.mask{suffix} to {resultMaskType}");
            if (group > 0)
                _output.AppendLine($"  %vector.mask.shifted{suffix} = shl {resultMaskType} %vector.mask.wide{suffix}, {laneCount * group}");
        }
        for (var group = 1; group < unroll; group++)
        {
            var suffix = $".{group}";
            if (group == 1)
                _output.AppendLine($"  %vector.combined.1 = or {resultMaskType} %vector.mask.wide.0, %vector.mask.shifted{suffix}");
            else
                _output.AppendLine($"  %vector.combined.{group} = or {resultMaskType} %vector.combined.{group - 1}, %vector.mask.shifted{suffix}");
        }
        _output.AppendLine($"  %vector.lane = call i{resultBits} @llvm.cttz.i{resultBits}({resultMaskType} %vector.combined.{unroll - 1}, i1 false)");
        if (resultBits > 32)
            _output.AppendLine($"  %vector.lane.i32 = trunc i{resultBits} %vector.lane to i32");
        _output.AppendLine($"  %vector.result = add i32 %vector.index, {(resultBits > 32 ? "%vector.lane.i32" : "%vector.lane")}");
        _output.AppendLine("  ret i32 %vector.result");
        _output.AppendLine("vector.advance:");
        _output.AppendLine($"  %vector.next = add i32 %vector.index, {totalLanes}");
        _output.AppendLine("  br label %vector.loop");
    }

    private bool TryGetArrayIndexOfVectorSpec(string type, out string vectorType, out int laneCount, out string laneMaskType)
    {
        vectorType = string.Empty;
        laneCount = 0;
        laneMaskType = string.Empty;
        if (type.EndsWith("?", StringComparison.Ordinal) || IsStringType(type)) return false;
        if (type == "bool")
        {
            vectorType = "i8";
            laneCount = 32;
            laneMaskType = "i32";
            return true;
        }
        var comparisonType = compilation.EnumUnderlyingTypes.GetValueOrDefault(type) ?? type;
        if (!IsArrayIndexOfPrimitive(comparisonType)) return false;
        vectorType = LlvmType(comparisonType);
        var width = vectorType switch
        {
            "i8" => 8,
            "i16" => 16,
            "i32" or "float" => 32,
            "i64" or "double" => 64,
            _ => 0
        };
        if (width == 0) return false;
        // Narrow 16-bit searches benefit from processing a full 512-bit lane
        // on CPUs that support it; LLVM splits the same IR into legal chunks
        // when the requested target CPU has a smaller vector width.
        laneCount = width == 16 ? 512 / width : 256 / width;
        laneMaskType = $"i{laneCount}";
        return laneCount is 4 or 8 or 16 or 32;
    }

    private bool IsArrayIndexOfBigEndianTarget()
    {
        if (compilation.TargetTriple == "host") return !BitConverter.IsLittleEndian;
        var triple = compilation.TargetTriple;
        var architecture = triple.Split('-', 2)[0];
        return architecture.EndsWith("_be", StringComparison.OrdinalIgnoreCase) ||
            architecture.EndsWith("eb", StringComparison.OrdinalIgnoreCase);
    }

    private void EmitArrayIndexOfEquality(string type, string llvmType, string left, string right,
        string foundLabel, string nextLabel)
    {
        var valueType = type.EndsWith("?", StringComparison.Ordinal) ? type[..^1] : type;
        if (IsStringType(type))
        {
            _output.AppendLine($"  %string.same = icmp eq ptr {left}, {right}");
            _output.AppendLine($"  br i1 %string.same, label %{foundLabel}, label %string.check.null");
            _output.AppendLine("string.check.null:");
            _output.AppendLine($"  %string.left.null = icmp eq ptr {left}, null");
            _output.AppendLine($"  %string.right.null = icmp eq ptr {right}, null");
            _output.AppendLine("  %string.either.null = or i1 %string.left.null, %string.right.null");
            _output.AppendLine($"  br i1 %string.either.null, label %{nextLabel}, label %string.compare");
            _output.AppendLine("string.compare:");
            _output.AppendLine($"  %string.equal = call i1 @luma_string_equals(ptr {left}, ptr {right})");
            _output.AppendLine($"  br i1 %string.equal, label %{foundLabel}, label %{nextLabel}");
            return;
        }

        if (IsArrayIndexOfPrimitive(valueType) || compilation.EnumUnderlyingTypes.ContainsKey(valueType))
        {
            var compareType = compilation.EnumUnderlyingTypes.GetValueOrDefault(valueType) ?? valueType;
            var compareLlvmType = LlvmType(compareType);
            if (compareType is "float" or "double")
            {
                _output.AppendLine($"  %ordered.equal = fcmp oeq {compareLlvmType} {left}, {right}");
                _output.AppendLine($"  %left.nan = fcmp uno {compareLlvmType} {left}, {left}");
                _output.AppendLine($"  %right.nan = fcmp uno {compareLlvmType} {right}, {right}");
                _output.AppendLine("  %both.nan = and i1 %left.nan, %right.nan");
                _output.AppendLine("  %float.equal = or i1 %ordered.equal, %both.nan");
                _output.AppendLine($"  br i1 %float.equal, label %{foundLabel}, label %{nextLabel}");
            }
            else
            {
                _output.AppendLine($"  %integer.equal = icmp eq {compareLlvmType} {left}, {right}");
                _output.AppendLine($"  br i1 %integer.equal, label %{foundLabel}, label %{nextLabel}");
            }
            return;
        }

        var equalitySlot = FindArrayIndexOfEqualitySlot(type);
        if (compilation.StructTypes.ContainsKey(valueType))
        {
            var implementation = FindArrayIndexOfStructEqualityImplementation(valueType, equalitySlot);
            if (implementation is null)
                throw new InvalidOperationException($"Array.IndexOf for value type '{type}' has no IEquality implementation.");
            _output.AppendLine($"  %struct.receiver = alloca {LlvmType(valueType)}, align 1");
            if (type == valueType)
                _output.AppendLine($"  store {LlvmType(valueType)} {left}, ptr %struct.receiver, align 1");
            else
                _output.AppendLine($"  store {LlvmType(valueType)} {left}, ptr %struct.receiver, align 1");
            var argumentType = implementation.Declaration.Parameters[0].TypeName;
            _output.AppendLine($"  %struct.equal = call i1 @{FunctionName(implementation, compilation.Methods)}(ptr %struct.receiver, {LlvmType(argumentType)} {right})");
            if (!IsProvenNonThrowingArrayIndexOfEquality(implementation))
                EmitArrayIndexOfPendingExceptionCheck();
            _output.AppendLine($"  br i1 %struct.equal, label %{foundLabel}, label %{nextLabel}");
            return;
        }

        if (llvmType == "ptr")
        {
            if (equalitySlot is null)
            {
                _output.AppendLine($"  %reference.equal = icmp eq ptr {left}, {right}");
                _output.AppendLine($"  br i1 %reference.equal, label %{foundLabel}, label %{nextLabel}");
                return;
            }
            _output.AppendLine($"  %reference.left.null = icmp eq ptr {left}, null");
            _output.AppendLine($"  %reference.right.null = icmp eq ptr {right}, null");
            _output.AppendLine("  %reference.both.null = and i1 %reference.left.null, %reference.right.null");
            _output.AppendLine($"  br i1 %reference.both.null, label %{foundLabel}, label %reference.check.null");
            _output.AppendLine("reference.check.null:");
            _output.AppendLine("  %reference.either.null = or i1 %reference.left.null, %reference.right.null");
            _output.AppendLine($"  br i1 %reference.either.null, label %{nextLabel}, label %reference.dispatch");

            // A non-virtual class with no compiled subclasses cannot vary at
            // runtime. Emit a direct call so
            // LLVM can inline the equality body instead of loading a type
            // descriptor, interface table, and indirect function pointer for
            // every array element.
            if (compilation.ClassTypes.TryGetValue(valueType, out var exactClass) && !exactClass.Declaration.IsVirtual &&
                !HasArrayIndexOfDerivedClass(valueType) &&
                FindArrayIndexOfReferenceEqualityImplementation(valueType, equalitySlot) is { } exactImplementation)
            {
                var argumentType = exactImplementation.Declaration.Parameters[0].TypeName;
                _output.AppendLine($"reference.dispatch:");
                _output.AppendLine($"  %reference.equal = call i1 @{FunctionName(exactImplementation, compilation.Methods)}(ptr {left}, {LlvmType(argumentType)} {right})");
                if (!IsProvenNonThrowingArrayIndexOfEquality(exactImplementation))
                    EmitArrayIndexOfPendingExceptionCheck();
                _output.AppendLine($"  br i1 %reference.equal, label %{foundLabel}, label %{nextLabel}");
                return;
            }

            _output.AppendLine("reference.dispatch:");
            _output.AppendLine($"  %reference.descriptor = load ptr, ptr {left}");
            _output.AppendLine("  %reference.table.address = getelementptr %luma.typedesc, ptr %reference.descriptor, i32 0, i32 6");
            _output.AppendLine("  %reference.table = load ptr, ptr %reference.table.address");
            _output.AppendLine($"  %reference.function.address = getelementptr ptr, ptr %reference.table, i32 {equalitySlot.Index}");
            _output.AppendLine("  %reference.function = load ptr, ptr %reference.function.address");
            _output.AppendLine($"  %reference.equal = call i1 %reference.function(ptr {left}, ptr {right})");
            EmitArrayIndexOfPendingExceptionCheck();
            _output.AppendLine($"  br i1 %reference.equal, label %{foundLabel}, label %{nextLabel}");
            return;
        }

        if (llvmType.EndsWith('*') || llvmType.StartsWith('i') || llvmType is "float" or "double")
        {
            var operation = llvmType is "float" or "double" ? "fcmp oeq" : "icmp eq";
            _output.AppendLine($"  %value.equal = {operation} {llvmType} {left}, {right}");
            _output.AppendLine($"  br i1 %value.equal, label %{foundLabel}, label %{nextLabel}");
            return;
        }

        throw new NotSupportedException($"Array.IndexOf does not support element type '{type}'.");
    }

    private bool IsArrayIndexOfNullableValue(string type)
    {
        if (!type.EndsWith("?", StringComparison.Ordinal)) return false;
        var underlying = type[..^1];
        if (underlying is "string" or "System.String" or "object" or "System.Object" ||
            compilation.ClassTypes.ContainsKey(underlying) || compilation.InterfaceTypes.ContainsKey(underlying) ||
            compilation.DelegateTypes.ContainsKey(underlying) || SemanticAnalyzer.TryParseManagedArrayType(underlying, out _, out _)) return false;
        return IsArrayIndexOfPrimitive(underlying) || compilation.EnumUnderlyingTypes.ContainsKey(underlying) || compilation.StructTypes.ContainsKey(underlying);
    }

    private static bool IsArrayIndexOfPrimitive(string type) => type is "bool" or "char" or "int8" or "uint8" or "int16" or "uint16" or
        "int32" or "uint32" or "int64" or "uint64" or "int" or "uint" or "float" or "double";

    private InterfaceMethodSlot? FindArrayIndexOfEqualitySlot(string elementType)
    {
        var valueType = elementType.EndsWith("?", StringComparison.Ordinal) ? elementType[..^1] : elementType;
        var referenceElement = IsStringType(elementType) || compilation.ClassTypes.ContainsKey(valueType) ||
            compilation.InterfaceTypes.ContainsKey(valueType) || compilation.DelegateTypes.ContainsKey(valueType) ||
            SemanticAnalyzer.TryParseManagedArrayType(valueType, out _, out _);
        return compilation.InterfaceMethodSlots.FirstOrDefault(slot =>
            slot.InterfaceType.Namespace == "System" && slot.InterfaceType.Name.StartsWith("IEquality", StringComparison.Ordinal) &&
            slot.Contract.Declaration.Name == "Equals" && slot.Contract.Declaration.Parameters.Count == 1 &&
            (slot.Contract.Declaration.Parameters[0].TypeName == elementType || referenceElement &&
                slot.Contract.Declaration.Parameters[0].TypeName.TrimEnd('?') == valueType));
    }
    private MethodSymbol? FindArrayIndexOfStructEqualityImplementation(string structType, InterfaceMethodSlot? equalitySlot)
    {
        if (equalitySlot is null) return null;
        if (!compilation.InterfaceDispatchTables.TryGetValue(structType, out var entries)) return null;
        return equalitySlot.Index < entries.Count ? entries[equalitySlot.Index] : null;
    }

    private MethodSymbol? FindArrayIndexOfReferenceEqualityImplementation(string classType, InterfaceMethodSlot equalitySlot)
    {
        if (!compilation.InterfaceDispatchTables.TryGetValue(classType, out var entries)) return null;
        return equalitySlot.Index < entries.Count ? entries[equalitySlot.Index] : null;
    }

    private bool HasArrayIndexOfDerivedClass(string classType)
    {
        foreach (var candidate in compilation.ClassTypes.Values)
        {
            var baseType = candidate.BaseTypeName;
            while (baseType is not null)
            {
                if (baseType == classType) return true;
                baseType = compilation.ClassTypes.TryGetValue(baseType, out var parent) ? parent.BaseTypeName : null;
            }
        }
        return false;
    }

    private bool IsProvenNonThrowingArrayIndexOfEquality(MethodSymbol method)
    {
        if (method.Declaration.Body is not { Statements.Count: 1 } body ||
            body.Statements[0] is not ReturnStatement { Value: { } value })
            return false;

        var parameterNames = method.Declaration.Parameters.Select(parameter => parameter.Name).ToHashSet(StringComparer.Ordinal);
        return IsNonThrowingArrayIndexOfExpression(value, parameterNames);
    }

    private bool IsNonThrowingArrayIndexOfExpression(Expression expression, HashSet<string> parameterNames) => expression switch
    {
        LiteralExpression or ThisExpression => true,
        NameExpression name => parameterNames.Contains(name.Name) || compilation.UnqualifiedClassFields.ContainsKey(name) ||
            compilation.UnqualifiedStructFields.ContainsKey(name) || compilation.UnqualifiedConstantFields.ContainsKey(name),
        MemberAccessExpression member when !member.IsNullConditional &&
            (member.Receiver is ThisExpression || member.Receiver is NameExpression receiver && parameterNames.Contains(receiver.Name)) &&
            (compilation.ClassFieldAccesses.ContainsKey(member) || compilation.StructFieldAccesses.ContainsKey(member)) => true,
        BinaryExpression binary when binary.Operator is "==" or "!=" or "<" or "<=" or ">" or ">=" or "&&" or "||" or "&" or "|" or "^" &&
            !compilation.OperatorTargets.ContainsKey(binary) &&
            IsNonThrowingArrayIndexOfExpression(binary.Left, parameterNames) &&
            IsNonThrowingArrayIndexOfExpression(binary.Right, parameterNames) => true,
        UnaryExpression unary when unary.Operator is "!" or "~" &&
            IsNonThrowingArrayIndexOfExpression(unary.Operand, parameterNames) => true,
        ConditionalExpression conditional when
            IsNonThrowingArrayIndexOfExpression(conditional.Condition, parameterNames) &&
            IsNonThrowingArrayIndexOfExpression(conditional.WhenTrue, parameterNames) &&
            IsNonThrowingArrayIndexOfExpression(conditional.WhenFalse, parameterNames) => true,
        _ => false
    };

    private void EmitArrayIndexOfPendingExceptionCheck()
    {
        if (!compilation.UsesExceptionHandling) return;
        _output.AppendLine("  %indexof.pending.exception = load ptr, ptr @luma_pending_exception");
        _output.AppendLine("  %indexof.has.exception = icmp ne ptr %indexof.pending.exception, null");
        _output.AppendLine("  br i1 %indexof.has.exception, label %exception.pending, label %exception.clear");
        _output.AppendLine("exception.pending:");
        _output.AppendLine("  ret i32 -1");
        _output.AppendLine("exception.clear:");
    }
}
