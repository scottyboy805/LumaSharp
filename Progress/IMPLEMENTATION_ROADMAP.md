# LumaSharp implementation status

This document is a current status snapshot. Historical implementation notes belong in Git history; the language and compiler specifications define supported behavior and explicit limits.

## Compiler and build system

LumaSharp is implemented in C# on .NET 10. The compiler parses and analyzes `.ls` sources, lowers supported language constructs to LLVM IR, verifies and optimizes that IR when requested, emits native objects, and links executables or static libraries. The `luma` CLI supports source builds and JSON project builds with recursive source discovery, runtime-library selection, target selection, diagnostics, and cancellation. The compiler library is also callable directly.

Unsupported source constructs and specialization limits are reported as diagnostics. Build and runtime conformance are covered by `Tests/LumaSharp.Tests`; `Tests/LumaSharp.Benchmarks` contains opt-in native comparisons with C# equivalents. Benchmark timings depend on the machine, workload, optimization settings, and process-startup measurement.

## Generics

Closed generic classes, structs, interfaces, delegates, and methods are specialized into concrete declarations. Generic constraints, constructed and nullable type patterns, arrays, method inference, method groups, lambdas, static storage per closed type, nested types, accessibility, and runtime type metadata are covered by compiler and execution tests.

Nested types carry only containing type parameters referenced by their own signatures, constraints, or bodies. Captured outer arguments precede the nested type's declared arguments in the flattened spelling: `Outer.Inner<int32, string>`. If the nested type does not use the containing parameter, it is omitted: `Outer.Inner<string>`. Closed generic `System.Type` handles are shared per compilation and expose the source type name and arguments in `Name`.

Generic specialization detects expansive recursive type and method instantiations and reports `LMC3000`. Generic type arguments are limited to 64 levels of nesting, and one compilation may create at most 16,384 closed generic method specializations. Open generic member bodies receive full expression binding when a closed specialization is created; diagnostics in an unused open body can therefore be deferred until that member is used.

The language does not currently support generic extension methods or targets, generic virtual/override methods, generic interface methods, variance, open-generic reflection, or unbound generic expressions. Some combinations of overload resolution and nested receiver/argument inference remain unsupported and produce `LS1101`.

## Runtime and library

The runtime uses deterministic reference counting, scoped allocations, exception unwinding, virtual/interface dispatch, type metadata, arrays, strings, closures, delegates, thread and monitor primitives, and a managed worker pool for async methods. `System.Type` exposes names, namespaces, type flags, stable handles, and `New`; this is not full reflection.

The library includes common sequential collections and monitor-based concurrent queue, stack, bag, and dictionary types. Concurrent collection operations synchronize per instance and enumerators use snapshots; they are not lock-free. Collection hash/equality behavior is constrained by `IHashable` and `IEquality<T>` where required.

`AsyncStream` declares pure virtual asynchronous byte-array operations. `FileStream` implements them without blocking the initiating caller: Windows submits overlapped requests and POSIX performs file operations on runtime workers. Reads and writes run in chunks and observe cooperative cancellation between OS calls; an active OS call cannot be interrupted. `MemoryStream` remains synchronous.

The runtime ABI marker is version 2. The current generated contract and its limits are described in [RUNTIME_ABI.md](RUNTIME_ABI.md); this marker does not yet provide a separately distributable shared runtime for independently linked modules.

## Current language limits

LumaSharp is not a C# compatibility implementation. The language specification's current boundaries include:

- no garbage collector or automatic cycle collection; strong reference cycles must be broken explicitly;
- no tuple syntax/deconstruction, local functions, object initializers, `init`, attributes, LINQ syntax, exception filters, or raw string literals;
- no function-pointer invocation, automatic native marshalling, or callbacks from native code into LumaSharp;
- no multicast delegates, async lambdas, custom schedulers, or synchronization contexts;
- no general reflection, generic argument enumeration, or arbitrary method invocation through `System.Type`;
- no conversion chaining for user-defined conversions, collection comparers, or dictionary enumeration;
- generic constraints, recursive specialization, inference, and open-body checking follow the limits in §12 of the language specification.

These boundaries are part of the current language/compiler contract unless revised in the specifications. Recognized unsupported forms should produce diagnostics instead of silently compiling with placeholder behavior.

## Verification

The full suite is run for this status update with:

```powershell
dotnet test LumaSharp.slnx --no-restore
```

The run passed all 544 compiler/runtime tests and all 4 benchmark tests. The compiler and CLI built successfully as part of the run. NuGet printed `NU1900` warnings because vulnerability metadata could not be fetched from `api.nuget.org`; the existing restore assets were sufficient and all builds/tests completed.
