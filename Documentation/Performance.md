# Compiler performance

## Measurement setup

`Tests/LumaSharp.Benchmarks/CompilerPerformanceBenchmarkTests.cs` measures a complete build of one LumaSharp program with the runtime library enabled. The fixture uses `List<int32>` to include runtime generic expansion and collection lowering. It records source loading, lexing, parsing, async lowering, generic expansion, semantic analysis, LLVM IR emission (including runtime and method-body subphases), LLVM verification/object generation, and native linking. Debug and optimized builds each have one warmup followed by three measured runs; reported build times are medians.

The figures below are from the Windows x64 development environment using .NET 10 and LLVM 22. They are approximate and should be compared on the same machine and toolchain. The runtime library at measurement time contained 84 source files and about 5,100 lines.

## Results

| Measurement | Original | After earlier optimizations | Current |
| --- | ---: | ---: | ---: |
| Debug build median | 3.12 s | 0.99 s | ~0.84 s |
| Optimized build median | 6.00 s | 5.24 s | ~1.02 s |
| LLVM IR emission, debug | 1.16 s | 0.15 s | ~0.11 s |
| LLVM IR emission allocations, debug | 1.05 GB | 156 MB | ~114 MB |
| LLVM method body emission, debug | 0.88 s | 0.11 s | ~0.09 s |
| LLVM object generation, debug | 1.44 s | 0.26 s | ~0.08 s |
| Semantic analysis allocations | ~196 MB | ~196 MB | ~67 MB |

The latest benchmark run measured debug builds at 0.79–0.92 seconds and optimized builds at 1.01–1.02 seconds across three measured samples. Their LLVM `default<O2>` pipeline took about 0.37–0.40 seconds, LLVM IR parsing about 0.11–0.12 seconds, and object generation about 0.08 seconds. Source discovery and loading took about 5–7 ms, lexing 18–22 ms, and parsing 23–29 ms. Semantic analysis varied between about 0.19 and 0.36 seconds across runs; its allocation count was stable at about 67 MB after caching qualified type names, down from about 196 MB. Generic expansion used about 27 MB and took about 0.06–0.09 seconds. These results show that parsing runtime sources is not the main build-time cost.

## Changes

- Cached the LLVM reference-type set once per emission instead of rebuilding a `HashSet` on every Luma-to-LLVM type conversion. `FunctionEmitter` shares this set, and the emitter also caches its managed-class reachability set.
- Precomputed LLVM function names and overload-collision counts for each compilation. Previously, emitting a method repeatedly scanned the complete method list to determine its name.
- Applied LLVM `globaldce` to executable debug builds after compiler-managed definitions have internal linkage. The pass removes unreferenced functions and data without running the full optimization pipeline on the remaining debug code. Static-library builds skip this pass so exported library contents are preserved. A test compares object output with and without the debug pass.
- Removed LLVM's per-pass verification from optimized pipelines. LLVM verifies the completed module once before object emission, retaining a check of the exact IR sent to code generation while avoiding repeated whole-module verification after every optimization pass. On the benchmark fixture, this reduced the optimized build from about 5.05 seconds to about 1.1–1.2 seconds.
- Cached qualified type names by `TypeDeclaration` identity for the lifetime of a semantic analysis. This avoids repeatedly allocating the same `Namespace.Type` strings during binding and validation. On the benchmark fixture, semantic analysis allocation fell from about 196 MB to 67 MB and its measured GC collections fell from roughly 13 to 5.
- Added LLVM backend subphase measurements for target initialization, IR parsing, target-machine setup, optimization/dead-code passes, final verification, and object emission. The profiler remains opt-in and does not add profiling overhead to normal builds.
- Replaced duplicate-declaration checks that copied and searched the whole LLVM module with an exact declaration set. The runtime-library fixture's IR emission allocations fell from about 156 MB to 114 MB (about 27% fewer), while keeping the generated module verifiable.
- Added a phase benchmark that reports elapsed time, per-thread allocations where the phase stays on one thread, GC collections, and emitted IR size. Allocation reporting marks asynchronous phases unavailable when an async continuation moves to another thread.

These changes preserve the full generated IR text in the intermediate `.ll` file. Dead-code elimination currently happens during native object generation, after IR emission.

## Array.IndexOf

`Tests/LumaSharp.Benchmarks/ArrayIndexOfBenchmarkTests.cs` compares the full-array, start-index, and section overloads against Release C# `Array.IndexOf`. It covers hit and miss cases for booleans, character and integer widths, native integers, floats, doubles, enums, nullable values, strings, a user struct with `IEquality<T>`, and user classes with ordinary and scope-borrowed equality. Each case searches a 2,048-element array 100,000 times; nine measurements are taken after warmup. Luma is built with `Optimize: true` and `TargetCpu: "native"`, and checksums are compared before timings are reported.

On the Windows x64 development machine, repeated runs show these broad patterns:

| Element category | Observed Luma result versus C# |
| --- | --- |
| `float` / `double` | About 11–17× faster for `float` and 4–5× for `double`. |
| `int32` / `uint32`, `int64` / `uint64`, native integers | Usually near parity; section searches are often up to about 25% faster, while some start-index cases trail slightly. |
| `bool`, `int8`, `uint8`, byte enums | Full searches are near parity; start-index calls can be about 1.5× slower, and section calls are usually within 20%. |
| `char`, `int16`, `uint16` | The 512-bit native-target path brings full scans near parity; start-index scans remain about 1.3–1.5× slower, with section results closer. |
| `int32?` | Four-at-a-time scalar lowering improved full scans by about 25%, but they remain roughly 1.6× slower; start-index and section searches are within about 10%. |
| strings | Full searches are near parity; ranged searches are often about 1.5× faster. |
| user equality types | Struct timings vary by overload and run (ranged searches are near parity). Null-heavy class arrays benefit from the dedicated null path; class results ranged from about 1.5× slower to 2× faster across runs. A `scope` equality parameter is supported but does not change `IEquality<T>` itself. |

These figures are directional rather than a cross-machine guarantee. The benchmark compares each language's native array API, uses a small hot array, and is sensitive to CPU frequency and cache state; repeated class and user-struct medians varied substantially. The remaining start-index, narrow-integer, nullable-value, and user-equality gaps are tracked by the per-type matrix, so future changes can be evaluated without masking one overload's regression behind an aggregate score.

The implementation specializes primitive and enum arrays with vector comparisons and ordered lane-mask extraction, then scans any remaining tail scalarly. Reference arrays preserve `IEquality<T>` dispatch or identity semantics; value structs require the equality interface. The List implementation delegates its active range to this API.

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
