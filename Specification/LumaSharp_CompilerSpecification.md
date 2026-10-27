# LumaSharp Compiler Specification

**Document:** 2 of 2 (Compiler)  
**Version:** 0.1 (Draft for implementation)  
**Target language specification:** *LumaSharp Language and Runtime Specification*, version 0.3  
**Status:** Normative unless marked *Informative* or *Implementation-defined*

---

## 1. Purpose and Conformance

This document specifies the minimum external behaviour and integration contract for a LumaSharp compiler. It is intended as an implementation reference, not as a prescription for the compiler's internal architecture.

The *LumaSharp Language and Runtime Specification* (the **Language Specification**) is authoritative for LumaSharp syntax, type rules, diagnostics required by the language, run-time behaviour, ownership, memory management, exceptions, and the runtime interface. This document supplements it with compiler packaging, build inputs, project configuration, output, and command-line requirements. If the two documents appear to conflict about language semantics, the Language Specification takes precedence.

A conforming compiler MUST:

- accept the language defined by the Language Specification, including its required compile-time errors and warnings;
- emit native code whose observable behaviour conforms to the Language Specification and its runtime contract;
- use LLVM for native code generation;
- provide the C# compiler library and separate command-line executable described below;
- report diagnostics and recover from source errors as specified in §7;
- produce no newly built executable or library if the compilation has any errors.

The lexer, parser implementation, syntax-tree representation, type-checker design, intermediate representations, generic-lowering strategy, optimisation algorithms, LLVM bindings, and internal class structure are implementation-defined. The implementation MAY use the Roslyn source code and documentation as an architectural reference. Roslyn does not define LumaSharp semantics and is not required as a compiler dependency.

## 2. Deliverables and Separation of Responsibilities

### 2.1 Compiler library

The compiler MUST be implemented in C# and distributed as a .NET dynamic library, for example `LumaSharp.Compiler.dll`.

The library MUST expose a callable compilation API that accepts a compilation request and returns a compilation result containing diagnostics, success/failure status, and produced artifact paths where applicable. Exact public type names are implementation-defined, but the API MUST represent at least:

- the ordered set of source files and their paths;
- build options and the target platform;
- output and intermediate directories;
- structured diagnostics, including severity, stable code, message, and source location when available;
- whether a build succeeded and the paths of its outputs.

The library MUST be usable without invoking the command-line executable. It MUST NOT require callers to parse console output to obtain diagnostics, and it SHOULD NOT write routine status messages directly to the console. File-system, parsing, binding, code-generation, and linker failures SHOULD be returned as structured diagnostics wherever practical.

An implementation may use different public names and data structures. The following is an illustrative minimal API shape, not a required class design:

```csharp
public interface ILumaCompiler
{
    CompilationResult Compile(
        CompilationRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record CompilationRequest(
    IReadOnlyList<string> SourcePaths,
    CompilerOptions Options,
    string OutputDirectory,
    string IntermediateDirectory);

public sealed record CompilationResult(
    bool Succeeded,
    IReadOnlyList<CompilerDiagnostic> Diagnostics,
    IReadOnlyList<string> OutputPaths);
```

`CompilerDiagnostic` should expose severity, code, message, and an optional source location. The implementation may add metadata, logging callbacks, in-memory source inputs, or other facilities without changing the required behaviour above.

### 2.2 Command-line executable

A separate executable, invoked as `luma`, MUST parse command-line arguments, load a project if supplied, determine the source files and options, invoke the compiler library, print diagnostics and status messages, and return a process exit code. The executable MUST call the compiler library rather than maintaining a separate compiler implementation.

The CLI may be a framework-dependent or self-contained .NET application. Packaging details are implementation-defined, provided the `luma` command is available as documented.

### 2.3 Runtime dependency

The compiler MUST generate code that follows the Language Specification's runtime contract. An executable MUST link the matching LumaSharp runtime as required by Part II of the Language Specification. A static-library build MUST expose or preserve the runtime dependencies needed when the library is linked into a final executable.

Appendix E of the Language Specification describes a reference ABI. A compiler and runtime built as one coordinated implementation MAY use a different internal ABI, but the generated program and runtime MUST agree exactly. If the reference ABI is used, its layouts, calling conventions, ownership conventions, and function contracts MUST be followed.

