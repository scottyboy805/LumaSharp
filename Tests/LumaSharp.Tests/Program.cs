using LumaSharp.Compiler;
using System.Text;
using Xunit;

namespace LumaSharp.Tests;

public sealed class CompilerTests
{
    private static CompilationResult Compile(string source, CompilationOptions? options = null) => LumaCompiler.Compile([("test.ls", source)], options);
    private static string RuntimeSourcePath(string relativePath)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate)) return candidate;
        }
        throw new FileNotFoundException($"Could not locate runtime source '{relativePath}'.");
    }

    [Theory]
    [InlineData("private class Program { public static int32 Main() { return 0; } }")]
    [InlineData("protected class Program { public static int32 Main() { return 0; } }")]
    [InlineData("public private class Program { public static int32 Main() { return 0; } }")]
    [InlineData("public protected class Program { public static int32 Main() { return 0; } }")]
    [InlineData("public public class Program { public static int32 Main() { return 0; } }")]
    [InlineData("class Program { public private int32 value; public static int32 Main() { return 0; } }")]
    [InlineData("class Program { private internal int32 value; public static int32 Main() { return 0; } }")]
    [InlineData("class Program { public static const int32 Value = 1; public static int32 Main() { return 0; } }")]
    [InlineData("class Program { public readonly const int32 Value = 1; public static int32 Main() { return 0; } }")]
    [InlineData("class Program { public static virtual int32 Value() { return 0; } public static int32 Main() { return 0; } }")]
    [InlineData("static struct Data {} class Program { public static int32 Main() { return 0; } }")]
    [InlineData("class Program { public readonly int32 Value() { return 1; } public static int32 Main() { return 0; } }")]
    [InlineData("class Program { public virtual int32 Value; public static int32 Main() { return 0; } }")]
    public void InvalidModifiersAreRejectedForTheirDeclarationKind(string source)
    {
        var result = Compile(source);
        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS0001");
    }

    [Theory]
    [InlineData("public class Program { public static int32 Main() { return 0; } }")]
    [InlineData("internal class Program { public static int32 Main() { return 0; } }")]
    [InlineData("public virtual class Base { protected internal this() {} } class Program : Base { public static int32 Main() { return 0; } }")]
    [InlineData("public virtual class Base { private protected this() {} } class Program : Base { public static int32 Main() { return 0; } }")]
    public void ValidAccessibilityModifierFormsAreAccepted(string source)
    {
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
    }

    [Theory]
    [InlineData("class")]
    [InlineData("class Program {")]
    [InlineData("class Program { public static int32 Main( { return ; }")]
    [InlineData("enum E { A = 0xZZ } class Program { public static int32 Main() { return 0; } }")]
    [InlineData("class Program { public static int32 Main() { return ( ; } }")]
    [InlineData("interface I { int32 M( ; } class Program { public static int32 Main() { return 0; } }")]
    public void MalformedSourcesReturnStructuredDiagnosticsWithoutLeakingFrontEndExceptions(string source)
    {
        var result = Compile(source);
        Assert.False(result.Succeeded);
        Assert.Null(result.LlvmIr);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Code == "LMC9000");
    }

    [Fact]
    public void DirectCompilerApiPreservesCancellationDuringFrontEndWork()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => LumaCompiler.Compile(
            [("cancelled.ls", "class Program { public static int32 Main() { return 0; } }")],
            cancellationToken: cancellation.Token));
    }

    [Fact]
    public void RuntimeLibrarySuppliesCompilerKnownTypesAndTheirMembers()
    {
        var runtimeRoot = Path.GetDirectoryName(RuntimeSourcePath("LumaSharp.Library/System/Object.ls"))!;
        var runtimeSources = Directory.EnumerateFiles(runtimeRoot, "*.ls", SearchOption.AllDirectories)
            .Select(path => (path, File.ReadAllText(path)));
        var result = LumaCompiler.Compile(runtimeSources.Append(("program.ls",
            "class Program { public static int32 Main() { int32[] values = new int32[1]; System.Array array = values; object root = array; try { throw new System.Exception(\"runtime\"); } catch (System.Exception error) { return array.Length == 1 && error.Message == \"runtime\" ? 0 : 1; } } }")));

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
    }

    [Fact]
    public void PrimitiveRuntimeTypesAreValueTypeStructsAndBoxImplicitlyToObject()
    {
        var runtimeRoot = Path.GetDirectoryName(RuntimeSourcePath("LumaSharp.Library/System/Object.ls"))!;
        var runtimeSources = Directory.EnumerateFiles(runtimeRoot, "*.ls", SearchOption.AllDirectories)
            .Select(path => (path, File.ReadAllText(path)));
        var result = LumaCompiler.Compile(runtimeSources.Append(("primitive-value-type-test.ls", """
            using System;

            public class Program
            {
                public static int32 Main()
                {
                    ValueType marker = default;
                    object boxed = 42;
                    return Int32.Parse("42") == 42 && Char.Parse("A") == 'A' &&
                        boxed is int32 ? 0 : 1;
                }
            }
            """)));

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("%struct.System_ValueType = type", result.LlvmIr!, StringComparison.Ordinal);
        var integerBoxDescriptor = Assert.Single(result.LlvmIr!.Split('\n').Where(line => line.StartsWith("@luma_boxdesc_int32 =", StringComparison.Ordinal)));
        Assert.Contains("ptr @luma_classdesc_System_Object", integerBoxDescriptor, StringComparison.Ordinal);
    }

    [Fact]
    public void StructsMayUseOnlyValueTypeAsTheirSpecialClassBase()
    {
        var result = Compile("virtual class Base {} struct Invalid : Base {} class Program { public static int32 Main() { return 0; } }");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS0001" && diagnostic.Message.Contains("System.ValueType", StringComparison.Ordinal));
    }

    [Fact]
    public void CollectionsNamespaceTypesBuildAndRunWithMovedIterationContracts()
    {
        var runtimeRoot = Path.GetDirectoryName(RuntimeSourcePath("LumaSharp.Library/System/Object.ls"))!;
        var runtimeSources = Directory.EnumerateFiles(runtimeRoot, "*.ls", SearchOption.AllDirectories)
            .Select(path => (path, File.ReadAllText(path)));
        var result = LumaCompiler.Compile(runtimeSources.Append(("collections-runtime-test.ls", """
            using System.Collections;

            public class TestKey : System.IEquality<TestKey>, System.IHashable
            {
                public int32 value;

                public this(int32 value)
                {
                    this.value = value;
                }

                public bool Equals(TestKey other)
                {
                    return value == other.value;
                }

                public int32 Hash
                {
	                get => value;
                }
            }

            public class TestIterator : IIterator<int32>
            {
                private int32 _index;

                public this()
                {
                    _index = -1;
                }

                public bool Next() mut
                {
                    _index++;
                    return _index < 2;
                }

                public int32 Current
                {
                    get => _index + 1;
                }
            }

            public class TestValues : IEnumerable<int32>
            {
                public IIterator<int32> Iterate()
                {
                    return new TestIterator();
                }
            }

            public class Program
            {
                public static unsafe int32 Main()
                {
                    List<int32> list = new List<int32>();
                    list.Add(10);
                    list.Add(20);
                    list.Insert(1, 15);
                    bool contains = list.Contains(15);
                    int32 foundIndex = list.IndexOf(20);
                    int32* firstItem = list.Ptr;
                    int32 pointerValue = *firstItem;
                    int32[] copiedValues = list.ToArray();
                    int32 total = 0;
                    for (let item in new TestValues())
                        total += item;

                    Stack<int32> stack = new Stack<int32>();
                    stack.Push(1);
                    stack.Push(2);
                    int32 stackTotal = 0;
                    for (let item in stack)
                        stackTotal += item;
                    Queue<int32> queue = new Queue<int32>();
                    queue.Enqueue(3);
                    queue.Enqueue(4);
                    int32 queueTotal = 0;
                    for (let item in queue)
                        queueTotal += item;

                    Dictionary<TestKey?, string?> map = new Dictionary<TestKey?, string?>(1);
                    map.Add(new TestKey(7), "seven");
                    bool inserted = map.TryAdd(new TestKey(20), "twenty");
                    int32 keyIndex = 0;
                    while (keyIndex < 12)
                    {
                        if (keyIndex != 7)
                            map[new TestKey(keyIndex)] = "item";
                        keyIndex++;
                    }
                    bool found = map.TryGetValue(new TestKey(7), out string? value);
                    HashSet<TestKey?> set = new HashSet<TestKey?>(1);
                    bool added = set.Add(new TestKey(9));
                    bool duplicate = set.Add(new TestKey(9));
                    int32 setCount = 0;
                    for (let item in set)
                        setCount++;

                    return list.Count == 3 && list[1] == 15 && contains && foundIndex == 2 && pointerValue == 10 &&
                        copiedValues.Length == 3 && copiedValues[1] == 15 && inserted &&
                        stack.Pop() == 2 && stack.Pop() == 1 &&
                        queue.Dequeue() == 3 && queue.Dequeue() == 4 &&
                        total == 3 && stackTotal == 3 && queueTotal == 7 &&
                        found && value == "seven" && map.ContainsKey(new TestKey(11)) &&
                        map.Remove(new TestKey(11)) && added && !duplicate &&
                        setCount == 1 && set.Contains(new TestKey(9)) ? 0 : 1;
                }
            }
            """)), new CompilationOptions(UnsafeEnabled: true));

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
    }

    [Fact]
    public void RuntimeStringSupportsLiteralAndOwnedStrings()
    {
        var runtimeRoot = Path.GetDirectoryName(RuntimeSourcePath("LumaSharp.Library/System/Object.ls"))!;
        var runtimeSources = Directory.EnumerateFiles(runtimeRoot, "*.ls", SearchOption.AllDirectories)
            .Select(path => (path, File.ReadAllText(path)));
        var result = LumaCompiler.Compile(runtimeSources.Append(("string-runtime-test.ls", """
            public class Program
            {
                public static int32 Main()
                {
                    string literal = "A🙂Z";
                    string middle = literal.Substring(1, 2);
                    char[] characters = new char[3];
                    characters[0] = 'c';
                    characters[1] = 'a';
                    characters[2] = 't';
                    string owned = new string(characters);
                    string repeated = new string('x', 3);
                    string replaced = owned.Replace('a', 'A');
                    string trimmed = "  cat\t".Trim();
                    return literal.Length == 4 && middle == "🙂" && owned == "cat" &&
                        replaced == "cAt" && repeated == "xxx" && trimmed == "cat" &&
                        owned.IndexOf("at") == 1 && owned.StartsWith("c") && owned.EndsWith("t") ? 0 : 1;
                }
            }
            """)));

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
    }

    [Fact]
    public void PrimitiveAliasesExposeBoundsParsingAndStringHelpers()
    {
        var runtimeRoot = Path.GetDirectoryName(RuntimeSourcePath("LumaSharp.Library/System/Object.ls"))!;
        var runtimeSources = Directory.EnumerateFiles(runtimeRoot, "*.ls", SearchOption.AllDirectories)
            .Select(path => (path, File.ReadAllText(path)));
        var source = """
            public class Program
            {
                public static int32 Main()
                {
                    int32 top = int32.MaxValue;
                    int32 parsed = int32.Parse("-2147483648");
                    bool parsedBool = bool.Parse("tRuE");
                    bool valid = int32.TryParse("2147483647", out int32 maximum);
                    bool invalid = int32.TryParse("2147483648", out int32 ignored);
                    int nativeMin = int.MinValue;
                    uint nativeMax = uint.MaxValue;
                    int8 smallMin = int8.MinValue;
                    char letter = char.Parse("A");
                    string empty = string.Empty;
                    double precise = double.Parse("1.25e2");
                    float single = float.Parse("1.5");
                    string formatted = (42).ToString();
                    return top == 2147483647 && parsed == int32.MinValue && parsedBool && valid && maximum == top &&
                        !invalid && nativeMin < 0 && nativeMax > 0 && smallMin == -128 && letter == 'A' &&
                        empty.Length == 0 && precise == 125.0 && single == 1.5f && formatted == "42" &&
                        string.IsNullOrWhiteSpace(" \t") ? 0 : 1;
                }
            }
            """;
        var result = LumaCompiler.Compile(runtimeSources.Append(("primitive-runtime-test.ls", source)));

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public async Task DirectCompilerApiStopsDuringLongLexicalInputWhenCancelled()
    {
        var source = "/*" + new string('x', 8_000_000) + "*/";
        using var cancellation = new CancellationTokenSource();
        using var started = new ManualResetEventSlim();
        var compile = Task.Run(() =>
        {
            started.Set();
            return LumaCompiler.Compile([("long-comment.ls", source)], cancellationToken: cancellation.Token);
        });
        started.Wait();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => compile);
    }

    [Fact]
    public void LlvmObjectEmissionPreservesCancellationAndDoesNotCreateAnArtifact()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var outputPath = Path.Combine(Path.GetTempPath(), "lumasharp-cancelled-" + Guid.NewGuid().ToString("N") + ".obj");
        Assert.Throws<OperationCanceledException>(() => LlvmToolchain.VerifyAndEmitObject(
            "not valid LLVM IR", outputPath, cancellationToken: cancellation.Token));
        Assert.False(File.Exists(outputPath));
    }

    [Fact]
    public void DirectCompilerApiCollectsRecoverableDiagnosticsAcrossSourceFiles()
    {
        var result = LumaCompiler.Compile([
            ("first.ls", "class"),
            ("second.ls", "class Broken {")
        ]);

        Assert.False(result.Succeeded);
        Assert.Null(result.LlvmIr);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Location?.File == "first.ls");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Location?.File == "second.ls");
        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Code == "LMC9000");
    }

    [Fact]
    public void StaticClassesSupportStaticMethodsWithoutAnInstanceConstructor()
    {
        const string source = "static class MathTools { public static int32 Add(int32 left, int32 right) { return left + right; } } class Program { public static int32 Main() { return MathTools.Add(19, 23) == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
    }

    [Theory]
    [InlineData("static class Tools { int32 value; }")]
    [InlineData("static class Tools { public int32 GetValue() { return 1; } }")]
    [InlineData("static class Tools {} class Program { public static int32 Main() { Tools value = new Tools(); return 0; } }")]
    [InlineData("static class Tools {} class Program : Tools { public static int32 Main() { return 0; } }")]
    public void StaticClassesRejectInstanceMembersInstantiationAndInheritance(string source)
    {
        var result = Compile(source);
        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS0001");
    }

    [Fact]
    public void ComputedClassPropertiesReadWriteAndSupportExpressionGetters()
    {
        const string source = "class Box { public int32 Value { get { return field; } set { field = value; } } public int32 Twice => Value * 2; } class Program { public static int32 Main() { Box box = new Box(); box.Value = 21; return box.Twice == 42 && box.Value == 21 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);

    }

    [Fact]
    public void ComputedStructPropertiesReadWriteThroughMutableReceivers()
    {
        const string source = "struct Box { int32 stored; public int32 Value { get => stored; set { stored = value; } } } class Program { public static int32 Main() { Box box = default; box.Value = 42; return box.Value == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
    }

    [Fact]
    public void StaticComputedPropertiesUseStaticAccessorCalls()
    {
        const string source = "static class Constants { public static int32 Value { get { return field; } set { field = value; } } public static int32 Twice => Value * 2; } class Program { public static int32 Main() { Constants.Value = 42; return Constants.Value == 42 && Constants.Twice == 84 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
    }

    [Fact]
    public void SetOnlyComputedPropertyCanBeAssignedWithoutInvokingGetter()
    {
        const string source = "class Box { int32 stored; public int32 Value { set { stored = value; } } } class Program { public static int32 Main() { Box box = new Box(); box.Value = 42; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);

        var auto = Compile("class Box { public int32 Value { set; } } class Program { public static int32 Main() { Box box = new Box(); box.Value = 42; return 0; } }");
        Assert.True(auto.Succeeded, string.Join(Environment.NewLine, auto.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(auto.LlvmIr!).ExitCode);
    }

    [Fact]
    public void VirtualComputedPropertyDispatchesToOverrides()
    {
        const string source = "virtual class Base { public virtual int32 Value => 1; } class Derived : Base { public override int32 Value => 42; } class Program { public static int32 Main() { Base item = new Derived(); return item.Value == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
    }

    [Theory]
    [InlineData("class Box { int32 stored; public int32 Value { get { return stored; } } } class Program { public static int32 Main() { Box box = new Box(); box.Value = 1; return 0; } }")]
    [InlineData("class Box { int32 stored; public int32 Value { get { return stored; } set { stored = value; } } public void Bad() { this.Value = 1; } } class Program { public static int32 Main() { return 0; } }")]
    [InlineData("struct Box { int32 stored; public int32 Value { get { return stored; } set { stored = value; } } } class Program { public static int32 Main() { let Box box = default; box.Value = 1; return 0; } }")]
    public void ComputedPropertyWritesEnforceSetterAndReceiverMutabilityRules(string source)
    {
        var result = Compile(source);
        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code is "LMC3012" or "LS0821" or "LS0823");
    }

    [Fact]
    public void ComputedClassIndexerReadsWritesAndSupportsMultipleIndices()
    {
        const string source = "class Grid { int32 stored; public int32 this[int32 row, int32 column] { get => stored + row + column; set { stored = value - row - column; } } } class Program { public static int32 Main() { Grid grid = new Grid(); grid[2, 3] = 42; return grid[2, 3] == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void ComputedStructIndexerMutatesMutableReceivers()
    {
        const string source = "struct Cell { int32 stored; public int32 this[int32 index] { get => stored + index; set { stored = value - index; } } } class Program { public static int32 Main() { Cell cell = default; cell[2] = 42; return cell[2] == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
    }

    [Fact]
    public void ComputedIndexersOverloadByParameterTypes()
    {
        const string source = "class Lookup { public int32 this[int32 index] => index + 10; public int32 this[char index] => (int32)index + 20; } class Program { public static int32 Main() { Lookup lookup = new Lookup(); return lookup[2] == 12 && lookup['A'] == 85 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
    }

    [Fact]
    public void SetOnlyIndexerCanBeAssignedWithoutAGetter()
    {
        const string source = "class Sink { int32 stored; public int32 this[int32 index] { set { stored = value + index; } } public int32 Read() { return stored; } } class Program { public static int32 Main() { Sink sink = new Sink(); sink[2] = 40; return sink.Read() == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
    }

    [Fact]
    public void ComputedClassPropertiesAndIndexersSupportSingleEvaluationCompoundAssignment()
    {
        const string source = "class Box { int32 stored = 10; string text = \"\"; public int32 Value { get { Program.GetterCalls++; return stored; } set { stored = value; } } public string Text { get => text; set { text = value; } } public int32 this[int32 index] { get { Program.IndexGetterCalls++; return stored + index; } set { stored = value - index; } } } class Program { public static int32 GetterCalls; public static int32 IndexGetterCalls; public static int32 ReceiverCalls; public static int32 IndexArgumentCalls; public static Box GetBox() { Program.ReceiverCalls++; return new Box(); } public static int32 NextIndex() { Program.IndexArgumentCalls++; return 0; } public static int32 Main() { Box box = new Box(); box.Value += 2; if (Program.GetterCalls != 1 || box.Value != 12) return 1; Program.GetterCalls = 0; GetBox().Value += 3; if (Program.ReceiverCalls != 1 || Program.GetterCalls != 1) return 2; Program.IndexGetterCalls = 0; Program.IndexArgumentCalls = 0; box[NextIndex()] += 2; if (Program.IndexGetterCalls != 1 || Program.IndexArgumentCalls != 1) return 3; box.Text += \"ok\"; if (box.Text != \"ok\") return 4; return box[0] == 14 ? 0 : 5; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void ComputedStructPropertiesAndIndexersSupportCompoundAssignment()
    {
        const string source = "struct Cell { int32 stored; public int32 Value { get => stored; set { stored = value; } } public int32 this[int32 index] { get => stored + index; set { stored = value - index; } } } class Program { public static int32 Main() { Cell cell = default; cell.Value = 10; cell.Value += 2; cell[3] += 4; return cell.Value == 16 && cell[3] == 19 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
    }

    [Fact]
    public void ComputedNullablePropertiesAndIndexersSupportCoalescingAssignment()
    {
        const string source = "class TextBox { string? stored; string? indexed; public string? Value { get { Program.GetterCalls++; return stored; } set { Program.SetterCalls++; stored = value; } } public string? this[int32 index] { get { Program.IndexGetterCalls++; return indexed; } set { Program.IndexSetterCalls++; indexed = value; } } } class Program { public static int32 GetterCalls; public static int32 SetterCalls; public static int32 IndexGetterCalls; public static int32 IndexSetterCalls; public static int32 Main() { TextBox box = new TextBox(); string? first = box.Value ??= \"first\"; if (first != \"first\" || Program.GetterCalls != 1 || Program.SetterCalls != 1) return 1; Program.GetterCalls = 0; Program.SetterCalls = 0; string? existing = box.Value ??= \"second\"; if (existing != \"first\" || Program.GetterCalls != 1 || Program.SetterCalls != 0) return 2; string? indexValue = box[0] ??= \"indexed\"; if (indexValue != \"indexed\" || Program.IndexGetterCalls != 1 || Program.IndexSetterCalls != 1) return 3; Program.IndexGetterCalls = 0; Program.IndexSetterCalls = 0; string? indexExisting = box[0] ??= \"unused\"; return indexExisting == \"indexed\" && Program.IndexGetterCalls == 1 && Program.IndexSetterCalls == 0 ? 0 : 4; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void ComputedNullableClassAndValuePropertiesCoalesceWithCorrectOwnership()
    {
        const string source = "class Node { } class Container { Node? node; int32? count; public Node? Current { get { Program.NodeReads++; return node; } set { Program.NodeWrites++; node = value; } } public int32? Count { get => count; set { count = value; } } } class Program { public static int32 NodeReads; public static int32 NodeWrites; public static int32 Main() { Container container = new Container(); Node? first = container.Current ??= new Node(); if (first == null || Program.NodeReads != 1 || Program.NodeWrites != 1) return 1; Program.NodeReads = 0; Program.NodeWrites = 0; Node? same = container.Current ??= new Node(); if (same == null || Program.NodeReads != 1 || Program.NodeWrites != 0) return 2; int32? firstCount = container.Count ??= 7; if (firstCount != 7) return 3; int32? sameCount = container.Count ??= 8; return sameCount == 7 ? 0 : 4; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void NullableAutoPropertiesSupportCoalescingAssignment()
    {
        const string source = "class Box { public string? Value { get; set; } } class Program { public static int32 Main() { Box box = new Box(); string? first = box.Value ??= \"set\"; if (first != \"set\" || box.Value != \"set\") return 1; string? second = box.Value ??= \"unused\"; return second == \"set\" ? 0 : 2; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void NullableStringComputedPropertiesSupportCompoundConcatenation()
    {
        const string source = "class Box { string? text; public string? Text { get { Program.Reads++; return text; } set { Program.Writes++; text = value; } } } class Program { public static int32 Reads; public static int32 Writes; public static int32 Main() { Box box = new Box(); box.Text += \"value\"; if (box.Text != \"value\" || Program.Reads != 2 || Program.Writes != 1) return 1; box.Text += \"!\"; return box.Text == \"value!\" && Program.Reads == 4 && Program.Writes == 2 ? 0 : 2; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void AutoAndExplicitPropertyAccessorsCanBeMixedWithRestrictedSetter()
    {
        const string source = "class Box { public int32 Value { get => field; private set; } = 7; public void Update(int32 value) mut { Value = value; } } class Program { public static int32 Main() { Box box = new Box(); if (box.Value != 7) return 1; box.Update(42); return box.Value == 42 ? 0 : 2; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);

        var inaccessibleSetter = Compile("class Box { public int32 Value { get; private set; } } class Program { public static int32 Main() { Box box = new Box(); box.Value = 1; return 0; } }");
        Assert.False(inaccessibleSetter.Succeeded);
        Assert.Contains(inaccessibleSetter.Diagnostics, diagnostic => diagnostic.Code == "LMC3012");

        var invalidAccessorAccess = Compile("class Box { private int32 Value { public get => 1; } } class Program { public static int32 Main() { return 0; } }");
        Assert.False(invalidAccessorAccess.Succeeded);
        Assert.Contains(invalidAccessorAccess.Diagnostics, diagnostic => diagnostic.Code == "LS0001");

        var virtualAuto = Compile("virtual class Base { public virtual int32 Value { get; set; } = 3; } class Derived : Base { public override int32 Value { get; set; } = 9; } class Program { public static int32 Main() { Base value = new Derived(); if (value.Value != 9) return 1; value.Value = 12; return value.Value == 12 ? 0 : 2; } }");
        Assert.True(virtualAuto.Succeeded, string.Join(Environment.NewLine, virtualAuto.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(virtualAuto.LlvmIr!).ExitCode);
    }

    [Fact]
    public void ComputedIndexerAccessorsHonorTheirOwnAccessibility()
    {
        var readable = Compile("class Box { public int32 this[int32 index] { get => index + 1; private set { } } } class Program { public static int32 Main() { Box box = new Box(); return box[4] == 5 ? 0 : 1; } }");
        Assert.True(readable.Succeeded, string.Join(Environment.NewLine, readable.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(readable.LlvmIr!).ExitCode);

        var inaccessible = Compile("class Box { public int32 this[int32 index] { get => index; private set { } } } class Program { public static int32 Main() { Box box = new Box(); box[0] = 1; return 0; } }");
        Assert.False(inaccessible.Succeeded);
        Assert.Contains(inaccessible.Diagnostics, diagnostic => diagnostic.Code == "LMC3012");

        var invalid = Compile("class Box { private int32 this[int32 index] { public get => index; } } class Program { public static int32 Main() { return 0; } }");
        Assert.False(invalid.Succeeded);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == "LS0001");
    }

    [Fact]
    public void VirtualComputedIndexerDispatchesToOverride()
    {
        const string source = "virtual class Base { public virtual int32 this[int32 index] => 1; } class Derived : Base { public override int32 this[int32 index] => 42; } class Program { public static int32 Main() { Base value = new Derived(); return value[0] == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
    }

    [Fact]
    public void NullConditionalComputedIndexerReturnsNullableValue()
    {
        const string source = "class Box { public int32 this[int32 index] => 42; } class Program { public static int32? Read(Box? box) { return box?[0]; } public static int32 Main() { int32? absent = Read(null); if (absent != null) return 1; int32? present = Read(new Box()); if (present != 42) return 2; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void NullConditionalComputedPropertyReturnsNullableValue()
    {
        const string source = "class Box { public int32 Value => 42; } class Program { public static int32? Read(Box? box) { return box?.Value; } public static int32 Main() { return Read(null) == null && Read(new Box()) == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
    }

    [Fact]
    public void VirtualComputedIndexerSetterDispatchesToOverride()
    {
        const string source = "virtual class Base { public static int32 SetMarker; public virtual int32 this[int32 index] { get => 1; set { Base.SetMarker = 1; } } } class Derived : Base { public override int32 this[int32 index] { get => 42; set { Base.SetMarker = 2; } } } class Program { public static int32 Main() { Base item = new Derived(); item[0] = 9; return Base.SetMarker == 2 && item[0] == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
    }

    [Theory]
    [InlineData("class Box { public int32 this[int32 index] { get => index; } } class Program { public static int32 Main() { Box box = new Box(); box[0] = 1; return 0; } }")]
    [InlineData("class Box { public int32 this[int32 index] { set { } } } class Program { public static int32 Main() { Box box = new Box(); return box[0]; } }")]
    [InlineData("struct Box { int32 stored; public int32 this[int32 index] { get => stored; set { stored = value; } } } class Program { public static int32 Main() { let Box box = default; box[0] = 1; return 0; } }")]
    [InlineData("class Box { public int32 this[int32 index] => index; public int32 this[int32 other] => other; } class Program { public static int32 Main() { return 0; } }")]
    public void ComputedIndexerAccessorsAndReceiverMutabilityAreChecked(string source)
    {
        var result = Compile(source);
        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code is "LMC3012" or "LMC3013" or "LS0200" or "LS0823");
    }

    [Theory]
    [InlineData("class Box { public static int32 this[int32 index] => index; }")]
    [InlineData("class Box { public int32 this[int32 index] { get; } }")]
    [InlineData("interface IBox { int32 this[int32 index] { get; } }")]
    [InlineData("class Box { public int32 this[int32 index] { private get => index; } }")]
    public void UnsupportedIndexerFormsAndCompoundWritesProduceDiagnostics(string source)
    {
        var result = Compile(source);
        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void CompilesEntryPointAndCheckedArithmeticWithCorrectPrecedence()
    {
        var result = Compile("class Program { public static int32 Main() { let x = 1 + 2 * 3; return x; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var ir = result.LlvmIr!;
        Assert.True(ir.IndexOf("call { i32, i1 } @llvm.smul", StringComparison.Ordinal) < ir.IndexOf("call { i32, i1 } @llvm.sadd", StringComparison.Ordinal));
        Assert.Contains("ret i32", ir);
        Assert.Contains("luma_throw_overflow", ir);
    }

    [Fact]
    public void UnqualifiedMutableStaticFieldsSupportIncrementAndAssignment()
    {
        const string source = "class Program { public static int32 value; public static int32 Main() { value = 40; int32 prior = value++; ++value; return prior == 40 && value == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void OmitsManagedStringRuntimeFromNumericOnlyModules()
    {
        var result = Compile("class Program { public static int32 Main() { return 42; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.DoesNotContain("@malloc", result.LlvmIr!, StringComparison.Ordinal);
        Assert.DoesNotContain("@memcpy", result.LlvmIr, StringComparison.Ordinal);
        Assert.DoesNotContain("@luma_string_concat", result.LlvmIr, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("x86_64-pc-linux-gnu", 64)]
    [InlineData("i386-pc-linux-gnu", 32)]
    public void EmitsStringRuntimeForTargetPointerWidth(string targetTriple, int pointerBits)
    {
        const string source = "class Program { public static int32 Main() { string left = \"A\"; string right = \"β\"; return (left + right).Length; } }";
        var result = Compile(source, new CompilationOptions(TargetTriple: targetTriple));
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var ir = result.LlvmIr!;
        Assert.Contains($"declare ptr @malloc(i{pointerBits})", ir);
        Assert.Contains("%luma.string = type", ir);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-string-target-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try
        {
            var emitted = LlvmToolchain.VerifyAndEmitObject(ir, objectPath, targetTriple, optimize: true);
            Assert.True(emitted.Succeeded, emitted.Error);
        }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }
    }

    [Fact]
    public void ExecutesGeneratedStringRuntimeAndReleasesTemporaryObjects()
    {
        const string source = "class Program { public static int32 Main() { string seed = Make(\"A\"); string value = seed + \"β\"; value += value; if (value != \"AAβAAβ\") return 1; if (value.Length != 6) return 2; if ((int32)value[0] != 65) return 3; if ((int32)value[2] != 946) return 4; return 0; } public static string Make(string suffix) { return \"A\" + suffix; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

    }

    [Fact]
    public void ExecutesStringModuleWithNoFormattingCalls()
    {
        var result = Compile("class Program { public static int32 Main() { string value = \"x\" + \"y\"; return value.Length == 2 ? 0 : 1; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void CreatesIndexesAndReleasesOneDimensionalManagedArrays()
    {
        const string source = "class Program { public static int32 Main() { int32[] values = new int32[3]; if (values.Length != 3) return 1; if (values[0] != 0) return 2; values[1] = 41; values[1] += 1; if (values[1] != 42) return 3; if (values[2] != 0) return 4; int32[] inferred = new [2]; if (inferred.Length != 2) return 5; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Code == "LMC3101");
        Assert.Contains("%luma.array = type", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

    }

    [Fact]
    public void CollectionInitializersAndArrayAliasesRetainReferences()
    {
        const string source = "class Program { public static int32 Main() { int32[] first = { 3, 5, 8 }; int32[] alias = first; alias[1] = 13; if (first[1] != 13) return 1; int32[] second = new int32[] { 21, 34 }; first = second; if (first[0] != 21) return 2; if (alias[0] != 3) return 3; if (first == alias) return 4; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

    }

    [Fact]
    public void ManagedArraysCanCrossValueParametersAndReturns()
    {
        const string source = "class Program { public static int32[] Create(int32 length) { return new int32[length]; } public static int32 Read(int32[] values) { return values[0]; } public static int32 Main() { int32[] values = Create(2); values[0] = 23; return Read(values) == 23 && values.Length == 2 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

    }

    [Fact]
    public void ManagedArraysExposeRankLengthAndForInIteration()
    {
        const string source = "class Program { public static int32 Main() { int32[] values = { 2, 3, 5 }; int32 sum = 0; for (let value in values) sum += value; if (values.Rank != 1) return 1; if (values.GetLength(0) != values.Length) return 2; return sum == 10 ? 0 : 3; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void StringArraysRequireInitialValuesAndReleaseOwnedElements()
    {
        const string source = "class Program { public static int32 Main() { string[] values = { \"\", \"\" }; if (values[0] != \"\") return 1; string text = \"value\" + \"!\"; values[0] = text; string[] alias = values; alias[1] = text; string[] initialized = { text, \"fixed\" }; if (values[0] != \"value!\") return 2; if (values[1] != \"value!\") return 3; if (initialized[0] != \"value!\") return 4; if (initialized[1] != \"fixed\") return 5; string[] letters = { \"a\", \"b\" }; string joined = \"\"; for (let letter in letters) joined += letter; if (joined != \"ab\") return 6; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void NullableStringArraysDefaultToNullAndReleaseAssignedElements()
    {
        const string source = "class Program { public static int32 Main() { string?[] values = new string?[2]; if (values[0] != null) return 1; string? text = \"value\" + \"!\"; values[0] = text; values[1] = values[0]; values[0] = null; if (values[1] != \"value!\") return 2; int32 matches = 0; for (let item in values) if (item == \"value!\") matches++; string?[] alias = values; alias[1] = null; return matches == 1 && values[1] == null ? 0 : 3; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void NonNullableStringsHaveNoDefaultValue()
    {
        var defaultString = Compile("class Program { public static int32 Main() { string value = default; return 0; } }");
        Assert.Contains(defaultString.Diagnostics, diagnostic => diagnostic.Code == "LS0810");

        var uninitializedString = Compile("class Program { public static int32 Main() { string value; return 0; } }");
        Assert.Contains(uninitializedString.Diagnostics, diagnostic => diagnostic.Code == "LS0810");

        var stringArray = Compile("class Program { public static int32 Main() { string[] values = new string[2]; return 0; } }");
        Assert.Contains(stringArray.Diagnostics, diagnostic => diagnostic.Code == "LS1501");

        var nullableString = Compile("class Slot<T> where T : default { public T Get() { return default(T); } } class Program { public static int32 Main() { Slot<string?> slot = new Slot<string?>(); string? value = slot.Get(); return value == null ? 0 : 1; } }");
        Assert.True(nullableString.Succeeded, string.Join(Environment.NewLine, nullableString.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(nullableString.LlvmIr!).ExitCode);

        var nonNullableStringArgument = Compile("class Slot<T> where T : default { } class Program { public static int32 Main() { Slot<string> slot = new Slot<string>(); return 0; } }");
        Assert.Contains(nonNullableStringArgument.Diagnostics, diagnostic => diagnostic.Code == "LS1102");
    }

    [Fact]
    public void RectangularStringArraysRetainAndReleaseElements()
    {
        const string source = "class Program { public static int32 Main() { string[,] values = new string[,] { { \"\", \"\" } }; if (values[0, 0] != \"\") return 1; string text = \"matrix\" + \"!\"; values[0, 1] = text; return values.GetLength(1) == 2 && values[0, 1] == \"matrix!\" ? 0 : 2; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ExecutesRectangularArrayCreationInitializersAndRowMajorIteration()
    {
        const string source = "class Program { public static int32 Main() { int32[,] values = new int32[,] { { 1, 2, 3 }, { 4, 5, 6 } }; if (values.Rank != 2) return 1; if (values.Length != 6) return 2; if (values.GetLength(0) != 2 || values.GetLength(1) != 3) return 3; if (values[1, 0] != 4) return 4; values[1, 2] = 9; int32 sum = 0; for (let value in values) sum += value; return sum == 24 ? 0 : 5; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ExecutesThreeDimensionalManagedArrayIndexing()
    {
        const string source = "class Program { public static int32 Main() { int32[,,] cube = new int32[2, 2, 2]; if (cube.Rank != 3 || cube.Length != 8) return 1; if (cube.GetLength(2) != 2) return 2; cube[1, 1, 1] = 17; cube[1, 0, 1] = 5; return cube[1, 1, 1] == 17 && cube[1, 0, 1] == 5 && cube[0, 1, 0] == 0 ? 0 : 3; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void RectangularArrayShapeAndMaximumRankErrorsHaveStableDiagnostics()
    {
        var ragged = Compile("class Program { public static int32 Main() { int32[,] values = { { 1, 2 }, { 3 } }; return 0; } }");
        Assert.False(ragged.Succeeded);
        Assert.Contains(ragged.Diagnostics, diagnostic => diagnostic.Code == "LS1504");
        Assert.Null(ragged.LlvmIr);

        var tooManyDimensions = Compile("class Program { public static int32 Main() { int32[,,,] values = new int32[1, 1, 1, 1]; return 0; } }");
        Assert.False(tooManyDimensions.Succeeded);
        Assert.Contains(tooManyDimensions.Diagnostics, diagnostic => diagnostic.Code == "LS1503");
        Assert.Null(tooManyDimensions.LlvmIr);
    }

    [Fact]
    public void ArrayDimensionFailuresRaiseCatchableOverflowAndBoundsExceptions()
    {
        const string source = "class Program { public static int32 Main() { try { int32[,] invalid = new int32[-1, -1]; return 10; } catch (OverflowException) { } int32[,] values = new int32[2, 2]; try { return values.GetLength(2); } catch (IndexOutOfRangeException) { return 0; } } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Code is "LMC3101" or "LMC3102");
        Assert.True(System.Text.RegularExpressions.Regex.Matches(result.LlvmIr!, "icmp sge i32").Count >= 2);
        Assert.Contains("@luma_throw_overflow", result.LlvmIr!, StringComparison.Ordinal);

        var promoted = Compile(source, new CompilationOptions(WarningsAsErrors: true));
        Assert.True(promoted.Succeeded, string.Join(Environment.NewLine, promoted.Diagnostics));

        var disabled = Compile(source, new CompilationOptions(DisabledWarnings: new HashSet<string> { "LMC3102" }));
        Assert.True(disabled.Succeeded, string.Join(Environment.NewLine, disabled.Diagnostics));
        Assert.DoesNotContain(disabled.Diagnostics, diagnostic => diagnostic.Code == "LMC3102");

        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ExecutesRaggedJaggedArrayInitializationIterationAndReplacement()
    {
        const string source = "class Program { public static int32[][] Echo(int32[][] rows) { return rows; } public static int32 Main() { int32[][] rows = new int32[3][] { { 1, 2 }, { 3 }, { } }; rows = Echo(rows); if (rows.Length != 3 || rows[0].Length != 2 || rows[2].Length != 0) return 1; rows[1] = new int32[] { 5, 8 }; int32 sum = 0; for (let row in rows) for (let value in row) sum += value; return sum == 16 && rows[1][1] == 8 ? 0 : 2; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ReleasesStringElementsInsideJaggedArrays()
    {
        const string source = "class Program { public static int32 Main() { string[][] rows = { { \"a\", \"b\" }, { \"c\" } }; string joined = \"\"; for (let row in rows) for (let value in row) joined += value; return joined == \"abc\" ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void RejectsJaggedArraysWithoutValuesForNonNullableRows()
    {
        var result = Compile("class Program { public static int32 Main() { int32[][] rows = new int32[2][]; return 0; } }");
        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS1501");
        Assert.Null(result.LlvmIr);
    }

    [Fact]
    public void PassesManagedArrayElementsByRefAndOut()
    {
        const string source = "class Program { public static void Increment(ref int32 value) { value += 1; } public static void Initialize(out int32 value) { value = 7; } public static void Replace(ref string value) { value = value + \"!\"; } public static void Set(out string value) { value = \"out\"; } public static int32 Main() { int32[,] values = new int32[2, 2]; Increment(ref values[1, 0]); Initialize(out values[0, 1]); string[] texts = { \"\", \"\" }; Replace(ref texts[0]); Set(out texts[1]); return values[1, 0] == 1 && values[0, 1] == 7 && texts[0] == \"!\" && texts[1] == \"out\" ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void SupportsBooleanCharacterAndEnumArrayElements()
    {
        const string source = "enum Flag : uint8 { A = 1, B = 2 } class Program { public static int32 Main() { bool[] bits = new bool[2]; char[] letters = { 'x', 'λ' }; Flag[] flags = { Flag.A, Flag.B }; bits[1] = true; return !bits[0] && bits[1] && (int32)letters[1] == 955 && flags[0] == Flag.A && flags[1] == Flag.B ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Theory]
    [InlineData("x86_64-pc-linux-gnu")]
    [InlineData("i386-pc-linux-gnu")]
    public void EmitsManagedArrayRuntimeForTargetPointerWidth(string targetTriple)
    {
        var result = Compile("class Program { public static int32 Main() { int32[,] values = new int32[2, 3]; values[1, 2] = 7; return values[1, 2]; } }", new CompilationOptions(TargetTriple: targetTriple));
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-array-target-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try
        {
            var emitted = LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, targetTriple, optimize: true);
            Assert.True(emitted.Succeeded, emitted.Error);
        }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }
    }

    [Theory]
    [InlineData("x86_64-pc-linux-gnu")]
    [InlineData("i386-pc-linux-gnu")]
    public void EmitsValueTypeArrayLayoutsForTargetPointerWidth(string targetTriple)
    {
        const string source = "struct Record { public uint8 tag; public int64 value; } class Program { public static int32 Main() { Record[] values = { default }; values[0].tag = 3; values[0].value = 42L; return values[0].tag == 3 && values[0].value == 42L ? 0 : 1; } }";
        var result = Compile(source, new CompilationOptions(TargetTriple: targetTriple));
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-struct-array-target-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try
        {
            var emitted = LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, targetTriple, optimize: true);
            Assert.True(emitted.Succeeded, emitted.Error);
        }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }
    }

    [Fact]
    public void SimultaneousReferenceAssignmentsRetainStringAndClassSnapshots()
    {
        const string source = "class Program { public static int32 Main() { string left = \"left\" + \"!\"; string right = \"right\" + \"?\"; left, right = right, left; if (left != \"right?\") return 1; if (right != \"left!\") return 2; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        const string classSource = "class Node { public static int32 finalized; int32 value; public this(int32 value) { this.value = value; } public int32 Read() { return value; } ~this() { Node.finalized++; } } class Program { public static int32 Main() { Node? left = new Node(1); Node? right = new Node(2); left, right = right, left; if (left!.Read() != 2 || right!.Read() != 1 || Node.finalized != 0) return 1; left = null; if (Node.finalized != 1) return 2; right = null; return Node.finalized == 2 ? 0 : 3; } }";
        var classResult = Compile(classSource);
        Assert.True(classResult.Succeeded, string.Join(Environment.NewLine, classResult.Diagnostics));
        var classExecution = LlvmJit.RunMain(classResult.LlvmIr!);
        Assert.Equal(0, classExecution.ExitCode);
        Assert.Equal(0, classExecution.OutstandingAllocations);
        Assert.Null(classExecution.InteropError);

        const string arrayElementSource = "class Program { public static int32 Main() { int32 i = 0; int32[] values = new int32[3]; values[i], i = 99, i + 1; return values[0] == 99 && i == 1 ? 0 : 1; } }";
        var arrayElementResult = Compile(arrayElementSource);
        Assert.True(arrayElementResult.Succeeded, string.Join(Environment.NewLine, arrayElementResult.Diagnostics));
        var arrayElementExecution = LlvmJit.RunMain(arrayElementResult.LlvmIr!);
        Assert.Equal(0, arrayElementExecution.ExitCode);
        Assert.Equal(0, arrayElementExecution.OutstandingAllocations);
        Assert.Null(arrayElementExecution.InteropError);

        const string heldArrayTargetSource = "class Program { public static int32 Main() { int32[]? old = new int32[1]; int32[] alias = old!; int32[] next = new int32[1]; old, old[0] = next, 5; return old![0] == 0 && alias[0] == 5 ? 0 : 1; } }";
        var heldArrayTargetResult = Compile(heldArrayTargetSource);
        Assert.True(heldArrayTargetResult.Succeeded, string.Join(Environment.NewLine, heldArrayTargetResult.Diagnostics));
        var heldArrayTargetExecution = LlvmJit.RunMain(heldArrayTargetResult.LlvmIr!);
        Assert.Equal(0, heldArrayTargetExecution.ExitCode);
        Assert.Equal(0, heldArrayTargetExecution.OutstandingAllocations);
        Assert.Null(heldArrayTargetExecution.InteropError);

        const string heldClassTargetSource = "class Holder { public static int32 observed; public int32 value; ~this() { if (value == 42) Holder.observed = 1; } } class Program { public static int32 Main() { Holder? old = new Holder(); Holder replacement = new Holder(); old, old.value = replacement, 42; return Holder.observed == 1 ? 0 : 1; } }";
        var heldClassTargetResult = Compile(heldClassTargetSource);
        Assert.True(heldClassTargetResult.Succeeded, string.Join(Environment.NewLine, heldClassTargetResult.Diagnostics));
        var heldClassTargetExecution = LlvmJit.RunMain(heldClassTargetResult.LlvmIr!);
        Assert.Equal(0, heldClassTargetExecution.ExitCode);
        Assert.Equal(0, heldClassTargetExecution.OutstandingAllocations);
        Assert.Null(heldClassTargetExecution.InteropError);

        const string valueAndStaticFieldSource = "struct Point { public int32 x; } class Program { public static int32 count; public static int32 Main() { Point point = default; int32 local = 0; point.x, Program.count, local = 3, 4, 5; return point.x == 3 && Program.count == 4 && local == 5 ? 0 : 1; } }";
        var valueAndStaticFieldResult = Compile(valueAndStaticFieldSource);
        Assert.True(valueAndStaticFieldResult.Succeeded, string.Join(Environment.NewLine, valueAndStaticFieldResult.Diagnostics));
        var valueAndStaticFieldExecution = LlvmJit.RunMain(valueAndStaticFieldResult.LlvmIr!);
        Assert.Equal(0, valueAndStaticFieldExecution.ExitCode);
        Assert.Equal(0, valueAndStaticFieldExecution.OutstandingAllocations);
        Assert.Null(valueAndStaticFieldExecution.InteropError);
    }

    [Fact]
    public void MultipleAssignmentSupportsComputedPropertiesIndexersAndPointerDereferences()
    {
        const string source = "class Box { int32 stored; int32 indexed; public int32 Value { get => stored; set { Program.setters = Program.setters * 10 + 1; stored = value; } } public int32 this[int32 index] { set { Program.setters = Program.setters * 10 + 2; indexed = value + index; } } public int32 ReadIndex() { return indexed; } } class Program { static Box? shared; public static int32 order; public static int32 setters; public static Box GetBox() { Program.order = Program.order * 10 + 1; return Program.shared!; } public static int32 TargetIndex() { Program.order = Program.order * 10 + 2; return 3; } public static int32 FirstValue() { Program.order = Program.order * 10 + 3; return 40; } public static int32 SecondValue() { Program.order = Program.order * 10 + 4; return 50; } public static int32 Main() { Box localBox = new Box(); Program.shared = localBox; localBox.Value, GetBox()[TargetIndex()] = FirstValue(), SecondValue(); if (Program.order != 3412 || Program.setters != 12) return 10; if (localBox.Value != 40) return 11; if (localBox.ReadIndex() != 53) return 12; Program.shared = null; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        const string pointerSource = "unsafe class Program { public static int32 Main() { int32 value = 0; int32 other = 0; int32* pointer = &value; *pointer, other = 42, 7; return value == 42 && other == 7 ? 0 : 1; } }";
        var pointerResult = Compile(pointerSource, new CompilationOptions(UnsafeEnabled: true));
        Assert.True(pointerResult.Succeeded, string.Join(Environment.NewLine, pointerResult.Diagnostics));
        var pointerExecution = LlvmJit.RunMain(pointerResult.LlvmIr!);
        Assert.Equal(0, pointerExecution.ExitCode);
        Assert.Equal(0, pointerExecution.OutstandingAllocations);
        Assert.Null(pointerExecution.InteropError);

        const string pointerIndexSource = "unsafe class Program { public static int32 Main() { int32[] values = new int32[2]; int32* pointer = &values[0]; pointer[0], values[1] = 42, 7; return values[0] == 42 && values[1] == 7 ? 0 : 1; } }";
        var pointerIndexResult = Compile(pointerIndexSource, new CompilationOptions(UnsafeEnabled: true));
        Assert.True(pointerIndexResult.Succeeded, string.Join(Environment.NewLine, pointerIndexResult.Diagnostics));
        var pointerIndexExecution = LlvmJit.RunMain(pointerIndexResult.LlvmIr!);
        Assert.Equal(0, pointerIndexExecution.ExitCode);
        Assert.Equal(0, pointerIndexExecution.OutstandingAllocations);
        Assert.Null(pointerIndexExecution.InteropError);

        const string classPointerSource = "class Box { public int32 value; } unsafe class Program { public static int32 Main() { Box box = new Box(); Box* pointer = &box; pointer.value, box.value = 4, 5; return box.value == 5 ? 0 : 1; } }";
        var classPointerResult = Compile(classPointerSource, new CompilationOptions(UnsafeEnabled: true));
        Assert.True(classPointerResult.Succeeded, string.Join(Environment.NewLine, classPointerResult.Diagnostics));
        var classPointerExecution = LlvmJit.RunMain(classPointerResult.LlvmIr!);
        Assert.Equal(0, classPointerExecution.ExitCode);
        Assert.Equal(0, classPointerExecution.OutstandingAllocations);
        Assert.Null(classPointerExecution.InteropError);

        const string structSource = "struct Cell { int32 stored; public int32 Value { get => stored; set { stored = value; } } public int32 this[int32 index] { get => stored; set { stored = value + index; } } } class Program { public static int32 Main() { Cell cell = default; cell.Value, cell[2] = 10, 40; return cell.Value == 42 && cell[2] == 42 ? 0 : 1; } }";
        var structResult = Compile(structSource);
        Assert.True(structResult.Succeeded, string.Join(Environment.NewLine, structResult.Diagnostics));
        var structExecution = LlvmJit.RunMain(structResult.LlvmIr!);
        Assert.Equal(0, structExecution.ExitCode);
        Assert.Equal(0, structExecution.OutstandingAllocations);
        Assert.Null(structExecution.InteropError);

        const string interfaceSource = "interface IBox { int32 Value { get; set; } int32 this[int32 index] { get; set; } } class Box : IBox { int32 stored; int32 indexed; public int32 Value { get => stored; set { stored = value; } } public int32 this[int32 index] { get => indexed; set { indexed = value + index; } } } class Program { public static int32 Main() { IBox box = new Box(); box.Value, box[2] = 40, 50; return box.Value == 40 && box[2] == 52 ? 0 : 1; } }";
        var interfaceResult = Compile(interfaceSource);
        Assert.True(interfaceResult.Succeeded, string.Join(Environment.NewLine, interfaceResult.Diagnostics));
        var interfaceExecution = LlvmJit.RunMain(interfaceResult.LlvmIr!);
        Assert.Equal(0, interfaceExecution.ExitCode);
        Assert.Equal(0, interfaceExecution.OutstandingAllocations);
        Assert.Null(interfaceExecution.InteropError);

        const string staticAndImplicitSource = "class Holder { static int32 total; int32 stored; public static int32 Total { get => total; set { total = value; } } public int32 Value { get => stored; set { stored = value; } } public int32 Update() mut { Value, stored = 5, 7; return Value; } } class Program { public static int32 Main() { Holder holder = new Holder(); int32 other = 0; Holder.Total, other = 11, 12; return holder.Update() == 7 && Holder.Total == 11 && other == 12 ? 0 : 1; } }";
        var staticAndImplicitResult = Compile(staticAndImplicitSource);
        Assert.True(staticAndImplicitResult.Succeeded, string.Join(Environment.NewLine, staticAndImplicitResult.Diagnostics));
        var staticAndImplicitExecution = LlvmJit.RunMain(staticAndImplicitResult.LlvmIr!);
        Assert.Equal(0, staticAndImplicitExecution.ExitCode);
        Assert.Equal(0, staticAndImplicitExecution.OutstandingAllocations);
        Assert.Null(staticAndImplicitExecution.InteropError);

        const string throwingTarget = "class Box { public int32 Value { get; set; } ~this() { Program.finalized++; } } class Program { public static int32 finalized; public static int32 Main() { int32[] values = new int32[1]; Box? box = new Box(); try { box.Value, values[2] = 42, 7; } catch (IndexOutOfRangeException) { } box = null; return Program.finalized == 1 ? 0 : 1; } }";
        var throwingTargetResult = Compile(throwingTarget);
        Assert.True(throwingTargetResult.Succeeded, string.Join(Environment.NewLine, throwingTargetResult.Diagnostics));
        var throwingTargetExecution = LlvmJit.RunMain(throwingTargetResult.LlvmIr!);
        Assert.Equal(0, throwingTargetExecution.ExitCode);
        Assert.Equal(0, throwingTargetExecution.OutstandingAllocations);
        Assert.Null(throwingTargetExecution.InteropError);

        const string missingSetter = "class Box { public int32 Value => 1; } class Program { public static int32 Main() { Box box = new Box(); box.Value, box.Value = 2, 3; return 0; } }";
        var invalid = Compile(missingSetter);
        Assert.False(invalid.Succeeded);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == "LMC3012" && diagnostic.Message.Contains("set accessor", StringComparison.OrdinalIgnoreCase));

        const string nullableTarget = "class Box { public int32 Value; } class Program { public static int32 Main() { Box? box = null; int32 result = 0; box!.Value, result = 1, box.Value; return 0; } }";
        var nullableTargetResult = Compile(nullableTarget);
        Assert.False(nullableTargetResult.Succeeded);
        Assert.Contains(nullableTargetResult.Diagnostics, diagnostic => diagnostic.Code == "LS1303");

        const string initializedTarget = "class Box { public int32 Value; } class Program { public static int32 Main() { Box? box = null; box, box.Value = (box = new Box()), 42; return box.Value == 42 ? 0 : 1; } }";
        var initializedTargetResult = Compile(initializedTarget);
        Assert.True(initializedTargetResult.Succeeded, string.Join(Environment.NewLine, initializedTargetResult.Diagnostics));
        var initializedTargetExecution = LlvmJit.RunMain(initializedTargetResult.LlvmIr!);
        Assert.Equal(0, initializedTargetExecution.ExitCode);
        Assert.Equal(0, initializedTargetExecution.OutstandingAllocations);
        Assert.Null(initializedTargetExecution.InteropError);
    }

    [Fact]
    public void ConcatenatesPrimitiveValuesUsingTheirTextRepresentations()
    {
        const string source = "class Program { public static int32 Main() { string value = \"n=\" + -42; if (value != \"n=-42\") return 1; value += true; if (value != \"n=-42true\") return 2; value = value + 'λ'; if (value != \"n=-42trueλ\") return 3; value += null; if (value != \"n=-42trueλ\") return 4; string unsigned = \"u=\" + 18446744073709551615UL; if (unsigned != \"u=18446744073709551615\") return 5; int64 minimum = (int64)0 - 9223372036854775807L - 1L; string signed = \"s=\" + minimum; if (signed != \"s=-9223372036854775808\") return 6; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("@luma_string_from_i64", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("@luma_string_from_char", result.LlvmIr!, StringComparison.Ordinal);
        Assert.DoesNotContain("@snprintf", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ConcatenatesFloatingPointValuesUsingRoundTripFormatting()
    {
        const string source = "class Program { public static int32 Main() { string single = \"f=\" + (float)1.25; string wide = \"d=\" + 1.25; if (single != \"f=1.25\") return 1; if (wide != \"d=1.25\") return 2; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("@snprintf", result.LlvmIr!, StringComparison.Ordinal);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-float-string-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try
        {
            var emitted = LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, "host", optimize: true);
            Assert.True(emitted.Succeeded, emitted.Error);
        }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void EmitsVersionedRuntimeAbiModuleFlags()
    {
        var result = Compile("class Program { public static int32 Main() { return 0; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("!llvm.module.flags = !{!0}", result.LlvmIr!);
        Assert.Contains($"!0 = !{{i32 1, !\"{RuntimeAbi.ModuleFlag}\", i32 {RuntimeAbi.Version}}}", result.LlvmIr);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-abi-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try
        {
            var emitted = LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, "host", optimize: true);
            Assert.True(emitted.Succeeded, emitted.Error);
        }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }
    }

    [Fact]
    public void LlvmParsesVerifiesOptimizesAndEmitsObject()
    {
        var result = Compile("class Program { public static int32 Main() { return 21 * 2; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try
        {
            var emitted = LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, "host", optimize: true);
            Assert.True(emitted.Succeeded, emitted.Error);
            Assert.True(File.Exists(objectPath));
            Assert.NotNull(LlvmToolchain.GetHostTargetTriple());
        }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }
    }

    [Fact]
    public void EmitsLoopsBranchesAndCallsAcrossMethods()
    {
        var result = Compile("class Program { public static int32 Main() { var sum = 0; for (int32 i = 0; i < 4; i++) { if (i == 2) continue; sum += i; } return Twice(sum); } public static int32 Twice(int32 value) { return value * 2; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("for.cond", result.LlvmIr!);
        Assert.Contains("if.then", result.LlvmIr);
        Assert.Contains("call i32 @luma_Program_Twice_1", result.LlvmIr);
    }

    [Fact]
    public void ResolvesOverloadsByExactAndBetterNumericConversions()
    {
        var exact = Compile("class Program { public static int32 Main() { return Pick(5); } public static int32 Pick(int32 value) { return 1; } public static int32 Pick(int64 value) { return 2; } }");
        Assert.True(exact.Succeeded, string.Join(Environment.NewLine, exact.Diagnostics));
        Assert.Contains("call i32 @luma_Program_Pick_1_int32", exact.LlvmIr!);
        Assert.Contains("define i32 @luma_Program_Pick_1_int64", exact.LlvmIr);

        var preferred = Compile("class Program { public static int32 Main() { return Choose(3); } public static int32 Choose(int64 value) { return 11; } public static int32 Choose(double value) { return 22; } }");
        Assert.True(preferred.Succeeded, string.Join(Environment.NewLine, preferred.Diagnostics));
        Assert.Contains("call i32 @luma_Program_Choose_1_int64", preferred.LlvmIr!);
    }

    [Fact]
    public void BindsNamedAndOptionalArgumentsAndDiagnosesInvalidCalls()
    {
        var result = Compile("class Program { public static int32 Main() { return Add(second: 40, first: 2) + Add(3); } public static int32 Add(int32 first, int32 second = 1) { return first + second; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("call i32 @luma_Program_Add_2(i32 2, i32 40)", result.LlvmIr!);
        Assert.Contains("call i32 @luma_Program_Add_2(i32 3, i32 1)", result.LlvmIr);

        var positionalAfterNamed = Compile("class Program { public static int32 Main() { return Add(first: 1, 2); } public static int32 Add(int32 first, int32 second) { return first + second; } }");
        Assert.Contains(positionalAfterNamed.Diagnostics, d => d.Code == "LS0740");

        var missingRequired = Compile("class Program { public static int32 Main() { return Add(second: 2); } public static int32 Add(int32 first, int32 second = 1) { return first + second; } }");
        Assert.Contains(missingRequired.Diagnostics, d => d.Code == "LS0751");

        var requiredAfterOptional = Compile("class Program { public static int32 Main() { return 0; } public static int32 Add(int32 first = 1, int32 second) { return first + second; } }");
        Assert.Contains(requiredAfterOptional.Diagnostics, d => d.Code == "LMC3010" && d.Message.Contains("before optional", StringComparison.Ordinal));

        var nonConstantDefault = Compile("class Program { public static int32 Main() { return 0; } public static int32 Value() { return 1; } public static int32 Add(int32 value = Value()) { return value; } }");
        Assert.Contains(nonConstantDefault.Diagnostics, d => d.Code == "LMC3010" && d.Message.Contains("compile-time constants", StringComparison.Ordinal));

        var unrepresentableDefault = Compile("class Program { public static int32 Main() { return 0; } public static int32 Add(int8 value = 300) { return value; } }");
        Assert.Contains(unrepresentableDefault.Diagnostics, d => d.Code == "LMC3010");

        var evaluationOrder = Compile("class Program { public static int32 Main() { return Combine(second: Mark(2), first: Mark(1)); } public static int32 Mark(int32 value) { return value; } public static int32 Combine(int32 first, int32 second) { return first + second; } }");
        Assert.True(evaluationOrder.Succeeded, string.Join(Environment.NewLine, evaluationOrder.Diagnostics));
        var evaluationIr = evaluationOrder.LlvmIr!;
        Assert.True(evaluationIr.IndexOf("call i32 @luma_Program_Mark_1(i32 2)", StringComparison.Ordinal) < evaluationIr.IndexOf("call i32 @luma_Program_Mark_1(i32 1)", StringComparison.Ordinal));
    }

    [Fact]
    public void ExpandsVariadicArgumentsAndPassesExistingArraysWithoutCopy()
    {
        const string source = """
            class Collector {
                public int32 total;
                public this(int32... values) { for (let value in values) { this.total += value; } }
                public int32 Add(int32... values) mut { for (let value in values) { this.total += value; } return this.total; }
                public int32 Value() { return this.total; }
            }
            class Program {
                public static int32 Main() {
                    if (Sum() != 0) return 1;
                    if (Sum(1) != 1) return 2;
                    if (Sum(1, 2, 3, 4) != 10) return 3;
                    int32[] existing = new int32[] { 5, 6 };
                    if (Sum(existing) != 11) return 4;
                    if (Sum(values: existing) != 11) return 11;
                    if (Pick(9) != 1 || Pick(existing) != 2) return 12;
                    Mutate(existing);
                    if (existing[0] != 42) return 5;
                    if (SumFrom() != 7) return 6;
                    if (SumFrom(1, 2, 3) != 6) return 7;
                    if (TotalLength("a", "bc") != 3) return 8;
                    Collector collector = new Collector(2, 3);
                    if (collector.Value() != 5) return 9;
                    if (collector.Add(4, 5) != 14) return 10;
                    return 0;
                }
                public static int32 Sum(readonly int32... values) { int32 total = 0; for (let value in values) { total += value; } return total; }
                public static void Mutate(int32... values) { values[0] = 42; }
                public static int32 SumFrom(int32 initial = 7, int32... values) { int32 total = initial; for (let value in values) { total += value; } return total; }
                public static int32 TotalLength(string... values) { int32 total = 0; for (let value in values) { total += value.Length; } return total; }
                public static int32 Pick(int32 value) { return 1; }
                public static int32 Pick(int32... values) { return 2; }
            }
            """;
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void DiagnosesInvalidVariadicParameterDeclarationsAndExternLowering()
    {
        var notLast = Compile("class Program { public static int32 Main() { return 0; } public static int32 Sum(int32... values, int32 trailing) { return trailing; } }");
        Assert.Contains(notLast.Diagnostics, diagnostic => diagnostic.Code == "LMC3010" && diagnostic.Message.Contains("final parameter", StringComparison.Ordinal));

        var passingMode = Compile("class Program { public static int32 Main() { return 0; } public static int32 Sum(ref int32... values) { return 0; } }");
        Assert.Contains(passingMode.Diagnostics, diagnostic => diagnostic.Code == "LMC3010" && diagnostic.Message.Contains("cannot use ref", StringComparison.Ordinal));

        var defaultValue = Compile("class Program { public static int32 Main() { return 0; } public static int32 Sum(int32... values = default) { return 0; } }");
        Assert.Contains(defaultValue.Diagnostics, diagnostic => diagnostic.Code == "LMC3010" && diagnostic.Message.Contains("cannot have a default", StringComparison.Ordinal));

        var externVariadic = Compile("unsafe class Program { public static int32 Main() { return 0; } public static extern int32 Sum(int32... values); }");
        Assert.Contains(externVariadic.Diagnostics, diagnostic => diagnostic.Code == "LS2101" && diagnostic.Message.Contains("Variadic", StringComparison.Ordinal));
    }

    [Fact]
    public void SupportsUnsafeLocalPointersAndReportsPointerRestrictions()
    {
        var result = Compile("unsafe class Program { public static int32 Main() { int32 value = 6; int32* pointer = &value; *pointer = 13; pointer[0] = 15; int32* first = &pointer[0]; pointer += 1; pointer -= 1; void* erased = pointer; int32* restored = (int32*)erased; uint64 address = (uint64)restored; int32* fromAddress = (int32*)address; int32* next = fromAddress + 1; int64 distance = next - fromAddress; if (distance == 1L && fromAddress != null) return first[0]; return 0; } public static unsafe int32 Read(int32* pointer) { return *pointer; } public static unsafe int32 Size() { return sizeof(int32*); } }", new CompilationOptions(UnsafeEnabled: true));
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("getelementptr i32", result.LlvmIr!);
        Assert.Contains("icmp ne ptr", result.LlvmIr);
        Assert.Contains("load i32, ptr", result.LlvmIr);
        Assert.Contains("ptrtoint ptr", result.LlvmIr);
        Assert.Contains("inttoptr i64", result.LlvmIr);
        Assert.Contains("sdiv i64", result.LlvmIr);
        Assert.Contains("define i32 @luma_Program_Read_1", result.LlvmIr);
        Assert.Contains($"ret i32 {IntPtr.Size}", result.LlvmIr);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-pointer-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try { Assert.True(LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, "host", optimize: false).Succeeded); }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }

        var safeUse = Compile("class Program { public static int32 Main() { int32 value = 1; int32* pointer = &value; return 0; } }", new CompilationOptions(UnsafeEnabled: true));
        Assert.Contains(safeUse.Diagnostics, d => d.Code == "LMC3000" && d.Message.Contains("unsafe context", StringComparison.OrdinalIgnoreCase));
        var immutableAddress = Compile("unsafe class Program { public static int32 Main() { let value = 1; int32* pointer = &value; return 0; } }", new CompilationOptions(UnsafeEnabled: true));
        Assert.Contains(immutableAddress.Diagnostics, d => d.Code == "LS1902");
        var disallowedType = Compile("unsafe class Program { public static int32 Main() { string* pointer = null; return 0; } }", new CompilationOptions(UnsafeEnabled: true));
        Assert.Contains(disallowedType.Diagnostics, d => d.Code == "LS1903");
        var disabled = Compile("unsafe class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(disabled.Diagnostics, d => d.Code == "LS1901");
    }

    [Fact]
    public void SupportsTakingAddressesOfMutableStaticFields()
    {
        const string source = "unsafe class Program { public static int32 count = 4; public static int32 Main() { int32* unqualified = &count; int32* qualified = &Program.count; *unqualified += 2; return *qualified == 6 ? 0 : 1; } }";
        var result = Compile(source, new CompilationOptions(UnsafeEnabled: true));
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var immutable = Compile("unsafe class Program { public static readonly int32 count = 4; public static int32 Main() { int32* pointer = &Program.count; return 0; } }", new CompilationOptions(UnsafeEnabled: true));
        Assert.Contains(immutable.Diagnostics, diagnostic => diagnostic.Code == "LS1902");
        Assert.Null(immutable.LlvmIr);
    }

    [Fact]
    public void SupportsUnsafeLocalInlineArraysAndChecksTheirBounds()
    {
        const string source = "unsafe class Program { public static int32 Main() { int32[1 + 3] values = { 10, 20 }; values[2] = 30; return values[0] + values[2] + values.Length; } public static int32 Size() { return sizeof(int32[4]); } public static int32 Sum() { int32[3] values = { 2, 3, 4 }; int32 total = 0; outer: for (let item in values) { if (item < 0) continue outer; total += item; } for (var mutable in values) { mutable += 1; } for (int64 widened in values) { total += (int32)widened; } return total; } public static int32 Cleared() { int32[2] values; return values[1]; } }";
        var result = Compile(source, new CompilationOptions(UnsafeEnabled: true));
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("alloca [4 x i32]", result.LlvmIr!);
        Assert.Contains("insertvalue [4 x i32]", result.LlvmIr);
        Assert.Contains("getelementptr [4 x i32]", result.LlvmIr);
        Assert.Contains("icmp sge i32", result.LlvmIr);
        Assert.Contains("@luma_throw_bounds", result.LlvmIr);
        Assert.Contains("forin.cond", result.LlvmIr);
        Assert.Contains("forin.step", result.LlvmIr);
        Assert.Contains("store [2 x i32] zeroinitializer", result.LlvmIr);
        Assert.Contains("ret i32 16", result.LlvmIr);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-inline-array-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try { Assert.True(LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, "host", optimize: false).Succeeded); }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }

        var tooMany = Compile("unsafe class Program { public static int32 Main() { int32[2] values = { 1, 2, 3 }; return 0; } }", new CompilationOptions(UnsafeEnabled: true));
        Assert.Contains(tooMany.Diagnostics, diagnostic => diagnostic.Code == "LS1504");
        var invalidElement = Compile("unsafe class Program { public static int32 Main() { string[2] values; return 0; } }", new CompilationOptions(UnsafeEnabled: true));
        Assert.Contains(invalidElement.Diagnostics, diagnostic => diagnostic.Code == "LS1510");
        var nonPositiveLength = Compile("unsafe class Program { public static int32 Main() { int32[2 - 2] values; return 0; } }", new CompilationOptions(UnsafeEnabled: true));
        Assert.Contains(nonPositiveLength.Diagnostics, diagnostic => diagnostic.Code == "LS1510");
        var nonConstantLength = Compile("unsafe class Program { public static int32 Main() { int32[count] values; return 0; } }", new CompilationOptions(UnsafeEnabled: true));
        Assert.Contains(nonConstantLength.Diagnostics, diagnostic => diagnostic.Code == "LS1510");
        var pointerElements = Compile("unsafe class Program { public static int32 Main() { int32 value = 42; int32* pointer = &value; int32*[1] pointers = { pointer }; return *pointers[0]; } }", new CompilationOptions(UnsafeEnabled: true));
        Assert.True(pointerElements.Succeeded, string.Join(Environment.NewLine, pointerElements.Diagnostics));
        Assert.Contains("alloca [1 x ptr]", pointerElements.LlvmIr!);
        var enumElements = Compile("enum Flag : uint8 { A = 1, B = 2 } unsafe class Program { public static int32 Main() { Flag[2] flags = { Flag.A, Flag.B }; return (int32)flags[1]; } }", new CompilationOptions(UnsafeEnabled: true));
        Assert.True(enumElements.Succeeded, string.Join(Environment.NewLine, enumElements.Diagnostics));
        Assert.Contains("alloca [2 x i8]", enumElements.LlvmIr!);
        var immutableElement = Compile("unsafe class Program { public static int32 Main() { let int32[2] values = { 1 }; values[0] = 2; return 0; } }", new CompilationOptions(UnsafeEnabled: true));
        Assert.Contains(immutableElement.Diagnostics, diagnostic => diagnostic.Code == "LMC3012");
        var immutableIteration = Compile("unsafe class Program { public static int32 Main() { int32[2] values = { 1, 2 }; for (let item in values) { item++; } return 0; } }", new CompilationOptions(UnsafeEnabled: true));
        Assert.Contains(immutableIteration.Diagnostics, diagnostic => diagnostic.Code == "LMC3012");
        foreach (var allocation in new[] { "new { 1 }", "new int32[2] { 1 }", "scope { 1 }" })
        {
            var allocatedInlineArray = Compile($"unsafe class Program {{ public static int32 Main() {{ int32[2] values = {allocation}; return 0; }} }}", new CompilationOptions(UnsafeEnabled: true));
            Assert.Contains(allocatedInlineArray.Diagnostics, diagnostic => diagnostic.Code == "LS1505");
        }
    }

    [Fact]
    public void SupportsInlineArraysAsClassFields()
    {
        const string source = "unsafe class Buffer { public const int32 Capacity = 3; public int32[Capacity] values = { 4, 5 }; public int32[1] cleared; public int32 Sum() { int32 total = 0; for (let item in values) total += item; return total + values.Length; } } unsafe class Program { public static int32 Bounds() { Buffer buffer = new Buffer(); try { return buffer.values[3]; } catch (IndexOutOfRangeException error) { return 0; } } public static int32 Main() { Buffer buffer = new Buffer(); if (Bounds() != 0 || buffer.Sum() != 12 || buffer.cleared[0] != 0 || buffer.cleared.Length != 1) return 1; buffer.values[2] = 6; return buffer.Sum() == 18 ? 0 : 1; } }";
        var result = Compile(source, new CompilationOptions(UnsafeEnabled: true));
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("%class.Buffer = type { ptr, i32, i32, [3 x i32], [1 x i32] }", result.LlvmIr!);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);

        var invalidElement = Compile("unsafe class Buffer { public string[2] values; } class Program { public static int32 Main() { return 0; } }", new CompilationOptions(UnsafeEnabled: true));
        Assert.Contains(invalidElement.Diagnostics, diagnostic => diagnostic.Code == "LS1510");

        const string staticSource = "unsafe class Storage { public static int32[2] Values = { 20, 22 }; } unsafe class Program { public static int32 Main() { int32 total = 0; for (let item in Storage.Values) total += item; return total == 42 && Storage.Values.Length == 2 ? 0 : 1; } }";
        var staticResult = Compile(staticSource, new CompilationOptions(UnsafeEnabled: true));
        Assert.True(staticResult.Succeeded, string.Join(Environment.NewLine, staticResult.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(staticResult.LlvmIr!).ExitCode);
    }

    [Fact]
    public void StructInlineArrayFieldInitializersAreEmittedAndZeroFillRemainder()
    {
        const string source = "unsafe struct Buffer { public int32[3] values = { 4, 5 }; public this(int32 ignored) {} } unsafe class Program { public static int32 Main() { Buffer buffer = Buffer(0); int32 total = 0; for (let item in buffer.values) total += item; return total + buffer.values.Length == 12 ? 0 : 1; } }";
        var result = Compile(source, new CompilationOptions(UnsafeEnabled: true));
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(result.LlvmIr!).ExitCode);
    }

    [Fact]
    public void InlineArrayLengthsResolveConstantAndEnumMembers()
    {
        const string source = "enum Dimensions : int32 { Base = 2, Four = Base * 2 } class Limits { public const int32 Capacity = 2 + 2; } unsafe class Program { public static int32 Main() { int32[Limits.Capacity] fromConst = { 1, 2, 3, 4 }; int32[Dimensions.Four] fromEnum = { 1, 2, 3, 4 }; int32[sizeof(int64)] fromSize; return fromConst.Length == 4 && fromEnum.Length == 4 && fromSize.Length == 8 ? 0 : 1; } }";
        var result = Compile(source, new CompilationOptions(UnsafeEnabled: true));
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("alloca [4 x i32]", result.LlvmIr!);
        Assert.Contains("alloca [8 x i32]", result.LlvmIr);
        Assert.Equal(0, LlvmJit.RunMain(result.LlvmIr!).ExitCode);

        var nonIntegerConstant = Compile("class Limits { public const bool Enabled = true; } unsafe class Program { public static int32 Main() { int32[Limits.Enabled] values; return 0; } }", new CompilationOptions(UnsafeEnabled: true));
        Assert.Contains(nonIntegerConstant.Diagnostics, diagnostic => diagnostic.Code == "LS1510");
    }

    [Fact]
    public void ConstantFieldExpressionsResolveForwardAndUnqualifiedReferences()
    {
        const string source = "class Constants { public const int32 Answer = BaseValue + 2; public const int32 BaseValue = 40; public const string Product = Prefix + \"Sharp\"; public const string Prefix = \"Luma\"; public static int32 GetAnswer() { return Answer; } } virtual class BaseConstants { public const int32 Inherited = 8; } class DerivedConstants : BaseConstants { public static int32 ReadInherited() { return Inherited; } } class Program { public static int32 Initialized = Constants.Answer + 1; public static int32 Main() { return Constants.Answer == 42 && Constants.Product == \"LumaSharp\" && Constants.GetAnswer() == 42 && DerivedConstants.ReadInherited() == 8 && Initialized == 43 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(result.LlvmIr!).ExitCode);

        var cycle = Compile("class Constants { public const int32 First = Second; public const int32 Second = First; } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(cycle.Diagnostics, diagnostic => diagnostic.Code == "LMC3010" && diagnostic.Message.Contains("dependency cycle", StringComparison.Ordinal));
    }

    [Fact]
    public void SupportsStructValueFieldsCopiesAndAbiLayout()
    {
        const string source = "struct Inner { public int16 value; } struct Pair { public int32 x; public int32 y; public this(int32 x, int32 y) { this.x = x; this.y = y; } public void Move(int32 dx, int32 dy) mut { x += dx; y += dy; } public int32 LengthSquared() { return x * x + y * y; } } struct Outer { public uint8 tag; public Inner inner; } unsafe struct Buffer { public int32[3] values; } unsafe struct PairBuffer { public Pair[2] pairs; } unsafe class Program { public static int32 Main() { Pair point = Pair(); point.x = 12; point.y = 30; Pair constructed = Pair(15, 31); int32 lengthSquared = constructed.LengthSquared(); constructed.Move(1, 2); Pair copy = point; copy.x = 7; Outer outer = default; outer.inner.value = 5; Buffer buffer = Buffer(); buffer.values[0] = 11; int32 bufferSum = 0; for (let element in buffer.values) { bufferSum += element; } PairBuffer pairBuffer = default; pairBuffer.pairs[0] = point; pairBuffer.pairs[0].x = 9; for (var item in pairBuffer.pairs) { item.x += 1; } return point.x + copy.x + constructed.x + lengthSquared + outer.inner.value + sizeof(Pair) + sizeof(Outer) + buffer.values.Length + bufferSum; } public static Pair Echo(Pair value) { return value; } }";
        var result = Compile(source, new CompilationOptions(UnsafeEnabled: true));
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("%struct.Pair = type { i32, i32 }", result.LlvmIr!);
        Assert.Contains("%struct.Outer = type { i8, %struct.Inner }", result.LlvmIr);
        Assert.Contains("%struct.Buffer = type { [3 x i32] }", result.LlvmIr);
        Assert.Contains("%struct.PairBuffer = type { [2 x %struct.Pair] }", result.LlvmIr);
        Assert.Contains("define %struct.Pair @luma_Program_Echo_1(%struct.Pair %arg0)", result.LlvmIr);
        Assert.Contains("define %struct.Pair @luma_Pair_ctor_2(i32 %arg0, i32 %arg1)", result.LlvmIr);
        Assert.Contains("call %struct.Pair @luma_Pair_ctor_2(i32 15, i32 31)", result.LlvmIr);
        Assert.Contains("define void @luma_Pair_inst_Move_2(ptr %this, i32 %arg0, i32 %arg1)", result.LlvmIr);
        Assert.Contains("define i32 @luma_Pair_inst_LengthSquared_0(ptr %this)", result.LlvmIr);
        Assert.Contains("getelementptr %struct.Pair", result.LlvmIr);
        Assert.Contains("i32 8", result.LlvmIr);
        Assert.Contains("i32 4", result.LlvmIr);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-struct-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try { Assert.True(LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, "host", optimize: false).Succeeded); }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }

        var immutable = Compile("struct Pair { public int32 x; } class Program { public static int32 Main() { let Pair point = Pair(); point.x = 1; return 0; } }");
        Assert.Contains(immutable.Diagnostics, diagnostic => diagnostic.Code == "LMC3012");
        var recursive = Compile("struct A { public B b; } struct B { public A a; } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(recursive.Diagnostics, diagnostic => diagnostic.Code == "LS0900");
        var referenceField = Compile("struct Bad { public string label; } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(referenceField.Diagnostics, diagnostic => diagnostic.Code == "LS0902");
        var newStruct = Compile("struct Pair { public int32 x; } class Program { public static int32 Main() { Pair point = new Pair(); return 0; } }");
        Assert.Contains(newStruct.Diagnostics, diagnostic => diagnostic.Code == "LS0801");
        var immutableReceiver = Compile("struct Pair { public void Move() mut { } } class Program { public static int32 Main() { let Pair point = Pair(); point.Move(); return 0; } }");
        Assert.Contains(immutableReceiver.Diagnostics, diagnostic => diagnostic.Code == "LS0823");
        var readonlyReceiverMutation = Compile("struct Pair { public int32 x; public void Bad() { x = 1; } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(readonlyReceiverMutation.Diagnostics, diagnostic => diagnostic.Code == "LS0820");
        var noMatchingConstructor = Compile("struct Pair { public this(int32 value) { this.x = value; } public int32 x; } class Program { public static int32 Main() { Pair point = Pair(1.5); return 0; } }");
        Assert.Contains(noMatchingConstructor.Diagnostics, diagnostic => diagnostic.Code == "LS0751");
        var declaredParameterlessConstructor = Compile("struct Pair { public this() { } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(declaredParameterlessConstructor.Diagnostics, diagnostic => diagnostic.Code == "LMC3000" && diagnostic.Message.Contains("parameterless", StringComparison.Ordinal));
        var structEquality = Compile("struct Pair { public int32 x; } class Program { public static int32 Main() { Pair left = Pair(); Pair right = Pair(); if (left == right) return 1; return 0; } }");
        Assert.Contains(structEquality.Diagnostics, diagnostic => diagnostic.Code == "LMC3010" && diagnostic.Message.Contains("equality", StringComparison.OrdinalIgnoreCase));
        var userEquality = Compile("struct Pair { public int32 x; public static bool operator ==(Pair left, Pair right) { return left.x == right.x; } } class Program { public static int32 Main() { Pair left = Pair(); Pair right = Pair(); left.x = 4; right.x = 4; if (left != right) return 1; return 0; } }");
        Assert.True(userEquality.Succeeded, string.Join(Environment.NewLine, userEquality.Diagnostics));
        Assert.Contains("define i1 @luma_Pair_op_eq_2(%struct.Pair %arg0, %struct.Pair %arg1)", userEquality.LlvmIr!);
        Assert.Contains("xor i1", userEquality.LlvmIr);
        var equalityObjectPath = Path.Combine(Path.GetTempPath(), "lumasharp-struct-equality-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try { Assert.True(LlvmToolchain.VerifyAndEmitObject(userEquality.LlvmIr!, equalityObjectPath, "host", optimize: false).Succeeded); }
        finally { if (File.Exists(equalityObjectPath)) File.Delete(equalityObjectPath); }
        var declaredInequality = Compile("struct Pair { public static bool operator !=(Pair left, Pair right) { return true; } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(declaredInequality.Diagnostics, diagnostic => diagnostic.Code == "LS0401");
        var structFieldCollision = Compile("struct Pair { public static int32 value; public int32 value; } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(structFieldCollision.Diagnostics, diagnostic => diagnostic.Code == "LMC3013");
        var readonlyConstructorField = Compile("struct Pair { public readonly int32 value; public this(int32 value) { this.value = value; } } class Program { public static int32 Main() { Pair pair = Pair(7); return pair.value; } }");
        Assert.True(readonlyConstructorField.Succeeded, string.Join(Environment.NewLine, readonlyConstructorField.Diagnostics));
    }

    [Fact]
    public void StructFieldAndAutoPropertyInitializersRunBeforeParameterizedConstructorBodies()
    {
        const string source = "struct Configuration { public readonly int32 Seed = 40; public int32 Value { get; } = Program.InitialValue(); public int32 Adjusted { get; set; } = 0; public this(int32 ignored) { Adjusted += Seed + Value; } } class Program { public static int32 InitialValue() { return 2; } public static int32 Main() { Configuration value = Configuration(0); Configuration zero = default; return value.Seed == 40 && value.Value == 2 && value.Adjusted == 42 && zero.Seed == 0 && zero.Value == 0 ? 0 : 1; } }";
        var result = Compile(source);

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(result.LlvmIr!).ExitCode);

        var nullableReferenceField = Compile("struct WithNullableReferences { public string? Label { get; set; } = \"text\"; public this(int32 ignored) { } } class Program { public static int32 Main() { WithNullableReferences value = WithNullableReferences(0); return value.Label == \"text\" ? 0 : 1; } }");
        Assert.True(nullableReferenceField.Succeeded, string.Join(Environment.NewLine, nullableReferenceField.Diagnostics));
        var nullableReferenceExecution = LlvmJit.RunMain(nullableReferenceField.LlvmIr!);
        Assert.Equal(0, nullableReferenceExecution.ExitCode);
        Assert.Equal(0, nullableReferenceExecution.OutstandingAllocations);
    }

    [Fact]
    public void PropertiesUsingContextualBackingStorageAcceptInitializers()
    {
        const string source = "class ClassValue { public int32 Value { get => field; set => field = value; } = 40; } struct StructValue { public int32 Value { get => field; set => field = value; } = 2; public this(int32 ignored) { Value += 40; } } class Program { public static int32 Main() { ClassValue classValue = new ClassValue(); StructValue structValue = StructValue(0); if (classValue.Value != 40) return 10; if (structValue.Value != 42) return 20; return 0; } }";
        var result = Compile(source);

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(result.LlvmIr!).ExitCode);

        var computed = Compile("class Value { public int32 Number { get => 42; set { } } = 1; } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(computed.Diagnostics, diagnostic => diagnostic.Code == "LMC3000" && diagnostic.Message.Contains("without compiler backing storage", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("x86_64-pc-linux-gnu", 16)]
    [InlineData("i386-pc-linux-gnu", 8)]
    [InlineData("armv7a-unknown-linux-gnueabihf", 8)]
    [InlineData("arm64e-apple-ios", 16)]
    [InlineData("aarch64_32-apple-watchos", 8)]
    public void UsesTargetCAlignmentForUnmanagedStructLayout(string targetTriple, int expectedSize)
    {
        var result = Compile("unsafe struct NativeLayout { public uint8 prefix; public int machineWord; } unsafe class Program { public static int32 Main() { return sizeof(NativeLayout); } }",
            new CompilationOptions(UnsafeEnabled: true, TargetTriple: targetTriple));
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains($"ret i32 {expectedSize}", result.LlvmIr!);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-layout-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try
        {
            var emitted = LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, targetTriple, optimize: false);
            Assert.True(emitted.Succeeded, emitted.Error);
        }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }
    }

    [Fact]
    public void BindsNamedAndOptionalStructConstructorArgumentsInParameterOrder()
    {
        var result = Compile("struct Point { public int32 x; public int32 y; public this(int32 x, int32 y = 2) { this.x = x; this.y = y; } } class Program { public static int32 Main() { Point first = Point(y: 7, x: 3); Point second = Point(5); return first.x + first.y + second.x + second.y; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("call %struct.Point @luma_Point_ctor_2(i32 3, i32 7)", result.LlvmIr!);
        Assert.Contains("call %struct.Point @luma_Point_ctor_2(i32 5, i32 2)", result.LlvmIr);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-struct-ctor-args-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try { Assert.True(LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, "host", optimize: false).Succeeded); }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }
    }

    [Fact]
    public void SupportsOutArgumentsInStructConstructors()
    {
        var result = Compile("struct Pair { public int32 x; public this(out int32 result) { result = 7; this.x = 2; } } class Program { public static int32 Main() { Pair pair = Pair(out int32 result); return result + pair.x; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("call %struct.Pair @luma_Pair_ctor_1(ptr", result.LlvmIr!);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-struct-ctor-out-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try { Assert.True(LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, "host", optimize: false).Succeeded); }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }
    }

    [Fact]
    public void PassesRefInAndOutParametersAndChecksOutDefiniteAssignment()
    {
        var result = Compile("class Program { public static int32 Main() { int32 value = 1; Bump(ref value); Set(out value); return Read(in value); } public static void Bump(ref int32 target) { target++; } public static void Set(out int32 target) { if (true) target = 42; else target = 43; } public static int32 Read(in int32 target) { return target; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("define void @luma_Program_Bump_1(ptr %arg0)", result.LlvmIr!);
        Assert.Contains("define void @luma_Program_Set_1(ptr %arg0)", result.LlvmIr);
        Assert.Contains("call void @luma_Program_Bump_1(ptr %v", result.LlvmIr);
        Assert.Contains("call void @luma_Program_Set_1(ptr %v", result.LlvmIr);
        Assert.Contains("define i32 @luma_Program_Read_1(i32 %arg0)", result.LlvmIr);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-refs-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try { Assert.True(LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, "host", optimize: false).Succeeded); }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }

        var missing = Compile("class Program { public static int32 Main() { return 0; } public static void Missing(out int32 value) { } }");
        Assert.Contains(missing.Diagnostics, d => d.Code == "LS0720");
        var readBeforeWrite = Compile("class Program { public static int32 Main() { return 0; } public static int32 Missing(out int32 value) { return value; } }");
        Assert.Contains(readBeforeWrite.Diagnostics, d => d.Code == "LS0720" && d.Message.Contains("read before", StringComparison.Ordinal));
        var constantBranch = Compile("class Program { public static int32 Main() { return 0; } public static void Set(out int32 value) { if (true) value = 1; } }");
        Assert.True(constantBranch.Succeeded, string.Join(Environment.NewLine, constantBranch.Diagnostics));
        var incompleteBranch = Compile("class Program { public static int32 Main() { return 0; } public static void Missing(bool choose, out int32 value) { if (choose) value = 1; } }");
        Assert.Contains(incompleteBranch.Diagnostics, d => d.Code == "LS0720" && d.Message.Contains("before returning", StringComparison.Ordinal));

        foreach (var loopSource in new[]
        {
            "class Program { public static int32 Main() { int32 result = 0; Set(out result); return result == 42 ? 0 : 1; } public static void Set(out int32 value) { while (true) { value = 42; break; } } }",
            "class Program { public static int32 Main() { int32 result = 0; Set(out result); return result == 17 ? 0 : 1; } public static void Set(out int32 value) { do { value = 17; break; } while (true); } }",
            "class Program { public static int32 Main() { int32 result = 0; Set(out result); return result == 23 ? 0 : 1; } public static void Set(out int32 value) { for (;;) { value = 23; break; } } }",
            "class Program { public static int32 Main() { int32 result = 0; Set(out result); return result == 31 ? 0 : 1; } public static void Set(out int32 value) { while (true) { switch (1) { case 1: value = 31; break; default: value = 0; } break; } } }",
            "class Program { public static int32 Main() { int32 result = 0; Set(out result); return result == 43 ? 0 : 1; } public static void Set(out int32 value) { while (true) { try { break; } finally { value = 43; } } } }",
            "class Program { public static int32 Main() { int32 result = 0; Set(out result); return result == 41 ? 0 : 1; } public static void Set(out int32 value) { do { try { continue; } finally { value = 41; } } while (false); } }"
        })
        {
            var loopResult = Compile(loopSource);
            Assert.True(loopResult.Succeeded, string.Join(Environment.NewLine, loopResult.Diagnostics));
            var loopExecution = LlvmJit.RunMain(loopResult.LlvmIr!);
            Assert.Equal(0, loopExecution.ExitCode);
            Assert.Equal(0, loopExecution.OutstandingAllocations);
            Assert.Null(loopExecution.InteropError);
        }

        var incompleteLoopExit = Compile("class Program { public static int32 Main() { return 0; } public static void Missing(bool choose, out int32 value) { while (true) { if (choose) { value = 1; break; } else break; } } }");
        Assert.Contains(incompleteLoopExit.Diagnostics, diagnostic => diagnostic.Code == "LS0720" && diagnostic.Message.Contains("before returning", StringComparison.Ordinal));
        var unreachableAfterBreak = Compile("class Program { public static int32 Main() { return 0; } public static void Missing(out int32 value) { while (true) { break; value = 1; } } }");
        Assert.Contains(unreachableAfterBreak.Diagnostics, diagnostic => diagnostic.Code == "LS0720" && diagnostic.Message.Contains("before returning", StringComparison.Ordinal));
        var deadLoopReturn = Compile("class Program { public static int32 Main() { return 0; } public static void Set(out int32 value) { while (false) { return; } value = 5; } }");
        Assert.True(deadLoopReturn.Succeeded, string.Join(Environment.NewLine, deadLoopReturn.Diagnostics));
        var deadOutRead = Compile("class Program { public static int32 Main() { return 0; } public static void Read(out int32 value) { while (false) { int32 copy = value; } value = 5; } }");
        Assert.Contains(deadOutRead.Diagnostics, diagnostic => diagnostic.Code == "LS0720" && diagnostic.Message.Contains("read before", StringComparison.Ordinal));
        var finallyAssignment = Compile("class Program { public static int32 Main() { int32 result = 0; Set(out result); return result == 29 ? 0 : 1; } public static void Set(out int32 value) { try { return; } finally { value = 29; } } }");
        Assert.True(finallyAssignment.Succeeded, string.Join(Environment.NewLine, finallyAssignment.Diagnostics));
        var finallyExecution = LlvmJit.RunMain(finallyAssignment.LlvmIr!);
        Assert.Equal(0, finallyExecution.ExitCode);
        Assert.Equal(0, finallyExecution.OutstandingAllocations);
        Assert.Null(finallyExecution.InteropError);
        var missingFinallyAssignment = Compile("class Program { public static int32 Main() { return 0; } public static void Missing(out int32 value) { try { return; } finally { } } }");
        Assert.Contains(missingFinallyAssignment.Diagnostics, diagnostic => diagnostic.Code == "LS0720" && diagnostic.Message.Contains("before returning", StringComparison.Ordinal));
        var catchAssignment = Compile("class Program { public static int32 Main() { int32 result = 0; Set(out result); return result == 37 ? 0 : 1; } public static void Set(out int32 value) { try { throw new System.Exception(); } catch (...) { value = 37; } } }");
        Assert.True(catchAssignment.Succeeded, string.Join(Environment.NewLine, catchAssignment.Diagnostics));
        var catchExecution = LlvmJit.RunMain(catchAssignment.LlvmIr!);
        Assert.Equal(0, catchExecution.ExitCode);
        Assert.Equal(0, catchExecution.OutstandingAllocations);
        Assert.Null(catchExecution.InteropError);
        var catchIsUnreachable = Compile("class Program { public static int32 Main() { int32 result = 0; Set(out result); return result == 13 ? 0 : 1; } public static void Set(out int32 value) { try { value = 13; } catch (...) { } } }");
        Assert.True(catchIsUnreachable.Succeeded, string.Join(Environment.NewLine, catchIsUnreachable.Diagnostics));
        var assignmentBeforeThrowingCall = Compile("class Program { public static int32 Main() { int32 result = 0; Set(out result); return result == 19 ? 0 : 1; } public static void MayThrow() { throw new System.Exception(); } public static void Set(out int32 value) { try { value = 19; MayThrow(); } catch (...) { } } }");
        Assert.True(assignmentBeforeThrowingCall.Succeeded, string.Join(Environment.NewLine, assignmentBeforeThrowingCall.Diagnostics));
        var callExceptionBeforeAssignment = Compile("class Program { public static int32 Main() { return 0; } public static void MayThrow() { throw new System.Exception(); } public static void Set(out int32 value) { try { MayThrow(); value = 19; } catch (...) { } } }");
        Assert.Contains(callExceptionBeforeAssignment.Diagnostics, diagnostic => diagnostic.Code == "LS0720" && diagnostic.Message.Contains("before returning", StringComparison.Ordinal));
        const string typedCatchFlowSource = "class ExpectedError : System.Exception { } class UnrelatedError : System.Exception { } class Program { public static void Set(out int32 value, ExpectedError error) { try { throw error; } catch (UnrelatedError) { return; } catch (...) { value = 42; } } public static int32 Main() { int32 value = 0; Set(out value, new ExpectedError()); return value == 42 ? 0 : 1; } }";
        var typedCatchFlow = Compile(typedCatchFlowSource);
        Assert.True(typedCatchFlow.Succeeded, string.Join(Environment.NewLine, typedCatchFlow.Diagnostics));
        var typedCatchExecution = LlvmJit.RunMain(typedCatchFlow.LlvmIr!);
        Assert.Equal(0, typedCatchExecution.ExitCode);
        Assert.Equal(0, typedCatchExecution.OutstandingAllocations);
        Assert.Null(typedCatchExecution.InteropError);

        const string virtualBaseCatchFlowSource = "virtual class BaseError : System.Exception { } class SpecificError : BaseError { } class Program { public static void Set(out int32 value, BaseError error) { try { throw error; } catch (SpecificError) { return; } catch (...) { value = 42; } } public static int32 Main() { return 0; } }";
        var virtualBaseCatchFlow = Compile(virtualBaseCatchFlowSource);
        Assert.Contains(virtualBaseCatchFlow.Diagnostics, diagnostic => diagnostic.Code == "LS0720" && diagnostic.Message.Contains("before returning", StringComparison.Ordinal));
        var immutableArgument = Compile("class Program { public static int32 Main() { let value = 1; Update(ref value); return value; } public static void Update(ref int32 value) { value = 2; } }");
        Assert.Contains(immutableArgument.Diagnostics, d => d.Code == "LS0751");
        var readonlyMutation = Compile("class Program { public static int32 Main() { return Read(1); } public static int32 Read(in int32 value) { value = 2; return value; } }");
        Assert.Contains(readonlyMutation.Diagnostics, d => d.Code == "LMC3012");
    }

    [Fact]
    public void UsesTargetPointerWidthForNativeIntegersAndSizeof()
    {
        const string source = "class Program { public static int32 Main() { return 0; } public static unsafe int32 NativeSize() { return sizeof(int); } public static unsafe int32 PointerSize() { return sizeof(int32*); } public static int Add(int left, int right) { return left + right; } }";
        var x86 = Compile(source, new CompilationOptions(UnsafeEnabled: true, TargetTriple: "i386-pc-linux-gnu"));
        Assert.True(x86.Succeeded, string.Join(Environment.NewLine, x86.Diagnostics));
        Assert.Contains("define i32 @luma_Program_Add_2(i32 %arg0, i32 %arg1)", x86.LlvmIr!);
        Assert.Contains("ret i32 4", x86.LlvmIr);
        Assert.DoesNotContain("define i64 @luma_Program_Add_2", x86.LlvmIr);

        var x64 = Compile(source, new CompilationOptions(UnsafeEnabled: true, TargetTriple: "x86_64-unknown-linux-gnu"));
        Assert.True(x64.Succeeded, string.Join(Environment.NewLine, x64.Diagnostics));
        Assert.Contains("define i64 @luma_Program_Add_2(i64 %arg0, i64 %arg1)", x64.LlvmIr!);
        Assert.Contains($"ret i32 {IntPtr.Size}", x64.LlvmIr);
    }

    [Fact]
    public void EmitsCAbiExternDeclarationsAndRequiresUnsafeCalls()
    {
        const string source = "class Native { public static extern int32 Read(int8 code); public static extern bool Ready(bool enabled); } class Program { public static unsafe int32 Main() { if (Native.Ready(true)) { return Native.Read((int8)5); } return 0; } }";
        var result = Compile(source, new CompilationOptions(UnsafeEnabled: true));
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("declare i32 @Read(i8 signext)", result.LlvmIr!);
        Assert.Contains("declare zeroext i1 @Ready(i1 zeroext)", result.LlvmIr);
        Assert.Contains("call zeroext i1 @Ready(i1 zeroext 1)", result.LlvmIr);
        Assert.Contains("call i32 @Read(i8 signext %t", result.LlvmIr);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-extern-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try { Assert.True(LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, "host", optimize: false).Succeeded); }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }

        var unsafeRequired = Compile("class Native { public static extern int32 Read(int32 code); } class Program { public static int32 Main() { return Native.Read(5); } }");
        Assert.Contains(unsafeRequired.Diagnostics, diagnostic => diagnostic.Code == "LMC3000" && diagnostic.Message.Contains("unsafe context", StringComparison.Ordinal));

        var invalidAbi = Compile("class Native { public static extern int32 Read(string value); } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(invalidAbi.Diagnostics, diagnostic => diagnostic.Code == "LS2101");
    }

    [Fact]
    public void SupportsTypedAndInferredInlineOutVariables()
    {
        const string source = "class Program { public static int32 Main() { if (Try(out int32 parsed)) { return parsed; } Try(out var inferred); return inferred; } public static bool Try(out int32 result) { result = 42; return true; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("call i1 @luma_Program_Try_1(ptr %t", result.LlvmIr!);
        Assert.Contains("load i32, ptr %t", result.LlvmIr);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-inline-out-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try { Assert.True(LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, "host", optimize: false).Succeeded); }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }

        var wrongParameterMode = Compile("class Program { public static int32 Main() { Read(out int32 value); return 0; } public static void Read(int32 value) { } }");
        Assert.Contains(wrongParameterMode.Diagnostics, diagnostic => diagnostic.Code == "LS0751");

        var conflictingName = Compile("class Program { public static int32 Main() { int32 value = 0; Read(out int32 value); return value; } public static void Read(out int32 result) { result = 1; } }");
        Assert.Contains(conflictingName.Diagnostics, diagnostic => diagnostic.Code == "LS0201");
    }

    [Fact]
    public void ResolvesQualifiedStaticCallsAcrossNamespacesAndSourceFiles()
    {
        var result = LumaCompiler.Compile([
            ("main.ls", "namespace Demo; class Program { public static int32 Main() { return MathUtil.Add(20, 22); } }"),
            ("math.ls", "namespace Demo; class MathUtil { public static int32 Add(int32 left, int32 right) { return left + right; } }")
        ]);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("call i32 @luma_Demo_MathUtil_Add_2", result.LlvmIr!);

        var unqualified = LumaCompiler.Compile([
            ("main.ls", "namespace Demo; class Program { public static int32 Main() { return Add(20, 22); } }"),
            ("math.ls", "namespace Demo; class MathUtil { public static int32 Add(int32 left, int32 right) { return left + right; } }")
        ]);
        Assert.Contains(unqualified.Diagnostics, d => d.Code == "LS0751");
    }

    [Fact]
    public void ResolvesImportedNamespacesAliasesAndAmbiguousNames()
    {
        var imported = LumaCompiler.Compile([
            ("main.ls", "using MathTools; namespace App; public class Program { public static int32 Main() { return Operations.Add(20, 22); } }"),
            ("ops.ls", "namespace MathTools; public class Operations { public static int32 Add(int32 left, int32 right) { return left + right; } }")
        ]);
        Assert.True(imported.Succeeded, string.Join(Environment.NewLine, imported.Diagnostics));
        Assert.Contains("call i32 @luma_MathTools_Operations_Add_2", imported.LlvmIr!);

        var alias = LumaCompiler.Compile([
            ("main.ls", "using Ops = MathTools.Operations; namespace App; public class Program { public static int32 Main() { return Ops.Add(20, 22); } }"),
            ("ops.ls", "namespace MathTools; public class Operations { public static int32 Add(int32 left, int32 right) { return left + right; } }")
        ]);
        Assert.True(alias.Succeeded, string.Join(Environment.NewLine, alias.Diagnostics));

        var ambiguous = LumaCompiler.Compile([
            ("main.ls", "using First; using Second; namespace App; public class Program { public static int32 Main() { return Utility.Get(); } }"),
            ("first.ls", "namespace First; public class Utility { public static int32 Get() { return 1; } }"),
            ("second.ls", "namespace Second; public class Utility { public static int32 Get() { return 2; } }")
        ]);
        Assert.Contains(ambiguous.Diagnostics, d => d.Code == "LS2001" && d.Message.Contains("ambiguous", StringComparison.OrdinalIgnoreCase));

        var notImported = LumaCompiler.Compile([
            ("main.ls", "namespace App; public class Program { public static int32 Main() { return Utility.Get(); } }"),
            ("first.ls", "namespace First; public class Utility { public static int32 Get() { return 1; } }")
        ]);
        Assert.Contains(notImported.Diagnostics, d => d.Code == "LS0200");
    }

    [Fact]
    public void SupportsIntegralEnumsConstantsOperatorsCastsAndSwitches()
    {
        var result = LumaCompiler.Compile([
            ("colors.ls", "namespace Demo; public enum Color { Red, Green = 4, Blue } public enum Flags : uint8 { None = 0, A = 1, B = A << 1, C = Flags.B << 1, All = A | B | C }"),
            ("main.ls", "using Demo; namespace App; public class Program { public const uint8 Mask = Flags.All; public static uint8 MutableMask = Flags.All; public static int32 Main() { Flags flags = Flags.A | Flags.C; if ((flags & Flags.C) != Flags.None) { Color color = Color.Blue; switch (color) { case Color.Red: return 1; case Color.Green: return 2; case Color.Blue: return (int32)color; } } return 0; } public static uint8 AsByte(Flags value) { return value; } public static Flags FromByte(uint8 value) { return (Flags)value; } public static uint8 EnumValues() { return Program.Mask; } public static unsafe int32 Size() { return sizeof(Flags); } }")
        ], new CompilationOptions(UnsafeEnabled: true));
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("define i8 @luma_App_Program_AsByte_1", result.LlvmIr!);
        Assert.Contains("switch.test", result.LlvmIr);
        Assert.Contains("icmp ne i8", result.LlvmIr);
        Assert.Contains("ret i32 1", result.LlvmIr);
        Assert.Contains("define i8 @luma_App_Program_EnumValues_0()", result.LlvmIr);
        Assert.Contains("ret i8 7", result.LlvmIr);
        Assert.Contains("@luma_global_App_Program_MutableMask = global i8 7", result.LlvmIr);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-enum-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try { Assert.True(LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, "host", optimize: false).Succeeded); }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }

        var invalidUnderlying = Compile("enum Invalid : bool { No = 0 } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(invalidUnderlying.Diagnostics, d => d.Code == "LS1101");
        var overflow = Compile("enum Small : uint8 { Max = 255, Next } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(overflow.Diagnostics, d => d.Code == "LS1103");
    }

    [Fact]
    public void ReportsAmbiguousAndMissingApplicableOverloads()
    {
        var ambiguous = Compile("class Program { public static int32 Main() { uint32 value = 1u; return Pick(value); } public static int32 Pick(int64 value) { return 1; } public static int32 Pick(uint64 value) { return 2; } }");
        Assert.Contains(ambiguous.Diagnostics, d => d.Code == "LS0750");

        var missing = Compile("class Program { public static int32 Main() { return Pick(true); } public static int32 Pick(int32 value) { return value; } }");
        Assert.Contains(missing.Diagnostics, d => d.Code == "LS0751");
    }

    [Fact]
    public void ReturnTypeDoesNotDistinguishDuplicateMethodSignatures()
    {
        var result = Compile("class Program { public static void Main() {} public static int32 Value(int32 x) { return x; } public static int64 Value(int32 y) { return y; } }");
        Assert.Contains(result.Diagnostics, d => d.Code == "LMC3013");
    }

    [Fact]
    public void EmitsIeeeFloatingPointArithmeticAndComparisons()
    {
        var result = Compile("class Program { public static int32 Main() { if (Scale(2.5f) > 0.0f) return 0; return 1; } public static float Scale(float value) { return value * 2; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("fmul float", result.LlvmIr!);
        Assert.Contains("sitofp i32 2 to float", result.LlvmIr);
        Assert.Contains("fcmp ogt float", result.LlvmIr);
        Assert.DoesNotContain("llvm.smul.with.overflow.float", result.LlvmIr);
    }

    [Fact]
    public void PromotesUnsignedAndSignedOperandsAndNarrowUnaryValues()
    {
        var mixed = Compile("class Program { public static int32 Main() { let int64 value = Mix(1u, 2); if (value == 3L) return 0; return 1; } public static int64 Mix(uint32 left, int32 right) { return left + right; } }");
        Assert.True(mixed.Succeeded, string.Join(Environment.NewLine, mixed.Diagnostics));
        Assert.Contains("zext i32", mixed.LlvmIr!);
        Assert.Contains("sext i32", mixed.LlvmIr);
        Assert.Contains("@llvm.sadd.with.overflow.i64", mixed.LlvmIr);
        var narrow = Compile("class Program { public static int32 Main() { int8 value = 127; return -value; } }");
        Assert.True(narrow.Succeeded, string.Join(Environment.NewLine, narrow.Diagnostics));
        Assert.Contains("sext i8", narrow.LlvmIr!);
    }

    [Fact]
    public void CompoundAssignmentChecksNarrowingOverflow()
    {
        var result = Compile("class Program { public static int32 Main() { int8 value = 127; value += 1; return value; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("llvm.sadd.with.overflow.i32", result.LlvmIr!);
        Assert.Contains("trunc i32", result.LlvmIr);
        Assert.Contains("luma_throw_overflow", result.LlvmIr);
    }

    [Fact]
    public void CheckedAndUncheckedContextsControlIntegerOverflowChecks()
    {
        const string source = "class Program { public static int32 Main() { unsafe { let a = unchecked(1 + 2); let b = checked(3 + 4); unchecked { let c = a + b; } return a + b; } } }";
        var result = Compile(source, new CompilationOptions(UnsafeEnabled: true));
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("add i32", result.LlvmIr!);
        Assert.Contains("@llvm.sadd.with.overflow.i32", result.LlvmIr);
    }

    [Fact]
    public void UncheckedExpressionOutsideUnsafeContextIsRejected()
    {
        var result = Compile("class Program { public static int32 Main() { return unchecked(1 + 2); } }");
        Assert.Contains(result.Diagnostics, d => d.Code == "LS0501");
    }

    [Fact]
    public void CheckedBlocksPreserveReturnFlowAndUseCheckedOperations()
    {
        var result = Compile("class Program { public static int32 Main() { checked { return 1 + 2; } } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("@llvm.sadd.with.overflow.i32", result.LlvmIr!);
        Assert.DoesNotContain(result.Diagnostics, d => d.Code == "LS0630");
    }

    [Fact]
    public void PreservesNamespaceIdentityInLlvmSymbols()
    {
        var result = LumaCompiler.Compile([
            ("alpha.ls", "namespace Alpha; class Program { public static void Main() {} }"),
            ("beta.ls", "namespace Beta; class Program { public static int32 Value() { return 1; } }")
        ]);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("@luma_Alpha_Program_Main_0", result.LlvmIr!);
        Assert.Contains("@luma_Beta_Program_Value_0", result.LlvmIr);
    }

    [Fact]
    public void RequiresOneFileScopedNamespaceAndLeadingUsingDirectives()
    {
        var blockScoped = Compile("namespace Alpha { class Program { public static int32 Main() { return 0; } } }");
        Assert.Contains(blockScoped.Diagnostics, diagnostic => diagnostic.Code == "LMC3000" && diagnostic.Message.Contains("Block-scoped namespace", StringComparison.Ordinal));
        Assert.Null(blockScoped.LlvmIr);

        var multipleNamespaces = Compile("namespace Alpha; namespace Beta; class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(multipleNamespaces.Diagnostics, diagnostic => diagnostic.Code == "LS0001" && diagnostic.Message.Contains("only one", StringComparison.Ordinal));
        Assert.Null(multipleNamespaces.LlvmIr);

        var namespaceAfterType = Compile("class Earlier { } namespace Alpha; class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(namespaceAfterType.Diagnostics, diagnostic => diagnostic.Code == "LS0001" && diagnostic.Message.Contains("precede every type", StringComparison.Ordinal));
        Assert.Null(namespaceAfterType.LlvmIr);

        var usingAfterNamespace = Compile("namespace Alpha; using System; class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(usingAfterNamespace.Diagnostics, diagnostic => diagnostic.Code == "LS0001" && diagnostic.Message.Contains("before the namespace", StringComparison.Ordinal));
        Assert.Null(usingAfterNamespace.LlvmIr);
    }

    [Fact]
    public void GenericTypeArityAndConstraintsAreReportedAndNonVirtualBaseTypesAreRejected()
    {
        var wrongArity = Compile("class Box<T> {} class Program { public static int32 Main() { Box<int32, int32> value; return 0; } }");
        Assert.Contains(wrongArity.Diagnostics, d => d.Code == "LMC3000" && d.Message.Contains("requires 1 type argument"));
        var constrained = Compile("class Box<T> where T : struct {} class Program { public static int32 Main() { Box<string> value = new Box<string>(); return 0; } }");
        Assert.Contains(constrained.Diagnostics, diagnostic => diagnostic.Code == "LS1102" && diagnostic.Message.Contains("does not satisfy constraint 'struct'"));
        var inherited = Compile("class Base {} class Derived : Base { public static void Main() {} }");
        Assert.Contains(inherited.Diagnostics, d => d.Code == "LS0830");
    }

    [Fact]
    public void ClassConstraintsAcceptNonNullableDelegateTypes()
    {
        const string source = "delegate int32 Reader(); delegate T Factory<T>(); class Holder<T> where T : class { } class Program { public static T Identity<T>(T value) where T : class { return value; } public static int32 Main() { Reader reader = () => 20; Factory<int32> factory = () => 22; Holder<Reader> readerHolder = new Holder<Reader>(); Holder<Factory<int32>> factoryHolder = new Holder<Factory<int32>>(); Holder<object> objectHolder = new Holder<object>(); Reader identityReader = Identity(reader); Factory<int32> identityFactory = Identity(factory); return identityReader() + identityFactory() == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var nullableDelegate = Compile("delegate int32 Reader(); class Holder<T> where T : class { } class Program { public static int32 Main() { Holder<Reader?> holder = new Holder<Reader?>(); return 0; } }");
        Assert.Contains(nullableDelegate.Diagnostics, diagnostic => diagnostic.Code == "LS1102" && diagnostic.Message.Contains("does not satisfy constraint 'class'", StringComparison.Ordinal));
    }

    [Fact]
    public void GenericClassesSpecializeFieldsConstructorsAndMethods()
    {
        const string source = "class Box<T> { public T Value; public this(T value) { Value = value; } public T Get() { return Value; } } class Program { public static int32 Main() { Box<int32> box = new Box<int32>(42); return box.Get() == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("__g_", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Theory]
    [InlineData("Map<T, Pair<int32, U>>?*", "Map<int32,Pair<int32,string>>?*")]
    [InlineData("T[][]", "int32[][]")]
    [InlineData("T[16]", "int32[16]")]
    public void StructuredTypeNamesRewriteOnlyTypeIdentifiers(string typeName, string expected)
    {
        Assert.True(TypeNameSyntax.TryParse(typeName, out var parsed), $"Could not parse '{typeName}'.");
        var rewritten = parsed.Rewrite(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["T"] = "int32",
            ["U"] = "string",
            ["Map"] = "Map",
        });
        Assert.Equal(expected, rewritten.ToString());
    }

    [Fact]
    public void StructuredTypeNamesDiscoverNestedGenericUsesAndExactTypeParameters()
    {
        Assert.True(TypeNameSyntax.TryParse("Outer<T, Inner<Pair<U, int32>>[]>?", out var parsed));
        var genericNodes = parsed.DescendantsAndSelf().Where(node => node.Arguments.Count > 0).ToArray();
        Assert.Equal(["Outer", "Inner", "Pair"], genericNodes.Select(node => node.Name));
        Assert.True(GenericTypeSpecializer.ReferencesAnyTypeParameter("Container<Tail, T>", new HashSet<string>(["T"], StringComparer.Ordinal)));
        Assert.False(GenericTypeSpecializer.ReferencesAnyTypeParameter("Container<Tail>", new HashSet<string>(["T"], StringComparer.Ordinal)));

        Assert.True(TypeNameSyntax.TryParse("int32[,][]?*", out var arrayType));
        Assert.Equal(new ArrayRankTypeNameSuffix(2), arrayType.Suffixes[0]);
        Assert.Equal(new ArrayRankTypeNameSuffix(1), arrayType.Suffixes[1]);
        Assert.IsType<NullableTypeNameSuffix>(arrayType.Suffixes[2]);
        Assert.IsType<PointerTypeNameSuffix>(arrayType.Suffixes[3]);
        Assert.True(TypeNameSyntax.TryParse("int32[16]", out var inlineArrayType));
        Assert.Equal(new InlineArrayTypeNameSuffix("16"), inlineArrayType.Suffixes[0]);
        Assert.True(TypeNameSyntax.TryParse("int32[ , ]", out var spacedRank));
        Assert.Equal("int32[,]", spacedRank.ToString());

        Assert.True(SemanticAnalyzer.TryParseManagedArrayType("Box<int32>[,]?", out var arrayElementType, out var arrayRank));
        Assert.Equal("Box<int32>", arrayElementType);
        Assert.Equal(2, arrayRank);
        Assert.True(SemanticAnalyzer.TryParseManagedArrayType("Item?[]", out var nullableElementType, out _));
        Assert.Equal("Item?", nullableElementType);
        Assert.False(SemanticAnalyzer.TryParseManagedArrayType("int32[16]", out _, out _));
        Assert.True(SemanticAnalyzer.TryParseInlineArrayType("Pair<int32>[16]", out var inlineElementType, out var inlineCount));
        Assert.Equal("Pair<int32>", inlineElementType);
        Assert.Equal(16, inlineCount);
    }

    [Fact]
    public void UsingAliasesSupportConstructedAndNestedGenericTypes()
    {
        const string source = "using IntBox = Demo.Box<int32>; using NestedBox = Demo.Box<Demo.Box<int32>>; namespace Demo; public class Box<T> { T value; public this(T value) { this.value = value; } public T Read() { return value; } } public class Program { public static int32 Main() { IntBox simple = new IntBox(40); NestedBox nested = new NestedBox(new IntBox(2)); return simple.Read() == 40 && nested.Read().Read() == 2 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(result.LlvmIr!).ExitCode);
    }

    [Fact]
    public void GenericStructsAndNestedGenericUsesAreSpecialized()
    {
        const string source = "struct Cell<T> where T : struct { public T Value; } class Box<T> { T _value; public this(T value) { _value = value; } public T Read() { return _value; } } class Program { public static int32 Main() { Box<Cell<int32>> box = new Box<Cell<int32>>(default(Cell<int32>)); Cell<int32> cell = box.Read(); cell.Value = 42; return cell.Value == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
    }

    [Fact]
    public void GenericInterfacesSpecializeAndDispatchThroughClosedTypes()
    {
        const string source = "interface IValue<T> { T Get(); } class Box<T> : IValue<T> { T _value; public this(T value) { _value = value; } public T Get() { return _value; } } class Program { public static int32 Main() { IValue<int32> value = new Box<int32>(42); return value.Get() == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void GenericInstantiationsHaveDistinctTypeAndStaticStorage()
    {
        const string source = "class Counter<T> { public static int32 Value = Initialize(); public static int32 Initialize() { return 1; } static this() { Value += 1; } } class Program { public static int32 Main() { return Counter<int32>.Value == 2 && Counter<double>.Value == 2 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void GenericConstructorConstraintsAndTypeSubstitutionAreChecked()
    {
        const string source = "class Number { int32 _value; public this(int32 value) { _value = value; } public int32 Read() { return _value; } } class Factory<T> where T : new(int32) { public T Make(int32 value) { return new T(value); } } class Program { public static int32 Main() { Factory<Number> factory = new Factory<Number>(); Number value = factory.Make(42); return value.Read() == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
    }

    [Fact]
    public void GenericClassAndInterfaceConstraintsAcceptMatchingArguments()
    {
        const string source = "interface IReadable { int32 Read(); } class Data : IReadable { public int32 Read() { return 42; } } class Holder<T> where T : class, IReadable, new() { T _item; public this() { _item = new T(); } public int32 Read() { return _item.Read(); } } class Program { public static int32 Main() { Holder<Data> holder = new Holder<Data>(); return holder.Read() == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void InlineArrayArgumentsSatisfyStructAndRejectClassConstraints()
    {
        const string validSource = "class Marker<T> where T : struct { } unsafe class Program { public static int32 Main() { Marker<int32[2]> marker = new Marker<int32[2]>(); return marker == null ? 1 : 0; } }";
        var valid = Compile(validSource, new CompilationOptions(UnsafeEnabled: true));
        Assert.True(valid.Succeeded, string.Join(Environment.NewLine, valid.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(valid.LlvmIr!).ExitCode);

        const string invalidSource = "class Marker<T> where T : class { } unsafe class Program { public static int32 Main() { Marker<int32[2]> marker = new Marker<int32[2]>(); return 0; } }";
        var invalid = Compile(invalidSource, new CompilationOptions(UnsafeEnabled: true));
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == "LS1102" && diagnostic.Message.Contains("does not satisfy constraint 'class'"));
    }

    [Fact]
    public void DefaultConstraintAllowsDefaultValuesAndInitializedGenericArrays()
    {
        var defaultValue = Compile("""
            class Item { }
            class Slot<T> where T : default
            {
                public T Get() { return default(T); }
                public T GetInferred() { T value = default; return value; }
                public T[] Create(int32 length) { return new T[length]; }
            }
            struct Holder<T> where T : default
            {
                public T value;
                public T GetUninitializedLocal() { T item; return item; }
            }
            class Program
            {
                public static T GetDefault<T>() where T : default { return default(T); }
                public static int32 Main()
                {
                    Slot<int32> numbers = new Slot<int32>();
                    Slot<Item?> references = new Slot<Item?>();
                    int32 number = numbers.Get();
                    int32 inferredNumber = numbers.GetInferred();
                    int32[] numberArray = numbers.Create(2);
                    Item? item = references.Get();
                    Item? inferred = references.GetInferred();
                    Item?[] values = references.Create(2);
                    int32 methodDefault = GetDefault<int32>();
                    Item? methodNullable = GetDefault<Item?>();
                    Holder<int32> numberHolder = default(Holder<int32>);
                    int32 heldNumber = numberHolder.GetUninitializedLocal();
                    int32 concreteDefault = default(int32);
                    Item? concreteNullable = default(Item?);
                    if (number != 0 || inferredNumber != 0 || numberArray[0] != 0) return 1;
                    if (item != null || inferred != null || values[0] != null) return 2;
                    if (heldNumber != 0) return 3;
                    if (methodDefault != 0 || methodNullable != null || concreteDefault != 0 || concreteNullable != null) return 4;
                    return 0;
                }
            }
            """);
        Assert.True(defaultValue.Succeeded, string.Join(Environment.NewLine, defaultValue.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(defaultValue.LlvmIr!).ExitCode);

        var invalidInstantiation = Compile("class Item { } class Slot<T> where T : default { public T[] Create(int32 length) { return new T[length]; } } class Program { public static int32 Main() { Slot<Item> slot = new Slot<Item>(); return 0; } }");
        Assert.Contains(invalidInstantiation.Diagnostics, diagnostic => diagnostic.Code == "LS1102" && diagnostic.Message.Contains("does not satisfy constraint 'default'", StringComparison.Ordinal));

        var unconstrainedDefault = Compile("class Slot<T> { public T Get() { return default(T); } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(unconstrainedDefault.Diagnostics, diagnostic => diagnostic.Code == "LMC3000" && diagnostic.Message.Contains("requires a 'default'", StringComparison.Ordinal));

        var uninitializedLocal = Compile("class Slot<T> { public T Get() { T value; return value; } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(uninitializedLocal.Diagnostics, diagnostic => diagnostic.Code == "LMC3000" &&
            diagnostic.Message.Contains("Uninitialized local of type parameter 'T' requires a 'default'", StringComparison.Ordinal));

        var structField = Compile("struct Slot<T> { public T value; } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(structField.Diagnostics, diagnostic => diagnostic.Code == "LMC3000" &&
            diagnostic.Message.Contains("Generic struct field 'T' requires a 'default'", StringComparison.Ordinal));

        var unconstrainedArray = Compile("class Item { } class Slot<T> { T[] arr; public this() { arr = new T[0]; } public T[] Get() { return arr; } } class Program { public static int32 Main() { Slot<Item> slot = new Slot<Item>(); Item[] values = slot.Get(); return values.Length; } }");
        Assert.True(unconstrainedArray.Succeeded, string.Join(Environment.NewLine, unconstrainedArray.Diagnostics));
        var unconstrainedArrayExecution = LlvmJit.RunMain(unconstrainedArray.LlvmIr!);
        Assert.Equal(0, unconstrainedArrayExecution.ExitCode);
        Assert.Equal(0, unconstrainedArrayExecution.OutstandingAllocations);

        var uninitializedReferenceArray = Compile("class Item { } class Slot<T> { public T[] Create(int32 length) { return new T[length]; } } class Program { public static int32 Main() { Slot<Item> slot = new Slot<Item>(); Item[] values = slot.Create(1); return 0; } }");
        Assert.Contains(uninitializedReferenceArray.Diagnostics, diagnostic => diagnostic.Code == "LMC3000" && diagnostic.Message.Contains("requires a 'default'", StringComparison.Ordinal));

        var nullableArray = Compile("class Item { } class Slot<T> { public T?[] Create(int32 length) { return new T?[length]; } } class Program { public static int32 Main() { Slot<Item> slot = new Slot<Item>(); Item?[] values = slot.Create(2); return values.Length == 2 && values[0] == null ? 0 : 1; } }");
        Assert.True(nullableArray.Succeeded, string.Join(Environment.NewLine, nullableArray.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(nullableArray.LlvmIr!).ExitCode);

        var valid = Compile("class Slot<T> where T : struct { public T Get() { return default(T); } } class Program { public static int32 Main() { Slot<int32> slot = new Slot<int32>(); return slot.Get(); } }");
        Assert.True(valid.Succeeded, string.Join(Environment.NewLine, valid.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(valid.LlvmIr!).ExitCode);

        const string valueArraySource = "class Slot<T> where T : struct { public T[] Create(int32 length) { return new T[length]; } } class Program { public static int32 Main() { Slot<int32> slot = new Slot<int32>(); int32[] values = slot.Create(2); return values.Length == 2 ? 0 : 1; } }";
        var valueArray = Compile(valueArraySource);
        Assert.True(valueArray.Succeeded, string.Join(Environment.NewLine, valueArray.Diagnostics));
        var valueArrayExecution = LlvmJit.RunMain(valueArray.LlvmIr!);
        Assert.Equal(0, valueArrayExecution.ExitCode);
        Assert.Equal(0, valueArrayExecution.OutstandingAllocations);
    }

    [Fact]
    public void ClosedGenericInterfacesSupportContractsAndDispatch()
    {
        const string source = "interface IValue<T> { T Get(); } class Box : IValue<int32> { public int32 Get() { return 42; } } class Program { public static int32 Main() { IValue<int32> value = new Box(); return value.Get() == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);

    }

    [Fact]
    public void InheritedGenericInterfaceSupportsStructBoxingAndDispatch()
    {
        const string source = "interface IValue<T> { T Get(); } interface IReadValue<T> : IValue<T> { } struct Value : IReadValue<int32> { public int32 Get() { return 42; } } class Program { public static int32 Main() { IReadValue<int32> value = Value(); return value.Get() == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void GenericMethodsSupportExplicitArgumentsAndSimpleInference()
    {
        const string source = "class Program { public static T Identity<T>(T value) { return value; } public static int32 Main() { int32 explicitValue = Identity<int32>(19); int32 inferredValue = Identity(23); return explicitValue + inferredValue == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
    }

    [Fact]
    public void GenericMethodInferenceUnifiesNestedGenericParameterTypes()
    {
        const string source = "class Box<T> { T value; public this(T value) { this.value = value; } public T Read() { return value; } } class Program { public static T Unwrap<T>(Box<T> box) { return box.Read(); } public static int32 Main() { Box<int32> box = new Box<int32>(42); return Unwrap(box) == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);

    }

    [Fact]
    public void GenericMethodInferenceUsesExactBoundsForInvariantContainers()
    {
        const string source = "class Box<T> { } class Left { } class Right { } class Program { public static int32 Merge<T>(Box<T> first, Box<T> second) { return 0; } public static int32 Main() { Box<Left> left = new Box<Left>(); Box<Right> right = new Box<Right>(); return Merge(left, right); } }";
        var result = Compile(source);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS1101" && diagnostic.Message.Contains("could not be inferred"));

        const string arraySource = "class Left { } class Right { } class Program { public static int32 Merge<T>(T[] first, T[] second) { return 0; } public static int32 Main() { Left[] left = { new Left() }; Right[] right = { new Right() }; return Merge(left, right); } }";
        var arrayResult = Compile(arraySource);
        Assert.Contains(arrayResult.Diagnostics, diagnostic => diagnostic.Code == "LS1101" && diagnostic.Message.Contains("could not be inferred"));

        const string constructedConversionSource = "class Left { } class Right { } interface ISource<T> { } class LeftSource : ISource<Left> { } class RightSource : ISource<Right> { } virtual class Base<T> { } class LeftBase : Base<Left> { } class RightBase : Base<Right> { } class Program { public static int32 MergeSource<T>(ISource<T> first, ISource<T> second) { return 0; } public static int32 MergeBase<T>(Base<T> first, Base<T> second) { return 0; } public static int32 Main() { return MergeSource(new LeftSource(), new RightSource()) + MergeBase(new LeftBase(), new RightBase()); } }";
        var constructedConversion = Compile(constructedConversionSource);
        Assert.Equal(2, constructedConversion.Diagnostics.Count(diagnostic => diagnostic.Code == "LS1101" && diagnostic.Message.Contains("could not be inferred")));

        const string validSource = "class Box<T> { } class Item { } class Other { } class Program { public static int32 Merge<T>(Box<T> first, Box<T> second) { return 20; } public static int32 Merge(object first, object second) { return 1; } public static int32 MergeArrays<T>(T[] first, T[] second) { return 22; } public static int32 Main() { Box<Item> box = new Box<Item>(); Box<Other> other = new Box<Other>(); Item[] items = { new Item() }; return Merge(box, box) + MergeArrays(items, items) == 42 && Merge(box, other) == 1 ? 0 : 1; } }";
        var valid = Compile(validSource);
        Assert.True(valid.Succeeded, string.Join(Environment.NewLine, valid.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(valid.LlvmIr!).ExitCode);
    }

    [Fact]
    public void GenericMethodInferenceUsesExactBoundsForByReferenceArguments()
    {
        const string source = "class Program { public static T First<T>(ref T left, ref T right) { return left; } public static T Read<T>(in T value) { return value; } public static T Set<T>(out T value) where T : struct { value = default(T); return value; } public static int32 Main() { int32 left = 42; int32 right = 0; int32 output; return First(ref left, ref right) == 42 && Read(in left) == 42 && Set(out output) == 0 && output == 0 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        const string conflictingBounds = "class Base { } class Derived : Base { } class Program { public static T First<T>(ref T left, ref T right) { return left; } public static int32 Main() { Base left = new Derived(); Derived right = new Derived(); First(ref left, ref right); return 0; } }";
        var conflict = Compile(conflictingBounds);
        Assert.Contains(conflict.Diagnostics, diagnostic => diagnostic.Code == "LS1101" && diagnostic.Message.Contains("could not be inferred", StringComparison.Ordinal));
    }

    [Fact]
    public void GenericMethodInferenceUnifiesConstructedInterfaceImplementations()
    {
        const string source = "interface ISource<T> { T Read(); } class Source : ISource<int32> { public int32 Read() { return 42; } } class Program { public static T Unwrap<T>(ISource<T> source) { return source.Read(); } public static int32 Main() { return Unwrap(new Source()) == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void MoreSpecificGenericOverloadWinsAfterSpecialization()
    {
        const string source = "class Box<T> { } class Program { public static int32 Select<T>(T value) { return 1; } public static int32 Select<T>(Box<T> value) { return 2; } public static int32 Main() { Box<int32> box = new Box<int32>(); return Select(box) == 2 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(result.LlvmIr!).ExitCode);

        const string incomparableSource = "class Box<T> { } class Program { public static int32 Select<T>(T left, Box<int32> right) { return 1; } public static int32 Select<T>(Box<int32> left, T right) { return 2; } public static int32 Main() { Box<int32> left = new Box<int32>(); Box<int32> right = new Box<int32>(); return Select(left, right); } }";
        var incomparable = Compile(incomparableSource);
        Assert.Contains(incomparable.Diagnostics, diagnostic => diagnostic.Code == "LS0750" && diagnostic.Message.Contains("ambiguous"));
    }

    [Fact]
    public void GenericMethodInferenceUnifiesConstructedGenericBaseClasses()
    {
        const string source = "virtual class Box<T> { public T value; public T Read() { return value; } } class IntBox : Box<int32> { public this() { value = 42; } } class Program { public static T Unwrap<T>(Box<T> box) { return box.Read(); } public static int32 Main() { return Unwrap(new IntBox()) == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void GenericMethodInferenceCombinesNumericAndReferenceLowerBounds()
    {
        const string source = "virtual class Base { public this() {} } class Left : Base { } class Right : Base { } class Program { public static T First<T>(T left, T right) { return left; } public static int32 Main() { int64 wide = 2; int64 numeric = First(1, wide); Base shared = First(new Left(), new Right()); return numeric == 1 && shared is Base ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);

        const string nullableReferenceSource = "virtual class Node { } class Child : Node { } class Program { public static T First<T>(T left, T right) { return left; } public static int32 Main() { Node? maybe = null; Child child = new Child(); Node? result = First(maybe, child); Node? reverse = First(child, maybe); Node? nullFirst = First(null, child); Node? nullLast = First(child, null); return result == null && reverse != null && nullFirst == null && nullLast != null ? 0 : 1; } }";
        var nullableReference = Compile(nullableReferenceSource);
        Assert.True(nullableReference.Succeeded, string.Join(Environment.NewLine, nullableReference.Diagnostics));
        var nullableReferenceExecution = LlvmJit.RunMain(nullableReference.LlvmIr!);
        Assert.Equal(0, nullableReferenceExecution.ExitCode);
        Assert.Equal(0, nullableReferenceExecution.OutstandingAllocations);

        const string nullableValueSource = "struct Point { public int32 X; } class Program { public static T First<T>(T left, T right) { return left; } public static int32 Main() { int32? maybe = null; int32? result = First(maybe, 42); int32? reverse = First(42, maybe); int32? nullFirst = First(null, 42); int32? nullLast = First(42, null); int64? present = 7; int64? promoted = First(present, 42L); Point? absentPoint = null; Point? inferredPoint = First(absentPoint, Point()); return result == null && reverse == 42 && nullFirst == null && nullLast == 42 && promoted == 7L && inferredPoint == null ? 0 : 1; } }";
        var nullableValue = Compile(nullableValueSource);
        Assert.True(nullableValue.Succeeded, string.Join(Environment.NewLine, nullableValue.Diagnostics));
        var nullableValueExecution = LlvmJit.RunMain(nullableValue.LlvmIr!);
        Assert.Equal(0, nullableValueExecution.ExitCode);
        Assert.Equal(0, nullableValueExecution.OutstandingAllocations);

        const string incompatibleNumericBoundsSource = "class Program { public static T Merge<T>(T left, T right) { return left; } public static int32 Main() { int64 value = Merge(1, (uint32)2); return value == 1 ? 0 : 1; } }";
        var incompatibleNumericBounds = Compile(incompatibleNumericBoundsSource);
        Assert.Contains(incompatibleNumericBounds.Diagnostics, diagnostic => diagnostic.Code == "LS1101" &&
            diagnostic.Message.Contains("could not be inferred", StringComparison.Ordinal));

        const string incompatibleNullableNumericBoundsSource = "class Program { public static T Merge<T>(T left, T right) { return left; } public static int32 Main() { int32? small = 1; int64? value = Merge(small, 2L); return value == 1L ? 0 : 1; } }";
        var incompatibleNullableNumericBounds = Compile(incompatibleNullableNumericBoundsSource);
        Assert.Contains(incompatibleNullableNumericBounds.Diagnostics, diagnostic => diagnostic.Code == "LS1101" &&
            diagnostic.Message.Contains("could not be inferred", StringComparison.Ordinal));

        const string laterNumericBoundSource = "delegate T Factory<T>(); class Program { public static T Merge<T>(T first, T second, T third) { return first; } public static T MergeWithFactory<T>(T first, T second, Factory<T> factory) { return factory(); } public static T MergeWithExact<T>(T first, T second, ref T exact) { return exact; } public static T MergeWithExactFirst<T>(ref T exact, T first, T second) { return exact; } public static double Three() { return 3.0; } public static int32 Main() { double value = Merge(1, (uint32)2, 3.0); double reverse = Merge(3.0, 1, (uint32)2); double fromLambda = MergeWithFactory(1, (uint32)2, () => 3.0); double fromMethodGroup = MergeWithFactory(1, (uint32)2, Three); double exact = 1.0; double exactValue = MergeWithExact(1, (uint32)2, ref exact); double exactFirstValue = MergeWithExactFirst(ref exact, 1, (uint32)2); return value == 1.0 && reverse == 3.0 && fromLambda == 3.0 && fromMethodGroup == 3.0 && exactValue == 1.0 && exactFirstValue == 1.0 ? 0 : 1; } }";
        var laterNumericBound = Compile(laterNumericBoundSource);
        Assert.True(laterNumericBound.Succeeded, string.Join(Environment.NewLine, laterNumericBound.Diagnostics));
        var laterNumericBoundExecution = LlvmJit.RunMain(laterNumericBound.LlvmIr!);
        Assert.Equal(0, laterNumericBoundExecution.ExitCode);
        Assert.Equal(0, laterNumericBoundExecution.OutstandingAllocations);
    }

    [Fact]
    public void GenericMethodInferenceSelectsTheMostSpecificSharedInterface()
    {
        const string source = "interface IReadable<T> { T Read(); } class Left : IReadable<int32> { public int32 Read() { return 21; } } class Right : IReadable<int32> { public int32 Read() { return 42; } } class Program { public static T First<T>(T left, T right) where T : IReadable<int32> { return left; } public static IReadable<int32> Choose(bool chooseLeft, Left left, Right right) { return chooseLeft ? left : right; } public static IReadable<int32>? ChooseNullable(bool chooseLeft, Left? left, Right? right) { return chooseLeft ? left : right; } public static int32 Main() { IReadable<int32> value = First(new Left(), new Right()); object boxedReference = value; IReadable<int32>? nullable = ChooseNullable(false, null, new Right()); return value.Read() == 21 && boxedReference.ToString() == \"Left\" && Choose(false, new Left(), new Right()).Read() == 42 && nullable?.Read() == 42 && ChooseNullable(false, null, null) == null ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        const string allLowerBoundsSource = "interface IShared { int32 Read(); } interface ILeft { } interface IRight { } class FirstValue : IShared, ILeft { public int32 Read() { return 1; } } class SecondValue : IShared, ILeft, IRight { public int32 Read() { return 2; } } class ThirdValue : IShared { public int32 Read() { return 42; } } class Program { public static T Select<T>(T first, T second, T third) where T : IShared { return third; } public static int32 Main() { return Select(new FirstValue(), new SecondValue(), new ThirdValue()).Read() == 42 ? 0 : 1; } }";
        var allLowerBounds = Compile(allLowerBoundsSource);
        Assert.True(allLowerBounds.Succeeded, string.Join(Environment.NewLine, allLowerBounds.Diagnostics));
        var allLowerBoundsExecution = LlvmJit.RunMain(allLowerBounds.LlvmIr!);
        Assert.Equal(0, allLowerBoundsExecution.ExitCode);
        Assert.Equal(0, allLowerBoundsExecution.OutstandingAllocations);
        Assert.Null(allLowerBoundsExecution.InteropError);

        const string combinedLowerBoundsSource = "delegate T Factory<T>(); interface IShared { int32 Read(); } interface ILeft { } interface IRight { } class FirstValue : IShared, ILeft { public int32 Read() { return 1; } } class SecondValue : IShared, ILeft, IRight { public int32 Read() { return 2; } } class ThirdValue : IShared { public int32 Read() { return 42; } } class Program { public static T Select<T>(T first, T second, Factory<T> factory) where T : IShared { return factory(); } public static T SelectWithExactFirst<T>(ref T exact, T first, T second) where T : IShared { return exact; } public static T SelectWithExactLast<T>(T first, T second, ref T exact) where T : IShared { return exact; } public static ThirdValue CreateThird() { return new ThirdValue(); } public static int32 Main() { IShared fromLambda = Select(new FirstValue(), new SecondValue(), () => new ThirdValue()); IShared fromMethodGroup = Select(new FirstValue(), new SecondValue(), CreateThird); IShared exact = new ThirdValue(); IShared fromExactFirst = SelectWithExactFirst(ref exact, new FirstValue(), new SecondValue()); IShared fromExactLast = SelectWithExactLast(new FirstValue(), new SecondValue(), ref exact); return fromLambda.Read() == 42 && fromMethodGroup.Read() == 42 && fromExactFirst.Read() == 42 && fromExactLast.Read() == 42 ? 0 : 1; } }";
        var combinedLowerBounds = Compile(combinedLowerBoundsSource);
        Assert.True(combinedLowerBounds.Succeeded, string.Join(Environment.NewLine, combinedLowerBounds.Diagnostics));
        var combinedLowerBoundsExecution = LlvmJit.RunMain(combinedLowerBounds.LlvmIr!);
        Assert.Equal(0, combinedLowerBoundsExecution.ExitCode);
        Assert.Equal(0, combinedLowerBoundsExecution.OutstandingAllocations);
        Assert.Null(combinedLowerBoundsExecution.InteropError);
    }

    [Fact]
    public void GenericMethodInferenceDoesNotInferOnlyFromNull()
    {
        const string source = "class Program { public static void Capture<T>(T value) { } public static int32 Main() { Capture(null); return 0; } }";
        var result = Compile(source);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS1101" && diagnostic.Message.Contains("could not be inferred"));

        const string returnAndConstraintSource = "class Program { public static T Create<T>() where T : struct { return default(T); } public static int32 Main() { int32 value = Create(); return value; } }";
        var returnAndConstraint = Compile(returnAndConstraintSource);
        Assert.Contains(returnAndConstraint.Diagnostics, diagnostic => diagnostic.Code == "LS1101" && diagnostic.Message.Contains("could not be inferred"));
    }

    [Fact]
    public void GenericMethodInferenceUsesCallArgumentsAndLambdaInputOrOutputTypes()
    {
        const string source = "delegate int32 Transformer<T>(int32 value); class Program { public static int32 Apply<T>(Transformer<T> transform, T value) { return transform(1); } public static int32 Main() { return Apply(value => value + 41, 0) == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);

        const string genericPayloadSource = "delegate T Transformer<T>(T value); class Program { public static T Apply<T>(Transformer<T> transform, T value) { return transform(value); } public static int32 Main() { return Apply(value => value + 1, 41) == 42 ? 0 : 1; } }";
        var genericPayload = Compile(genericPayloadSource);
        Assert.True(genericPayload.Succeeded, string.Join(Environment.NewLine, genericPayload.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(genericPayload.LlvmIr!).ExitCode);

        const string explicitLambdaParameterSource = "delegate T Transformer<T>(T value); class Program { public static T Apply<T>(Transformer<T> transform) where T : struct { return transform(default(T)); } public static int32 Main() { return Apply((int32 value) => value + 1) == 1 ? 0 : 1; } }";
        var explicitLambdaParameter = Compile(explicitLambdaParameterSource);
        Assert.True(explicitLambdaParameter.Succeeded, string.Join(Environment.NewLine, explicitLambdaParameter.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(explicitLambdaParameter.LlvmIr!).ExitCode);

        const string lambdaOutputInferenceSource = "delegate T Factory<T>(); class Program { public static T Produce<T>(Factory<T> factory) { return factory(); } public static int32 Main() { return Produce(() => 42) == 42 ? 0 : 1; } }";
        var lambdaOutputInference = Compile(lambdaOutputInferenceSource);
        Assert.True(lambdaOutputInference.Succeeded, string.Join(Environment.NewLine, lambdaOutputInference.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(lambdaOutputInference.LlvmIr!).ExitCode);

        const string lambdaBlockOutputInferenceSource = "delegate T Factory<T>(); class Program { public static T Produce<T>(Factory<T> factory) { return factory(); } public static int32 Main() { int64 value = Produce(() => { if (true) return 42; else return 43L; }); return value == 42 ? 0 : 1; } }";
        var lambdaBlockOutputInference = Compile(lambdaBlockOutputInferenceSource);
        Assert.True(lambdaBlockOutputInference.Succeeded, string.Join(Environment.NewLine, lambdaBlockOutputInference.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(lambdaBlockOutputInference.LlvmIr!).ExitCode);

        const string mixedLambdaBlockOutputInferenceSource = "delegate T Factory<T>(); class Program { public static T Produce<T>(Factory<T> factory) { return factory(); } public static int32 Main() { bool choose = true; int32 value = Produce(() => { if (choose) return 42; else return default; }); return value == 42 ? 0 : 1; } }";
        var mixedLambdaBlockOutputInference = Compile(mixedLambdaBlockOutputInferenceSource);
        Assert.True(mixedLambdaBlockOutputInference.Succeeded, string.Join(Environment.NewLine, mixedLambdaBlockOutputInference.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(mixedLambdaBlockOutputInference.LlvmIr!).ExitCode);

        const string computedLambdaOutputInferenceSource = "delegate T Factory<T>(); class Program { public static T Produce<T>(Factory<T> factory) { return factory(); } public static int32 Double(int32 value) { return value * 2; } public static int32 Main() { return Produce(() => true ? Double(21) : 0) == 42 ? 0 : 1; } }";
        var computedLambdaOutputInference = Compile(computedLambdaOutputInferenceSource);
        Assert.True(computedLambdaOutputInference.Succeeded, string.Join(Environment.NewLine, computedLambdaOutputInference.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(computedLambdaOutputInference.LlvmIr!).ExitCode);

        const string capturedLambdaOutputInferenceSource = "delegate T Factory<T>(); class Program { public static T Produce<T>(Factory<T> factory) { return factory(); } public static int32 Main() { int32 seed = 40; return Produce(() => seed + 2) == 42 ? 0 : 1; } }";
        var capturedLambdaOutputInference = Compile(capturedLambdaOutputInferenceSource);
        Assert.True(capturedLambdaOutputInference.Succeeded, string.Join(Environment.NewLine, capturedLambdaOutputInference.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(capturedLambdaOutputInference.LlvmIr!).ExitCode);

        const string nestedLambdaOutputInferenceSource = "delegate T Factory<T>(); delegate Factory<T> FactoryOfFactory<T>(); class Program { public static T Produce<T>(FactoryOfFactory<T> factory) { return factory()(); } public static int32 Main() { return Produce(() => () => 42) == 42 ? 0 : 1; } }";
        var nestedLambdaOutputInference = Compile(nestedLambdaOutputInferenceSource);
        Assert.True(nestedLambdaOutputInference.Succeeded, string.Join(Environment.NewLine, nestedLambdaOutputInference.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(nestedLambdaOutputInference.LlvmIr!).ExitCode);

        const string methodGroupInferenceSource = "delegate int32 Mapper<T>(T value); delegate T Factory<T>(); class Calculator { public int32 Increment(int32 value) { return value + 1; } public T Identity<T>(T value) { return value; } } class Program { public static int32 Increment(int32 value) { return value + 1; } public static int32 GetAnswer() { return 42; } public static int32 Choose(int32 value) { return 1; } public static int32 Choose(int64 value) { return 2; } public static Calculator CreateCalculator() { return new Calculator(); } public static T Identity<T>(T value) { return value; } public static int32 Echo(int32 value) { return 7; } public static T Echo<T>(T value) { return value; } public static T ConstrainedIdentity<T>(T value) where T : struct { return value; } public static T GetGeneric<T>() where T : struct { return default(T); } public static int32 Invoke<T>(Mapper<T> mapper, T value) { return mapper(value); } public static int32 InvokeOverload<T>(Mapper<T> mapper, T value) { return mapper(value); } public static int32 InvokeGeneric<T>(Mapper<T> mapper, T value) { return mapper(value); } public static T Produce<T>(Factory<T> factory) { return factory(); } public static int32 Main() { Calculator calculator = new Calculator(); return Invoke(Increment, 41) == 42 && Invoke(calculator.Increment, 41) == 42 && Invoke(CreateCalculator().Increment, 41) == 42 && InvokeOverload(Choose, 41L) == 2 && InvokeGeneric(Identity, 42) == 42 && InvokeGeneric(calculator.Identity, 42) == 42 && InvokeGeneric(ConstrainedIdentity, 42) == 42 && InvokeGeneric(Echo, 42) == 7 && Produce(GetAnswer) == 42 && Produce<int32>(GetGeneric) == 0 ? 0 : 1; } }";
        var methodGroupInference = Compile(methodGroupInferenceSource);
        Assert.True(methodGroupInference.Succeeded, string.Join(Environment.NewLine, methodGroupInference.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(methodGroupInference.LlvmIr!).ExitCode);

        const string lambdaInferenceFlowIsolationSource = "delegate T Factory<T>(); class Program { public static T Produce<T>(Factory<T> factory) { return factory(); } public static int32 Main() { string? maybe = null; int32 length = Produce(() => maybe!.Length); return maybe.Length; } }";
        var lambdaInferenceFlowIsolation = Compile(lambdaInferenceFlowIsolationSource);
        Assert.Contains(lambdaInferenceFlowIsolation.Diagnostics, diagnostic => diagnostic.Code == "LS1303" && diagnostic.Message.Contains("nullable string"));

        const string targetDependentLambdaInferenceSource = "delegate T Factory<T>(); class Program { public static T Produce<T>(Factory<T> factory, T fallback) { return factory(); } public static int32 Main() { return Produce(() => default, 42) == 0 ? 0 : 1; } }";
        var targetDependentLambdaInference = Compile(targetDependentLambdaInferenceSource);
        Assert.True(targetDependentLambdaInference.Succeeded, string.Join(Environment.NewLine, targetDependentLambdaInference.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(targetDependentLambdaInference.LlvmIr!).ExitCode);
    }

    [Fact]
    public void GenericMethodGroupConstraintsAreCheckedAgainstTheDelegateSignature()
    {
        const string source = "delegate int32 Mapper<T>(T value); class Program { public static T ClassOnly<T>(T value) where T : class { return value; } public static int32 Invoke<T>(Mapper<T> mapper, T value) { return mapper(value); } public static int32 Main() { return Invoke(ClassOnly, 42); } }";
        var result = Compile(source);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS1102" && diagnostic.Message.Contains("does not satisfy constraint 'class'"));
    }

    [Fact]
    public void ExplicitLambdaParameterTypesContributeExactGenericInferenceBounds()
    {
        const string source = "delegate int32 Mapper<T>(T value); class Program { public static int32 Apply<T>(Mapper<T> mapper, T value) { return mapper(value); } public static int32 Main() { int64 value = 1L; return Apply((int32 input) => input, value); } }";
        var result = Compile(source);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS1101" && diagnostic.Message.Contains("could not be inferred"));
    }

    [Fact]
    public void ExplicitLambdaParameterInferenceUnifiesNestedGenericTypesExactly()
    {
        const string source = "class Box<T> { } delegate int32 Mapper<T>(Box<T> value); class Program { public static int32 Apply<T>(Mapper<T> mapper, Box<T> value) { return mapper(value); } public static int32 Main() { Box<int64> value = new Box<int64>(); return Apply((Box<int32> input) => 1, value); } }";
        var result = Compile(source);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS1101" && diagnostic.Message.Contains("could not be inferred"));
    }

    [Fact]
    public void GenericConstraintOrderingIsValidated()
    {
        var result = Compile("class Invalid<T> where T : new(), class { } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS0001" && diagnostic.Message.Contains("primary constraint"));
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS0001" && diagnostic.Message.Contains("must be last"));
    }

    [Fact]
    public void NestedGenericTypesKeepNullableSuffixesOnTheOuterType()
    {
        const string source = "class Box<T> { T? value; } class Program { public static int32 Main() { Box<Box<int32>>? value = null; return value == null ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(result.LlvmIr!).ExitCode);
    }

    [Fact]
    public void ExpansiveRecursiveGenericTypesFailWithFeatureDiagnostic()
    {
        const string source = "class Growing<T> { Growing<Growing<T>>? next; } class Program { public static int32 Main() { Growing<int32>? value = null; return value == null ? 0 : 1; } }";
        var result = Compile(source);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LMC3000" && diagnostic.Message.Contains("cannot be monomorphized"));
    }

    [Fact]
    public void NongenericOverloadWinsEquivalentGenericOverload()
    {
        const string source = "class Program { public static int32 Select<T>(T value) { return 1; } public static int32 Select(int32 value) { return 2; } public static int32 Main() { return Select(42) == 2 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(result.LlvmIr!).ExitCode);
    }

    [Fact]
    public void GenericMethodConstraintsAreCheckedAtCalls()
    {
        const string source = "class Program { public static int32 Measure<T>(T value) where T : struct { return 1; } public static int32 Main() { return Measure<string>(\"x\"); } }";
        var result = Compile(source);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS1102" && diagnostic.Message.Contains("does not satisfy constraint 'struct'"));
    }

    [Fact]
    public void GenericConstraintsCanChainThroughAnotherTypeParameter()
    {
        const string validSource = "interface IReader { int32 Read(); } class Data : IReader { public int32 Read() { return 42; } } class Use<T, U> where T : IReader where U : T { public int32 Read(U value) { return value.Read(); } } class Program { public static int32 Main() { Use<IReader, Data> use = new Use<IReader, Data>(); return use.Read(new Data()) == 42 ? 0 : 1; } }";
        var valid = Compile(validSource);
        Assert.True(valid.Succeeded, string.Join(Environment.NewLine, valid.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(valid.LlvmIr!).ExitCode);

        const string invalidSource = "interface IReader { int32 Read(); } interface IOther { } class NoReader : IOther { } class Use<T, U> where T : IReader where U : T { } class Program { public static int32 Main() { Use<IReader, NoReader> use = new Use<IReader, NoReader>(); return 0; } }";
        var invalid = Compile(invalidSource);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == "LS1102" && diagnostic.Message.Contains("does not satisfy constraint 'T'"));
    }

    [Fact]
    public void GenericMethodBodiesCannotUseMembersOutsideTheirConstraints()
    {
        const string source = "interface IReader { int32 Read(); } class Data : IReader { public int32 Read() { return 42; } public int32 Secret() { return 0; } } class Program { public static int32 Read<T>(T value) where T : IReader { return value.Read(); } public static int32 Invalid<T>(T value) where T : IReader { return value.Secret(); } public static int32 Main() { return Read(new Data()) == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS0200" && diagnostic.Message.Contains("Type parameter 'T' has no method 'Secret'"));
    }

    [Fact]
    public void GenericConstraintCallsMatchOverloadsAndSubstitutedParameterTypes()
    {
        const string validSource = "interface ISelector { int32 Select(int32 value); int32 Select(string value); int32 Select(double value); int32 Select(string? value); } class Use<T> where T : ISelector { public int32 FromText(T receiver) { var text = \"ok\"; return receiver.Select(text); } public int32 FromFloat(T receiver) { return receiver.Select(1.5f); } public int32 FromNull(T receiver) { return receiver.Select(null); } } class Program { public static int32 Main() { return 0; } }";
        var valid = Compile(validSource);
        Assert.True(valid.Succeeded, string.Join(Environment.NewLine, valid.Diagnostics));

        const string invalidSource = "interface ISelector { int32 Select(int32 value); int32 Select(string value); } class Use<T> where T : ISelector { public int32 Invalid(T receiver) { var flag = true; return receiver.Select(flag); } } class Program { public static int32 Main() { return 0; } }";
        var invalid = Compile(invalidSource);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == "LS0200" && diagnostic.Message.Contains("Type parameter 'T' has no method 'Select'"));

        const string narrowingSource = "interface ISelector { int32 Select(int32 value); } class Use<T> where T : ISelector { public int32 Invalid(T receiver) { return receiver.Select(1L); } } class Program { public static int32 Main() { return 0; } }";
        var narrowing = Compile(narrowingSource);
        Assert.Contains(narrowing.Diagnostics, diagnostic => diagnostic.Code == "LS0200" && diagnostic.Message.Contains("Type parameter 'T' has no method 'Select'"));

        const string substitutedSource = "interface IConverter<TValue> { int32 Convert(TValue value); } class Use<T> where T : IConverter<int32> { public int32 Valid(T receiver) { return receiver.Convert(1); } public int32 Invalid(T receiver) { return receiver.Convert(\"x\"); } } class Program { public static int32 Main() { return 0; } }";
        var substituted = Compile(substitutedSource);
        Assert.Contains(substituted.Diagnostics, diagnostic => diagnostic.Code == "LS0200" && diagnostic.Message.Contains("Type parameter 'T' has no method 'Convert'"));
        Assert.DoesNotContain(substituted.Diagnostics, diagnostic => diagnostic.Code == "LMC3000");
    }

    [Fact]
    public void GenericConstraintOverloadBindingUsesCompoundExpressionTypes()
    {
        const string validSource = "interface ISelector { int32 Select(int32 value); string Select(string value); } class Use<T> where T : ISelector { public int32 SelectConditional(T receiver, bool flag) { return receiver.Select(flag ? 1 : 2); } public int32 SelectArithmetic(T receiver, int32 value) { return receiver.Select(value + 1); } } class Program { public static int32 Main() { return 0; } }";
        var valid = Compile(validSource);
        Assert.True(valid.Succeeded, string.Join(Environment.NewLine, valid.Diagnostics));

        const string invalidSource = "interface ISelector { int32 Select(string value); } class Use<T> where T : ISelector { public int32 Invalid(T receiver, bool flag) { return receiver.Select(flag ? 1 : 2); } } class Program { public static int32 Main() { return 0; } }";
        var invalid = Compile(invalidSource);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == "LS0200" &&
            diagnostic.Message.Contains("Type parameter 'T' has no method 'Select'", StringComparison.Ordinal));
    }

    [Fact]
    public void GenericConstraintOverloadBindingInfersUnqualifiedCallResults()
    {
        const string validSource = "interface ISelector { int32 Select(int32 value); } class Use<T> where T : ISelector { int32 GetNumber() { return 1; } public int32 Select(T receiver) { return receiver.Select(GetNumber()); } } class Program { public static int32 Main() { return 0; } }";
        var valid = Compile(validSource);
        Assert.True(valid.Succeeded, string.Join(Environment.NewLine, valid.Diagnostics));

        const string invalidSource = "interface ISelector { int32 Select(int32 value); } class Use<T> where T : ISelector { string GetText() { return \"x\"; } public int32 Select(T receiver) { return receiver.Select(GetText()); } } class Program { public static int32 Main() { return 0; } }";
        var invalid = Compile(invalidSource);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == "LS0200" &&
            diagnostic.Message.Contains("Type parameter 'T' has no method 'Select'", StringComparison.Ordinal));
    }

    [Fact]
    public void GenericConstraintOverloadBindingInfersExplicitGenericCallResults()
    {
        const string validSource = "interface ISelector { int32 Select(int32 value); } class Use<T> where T : ISelector { U Echo<U>(U value) { return value; } public int32 Select(T receiver) { return receiver.Select(Echo<int32>(1)); } public int32 SelectInferred(T receiver) { return receiver.Select(Echo(1)); } } class Program { public static int32 Main() { return 0; } }";
        var valid = Compile(validSource);
        Assert.True(valid.Succeeded, string.Join(Environment.NewLine, valid.Diagnostics));

        const string invalidSource = "interface ISelector { int32 Select(int32 value); } class Use<T> where T : ISelector { U Echo<U>(U value) { return value; } public int32 Select(T receiver) { return receiver.Select(Echo<string>(\"x\")); } public int32 SelectInferred(T receiver) { return receiver.Select(Echo(\"x\")); } } class Program { public static int32 Main() { return 0; } }";
        var invalid = Compile(invalidSource);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == "LS0200" &&
            diagnostic.Message.Contains("Type parameter 'T' has no method 'Select'", StringComparison.Ordinal));
    }

    [Fact]
    public void GenericConstraintOverloadBindingInfersNestedGenericHelperResults()
    {
        const string source = "class Box<T> { } interface ISelector { int32 Select(Box<int32> value); } class Use<T> where T : ISelector { Box<U> Echo<U>(Box<U> value) { return value; } public int32 Valid(T receiver) { return receiver.Select(Echo(new Box<int32>())); } public int32 Invalid(T receiver) { return receiver.Select(Echo(new Box<string>())); } } class Program { public static int32 Main() { return 0; } }";
        var result = Compile(source);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS0200" &&
            diagnostic.Message.Contains("Type parameter 'T' has no method 'Select'", StringComparison.Ordinal));
    }

    [Fact]
    public void GenericConstraintOverloadBindingInfersArrayHelperResults()
    {
        const string source = "interface ISelector { int32 Select(int32[] value); } class Use<T> where T : ISelector { U[] Echo<U>(U[] value) { return value; } public int32 Valid(T receiver) { return receiver.Select(Echo(new int32[1])); } public int32 Invalid(T receiver) { return receiver.Select(Echo(new string[1])); } } class Program { public static int32 Main() { return 0; } }";
        var result = Compile(source);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS0200" &&
            diagnostic.Message.Contains("Type parameter 'T' has no method 'Select'", StringComparison.Ordinal));
    }

    [Fact]
    public void GenericConstraintOverloadBindingCombinesHelperMethodLowerBounds()
    {
        const string validSource = "interface ISelector { int64 Select(int64 value); } class Use<T> where T : ISelector { U Combine<U>(U left, U right) { return left; } public int64 Select(T receiver) { return receiver.Select(Combine(1, 1L)); } } class Program { public static int32 Main() { return 0; } }";
        var valid = Compile(validSource);
        Assert.True(valid.Succeeded, string.Join(Environment.NewLine, valid.Diagnostics));

        const string invalidSource = "interface ISelector { int32 Select(int32 value); } class Use<T> where T : ISelector { U Combine<U>(U left, U right) { return left; } public int32 Select(T receiver) { return receiver.Select(Combine(1, 1L)); } } class Program { public static int32 Main() { return 0; } }";
        var invalid = Compile(invalidSource);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == "LS0200" &&
            diagnostic.Message.Contains("Type parameter 'T' has no method 'Select'", StringComparison.Ordinal));
    }

    [Fact]
    public void GenericConstraintOverloadBindingChecksLambdaDelegateSignatures()
    {
        const string validSource = "delegate int32 Mapper(int32 value); interface ISelector { int32 Select(Mapper mapper); } class Use<T> where T : ISelector { public int32 Valid(T receiver) { return receiver.Select(value => value + 1); } } class Program { public static int32 Main() { return 0; } }";
        var valid = Compile(validSource);
        Assert.True(valid.Succeeded, string.Join(Environment.NewLine, valid.Diagnostics));

        const string covariantReturnSource = "delegate object ObjectMapper(int32 value); interface ISelector { int32 Select(ObjectMapper mapper); } class Use<T> where T : ISelector { string GetText(int32 value) { return \"ok\"; } public int32 Valid(T receiver) { return receiver.Select(value => GetText(value)); } } class Program { public static int32 Main() { return 0; } }";
        var covariantReturn = Compile(covariantReturnSource);
        Assert.True(covariantReturn.Succeeded, string.Join(Environment.NewLine, covariantReturn.Diagnostics));

        const string invalidSource = "delegate int32 Mapper(int32 value); interface ISelector { int32 Select(Mapper mapper); } class Use<T> where T : ISelector { public int32 InvalidParameter(T receiver) { return receiver.Select((string value) => 1); } public int32 InvalidReturn(T receiver) { return receiver.Select(value => \"bad\"); } } class Program { public static int32 Main() { return 0; } }";
        var invalid = Compile(invalidSource);
        Assert.Equal(2, invalid.Diagnostics.Count(diagnostic => diagnostic.Code == "LS0200" &&
            diagnostic.Message.Contains("Type parameter 'T' has no method 'Select'", StringComparison.Ordinal)));
    }

    [Fact]
    public void GenericConstraintOverloadBindingChecksMethodGroupDelegateSignatures()
    {
        const string source = "delegate int32 Mapper(int32 value); interface ISelector { int32 Select(Mapper mapper); } class Use<T> where T : ISelector { int32 Map(int32 value) { return value; } string ToText(string value) { return value; } public int32 Valid(T receiver) { return receiver.Select(Map); } public int32 Invalid(T receiver) { return receiver.Select(ToText); } } class Program { public static int32 Main() { return 0; } }";
        var result = Compile(source);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS0200" &&
            diagnostic.Message.Contains("Type parameter 'T' has no method 'Select'", StringComparison.Ordinal));
    }

    [Fact]
    public void GenericConstraintOverloadBindingInfersStaticHelperCallResults()
    {
        const string source = "class Utility { public static int32 GetNumber() { return 1; } public static string GetText() { return \"x\"; } } interface ISelector { int32 Select(int32 value); } class Use<T> where T : ISelector { public int32 Valid(T receiver) { return receiver.Select(Utility.GetNumber()); } public int32 Invalid(T receiver) { return receiver.Select(Utility.GetText()); } } class Program { public static int32 Main() { return 0; } }";
        var result = Compile(source);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS0200" &&
            diagnostic.Message.Contains("Type parameter 'T' has no method 'Select'", StringComparison.Ordinal));
    }

    [Fact]
    public void GenericConstraintNumericBindingUsesTheSelectedTargetPointerWidth()
    {
        const string source = "interface ISelector { int32 Select(int32 value); } class Use<T> where T : ISelector { public int32 Select(T receiver, int value) { return receiver.Select(value + 1); } } class Program { public static int32 Main() { return 0; } }";

        var thirtyTwoBit = Compile(source, new CompilationOptions(TargetTriple: "i386-pc-linux-gnu"));
        Assert.True(thirtyTwoBit.Succeeded, string.Join(Environment.NewLine, thirtyTwoBit.Diagnostics));

        var sixtyFourBit = Compile(source, new CompilationOptions(TargetTriple: "x86_64-pc-linux-gnu"));
        Assert.Contains(sixtyFourBit.Diagnostics, diagnostic => diagnostic.Code == "LS0200" &&
            diagnostic.Message.Contains("Type parameter 'T' has no method 'Select'", StringComparison.Ordinal));
    }

    [Fact]
    public void GenericConstraintPropertiesRespectAccessorAvailability()
    {
        const string validSource = "interface IReadValue { int32 Value { get; } } class Use<T> where T : IReadValue { public int32 Read(T receiver) { return receiver.Value; } } class Program { public static int32 Main() { return 0; } }";
        var valid = Compile(validSource);
        Assert.True(valid.Succeeded, string.Join(Environment.NewLine, valid.Diagnostics));

        const string writeOnlySource = "interface IWriteValue { int32 Value { set; } } class Use<T> where T : IWriteValue { public int32 Read(T receiver) { return receiver.Value; } } class Program { public static int32 Main() { return 0; } }";
        var writeOnly = Compile(writeOnlySource);
        Assert.Contains(writeOnly.Diagnostics, diagnostic => diagnostic.Code == "LS0200" && diagnostic.Message.Contains("no member 'Value'"));

        const string getOnlyWriteSource = "interface IReadValue { int32 Value { get; } } class Use<T> where T : IReadValue { public void Write(T receiver) { receiver.Value = 3; } } class Program { public static int32 Main() { return 0; } }";
        var getOnlyWrite = Compile(getOnlyWriteSource);
        Assert.Contains(getOnlyWrite.Diagnostics, diagnostic => diagnostic.Code == "LS0200" && diagnostic.Message.Contains("no member 'Value'"));

        const string incompatibleWriteSource = "interface IWriteValue { int32 Value { set; } } class Use<T> where T : IWriteValue { public void Write(T receiver) { receiver.Value = \"bad\"; } } class Program { public static int32 Main() { return 0; } }";
        var incompatibleWrite = Compile(incompatibleWriteSource);
        Assert.Contains(incompatibleWrite.Diagnostics, diagnostic => diagnostic.Code == "LS0200" && diagnostic.Message.Contains("no member 'Value'"));
    }

    [Fact]
    public void GenericConstraintIndexersRespectSignaturesAndAccessorAvailability()
    {
        const string validSource = "interface IReadIndex { int32 this[int32 index] { get; } } class Use<T> where T : IReadIndex { public int32 Read(T receiver) { return receiver[1]; } } class Program { public static int32 Main() { return 0; } }";
        var valid = Compile(validSource);
        Assert.True(valid.Succeeded, string.Join(Environment.NewLine, valid.Diagnostics));

        const string writeOnlySource = "interface IWriteIndex { int32 this[int32 index] { set; } } class Use<T> where T : IWriteIndex { public int32 Read(T receiver) { return receiver[1]; } } class Program { public static int32 Main() { return 0; } }";
        var writeOnly = Compile(writeOnlySource);
        Assert.Contains(writeOnly.Diagnostics, diagnostic => diagnostic.Code == "LS0200" && diagnostic.Message.Contains("no indexer"));

        const string getOnlyWriteSource = "interface IReadIndex { int32 this[int32 index] { get; } } class Use<T> where T : IReadIndex { public void Write(T receiver) { receiver[1] = 3; } } class Program { public static int32 Main() { return 0; } }";
        var getOnlyWrite = Compile(getOnlyWriteSource);
        Assert.Contains(getOnlyWrite.Diagnostics, diagnostic => diagnostic.Code == "LS0200" && diagnostic.Message.Contains("no indexer"));

        const string invalidSignatureSource = "interface IReadIndex { int32 this[string index] { get; } } class Use<T> where T : IReadIndex { public int32 Read(T receiver) { return receiver[1]; } } class Program { public static int32 Main() { return 0; } }";
        var invalidSignature = Compile(invalidSignatureSource);
        Assert.Contains(invalidSignature.Diagnostics, diagnostic => diagnostic.Code == "LS0200" && diagnostic.Message.Contains("no indexer"));

        const string incompatibleWriteSource = "interface IWriteIndex { int32 this[int32 index] { set; } } class Use<T> where T : IWriteIndex { public void Write(T receiver) { receiver[1] = \"bad\"; } } class Program { public static int32 Main() { return 0; } }";
        var incompatibleWrite = Compile(incompatibleWriteSource);
        Assert.Contains(incompatibleWrite.Diagnostics, diagnostic => diagnostic.Code == "LS0200" && diagnostic.Message.Contains("no indexer"));
    }

    [Fact]
    public void GenericConstraintOperatorsAllowDefaultEqualityButRequireOtherOperators()
    {
        const string classEqualitySource = "class Equality<T> where T : class { public bool Same(T left, T right) { return left == right; } public bool SameObject(T left, object right) { return left == right; } } class Program { public static int32 Main() { return 0; } }";
        var classEquality = Compile(classEqualitySource);
        Assert.True(classEquality.Succeeded, string.Join(Environment.NewLine, classEquality.Diagnostics));

        const string missingOperatorSource = "class Equality<T> { public bool Same(T left, T right) { return left == right; } public void Add(T left, T right) { left += right; } } class Program { public static int32 Main() { return 0; } }";
        var missingOperator = Compile(missingOperatorSource);
        Assert.Contains(missingOperator.Diagnostics, diagnostic => diagnostic.Code == "LS0751" && diagnostic.Message.Contains("Operator '+=' is not available"));

        const string declaredOperatorSource = "virtual class Base { public static Base operator +(Base left, Base right) { return left; } } class Sum<T> where T : Base { public Base Add(T left, T right) { return left + right; } } class Program { public static int32 Main() { return 0; } }";
        var declaredOperator = Compile(declaredOperatorSource);
        Assert.True(declaredOperator.Succeeded, string.Join(Environment.NewLine, declaredOperator.Diagnostics));

        const string derivedInequalitySource = "virtual class NumberBase { public int32 Value; public this(int32 value) { Value = value; } public static bool operator ==(NumberBase left, NumberBase right) { return left.Value == right.Value; } } class Compare<T> where T : NumberBase { public bool Different(T left, T right) { return left != right; } } class Program { public static int32 Main() { Compare<NumberBase> compare = new Compare<NumberBase>(); return compare.Different(new NumberBase(7), new NumberBase(7)) ? 1 : 0; } }";
        var derivedInequality = Compile(derivedInequalitySource);
        Assert.True(derivedInequality.Succeeded, string.Join(Environment.NewLine, derivedInequality.Diagnostics));
        var derivedInequalityExecution = LlvmJit.RunMain(derivedInequality.LlvmIr!);
        Assert.Equal(0, derivedInequalityExecution.ExitCode);
        Assert.Equal(0, derivedInequalityExecution.OutstandingAllocations);
        Assert.Null(derivedInequalityExecution.InteropError);
    }

    [Fact]
    public void GenericConstraintValidationFollowsNestedGenericMemberTypes()
    {
        const string validSource = "interface IReader { int32 Read(); } interface IHas<TValue> { TValue Value { get; } } class Use<T, U> where T : IHas<U> where U : IReader { public int32 Read(T value) { return value.Value.Read(); } } class Program { public static int32 Main() { return 0; } }";
        var valid = Compile(validSource);
        Assert.True(valid.Succeeded, string.Join(Environment.NewLine, valid.Diagnostics));

        const string invalidSource = "interface IHas<TValue> { TValue Value { get; } } class Use<T, U> where T : IHas<U> { public int32 Read(T value) { return value.Value.Read(); } } class Program { public static int32 Main() { return 0; } }";
        var invalid = Compile(invalidSource);
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == "LS0200" && diagnostic.Message.Contains("Type parameter 'U' has no method 'Read'"));

        const string methodResultSource = "interface IReader { int32 Read(); } interface IFactory<TValue> { TValue Create(); } class Use<T, U> where T : IFactory<U> where U : IReader { public int32 Read(T factory) { return factory.Create().Read(); } } class Program { public static int32 Main() { return 0; } }";
        var methodResult = Compile(methodResultSource);
        Assert.True(methodResult.Succeeded, string.Join(Environment.NewLine, methodResult.Diagnostics));

        const string unconstrainedResultSource = "interface IFactory<TValue> { TValue Create(); } class Use<T, U> where T : IFactory<U> { public int32 Read(T factory) { return factory.Create().Read(); } } class Program { public static int32 Main() { return 0; } }";
        var unconstrainedResult = Compile(unconstrainedResultSource);
        Assert.Contains(unconstrainedResult.Diagnostics, diagnostic => diagnostic.Code == "LS0200" && diagnostic.Message.Contains("Type parameter 'U' has no method 'Read'"));

        const string indexerResultSource = "interface IReader { int32 Read(); } interface IIndexed<TValue> { TValue this[int32 index] { get; } } class Use<T, U> where T : IIndexed<U> where U : IReader { public int32 Read(T value) { return value[0].Read(); } } class Program { public static int32 Main() { return 0; } }";
        var indexerResult = Compile(indexerResultSource);
        Assert.True(indexerResult.Succeeded, string.Join(Environment.NewLine, indexerResult.Diagnostics));

        const string unconstrainedIndexerResultSource = "interface IIndexed<TValue> { TValue this[int32 index] { get; } } class Use<T, U> where T : IIndexed<U> { public int32 Read(T value) { return value[0].Read(); } } class Program { public static int32 Main() { return 0; } }";
        var unconstrainedIndexerResult = Compile(unconstrainedIndexerResultSource);
        Assert.Contains(unconstrainedIndexerResult.Diagnostics, diagnostic => diagnostic.Code == "LS0200" && diagnostic.Message.Contains("Type parameter 'U' has no method 'Read'"));
    }

    [Fact]
    public void GenericConstructionHonorsStructAndConstructorConstraints()
    {
        const string defaultValueSource = "class Factory<T> where T : struct { public T Create() { return new T(); } } class Program { public static int32 Main() { Factory<int32> factory = new Factory<int32>(); return factory.Create(); } }";
        var defaultValue = Compile(defaultValueSource);
        Assert.True(defaultValue.Succeeded, string.Join(Environment.NewLine, defaultValue.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(defaultValue.LlvmIr!).ExitCode);

        const string constructorSource = "struct Number { int32 value; public this(int32 value) { this.value = value; } public int32 Read() { return value; } } class Factory<T> where T : new(int32) { public T Create(int32 value) { return new T(value); } } class Program { public static int32 Main() { Factory<Number> factory = new Factory<Number>(); return factory.Create(42).Read() == 42 ? 0 : 1; } }";
        var constructed = Compile(constructorSource);
        Assert.True(constructed.Succeeded, string.Join(Environment.NewLine, constructed.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(constructed.LlvmIr!).ExitCode);
    }

    [Fact]
    public void GenericConstructorConstraintsRejectAbstractClassArguments()
    {
        const string parameterlessSource = "virtual class Shape { public this() {} public virtual int32 Area(); } class Factory<T> where T : new() { public T Create() { return new T(); } } class Program { public static int32 Main() { Factory<Shape> factory = new Factory<Shape>(); return 0; } }";
        var parameterless = Compile(parameterlessSource);
        Assert.Contains(parameterless.Diagnostics, diagnostic => diagnostic.Code == "LS1102" &&
            diagnostic.Message.Contains("does not satisfy constraint 'new()'", StringComparison.Ordinal));

        const string parameterizedSource = "virtual class Shape { public this(int32 size) {} public virtual int32 Area(); } class Factory<T> where T : new(int32) { public T Create(int32 size) { return new T(size); } } class Program { public static int32 Main() { Factory<Shape> factory = new Factory<Shape>(); return 0; } }";
        var parameterized = Compile(parameterizedSource);
        Assert.Contains(parameterized.Diagnostics, diagnostic => diagnostic.Code == "LS1102" &&
            diagnostic.Message.Contains("does not satisfy constraint 'new(int32)'", StringComparison.Ordinal));

        const string concreteDerivedSource = "virtual class Shape { public this() {} public virtual int32 Area(); } class Circle : Shape { public override int32 Area() { return 42; } } class Factory<T> where T : new() { public T Create() { return new T(); } } class Program { public static int32 Main() { Factory<Circle> factory = new Factory<Circle>(); return factory.Create().Area() == 42 ? 0 : 1; } }";
        var concreteDerived = Compile(concreteDerivedSource);
        Assert.True(concreteDerived.Succeeded, string.Join(Environment.NewLine, concreteDerived.Diagnostics));
        var execution = LlvmJit.RunMain(concreteDerived.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void GenericConstructionArgumentsMustMatchTheDeclaredConstructorConstraint()
    {
        const string source = "class Factory<T> where T : new(string) { public T Create(int32 value) { return new T(value); } } class Program { public static int32 Main() { return 0; } }";
        var result = Compile(source);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS0751" && diagnostic.Message.Contains("do not match any 'new(types)' constraint"));

        const string narrowingSource = "class Factory<T> where T : new(int32) { public T Create(int64 value) { return new T(value); } } class Program { public static int32 Main() { return 0; } }";
        var narrowing = Compile(narrowingSource);
        Assert.Contains(narrowing.Diagnostics, diagnostic => diagnostic.Code == "LS0751" && diagnostic.Message.Contains("do not match any 'new(types)' constraint"));

        const string wideningSource = "class Factory<T> where T : new(int64) { public T Create(int32 value) { return new T(value); } } class Program { public static int32 Main() { return 0; } }";
        var widening = Compile(wideningSource);
        Assert.DoesNotContain(widening.Diagnostics, diagnostic => diagnostic.Code == "LS0751");
    }

    [Fact]
    public void InheritanceSupportsBaseConversionsInheritedMembersAndConstructorChains()
    {
        const string source = "virtual class Base { public int32 value = 4; public this() {} public int32 Read() { return value; } public virtual int32 Dispatch() { return 10; } } class Derived : Base { public int32 extra = 3; public override int32 Dispatch() { return 22; } } class Program { public static int32 Main() { Derived derived = new Derived(); Base baseValue = derived; if (baseValue.Read() != 4 || derived.extra != 3 || derived.Dispatch() != 22 || baseValue.Dispatch() != 22) return 1; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("%class.Derived = type { ptr, i32, i32, i32, i32 }", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("@luma_vtable_Derived = private constant [2 x ptr] [ptr @luma_object_tostring, ptr @luma_Derived_inst_Dispatch_0]", result.LlvmIr, StringComparison.Ordinal);
        Assert.Contains("getelementptr %luma.typedesc", result.LlvmIr, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void SupportsExplicitBaseAndThisConstructorInitializers()
    {
        const string source = "virtual class Base { public static int32 order; public static int32 observed; int32 baseValue = Step(2); public this(int32 value) { baseValue = value; Base.observed = Describe(); } public static int32 Step(int32 digit) { Base.order = Base.order * 10 + digit; return digit; } public virtual int32 Describe() { return 2; } public int32 ReadBase() { return baseValue; } } class Derived : Base { int32 derivedValue = Base.Step(1); public this() : base(value: 7) { } public override int32 Describe() { return derivedValue; } public int32 ReadDerived() { return derivedValue; } } class Chained { public static int32 initializerCount; int32 initialized = Initialize(); int32 value; private static int32 Initialize() { Chained.initializerCount += 1; return 1; } public this(int32 value, int32 factor = 2) { this.value = value * factor; } public this(int32 value) : this(factor: 3, value: value) { this.value += 1; } public int32 Read() { return this.value; } } class Program { public static int32 Main() { Derived derived = new Derived(); if (Base.order != 12 || Base.observed != 2 || derived.ReadBase() != 7 || derived.ReadDerived() != 1 || derived.Describe() != 1) return 1; Chained chained = new Chained(5); if (Chained.initializerCount != 1 || chained.Read() != 16) return 2; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void SupportsUnqualifiedInstanceMethodCallsOnTheCurrentReceiver()
    {
        const string source = "class Counter { int32 value; public this() { value = 1; } public int32 Read() { return value; } public void Set(int32 next) mut { value = next; } public static int32 Pick(int32 value) { return value; } public int32 Pick(string ignored) { return Read(); } public int32 Bump(int32 delta) mut { Set(Pick(Read()) + Pick(\"x\") + delta); return Read(); } } struct Vector { int32 value; public this(int32 value) { this.value = value; } public int32 Read() { return value; } public int32 Add(int32 delta) mut { value += delta; return Read(); } } class Program { public static int32 Main() { Counter counter = new Counter(); if (counter.Bump(4) != 6) return 1; Vector vector = Vector(7); if (vector.Add(3) != 10) return 2; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void DiagnosesInvalidConstructorInitializersAndDelegationCycles()
    {
        var missingBaseConstructor = Compile("virtual class Base { public this(int32 value) { } } class Derived : Base { public this() { } }");
        Assert.Contains(missingBaseConstructor.Diagnostics, diagnostic => diagnostic.Code == "LS0811");

        var cycle = Compile("class Cycle { public this(int32 value) : this(text: \"cycle\") { } public this(string text) : this(value: 1) { } }");
        Assert.Contains(cycle.Diagnostics, diagnostic => diagnostic.Code == "LMC3014");

        var thisInInitializer = Compile("class Value { public this(Value other) { } public this() : this(other: this) { } }");
        Assert.Contains(thisInInitializer.Diagnostics, diagnostic => diagnostic.Code == "LS0812" && diagnostic.Message.Contains("initializer argument", StringComparison.Ordinal));
    }

    [Fact]
    public void AppliesConstructorAccessibilityToBaseInitializersAndObjectCreation()
    {
        const string accessible = "virtual class Base { protected this(int32 value) { } } class Derived : Base { public this() : base(7) { } } class Program { public static int32 Main() { Derived value = new Derived(); return 0; } }";
        var valid = Compile(accessible);
        Assert.True(valid.Succeeded, string.Join(Environment.NewLine, valid.Diagnostics));

        var inaccessibleBase = Compile("virtual class Base { private this(int32 value) { } } class Derived : Base { public this() : base(7) { } }");
        Assert.Contains(inaccessibleBase.Diagnostics, diagnostic => diagnostic.Code == "LS0811");

        var inaccessibleCreation = Compile("class Hidden { private this() { } } class Program { public static int32 Main() { Hidden value = new Hidden(); return 0; } }");
        Assert.Contains(inaccessibleCreation.Diagnostics, diagnostic => diagnostic.Code == "LS0751");

        var virtualImplicitConstructor = Compile("virtual class Base { } class Program { public static int32 Main() { Base value = new Base(); return 0; } }");
        Assert.Contains(virtualImplicitConstructor.Diagnostics, diagnostic => diagnostic.Code == "LS0751");
    }

    [Fact]
    public void ResolvesProtectedAndPrivateInstanceMethodAccessibility()
    {
        const string source = "virtual class Base { protected int32 ProtectedValue() { return 3; } private int32 PrivateValue() { return 4; } public int32 ReadPrivateValue() { return PrivateValue(); } } class Derived : Base { public int32 ReadProtectedValue() { return ProtectedValue(); } } class Program { public static int32 Main() { Derived value = new Derived(); return value.ReadProtectedValue() == 3 && value.ReadPrivateValue() == 4 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);

        var inaccessible = Compile("virtual class Base { private int32 Hidden() { return 1; } } class Derived : Base { } class Program { public static int32 Main() { Derived value = new Derived(); return value.Hidden(); } }");
        Assert.Contains(inaccessible.Diagnostics, diagnostic => diagnostic.Code is "LS0200" or "LS0751");
    }

    [Fact]
    public void ResolvesProtectedAndPrivateFieldAccessibilityAcrossInheritance()
    {
        const string source = "virtual class Base { protected int32 value = 3; private int32 secret = 4; protected static int32 shared = 5; public int32 ReadSecret() { return secret; } } class Derived : Base { public int32 ReadProtected() { return value; } public static int32 ReadProtectedStatic() { return Base.shared; } } class Program { public static int32 Main() { Derived item = new Derived(); return item.ReadProtected() == 3 && item.ReadSecret() == 4 && Derived.ReadProtectedStatic() == 5 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);

        var inaccessible = Compile("virtual class Base { private int32 secret; } class Derived : Base { } class Program { public static int32 Main() { Derived item = new Derived(); return item.secret; } }");
        Assert.Contains(inaccessible.Diagnostics, diagnostic => diagnostic.Code == "LMC3012");
    }

    [Fact]
    public void DiagnosesPublicMembersThatExposeLessAccessibleTypes()
    {
        const string source = "class Hidden { } public class PublicApi { public Hidden value = new Hidden(); public Hidden Make() { return new Hidden(); } public void Accept(Hidden input) { } } class Program { public static int32 Main() { return 0; } }";
        var result = Compile(source);
        Assert.True(result.Diagnostics.Count(diagnostic => diagnostic.Code == "LS0840") >= 3, string.Join(Environment.NewLine, result.Diagnostics));
    }

    [Fact]
    public void ConstructorDelegationDoesNotMarkTheClassCompleteBeforeTheOuterBody()
    {
        const string source = "class Failing { public static int32 finalized; public this() { } public this(int32 value) : this() { throw new System.Exception(\"failure\"); } ~this() { Failing.finalized += 1; } } class Program { public static int32 Main() { try { Failing value = new Failing(1); } catch (System.Exception) { } return Failing.finalized; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void BaseConstructorFailureReleasesDerivedFieldInitializersWithoutRunningIncompleteFinalizers()
    {
        const string source = "virtual class Base { public static int32 finalized; public this() { throw new System.Exception(\"base failure\"); } ~this() { Base.finalized += 1; } } class Node { public static int32 finalized; ~this() { Node.finalized += 1; } } class Derived : Base { Node child = new Node(); public this() : base() { } } class Program { public static int32 Main() { try { Derived value = new Derived(); } catch (System.Exception) { } return Node.finalized == 1 && Base.finalized == 0 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void EnforcesConstructorThisRestrictionsUntilRequiredFieldsAreAssigned()
    {
        var fieldRead = Compile("class Node { } class Holder { Node first; Node second; public this() { first = second; second = new Node(); } }");
        Assert.Contains(fieldRead.Diagnostics, diagnostic => diagnostic.Code == "LS0812" && diagnostic.Message.Contains("Field 'second'", StringComparison.Ordinal));

        var earlyCall = Compile("class Node { } class Holder { Node first; Node second; public this() { first = new Node(); Read(); second = new Node(); } public void Read() { } }");
        Assert.Contains(earlyCall.Diagnostics, diagnostic => diagnostic.Code == "LS0812" && diagnostic.Message.Contains("Instance methods cannot be called", StringComparison.Ordinal));

        const string delegatedSource = "class Node { } class Holder { Node value; public this() : this(1) { let alias = this; } public this(int32 ignored) { value = new Node(); } } class Program { public static int32 Main() { Holder holder = new Holder(); return 0; } }";
        var delegated = Compile(delegatedSource);
        Assert.True(delegated.Succeeded, string.Join(Environment.NewLine, delegated.Diagnostics));
        var execution = LlvmJit.RunMain(delegated.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);

        var unreachableWrite = Compile("class Node { } class Holder { Node value; public this() { return; value = new Node(); } }");
        Assert.Contains(unreachableWrite.Diagnostics, diagnostic => diagnostic.Code == "LS0810");

        const string returningBranch = "class Node { } class Holder { Node value; public this(bool initialize) { if (initialize) value = new Node(); else return; } } class Program { public static int32 Main() { Holder holder = new Holder(true); return 0; } }";
        var validBranch = Compile(returningBranch);
        Assert.True(validBranch.Succeeded, string.Join(Environment.NewLine, validBranch.Diagnostics));
    }

    [Fact]
    public void VirtualMemberDeclarationsUseSpecifiedDiagnostics()
    {
        var virtualOnClosed = Compile("class Closed { public virtual int32 Get() { return 1; } }");
        Assert.Contains(virtualOnClosed.Diagnostics, diagnostic => diagnostic.Code == "LS0831");

        var overrideWithoutBase = Compile("class Derived { public override int32 Get() { return 1; } }");
        Assert.Contains(overrideWithoutBase.Diagnostics, diagnostic => diagnostic.Code == "LS0832");

        var missingOverride = Compile("virtual class Base { public virtual int32 Get() { return 1; } } class Derived : Base { public int32 Get() { return 2; } }");
        Assert.Contains(missingOverride.Diagnostics, diagnostic => diagnostic.Code == "LS0833");
    }

    [Fact]
    public void ScopeAllocatedClassesDestroyOnceAfterAliasesLeaveTheirBlock()
    {
        const string source = "class Counter { public static int32 count; int32 value = 42; public static int32 Count() { return Counter.count; } public int32 Read() { return value; } ~this() { Counter.count += 1; } } class Program { public static int32 Main() { { scope Counter(); if (Counter.Count() != 1) return 3; } { let scoped = scope Counter(); let alias = scoped; if (alias.Read() != 42) return 1; } return Counter.Count() == 2 ? 0 : 2; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ScopeAllocatedArraysAreReleasedAtBlockExit()
    {
        const string source = "class Program { public static int32 ReadScopedValue() { int32[] values = scope int32[1]; values[0] = 42; return values[0]; } " +
            "public static int32 Main() { { int32[] values = scope int32[3]; values[1] = 42; if (values.Length != 3 || values[1] != 42) return 1; } return ReadScopedValue() == 42 ? 0 : 2; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ScopedArraysOwnScopedElementsUntilTheContainerIsReleased()
    {
        const string source = "class Node { public static int32 destroyed; public static int32 order; int32 id; int32 value = 42; public this(int32 id) { this.id = id; } public int32 Read() { return value; } ~this() { Node.destroyed += 1; Node.order = Node.order * 10 + id; } } " +
            "class Program { public static int32 Main() { { Node[] nodes = scope { scope Node(1), scope Node(2) }; " +
            "if (nodes.Length != 2 || nodes[1].Read() != 42) return 1; } if (Node.destroyed != 2 || Node.order != 21) return 2; " +
            "{ Node[] nodes = scope { scope Node(3) }; nodes[0] = scope Node(4); if (nodes[0].Read() != 42) return 3; } " +
            "{ Node?[] nodes = scope [2]; nodes[0] = scope Node(5); nodes[1] ??= scope Node(6); if (nodes[0] == null || nodes[0]!.Read() != 42 || nodes[1] == null || nodes[1]!.Read() != 42) return 4; } " +
            "{ int32[][] children = scope { scope int32[1] }; children[0][0] = 42; if (children[0][0] != 42) return 5; } " +
            "{ Node outer = scope Node(7); { Node[] nodes = scope { outer }; if (nodes[0].Read() != 42) return 6; } } " +
            "return Node.destroyed == 7 && Node.order == 2134657 ? 0 : 7; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));

        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ScopedArraysCanOwnScopeBoundDelegatesWithoutLettingThemEscape()
    {
        const string source = "delegate int32 Getter(); class Node { public int32 Read() { return 42; } } class Holder { public Getter? Callback; } class Program { public static int32 Main() { Node node = scope Node(); Getter[] callbacks = scope { () => node.Read() }; Getter[][] nested = scope { { () => node.Read() } }; Getter callback = () => node.Read(); Getter?[] assigned = scope Getter?[1]; assigned[0] = callback; Holder holder = scope Holder(); holder.Callback = callback; Holder directHolder = scope Holder(); directHolder.Callback = () => node.Read(); if (callbacks.Length != 1 || callbacks[0]() != 42 || nested[0][0]() != 42 || assigned[0]!() != 42 || holder.Callback!() != 42 || directHolder.Callback!() != 42) return 1; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var heapArray = Compile("delegate int32 Getter(); class Node { public int32 Read() { return 42; } } class Program { public static int32 Main() { Node node = scope Node(); Getter[] callbacks = { () => node.Read() }; return 0; } }");
        Assert.Contains(heapArray.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var heapOwner = Compile("delegate int32 Getter(); class Node { public int32 Read() { return 42; } } class Holder { public Getter? Callback; } class Program { public static int32 Main() { Node node = scope Node(); Getter callback = () => node.Read(); Holder holder = new Holder(); holder.Callback = callback; return 0; } }");
        Assert.Contains(heapOwner.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var escaped = Compile("delegate int32 Getter(); class Node { public int32 Read() { return 42; } } class Factory { public static Getter Create() { Node node = scope Node(); Getter[] callbacks = scope { () => node.Read() }; return callbacks[0]; } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(escaped.Diagnostics, diagnostic => diagnostic.Code == "LS0701");
    }

    [Fact]
    public void ScopeBoundArrayElementsCannotEscapeIntoHeapOrShorterLivedArrays()
    {
        var heapArray = Compile("class Node { } class Program { public static int32 Main() { Node[] nodes = { scope Node() }; return 0; } }");
        Assert.Contains(heapArray.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var shorterLivedElement = Compile("class Node { } class Program { public static int32 Main() { Node[] nodes = scope Node[1]; { nodes[0] = scope Node(); } return 0; } }");
        Assert.Contains(shorterLivedElement.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var returnedElement = Compile("class Node { } class Factory { public static Node Get() { Node[] nodes = scope { scope Node() }; return nodes[0]; } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(returnedElement.Diagnostics, diagnostic => diagnostic.Code == "LS0701");
    }

    [Fact]
    public void ScopedArrayElementStoresRespectAllocationOrder()
    {
        var laterElement = Compile("class Node { } class Program { public static int32 Main() { Node?[] nodes = scope Node?[1]; Node child = scope Node(); nodes[0] = child; return 0; } }");
        Assert.Contains(laterElement.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var laterCoalescingElement = Compile("class Node { } class Program { public static int32 Main() { Node?[] nodes = scope Node?[1]; Node child = scope Node(); nodes[0] ??= child; return 0; } }");
        Assert.Contains(laterCoalescingElement.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var laterMultiAssignmentElement = Compile("class Node { } class Program { public static int32 Main() { Node?[] nodes = scope Node?[1]; Node child = scope Node(); int32 status; nodes[0], status = child, 1; return status; } }");
        Assert.Contains(laterMultiAssignmentElement.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        const string earlierElementSource = "class Node { public int32 Read() { return 42; } } class Program { public static int32 Main() { Node child = scope Node(); Node?[] nodes = scope Node?[1]; nodes[0] = child; if (nodes[0]!.Read() != 42) return 1; { Node?[] inner = scope Node?[1]; inner[0] = child; if (inner[0]!.Read() != 42) return 2; } return 0; } }";
        var earlierElement = Compile(earlierElementSource);
        Assert.True(earlierElement.Succeeded, string.Join(Environment.NewLine, earlierElement.Diagnostics));
        var earlierElementExecution = LlvmJit.RunMain(earlierElement.LlvmIr!);
        Assert.Equal(0, earlierElementExecution.ExitCode);
        Assert.Equal(0, earlierElementExecution.OutstandingAllocations);
        Assert.Null(earlierElementExecution.InteropError);

        const string directScopedElementSource = "class Node { public int32 Read() { return 42; } } class Program { public static int32 Main() { Node?[] nodes = scope Node?[1]; nodes[0] = scope Node(); return nodes[0]!.Read() == 42 ? 0 : 1; } }";
        var directScopedElement = Compile(directScopedElementSource);
        Assert.True(directScopedElement.Succeeded, string.Join(Environment.NewLine, directScopedElement.Diagnostics));
        var directScopedElementExecution = LlvmJit.RunMain(directScopedElement.LlvmIr!);
        Assert.Equal(0, directScopedElementExecution.ExitCode);
        Assert.Equal(0, directScopedElementExecution.OutstandingAllocations);
        Assert.Null(directScopedElementExecution.InteropError);
    }

    [Fact]
    public void ScopedOwnersMayHoldScopedFieldsOnlyWhenDestructionOrderIsSafe()
    {
        const string scopedAccessorSource = "class Node { public int32 Read() { return 17; } } class Holder { public Node? Item; public Node? Value { get { return Item; } } public Node? this[int32 index] { get { return Item; } } } class Program { public static int32 Main() { Node child = scope Node(); Holder owner = scope Holder(); owner.Item = child; return owner.Value!.Read() == 17 && owner[0]!.Read() == 17 ? 0 : 1; } }";
        var scopedAccessor = Compile(scopedAccessorSource);
        Assert.True(scopedAccessor.Succeeded, string.Join(Environment.NewLine, scopedAccessor.Diagnostics));
        var scopedAccessorExecution = LlvmJit.RunMain(scopedAccessor.LlvmIr!);
        Assert.Equal(0, scopedAccessorExecution.ExitCode);
        Assert.Equal(0, scopedAccessorExecution.OutstandingAllocations);
        Assert.Null(scopedAccessorExecution.InteropError);

        const string safeSource = "class Node { public static int32 order; int32 id; public this(int32 id) { this.id = id; } public int32 Read() { return id; } ~this() { Node.order = Node.order * 10 + id; } } " +
            "class Holder { public Node? Item; ~this() { Node.order = Node.order * 10 + 9; } } " +
            "class Program { public static int32 Main() { { Node child = scope Node(1); Holder owner = scope Holder(); owner.Item = child; if (owner.Item!.Read() != 1) return 1; } return Node.order == 91 ? 0 : 2; } }";
        var safe = Compile(safeSource);
        Assert.True(safe.Succeeded, string.Join(Environment.NewLine, safe.Diagnostics));
        var execution = LlvmJit.RunMain(safe.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        const string innerOwnerSource = "class Node { public static int32 order; int32 id; public this(int32 id) { this.id = id; } ~this() { Node.order = Node.order * 10 + id; } } " +
            "class Holder { public Node? Item; ~this() { Node.order = Node.order * 10 + 9; } } " +
            "class Program { public static int32 Main() { Node child = scope Node(2); { Holder owner = scope Holder(); owner.Item = child; } if (Node.order != 9) return 1; return 0; } }";
        var innerOwner = Compile(innerOwnerSource);
        Assert.True(innerOwner.Succeeded, string.Join(Environment.NewLine, innerOwner.Diagnostics));
        var innerExecution = LlvmJit.RunMain(innerOwner.LlvmIr!);
        Assert.Equal(0, innerExecution.ExitCode);
        Assert.Equal(0, innerExecution.OutstandingAllocations);
        Assert.Null(innerExecution.InteropError);

        const string constructorSource = "class Node { public static int32 order; int32 id; public this(int32 id) { this.id = id; } public int32 Read() { return id; } ~this() { Node.order = Node.order * 10 + id; } } " +
            "class Holder { public Node? Item; public this(Node item) { Item = item; } ~this() { Node.order = Node.order * 10 + 9; } } " +
            "class Program { public static int32 Main() { { Node child = scope Node(3); Holder owner = scope Holder(child); if (owner.Item!.Read() != 3) return 1; } return Node.order == 93 ? 0 : 2; } }";
        var constructorStore = Compile(constructorSource);
        Assert.True(constructorStore.Succeeded, string.Join(Environment.NewLine, constructorStore.Diagnostics));
        var constructorExecution = LlvmJit.RunMain(constructorStore.LlvmIr!);
        Assert.Equal(0, constructorExecution.ExitCode);
        Assert.Equal(0, constructorExecution.OutstandingAllocations);
        Assert.Null(constructorExecution.InteropError);

        const string ordinaryReferenceSource = "class Node { public static int32 order; int32 id; public this(int32 id) { this.id = id; } ~this() { Node.order = Node.order * 10 + id; } } " +
            "class Holder { public Node? Item; ~this() { Node.order = Node.order * 10 + 9; } } " +
            "class Program { public static int32 Main() { { Holder owner = scope Holder(); Node heapChild = new Node(4); owner.Item = heapChild; } return Node.order == 94 ? 0 : 1; } }";
        var ordinaryReference = Compile(ordinaryReferenceSource);
        Assert.True(ordinaryReference.Succeeded, string.Join(Environment.NewLine, ordinaryReference.Diagnostics));
        var ordinaryExecution = LlvmJit.RunMain(ordinaryReference.LlvmIr!);
        Assert.Equal(0, ordinaryExecution.ExitCode);
        Assert.Equal(0, ordinaryExecution.OutstandingAllocations);
        Assert.Null(ordinaryExecution.InteropError);

        const string scopedArrayFieldSource = "class Node { public static int32 order; public int32 Read() { return 7; } ~this() { Node.order = Node.order * 10 + 1; } } " +
            "class Holder { public Node[]? Items; ~this() { Node.order = Node.order * 10 + 9; } } " +
            "class Program { public static int32 Main() { { Node[] items = scope { scope Node() }; Holder owner = scope Holder(); owner.Items = items; if (owner.Items!.Length != 1 || owner.Items[0].Read() != 7) return 1; } return Node.order == 91 ? 0 : 2; } }";
        var scopedArrayField = Compile(scopedArrayFieldSource);
        Assert.True(scopedArrayField.Succeeded, string.Join(Environment.NewLine, scopedArrayField.Diagnostics));
        var scopedArrayExecution = LlvmJit.RunMain(scopedArrayField.LlvmIr!);
        Assert.Equal(0, scopedArrayExecution.ExitCode);
        Assert.Equal(0, scopedArrayExecution.OutstandingAllocations);
        Assert.Null(scopedArrayExecution.InteropError);

        const string scopedArrayConstructorSource = "class Node { public static int32 order; public int32 Read() { return 8; } ~this() { Node.order = Node.order * 10 + 1; } } " +
            "class Holder { public Node[]? Items; public this(Node[] items) { Items = items; } ~this() { Node.order = Node.order * 10 + 9; } } " +
            "class Program { public static int32 Main() { { Node[] items = scope { scope Node() }; Holder owner = scope Holder(items); if (owner.Items!.Length != 1 || owner.Items![0].Read() != 8) return 1; } return Node.order == 91 ? 0 : 2; } }";
        var scopedArrayConstructor = Compile(scopedArrayConstructorSource);
        Assert.True(scopedArrayConstructor.Succeeded, string.Join(Environment.NewLine, scopedArrayConstructor.Diagnostics));
        var scopedArrayConstructorExecution = LlvmJit.RunMain(scopedArrayConstructor.LlvmIr!);
        Assert.Equal(0, scopedArrayConstructorExecution.ExitCode);
        Assert.Equal(0, scopedArrayConstructorExecution.OutstandingAllocations);
        Assert.Null(scopedArrayConstructorExecution.InteropError);

        var heapOwner = Compile("class Node { } class Holder { public Node? Item; } class Program { public static int32 Main() { Holder owner = new Holder(); Node child = scope Node(); owner.Item = child; return 0; } }");
        Assert.Contains(heapOwner.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var reversedOrder = Compile("class Node { } class Holder { public Node? Item; } class Program { public static int32 Main() { Holder owner = scope Holder(); Node child = scope Node(); owner.Item = child; return 0; } }");
        Assert.Contains(reversedOrder.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var unsafeConstructorOrder = Compile("class Node { } class Holder { public Node? Item; public this(Node item) { Item = item; } } class Program { public static int32 Main() { Holder owner = scope Holder(scope Node()); return 0; } }");
        Assert.Contains(unsafeConstructorOrder.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var heapConstructorOwner = Compile("class Node { } class Holder { public Node? Item; public this(Node item) { Item = item; } } class Program { public static int32 Main() { Node child = scope Node(); Holder owner = new Holder(child); return 0; } }");
        Assert.Contains(heapConstructorOwner.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var escapingConstructor = Compile("class Node { } class Holder { public static Node? Saved; public Node? Item; public this(Node item) { Item = item; Holder.Saved = item; } } class Program { public static int32 Main() { Node child = scope Node(); Holder owner = scope Holder(child); return 0; } }");
        Assert.Contains(escapingConstructor.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var shorterOwner = Compile("class Node { } class Holder { public Node? Item; } class Program { public static int32 Main() { Holder owner = scope Holder(); { Node child = scope Node(); owner.Item = child; } return 0; } }");
        Assert.Contains(shorterOwner.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var returnedField = Compile("class Node { } class Holder { public Node? Item; } class Factory { public static Node? Get() { Node child = scope Node(); Holder owner = scope Holder(); owner.Item = child; return owner.Item; } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(returnedField.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var returnedThroughReadonlyParameter = Compile("class Node { } class Holder { public Node? Item; } class Factory { public static Node? Leak(readonly Holder owner) { return owner.Item; } public static int32 Main() { Node child = scope Node(); Holder owner = scope Holder(); owner.Item = child; Node? escaped = Leak(owner); return 0; } }");
        Assert.Contains(returnedThroughReadonlyParameter.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var returnedThroughReadonlyProperty = Compile("class Node { } class Holder { public Node? Item; public Node? Value { get { return Item; } } } class Factory { public static Node? Leak(readonly Holder owner) { return owner.Value; } } class Program { public static int32 Main() { Node child = scope Node(); Holder owner = scope Holder(); owner.Item = child; Node? escaped = Factory.Leak(owner); return 0; } }");
        Assert.Contains(returnedThroughReadonlyProperty.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var returnedThroughReadonlyIndexer = Compile("class Node { } class Holder { public Node? Item; public Node? this[int32 index] { get { return Item; } } } class Factory { public static Node? Leak(readonly Holder owner) { return owner[0]; } } class Program { public static int32 Main() { Node child = scope Node(); Holder owner = scope Holder(); owner.Item = child; Node? escaped = Factory.Leak(owner); return 0; } }");
        Assert.Contains(returnedThroughReadonlyIndexer.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var returnedThroughInstanceCall = Compile("class Node { } class Holder { public Node? Item; public Node? GetItem() { return Item; } } class Program { public static Node? Leak() { Node child = scope Node(); Holder owner = scope Holder(); owner.Item = child; return owner.GetItem(); } public static int32 Main() { return 0; } }");
        Assert.Contains(returnedThroughInstanceCall.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var returnedThroughProperty = Compile("class Node { } class Holder { public Node? Item; public Node? Value { get { return Item; } } } class Program { public static Node? Leak() { Node child = scope Node(); Holder owner = scope Holder(); owner.Item = child; return owner.Value; } public static int32 Main() { return 0; } }");
        Assert.Contains(returnedThroughProperty.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var returnedThroughIndexer = Compile("class Node { } class Holder { public Node? Item; public Node? this[int32 index] { get { return Item; } } } class Program { public static Node? Leak() { Node child = scope Node(); Holder owner = scope Holder(); owner.Item = child; return owner[0]; } public static int32 Main() { return 0; } }");
        Assert.Contains(returnedThroughIndexer.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var constructorGetter = Compile("class Node { } class Holder { public Node? Item; public this(Node item) { Item = item; } public Node? GetItem() { return Item; } } class Factory { public static Node? Leak() { Node child = scope Node(); Holder owner = scope Holder(child); return owner.GetItem(); } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(constructorGetter.Diagnostics, diagnostic => diagnostic.Code == "LS0701");
    }

    [Fact]
    public void ScopeAndReadonlyReferencesCannotEscape()
    {
        const string aliasAssignmentSource = "class Node { int32 value = 17; public int32 Read() { return value; } } class Program { public static int32 Main() { Node? sameScope = null; Node child = scope Node(); sameScope = child; if (sameScope!.Read() != 17) return 1; { Node? innerAlias = null; innerAlias = child; if (innerAlias!.Read() != 17) return 2; } Node? loopAlias = null; int32 count = 0; while (count < 2) { loopAlias = child; count++; } if (loopAlias!.Read() != 17) return 3; return 0; } }";
        var assignedAliases = Compile(aliasAssignmentSource);
        Assert.True(assignedAliases.Succeeded, string.Join(Environment.NewLine, assignedAliases.Diagnostics));
        var aliasExecution = LlvmJit.RunMain(assignedAliases.LlvmIr!);
        Assert.Equal(0, aliasExecution.ExitCode);
        Assert.Equal(0, aliasExecution.OutstandingAllocations);
        Assert.Null(aliasExecution.InteropError);

        const string multiAssignmentAliasSource = "class Node { public int32 Read() { return 17; } } class Program { public static int32 ReadBorrowed(readonly Node source) { Node? alias = null; int32 borrowedValue = 0; alias, borrowedValue = source, source.Read(); return alias!.Read() == borrowedValue ? 0 : 1; } public static int32 Main() { Node? alias = null; int32 status = 1; Node child = scope Node(); alias, status = child, 7; return alias!.Read() == 17 && status == 7 && ReadBorrowed(child) == 0 ? 0 : 1; } }";
        var multiAssignmentAliases = Compile(multiAssignmentAliasSource);
        Assert.True(multiAssignmentAliases.Succeeded, string.Join(Environment.NewLine, multiAssignmentAliases.Diagnostics));
        var multiAssignmentAliasExecution = LlvmJit.RunMain(multiAssignmentAliases.LlvmIr!);
        Assert.Equal(0, multiAssignmentAliasExecution.ExitCode);
        Assert.Equal(0, multiAssignmentAliasExecution.OutstandingAllocations);
        Assert.Null(multiAssignmentAliasExecution.InteropError);

        var outerAliasAssignment = Compile("class Node { } class Program { public static int32 Main() { Node? outer = null; { Node child = scope Node(); outer = child; } return 0; } }");
        Assert.Contains(outerAliasAssignment.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var heapReplacement = Compile("class Node { } class Program { public static int32 Main() { Node? alias = null; Node child = scope Node(); alias = child; alias = new Node(); return 0; } }");
        Assert.Contains(heapReplacement.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var nonReadonlyArgument = Compile("class Node { } class Sink { public static void Keep(Node node) { } } class Program { public static int32 Main() { let node = scope Node(); Sink.Keep(node); return 0; } }");
        Assert.Contains(nonReadonlyArgument.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var returned = Compile("class Node { } class Factory { public static Node Create() { return scope Node(); } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(returned.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var escapingReceiver = Compile("class Node { public static void Keep(Node node) { } public void Escape() { Node.Keep(this); } } class Program { public static int32 Main() { let node = scope Node(); node.Escape(); return 0; } }");
        Assert.Contains(escapingReceiver.Diagnostics, diagnostic => diagnostic.Code == "LS0702");

        const string implicitReceiverEscapeSource = "class Node { public static Node? Last; public void Remember() { Node.Last = this; } public void ForwardDirect() { Remember(); } public void ForwardThroughTry() { try { Remember(); } catch (...) { } } public void ForwardThroughFinally() { try { } finally { Remember(); } } } class Program { public static int32 Main() { Node node = scope Node(); node.ForwardDirect(); node.ForwardThroughTry(); node.ForwardThroughFinally(); return 0; } }";
        var implicitReceiverEscape = Compile(implicitReceiverEscapeSource);
        Assert.True(implicitReceiverEscape.Diagnostics.Count(diagnostic => diagnostic.Code == "LS0702") >= 3,
            string.Join(Environment.NewLine, implicitReceiverEscape.Diagnostics));

        const string virtualReceiverEscapeSource = "virtual class BaseNode { public static BaseNode? Last; public void Forward() { Escape(); } public virtual void Escape() { } } class DerivedNode : BaseNode { public override void Escape() { BaseNode.Last = this; } } class Program { public static int32 Main() { DerivedNode node = scope DerivedNode(); node.Forward(); return 0; } }";
        var virtualReceiverEscape = Compile(virtualReceiverEscapeSource);
        Assert.Contains(virtualReceiverEscape.Diagnostics, diagnostic => diagnostic.Code == "LS0702");

        const string safeVirtualReceiverSource = "virtual class SafeBase { public virtual int32 Value() { return 41; } public int32 Forward() { return Value(); } } class SafeDerived : SafeBase { public override int32 Value() { return 42; } } class Program { public static int32 Main() { SafeDerived node = scope SafeDerived(); return node.Forward() == 42 ? 0 : 1; } }";
        var safeVirtualReceiver = Compile(safeVirtualReceiverSource);
        Assert.True(safeVirtualReceiver.Succeeded, string.Join(Environment.NewLine, safeVirtualReceiver.Diagnostics));

        const string safeExplicitThisReadSource = "class Node { public int32 value = 42; public int32 Read() { return this.value; } } class Program { public static int32 Main() { Node node = scope Node(); return node.Read() == 42 ? 0 : 1; } }";
        var safeExplicitThisRead = Compile(safeExplicitThisReadSource);
        Assert.True(safeExplicitThisRead.Succeeded, string.Join(Environment.NewLine, safeExplicitThisRead.Diagnostics));

        const string safeScopedReceiverFieldWriteSource = "class Node { public Node? child; public int32 value = 42; public void SetChild() mut { this.child = new Node(); } public int32 Read() { return this.value; } public int32 ReadChild() { return this.child!.Read(); } } class Program { public static int32 Main() { Node node = scope Node(); node.SetChild(); return node.ReadChild() == 42 ? 0 : 1; } }";
        var safeScopedReceiverFieldWrite = Compile(safeScopedReceiverFieldWriteSource);
        Assert.True(safeScopedReceiverFieldWrite.Succeeded, string.Join(Environment.NewLine, safeScopedReceiverFieldWrite.Diagnostics));
        var safeScopedReceiverFieldWriteExecution = LlvmJit.RunMain(safeScopedReceiverFieldWrite.LlvmIr!);
        Assert.Equal(0, safeScopedReceiverFieldWriteExecution.ExitCode);
        Assert.Equal(0, safeScopedReceiverFieldWriteExecution.OutstandingAllocations);

        const string openVirtualReceiverSource = "virtual class OpenBase { public virtual void Read() { } public void Forward() { Read(); } } class ClosedChild : OpenBase { } class Program { public static int32 Main() { OpenBase node = scope ClosedChild(); node.Forward(); return 0; } }";
        var openVirtualReceiver = Compile(openVirtualReceiverSource);
        Assert.Contains(openVirtualReceiver.Diagnostics, diagnostic => diagnostic.Code == "LS0702");

        const string baseQualifiedReceiverEscapeSource = "virtual class BaseNode { public static BaseNode? Last; public virtual void Escape() { BaseNode.Last = this; } } class DerivedNode : BaseNode { public void ForwardBase() { base.Escape(); } } class Program { public static int32 Main() { DerivedNode node = scope DerivedNode(); node.ForwardBase(); return 0; } }";
        var baseQualifiedReceiverEscape = Compile(baseQualifiedReceiverEscapeSource);
        Assert.Contains(baseQualifiedReceiverEscape.Diagnostics, diagnostic => diagnostic.Code == "LS0702");

        const string interfaceReceiverEscapeSource = "interface INode { void Escape(); } class Node : INode { public static Node? Last; public void Escape() { Node.Last = this; } } class Program { public static int32 Main() { INode node = scope Node(); node.Escape(); return 0; } }";
        var interfaceReceiverEscape = Compile(interfaceReceiverEscapeSource);
        Assert.Contains(interfaceReceiverEscape.Diagnostics, diagnostic => diagnostic.Code == "LS0702");

        const string safeInterfaceReceiverSource = "interface INode { int32 Read(); } class Node : INode { public int32 Read() { return 42; } } class Program { public static int32 Main() { INode node = scope Node(); return node.Read() == 42 ? 0 : 1; } }";
        var safeInterfaceReceiver = Compile(safeInterfaceReceiverSource);
        Assert.True(safeInterfaceReceiver.Succeeded, string.Join(Environment.NewLine, safeInterfaceReceiver.Diagnostics));

        const string openInterfaceReceiverSource = "interface INode { int32 Read(); } virtual class Node : INode { public virtual int32 Read() { return 42; } } class Program { public static int32 Main() { INode node = scope Node(); return node.Read(); } }";
        var openInterfaceReceiver = Compile(openInterfaceReceiverSource);
        Assert.Contains(openInterfaceReceiver.Diagnostics, diagnostic => diagnostic.Code == "LS0702");

        const string readonlyInterfaceEscapeSource = "interface INode { void Escape(); } class Node : INode { public static Node? Last; public void Escape() { Node.Last = this; } } class Program { public static int32 Use(readonly INode node) { node.Escape(); return 0; } public static int32 Main() { Node node = new Node(); return Use(node); } }";
        var readonlyInterfaceEscape = Compile(readonlyInterfaceEscapeSource);
        Assert.Contains(readonlyInterfaceEscape.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        const string safeReadonlyInterfaceSource = "interface INode { int32 Read(); } class Node : INode { public int32 Read() { return 42; } } class Program { public static int32 Use(readonly INode node) { return node.Read(); } public static int32 Main() { Node node = new Node(); return Use(node) == 42 ? 0 : 1; } }";
        var safeReadonlyInterface = Compile(safeReadonlyInterfaceSource);
        Assert.True(safeReadonlyInterface.Succeeded, string.Join(Environment.NewLine, safeReadonlyInterface.Diagnostics));

        var readonlyBorrow = Compile("class Node { public static int32 Read(readonly Node node) { return 0; } } class Program { public static int32 Main() { let node = scope Node(); return Node.Read(node); } }");
        Assert.True(readonlyBorrow.Succeeded, string.Join(Environment.NewLine, readonlyBorrow.Diagnostics));

        const string readonlyLocalAliasSource = "class Node { public int32 Read() { return 23; } } class Program { public static int32 Read(readonly Node source) { Node? alias = null; alias = source; return alias!.Read(); } public static int32 Main() { Node node = new Node(); return Read(node) == 23 ? 0 : 1; } }";
        var readonlyLocalAlias = Compile(readonlyLocalAliasSource);
        Assert.True(readonlyLocalAlias.Succeeded, string.Join(Environment.NewLine, readonlyLocalAlias.Diagnostics));
        var readonlyAliasExecution = LlvmJit.RunMain(readonlyLocalAlias.LlvmIr!);
        Assert.Equal(0, readonlyAliasExecution.ExitCode);
        Assert.Equal(0, readonlyAliasExecution.OutstandingAllocations);
        Assert.Null(readonlyAliasExecution.InteropError);

        const string readonlyAliasKeepsOwnedCallResultAliveSource = "delegate int32 Getter(); class Node { public static int32 destroyed; public int32 Read() { return 42; } ~this() { Node.destroyed++; } } class Owner { public Node Create() { return new Node(); } public Node Value { get { return new Node(); } } public Node this[int32 index] { get { return new Node(); } } } class Program { public static int32 Read(readonly Owner owner) { Node? alias = owner.Create(); if (Node.destroyed != 0) return 99; return alias!.Read(); } public static int32 Assign(readonly Owner owner) { Node? alias = null; alias = owner.Create(); if (Node.destroyed != 1) return 99; return alias!.Read(); } public static int32 Capture(readonly Owner owner) { Node? alias = owner.Create(); Getter closure = () => alias!.Read(); if (Node.destroyed != 2) return 99; return closure(); } public static int32 ReadProperty(readonly Owner owner) { Node? alias = owner.Value; if (Node.destroyed != 3) return 99; return alias!.Read(); } public static int32 ReadIndexer(readonly Owner owner) { Node? alias = owner[0]; if (Node.destroyed != 4) return 99; return alias!.Read(); } public static int32 Main() { Owner owner = new Owner(); return Read(owner) == 42 && Node.destroyed == 1 && Assign(owner) == 42 && Node.destroyed == 2 && Capture(owner) == 42 && Node.destroyed == 3 && ReadProperty(owner) == 42 && Node.destroyed == 4 && ReadIndexer(owner) == 42 && Node.destroyed == 5 ? 0 : 1; } }";
        var readonlyAliasKeepsOwnedCallResultAlive = Compile(readonlyAliasKeepsOwnedCallResultAliveSource);
        Assert.True(readonlyAliasKeepsOwnedCallResultAlive.Succeeded, string.Join(Environment.NewLine, readonlyAliasKeepsOwnedCallResultAlive.Diagnostics));
        var readonlyAliasKeepsOwnedCallResultAliveExecution = LlvmJit.RunMain(readonlyAliasKeepsOwnedCallResultAlive.LlvmIr!);
        Assert.Equal(0, readonlyAliasKeepsOwnedCallResultAliveExecution.ExitCode);
        Assert.Equal(0, readonlyAliasKeepsOwnedCallResultAliveExecution.OutstandingAllocations);
        Assert.Null(readonlyAliasKeepsOwnedCallResultAliveExecution.InteropError);

        const string readonlyInitializedAliasesSource = "delegate int32 Getter(); class Node { public int32 Read() { return 23; } } class Program { public static int32 ReadNode(readonly Node source) { Node? alias = source; return alias!.Read(); } public static int32 ReadNodeByRef(readonly ref Node source) { Node? alias = source; return alias!.Read(); } public static int32 ReadNodeRefReadonly(ref readonly Node source) { Node? alias = source; return alias!.Read(); } public static int32 ReadNodeLoopAlias(readonly Node source) { Node? alias = null; int32 count = 0; while (count < 2) { alias = source; count++; } return alias!.Read(); } public static int32 ReadNodeViaLambda(readonly Node source) { Node? alias = source; Getter closure = () => alias!.Read(); return closure(); } public static int32 ReadArray(readonly int32[] source) { int32[]? alias = source; return alias!.Length; } public static int32 ReadString(readonly string source) { string? alias = source; return alias!.Length; } public static int32 ReadStringAssigned(readonly string source) { string? alias = null; alias = source; return alias!.Length; } public static int32 ReadDelegate(readonly Getter source) { Getter? alias = source; return alias!(); } public static int32 Main() { Node node = new Node(); int32[] items = { 1, 2, 3 }; string text = \"ab\"; text += \"cd\"; Getter getter = () => 37; return ReadNode(node) == 23 && ReadNodeByRef(ref node) == 23 && ReadNodeRefReadonly(ref node) == 23 && ReadNodeLoopAlias(node) == 23 && ReadNodeViaLambda(node) == 23 && ReadArray(items) == 3 && ReadString(text) == 4 && ReadStringAssigned(text) == 4 && ReadDelegate(getter) == 37 ? 0 : 1; } }";
        var readonlyInitializedAliases = Compile(readonlyInitializedAliasesSource);
        Assert.True(readonlyInitializedAliases.Succeeded, string.Join(Environment.NewLine, readonlyInitializedAliases.Diagnostics));
        var readonlyInitializedAliasesExecution = LlvmJit.RunMain(readonlyInitializedAliases.LlvmIr!);
        Assert.Equal(0, readonlyInitializedAliasesExecution.ExitCode);
        Assert.Equal(0, readonlyInitializedAliasesExecution.OutstandingAllocations);
        Assert.Null(readonlyInitializedAliasesExecution.InteropError);

        const string readonlyArrayAliasSource = "class Program { public static int32 Read(readonly int32[] source) { int32[]? alias = null; alias = source; return alias!.Length; } public static int32 Main() { int32[] items = { 1, 2, 3 }; return Read(items) == 3 ? 0 : 1; } }";
        var readonlyArrayAlias = Compile(readonlyArrayAliasSource);
        Assert.True(readonlyArrayAlias.Succeeded, string.Join(Environment.NewLine, readonlyArrayAlias.Diagnostics));
        var readonlyArrayAliasExecution = LlvmJit.RunMain(readonlyArrayAlias.LlvmIr!);
        Assert.Equal(0, readonlyArrayAliasExecution.ExitCode);
        Assert.Equal(0, readonlyArrayAliasExecution.OutstandingAllocations);
        Assert.Null(readonlyArrayAliasExecution.InteropError);

        var readonlyAliasEscapesInnerScope = Compile("class Node { } class Program { public static void Keep(readonly Node source) { Node? outer = null; { Node? inner = null; inner = source; outer = inner; } } public static int32 Main() { return 0; } }");
        Assert.Contains(readonlyAliasEscapesInnerScope.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var readonlyAliasHeapReplacement = Compile("class Node { } class Program { public static void Replace(readonly Node source) { Node? alias = null; alias = source; alias = new Node(); } public static int32 Main() { return 0; } }");
        Assert.Contains(readonlyAliasHeapReplacement.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var readonlyStore = Compile("class Node { } class Holder { Node? value; public void Keep(readonly Node node) mut { value = node; } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(readonlyStore.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        var fieldInitializer = Compile("class Node { } class Holder { Node? value = scope Node(); } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(fieldInitializer.Diagnostics, diagnostic => diagnostic.Code == "LS0803");
    }

    [Fact]
    public void NullableFlowRefinesNormalLoopExitsAndMergesBreakPaths()
    {
        const string source = "class Node { } class Program { public static int32 Main() { Node? whileNode = null; while (whileNode == null) { whileNode = new Node(); } Node afterWhile = whileNode; Node? forNode = null; for (; forNode == null;) { forNode = new Node(); } Node afterFor = forNode; Node? doNode = null; do { doNode = new Node(); } while (doNode == null); Node afterDo = doNode; Node? breakNode = null; while (breakNode == null) { breakNode = new Node(); break; } Node afterBreak = breakNode; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));

        var unsafeBreak = Compile("class Node { } class Program { public static int32 Main() { Node? value = null; while (value == null) { break; } Node invalid = value; return 0; } }");
        Assert.Contains(unsafeBreak.Diagnostics, diagnostic => diagnostic.Code == "LS1302");

        var zeroIteration = Compile("class Node { } class Program { public static int32 Main() { int32[] items = { 1 }; Node? value = null; for (let item in items) { value = new Node(); } Node invalid = value; return 0; } }");
        Assert.Contains(zeroIteration.Diagnostics, diagnostic => diagnostic.Code == "LS1302");
    }

    [Fact]
    public void ConstantIfConditionsDriveReachabilityAndNullableFlow()
    {
        const string source = "class Node { public int32 Read() { return 42; } } class Program { public static int32 Main() { Node? whenTrue = null; if (true) whenTrue = new Node(); else whenTrue = null; Node trueValue = whenTrue; Node? whenFalse = null; if (false) whenFalse = null; else whenFalse = new Node(); Node falseValue = whenFalse; if (true) { return trueValue.Read() == 42 && falseValue.Read() == 42 ? 0 : 1; } else { return 2; } int32 unreachable = 3; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS3001" && diagnostic.Severity == DiagnosticSeverity.Warning);
        Assert.True(result.Diagnostics.Count(diagnostic => diagnostic.Code == "LS3001") >= 3);

        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var deadBranchStillValidated = Compile("class Program { public static int32 Main() { if (true) { } else { int32 invalid = \"not an integer\"; } return 0; } }");
        Assert.Contains(deadBranchStillValidated.Diagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.Contains(deadBranchStillValidated.Diagnostics, diagnostic => diagnostic.Code == "LS3001");

        var deadFalseBreak = Compile("class Node { public int32 Read() { return 9; } } class Program { public static int32 Main() { Node? value = new Node(); while (true) { if (false) { value = null; break; } break; } Node nonNull = value; return nonNull.Read() == 9 ? 0 : 1; } }");
        Assert.True(deadFalseBreak.Succeeded, string.Join(Environment.NewLine, deadFalseBreak.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(deadFalseBreak.LlvmIr!).ExitCode);

        var deadElseBreak = Compile("class Node { public int32 Read() { return 11; } } class Program { public static int32 Main() { Node? value = new Node(); while (true) { if (true) { } else { value = null; break; } break; } Node nonNull = value; return nonNull.Read() == 11 ? 0 : 1; } }");
        Assert.True(deadElseBreak.Succeeded, string.Join(Environment.NewLine, deadElseBreak.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(deadElseBreak.LlvmIr!).ExitCode);

        var constantReturningBranches = Compile("class Program { public static int32 Main() { if (true) return 23; } }");
        Assert.True(constantReturningBranches.Succeeded, string.Join(Environment.NewLine, constantReturningBranches.Diagnostics));
        Assert.Equal(23, LlvmJit.RunMain(constantReturningBranches.LlvmIr!).ExitCode);

        var constantElseReturn = Compile("class Program { public static int32 Main() { if (false) return 1; else return 29; } }");
        Assert.True(constantElseReturn.Succeeded, string.Join(Environment.NewLine, constantElseReturn.Diagnostics));
        Assert.Equal(29, LlvmJit.RunMain(constantElseReturn.LlvmIr!).ExitCode);

        var constantConditionalFlow = Compile("class Node { public int32 Read() { return 31; } } class Program { public static int32 Main() { Node? selected = null; bool wasNull = true ? (selected = new Node()) == null : true; Node nonNull = selected; return !wasNull && nonNull.Read() == 31 ? 0 : 1; } }");
        Assert.True(constantConditionalFlow.Succeeded, string.Join(Environment.NewLine, constantConditionalFlow.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(constantConditionalFlow.LlvmIr!).ExitCode);
    }

    [Fact]
    public void NullableLoopFlowUsesBreakAndContinueExitPaths()
    {
        const string constantFalseLoops = "class Node { public int32 Read() { return 7; } } class Program { public static int32 Main() { Node? fromWhile = new Node(); while (false) fromWhile = null; Node whileValue = fromWhile; Node? fromFor = new Node(); for (; false;) fromFor = null; Node forValue = fromFor; return whileValue.Read() == 7 && forValue.Read() == 7 ? 0 : 1; } }";
        var constantFalse = Compile(constantFalseLoops);
        Assert.True(constantFalse.Succeeded, string.Join(Environment.NewLine, constantFalse.Diagnostics));
        Assert.Equal(2, constantFalse.Diagnostics.Count(diagnostic => diagnostic.Code == "LS3001" && diagnostic.Severity == DiagnosticSeverity.Warning));
        var constantFalseExecution = LlvmJit.RunMain(constantFalse.LlvmIr!);
        Assert.Equal(0, constantFalseExecution.ExitCode);
        Assert.Equal(0, constantFalseExecution.OutstandingAllocations);
        Assert.Null(constantFalseExecution.InteropError);

        var trueLoopBreak = Compile("class Node { } class Program { public static int32 Main() { Node? value = null; while (true) { if (value == null) return 0; break; } Node nonNull = value; return 0; } }");
        Assert.True(trueLoopBreak.Succeeded, string.Join(Environment.NewLine, trueLoopBreak.Diagnostics));

        var trueForBreak = Compile("class Node { } class Program { public static int32 Main() { Node? value = null; for (;;) { if (value == null) { value = new Node(); continue; } break; } Node nonNull = value; return 0; } }");
        Assert.True(trueForBreak.Succeeded, string.Join(Environment.NewLine, trueForBreak.Diagnostics));

        var doContinue = Compile("class Node { } class Program { public static int32 Main() { Node? value = null; do { value = new Node(); continue; value = null; } while (value == null); Node nonNull = value; return 0; } }");
        Assert.True(doContinue.Succeeded, string.Join(Environment.NewLine, doContinue.Diagnostics));
        Assert.Contains(doContinue.Diagnostics, diagnostic => diagnostic.Code == "LS3001" && diagnostic.Severity == DiagnosticSeverity.Warning);

        var nullableBreak = Compile("class Node { } class Program { public static int32 Main() { Node? value = null; while (true) { break; } Node nonNull = value; return 0; } }");
        Assert.Contains(nullableBreak.Diagnostics, diagnostic => diagnostic.Code == "LS1302");

        var unreachableDoCondition = Compile("class Node { } class Program { public static int32 Main() { Node? value = null; do { break; } while (value == null); Node nonNull = value; return 0; } }");
        Assert.Contains(unreachableDoCondition.Diagnostics, diagnostic => diagnostic.Code == "LS1302");
    }

    [Fact]
    public void NullableLoopFlowRechecksBodiesUntilTheLoopStateConverges()
    {
        const string whileSource = "class Node { public int32 Read() { return 1; } } class Program { public static int32 Main() { Node? value = new Node(); while (true) { value.Read(); value = null; } } }";
        var whileResult = Compile(whileSource);
        Assert.Contains(whileResult.Diagnostics, diagnostic => diagnostic.Code == "LS1303");

        const string doSource = "class Node { public int32 Read() { return 1; } } class Program { public static int32 Main() { Node? value = new Node(); do { value.Read(); value = null; } while (true); } }";
        var doResult = Compile(doSource);
        Assert.Contains(doResult.Diagnostics, diagnostic => diagnostic.Code == "LS1303");

        const string forSource = "class Node { public int32 Read() { return 1; } } class Program { public static int32 Main() { Node? value = new Node(); for (; true;) { value.Read(); value = null; } } }";
        var forResult = Compile(forSource);
        Assert.Contains(forResult.Diagnostics, diagnostic => diagnostic.Code == "LS1303");

        const string forInSource = "class Node { public int32 Read() { return 1; } } class Program { public static int32 Main() { int32[] items = { 1, 2 }; Node? value = new Node(); for (let item in items) { value.Read(); value = null; } return 0; } }";
        var forInResult = Compile(forInSource);
        Assert.Contains(forInResult.Diagnostics, diagnostic => diagnostic.Code == "LS1303");

        const string refinedSource = "class Node { public int32 Read() { return 1; } } class Program { public static int32 Main() { Node? value = new Node(); while (value != null) { value.Read(); value = null; } return 0; } }";
        var refinedResult = Compile(refinedSource);
        Assert.True(refinedResult.Succeeded, string.Join(Environment.NewLine, refinedResult.Diagnostics));

        const string refinedForSource = "class Node { public int32 Read() { return 1; } } class Program { public static int32 Main() { Node? value = new Node(); for (; value != null;) { value.Read(); value = null; } return 0; } }";
        var refinedForResult = Compile(refinedForSource);
        Assert.True(refinedForResult.Succeeded, string.Join(Environment.NewLine, refinedForResult.Diagnostics));

        const string refinedForInSource = "class Node { public int32 Read() { return 1; } } class Program { public static int32 Main() { int32[] items = { 1, 2 }; Node? value = new Node(); for (let item in items) { if (value != null) value.Read(); value = null; } return 0; } }";
        var refinedForInResult = Compile(refinedForInSource);
        Assert.True(refinedForInResult.Succeeded, string.Join(Environment.NewLine, refinedForInResult.Diagnostics));
    }

    [Fact]
    public void NonVoidMethodsMayTerminateInUnbreakableInfiniteLoops()
    {
        foreach (var source in new[]
        {
            "class Program { public static int32 Main() { while (true) { } } }",
            "class Program { public static int32 Main() { for (;;) { } } }",
            "class Program { public static int32 Main() { do { } while (true); } }",
            "class Program { public static int32 Main() { while (true) { while (true) { break; } } } }",
            "class Program { public static void Main() { Never(out int32 value); } public static void Never(out int32 value) { while (true) { } } }"
        })
        {
            var result = Compile(source);
            Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
            Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Code == "LS0630" || diagnostic.Code == "LS0720");
        }

        var conditionalBreak = Compile("class Program { public static int32 Main() { while (true) { if (true) break; } } }");
        Assert.Contains(conditionalBreak.Diagnostics, diagnostic => diagnostic.Code == "LS0630");

        var labelledBreak = Compile("class Program { public static int32 Main() { outer: while (true) { while (true) { break outer; } } } }");
        Assert.Contains(labelledBreak.Diagnostics, diagnostic => diagnostic.Code == "LS0630");
    }

    [Theory]
    [InlineData("'A'", "65")]
    [InlineData("'\\n'", "10")]
    [InlineData("'\\u0041'", "65")]
    public void CompilesUtf16CharacterLiterals(string literal, string value)
    {
        var result = Compile($"class Program {{ public static int32 Main() {{ let char ch = {literal}; return ch; }} }}");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("zext i16", result.LlvmIr!);
        Assert.Contains("to i32", result.LlvmIr);
        Assert.Contains($"store i16 {value}", result.LlvmIr);
    }

    [Fact]
    public void TokenizesHexadecimalDigitsAndSignedFloatingExponentsCorrectly()
    {
        var hex = Compile("class Program { public static int32 Main() { let value = 0xDEAD_BEEF; if (value == 3735928559u) return 0; return 1; } }");
        Assert.True(hex.Succeeded, string.Join(Environment.NewLine, hex.Diagnostics));
        var exponent = Compile("class Program { public static int32 Main() { let value = 1e+2; if (value + 1 == 101.0) return 0; return 1; } }");
        Assert.True(exponent.Succeeded, string.Join(Environment.NewLine, exponent.Diagnostics));
        Assert.Contains("sitofp", exponent.LlvmIr!);
        Assert.Contains("fcmp oeq double", exponent.LlvmIr);
    }

    [Theory]
    [InlineData("0b102")]
    [InlineData("18446744073709551616")]
    [InlineData("1__0")]
    [InlineData("1e+_2")]
    public void MalformedAndOutOfRangeNumericLiteralsAreDiagnosed(string literal)
    {
        var result = Compile($"class Program {{ public static int32 Main() {{ let value = {literal}; return 0; }} }}");
        Assert.Contains(result.Diagnostics, d => d.Code == "LS0001" && d.Severity == DiagnosticSeverity.Error);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void SupportsBooleanBitwiseCharPromotionAndInt32ShiftCounts()
    {
        var result = Compile("class Program { public static int32 Main() { let flags = true & false; int8 value = 1; int8 count = 1; let shifted = value << count; char letter = 'A'; let code = letter + 1; if ((flags | (code == 66)) && shifted == 2) return 0; return 1; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var ir = result.LlvmIr!;
        Assert.Contains("and i1", ir);
        Assert.True(ir.Contains("or i1", StringComparison.Ordinal), ir);
        Assert.Contains("shl i32", ir);
        Assert.Contains("zext i16", ir);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-bitwise-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try { Assert.True(LlvmToolchain.VerifyAndEmitObject(ir, objectPath, "host", optimize: false).Succeeded); }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }
    }

    [Fact]
    public void SupportsArithmeticBitwiseAndShiftCompoundAssignments()
    {
        var result = Compile("class Program { public static int32 Main() { int32 value = 64; value -= 4; value *= 2; value /= 4; value %= 7; value <<= 1; value >>= 1; value &= 3; value |= 4; value ^= 1; bool flags = true; flags &= false; flags |= false; flags ^= false; if (!flags && value == 7) return 0; return 1; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var ir = result.LlvmIr!;
        Assert.Contains("llvm.ssub.with.overflow.i32", ir);
        Assert.Contains("llvm.smul.with.overflow.i32", ir);
        Assert.Contains("sdiv i32", ir);
        Assert.Contains("srem i32", ir);
        Assert.Contains("shl i32", ir);
        Assert.Contains("ashr i32", ir);
        Assert.Contains("and i1", ir);
        Assert.Contains("or i32", ir);
        Assert.Contains("xor i32", ir);
    }

    [Fact]
    public void SupportsTypedAndInferredMultipleLocalDeclarations()
    {
        var result = Compile("class Program { public static int32 Main() { int32 a, b, c = 1, 2; var x, y = 3, 4; let int64 first, second = 5, 6; if (a + b + c + x + y + first + second == 21) return 0; return 1; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("store i32 zeroinitializer", result.LlvmIr!);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-multidecl-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try { Assert.True(LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, "host", optimize: false).Succeeded); }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }
    }

    [Fact]
    public void EnforcesMultipleDeclarationInitializerRules()
    {
        var mixedInference = Compile("class Program { public static void Main() { var a, b = 1, 2.0; } }");
        Assert.Contains(mixedInference.Diagnostics, d => d.Code == "LS0213");
        Assert.DoesNotContain(mixedInference.Diagnostics, d => d.Code == "LMC3010");

        var missingInitializer = Compile("class Program { public static void Main() { let a, b = 1; } }");
        Assert.Contains(missingInitializer.Diagnostics, d => d.Code == "LS0213");

        var extraInitializer = Compile("class Program { public static void Main() { int32 a = 1, 2; } }");
        Assert.Contains(extraInitializer.Diagnostics, d => d.Code == "LS0212");

        var csharpForm = Compile("class Program { public static void Main() { int32 a = 1, b = 2; } }");
        Assert.Contains(csharpForm.Diagnostics, d => d.Code == "LS0214");
    }

    [Fact]
    public void MultipleDeclarationNamesCannotBeReusedInTheirInitializers()
    {
        var result = Compile("class Program { public static void Main() { int32 first, second = 1, first; } }");
        Assert.Contains(result.Diagnostics, d => d.Code == "LS0201");
        Assert.DoesNotContain(result.Diagnostics, d => d.Code == "LS0200");
    }

    [Fact]
    public void SupportsPrimitiveConstantLocalsAndRejectsNonConstantInitializers()
    {
        var result = Compile("class Program { public static int32 Main() { const int32 baseValue = 6; const int32 answer = baseValue * 7; if (answer == 42) return 0; return 1; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("store i32", result.LlvmIr!);

        var reassigned = Compile("class Program { public static void Main() { const int32 value = 1; value = 2; } }");
        Assert.Contains(reassigned.Diagnostics, d => d.Code == "LMC3012");

        var call = Compile("class Program { public static int32 Value() { return 4; } public static void Main() { const int32 value = Value(); } }");
        Assert.Contains(call.Diagnostics, d => d.Code == "LMC3010" && d.Message.Contains("constant expression", StringComparison.Ordinal));

        var missing = Compile("class Program { public static void Main() { const int32 value; } }");
        Assert.Contains(missing.Diagnostics, d => d.Code == "LS0213");
    }

    [Fact]
    public void SupportsSimultaneousMultipleAssignment()
    {
        var result = Compile("class Program { public static int32 Main() { int32 a, b = 1, 2; a, b = b, a; if (a == 2 && b == 1) return 0; return 1; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var ir = result.LlvmIr!;
        var firstLoad = ir.IndexOf("load i32", StringComparison.Ordinal);
        var secondLoad = ir.IndexOf("load i32", firstLoad + "load i32".Length, StringComparison.Ordinal);
        var firstStoreAfterLoads = ir.IndexOf("store i32", secondLoad, StringComparison.Ordinal);
        Assert.True(secondLoad >= 0 && firstStoreAfterLoads > secondLoad, "All right-hand values must be loaded before the first assignment store.");
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-multiassign-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try { Assert.True(LlvmToolchain.VerifyAndEmitObject(ir, objectPath, "host", optimize: false).Succeeded); }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }

        var mismatch = Compile("class Program { public static void Main() { int32 a, b = 1, 2; a, b = 3; } }");
        Assert.Contains(mismatch.Diagnostics, d => d.Code == "LS0210");
        var repeated = Compile("class Program { public static void Main() { int32 a = 1; a, a = 2, 3; } }");
        Assert.Contains(repeated.Diagnostics, d => d.Code == "LMC3012" && d.Message.Contains("more than once", StringComparison.Ordinal));
    }

    [Fact]
    public void SupportsLabeledLoopsAndDiagnosesInvisibleLabels()
    {
        var result = Compile("class Program { public static int32 Main() { outer: while (true) { while (true) { break outer; } } return 0; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("br label %loop.end.2", result.LlvmIr!);

        var invalidTarget = Compile("class Program { public static void Main() { while (true) { break missing; } } }");
        Assert.Contains(invalidTarget.Diagnostics, d => d.Code == "LS0603");

        var duplicate = Compile("class Program { public static void Main() { loop: while (true) { loop: for (;;) { break; } } } }");
        Assert.Contains(duplicate.Diagnostics, d => d.Code == "LS0604");

        var invalidPrefix = Compile("class Program { public static void Main() { label: return; } }");
        Assert.Contains(invalidPrefix.Diagnostics, d => d.Code == "LS0602");
    }

    [Fact]
    public void SupportsTargetTypedDefaultAndUnsafePrimitiveSizeOf()
    {
        var result = Compile("class Program { public static unsafe int32 Main() { int32 zero = default; const int32 width = sizeof(int32); return zero + width - 4; } }", new CompilationOptions(UnsafeEnabled: true));
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("store i32 zeroinitializer", result.LlvmIr!);
        Assert.Contains("store i32 4", result.LlvmIr!);

        var callTarget = Compile("class Program { public static int32 Main() { return Identity(default); } public static int32 Identity(int32 value) { return value; } }");
        Assert.True(callTarget.Succeeded, string.Join(Environment.NewLine, callTarget.Diagnostics));
        Assert.Contains("(i32 zeroinitializer)", callTarget.LlvmIr!);

        var ambiguousDefault = Compile("class Program { public static void Main() { Pick(default); } public static int32 Pick(int32 value) { return value; } public static int32 Pick(double value) { return 0; } }");
        Assert.Contains(ambiguousDefault.Diagnostics, d => d.Code == "LS0750");

        var inferredDefault = Compile("class Program { public static void Main() { var value = default; } }");
        Assert.Contains(inferredDefault.Diagnostics, d => d.Code == "LMC3010" && d.Message.Contains("target type", StringComparison.Ordinal));

        var safeSizeOf = Compile("class Program { public static int32 Main() { return sizeof(int32); } }");
        Assert.Contains(safeSizeOf.Diagnostics, d => d.Code == "LMC3000" && d.Message.Contains("unsafe context", StringComparison.Ordinal));

        var unsupportedSize = Compile("class Program { public static unsafe int32 Main() { return sizeof(void); } }", new CompilationOptions(UnsafeEnabled: true));
        Assert.Contains(unsupportedSize.Diagnostics, d => d.Code == "LMC3000" && d.Message.Contains("sizeof", StringComparison.Ordinal));
    }

    [Fact]
    public void SupportsConstantSwitchSectionsAndSwitchBreaks()
    {
        var result = Compile("class Program { public static int32 Main() { int32 value = 2; switch (value) { case 1: value = 0; break; case 2, 3: value = 42; break; default: value = 9; } return value == 42 ? 0 : 1; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("switch.section", result.LlvmIr!);
        Assert.Contains("switch.test", result.LlvmIr!);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-switch-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try { Assert.True(LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, "host", optimize: false).Succeeded); }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }

        var duplicate = Compile("class Program { public static void Main() { int32 value = 1; switch (value) { case 1: break; case 1: break; } } }");
        Assert.Contains(duplicate.Diagnostics, d => d.Code == "LS0622");
        var outsideBreak = Compile("class Program { public static void Main() { break; } }");
        Assert.Contains(outsideBreak.Diagnostics, d => d.Code == "LS0605");
        const string guardedSource = "class Program { public static int32 Main() { int32 value = 2; int32 result = 0; switch (value) { case 1, 2 when value > 2: result = 1; case 2 when value == 2: result = 42; default: result = 9; } return result == 42 ? 0 : 1; } }";
        var guarded = Compile(guardedSource);
        Assert.True(guarded.Succeeded, string.Join(Environment.NewLine, guarded.Diagnostics));
        var guardedExecution = LlvmJit.RunMain(guarded.LlvmIr!);
        Assert.Equal(0, guardedExecution.ExitCode);
        Assert.Equal(0, guardedExecution.OutstandingAllocations);
        var invalidGuard = Compile("class Program { public static void Main() { int32 value = 1; switch (value) { case 1 when 2: break; } } }");
        Assert.Contains(invalidGuard.Diagnostics, d => d.Code == "LMC3010" && d.Message.Contains("guard must have type 'bool'", StringComparison.Ordinal));
        const string nullableSwitchSource = "class Program { public static int32 Main() { string? value = null; switch (value) { case not null: return 1; case null: return 0; } return 2; } }";
        var nullableSwitch = Compile(nullableSwitchSource);
        Assert.True(nullableSwitch.Succeeded, string.Join(Environment.NewLine, nullableSwitch.Diagnostics));
        var nullableSwitchExecution = LlvmJit.RunMain(nullableSwitch.LlvmIr!);
        Assert.Equal(0, nullableSwitchExecution.ExitCode);
        Assert.Equal(0, nullableSwitchExecution.OutstandingAllocations);
        var nullableValueSwitch = Compile("class Program { public static int32 Main() { int32? value = 3; switch (value) { case 3: return 0; case null: return 1; } return 2; } }");
        Assert.True(nullableValueSwitch.Succeeded, string.Join(Environment.NewLine, nullableValueSwitch.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(nullableValueSwitch.LlvmIr!).ExitCode);
        var nullableClassSwitch = Compile("class Node { } class Program { public static int32 Main() { Node? value = null; switch (value) { case null: return 0; case not null: return 1; } return 2; } }");
        Assert.True(nullableClassSwitch.Succeeded, string.Join(Environment.NewLine, nullableClassSwitch.Diagnostics));
        var nullableClassExecution = LlvmJit.RunMain(nullableClassSwitch.LlvmIr!);
        Assert.Equal(0, nullableClassExecution.ExitCode);
        Assert.Equal(0, nullableClassExecution.OutstandingAllocations);
        var duplicateNull = Compile("class Program { public static void Main() { string? value = null; switch (value) { case null: break; case null: break; } } }");
        Assert.Contains(duplicateNull.Diagnostics, d => d.Code == "LS0622" && d.Message.Contains("duplicates", StringComparison.Ordinal));
        var discardSwitch = Compile("class Program { public static int32 Main() { int32 value = 42; switch (value) { case 1: return 1; case _: return 0; } } }");
        Assert.True(discardSwitch.Succeeded, string.Join(Environment.NewLine, discardSwitch.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(discardSwitch.LlvmIr!).ExitCode);
        var subsumed = Compile("class Program { public static void Main() { int32 value = 0; switch (value) { case _: break; case 1: break; } } }");
        Assert.Contains(subsumed.Diagnostics, d => d.Code == "LS0622" && d.Message.Contains("catch-all", StringComparison.Ordinal));
        var emptySection = Compile("class Program { public static void Main() { int32 value = 1; switch (value) { case 1: } } }");
        Assert.Contains(emptySection.Diagnostics, d => d.Code == "LS0620");
    }

    [Fact]
    public void RecognizesExhaustiveBooleanAndEnumSwitchesForReturnAnalysis()
    {
        const string source = """
            enum Mode { Idle, Busy, BusyAlias = Busy }
            class Program {
                public static int32 Main() {
                    return Select(false) == 0 && Select(true) == 1 && SelectNullable(null) == 0 && SelectNullable(true) == 1 &&
                        SelectNullableByNull(false) == 1 && SelectMode(Mode.Idle) == 2 && SelectMode(Mode.Busy) == 3 &&
                        SelectNullableMode(null) == 0 && SelectNullableMode(Mode.Busy) == 3 &&
                        SelectSmall(-1) == 1 && SelectSmall(0) == 2 && SelectUnsigned(255) == 2 &&
                        SelectNullableSmall(null) == 3 && SelectNullableSmall(-1) == 1 && NarrowConstant() == 2 ? 0 : 1;
                }
                public static int32 Select(bool value) { switch (value) { case true: return 1; case false: return 0; } }
                public static int32 SelectNullable(bool? value) { switch (value) { case true: return 1; case false: return 2; case null: return 0; } }
                public static int32 SelectNullableByNull(bool? value) { switch (value) { case not null: return 1; case null: return 0; } }
                public static int32 SelectMode(Mode value) { switch (value) { case Mode.Idle: return 2; case Mode.Busy: return 3; } }
                public static int32 SelectNullableMode(Mode? value) { switch (value) { case Mode.Idle: return 2; case Mode.Busy: return 3; case null: return 0; } }
                public static int32 SelectSmall(int8 value) { switch (value) { case <= -1: return 1; case >= 0: return 2; } }
                public static int32 SelectUnsigned(uint8 value) { switch (value) { case < 128: return 1; case >= 128: return 2; } }
                public static int32 SelectNullableSmall(int8? value) { switch (value) { case <= -1: return 1; case >= 0: return 2; case null: return 3; } }
                public static int32 NarrowConstant() { int8 value = 1 + 1; return value; }
            }
            """;
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);

        var missingBooleanCase = Compile("class Program { public static int32 Select(bool value) { switch (value) { case true: return 1; } } public static int32 Main() { return 0; } }");
        Assert.Contains(missingBooleanCase.Diagnostics, diagnostic => diagnostic.Code == "LS0630");

        var missingEnumCase = Compile("enum Mode { Idle, Busy } class Program { public static int32 Select(Mode value) { switch (value) { case Mode.Idle: return 1; } } public static int32 Main() { return 0; } }");
        Assert.Contains(missingEnumCase.Diagnostics, diagnostic => diagnostic.Code == "LS0630");

        var missingIntegralValue = Compile("class Program { public static int32 Select(int8 value) { switch (value) { case <= -1: return 1; case >= 1: return 2; } } public static int32 Main() { return 0; } }");
        Assert.Contains(missingIntegralValue.Diagnostics, diagnostic => diagnostic.Code == "LS0630");

        var guardedBooleanCase = Compile("class Program { public static int32 Select(bool value) { switch (value) { case true when value: return 1; case false: return 0; } } public static int32 Main() { return 0; } }");
        Assert.Contains(guardedBooleanCase.Diagnostics, diagnostic => diagnostic.Code == "LS0630");

        var duplicateEnumAlias = Compile("enum Mode { Busy, BusyAlias = Busy } class Program { public static void Main() { Mode value = Mode.Busy; switch (value) { case Mode.Busy: break; case Mode.BusyAlias: break; } } }");
        Assert.Contains(duplicateEnumAlias.Diagnostics, diagnostic => diagnostic.Code == "LS0622");

        var unreachableDefault = Compile("class Program { public static int32 Main() { bool value = true; switch (value) { case true: break; case false: break; default: break; } return 0; } }");
        Assert.Contains(unreachableDefault.Diagnostics, diagnostic => diagnostic.Code == "LS0622" && diagnostic.Message.Contains("subsumed", StringComparison.Ordinal));

        var nullableDefaultStillMatchesNull = Compile("class Program { public static int32 Main() { bool? value = true; switch (value) { case true: break; case false: break; default: break; } return 0; } }");
        Assert.DoesNotContain(nullableDefaultStillMatchesNull.Diagnostics, diagnostic => diagnostic.Code == "LS0622");
    }

    [Fact]
    public void ObjectTypePatternsCoverInterfaceReferenceSwitchesAndTheirNullableValues()
    {
        const string source = "interface IRead { int32 Read(); } delegate int32 Getter(); class Number : IRead { public int32 Read() { return 42; } } class Program { public static int32 Main() { IRead value = new Number(); IRead? absent = null; object boxed = value; Getter? captured = CaptureConditional(boxed); if (captured == null || captured!() != 42) return 1; captured = null; if (CaptureConditional(new object()) != null) return 2; return Select(value) == 42 && SelectNullable(value) == 42 && SelectNullable(absent) == 0 && ReadObject(boxed) == 42 && ReadConditional(boxed) == 42 && ReadConditional(new object()) == -1 ? 0 : 3; } public static int32 Select(IRead value) { switch (value) { case object: return 42; } } public static int32 SelectNullable(IRead? value) { switch (value) { case object: return 42; case null: return 0; } } public static int32 ReadObject(object value) { if (value is IRead readable) return readable.Read(); return -1; } public static int32 ReadConditional(object value) { return value is IRead readable ? readable.Read() : -1; } public static Getter? CaptureConditional(object value) { return value is IRead readable ? () => readable.Read() : null; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Code == "LS0630");
        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Code == "LS3101" && diagnostic.Message.Contains("System.Object", StringComparison.Ordinal));

        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var falseBranchLeak = Compile("interface IRead { int32 Read(); } class Program { public static int32 Read(object value) { return value is IRead readable ? 0 : readable.Read(); } public static int32 Main() { return 0; } }");
        Assert.Contains(falseBranchLeak.Diagnostics, diagnostic => diagnostic.Code == "LS0200" && diagnostic.Message.Contains("readable", StringComparison.Ordinal));
    }

    [Fact]
    public void TypeTestConstantWarningsRespectInterfacesAndOpenClassHierarchies()
    {
        const string source = "interface IRoot { } interface IChild : IRoot { } class Empty { } class Program { public static bool InterfaceIsObject(IChild value) { return value is object; } public static bool NullableInterfaceMayBeNull(IChild? value) { return value is object; } public static bool DerivedInterfaceIsBase(IChild value) { return value is IRoot; } public static bool SealedClassCannotImplement(Empty value) { return value is IRoot; } public static bool ObjectIsObject(object value) { return value is object; } public static int32 Main() { return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));

        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS3101" && diagnostic.Message.Contains("always true", StringComparison.Ordinal) && diagnostic.Message.Contains("IChild", StringComparison.Ordinal));
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS3101" && diagnostic.Message.Contains("always false", StringComparison.Ordinal) && diagnostic.Message.Contains("Empty", StringComparison.Ordinal));
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS3101" && diagnostic.Message.Contains("always true", StringComparison.Ordinal) && diagnostic.Message.Contains("System.Object", StringComparison.Ordinal));
        var objectMayImplement = Compile("interface IRoot { } class Program { public static bool Test(object value) { return value is IRoot; } public static int32 Main() { return 0; } }");
        Assert.DoesNotContain(objectMayImplement.Diagnostics, diagnostic => diagnostic.Code == "LS3101");

        var openBaseMayBeDerived = Compile("virtual class Animal { } class Dog : Animal { } class Program { public static bool Test(Animal value) { return value is Dog; } public static int32 Main() { return 0; } }");
        Assert.DoesNotContain(openBaseMayBeDerived.Diagnostics, diagnostic => diagnostic.Code == "LS3101");
    }

    [Fact]
    public void ConditionalExpressionsLiftNullIntoNullableValueAndReferenceResults()
    {
        const string source = "enum Mode { Off, On } struct Point { public int32 X; public this(int32 x) { X = x; } } delegate int32 Getter(); class Program { public static int32 Main() { int32? present = ChooseNumber(true); int32? absent = ChooseNumber(false); int32[]? array = ChooseArray(true); int32[]? noArray = ChooseArray(false); Getter? getter = ChooseGetter(true); Getter? noGetter = ChooseGetter(false); Point? point = ChoosePoint(true); Point? noPoint = ChoosePoint(false); Mode? mode = ChooseMode(true); if (present != 42 || absent != null || array == null || array![0] != 42 || noArray != null || getter == null || getter!() != 42 || noGetter != null || point == null || point!.X != 7 || noPoint != null || mode != Mode.On) return 1; getter = null; array = null; return 0; } public static int32? ChooseNumber(bool choose) { return choose ? 42 : null; } public static int32[]? ChooseArray(bool choose) { int32[] data = new int32[] { 42 }; return choose ? data : null; } public static Getter? ChooseGetter(bool choose) { Getter value = () => 42; return choose ? value : null; } public static Point? ChoosePoint(bool choose) { return choose ? Point(7) : null; } public static Mode? ChooseMode(bool choose) { return choose ? Mode.On : null; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));

        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ConditionalExpressionsPromoteNumericBranchesBeforeTheJoin()
    {
        const string source = "class Program { public static int32 Main() { double floating = SelectFloating(true); double floatCommon = SelectFloatCommon(true); int64 integer = SelectInteger(false); int64? nullable = SelectNullable(false); int64 signedUnsigned = SelectSignedUnsigned(false); return floating == 1.0 && floatCommon == 3.0 && integer == 2L && nullable == 9000000000L && signedUnsigned == 4294967295L ? 0 : 1; } public static double SelectFloating(bool choose) { var result = choose ? 1 : 2.5; return result; } public static double SelectFloatCommon(bool choose) { int32 value = 3; var result = choose ? value : 2.5f; return result; } public static int64 SelectInteger(bool choose) { var result = choose ? 1 : 2L; return result; } public static int64? SelectNullable(bool choose) { int32? small = 5; var result = choose ? small : 9000000000L; return result; } public static int64 SelectSignedUnsigned(bool choose) { var result = choose ? -1 : (uint32)4294967295; return result; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("phi double", result.LlvmIr!);
        Assert.Contains("phi i64", result.LlvmIr);

        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var noCommonType = Compile("class Program { public static int32 Main() { return 0; } public static void Select(bool choose, int64 signed, uint64 unsigned) { var result = choose ? signed : unsigned; } }");
        Assert.Contains(noCommonType.Diagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error &&
            diagnostic.Message.Contains("common supported type", StringComparison.Ordinal));
    }

    [Fact]
    public void ConditionalReferenceResultsPreserveTheirInferredNullability()
    {
        const string source = "class Cell { public int32 Value; public this(int32 value) { Value = value; } } class Program { public static int32 Main() { return Read(true) == 9 && Read(false) == 7 && ReadArray(true) == 9 && ReadArray(false) == 7 ? 0 : 1; } public static int32 Read(bool choose) { try { return (choose ? null : new Cell(7))!.Value; } catch (NullReferenceException) { return 9; } } public static int32 ReadArray(bool choose) { try { return (choose ? null : new int32[] { 7 })![0]; } catch (NullReferenceException) { return 9; } } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));

        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void NullableStructTypePatternsParticipateInSwitchReturnAnalysis()
    {
        const string source = "struct Number { public int32 Value; public this(int32 value) { Value = value; } } class Program { public static int32 Main() { Number? present = Number(42); Number? absent = null; Number direct = Number(7); return Select(present) == 42 && Select(absent) == 0 && SelectDirect(direct) == 7 && SelectNotNull(present) == 42 && SelectNotNull(absent) == 0 ? 0 : 1; } public static int32 Select(Number? value) { switch (value) { case Number number: return number.Value; case null: return 0; } } public static int32 SelectNotNull(Number? value) { switch (value) { case not null: return value!.Value; case null: return 0; } } public static int32 SelectDirect(Number value) { switch (value) { case Number number: return number.Value; } } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var missingNull = Compile("struct Number { public int32 Value; } class Program { public static int32 Select(Number? value) { switch (value) { case Number number: return number.Value; } } public static int32 Main() { return 0; } }");
        Assert.Contains(missingNull.Diagnostics, diagnostic => diagnostic.Code == "LS0630");

        var redundantDefault = Compile("struct Number { public int32 Value; } class Program { public static void Select(Number value) { switch (value) { case Number: break; default: break; } } public static int32 Main() { return 0; } }");
        Assert.Contains(redundantDefault.Diagnostics, diagnostic => diagnostic.Code == "LS0622" && diagnostic.Message.Contains("subsumed", StringComparison.Ordinal));
    }

    [Fact]
    public void SupportsRelationalSwitchPatternsWithNullableValuesAndSubsumptionChecks()
    {
        const string source = """
            class Program {
                public static int32 Main() {
                    if (Classify(-2) != 1 || Classify(0) != 1) return 1;
                    if (Classify(1) != 2 || Classify(9) != 2 || Classify(10) != 3) return 2;
                    if (ClassifyDouble(0.75) != 1 || ClassifyDouble(0.25) != 0) return 3;
                    if (ClassifyNullable(5) != 1 || ClassifyNullable(0) != 0 || ClassifyNullable(null) != 0) return 4;
                    return 0;
                }
                public static int32 Classify(int32 value) {
                    switch (value) {
                        case <= 0: return 1;
                        case > 0 when value < 10: return 2;
                        case >= 10: return 3;
                    }
                    return 4;
                }
                public static int32 ClassifyDouble(double value) {
                    switch (value) { case > 0.5: return 1; default: return 0; }
                }
                public static int32 ClassifyNullable(int32? value) {
                    switch (value) { case > 2: return 1; default: return 0; }
                }
            }
            """;
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var subsumed = Compile("class Program { public static void Main() { int32 value = 0; switch (value) { case > 10: break; case >= 20: break; } } }");
        Assert.Contains(subsumed.Diagnostics, diagnostic => diagnostic.Code == "LS0622" && diagnostic.Message.Contains("relational", StringComparison.Ordinal));

        var guarded = Compile("class Program { public static void Main() { int32 value = 0; switch (value) { case > 10 when false: break; case > 20: break; } } }");
        Assert.True(guarded.Succeeded, string.Join(Environment.NewLine, guarded.Diagnostics));

        var combinedSubsumed = Compile("class Program { public static void Main() { int32 value = 0; switch (value) { case > 0 and < 10: break; case > 2 and < 8: break; } } }");
        Assert.Contains(combinedSubsumed.Diagnostics, diagnostic => diagnostic.Code == "LS0622" && diagnostic.Message.Contains("combined numeric", StringComparison.Ordinal));

        var unionSubsumed = Compile("class Program { public static void Main() { int32 value = 0; switch (value) { case < 5: break; case >= 5 and < 10: break; case >= 0 and < 10: break; } } }");
        Assert.Contains(unionSubsumed.Diagnostics, diagnostic => diagnostic.Code == "LS0622" && diagnostic.Message.Contains("combined numeric", StringComparison.Ordinal));

        var uncoveredPoint = Compile("class Program { public static void Main() { int32 value = 0; switch (value) { case < 5: break; case > 5 and < 10: break; case >= 0 and < 10: break; } } }");
        Assert.DoesNotContain(uncoveredPoint.Diagnostics, diagnostic => diagnostic.Code == "LS0622" && diagnostic.Message.Contains("combined numeric", StringComparison.Ordinal));

        var negatedRangeSubsumes = Compile("class Program { public static void Main() { int32 value = 0; switch (value) { case not 0: break; case 1: break; } } }");
        Assert.Contains(negatedRangeSubsumes.Diagnostics, diagnostic => diagnostic.Code == "LS0622" && diagnostic.Message.Contains("subsumed", StringComparison.Ordinal));

        var negatedRelationalRange = Compile("class Program { public static void Main() { int32 value = 0; switch (value) { case not > 0: break; case -1: break; case 1: break; } } }");
        Assert.Equal(1, negatedRelationalRange.Diagnostics.Count(diagnostic => diagnostic.Code == "LS0622"));

        var invalidType = Compile("class Program { public static void Main() { string value = \"x\"; switch (value) { case > 1: break; } } }");
        Assert.Contains(invalidType.Diagnostics, diagnostic => diagnostic.Code == "LMC3010" && diagnostic.Message.Contains("numeric", StringComparison.Ordinal));
    }

    [Fact]
    public void SupportsCombinedAndNegatedSwitchPatterns()
    {
        const string source = """
            class Program {
                public static int32 Main() {
                    if (InRange(5) != 1 || InRange(0) != 0 || InRange(10) != 0) return 1;
                    if (OneOrTwo(1) != 1 || OneOrTwo(2) != 1 || OneOrTwo(3) != 0) return 2;
                    if (Guarded(1) != 0 || Guarded(2) != 1) return 3;
                    if (Negated(5) != 1 || Negated(0) != 0 || Negated(10) != 0) return 4;
                    return 0;
                }
                public static int32 InRange(int32 value) {
                    switch (value) { case > 0 and < 10: return 1; default: return 0; }
                }
                public static int32 OneOrTwo(int32 value) {
                    switch (value) { case 1 or 2: return 1; default: return 0; }
                }
                public static int32 Guarded(int32 value) {
                    switch (value) { case 1 or 2 when value == 2: return 1; default: return 0; }
                }
                public static int32 Negated(int32 value) {
                    switch (value) { case not 0 and < 10: return 1; default: return 0; }
                }
            }
            """;
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var subsumedAlternative = Compile("class Program { public static void Main() { int32 value = 0; switch (value) { case 1 or 2: break; case 2: break; } } }");
        Assert.Contains(subsumedAlternative.Diagnostics, diagnostic => diagnostic.Code == "LS0622");
    }

    [Fact]
    public void SwitchPatternVariablesAreBoundAcrossAndOrAndSharedSectionAlternatives()
    {
        const string source = "interface IAnimal { int32 Kind(); } interface IPet : IAnimal { } class Dog : IPet { public int32 Kind() { return 2; } } class Cat : IPet { public int32 Kind() { return 3; } } class Rock : IAnimal { public int32 Kind() { return 4; } } class Program { public static int32 Main() { return Classify(new Dog()) == 2 && Classify(new Cat()) == 3 && Classify(new Rock()) == 4 && Classify(null) == 0 && ReadBoth(new Dog()) == 4 ? 0 : 1; } public static int32 Classify(IAnimal? value) { switch (value) { case IPet pet and Dog or IPet pet and Cat when pet.Kind() == 3: return pet.Kind(); case IPet pet and Dog, IPet pet and Cat: return pet.Kind(); case IAnimal animal: return animal.Kind(); case null: return 0; } } public static int32 ReadBoth(IAnimal value) { switch (value) { case IPet pet and Dog dog when pet.Kind() == dog.Kind(): return pet.Kind() + dog.Kind(); case IAnimal animal: return animal.Kind(); } } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void BoxedStructSwitchPatternBindingsReleaseManagedFieldsAtSwitchExit()
    {
        const string source = "class Child { public static int32 Finalized; public this() { } ~this() { Child.Finalized += 1; } } struct Carrier { public Child? child; } class Program { public static int32 Main() { Carrier carrier = default; carrier.child = new Child(); object? boxed = carrier; carrier.child = null; switch (boxed) { case Carrier rejected when rejected.child == null: Child.Finalized += 10; break; case Carrier copy: if (copy.child == null) return 1; break; default: return 2; } if (Child.Finalized != 0) return 3; boxed = null; return Child.Finalized == 1 ? 0 : 4; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void SupportsClassTypeAndDeclarationSwitchPatterns()
    {
        const string source = """
            virtual class Animal { public this() { } }
            class Dog : Animal { public int32 Code() { return 0; } }
            class Cat : Animal { }
            class Program {
                public static int32 Main() {
                    Animal? dog = new Dog();
                    Animal cat = new Cat();
                    Animal? other = new Animal();
                    Animal? none = null;
                    if (Classify(dog) != 2) return 1;
                    if (Classify(cat) != 3) return 2;
                    if (Classify(other) != 4) return 3;
                    if (Classify(none) != 0) return 4;
                    if (All(new Dog()) != 10 || AllNullable(null) != 0 || AllNullable(new Cat()) != 10) return 5;
                    return 0;
                }
                public static int32 Classify(Animal? value) {
                    switch (value) {
                        case Dog dog when dog.Code() > 0: return 1;
                        case Dog dog: return 2;
                        case Cat: return 3;
                        case null: return 0;
                        case Animal: return 4;
                    }
                    return 5;
                }
                public static int32 All(Animal value) { switch (value) { case Animal: return 10; } }
                public static int32 AllNullable(Animal? value) { switch (value) { case Animal: return 10; case null: return 0; } }
            }
            """;
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("call i1 @luma_class_is_instance", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        const string interfacePatternSource = "interface IAnimal { int32 Kind(); } interface IPet : IAnimal { } class Dog : IPet { public int32 Kind() { return 2; } } class Rock : IAnimal { public int32 Kind() { return 1; } } class Program { public static int32 Main() { IAnimal? dog = new Dog(); IAnimal rock = new Rock(); return Classify(dog) == 2 && Classify(rock) == 1 && Classify(null) == 0 && All(rock) == 1 ? 0 : 1; } public static int32 Classify(IAnimal? value) { switch (value) { case IPet pet and IAnimal when pet.Kind() == 2: return pet.Kind(); case IAnimal animal: return animal.Kind(); case null: return 0; } } public static int32 All(IAnimal value) { switch (value) { case IAnimal animal: return animal.Kind(); } } }";
        var interfacePatterns = Compile(interfacePatternSource);
        Assert.True(interfacePatterns.Succeeded, string.Join(Environment.NewLine, interfacePatterns.Diagnostics));
        Assert.Contains("call i1 @luma_class_implements", interfacePatterns.LlvmIr!, StringComparison.Ordinal);
        var interfacePatternExecution = LlvmJit.RunMain(interfacePatterns.LlvmIr!);
        Assert.Equal(0, interfacePatternExecution.ExitCode);
        Assert.Equal(0, interfacePatternExecution.OutstandingAllocations);
        Assert.Null(interfacePatternExecution.InteropError);

        var subsumed = Compile("virtual class Animal { } class Dog : Animal { } class Program { public static void Main() { Animal value = new Dog(); switch (value) { case Animal: break; case Dog: break; } } }");
        Assert.Contains(subsumed.Diagnostics, diagnostic => diagnostic.Code == "LS0622" && diagnostic.Message.Contains("subsumed", StringComparison.Ordinal));

        var subsumedInterface = Compile("interface IAnimal { } interface IPet : IAnimal { } class Program { public static void Main() { IAnimal value = null!; switch (value) { case IAnimal: break; case IPet: break; } } }");
        Assert.Contains(subsumedInterface.Diagnostics, diagnostic => diagnostic.Code == "LS0622" && diagnostic.Message.Contains("subsumed", StringComparison.Ordinal));

        var invalidGoverningType = Compile("class Dog { } class Program { public static void Main() { int32 value = 1; switch (value) { case Dog: break; } } }");
        Assert.Contains(invalidGoverningType.Diagnostics, diagnostic => diagnostic.Code == "LMC3010" && diagnostic.Message.Contains("class-reference", StringComparison.Ordinal));

        var incompatibleValuePattern = Compile("class Program { public static void Main() { int32 value = 1; switch (value) { case int64: break; } } }");
        Assert.Contains(incompatibleValuePattern.Diagnostics, diagnostic => diagnostic.Code == "LS0622" && diagnostic.Message.Contains("cannot match", StringComparison.Ordinal));

        var sharedDeclarationSection = Compile("class Dog { } class Program { public static void Main() { Dog value = new Dog(); switch (value) { case Dog dog, null: break; } } }");
        Assert.Contains(sharedDeclarationSection.Diagnostics, diagnostic => diagnostic.Code == "LMC3010" && diagnostic.Message.Contains("every label", StringComparison.Ordinal));

        var unsupportedAlternativeDeclarations = Compile("virtual class Animal { } class Dog : Animal { } class Cat : Animal { } class Program { public static void Main() { Animal value = new Dog(); switch (value) { case Dog pet or Cat pet: break; } } }");
        Assert.Contains(unsupportedAlternativeDeclarations.Diagnostics, diagnostic => diagnostic.Code == "LMC3010" && diagnostic.Message.Contains("same names and types", StringComparison.Ordinal));

        var incompleteTypeSwitch = Compile("virtual class Animal { } class Dog : Animal { } class Program { public static int32 Select(Animal value) { switch (value) { case Dog: return 1; } } public static int32 Main() { return 0; } }");
        Assert.Contains(incompleteTypeSwitch.Diagnostics, diagnostic => diagnostic.Code == "LS0630" && diagnostic.Message.Contains("Not all code paths", StringComparison.Ordinal));
    }

    [Fact]
    public void SupportsPrimitiveAndEnumSwitchTypePatterns()
    {
        const string source = "enum Shade : uint8 { Red = 1, Blue = 2 } class Program { public static int32 Main() { object boxed = 42; object boxedEnum = Shade.Blue; object? missing = null; int32? present = 7; int32? empty = null; Shade shade = Shade.Red; int32 direct = 9; return Classify(boxed) == 42 && Classify(boxedEnum) == 2 && Classify(missing) == 0 && Classify(new object()) == -2 && Select(present) == 7 && Select(empty) == 0 && SelectEnum(shade) == 1 && SelectDirect(direct) == 9 ? 0 : 1; } public static int32 Classify(object? value) { switch (value) { case int32 number when number > 0: return number; case Shade color: return (int32)color; case null: return 0; default: return -2; } } public static int32 Select(int32? value) { switch (value) { case int32 actual: return actual; case null: return 0; } } public static int32 SelectEnum(Shade value) { switch (value) { case Shade color: return (int32)color; } } public static int32 SelectDirect(int32 value) { switch (value) { case int32 number: return number; } } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("@luma_boxdesc_int32", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("@luma_boxdesc_Shade", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var stringPrimitivePattern = Compile("class Program { public static int32 Main() { string value = \"text\"; switch (value) { case int32: return 1; default: return 0; } } }");
        Assert.True(stringPrimitivePattern.Succeeded, string.Join(Environment.NewLine, stringPrimitivePattern.Diagnostics));
        var stringPatternExecution = LlvmJit.RunMain(stringPrimitivePattern.LlvmIr!);
        Assert.Equal(0, stringPatternExecution.ExitCode);
        Assert.Equal(0, stringPatternExecution.OutstandingAllocations);
    }

    [Fact]
    public void SupportsIsTypePatternsAndScopedVariablesInIfStatements()
    {
        const string source = "virtual class Animal { } class Dog : Animal { public int32 Code() { return 7; } } class Program { public static int32 Main() { object boxed = 42; Animal? animal = new Dog(); object unrelated = new object(); string? text = \"hello\"; string? empty = null; bool skip = false; int32 visits = 0; int32 forVisits = 0; if (boxed is int32 number) { if (number != 42) return 1; } else return 2; if (boxed is int32 filtered && filtered > 40) { if (filtered != 42) return 3; } else return 4; if (animal is Dog dog) { if (dog.Code() != 7) return 5; } else return 6; if (animal is Dog matchingDog && matchingDog.Code() == 7) { if (matchingDog.Code() != 7) return 7; } else return 8; if (unrelated is int32 notNumber) return 9; if (animal is Program wrongType) return 10; if (text is string content) { if (content != \"hello\") return 11; } else return 12; if (skip && animal is Dog skippedDog) return 13; if (empty is string absent) return 14; while (boxed is int32 loopValue && visits < 1) { if (loopValue != 42) return 15; visits++; } if (visits != 1) return 16; for (; boxed is int32 forValue && forVisits < 1; forVisits++) { if (forValue != 42) return 17; } return forVisits == 1 ? 0 : 18; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var duplicate = Compile("class Program { public static void Main() { int32 value = 0; object boxed = value; int32 item = 1; if (boxed is int32 item) { } } }");
        Assert.Contains(duplicate.Diagnostics, diagnostic => diagnostic.Code == "LS0201" && diagnostic.Message.Contains("Pattern variable 'item'", StringComparison.Ordinal));
    }

    [Fact]
    public void SupportsDelegateTypeSwitchPatterns()
    {
        const string source = "delegate int32 Callback(); class Program { public static int32 Main() { Callback callback = Answer; Callback? maybe = callback; return Invoke(maybe) == 42 && Invoke(null) == 0 ? 0 : 1; } public static int32 Answer() { return 42; } public static int32 Invoke(Callback? value) { switch (value) { case Callback callback: return callback(); case null: return 0; } } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("@luma_class_is_instance", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var missingNull = Compile("delegate int32 Callback(); class Program { public static int32 Select(Callback? value) { switch (value) { case Callback callback: return callback(); } } public static int32 Main() { return 0; } }");
        Assert.Contains(missingNull.Diagnostics, diagnostic => diagnostic.Code == "LS0630");

        var impossibleCrossCategoryPattern = Compile("delegate int32 Callback(); virtual class Animal { } class Program { public static void Main() { Animal animal = null!; switch (animal) { case Callback callback: break; } } }");
        Assert.Contains(impossibleCrossCategoryPattern.Diagnostics, diagnostic => diagnostic.Code == "LS0622" && diagnostic.Message.Contains("cannot match", StringComparison.Ordinal));
    }

    [Fact]
    public void SupportsStringTypeSwitchPatterns()
    {
        const string source = "class Program { public static int32 Main() { string? value = \"hi\"; return Length(value) == 2 && Length(null) == 0 && All(\"x\") == 1 ? 0 : 1; } public static int32 Length(string? value) { switch (value) { case string text: return text.Length; case null: return 0; } } public static int32 All(string value) { switch (value) { case string text: return text.Length; } } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var subsumed = Compile("class Program { public static void Main() { string value = \"x\"; switch (value) { case string text: break; case \"x\": break; } } }");
        Assert.Contains(subsumed.Diagnostics, diagnostic => diagnostic.Code == "LS0622" && diagnostic.Message.Contains("subsumed", StringComparison.Ordinal));
    }

    [Fact]
    public void SupportsAbstractVirtualMembersAndRejectsIncompleteTypes()
    {
        const string source = "virtual class Shape { public virtual int32 Area(); public virtual int32 Dimensions() { return 2; } } class Square : Shape { public override int32 Area() { return 25; } } class Program { public static int32 Main() { Shape shape = new Square(); return shape.Area() == 25 && shape.Dimensions() == 2 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var abstractAllocation = Compile("virtual class Shape { public virtual int32 Area(); } class Program { public static void Main() { Shape shape = new Shape(); } }");
        Assert.Contains(abstractAllocation.Diagnostics, diagnostic => diagnostic.Code == "LS0834");

        var missingOverride = Compile("virtual class Shape { public virtual int32 Area(); } class Square : Shape { } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(missingOverride.Diagnostics, diagnostic => diagnostic.Code == "LS0835");

        var bodylessConcreteMember = Compile("virtual class Shape { public int32 Area(); } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(bodylessConcreteMember.Diagnostics, diagnostic => diagnostic.Code == "LMC3000" && diagnostic.Message.Contains("Bodyless methods", StringComparison.Ordinal));
    }

    [Fact]
    public void SupportsDirectBaseMemberAccessAndCalls()
    {
        const string source = """
            virtual class Parent {
                public int32 Value = 4;
                public virtual int32 Read() { return Value; }
            }
            class Child : Parent {
                public override int32 Read() mut {
                    base.Value += 1;
                    return base.Read() + 1;
                }
            }
            class Program {
                public static int32 Main() {
                    Child child = new Child();
                    Parent parent = child;
                    if (parent.Read() != 6 || child.Value != 5) return 1;
                    return 0;
                }
            }
            """;
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var rootBase = Compile("class Root { public int32 Value() { return 1; } public int32 Call() { return base.Value(); } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(rootBase.Diagnostics, diagnostic => diagnostic.Code == "LS0200" && diagnostic.Message.Contains("System.Object", StringComparison.Ordinal));

        var staticBase = Compile("class Root { public static int32 Call() { return base.Value(); } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(staticBase.Diagnostics, diagnostic => diagnostic.Code == "LMC3000" && diagnostic.Message.Contains("inside class constructors and instance methods", StringComparison.Ordinal));

        var abstractBaseCall = Compile("virtual class Shape { public virtual int32 Area(); } class Square : Shape { public override int32 Area() { return base.Area(); } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(abstractBaseCall.Diagnostics, diagnostic => diagnostic.Code == "LS0836");
    }

    [Fact]
    public void SupportsAutoPropertiesThroughClassAndStructStorage()
    {
        const string source = "virtual class Entity { public int32 Id { get; set; } = 7; public string Name { get; set; } = \"new\"; public int32 Kind { get; } = 1; } class Child : Entity { public int32 Extra { get; set; } } struct Point { public int32 X { get; set; } public int32 Y { get; } } class Program { public static int32 Shared { get; set; } = 3; public static int32 Main() { Child child = new Child(); if (child.Id != 7 || child.Name != \"new\" || child.Kind != 1 || Program.Shared != 3) return 1; child.Id = 42; child.Name = \"updated\"; child.Extra = 5; Program.Shared = 11; if (child.Id != 42 || child.Name != \"updated\" || child.Extra != 5 || Program.Shared != 11) return 2; Point point = default; point.X = 9; if (point.X != 9 || point.Y != 0) return 3; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var readonlyWrite = Compile("class Item { public int32 Id { get; } } class Program { public static void Main() { Item item = new Item(); item.Id = 3; } }");
        Assert.Contains(readonlyWrite.Diagnostics, diagnostic => diagnostic.Code == "LMC3012");

        var computed = Compile("class Item { public int32 Id => 42; } class Program { public static int32 Main() { Item item = new Item(); return item.Id == 42 ? 0 : 1; } }");
        Assert.True(computed.Succeeded, string.Join(Environment.NewLine, computed.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(computed.LlvmIr!).ExitCode);

        var explicitAccessor = Compile("class Item { int32 stored; public int32 Id { get { return stored; } set { stored = value; } } } class Program { public static int32 Main() { Item item = new Item(); item.Id = 42; return item.Id == 42 ? 0 : 1; } }");
        Assert.True(explicitAccessor.Succeeded, string.Join(Environment.NewLine, explicitAccessor.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(explicitAccessor.LlvmIr!).ExitCode);
    }

    [Fact]
    public void AutoPropertySettersSatisfyNonNullableClassFieldInitialization()
    {
        const string source = "class Node { } class Box { public Node Value { get; set; } public this(Node value) { Value = value; } } class Program { public static int32 Main() { Box box = new Box(new Node()); return box.Value == null ? 1 : 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void SupportsCompileTimeInitializedStaticReadonlyStorage()
    {
        const string source = "class Program { public static readonly int32 Field = 42; public static int32 Property { get; } = 17; public static int32 Main() { return Program.Field == 42 && Program.Property == 17 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("@luma_global_Program_Field = constant i32 42", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Null(execution.InteropError);

        var write = Compile("class Program { public static readonly int32 Field = 42; public static void Main() { Program.Field = 7; } }");
        Assert.Contains(write.Diagnostics, diagnostic => diagnostic.Code is "LS0231" or "LMC3012");

        var missingInitializer = Compile("class Program { public static readonly int32 Field; public static void Main() { } }");
        Assert.Contains(missingInitializer.Diagnostics, diagnostic => diagnostic.Code == "LMC3000" && diagnostic.Message.Contains("compile-time initializer", StringComparison.Ordinal));
    }

    [Fact]
    public void RuntimeStaticInitializationIsLazyOrderedAndRunsOnce()
    {
        const string source = """
            class Recorder { public static int32 Count; }
            class Counter {
                public static int32 Value = Compute();
                public static readonly int32 Captured;
                static this() { Value += 10; Captured = Recorder.Count; Recorder.Count += 1; }
                public static int32 Compute() { Recorder.Count += 1; return Recorder.Count; }
                public static int32 Read() { return Value * 100 + Captured * 10 + Recorder.Count; }
            }
            class Program {
                public static int32 Main() {
                    int32 first = Counter.Read();
                    int32 second = Counter.Read();
                    return first == 1112 && second == 1112 ? 0 : 1;
                }
            }
            """;
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("@luma_type_init_Counter", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void StaticConstructorDefiniteAssignmentAcceptsEveryBranchAndRejectsMissingPaths()
    {
        const string validSource = "class Node { } class Settings { static int32 Flag; public static readonly int32 Value; public static readonly Node Instance; static this() { if (Flag == 0) { Value = 42; Instance = new Node(); } else { Value = 0; Instance = new Node(); } } } class Program { public static int32 Main() { return Settings.Value == 42 ? (Settings.Instance == null ? 3 : 0) : 2; } }";
        var valid = Compile(validSource);
        Assert.True(valid.Succeeded, string.Join(Environment.NewLine, valid.Diagnostics));
        var execution = LlvmJit.RunMain(valid.LlvmIr!);
        Assert.True(execution.ExitCode == 0, $"Unexpected exit code {execution.ExitCode}; stderr: {LlvmJit.CapturedStderr}");
        Assert.Equal(1, execution.OutstandingAllocations);

        var constantBranch = Compile("class Settings { public static readonly int32 Value; static this() { if (true) Value = 42; } } class Program { public static int32 Main() { return Settings.Value == 42 ? 0 : 1; } }");
        Assert.True(constantBranch.Succeeded, string.Join(Environment.NewLine, constantBranch.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(constantBranch.LlvmIr!).ExitCode);

        var missingReferencePath = Compile("class Node { } class Settings { static int32 Flag; public static Node Instance; static this() { if (Flag == 0) Instance = new Node(); } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(missingReferencePath.Diagnostics, diagnostic => diagnostic.Code == "LS0810");

        var missingReadonlyPath = Compile("class Settings { static int32 Flag; public static readonly int32 Value; static this() { if (Flag == 0) Value = 42; } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(missingReadonlyPath.Diagnostics, diagnostic => diagnostic.Code == "LMC3000" && diagnostic.Message.Contains("static readonly", StringComparison.Ordinal));
    }

    [Fact]
    public void StaticConstructorDefiniteAssignmentTracksLoopExitPaths()
    {
        foreach (var source in new[]
        {
            "class Settings { public static readonly int32 Value; static this() { do { Value = 42; } while (false); } } class Program { public static int32 Main() { return Settings.Value == 42 ? 0 : 1; } }",
            "class Settings { public static readonly int32 Value; static this() { while (true) { Value = 42; break; } } } class Program { public static int32 Main() { return Settings.Value == 42 ? 0 : 1; } }",
            "class Settings { public static readonly int32 Value; static this() { for (;;) { Value = 42; break; } } } class Program { public static int32 Main() { return Settings.Value == 42 ? 0 : 1; } }",
            "class Settings { public static readonly int32 Value; static this() { outer: while (true) { for (;;) { Value = 42; break outer; } } } } class Program { public static int32 Main() { return Settings.Value == 42 ? 0 : 1; } }",
            "class Settings { public static readonly int32 Value; static this() { for (Value = 42; true;) { break; } } } class Program { public static int32 Main() { return Settings.Value == 42 ? 0 : 1; } }",
            "class Settings { public static readonly int32 Value; static this() { while (true) { try { break; } finally { Value = 42; } } } } class Program { public static int32 Main() { return Settings.Value == 42 ? 0 : 1; } }"
        })
        {
            var result = Compile(source);
            Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
            Assert.Equal(0, LlvmJit.RunMain(result.LlvmIr!).ExitCode);
        }

        var breakBeforeAssignment = Compile("class Settings { static int32 Flag; public static readonly int32 Value; static this() { while (true) { if (Flag == 0) break; Value = 42; } } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(breakBeforeAssignment.Diagnostics, diagnostic => diagnostic.Code == "LMC3000" && diagnostic.Message.Contains("static readonly", StringComparison.Ordinal));

        var zeroIteration = Compile("class Settings { public static readonly int32 Value; static this() { while (false) { Value = 42; } } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(zeroIteration.Diagnostics, diagnostic => diagnostic.Code == "LMC3000" && diagnostic.Message.Contains("static readonly", StringComparison.Ordinal));

        var nestedLabeledBreakBeforeAssignment = Compile("class Settings { public static readonly int32 Value; static this() { outer: while (true) { for (;;) { break outer; } Value = 42; } } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(nestedLabeledBreakBeforeAssignment.Diagnostics, diagnostic => diagnostic.Code == "LMC3000" && diagnostic.Message.Contains("static readonly", StringComparison.Ordinal));

        var infiniteConstructor = Compile("class Settings { public static readonly int32 Value; static this() { while (true) { Value = 42; continue; } } } class Program { public static int32 Main() { return 0; } }");
        Assert.True(infiniteConstructor.Succeeded, string.Join(Environment.NewLine, infiniteConstructor.Diagnostics));
    }

    [Fact]
    public void StaticInitializationFailuresAreWrappedCachedAndCatchable()
    {
        const string source = """
            class Broken {
                public static int32 Value = Fail();
                public static int32 Fail() { throw new System.Exception("boom"); }
            }
            class Program {
                public static int32 Main() {
                    System.TypeInitializationException? first = null;
                    try { int32 value = Broken.Value; } catch (System.TypeInitializationException error) { first = error; }
                    System.TypeInitializationException? second = null;
                    try { int32 value = Broken.Value; } catch (System.TypeInitializationException error) { second = error; }
                    return first == null ? 1 : first != second ? 2 : first.InnerException == null ? 3 : first.InnerException.Message != "boom" ? 4 : 0;
                }
            }
            """;
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void CrossTypeStaticInitializationCyclesThrowTypeInitializationException()
    {
        const string source = """
            class First {
                public static int32 Value = Second.Read();
                public static int32 Read() { return First.Value; }
            }
            class Second {
                public static int32 Value = First.Read();
                public static int32 Read() { return Second.Value; }
            }
            class Program {
                public static int32 Main() {
                    try { int32 value = First.Value; }
                    catch (System.TypeInitializationException error) {
                        return error.InnerException == null ? 1 : 0;
                    }
                    return 2;
                }
            }
            """;
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void StaticInitializationRunsBaseFirstAndBeforeClassOrStructConstruction()
    {
        const string source = """
            class Events {
                public static int32 Order;
                public static int32 Mark(int32 value) { Order = Order * 10 + value; return Order; }
            }
            virtual class Base { public static int32 Value = Events.Mark(1); }
            class Derived : Base { public static int32 Value = Events.Mark(2); }
            class Box {
                public static int32 Value = Compute();
                public static int32 Compute() { return 17; }
                public this() { }
            }
            struct Marker {
                public static int32 Value = Compute();
                public int32 Data;
                public static int32 Compute() { return 25; }
                public this(int32 data) { this.Data = data; }
            }
            class Program {
                public static int32 Main() {
                    Box box = new Box();
                    Marker marker = Marker(7);
                    return Derived.Value == 12 && Base.Value == 1 && Box.Value == 17 &&
                        Marker.Value == 25 && marker.Data == 7 ? 0 : 1;
                }
            }
            """;
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void StaticClassReferenceInitializersAreOwnedForProgramLifetime()
    {
        const string source = """
            class Node {
                public int32 Value;
                public this(int32 value) { this.Value = value; }
            }
            class Cache {
                public static readonly Node Value;
                static this() { Value = new Node(42); }
            }
            class Program {
                public static int32 Main() { return Cache.Value.Value == 42 ? 0 : 1; }
            }
            """;
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(1, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void EntryTypeInitializationFailureStopsBeforeMain()
    {
        const string source = "class Program { public static int32 Value = Fail(); public static int32 Fail() { throw new System.Exception(\"entry boom\"); } public static int32 Main() { return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(70, execution.ExitCode);
        Assert.Contains("Unhandled exception: System.TypeInitializationException", LlvmJit.CapturedStderr, StringComparison.Ordinal);
        Assert.Contains("InnerException: System.Exception: entry boom", LlvmJit.CapturedStderr, StringComparison.Ordinal);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void EntryTypeInitializationFailureReleasesProcessArguments()
    {
        const string source = "class Program { public static int32 Value = Fail(); public static int32 Fail() { throw new System.Exception(\"entry boom\"); } public static int32 Main(string[] args) { return args.Length; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var initializationFailureBlock = result.LlvmIr!.Split("entry.initialization.failure:", StringSplitOptions.None).ElementAtOrDefault(1)?
            .Split("entry.call:", StringSplitOptions.None).FirstOrDefault();
        Assert.NotNull(initializationFailureBlock);
        Assert.Contains("call void @luma_array_release(ptr %luma.arguments)", initializationFailureBlock!, StringComparison.Ordinal);

        if (OperatingSystem.IsWindows())
        {
            // MCJIT on Windows can abort while relocating a function that combines Win32
            // command-line imports with the exception TLS model; verify native object emission here.
            var target = LlvmToolchain.GetHostTargetTriple() ?? "host";
            var targetStatus = LlvmToolchain.ValidateTarget(target);
            Assert.True(targetStatus.Succeeded, targetStatus.Error);
            var temporaryDirectory = Path.Combine(Path.GetTempPath(), "lumasharp-entry-cleanup-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temporaryDirectory);
            try
            {
                var emitted = LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, Path.Combine(temporaryDirectory, "entry.obj"), target);
                Assert.True(emitted.Succeeded, emitted.Error);
            }
            finally
            {
                Directory.Delete(temporaryDirectory, recursive: true);
            }
        }
        else
        {
            var withoutArguments = LlvmJit.RunMain(result.LlvmIr!);
            var withArguments = LlvmJit.RunMain(result.LlvmIr!, arguments: ["Aé語🌙"]);
            Assert.Equal(70, withoutArguments.ExitCode);
            Assert.Equal(70, withArguments.ExitCode);
            Assert.Equal(withoutArguments.OutstandingAllocations, withArguments.OutstandingAllocations);
            Assert.Null(withArguments.InteropError);
        }
    }

    [Fact]
    public void UnsupportedStaticReferenceCasesReceiveSpecificDiagnostics()
    {
        var missingInitializer = Compile("class Node { } class Cache { public static Node Value; } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(missingInitializer.Diagnostics, diagnostic => diagnostic.Code == "LS0810");

        var interfaceStorage = Compile("interface I { } class Cache { public static I? Value; } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(interfaceStorage.Diagnostics, diagnostic => diagnostic.Code == "LMC3000" && diagnostic.Message.Contains("Static interface field", StringComparison.Ordinal));
    }

    [Fact]
    public void StaticStringAndArrayInitializersRunThroughTheSharedTypeInitializer()
    {
        const string source = """
            class Cache {
                public static string Text = MakeText();
                public static int32[] Values = MakeValues();
                public static string MakeText() { return "Luma" + "Sharp"; }
                public static int32[] MakeValues() { return new int32[2]; }
                static this() { Values[0] = Text.Length; }
            }
            class Program {
                public static int32 Main() {
                    return Cache.Text.Length == 9 && Cache.Values[0] == 9 && Cache.Values[1] == 0 ? 0 : 1;
                }
            }
            """;
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void SupportsStructBinaryOperatorOverloads()
    {
        const string source = """
            struct Number {
                public int32 Value;
                public this(int32 value) { this.Value = value; }
                public static Number operator +(Number left, Number right) => Number(left.Value + right.Value);
                public static Number operator -(Number left, Number right) => Number(left.Value - right.Value);
                public static Number operator *(Number left, Number right) => Number(left.Value * right.Value);
                public static Number operator /(Number left, Number right) => Number(left.Value / right.Value);
                public static Number operator %(Number left, Number right) => Number(left.Value % right.Value);
                public static Number operator &(Number left, Number right) => Number(left.Value & right.Value);
                public static Number operator |(Number left, Number right) => Number(left.Value | right.Value);
                public static Number operator ^(Number left, Number right) => Number(left.Value ^ right.Value);
                public static Number operator <<(Number left, Number right) => Number(left.Value << right.Value);
                public static Number operator >>(Number left, Number right) => Number(left.Value >> right.Value);
                public static bool operator ==(Number left, Number right) => left.Value == right.Value;
                public static bool operator <(Number left, Number right) => left.Value < right.Value;
                public static bool operator >(Number left, Number right) => left.Value > right.Value;
                public static bool operator <=(Number left, Number right) => left.Value <= right.Value;
                public static bool operator >=(Number left, Number right) => left.Value >= right.Value;
            }
            class Program {
                public static int32 Main() {
                    Number twelve = Number(12);
                    Number five = Number(5);
                    if ((twelve + five).Value != 17 || (twelve - five).Value != 7 || (twelve * five).Value != 60) return 1;
                    if ((twelve / five).Value != 2 || (twelve % five).Value != 2) return 2;
                    if ((twelve & five).Value != 4 || (twelve | five).Value != 13 || (twelve ^ five).Value != 9) return 3;
                    if ((twelve << Number(1)).Value != 24 || (twelve >> Number(1)).Value != 6) return 4;
                    if (!(twelve > five) || !(twelve >= five) || !(five < twelve) || five <= Number(4)) return 5;
                    if (twelve != Number(12) || twelve == five) return 6;
                    Number compound = Number(12);
                    compound += Number(5);
                    compound -= Number(2);
                    compound *= Number(2);
                    compound /= Number(3);
                    compound %= Number(4);
                    compound &= Number(3);
                    compound |= Number(8);
                    compound ^= Number(3);
                    compound <<= Number(1);
                    compound >>= Number(1);
                    if (compound.Value != 9) return 7;
                    return 0;
                }
            }
            """;
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("call %struct.Number @luma_Number__operator_add_2", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Null(execution.InteropError);

        var invalidSignature = Compile("struct Value { public Value operator +(Value left, Value right) => left; } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(invalidSignature.Diagnostics, diagnostic => diagnostic.Code == "LMC3010" && diagnostic.Message.Contains("public static", StringComparison.Ordinal));

        var invalidNotEqual = Compile("struct Value { public static bool operator !=(Value left, Value right) => true; } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(invalidNotEqual.Diagnostics, diagnostic => diagnostic.Code == "LS0401");
    }

    [Fact]
    public void SupportsStructUnaryOperatorOverloads()
    {
        const string source = """
            struct Counter {
                public int32 Value;
                public this(int32 value) { this.Value = value; }
                public static Counter operator +(Counter value) => Counter(value.Value);
                public static Counter operator -(Counter value) => Counter(-value.Value);
                public static Counter operator ~(Counter value) => Counter(~value.Value);
                public static bool operator !(Counter value) => value.Value == 0;
                public static Counter operator ++(Counter value) => Counter(value.Value + 1);
                public static Counter operator --(Counter value) => Counter(value.Value - 1);
            }
            class Program {
                public static int32 Main() {
                    Counter one = Counter(1);
                    if ((+one).Value != 1 || (-one).Value != -1 || (~one).Value != -2 || (!Counter(0)) == false || !Counter(1)) return 1;
                    Counter old = one++;
                    if (old.Value != 1 || one.Value != 2) return 2;
                    Counter current = ++one;
                    if (current.Value != 3 || one.Value != 3) return 3;
                    Counter oldAgain = one--;
                    if (oldAgain.Value != 3 || one.Value != 2) return 4;
                    Counter currentAgain = --one;
                    if (currentAgain.Value != 1 || one.Value != 1) return 5;
                    return 0;
                }
            }
            """;
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Null(execution.InteropError);

        var invalidIncrement = Compile("struct Counter { public static int32 operator ++(Counter value) => 1; } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(invalidIncrement.Diagnostics, diagnostic => diagnostic.Code == "LMC3010" && diagnostic.Message.Contains("valid operator result", StringComparison.Ordinal));
    }

    [Fact]
    public void SupportsClassOperatorOverloadsWithManagedReferenceOwnership()
    {
        const string source = """
            class Number {
                public int32 Value;
                public this(int32 value) { this.Value = value; }
                public static int32 operator +(Number left, int32 right) => left.Value + right;
                public static Number operator +(Number left, Number right) => new Number(left.Value + right.Value);
                public static Number operator -(Number left, Number right) => new Number(left.Value - right.Value);
                public static Number operator *(Number left, Number right) => new Number(left.Value * right.Value);
                public static Number operator /(Number left, Number right) => new Number(left.Value / right.Value);
                public static Number operator %(Number left, Number right) => new Number(left.Value % right.Value);
                public static Number operator &(Number left, Number right) => new Number(left.Value & right.Value);
                public static Number operator |(Number left, Number right) => new Number(left.Value | right.Value);
                public static Number operator ^(Number left, Number right) => new Number(left.Value ^ right.Value);
                public static Number operator <<(Number left, Number right) => new Number(left.Value << right.Value);
                public static Number operator >>(Number left, Number right) => new Number(left.Value >> right.Value);
                public static Number operator +(Number value) => new Number(value.Value);
                public static Number operator -(Number value) => new Number(-value.Value);
                public static Number operator ~(Number value) => new Number(~value.Value);
                public static bool operator !(Number value) => value.Value == 0;
                public static Number operator ++(Number value) => new Number(value.Value + 1);
                public static Number operator --(Number value) => new Number(value.Value - 1);
                public static bool operator ==(Number left, Number right) => left.Value == right.Value;
                public static bool operator <(Number left, Number right) => left.Value < right.Value;
                public static bool operator >(Number left, Number right) => left.Value > right.Value;
                public static bool operator <=(Number left, Number right) => left.Value <= right.Value;
                public static bool operator >=(Number left, Number right) => left.Value >= right.Value;
            }
            class Program {
                public static int32 Main() {
                    Number five = new Number(5);
                    Number two = new Number(2);
                    if ((+five).Value != 5 || (-five).Value != -5 || (~five).Value != -6) return 1;
                    if ((five + two).Value != 7 || (five - two).Value != 3 || (five * two).Value != 10) return 2;
                    if ((five / two).Value != 2 || (five % two).Value != 1) return 3;
                    if ((five & two).Value != 0 || (five | two).Value != 7 || (five ^ two).Value != 7) return 4;
                    if ((five << two).Value != 20 || (five >> two).Value != 1) return 5;
                    if (!(five > two) || !(five >= two) || !(two < five) || two <= new Number(1)) return 6;
                    if (five + 2 != 7 || (!new Number(0)) != true || (!five) != false || two < new Number(1)) return 7;
                    Number sum = five + new Number(2);
                    if (sum.Value != 7 || five != new Number(5)) return 8;
                    Number compound = new Number(3);
                    compound += new Number(4);
                    if (compound.Value != 7) return 9;
                    Number previous = compound++;
                    if (previous.Value != 7 || compound.Value != 8) return 10;
                    Number current = --compound;
                    if (current.Value != 7 || compound.Value != 7) return 11;
                    return 0;
                }
            }
            """;
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var invalidInstanceOperator = Compile("class Number { public int32 Value; public Number operator +(Number left, Number right) => left; } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(invalidInstanceOperator.Diagnostics, diagnostic => diagnostic.Code == "LMC3010" && diagnostic.Message.Contains("public static", StringComparison.Ordinal));
    }

    [Fact]
    public void ReportsReservedFutureKeyword()
    {
        var result = Compile("class Program { public static void Main() { let x = await; } }");
        Assert.Contains(result.Diagnostics, d => d.Code == "LS0104");
    }

    [Fact]
    public void RejectsMissingAndDuplicateEntryPoints()
    {
        var missing = Compile("class Program { public static void Other() {} }");
        Assert.Contains(missing.Diagnostics, d => d.Code == "LS2201");
        var duplicate = Compile("class A { public static void Main() {} } class B { public static int32 Main() { return 0; } }");
        Assert.Contains(duplicate.Diagnostics, d => d.Code == "LS2201");
    }

    [Fact]
    public void EntryPointAcceptsUtf8ProcessArgumentsAndExcludesTheProgramName()
    {
        const string source = "class Program { public static int32 Main(string[] args) { return args.Length == 2 && args[0] == \"first\" && args[1] == \"Aé語🌙\" && args[1].Length == 5 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("define i32 @main(i32 %argc, ptr %argv)", result.LlvmIr!, StringComparison.Ordinal);

        var execution = LlvmJit.RunMain(result.LlvmIr!, arguments: ["first", "Aé語🌙"]);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        const string malformedUtf8Source = "class Program { public static int32 Main(string[] args) { return args.Length == 1 && args[0] == \"�(\" && args[0].Length == 2 ? 0 : 1; } }";
        var malformedUtf8 = Compile(malformedUtf8Source);
        Assert.True(malformedUtf8.Succeeded, string.Join(Environment.NewLine, malformedUtf8.Diagnostics));
        var malformedUtf8Execution = LlvmJit.RunMain(malformedUtf8.LlvmIr!, rawUtf8Arguments: [new byte[] { 0xC3, 0x28 }]);
        Assert.Equal(0, malformedUtf8Execution.ExitCode);
        Assert.Equal(0, malformedUtf8Execution.OutstandingAllocations);

        const string emptyArgumentsSource = "class Program { public static int32 Main(string[] args) { return args.Length; } }";
        var emptyArguments = Compile(emptyArgumentsSource);
        Assert.True(emptyArguments.Succeeded, string.Join(Environment.NewLine, emptyArguments.Diagnostics));
        var emptyArgumentsExecution = LlvmJit.RunMain(emptyArguments.LlvmIr!);
        Assert.Equal(0, emptyArgumentsExecution.ExitCode);
        Assert.Equal(0, emptyArgumentsExecution.OutstandingAllocations);

        const string voidEntrySource = "class Program { public static void Main(string[] args) { } }";
        var voidEntry = Compile(voidEntrySource);
        Assert.True(voidEntry.Succeeded, string.Join(Environment.NewLine, voidEntry.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(voidEntry.LlvmIr!).ExitCode);

        var unsupported = Compile("class Program { public static int32 Main(int32[] args) { return 0; } }");
        Assert.Contains(unsupported.Diagnostics, diagnostic => diagnostic.Code == "LS2201");
        var genericOwner = Compile("class Program<T> { public static int32 Main() { return 0; } }");
        Assert.Contains(genericOwner.Diagnostics, diagnostic => diagnostic.Code == "LS2201");
        var duplicate = Compile("class Program { public static void Main() {} public static void Main(string[] args) {} }");
        Assert.Contains(duplicate.Diagnostics, diagnostic => diagnostic.Code == "LS2201");
    }

    [Fact]
    public void WindowsArgumentEntryUsesTheWideUnicodeCommandLineApi()
    {
        const string source = "class Program { public static int32 Main(string[] args) { return args.Length; } }";
        const string target = "x86_64-pc-windows-msvc";
        var result = LumaCompiler.Compile([("windows-args.ls", source)], new CompilationOptions(TargetTriple: target));
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("@CommandLineToArgvW", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("@WideCharToMultiByte(i32 65001", result.LlvmIr!, StringComparison.Ordinal);
        Assert.DoesNotContain("call ptr @luma_string_from_utf8z(ptr %luma.native.argument)", result.LlvmIr!, StringComparison.Ordinal);

        var targetStatus = LlvmToolchain.ValidateTarget(target);
        Assert.True(targetStatus.Succeeded, targetStatus.Error);
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), "lumasharp-windows-args-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            var emitted = LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, Path.Combine(temporaryDirectory, "args.obj"), target);
            Assert.True(emitted.Succeeded, emitted.Error);
        }
        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    [Fact]
    public void StaticLibraryDoesNotRequireOrGenerateEntryPoint()
    {
        var result = Compile("class Utilities { public static int32 Id(int32 value) { return value; } }", new CompilationOptions(StaticLibrary: true));
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.DoesNotContain("@main", result.LlvmIr!);
    }

    [Fact]
    public void UnsupportedConstructsProduceCompilerDiagnostics()
    {
        var result = Compile("class Node { } class Program { public static void Main() { Node value = new Node { }; } }");
        Assert.Contains(result.Diagnostics, d => d.Code == "LMC3000" && d.Message.Contains("object initializers", StringComparison.Ordinal));
    }

    [Fact]
    public void ParenthesizedSwitchPatternsPreserveCombinedPatternSemantics()
    {
        const string source = "class Program { public static int32 Main() { int32 value = 2; switch (value) { case (1 or 2): return 42; default: return 0; } } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(42, LlvmJit.RunMain(result.LlvmIr!).ExitCode);
    }

    [Theory]
    [InlineData("(int32, int32) pair = (1, 2); return 0;")]
    [InlineData("return (1, 2);")]
    [InlineData("return (1 < 2, 3);")]
    [InlineData("for ((int32, int32) pair = (1, 2); false; ) { } return 0;")]
    [InlineData("switch (1) { case (1, 2): return 1; default: return 0; } return 0;")]
    public void ExcludedTupleSyntaxProducesOneFocusedSyntaxDiagnostic(string body)
    {
        var result = Compile($"class Program {{ public static int32 Main() {{ {body} }} }}");
        Assert.False(result.Succeeded);
        Assert.Null(result.LlvmIr);
        Assert.Single(result.Diagnostics, diagnostic => diagnostic.Code == "LS0001" && diagnostic.Message.Contains("Tuple", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Code == "LMC9000");
    }

    [Fact]
    public void ExceptionSyntaxLowersAndFiltersRemainExplicitlyUnsupported()
    {
        const string source = "virtual class AppError : System.Exception { public this() { } } class Program { public static void Main() { try { throw new AppError(); } catch (System.InvalidOperationException error) { } catch (...) { } finally { } } }";
        var result = Compile(source);
        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Code == "LS0001");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("@luma_eh_find_handler", result.LlvmIr!);
        Assert.Contains("@luma_eh_top = internal thread_local global ptr null", result.LlvmIr, StringComparison.Ordinal);
        Assert.Contains("@luma_pending_exception = internal thread_local global ptr null", result.LlvmIr, StringComparison.Ordinal);
        Assert.Contains("@luma_pending_handler = internal thread_local global ptr null", result.LlvmIr, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Null(execution.InteropError);

        var filteredCatch = Compile("class Program { public static void Main() { try { } catch (Exception error) when (true) { } } }");
        Assert.DoesNotContain(filteredCatch.Diagnostics, diagnostic => diagnostic.Code == "LS0001");
        Assert.Contains(filteredCatch.Diagnostics, diagnostic => diagnostic.Code == "LMC3000" && diagnostic.Message.Contains("filters", StringComparison.Ordinal));

        var standardIoCatch = Compile("class Program { public static void Main() { try { } catch (IOException) { } } }");
        Assert.DoesNotContain(standardIoCatch.Diagnostics, diagnostic => diagnostic.Code == "LS1800");
    }

    [Fact]
    public void ExceptionTlsStateVerifiesAndEmitsNativeObjectsAcrossTargets()
    {
        const string source = "class Program { public static int32 Main() { try { throw new System.Exception(); } catch (System.Exception) { return 0; } } }";
        var hostTriple = LlvmToolchain.GetHostTargetTriple();
        Assert.False(string.IsNullOrWhiteSpace(hostTriple));
        var targets = new[] { hostTriple!, "x86_64-unknown-linux-gnu" }.Distinct(StringComparer.Ordinal).ToArray();

        foreach (var target in targets)
        {
            var result = Compile(source, new CompilationOptions(TargetTriple: target));
            Assert.True(result.Succeeded, $"{target}: {string.Join(Environment.NewLine, result.Diagnostics)}");
            Assert.Contains("@luma_eh_top = internal thread_local global ptr null", result.LlvmIr!, StringComparison.Ordinal);
            Assert.Contains("@luma_pending_exception = internal thread_local global ptr null", result.LlvmIr!, StringComparison.Ordinal);
            Assert.Contains("@luma_pending_handler = internal thread_local global ptr null", result.LlvmIr!, StringComparison.Ordinal);

            var objectPath = Path.Combine(Path.GetTempPath(), $"lumasharp-exception-tls-{Guid.NewGuid():N}.obj");
            try
            {
                var emitted = LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, target, optimize: true);
                Assert.True(emitted.Succeeded, $"{target}: {emitted.Error}");
                Assert.True(File.Exists(objectPath));
            }
            finally
            {
                if (File.Exists(objectPath)) File.Delete(objectPath);
            }
        }
    }

    [Fact]
    public void UnhandledExceptionsPrintTypeMessageAndInnerExceptionChain()
    {
        const string source = "class Program { public static int32 Main() { " +
            "System.Exception cause = new System.Exception(\"inner details\"); " +
            "throw new System.Exception(\"outer details\", cause); } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var objectPath = Path.Combine(Path.GetTempPath(), $"lumasharp-unhandled-{Guid.NewGuid():N}.obj");
        try
        {
            var emitted = LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, "host", optimize: true);
            Assert.True(emitted.Succeeded, emitted.Error);
        }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(70, execution.ExitCode);
        Assert.Contains("Unhandled exception: System.Exception: outer details", LlvmJit.CapturedStderr, StringComparison.Ordinal);
        Assert.Contains("InnerException: System.Exception: inner details", LlvmJit.CapturedStderr, StringComparison.Ordinal);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ExceptionsPropagateAcrossCallsAndDispatchDerivedTypesInOrder()
    {
        const string source = "virtual class BaseFailure : Exception { } class ChildFailure : BaseFailure { } class Program { public static void Raise() { throw new ChildFailure(); } public static int32 Main() { int32 result = 9; try { Raise(); } catch (ChildFailure error) { result = error == null ? 1 : 42; } catch (BaseFailure) { result = 2; } return result == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ExceptionsRethrowAndRunFinallyWhileUnwinding()
    {
        const string source = "class Failure : Exception { } class Program { public static int32 Value; public static void Raise() { throw new Failure(); } public static int32 Main() { try { try { Raise(); } finally { Program.Value = 42; } } catch (...) { try { throw; } catch (Exception) { } } return Program.Value == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void FinallyRunsForReturnBreakAndContinueTransfers()
    {
        const string source = "class Program { public static int32 Value; public static int32 ReturnInsideTry() { try { return 7; } finally { Program.Value = 42; } } public static int32 Main() { int32 returned = ReturnInsideTry(); while (true) { try { break; } finally { Program.Value += 1; } } int32 count = 0; while (count < 2) { try { count++; continue; } finally { Program.Value += 1; } } return returned == 7 && count == 2 && Program.Value == 45 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ExceptionThrownByFinallyReplacesThePropagatingException()
    {
        const string source = "class FirstFailure : Exception { } class SecondFailure : Exception { } class Program { public static int32 Main() { try { try { throw new FirstFailure(); } finally { throw new SecondFailure(); } } catch (SecondFailure) { return 0; } catch (FirstFailure) { return 1; } } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void CheckedArithmeticNullAndBoundsFailuresRaiseCatchableExceptions()
    {
        const string source = "class Program { public static int32 Overflow() { int32 value = 2147483647; value++; return value; } public static int32 Main() { int32 caught = 0; try { Program.Overflow(); } catch (OverflowException) { caught++; } try { int32 numerator = 1; int32 denominator = 0; int32 quotient = numerator / denominator; } catch (DivideByZeroException) { caught++; } int32[] values = new int32[1]; try { int32 item = values[3]; } catch (IndexOutOfRangeException) { caught++; } string text = \"x\"; try { char item = text[2]; } catch (IndexOutOfRangeException) { caught++; } string? nullable = null; try { int32 length = nullable!.Length; } catch (NullReferenceException) { caught++; } return caught == 5 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ClassRuntimeTypeTestsAndConditionalCastsUseDynamicTypeDescriptors()
    {
        const string source = "virtual class Animal { } class Dog : Animal { } class Cat : Animal { } class Program { public static int32 Main() { Animal pet = new Dog(); if (!(pet is Dog)) return 1; if (pet is not Animal) return 2; if (!(pet is not Cat)) return 3; Dog? dog = pet as Dog; Cat? cat = pet as Cat; if (dog == null || cat != null) return 4; Dog explicitDog = (Dog)pet; if (explicitDog is not Dog) return 8; Animal? optional = null; if (optional is Animal || !(optional is not Animal)) return 5; Animal proven = new Dog() as Animal; return proven is Animal ? 0 : 6; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("@luma_class_is_instance", result.LlvmIr!, StringComparison.Ordinal);

        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.True(execution.ExitCode == 0, $"Unexpected exit {execution.ExitCode}; interop={execution.InteropError}; stderr={LlvmJit.CapturedStderr}");
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        const string failedCast = "virtual class Animal { } class Dog : Animal { } class Cat : Animal { } class Program { public static int32 Main() { Animal pet = new Cat(); try { Dog wrong = (Dog)pet; return 1; } catch (InvalidCastException) { } Animal? missing = null; try { Dog wrong = (Dog)missing; return 2; } catch (NullReferenceException) { return 0; } } }";
        var failedCastResult = Compile(failedCast);
        Assert.True(failedCastResult.Succeeded, string.Join(Environment.NewLine, failedCastResult.Diagnostics));
        var failedCastExecution = LlvmJit.RunMain(failedCastResult.LlvmIr!);
        Assert.Equal(0, failedCastExecution.ExitCode);
        Assert.Equal(0, failedCastExecution.OutstandingAllocations);
        Assert.Null(failedCastExecution.InteropError);

        var valueTypeTest = Compile("class Program { public static int32 Main() { return 1 is int32 ? 0 : 1; } }");
        Assert.True(valueTypeTest.Succeeded, string.Join(Environment.NewLine, valueTypeTest.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(valueTypeTest.LlvmIr!).ExitCode);
    }

    [Fact]
    public void ManagedArraysHaveRuntimeTypeDescriptorsForIsAndAs()
    {
        const string source = "class Program { public static int32 Main() { int32[] values = new int32[] { 7, 9 }; object boxed = values; if (!(values is int32[]) || !(values is object) || !(boxed is int32[])) return 1; if (boxed is int32[] matched) { if (matched[0] != 7) return 6; } else return 7; int32 switched = 0; switch (boxed) { case int32[] selected: switched = selected[1]; break; default: return 8; } if (switched != 9) return 9; switch (values) { case int32[] direct: if (direct[0] != 7) return 10; break; } int32[]? recovered = boxed as int32[]; if (recovered == null || recovered![1] != 9) return 2; int64[]? wrong = boxed as int64[]; if (wrong != null) return 3; if (values is int32) return 16; int32? unboxed = values as int32; if (unboxed != null) return 17; object other = new object(); int32[]? missing = other as int32[]; if (missing != null) return 4; string text = \"text\"; int32[]? fromString = text as int32[]; if (fromString != null) return 14; int32[]? empty = null; if (empty is int32[]) return 12; int32[]? emptyCast = empty as int32[]; if (emptyCast != null) return 13; int32[,] rectangular = new int32[1, 1]; if (rectangular is int32[]) return 5; if (Classify(values) != 2) return 15; return 0; } public static int32 Classify(int32[] input) { switch (input) { case int32[] items: return items.Length; } } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("@luma_arraydesc_int32", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ManagedArraysImplementCompilerKnownArrayBaseType()
    {
        const string source = "class Program { public static Array Pass(Array value) { return value; } public static int32 Main() { int32[,] matrix = new int32[2, 3]; Array dimensions = matrix; if (!(matrix is Array) || !(dimensions is object) || dimensions != matrix) return 1; if (dimensions.Length != 6 || dimensions.Rank != 2) return 2; if (dimensions.GetLength(0) != 2 || dimensions.GetLength(1) != 3) return 3; if (dimensions.ToString() != \"int32[,]\") return 4; Array returned = Pass(dimensions); if (returned.Length != 6 || returned.GetLength(1) != 3) return 5; object boxed = matrix; if (!(boxed is Array) || boxed != matrix) return 6; Array? recovered = boxed as Array; if (recovered == null || recovered.Length != 6 || recovered != matrix) return 7; Array explicitCast = (Array)boxed; if (explicitCast.Rank != 2) return 8; try { Array invalid = (Array)new object(); return 9; } catch (InvalidCastException) { } Array? optional = null; if (optional?.Length != null || optional?.Rank != null || optional?.GetLength(0) != null) return 10; optional = matrix; if (optional?.Length != 6 || optional?.Rank != 2 || optional?.GetLength(1) != 3) return 11; try { returned.GetLength(2); return 12; } catch (IndexOutOfRangeException) { } return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS3101");
        Assert.Contains("@luma_classdesc_System_Array", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("ptr @luma_classdesc_System_Array", result.LlvmIr, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ManagedArraySwitchesSupportArrayAndObjectBasePatterns()
    {
        const string source = "class Program { public static int32 Main() { int32[,] matrix = new int32[2, 3]; object boxed = matrix; return ArrayLength(matrix) == 6 && NullableArrayLength(matrix) == 6 && NullableArrayLength(null) == 0 && ObjectArrayLength(boxed) == 6 ? 0 : 1; } public static int32 ArrayLength(int32[,] value) { switch (value) { case Array array: return array.Length; } } public static int32 NullableArrayLength(Array? value) { switch (value) { case Array array: return array.Length; case null: return 0; } } public static int32 ObjectArrayLength(object value) { switch (value) { case Array array: return array.Length; default: return -1; } } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var missingNull = Compile("class Program { public static int32 Select(Array? value) { switch (value) { case Array array: return array.Length; } } public static int32 Main() { return 0; } }");
        Assert.Contains(missingNull.Diagnostics, diagnostic => diagnostic.Code == "LS0630");

        var exactAfterBase = Compile("class Program { public static void Main() { int32[] values = new int32[1]; switch (values) { case Array: break; case int32[]: break; } } }");
        Assert.Contains(exactAfterBase.Diagnostics, diagnostic => diagnostic.Code == "LS0622");

        var baseAfterExact = Compile("class Program { public static void Main() { int32[] values = new int32[1]; switch (values) { case int32[]: break; case Array: break; } } }");
        Assert.Contains(baseAfterExact.Diagnostics, diagnostic => diagnostic.Code == "LS0622");
    }

    [Fact]
    public void NullableArrayBaseDirectAccessRequiresNullProof()
    {
        var result = Compile("class Program { public static int32 Main() { Array? values = null; return values.Length; } }");
        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS1303");
        Assert.Null(result.LlvmIr);
    }

    [Fact]
    public void CompilerKnownArrayBaseCannotBeConstructedOrExtended()
    {
        var construction = Compile("class Program { public static int32 Main() { Array value = new Array(); return 0; } }");
        Assert.False(construction.Succeeded);
        Assert.Contains(construction.Diagnostics, diagnostic => diagnostic.Code == "LS0834");
        Assert.Null(construction.LlvmIr);

        var inheritance = Compile("class FakeArray : Array { } class Program { public static int32 Main() { return 0; } }");
        Assert.False(inheritance.Succeeded);
        Assert.Contains(inheritance.Diagnostics, diagnostic => diagnostic.Code == "LS0830");
        Assert.Null(inheritance.LlvmIr);

        var shadowing = Compile("class Array { } class Program { public static int32 Main() { Array value = new Array(); return 0; } }");
        Assert.True(shadowing.Succeeded, string.Join(Environment.NewLine, shadowing.Diagnostics));
    }

    [Fact]
    public void NullableManagedArrayGetLengthUsesConditionalEvaluation()
    {
        const string source = "class Program { public static int32 dimensionsRead; public static int32 NextDimension() { Program.dimensionsRead++; return Program.dimensionsRead - 1; } public static int32 Main() { int32[,]? matrix = null; if (matrix?.GetLength(NextDimension()) != null || Program.dimensionsRead != 0) return 1; matrix = new int32[2, 3]; if (matrix?.GetLength(NextDimension()) != 2 || matrix?.GetLength(NextDimension()) != 3 || Program.dimensionsRead != 2) return 2; try { matrix?.GetLength(NextDimension()); return 3; } catch (IndexOutOfRangeException) { } return Program.dimensionsRead == 3 ? 0 : 4; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void StringsCanBeBoxedAndUseManagedObjectTypeTestsAndCasts()
    {
        const string source = "class Program { public static int32 Main() { string text = \"hello\"; object boxed = text; if (!(boxed is string)) return 1; if (boxed is string matched) { if (matched != \"hello\") return 2; } else return 3; string? recovered = boxed as string; if (recovered == null || recovered != text) return 4; string explicitCast = (string)boxed; if (explicitCast != text) return 14; if (text is int32) return 15; int32? notBoxed = text as int32; if (notBoxed != null) return 16; if (boxed.ToString() != text) return 5; object other = new object(); if (other is string) return 6; string? missing = other as string; if (missing != null) return 7; try { string invalid = (string)other; return 17; } catch (InvalidCastException) { } try { string? invalidNullable = (string?)other; return 18; } catch (InvalidCastException) { } object? nothing = null; try { string invalidNull = (string)nothing; return 19; } catch (NullReferenceException) { } int32[] values = new int32[0]; object arrayObject = values; if (arrayObject is string) return 8; string? arrayCast = arrayObject as string; if (arrayCast != null) return 9; int32 switches = 0; switch (boxed) { case string selected: if (selected != text) return 10; switches++; break; default: return 11; } string dynamicText = \"dynamic\" + \" text\"; object dynamicBox = dynamicText; dynamicText = \"replaced\"; if (dynamicBox.ToString() != \"dynamic text\") return 13; return switches == 1 ? 0 : 12; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("@luma_string_type = private constant %luma.typedesc", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ManagedArraysRetainClassReferencesAndRequireCompleteNonNullableInitialization()
    {
        const string source = "virtual class Animal { } class Dog : Animal { } class Cat : Animal { } class Program { public static int32 Main() { Animal?[] nullable = new Animal?[2]; Animal current = new Dog(); nullable[0] = current; current = new Cat(); if (!(nullable[0] is Dog)) return 1; Animal[] complete = { new Dog(), new Cat() }; int32 dogs = 0; for (let item in complete) if (item is Dog) dogs++; int32[]?[] nested = new int32[]?[2]; if (nested[0] != null) return 3; nested[0] = new int32[] { 7 }; if (nested[0]![0] != 7) return 4; return dogs == 1 ? 0 : 2; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var incomplete = Compile("class Node { } class Program { public static int32 Main() { Node[] nodes = new Node[2]; return nodes.Length; } }");
        Assert.Contains(incomplete.Diagnostics, diagnostic => diagnostic.Code == "LS1501");
        Assert.Null(incomplete.LlvmIr);

        var nonNullableNested = Compile("class Program { public static int32 Main() { int32[][] values = new int32[][2]; return values.Length; } }");
        Assert.Contains(nonNullableNested.Diagnostics, diagnostic => diagnostic.Code == "LS1501");
        Assert.Null(nonNullableNested.LlvmIr);
    }

    [Fact]
    public void ManagedArrayCovarianceIsRejectedWithItsSpecifiedDiagnostic()
    {
        const string source = "virtual class Animal { } class Dog : Animal { } class Program { public static int32 Main() { Dog[] dogs = { new Dog() }; Animal[] animals = dogs; return animals.Length; } }";
        var result = Compile(source);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS1502" && diagnostic.Message.Contains("invariant", StringComparison.Ordinal));
        Assert.Null(result.LlvmIr);
    }

    [Fact]
    public void ManagedArraysStoreCopyMutateAndIterateValueTypeElements()
    {
        const string source = "struct Pair { public int32 x; public int32 y; public this(int32 x, int32 y) { this.x = x; this.y = y; } public void Translate(int32 dx, int32 dy) mut { x += dx; y += dy; } } struct Nested { public int32 tag; public Pair pair; } class Program { public static int32 Sum(Pair[] values) { int32 sum = 0; for (let item in values) sum += item.x + item.y; return sum; } public static Pair[] Echo(Pair[] values) { return values; } public static int32 Main() { Pair[] values = { Pair(2, 3), Pair(5, 7) }; values[0].x = 11; values[1].Translate(1, 2); Pair copy = values[0]; copy.y = 90; if (values[0].y != 3) return 1; Pair[] alias = Echo(values); alias[0].y = 4; Pair[,] grid = new Pair[1, 2]; grid[0, 1].x = 8; Pair[] zeroed = new Pair[1]; Pair[][] jagged = { { Pair(4, 5) }, { Pair(6, 7), Pair(8, 9) } }; jagged[1][0].x = 10; Nested nested = default; nested.pair.x = 6; int32 sum = Sum(values); int32 jaggedSum = Sum(jagged[1]); return sum == 30 && jaggedSum == 34 && values[0].y == 4 && grid[0, 1].x == 8 && zeroed[0].x == 0 && nested.pair.x == 6 ? 0 : 2; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("getelementptr %struct.Pair", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void StructsWithManagedReferenceFieldsAreSupportedInManagedArrays()
    {
        const string source = "class Node { public int32 Value; public this(int32 value) { Value = value; } } struct Holder { public Node? value; public this(Node? initial) { value = initial; } } class Program { public static int32 Main() { Node node = new Node(12); Holder[] values = new Holder[1] { Holder(node) }; Holder alias = values[0]; values[0] = default; return alias.value!.Value == 12 && values.Length == 1 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void CompilerKnownExceptionBaseCanBeUsedAsAManagedClassType()
    {
        const string source = "class CustomFailure : Exception { public this(string message) { this.Message = message; } } class Program { public static int32 Main() { let leaf = new CustomFailure(\"inner\"); let wrapped = new System.TypeInitializationException(\"outer\", leaf); let inner = wrapped.InnerException; return inner == null || inner.Message != \"inner\" || wrapped.Message != \"outer\" ? 1 : 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ExceptionDiagnosticsRejectInvalidThrowsCatchOrderAndFinallyTransfers()
    {
        var invalidThrow = Compile("class Program { public static void Main() { throw 1; } }");
        Assert.Contains(invalidThrow.Diagnostics, diagnostic => diagnostic.Code == "LS1800");

        var nonExceptionThrow = Compile("class NotFailure { } class Program { public static void Main() { throw new NotFailure(); } }");
        Assert.Contains(nonExceptionThrow.Diagnostics, diagnostic => diagnostic.Code == "LS1800" && diagnostic.Message.Contains("System.Exception", StringComparison.Ordinal));

        var invalidRethrow = Compile("class Program { public static void Main() { throw; } }");
        Assert.Contains(invalidRethrow.Diagnostics, diagnostic => diagnostic.Code == "LS1800");

        var nullableThrow = Compile("class Program { public static void Main() { System.Exception? error = null; throw error; } }");
        Assert.Contains(nullableThrow.Diagnostics, diagnostic => diagnostic.Code == "LS1800" && diagnostic.Message.Contains("non-nullable", StringComparison.Ordinal));

        var invalidCatchType = Compile("class Program { public static void Main() { try { } catch (int32) { } } }");
        Assert.Contains(invalidCatchType.Diagnostics, diagnostic => diagnostic.Code == "LS1800");

        var handlerlessTry = Compile("class Program { public static void Main() { try { } } }");
        Assert.Contains(handlerlessTry.Diagnostics, diagnostic => diagnostic.Code == "LS1800");

        var readonlyCatchVariable = Compile("class Program { public static void Main() { try { } catch (Exception error) { error = new Exception(); } } }");
        Assert.Contains(readonlyCatchVariable.Diagnostics, diagnostic => diagnostic.Code == "LMC3012");

        const string unreachableCatch = "virtual class ParentError : Exception { } class ChildError : ParentError { } class Program { public static void Main() { try { } catch (ParentError) { } catch (ChildError) { } } }";
        var ordered = Compile(unreachableCatch);
        Assert.Contains(ordered.Diagnostics, diagnostic => diagnostic.Code == "LS1801");

        var misplacedCatchAll = Compile("class Program { public static void Main() { try { } catch (...) { } catch (Exception) { } } }");
        Assert.Contains(misplacedCatchAll.Diagnostics, diagnostic => diagnostic.Code == "LS1801");

        var redundantCatchAll = Compile("class Program { public static void Main() { try { } catch (Exception) { } catch (...) { } } }");
        Assert.Contains(redundantCatchAll.Diagnostics, diagnostic => diagnostic.Code == "LS1801" && diagnostic.Message.Contains("unreachable", StringComparison.OrdinalIgnoreCase));

        var finallyTransfer = Compile("class Program { public static void Main() { try { } finally { return; } } }");
        Assert.Contains(finallyTransfer.Diagnostics, diagnostic => diagnostic.Code == "LS0610");

        var localLoopTransfer = Compile("class Program { public static void Main() { try { } finally { while (true) { break; } } } }");
        Assert.DoesNotContain(localLoopTransfer.Diagnostics, diagnostic => diagnostic.Code == "LS0610");

        var throwHandledByReturningCatch = Compile("class Failure : Exception { } class Program { public static int32 Main() { try { throw new Failure(); } catch (...) { return 2; } } }");
        Assert.DoesNotContain(throwHandledByReturningCatch.Diagnostics, diagnostic => diagnostic.Code == "LS0630");
    }

    [Fact]
    public void NullableAndArrayIndexSyntaxReceivesFeatureSpecificDiagnostics()
    {
        var assertion = Compile("class Program { public static int32 Main() { int32 value = 1; return value!; } }");
        Assert.Contains(assertion.Diagnostics, d => d.Code == "LMC3010" && d.Message.Contains("Null assertion requires", StringComparison.Ordinal));
        Assert.DoesNotContain(assertion.Diagnostics, d => d.Code == "LS0001");

        var coalesce = Compile("class Program { public static int32 Main() { int32 value = 1; return value ?? 0; } }");
        Assert.Contains(coalesce.Diagnostics, d => d.Code == "LMC3010" && d.Message.Contains("left operand of '??'", StringComparison.Ordinal));

        var uninitializedArray = Compile("class Program { public static int32 Main() { int32[] values; return values[0]; } }");
        Assert.Contains(uninitializedArray.Diagnostics, d => d.Code == "LS0810");
        Assert.DoesNotContain(uninitializedArray.Diagnostics, d => d.Code == "LMC3101");

        var invalidRank = Compile("class Program { public static int32 Main() { int32[] values = new int32[2]; return values[0, 1]; } }");
        Assert.Contains(invalidRank.Diagnostics, d => d.Code == "LMC3010" && d.Message.Contains("requires 1 index", StringComparison.Ordinal));
    }

    [Fact]
    public void SupportsPrimitiveStaticConstantFieldsAcrossTypes()
    {
        var result = LumaCompiler.Compile([
            ("constants.ls", "class Constants { public const int32 Answer = 6 * 7; }"),
            ("main.ls", "class Program { public static int32 Main() { const int32 result = Constants.Answer; return result; } }")
        ]);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("ret i32", result.LlvmIr!);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-constfield-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try { Assert.True(LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, "host", optimize: false).Succeeded); }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }

        var inaccessible = LumaCompiler.Compile([
            ("constants.ls", "class Constants { const int32 Answer = 42; }"),
            ("main.ls", "class Program { public static int32 Main() { return Constants.Answer; } }")
        ]);
        Assert.Contains(inaccessible.Diagnostics, d => d.Code == "LMC3012" && d.Message.Contains("not accessible", StringComparison.Ordinal));

        var call = Compile("class Program { public const int32 Value = Helper(); public static int32 Helper() { return 1; } public static int32 Main() { return Value; } }");
        Assert.Contains(call.Diagnostics, d => d.Code == "LMC3010" && d.Message.Contains("constant expression", StringComparison.Ordinal));

        var overflow = Compile("class Program { public const int32 Value = 2147483647 + 1; public static int32 Main() { return 0; } }");
        Assert.Contains(overflow.Diagnostics, d => d.Code == "LMC3010" && d.Message.Contains("overflows", StringComparison.Ordinal));

        var wrapped = Compile("unsafe class Program { public const int32 Value = unchecked(2147483647 + 1); public static int32 Main() { return Program.Value; } }", new CompilationOptions(UnsafeEnabled: true));
        Assert.True(wrapped.Succeeded, string.Join(Environment.NewLine, wrapped.Diagnostics));
        Assert.Contains("ret i32 -2147483648", wrapped.LlvmIr!);
    }

    [Fact]
    public void SupportsMutablePrimitiveStaticFieldsAcrossTypes()
    {
        var result = LumaCompiler.Compile([
            ("state.ls", "namespace Demo; class State { public static int32 Value = 1; public static int32 Other; }"),
            ("main.ls", "namespace Demo; class Program { public static int32 Main() { State.Value += 41; State.Other = State.Value; State.Other++; return State.Other; } }")
        ]);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("@luma_global_Demo_State_Value = global i32 1", result.LlvmIr!);
        Assert.Contains("@luma_global_Demo_State_Other = global i32 0", result.LlvmIr);
        Assert.Contains("store i32", result.LlvmIr);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-staticfield-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try { Assert.True(LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, "host", optimize: false).Succeeded); }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }

        var inaccessible = LumaCompiler.Compile([
            ("state.ls", "class State { static int32 Value; }"),
            ("main.ls", "class Program { public static int32 Main() { return State.Value; } }")
        ]);
        Assert.Contains(inaccessible.Diagnostics, d => d.Code == "LMC3012" && d.Message.Contains("not accessible", StringComparison.Ordinal));

        var instanceField = Compile("class State { public int32 Value; } class Program { public static int32 Main() { let state = new State(); return state.Value; } }");
        Assert.True(instanceField.Succeeded, string.Join(Environment.NewLine, instanceField.Diagnostics));
        Assert.Contains("%class.State = type { ptr, i32, i32, i32 }", instanceField.LlvmIr!, StringComparison.Ordinal);
    }

    [Fact]
    public void SupportsExplicitNumericCastsWithCheckedOverflow()
    {
        var result = Compile("class Program { public static int32 Main() { int32 source = 257; uint8 low = (uint8)source; double precise = 12.75; int32 whole = (int32)precise; float rounded = (float)precise; return low + whole; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("icmp sge i128", result.LlvmIr!);
        Assert.Contains("@llvm.trunc.f64", result.LlvmIr);
        Assert.Contains("fptrunc double", result.LlvmIr);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-cast-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try { Assert.True(LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, "host", optimize: false).Succeeded); }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }

        var constantOverflow = Compile("class Program { public static int32 Main() { uint8 value = (uint8)300; return value; } }");
        Assert.Contains(constantOverflow.Diagnostics, d => d.Code == "LS0301" && d.Message.Contains("overflows", StringComparison.Ordinal));

        var constantTruncation = Compile("class Program { public static int32 Main() { int32 value = (int32)12.75; return value; } }");
        Assert.True(constantTruncation.Succeeded, string.Join(Environment.NewLine, constantTruncation.Diagnostics));
        var floatingOverflow = Compile("class Program { public static int32 Main() { uint8 value = (uint8)256.0; return value; } }");
        Assert.Contains(floatingOverflow.Diagnostics, d => d.Code == "LS0301" && d.Message.Contains("overflows", StringComparison.Ordinal));
        var namedConstant = Compile("class Program { public static int32 Main() { const int32 source = 300; uint8 value = (uint8)source; return value; } }");
        Assert.True(namedConstant.Succeeded, string.Join(Environment.NewLine, namedConstant.Diagnostics));

        var uncheckedResult = Compile("class Program { public static unsafe int32 Main() { int32 source = 257; uint8 value = unchecked((uint8)source); return value; } }", new CompilationOptions(UnsafeEnabled: true));
        Assert.True(uncheckedResult.Succeeded, string.Join(Environment.NewLine, uncheckedResult.Diagnostics));
        Assert.DoesNotContain("icmp sge i128", uncheckedResult.LlvmIr!);

        var invalid = Compile("class Program { public static int32 Main() { bool value = (bool)1; return 0; } }");
        Assert.Contains(invalid.Diagnostics, d => d.Code == "LMC3010" && d.Message.Contains("Explicit conversion", StringComparison.Ordinal));
    }

    [Fact]
    public void ImmutableLocalCannotBeReassigned()
    {
        var result = Compile("class Program { public static int32 Main() { let x = 1; x = 2; return x; } }");
        Assert.Contains(result.Diagnostics, d => d.Code == "LMC3012");
    }

    [Fact]
    public void ConditionsMustHaveBooleanType()
    {
        var result = Compile("class Program { public static void Main() { if (1) { return; } } }");
        Assert.Contains(result.Diagnostics, d => d.Code == "LMC3010");
    }

    [Fact]
    public void NonVoidMethodMustReturnOnEveryPath()
    {
        var result = Compile("class Program { private static bool Condition() { return true; } public static int32 Main() { if (Condition()) { return 1; } } }");
        Assert.Contains(result.Diagnostics, d => d.Code == "LS0630");
    }

    [Fact]
    public void RejectsInvalidExpressionStatement()
    {
        var result = Compile("class Program { public static void Main() { 1 + 2; } }");
        Assert.Contains(result.Diagnostics, d => d.Code == "LS0601");
    }

    [Fact]
    public void EntryPointMustBePublicAndStatic()
    {
        var result = Compile("class Program { private static void Main() {} }");
        Assert.Contains(result.Diagnostics, d => d.Code == "LS2201");
    }

    [Fact]
    public void UnreachableCodeIsWarnedAndCanBePromotedOrDisabled()
    {
        const string source = "class Program { public static void Main() { return; let x = 1; } }";
        var warning = Compile(source);
        Assert.Contains(warning.Diagnostics, d => d.Code == "LS3001" && d.Severity == DiagnosticSeverity.Warning);
        var promoted = Compile(source, new CompilationOptions(WarningsAsErrors: true));
        Assert.False(promoted.Succeeded);
        Assert.Contains(promoted.Diagnostics, d => d.Code == "LS3001" && d.Severity == DiagnosticSeverity.Error);
        var disabled = Compile(source, new CompilationOptions(DisabledWarnings: new HashSet<string> { "LS3001" }));
        Assert.True(disabled.Succeeded);
        Assert.DoesNotContain(disabled.Diagnostics, d => d.Code == "LS3001");
    }

    [Fact]
    public void RequiredHidingTypeTestNullAssertionAndEnumDiagnosticsAreCovered()
    {
        var hiding = Compile("class Base { public int32 Read() { return 1; } } class Derived : Base { public int32 Read() { return 2; } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(hiding.Diagnostics, diagnostic => diagnostic.Code == "LS3002" && diagnostic.Severity == DiagnosticSeverity.Warning);

        var typeTest = Compile("virtual class Animal { } class Dog : Animal { } class Program { public static bool IsAnimal(Dog value) { return value is Animal; } public static int32 Main() { return IsAnimal(new Dog()) ? 0 : 1; } }");
        Assert.Contains(typeTest.Diagnostics, diagnostic => diagnostic.Code == "LS3101" && diagnostic.Severity == DiagnosticSeverity.Warning);

        var assertion = Compile("class Box { } class Program { public static int32 Main() { Box value = new Box(); Box same = value!; return same == value ? 0 : 1; } }");
        Assert.Contains(assertion.Diagnostics, diagnostic => diagnostic.Code == "LS3103" && diagnostic.Severity == DiagnosticSeverity.Warning);

        var enumRange = Compile("enum Color : uint8 { Red = 300 } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(enumRange.Diagnostics, diagnostic => diagnostic.Code == "LS1102" && diagnostic.Severity == DiagnosticSeverity.Error);

        var mutatingInterface = Compile("interface IIterator { bool Next() mut; } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(mutatingInterface.Diagnostics, diagnostic => diagnostic.Code == "LS0824" && diagnostic.Severity == DiagnosticSeverity.Error);

        var lambdas = Compile("class Program { public static void Capture(ref int32 value) { var missingTarget = () => value; var inferred = item => item; var blockCapture = () => { value = value + 1; }; var shadowed = value => value; } public static int32 Main() { return 0; } }");
        Assert.Contains(lambdas.Diagnostics, diagnostic => diagnostic.Code == "LS0730" && diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.Contains(lambdas.Diagnostics, diagnostic => diagnostic.Code == "LS1701" && diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.Contains(lambdas.Diagnostics, diagnostic => diagnostic.Code == "LS0201" && diagnostic.Message.Contains("Lambda parameter", StringComparison.Ordinal));
    }

    [Fact]
    public void ConstantStringArrayAndPrimitiveTypeTestsEmitLS3101Warnings()
    {
        const string source = "class Program { public static int32 Main() { string text = \"value\"; int32[] values = new int32[0]; int32 number = 1; return text is string && text is not int32 && text is object && text is not Program && values is int32[] && values is object && number is int32 && number is not uint32 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var warnings = result.Diagnostics.Where(diagnostic => diagnostic.Code == "LS3101").ToArray();
        Assert.Equal(8, warnings.Length);
        Assert.All(warnings, warning => Assert.Equal(DiagnosticSeverity.Warning, warning.Severity));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);

        var promoted = Compile(source, new CompilationOptions(WarningsAsErrors: true));
        Assert.False(promoted.Succeeded);
        Assert.Contains(promoted.Diagnostics, diagnostic => diagnostic.Code == "LS3101" && diagnostic.Severity == DiagnosticSeverity.Error);
        var disabled = Compile(source, new CompilationOptions(DisabledWarnings: new HashSet<string> { "LS3101" }));
        Assert.True(disabled.Succeeded, string.Join(Environment.NewLine, disabled.Diagnostics));
        Assert.DoesNotContain(disabled.Diagnostics, diagnostic => diagnostic.Code == "LS3101");
    }

    [Fact]
    public void DelegateDeclarationsPreserveSignaturesAndRejectUnsupportedUsesPrecisely()
    {
        var declarations = Compile("delegate int32 BinaryOp(int32 left, int32 right); class Container { public delegate bool Predicate(int32 value); } class Program { public static int32 Main() { return 0; } }");
        Assert.True(declarations.Succeeded, string.Join(Environment.NewLine, declarations.Diagnostics));

        var optionalParameter = Compile("delegate int32 Invalid(int32 value = 0); class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(optionalParameter.Diagnostics, diagnostic => diagnostic.Code == "LS0001" && diagnostic.Message.Contains("Delegate parameters cannot be optional", StringComparison.Ordinal));

        var valueUse = Compile("delegate int32 BinaryOp(int32 value); class Program { public static int32 Main() { BinaryOp operation; return 0; } }");
        Assert.Contains(valueUse.Diagnostics, diagnostic => diagnostic.Code == "LS0810" && diagnostic.Message.Contains("BinaryOp", StringComparison.Ordinal));

        var generic = Compile("delegate T Identity<T>(T value); class Program { public static int32 Twice(int32 value) { return value * 2; } public static int32 Main() { Identity<int32> identity = Twice; return identity(21); } }");
        Assert.True(generic.Succeeded, string.Join(Environment.NewLine, generic.Diagnostics));
        Assert.Equal(42, LlvmJit.RunMain(generic.LlvmIr!).ExitCode);
    }

    [Fact]
    public void NullableDelegateStaticFieldsUseNullAsTheirLlvmDefault()
    {
        const string source = "delegate int32 Worker(); class Program { public static Worker? Current; public static int32 Main() { return Current == null ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("@luma_global_Program_Current = global ptr null", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Equal(0, LlvmJit.RunMain(result.LlvmIr!).ExitCode);
    }

    [Fact]
    public void MethodGroupsConvertToManagedDelegatesAndInvokeThroughFunctionPointers()
    {
        var result = Compile("delegate int32 Unary(int32 value); class Program { public static int32 Twice(int32 value) { return value * 2; } public static int32 Main() { Unary operation = Twice; return operation(value: 21); } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("%delegate.Unary = type { ptr, i32, i32, ptr, ptr }", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("store ptr @luma_Program_Twice_1", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("call i32 %t", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Equal(42, LlvmJit.RunMain(result.LlvmIr!).ExitCode);

        var mismatch = Compile("delegate int32 Unary(int32 value); class Program { public static int32 Twice(int32 left, int32 right) { return left + right; } public static int32 Main() { Unary operation = Twice; return 0; } }");
        Assert.Contains(mismatch.Diagnostics, diagnostic => diagnostic.Code == "LMC3010" && diagnostic.Message.Contains("not compatible with delegate", StringComparison.Ordinal));

        var argument = Compile("delegate int32 Unary(int32 value); class Program { public static int32 Twice(int32 value) { return value * 2; } public static int32 Apply(Unary operation, int32 value) { return operation(value); } public static int32 Main() { return Apply(Twice, 21); } }");
        Assert.True(argument.Succeeded, string.Join(Environment.NewLine, argument.Diagnostics));
        Assert.Equal(42, LlvmJit.RunMain(argument.LlvmIr!).ExitCode);

        var nullable = Compile("delegate int32 Unary(int32 value); class Program { public static int32 Twice(int32 value) { return value * 2; } public static int32 Main() { Unary? operation = Twice; if (operation != null) { return operation(21); } return 0; } }");
        Assert.True(nullable.Succeeded, string.Join(Environment.NewLine, nullable.Diagnostics));
        Assert.Equal(42, LlvmJit.RunMain(nullable.LlvmIr!).ExitCode);

        var asserted = Compile("delegate int32 Unary(int32 value); class Program { public static int32 Twice(int32 value) { return value * 2; } public static int32 Main() { Unary? operation = Twice; return operation!(21); } }");
        Assert.True(asserted.Succeeded, string.Join(Environment.NewLine, asserted.Diagnostics));
        Assert.Equal(42, LlvmJit.RunMain(asserted.LlvmIr!).ExitCode);
        var unsafeNullableCall = Compile("delegate int32 Unary(int32 value); class Program { public static int32 Main() { Unary? operation = null; return operation(21); } }");
        Assert.Contains(unsafeNullableCall.Diagnostics, diagnostic => diagnostic.Code == "LS1303" && diagnostic.Message.Contains("nullable delegate", StringComparison.Ordinal));

        var returned = Compile("delegate int32 Unary(int32 value); class Program { public static int32 Twice(int32 value) { return value * 2; } public static Unary Select() { return Twice; } public static int32 Main() { Unary operation = Select(); return operation(21); } }");
        Assert.True(returned.Succeeded, string.Join(Environment.NewLine, returned.Diagnostics));
        Assert.Equal(42, LlvmJit.RunMain(returned.LlvmIr!).ExitCode);

        var equality = Compile("delegate int32 Unary(int32 value); class Program { public static int32 Twice(int32 value) { return value * 2; } public static int32 Main() { Unary first = Twice; Unary second = Twice; if (first == second) { return 42; } return 0; } }");
        Assert.True(equality.Succeeded, string.Join(Environment.NewLine, equality.Diagnostics));
        Assert.Equal(42, LlvmJit.RunMain(equality.LlvmIr!).ExitCode);

        var qualified = Compile("delegate int32 Unary(int32 value); class Program { public static int32 Twice(int32 value) { return value * 2; } public static int32 Main() { Unary operation = Program.Twice; return operation(21); } }");
        Assert.True(qualified.Succeeded, string.Join(Environment.NewLine, qualified.Diagnostics));
        Assert.Equal(42, LlvmJit.RunMain(qualified.LlvmIr!).ExitCode);

        var delegateBase = Compile("delegate int32 Unary(int32 value); class Program { public static int32 Twice(int32 value) { return value * 2; } public static int32 Main() { Unary operation = Twice; object value = operation; return value is System.Delegate ? 0 : 1; } }");
        Assert.True(delegateBase.Succeeded, string.Join(Environment.NewLine, delegateBase.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(delegateBase.LlvmIr!).ExitCode);

        var array = Compile("delegate int32 Unary(int32 value); class Program { public static int32 Twice(int32 value) { return value * 2; } public static int32 Main() { Unary[] operations = new Unary[1] { Twice }; return operations[0](21); } }");
        Assert.True(array.Succeeded, string.Join(Environment.NewLine, array.Diagnostics));
        Assert.Equal(42, LlvmJit.RunMain(array.LlvmIr!).ExitCode);

        var objectMethod = Compile("delegate string Text(); class Box { } class Program { public static int32 Main() { Box box = new Box(); Text text = box.ToString; return text() == \"Box\" ? 0 : 1; } }");
        Assert.True(objectMethod.Succeeded, string.Join(Environment.NewLine, objectMethod.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(objectMethod.LlvmIr!).ExitCode);

        const string instanceSource = "delegate int32 Reader(); class Counter { public static int32 finalized; public int32 value = 42; public int32 Read() { return value; } ~this() { Counter.finalized += 1; } } class Program { public static int32 Main() { { Counter counter = new Counter(); Reader first = counter.Read; Reader second = counter.Read; if (first != second) return 1; if (Counter.finalized != 0 || first() != 42) return 2; } return Counter.finalized == 1 ? 0 : 3; } }";
        var instance = Compile(instanceSource);
        Assert.True(instance.Succeeded, string.Join(Environment.NewLine, instance.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(instance.LlvmIr!).ExitCode);

        var differentReceivers = Compile("delegate int32 Reader(); class Counter { public int32 Read() { return 1; } } class Program { public static int32 Main() { Counter first = new Counter(); Counter second = new Counter(); Reader a = first.Read; Reader b = second.Read; return a == b ? 1 : 0; } }");
        Assert.True(differentReceivers.Succeeded, string.Join(Environment.NewLine, differentReceivers.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(differentReceivers.LlvmIr!).ExitCode);

        var virtualMethod = Compile("delegate int32 Reader(); virtual class Base { public virtual int32 Read() { return 1; } } class Derived : Base { public override int32 Read() { return 42; } } class Program { public static int32 Main() { Base value = new Derived(); Reader read = value.Read; return read() == 42 ? 0 : 1; } }");
        Assert.True(virtualMethod.Succeeded, string.Join(Environment.NewLine, virtualMethod.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(virtualMethod.LlvmIr!).ExitCode);

        var baseMethod = Compile("delegate int32 Reader(); virtual class Base { public virtual int32 Read() { return 1; } } class Derived : Base { public override int32 Read() { return 42; } public int32 ReadBase() { Reader read = base.Read; return read(); } } class Program { public static int32 Main() { return new Derived().ReadBase() == 1 ? 0 : 1; } }");
        Assert.True(baseMethod.Succeeded, string.Join(Environment.NewLine, baseMethod.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(baseMethod.LlvmIr!).ExitCode);

        var mutatingThis = Compile("delegate void Action(); class Counter { public void Increment() mut { } public Action Capture() { Action action = this.Increment; return action; } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(mutatingThis.Diagnostics, diagnostic => diagnostic.Code == "LS0821" && diagnostic.Message.Contains("create a delegate", StringComparison.Ordinal));

        var field = Compile("delegate int32 Unary(int32 value); class Holder { public Unary Operation = Program.Twice; public int32 Run(int32 value) { return this.Operation(value); } } class Program { public static int32 Twice(int32 value) { return value * 2; } public static int32 Main() { Holder holder = new Holder(); return holder.Run(21); } }");
        Assert.True(field.Succeeded, string.Join(Environment.NewLine, field.Diagnostics));
        Assert.Equal(42, LlvmJit.RunMain(field.LlvmIr!).ExitCode);

        var coalescing = Compile("delegate int32 Unary(int32 value); class Program { public static int32 Twice(int32 value) { return value * 2; } public static int32 Main() { Unary? maybe = null; maybe ??= Twice; Unary operation = maybe ?? Twice; return operation(21); } }");
        Assert.True(coalescing.Succeeded, string.Join(Environment.NewLine, coalescing.Diagnostics));
        Assert.Equal(42, LlvmJit.RunMain(coalescing.LlvmIr!).ExitCode);

        var returnedCall = Compile("delegate int32 Unary(int32 value); class Program { public static int32 Twice(int32 value) { return value * 2; } public static Unary Select() { return Twice; } public static int32 Main() { return Select()(21); } }");
        Assert.True(returnedCall.Succeeded, string.Join(Environment.NewLine, returnedCall.Diagnostics));
        Assert.Equal(42, LlvmJit.RunMain(returnedCall.LlvmIr!).ExitCode);
    }

    [Fact]
    public void NonCapturingLambdasLowerToManagedDelegateMethods()
    {
        const string source = "delegate int32 Unary(int32 value); delegate void Action(); class Program { public static int32 Main() { Unary twice = value => value * 2; Action run = () => { int32 local = 1; }; run(); return twice(21); } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("@luma_Program__lambda", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Equal(42, LlvmJit.RunMain(result.LlvmIr!).ExitCode);

        var captured = Compile("delegate int32 Unary(int32 value); class Program { public static int32 Main() { int32 factor = 2; Unary twice = value => value * factor; return twice(21); } }");
        Assert.True(captured.Succeeded, string.Join(Environment.NewLine, captured.Diagnostics));
        Assert.Equal(42, LlvmJit.RunMain(captured.LlvmIr!).ExitCode);
    }

    [Fact]
    public void CapturingLambdasShareManagedClosureStorage()
    {
        const string counterSource = "delegate int32 Counter(); class Program { public static Counter Create(int32 start) { int32 count = start; return () => ++count; } public static int32 Main() { Counter counter = Create(40); if (counter() != 41) return 1; return counter() == 42 ? 0 : 2; } }";
        var counter = Compile(counterSource);
        Assert.True(counter.Succeeded, string.Join(Environment.NewLine, counter.Diagnostics));
        Assert.Contains("__LumaClosure_", counter.LlvmIr!, StringComparison.Ordinal);
        Assert.Equal(0, LlvmJit.RunMain(counter.LlvmIr!).ExitCode);

        const string forHeaderSource = "delegate int32 Getter(); class Program { public static Getter Create() { Getter first = () => 0; Getter second = () => 0; for (int32 i = 40; i < 42; i++) { if (i == 40) first = () => i; second = () => i; } return first() == 42 && second() == 42 ? first : second; } public static int32 Main() { Getter getter = Create(); return getter() == 42 ? 0 : 1; } }";
        var forHeader = Compile(forHeaderSource);
        Assert.True(forHeader.Succeeded, string.Join(Environment.NewLine, forHeader.Diagnostics));
        var forHeaderExecution = LlvmJit.RunMain(forHeader.LlvmIr!);
        Assert.Equal(0, forHeaderExecution.ExitCode);
        Assert.Equal(0, forHeaderExecution.OutstandingAllocations);

        const string repeatedForHeaderSource = "delegate int32 Getter(); class Program { public static int32 Main() { Getter first = () => 0; Getter second = () => 0; for (int32 round = 0; round < 2; round++) { for (int32 i = round * 10; i < round * 10 + 1; i++) { if (round == 0) first = () => i; else second = () => i; } } return first() == 1 && second() == 11 ? 0 : 1; } }";
        var repeatedForHeader = Compile(repeatedForHeaderSource);
        Assert.True(repeatedForHeader.Succeeded, string.Join(Environment.NewLine, repeatedForHeader.Diagnostics));
        var repeatedForHeaderExecution = LlvmJit.RunMain(repeatedForHeader.LlvmIr!);
        Assert.Equal(0, repeatedForHeaderExecution.ExitCode);
        Assert.Equal(0, repeatedForHeaderExecution.OutstandingAllocations);

        const string forInCaptureSource = "delegate int32 Getter(); class Program { public static int32 Main() { Getter first = () => 0; Getter second = () => 0; int32[] values = { 10, 20 }; for (let value in values) { if (value == 10) first = () => value; else second = () => value; } return first() == 10 && second() == 20 ? 0 : 1; } }";
        var forInCapture = Compile(forInCaptureSource);
        Assert.True(forInCapture.Succeeded, string.Join(Environment.NewLine, forInCapture.Diagnostics));
        var forInCaptureExecution = LlvmJit.RunMain(forInCapture.LlvmIr!);
        Assert.Equal(0, forInCaptureExecution.ExitCode);
        Assert.Equal(0, forInCaptureExecution.OutstandingAllocations);

        const string enumerableCaptureSource = "interface IEnumerable<T> { IIterator<T> Iterate(); } interface IIterator<T> { bool Next(); T Current { get; } } class CountingIterator : IIterator<int32> { int32 current = -1; public bool Next() mut { current++; return current < 2; } public int32 Current => current; } class Values : IEnumerable<int32> { public IIterator<int32> Iterate() => new CountingIterator(); } delegate int32 Getter(); class Program { public static int32 Main() { Getter first = () => 0; Getter second = () => 0; int32 index = 0; for (let value in new Values()) { if (index == 0) first = () => value; else second = () => value; index++; } return first() == 0 && second() == 1 ? 0 : 1; } }";
        var enumerableCapture = Compile(enumerableCaptureSource);
        Assert.True(enumerableCapture.Succeeded, string.Join(Environment.NewLine, enumerableCapture.Diagnostics));
        var enumerableCaptureExecution = LlvmJit.RunMain(enumerableCapture.LlvmIr!);
        Assert.Equal(0, enumerableCaptureExecution.ExitCode);
        Assert.Equal(0, enumerableCaptureExecution.OutstandingAllocations);

        const string conditionalForHeaderSource = "delegate int32 Getter(); class Program { public static int32 Main() { try { if (false) { for (int32 i = 0; i < 1; i++) { Getter getter = () => i; } } return 0; } catch (System.OutOfMemoryException error) { return 1; } } }";
        var conditionalForHeader = Compile(conditionalForHeaderSource);
        Assert.True(conditionalForHeader.Succeeded, string.Join(Environment.NewLine, conditionalForHeader.Diagnostics));
        var conditionalForHeaderExecution = LlvmJit.RunMain(conditionalForHeader.LlvmIr!, allocationFailureAfter: 0);
        Assert.Equal(0, conditionalForHeaderExecution.ExitCode);
        Assert.Equal(0, conditionalForHeaderExecution.OutstandingAllocations);

        const string enteredForHeaderSource = "delegate int32 Getter(); class Program { public static int32 Main() { try { for (int32 i = 0; i < 0; i++) { Getter getter = () => i; } return 1; } catch (System.OutOfMemoryException error) { return 0; } } }";
        var enteredForHeader = Compile(enteredForHeaderSource);
        Assert.True(enteredForHeader.Succeeded, string.Join(Environment.NewLine, enteredForHeader.Diagnostics));
        var enteredForHeaderExecution = LlvmJit.RunMain(enteredForHeader.LlvmIr!, allocationFailureAfter: 0);
        Assert.Equal(0, enteredForHeaderExecution.ExitCode);
        Assert.Equal(0, enteredForHeaderExecution.OutstandingAllocations);

        const string parameterSource = "delegate int32 Counter(); class Program { public static Counter Create(int32 start) { return () => ++start; } public static int32 Main() { Counter counter = Create(10); if (counter() != 11) return 1; return counter() == 12 ? 0 : 2; } }";
        var parameter = Compile(parameterSource);
        Assert.True(parameter.Succeeded, string.Join(Environment.NewLine, parameter.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(parameter.LlvmIr!).ExitCode);

        const string sharedSource = "delegate void Action(); class Program { public static int32 Main() { int32 value = 0; Action first = () => { value++; }; Action second = () => { value += 2; }; first(); second(); return value == 3 ? 0 : 1; } }";
        var shared = Compile(sharedSource);
        Assert.True(shared.Succeeded, string.Join(Environment.NewLine, shared.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(shared.LlvmIr!).ExitCode);

        const string genericSource = "delegate T Mapper<T>(T value); class Program { public static int32 Main() { int32 factor = 2; Mapper<int32> multiply = value => value * factor; return multiply(21); } }";
        var generic = Compile(genericSource);
        Assert.True(generic.Succeeded, string.Join(Environment.NewLine, generic.Diagnostics));
        Assert.Equal(42, LlvmJit.RunMain(generic.LlvmIr!).ExitCode);

        const string referenceSource = "delegate int32 Reader(); class Value { public static int32 finalized; public int32 number = 42; ~this() { Value.finalized += 1; } } class Program { public static Reader Create() { Value value = new Value(); return () => value.number; } public static int32 Main() { Reader? reader = Create(); if (Value.finalized != 0 || reader!() != 42) return 1; reader = null; return Value.finalized == 1 ? 0 : 2; } }";
        var reference = Compile(referenceSource);
        Assert.True(reference.Succeeded, string.Join(Environment.NewLine, reference.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(reference.LlvmIr!).ExitCode);

        const string referenceParameterSource = "delegate int32 Reader(); class Value { public int32 number = 42; } class Program { public static Reader Create(Value value) { return () => value.number; } public static int32 Main() { Reader reader = Create(new Value()); return reader() == 42 ? 0 : 1; } }";
        var referenceParameter = Compile(referenceParameterSource);
        Assert.True(referenceParameter.Succeeded, string.Join(Environment.NewLine, referenceParameter.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(referenceParameter.LlvmIr!).ExitCode);

        const string receiverSource = "delegate int32 Reader(); class Counter { public static int32 finalized; private int32 value = 42; private int32 Read() { return value; } public Reader MakeExplicit() { return () => this.value; } public Reader MakeImplicit() { return () => Read(); } ~this() { Counter.finalized += 1; } } class Program { public static int32 Main() { Reader? saved = null; { Counter counter = new Counter(); Reader explicitRead = counter.MakeExplicit(); Reader implicitRead = counter.MakeImplicit(); if (explicitRead() != 42 || implicitRead() != 42) return 1; saved = counter.MakeImplicit(); } if (Counter.finalized != 0 || saved!() != 42) return 2; saved = null; return Counter.finalized == 1 ? 0 : 3; } }";
        var receiver = Compile(receiverSource);
        Assert.True(receiver.Succeeded, string.Join(Environment.NewLine, receiver.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(receiver.LlvmIr!).ExitCode);
    }

    [Fact]
    public void LambdasImplementRefInAndOutDelegateParameters()
    {
        const string source = "delegate int32 RefUpdate(ref int32 value); delegate void OutAssign(out int32 value); delegate int32 InRead(in int32 value); class Program { public static int32 Main() { RefUpdate update = value => ++value; int32 number = 40; if (update(ref number) != 41 || number != 41) return 1; OutAssign assign = value => { value = 42; }; int32 output; assign(out output); if (output != 42) return 2; InRead read = value => value; return read(number) == 41 ? 0 : 3; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);

        var invalidInMutation = Compile("delegate void InAction(in int32 value); class Program { public static int32 Main() { InAction action = value => { value = 1; }; return 0; } }");
        Assert.Contains(invalidInMutation.Diagnostics, diagnostic => diagnostic.Code == "LMC3012");
    }

    [Fact]
    public void CapturingLambdasRejectCapturesWithUnsupportedEscapeRules()
    {
        var refParameter = Compile("delegate int32 Getter(); class Program { public static Getter Capture(ref int32 value) { return () => value; } public static int32 Main() { return 0; } }");
        Assert.Contains(refParameter.Diagnostics, diagnostic => diagnostic.Code == "LS0730" && diagnostic.Message.Contains("ref parameter", StringComparison.Ordinal));

        var readonlyParameter = Compile("delegate int32 Getter(); class Program { public static Getter Capture(readonly int32 value) { return () => value; } public static int32 Main() { return 0; } }");
        Assert.Contains(readonlyParameter.Diagnostics, diagnostic => diagnostic.Code == "LS0701" && diagnostic.Message.Contains("scope-bound", StringComparison.Ordinal));

        var nestedBlock = Compile("delegate int32 Getter(); class Program { public static Getter Capture() { Getter result = () => 0; { int32 value = 40; result = () => ++value; } return result; } public static int32 Main() { Getter getter = Capture(); if (getter() != 41) return 1; return getter() == 42 ? 0 : 2; } }");
        Assert.True(nestedBlock.Succeeded, string.Join(Environment.NewLine, nestedBlock.Diagnostics));
        var nestedBlockExecution = LlvmJit.RunMain(nestedBlock.LlvmIr!);
        Assert.Equal(0, nestedBlockExecution.ExitCode);
        Assert.Equal(0, nestedBlockExecution.OutstandingAllocations);

        var switchSection = Compile("delegate int32 Getter(); class Program { public static int32 Main() { Getter getter = () => 0; switch (1) { case 1: int32 value = 42; getter = () => value; value += 1; break; default: break; } return getter() == 43 ? 0 : 1; } }");
        Assert.True(switchSection.Succeeded, string.Join(Environment.NewLine, switchSection.Diagnostics));
        var switchSectionExecution = LlvmJit.RunMain(switchSection.LlvmIr!);
        Assert.Equal(0, switchSectionExecution.ExitCode);
        Assert.Equal(0, switchSectionExecution.OutstandingAllocations);
        Assert.Null(switchSectionExecution.InteropError);

        var switchPatternCapture = Compile("delegate int32 Getter(); class Box { public int32 Value = 42; } class Program { public static int32 Main() { Getter getter = () => 0; object value = new Box(); switch (value) { case Box matched: getter = () => matched.Value; break; default: break; } return getter() == 42 ? 0 : 1; } }");
        Assert.True(switchPatternCapture.Succeeded, string.Join(Environment.NewLine, switchPatternCapture.Diagnostics));
        var switchPatternExecution = LlvmJit.RunMain(switchPatternCapture.LlvmIr!);
        Assert.Equal(0, switchPatternExecution.ExitCode);
        Assert.Equal(0, switchPatternExecution.OutstandingAllocations);
        Assert.Null(switchPatternExecution.InteropError);

        var switchGuardCapture = Compile("delegate bool Guard(); class Box { public int32 Value = 42; } class Program { public static Guard? Saved; public static bool Save(Guard guard) { Saved = guard; return false; } public static int32 Main() { int32 expected = 42; object value = new Box(); switch (value) { case Box matched when Save(() => matched.Value == expected): return 1; default: break; } Guard saved = Saved!; Saved = null; return saved() ? 0 : 2; } }");
        Assert.True(switchGuardCapture.Succeeded, string.Join(Environment.NewLine, switchGuardCapture.Diagnostics));
        var switchGuardExecution = LlvmJit.RunMain(switchGuardCapture.LlvmIr!);
        Assert.Equal(0, switchGuardExecution.ExitCode);
        Assert.Equal(0, switchGuardExecution.OutstandingAllocations);
        Assert.Null(switchGuardExecution.InteropError);

        var catchCapture = Compile("delegate int32 Getter(); class Failure : System.Exception { public int32 Value = 42; } class Program { public static Getter? Saved; public static int32 Main() { try { throw new Failure(); } catch (Failure error) { Saved = () => error.Value; } Getter getter = Saved!; Saved = null; return getter() == 42 ? 0 : 1; } }");
        Assert.True(catchCapture.Succeeded, string.Join(Environment.NewLine, catchCapture.Diagnostics));
        var catchCaptureExecution = LlvmJit.RunMain(catchCapture.LlvmIr!);
        Assert.Equal(0, catchCaptureExecution.ExitCode);
        Assert.Equal(0, catchCaptureExecution.OutstandingAllocations);
        Assert.Null(catchCaptureExecution.InteropError);

        var conditionPatternCapture = Compile("delegate int32 Getter(); class Box { public int32 Value = 42; } class Program { public static Getter? Saved; public static int32 Main() { object value = new Box(); if (value is Box matched) Saved = () => matched.Value; Getter inIf = Saved!; Saved = null; if (inIf() != 42) return 1; int32 iterations = 0; while (value is Box current && iterations < 1) { Saved = () => current.Value; iterations++; } Getter inWhile = Saved!; Saved = null; if (inWhile() != 42) return 2; int32 count = 0; for (; value is Box each && count < 1; count++) Saved = () => each.Value; Getter inFor = Saved!; Saved = null; return inFor() == 42 ? 0 : 3; } }");
        Assert.True(conditionPatternCapture.Succeeded, string.Join(Environment.NewLine, conditionPatternCapture.Diagnostics));
        var conditionPatternExecution = LlvmJit.RunMain(conditionPatternCapture.LlvmIr!);
        Assert.Equal(0, conditionPatternExecution.ExitCode);
        Assert.Equal(0, conditionPatternExecution.OutstandingAllocations);
        Assert.Null(conditionPatternExecution.InteropError);

        var shortCircuitPatternCapture = Compile("delegate int32 Getter(); class Box { public int32 Value = 42; } class Program { public static Getter? Saved; public static int32 Main() { object value = new Box(); if (value is Box matched && (Saved = () => matched.Value) != null) { if (matched.Value != 42) return 3; Getter getter = Saved!; Saved = null; return getter() == 42 ? 0 : 1; } return 2; } } ");
        Assert.True(shortCircuitPatternCapture.Succeeded, string.Join(Environment.NewLine, shortCircuitPatternCapture.Diagnostics));
        var shortCircuitPatternExecution = LlvmJit.RunMain(shortCircuitPatternCapture.LlvmIr!);
        Assert.Equal(0, shortCircuitPatternExecution.ExitCode);
        Assert.Equal(0, shortCircuitPatternExecution.OutstandingAllocations);
        Assert.Null(shortCircuitPatternExecution.InteropError);

        var repeatedNestedBlock = Compile("delegate int32 Getter(); class Program { public static int32 Main() { Getter first = () => 0; Getter second = () => 0; for (int32 round = 0; round < 2; round++) { int32 value = round * 10; if (round == 0) first = () => value; else second = () => value; } return first() == 0 && second() == 10 ? 0 : 1; } }");
        Assert.True(repeatedNestedBlock.Succeeded, string.Join(Environment.NewLine, repeatedNestedBlock.Diagnostics));
        var repeatedNestedBlockExecution = LlvmJit.RunMain(repeatedNestedBlock.LlvmIr!);
        Assert.Equal(0, repeatedNestedBlockExecution.ExitCode);
        Assert.Equal(0, repeatedNestedBlockExecution.OutstandingAllocations);

        var mixedBlockScope = Compile("delegate int32 Getter(); class Program { public static Getter Capture() { int32 outer = 1; Getter result = () => 0; { int32 inner = 40; result = () => ++outer + ++inner; } return result; } public static int32 Main() { Getter getter = Capture(); if (getter() != 43) return 1; return getter() == 45 ? 0 : 2; } }");
        Assert.True(mixedBlockScope.Succeeded, string.Join(Environment.NewLine, mixedBlockScope.Diagnostics));
        var mixedBlockScopeExecution = LlvmJit.RunMain(mixedBlockScope.LlvmIr!);
        Assert.Equal(0, mixedBlockScopeExecution.ExitCode);
        Assert.Equal(0, mixedBlockScopeExecution.OutstandingAllocations);

        var mixedNestedBlocks = Compile("delegate int32 Getter(); class Program { public static int32 Main() { Getter getter = () => 0; { int32 outer = 20; { int32 inner = 22; getter = () => outer + inner; } } return getter() == 42 ? 0 : 1; } }");
        Assert.True(mixedNestedBlocks.Succeeded, string.Join(Environment.NewLine, mixedNestedBlocks.Diagnostics));
        var mixedNestedBlocksExecution = LlvmJit.RunMain(mixedNestedBlocks.LlvmIr!);
        Assert.Equal(0, mixedNestedBlocksExecution.ExitCode);
        Assert.Equal(0, mixedNestedBlocksExecution.OutstandingAllocations);

        var mixedRepeatedScope = Compile("delegate int32 Getter(); class Program { public static int32 Main() { int32 outer = 5; Getter first = () => 0; Getter second = () => 0; for (int32 round = 0; round < 2; round++) { int32 inner = round; if (round == 0) first = () => outer + inner; else second = () => outer + inner; } return first() == 5 && second() == 6 ? 0 : 1; } }");
        Assert.True(mixedRepeatedScope.Succeeded, string.Join(Environment.NewLine, mixedRepeatedScope.Diagnostics));
        var mixedRepeatedScopeExecution = LlvmJit.RunMain(mixedRepeatedScope.LlvmIr!);
        Assert.Equal(0, mixedRepeatedScopeExecution.ExitCode);
        Assert.Equal(0, mixedRepeatedScopeExecution.OutstandingAllocations);

        var mixedReceiverAndBlock = Compile("delegate int32 Getter(); class Counter { public int32 value = 41; public Getter Capture() { Getter result = () => 0; { int32 offset = 1; result = () => this.value + offset; } return result; } } class Program { public static int32 Main() { Getter getter = () => 0; { Counter counter = new Counter(); getter = counter.Capture(); } return getter() == 42 ? 0 : 1; } }");
        Assert.True(mixedReceiverAndBlock.Succeeded, string.Join(Environment.NewLine, mixedReceiverAndBlock.Diagnostics));
        var mixedReceiverAndBlockExecution = LlvmJit.RunMain(mixedReceiverAndBlock.LlvmIr!);
        Assert.Equal(0, mixedReceiverAndBlockExecution.ExitCode);
        Assert.Equal(0, mixedReceiverAndBlockExecution.OutstandingAllocations);

        var mixedForHeaderScope = Compile("delegate int32 Getter(); class Program { public static int32 Main() { int32 outer = 1; Getter getter = () => 0; for (int32 index = 0; index < 1; index++) getter = () => outer + index; return getter() == 2 ? 0 : 1; } }");
        Assert.True(mixedForHeaderScope.Succeeded, string.Join(Environment.NewLine, mixedForHeaderScope.Diagnostics));
        var mixedForHeaderScopeExecution = LlvmJit.RunMain(mixedForHeaderScope.LlvmIr!);
        Assert.Equal(0, mixedForHeaderScopeExecution.ExitCode);
        Assert.Equal(0, mixedForHeaderScopeExecution.OutstandingAllocations);

        var mixedForInScope = Compile("delegate int32 Getter(); class Program { public static int32 Main() { int32 outer = 1; int32[] values = { 2 }; Getter getter = () => 0; for (let value in values) getter = () => outer + value; return getter() == 3 ? 0 : 1; } }");
        Assert.True(mixedForInScope.Succeeded, string.Join(Environment.NewLine, mixedForInScope.Diagnostics));
        var mixedForInScopeExecution = LlvmJit.RunMain(mixedForInScope.LlvmIr!);
        Assert.Equal(0, mixedForInScopeExecution.ExitCode);
        Assert.Equal(0, mixedForInScopeExecution.OutstandingAllocations);

        var nestedLambda = Compile("delegate int32 Getter(); delegate Getter Factory(); class Program { public static Factory Capture() { int32 value = 40; return () => () => ++value; } public static int32 Main() { Factory factory = Capture(); Getter getter = factory(); if (getter() != 41) return 1; return getter() == 42 ? 0 : 2; } }");
        Assert.True(nestedLambda.Succeeded, string.Join(Environment.NewLine, nestedLambda.Diagnostics));
        var nestedLambdaExecution = LlvmJit.RunMain(nestedLambda.LlvmIr!);
        Assert.Equal(0, nestedLambdaExecution.ExitCode);
        Assert.Equal(0, nestedLambdaExecution.OutstandingAllocations);

        var nestedLambdaParameter = Compile("delegate int32 Getter(); delegate Getter Factory(int32 amount); class Program { public static Factory Capture() { int32 offset = 40; return amount => () => offset + amount; } public static int32 Main() { Factory factory = Capture(); Getter getter = factory(2); return getter() == 42 ? 0 : 1; } }");
        Assert.True(nestedLambdaParameter.Succeeded, string.Join(Environment.NewLine, nestedLambdaParameter.Diagnostics));
        var nestedLambdaParameterExecution = LlvmJit.RunMain(nestedLambdaParameter.LlvmIr!);
        Assert.Equal(0, nestedLambdaParameterExecution.ExitCode);
        Assert.Equal(0, nestedLambdaParameterExecution.OutstandingAllocations);

        var baseCapture = Compile("delegate int32 Getter(); virtual class Base { public int32 value = 40; public virtual int32 Value => value; public virtual int32 Read() { return value; } } class Derived : Base { public override int32 Value => 99; public override int32 Read() { return 99; } public Getter Capture() { Getter getter = () => 0; { int32 offset = 2; getter = () => base.Read() + base.value + base.Value + offset; } return getter; } } class Program { public static int32 Main() { Getter getter = () => 0; { Derived derived = new Derived(); getter = derived.Capture(); } return getter() == 122 ? 0 : 1; } }");
        Assert.True(baseCapture.Succeeded, string.Join(Environment.NewLine, baseCapture.Diagnostics));
        var baseCaptureExecution = LlvmJit.RunMain(baseCapture.LlvmIr!);
        Assert.Equal(0, baseCaptureExecution.ExitCode);
        Assert.Equal(0, baseCaptureExecution.OutstandingAllocations);

        var nestedBaseCapture = Compile("delegate int32 Getter(); delegate Getter Factory(); virtual class Base { public int32 value = 40; public virtual int32 Read() { return value; } } class Derived : Base { public override int32 Read() { return 99; } public Factory Capture() { return () => () => base.Read() + base.value; } } class Program { public static int32 Main() { Factory factory = () => () => 0; { Derived derived = new Derived(); factory = derived.Capture(); } Getter getter = factory(); return getter() == 80 ? 0 : 1; } }");
        Assert.True(nestedBaseCapture.Succeeded, string.Join(Environment.NewLine, nestedBaseCapture.Diagnostics));
        var nestedBaseCaptureExecution = LlvmJit.RunMain(nestedBaseCapture.LlvmIr!);
        Assert.Equal(0, nestedBaseCaptureExecution.ExitCode);
        Assert.Equal(0, nestedBaseCaptureExecution.OutstandingAllocations);

        var constructorCapture = Compile("delegate int32 Getter(); class Value { } class Counter { public Value item; public this() { Getter getter = () => this.Read(); item = new Value(); } public int32 Read() { return 0; } } class Program { public static int32 Main() { Counter counter = new Counter(); return 0; } }");
        Assert.Contains(constructorCapture.Diagnostics, diagnostic => diagnostic.Code == "LS0812" && diagnostic.Message.Contains("cannot be captured", StringComparison.Ordinal));

        var structReceiverCapture = Compile("delegate int32 Getter(); struct Counter { public int32 value; public Getter Capture() { return () => this.value; } } class Program { public static int32 Main() { Counter counter = default; counter.value = 42; Getter getter = counter.Capture(); counter.value = 1; return getter() == 42 ? 0 : 1; } }");
        Assert.True(structReceiverCapture.Succeeded, string.Join(Environment.NewLine, structReceiverCapture.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(structReceiverCapture.LlvmIr!).ExitCode);

        var mutatingStructReceiverCapture = Compile("delegate int32 Getter(); struct Counter { public int32 value; public Getter Capture() mut { return () => ++value; } } class Program { public static int32 Main() { Counter counter = default; counter.value = 40; Getter getter = counter.Capture(); return getter() == 41 && getter() == 42 && counter.value == 40 ? 0 : 1; } }");
        Assert.True(mutatingStructReceiverCapture.Succeeded, string.Join(Environment.NewLine, mutatingStructReceiverCapture.Diagnostics));
        var mutatingStructExecution = LlvmJit.RunMain(mutatingStructReceiverCapture.LlvmIr!);
        Assert.Equal(0, mutatingStructExecution.ExitCode);
        Assert.Equal(0, mutatingStructExecution.OutstandingAllocations);

        var managedStructReceiverCapture = Compile("delegate int32 Getter(); class Node { public static int32 destroyed; private int32 value; public this(int32 value) { this.value = value; } public int32 Read() { return value; } ~this() { Node.destroyed++; } } struct Holder { Node? node; public this(Node value) { node = value; } public Getter Capture() { return () => node!.Read(); } } class Program { public static int32 Main() { Getter? getter = null; { Holder holder = Holder(new Node(42)); getter = holder.Capture(); } if (Node.destroyed != 0 || getter!() != 42) return 1; getter = null; return Node.destroyed == 1 ? 0 : 2; } }");
        Assert.True(managedStructReceiverCapture.Succeeded, string.Join(Environment.NewLine, managedStructReceiverCapture.Diagnostics));
        var managedStructExecution = LlvmJit.RunMain(managedStructReceiverCapture.LlvmIr!);
        Assert.Equal(0, managedStructExecution.ExitCode);
        Assert.Equal(0, managedStructExecution.OutstandingAllocations);

        var nonMutatingStructCapture = Compile("delegate int32 Getter(); struct Counter { public int32 value; public Getter Capture() { return () => ++value; } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(nonMutatingStructCapture.Diagnostics, diagnostic => diagnostic.Code == "LS0821");

        var scopeBound = Compile("delegate Sample Factory(); class Sample { } class Program { public static Factory Capture() { Sample value = scope Sample(); return () => value; } public static int32 Main() { return 0; } }");
        Assert.Contains(scopeBound.Diagnostics, diagnostic => diagnostic.Code == "LS0701" && diagnostic.Message.Contains("scope-bound", StringComparison.Ordinal));

        var mutatingReceiver = Compile("delegate void Action(); class Counter { public int32 value; public Action Capture() { return () => this.value++; } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(mutatingReceiver.Diagnostics, diagnostic => diagnostic.Code == "LS0821" && diagnostic.Message.Contains("non-mutating method", StringComparison.Ordinal));

        var mutatingBaseReceiver = Compile("delegate void Action(); virtual class Base { public void Touch() mut { } } class Derived : Base { public Action Capture() { return () => base.Touch(); } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(mutatingBaseReceiver.Diagnostics, diagnostic => diagnostic.Code == "LS0821" && diagnostic.Message.Contains("non-mutating method", StringComparison.Ordinal));

        var scopedReceiver = Compile("delegate void Action(); class Counter { public void Touch() { } public Action Capture() { return () => Touch(); } } class Program { public static int32 Main() { Counter counter = scope Counter(); Action action = counter.Capture(); return 0; } }");
        Assert.Contains(scopedReceiver.Diagnostics, diagnostic => diagnostic.Code == "LS0702");

        var mutatingCapture = Compile("delegate void Action(); class Counter { public int32 value; public Action Capture() mut { return () => this.value++; } } class Program { public static int32 Main() { Counter counter = new Counter(); Action action = counter.Capture(); action(); return counter.value == 1 ? 0 : 1; } }");
        Assert.True(mutatingCapture.Succeeded, string.Join(Environment.NewLine, mutatingCapture.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(mutatingCapture.LlvmIr!).ExitCode);
    }

    [Fact]
    public void ScopeBoundLambdasCanCaptureScopedReferencesWithinTheirLifetime()
    {
        const string source = "delegate int32 Getter(); class Sample { public int32 value = 42; public int32 Read() { return value; } } class Program { public static int32 Main() { Sample sample = scope Sample(); Getter outer = () => sample.Read(); { Getter inner = () => sample.Read(); if (inner() != 42) return 1; } return outer() == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void ScopeBoundLambdasMayBePassedDirectlyToReadonlyDelegateParameters()
    {
        const string source = "delegate int32 Getter(); class Sample { public int32 Read() { return 42; } } class Program { public static int32 Invoke(readonly Getter getter) { return getter(); } public static int32 Main() { Sample sample = scope Sample(); return Invoke(() => sample.Read()) == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void ScopeBoundLambdaCaptureStillRejectsEscapingReceiverCalls()
    {
        const string source = "delegate void Action(); class Sample { public static Sample? Last; public void Remember() { Sample.Last = this; } } class Program { public static int32 Main() { Sample sample = scope Sample(); Action remember = () => sample.Remember(); remember(); return 0; } }";
        var result = Compile(source);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS0702");
    }

    [Fact]
    public void InlineOutLocalsDeclaredInNestedBlocksCanBeCaptured()
    {
        const string source = "delegate int32 Getter(); class Program { public static bool Try(out int32 value) { value = 40; return true; } public static int32 Main() { Getter getter = () => 0; { if (Try(out int32 captured)) getter = () => ++captured; } return getter() == 41 && getter() == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void NonGenericInterfaceMethodImplementationsAreChecked()
    {
        var missing = Compile("interface IValue { int32 Read(); } class Box : IValue { } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(missing.Diagnostics, diagnostic => diagnostic.Code == "LS1001" && diagnostic.Severity == DiagnosticSeverity.Error);
        var inaccessible = Compile("interface IValue { int32 Read(); } class Box : IValue { private int32 Read() { return 1; } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(inaccessible.Diagnostics, diagnostic => diagnostic.Code == "LS1001");
        var wrongSignature = Compile("interface IValue { int32 Read(); } class Box : IValue { public int32 Read(int32 extra) { return extra; } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(wrongSignature.Diagnostics, diagnostic => diagnostic.Code == "LS1001");

        const string inheritedContract = "interface IBase { int32 Read(); } interface IChild : IBase { bool Ready(); } class Box : IChild { public int32 Read() { return 42; } } class Program { public static int32 Main() { return 0; } }";
        var missingInherited = Compile(inheritedContract);
        Assert.Contains(missingInherited.Diagnostics, diagnostic => diagnostic.Code == "LS1001" && diagnostic.Message.Contains("Ready", StringComparison.Ordinal));
        var inheritedImplemented = Compile(inheritedContract.Replace("public int32 Read() { return 42; }", "public int32 Read() { return 42; } public bool Ready() { return true; }", StringComparison.Ordinal));
        Assert.True(inheritedImplemented.Succeeded, string.Join(Environment.NewLine, inheritedImplemented.Diagnostics));

        var implemented = Compile("interface IValue { int32 Read(); } class Box : IValue { public int32 Read() { return 42; } } class Program { public static int32 Main() { Box value = new Box(); return value.Read() == 42 ? 0 : 1; } }");
        Assert.True(implemented.Succeeded, string.Join(Environment.NewLine, implemented.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(implemented.LlvmIr!).ExitCode);

        var missingProperty = Compile("interface IValue { int32 Value { get; } } class Box : IValue { } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(missingProperty.Diagnostics, diagnostic => diagnostic.Code == "LS1001");
        var missingSetter = Compile("interface IValue { int32 Value { get; set; } } class Box : IValue { public int32 Value { get; } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(missingSetter.Diagnostics, diagnostic => diagnostic.Code == "LS1001");

        var implementedProperty = Compile("interface IValue { int32 Value { get; set; } } class Box : IValue { public int32 Value { get; set; } } class Program { public static int32 Main() { Box box = new Box(); box.Value = 42; return box.Value == 42 ? 0 : 1; } }");
        Assert.True(implementedProperty.Succeeded, string.Join(Environment.NewLine, implementedProperty.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(implementedProperty.LlvmIr!).ExitCode);

        var inheritedProperty = Compile("interface IValue { int32 Value { get; set; } } virtual class Base { public int32 Value { get; set; } } class Box : Base, IValue { } class Program { public static int32 Main() { Box box = new Box(); box.Value = 42; return box.Value == 42 ? 0 : 1; } }");
        Assert.True(inheritedProperty.Succeeded, string.Join(Environment.NewLine, inheritedProperty.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(inheritedProperty.LlvmIr!).ExitCode);

        var implementedIndexer = Compile("interface IValue { int32 this[int32 index] { get; } } class Box : IValue { public int32 this[int32 index] => index + 40; } class Program { public static int32 Main() { Box box = new Box(); return box[2] == 42 ? 0 : 1; } }");
        Assert.True(implementedIndexer.Succeeded, string.Join(Environment.NewLine, implementedIndexer.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(implementedIndexer.LlvmIr!).ExitCode);
        var missingIndexer = Compile("interface IValue { int32 this[int32 index] { get; } } class Box : IValue { } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(missingIndexer.Diagnostics, diagnostic => diagnostic.Code == "LS1001");
    }

    [Fact]
    public void InterfaceReferencesConvertDispatchAndPreserveManagedOwnership()
    {
        const string source = "interface IReader { int32 Read(); } interface IChildReader : IReader { } " +
            "class FirstReader : IReader { public int32 Read() { return 41; } } " +
            "class SecondReader : IChildReader { public int32 Read() { return 42; } } " +
            "class Program { public static IReader Create() { return new FirstReader(); } " +
            "public static int32 Invoke(IReader reader) { return reader.Read(); } " +
            "public static int32 Main() { IReader first = Create(); IReader second = new SecondReader(); " +
            "return Invoke(first) == 41 && Invoke(second) == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(result.LlvmIr!).ExitCode);

        var invalidConversion = Compile("interface IReader { int32 Read(); } class Empty { } " +
            "class Program { public static int32 Main() { IReader reader = new Empty(); return 0; } }");
        Assert.Contains(invalidConversion.Diagnostics, diagnostic => diagnostic.Code == "LMC3010" &&
            diagnostic.Message.Contains("Cannot convert", StringComparison.Ordinal));
    }

    [Fact]
    public void InterfacePropertiesAndIndexersDispatchThroughAccessorSlots()
    {
        const string source = "interface IValue { int32 Value { get; set; } int32 this[int32 index] { get; set; } } " +
            "class Box : IValue { public int32 Value { get; set; } " +
            "public int32 this[int32 index] { get => index + 40; set { } } } " +
            "class Program { public static int32 Main() { IValue value = new Box(); value.Value = 2; value[0] = 3; " +
            "return value.Value == 2 && value[2] == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(result.LlvmIr!).ExitCode);

        var inaccessible = Compile("interface IValue { int32 Value { get; } int32 this[int32 index] { get; } } " +
            "class Box : IValue { int32 IValue.Value => 1; int32 IValue.this[int32 index] => index; } " +
            "class Program { public static int32 Main() { Box box = new Box(); return box.Value + box[0]; } }");
        Assert.False(inaccessible.Succeeded);
    }

    [Fact]
    public void InterfaceTypeTestsAndConditionalCastsUseRuntimeImplementationMetadata()
    {
        const string source = "interface IReader { int32 Read(); } " +
            "class Reader : IReader { public int32 Read() { return 42; } } class Empty { } " +
            "class Program { public static int32 Main() { Empty empty = new Empty(); " +
            "if (empty is IReader) { return 1; } IReader? missing = empty as IReader; " +
            "if (missing != null) { return 2; } Reader reader = new Reader(); " +
            "if (!(reader is IReader)) { return 3; } IReader value = reader as IReader; " +
            "IReader checkedValue = (IReader)reader; " +
            "return value.Read() == 42 && checkedValue.Read() == 42 ? 0 : 4; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(result.LlvmIr!).ExitCode);
    }

    [Fact]
    public void ExplicitInterfaceMethodsResolveNameClashesOnlyThroughInterfaceReferences()
    {
        const string source = "interface ILeft { int32 Read(); } interface IRight { int32 Read(); } " +
            "class Both : ILeft, IRight { int32 ILeft.Read() { return 11; } int32 IRight.Read() { return 22; } } " +
            "class Program { public static int32 Main() { Both both = new Both(); ILeft left = both; IRight right = both; " +
            "return left.Read() == 11 && right.Read() == 22 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(result.LlvmIr!).ExitCode);

        var inaccessible = Compile("interface IReader { int32 Read(); } class Reader : IReader { int32 IReader.Read() { return 1; } } " +
            "class Program { public static int32 Main() { Reader reader = new Reader(); return reader.Read(); } }");
        Assert.Contains(inaccessible.Diagnostics, diagnostic => diagnostic.Code == "LS0200" &&
            diagnostic.Message.Contains("no accessible instance method named 'Read'", StringComparison.Ordinal));

        var invalidQualifier = Compile("interface IReader { int32 Read(); } interface IOther { } " +
            "class Reader : IReader { int32 IOther.Read() { return 1; } }");
        Assert.Contains(invalidQualifier.Diagnostics, diagnostic => diagnostic.Code == "LMC3010" &&
            diagnostic.Message.Contains("not an interface implemented", StringComparison.Ordinal));
    }

    [Fact]
    public void ExplicitInterfacePropertiesAndIndexersUsePerInterfaceAccessorSlots()
    {
        const string source = "interface ILeft { int32 Value { get; set; } int32 this[int32 index] { get; set; } } " +
            "interface IRight { int32 Value { get; set; } int32 this[int32 index] { get; set; } } " +
            "class Both : ILeft, IRight { int32 ILeft.Value { get; set; } int32 IRight.Value { get; set; } " +
            "int32 ILeft.this[int32 index] { get => index + 10; set { } } " +
            "int32 IRight.this[int32 index] { get => index + 20; set { } } } " +
            "class Program { public static int32 Main() { Both both = new Both(); ILeft left = both; IRight right = both; " +
            "left.Value = 1; right.Value = 2; left[0] = 3; right[0] = 4; " +
            "return left.Value == 1 && right.Value == 2 && left[1] == 11 && right[1] == 21 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(0, LlvmJit.RunMain(result.LlvmIr!).ExitCode);
    }

    [Fact]
    public void EnumerableForInReportsAmbiguityAndHonorsExplicitElementTypeSelection()
    {
        const string source = "interface IEnumerable<T> { } class Items : IEnumerable<int32>, IEnumerable<string> { } class Program { public static int32 Main() { Items items = new Items(); for (let value in items) { } return 0; } }";
        var ambiguous = Compile(source);
        Assert.Contains(ambiguous.Diagnostics, diagnostic => diagnostic.Code == "LS0640" && diagnostic.Severity == DiagnosticSeverity.Error);

        var selected = Compile(source.Replace("for (let value in items)", "for (int32 value in items)", StringComparison.Ordinal));
        Assert.DoesNotContain(selected.Diagnostics, diagnostic => diagnostic.Code == "LS0640");
        Assert.Contains(selected.Diagnostics, diagnostic => diagnostic.Code == "LMC3000" && diagnostic.Message.Contains("iteration protocol", StringComparison.Ordinal));

        var invalidSelection = Compile(source.Replace("for (let value in items)", "for (bool value in items)", StringComparison.Ordinal));
        Assert.DoesNotContain(invalidSelection.Diagnostics, diagnostic => diagnostic.Code == "LS0640");
        Assert.Contains(invalidSelection.Diagnostics, diagnostic => diagnostic.Code == "LMC3010" && diagnostic.Message.Contains("No IEnumerable<T> element type", StringComparison.Ordinal));

        const string inheritedSource = "interface IEnumerable<T> { } interface IntItems : IEnumerable<int32> { } interface TextItems : IEnumerable<string> { } class Items : IntItems, TextItems { } class Program { public static int32 Main() { Items items = new Items(); for (let value in items) { } return 0; } }";
        var inherited = Compile(inheritedSource);
        Assert.Contains(inherited.Diagnostics, diagnostic => diagnostic.Code == "LS0640");
    }

    [Fact]
    public void EnumerableForInLowersIteratorProtocolAndReleasesIteratorObjects()
    {
        const string source = "interface IEnumerable<T> { IIterator<T> Iterate(); } interface IIterator<T> { bool Next(); T Current { get; } } " +
            "class CountingIterator : IIterator<int32> { int32 current; public bool Next() mut { current++; return current <= 3; } public int32 Current { get { return current * 10; } } } " +
            "class Numbers : IEnumerable<int32> { public IIterator<int32> Iterate() { return new CountingIterator(); } } " +
            "class Program { public static int32 Main() { Numbers numbers = new Numbers(); int32 sum = 0; for (let value in numbers) { sum += value; } " +
            "IEnumerable<int32> enumerable = numbers; int32 continued = 0; for (let value in enumerable) { if (value == 20) continue; continued += value; } " +
            "int32 count = 0; for (let value in numbers) { count++; if (value == 20) break; } return sum == 60 && continued == 40 && count == 2 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void EnumerableForInRejectsMalformedIteratorProtocols()
    {
        const string wrongReturnType = "interface IEnumerable<T> { int32 Iterate(); } " +
            "class Items : IEnumerable<int32> { public int32 Iterate() { return 0; } } " +
            "class Program { public static int32 Main() { Items items = new Items(); for (let item in items) { } return 0; } }";
        const string wrongNextType = "interface IEnumerable<T> { IIterator<T> Iterate(); } interface IIterator<T> { int32 Next(); T Current { get; } } " +
            "class Iterator : IIterator<int32> { public int32 Next() { return 1; } public int32 Current { get { return 1; } } } " +
            "class Items : IEnumerable<int32> { public IIterator<int32> Iterate() { return new Iterator(); } } " +
            "class Program { public static int32 Main() { Items items = Items(); for (let item in items) { } return 0; } }";
        const string wrongCurrentType = "interface IEnumerable<T> { IIterator<T> Iterate(); } interface IIterator<T> { bool Next(); string Current { get; } } " +
            "class Iterator : IIterator<int32> { public bool Next() { return false; } public string Current { get { return \"\"; } } } " +
            "class Items : IEnumerable<int32> { public IIterator<int32> Iterate() { return new Iterator(); } } " +
            "class Program { public static int32 Main() { Items items = new Items(); for (let item in items) { } return 0; } }";

        foreach (var source in new[] { wrongReturnType, wrongNextType, wrongCurrentType })
        {
            var result = Compile(source);
            Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LMC3000" &&
                (diagnostic.Message.Contains("iteration protocol", StringComparison.Ordinal) ||
                 diagnostic.Message.Contains("IIterator<T> contract", StringComparison.Ordinal)));
        }
    }

    [Fact]
    public void EnumerableForInDiagnosesNullableSourcesAndSupportsStructBoxing()
    {
        const string nullableSource = "interface IEnumerable<T> { IIterator<T> Iterate(); } interface IIterator<T> { bool Next(); T Current { get; } } " +
            "class Items : IEnumerable<int32> { public IIterator<int32> Iterate() { return null; } } " +
            "class Program { public static int32 Main() { Items? items = null; for (let item in items) { } return 0; } }";
        var nullable = Compile(nullableSource);
        Assert.Contains(nullable.Diagnostics, diagnostic => diagnostic.Code == "LS1303");

        const string structSource = "interface IEnumerable<T> { IIterator<T> Iterate(); } interface IIterator<T> { bool Next(); T Current { get; } } " +
            "class Iterator : IIterator<int32> { public bool Next() { return false; } public int32 Current { get { return 0; } } } " +
            "struct Items : IEnumerable<int32> { public IIterator<int32> Iterate() { return new Iterator(); } } " +
            "class Program { public static int32 Main() { Items items = Items(); for (let item in items) { } return 0; } }";
        var structure = Compile(structSource);
        Assert.True(structure.Succeeded, string.Join(Environment.NewLine, structure.Diagnostics));
        var execution = LlvmJit.RunMain(structure.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void EnumerableForInReleasesIteratorWhenTheLoopBodyThrows()
    {
        const string source = "interface IEnumerable<T> { IIterator<T> Iterate(); } interface IIterator<T> { bool Next(); T Current { get; } } " +
            "class Iterator : IIterator<int32> { public bool Next() { return true; } public int32 Current { get { return 1; } } } " +
            "class Items : IEnumerable<int32> { public IIterator<int32> Iterate() { return new Iterator(); } } " +
            "class Program { public static int32 Main() { Items items = new Items(); try { for (let item in items) { throw new System.Exception(\"stop\"); } } " +
            "catch (System.Exception) { } return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void SpecializedGenericInterfacePropertiesAndIndexersValidateAndDispatch()
    {
        const string source = "interface IBox<T> { T Value { get; } T this[int32 index] { get; } } " +
            "class Box : IBox<int32> { public int32 Value { get { return 40; } } public int32 this[int32 index] { get { return Value + index; } } } " +
            "class Program { public static int32 Main() { IBox<int32> box = new Box(); return box.Value == 40 && box[2] == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void UnsafePointerMemberAccessAutomaticallyDereferencesClassAndStructLocations()
    {
        const string source = "struct Vector { public int32 x; public int32 X { get { return x; } set { x = value; } } " +
            "public int32 this[int32 index] { get { return x + index; } set { x = value - index; } } " +
            "public void Move(int32 amount) mut { x += amount; } public int32 Read() { return x; } } " +
            "virtual class Box { public int32 value; public int32 Value { get { return value; } set { this.value = value; } } " +
            "public int32 this[int32 index] { get { return value + index; } set { this.value = value - index; } } " +
            "public void Add(int32 amount) mut { value += amount; } public virtual int32 Read() { return value; } } " +
            "class DerivedBox : Box { public override int32 Read() { return value + 1; } } " +
            "class Program { public static unsafe int32 Main() { Vector vector = default; vector.x = 1; Vector* vectorPointer = &vector; " +
            "vectorPointer.x = 7; vectorPointer.X = 8; vectorPointer[1] = 20; vectorPointer.Move(2); " +
            "Box box = new DerivedBox(); Box* boxPointer = &box; Box dereferenced = *boxPointer; boxPointer.value = 10; boxPointer.Value = 11; boxPointer[1] = 20; boxPointer.Add(3); " +
            "return vectorPointer.Read() != 21 ? 2 : vectorPointer.X != 21 ? 3 : vectorPointer[1] != 22 ? 4 : " +
            "boxPointer.Read() != 23 ? 5 : boxPointer.Value != 22 ? 6 : boxPointer[1] != 23 ? 7 : dereferenced != box ? 8 : 0; } }";
        var result = Compile(source, new CompilationOptions(UnsafeEnabled: true));
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        const string classFieldAddress = "class Box { public int32 value; } class Holder { public Box child = new Box(); } " +
            "unsafe class Program { public static int32 Main() { Holder holder = new Holder(); holder.child.value = 4; Box* pointer = &holder.child; pointer.value = 9; return holder.child.value == 9 ? 0 : 1; } }";
        var fieldResult = Compile(classFieldAddress, new CompilationOptions(UnsafeEnabled: true));
        Assert.True(fieldResult.Succeeded, string.Join(Environment.NewLine, fieldResult.Diagnostics));
        var fieldExecution = LlvmJit.RunMain(fieldResult.LlvmIr!);
        Assert.Equal(0, fieldExecution.ExitCode);
        Assert.Equal(0, fieldExecution.OutstandingAllocations);
        Assert.Null(fieldExecution.InteropError);

        var invalidClassDerefAssignment = Compile("unsafe class Box { public static int32 Main() { Box value = new Box(); Box* pointer = &value; *pointer = value; return 0; } }", new CompilationOptions(UnsafeEnabled: true));
        Assert.Contains(invalidClassDerefAssignment.Diagnostics, diagnostic => diagnostic.Code == "LMC3012");
        Assert.Null(invalidClassDerefAssignment.LlvmIr);

        const string pointerToPointer = "struct Point { public int32 x; } class Program { public static unsafe int32 Main() { " +
            "Point point = default; Point* pointer = &point; Point** pointerToPointer = &pointer; return pointerToPointer.x; } }";
        var invalid = Compile(pointerToPointer, new CompilationOptions(UnsafeEnabled: true));
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == "LMC3010" &&
            diagnostic.Message.Contains("pointer-to-pointer", StringComparison.Ordinal));

        const string unsafeOptionRequired = "struct Point { public int32 x; } class Program { public static int32 Main() { " +
            "unsafe { Point point = default; Point* pointer = &point; return pointer.x; } } }";
        var disabled = Compile(unsafeOptionRequired);
        Assert.Contains(disabled.Diagnostics, diagnostic => diagnostic.Code == "LS1901" &&
            diagnostic.Message.Contains("Pointer member access", StringComparison.Ordinal));
    }

    [Fact]
    public void UnsafeRequiresProjectSetting()
    {
        var result = Compile("class Program { public static void Main() { unsafe { } } }");
        Assert.Contains(result.Diagnostics, d => d.Code == "LS1901");
    }

    [Fact]
    public void UnsafeMethodModifierEnablesUncheckedExpressionWhenConfigured()
    {
        const string source = "class Program { public static unsafe int32 Main() { return unchecked(2147483647 + 1); } }";
        var enabled = Compile(source, new CompilationOptions(UnsafeEnabled: true));
        Assert.True(enabled.Succeeded, string.Join(Environment.NewLine, enabled.Diagnostics));
        Assert.Contains("add i32 2147483647, 1", enabled.LlvmIr!);
        Assert.DoesNotContain("@llvm.sadd.with.overflow.i32(i32 2147483647", enabled.LlvmIr);
        var disabled = Compile(source);
        Assert.Contains(disabled.Diagnostics, d => d.Code == "LS1901");
    }

    [Fact]
    public void StaticUsingDirectivesAreReportedAsUnsupported()
    {
        var result = Compile("using static Utilities; class Program { public static void Main() {} }");
        Assert.Contains(result.Diagnostics, d => d.Code == "LMC3000" && d.Message.Contains("Static using directive"));
    }

    [Fact]
    public void BreakOutsideLoopIsRejected()
    {
        var result = Compile("class Program { public static void Main() { break; } }");
        Assert.Contains(result.Diagnostics, d => d.Code == "LS0605");
    }

    [Fact]
    public void UnicodeIdentifierCompiles()
    {
        var result = Compile("class Program { public static int32 Main() { let Ω2 = 7; return Ω2; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));

        var supplementaryLetter = Compile("class Program { public static int32 Main() { let \U00010400Value = 7; return \U00010400Value; } }");
        Assert.True(supplementaryLetter.Succeeded, string.Join(Environment.NewLine, supplementaryLetter.Diagnostics));
    }

    [Fact]
    public void UnicodeIdentifiersRejectCombiningMarksAndLetterNumbers()
    {
        // Version 0.3 identifiers are letters/underscore followed by letters,
        // decimal digits, or underscore; combining marks and Unicode letter
        // numbers are deliberately outside that grammar.
        var combiningMark = Compile("class Program { public static int32 Main() { let e\u0301 = 7; return 0; } }");
        Assert.Contains(combiningMark.Diagnostics, d => d.Code == "LS0001" && d.Message.Contains("Unexpected character", StringComparison.Ordinal));

        var letterNumber = Compile("class Program { public static int32 Main() { let Ⅷ = 7; return 0; } }");
        Assert.Contains(letterNumber.Diagnostics, d => d.Code == "LS0001" && d.Message.Contains("Unexpected character", StringComparison.Ordinal));
    }

    [Fact]
    public void RejectsUnescapedKeywordsAsNamesButAcceptsEscapedIdentifiers()
    {
        var invalid = Compile("class Program { public static void Main() { int32 return = 1; } }");
        Assert.Contains(invalid.Diagnostics, d => d.Code == "LS0001");

        var escaped = Compile("class Program { public static int32 Main() { int32 @return = 1; return @return; } }");
        Assert.True(escaped.Succeeded, string.Join(Environment.NewLine, escaped.Diagnostics));
    }

    [Fact]
    public void DiagnosticColumnsCountUtf16CodeUnits()
    {
        const string source = "class Program { public static int32 Main() { let note = \"😀\"; return missing; } }";
        var result = Compile(source);
        var diagnostic = Assert.Single(result.Diagnostics, d => d.Code == "LS0200");
        Assert.NotNull(diagnostic.Location);
        Assert.Equal(source.IndexOf("missing", StringComparison.Ordinal) + 1, diagnostic.Location!.Column);
        Assert.Equal(1, diagnostic.Location.Line);
    }

    [Fact]
    public void SupportsUtf8StringLiteralsLengthIndexingEqualityAndAbiValues()
    {
        const string source = """
            class Program {
                public static string Empty = "";
                public static string Name = "h\u00E9\U0001F642";
                public static int32 Main() {
                    string absent = "";
                    string empty = "";
                    string text = "h\u00E9\U0001F642";
                    string copy = Echo(text);
                    Program.Name = text;
                    if (text == Program.Name && copy == "h\u00E9\U0001F642" && absent == empty && Program.Empty == "")
                        return text.Length + (int32)text[0] + (int32)text[2];
                    return 0;
                }
                public static string Echo(string value) { return value; }
            }
            """;
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var ir = result.LlvmIr!;
        Assert.Contains("%luma.string = type { ptr, i32, i32, ptr, i32, i32, i8, i8, ptr }", ir);
        Assert.Contains("@luma_string_empty = private unnamed_addr constant %luma.string", ir);
        Assert.Contains("i32 4, i32 7", ir); // UTF-16 code units and UTF-8 bytes for h + é + 🙂.
        Assert.Contains("i16 55357, i16 56898", ir); // The supplementary scalar is cached as its UTF-16 surrogate pair.
        Assert.Contains("call i1 @luma_string_equals", ir);
        Assert.Contains("getelementptr %luma.string", ir);
        Assert.Contains("define internal i32 @luma_string_length(ptr %input)", ir);
        Assert.Contains("define internal i16 @luma_string_char(ptr %input, i32 %index)", ir);
        Assert.Contains("getelementptr i16, ptr", ir);
        Assert.Contains("call ptr @luma_Program_Echo_1", ir);
        Assert.Contains("store ptr", ir);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-string-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try
        {
            var emitted = LlvmToolchain.VerifyAndEmitObject(ir, objectPath, "host", optimize: true);
            Assert.True(emitted.Succeeded, emitted.Error);
            Assert.True(File.Exists(objectPath));
        }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }
    }

    [Fact]
    public void SupportsVerbatimStringsEscapesAndEmbeddedNullStorage()
    {
        var result = Compile("class Program { public static int32 Main() { string path = @\"C:\\folder\"\"name\"\"\"; string text = \"A\\0B\"; return path.Length + text.Length; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("i8] c\"\\41\\00\\42\\00\"", result.LlvmIr!);
        Assert.Contains("i32 3, i32 3", result.LlvmIr);
    }

    [Fact]
    public void DecodesEverySpecifiedStringEscapeToUtf16CodeUnits()
    {
        const string source = """
            class Program {
                public static int32 Main() {
                    string value = "\0\a\b\f\n\r\t\v\\\'\"\x41";
                    return value.Length;
                }
            }
            """;
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("[12 x i16] [i16 0, i16 7, i16 8, i16 12, i16 10, i16 13, i16 9, i16 11, i16 92, i16 39, i16 34, i16 65]", result.LlvmIr!);

        var malformedHex = Compile("class Program { public static int32 Main() { string value = \"\\x\"; return 0; } }");
        Assert.Contains(malformedHex.Diagnostics, diagnostic => diagnostic.Code == "LS0001" && diagnostic.Message.Contains("one to four hexadecimal digits", StringComparison.Ordinal));
    }

    [Fact]
    public void SupportsDynamicStringConcatenationAndDiagnosesUnsupportedConversions()
    {
        var concatenation = Compile("""
            class Program {
                public static int32 Main() {
                    string first = Echo("a");
                    string second = "b";
                    string text = first + second;
                    text += Echo("c");
                    {
                        string scoped = Echo(text);
                        if (scoped != "abc") return 1;
                    }
                    for (int32 i = 0; i < 2; i++) {
                        string item = Echo("!");
                        if (i == 0) continue;
                        if (i == 1) break;
                    }
                    string chosen = true ? text : "unused";
                    Echo(chosen);
                    string assigned;
                    assigned = Echo(chosen);
                    if (assigned == "abc") return assigned.Length;
                    return 0;
                }
                public static string Echo(string value) { return value; }
            }
            """);
        Assert.True(concatenation.Succeeded, string.Join(Environment.NewLine, concatenation.Diagnostics));
        var ir = concatenation.LlvmIr!;
        Assert.Contains("define internal ptr @luma_string_concat", ir);
        Assert.Contains("%luma.typedesc = type", ir);
        Assert.Contains("@luma_string_type = private constant %luma.typedesc", ir);
        Assert.Contains("call ptr @luma_string_concat", ir);
        Assert.Contains("call void @luma_string_retain", ir);
        Assert.Contains("call void @luma_string_release", ir);
        Assert.Contains("cmpxchg ptr %count.address", ir);
        Assert.Contains("atomicrmw sub ptr %count.address", ir);
        Assert.Contains("phi ptr", ir);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-string-concat-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try
        {
            var emitted = LlvmToolchain.VerifyAndEmitObject(ir, objectPath, "host", optimize: true);
            Assert.True(emitted.Succeeded, emitted.Error);
        }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }

        var nonStringConversion = Compile("class Program { public static int32 Main() { unsafe { string text = \"count=\" + (int32*)0; return text.Length; } } }", new CompilationOptions(UnsafeEnabled: true));
        Assert.Contains(nonStringConversion.Diagnostics, diagnostic => diagnostic.Code == "LMC3000" && diagnostic.Message.Contains("cannot convert 'int32*' to text", StringComparison.OrdinalIgnoreCase));

        var malformedEscape = Compile("class Program { public static int32 Main() { string text = \"bad\\q\"; return 0; } }");
        Assert.Contains(malformedEscape.Diagnostics, diagnostic => diagnostic.Code == "LS0001" && diagnostic.Message.Contains("Unknown string escape", StringComparison.Ordinal));

        var invalidUnicode = Compile("class Program { public static int32 Main() { string text = \"\\uD800\"; return 0; } }");
        Assert.Contains(invalidUnicode.Diagnostics, diagnostic => diagnostic.Code == "LS0001" && diagnostic.Message.Contains("unpaired UTF-16 surrogate", StringComparison.Ordinal));

        var malformedConstant = Compile("class Config { public const string Invalid = \"bad\\q\"; } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(malformedConstant.Diagnostics, diagnostic => diagnostic.Code == "LS0001" && diagnostic.Message.Contains("Unknown string escape", StringComparison.Ordinal));

        var nullable = Compile("class Program { public static int32 Main() { string? text = null; return 0; } }");
        Assert.True(nullable.Succeeded, string.Join(Environment.NewLine, nullable.Diagnostics));
        var nullableExecution = LlvmJit.RunMain(nullable.LlvmIr!);
        Assert.Equal(0, nullableExecution.ExitCode);
        Assert.Equal(0, nullableExecution.OutstandingAllocations);
        Assert.DoesNotContain(nullable.Diagnostics, diagnostic => diagnostic.Code == "LS0001");
    }

    [Fact]
    public void StringConcatenationUsesTheSpecifiedDefaultNameForEnums()
    {
        const string source = "enum State { Ready, Running } class Program { public static int32 Main() { State current = State.Running; string fromLocal = \"state=\" + current; string fromConstant = State.Ready + \"!\"; string fromMethod = current.ToString(); return fromLocal == \"state=State\" && fromConstant == \"State!\" && fromMethod == \"State\" ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));

        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

    }

    [Fact]
    public void DefaultClassToStringUsesTheRuntimeTypeDescriptor()
    {
        const string source = "virtual class Animal { public this() { } } virtual class Dog : Animal { public this() : base() { } } class Program { public static int32 Main() { Animal animal = new Dog(); string direct = animal.ToString(); string joined = \"type=\" + animal; Animal? absent = null; string nullText = \"absent=\" + absent; string? absentString = absent?.ToString(); return direct == \"Dog\" && joined == \"type=Dog\" && nullText == \"absent=\" && absentString == null ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("i32 9", result.LlvmIr!, StringComparison.Ordinal);

        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void CustomClassToStringOverridesDispatchThroughBaseReferences()
    {
        const string source = "interface INode { } virtual class Node : INode { public this() { } public override string ToString() { return \"node\"; } } virtual class DerivedNode : Node { public this() : base() { } public override string ToString() { return \"derived\"; } } virtual class Root { public this() { } } virtual class NameNode : Root { public this() : base() { } public override string ToString() { return \"root=\" + base.ToString(); } } class Program { public static int32 Main() { Node value = new DerivedNode(); INode asInterface = value; string direct = value.ToString(); string viaInterface = asInterface.ToString(); string joined = \"name=\" + value; INode? absent = null; string? absentString = absent?.ToString(); Root root = new NameNode(); if (direct != \"derived\") return 1; if (viaInterface != \"derived\") return 2; if (joined != \"name=derived\") return joined.Length; if (absentString != null) return 4; if (root.ToString() != \"root=NameNode\") return 5; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var missingOverride = Compile("class Node { public string ToString() { return \"node\"; } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(missingOverride.Diagnostics, diagnostic => diagnostic.Code == "LS0833");
    }

    [Fact]
    public void SystemObjectIsAnImplicitRootAndDispatchesToStringDynamically()
    {
        const string source = "class Plain { public this() { } } virtual class BaseNode { public this() { } public override string ToString() { return \"base\"; } } virtual class ChildNode : BaseNode { public this() : base() { } public override string ToString() { return \"child\"; } } class Program { public static int32 Main() { object value = new Plain(); System.Object qualified = value; object? optional = value; object child = new ChildNode(); object root = new object(); return value.ToString() == \"Plain\" && qualified.ToString() == \"Plain\" && optional?.ToString() == \"Plain\" && child.ToString() == \"child\" && root.ToString() == \"System.Object\" && value is object && child is System.Object ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));

        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var reservedRoot = Compile("namespace System; public class Object { } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(reservedRoot.Diagnostics, diagnostic => diagnostic.Code == "LS0001");

        var primitiveBoxing = Compile("class Program { public static int32 Main() { int32 value = 1; object boxed = value; return boxed is int32 ? 0 : 1; } }");
        Assert.True(primitiveBoxing.Succeeded, string.Join(Environment.NewLine, primitiveBoxing.Diagnostics));
        var primitiveBoxingExecution = LlvmJit.RunMain(primitiveBoxing.LlvmIr!);
        Assert.Equal(0, primitiveBoxingExecution.ExitCode);
        Assert.Equal(0, primitiveBoxingExecution.OutstandingAllocations);
        var structBoxing = Compile("struct Point { public int32 x; } class Program { public static int32 Main() { Point value = default; object boxed = value; return boxed is Point ? 0 : 1; } }");
        Assert.True(structBoxing.Succeeded, string.Join(Environment.NewLine, structBoxing.Diagnostics));
        var structBoxingExecution = LlvmJit.RunMain(structBoxing.LlvmIr!);
        Assert.Equal(0, structBoxingExecution.ExitCode);
        Assert.Equal(0, structBoxingExecution.OutstandingAllocations);
    }

    [Fact]
    public void ReferenceFreeStructsBoxUnboxAndParticipateInRuntimeTypePatterns()
    {
        const string source = """
            struct Point {
                public int32 X;
                public int64 Y;
                public this(int32 x, int64 y) { X = x; Y = y; }
            }
            struct Pair { public Point Value; public int32 Tag; }
            class Program {
                public static int32 Main() {
                    Point original = Point(7, 9);
                    object boxed = original;
                    original.X = 100;
                    if (!(boxed is Point)) return 1;
                    if (boxed is Point matched) { if (matched.X != 7 || matched.Y != 9) return 2; }
                    else return 3;
                    Point recovered = (Point)boxed;
                    if (recovered.X != 7 || recovered.Y != 9) return 4;
                    Point? conditional = boxed as Point;
                    if (conditional == null || conditional!.X != 7) return 5;
                    Point? checkedNullable = (Point?)boxed;
                    if (checkedNullable == null || checkedNullable!.Y != 9) return 6;
                    Point? absent = null;
                    object? nullBox = absent;
                    if (nullBox != null) return 7;
                    Point? present = Point(11, 13);
                    object presentBox = present;
                    Point? roundTrip = (Point?)presentBox;
                    if (roundTrip == null || roundTrip!.X != 11 || roundTrip.Y != 13) return 8;
                    if (presentBox is Pair) return 9;
                    Point fromSwitch = default;
                    switch (boxed) {
                        case Point selected: fromSwitch = selected; break;
                        default: return 10;
                    }
                    if (fromSwitch.Y != 9) return 11;
                    switch (recovered) {
                        case Point direct: if (direct.X != 7) return 12; break;
                    }
                    try { Pair invalid = (Pair)boxed; return 14; }
                    catch (InvalidCastException) { }
                    try { object? nothing = null; Point invalidNull = (Point)nothing; return 15; }
                    catch (NullReferenceException) { }
                    try { Point? invalidNullable = (Point?)new object(); return 16; }
                    catch (InvalidCastException) { }
                    return boxed.ToString() == "Point" && presentBox.ToString() == "Point" ? 0 : 17;
                }
            }
            """;
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("%luma.structbox.Point", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("@luma_boxdesc_Point", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var managedReferenceStruct = Compile("class Node { } struct Holder { public Node? Value; } class Program { public static int32 Main() { Node node = new Node(); Holder holder = default; holder.Value = node; object boxed = holder; holder.Value = null; Holder copy = (Holder)boxed; return copy.Value == node ? 0 : 1; } }");
        Assert.True(managedReferenceStruct.Succeeded, string.Join(Environment.NewLine, managedReferenceStruct.Diagnostics));
        var managedReferenceExecution = LlvmJit.RunMain(managedReferenceStruct.LlvmIr!);
        Assert.Equal(0, managedReferenceExecution.ExitCode);
        Assert.Equal(0, managedReferenceExecution.OutstandingAllocations);
    }

    [Fact]
    public void StructMethodGroupDelegatesRetainAndMutateTheirBoxedCopy()
    {
        const string source = "delegate int32 ValueReader(); struct Counter { private int32 _value; public this(int32 initial) { _value = initial; } public int32 ReadValue() { return _value; } public int32 Increment() mut { _value++; return _value; } } class Program { public static int32 Main() { Counter original = Counter(10); ValueReader read = original.ReadValue; ValueReader increment = original.Increment; if (original.ReadValue() != 10 || read() != 10) return 1; if (increment() != 11 || increment() != 12) return 2; if (read() != 10 || original.ReadValue() != 10) return 3; object boxed = original; Counter unboxed = (Counter)boxed; unboxed.Increment(); if (original.ReadValue() != 10 || unboxed.ReadValue() != 11) return 4; return increment() == 13 && read() == 10 ? 0 : 5; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("luma_struct_delegate_ValueReader_luma_Counter_inst_Increment_0", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ReferenceBearingStructsBoxUnboxAndDispatchWithoutLosingFieldOwnership()
    {
        const string source = "interface IValue { int32 Read(); } delegate int32 Reader(); class Node { public int32 Value; public this(int32 value) { Value = value; } } struct Payload : IValue { public string? Label; public Node? Owner; public this(string? label, Node? owner) { Label = label; Owner = owner; } public int32 Read() { return Owner!.Value; } public int32 Increment() mut { Owner!.Value++; return Owner!.Value; } } class Program { public static int32 Main() { Node node = new Node(10); Payload original = Payload(\"payload\", node); object boxed = original; original.Owner = null; Payload unboxed = (Payload)boxed; if (unboxed.Label != \"payload\" || unboxed.Owner!.Value != 10 || original.Owner != null) return 1; unboxed.Label = \"copy\"; if (unboxed.Label != \"copy\" || ((Payload)boxed).Label != \"payload\") return 2; Payload? conditional = boxed as Payload; if (conditional == null || conditional!.Read() != 10) return 3; if (boxed is Payload matched) { if (matched.Label != \"payload\") return 4; } else return 4; Payload switched = default; switch (boxed) { case Payload selected: switched = selected; break; default: return 5; } if (switched.Read() != 10) return 6; IValue contract = (IValue)boxed; if (contract.Read() != 10) return 7; Reader reader = unboxed.Increment; if (reader() != 11 || node.Value != 11 || contract.Read() != 11) return 8; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("luma_structbox_release_Payload", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("luma_struct_iface_Payload_", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ReferenceFreeStructsBoxToInterfacesAndDispatchThroughTheBox()
    {
        const string source = "interface ICounter { int32 Read(); int32 Increment(); } struct Counter : ICounter { private int32 _value; public this(int32 initial) { _value = initial; } public int32 Read() { return _value; } public int32 Increment() mut { _value++; return _value; } public override string ToString() { return \"counter=\" + _value; } } class Program { public static T Advance<T>(T value) where T : struct, ICounter { value.Increment(); return value; } public static int32 Main() { Counter value = Counter(10); ICounter contract = value; if (contract.Read() != 10) return 1; if (contract.Increment() != 11 || contract.Increment() != 12) return 2; if (contract.Read() != 12 || value.Read() != 10) return 3; object boxed = value; if (!(boxed is ICounter) || boxed.ToString() != \"counter=10\") return 4; ICounter fromBox = (ICounter)boxed; if (fromBox.Read() != 10) return 5; if (fromBox.Increment() != 11 || fromBox.Read() != 11) return 6; Counter advanced = Advance(value); if (advanced.Read() != 11 || value.Read() != 10) return 7; Counter? absent = null; ICounter? noContract = absent; if (noContract != null) return 8; Counter? present = Counter(20); ICounter? nullableContract = present; if (nullableContract == null || nullableContract!.Read() != 20) return 9; return nullableContract.Increment() == 21 && nullableContract.Read() == 21 ? 0 : 10; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("@luma_struct_itable_Counter", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("@luma_struct_iface_Counter_", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void StructsRecursivelyRetainAndReleaseNullableManagedFields()
    {
        const string source = "class Node { public int32 Value; public this(int32 value) { Value = value; } } struct Payload { public string? Label; public Node? Owner; public this(string? label, Node? owner) { Label = label; Owner = owner; } } struct Envelope { public Payload Data; } unsafe struct Batch { public Payload[2] Items; } class Holder { private Envelope _data; public this(Envelope data) { _data = data; } public Envelope Read() { return _data; } } class Program { public static unsafe int32 Main() { Node node = new Node(7); Payload first = Payload(\"left\" + \"one\", node); Payload alias = first; alias.Label = \"right\" + \"two\"; alias.Owner = null; if (first.Label != \"leftone\" || first.Owner!.Value != 7) return 1; if (alias.Label != \"righttwo\" || alias.Owner != null) return 2; Batch batch = default; batch.Items[1] = first; Payload batchCopy = batch.Items[1]; batch.Items[1] = default; if (batchCopy.Label != \"leftone\" || batchCopy.Owner!.Value != 7) return 3; Envelope envelope = default; envelope.Data = first; Holder holder = new Holder(envelope); envelope.Data = default; Envelope returned = holder.Read(); if (returned.Data.Label != \"leftone\" || returned.Data.Owner!.Value != 7) return 4; returned.Data.Label = null; if (returned.Data.Label != null) return 5; return 0; } }";
        var result = Compile(source, new CompilationOptions(UnsafeEnabled: true));
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("luma_aggregate_retain_Payload_", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("luma_aggregate_release_Envelope_", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("luma_aggregate_release_Batch_", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var invalid = Compile("struct Invalid { public string Value; } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(invalid.Diagnostics, diagnostic => diagnostic.Code == "LS0902");
        Assert.Null(invalid.LlvmIr);
    }

    [Fact]
    public void NullableReferenceBearingStructCoalescingTransfersOwnership()
    {
        const string source = "class Node { public int32 Value; public this(int32 value) { Value = value; } } struct Payload { public string? Label; public Node? Owner; public this(string? label, Node? owner) { Label = label; Owner = owner; } } class Program { public static int32 Main() { Node node = new Node(17); Payload original = Payload(\"original\" + \" value\", node); Payload? absent = null; Payload fallback = absent ?? Payload(\"fallback\", node); if (fallback.Label != \"fallback\" || fallback.Owner!.Value != 17) return 1; Payload? present = original; Payload retained = present ?? Payload(\"unused\", null); if (retained.Label != \"original value\" || retained.Owner!.Value != 17) return 2; Payload? assigned = null; Payload? assignmentResult = assigned ??= original; if (assignmentResult == null || assignmentResult!.Label != \"original value\" || assigned!.Owner!.Value != 17) return 3; Payload? alreadyAssigned = original; Payload? existingResult = alreadyAssigned ??= Payload(\"unused again\", null); if (existingResult == null || existingResult!.Label != \"original value\") return 4; absent = null; Payload? nullableFallback = absent ?? (Payload?)null; if (nullableFallback != null) return 5; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ManagedArraysOwnReferenceBearingStructElements()
    {
        const string source = "class Node { public int32 Value; public this(int32 value) { Value = value; } } struct Payload { public string? Label; public Node? Owner; public this(string? label, Node? owner) { Label = label; Owner = owner; } } class Program { public static int32 Main() { Node node = new Node(23); Payload first = Payload(\"one\", node); Payload second = Payload(\"two\", node); Payload[] values = new Payload[2]; values[0] = first; values[1] = second; Payload copy = values[0]; values[0] = default; if (copy.Label != \"one\" || copy.Owner!.Value != 23) return 1; Payload[] initialized = new Payload[] { first, second }; int32 total = 0; for (Payload item in initialized) { if (item.Owner != null) total += item.Owner!.Value; } if (total != 46) return 2; Payload? optional = values?[1]; if (optional == null || optional!.Label != \"two\" || optional.Owner!.Value != 23) return 3; values[1] = default; if (optional!.Owner!.Value != 23) return 4; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("luma_aggregate_release_Payload_", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("_element_size = private constant i32", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void PrimitiveAndEnumBoxesSupportExactTypeTestsCastsAndNullableBoxing()
    {
        const string source = "enum Tone : int32 { Warm = 3, Cool = 4 } class Program { public static int32 Main() { int32 value = 42; object boxed = value; object alias = boxed; if (!(boxed is int32) || boxed is int64) return 1; int32? maybe = boxed as int32; if (maybe != 42) return 2; int32 unboxed = (int32)alias; if (unboxed != 42) return 3; try { int64 mismatch = (int64)boxed; return 4; } catch (InvalidCastException) { } object? nullableObject = null; try { int32 missing = (int32)nullableObject; return 5; } catch (NullReferenceException) { } int32? empty = null; object? emptyBox = empty; if (emptyBox != null) return 6; int32? nullableSource = 9; object nullableBox = nullableSource; int32? nullableUnbox = (int32?)nullableBox; if (nullableUnbox != 9) return 7; Tone color = Tone.Cool; object colorBox = color; if (!(colorBox is Tone)) return 8; Tone recovered = (Tone)colorBox; if (recovered != Tone.Cool || colorBox is int32) return 9; object doubleBox = 3.25; double floating = (double)doubleBox; if (floating != 3.25) return 10; if (doubleBox as int32 != null) return 11; bool flag = true; object boolBox = flag; if (!(boolBox is bool) || !(bool)boolBox) return 12; char letter = 'A'; object charBox = letter; if ((char)charBox != 'A') return 13; int8 small = -12; object smallBox = small; if ((int8)smallBox != -12) return 14; uint64 wide = 18446744073709551615u; object wideBox = wide; if ((uint64)wideBox != wide) return 15; float single = 1.25f; object floatBox = single; if ((float)floatBox != single) return 16; return boxed.ToString() == \"int32\" && colorBox.ToString() == \"Tone\" ? 0 : 17; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("@luma_boxdesc_int32", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("@luma_boxdesc_Tone", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        const string allocationFailure = "class Program { public static int32 Main() { try { object boxed = 7; return 1; } catch (OutOfMemoryException) { return 0; } } }";
        var allocationFailureResult = Compile(allocationFailure);
        Assert.True(allocationFailureResult.Succeeded, string.Join(Environment.NewLine, allocationFailureResult.Diagnostics));
        var allocationFailureExecution = LlvmJit.RunMain(allocationFailureResult.LlvmIr!, allocationFailureAfter: 0);
        Assert.Equal(0, allocationFailureExecution.ExitCode);
        Assert.Equal(0, allocationFailureExecution.OutstandingAllocations);
        Assert.Null(allocationFailureExecution.InteropError);
    }

    [Fact]
    public void NestedClassTypesResolveByQualifiedAndContainingScopeNames()
    {
        const string source = "public class Outer { private int32 secret = 9; class Hidden { public int32 Value() { return 2; } } public struct Pair { public int32 x; } public enum Kind { First, Second } public interface IValue { int32 Value(); } public class Inner : IValue { public int32 Value() { return 5; } public int32 Read(Outer owner) { return owner.secret; } } public Pair MakePair() { Pair pair = default; pair.x = 4; return pair; } public Kind GetKind() { return Kind.Second; } public int32 ReadNested() { Inner inner = new Inner(); return inner.Read(this); } public int32 CreateHidden() { Hidden hidden = new Hidden(); return hidden.Value(); } } class Program { public static int32 Main() { Outer outer = new Outer(); Outer.Inner nested = new Outer.Inner(); Outer.Pair pair = outer.MakePair(); Outer.IValue contract = new Outer.Inner(); Outer.Kind kind = outer.GetKind(); return outer.ReadNested() == 9 && outer.CreateHidden() == 2 && pair.x == 4 && contract.Value() == 5 && kind == Outer.Kind.Second ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("luma_Outer_Inner", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Contains("%struct.Outer_Pair = type", result.LlvmIr!, StringComparison.Ordinal);

        var inaccessible = Compile("public class Outer { class Hidden { } public int32 Create() { Hidden value = new Hidden(); return 0; } } class Program { public static int32 Main() { Outer.Hidden value = new Outer.Hidden(); return 0; } }");
        Assert.False(inaccessible.Succeeded);
        Assert.Contains(inaccessible.Diagnostics, diagnostic => diagnostic.Code == "LMC3012" && diagnostic.Message.Contains("inaccessible", StringComparison.Ordinal));
    }

    [Fact]
    public void NestedTypeAccessibilityAppliesToTypeExpressionsAndGenericArguments()
    {
        const string source = "public class Outer { private class Hidden { public class Nested { } } public static object Create() { return new Hidden(); } } class Box<T> { } class Child : Outer.Hidden { } class TypeUse<T> where T : Outer.Hidden { } class ConstructorUse<T> where T : new(Outer.Hidden) { } class GenericMethods { public static int32 Ignore<T>() { return 0; } } class Program { public static T MethodUse<T>(T value) where T : Outer.Hidden { return value; } public static int32 Main() { object value = Outer.Create(); bool matched = value is Outer.Hidden; bool nestedMatched = value is Outer.Hidden.Nested; object? casted = value as Outer.Hidden; object? defaulted = default(Outer.Hidden); int32 size = sizeof(Outer.Hidden); Box<Outer.Hidden> box = new Box<Outer.Hidden>(); Box<Outer.Hidden[]> arrayBox = new Box<Outer.Hidden[]>(); int32 explicitTypeArgument = GenericMethods.Ignore<Outer.Hidden>(); return matched || nestedMatched || casted != null || defaulted != null || size != 0 || explicitTypeArgument != 0 ? 1 : 0; } }";
        var result = Compile(source);
        Assert.True(result.Diagnostics.Count(diagnostic => diagnostic.Code == "LMC3012") >= 9,
            string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LMC3012" && diagnostic.Message.Contains("Outer.Hidden.Nested", StringComparison.Ordinal));
        Assert.Null(result.LlvmIr);

        var inaccessibleAlias = Compile("using HiddenAlias = Outer.Hidden; public class Outer { private class Hidden { } } public class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(inaccessibleAlias.Diagnostics, diagnostic => diagnostic.Code == "LMC3012" &&
            diagnostic.Message.Contains("Outer.Hidden", StringComparison.Ordinal));
        Assert.Null(inaccessibleAlias.LlvmIr);

        var inaccessibleAliasInEmptyFile = LumaCompiler.Compile([
            ("aliases.ls", "using HiddenAlias = Outer.Hidden;"),
            ("types.ls", "public class Outer { private class Hidden { } } public class Program { public static int32 Main() { return 0; } }")
        ]);
        Assert.Contains(inaccessibleAliasInEmptyFile.Diagnostics, diagnostic => diagnostic.Code == "LMC3012" &&
            diagnostic.Message.Contains("Outer.Hidden", StringComparison.Ordinal));
        Assert.Null(inaccessibleAliasInEmptyFile.LlvmIr);

        var inaccessibleSwitchPattern = Compile("public class Outer { private class Hidden { } public static object Create() { return new Hidden(); } } class Program { public static int32 Main() { object value = Outer.Create(); switch (value) { case Outer.Hidden hidden: return 1; default: return 0; } } }");
        Assert.Contains(inaccessibleSwitchPattern.Diagnostics, diagnostic => diagnostic.Code == "LMC3012" &&
            diagnostic.Message.Contains("Outer.Hidden", StringComparison.Ordinal));
        Assert.DoesNotContain(inaccessibleSwitchPattern.Diagnostics, diagnostic => diagnostic.Code == "LS0200");
        Assert.Null(inaccessibleSwitchPattern.LlvmIr);

        var inaccessibleConstruction = Compile("public class Outer { private class Hidden { } } class Program { public static int32 Main() { object value = new Outer.Hidden(); return 0; } }");
        Assert.Contains(inaccessibleConstruction.Diagnostics, diagnostic => diagnostic.Code == "LMC3012" &&
            diagnostic.Message.Contains("Outer.Hidden", StringComparison.Ordinal));
        Assert.DoesNotContain(inaccessibleConstruction.Diagnostics, diagnostic => diagnostic.Code == "LS0200");
        Assert.Null(inaccessibleConstruction.LlvmIr);

        var inaccessibleCatchType = Compile("public class Outer { private class HiddenException : System.Exception { } } class Program { public static int32 Main() { try { return 0; } catch (Outer.HiddenException error) { return 1; } } }");
        Assert.Contains(inaccessibleCatchType.Diagnostics, diagnostic => diagnostic.Code == "LMC3012" &&
            diagnostic.Message.Contains("Outer.HiddenException", StringComparison.Ordinal));
        Assert.DoesNotContain(inaccessibleCatchType.Diagnostics, diagnostic => diagnostic.Code == "LS1800");
        Assert.Null(inaccessibleCatchType.LlvmIr);

        var inaccessibleStaticCall = Compile("delegate int32 Getter(); public class Outer { private class Hidden { public static int32 Value = 42; public static int32 Read() { return 42; } } } class Program { public static int32 Main() { Getter read = Outer.Hidden.Read; return read() + Outer.Hidden.Read() + Outer.Hidden.Value; } }");
        Assert.Contains(inaccessibleStaticCall.Diagnostics, diagnostic => diagnostic.Code == "LMC3012" &&
            diagnostic.Message.Contains("Outer.Hidden", StringComparison.Ordinal));
        Assert.DoesNotContain(inaccessibleStaticCall.Diagnostics, diagnostic => diagnostic.Code is "LS0200" or "LS0751");
        Assert.Null(inaccessibleStaticCall.LlvmIr);

        var namespaceCannotImpersonateContainingType = LumaCompiler.Compile([
            ("outer.ls", "public class Outer { private static int32 Secret = 41; private class Hidden { } public static object Create() { return new Hidden(); } }"),
            ("program.ls", "namespace Outer; public class Program { public static int32 Main() { int32 secret = Outer.Secret; object value = Outer.Create(); Outer.Hidden hidden = (Outer.Hidden)value; return hidden == null ? secret : 0; } }")
        ]);
        Assert.Contains(namespaceCannotImpersonateContainingType.Diagnostics, diagnostic => diagnostic.Code == "LMC3012" &&
            diagnostic.Message.Contains("Outer.Hidden", StringComparison.Ordinal));
        Assert.Contains(namespaceCannotImpersonateContainingType.Diagnostics, diagnostic => diagnostic.Code == "LMC3012" &&
            diagnostic.Message.Contains("Outer.Secret", StringComparison.Ordinal));
        Assert.Null(namespaceCannotImpersonateContainingType.LlvmIr);
    }

    [Fact]
    public void DefaultStructToStringAndConcatenationUseTheQualifiedTypeName()
    {
        const string source = "namespace Geometry; struct Point { int32 x; } public class Program { public static int32 Main() { Geometry.Point point = default; string direct = point.ToString(); string joined = \"value=\" + point; return direct == \"Geometry.Point\" && joined == \"value=Geometry.Point\" ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));

        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void StructToStringOverridesAreUsedByDirectCallsAndConcatenation()
    {
        const string source = "struct Point { int32 _value; public this(int32 value) { _value = value; } public override string ToString() { return \"Point(\" + _value + \")\"; } } class Program { public static int32 Main() { Point point = Point(42); string direct = point.ToString(); string joined = \"value=\" + point; string compound = \"compound=\"; compound += point; return direct == \"Point(42)\" && joined == \"value=Point(42)\" && compound == \"compound=Point(42)\" ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));

        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void StringRuntimeAllocationFailuresRaiseCatchableOutOfMemoryExceptions()
    {
        const string concatenation = """
            class Program {
                public static int32 Main() {
                    string left = "left";
                    string right = "right";
                    try { string combined = left + right; return 1; }
                    catch (System.OutOfMemoryException error) { return error == null ? 2 : 0; }
                }
            }
            """;
        var concatResult = Compile(concatenation);
        Assert.True(concatResult.Succeeded, string.Join(Environment.NewLine, concatResult.Diagnostics));
        Assert.Contains("call void @luma_raise_out_of_memory()", concatResult.LlvmIr!, StringComparison.Ordinal);
        foreach (var failedAllocation in new[] { 0, 1, 2 })
        {
            var outOfMemory = LlvmJit.RunMain(concatResult.LlvmIr!, allocationFailureAfter: failedAllocation);
            Assert.Equal(0, outOfMemory.ExitCode);
            Assert.Equal(0, outOfMemory.OutstandingAllocations);
            Assert.Null(outOfMemory.InteropError);
        }

        const string stringification = """
            class Program {
                public static int32 Main() {
                    int32 value = 42;
                    try { string text = "value=" + value; return 1; }
                    catch (System.OutOfMemoryException) { return 0; }
                }
            }
            """;
        var stringificationResult = Compile(stringification);
        Assert.True(stringificationResult.Succeeded, string.Join(Environment.NewLine, stringificationResult.Diagnostics));
        foreach (var failedAllocation in new[] { 0, 1, 2 })
        {
            var conversionOutOfMemory = LlvmJit.RunMain(stringificationResult.LlvmIr!, allocationFailureAfter: failedAllocation);
            Assert.Equal(0, conversionOutOfMemory.ExitCode);
            Assert.Equal(0, conversionOutOfMemory.OutstandingAllocations);
            Assert.Null(conversionOutOfMemory.InteropError);
        }
    }

    [Fact]
    public void StringRuntimeLengthOverflowsRaiseCatchableOverflowExceptions()
    {
        const string source = "class Program { public static int32 Main() { try { string combined = \"left\" + \"right\"; return 1; } catch (System.OverflowException error) { return error == null ? 2 : 0; } } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("call void @luma_raise_overflow()", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("@luma_overflow_instance", result.LlvmIr, StringComparison.Ordinal);

        foreach (var overflow in new[] { "length", "bytes" })
        {
            var execution = LlvmJit.RunMain(result.LlvmIr!, forceStringRuntimeOverflow: overflow);
            Assert.Equal(0, execution.ExitCode);
            Assert.Equal(0, execution.OutstandingAllocations);
            Assert.Null(execution.InteropError);
        }
    }

    [Fact]
    public void StringReferenceCountOverflowUsesFatalDiagnosticPath()
    {
        var result = Compile("class Program { public static int32 Main() { string first = \"left\" + \"right\"; string second = first; return second.Length; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var ir = result.LlvmIr!;
        Assert.Contains("@luma_fatal_refcount_message", ir, StringComparison.Ordinal);
        var encodedMessage = string.Concat(Encoding.UTF8.GetBytes("LumaSharp fatal error: reference-count overflow\n").Select(value => "\\" + value.ToString("X2")));
        Assert.Contains(encodedMessage, ir, StringComparison.Ordinal);
        Assert.Contains("declare void @abort() noreturn", ir, StringComparison.Ordinal);
        var retainBody = ir.Split("define internal void @luma_string_retain", StringSplitOptions.None)[1].Split("\n}", 2, StringSplitOptions.None)[0];
        Assert.Contains("call void @luma_fatal_reference_count_overflow()", retainBody, StringComparison.Ordinal);
        Assert.DoesNotContain("luma_throw_overflow", retainBody, StringComparison.Ordinal);

        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-refcount-fatal-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try
        {
            var emitted = LlvmToolchain.VerifyAndEmitObject(ir, objectPath, "host", optimize: false);
            Assert.True(emitted.Succeeded, emitted.Error);
        }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }
    }

    [Fact]
    public void EveryManagedRetainPathChecksForReferenceCountOverflow()
    {
        const string source = "class Node { } delegate int32 Getter(); class Program { public static int32 Main() { Node node = new Node(); Node alias = node; int32[] values = { 7 }; int32[] arrayAlias = values; object boxed = 42; object boxAlias = boxed; Getter getter = () => 42; Getter getterAlias = getter; return alias != null && arrayAlias[0] == 7 && (int32)boxAlias == 42 && getterAlias() == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var ir = result.LlvmIr!;
        Assert.DoesNotContain("atomicrmw add", ir, StringComparison.Ordinal);
        Assert.Contains("array.retain.fatal", ir, StringComparison.Ordinal);
        Assert.Contains("class.retain.fatal", ir, StringComparison.Ordinal);
        Assert.Contains("box.retain.fatal", ir, StringComparison.Ordinal);
        Assert.Contains("delegate.retain.fatal", ir, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(ir);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void ReplacesOutStringValuesWithBalancedOwnership()
    {
        var result = Compile("class Program { public static int32 Main() { string current = \"old\"; Replace(out current); Replace(out string created); if (current == created && created == \"new\") return 0; return 1; } public static void Replace(out string value) { value = \"new\"; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var ir = result.LlvmIr!;
        Assert.Contains("store ptr @luma_string_empty", ir);
        Assert.Contains("call void @luma_string_release", ir);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-out-string-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try
        {
            var emitted = LlvmToolchain.VerifyAndEmitObject(ir, objectPath, "host", optimize: false);
            Assert.True(emitted.Succeeded, emitted.Error);
        }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }
    }

    [Fact]
    public void SwitchesOnStringConstantsUsingContentSemantics()
    {
        var result = Compile("class Program { public static int32 Main() { string value = Choose(); switch (value) { case \"red\": return 1; case @\"green\": return 2; default: return 3; } } public static string Choose() { return \"green\"; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("call i1 @luma_string_equals", result.LlvmIr!);
        Assert.Contains("switch.test", result.LlvmIr);
        var objectPath = Path.Combine(Path.GetTempPath(), "lumasharp-string-switch-" + Guid.NewGuid().ToString("N") + (OperatingSystem.IsWindows() ? ".obj" : ".o"));
        try
        {
            var emitted = LlvmToolchain.VerifyAndEmitObject(result.LlvmIr!, objectPath, "host", optimize: true);
            Assert.True(emitted.Succeeded, emitted.Error);
        }
        finally { if (File.Exists(objectPath)) File.Delete(objectPath); }

        var duplicate = Compile("class Program { public static int32 Main() { switch (\"a\") { case \"a\": return 1; case \"\\x61\": return 2; default: return 0; } } }");
        Assert.Contains(duplicate.Diagnostics, diagnostic => diagnostic.Code == "LS0622" && diagnostic.Message.Contains("duplicates", StringComparison.OrdinalIgnoreCase));

        var duplicateConstant = Compile("class Settings { public const string Letter = \"a\"; } class Program { public static int32 Main() { switch (\"a\") { case Settings.Letter: return 1; case \"a\": return 2; default: return 0; } } }");
        Assert.Contains(duplicateConstant.Diagnostics, diagnostic => diagnostic.Code == "LS0622" && diagnostic.Message.Contains("duplicates", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void FoldsStringConstantsAndUsesInternedStringObjects()
    {
        var result = Compile("class Settings { public const string Product = \"Luma\" + \"Sharp\"; public const bool Matches = \"Luma\" + \"Sharp\" == \"LumaSharp\"; } class Program { public static int32 Main() { const string local = \"LumaSharp\"; if (local == Settings.Product && Settings.Matches) return 0; return 1; } }");
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var ir = result.LlvmIr!;
        Assert.Contains("call i1 @luma_string_equals", ir);
        var productObjects = ir.Split(Environment.NewLine).Where(line => line.StartsWith("@luma_string_", StringComparison.Ordinal) && line.Contains("constant %luma.string {", StringComparison.Ordinal) && line.Contains("i32 9, i32 9", StringComparison.Ordinal)).ToArray();
        Assert.Single(productObjects);

        var invalidRelationalConstant = Compile("class Program { public static int32 Main() { const bool before = \"a\" < \"b\"; return 0; } }");
        Assert.Contains(invalidRelationalConstant.Diagnostics, diagnostic => diagnostic.Code == "LMC3010");
    }

    [Fact]
    public void StringCharactersAreImmutableAndIndexingUsesInt32()
    {
        var assignment = Compile("class Program { public static int32 Main() { string text = \"a\"; text[0] = 'b'; return 0; } }");
        Assert.Contains(assignment.Diagnostics, diagnostic => diagnostic.Code == "LMC3012" && diagnostic.Message.Contains("Cannot assign", StringComparison.Ordinal));

        var invalidIndex = Compile("class Program { public static int32 Main() { string text = \"a\"; int64 index = 1; return (int32)text[index]; } }");
        Assert.Contains(invalidIndex.Diagnostics, diagnostic => diagnostic.Code == "LMC3010" && diagnostic.Message.Contains("string index", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void StringBoundsFailuresUseCatchableIndexExceptions()
    {
        const string source = "class Program { public static int32 Main() { string value = \"a\"; return (int32)value[0]; } }";
        var warning = Compile(source);
        Assert.True(warning.Succeeded, string.Join(Environment.NewLine, warning.Diagnostics));
        Assert.DoesNotContain(warning.Diagnostics, diagnostic => diagnostic.Code == "LMC3101");

        var promoted = Compile(source, new CompilationOptions(WarningsAsErrors: true));
        Assert.True(promoted.Succeeded, string.Join(Environment.NewLine, promoted.Diagnostics));
    }

    [Fact]
    public void ConstructsClassesCallsInstanceMethodsAndRetainsAliases()
    {
        const string source = "class Counter { int32 value = 40; public this(int32 start) { value = start; } public int32 Read() { return value; } public void Add(int32 amount) mut { value += amount; } } class Program { public static int32 Main() { let first = new Counter(41); let alias = first; alias.Add(1); if (first.Read() != 42) return 1; if (first != alias) return 2; let other = new Counter(42); if (first == other) return 3; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("%class.Counter = type { ptr, i32, i32, i32 }", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("@luma_class_Counter_retain", result.LlvmIr, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void TargetTypedAllocationsRequireAnExpectedType()
    {
        const string targetTyped = "class Box { public this() { } } class Program { public static int32 Main() { Box value = new(); return value == null ? 1 : 0; } }";
        var valid = Compile(targetTyped);
        Assert.True(valid.Succeeded, string.Join(Environment.NewLine, valid.Diagnostics));
        var execution = LlvmJit.RunMain(valid.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);

        var missingTarget = Compile("class Box { public this() { } } class Program { public static int32 Main() { var value = new(); return 0; } }");
        Assert.Contains(missingTarget.Diagnostics, diagnostic => diagnostic.Code == "LS0802");
        Assert.DoesNotContain(missingTarget.Diagnostics, diagnostic => diagnostic.Code == "LMC3000");
    }

    [Fact]
    public void GeneratesDefaultClassConstructorAndInitializesStringFields()
    {
        const string source = "class Label { string text = \"ready\"; public string Read() { return text; } } class Program { public static int32 Main() { let label = new Label(); if (label.Read() == \"ready\") return 0; return 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void DiagnosesClassMutabilityAndUninitializedReferenceFields()
    {
        var immutable = Compile("class Counter { int32 value; public void Set(int32 next) { value = next; } } class Program { public static int32 Main() { let item = new Counter(); return 0; } }");
        Assert.Contains(immutable.Diagnostics, diagnostic => diagnostic.Code == "LS0820");

        var uninitializedField = Compile("class Child { } class Parent { Child child; } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(uninitializedField.Diagnostics, diagnostic => diagnostic.Code == "LS0810" && diagnostic.Message.Contains("every constructor", StringComparison.Ordinal));
    }

    [Fact]
    public void WarnsOnceForEachStrongReferenceTypeCycleAndIgnoresNullableEdges()
    {
        const string source = "class A { B child; public this(B value) { child = value; } } class B { A parent; public this(A value) { parent = value; } } class Self { Self next; public this(Self value) { next = value; } } class Node { Node? next; public this() { next = null; } } virtual class Root { Link link; public this() { link = new Link(new Derived()); } } class Derived : Root { } class Link { Derived target; public this(Derived value) { target = value; } } class Program { public static int32 Main() { return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var cycleWarnings = result.Diagnostics.Where(diagnostic => diagnostic.Code == "LS3301").ToArray();
        Assert.Equal(3, cycleWarnings.Length);
        Assert.All(cycleWarnings, warning => Assert.Equal(DiagnosticSeverity.Warning, warning.Severity));
        Assert.Contains(cycleWarnings, warning => warning.Message.Contains("A.child -> B", StringComparison.Ordinal) && warning.Message.Contains("B.parent -> A", StringComparison.Ordinal));
        Assert.Contains(cycleWarnings, warning => warning.Message.Contains("Self.next -> Self", StringComparison.Ordinal));
        Assert.Contains(cycleWarnings, warning => warning.Message.Contains("Derived.link -> Link", StringComparison.Ordinal) && warning.Message.Contains("Link.target -> Derived", StringComparison.Ordinal));
        Assert.DoesNotContain(cycleWarnings, warning => warning.Message.Contains("Node.next", StringComparison.Ordinal));
    }

    [Fact]
    public void SupportsNestedClassFieldsAndNamedOptionalConstructorArguments()
    {
        const string source = "class Child { int32 value; public this(int32 start, int32 delta = 1) { value = start + delta; } public int32 Read() { return value; } } class Parent { Child child; public this(int32 start) { child = new Child(delta: 2, start: start); } public int32 Read() { return child.Read(); } } class Program { public static int32 Main() { let item = new Parent(40); return item.Read() == 42 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("call void @luma_class_release_dynamic", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("ptr @luma_class_Child_release", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("@luma_classdesc_Parent", result.LlvmIr, StringComparison.Ordinal);
        Assert.Contains("@luma_classdesc_Child", result.LlvmIr, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadonlyInstanceFieldsAreConstructorOnlyAndUseTheRequiredDiagnostic()
    {
        const string validSource = "class Settings { public readonly int32 id; public this(int32 value) { id = value; } public int32 Read() { return id; } } class Program { public static int32 Main() { return new Settings(17).Read() == 17 ? 0 : 1; } }";
        var valid = Compile(validSource);
        Assert.True(valid.Succeeded, string.Join(Environment.NewLine, valid.Diagnostics));
        var execution = LlvmJit.RunMain(valid.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);

        var outsideConstructor = Compile("class Settings { public readonly int32 id; public this(int32 value) { id = value; } public void Change() mut { id = 2; } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(outsideConstructor.Diagnostics, diagnostic => diagnostic.Code == "LS0231");
        Assert.Null(outsideConstructor.LlvmIr);

        var otherInstance = Compile("class Settings { public readonly int32 id; public this(int32 value) { id = value; } } class Holder { public this(Settings other) { other.id = 3; } } class Program { public static int32 Main() { return 0; } }");
        Assert.Contains(otherInstance.Diagnostics, diagnostic => diagnostic.Code == "LS0231");
        Assert.Null(otherInstance.LlvmIr);
    }

    [Fact]
    public void RunsClassFinalizerExactlyOnceAndAllowsReadonlyBorrowDuringDestruction()
    {
        const string source = "class Finalizable { public static int32 count; public static int32 Count() { return Finalizable.count; } public static void Observe(readonly Finalizable value) { } ~this() { Finalizable.count += 1; Observe(this); } } class Program { public static int32 Main() { { let item = new Finalizable(); let alias = item; } return Finalizable.Count() == 1 ? 0 : 1; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("define void @luma_Finalizable_finalizer_0(ptr %this)", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("call void @luma_Finalizable_finalizer_0(ptr %input)", result.LlvmIr, StringComparison.Ordinal);
        Assert.Contains("atomicrmw or ptr %destroy.flags.address, i32 4", result.LlvmIr, StringComparison.Ordinal);

        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void FailedConstructorRunsOnlyFinalizersForCompletedClassesAndReleasesStorage()
    {
        const string source = "virtual class Base { public static int32 finalized; public this() { } ~this() { Base.finalized += 1; } } class Derived : Base { public this(bool fail) { if (fail) throw new Exception(\"failed\"); } ~this() { Base.finalized += 10; } } class Program { public static int32 Main() { int32 caught = 0; try { let broken = new Derived(true); } catch (Exception) { caught++; } if (caught != 1 || Base.finalized != 1) return 1; { let complete = new Derived(false); } return Base.finalized == 12 ? 0 : 2; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void RejectsFinalizerThisEscapeToNonReadonlyParameter()
    {
        const string source = "class Root { public static void Keep(Root value) { } ~this() { Keep(this); } } class Program { public static int32 Main() { let root = new Root(); return 0; } }";
        var result = Compile(source);
        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS0701" && diagnostic.Message.Contains("finalizer", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SupportsNullableClassReferencesFlowCoalescingAndNullAssertion()
    {
        const string source = "class Node { int32 value = 42; public int32 Read() { return value; } } class Holder { Node? child; public this(Node? initial) { child = initial; } public int32 Read() { return child!.Read(); } } class Program { public static int32 Main() { Node? optional = null; if (optional != null) return 1; optional = new Node(); if (optional == null) return 2; Node value = optional; Node? picked = optional ?? new Node(); Node asserted = optional!; Node? lazy = null; lazy ??= new Node(); Node? snapshot = lazy; lazy ??= new Node(); Node lazilyInitialized = lazy; if (optional != null && optional.Read() == 42) { Node shortCircuitProof = optional; } else return 5; Node conditionalProof = optional != null ? optional : new Node(); let holder = new Holder(optional); if (holder.Read() != 42 || value.Read() != 42 || picked.Read() != 42 || asserted.Read() != 42 || lazilyInitialized.Read() != 42 || conditionalProof.Read() != 42) return 3; if (optional != picked || lazy != snapshot) return 4; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("phi ptr", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("luma_throw_null", result.LlvmIr, StringComparison.Ordinal);

        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void NullableClassReferencesDiagnoseUnsafeConversionsAndMemberAccess()
    {
        var nullConversion = Compile("class Node { } class Program { public static int32 Main() { Node value = null; return 0; } }");
        Assert.Contains(nullConversion.Diagnostics, diagnostic => diagnostic.Code == "LS1301");

        var maybeNullConversion = Compile("class Node { } class Program { public static int32 Main() { Node? optional = null; Node value = optional; return 0; } }");
        Assert.Contains(maybeNullConversion.Diagnostics, diagnostic => diagnostic.Code == "LS1302");

        var unsafeAccess = Compile("class Node { public int32 Read() { return 1; } } class Program { public static int32 Main() { Node? optional = null; return optional.Read(); } }");
        Assert.Contains(unsafeAccess.Diagnostics, diagnostic => diagnostic.Code == "LS1303");

        var invalidCoalesceAssignment = Compile("class Program { public static int32 Main() { int32 value = 1; value ??= 2; return value; } }");
        Assert.Contains(invalidCoalesceAssignment.Diagnostics, diagnostic => diagnostic.Code == "LMC3010" && diagnostic.Message.Contains("left operand of '??='", StringComparison.Ordinal));

        var nullableArray = Compile("class Program { public static int32 Main() { int32[]? values = null; int32[]? copy = values; if (copy == null) return 0; return 1; } }");
        Assert.True(nullableArray.Succeeded, string.Join(Environment.NewLine, nullableArray.Diagnostics));

        var referenceBearingStruct = Compile("struct Text { public string? value; } class Program { public static int32 Main() { Text text = default; text.value = \"stored\"; Text copy = text; text.value = null; return copy.value == \"stored\" ? 0 : 1; } }");
        Assert.True(referenceBearingStruct.Succeeded, string.Join(Environment.NewLine, referenceBearingStruct.Diagnostics));
        var referenceBearingExecution = LlvmJit.RunMain(referenceBearingStruct.LlvmIr!);
        Assert.Equal(0, referenceBearingExecution.ExitCode);
        Assert.Equal(0, referenceBearingExecution.OutstandingAllocations);
    }

    [Fact]
    public void NullableArraysSupportFlowCoalescingSafeIndexingAndLength()
    {
        const string source = "class Shelf { int32[]? items; public this(int32[]? initial) { items = initial; } public int32? Read() { return items?[0]; } public int32? Size() { return items?.Length; } } class Program { public static int32 Main() { int32[]? values = null; int32? absent = values?[0]; int32? absentLength = values?.Length; int32? absentRank = values?.Rank; if (absent != null || absentLength != null || absentRank != null) return 1; values = new int32[2]; values[0] = 7; int32? present = values?[0]; int32? presentLength = values?.Length; int32 element = present ?? 0; int32 length = presentLength ?? 0; int32[]? alias = values; int32[] chosen = alias ?? new int32[1]; int32[]? assigned = null; assigned ??= new int32[1]; assigned[0] = 9; Shelf shelf = new Shelf(values); int32? fieldElement = shelf.Read(); int32? fieldLength = shelf.Size(); if (chosen[0] != 7 || assigned[0] != 9 || element != 7 || length != 2 || (fieldElement ?? 0) != 7 || (fieldLength ?? 0) != 2) return 2; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("null.array.index", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("coalesce.array", result.LlvmIr, StringComparison.Ordinal);

        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void NullConditionalArrayIndexRetainsReferenceElementsAndPreservesExistingNullability()
    {
        const string source = "class Node { public static int32 Destroyed; int32 value; public this(int32 value) { this.value = value; } public int32 Read() { return value; } ~this() { Node.Destroyed++; } } class Program { public static int32 Main() { Node?[]? values = null; Node? absent = values?[0]; if (absent != null) return 1; values = new Node?[1]; values[0] = new Node(7); Node? selected = values?[0]; values[0] = null; if (Node.Destroyed != 0 || selected == null || selected.Read() != 7) return 2; selected = null; if (Node.Destroyed != 1) return 3; values = null; Node[]? nonNullableElements = new Node[] { new Node(13) }; Node? copied = nonNullableElements?[0]; nonNullableElements = null; if (Node.Destroyed != 1 || copied == null || copied.Read() != 13) return 4; copied = null; if (Node.Destroyed != 2) return 5; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Code == "LMC3000");

        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void NullableArrayIndexAndMemberAccessRequireFlowProofOutsideConditionalAccess()
    {
        var unsafeIndex = Compile("class Program { public static int32 Main() { int32[]? values = null; return values[0]; } }");
        Assert.Contains(unsafeIndex.Diagnostics, diagnostic => diagnostic.Code == "LS1303");

        var unsafeLength = Compile("class Program { public static int32 Main() { int32[]? values = null; return values.Length; } }");
        Assert.Contains(unsafeLength.Diagnostics, diagnostic => diagnostic.Code == "LS1303");

        var nullConditionalAssignment = Compile("class Program { public static int32 Main() { int32[]? values = null; values?[0] = 1; return 0; } }");
        Assert.Contains(nullConditionalAssignment.Diagnostics, diagnostic => diagnostic.Code == "LMC3012");
    }

    [Fact]
    public void NullableClassFlowSurvivesEarlyReturnInEitherBranch()
    {
        const string source = "class Node { public int32 Read() { return 7; } } class Program { public static int32 Main() { Node? left = new Node(); if (left == null) return 1; Node leftValue = left; Node? right = new Node(); if (right != null) { } else return 2; Node rightValue = right; return leftValue.Read() == 7 && rightValue.Read() == 7 ? 0 : 3; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void NullableFlowTracksClassMemberPathsAndLoopConditions()
    {
        const string source = "class Node { public int32 Read() { return 5; } } class Holder { public Node? child; public int32 Touch() { return 0; } } class Program { public static int32 Main() { Holder? holder = new Holder(); if (holder == null) return 1; holder.child = new Node(); if (holder.child != null && holder.child.Read() == 5) { } else return 2; Node? current = new Node(); int32 observed = 0; while (current != null) { observed = current.Read(); current = null; } return observed == 5 ? 0 : 3; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void NullableFlowInvalidatesMemberFactsAcrossJoinsAndCalls()
    {
        var afterJoin = Compile("class Node { public int32 Read() { return 1; } } class Holder { public Node? child; } class Program { public static int32 Main() { Holder holder = new Holder(); if (holder.child != null) { } return holder.child.Read(); } }");
        Assert.Contains(afterJoin.Diagnostics, diagnostic => diagnostic.Code == "LS1303");

        var afterCall = Compile("class Node { public int32 Read() { return 1; } } class Holder { public Node? child; public void Mutate() {} } class Program { public static int32 Main() { Holder holder = new Holder(); holder.child = new Node(); if (holder.child != null) { holder.Mutate(); return holder.child.Read(); } return 0; } }");
        Assert.Contains(afterCall.Diagnostics, diagnostic => diagnostic.Code == "LS1303");
    }

    [Fact]
    public void NullConditionalClassFieldsAndCallsShortCircuitAndRetainResults()
    {
        const string source = "class Node { public static int32 calls; public Node? child; public Node? GetChild() { return child; } public void Touch() mut { Node.calls += 1; } } class Program { public static int32 Main() { Node? optional = null; optional?.Touch(); Node? absentField = optional?.child; Node? absentCall = optional?.GetChild(); if (absentField != null || absentCall != null || Node.calls != 0) return 1; optional = new Node(); optional?.Touch(); Node? presentField = optional?.child; Node? presentCall = optional?.GetChild(); if (presentField != null || presentCall != null || Node.calls != 1) return 2; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("null.call.invoke", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("null.field.value", result.LlvmIr, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void NullConditionalClassValueResultsBecomeNullableAndShortCircuit()
    {
        const string source = "class Node { public int32 number = 6; public int32 Read(int32 add) { return number + add; } } class Program { public static int32 Main() { Node? value = null; int32? absentField = value?.number; int32? absentCall = value?.Read(3); if (absentField != null || absentCall != null) return 1; value = new Node(); int32? presentField = value?.number; int32? presentCall = value?.Read(3); if (presentField != 6 || presentCall != 9) return 2; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void NullConditionalCallsReturnNullableDelegatesAndShortCircuitArguments()
    {
        const string source = "delegate int32 Reader(); class Holder { public static int32 argumentCalls; public int32 NextValue() { Holder.argumentCalls++; return Holder.argumentCalls; } public Reader Create(int32 value) { return () => value; } } class Program { public static int32 Main() { Holder? absent = null; Reader? missing = absent?.Create(absent!.NextValue()); if (missing != null || Holder.argumentCalls != 0) return 1; Holder holder = new Holder(); Reader? present = holder?.Create(holder.NextValue()); if (present == null || Holder.argumentCalls != 1 || present() != 1) return 2; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("null.call.invoke", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void NullConditionalCallsLiftAggregateStringAndArrayResultsExactlyOnce()
    {
        const string source = "enum State : int32 { Ready = 7 } struct Point { public int32 x; public this(int32 value) { x = value; } } class Provider { public Point GetPoint() { return Point(42); } public Point? MaybePoint(bool hasValue) { if (hasValue) return Point(20); return null; } public State GetState() { return State.Ready; } public string GetText() { return \"value\"; } public string? MaybeText(bool hasValue) { if (hasValue) return \"maybe\"; return null; } public int32[] GetValues() { return new int32[1] { 24 }; } public int32[]? MaybeValues(bool hasValue) { if (hasValue) return new int32[1] { 33 }; return null; } } class Program { public static int32 Main() { Provider? absent = null; if (absent?.GetPoint() != null || absent?.MaybePoint(true) != null || absent?.GetState() != null || absent?.GetText() != null || absent?.MaybeText(true) != null || absent?.GetValues() != null || absent?.MaybeValues(true) != null) return 1; Provider provider = new Provider(); Point? point = provider?.GetPoint(); Point? maybePoint = provider?.MaybePoint(false); Point? presentMaybePoint = provider?.MaybePoint(true); State? state = provider?.GetState(); string? text = provider?.GetText(); string? maybeText = provider?.MaybeText(false); int32[]? values = provider?.GetValues(); int32[]? maybeValues = provider?.MaybeValues(false); int32[]? presentMaybeValues = provider?.MaybeValues(true); if (point == null || point!.x != 42 || maybePoint != null || presentMaybePoint == null || presentMaybePoint!.x != 20 || state != State.Ready || text != \"value\" || maybeText != null || values == null || values![0] != 24 || maybeValues != null || presentMaybeValues == null || presentMaybeValues![0] != 33) return 2; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void NullConditionalStringIndexReturnsNullableCharAndSkipsIndexEvaluation()
    {
        const string source = "class Program { public static int32 indexCalls; public static int32 NextIndex() { Program.indexCalls++; return Program.indexCalls - 1; } public static string BuildText() { return \"xy\" + \"\"; } public static int32 Main() { string? absent = null; char? missing = absent?[NextIndex()]; if (missing != null || Program.indexCalls != 0) return 1; string? text = \"abc\"; char? first = text?[NextIndex()]; if (first != 'a' || Program.indexCalls != 1) return 2; try { char? invalid = text?[NextIndex() + 2]; return invalid == null ? 3 : 4; } catch (IndexOutOfRangeException) { } if (Program.indexCalls != 2) return 5; char? transient = BuildText()?[1]; if (transient != 'y') return 6; try { char? invalidTemporary = BuildText()?[9]; return invalidTemporary == null ? 7 : 8; } catch (IndexOutOfRangeException) { } return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("null.string.index", result.LlvmIr!, StringComparison.Ordinal);
        Assert.Contains("{ i1, i16 }", result.LlvmIr, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void SupportsNullablePrimitiveValuesDefaultsConversionsAndCoalescing()
    {
        const string source = "class Program { public static int32 Main() { int32? present = 5; int32? absent = null; int32? defaulted = default; bool hasValue = present != null; int32 unwrapped = present!; int32 fallback = absent ?? 9; int32? sum = present + 2; int32? retained = absent ?? present; int32? sequence = 4; int32? old = sequence++; sequence += 2; --sequence; int32? remainsNull = null; remainsNull++; remainsNull += 1; bool? yes = true; bool? no = false; bool? masked = yes & no; bool? missingMask = absent == null ? (bool?)null : yes; if (!hasValue || unwrapped != 5 || fallback != 9 || sum != 7 || retained != 5 || defaulted != null || old != 4 || sequence != 6 || remainsNull != null || masked != false || missingMask != null) return 1; if (present != 5 || absent == 5 || present == absent) return 2; if (absent == null) { int32? recovered = absent ??= 11; if (recovered != 11 || absent != 11) return 3; } return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("{ i1, i32 }", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void NullablePrimitiveLiftedComparisonsReturnFalseForNull()
    {
        const string source = "class Program { public static int32 Main() { int32? absent = null; int32? present = 3; int32? alsoPresent = 4; if (absent < present || present < absent) return 1; if (!(present < alsoPresent) || absent >= present) return 2; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void NullableStructMemberAccessIsFlowCheckedAndMutatesTheContainedValue()
    {
        const string source = "struct Point { public int32 x; public void Add(int32 amount) mut { x += amount; } } class Program { public static int32 Main() { Point? point = Point(); point!.x = 3; point!.Add(4); Point? empty = null; if (empty != null) empty.x = 99; Point copy = point; if (copy.x != 7) return 1; return point!.x == 7 ? 0 : 2; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("luma_throw_null", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);

        var unsafeAccess = Compile("struct Point { public int32 x; } class Program { public static int32 Main() { Point? point = null; return point.x; } }");
        Assert.Contains(unsafeAccess.Diagnostics, diagnostic => diagnostic.Code == "LS1303");
        Assert.Null(unsafeAccess.LlvmIr);
    }

    [Fact]
    public void NullableStringsPreserveNullAndOwnValuesAcrossCoalescingFieldsAndNullConditionalLength()
    {
        const string source = "class Holder { public string? text; public string? Read() { return text; } } class Program { public static int32 Main() { string? value = null; if (value == \"\" || value != null) return 1; string fallback = value ?? \"fallback\"; string combined = \"prefix:\" + value; int32? missingLength = value?.Length; if (fallback != \"fallback\" || combined != \"prefix:\" || missingLength != null) return 2; value ??= \"owned\"; string? copy = value; string revealed = value!; int32? length = copy?.Length; if (revealed != \"owned\" || length != 5 || copy != value) return 3; Holder? absent = null; string? absentField = absent?.text; string? absentCall = absent?.Read(); Holder holder = new Holder(); holder.text = copy; string? presentField = holder.text; string? safeField = absent?.text; string? presentCall = holder?.Read(); if (absentField != null || absentCall != null || safeField != null || presentField != \"owned\" || presentCall != \"owned\") return 4; value = null; string? conditional = value == null ? null : value; if (conditional != null) return 5; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains("luma_string_release", result.LlvmIr!, StringComparison.Ordinal);
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
        Assert.Null(execution.InteropError);
    }

    [Fact]
    public void NullableStructAndEnumValuesSupportEqualityAndZeroDefault()
    {
        const string source = "enum Shade : uint8 { Red = 1, Blue = 2 } struct Point { public int32 x; public static bool operator ==(Point left, Point right) { return left.x == right.x; } } class Program { public static int32 Main() { Point? first = Point(); first!.x = 8; Point? second = Point(); second!.x = 8; Point? empty = default; Shade? color = Shade.Blue; Shade? noColor = null; if (first != second || empty != null || color != Shade.Blue || noColor == Shade.Red || noColor != null) return 1; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);
    }

    [Fact]
    public void ReadonlyParametersMayBeBorrowedByNonEscapingLambdas()
    {
        const string source = "delegate int32 Getter(); class Sample { public int32 Read() { return 42; } } class Program { public static int32 Invoke(readonly Getter getter) { return getter(); } public static int32 CaptureLocally(readonly Sample sample) { Getter getter = () => sample.Read(); return getter(); } public static int32 CaptureValueLocally(readonly int32 value) { Getter getter = () => value; return getter(); } public static int32 PassDirectly(readonly Sample sample) { return Invoke(() => sample.Read()); } public static int32 Main() { Sample sample = new Sample(); if (CaptureLocally(sample) != 42) return 1; if (CaptureValueLocally(43) != 43) return 2; if (PassDirectly(sample) != 42) return 3; return 0; } }";
        var result = Compile(source);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Diagnostics));
        var execution = LlvmJit.RunMain(result.LlvmIr!);
        Assert.Equal(0, execution.ExitCode);
        Assert.Equal(0, execution.OutstandingAllocations);

        var escaping = Compile("delegate int32 Getter(); class Sample { public int32 Read() { return 42; } } class Program { public static Getter Capture(readonly Sample sample) { return () => sample.Read(); } public static int32 Main() { return 0; } }");
        Assert.Contains(escaping.Diagnostics, diagnostic => diagnostic.Code == "LS0701" && diagnostic.Message.Contains("scope-bound", StringComparison.OrdinalIgnoreCase));

        var escapingValue = Compile("delegate int32 Getter(); class Program { public static Getter Capture(readonly int32 value) { return () => value; } public static int32 Main() { return 0; } }");
        Assert.Contains(escapingValue.Diagnostics, diagnostic => diagnostic.Code == "LS0701");
    }

    [Fact]
    public void ReadonlyDelegateParametersCannotEscapeThroughHeapContainers()
    {
        const string source = "delegate int32 Getter(); class Holder { public Getter? Callback; public void Store(readonly Getter callback) mut { Callback = callback; } } class Program { public static int32 Main() { return 0; } }";
        var result = Compile(source);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "LS0701");

        const string capturedAliasSource = "delegate int32 Getter(); class Sample { public int32 Read() { return 42; } } class Holder { public Getter? Callback; } class Program { public static void Store(readonly Sample sample) { Holder holder = new Holder(); Getter callback = () => sample.Read(); holder.Callback = callback; } public static int32 Main() { return 0; } }";
        var capturedAlias = Compile(capturedAliasSource);
        Assert.Contains(capturedAlias.Diagnostics, diagnostic => diagnostic.Code == "LS0701");
    }
}
