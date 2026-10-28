# LumaSharp runtime benchmarks

This xUnit test project builds standalone LumaSharp fixtures as native executables through the compiler build API, then compares them with equivalent C# workloads. Use the test runner to execute benchmarks; there is no benchmark-specific C# `Main` program.

Run from the repository root:

```powershell
dotnet test Tests/LumaSharp.Benchmarks/LumaSharp.Benchmarks.csproj -c Release --logger "console;verbosity=detailed"
```

The editable LumaSharp workload lives in `Fixtures/ListBenchmark.ls`; its C# counterpart is in `CSharpListBenchmark.cs`. The test generates only a small LumaSharp `Main` entry point and compiles it together with that fixture and the runtime source tree. Both sides produce a checksum which xUnit verifies before reporting median timings through its test output. `List<T>`'s `IEnumerable<T>` implementation and iterator workload are temporarily disabled until the backend supports nested generic iterator allocation.

The LumaSharp measurement is wall-clock time around launching the generated native executable, so it includes process startup. The C# measurement is the in-process workload only. These first measurements are useful for smoke/performance exploration, but are not a rigorous apples-to-apples benchmark. Run a Release build, use several samples, and increase the iteration count to reduce startup and timer noise.

The `List<T>.Ptr` nullable-element pointer mismatch is fixed with an explicit unsafe pointer conversion. The list iterator is disabled while the backend limitation remains. In the current environment, VSTest fails to connect to its testhost, even though the generated benchmark executable builds and runs successfully. The workload was therefore also timed directly; those measurements include native process startup and should be treated as exploratory.