## 3. Build Command and Input Forms

The primary command MUST be simple to invoke:

```text
luma build <source-or-project> [additional-source-files] [options]
```

The following are valid forms:

```text
luma build src/Program.ls
luma build src/Program.ls src/Utilities.ls
luma build luma.json
luma build --help
luma --help
luma --version
```

A build invocation accepts exactly one of the following input modes:

1. **Source mode:** one or more `.ls` source-file paths. All supplied source files are compiled together into one output.
2. **Project mode:** one JSON project-file path. The project describes compiler options, while source files are discovered automatically (§4). A project path MUST NOT be combined with explicit source-file paths in the same invocation.

Source paths and the project-file path are interpreted relative to the current working directory unless absolute. Every explicit source path MUST identify an existing readable `.ls` file. Supplying no input, a path of an unsupported type, both input modes together, or an invalid command MUST produce an appropriate usage/error message and a non-zero exit code.

The CLI MUST support help for the root command and the `build` command. Help MUST describe input forms and the principal options. Unknown commands, unknown options, missing option values, and malformed arguments MUST be diagnosed; they MUST NOT silently be ignored.

### 3.1 Multiple source files

All source files supplied in source mode constitute one compilation. Declarations in one file MUST be resolvable from another file in the same compilation, subject to the Language Specification's namespace, accessibility, and declaration rules. The compiler MUST NOT emit a separate output for every source file.

Repeated references to the same canonical file path SHOULD be de-duplicated. The compiler MUST use a stable source ordering for deterministic diagnostics and intermediate output. Source mode SHOULD preserve the order of source paths supplied on the command line; project mode MUST use a deterministic ordering of discovered paths.

## 4. JSON Project and Source Discovery

### 4.1 Project rules

A project is a UTF-8 JSON file. The directory containing the project file is the **project root**. Project settings control how the discovered source files are compiled.

A project MUST NOT contain source-file paths or an explicit list of source files. Properties such as `sources`, `sourceFiles`, or `includeFiles` are invalid. When such a property is present, the CLI MUST report a project-configuration error rather than silently using or ignoring it.

The compiler MUST validate JSON syntax, property types, enum values, and supported schema version before starting compilation. Unknown properties SHOULD be rejected with a clear diagnostic so spelling mistakes do not silently change build behaviour.

### 4.2 Recursive discovery

For project mode, the CLI MUST recursively discover `.ls` files under the project root, including nested subdirectories. Discovery MUST NOT depend on the process's current working directory after the project has been located.

Discovery MUST exclude the configured output and intermediate directories, along with version-control metadata directories such as `.git`, `.hg`, and `.svn`, so generated or repository metadata files are not accidentally compiled. Additional directories MAY be excluded with the `excludeDirectories` setting. Exclusion paths are relative to the project root and identify directories, not individual source files. Directory symlinks SHOULD NOT be followed by default.

If no source files are discovered, the build MUST fail with a clear diagnostic. Files MUST be ordered deterministically, using their normalized project-relative paths, before the compiler library is invoked.

### 4.3 Project settings

The following JSON illustrates the initial project settings contract. All fields other than `schemaVersion` are optional and have the defaults shown here.

```json
{
  "schemaVersion": 1,
  "outputKind": "executable",
  "outputName": "Example",
  "outputDirectory": "bin",
  "intermediateDirectory": "obj",
  "targetTriple": "host",
  "unsafe": false,
  "optimize": false,
  "logLevel": "normal",
  "warningsAsErrors": false,
  "disabledWarnings": [],
  "excludeDirectories": []
}
```

