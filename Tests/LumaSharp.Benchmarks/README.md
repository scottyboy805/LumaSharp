# LumaSharp runtime benchmarks

This project builds standalone LumaSharp fixtures as native executables through the compiler build API, then compares them with equivalent C# workloads.

Run from the repository root:

```powershell
dotnet run --project Tests/LumaSharp.Benchmarks/LumaSharp.Benchmarks.csproj -c Release -- --iterations 100000 --samples 5
```

The editable LumaSharp workload lives in `Fixtures/ListBenchmark.ls`. The harness generates only a small `Main` entry point and compiles it together with that fixture and the runtime source tree. The C# side uses `System.Collections.Generic.List<int>` and follows the same operation sequence. Both sides produce a checksum which the harness compares before reporting results.

The LumaSharp measurement is wall-clock time around launching the generated native executable, so it includes process startup. The C# measurement is the in-process workload only. These first measurements are useful for smoke/performance exploration, but are not a rigorous apples-to-apples benchmark. Run a Release build, use several samples, and increase the iteration count to reduce startup and timer noise.

At present, the benchmark build reaches a compiler limitation when specializing `System.Collections.List<int32>`: the `Ptr` property has a nullable-element pointer mismatch and the nested generic iterator cannot be allocated/converted to its specialized `IIterator<int32>` type. The harness reports the compiler diagnostics and exits instead of printing misleading timings. Once those generic specialization paths are fixed, the same command will build and run the comparison.
