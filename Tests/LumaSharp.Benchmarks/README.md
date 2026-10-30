# LumaSharp runtime benchmarks

This xUnit test project builds standalone LumaSharp fixtures as native executables through the compiler build API, then compares them with equivalent C# workloads. Use the test runner to execute benchmarks; there is no benchmark-specific C# `Main` program.

Run from the repository root:

```powershell
dotnet test Tests/LumaSharp.Benchmarks/LumaSharp.Benchmarks.csproj -c Release --logger "console;verbosity=detailed"
```

The editable LumaSharp workload lives in `Fixtures/ListBenchmark.ls`; its C# counterpart is in `CSharpListBenchmark.cs`. The test generates a small LumaSharp `Main` entry point and compiles it with the fixture and runtime sources. Each implementation starts and stops its own timer immediately around the list workload: C# uses `System.Diagnostics.Stopwatch`, and LumaSharp uses `System.Diagnostics.StopWatch`. Both print or return a checksum and elapsed duration, which the test compares before reporting medians and sample ranges.

The reported durations exclude compiler/build time and process startup. The benchmark runs in Release with multiple warm-up and measurement executions. LumaSharp durations are converted from `TimeSpan.Ticks` to milliseconds. Process launch still affects system load between samples, but it is outside the measured intervals.

The `List<T>.Ptr` nullable-element pointer mismatch is fixed with an explicit unsafe pointer conversion. The list iterator is available again through `IEnumerable<T>`; this workload currently exercises the direct list member operations, not iteration. If the test runner cannot connect to its testhost, the generated LumaSharp executable can still be run directly, and its output contains the internal checksum and elapsed ticks.