| Setting | Type and accepted values | Default and meaning |
|---|---|---|
| `schemaVersion` | Integer | `1`. Unsupported versions are an error. |
| `outputKind` | `"executable"` or `"staticLibrary"` | `"executable"`. Selects the build kind defined in the Language Specification. |
| `outputName` | String | Project filename without `.json`. Used as the base output name; the platform-specific extension is chosen by the toolchain. |
| `outputDirectory` | Path string | `bin`. Destination for final output. |
| `intermediateDirectory` | Path string | `obj`. Destination for temporary and intermediate artifacts. |
| `targetTriple` | `"host"` or an LLVM target triple | `"host"`. Selects the host target or an explicitly named target. |
| `unsafe` | Boolean | `false`. If false, source using unsafe constructs is rejected as required by the Language Specification (including LS1901). It does not change the meaning of valid safe code. |
| `optimize` | Boolean | `false`. Enables additional LLVM optimisation when true; optimisation MUST preserve language semantics. |
| `logLevel` | `"quiet"`, `"normal"`, `"verbose"`, or `"debug"` | `"normal"`. Controls informational build logging, not whether required diagnostics are reported. |
| `warningsAsErrors` | Boolean | `false`. Treats warnings as errors for the current build. |
| `disabledWarnings` | Array of diagnostic-code strings | Empty. Disables listed warnings, subject to the Language Specification's diagnostic rules. |
| `excludeDirectories` | Array of relative directory paths | Empty. Adds project-root-relative directories to source-discovery exclusions. |

Relative project paths (including output and intermediate directories and exclusion paths) MUST be resolved from the project root. Absolute output and intermediate paths MAY be accepted. `outputDirectory` and `intermediateDirectory` MUST be kept outside source discovery even if their names are not `bin` and `obj`.

The project schema MAY be extended by a later schema version. Any extension MUST preserve the rule that project files do not enumerate source files. A compiler MAY support additional settings, but their names, types, defaults, and precedence MUST be documented.

## 5. Output and Directory Rules

### 5.1 Default directories

Unless overridden:

- in source mode, the output directory is `./bin` and the intermediate directory is `./obj`, both under the current working directory;
- in project mode, the output directory is `bin` and the intermediate directory is `obj`, both under the project root.

CLI paths that override these directories are resolved relative to the current working directory in source mode and relative to the project root in project mode, unless absolute.

The compiler MUST create missing output and intermediate directories when possible. Failure to create or write them MUST be reported as a diagnostic and must not terminate the process with an unhandled exception.

### 5.2 Default output name and kind

The default output kind is `executable`; it can be changed to `staticLibrary` in project settings or by a CLI option.

Unless `outputName` or a corresponding CLI option is supplied, the base output name is:

- the source filename without `.ls` when exactly one source file is supplied explicitly;
- `LumaProgram` when multiple source files are supplied explicitly;
- the project filename without `.json` in project mode.

The compiler/toolchain chooses the target-appropriate file extension and native format. The final artifact MUST be written to the output directory, not the intermediate directory. Intermediate LLVM IR, object files, temporary link outputs, diagnostic logs, and caches belong under the intermediate directory unless a documented option overrides this behaviour.

A compiler MUST NOT publish a partially linked or otherwise incomplete output as a successful build. It SHOULD emit to a temporary path and replace the final artifact only after successful completion. If a previous output exists and a new build fails, the old output MAY remain, but the CLI MUST report that the build failed and the output was not updated.

### 5.3 Build kind and entry point

Executable builds MUST validate the entry-point requirements in §22.1 of the Language Specification and report LS2201 for a missing or duplicate entry point. Static-library builds do not require a `Main` entry point.

## 6. Command-Line Options and Precedence

The CLI MUST provide command-line overrides for the commonly used settings. At minimum, it SHOULD support equivalents of:

```text
--kind <executable|staticLibrary>
--name <output-name>
--out-dir <path>
--intermediate-dir <path>
--target <host-or-LLVM-triple>
--unsafe / --no-unsafe
--optimize / --no-optimize
--log-level <quiet|normal|verbose|debug>
--warnings-as-errors
--no-warn <diagnostic-code>
```

The exact spelling of optional convenience aliases is implementation-defined, but the `luma build` command and the documented options MUST remain consistent.

Settings are resolved in this order, from lowest to highest precedence:

1. compiler defaults;
2. project settings, when a project is used;
3. command-line options.

In source mode, only compiler defaults and command-line options apply. A CLI option MUST override a project value when both specify the same setting. Repeated options with scalar values SHOULD use the last value supplied; duplicate or conflicting options that cannot be resolved this way MUST be diagnosed.

