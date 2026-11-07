using LumaSharp.Compiler;
using Xunit;

namespace LumaSharp.Tests;

public sealed class ArrayIndexOfTests
{
    [Fact]
    public void ArrayIndexOfSupportsPrimitiveEnumNullableStringAndUserEqualityTypes()
    {
        var runtimeRoot = Path.GetDirectoryName(RuntimeSourcePath("LumaSharp.Library/System/Object.ls"))!;
        var runtimeSources = Directory.EnumerateFiles(runtimeRoot, "*.ls", SearchOption.AllDirectories)
            .Select(path => (path, File.ReadAllText(path))).ToArray();
        const string source = """
            using System;
            enum Shade : uint8 { Red, Green, Blue }
            public class Key : System.IEquality<Key?>
            {
                public int32 Value;
                public this(int32 value) { Value = value; }
                public bool Equals(Key? other) => other != null && Value == other.Value;
            }
            public struct Point : System.IEquality<Point>
            {
                public int32 X;
                public this(int32 x) { X = x; }
                public bool Equals(Point other) => X == other.X;
            }
            public class Plain
            {
                public int32 Value;
                public this(int32 value) { Value = value; }
            }
            public class ThrowingKey : System.IEquality<ThrowingKey?>
            {
                public this() { }
                public bool Equals(ThrowingKey? other) { throw new Exception(); }
            }
            public virtual class EqualityBase : System.IEquality<EqualityBase?>
            {
                public int32 Value;
                public this(int32 value) { Value = value; }
                public virtual bool Equals(EqualityBase? other) => false;
            }
            public class EqualityDerived : EqualityBase
            {
                public this(int32 value) : base(value) { }
                public override bool Equals(EqualityBase? other) => other != null && Value == other.Value;
            }
            public class Program
            {
                private static int32? GetNullableTwo() => 2;
                private static Point? GetNullablePoint() => Point(9);
                public static int32 Main()
                {
                    bool[] booleans = { true, false, true };
                    char[] characters = { 'a', 'b', 'c' };
                    int8[] signedBytes = { 1, 2, 3 };
                    uint8[] unsignedBytes = { 1, 2, 3 };
                    int16[] signedShorts = { 1, 2, 3 };
                    uint16[] unsignedShorts = { 1, 2, 3 };
                    int32[] signedInts = { 1, 2, 3 };
                    uint32[] unsignedInts = { 1, 2, 3 };
                    int64[] signedLongs = { 1, 2, 3 };
                    uint64[] unsignedLongs = { 1, 2, 3 };
                    int[] nativeInts = { 1, 2, 3 };
                    uint[] nativeUInts = { 1, 2, 3 };
                    float[] singles = { 1.0f, 2.0f, 3.0f };
                    double[] doubles = { 1.0, 2.0, 3.0 };
                    Shade[] shades = { Shade.Red, Shade.Green, Shade.Blue };
                    int32?[] optional = { null, 2, null };
                    int32?[] optionalLong = new int32?[70];
                    optionalLong[4] = 7;
                    optionalLong[69] = 42;
                    float?[] optionalFloats = { null, -0.0f, Float.NaN };
                    string?[] strings = { "one", "needle", null };
                    Key?[] keys = { new Key(1), new Key(2), null };
                    Point[] points = { Point(4), Point(9) };
                    Point?[] optionalPoints = { null, GetNullablePoint() };
                    Plain firstPlain = new Plain(7);
                    Plain secondPlain = new Plain(7);
                    Plain[] plains = { firstPlain };
                    EqualityBase?[] polymorphic = { new EqualityDerived(42) };
                    int32[][] jagged = { new int32[1], new int32[1] };

                    if (Array.IndexOf(booleans, false) != 1 || Array.IndexOf(characters, 'c') != 2) return 1;
                    if (Array.IndexOf(signedBytes, (int8)2) != 1 || Array.IndexOf(unsignedBytes, (uint8)2) != 1) return 2;
                    if (Array.IndexOf(signedShorts, (int16)2) != 1 || Array.IndexOf(unsignedShorts, (uint16)2) != 1) return 3;
                    if (Array.IndexOf(signedInts, 2) != 1 || Array.IndexOf(unsignedInts, (uint32)2) != 1) return 4;
                    if (Array.IndexOf(signedLongs, (int64)2) != 1 || Array.IndexOf(unsignedLongs, (uint64)2) != 1) return 5;
                    if (Array.IndexOf(nativeInts, (int)2) != 1 || Array.IndexOf(nativeUInts, (uint)2) != 1) return 6;
                    if (Array.IndexOf(singles, 2.0f) != 1 || Array.IndexOf(doubles, 2.0) != 1) return 7;
                    if (Array.IndexOf(shades, Shade.Blue) != 2) return 8;
                    if (Array.IndexOf(optional, default(int32?)) != 0 || Array.IndexOf(optional, GetNullableTwo()) != 1) return 9;
                    if (Array.IndexOf(optionalLong, 7) != 4 || Array.IndexOf(optionalLong, 42, 5) != 69 || Array.IndexOf(optionalLong, 99) != -1) return 26;
                    if (Array.IndexOf(optionalFloats, default(float?)) != 0 || Array.IndexOf(optionalFloats, 0.0f) != 1 || Array.IndexOf(optionalFloats, Float.NaN) != 2) return 25;
                    if (Array.IndexOf(strings, "nee" + "dle") != 1 || Array.IndexOf(strings, null) != 2) return 10;
                    if (Array.IndexOf(keys, new Key(2)) != 1 || Array.IndexOf(keys, null) != 2) return 11;
                    if (Array.IndexOf(points, Point(9)) != 1) return 12;
                    if (Array.IndexOf(optionalPoints, default(Point?)) != 0 || Array.IndexOf(optionalPoints, GetNullablePoint()) != 1) return 24;
                    if (Array.IndexOf(plains, firstPlain) != 0 || Array.IndexOf(plains, secondPlain) != -1) return 13;
                    if (Array.IndexOf(polymorphic, new EqualityBase(42)) != 0) return 28;
                    if (Array.IndexOf(jagged, jagged[1]) != 1 || Array.IndexOf(jagged, new int32[1]) != -1) return 29;

                    int32[] duplicates = { 4, 8, 8, 9 };
                    if (Array.IndexOf(duplicates, 8) != 1) return 14;
                    if (Array.IndexOf(duplicates, 8, 2) != 2) return 15;
                    if (Array.IndexOf(duplicates, 8, 2, 1) != 2 || Array.IndexOf(duplicates, 8, 3, 1) != -1) return 16;
                    if (Array.IndexOf(duplicates, 8, duplicates.Length, 0) != -1) return 17;
                    int32[] empty = new int32[0];
                    if (Array.IndexOf(empty, 8) != -1 || Array.IndexOf(empty, 8, 0, 0) != -1) return 25;

                    float nan = Float.NaN;
                    float[] nanValues = { nan, 1.0f };
                    if (Array.IndexOf(nanValues, Float.NaN) != 0) return 18;
                    float negativeZero = -0.0f;
                    float[] zeroValues = { negativeZero };
                    if (Array.IndexOf(zeroValues, 0.0f) != 0) return 30;

                    try { Array.IndexOf(duplicates, 8, -1); return 19; }
                    catch (ArgumentOutOfRangeException) { }
                    try { Array.IndexOf(duplicates, 8, 4, 1); return 20; }
                    catch (ArgumentOutOfRangeException) { }
                    try { Array.IndexOf(duplicates, 8, 0, -1); return 21; }
                    catch (ArgumentOutOfRangeException) { }
                    try { Array.IndexOf(duplicates, 8, 2, 3); return 22; }
                    catch (ArgumentOutOfRangeException) { }
                    int32[]? missing = null;
                    try { Array.IndexOf(missing, 8); return 23; }
                    catch (ArgumentNullException) { }
                    try { Array.IndexOf(missing, 8, 0); return 26; }
                    catch (ArgumentNullException) { }
                    try { Array.IndexOf(missing, 8, 0, 0); return 27; }
                    catch (ArgumentNullException) { }
                    try { Array.IndexOf(duplicates, 8, 2147483647); return 31; }
                    catch (ArgumentOutOfRangeException) { }
                    try { Array.IndexOf(duplicates, 8, 0, 2147483647); return 32; }
                    catch (ArgumentOutOfRangeException) { }
                    try
                    {
                        ThrowingKey?[] throwingKeys = { new ThrowingKey() };
                        Array.IndexOf(throwingKeys, throwingKeys[0]);
                        return 34;
                    }
                    catch (Exception) { }
                    try
                    {
                        ThrowingKey?[] throwingKeys = { new ThrowingKey() };
                        Array.IndexOf(throwingKeys, new ThrowingKey());
                        return 33;
                    }
                    catch (Exception) { }
                    return 0;
                }
            }
            """;

        var result = LumaCompiler.Compile(runtimeSources.Append(("array-indexof-test.ls", source)));
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));

        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.True(execution.ExitCode == 0,
            $"Exit code {execution.ExitCode}; stderr: {LlvmJit.CapturedStderr}; interop: {execution.InteropError}");
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ArrayIndexOfRejectsUserValueTypesWithoutExplicitEquality()
    {
        const string source = "struct Point { public int32 X; } class Program { public static int32 Main() { Point[] values = { Point() }; return Array.IndexOf(values, Point()); } }";
        var runtimeRoot = Path.GetDirectoryName(RuntimeSourcePath("LumaSharp.Library/System/Object.ls"))!;
        var runtimeSources = Directory.EnumerateFiles(runtimeRoot, "*.ls", SearchOption.AllDirectories)
            .Select(path => (path, File.ReadAllText(path)));
        var result = LumaCompiler.Compile(runtimeSources.Append(("array-indexof-no-equality-test.ls", source)));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS1102" && diagnostic.Message.Contains("IEquality", StringComparison.Ordinal));
        Assert.Null(result.LlvmIr);
    }

    [Fact]
    public void ArrayIndexOfRejectsRectangularArrays()
    {
        const string source = "class Program { public static int32 Main() { int32[,] values = new int32[2, 2]; return Array.IndexOf(values, 1); } }";
        var runtimeRoot = Path.GetDirectoryName(RuntimeSourcePath("LumaSharp.Library/System/Object.ls"))!;
        var runtimeSources = Directory.EnumerateFiles(runtimeRoot, "*.ls", SearchOption.AllDirectories)
            .Select(path => (path, File.ReadAllText(path)));
        var result = LumaCompiler.Compile(runtimeSources.Append(("array-indexof-rectangular-test.ls", source)));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.Null(result.LlvmIr);
    }

    [Fact]
    public void ArrayIndexOfSimdPathsPreserveLaneOrderAndScalarTails()
    {
        var runtimeRoot = Path.GetDirectoryName(RuntimeSourcePath("LumaSharp.Library/System/Object.ls"))!;
        var runtimeSources = Directory.EnumerateFiles(runtimeRoot, "*.ls", SearchOption.AllDirectories)
            .Select(path => (path, File.ReadAllText(path))).ToArray();
        const string source = """
            using System;
            enum Tint : uint8 { Dark, Light, Bright }
            public class Program
            {
                public static int32 Main()
                {
                    int8[] bytes = new int8[70];
                    uint8[] unsignedBytes = new uint8[70];
                    int16[] shorts = new int16[70];
                    uint16[] unsignedShorts = new uint16[70];
                    char[] characters = new char[70];
                    int32[] integers = new int32[70];
                    uint32[] unsignedIntegers = new uint32[70];
                    int64[] longs = new int64[10];
                    uint64[] unsignedLongs = new uint64[10];
                    int[] nativeIntegers = new int[10];
                    uint[] nativeUnsignedIntegers = new uint[10];
                    Tint[] tints = new Tint[70];
                    float[] singles = new float[38];
                    double[] doubles = new double[10];
                    bool[] flags = new bool[70];
                    int8[] unrolledBytes = new int8[256];
                    uint8[] unrolledUnsignedBytes = new uint8[256];
                    bool[] unrolledFlags = new bool[256];
                    Tint[] unrolledTints = new Tint[256];
                    for (int32 i = 0; i < bytes.Length; i++) bytes[i] = (int8)i;
                    for (int32 i = 0; i < shorts.Length; i++) shorts[i] = (int16)i;
                    for (int32 i = 0; i < integers.Length; i++) integers[i] = i;
                    for (int32 i = 0; i < longs.Length; i++) longs[i] = (int64)i;
                    for (int32 i = 0; i < singles.Length; i++) singles[i] = (float)i;
                    for (int32 i = 0; i < doubles.Length; i++) doubles[i] = (double)i;
                    flags[63] = true;
                    flags[69] = true;
                    unrolledBytes[5] = 1; unrolledBytes[37] = 2; unrolledBytes[75] = 3; unrolledBytes[110] = 4;
                    unrolledBytes[133] = 5; unrolledBytes[165] = 6; unrolledBytes[205] = 7; unrolledBytes[250] = 8;
                    unrolledUnsignedBytes[5] = (uint8)1; unrolledUnsignedBytes[37] = (uint8)2;
                    unrolledUnsignedBytes[75] = (uint8)3; unrolledUnsignedBytes[110] = (uint8)4;
                    unrolledUnsignedBytes[133] = (uint8)5; unrolledUnsignedBytes[165] = (uint8)6;
                    unrolledUnsignedBytes[205] = (uint8)7; unrolledUnsignedBytes[250] = (uint8)8;
                    unrolledFlags[110] = true; unrolledFlags[238] = true;
                    for (int32 i = 0; i < unrolledTints.Length; i++) unrolledTints[i] = Tint.Bright;
                    unrolledTints[110] = Tint.Dark; unrolledTints[238] = Tint.Dark;
                    unsignedBytes[63] = (uint8)3;
                    unsignedBytes[69] = (uint8)3;
                    unsignedShorts[31] = (uint16)3;
                    unsignedShorts[37] = (uint16)3;
                    unsignedShorts[63] = (uint16)4;
                    unsignedShorts[69] = (uint16)4;
                    characters[15] = 'b';
                    characters[16] = 'c';
                    characters[31] = 'd';
                    characters[37] = 'z';
                    characters[63] = 'e';
                    characters[69] = 'x';
                    unsignedIntegers[31] = (uint32)3;
                    unsignedIntegers[63] = (uint32)3;
                    unsignedIntegers[69] = (uint32)4;
                    unsignedLongs[7] = (uint64)3;
                    unsignedLongs[9] = (uint64)3;
                    nativeIntegers[7] = (int)3;
                    nativeIntegers[9] = (int)3;
                    nativeUnsignedIntegers[7] = (uint)3;
                    nativeUnsignedIntegers[9] = (uint)3;
                    tints[63] = Tint.Bright;
                    tints[69] = Tint.Bright;
                    integers[5] = 99;
                    integers[12] = 99;
                    singles[10] = Float.NaN;
                    doubles[8] = Double.NaN;

                    if (Array.IndexOf(flags, true) != 63 || Array.IndexOf(flags, false) != 0) return 10;
                    if (Array.IndexOf(unrolledBytes, (int8)1) != 5 || Array.IndexOf(unrolledBytes, (int8)2) != 37 ||
                        Array.IndexOf(unrolledBytes, (int8)3) != 75 || Array.IndexOf(unrolledBytes, (int8)4) != 110 ||
                        Array.IndexOf(unrolledBytes, (int8)5) != 133 || Array.IndexOf(unrolledBytes, (int8)6) != 165 ||
                        Array.IndexOf(unrolledBytes, (int8)7) != 205 || Array.IndexOf(unrolledBytes, (int8)8) != 250 ||
                        Array.IndexOf(unrolledBytes, (int8)9) != -1) return 21;
                    if (Array.IndexOf(unrolledUnsignedBytes, (uint8)1) != 5 || Array.IndexOf(unrolledUnsignedBytes, (uint8)2) != 37 ||
                        Array.IndexOf(unrolledUnsignedBytes, (uint8)3) != 75 || Array.IndexOf(unrolledUnsignedBytes, (uint8)4) != 110 ||
                        Array.IndexOf(unrolledUnsignedBytes, (uint8)5) != 133 || Array.IndexOf(unrolledUnsignedBytes, (uint8)6) != 165 ||
                        Array.IndexOf(unrolledUnsignedBytes, (uint8)7) != 205 || Array.IndexOf(unrolledUnsignedBytes, (uint8)8) != 250 ||
                        Array.IndexOf(unrolledUnsignedBytes, (uint8)9) != -1) return 22;
                    if (Array.IndexOf(unrolledFlags, true) != 110 || Array.IndexOf(unrolledFlags, true, 32) != 110 ||
                        Array.IndexOf(unrolledFlags, true, 64) != 110 || Array.IndexOf(unrolledFlags, true, 96) != 110 ||
                        Array.IndexOf(unrolledFlags, true, 128) != 238) return 23;
                    if (Array.IndexOf(unrolledTints, Tint.Dark) != 110 || Array.IndexOf(unrolledTints, Tint.Dark, 32) != 110 ||
                        Array.IndexOf(unrolledTints, Tint.Dark, 64) != 110 || Array.IndexOf(unrolledTints, Tint.Dark, 96) != 110 ||
                        Array.IndexOf(unrolledTints, Tint.Dark, 128) != 238) return 24;
                    if (Array.IndexOf(bytes, (int8)0) != 0 || Array.IndexOf(bytes, (int8)15) != 15 || Array.IndexOf(bytes, (int8)16) != 16 || Array.IndexOf(bytes, (int8)63) != 63 || Array.IndexOf(bytes, (int8)69) != 69 || Array.IndexOf(bytes, (int8)-1) != -1) return 1;
                    if (Array.IndexOf(unsignedBytes, (uint8)3) != 63 || Array.IndexOf(unsignedBytes, (uint8)2) != -1) return 11;
                    if (Array.IndexOf(shorts, (int16)7) != 7 || Array.IndexOf(shorts, (int16)8) != 8 || Array.IndexOf(shorts, (int16)31) != 31 || Array.IndexOf(shorts, (int16)37) != 37 || Array.IndexOf(shorts, (int16)-1) != -1) return 2;
                    if (Array.IndexOf(unsignedShorts, (uint16)3) != 31 || Array.IndexOf(unsignedShorts, (uint16)2) != -1) return 12;
                    if (Array.IndexOf(unsignedShorts, (uint16)4) != 63) return 20;
                    if (Array.IndexOf(characters, 'b') != 15 || Array.IndexOf(characters, 'c') != 16 || Array.IndexOf(characters, 'd') != 31 || Array.IndexOf(characters, 'z') != 37 || Array.IndexOf(characters, 'e') != 63 || Array.IndexOf(characters, 'x') != 69) return 13;
                    if (Array.IndexOf(integers, 0) != 0 || Array.IndexOf(integers, 3) != 3 || Array.IndexOf(integers, 4) != 4 || Array.IndexOf(integers, 15) != 15 || Array.IndexOf(integers, 16) != 16 || Array.IndexOf(integers, 31) != 31 || Array.IndexOf(integers, 32) != 32 || Array.IndexOf(integers, 63) != 63 || Array.IndexOf(integers, 64) != 64 || Array.IndexOf(integers, 69) != 69 || Array.IndexOf(integers, -1) != -1) return 3;
                    if (Array.IndexOf(unsignedIntegers, (uint32)3) != 31 || Array.IndexOf(unsignedIntegers, (uint32)4) != 69 || Array.IndexOf(unsignedIntegers, (uint32)2) != -1) return 14;
                    if (Array.IndexOf(longs, (int64)1) != 1 || Array.IndexOf(longs, (int64)2) != 2 || Array.IndexOf(longs, (int64)7) != 7 || Array.IndexOf(longs, (int64)9) != 9 || Array.IndexOf(longs, (int64)-1) != -1) return 4;
                    if (Array.IndexOf(unsignedLongs, (uint64)3) != 7 || Array.IndexOf(unsignedLongs, (uint64)2) != -1) return 15;
                    if (Array.IndexOf(nativeIntegers, (int)3) != 7 || Array.IndexOf(nativeIntegers, (int)2) != -1) return 16;
                    if (Array.IndexOf(nativeUnsignedIntegers, (uint)3) != 7 || Array.IndexOf(nativeUnsignedIntegers, (uint)2) != -1) return 17;
                    if (Array.IndexOf(tints, Tint.Bright) != 63 || Array.IndexOf(tints, Tint.Dark) != 0) return 18;
                    if (Array.IndexOf(singles, 3.0f) != 3 || Array.IndexOf(singles, 4.0f) != 4 || Array.IndexOf(singles, 15.0f) != 15 || Array.IndexOf(singles, 17.0f) != 17 || Array.IndexOf(singles, 31.0f) != 31 || Array.IndexOf(singles, 32.0f) != 32 || Array.IndexOf(singles, 37.0f) != 37) return 5;
                    if (Array.IndexOf(singles, Float.NaN) != 10) return 9;
                    if (Array.IndexOf(doubles, 1.0) != 1 || Array.IndexOf(doubles, 2.0) != 2 || Array.IndexOf(doubles, 7.0) != 7 || Array.IndexOf(doubles, 9.0) != 9) return 6;
                    if (Array.IndexOf(doubles, Double.NaN) != 8) return 19;
                    if (Array.IndexOf(integers, 16, 16, 1) != 16 || Array.IndexOf(integers, 15, 16, 6) != -1) return 7;
                    if (Array.IndexOf(integers, 99) != 5 || Array.IndexOf(integers, 99, 6, 7) != 12) return 8;
                    return 0;
                }
            }
            """;

        var result = LumaCompiler.Compile(runtimeSources.Append(("array-indexof-vector-test.ls", source)), new CompilationOptions(Optimize: true));
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("declare i128 @llvm.cttz.i128(i128, i1 immarg)", result.LlvmIr, StringComparison.Ordinal);
        Assert.Contains("shl i128", result.LlvmIr, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.True(execution.ExitCode == 0,
            $"Exit code {execution.ExitCode}; stderr: {LlvmJit.CapturedStderr}; interop: {execution.InteropError}");
        Assert.Null(execution.InteropError);

        var bigEndianResult = LumaCompiler.Compile(runtimeSources.Append(("array-indexof-vector-big-endian-test.ls", source)),
            new CompilationOptions(TargetTriple: "aarch64_be-unknown-linux-gnu"));
        Assert.True(bigEndianResult.Succeeded, string.Join(Environment.NewLine, bigEndianResult.Diagnostics));
        Assert.Contains("@llvm.bitreverse.i32", bigEndianResult.LlvmIr, StringComparison.Ordinal);
    }

    private static string RuntimeSourcePath(string relativePath)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate)) return candidate;
        }
        throw new FileNotFoundException($"Could not locate runtime source '{relativePath}'.");
    }
}
