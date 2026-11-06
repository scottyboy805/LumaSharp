# Compiler performance

## Measurement setup

`Tests/LumaSharp.Benchmarks/CompilerPerformanceBenchmarkTests.cs` measures a complete build of one LumaSharp program with the runtime library enabled. The fixture uses `List<int32>` to include runtime generic expansion and collection lowering. It records source loading, lexing, parsing, async lowering, generic expansion, semantic analysis, LLVM IR emission (including runtime and method-body subphases), LLVM verification/object generation, and native linking. Debug and optimized builds each have one warmup followed by three measured runs; reported build times are medians.

The figures below are from the Windows x64 development environment using .NET 10 and LLVM 22. They are approximate and should be compared on the same machine and toolchain. The runtime library at measurement time contained 84 source files and about 5,100 lines.

## Results

| Measurement | Before | After | Change |
| --- | ---: | ---: | ---: |
| Debug build median | 3.12 s | 0.99 s | 68% faster |
| Optimized build median | 6.00 s | 5.24 s | 13% faster |
| LLVM IR emission, debug | 1.16 s | 0.15 s | 87% faster |
| LLVM IR emission allocations, debug | 1.05 GB | 156 MB | 85% fewer |
| LLVM method body emission, debug | 0.88 s | 0.11 s | 88% faster |
| LLVM object generation, debug | 1.44 s | 0.26 s | 82% faster |

The current optimized build is still dominated by LLVM's `default<O2>` pipeline, which took about 4.7 seconds in the phase benchmark. Source discovery and loading took about 7 ms, lexing 15–19 ms, and parsing 22–32 ms. Semantic analysis took about 0.25–0.36 seconds and generic expansion about 0.08–0.10 seconds. This indicates that parsing the runtime sources is not the main reason optimized builds take several seconds.

## Changes

- Cached the LLVM reference-type set once per emission instead of rebuilding a `HashSet` on every Luma-to-LLVM type conversion. `FunctionEmitter` shares this set, and the emitter also caches its managed-class reachability set.
- Precomputed LLVM function names and overload-collision counts for each compilation. Previously, emitting a method repeatedly scanned the complete method list to determine its name.
- Applied LLVM `globaldce` to executable debug builds after compiler-managed definitions have internal linkage. The pass removes unreferenced functions and data without running the full optimization pipeline on the remaining debug code. Static-library builds skip this pass so exported library contents are preserved. A test compares object output with and without the debug pass.
- Added a phase benchmark that reports elapsed time, per-thread allocations where the phase stays on one thread, GC collections, and emitted IR size. Allocation reporting marks asynchronous phases unavailable when an async continuation moves to another thread.

These changes preserve the full generated IR text in the intermediate `.ll` file. Dead-code elimination currently happens during native object generation, after IR emission.

## Runtime-library artifact caching

A separate cached runtime static library was investigated but is not implemented in this iteration. The compiler currently analyzes runtime and application declarations together and emits them into one LLVM module. The generated module contains shared class descriptors, interface identifiers and dispatch tables, static initialization state, ownership helpers, type metadata, and generic specializations selected by the consuming program. Splitting those pieces across an archive and an application module requires a defined cross-module runtime ABI and careful handling of app-specific generic specializations; simply compiling the current runtime sources as a static archive would create duplicate or module-local type metadata and could break dispatch, type identity, and initialization.

The measurements also show that a parse-tree cache alone would save only roughly 40–60 ms per build, while the optimized backend takes about 4.7 seconds. A cache that meaningfully removes runtime compilation work needs the module/ABI split above. That redesign is a separate milestone; this change avoids adding a small, memory-retaining cache that would not address the measured bottleneck.

## Reproducing

From the repository root, run:

```powershell
dotnet test Tests/LumaSharp.Benchmarks/LumaSharp.Benchmarks.csproj --filter FullyQualifiedName~ProfileFullRuntimeLibraryBuildByCompilerPhase --logger "console;verbosity=detailed"
```

Run the full compiler, runtime, and benchmark test suite with:

```powershell
dotnet test LumaSharp.slnx
```