`logLevel` controls status and debugging output only. It MUST NOT suppress errors, required warnings, or source diagnostics that remain enabled under warning configuration. `quiet` may suppress success/progress messages.

## 7. Diagnostics, Recovery, and Failure Handling

### 7.1 Diagnostic contract

Each source diagnostic MUST contain severity, a stable diagnostic code, a human-readable message, and source file, line, and column when a source location exists. A source range SHOULD be included where possible. Line and column numbering MUST follow §2.7 of the Language Specification: 1-based, with columns measured in UTF-16 code units.

Language diagnostics MUST follow Chapter 23 of the Language Specification, including its required `LS` codes and required warnings. Compiler/project/CLI diagnostics SHOULD use a separate stable code range or prefix so they cannot be confused with language diagnostics.

A readable text format such as the following is recommended:

```text
src/Program.ls(12,8): error LS1302: Nullable value may be null here.
luma.json: error LMC1002: Unsupported project schemaVersion '2'.
```

Diagnostics MUST be printed to standard error by the CLI. Informational progress and successful-output messages SHOULD be printed to standard output. If no source location is available, the diagnostic MUST still identify the relevant project, path, option, compiler stage, or linker operation where possible.

### 7.2 Multiple diagnostics and recovery

The compiler MUST attempt to report all reasonably discoverable errors and warnings in one invocation. Lexing and parsing SHOULD recover after malformed input and continue into later declarations and files. Binding and semantic analysis SHOULD continue where safe to do so. The compiler MUST NOT emit cascaded errors for names that have already been reported as undefined, as required by §23.4 of the Language Specification.

An error in one source file SHOULD NOT prevent diagnostics from being collected from other readable source files. A failure that makes further analysis impossible (for example, an invalid project file or an unreadable source needed to form the compilation) may stop the affected stage, but all diagnostics already discovered MUST still be reported.

If any error remains after diagnostic configuration, the compiler MUST NOT emit or update the final executable or library. Warnings do not prevent output unless promoted to errors. No code generation or linker failure may be reported as a successful build.

### 7.3 Compiler failures and cancellation

Expected input, filesystem, target, LLVM, and linker failures MUST be handled and reported. The CLI and library MUST NOT crash due to ordinary malformed source or project input. Unexpected internal exceptions MUST be caught at the compilation boundary, reported as an internal compiler failure, and optionally include a stack trace at `debug` log level. They MUST NOT be disguised as user source errors.

Cancellation MAY be supported by the library API and CLI. If cancelled, the build MUST stop safely, MUST NOT publish partial output, and MUST return a non-zero exit code.

### 7.4 Exit codes

The CLI MUST use exit codes consistently. The following are recommended:

| Exit code | Meaning |
|---:|---|
| `0` | Build succeeded and requested output was produced. |
| `1` | Compilation failed because of source diagnostics or warnings promoted to errors. |
| `2` | Invalid command line, invalid project configuration, or invalid input paths. |
| `3` | Toolchain, runtime, linker, or internal compiler failure. |
| `130` | Build cancelled, if cancellation is supported. |

The exact values are implementation-defined if documented, but success MUST be distinguishable from every failure class.

## 8. Required Compiler Pipeline and LLVM

The implementation architecture is open, but the compiler MUST implement all semantic checks and code-generation obligations imposed by the Language Specification. A reasonable pipeline is:

1. **Input and source loading:** validate inputs, read UTF-8 source text, retain source paths, and construct source-location information.
2. **Lexing and parsing:** tokenize and parse every source into a representation that permits error recovery.
3. **Declaration collection:** collect namespaces, types, members, and declarations across the complete compilation; make cross-file declarations available for binding.
4. **Binding and semantic analysis:** perform name lookup, accessibility checks, type inference, conversions, overload resolution, generic constraint checking, nullability and definite-assignment analysis, `mut` validation, control-flow checks, scope/lifetime/escape analysis, and all other checks required by the Language Specification.
5. **Lowering:** transform language constructs into implementation-level forms while preserving evaluation order, checked arithmetic, exceptions, reference counting, scope unwinding, virtual/interface dispatch, and all other observable language behaviour.
6. **LLVM IR generation:** generate LLVM IR for the selected target and implement the required runtime calls and object/type representations.
7. **IR verification and optimisation:** verify generated LLVM IR before emission. If `optimize` is enabled, run compatible LLVM optimisation passes without changing observable behaviour. Verification or pass failures are build failures.
8. **Native emission and linking:** use LLVM's target support to emit native object code for the selected target, then invoke a compatible linker (such as LLVM's linker toolchain or a supported system linker) to produce the requested native executable or static library artifact.
9. **Publication:** publish the final artifact only after all required stages succeed.

