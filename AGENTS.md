## Project
LumaSharp is a statically typed language with C#-style syntax, ahead-of-time native compilation through LLVM, built-in nullability, and deterministic reference-counted memory management. The compiler and language are under active development; the implementation supports a broad but incomplete slice of the specifications.

## Project layout
 
- `Specification/` contains the complete versioned specification for LumaSharp language and compiler.
- `Documentation/` contains the quick reference and detailed documentation of the language and it's usage.
- `LumaSharp.Cli/` contains the command line interface used to run the compiler. It calls LumaSharp.Compiler which contains the implementation.
- `LumaSharp.Compiler/` contains the actual compiler API and implementation, as defined by the language and compiler specification.
- `LumaSharp.Library/` contains the luma common runtime and other libraries, written themselfs in LumaSharp. It is equivilent to C standard library or C# core lib.
- `Tests/` contains the tests for the compiler and execution analysis.
- `Progress/` should contain any documents to track the current progress and implementation of LumaSharp by any agents, and should always remain up to date with any work done.
- You can use Unity CLI to work on this project if it is easier.

## Deliverable

- The agent should pick a specific task from the current progress (or specification if no progress can be determined) and complete it from start to end, including implementing runtime, any associated editor tooling, test suite and where appropriate a
- Once a task has been completed as above, the agent should create a commit and push changes to the main branch.