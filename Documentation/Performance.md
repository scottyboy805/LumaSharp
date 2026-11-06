# Compiler performance

## Measurement setup

`Tests/LumaSharp.Benchmarks/CompilerPerformanceBenchmarkTests.cs` measures a complete build of one LumaSharp program with the runtime library enabled. The fixture uses `List<int32>` to include runtime generic expansion and collection lowering. It records source loading, lexing, parsing, async lowering, generic expansion, semantic analysis, LLVM IR emission (including runtime and method-body subphases), LLVM verification/object generation, and native linking. Debug and optimized builds each have one warmup followed by three measured runs; reported build times are medians.

The figures below are from the Windows x64 development environment using .NET 10 and LLVM 22. They are approximate and should be compared on the same machine and toolchain. The runtime library at measurement time contained 84 source files and about 5,100 lines.

## Results

| Measurement | Original | After earlier optimizations | Current |
| --- | ---: | ---: | ---: |
| Debug build median | 3.12 s | 0.99 s | ~0.96 s |
| Optimized build median | 6.00 s | 5.24 s | ~1.15 s |
| LLVM IR emission, debug | 1.16 s | 0.15 s | ~0.15 s |
| LLVM IR emission allocations, debug | 1.05 GB | 156 MB | ~156 MB |
| LLVM method body emission, debug | 0.88 s | 0.11 s | ~0.09 s |
| LLVM object generation, debug | 1.44 s | 0.26 s | ~0.09 s |
| Semantic analysis allocations | ~196 MB | ~196 MB | ~67 MB |

The latest benchmark run measured debug builds at 0.85–1.05 seconds and optimized builds at 1.13–1.19 seconds across three measured samples. Their LLVM `default<O2>` pipeline took about 0.38–0.42 seconds, LLVM IR parsing about 0.11–0.12 seconds, and object generation about 0.08–0.11 seconds. Source discovery and loading took about 5–8 ms, lexing 13–19 ms, and parsing 22–30 ms. Semantic analysis varied between about 0.22 and 0.40 seconds across runs; its allocation count was stable at about 67 MB after caching qualified type names, down from about 196 MB. Generic expansion used about 27 MB and took about 0.07–0.11 seconds. These results show that parsing runtime sources is not the main build-time cost.

## Changes

- Cached the LLVM reference-type set once per emission instead of rebuilding a `HashSet` on every Luma-to-LLVM type conversion. `FunctionEmitter` shares this set, and the emitter also caches its managed-class reachability set.
- Precomputed LLVM function names and overload-collision counts for each compilation. Previously, emitting a method repeatedly scanned the complete method list to determine its name.
- Applied LLVM `globaldce` to executable debug builds after compiler-managed definitions have internal linkage. The pass removes unreferenced functions and data without running the full optimization pipeline on the remaining debug code. Static-library builds skip this pass so exported library contents are preserved. A test compares object output with and without the debug pass.
- Removed LLVM's per-pass verification from optimized pipelines. LLVM verifies the completed module once before object emission, retaining a check of the exact IR sent to code generation while avoiding repeated whole-module verification after every optimization pass. On the benchmark fixture, this reduced the optimized build from about 5.05 seconds to about 1.1–1.2 seconds.
- Cached qualified type names by `TypeDeclaration` identity for the lifetime of a semantic analysis. This avoids repeatedly allocating the same `Namespace.Type` strings during binding and validation. On the benchmark fixture, semantic analysis allocation fell from about 196 MB to 67 MB and its measured GC collections fell from roughly 13 to 5.
- Added LLVM backend subphase measurements for target initialization, IR parsing, target-machine setup, optimization/dead-code passes, final verification, and object emission. The profiler remains opt-in and does not add profiling overhead to normal builds.
- Added a phase benchmark that reports elapsed time, per-thread allocations where the phase stays on one thread, GC collections, and emitted IR size. Allocation reporting marks asynchronous phases unavailable when an async continuation moves to another thread.

These changes preserve the full generated IR text in the intermediate `.ll` file. Dead-code elimination currently happens during native object generation, after IR emission.

## Runtime-library artifact caching

A separate cached runtime object was investigated further and is not safe to add as a cache-only change. Today, runtime method definitions are emitted with internal linkage because their owner is a runtime source type, and runtime static fields are likewise internal. A standalone runtime archive therefore does not currently export the symbols an application object would need. The generated class descriptors and interface IDs are also private to an LLVM module. Duplicating them in the application and runtime objects would make descriptor and interface identity depend on which module created an object.

There is additional shared state in each emitted module: exception-handler and pending-exception thread locals, stack-trace thread locals, and type-initialization state. A method compiled into a cached object would use that object's state, while application code would use a second copy. Type construction factories are generated from the complete set of runtime types and constructors in the current project, and generic specializations are selected while analyzing the merged runtime and application sources. A naive cached archive could therefore fail across exceptions, stack traces, static initialization, dynamic `Type.New`, generic calls, interface dispatch, or reference ownership, even if ordinary calls linked.

To make an artifact cache safe, the compiler first needs a module ABI that defines ownership and export rules for methods, static fields, class/interface descriptors, type-initialization state, exception and stack state, and intrinsic helpers. The runtime module would then need to leave consumer-specific generic and dynamic-type factories for the application compilation. The existing static-library option does not provide these runtime-module rules. A cache key based on runtime source hashes, compiler/runtime ABI version, target, and code-generation options can then handle invalidation, but it cannot replace this module split.

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