This list defines responsibilities, not required class boundaries or a mandatory one-pass-per-stage implementation. Phases may be merged or repeated. Implementations MAY cache intermediate results, but cached state MUST NOT change the semantics or diagnostics of a build.

### 8.1 Target selection

`targetTriple: "host"` selects the host target supported by the installed LLVM/toolchain and matching runtime. Explicit targets MUST be validated before code emission. The compiler MUST fail clearly if LLVM, a compatible linker, native system libraries, or a matching LumaSharp runtime is unavailable for the requested target; it MUST NOT silently generate a binary for a different target.

Platform-specific executable formats, calling conventions, object formats, link arguments, and file extensions are handled by the target/toolchain layer. A target is supported only when the compiler and matching runtime can satisfy the platform assumptions and semantics in the Language Specification.

### 8.2 Optimisation requirements

Optimisation MUST preserve the Language Specification's observable behaviour. In particular, it MUST NOT remove required null, bounds, cast, overflow, or lifetime checks; change specified evaluation order; change exception behaviour; or invalidate deterministic reference-counting and `scope` cleanup semantics. An implementation MAY optimise checks only where it proves that doing so is semantics-preserving.

## 9. Testing and Quality Requirements

The compiler SHOULD provide automated tests covering:

- grammar and lexical rules, including Unicode source positions;
- required diagnostics in Chapter 23 of the Language Specification;
- name resolution, multi-file compilation, type checking, generics, nullable flow, `mut`, unsafe code, and lifetime/escape checks;
- runtime conformance for exceptions, checked operations, allocation, destruction, dispatch, strings, arrays, and entry-point execution;
- all supported target triples and the executable/static-library link path;
- recovery after multiple errors, warning configuration, invalid project files, missing source files, invalid command-line input, and failed output publication.

The implementation SHOULD include a conformance suite derived from normative examples and rules in the Language Specification. LLVM IR verification SHOULD run in development and test builds even when production configuration changes the amount of diagnostic logging.

## 10. Reference Material (Informative)

The following resources may help with implementation but do not supersede either LumaSharp specification:

- [Roslyn compiler API model](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/compiler-api-model) — an architectural reference for separating parsing, declaration collection, binding/semantic analysis, diagnostics, and emission.
- [Roslyn semantic analysis](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/work-with-semantics) — a reference for syntax trees, symbols, and semantic models.
- [LLVM code generator documentation](https://llvm.org/docs/CodeGenerator.html) — background on LLVM target-dependent native code generation.
- [LLVM backend overview](https://llvm.org/docs/WritingAnLLVMBackend.html) — background on LLVM-based native code generation.

---

## Appendix A: Project and CLI Error Examples

The following compiler diagnostics are recommended. Implementations may choose different codes if the mapping is documented and stable.

| Code | Condition |
|---|---|
| `LMC1001` | Invalid or incomplete command line. |
| `LMC1002` | Malformed JSON, unsupported schema version, or invalid project setting. |
| `LMC1003` | Project contains a forbidden source-file list/property. |
| `LMC1004` | No `.ls` files found for the build. |
| `LMC1005` | Input file or project path cannot be read. |
| `LMC1006` | Output/intermediate directory cannot be created or written. |
| `LMC2001` | Requested LLVM target or compatible toolchain is unavailable. |
| `LMC2002` | Runtime/linker dependency missing or linking failed. |
| `LMC9000` | Unexpected internal compiler failure. |

These are compiler-tool diagnostics, not replacements for the language diagnostic codes in Chapter 23 of the Language Specification.
