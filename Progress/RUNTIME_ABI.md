# LumaSharp runtime ABI

The compiler emits the runtime helpers and runtime-owned data needed by a compilation
into its LLVM module. The current generated contract is identified as ABI version 2
by the LLVM module flag `LumaSharpRuntimeABI`. LLVM's `Error` module-flag behavior
causes bitcode/LTO linking to reject inputs that declare a different version.

`LumaSharp.Compiler.RuntimeAbi.Version` is the authoritative version constant. It
must be incremented whenever object layouts, helper signatures, ownership rules, or
runtime state change incompatibly. Compatible generated modules use the same version.

This marker is an initial compatibility boundary, not a standalone runtime package.
The runtime implementation is still emitted into generated modules, and ordinary
native archive linking may discard LLVM module flags. A distributable shared runtime
and compatibility checks for separately linked static libraries remain open work.

ABI version 2 adds a nullable weak-control pointer to class, string, and array headers.
The compiler emits synchronized control blocks on first use, target promotion and
invalidation helpers, and weak-handle accounting. Immortal strings and static objects
use detached control blocks so read-only literal storage is never modified.

`String.Ptr` exposes the string's NUL-terminated UTF-16 code-unit buffer as
`char*`; `Length` remains the number of UTF-16 code units and excludes the
terminator. String literals, the empty string, and dynamically created strings
all provide this terminated representation. Generated `System.IO` shims consume
UTF-16 paths and text: Windows passes them to wide-character APIs, while POSIX
shims transcode to UTF-8 at the operating-system boundary. This OS shim contract
does not define general C extern string marshalling; that will require an
explicit encoding and lifetime policy.
