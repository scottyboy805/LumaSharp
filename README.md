# LumaSharp

LumaSharp is a statically typed language with C#-style syntax, ahead-of-time native compilation through LLVM, built-in nullability, and deterministic reference-counted memory management. The compiler and language are under active development; the implementation supports a broad but incomplete slice of the specifications.

## Repository layout

- `LumaSharp.Compiler/` — compiler library, parser, semantic analysis, LLVM backend, and build API.
- `LumaSharp.Cli/` — `luma` command-line executable.
- `LumaSharp.Library/` — compiler-known `System` library declarations and metadata.
- `Tests/LumaSharp.Tests/` — compiler, runtime/JIT, build API, and CLI tests; `Tests/fixtures/` contains sample inputs.
- `Documentation/` — practical language and project guides.
- `Specification/` — normative language/runtime and compiler specifications.
- `Progress/` — implementation roadmap and runtime ABI notes.

Start with [Documentation/Overview.md](Documentation/Overview.md) for a compact syntax reference. The [language specification](Specification/LumaSharp_LanguageSpecification.md) defines intended semantics; the [compiler specification](Specification/LumaSharp_CompilerSpecification.md) describes project files, outputs, and CLI behaviour. Some specified features are not implemented yet.

## Requirements

- .NET 10 SDK.
- A compatible system linker or archiver for native executable and static-library builds.
- LLVM runtime packages are supplied through the compiler's LLVMSharp NuGet dependencies for supported host architectures.

## Build and test

From the repository root:

```powershell
dotnet build LumaSharp.slnx
dotnet test Tests/LumaSharp.Tests/LumaSharp.Tests.csproj
```

Build one source file, or several files as a single program:

```powershell
dotnet run --project LumaSharp.Cli/LumaSharp.Cli.csproj -- build path/to/Program.ls
dotnet run --project LumaSharp.Cli/LumaSharp.Cli.csproj -- build path/to/Program.ls path/to/Library.ls
```

`--no-llvm` runs parsing, semantic analysis, LLVM verification, and object emission, then writes verified LLVM IR without linking. A normal build links and publishes the native output only after success:

```powershell
dotnet run --project LumaSharp.Cli/LumaSharp.Cli.csproj -- build path/to/Program.ls --no-llvm
dotnet run --project LumaSharp.Cli/LumaSharp.Cli.csproj -- build path/to/Program.ls --kind staticLibrary
```

Use `dotnet run --project LumaSharp.Cli/LumaSharp.Cli.csproj -- --help` for options. Common options include `--name`, `--out-dir`, `--intermediate-dir`, `--target`, `--unsafe`, `--optimize`, `--warnings-as-errors`, and `--no-warn <code>`.

### JSON project builds

A `.json` project file sets build options; it does not list source files. The CLI recursively finds `.ls` files below the project directory and excludes version-control, `bin`, `obj`, and configured output directories.

```json
{
  "schemaVersion": 1,
  "outputKind": "executable",
  "outputName": "Example",
  "outputDirectory": "bin",
  "intermediateDirectory": "obj",
  "targetTriple": "host",
  "unsafe": false,
  "optimize": false
}
```

Save as `Example.json`, then run `dotnet run --project LumaSharp.Cli/LumaSharp.Cli.csproj -- build Example.json`. Source discovery and relative paths are rooted at the project file's directory. See the compiler specification for every setting and default.

## Compiler library

The compiler can also be called without the CLI. `LumaCompiler.Compile` compiles in-memory text to diagnostics and LLVM IR; `LumaBuild.BuildAsync` builds ordered source paths and returns structured diagnostics and artifact paths.

```csharp
var request = new CompilerBuildRequest(
    SourcePaths: ["Program.ls"],
    Options: new CompilationOptions(EmitLlvmIrOnly: true),
    OutputDirectory: "bin",
    IntermediateDirectory: "obj",
    OutputName: "Program");

CompilerBuildResult result = await LumaBuild.BuildAsync(request, cancellationToken);
```

## Current implementation

The compiler includes classes, structs, interfaces, enums, generics, delegates and closures, properties and indexers, nullable values/references, arrays, strings, pattern-capable `switch` and `is`, exceptions, unsafe pointers, native interop, static initialization, ownership/lifetime checks, and native code generation. Support varies by feature and edge case; recognized unsupported constructs produce diagnostics. The language and compiler specifications describe the wider design, while the tests document many implemented behaviours.

## License

See [LICENSE](LICENSE). Community expectations are in [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).
