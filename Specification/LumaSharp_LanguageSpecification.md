# LumaSharp Language and Runtime Specification

**Document:** 1 of 2 (Language and Runtime · Compiler)
**Version:** 0.3 (Draft for implementation)
**Status:** Normative unless a section or paragraph is marked *Informative*, or labelled *Reference*/*Recommended*

---

## Table of Contents

**Part I: Language**

1. [Introduction](#1-introduction)
2. [Lexical Structure](#2-lexical-structure)
3. [Types](#3-types)
4. [Variables and Assignment](#4-variables-and-assignment)
5. [Expressions and Operators](#5-expressions-and-operators)
6. [Statements and Control Flow](#6-statements-and-control-flow)
7. [Methods and Parameters](#7-methods-and-parameters)
8. [Classes](#8-classes)
9. [Structs](#9-structs)
10. [Interfaces](#10-interfaces)
11. [Enums](#11-enums)
12. [Generics](#12-generics)
13. [Nullability](#13-nullability)
14. [Ownership, Lifetime and `scope`](#14-ownership-lifetime-and-scope)
15. [Arrays](#15-arrays)
16. [Strings and Characters](#16-strings-and-characters)
17. [Delegates and Lambdas](#17-delegates-and-lambdas)
18. [Exceptions](#18-exceptions)
19. [Unsafe Code](#19-unsafe-code)
20. [Namespaces, Imports and Visibility](#20-namespaces-imports-and-visibility)
21. [Native Interop](#21-native-interop)
22. [Program Entry and Static Initialization](#22-program-entry-and-static-initialization)
23. [Diagnostics](#23-diagnostics)
24. [Features Excluded From This Version](#24-features-excluded-from-this-version)

**Part II: Runtime**

25. [Runtime Overview](#25-runtime-overview)
26. [Program Lifecycle](#26-program-lifecycle)
27. [Memory Management](#27-memory-management)
28. [Type Representation](#28-type-representation)
29. [Execution Semantics](#29-execution-semantics)
30. [Checked Operations and Numeric Behaviour](#30-checked-operations-and-numeric-behaviour)
31. [Exceptions](#31-exceptions)
32. [Runtime Bootstrapping and Compiler-Known Types (Informative)](#32-runtime-bootstrapping-and-compiler-known-types-informative)
33. [Native Interop at Run Time](#33-native-interop-at-run-time)

**Appendices**

- [A: Grammar (EBNF)](#appendix-a-grammar-ebnf)
- [B: Keywords](#appendix-b-keywords)
- [C: Operator Precedence](#appendix-c-operator-precedence)
- [D: Decision Log](#appendix-d-decision-log)
- [E: Runtime Interface Reference](#appendix-e-runtime-interface-reference-reference-abi-v1)
- [F: Complete Example Program](#appendix-f-complete-example-program)

---

# PART I: LANGUAGE

# 1. Introduction

## 1.1 Purpose

LumaSharp is a statically typed, natively compiled, general-purpose language with C#-style syntax. It takes forward only the modern and essential parts of C#, and changes or adds a few things. Programs are compiled ahead of time to platform-native code (x64, ARM, …) through LLVM. Memory is managed and fully automatic using **deterministic reference counting** (as in Swift), with no garbage collector.

This document is a combined **Language and Runtime Specification**. It describes the language (Part I), the behaviour and contract of the runtime that every compiled program links against (Part II), and enough detail on both that an independent implementation can be built from this document alone. The *Compiler Specification* (inputs, project files, outputs, command line) is a separate document.

The language is designed around six principles:

1. **Familiar syntax.** Where this document is silent, C# behaviour applies (§1.5).
2. **Deterministic memory management.** Class instances are reference counted and destroyed at a well-defined point, never by a tracing collector.
3. **Invisible ownership.** The compiler performs all ownership, borrowing, retain/release and lifetime analysis. Ordinary code contains no ownership syntax.
4. **Safe by default.** Null-safety, bounds checking, overflow checking and lifetime checking are on in ordinary code. Anything that can cause undefined behaviour requires an `unsafe` context.
5. **Reduced magic.** The language knows about very few library types. Most functionality lives in the standard library, which will eventually be written in LumaSharp itself.
6. **Implementation freedom.** This document defines observable behaviour. It does not prescribe compiler architecture, generic implementation strategy, array layout beyond what is stated, nullable representation, or optimisation techniques.

## 1.2 Structure of this Document

| Part | Chapters | Content |
|---|---|---|
| **Part I: Language** | 1–24 | Syntax and semantics of every language feature |
| **Part II: Runtime** | 25–33 | Program lifecycle, memory management, type representation, execution semantics, exceptions, native interop, the runtime interface |
| **Appendices** | A–F | Grammar, keywords, precedence, decision log, runtime interface reference, a complete example |

Chapters in Part I describe what programs *mean*; where behaviour at run time needs more precision than fits in the language chapter, the language chapter states the observable rule and Part II states how a conforming runtime provides it.

## 1.3 Terminology and Conformance Language

The words **MUST**, **MUST NOT**, **SHOULD**, **MAY** are used as in RFC 2119.

| Term | Meaning |
|---|---|
| **Compile-time error** | A program containing it is not valid. A conforming compiler MUST report it and MUST NOT produce an executable or library for the program. |
| **Warning** | A conforming compiler MAY accept the program. Warnings marked *required* MUST be reported. |
| **Runtime exception** | A well-defined exception is thrown (see §18 and Chapter 31). |
| **Fatal error** | The program terminates immediately in the manner described in §26.5. Fatal errors cannot be caught. |
| **Implementation-defined** | Behaviour is chosen by the implementation and MUST be documented by it. |
| **Unspecified** | Behaviour is chosen by the implementation and need not be documented. |
| **Undefined behaviour (UB)** | The language places no requirements on the result. UB can occur only inside an `unsafe` context (or through a runtime bug). Code outside `unsafe` MUST NOT be able to cause UB. |

> **Safety invariant.** A program that contains no `unsafe` contexts MUST NOT exhibit undefined behaviour: it never observes a freed object, a null value through a non-nullable type, an out-of-bounds array element, or an integer overflow that silently wraps.

## 1.4 Annotation Conventions

- Code examples use `lumasharp` fenced blocks. A trailing `// ...` comment states the result.
- `// error` means the line is a compile-time error. `// warning` means a warning. `// throws X` means a runtime exception.
- **`[P-n]`** marks a *provisional* decision: the design discussion did not explicitly settle it, so this document proposes a rule. Every `[P-n]` is listed in [Appendix D](#appendix-d-decision-log) for review.
- `Print(...)` and `Console.WriteLine(...)` in examples stand for standard-library output facilities, which are outside this specification.
- *Informative* text explains or illustrates but imposes no requirement.

## 1.5 Relationship to C#

LumaSharp deliberately resembles C#. The following rule applies throughout:

> **C# fallback rule.** If this specification does not define a syntactic form or semantic rule, and the feature is not excluded in §24, the behaviour is that of C# (ECMA-334 / current Microsoft C#), adapted for LumaSharp's type system. In case of doubt, choose the C# behaviour that makes most sense for a deterministic, null-safe, natively compiled language. `[P-25]`

Features inherited from C# without change include operator precedence and associativity, short-circuit evaluation, `if`/`while`/`do`/`for`/`return`, `try/catch/finally`, the conditional operator `?:`, null-coalescing `??`, null-conditional `?.`, generic argument syntax (`List<int32>`), and the C# visibility modifiers.

### Principal departures from C#

| Area | C# | LumaSharp |
|---|---|---|
| Memory | Tracing GC | Deterministic reference counting; `scope` allocation |
| Primitive names | `int`/`uint` are 32-bit; `long`, `byte`, … | `int`/`uint` (native pointer width), `int8`…`uint64`, `float`, `double` |
| Nullability | Opt-in annotations (warnings) | Built in: `Foo` is never null; `Foo?` may be null (errors) |
| Local declarations | `var`, `const` | `let` (immutable), `var` (mutable), `const` (explicit type) |
| Multiple declarators | `int a = 1, b = 2;` | `int32 a, b = 1, 2;` |
| Method mutability | No notion | Instance methods are non-mutating unless suffixed `mut` |
| Inheritance | `sealed`/`abstract` keywords | Classes sealed by default; `virtual class` opens them; no `abstract`/`sealed` keywords |
| Constructors | `ClassName(...)` | `this(...)` |
| Static constructor | `static ClassName()` | `static this()` |
| Finalizer | Non-deterministic | `~ClassName()`, deterministic |
| Variadics | `params T[]` | `T... name` |
| Struct creation | `new S()` | `S()` (`new` is for heap allocation only) |
| Struct reference fields | Any | Only nullable references |
| Iteration | `foreach (var x in c)` | `for (let x in c)` |
| Loop exits | `goto`, labels | Loop labels with `break label;` / `continue label;` |
| Switch | `break` required | No fall-through; `case 1, 2, 3:` |
| Overflow | Unchecked by default | Checked by default; `unchecked` requires `unsafe` |
| `!=` | Overloadable | Always derived from `==` |
| `object` | `Equals`, `GetHashCode`, `GetType`, `ToString` | Only `ToString` |
| Pointers | `->` and `.` | `.` only (automatic dereference) |
| Non-nullable `string` default | No default value | Non-nullable reference |
| Events, records, partial, params, goto, … | Present | Absent (§24) |

## 1.6 Language Version 1 Scope

**Included:** classes, structs, interfaces, enums, generics (with constraints), delegates and lambdas, properties, indexers, operators, exceptions, arrays (single-dimensional, jagged, and multi-dimensional up to three dimensions), strings, nullable types, pattern-capable `switch` and `is`, loop labels, unsafe code and pointers, inline arrays, native interop, deterministic reference counting, `scope`, `mut`, `let`, `const`, `readonly`, multiple assignment, `for`-in loops, optional/named arguments, variadic arguments.

**Excluded:** see §24.

## 1.7 Compiler-Known Library Types

The language itself needs a small, fixed set of types to exist in the standard library/runtime. They live in namespace `System` and are called **compiler-known types**. The compiler MUST resolve them by these exact names; during bootstrap it MAY supply them intrinsically (see Chapter 32 and the Compiler Specification). `[P-22]`

| Type | Purpose and required members |
|---|---|
| `object` (`System.Object`) | Root of the class hierarchy. Only member: `virtual string ToString()` |
| `System.ValueType` | Special struct base marker for primitive, enum, and struct values; boxing produces an object reference implicitly (§3.7, §9.1) |
| `string` (`System.String`) | Immutable string: `int32 Length`, `char this[int32]`, `==`, `+` (§16) |
| `Exception` | Base class of everything thrown: `string Message { get; }`, `Exception? InnerException { get; }`, constructors `this()`, `this(string message)`, `this(string message, Exception? inner)` |
| `NullReferenceException`, `InvalidCastException`, `IndexOutOfRangeException`, `OverflowException`, `DivideByZeroException`, `OutOfMemoryException`, `TypeInitializationException` | Thrown by language operations (§18.5); each derives from `Exception` and has the `Exception` constructors |
| `IEnumerable<T>`, `IIterator<T>` | Protocol behind `for (let x in c)` (§6.6) |
| `Array` (implicit base of all arrays) | `int32 Length` (total element count), `int32 Rank`, `int32 GetLength(int32 dimension)` (§15) |
| `Delegate` (implicit base of all delegates) | Base of all delegate types |

Other library interfaces mentioned in this document, such as `IHashable`, `IEquality<T>` and `IComparable<T>`, are **ordinary library types**, not compiler-known. The compiler never refers to them by name.

> The manual/unmanaged allocation facility (the planned `NativeMemory`) is **not** part of this version (§24).

---

# 2. Lexical Structure

## 2.1 Source Files

- Source files use the extension **`.ls`**.
- Source text is **UTF-8**. A leading byte-order mark MUST be accepted and ignored.
- Line terminators are `\n`, `\r\n`, or `\r`. All are equivalent.
- A source file is a sequence of tokens separated by whitespace and comments. There is **no preprocessor**.
- Statements are terminated by `;`. Blocks use `{ }`.

## 2.2 Whitespace and Comments

```lumasharp
// single-line comment
/* multi-line comment
   (does not nest) */
/// documentation comment: treated as an ordinary comment
```

Whitespace has no meaning beyond separating tokens.

## 2.3 Identifiers

An identifier begins with a Unicode letter or `_`, followed by letters, decimal digits, or `_`. Identifiers are case-sensitive.

A reserved keyword may be used as an identifier by prefixing it with `@`:

```lumasharp
int32 @class = 5;   // identifier named "class"
```

The identifier `_` on its own is a *discard* where a pattern or assignment target allows it (§6.8).

## 2.4 Keywords

**Reserved keywords** (cannot be identifiers unless `@`-prefixed):

```
as base bool break case catch char checked class const continue default delegate
do double else enum extern false finally float for if in int int8 int16 int32 int64
interface internal is let mut namespace new null object operator out override
private protected public readonly ref return scope sizeof static string struct
switch this throw true try uint uint8 uint16 uint32 uint64 unchecked unsafe using var
virtual void while
```

**Contextual keywords** (keywords only in a specific position): `get`, `set`, `field`, `value`, `when`, `and`, `or`, `not`, `where`. A loop label is an identifier followed by `:` at the start of a loop statement (§6.7).

**Reserved for future versions** (an identifier with this spelling is a compile-time error, LS0104): `async`, `await`, `lock`. The attribute syntax `[ ... ]` before a declaration is likewise reserved. `[P-23]`

Features that will **never** be part of the language (`event`, `record`, `partial`, `params`, `sealed`, `abstract`, `goto`, `fixed`, `stackalloc`, `dynamic`, LINQ query syntax, and similar) are not keywords; those spellings are ordinary identifiers.

See [Appendix B](#appendix-b-keywords) for the complete categorised list.

## 2.5 Literals

### 2.5.1 Boolean and null

```lumasharp
bool a = true;
bool b = false;
Foo? c = null;       // null is only assignable to nullable types, see §13
```

### 2.5.2 Integer literals

| Form | Example | Notes |
|---|---|---|
| Decimal | `42`, `1_000_000` | `_` digit separator allowed between digits |
| Hexadecimal | `0xFF`, `0xDEAD_BEEF` | |
| Binary | `0b1010_0101` | |

**Typing:** an unsuffixed literal has the first of `int32`, `uint32`, `int64`, `uint64` that can represent its value. Suffixes: `u` / `U` (unsigned), `l` / `L` (64-bit), `ul` / `UL` (unsigned 64-bit).

```lumasharp
let a = 5;            // int32
let b = 3_000_000_000; // uint32 (does not fit int32)
let c = 5L;           // int64
let d = 5UL;          // uint64
```

A constant integer expression converts implicitly to any narrower integer type if the value fits (§5.9).

### 2.5.3 Floating-point literals

```lumasharp
let a = 1.5;      // double (default)
let b = 1.5f;     // float  (suffix f/F)
let c = 1.5d;     // double (suffix d/D)
let d = 2e10;     // double
let e = 1_000.5;  // digit separators allowed
```

### 2.5.4 Character literals

A `char` literal is one UTF-16 code unit in single quotes.

```lumasharp
char a = 'x';
char b = '\n';
char c = '\u00E9';   // é
```

Escape sequences: `\0 \a \b \f \n \r \t \v \\ \' \" \xH{1,4} \uHHHH`. A character outside the Basic Multilingual Plane does not fit in a `char` literal (compile-time error).

### 2.5.5 String literals

```lumasharp
string a = "hello\tworld\n";
string b = @"C:\no\escapes";     // verbatim string: backslash is literal, "" is a quote
string c = "Unicode: \u00E9 \U0001F600";
```

- A string literal has type `string`. Its encoding and storage are defined in §16 and §28.10.
- Interpolated strings (`$"..."`), raw string literals, and UTF-8 literal suffixes are **not** part of this version.

## 2.6 Operators and Punctuators

```
{ } [ ] ( ) . , : ; ? ?. ?[ ?? ??= => ...
+ - * / % & | ^ ~ ! << >> && || ++ --
= += -= *= /= %= &= |= ^= <<= >>= == != < > <= >=
```

**Tokenisation notes:**

- Greedy matching: the longest valid token is chosen (`>>=` before `>>` before `>`).
- `>` and `>>` are split when closing generic argument lists (`List<List<int32>>`), as in C#.
- The postfix null assertion `!` (§13.4) and the inequality operator `!=` share a first character. The tokeniser always prefers `!=`, so `a!=b` is `a != b`. An assertion directly followed by `=` must be separated by whitespace or parentheses.

## 2.7 Line/Position Information

A conforming compiler MUST track line and column for each token (1-based, columns measured in UTF-16 code units) for use in diagnostics (§23).

---

# 3. Types

## 3.1 Type Categories

Every type is exactly one of:

| Category | Types | Copy semantics | Identity |
|---|---|---|---|
| **Value types** | primitives (except `string`), `struct`, `enum`, nullable value types `T?`, pointers | Assignment copies the value | None |
| **Reference types** | `class`, `interface`, `delegate`, arrays, `string`, `object` | Assignment copies a *reference* (+1 reference count) | Object identity |

`void` is not the type of any value; it is only a method return type.

## 3.2 Primitive Types

The complete list of primitive types is:

| Keyword | Size | Description | Default |
|---|---|---|---|
| `bool` | 1 byte | `true` / `false` | `false` |
| `char` | 2 bytes | UTF-16 code unit (unsigned 16-bit) | `'\0'` |
| `int8` / `uint8` | 1 byte | signed / unsigned integer | `0` |
| `int16` / `uint16` | 2 bytes | signed / unsigned integer | `0` |
| `int32` / `uint32` | 4 bytes | signed / unsigned integer | `0` |
| `int64` / `uint64` | 8 bytes | signed / unsigned integer | `0` |
| `int` | native pointer size | platform-native signed integer; pointer-width | `0` |
| `uint` | native pointer size | platform-native unsigned integer; pointer-width | `0` |
| `float` | 4 bytes | IEEE 754 binary32 | `0.0` |
| `double` | 8 bytes | IEEE 754 binary64 | `0.0` |
| `string` | reference | Immutable text (a reference type, §16) | none; explicit initialization required |

`int` and `uint` are platform-native signed and unsigned integer types whose size matches the native pointer size (normally 32 or 64 bits). They are intended for native-width values and can hold address-sized integer values in safe code; converting between an actual pointer and an integer remains subject to the unsafe rules in §3.6.2 and §19.3. For numeric conversions, promotions, and overflow, `int`/`uint` follow the corresponding `int32`/`uint32` rules on 32-bit targets and `int64`/`uint64` rules on 64-bit targets; each native type is implicitly convertible to its same-width fixed-width counterpart and vice versa. There are no `nint`/`nuint` spellings. Sizes, indices, array lengths, and `sizeof` results remain `int32`; pointer differences yield `int64`, and pointer ↔ integer casts accept any integer type (§19.3).

- Signed integers use two's complement.
- Every integer type exposes the compile-time constants `MinValue` and `MaxValue`:

```lumasharp
int32 big = int32.MaxValue;   // 2147483647
uint8 low = uint8.MinValue;   // 0
```

- Floating-point arithmetic follows IEEE 754 and never throws (division by zero yields ±∞ or NaN).
- `int` and `uint` are reserved keywords for the native-width primitive types above. The names `long`, `byte`, `float32`, and `float64` are not keywords and have no special meaning.

## 3.3 `object` and `string`

- `object` is the implicit root of every class and the target of boxing (§3.7).
- `string` is an immutable non-nullable reference type. It has no default value; use `string?` when a null default is required.
- Both keywords are aliases for `System.Object` and `System.String`.

## 3.4 Type Syntax

```
type              = non_nullable_type [ "?" ]
non_nullable_type = primitive | type_name [ type_args ] | array_type | pointer_type
array_type        = non_nullable_type rank_specifier { rank_specifier }
rank_specifier    = "[" "]" | "[" "," "]" | "[" "," "," "]"
pointer_type      = non_nullable_type "*" { "*" }
```

```lumasharp
int32                   // value type
Foo                     // non-nullable reference
Foo?                    // nullable reference
int32?                  // nullable value
int32[]                 // array of int32
int32[,]                // two-dimensional array
int32[,,]               // three-dimensional array
int32[][]               // jagged array (array of int32[])
int32[,][]              // 2-D array whose elements are int32[] (non-nullable elements!)
Foo?[]                  // array whose elements may be null
Foo[]?                  // nullable array of non-null Foo
Dictionary<string, int32>
int32*                  // pointer (unsafe only)
int32[16]               // inline array (unsafe only, in declarations: §15.9)
```

`?` binds to the type to its left: `Foo?[]` is an array of `Foo?`; `Foo[]?` is a `Foo[]` that may be null. Rectangular arrays have at most **three** dimensions; `T[,,,]` is a compile-time error (LS1503).

## 3.5 Default Values and Default-Constructibility

Every **value type** has a *default value*: the all-zero bit pattern. A **nullable** type's default is `null`. Every **non-nullable reference type**, including `string`, has no default value because it cannot be null and can only be assigned an allocation or another valid non-null value. A type is **default-constructible** if it has a default value:

| Type | Default-constructible? |
|---|---|
| primitives, enums, pointers | yes (zero) |
| structs (all of them, see §9.1) | yes |
| `T?` for any `T` | yes (`null`) |
| inline arrays of default-constructible element type | yes |
| every non-nullable reference type (`string`, `object`, classes, interfaces, delegates, arrays) | **no** |

The expression `default` (target-typed) produces the default value and is a compile-time error for a type that is not default-constructible. `default(T)` follows the same rule when `T` names a concrete type. Inside generic code, `default(T)` and target-typed `default` for `T` require `T` to have a `default`, `struct`, or `enum` constraint (§12.3). A concrete nullable reference such as `default(MyClass?)` is valid and produces `null`.

```lumasharp
int32 a = default;        // 0
Foo?  b = default;        // null
string? s = default;      // null
Foo   c = default;        // error: Foo is not default-constructible
Vector v = default;       // zeroed Vector
```

The consequences of these rules are collected in §4.3 (locals), §8.3.1 (fields), §9.1 (struct fields), §15.2 (array elements) and §12.4 (type parameters). **It is never possible to assign `null` to a non-nullable reference type or to observe one holding null in code outside `unsafe` contexts.**

## 3.6 Conversions

Conversions are *implicit* (performed automatically) or *explicit* (require a cast `(T)e`). The built-in conversions are listed here. A class or struct may also declare public static `implicit operator` and `explicit operator` conversions (§9.4).

### 3.6.1 Implicit conversions

| From | To |
|---|---|
| any type `T` | `T` (identity) |
| `int8` | `int16`, `int32`, `int64`, `float`, `double` |
| `uint8` | `int16`, `uint16`, `int32`, `uint32`, `int64`, `uint64`, `float`, `double` |
| `int16` | `int32`, `int64`, `float`, `double` |
| `uint16` | `int32`, `uint32`, `int64`, `uint64`, `float`, `double` |
| `int32` | `int64`, `double` |
| `uint32` | `int64`, `uint64`, `double` |
| `int64`, `uint64` | *(none; see constant conversion)* |
| `float` | `double` |
| `char` | `uint16`, `int32`, `uint32`, `int64`, `uint64`, `float`, `double` |
| enum `E` | its underlying integer type (and, transitively, anything that type converts to) |
| class `D` | any base class `B` of `D`; any interface `D` implements |
| interface `I` | any base interface of `I`; `object` |
| struct / enum / primitive `V` | `object`; any interface `V` implements (**boxing**, §3.7) |
| array, delegate, class, string | `object` |
| one-dimensional `T[]` | `Span<T>`, `ReadOnlySpan<T>` |
| `T` | `T?` |
| `null` literal | any nullable reference type, any `T?`, any pointer type |
| `Foo?` | `Foo` *only* where the compiler proves the value non-null (§13.3) |
| `void*` ↔ pointer types | pointer to `void*` implicit; reverse is explicit (unsafe) |
| constant expression of integer type | any integer type where the value fits (§5.9) |
| lambda / method group | compatible delegate type (§17) |
| user-defined `implicit operator` source type | its declared target type |

### 3.6.2 Explicit conversions

An explicit conversion `(T)e` is permitted for:

- any numeric conversion not listed as implicit (checked unless in an `unchecked` context, §5.7);
- `char` ↔ numeric types, integer ↔ enum, enum ↔ enum;
- a base class/interface to a derived class/interface (runtime-checked);
- `object` (or an interface) to a value type (**unboxing**, runtime-checked);
- nullable → non-nullable (`(Foo)x` where `x` is `Foo?` or `(MyStruct)v` where `v` is `MyStruct?`): throws `NullReferenceException` if the value is null (equivalent to `(Foo)x!`);
- pointer ↔ pointer, pointer ↔ integer (unsafe context only).
- a user-defined `explicit operator`, or an applicable user-defined `implicit operator` (§9.4).

**A cast that fails always throws an exception; it never produces null.** (The `as` operator is the non-throwing form, §5.12.)

```lumasharp
int32 i = 300;
uint8 b = (uint8)i;      // throws OverflowException (checked, default)
double d = i;            // implicit, 300.0
object o = i;            // boxing
int32 j = (int32)o;      // unboxing
string s = (string)o;    // throws InvalidCastException
Foo? maybe = Find();
Foo definite = (Foo)maybe;  // throws NullReferenceException if maybe is null
```

There are no other conversions. In particular there are no implicit conversions between `bool` and integers, from `int32` to `float`, from a signed type to an unsigned type, or from `int64` to any narrower or floating type.

## 3.7 Boxing and Unboxing

Converting a value type (primitive, struct, enum, or non-null nullable value) to `object` or to an interface it implements **boxes** it: a new heap object is allocated holding type information and a *copy* of the value. The box is an ordinary reference-counted object (§28.9).

```lumasharp
int32 x = 5;
object a = x;          // box: new heap object containing 5
object b = a;          // same box, reference count 2
x = 6;                 // a still holds 5
int32 y = (int32)a;    // unbox: y == 5
bool t = a is int32;   // true
bool u = a is int64;   // false
int32? n = a as int32; // n == 5
int64 z = (int64)a;    // throws InvalidCastException (box holds int32, not int64)
```

- Unboxing requires the exact boxed type. Unboxing `(int64)a` where the box holds `int32` throws `InvalidCastException`; it is not a numeric conversion. A boxed enum unboxes only to that enum type.
- Boxing a `T?` with no value yields `null`; boxing a `T?` with a value boxes the `T`.
- Calling a `mut` method through an interface on a boxed struct mutates the box, not the original variable.

## 3.8 Type Inference (`var`, `let`)

`var` and `let` infer the variable's type from its initializer. The initializer is mandatory. The inferred type is the static type of the initializer expression, including nullability:

```lumasharp
var a = 5;                 // int32
let b = "text";            // string
Foo? maybe = Find();
var c = maybe;             // Foo?
var d = null;              // error: cannot infer type from null
var e;                     // error: initializer required
var f = new();             // error: target-typed 'new' needs an explicit type
var g = { 1, 2, 3 };       // error: collection initializers need type info with var/let
var h = new int32[] { 1, 2, 3 };   // ok
```

## 3.9 Type Identity and Aliases

Two types are identical if they denote the same declaration with identical type arguments, the same array element type and rank (array types are **invariant**, §15.7), and the same nullability. `Foo` and `Foo?` are different types.

`using` aliases (§20.2) introduce alternative names for types.

---

# 4. Variables and Assignment

## 4.1 Variable Kinds

| Kind | Lifetime |
|---|---|
| **Local variable** | Enclosing block (§4.4) |
| **Parameter** | Method invocation |
| **Field (instance)** | Lifetime of the containing object or struct value |
| **Field (static)** | Lifetime of the program (§22.2) |
| **Array element** | Lifetime of the array |
| **Captured variable** | Lifetime of the longest-living closure (§17.4) |

## 4.2 Local Variable Declarations

```
local_declaration = ( type | "var" | "let" [ type ] ) identifier { "," identifier }
                    [ "=" initializer { "," initializer } ] ";"
                  | "const" type identifier { "," identifier } "=" initializer { "," initializer } ";"
```

### 4.2.1 Mutable locals

```lumasharp
int32 x = 10;
x = 20;                // valid
int32 y;               // y is 0: value types default-initialise (§4.3)
var z = 3.5;           // double, mutable (type inferred)
```

### 4.2.2 Immutable locals: `let`

A `let` local MUST be initialised in its declaration and cannot be assigned again. The type is inferred, or may be given explicitly (`let int32 a = 5;`):

```lumasharp
let a = 10;
a = 20;                // error: cannot assign to 'let' variable
let int64 b = 5;       // explicit type
let c;                 // error: initializer required
```

`let` makes the *binding* immutable. For value types (structs) it also forbids calling `mut` methods and assigning fields, because the value *is* the binding. For reference types it forbids rebinding only; the referenced object's own mutability is governed by `mut` (§8.5):

```lumasharp
let v = Vector();
v.x = 5;               // error: v is let (struct value is immutable)
v.Move(1, 2);          // error if Move is mut

let f = new Foo();
f.SetValue(3);         // valid if SetValue is mut: only the binding is immutable
f = new Foo();         // error
```

### 4.2.3 Constant locals: `const`

A local constant MUST be declared with an **explicit type** (`let` and `var` cannot declare constants) and MUST be assigned a **constant expression** (§5.9) in the same declaration:

```lumasharp
const int32 myLocal = 12;
const string name = "luma";
const var bad = 12;            // error: constants need an explicit type
const int32 other;             // error: constant must be initialised
const int32 computed = Foo();  // error: not a constant expression
```

Permitted types: `bool`, `char`, all integer types, `float`, `double`, enums, and `string` `[P-32]`. Constants cannot be reassigned, and may not have their address taken.

### 4.2.4 Multiple declarators

Several variables of one type may be declared in one statement. Unlike C#, the initializers follow a **single `=`** as a comma-separated list, matched to the identifiers **left to right**:

```lumasharp
int32 a, b = 10, 20;           // a == 10, b == 20
int32 a, b, c, d = 1, 2;       // a == 1, b == 2, c == 0, d == 0
```

Rules:

1. The number of initializers MUST be **less than or equal to** the number of identifiers (error LS0212).
2. Initializers are matched to identifiers in order. Each remaining identifier receives its type's **default** (§4.3): zero for value types and `null` for nullable types. A remaining identifier of a non-default-constructible type (including non-nullable references such as `string`) is an error (LS0810).
3. Initializers are evaluated **left to right**. An initializer MUST NOT refer to any variable declared by the same statement (error LS0201).
4. Initializer expressions in a multiple declaration cannot be assignment expressions at top level. The C# form `int32 a = 1, b = 2;` is therefore an error (LS0214: "use `int32 a, b = 1, 2;`").
5. With `var` or `let` and **no explicit type**, every identifier MUST have an initializer (equal counts) and all initializers MUST have the **same single type** (including nullability); otherwise error LS0213. To mix types, or when no single type exists, give the type explicitly with `let`.
6. `var` cannot be combined with an explicit type (a normal typed declaration is already mutable).

```lumasharp
var a, b = 1, 2;                 // both int32
let x, y = "p", "q";             // both string
var m, n = 1, 2.5;               // error LS0213: int32 and double differ
let int32 p, q, r = 4, 5, 6;     // explicit type with let; all must be initialised
let int64 s, t = 1, 2;           // 1 and 2 convert implicitly to int64
let int32 u, v = 4;              // error: let requires every identifier initialised
const int32 K1, K2 = 1, 2;       // const: every identifier needs an initializer
Foo? f, g = null;                // both null
Foo h, i = new Foo();            // error LS0810: 'i' has no initializer (and Foo has no default)
string s1 = "";
string s2 = "a";
```

The same declarator-list form is used for fields (§8.3.1) and for the initializer part of a `for` statement (§6.5).

## 4.3 Default Initialization and Initialization Rules

A variable declared **without** an initializer is initialised from the *default value* of its type (§3.5):

| Type of the declaration | Declared without an initializer |
|---|---|
| Value type, struct, enum, pointer | zero (default value); usable immediately |
| Nullable (`Foo?`, `T?`) | `null` |
| Any non-nullable reference type (`string`, class, interface, delegate, array, `object`) | **Compile-time error** (LS0810). It MUST be assigned in the declaration, or, for fields, in the field initializer or in every constructor |

Because there is no way to give a non-nullable reference a "no value" state, a declaration of a non-nullable reference **never** exists without a value:

```lumasharp
int32 n;                  // 0
Foo? maybe;               // null
string s;                 // error LS0810: string must be assigned
Vector v;                 // zeroed Vector

Foo foo;                  // error LS0810: Foo must be assigned in its declaration
Foo bar = new Foo();      // ok
Foo baz;                  // error even if assigned on every following path
baz = new Foo();
```

Variables of non-nullable reference type that are *initialised by the construct itself* are not declared without a value: pattern variables (`x is Foo f`), `catch` variables, `for`-in loop variables, lambda parameters, and `out` declaration expressions (`out Foo f`, `out var f`; the callee assigns it, §7.2.4).

**Fields and array elements** of non-nullable reference types, including `string`, must be initialised as described in §8.3 (constructors) and §15.2 (array creation); **struct fields** of any non-nullable reference type are not allowed (§9.1).

## 4.4 Scope and Name Hiding

- A local variable's scope is the block in which it is declared, from its declaration to the end of the block. Using it before its declaration is an error.
- A local variable or parameter MUST NOT have the same name as another local variable, parameter, or lambda parameter in an enclosing scope within the same method (error LS0201). Names may be reused in sibling scopes.
- A local may shadow a field or property of the containing type. Use `this.name` to reach the member.
- Loop labels (§6.7) live in a separate namespace and never conflict with variable names.

```lumasharp
void M(int32 p)
{
    int32 a = 1;
    { int32 a = 2; }    // error: 'a' already declared in enclosing scope
    { int32 b = 1; }
    { int32 b = 2; }    // valid: sibling scopes
}
```

## 4.5 Constants and `readonly` Fields

`const` and `readonly` are both available on fields; the **context decides the meaning** of each keyword. In a parameter list, `readonly` has a different meaning (§14.7).

### 4.5.1 `const` fields

On a field, `const` acts as an access-modifier-like keyword. The field is implicitly `static`, MUST have an explicit type from the permitted list (§4.2.3) and MUST be assigned a constant expression in its declaration:

```lumasharp
class Config
{
    public const int32 MaxItems = 100;
    public const string Name = "LumaSharp";
    const double Pi = 3.14159;
}
int32 n = Config.MaxItems;          // 100: substituted at compile time
```

### 4.5.2 `readonly` fields

A `readonly` field can be assigned only (a) in its declaration/initializer, or (b) in a **constructor** of the declaring type (for `static readonly`, in the static initializer/`static this()`). It does **not** need a constant value. `[P-31]`

```lumasharp
class Window
{
    public readonly int32 Id = NextId();          // runtime value, assigned once in initializer
    public readonly Config Settings;               // must be assigned in every constructor
    public static readonly Window Main = new Window(1);

    public this(int32 id)
    {
        Settings = new Config();                   // ok: constructor
    }

    void Rename() mut
    {
        Id = 5;                                    // error LS0231: readonly field outside constructor
    }
}
```

- A `readonly` field of non-nullable reference type obeys the normal initialisation rule: initialised inline or in **every** constructor.
- For a `readonly` field of struct type, the value is immutable: assigning its fields, or calling `mut` methods on it, outside a constructor is an error. For a reference-typed `readonly` field only the binding is read-only.
- An auto-property with only a `get` accessor behaves the same way: it may be assigned in its initializer or in constructors.
- `readonly` and `const` cannot be combined; `static const` is an error (`const` is implicitly static).

## 4.6 Assignment

### 4.6.1 Simple assignment

`target = value;` evaluates `value`, converts it implicitly to the target type, and stores it. The result of the assignment expression is the stored value.

If the target is a reference-type storage location, the previous referent (if any) loses one reference and the new referent gains one (§14.2). If the previous referent's count reaches zero, it is destroyed before the next statement begins.

**`null` can never be assigned to a non-nullable reference type**: `Foo f = new Foo(); f = null;` is error LS1301.

Valid targets: local variables (except `let`/`const`), parameters (except `in`), fields (subject to `readonly`), properties with a `set`, indexers with a `set`, array elements, dereferenced pointers, `ref`/`out` parameters.

### 4.6.2 Compound assignment

`x op= y` is equivalent to `x = (T)(x op y)` where `x` is evaluated once. Supported: `+= -= *= /= %= &= |= ^= <<= >>= ??=`.

```lumasharp
int32 a = 5;
a += 3;           // 8
a <<= 1;          // 16
Foo? f = null;
f ??= new Foo();  // f assigned only if it was null
```

### 4.6.3 Multiple assignment `[P-28]`

Several existing targets may be assigned from several values in one statement:

```
multi_assign = target "," target { "," target } "=" expr "," expr { "," expr } ";"
```

This is a **statement** distinguished from a multiple *declaration* (§4.2.4) by the absence of a leading type. The number of targets MUST equal the number of expressions (error LS0210). Evaluation is **simultaneous**, in this exact order:

1. Evaluate every right-hand expression **left to right**, storing each result in a temporary.
2. Evaluate the sub-expressions of every target (receiver expressions, index expressions) **left to right**.
3. Assign the temporaries to the targets **left to right**.

No target is modified until every right-hand expression has been evaluated.

```lumasharp
a, b = 1, 2;               // a == 1, b == 2
a, b = b, a;               // swap: a == 2, b == 1

int32 i = 0;
int32[] arr = new int32[3];
arr[i], i = 99, i + 1;     // RHS: 99 and 1 (old i == 0).
                           // targets: arr[0] (uses old i), then i
                           // result: arr[0] == 99, i == 1
```

Rules:

- Compound operators are not allowed in multiple assignment.
- The same variable appearing twice as a target is a compile-time error if statically identical (`a, a = 1, 2;`).
- Every target MUST satisfy the ordinary assignability, `mut`, `let` and `readonly` rules individually.
- It is not an expression and does not construct a tuple value: `x = (a, b = 1, 2)` is invalid. Tuple-like types, if provided by a library, have no special language syntax or semantics.
- If evaluating any right-hand side or target sub-expression throws, no assignment has occurred.

---

# 5. Expressions and Operators

## 5.1 Evaluation Order

Operands are evaluated **left to right**. Arguments are evaluated left to right in the order written at the call site (named arguments included). Short-circuit operators (`&&`, `||`, `??`, `?:`, `?.`) evaluate operands only as needed. Precedence and associativity: [Appendix C](#appendix-c-operator-precedence).

## 5.2 Primary Expressions

| Form | Meaning |
|---|---|
| `x` | Variable, parameter, field, or constant |
| `this` | Current instance (§8.1) |
| `base.M(...)`, `base[i]` | Access to base-class member |
| `e.Name`, `e?.Name` | Member access; null-conditional access (§13.5). On a pointer `p.Name` dereferences automatically (§19.3) |
| `e[i]`, `e[i, j]`, `e?[i]` | Element/indexer access |
| `e(args)` | Method or delegate invocation |
| `e!` | Null assertion (§13.4) |
| `e++`, `e--` | Post-increment/-decrement |
| `new T(args)`, `new(args)` | Heap allocation of class `T`; shorthand infers `T` (§5.14) |
| `scope T(args)`, `scope(args)` | Lexically scoped allocation (§14.4); shorthand infers `T` |
| `T(args)` | Struct construction (§9.2) |
| `new T[n]`, `scope T[n]`, `new T[] {…}`, `{…}`, `new [n]`, `scope [n]` | Array creation (§15.2) |
| `default`, `default(T)` | Default value (§3.5) |
| `sizeof(T)` | Size in bytes (unsafe, unmanaged `T`), type `int32` |
| `checked(e)`, `unchecked(e)` | Overflow-context expression (§5.7) |
| `(e)` | Parenthesised expression |
| lambda | §17 |

## 5.3 Arithmetic Operators

Binary: `+ - * / %`. Unary: `+ -`.

**Binary numeric promotion** (both operands are converted to a common type, following C#):

1. If either operand is `double` -> `double`.
2. Else if either is `float` -> `float`.
3. Else if either is `uint64`: the other operand must be unsigned or a non-negative constant (otherwise error LS0301); the result is `uint64`.
4. Else if either is `int64` -> `int64`.
5. Else if either is `uint32` and the other is signed -> `int64`; `uint32` with `uint32` or a smaller unsigned type -> `uint32`.
6. Else -> `int32`.

`int8`, `int16`, `uint8`, `uint16` and `char` operands are promoted to `int32` first, so the result of an arithmetic operation on them has the promoted type. Assign back to a narrow type with an explicit cast (or a compound assignment, which casts implicitly).

```lumasharp
int32 a = 7 / 2;          // 3 (truncates toward zero)
int32 b = -7 / 2;         // -3
int32 c = -7 % 2;         // -1 (sign of dividend)
double d = 7 / 2.0;       // 3.5
float  f = 1.5f * 2;      // 3.0 (float)
uint8 x = 200;
uint8 y = 100;
int32 s = x + y;          // 300 (promoted to int32)
uint8 t = (uint8)(x + y); // throws OverflowException (checked)
```

**Division by zero:** integer `/` and `%` by zero throw `DivideByZeroException` in all contexts. `int32.MinValue / -1` (and the equivalent for other signed types) throws `OverflowException` in all contexts. Floating-point division never throws.

`%` on floating-point types follows IEEE 754 `fmod` semantics (result has the sign of the dividend). `+` also concatenates strings (§16.5).

## 5.4 Bitwise and Shift Operators

`& | ^ ~` operate on integral types (and `&|^` on `bool`, non-short-circuiting). `<<` and `>>` shift by a count of type `int32`:

- operands narrower than 32 bits are first promoted to `int32`; the shift count is then masked to the operand width (`& 31` for 32-bit operands, `& 63` for 64-bit operands);
- `>>` is arithmetic for signed types and logical for unsigned types;
- shifts never throw.

```lumasharp
uint32 a = 0b1100;
uint32 b = a & 0b0110;    // 0b0100
int32 c = 1 << 33;        // 1 << (33 & 31) == 2
int32 d = -8 >> 1;        // -4 (arithmetic)
```

## 5.5 Relational and Equality Operators

`< > <= >=` apply to numeric types, `char`, enums (same enum type), and pointers. `==` and `!=` are defined in §5.6.

Floating-point comparisons follow IEEE 754: any comparison involving NaN is `false` except `!=`, which is `true`.

## 5.6 Equality: `==` and `!=`

`==` is an operator whose meaning depends on the operand types:

| Operand types | `a == b` means |
|---|---|
| numeric, `char`, `bool`, enum (same type) | value equality |
| class types (no user-defined `==`) | **reference identity** (same object) |
| class type with user-defined `static bool operator ==` | the user-defined operator |
| `string` | content equality (encoding-independent, §16.4) |
| `object` | reference identity (even when both are boxed values) |
| struct | the struct's user-defined `operator ==`; **error if none is defined** |
| nullable value type | lifted (§5.8) |
| pointers | address equality |
| delegates | same method **and** same target object (§17.1) |

**`!=` is never user-defined.** `a != b` is always evaluated as `!(a == b)` (with `a`, `b` each evaluated once, in order). Declaring `operator !=` is a compile-time error (LS0401).

```lumasharp
Foo a = new Foo();
Foo b = a;
Foo c = new Foo();
bool r1 = a == b;           // true  (same object)
bool r2 = a == c;           // false
bool r3 = a != c;           // true

struct Point { int32 x; int32 y; }
Point p = Point(), q = Point();
bool r4 = p == q;           // error: Point defines no operator ==

struct Money
{
    int64 cents;
    static bool operator ==(Money l, Money r) { return l.cents == r.cents; }
}
bool r5 = Money() != Money();   // legal: derived from ==, result false
```

Comparing operands of unrelated types is an error (`MyStruct == MyClass`, `Point == Vector`) unless one converts implicitly to the other.

## 5.7 `checked` and `unchecked`

Integer overflow behaviour is controlled by the **overflow context**:

- **Default context: checked.** Arithmetic that overflows throws `OverflowException`. This applies to `+ - * ++ -- unary-`, compound assignments, and explicit integer/float → integer conversions.
- **`checked`** (explicit): identical to the default. May appear anywhere (it does *not* require `unsafe`).
- **`unchecked`**: overflow *wraps* (two's complement). `unchecked` is only permitted inside an **`unsafe` context** (error LS0501). Within an unchecked context, array bounds checks MAY also be omitted (§15.5, §19.6).

Forms:

```lumasharp
checked { x = a + b; }                    // block form
int32 y = checked(a + b);                  // expression form

unsafe unchecked                           // block form: both keywords
{
    uint32 h = 2166136261;
    h = h * 16777619;                      // wraps silently
}

unsafe int32 Hash(int32 a, int32 b)
{
    return unchecked(a * 31 + b);          // expression form (inside unsafe member)
}

int32 bad = unchecked(a + b);              // error: unchecked requires an unsafe context
```

What `unchecked` changes: integer overflow wrapping, float → integer conversion of out-of-range values (result unspecified), and permission to omit array bounds checks. What it does **not** change: division by zero, `MinValue / -1`, null checks, cast checks, or any exception from library code.

Constant expressions that overflow at compile time are errors in a checked context (default) and wrap in an unchecked context.

## 5.8 Lifted Operators on Nullable Value Types

For nullable value types `T?` the predefined operators are *lifted*:

| Operator | Result |
|---|---|
| unary `+ - ~ ++ --`, binary `+ - * / % & \| ^ << >>` | `null` if any operand is `null`; otherwise `T?` wrapping the result |
| `<  >  <=  >=` | `false` if any operand is `null` |
| `==` | `true` if both `null`; `false` if exactly one is `null`; otherwise compare values |
| `!=` | `!(a == b)` |
| `&&`, `\|\|`, `!` on `bool?` | not defined in this version; extract with `!` or compare to `true` |

```lumasharp
int32? a = 5;
int32? b = null;
int32? c = a + b;      // null
bool   d = a == b;     // false
bool   e = a < b;      // false
bool   f = b == null;  // true
int32  g = a!;         // 5
int32  h = b!;         // throws NullReferenceException
```

## 5.9 Constant Expressions and Constant Conversion

A **constant expression** is composed of literals, `const` members, enum members, `sizeof` of primitive types, and operators over those. It is evaluated at compile time.

An integer constant converts implicitly to a narrower integer type if its value lies within the target's range:

```lumasharp
uint8 a = 200;        // ok
uint8 b = 300;        // error: 300 does not fit in uint8
int64 c = 5;          // widening, always fine
uint32 d = -1;        // error
```

## 5.10 Logical and Conditional Operators

`&&`, `||` short-circuit and require `bool` operands. `!x` is logical negation (prefix; not to be confused with postfix `x!`, §13.4). `c ? a : b` requires `c` to be `bool`; the type is the common type of `a` and `b` (if one is `null`, the other must be nullable-compatible, and the result is nullable).

```lumasharp
Foo? f = flag ? new Foo() : null;     // Foo?
int32 m = (a > b) ? a : b;
```

## 5.11 `is` Type Test

```
e is T            // bool
e is T name       // bool; binds 'name' of type T when true
e is not T        // bool
e is null         // bool
```

`e is T` is `true` when `e` is not null and its runtime type is `T` or derives from/implements `T`. For boxed values, `T` may be a value type (`o is int32`). If the static types make the result a compile-time constant, a warning is issued (LS3101).

A pattern variable introduced by `is` is definitely assigned, and non-null for a non-nullable `T`, inside the region where the test is known true:

```lumasharp
object o = 42;
if (o is int32 n) { Print(n + 1); }          // 43
if (o is not string) { Print("not a string"); }
Foo? f = Find();
if (f is Foo real) { real.Bar(); }           // real is Foo (non-null)
```

See §6.8 for the full pattern syntax.

## 5.12 `as` Conditional Cast `[P-7]`

`e as T` attempts the conversion and **never throws for a failed type test**. The result type is the **nullable** form of `T`:

| Situation | Result |
|---|---|
| `e` is `null` | `null` |
| `e`'s runtime type is `T` (or derives/implements) | `e` converted to `T?` |
| otherwise | `null` |

`T` may be a class, interface, delegate, array, `string`, or a value type (for unboxing). It MUST NOT be a pointer, nor a type parameter without a `class`/`struct` constraint.

The nullable result is avoided **only when the compiler can prove** that the conversion always succeeds *and* the operand is non-null (for example an upcast of a non-null value to a base class or implemented interface). In that case the expression's null-state is *non-null* (§13.3) and it converts implicitly to `T`.

```lumasharp
object o = 123;
int32? a = o as int32;       // 123
string? b = o as string;     // null
int32 c = o as int32;        // error: 'as' yields int32?, not provably non-null
int32 d = (o as int32)!;     // 123, or throws NullReferenceException
int32 e = (int32)o;          // 123, or throws InvalidCastException
Dog dog = new Dog();
Animal an = dog as Animal;   // ok: upcast of a non-null value is proven to succeed
Dog? back = an as Dog;       // nullable: not provable
```

## 5.13 Cast Expression

`(T)e` performs an explicit conversion (§3.6.2). **A cast that cannot succeed always throws an exception; it never yields null.** The compiler may skip the runtime check only when it can prove the conversion cannot fail.

| Failure | Exception |
|---|---|
| Runtime type does not match target | `InvalidCastException` |
| Source is `null` and the target is non-nullable | `NullReferenceException` |
| Numeric value out of range (checked context) | `OverflowException` |

Casting `null` to a nullable target yields `null`. Casting a non-null value whose runtime type does not match to a nullable target (`(Foo?)o`) throws `InvalidCastException`; use `as` for the non-throwing form.

## 5.14 Allocation Expressions and Target Typing `[P-29]`

- `new T(args)` allocates a class instance on the reference-counted heap. `new` is **only** for heap allocation (classes, arrays, boxes) and is never used with structs.
- `scope T(args)` allocates with lexically bounded lifetime (§14.4).
- A struct is constructed with `T(args)` (§9.2).

**Shorthand.** When the type can be inferred from the context, `T` may be omitted:

```lumasharp
MyClass a = new();                    // new MyClass()
MyClass b = new(1, 2);                // new MyClass(1, 2)
MyClass c = scope(someVal);           // scope MyClass(someVal)
MyClass?[] d = new [5];               // new MyClass?[5]
int32[] e = { 1, 2, 3 };              // new int32[] { 1, 2, 3 }
```

The **target type** is known in these contexts: a variable declaration or assignment with an explicit type, a field/property initializer, a `return` in a method with a declared return type, an element of a collection initializer, and an argument whose parameter type is the same for every applicable overload. If the type cannot be inferred (`var x = new();`, an overloaded call with different parameter types, a lambda return) the shorthand is error LS0802.

The target type of `new()`/`scope()` MUST be a class (or, for the array form, an array type); a struct target is error LS0801. The target type MUST NOT be abstract (§8.6.1).

## 5.15 Member Access on Nullable Types

Members of a nullable type **cannot** be accessed unless the compiler can prove the value is non-null at that point (§13.5). The null-assertion operator `e!` subverts the compile-time check and inserts a runtime check instead (§13.4).

---

# 6. Statements and Control Flow

## 6.1 Statement Overview

```
statement = block | local_declaration | multi_assign | expression_statement
          | if | switch | labeled_loop | while | do_while | for | for_in
          | break | continue | return | throw | try
          | unsafe_statement | checked_statement | unchecked_statement | ";" ;
```

An **expression statement** MUST be an invocation, assignment, increment/decrement, or `new`/`scope` allocation. A bare expression such as `a + b;` is an error (LS0601).

## 6.2 Blocks and Scopes

A block `{ ... }` introduces a lexical scope. This scope is the unit for local-variable lifetime (§4.4) and `scope` allocation lifetime (§14.4). Leaving a block (normally, or via `return`, `break`, `continue`, or an exception) triggers the scope-exit actions of §14.6.

## 6.3 `if`

```lumasharp
if (condition) statement
if (condition) statement else statement
```

`condition` MUST be of type `bool` (no integer or reference truthiness).

```lumasharp
if (x > 0)
{
    Print("positive");
}
else if (x < 0)
{
    Print("negative");
}
else
{
    Print("zero");
}
```

Flow analysis for nullability is performed through conditions (§13.3).

## 6.4 `while` and `do`

```lumasharp
int32 i = 0;
while (i < 3) { Print(i); i++; }          // 0 1 2

do { Print(i); i--; } while (i > 0);       // 3 2 1  (body runs at least once)
```

Both may carry a loop label (§6.7).

## 6.5 `for` (C-style)

```lumasharp
for (initializer; condition; iterator) body
```

- `initializer`: a local declaration (including multiple-declarator form, §4.2.4) or a comma-separated list of expression statements. Variables declared here are scoped to the loop and form **one** variable shared by all iterations.
- `condition`: a `bool` expression; omitted means `true`.
- `iterator`: a comma-separated list of expression statements.
- A `for` statement may carry a loop label (§6.7).

```lumasharp
for (int32 i = 0; i < 3; i++)
{
    Print(i);                      // 0 1 2
}

for (int32 a, b = 0, 10; a < b; a++, b--) { }

for (;;) { if (Done()) break; }    // infinite loop with exit
```

## 6.6 `for` … `in` (iteration)

LumaSharp has no `foreach` keyword. The `for` statement has a second form that iterates a collection; it is recognised by the `in` after the loop variable:

```lumasharp
for (let item in collection) body        // immutable loop variable (preferred)
for (var item in collection) body        // mutable loop variable
for (Type item in collection) body       // explicit type (mutable)
```

```lumasharp
for (let a in collection)
    DoSomething(a);
```

The loop variable is scoped to the body, and a **fresh variable** is created for each iteration (so closures capture a per-iteration variable). Loop variables of non-nullable reference type are initialised by the loop and never null.

**Iteration protocol.** `collection` is evaluated exactly once, MUST be of a non-nullable type, and MUST be one of:

1. an **array** of any supported rank: iterated in order. One-dimensional arrays iterate indices `0 .. Length-1`. Multi-dimensional arrays iterate **every element once in row-major order** (the last index varies fastest);
2. a type implementing `IEnumerable<T>`.

The compiler-known interfaces have exactly these members:

```lumasharp
interface IEnumerable<T>
{
    IIterator<T> Iterate();
}

interface IIterator<T>
{
    bool Next();             // advances; returns true if Current is now valid, false at the end
    T Current { get; }
}
```

`Next()` returns `bool`. Implementations declare `Next()` as `mut` (§8.5) because it changes the iterator's state; the interface itself does not (§10.1).

If a type implements `IEnumerable<T>` for more than one `T`, the loop is ambiguous (LS0640) unless an explicit loop-variable type selects exactly one.

**Lowering** (normative semantics; the compiler may generate any equivalent code, e.g. an index loop for arrays):

```lumasharp
// for (let x in c) body
{
    var it = c.Iterate();
    while (it.Next())
    {
        let x = it.Current;
        body
    }
}
```

`break` leaves the loop; `continue` proceeds to the next `Next()` call. Modifying the underlying collection during iteration has library-defined behaviour. An explicit loop-variable type MUST be implicitly convertible from `T`. The iterator object returned by `Iterate()` is an ordinary reference-counted object, released when the loop is left.

```lumasharp
int32[] values = { 10, 20, 30 };
for (let v in values) { Print(v); }          // 10 20 30
for (var v in values) { v += 1; }            // changes the copy only, not the array

int32[,] grid = { { 1, 2 }, { 3, 4 } };
for (let g in grid) { Print(g); }            // 1 2 3 4  (row-major)
```

## 6.7 Loop Labels, `break` and `continue`

Any loop statement (`while`, `do`, `for`, `for`-in) may be given a **prefix label**: an identifier followed by `:` immediately before the loop. A label may be referenced by `break` or `continue` to leave or continue a specific enclosing loop, which is useful for nested loops:

```
labeled_loop = identifier ":" ( while | do_while | for | for_in ) ;
break        = "break" [ identifier ] ";"
continue     = "continue" [ identifier ] ";"
```

```lumasharp
for (let a in values)
    for (let b in a)
        break;                    // exits the inner loop only

outer: for (let a in values)
    for (let b in a)
        break outer;              // exits the outer loop

rows: for (int32 y = 0; y < h; y++)
{
    for (int32 x = 0; x < w; x++)
    {
        if (Skip(x, y)) continue rows;   // next iteration of the OUTER loop
        if (Done(x, y)) break rows;      // leave the OUTER loop
    }
}
```

Rules:

- **Without a label**, `break` exits the innermost enclosing loop (or `switch`, see below) and `continue` starts the next cycle of the innermost enclosing loop.
- **With a label**, `break label;` exits and `continue label;` continues the enclosing loop carrying that label. The label MUST name a loop that encloses the statement (error LS0603 if the label is unknown, LS0604 if it labels something that is not an enclosing loop).
- A label is visible only within the loop it labels. Two enclosing loops MUST NOT use the same label name; sibling loops may. Labels do not conflict with variable names.
- Labels can only prefix loop statements; a label before any other statement is error LS0602.
- `break`/`continue` MUST be inside a loop (or, for unlabeled `break`, a `switch`); otherwise error LS0605. They cannot cross a lambda boundary or leave a `finally` block (error LS0610).
- **Inside a `switch`:** an unlabeled `break` exits the `switch` (it is never required, §6.8). `continue` ignores the `switch` and applies to the innermost enclosing loop. A **labeled** `break`/`continue` inside a `switch` section is valid only if the label names a loop that encloses the `switch`; a labeled `break` in a `switch` that is not inside any such loop is an error (LS0603).
- Leaving a loop by `break`, `continue`, `return` or an exception triggers the scope-exit actions of every block being left (§14.6).

```lumasharp
loop: while (true)
{
    switch (state)
    {
        case State.Done:
            break loop;           // exit the while loop from inside the switch
        case State.Skip:
            continue;             // next iteration of the while loop
        default:
            Step();
            break;                // leaves the switch only (redundant)
    }
}
```

## 6.8 `switch`

```
switch_statement = "switch" "(" expr ")" "{" { switch_section } "}"
switch_section   = switch_label statement { statement }
switch_label     = "case" pattern { "," pattern } [ "when" expr ] ":" | "default" ":"
```

`switch` works like C# with these differences: **there is no implicit fall-through and `break` is not required**, and several matching cases may share one section by listing them separated by commas. When the statements of a section finish, control leaves the `switch`.

```lumasharp
switch (val)
{
    case 1: DoSomething();              // does NOT fall into the next case
    case 2, 3, 4:
    {
        DoSomethingElse();
    }
    default: Error();
}
```

- A section's statement list MUST be non-empty (an empty section is error LS0620); to do nothing write `{ }`. A statement list that ends without an explicit `break` is fine.
- `break;` is permitted inside a section to leave the `switch` early (§6.7).
- `default` may appear at most once and in any position. If no label matches and there is no `default`, nothing executes.
- A `case` label with a comma list is *one* section: `case 1, 2:` shares a body, whereas two consecutive `case` lines are two separate sections (the first must therefore be non-empty).
- There is no mechanism to continue into another section (no `goto case`); share bodies with comma lists.

**Governing type** may be any type for which the patterns below are valid: integers, `char`, `bool`, `string`, enums, nullable types, and reference types (with type patterns).

**Case labels use patterns** `[P-18]`:

| Pattern | Example | Matches |
|---|---|---|
| Constant | `case 5:`, `case "x":`, `case Color.Red:`, `case null:` | value equals constant |
| Type | `case Dog:` | non-null value whose runtime type is `Dog` (or derives) |
| Declaration | `case Dog d:` | as above, and binds `d` of type `Dog` |
| Relational | `case > 10:`, `case <= 0:` | numeric comparison with a constant |
| Negated | `case not null:` | pattern does not match |
| Combined | `case > 0 and < 10:`, `case 1 or 2:` | `and` / `or` of patterns |
| Discard | `case _:` | anything (equivalent to `default`) |

A `when` guard adds a condition evaluated after the pattern matches:

```lumasharp
switch (shape)
{
    case Circle c when c.Radius > 10:
        Print("big circle");
    case Circle c:
        Print("circle");
    case Square:
        Print("square");
    case null:
        Print("nothing");
    default:
        Print("other");
}
```

Cases are tested **in textual order**; the first matching section runs. It is an error if a label can never match because an earlier label subsumes it (LS0622). Pattern variables are scoped to their section; a pattern variable of non-nullable reference type is non-null. For `string` constant cases, comparison is by content (§16.4).

The same patterns can be used with `is` (§5.11): `x is > 5 and < 10`, `x is not null`.

## 6.9 `return`

```lumasharp
return;            // in void methods
return expr;       // in non-void methods; expr implicitly converted to the return type
```

`return` triggers all scope-exit actions (§14.6) of the blocks being left, **after** the return value has been computed. A returned reference transfers ownership to the caller without redundant retain/release (§14.3).

A `scope`-allocated object, or any reference bound to one, MUST NOT be returned (error LS0701, §14.5).

## 6.10 `throw` and `try`

See §18.

## 6.11 `unsafe`, `checked`, `unchecked` statements

```lumasharp
unsafe { /* unsafe context */ }
checked { /* checked arithmetic (the default) */ }
unsafe unchecked { /* wrapping arithmetic, bounds checks may be omitted */ }
```

`unchecked` MUST be nested in, or combined with, `unsafe`. See §5.7 and Chapter 19.

## 6.12 Unreachable Code and Missing Returns

- A non-`void` method whose end is reachable is an error (LS0630: "not all code paths return a value").
- Code after an unconditional `return`, `throw`, `break`, or `continue` produces warning LS3001 *(required)*.

---

# 7. Methods and Parameters

## 7.1 Method Declaration

```
method = { modifier } return_type identifier [ type_params ] "(" [ parameters ] ")"
         [ "mut" ] [ constraints ] ( block | "=>" expr ";" | ";" ) ;
modifier = "public" | "private" | "protected" | "internal" | "static" | "virtual"
         | "override" | "extern" | "unsafe" ;   (* "protected internal" and "private protected" are
                                                    two-keyword combinations *)
```

```lumasharp
class Calculator
{
    public int32 Add(int32 a, int32 b)
    {
        return a + b;
    }

    public int32 Twice(int32 a) => a * 2;      // expression-bodied

    public static int32 Max(int32 a, int32 b) => a > b ? a : b;
}
```

- A method with return type `void` returns nothing.
- Methods may be overloaded: the **signature** is the name, type-parameter count, and parameter types/modifiers. The return type and `mut` are *not* part of the signature (two methods differing only in them are a duplicate-member error). `mut` is a **suffix** rather than a modifier precisely because it does not change a method's signature, only which receiver state the body may access (§8.5).
- Local functions (methods declared inside methods) are not part of this version.

## 7.2 Parameter Modifiers

```
parameter = { param_modifier } type identifier [ "=" constant ]
          | { param_modifier } type "..." identifier ;
param_modifier = "ref" | "in" | "out" | "readonly" ;
```

Parameter modifiers precede the type and may be written **in any order**: `readonly ref int32 a` and `ref readonly int32 a` are the same.

| Modifier | Meaning |
|---|---|
| *(none)* | Pass by value. A reference-type argument passes the reference (the callee may retain it). A value-type argument is copied. |
| `ref` | The callee operates directly on the caller's storage. The argument MUST be an initialised variable; the call site writes `ref`. |
| `in` | Read-only alias of the caller's storage: the callee cannot assign through it. Call site may write `in` (optional). |
| `out` | The callee writes to caller storage and MUST assign it before returning normally. The call site writes `out`. |
| `readonly` | The reference (or value) received **cannot escape the invocation** (§14.7). |

### 7.2.1 By-value parameters

```lumasharp
void Increment(int32 n) { n++; }      // modifies the local copy
int32 a = 1;
Increment(a);                          // a is still 1

void Rename(Person p)                  // p is a parameter, not receiver state
{
    p.SetName("Ann");                  // fine even though Rename is non-mut (SetName is mut on Person)
}
```

Parameters are mutable local variables inside the method (assigning to a by-value parameter is allowed; it affects only the local copy).

### 7.2.2 `ref`

```lumasharp
void Swap(ref int32 a, ref int32 b)
{
    a, b = b, a;
}

int32 x = 1, y = 2;
Swap(ref x, ref y);                    // x == 2, y == 1

void Replace(ref Foo target)
{
    target = new Foo();                // old referent loses one reference; new one gains one
}
```

For reference types, `ref Foo` is a reference **to the variable holding the reference**; assigning through it changes what the caller's variable refers to.

### 7.2.3 `in`

```lumasharp
double Length(in Vector v)            // v cannot be assigned or mutated through
{
    return Math.Sqrt(v.x * v.x + v.y * v.y);
}
Length(v);                             // 'in' is optional at the call site
Length(in v);
Length(Vector());                      // an rvalue is allowed: a temporary is created
```

Rules: no assignment to `v`, no assignment to its fields, no calling `mut` methods on it, no passing it as `ref`/`out`. The compiler may pass by address (no copy) or by value.

### 7.2.4 `out`

```lumasharp
bool TryParse(string s, out int32 result)
{
    if (s.Length == 0) { result = 0; return false; }
    result = 42;
    return true;
}

if (TryParse("42", out int32 value))       // out declaration
{
    Print(value);                           // 42
}
TryParse("7", out var other);               // out var: type inferred
```

Rules:

- The callee MUST NOT read an `out` parameter before assigning it (definite-assignment analysis), and MUST assign it on every path that returns normally (error LS0720).
- If the callee throws, the caller's variable is considered **unassigned** after the call (flow analysis) but keeps whatever value it had.
- **Ownership:** the caller's storage is treated as uninitialised for the callee. The first assignment inside the callee *replaces* any previous content; if the variable previously held a reference, that reference is released at the moment of that first assignment (§14.2). No retain is required merely because the variable is being passed.

### 7.2.5 `readonly`

`readonly` has no call-site syntax. It limits what the callee may do with the received value (§14.7):

```lumasharp
void Observe(readonly Foo foo)
{
    foo.Describe();                    // fine: use it
    stored = foo;                      // error: readonly parameter cannot escape
}
readonly ref int32 r                   // order of modifiers is free
ref readonly int32 r2                  // same meaning
```

### 7.2.6 Restrictions on `ref`, `in`, `out`

- `ref`/`in`/`out` parameters cannot be captured by lambdas (error LS0730).
- There are no `ref` locals, `ref` returns, or `ref` fields in this version. A `ref`/`in`/`out` parameter can only be used within the method.
- `ref`/`out` are not allowed on iterator/async methods (not in this version).
- `out` and `ref` arguments MUST be variables: locals, parameters, fields, array elements, or dereferenced pointers (not properties or `let` locals).

## 7.3 Optional and Named Arguments `[P-24]`

A parameter with a default value is **optional**. The default MUST be a compile-time constant, `default`, or `null` for nullable types. Optional parameters MUST come after all required parameters (a variadic parameter, if any, comes last).

```lumasharp
void Log(string message, int32 level = 1, bool timestamp = false) { }

Log("hi");                       // level == 1, timestamp == false
Log("hi", 2);                    // timestamp == false
Log("hi", timestamp: true);      // named: level == 1
Log(level: 3, message: "hi");    // named arguments may be reordered
Log(message: "hi", 2);           // error: positional argument after named
```

Rules:

- Positional arguments come first; once a named argument appears, all following arguments MUST be named (error LS0740).
- Every required parameter MUST be provided exactly once; no parameter may be supplied twice.
- Default values are inserted at the **call site** at compile time.
- Named arguments are evaluated in the order **written**, not parameter order.

## 7.4 Variable Arguments (`...`) `[P-27]`

The last parameter may be declared with `...` after its element type instead of C#'s `params`:

```lumasharp
int32 Sum(int32... values)
{
    int32 total = 0;
    for (let v in values) { total += v; }
    return total;
}

Sum();                          // 0   (empty sequence)
Sum(1);                         // 1
Sum(1, 2, 3, 4);                // 10
int32[] existing = new int32[] { 5, 6 };
Sum(existing);                  // 11  — passes the array directly, no copy
```

Rules:

- Inside the method, the parameter has the type `T[]` (an array) in this version. The language reserves the right to change the parameter's representation to a read-only span in a later version without changing call-site syntax; code SHOULD NOT mutate it.
- At the call site the compiler gathers the trailing arguments into a sequence. **Representation is implementation-defined** (a new array, a `scope` array, a stack array, or reuse of an existing one), provided the parameter behaves like an array for the duration of the call.
- **Normal form first:** if the call supplies exactly one argument implicitly convertible to `T[]`, it is passed as the array itself *without copying*. The callee therefore aliases the caller's array. Mutating it would affect the caller (a known limitation until read-only spans exist).
- The `...` parameter MUST be last, MUST NOT have a default value, and MUST NOT carry `ref`/`out`/`in`. It MAY carry `readonly`.
- The `...` parameter itself is non-null and its length may be zero.

```lumasharp
void Print(string format, object... args) { }
Print("x");                     // args is an empty array
Print("x", 1, "two", 3.0);      // args == { 1, "two", 3.0 } (boxed)
```

## 7.5 Overload Resolution

Overload resolution follows C#'s algorithm with these clarifications:

1. Collect accessible candidates with the invoked name and applicable type arguments.
2. A candidate is **applicable** if every argument converts implicitly to its parameter (honouring `ref`/`in`/`out`, named arguments, optional parameters, and variadic expansion).
3. Among applicable candidates choose the **best**: one that is no worse for every argument and better for at least one, where "better" prefers identity, then the more specific implicit conversion (e.g. `int32 → int64` better than `int32 → double`).
4. Ties are broken in this order: candidate needing no optional-parameter defaults; candidate not using variadic expansion (normal form preferred); non-generic over generic; more specific generic over less specific.
5. If no single best candidate remains, the call is ambiguous (error LS0750); if none is applicable, error LS0751.

`null`, lambdas, and method groups participate according to their conversion rules (§17).

## 7.6 Return Values

Methods return by value. A returned reference type transfers its ownership to the caller. A returned struct is a copy of the value. Methods cannot return `ref`/`in`.

## 7.7 `extern` Methods

`static extern` methods have no body and bind to a native symbol (§21).

## 7.8 Expression-Bodied Members

`=> expr;` may replace a block body for methods, properties (getter), indexers (getter), and operators. For `void` methods the expression MUST be a statement-valid expression.

## 7.9 Extension of the Receiver: `this`

Instance methods have an implicit parameter `this`. In class methods `this` is a reference of the enclosing class type (non-null). In struct methods `this` is an alias to the struct storage (like a `ref` parameter). Whether `this` may be assigned through is determined by `mut` (§8.5).

---

# 8. Classes

A class is a **reference type**. Every class instance is a reference-counted heap object (or a `scope` object, §14.4) with identity.

## 8.1 Class Declaration

```
class_decl = { modifier } "class" identifier [ type_params ] [ ":" base_list ] [ constraints ]
             "{" { member } "}" ;
base_list  = type { "," type } ;
member     = field | method | constructor | static_constructor | finalizer
           | property | indexer | operator | nested_type ;
modifier   = access | "virtual" | "static" | "unsafe" ;
access     = "public" | "private" | "protected" | "internal"
           | "protected" "internal" | "private" "protected" ;
```

```lumasharp
namespace Shapes;

public class Counter
{
    int32 count;                       // private field (default accessibility)

    public int32 Count => count;       // read-only property

    public void Increment() mut        // mutating method (§8.5)
    {
        count++;
    }
}
```

- At most **one** base class, plus any number of interfaces. The base class (if any) is written first in the base list.
- Every class implicitly derives from `object`.
- A class is **sealed by default**: it cannot be a base class. `virtual` before `class` makes it inheritable (§8.6). There are no `sealed` or `abstract` keywords.
- `static class` contains only static members, cannot be instantiated, and cannot be inherited.
- Nested types are permitted; a nested type has access to private members of its container.

## 8.2 Creating Instances

```lumasharp
Counter c = new Counter();         // heap allocation, reference count 1
Counter d = c;                     // same object, reference count 2
Counter e = new();                 // shorthand: type inferred from the declaration
var f = scope Counter();           // lexically scoped allocation (§14.4)
Counter g = scope();               // shorthand
```

`new T(args)` allocates storage, zero-initialises it, and runs the constructor chain (§8.3.2). `new` can only be applied to classes (and arrays and boxes). Using `new` on a struct is error LS0801; use `T(args)` (§9.2).

## 8.3 Fields and Constructors

### 8.3.1 Fields

```
field = { modifier | "readonly" | "const" } type identifier { "," identifier }
        [ "=" initializer { "," initializer } ] ";" ;
```

```lumasharp
class Person
{
    public string Name = "unnamed";    // field with initializer
    int32 age;                          // private, zero-initialised
    Person? friend;                     // nullable: null initially
    string nickname = "";
    static int32 instances;             // static field
    public readonly int32 Id;           // assigned in a constructor (§4.5.2)
    public const int32 MaxAge = 150;    // compile-time constant (§4.5.1)
    int32 width, height = 3, 4;         // multiple declarators (§4.2.4)
}
```

- Fields without an initializer receive their type's default: zero for value types and `null` for nullable types. A non-nullable reference field must be assigned in its declaration or by every constructor.
- **A field of non-nullable reference type, including `string`, MUST be initialised inline, or assigned in every constructor** (including through constructor chaining, §8.3.2); otherwise error LS0810. This is the same rule as for locals (§4.3) and applies equally to `readonly` fields and get-only auto-properties.

```lumasharp
class MyClass
{
    OtherClass field;                 // error LS0810 if any constructor does not assign it
    OtherClass other = new();         // ok: initialised inline
    OtherClass? maybe;                // ok: nullable
    public this()
    {
        field = new();
    }
}
```

- Static fields follow the same rule via their initializer or `static this()` (§22.2).
- A field initializer may not refer to `this` or other instance members, and may not contain `scope` allocation (LS0803).
- Field initializers of a class run in declaration order at the start of each of its constructors that does not delegate to another constructor of the same class (§8.3.2).

### 8.3.2 Constructors

A constructor is declared with the keyword **`this`** in place of the type name:

```lumasharp
class Rectangle
{
    int32 width;
    int32 height;

    public this(int32 w, int32 h)
    {
        width = w;
        height = h;
    }

    public this(int32 side) : this(side, side) { }   // constructor chaining

    public this() : this(1) { }
}
```

- `: this(args)` calls another constructor of the same class; `: base(args)` calls a base-class constructor. If neither is written, `: base()` is implied (error LS0811 if the base has no accessible parameterless constructor). The arguments of `: this(...)`/`: base(...)` cannot refer to `this`.
- **Execution order for `new T(args)`:**
  1. Storage is allocated and zeroed; the object's dynamic type is `T`.
  2. In the selected constructor of `T` (unless it delegates with `: this(...)`): the field initializers of `T` run in declaration order.
  3. The `: base(...)` (or `: this(...)`) constructor runs, recursively applying steps 2–4 to the base class.
  4. The constructor body runs.
- A class with no constructors declared receives an implicit `public this()` (protected if the class is `virtual`). If its fields cannot be initialised by that constructor, error LS0810 is reported.
- Constructors are implicitly `mut` (they may assign fields, including `readonly` ones).

**Using `this` inside a constructor `[P-12]`.** A reference to a non-nullable field that has not been assigned would break the null-safety guarantee, so a constructor body (after the base-constructor call) is restricted until every non-nullable reference field *declared by that class* that lacks an initializer is definitely assigned. In that restricted region the constructor MAY assign fields (`x = …`, `this.x = …`) and use expressions not involving `this`, but MUST NOT:

- read a field that is not yet definitely assigned;
- use `this` as a value (pass it, return it, store it, capture it in a lambda, convert it);
- call instance methods, properties, or indexers on `this`, or use `base.` members.

Violations are error LS0812. A constructor that delegates with `: this(...)` is never restricted (the target constructor assigned everything).

```lumasharp
class Holder
{
    Foo a;
    Foo b;
    public this()
    {
        Log(this);          // error LS0812: 'this' used before 'a' and 'b' are assigned
        a = new Foo();
        b = new Foo();
        Log(this);          // fine: all fields assigned
    }
}
```

**Dynamic type during construction.** While a class's constructor chain is running, the object behaves as if its type were the class whose constructor is *currently executing*: virtual calls, interface calls, `is` and `as` on the object under construction dispatch as for that class (as in C++), and **not** to overrides in more-derived classes whose fields are not yet initialised. When the base constructor returns, the object's dynamic type returns to the derived class. After the outermost constructor completes the object has its full dynamic type. This closes the only path by which derived code could observe an uninitialised non-nullable field.

```lumasharp
virtual class Base
{
    public this() { Describe(); }                  // runs Base's Describe, never Derived's
    public virtual void Describe() { Print("base"); }
}
class Derived : Base
{
    Foo foo = new Foo();
    public override void Describe() { foo.Use(); } // never runs during Base's constructor
}
var d = new Derived();                              // prints "base"
d.Describe();                                       // now runs Derived.Describe
```

**Failure.** If a constructor throws, the partially constructed object is destroyed as described in §8.4.1 and the exception propagates to the caller of `new`.

```lumasharp
var r1 = new Rectangle(3, 4);     // width 3, height 4
var r2 = new Rectangle(5);        // 5 x 5
Rectangle r3 = new();             // 1 x 1
```

### 8.3.3 Static constructor

`static this()` runs once, before first use of the type (§22.2). It takes no parameters, has no accessibility modifier, and cannot be called explicitly.

```lumasharp
class Table
{
    static int32[] primes = { 0 };
    static this()
    {
        primes = new int32[] { 2, 3, 5, 7 };
    }
}
```

## 8.4 Finalizers (Deterministic Destructors) `[P-26]`

A class may declare one finalizer, `~this()`, with no parameters, no modifiers, and no accessibility. It runs **exactly once, deterministically**, at the instant the object's reference count reaches zero (or a `scope` object leaves its scope).

```lumasharp
class FileHandle
{
    int32 fd;
    public this(int32 fd) { this.fd = fd; }

    ~this()
    {
        Close(fd);                     // runs when the last reference disappears
    }
}

void Test()
{
    var f = new FileHandle(3);
}                                       // f released → count 0 → ~FileHandle runs here
```

**Destruction sequence** for an object whose count reaches zero:

1. The object enters the *destroying* state. Its count can no longer cause a second destruction.
2. The finalizers run from the **most-derived class to the base class** (only classes that declare one).
3. The object's fields are released in **reverse order of declaration**, most-derived class's fields first, then each base class's.
4. The storage is freed.

Rules:

- Inside a finalizer, the implicit `this` behaves as a **`readonly` parameter** (§14.7): it cannot be stored in any field, static, array element, collection, closure, or passed to any parameter not marked `readonly`. Resurrection is therefore impossible in safe code.

```lumasharp
class Evil
{
    static Evil? saved;
    ~this()
    {
        saved = this;                  // error LS0701: 'this' cannot escape a finalizer
        Register(this);                // error unless Register(readonly Evil e)
        Log(this);                     // fine if Log(readonly Evil e)
    }
}
```

- A finalizer is implicitly `mut` (it may assign the object's own fields).
- A finalizer MUST NOT throw. An exception that escapes a finalizer is a **fatal error** (§26.5).
- Structs, interfaces, and enums cannot have finalizers.
- There is no `Dispose` mechanism; deterministic finalization replaces it.

### 8.4.1 Destruction after a failed constructor `[P-12]`

If a constructor chain throws, the object is destroyed without ever being visible to the program. The finalizer of a class runs **if and only if that class's own constructor completed**. Constructors complete base-first, so after a failure the finalizers of the completed base classes run (most-derived completed class first), followed by the field releases (§8.4 step 3; fields never assigned are default/null and are skipped) and the free. While those finalizers run, the object's dynamic type is the class whose finalizer is running.

## 8.5 `mut`: Mutating Instance Methods

**Every non-static instance method is non-mutating by default.** A non-mutating method may *read* the receiver's state but may not modify it, directly or indirectly. A method that may modify the receiver is declared with the `mut` keyword **after the parameter list**:

```lumasharp
class Foo
{
    int32 value;

    public int32 GetValue()            // non-mutating
    {
        return value;
    }

    public void SetValue(int32 x) mut  // mutating
    {
        value = x;
    }

    public void Broken()
    {
        value = 10;                    // error LS0820: assignment to receiver state in non-mut method
    }
}
```

`mut` is a suffix rather than a modifier because it does **not** change a method's signature (it plays no part in overloading, overriding signatures or interface contracts); it only governs which of the receiver's members the body may touch. Its purpose is to ensure that instance fields are not modified accidentally.

### 8.5.1 What counts as modifying the receiver

Receiver state is every storage location reachable from `this` **by a member-access path rooted at `this`** (`this.x`, `x`, `this.bar.y`, `bar.items[0]`, …). In a non-`mut` method (or a property/indexer `get` accessor), the compiler MUST reject:

1. assignment, compound assignment, `++`/`--`, or `ref`/`out` passing of any `this`-rooted path;
2. calling a `mut` method on `this` or on any `this`-rooted path (as declared on the **statically resolved** method);
3. invoking a `set` accessor or `set` indexer on `this`-rooted paths;
4. creating a method-group delegate of a `mut` method on a `this`-rooted receiver.

```lumasharp
class Bar { public int32 n; public void Bump() mut { n++; } }

class Foo
{
    int32 x;
    Bar bar = new Bar();

    void A()
    {
        B();                           // error LS0821: B is mut, A is not
    }
    void B() mut { x = 10; }

    void C()
    {
        int32 y = 10;                  // fine: local variable, not receiver state
        y = 20;                        // fine
        bar.Bump();                    // error: Bump is mut and 'bar' is this-rooted
        bar.n = 5;                     // error: assignment through this-rooted path
        var other = new Bar();
        other.Bump();                  // fine: 'other' is not rooted at this
    }

    void D() mut
    {
        B();                           // fine: D is mut
        bar.Bump();                    // fine
    }
}
```

`mut` does **not** make the whole method immutable: locals, parameters, and objects not rooted at `this` can be freely mutated.

### 8.5.2 Limits of the check

The rule is *path-based* and *static*, not a full const-correctness system.

- It does not track aliases or references that have left the receiver (for example an object retrieved through a getter and then mutated by the caller).
- **Interface calls are opaque.** Interface member declarations carry no `mut` (§10.1), so a call through an interface-typed receiver is never classified as mutating by this check, even when the implementation is `mut`. The same holds for delegate invocations.
- It guarantees only that a non-`mut` method body does not itself modify receiver-rooted state through `this` paths that the compiler can classify.

### 8.5.3 Other places `mut` applies

- Struct methods (§9.4) and property `set` accessors (implied `mut`, §8.8).
- **Implementing an interface member:** the implementing method declares `mut` if it needs to (the interface cannot say). Either form implements the interface member.
- **Overriding a class `virtual` member:** an override MAY add or omit `mut` regardless of whether the base member is `mut`, because `mut` is not part of the signature or override contract. Each method body is checked according to its own `mut` suffix; receiver-state checks at a call site use the statically resolved member declaration.
- Static methods, constructors (always allowed to assign fields), and finalizers (§8.4) have no `mut` suffix; writing `mut` on them is an error.

## 8.6 Inheritance, `virtual`, and `override`

```lumasharp
virtual class Animal                       // open for inheritance
{
    public virtual string Sound() => "...";    // may be overridden
    public string Kind() => "animal";          // not overridable
}

class Dog : Animal                         // sealed by default (no 'virtual')
{
    public override string Sound() => "woof";
}
```

| Keyword | On a class | On a method/property/indexer |
|---|---|---|
| `virtual` | The class may have derived classes | The member participates in dynamic dispatch and may be overridden |
| `override` | — | Replaces an inherited virtual member |

Rules:

1. A class can derive only from a `virtual` class (or `object`). Deriving from a non-`virtual` class is error LS0830.
2. `virtual` members are only valid in `virtual` classes. A `virtual` member in a non-virtual class is error LS0831.
3. `override` MUST match an inherited `virtual` (or `override`) member with an identical signature, return type, and accessibility, else error LS0832. Overriding is never implicit.
4. A member that has the same signature as an inherited **virtual** member but is declared without `override` is error LS0833. Hiding an inherited **non-virtual** member is warning LS3002; there is no `new` modifier for hiding.
5. An `override` member is itself virtual if its class is `virtual`, so a further-derived class may override it again.
6. Calling the base implementation: `base.Method(...)`.
7. Static members and constructors are never virtual.
8. Generic methods may not be `virtual`/`override` (§12.6).

```lumasharp
Animal a = new Dog();
Print(a.Sound());          // "woof"  — dynamic dispatch
Print(a.Kind());           // "animal"
```

### 8.6.1 Abstract methods and classes

There is no `abstract` keyword. A **virtual method without a body** is *abstract*:

```lumasharp
virtual class Shape
{
    public virtual double Area();                 // no body → abstract
    public virtual string Name() => "shape";       // has body → ordinary virtual
}

class Circle : Shape
{
    double r;
    public this(double r) { this.r = r; }
    public override double Area() => 3.14159 * r * r;   // implements the abstract method
}
```

Rules:

- A class that declares or inherits an unimplemented virtual method is **abstract**.
- A class declaring a body-less `virtual` method MUST be marked `virtual` (otherwise it could never be instantiated or derived).
- An abstract virtual method may be declared with or without `mut`. An override may independently add or omit `mut`; each implementation body is checked according to its own suffix (§8.5.3).
- An abstract class cannot be instantiated: `new Shape()` is error LS0834. It MAY be instantiated only through a derived class that implements every abstract method.
- A non-`virtual` class that does not implement every inherited abstract method is error LS0835.
- A `virtual` class with no abstract methods is simply an inheritable class.
- `base.M()` where `M` is abstract is error LS0836.
- A method declaration without a body is valid **only** if it is `virtual` (abstract), `extern`, or an interface member.

### 8.6.2 Constructors in derived classes

```lumasharp
virtual class Base { public this(int32 x) { } }
class Derived : Base
{
    public this() : base(5) { }        // required: Base has no parameterless constructor
}
```


## 8.7 Accessibility Modifiers

All C# visibility modifiers are supported:

| Modifier | Accessible from |
|---|---|
| `public` | Everywhere |
| `internal` | The same project/compilation (assembly) |
| `protected` | The declaring class and derived classes |
| `private` | The declaring type (and its nested types) only |
| `protected internal` | Derived classes **or** the same compilation |
| `private protected` | Derived classes **within** the same compilation |

**Defaults:** a member declared without a modifier is **`private`**. A top-level type declared without a modifier is `internal`. Interface and enum members are always public and take no modifier. The implicit default constructor is `public` (`protected` for a `virtual` class).

A type's accessibility MUST be at least as great as the accessibility of every signature in which it appears (inconsistent-accessibility error LS0840).

## 8.8 Properties `[P-18]`

A property is a member that looks like a field but runs accessor code.

```
property = { modifier } type identifier "{" accessors "}" [ "=" initializer ";" ]
         | { modifier } type identifier "=>" expr ";" ;
accessors = ( "get" accessor_body | "set" accessor_body )  — each at most once, either order
accessor_body = ";" | "=>" expr ";" | block ;
```

```lumasharp
class Temperature
{
    // auto-implemented: compiler supplies hidden backing storage
    public double Celsius { get; set; }

    // expression-bodied read-only property
    public double Fahrenheit => Celsius * 9.0 / 5.0 + 32.0;

    // explicit accessors using 'field' (the backing store) and 'value' (the assigned value)
    int32 steps;
    public int32 Steps
    {
        get => field;
        set
        {
            if (value < 0) { throw new Exception("negative"); }
            field = value;
        }
    }

    // property with initializer
    public string Label { get; set; } = "none";
}
```

- `get` is a non-mutating accessor: it is checked like a non-`mut` method (§8.5).
- **`set` is implicitly `mut`**: it may modify receiver state, and assigning to the property is itself a mutation of the receiver (so it is rejected in a non-`mut` method if the target is `this`-rooted).
- `field` (contextual keyword) names the compiler-generated backing store and is only available inside accessors of that property. `value` is the implicit parameter of `set`.
- An auto-property of non-nullable reference type MUST have an initializer or be definitely assigned in every constructor.
- A property with only `get` and no backing use is computed. A `set`-only property is permitted but unusual.
- Properties can be `static`, `virtual`, `override`, or interface members. Accessors may have a lower accessibility than the property (`public int32 X { get; private set; }`).
- Properties cannot be passed as `ref`/`out`.


## 8.9 Indexers

```lumasharp
class Grid
{
    int32[] cells = new int32[100];

    public int32 this[int32 row, int32 col]
    {
        get => cells[row * 10 + col];
        set => cells[row * 10 + col] = value;       // set is implicitly mut
    }
}

var g = new Grid();
g[2, 3] = 7;
Print(g[2, 3]);                                      // 7
```

Indexers follow property rules, may have multiple parameters, may be overloaded by parameter types, and may be `virtual`/`override` or interface members. They are declared with the keyword `this` followed by `[`.


## 8.10 Operator Overloading `[P-18]`

Operators are **static methods** with a reserved name, recognised by the compiler:

```
operator = { modifier } "static" type "operator" op "(" parameters ")" ( block | "=>" expr ";" ) ;
```

Overloadable unary: `+ - ! ~ ++ --`. Overloadable binary: `+ - * / % & | ^ << >> == < > <= >=`.

```lumasharp
struct Vector2
{
    public double x;
    public double y;

    public this(double x, double y) { this.x = x; this.y = y; }

    public static Vector2 operator +(Vector2 a, Vector2 b) => Vector2(a.x + b.x, a.y + b.y);
    public static Vector2 operator -(Vector2 v) => Vector2(-v.x, -v.y);
    public static bool operator ==(Vector2 a, Vector2 b) => a.x == b.x && a.y == b.y;
}

let sum = Vector2(1.0, 2.0) + Vector2(3.0, 4.0);   // (4.0, 6.0)
let same = Vector2(1.0, 0.0) != Vector2(1.0, 0.0); // false (derived from ==)
```

Rules:

- For non-conversion operators, at least one parameter (or the return type for unary `++`/`--`) MUST be the declaring type.
- Operator methods MUST be `public static`.
- **`!=` cannot be declared** (error LS0401); `a != b` is `!(a == b)`.
- Compound assignment operators (`+=`, …) are derived from the binary operator.
- `++`/`--` operators take and return the declaring type.
- A conversion operator has exactly one by-value parameter, is `public static`, and either its source or target type MUST be the declaring class or struct.
- `implicit operator T(Source value)` participates in implicit conversions and explicit casts. `explicit operator T(Source value)` participates only in explicit casts.
- A conversion operator is selected when its declared source and target exactly match the conversion's source and target types; this version does not chain user-defined conversions with other conversions.
- `&&`, `||`, `??`, `?:`, `=`, `.`, `is`, `as`, `new`, and `!` (postfix) cannot be overloaded.


## 8.11 `object`, `ToString`, Equality and Hashing

`object` has exactly one member: `virtual string ToString()`. It has **no** `Equals`, `GetHashCode`, or `GetType`. Equality of references and structs is provided by `==` (§5.6).

```lumasharp
class Point
{
    int32 x, y;
    public this(int32 x, int32 y) { this.x = x; this.y = y; }
    public override string ToString() => "(" + x + ", " + y + ")";
}
```

The default `ToString()` returns the fully qualified type name. Structs inherit `ToString` implicitly and may override it.

**Hashing and equality via interfaces.** Hashing and equality checks beyond `==` are expressed by library interfaces, which collections such as `Dictionary<K, V>` enforce through generic constraints. They are ordinary library types, not compiler-known (§1.7):

```lumasharp
class CustomHash : IHashable
{
    // Implement interface
    public int32 HashCode => 123;
}

class CustomEquals : IEquality<CustomEquals>
{
    // Implement interface
    public bool Equals(CustomEquals other) => false;
}

class Dictionary<K, V> where K : IHashable, IEquality<K> { /* library */ }
```

---

# 9. Structs

A struct is a **value type**: stored inline in its containing storage, copied on assignment, with no identity and no reference count of its own.

## 9.1 Declaration

```lumasharp
struct Vector
{
    public int32 x;
    public int32 y;

    public void Move(int32 dx, int32 dy) mut
    {
        x += dx;
        y += dy;
    }

    public int32 LengthSquared() => x * x + y * y;     // non-mutating
}
```

- A struct MAY implement interfaces and MAY declare the special struct base type `System.ValueType`. This marker does not add fields or inheritance to the unboxed struct. When a value is boxed, the compiler makes its box an `object` implicitly (§3.7). A struct MAY NOT derive from a class, be `virtual`, or have finalizers.
- A struct MAY declare constructors, methods, properties, indexers, operators, static members, `const` and `readonly` fields, and nested types.
- A struct MUST NOT contain itself, directly or through other structs (error LS0900); indirection through a nullable reference field is allowed.
- A struct MUST NOT declare a parameterless instance constructor `[P-6]`: `S()` always yields the default value (§9.2).

### 9.1.1 Reference fields in structs

**A struct cannot hold a field of any non-nullable reference type, including `string`.** Every struct has a default value (all bits zero, §3.5), and a zeroed reference would break the non-null contract. A struct can hold a reference only through a **nullable** type. Use `string?` for a string field in a struct.

```lumasharp
struct Good
{
    public int32 id;
    public Foo? owner;          // ok: nullable reference
    public string? label;       // ok: nullable reference, default is null
    public Vector position;     // ok: nested struct
}

struct Bad
{
    public Foo owner;           // error LS0902: struct fields of reference type must be nullable
    public string label;        // error LS0902: use string?
    public int32[] data;        // error LS0902: use int32[]?
    public Action callback;     // error LS0902: use Action?
}
```

Reading a nullable reference field is subject to the usual nullability rules (§13): `good.owner.Use()` is a compile-time error until the compiler can prove non-null.

## 9.2 Construction

Structs are created **without `new`**, using the type name as a call. This keeps `new` meaning "heap allocation":

```lumasharp
Vector a = Vector();              // default value: x == 0, y == 0
Vector b = Vector(3, 4);          // calls a declared constructor this(int32, int32)
Vector c = default;               // same as Vector()
Vector d;                         // implicitly zeroed
Vector e = new Vector();          // error LS0801: 'new' is for heap allocation
```

`S()` with no arguments always yields the default value (all fields zero/null). A declared struct constructor starts from the default value and then runs its body, so fields it does not assign keep their defaults:

```lumasharp
struct Person
{
    public string? Name;          // default null
    public int32 Age;
    public Person? Parent;        // default null
    public this(string name, int32 age) { Name = name; Age = age; }
}
Person p = Person("Ann", 30);     // Parent == null
Person q = Person();              // Name == null, Age == 0, Parent == null
```

## 9.3 Value Semantics

- Assignment, passing by value, and returning by value **copy** the struct.
- Nullable reference fields copy the reference (and retain the referent, §9.5).

```lumasharp
Vector a = Vector(1, 2);
Vector b = a;                     // independent copy
b.x = 10;
Print(a.x);                       // 1   (a is unchanged)
Print(b.x);                       // 10
```

## 9.4 `mut` on Struct Methods

Struct instance methods follow §8.5. In a struct, `this` is a `ref`-like alias of the storage.

- Non-`mut` methods receive `this` as a read-only alias (`in`); no defensive copy is made.
- `mut` methods receive `this` by reference and may modify the value in place.
- Calling a `mut` method requires a mutable *variable*: it is an error on a `let` local, an `in` parameter, a `readonly` field, a read-only property result, or an rvalue/temporary:

```lumasharp
Vector v = Vector(1, 1);
v.Move(2, 2);                     // ok: v is a mutable variable
let w = Vector(1, 1);
w.Move(2, 2);                     // error LS0823: 'w' is immutable
Vector Make() => Vector(1, 1);
Make().Move(1, 1);                // error: cannot call mut method on a temporary
```

## 9.5 Structs and Reference Counting

A struct that contains nullable reference fields owns (retains) those references; non-nullable reference fields, including `string`, are prohibited by §9.1.1. Copying such a struct retains each referent; destroying a copy (end of scope, overwrite) releases them. This is performed entirely by the compiler. A struct containing no references requires no reference counting.

## 9.6 Equality

Structs have **no** automatic `==`/`!=` (matching C#). `a == b` on structs is an error unless the struct declares `static bool operator ==`. There is no implicit field-by-field comparison.

## 9.7 Layout

Struct layout is specified in §28.2: fields are laid out sequentially in declaration order using the platform's C alignment rules when every field is of an unmanaged type.

---

# 10. Interfaces

An interface declares a contract that classes and structs implement. Interfaces in this version contain no code.

## 10.1 Declaration

```lumasharp
interface IShape
{
    float Area();                         // method
    void Scale(float factor);             // method (implementations may be 'mut')
    string Name { get; }                  // property
    float this[int32 i] { get; }          // indexer
}
```

Rules:

- Members are methods, properties, and indexers. No fields, constructors, operators, nested types, or static members.
- Members have **no body, no accessibility modifier, and no `virtual`/`override`**. All members are implicitly public.
- **Interface methods are never declared `mut`** (writing it is error LS0824), even when a member is clearly designed to change state (e.g. `IIterator<T>.Next()`). The implementing method may declare `mut` if it needs to (§8.5.3). `mut` does not change a method's signature, only the receiver-member access permitted in that method body, so it is not part of the interface contract.
- **No default implementations.**
- Interfaces can be generic (§12) and may carry constraints.

## 10.2 Interface Inheritance

```lumasharp
interface IReadable { string Read(); }
interface IWritable { void Write(string s); }
interface IStream : IReadable, IWritable { }     // inherits both, same as C#
```

A type implementing `IStream` must implement all inherited members.

## 10.3 Implementing Interfaces

```lumasharp
class Square : IShape
{
    float side;
    public this(float s) { side = s; }

    public float Area() => side * side;
    public void Scale(float factor) mut { side *= factor; }   // mut declared here, on the implementation
    public string Name => "square";
    public float this[int32 i] => side;
}
```

- Implementing members MUST be `public` and match the interface member's signature (name, parameters, return type). Whether they are `mut` is free.
- A class or struct that does not implement every interface member is error LS1001 (unless it is a `virtual` class that leaves the remaining members as body-less `virtual` members, §8.6.1).
- Interface implementations are non-virtual unless the implementing member is declared `virtual` in a `virtual` class.

### 10.3.1 Explicit interface implementation

Members are implemented explicitly by qualifying with the interface name. Explicit implementations have no accessibility modifier and are only reachable through the interface type:

```lumasharp
class Both : IReadable, IOther
{
    string IReadable.Read() => "readable";
    string IOther.Read() => "other";      // resolves the name clash
}

Both b = new Both();
b.Read();                                 // error: Read is not accessible on Both
IReadable r = b;
Print(r.Read());                          // "readable"
IOther o = b;
Print(o.Read());                          // "other"
```

### 10.3.2 Structs and interfaces

A struct may implement interfaces. Converting a struct to an interface type boxes it (§3.7). Constrained generic calls (`T : IShape` with `T` a struct) do not box (§12.4).

---

# 11. Enums

## 11.1 Declaration

```lumasharp
enum Color { Red, Green, Blue }                   // underlying int32; Red=0, Green=1, Blue=2
enum Flags : uint8 { None = 0, A = 1, B = 2, C = 4 }
enum Status { Ok = 200, NotFound = 404, Error }   // Error == 405 (previous + 1)
```

- The underlying type defaults to **`int32`** and may be any built-in integer type (`: uint8`, `: int64`, …). `char`, `bool`, `float`, `double` are not allowed.
- Members take consecutive values from the previous member (starting at 0) unless assigned an explicit constant expression.
- Enums are value types with the default value `0` (even if no member has that value).
- Enums cannot declare methods, properties, or interfaces; they are pure constant sets.

## 11.2 Conversions

| Conversion | Kind |
|---|---|
| enum → underlying integer type (and anything that converts from it) | **implicit** |
| integer → enum | **explicit** only |
| enum → different enum | explicit only |

```lumasharp
Color c = Color.Green;
int32 n = c;                    // implicit: 1
Color d = (Color)2;             // explicit: Color.Blue
Color e = 2;                    // error: integer to enum requires a cast
Color f = (Color)99;            // legal: values outside the declared set are allowed
bool g = (Color)99 == Color.Red;// false
```

## 11.3 Operators

Enums support `== != < > <= >=`, `& | ^ ~`, `+` and `-` with an integer (`Color.Red + 1` has type `Color`), and `++`/`--`. The result of arithmetic is checked against the underlying type like any integer arithmetic.

```lumasharp
Flags f = Flags.A | Flags.C;             // 5
bool hasC = (f & Flags.C) != Flags.None; // true
```

## 11.4 Enums in `switch`

```lumasharp
switch (c)
{
    case Color.Red:   Print("r");
    case Color.Green, Color.Blue: Print("g or b");
}
```

---

# 12. Generics

Generics provide type-parametrised classes, structs, interfaces, delegates, and methods. The **language requirement** is the source-level behaviour; the compiler may implement it by monomorphization, dictionaries, type erasure, specialization, or a combination. No behaviour in this document depends on the choice.

## 12.1 Declaring Generic Types

```lumasharp
class Box<T>
{
    T value;
    public this(T value) { this.value = value; }
    public T Get() => value;
    public void Set(T v) mut { value = v; }
}

struct Pair<TFirst, TSecond> { public TFirst First; public TSecond Second; }
interface IProducer<T> { T Produce() mut; }
delegate bool Predicate<T>(T item);
```

```lumasharp
Box<int32> a = new Box<int32>(5);
Box<string> b = new Box<string>("hi");
Dictionary<string, int32> d = new Dictionary<string, int32>();
Box<Box<int32>> nested = new Box<Box<int32>>(a);
```

- Each distinct set of type arguments produces a distinct type. `Box<int32>` and `Box<double>` are unrelated types with no common base besides `object`.
- Static members and static constructors are per-instantiation.
- Type parameters are in scope throughout the declaration.

## 12.2 Generic Methods

```lumasharp
static T Max<T>(T a, T b) where T : IComparable<T>
{
    return a.CompareTo(b) > 0 ? a : b;     // uses only members promised by the constraint
}

int32 m = Max<int32>(3, 7);                // explicit type argument
int32 n = Max(3, 7);                       // inferred: T = int32
```

**Type inference:** type arguments are inferred from the call's argument types using C#'s algorithm (exact/lower-bound inference per argument). Inference does not use return types or constraints. If inference fails, the type arguments must be given explicitly (error LS1101).

## 12.3 Constraints

```
constraints = { "where" type_param ":" constraint { "," constraint } } ;
constraint  = "class" | "struct" | "enum" | "default" | type | "new" "(" [ type { "," type } ] ")" ;
```

| Constraint | Requires type argument to be |
|---|---|
| `class` | a non-nullable reference type |
| `struct` | a non-nullable value type (`T?` is not permitted) |
| `enum` | an enum type |
| `default` | a type with a default value under §3.5: value types and nullable types, including nullable reference types; non-nullable reference types are rejected |
| a base class `B` | `B` or derived from `B` (`B` must be a `virtual` class) |
| an interface `I` | a type implementing `I` |
| another type parameter `U` | a type that is or derives from `U` |
| `new()` | a class with an accessible parameterless constructor, or any struct |
| `new(int32, string)` | a type with an accessible constructor taking exactly these parameter types (LumaSharp extension) |

Ordering within one `where` clause: a primary constraint (`class`/`struct`/`enum`/`default`/base class) first, then interfaces, then `new(...)` last. `default` is mutually exclusive with `class`, `struct`, `enum`, and a base-class primary constraint. There is no `unmanaged` constraint (struct semantics already provide it).

```lumasharp
class Cache<T> where T : class, IEquatable<T>, new() { }

class Factory<T> where T : new(int32)
{
    public T Make(int32 n) => new T(n);     // requires public this(int32 n) on T
}

class Numeric<T> where T : struct { }
Numeric<int32> ok;
Numeric<string> bad;                         // error LS1102: 'string' does not satisfy 'struct'

class InitializedBuffer<T> where T : default
{
    T[] items = new T[16];                    // every element receives a valid default(T)
    public T Empty() => default(T);
}
InitializedBuffer<int32> numbers;             // ok
InitializedBuffer<MyClass?> references;       // ok: each element starts as null
InitializedBuffer<MyClass> invalid;            // error LS1102: MyClass has no default value
```

Constraints are checked at every instantiation. Inside a generic body, only members promised by the constraints (and `object`'s) may be used on a value of type `T`.

## 12.4 Behaviour of `T` Inside Generic Code

- `T` with a `struct` constraint is a value type; with `class` is a non-nullable reference type; **an unconstrained `T` may be either**, so inside the generic body it is treated as a possible non-nullable reference type.
- `default(T)` and target-typed `default` in a `T` context require `T : default`, `T : struct`, or `T : enum`. The `default` constraint accepts nullable reference types, so `default(T)` may be null only when the argument type itself is nullable. `T x;` (a local declaration without an initializer) and storing `T` in a struct field also permit the `default` constraint, because it guarantees a valid zero-initialized value. `new T[n]` requires `default`, `struct`, or `enum`; its initialized elements therefore satisfy the element type's nullability. Without one of these constraints, only a provably zero-length `new T[0]` is allowed. Use `T?[]` when empty slots are part of the API and no default constraint is available:

```lumasharp
class Slot<T>
{
    T? item;                     // 'T?' is allowed for any T: null if T is a reference type,
                                 // or a nullable value when T is a value type
    public bool Has() => item != null;
    public void Put(T v) mut { item = v; }
    public T Take() mut { let r = item!; item = null; return r; }
}

class Stack<T>
{
    T?[] items = new T?[16];      // ok: nullable elements can start as null
    T[] empty = new T[0];         // ok: no elements need initialization
    T[] bad = new T[16];          // error without default/struct/enum constraint
}
```

- Calling an interface method on `T` where `T : IFoo` and `T` is a struct does **not** box.
- `==` on values of type `T` is only available if a constraint supplies it: with a `class` constraint it is reference identity; otherwise the operator must come from a constraint type that declares it.
- Each instantiation of a generic type has its **own static fields and static constructor** (§22.2): `Counter<int32>.count` and `Counter<string>.count` are different variables.

## 12.5 Generics Over Structs and References

Generic code behaves identically whether `T` is a struct, class, or primitive; value-type arguments are stored inline where the instantiation allows it (an implementation detail). Boxing never occurs implicitly for generic storage.

## 12.6 Restrictions `[P-16]`

- **Variance:** type parameters are invariant. There is no `in`/`out` variance syntax; `IProducer<Dog>` is not convertible to `IProducer<Animal>`.
- **Generic virtual methods:** a generic method MUST NOT be `virtual`, `override`, or an interface member. (Dynamic dispatch over an unbounded set of instantiations would force a particular implementation strategy.)
- No generic attributes (attributes are not in this version), no static abstract interface members, no open-generic reflection.
- The type of an expression can never be an unbound generic type (`Box<>`).

## 12.7 Generic Arrays and Nullable Combinations

```lumasharp
Box<int32>[] boxes = new Box<int32>[3];      // error LS1501: Box<int32> is a non-nullable class type
Box<int32>[] empty = new Box<int32>[0];      // ok: empty arrays have no elements to initialize
Box<int32>?[] okay = new Box<int32>?[3];     // elements null initially
Box<int32>[] filled = { new(1), new(2) };    // ok: every element provided
List<Foo?> list = new List<Foo?>();          // type argument may itself be nullable
```

---

# 13. Nullability

## 13.1 Reference Types Are Non-Null By Default

In LumaSharp, reference types are not allowed to be null unless they are declared with the nullable `?` operator. Value types can also be made nullable.

```lumasharp
Foo a = new Foo();        // never null; guaranteed by the compiler
Foo? b = null;            // may be null
Foo c = null;             // error LS1301: null cannot be assigned to non-nullable Foo
Foo d = b;                // error LS1302: Foo? to Foo (not proven non-null)
```

A **nullable reference type** `Foo?` has the same runtime representation as `Foo` with the additional possible value `null`. Nullable *value* types (`int32?`, `MyStruct?`) are described in §13.6. **Nullability is implemented at the language level: there is no `Nullable<T>` library type**, only a change to the type specification.

## 13.2 Null Safety Guarantee

Code outside `unsafe` contexts can never observe a null value through a non-nullable type, and can never assign `null` to one. Fields, locals, parameters, array elements, return values and generic arguments of non-nullable reference type are always non-null whenever read. The rules that make this possible are scattered through this document and summarised here:

| Rule | Section |
|---|---|
| `null` literal is not convertible to non-nullable types | §3.6 |
| Declarations of non-nullable references must be assigned (locals, fields, constructors) | §4.3, §8.3 |
| Struct fields may not be non-nullable references, including `string` | §9.1.1 |
| Array elements must be provided or the element type nullable | §15.2 |
| `T` may lack a default value, so `default(T)`/nonempty `new T[n]` require a `default`, `struct`, or `enum` constraint | §12.3–§12.4 |
| `this` may not be observed before fields are assigned; dynamic type during construction | §8.3.2 |
| Static initialization cannot be observed half-done | §22.2 |
| Nullable → non-nullable only via proof, `!`, `??` or a throwing cast | §13.3–13.5 |

## 13.3 Flow Analysis

For each local variable, parameter, `this`-rooted member path, and expression, the compiler tracks a **null-state**: *non-null*, *maybe-null*, or *null*. A nullable-typed expression that is **non-null** at a given program point converts implicitly to the non-nullable type there, and its members can be accessed (§13.5).

**State changes:**

| Construct | Effect |
|---|---|
| `x = new Foo()`, `x = nonNullExpr` | `x` becomes non-null |
| `x = null` | `x` is null |
| `x = maybeNullExpr` | `x` is maybe-null |
| `if (x != null) { … }` | `x` non-null inside; in `if (x == null) { … } else { … }`, non-null in `else` |
| `if (x == null) return; …` | `x` non-null after the `if` (early exit) |
| `x is Foo f`, `x is not null` | `f` non-null; `x` non-null where the test is true |
| `a && b`, `a \|\| b`, `?:` | states propagate according to short-circuit evaluation |
| `x!` | `x` becomes non-null after the expression (the assertion would have thrown otherwise) |
| `x ?? y` | result non-null if `y` non-null |
| `e as T` | non-null only if the conversion is provably successful and `e` is non-null (§5.12) |
| any method call | does not reset states of locals/parameters; **does** reset states of fields (tracked fields become maybe-null after a call or assignment through another path) |

**Required precision:** A conforming compiler MUST track locals and parameters through assignments, conditions (`==`, `!=`, `is`, `&&`, `||`, `!`, `?:`), early exits (`return`, `throw`, `break`, `continue`), and loops. Tracking of fields and properties is **implementation-defined** but MUST be conservative (treat as maybe-null when unsure). Assigning a nullable to a non-nullable relies on this analysis and is otherwise an error.

```lumasharp
Foo? foo = Find();
if (foo != null)
{
    foo.Bar();             // fine: non-null here
    Foo x = foo;           // fine: implicit conversion allowed, state is non-null
}
Foo y = foo;               // error LS1302

Foo? other = Find();
if (other == null) { return; }
Foo z = other;             // fine: early exit established non-null
other.Bar();               // fine
```

## 13.4 Null Assertion `!` (postfix)

`e!` asserts that `e` is non-null. Its static type is the non-nullable form of `e`'s type. It **subverts the compile-time check only**: the compiler injects a runtime safety check which throws a `NullReferenceException` if the value is null. It is also a flow-analysis assertion: afterwards the variable is considered non-null.

```lumasharp
Foo? myNullable = Find();
myNullable!.MyMember();    // assert, then access; throws NullReferenceException if null
Foo a = myNullable!;       // converts Foo? to Foo; throws NullReferenceException if null
Foo b = (Foo)myNullable;   // explicit cast form: same check
Foo c = (Foo)myNullable!;  // redundant but legal
int32? n = 5;
int32 m = n!;              // value-type form: m == 5; throws if n is null
```

When `e` is a **variable** of nullable value type, `e!` used as the object of a member access designates the contained value *in place*, so members can be assigned or mutated through it:

```lumasharp
MyStruct? val = MyStruct();
val!.field = 456;          // checks non-null, then assigns the field of the contained value
```

Applying `!` to an expression that is already non-null has no effect (warning LS3103).

## 13.5 Member Access on Nullable Types

**Members of a nullable type cannot be accessed unless the compiler can prove the instance is non-null.** Attempting to do so is a compile-time error (LS1303: "the instance may be null"). The same rule applies to indexing, invoking a nullable delegate, and iterating a nullable collection.

```lumasharp
Nullable? val = Find();

val.field = 123;           // error LS1303: val may be null

if (val != null)
{
    val.field = 123;       // fine: proven non-null in this branch
}
else
{
    val.field = 456;       // error LS1303: val is known to be null here
}

val!.field = 789;          // allowed: assertion, throws NullReferenceException if null at runtime
Nullable neverNull = (Nullable)val;   // throws NullReferenceException if val is null
```

Nullable structs work the same way:

```lumasharp
MyStruct? val = MyStruct();

if (val != null)
{
    DoSomething((MyStruct)val);   // cast to non-nullable (cannot fail inside this branch)
    val.field = 1;                // in-place access
}
else
{
    val.field = 456;              // error LS1303
}
```

### Null-conditional access `?.` and `?[]`

`a?.b` evaluates to `null` (of the nullable form of `b`'s type) if `a` is null; otherwise it accesses `b`. The whole chain short-circuits:

```lumasharp
Person? p = Find();
string? name = p?.Name;            // null if p is null
int32? len = p?.Name?.Length;      // null if p or p.Name is null (Name nullable)
p?.Save();                         // calls Save only if p is non-null
```

### Null-coalescing `??` and `??=`

```lumasharp
string a = maybe ?? "default";     // string: maybe if non-null, else "default"
Foo f = cache ?? new Foo();
cache ??= new Foo();               // assign only when null
```

`a ?? b`: the type is the non-nullable form of `a`'s type if `b` is non-nullable.

## 13.6 Nullable Value Types `T?`

`int32?`, `Vector?`, `Color?` are value types able to represent "no value". **The language specifies behaviour, not representation.** The compiler chooses the ABI form (e.g. `{ bool hasValue; T value }`, a tagged union, a niche in `T`), subject only to the requirement in §28.4 that the all-zero bit pattern represents `null`.

```lumasharp
int32? a = 5;
int32? b = null;
int32? c = default;        // null
bool has = a != null;      // true
int32 v = a!;              // 5
int32 w = b ?? 0;          // 0
int32? s = a + 1;          // 6  (lifted operator, §5.8)
```

- `T?` for a reference-type `T` is the nullable reference form, not a wrapper.
- `T??` is not a valid type.
- Nullable value types are boxed to `T` (or `null`), §3.7.
- Conversions: `T` → `T?` implicit; `T?` → `T` only via `!`, `??`, a cast, or flow-proven non-null.
- Member access on `T?` follows §13.5.

## 13.7 Summary

| Notation | Meaning |
|---|---|
| `Foo` | non-nullable reference |
| `Foo?` | nullable reference (same representation as `Foo`, may be null) |
| `Foo!` | *not a type*; `!` is a postfix expression operator only |
| `T?` (value type) | nullable value |
| `e!` | runtime non-null assertion, throws `NullReferenceException` |
| `(T)e` on a nullable | throwing conversion to non-nullable |
| `e as T` | non-throwing conversion; nullable result |

## 13.8 Cyclic-Reference Warning (cross-reference)

Non-nullable reference fields that can form a static cycle are diagnosed; nullable fields suppress the diagnostic. See §14.8.

---

# 14. Ownership, Lifetime and `scope`

This chapter defines the *observable* memory behaviour of LumaSharp programs. How the compiler and runtime achieve it (retain/release insertion, borrowing, elision, atomic operations) is specified in Part II (Chapters 25–33) and the Compiler Specification. **The programmer writes no ownership syntax**, with two exceptions: the `scope` allocation form (§14.4) and the `readonly` parameter modifier (§14.7).

## 14.1 The Model in One Page

1. Every **class instance, array, boxed value, string object, and closure environment** is a *managed object*.
2. A managed object has a **strong reference count**: the number of storage locations (variables, fields, array elements, captured variables, temporaries) currently holding a reference to it.
3. When the count reaches zero the object is **destroyed deterministically** (§8.4): finalizers run, fields are released, storage is freed.
4. All reference-count operations are inserted by the compiler. The programmer cannot observe them except through *when finalizers run*.
5. The compiler is free to **borrow** instead of retain/release wherever it can prove the referent stays alive (§14.3). Borrowing is an optimisation and never changes observable behaviour.
6. `scope` allocations are objects whose lifetime is **statically bounded** to a lexical scope. They are **not reference counted**: they are destroyed automatically when the scope dies, and the compiler rejects programs in which they could outlive it (§14.4–§14.7).
7. **Strong reference cycles are not collected** (§14.8). There is no tracing garbage collector.

## 14.2 Reference Semantics (Observable Rules)

For a storage location `L` of reference type:

| Operation | Effect on counts |
|---|---|
| `Foo a = new Foo();` | New object: count 1 (held by `a`) |
| `Foo b = a;` | Same object: count 2 |
| `a = other;` | Old referent: count −1 (destroyed at 0); new referent: count +1 |
| `a` goes out of scope | Referent count −1 |
| field/array-element store | as assignment |
| field/array destroyed | each held referent count −1 |
| `return expr;` | Ownership of the result is transferred to the caller |
| passing as argument | Ownership model per §14.3; callee storing it retains |
| closure captures variable | The environment holds the variable; the environment is itself a managed object (§17.4) |
| `x = null` (nullable `x`) | Old referent count −1 |

```lumasharp
class Foo { ~Foo() { Print("freed"); } }

void Test()
{
    Foo a = new Foo();        // count 1
    Foo b = a;                // count 2
    Print("A");
    a = new Foo();            // first object: count 1 (still held by b). New object created.
    Print("B");
}                             // end of scope: reverse declaration order:
                              //   b released → first object count 0 → "freed"
                              //   a released → second object count 0 → "freed"
                              // output: A, B, freed, freed
```

### 14.2.1 When is a local reference released? `[P-26]`

Observable semantics:

- A reference held by a **local variable or parameter** is released **no later than the end of the variable's scope**. When several variables are released at the same scope exit, they are released in **reverse order of declaration**.
- A **temporary** (the result of an expression that is not stored) is released at the end of the **full statement** containing it.
- The compiler MAY release a reference *earlier* (after its last use) or elide retain/release pairs **only if** this does not change the observable order or timing of finalizers relative to program-visible effects. In practice: a compiler may shorten the lifetime of an object whose destruction has no observable effect; it MUST NOT reorder the destruction of objects that (transitively) have finalizers. This is the *as-if rule*.

### 14.2.2 Safety invariant for borrowing

Whatever the compiler does internally, **safe code can never observe a destroyed object.** In particular, an argument remains alive for the entire duration of a call even if the callee (or code it calls) reassigns the caller's variable or field that originally held it:

```lumasharp
class Holder
{
    Foo item = new Foo();
    void Replace(Foo replacement) mut { item = replacement; }

    void Demo() mut
    {
        Use(item, this);            // 'item' is passed; Use calls h.Replace(...) which drops the old item
    }
    void Use(Foo f, Holder h) { h.Replace(new Foo()); f.Describe(); }   // f is still valid here
}
```

A compiler that cannot prove the argument remains alive MUST retain it for the duration of the call.

## 14.3 Implicit Borrowing and Transfer

Programmers do not write borrow/move syntax. The semantic guarantees that make the optimisations legal are:

- **Passing:** if the callee cannot retain the parameter beyond the call (it never stores, returns, or captures it), the compiler MAY pass it as a *borrow* with no count change.
- **Returning:** `return new Foo();` and `return localVar;` transfer ownership to the caller. The compiler MAY move the reference (no retain/release pair).
- **Initialization:** the first storage location initialised from a `new`/call result *owns* it; later aliases (`Foo b = a;`) are additional strong holders (count increments) unless the compiler proves a borrow suffices.
- **`in` and `ref` parameters** (§7.2): `in`/`ref` pass the caller's storage; no ownership is created merely by passing.
- **`out` parameters**: the callee's first assignment replaces the previous content (§7.2.4).

## 14.4 `scope` Allocation `[P-10]`

`scope` allocates an object whose **lifetime is bound to the enclosing lexical block** (a method body or any block statement). It is a heap allocation that is **not reference counted**: it is automatically destroyed when the scope dies.

```
scope_expr = "scope" type "(" args ")"            // class instance
           | "scope" "(" args ")"                  // class instance, type inferred (§5.14)
           | "scope" type "[" length "]"           // array
           | "scope" "[" length "]"                // array, type inferred
           | "scope" [ type "[" "]" ] "{" … "}" ;  // array with collection initializer
```

```lumasharp
void ScopeAllocation()
{
    if (true)
    {
        {
            // Only exists within scope 3
            var instance = scope MyClass();
        } // Scope 3
    } // Scope 2
} // Scope 1

void Test()
{
    var foo = scope Foo();             // destroyed when Test's block ends
    foo.DoSomething();
    int32[] buffer = scope int32[256]; // scoped array (zero-initialised)
    Foo other = scope();               // shorthand: type taken from the declaration
    int32[] small = scope { 1, 2, 3 }; // scoped array from a collection initializer
}                                       // small, other, buffer, foo destroyed here (reverse order)
```

Semantics:

- The object is a normal object: methods, fields, finalizers, and inheritance all work. It is *not* a different type; `Foo` is `Foo` whether allocated with `new` or `scope`.
- It is destroyed (finalizer run, fields released, storage freed) when execution leaves the **block that contains the `scope` expression's declaration**, in **reverse order** of the block's other scope allocations (§14.6).
- `scope` describes a **lifetime guarantee**, not a storage location. The compiler MAY place small scoped objects and arrays on the machine stack, in an arena, or on the heap, as it sees fit.
- A scoped object is **not reference counted** (§27.6). Aliases of it are not strong owners (§14.5).
- `scope` is valid for classes and arrays. It is not valid for structs (already values), strings, delegates, or type parameters not constrained to `class`.
- `scope` may only be used inside a method, constructor, accessor or lambda **body**. In a field initializer, static initializer, or constant context it is error LS0803 (there is no enclosing scope to bind to).
- A `scope` allocation that is the initializer of a local variable (or the right-hand side of its declaration) lives until the end of that variable's block. As an unnamed temporary (for example `F(scope Foo())`) its lifetime is the end of the enclosing full statement.
- A scope allocation inside a loop body is created and destroyed once per iteration (the loop body is a block).

### 14.4.1 Scoped elements of collections

Elements of an array that are themselves reference types may only be `scope` allocations **if the owning array is also scope allocated**. A heap array outlives the scope; storing a scope-bound element in it is an escape (LS0701).

```lumasharp
MyClass[] a = { scope MyClass() };            // error LS0701: heap array cannot own a scoped element
MyClass[] b = scope { scope MyClass() };      // ok: both die with the enclosing block
MyClass?[] c = scope [3];
c[0] = scope MyClass();                       // ok if c is scope-allocated in the same or an inner scope
```

In an ordinary block the array is destroyed *before* the elements that were allocated before it (reverse order of creation), so the array never refers to a destroyed element.

## 14.5 Scope-Bound References and Escape Rules `[P-11]`

A reference is **scope-bound** if it refers to a `scope` allocation made in a scope that is still active, or if it is a *closure* that captures a scope-bound reference (§17.4), or is derived from a scope-bound reference by assignment to a local variable (aliasing).

Aliasing is legal and *stays scope-bound*; the alias is not a strong owner:

```lumasharp
{
    Foo a = scope Foo();
    Foo b = a;               // legal: b is also scope-bound to a's scope
    b.DoSomething();         // legal
}                            // the object is destroyed here; a and b are both dead
```

### Required rejections (compile-time errors, LS0701)

A conforming compiler MUST reject every one of these uses of a scope-bound reference:

| Use | Example |
|---|---|
| `return` it (directly or via an alias) | `return foo;` |
| Store it in a field, static, or property of any object | `field = foo;` |
| Store it in an array element or collection | `list.Add(foo);`, `Foo[] arr = { scope Foo() };` (unless the container is itself scope allocated, see below and §14.4.1) |
| Store it into a variable declared in an **outer** scope | `Foo outer; { var f = scope Foo(); outer = f; }` |
| Pass it to a parameter that is not `readonly` | `Store(foo);` |
| Assign it through a `ref`/`out` parameter | `holder = foo;` where `holder` is a `ref Foo` or `out Foo` parameter |
| Capture it in a lambda that is itself not scope-bound (a closure stored/returned) | `field = () => foo.Run();` |
| Throw it as an exception | `throw foo;` |
| Call an instance method on it whose receiver `this` escapes (see below) | |

### Required acceptances

A conforming compiler MUST accept at least:

- Aliasing into locals of the same or an inner scope.
- Member access, calling methods, reading/writing fields on it (subject to `mut`).
- Passing it to a `readonly` parameter (including `readonly ref`).
- Passing it as the receiver of a method call when the method (and, for virtual/interface calls, every possible target known to the compiler) does not let `this` escape.
- Capturing it in a lambda that is assigned to a local of the same or inner scope, or passed directly to a `readonly` delegate parameter. The resulting delegate is scope-bound.
- Storing it into a scoped array/collection allocated with `scope` in the same or an inner scope **only** if the compiler can prove the container's lifetime is no longer than the referent's; otherwise reject.
- Using it as a constructor argument stored by another **scope**-allocated object in the same scope, only when the storing object is also scope-bound **and** was declared **after** the referent (so destruction order is safe).

Everything between the two lists is **implementation-defined, and MUST be conservative**: the compiler may reject programs it cannot prove safe, but MUST NOT accept a program in which a scope-bound reference could outlive its allocation. (This means some valid programs may not compile; the specification accepts this trade-off.)

### 14.5.1 Receiver escape

An instance method *lets `this` escape* if its body (or a callee it passes `this` to by non-`readonly` parameter) stores `this`, returns `this`, captures `this` in a non-scope-bound closure, or passes it to a non-`readonly` parameter. Calling such a method on a scope-bound receiver is error LS0702. For virtual/interface calls the compiler considers every override it can see; if the set of possible targets is open (cannot be proven), it MUST reject.

```lumasharp
class Node
{
    static Node? last;
    void Remember() mut { last = this; }          // lets 'this' escape
    void Show() { Print("node"); }               // does not
}
void Test()
{
    var n = scope Node();
    n.Show();                                     // fine
    n.Remember();                                 // error LS0702: 'this' escapes
}
```

### 14.5.2 What scoped objects may contain

- A scoped object **may hold ordinary heap references** in its fields (it retains them like any object; they are released when the scoped object is destroyed).
- A scoped object may hold a reference to another scope-bound object **only** under the destruction-order rule above (referent declared first). Otherwise storing a scope-bound reference into any object's field is rejected.

## 14.6 Scope Exit and Unwinding Order

When control leaves a block (by reaching its end, `return`, `break`, `continue`, or an exception propagating out) the following happen for that block, in **strict reverse order of declaration**:

- `scope` allocations are destroyed, and
- local variables holding references are released (and local struct values holding references are released),

each at the position where it was declared, so the **last declared item is unwound first**.

```lumasharp
void Demo()
{
    var a = scope Logger("a");
    var b = scope Logger("b");
    Print("body");
}
// output: body, b destroyed, a destroyed
```

If scope exit is caused by an exception, all of the above still run before the exception reaches an enclosing `catch`. A `try` block's scope-exit actions run **before** its `catch` or `finally` handler executes (the `try` block has been left). Whether the compiler implements this with cleanup landing pads, finally regions, or otherwise is an implementation detail. (The exception is fatal termination, which unwinds nothing: §26.5.)

```lumasharp
void Test()
{
    try
    {
        var foo = scope Foo();
        Throw();                  // exception
    }                              // foo destroyed here
    catch (Exception e)
    {
        // foo has already been destroyed when this handler runs
    }
}
```

## 14.7 `readonly` Parameters — Non-Escaping References

`readonly` before a parameter's type states: *this parameter's value cannot escape the invocation.*

```lumasharp
void Process(readonly Foo foo)
{
    foo.DoSomething();            // use it freely
}
```

Inside a method, a `readonly` parameter (and the implicit `this` of a finalizer) MUST NOT be:

- stored in a field, static, array element, or collection;
- returned (directly or inside another value);
- assigned to a variable of an outer scope or to a non-`readonly` `ref`/`out` target;
- captured by a lambda that is not scope-bound;
- passed to a parameter that is not itself `readonly`;
- used as the receiver of a method that lets `this` escape (§14.5.1).

It MAY be: read, have its members accessed, be passed to other `readonly` parameters, be compared, be assigned to a local variable of the callee's own scope (the alias is itself restricted).

```lumasharp
class Container
{
    Foo value = new Foo();
    public void Store(readonly Foo foo) mut
    {
        value = foo;              // error LS0701: readonly parameter cannot be stored
    }
    public void Draw(readonly Foo foo)
    {
        Render(foo);              // ok iff Render(readonly Foo f)
    }
}

void Caller()
{
    var s = scope Foo();
    Process(s);                   // ok: Process takes a readonly parameter
    container.Draw(s);            // ok: Draw only passes it on to readonly parameters
}                                  // s destroyed here; nothing could have kept it
```

`readonly` is also permitted in front of other modifiers (`readonly ref int32 a`, `ref readonly Foo f`) and means the same. It is **not** a mutability modifier for the referenced object's fields; `mut` governs that (§8.5).

Non-`readonly` ordinary heap references can be passed freely; only scope-bound references are restricted to `readonly` (or non-escaping-receiver) uses.

## 14.8 Reference Cycles `[P-21]`

LumaSharp uses reference counting. **A group of objects that strongly reference each other is not reclaimed automatically**, even if nothing else refers to the group. There is no cycle collector and, in this version, no `weak`/`unowned` reference. Programs that build cyclic structures MUST break the cycle themselves, typically by assigning `null` to a nullable reference field.

```lumasharp
class Parent { public Child child; }
class Child  { public Parent? parent; }       // nullable: can be cleared to break the cycle

var p = new Parent();
var c = new Child();
p.child = c;
c.parent = p;               // cycle: p ↔ c; neither will be freed automatically
c.parent = null;            // cycle broken; both are freed when p and c go out of scope
```

### Static cycle diagnostic (warning LS3301) — *required*

The compiler MUST analyse the type declarations and report a **warning** when non-nullable reference fields can form a cycle in the **type graph**:

- Build a directed graph whose nodes are class (and struct) types. For each field (or auto-property) of non-nullable reference type `X` declared in type `A`, add an edge `A → X` and an edge from `A` to **every class that derives from `X`** (virtual-class hierarchies are expanded; interface-typed fields expand to known implementers where the compiler can enumerate them).
- Edges from **nullable** fields (`X?`) are **not added**: nullability gives the program an explicit way to break the link.
- Array and generic-argument types are not followed in this version (`Foo[]`, `List<Foo>` do not create edges; the compiler MAY follow them).
- If the graph contains a cycle (including a self-edge), report the warning once per strongly connected component, naming the types and fields involved.

```lumasharp
class Node
{
    public Node innerNode;        // warning LS3301: potential strong reference cycle (Node → Node)
    public Node? otherNode;       // no diagnostic: nullable edge can be cleared
}

class A { public B b; }
class B { public A a; }           // warning LS3301: A → B → A

virtual class Base { public Other o; }
class Other { public Derived d; }
class Derived : Base { }          // warning: Base → Other → Derived (Derived is a Base)
```

This is a **diagnostic aid**, not a correctness rule. It is conservative: a program the compiler does not warn about may still leak through cycles at runtime (e.g. two `Node?` fields referencing each other), and a program it warns about may never create a cycle. Programs MUST NOT depend on the compiler detecting every cycle. A diagnostic-suppression mechanism is not part of this version.

Closure cycles (an object storing a delegate whose environment captures that object, §17.4) are **not diagnosed**; they are also not collected.

## 14.9 Thread Safety of Reference Counting

Reference-count updates performed by generated code MUST be **correct under concurrent access from multiple threads**: if two threads hold *separate* strong references to the same object and copy or release them concurrently, the count remains correct and the object is destroyed exactly once, after the last release.

This guarantee does **not** make user objects thread-safe. In particular:

- Concurrent unsynchronised reads/writes of the **same variable or field** (a data race on a reference-typed storage location itself) are not protected and are **undefined behaviour in unsafe contexts / unspecified in safe code** until synchronisation primitives are specified in a later version.
- Finalizers run on the thread that performs the final release.

Threading primitives themselves are not part of this version.

## 14.10 Examples of Complete Lifetimes

```lumasharp
Foo Create()
{
    return new Foo();               // ownership moves to caller: no extra retain
}

void UseIt()
{
    var f = Create();               // f owns the object (count 1)
    var g = f;                      // count 2
    f = Create();                   // old object count 1 (g), new object count 1 (f)
}                                   // g released → old object destroyed; f released → new one destroyed

void Process(readonly Foo foo) { }

void Caller()
{
    var s = scope Foo();            // scope-bound
    Process(s);                     // ok
    var t = new Foo();              // ordinary heap object
    Process(t);                     // ok
    list.Add(t);                    // ok: ordinary heap reference, retained by the list
    list.Add(s);                    // error LS0701: scope-bound reference cannot be stored
}
```

---

# 15. Arrays

## 15.1 Array Types

An array is a **reference type** (a managed, reference-counted object) holding a fixed number of elements of one type.

| Type | Meaning |
|---|---|
| `T[]` | One-dimensional array. Its elements are **always contiguous** in memory in index order |
| `T[,]` | Two-dimensional rectangular array |
| `T[,,]` | Three-dimensional rectangular array |
| `T[][]`, `T[,][]`, … | **Jagged** arrays: arrays whose elements are themselves arrays (each element an independent array object) |

Rectangular arrays support **up to three dimensions** (`T[,,,]` is error LS1503). How multi-dimensional arrays are laid out is implementation-defined (row-major contiguous storage is recommended, §28.11), but the semantics below are fixed.

- The length of each dimension is fixed at creation.
- Arrays implicitly derive from `object` (and the compiler-known `Array`) and implement `IEnumerable<T>`.
- `==` on arrays is reference identity.
- Element types: any type; but see §15.2 for non-nullable reference element types.
- `[P-20]` Members: `Length` (`int32`, the **total** number of elements across all dimensions), `Rank` (`int32`, number of dimensions), and `GetLength(int32 dimension)` (`int32`, the length of the zero-based dimension; `IndexOutOfRangeException` if out of range).

```lumasharp
int32[,] grid = new int32[3, 4];
int32 total = grid.Length;       // 12
int32 rank = grid.Rank;          // 2
int32 rows = grid.GetLength(0);  // 3
int32 cols = grid.GetLength(1);  // 4
```

## 15.2 Creation

| Form | Meaning |
|---|---|
| `new T[n]` | Heap array of `n` default-valued elements |
| `new T[n, m]`, `new T[a, b, c]` | Heap rectangular array |
| `scope T[n]` | Scoped array (§14.4) |
| `new T[] { a, b, c }` | Array with elements; length inferred |
| `new T[n] { a, b, c }` | Length given; MUST equal the number of initializers |
| `new T[,] { { a, b }, { c, d } }` | Rectangular array; shape inferred from the nested initializer |
| `new [n]`, `scope [n]` | Target-typed (`T` taken from the declaration), §5.14 |
| `{ a, b, c }` | **Collection initializer** shorthand, §15.3 |

```lumasharp
int32[] a = new int32[10];                 // ten zeros
int32[] b = new int32[] { 1, 2, 3 };       // {1,2,3}
int32[] c = new int32[3] { 1, 2, 3 };      // length stated and checked
int32[] d = scope int32[64];               // scoped, zero-initialised
string[] e = new string[3];                // error LS1501: string is a non-nullable reference
Foo?[] f = new Foo?[5];                    // five nulls
Foo?[] g = new [5];                        // same, target typed
int32[,] m = new int32[2, 3];              // six zeros
int32[,,] cube = new int32[2, 2, 2];       // eight zeros
```

**Element initialisation.** The elements of a new array are initialised to their type's default value: zero for value types and `null` for nullable types. **Non-nullable reference types, including `string`, have no default value, so an array of such elements cannot be created with a positive length and no initializer:**

```lumasharp
Foo[] h = new Foo[5];                      // error LS1501: Foo is not default-constructible
Foo[] empty = new Foo[0];                  // ok: there are no elements to initialize
Foo[] i = { new Foo(), new DerivedClass() }; // ok: every element is provided
Foo?[] j = new Foo?[5];                    // ok: nullable elements
int32[][] jag = new int32[3][];            // error LS1501: int32[] is a non-nullable reference
int32[]?[] jag2 = new int32[]?[3];         // ok: three null arrays
int32[][] jag3 = { { 1, 2 }, { 3 } };      // ok: inner initializers allocate each inner array
```

That is the nullable contract: a non-nullable element can never be left without a value. Therefore a reference-element array is created either with a collection initializer that provides every element, or with a nullable element type that is assigned later:

```lumasharp
MyClass?[] arr = new [5];                  // all null for now
arr[0] = new MyClass();
```

- `n` (and every dimension length) MUST be an integer expression implicitly convertible to `int32`. A negative length throws `OverflowException`. Allocation failure throws `OutOfMemoryException`.
- Elements that are `scope` allocations are only allowed in a `scope`-allocated array (§14.4.1).

## 15.3 Collection Initializers

A **collection initializer** is a brace-enclosed, comma-separated list of element expressions. It can appear on its own, without a preceding `new` expression, wherever the target array type is known:

```lumasharp
int32[] arr1 = new int32[] { 1, 2, 3 };        // fully explicit (with or without a size)
int32[] arr2 = { 1, 2, 3 };                     // shorthand: implies "new int32[] { 1, 2, 3 }"
int32[] arr3 = new { 1, 2, 3 };                 // 'new' may be written explicitly before the braces
int32[] arr4 = scope { 1, 2, 3 };               // scope allocation must be stated
var arr5 = new int32[] { 1, 2, 3 };             // var/let: the expanded form with type info is required
var arr6 = { 1, 2, 3 };                         // error: with var/let the type must be written
```

Rules:

- A bare `{ … }` allocates with `new` unless `scope` is written before it.
- With a `let` or `var` declaration there is no target type, so the **expanded form** `new T[] { … }` / `scope T[] { … }` is required.
- The length is the number of elements; if a size is given (`new T[3] { … }`) it must match exactly (LS1504).
- Elements are evaluated **left to right**; each is converted implicitly to the element type.
- **Multi-dimensional arrays** use nested initializers, one brace level per dimension; every sub-list in a dimension MUST have the same length:

```lumasharp
int32[,] m = { { 1, 2, 3 }, { 4, 5, 6 } };        // 2 x 3
int32[,,] c = { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } };   // 2 x 2 x 2
int32[][] j = { { 1 }, { 2, 3, 4 } };              // jagged: nested braces create one array per list
int32[][] js = scope { scope { 1 }, scope { 2, 3 } }; // all scoped
```

For a jagged array each inner `{ … }` is its own collection initializer and allocates with `new` unless it says `scope` (and a heap outer array may not contain scoped inner arrays, §14.4.1).

- An empty initializer `{ }` creates a zero-length array (when the element type is any type).
- **Inline arrays** are different: see §15.9.

## 15.4 Element Access

`a[i]` reads or writes element `i` (a variable, so it can be passed `ref`/`out`). Multi-dimensional arrays take one index per dimension: `a[i, j]`, `a[i, j, k]`. Jagged arrays chain: `a[i][j]`.

```lumasharp
int32[] a = { 10, 20, 30 };
a[1] = 99;                      // {10, 99, 30}
int32 n = a.Length;             // 3
int32 x = a[3];                 // throws IndexOutOfRangeException

int32[,] m = new int32[2, 3];
m[1, 2] = 7;
int32 bad = m[2, 0];            // throws IndexOutOfRangeException: first index out of range
```

Each index MUST be implicitly convertible to `int32`. A multi-dimensional access is **checked per dimension**: it throws if *any* index is outside its own dimension's range, even if the corresponding flattened offset would lie inside the storage.

## 15.5 Bounds Checking

Every element access in ordinary code is **bounds-checked**: an index `i < 0 || i >= length(dimension)` throws `IndexOutOfRangeException`. Inside an `unsafe unchecked` context the compiler **MAY omit** the check; an out-of-range access there is **undefined behaviour** (§19.6).

## 15.6 Iteration

```lumasharp
for (let x in a) { Print(x); }              // 10 99 30
for (int32 i = 0; i < a.Length; i++) { }
for (let v in m) { }                        // every element of a multi-dimensional array, row-major
```

## 15.7 Invariance

Arrays are **invariant**. There is no array covariance and therefore no array-store type check at runtime.

```lumasharp
Dog[] dogs = { new Dog() };
Animal[] animals = dogs;                    // error LS1502: Dog[] is not convertible to Animal[]
object o = dogs;                            // fine: any array converts to object
```

## 15.8 Arrays and Ownership

An array holds a strong reference to each non-null reference-typed element. Destroying the array (count 0, or scope exit) releases every element, in **reverse index order** (for multi-dimensional arrays, reverse of the storage order, i.e. reverse row-major). Assigning to an element releases the old element and retains the new one. Struct elements that contain references are released through their fields.

## 15.9 Inline Arrays (`unsafe`)

An **inline array** has a *compile-time-constant element count* and occupies storage **directly inside its containing storage location**. Inline arrays require an unsafe context.

```lumasharp
struct Header
{
    public unsafe int32[16] values;        // 16 ints physically inside every Header value
    public int32 count;
}

unsafe void Test()
{
    int32[16] local;                        // local inline array: no heap, no count, zeroed
    local[0] = 5;
    int32 n = local.Length;                 // 16 (constant)
    int32* p = &local[0];
    int32[4] init = { 1, 2, 3, 4 };         // collection initializer only (no allocation expression)
    int32[4] partial = { 1, 2 };            // remaining elements are zero
    local[1] = 7;                           // or assign elements individually
}
```

Rules:

- Syntax: `T[N] name;` where `N` is a constant expression > 0. Allowed as a **field** of a struct or class (marked `unsafe`) or as a **local** inside an unsafe context. They are not heap arrays: they cannot be created with `new` or `scope`, are not reference types, and are not convertible to `T[]`.
- **All elements are default-initialised** (zero / `null`).
- **Initialisation:** only the collection itself is written. The allocation expression MUST NOT be included: `int32[4] x = { 1, 2, 3, 4 };` is allowed, while `int32[4] x = new { … };`, `scope { … }` and `new int32[4] { … }` are errors (LS1505). Alternatively assign elements individually. The initializer may contain fewer than `N` elements (the rest are default) but not more `[P-9]`.
- **Element types:** primitives, enums, **structs**, and (in an unsafe context) **pointer types**. **Classes** (and other reference types: `string`, arrays, delegates, interfaces, `object`) are **not** allowed (error LS1510). A struct element may itself contain nullable reference fields; the compiler handles their reference counting when the inline array is copied or destroyed. Non-nullable `string` fields are prohibited in structs (§9.1.1).
- They copy with their container (copying a `Header` copies all 16 ints).
- Indexing is bounds-checked against `N` unless in `unsafe unchecked`.
- `Length` is a compile-time constant. `for (let x in values)` iterates the elements.
- The compiler may place a local inline array in stack storage.
- Inline arrays are one-dimensional.

---

# 16. Strings and Characters

## 16.1 `char`

`char` is a **16-bit UTF-16 code unit** (2 bytes, unsigned). It can hold a Basic-Multilingual-Plane character or half of a surrogate pair. Literals: §2.5.4.

```lumasharp
char c = 'A';
int32 code = c;           // 65 (implicit)
char d = (char)(c + 1);   // 'B'
```

## 16.2 `string`

`string` is an **immutable reference type**. Its contents never change after creation; any "modifying" operation (`Substring`, concatenation, replace) produces a new `string`.

A string consists logically of a **pointer** to character storage, a **character set (encoding)**, and a **length**.

- A string either **references** existing character memory (for example data embedded in the executable, as for a literal) or **owns** its own memory (for example the result of `Substring`). Either way strings are ordinary managed objects (embedded literal strings are immortal, §27.2).
- Several encodings are allowed. The encoding is part of the string's data and is runtime-defined; **UTF-8** is the default for literals and library results. UTF-16 and others may be supported.
- **UTF-8 strings always have a terminating `'\0'` stored after the last character.** The terminator is **not** counted in `Length`. This lets the buffer be shared with C-style APIs without copying.
- The exact object layout is defined in §28.10; the language guarantees only what is listed in this chapter.

### 16.2.1 Default value

`string` is a non-nullable reference type and has no default value. A local, field, parameter, or array element of type `string` must be explicitly assigned a non-null string before it can be read. `default(string)` is an error; `default(string?)` is `null`. The empty string literal `""` is an ordinary valid string value and may be shared as immutable literal data.

```lumasharp
string a = "";
string? b = default;                   // null
string[] arr = { "", "", "" };
class Person { public string Nick = ""; }
```

## 16.3 Length and Indexing

`s.Length` is the **number of UTF-16 code units** in the string, regardless of its storage encoding. This follows C# `String.Length` semantics: it never counts the terminating `\0` and is not the byte count of the stored encoding. `s[i]` returns the `char` at UTF-16 code-unit position `i` (`IndexOutOfRangeException` if out of range).

For example, a UTF-8 string with `Length` 5 occupies 6 bytes internally for ASCII text (five characters plus the terminating byte). A character outside the Basic Multilingual Plane is represented by a surrogate pair and therefore counts as two UTF-16 code units, exactly as in C#.

```lumasharp
string a = "abc";
int32 n = a.Length;           // 3
char c = a[1];                // 'b'
string e = "";
int32 z = e.Length;           // 0
string u = "h\u00E9llo";      // stored as UTF-8 (6 bytes + terminator)
int32 m = u.Length;           // 5: characters, not bytes
```

## 16.4 Equality and Comparison

`==` and `!=` on strings compare **content** (logical characters), regardless of encoding or storage. Two strings with different encodings but the same logical text are equal. `<`, `>`, etc. are not defined on strings in the language; use library methods.

Apart from the empty string (§16.2.1), whether two string literals with identical content are the same object is *unspecified*; programs MUST use `==` for content comparison and never rely on reference identity of strings.

## 16.5 Concatenation

`+` concatenates strings, producing a new string. If one operand is not a string, it is converted by calling `ToString()` on it (boxing value types if needed); a `null` operand contributes the empty text.

```lumasharp
string a = "x" + "y";        // "xy"
string b = "n=" + 5;          // "n=5"
string? s = null;
string c = "v:" + s;          // "v:"
```

## 16.6 Strings and Nullability

`string` is non-nullable and must be explicitly initialized before it can be read. `string?` may be null and follows §13.

## 16.7 Other Members

All other string members (`Substring`, `IndexOf`, `Replace`, …) are standard-library functionality and outside this specification, except that a conforming standard library MUST provide `Length`, indexing `this[int32]`, and `==`. Interpolated strings are not part of this version.

---

# 17. Delegates and Lambdas

## 17.1 Delegate Types `[P-15]`

A delegate type describes a method signature; its values refer to a method (and, for instance methods, a receiver) or a lambda.

```lumasharp
delegate int32 BinaryOp(int32 a, int32 b);
delegate void Action();
delegate bool Predicate<T>(T item);
```

- Delegates are **reference types** (managed, reference counted, non-nullable by default; `BinaryOp?` may be null).
- Delegate declarations may be generic and may be declared at namespace level or inside a type.
- A delegate value refers to **exactly one target**. **Multicast delegates are not supported** (no `+`/`-` combination of delegates), and there is no event system: such facilities are expected to be provided by system libraries.
- Invocation: `op(1, 2)`. A nullable delegate is invoked after a null test, or with the assertion form `op!(1, 2)`; there is no `Invoke` member and no `op?(…)` form in this version.
- Delegate equality (`==`) is true when both delegates refer to the **same method and the same target object** (or are both static/closure-free for the same method); otherwise false. Two separately written lambdas are never equal.
- A delegate invocation passes arguments exactly as a method call, including `ref`/`out`/`in` arguments. Delegate parameters cannot be optional.

```lumasharp
int32 Add(int32 a, int32 b) => a + b;

BinaryOp op = Add;                  // method-group conversion
Print(op(2, 3));                    // 5

BinaryOp? maybe = null;
if (maybe != null) { Print(maybe(1, 2)); }
maybe!(1, 2);                       // throws NullReferenceException
```

## 17.2 Method Group Conversion

A method name (or `obj.Method`) converts to a compatible delegate type: parameter types identical, return type identical or implicitly convertible (for reference returns). For an instance method group, the delegate **retains** the receiver. Creating a method group for a `mut` method on a `this`-rooted receiver is a mutation for the purposes of §8.5.

## 17.3 Lambda Expressions

```
lambda = [ "(" [ lambda_params ] ")" | identifier ] "=>" ( expression | block ) ;
lambda_params = lambda_param { "," lambda_param } ;
lambda_param  = [ type ] identifier ;
```

```lumasharp
BinaryOp add = (int32 a, int32 b) => a + b;      // explicit parameter types
BinaryOp mul = (a, b) => a * b;                  // inferred from the target delegate type
Predicate<int32> isEven = n => n % 2 == 0;       // single parameter, no parentheses
Action hello = () => { Print("hi"); };           // block body
```

- A lambda has **no type of its own**; it MUST be converted to a delegate type (target typing). `var f = x => x;` is error LS1701.
- Parameter types are inferred from the target; if given explicitly they MUST match it.
- The body's return type is inferred as the target's return type: all `return` expressions MUST convert to it.
- Lambda parameter names must not conflict with enclosing locals (§4.4).
- Lambdas may be `unsafe`-context members only if declared in an unsafe context.
- There are no `static` lambdas, no async lambdas, and no lambda attributes in this version.

## 17.4 Closures and Captured Variables

A lambda may use local variables, parameters, and `this` of the enclosing method. These are **captured**:

- Captured variables are shared by reference with the enclosing method (C# semantics): changes made by the lambda are visible to the method and vice-versa, and each closure invocation sees the current value.
- Captured variables live in a compiler-generated **closure environment**, itself a managed, reference-counted object that lives as long as any delegate referring to it.
- The environment **retains** every captured reference-typed variable's referent (strictly: the variable lives in the environment, and the environment owns the variable's reference). The environment releases them when it is destroyed.
- Capturing `this` retains the receiver.
- A lambda MUST NOT capture `ref`, `in`, or `out` parameters, nor a `readonly` parameter in a way that lets it escape (§14.7), nor `scope`-bound references unless the resulting delegate is itself scope-bound (§14.5).
- A lambda body inside a non-`mut` method may not mutate receiver state (checked as part of that method).

```lumasharp
Action MakeCounter()
{
    int32 count = 0;
    return () => { count++; Print(count); };    // 'count' lives in the closure environment
}

var counter = MakeCounter();
counter();                                       // 1
counter();                                       // 2
```

**Closure cycles.** A delegate stored in an object that the delegate captures forms a strong cycle (`Foo → delegate → environment → Foo`). It is **not** collected automatically; break it by setting the delegate field (declared nullable) to `null`:

```lumasharp
class Button
{
    public Action? OnClick;
    public void Wire() mut { OnClick = () => this.Pressed(); }   // cycle: Button → delegate → Button
    public void Unwire() mut { OnClick = null; }                  // breaks it
    void Pressed() { }
}
```

## 17.5 Delegate Conversions

| From | To |
|---|---|
| lambda / method group | delegate type with compatible signature |
| delegate type `D` | `object`, `Delegate` |
| `null` | nullable delegate type |

No conversions exist between distinct delegate types with identical signatures.

---

# 18. Exceptions

## 18.1 Model

LumaSharp has C#-style structured exceptions. An exception is any object whose class derives from the compiler-known base class `System.Exception`. Exceptions are ordinary reference-counted objects.

There is **no checked-exception declaration** on methods.

## 18.2 `throw`

```lumasharp
throw new InvalidOperationException("bad state");
```

- The operand MUST be non-nullable and of a class type deriving from `Exception` (`throw nullable;` requires `!`).
- `throw;` (no operand) is only valid inside a `catch` block and **rethrows the current exception** preserving it.
- Throwing begins **stack unwinding**: execution leaves enclosing scopes outward until a handler matches.

## 18.3 `try` / `catch` / `finally` `[P-30]`

```
try_statement = "try" block ( catch_clause { catch_clause } [ finally_clause ] | finally_clause ) ;
catch_clause  = "catch" "(" type [ identifier ] ")" block
              | "catch" "(" "..." ")" block ;
finally_clause = "finally" block ;
```

```lumasharp
try
{
    Risky();
}
catch (InvalidOperationException e)       // typed catch with variable
{
    Print("invalid: " + e.Message);
}
catch (IOException)                       // typed catch, no variable
{
    Print("io");
}
catch (...)                               // catch-all: any exception, no variable
{
    Print("something else");
    throw;                                // rethrow
}
finally
{
    Cleanup();                            // always runs
}
```

Rules:

- Catch clauses are examined **in order**; the first whose type matches the thrown object (same type or base type) handles it. A clause that can never be reached because an earlier clause catches its base type is error LS1801.
- `catch (...)` matches every exception and MUST be the last clause. It cannot name the exception; use `throw;` to rethrow it.
- The catch variable is a read-only local scoped to the block. Its type is the declared exception type (non-null).
- `finally` runs when control leaves the `try`/`catch` blocks by any path: normal completion, `return`, `break`, `continue`, or exception. A `finally` block MUST NOT leave via `return`/`break`/`continue` (error LS0610).
- If a `finally` block throws while another exception is propagating, the new exception **replaces** the propagating one (the original is released).
- Exception filters (`when`) are not part of this version.
- An exception thrown inside a `catch` block propagates outward (after that `try`'s `finally`, if any, runs).
- An exception for which no handler exists anywhere on the stack is *unhandled* and terminates the program (§26.5, §31.5).

## 18.4 Interaction with Scope and Finalizers

- On exception, scope-exit actions of every block being left (`scope` destruction, local release, §14.6) execute **before** control reaches the handler, innermost block first. If the exception is never handled (fatal termination), no such actions run (§26.5).
- A finalizer MUST NOT throw; an exception escaping a finalizer is a fatal error.
- If a constructor throws, the partially constructed object is destroyed as described in §8.4.1: finalizers of the classes whose constructors completed run, then the assigned fields are released.

```lumasharp
void Test()
{
    try
    {
        var a = scope Resource("a");
        Throw();
        Print("never");
    }
    catch (Exception e)
    {
        Print("caught");
    }
}
// order: a destroyed, caught
```

## 18.5 Exceptions Raised by the Language

| Situation | Exception |
|---|---|
| Failed null assertion `e!`, or a cast of `null` to a non-nullable type | `NullReferenceException` |
| Failed cast / unboxing | `InvalidCastException` |
| Array/string index out of range | `IndexOutOfRangeException` |
| Integer overflow in checked context; negative array length | `OverflowException` |
| Integer division/remainder by zero | `DivideByZeroException` |
| Allocation failure | `OutOfMemoryException` |
| Exception thrown by a static initializer / `static this()`, or circular static initialization | `TypeInitializationException` whose `InnerException` is the original (§22.2) |

Standard library code may define additional exceptions; their names and hierarchy are outside this specification.

---

# 19. Unsafe Code

Unsafe code is code in which the programmer, rather than the compiler, is responsible for memory safety. Unsafe code is the only place **undefined behaviour** can occur (§1.3).

## 19.1 Unsafe Contexts

The `unsafe` keyword is used as in C# to allow pointer usage. It can be applied as a modifier or as a statement:

```lumasharp
unsafe class RawBuffer { }                     // whole type is unsafe
class A
{
    unsafe void M() { }                        // whole member
    unsafe int32* ptr;                          // field of pointer type requires unsafe
    void N()
    {
        unsafe { /* unsafe block */ }
    }
}
unsafe unchecked { /* unsafe + wrapping arithmetic (§5.7) */ }
```

An unsafe context is required to: declare or use a pointer type; take an address with `&`; dereference with `*`; access members through a pointer; use `sizeof`; use `unchecked`; declare inline arrays (§15.9); call `extern` functions (§21). The project may disable unsafe code entirely (the Compiler Specification defines the setting, e.g. an `unsafe` flag in the project file); in that case any `unsafe` is error LS1901.

`unsafe` does **not** disable safety checks that still apply (null checks, bounds checks, checked arithmetic, null-assignment rules for non-nullable types). It merely permits the additional unsafe operations. Only `unchecked` removes checks.

## 19.2 Pointer Types

```
pointer_type = type "*" ;     // T*, T**, void*
```

- `T*` is a **value type** holding an address; the default is `null` (address 0).
- `T` MUST be: a primitive other than `string`, an enum, another pointer type, `void`, a struct, an inline array element type, or **a class type** (§19.5). Pointers to `string`, arrays, delegates, and interfaces are not permitted (error LS1903); take a pointer to the element or use `void*`.
- Pointers have **no ownership semantics**: they do not retain, release, or keep anything alive. A pointer to something that is later destroyed *dangles*; dereferencing it is UB.
- `void*` is an untyped pointer. Any `T*` converts implicitly to `void*`; `void*` converts to `T*` explicitly.
- Pointers are not nullable-tracked: `null` is assignable to any pointer without `?`, and a pointer can always be compared to `null`.

## 19.3 Pointer Operations

| Operation | Syntax | Meaning |
|---|---|---|
| Address-of | `&x` | Pointer to the storage of variable `x` (local, field, array element, parameter, inline-array element) |
| Dereference | `*p` | The value stored at the address (a variable: assignable) |
| Member access | `p.member` | **`.` automatically dereferences** a pointer to a struct or class: fields, properties, indexers and methods of the pointee |
| Indexing | `p[i]` | `*(p + i)` |
| Arithmetic | `p + n`, `p - n`, `p - q` | Scaled by `sizeof(T)`; `n` is any integer type; `p - q` yields `int64` |
| Comparison | `== != < > <= >=` | Compare addresses |
| Casts | `(T*)p`, `(uint64)p`, `(T*)n` | Explicit reinterpretation; pointer ↔ any integer type (truncating or zero-extending to the target's size) |
| Size | `sizeof(T)` | Size in bytes of an unmanaged `T`, type `int32` |

There is **no `->` operator**: member access through a pointer uses `.` exactly as for a reference, and the compiler dereferences automatically.

```lumasharp
unsafe void Demo()
{
    int32 x = 5;
    int32* p = &x;              // pointer to the local x
    *p = 10;                    // x == 10
    Print(*p);                  // 10

    int32[] arr = { 1, 2, 3 };
    int32* q = &arr[0];         // pointer to element 0 (no pinning needed; see §19.4)
    Print(*(q + 2));            // 3
    q[1] = 99;                  // arr[1] == 99

    Vector v = Vector(1, 2);
    Vector* pv = &v;
    pv.x = 7;                   // auto-dereference: v.x == 7
    pv.Move(1, 1);              // calls the mut method on v itself
}
```

`mut`, `readonly`, `let`, and accessibility rules apply to members accessed through a pointer exactly as through the pointee's own name (`&` on a `let` or `const` variable is error LS1902). Auto-dereference applies to a pointer **one level deep** (`T*`); a `T**` must be dereferenced explicitly.

## 19.4 Pointers and Heap Objects: No Pinning

Managed objects never move (there is no compacting collector), so **arrays do not need to be fixed and classes do not need to be pinned** to obtain an address, and no `fixed` construct exists. `&arr[0]` yields a stable pointer as long as the array is alive. The programmer is responsible for keeping the owner alive. The compiler does not extend lifetimes because of pointers.

```lumasharp
unsafe int32* Bad()
{
    int32[] tmp = new int32[4];
    return &tmp[0];             // legal to compile; dangling after return (tmp destroyed): UB to use
}

unsafe void Example(int32[] array, MyClass instance)
{
    int32* intPtr = &array[0];
    MyClass* instancePtr = &instance;
}
```

## 19.5 Pointers to Class Instances `[P-8]`

For a class type `Foo`, `Foo*` is an **unmanaged, non-owning reference to the object**. `&foo`, where `foo` is a variable of type `Foo`, yields a `Foo*` designating the **object `foo` refers to**, *not* the address of the variable `foo`.

```lumasharp
unsafe void Inspect(Foo foo)
{
    Foo* p = &foo;              // designates the object (no retain)
    p.counter = 3;              // field access through the pointer: same as foo.counter = 3
    p.Report();                 // method call (receiver borrowed, not retained)
    Foo again = *p;             // dereference yields a reference to the object (a normal strong reference: retained)
}
```

- A `Foo*` neither retains nor releases and does not keep the object alive. If the object has been destroyed, any use of the pointer (member access, `*p`) is UB.
- Supported on class pointers: `.` member access, `*p` (yields a `Foo`; the result is non-null; UB if the pointer is null), comparison, and casts to/from `void*` and integers (a round trip through `void*` or an integer yields the same designation).
- **Not supported** on class pointers: arithmetic and indexing.
- The numeric address that a `Foo*` holds is implementation-defined; it need not be the address of the first field.

## 19.6 `unchecked` and Bounds-Check Omission

`unsafe unchecked` regions allow:

- wrapping integer arithmetic (§5.7);
- omission of array and inline-array **bounds checks**. Out-of-range access there is **undefined behaviour**; the compiler MAY emit unchecked loads/stores.

```lumasharp
unsafe unchecked int32 Sum(int32[] values)
{
    int32 total = 0;
    for (int32 i = 0; i < values.Length; i++)
    {
        total += values[i];     // no bounds check required
    }
    return total;               // arithmetic wraps on overflow
}
```

## 19.7 Unmanaged Memory

Manual allocation of untracked memory is **not available in this version** (a runtime facility such as a `NativeMemory` class is planned for a future version). In this version pointers refer to existing storage: locals, fields, array elements, inline arrays, objects (§19.5), or addresses returned by native code through `extern` functions (§21). Memory obtained from native code is owned by that code; the runtime neither tracks nor frees it.

## 19.8 Managed References in Unmanaged Storage

Storing managed references in raw memory is not supported. Doing so through pointer casts is UB because it bypasses reference counting.

---

# 20. Namespaces, Imports and Visibility

## 20.1 File-Scoped Namespaces

A source file declares **one** namespace at the top, with a semicolon:

```lumasharp
namespace MyCompany.Shapes;

public class Circle { }
```

- All types in the file are in that namespace. `namespace` MUST precede any type declaration (but follow `using` directives, if any).
- A file with no namespace declaration places its types in the **global namespace**.
- Nested namespace declarations (`namespace X { }`) are not part of this version.
- Multiple files may declare the same namespace.

## 20.2 `using` Directives

```lumasharp
using System;
using System.Collections;
using Text = System.Text;                   // alias
```

- `using` directives appear at the **top of the file** (before the namespace declaration).
- `using N;` makes the types of namespace `N` available by simple name in the file.
- `using Alias = qualified.name;` introduces an alias for a namespace or a type, and for constructed generic types: `using IntList = System.Collections.List<int32>;`.
- Static imports (`using static`) and global usings are not part of this version.
- Fully qualified names always work: `System.Collections.List<int32>`.
- Name conflicts between imported namespaces are errors only when the ambiguous simple name is used (LS2001).

## 20.3 Name Lookup

A simple name is resolved by searching, in order: local scope and parameters → members of the enclosing type (and its bases) → enclosing types → types in the file's namespace → types in imported namespaces → global namespace. Ambiguity is an error.

## 20.4 Accessibility Across Files and Assemblies

`internal` access means: within the same **compilation** (project output). The project may name other compilations as *friends*; the mechanism is defined in the Compiler Specification, not the language.

---

# 21. Native Interop

## 21.1 `extern` Methods

A **static extern** method declares a function implemented outside LumaSharp (in the C ABI of the target platform). It has no body:

```lumasharp
public static class Native
{
    public static extern void SomeFunction(int32 value);
    public static extern int32 puts(uint8* text);
    public static extern void* malloc(uint64 size);
}
```

Semantics:

- The method binds at link time to the **unmangled symbol equal to the method's name** (`SomeFunction`, `puts`). The enclosing namespace and class names do **not** participate in the symbol.
- The call uses the **platform's default C calling convention**.
- **No marshalling is performed.** Parameters and return values MUST have a representation identical to the C ABI:
  - allowed: `bool` (1 byte), `char` (2 bytes), all integer types, `float`, `double`, enums (as their underlying type), pointers, `void`, and **structs containing only those** (passed per the platform C ABI);
  - **not allowed**: classes, strings, arrays, delegates, nullable value types, structs containing references. (Error LS2101.)
- Extern methods MUST be `static`, MUST have no body, MUST NOT be generic.
- **Calling an extern method requires an unsafe context** (the compiler cannot verify native code). The declaration itself requires no `unsafe` unless it uses pointer types.
- Struct layout for interop: a struct whose fields are all unmanaged is laid out **sequentially in declaration order** with the platform C alignment and padding rules. Other layouts are implementation-defined.
- Strings: a UTF-8 `string`'s internal buffer is NUL-terminated and may be shared with C APIs by the library/runtime (a language-level `string` → pointer facility is outside this version).

```lumasharp
unsafe void Hello()
{
    // Native.puts is an extern function; calling it needs unsafe.
    Native.puts(pointerToNulTerminatedUtf8);
}
```

- **Exceptions do not cross native frames:** a LumaSharp exception cannot propagate through native code (Chapter 31 describes the fatal outcome).
- The runtime behaviour of an `extern` call (symbol resolution, call sequence, argument passing) is described in Chapter 33.

## 21.2 What this Version Defers

Dynamic library loading, explicit calling conventions, symbol aliasing, function pointers, callbacks into LumaSharp from native code, and layout control are future work (§24).

---

# 22. Program Entry and Static Initialization

## 22.1 Build Kinds and the Entry Point `[P-19]`

A compilation produces either an **executable** or a **static library** (chosen by project settings or command-line arguments; see the Compiler Specification).

- **Executable:** MUST have exactly one **entry point**: a `public static` method named `Main` in any non-generic type, with one of these signatures:

```lumasharp
static void Main()
static void Main(string[] args)
static int32 Main()
static int32 Main(string[] args)
```

  The `int32` form's return value becomes the process exit code (`0` if the return type is `void`). `args` holds the command-line arguments without the program name. A missing or duplicate entry point in an executable is error LS2201.
- **Static library:** does **not** require a `Main` method or any entry point. A method named `Main` in a library is an ordinary method.

Program startup and shutdown behaviour is specified in Chapter 26.

## 22.2 Static Initialization `[P-13]`

Static fields (with initializers) and the static constructor `static this()` initialise a type **lazily, on first access**, so that any exception thrown by the initialization can be handled by the code that triggered it.

**A type is initialised immediately before the first of:**

- a static method of the type is invoked;
- a static field or static property of the type is read or written;
- an instance of the type is created (including via a constructor called through `new`, `scope`, or struct construction);
- the same operation on a given **instantiation** of a generic type. **Each generic instantiation has its own statics and its own initialization.**

`const` members never trigger initialization (they are compile-time values).

**Order and rules:**

1. The base class is initialised before the derived class.
2. Static field initializers run in **textual (declaration) order**, then the body of `static this()`.
3. Initialization happens **exactly once** per type (per instantiation), even with concurrent access; a second thread accessing the type waits until the first finishes.
4. **Re-entrant access**: if code running inside a type's initialization, on the same thread, accesses that same type's statics again (a cycle), the access throws `TypeInitializationException` ("circular static initialization"). A static of non-nullable type is therefore never observed before it is assigned.
5. **Failure.** If initialization throws exception `E`, the type is **permanently failed**. The access that triggered it throws `TypeInitializationException` with `InnerException == E`, and **every later access to the type throws that same `TypeInitializationException`**. The triggering code can catch it like any other exception.

```lumasharp
class Config
{
    public static int32 Level = Compute();   // runs on first access to Config
    static this() { Print("static ctor"); }
    static int32 Compute() { Print("init"); return 3; }
}

Print("start");
int32 a = Config.Level;                       // prints: init, static ctor
int32 b = Config.Level;                       // nothing more printed
// output: start, init, static ctor

class Broken
{
    public static int32 Value = Fail();
    static int32 Fail() { throw new Exception("boom"); }
}
try { int32 v = Broken.Value; }
catch (TypeInitializationException e)
{
    Print(e.InnerException!.Message);         // "boom"
}
```

Static fields of reference type hold strong references for the lifetime of the program. Behaviour at program exit is defined in §26.4.

### 22.2.1 Failure in the program's entry type

The class that declares the program's entry point is initialised **before `Main` runs**, as part of calling it (a static method invocation triggers initialization like any other). If that initialization fails, **the program crashes with the type initialization failure before the `Main` method is ever executed**: the runtime reports the `TypeInitializationException` and its inner exception, and terminates (§26.3). This is the **only** situation in which the program can crash without giving user code the opportunity to handle the exception, because no user code can run before the initialization of the entry type.

The compiler/runtime may implement this by injecting the entry-type initialization check into the program's startup stub or into the prologue of `Main`; the observable requirement is that it completes before the first statement of `Main`.

---

# 23. Diagnostics

## 23.1 Requirements

A conforming compiler MUST:

- report every **error** condition in this specification and produce no executable or library when any error is reported;
- report every **required warning**;
- give each diagnostic: severity, a stable **code**, a human-readable message, and source location (file, line, column; range recommended);
- allow warnings to be promoted to errors or disabled **only through project/compiler options** (defined in the Compiler Specification). There is no in-source suppression until a preprocessor exists (§24.2).

Codes below are *recommended identifiers* (`LS` + four digits). Programs MUST NOT depend on exact message text. Ranges: `LS0xxx` syntax/semantic, `LS1xxx` types/generics/nullability/arrays/unsafe, `LS2xxx` declarations/modules/interop, `LS3xxx` warnings.

## 23.2 Required Errors

| Code | Condition |
|---|---|
| LS0001 | Unexpected token / syntax error |
| LS0104 | Reserved-for-future keyword used as identifier (§2.4) |
| LS0201 | Local/parameter name conflicts with enclosing scope, or an initializer refers to a variable declared by the same statement (§4.2.4, §4.4) |
| LS0210 | Multiple assignment count mismatch (§4.6.3) |
| LS0212 | More initializers than identifiers in a multiple declaration (§4.2.4) |
| LS0213 | `var`/`let` multiple declaration: missing initializer or initializers of different types |
| LS0214 | C#-style multiple declarators (`int32 a = 1, b = 2;`) |
| LS0231 | `readonly` field assigned outside a constructor/initializer (§4.5.2) |
| LS0301 | Illegal mixed-sign operands (§5.3) |
| LS0401 | Declaring `operator !=` (§5.6, §8.10) |
| LS0501 | `unchecked` outside an unsafe context (§5.7) |
| LS0601 | Invalid expression statement |
| LS0602 | Label not followed by a loop statement (§6.7) |
| LS0603 | `break`/`continue` label unknown or not visible (including a labeled `break` in a `switch` outside any labeled loop) |
| LS0604 | Label does not name an enclosing loop |
| LS0605 | `break`/`continue` outside a loop (or `switch` for unlabeled `break`) |
| LS0610 | `break`/`continue`/`return` leaving a `finally` block or crossing a lambda boundary |
| LS0620 | Empty `switch` section (§6.8) |
| LS0622 | Switch label subsumed by an earlier label |
| LS0630 | Not all code paths return a value |
| LS0640 | Ambiguous `IEnumerable<T>` in a `for`-in loop (§6.6) |
| LS0701 | Escape of a scope-bound or `readonly` reference (§14.5, §14.7) |
| LS0702 | Calling a `this`-escaping method on a scope-bound receiver |
| LS0720 | `out` parameter not assigned on every normally-returning path |
| LS0730 | Capturing `ref`/`in`/`out` parameter in a lambda |
| LS0740 | Positional argument after named argument |
| LS0750 | Ambiguous overload call |
| LS0751 | No applicable overload |
| LS0801 | `new` applied to a struct (or a struct used as a `new()`/`scope()` target) |
| LS0802 | Target-typed `new()`/`scope()`/`new [n]` where the type cannot be inferred (§5.14) |
| LS0803 | `scope` used where there is no enclosing scope (field initializer, constant context) |
| LS0810 | Non-nullable reference declaration without a value (local, field, constructor path) (§4.3, §8.3.1) |
| LS0811 | No accessible base constructor |
| LS0812 | `this` used in a constructor before the class's non-nullable fields are assigned (§8.3.2) |
| LS0820 | Assignment to receiver state in non-`mut` method |
| LS0821 | `mut` method called from non-`mut` method on receiver-rooted path |
| LS0823 | `mut` method called on an immutable receiver (`let`, `in`, `readonly` field, temporary) |
| LS0824 | `mut` written on an interface member |
| LS0830 | Deriving from a non-`virtual` class |
| LS0831 | `virtual` member in a non-`virtual` class |
| LS0832 | `override` without a matching virtual member |
| LS0833 | Missing `override` for an inherited virtual member |
| LS0834 | Instantiating an abstract class |
| LS0835 | Non-virtual class leaves abstract members unimplemented |
| LS0836 | `base` call to an abstract member |
| LS0840 | Inconsistent accessibility |
| LS0900 | Recursive struct containment |
| LS0902 | Struct field of any non-nullable reference type, including `string` (§9.1.1) |
| LS1001 | Missing interface member implementation |
| LS1101 | Type arguments cannot be inferred |
| LS1102 | Type argument violates a constraint |
| LS1301 | `null` assigned to non-nullable type |
| LS1302 | Nullable to non-nullable without proof/assertion |
| LS1303 | Member access (or index, invoke, iteration) on a nullable value that may be or is null |
| LS1501 | Array whose element type is not default-constructible created without initializers |
| LS1502 | Array covariance attempted |
| LS1503 | Rectangular array with more than three dimensions |
| LS1504 | Collection-initializer length does not match the stated size, or ragged rectangular initializer |
| LS1505 | Allocation expression used with an inline-array initializer (§15.9) |
| LS1510 | Invalid inline-array element type |
| LS1701 | Lambda without target delegate type |
| LS1801 | Unreachable `catch` clause |
| LS1901 | `unsafe` used while unsafe code is disabled |
| LS1902 | Illegal address-of target |
| LS1903 | Pointer to a disallowed type (§19.2) |
| LS2001 | Ambiguous name from imported namespaces |
| LS2101 | Extern method with non-ABI-compatible type |
| LS2201 | Missing or duplicate entry point in an executable |

## 23.3 Required Warnings

| Code | Condition |
|---|---|
| LS3001 | Unreachable code |
| LS3002 | Member hides an inherited non-virtual member |
| LS3101 | `is` test whose result is a compile-time constant |
| LS3103 | `!` applied to a value already known to be non-null |
| LS3301 | Potential strong reference cycle (§14.8) |

Optional (implementation-defined) warnings include unused variables and unused loop labels.

## 23.4 Error Recovery

Parsing and binding SHOULD recover after an error so that multiple diagnostics are reported in one run. A compiler MUST NOT report cascading errors for names it has already reported as undefined.

---

# 24. Features Excluded From This Version

## 24.1 Features that will never be part of the language

These features are **not planned** and are not reserved as keywords. A program using their syntax is simply a syntax error.

| Feature | Notes |
|---|---|
| Events (`event`) | Libraries may provide event systems; delegates remain |
| Weak / unowned references | Strong reference counting only |
| Garbage collection | Never |
| `partial` types | |
| `record` types | Only `class`, `struct`, `enum`, `interface` and `delegate` exist |
| `params` | Replaced by `T... name` (§7.4) |
| `sealed`, `abstract` | Replaced by `virtual class` and body-less `virtual` methods (§8.6) |
| `goto` and arbitrary labels | Only loop labels exist (§6.7) |
| `fixed`, `stackalloc` | No pinning is needed (§19.4); use `scope` arrays or inline arrays |
| `dynamic` | |
| LINQ query syntax | |
| Tuple syntax and deconstruction | Not part of the language. Tuple-like types may be provided by the standard library, with no special syntax or compiler-known semantics |
| `using` statement | There is no disposable protocol; finalizers are deterministic (§8.4) |
| Default interface methods | |

## 24.2 Features deferred to later versions

| Feature | Notes |
|---|---|
| `async` / `await`, threading primitives, `lock`, atomics, memory model | `async`, `await`, `lock` are reserved words. Reference counting is thread-safe (§27.8) |
| Attributes | The syntax `[ … ]` before a declaration is reserved |
| Reflection, `typeof`, `GetType` | Runtime type information beyond `is`/`as`/casts is not exposed |
| Multicast delegates | Delegates are single-target (§17) |
| Extension methods | |
| User-defined implicit/explicit conversions | |
| `ref` locals, `ref` returns, `ref` fields, `ref struct` | |
| Ranges/indices, slicing, spans | `...` may move to a read-only span later |
| Rectangular arrays above three dimensions | |
| Nested namespace blocks, `using static`, global usings | |
| Generic variance (`in`/`out`), generic virtual methods, static abstract interface members | §12.6 |
| Exception filters (`when` on `catch`) | `when` is only used in `switch` guards |
| Function pointers, unmanaged callbacks, explicit calling conventions, struct layout control, dynamic library loading | §21.2 |
| Manual unmanaged memory allocation (`NativeMemory`) | §19.7 |
| Local functions, object initializers, `init` accessors, interpolated and raw string literals | |
| A preprocessor, and with it in-source diagnostic suppression | |
| The standard library beyond the compiler-known types (§1.7) | Defined separately |

---

# PART II: RUNTIME

Part II specifies the **runtime**: the code and conventions every compiled LumaSharp program relies on at run time. Part I defines what programs mean; Part II defines how a conforming implementation makes those meanings real: program startup and shutdown, memory management, object representation, execution of language constructs, numeric behaviour, exceptions, and native interop.

Each rule is either **normative** (a conforming implementation MUST behave this way, observably) or a **reference design** (labelled *Reference* or *Recommended*): a concrete choice that independent compiler and runtime implementations SHOULD follow so that they interoperate, but which a single implementation may replace as long as every observable rule still holds. The reference designs are collected in Appendix E.

# 25. Runtime Overview

## 25.1 Role of the Runtime

The runtime is a small library that is **statically linked into every executable** (and made available to static libraries) together with compiler-generated code. It provides:

- program startup, the entry-point call, and shutdown (Chapter 26);
- heap allocation, deterministic reference counting and object destruction (Chapter 27);
- type descriptors, type tests, dispatch support, strings, arrays, boxes, delegates (Chapter 28);
- static initialization, exceptions, and fatal-error handling (Chapters 29, 31);
- the support routines called by compiler-generated code for checked operations (Chapter 30);
- the glue for native calls (Chapter 33).

There is **no garbage collector, no virtual machine, no JIT, and no interpreter**. Compiled code runs directly on the machine. The runtime is itself intended to be written in LumaSharp, with the help of a minimal set of compiler intrinsics (Chapter 32).

## 25.2 Platform Assumptions

A conforming implementation targets a platform with:

- 8-bit bytes; two's-complement integers; IEEE 754 binary32 and binary64 floating point;
- 32-bit or 64-bit flat addresses (pointer size 4 or 8 bytes);
- atomic read-modify-write operations on 32-bit integers (needed for reference counts);
- a C-compatible calling convention for native interop (Chapter 33).

x64 and ARM (32-/64-bit) are the primary targets; any LLVM target meeting these assumptions is permitted. Multi-byte values use the **target's native byte order**. All layouts in this document are expressed in terms of the target's C ABI sizes and alignments.

## 25.3 Terms Used in Part II

| Term | Meaning |
|---|---|
| **Managed object** | A heap- or scope-allocated object with an object header: class instances, arrays, strings, boxes, delegates, closure environments |
| **Object header** | The runtime-owned prefix of every managed object holding at least its type and state (§27.2) |
| **Type descriptor** | A runtime record describing one concrete type (§28.6) |
| **Strong count** | The number of storage locations holding a reference to a counted object (§27.4) |
| **Counted / uncounted object** | Heap objects are counted; *immortal* and *scoped* objects are uncounted (reference-count operations on them do nothing) |
| **Destroying state** | The state of an object from the moment its count reaches zero until its storage is freed (§27.5) |
| **Fatal error** | An unrecoverable condition ending the process (§26.5) |
| **Reference ABI** | The reference object layout and runtime-interface functions of Appendix E |

## 25.4 Conformance of a Runtime

A runtime conforms if every program that conforms to Part I behaves as Part I and Part II prescribe: allocation, counts, destruction order, finalizer timing, exceptions, static initialization order, string behaviour, and numeric results are as specified. The layout and function names of Appendix E are **not** required for conformance unless two separately built components (for example a static library and an executable) must be linked together; in that case they MUST agree on one ABI version (the Compiler Specification defines ABI versioning).

---

# 26. Program Lifecycle

## 26.1 Startup Sequence (Executables)

When the operating system starts an executable, the following steps happen **in order**, before any user code:

1. **Platform entry.** The native process entry point (generated by the compiler/linker) captures `argc`/`argv` and calls the runtime start routine (`luma_start`, Appendix E).
2. **Runtime initialization.** The runtime initializes the allocator, the main-thread state, and its internal tables.
3. **Immortal objects.** The runtime creates (or statically embeds) the objects that must exist for the whole program: the **shared empty string** (§28.10.2), the **pre-allocated `OutOfMemoryException`** (§31.6), and the string literals of the program (§28.10.1). These are *immortal* (§27.2).
4. **Arguments.** If the entry point takes `string[] args`, the runtime builds the array from the process arguments, **excluding the program name**, as UTF-8 strings. Otherwise no array is built.
5. **Entry-type initialization.** The static initialization of the type that declares `Main` is triggered as part of calling `Main` (§26.3).
6. **Entry call.** `Main` is called on the main thread.

No user-defined code runs before step 5, and no user code runs before the entry type's initialization has completed.

## 26.2 Entry Point Invocation and Exit Code

- `Main` is invoked exactly once. Its return value (`int32`) or `0` (for `void`) is the **process exit code** (platform exit codes are truncated to the platform's range).
- When `Main` returns normally, its scope-exit actions run first (locals released, scoped objects destroyed, §14.6), then the `args` array is released, then the process exits (§26.4).
- An exception that leaves `Main` unhandled is an *unhandled exception* (§26.5).

## 26.3 Entry-Type Initialization Failure `[P-13]`

Calling `Main` is a static method invocation, so it triggers initialization of the entry type (§22.2). If that initialization throws:

1. The runtime **does not call `Main`**.
2. The exception cannot be handled by user code (none is running).
3. The program terminates as a fatal error (§26.5) of kind *unhandled exception*, and the report names the `TypeInitializationException` **and its inner exception** (the original failure) so that the cause is visible. The inner exception is the primary cause.

This is the only situation in which a program can crash without user code having the opportunity to handle the exception. The runtime start routine is responsible for catching the exception at this boundary; compiler-generated code does not need to inject any special handling into user methods (the initialization check may be placed in the startup stub or in `Main`'s prologue; see §22.2.1).

## 26.4 Normal Termination `[P-14]`

After `Main` returns, the runtime terminates the process by calling the platform's normal process-exit routine (C `exit`) with the exit code, so that standard platform exit handlers run.

- Objects still referenced from **static fields** are *not* released and their **finalizers do not run**. Statics live for the program's lifetime.
- Objects in unreclaimed reference cycles are not finalized (§27.9).
- Anything reachable only from `Main`'s locals *was* released when `Main` returned (so those finalizers ran, in the deterministic order of §14.6).
- Flushing of library-level buffered output is the responsibility of the standard library (for example through the platform exit handlers); the runtime itself holds no user-visible buffers.

## 26.5 Fatal Errors `[P-14]`

A **fatal error** ends the process immediately. The runtime:

1. writes a diagnostic to the standard error stream (kind, message, and for unhandled exceptions the exception chain; stack trace if the implementation has one);
2. terminates the process abnormally with a **non-zero** exit status (implementation-defined; *Recommended*: the platform's abort status, e.g. 134 on POSIX);
3. runs **no** scope-exit actions, **no** finalizers, and no further user code.

| Fatal error kind | Cause |
|---|---|
| Unhandled exception | An exception for which no handler exists on the stack (including entry-type initialization failure, §26.3) |
| Exception escaping a finalizer | A finalizer threw (§8.4) |
| Exception crossing a native frame | An exception tried to unwind through native code (§33.5) |
| Stack overflow | The thread's stack is exhausted (§31.8) |
| Reference-count overflow | A strong count would exceed its maximum (§27.4) |
| Heap corruption detected | Runtime-detected inconsistency (best effort) |
| Explicit runtime failure | The runtime library signalled an unrecoverable condition |

Fatal errors cannot be caught. Because a handler search precedes any cleanup (§31.3), an unhandled exception terminates the process **without** unwinding.

## 26.6 Static Libraries and Hosting

A **static library** has no entry point and no startup sequence. When an executable links it, the executable's startup (§26.1) initializes the runtime once for the whole program, and the library's types are initialized lazily like any other (§22.2). A native, non-LumaSharp host calling into a LumaSharp library is out of scope for this version (there is no language-level export mechanism), but a runtime SHOULD expose its initialization routine (`luma_runtime_init`, Appendix E) so that such hosting can be added.

---

# 27. Memory Management

## 27.1 Managed Objects and the Heap

Every class instance, array, string, boxed value, delegate and closure environment is a **managed object**. Managed objects live either on the **managed heap** (allocated by `new`, boxing, closures, string creation) or are **scoped** (allocated by `scope`, §27.6). Both kinds use the same object representation; they differ only in who decides when they die.

Value types (primitives, structs, enums, nullable values, pointers, inline arrays) are *not* managed objects: they live inline in locals, fields, array elements and boxes, and are copied by value. A struct may contain nullable reference fields, which are ordinary counted references; non-nullable reference fields, including `string`, are prohibited (§9.1.1).

Unmanaged memory (untracked, manually freed) does not exist in this version (§19.7).

## 27.2 Object Header and Immortal Objects `[P-22]`

Every managed object starts with an **object header** holding at least:

| Field | Requirement |
|---|---|
| Type | Identifies the object's type descriptor (§28.6). |
| Strong count | An unsigned integer of at least 32 bits, modified atomically (§27.8). Initial value 1 for a heap object (the reference held by the creating expression). |
| State flags | At least: **immortal**, **scoped**, **destroying**. |

*Reference layout:* `{ TypeDescriptor* type; uint32 strong; uint32 flags; }` (16 bytes on 64-bit targets, 12 on 32-bit), with the payload following at the next offset aligned to the payload's alignment (minimum 8).

**Immortal objects** are objects that are never destroyed: the shared empty string, string literals, the pre-allocated `OutOfMemoryException`, and static type data. Retain and release on an immortal object do nothing. They carry the *immortal* flag and are typically placed in read-only or static memory.

## 27.3 Allocation

- Every allocation is **zero-filled** before use. All-zero is the default value of every value type and nullable type. Safe code cannot read an uninitialized non-nullable reference slot; such storage must be assigned before use.
- Payload alignment is at least the alignment of the object's most strictly aligned field and at least 8 bytes (*Recommended*: 16 on 64-bit targets).
- If the system cannot satisfy an allocation, the runtime throws `OutOfMemoryException` (§31.6); it does **not** terminate the process. The same applies when a requested size overflows the address space or the implementation's maximum object size.
- A newly allocated heap object has strong count **1**, owned by the allocating expression. The expression's result is an *owned* reference: the compiler either moves it into its destination (variable, field, argument slot) or releases it at the end of the full statement (§14.2.1).

## 27.4 Retain and Release

**Retain** increments the strong count of a *counted* object (no effect on a null reference, an immortal object, or a scoped object). **Release** decrements it, and if the count reaches zero **destroys** the object (§27.5).

- Retain/release are inserted by the compiler according to the observable rules of §14.2; the programmer never writes them.
- *Reference ordering:* retain uses a relaxed atomic increment; release uses an atomic decrement with *release* ordering, followed by an *acquire* fence when the old value was 1, before destruction begins.
- The strong count has a maximum (at least 2^31 − 1). A retain that would exceed it is a **fatal error** (reference-count overflow).
- A count never becomes negative: releasing more times than retaining is a compiler or unsafe-code bug (undefined behaviour in unsafe code; a runtime MAY detect it and report a fatal error).
- Retain or release on an object in the **destroying** state is legal (for example when a finalizer passes `this` to a `readonly` parameter and the compiler conservatively retains it); such operations adjust the count but MUST NOT start a second destruction. When destruction completes, any outstanding count is irrelevant because the storage is freed (safe code cannot have kept a reference, §8.4).

## 27.5 Destruction Protocol

When a release brings a counted object's count to zero, the runtime executes, in this exact order:

1. **Enter the destroying state** (set the *destroying* flag).
2. **Finalizers.** If the object's class chain declares finalizers, run them from the **most-derived class to the base class**. (After a failed construction, only the finalizers of classes whose constructors completed run, and the dynamic type is lowered as in §8.4.1.) A finalizer that throws is a fatal error (§26.5).
3. **Release members** in **reverse declaration order**: for a class instance, the reference fields of the most-derived class (last declared first), then each base class's; for an array, elements from the last index to the first (struct elements through their own reference fields, in reverse); for a box, the references inside the boxed struct (reverse declaration order); for a closure environment, the captured variables in reverse order of declaration; for a delegate, its target. Null references and immortal objects are skipped. Each release may itself trigger destruction, recursively.
4. **Free the storage.**

**Order guarantee.** The sequence of finalizer invocations and member releases that results from one release MUST be the same as if the above steps were executed by direct recursion. Implementations SHOULD NOT consume unbounded native stack for long chains (for example a linked list of one million nodes): they MAY use an explicit stack or worklist **provided the order is identical to recursive execution**.

```lumasharp
class Node
{
    public Node? Next;
    ~Node() { Print("node"); }
}
// a -> b -> c, only 'a' referenced. Releasing a prints node, node, node:
//   a's finalizer; a's fields released -> b's finalizer; b's fields released -> c's finalizer.
```

A destroyed object's storage may be reused immediately. Safe code can never hold a reference to it.

## 27.6 Scoped Objects `[P-10]`

A `scope` allocation (§14.4) is a managed object with the **scoped** flag set. It is **not reference counted**: retain/release on it do nothing, and its lifetime is determined by the compiler-visible scope.

- The compiler emits the **scope-exit destruction** of each scoped object at the end of its block (and on every exit path, including exceptional ones), in reverse declaration order (§14.6).
- Scope-exit destruction runs destruction steps 1–3 of §27.5 (finalizers, member release) and then returns the storage to wherever it came from. For stack- or arena-placed objects this is free.
- The storage MAY come from the machine stack, a per-thread arena, or the heap. The choice is not observable.
- A scoped object may hold counted references (they are released at destruction) and may be passed by reference to code that follows the escape rules of §14.5. Because those rules guarantee no counted holder of a scoped object exists past its scope, no use-after-scope can occur in safe code.
- Scoped arrays and class instances use the same layouts as heap ones (Chapter 28).

## 27.7 Observable Constraints on Optimization

The compiler may elide, merge, or reorder retains and releases (borrowing, move semantics, redundant pair elimination) provided that:

1. **Safe code never observes a destroyed object** (§14.2.2): an object is alive for at least the duration of every use, including arguments for the duration of a call.
2. **Finalizer timing and order are preserved** as stated in §14.2.1 and §27.5: a reference held by a local is released no later than the end of its scope, and the compiler may release it earlier only if this does not change the order or the point at which observable effects (including finalizers) occur relative to other observable effects.
3. Objects whose class (or any contained object's class) has a finalizer are never destroyed earlier than their last use *and* never later than scope end; for such objects the compiler SHOULD simply release at scope end.

## 27.8 Thread Safety of Reference Counting

Reference-count updates MUST be **correct under concurrent access**: if several threads hold separate strong references to one object and copy or release them concurrently, the count stays consistent and the object is destroyed **exactly once**, after the last release, on the thread that performs it. A runtime for a program that can never create threads (this version has no thread-creation facility) MAY use non-atomic operations as long as behaviour is identical; once threads exist all operations MUST be atomic as specified in §27.4.

This does **not** make user data race-free: concurrent unsynchronised access to the *same* reference-typed variable or field is a data race (UB in unsafe code, unspecified in safe code) until synchronisation primitives are specified.

## 27.9 Cycles and Leaks

The runtime performs **no cycle detection**. A cycle of strong references that is no longer reachable from any variable keeps all its members alive until process exit, and their finalizers never run (§26.4). The compiler's cycle warning (§14.8) is an aid only.

## 27.10 Interaction with Unsafe Code

Pointers do not participate in counting (§19.2). Unsafe code that stores a reference-typed value through a raw pointer without the compiler's involvement, or keeps a pointer to a destroyed object, can corrupt the heap; such behaviour is undefined, and the runtime need not detect it.

---

# 28. Type Representation

## 28.1 Primitive Types

Fixed-width primitive sizes are fixed; `int` and `uint` use the native pointer size. Alignment is the target C ABI alignment of the corresponding C type (normally equal to the size).

| Type | Size | Representation |
|---|---|---|
| `bool` | 1 | `0` = false, `1` = true. Any other bit pattern (possible only through unsafe code) is undefined behaviour when read |
| `char` | 2 | unsigned 16-bit UTF-16 code unit |
| `int8`/`uint8` | 1 | two's complement / unsigned |
| `int16`/`uint16` | 2 | two's complement / unsigned |
| `int32`/`uint32` | 4 | two's complement / unsigned |
| `int64`/`uint64` | 8 | two's complement / unsigned |
| `int` | native pointer size | native-width two's complement signed integer |
| `uint` | native pointer size | native-width unsigned integer |
| `float` | 4 | IEEE 754 binary32 |
| `double` | 8 | IEEE 754 binary64 |
| pointers (`T*`) | pointer size | address; `null` = 0 |
| references | pointer size | §28.5 |

## 28.2 Struct Layout

- Fields are laid out **sequentially in declaration order**. Each field is placed at the next offset that satisfies its alignment; the struct's alignment is the maximum of its fields' alignments; its size is rounded up to a multiple of its alignment. Nested structs and inline arrays are laid out in place.
- A struct with no fields has size 1.
- For a struct whose fields are all **unmanaged** (no references, strings, delegates) this layout is **normative** and matches the C struct declared with the same field types, so it can be passed to native code (§33). Structs containing references have an implementation-defined layout (*Recommended*: the same rule).
- Struct copies are bitwise copies plus a retain of each contained reference; destroying a struct value releases them (§9.5, in reverse declaration order).
- Class instance payloads use the same sequential rule, base-class fields first, then derived-class fields; the exact payload layout is implementation-defined (it is observable only to unsafe code).

## 28.3 Enums

An enum is stored as its underlying integer type (§11.1) with the same size and alignment.

## 28.4 Nullable Types

- **Nullable reference** `T?`: the same representation as `T`; the null reference (address 0) means "no value".
- **Nullable value** `T?`: **implementation-defined representation** (for example `{ bool hasValue; T value; }`, a niche, or a tagged union). The language has no `Nullable<T>` library type and none may be required. One requirement is **normative**: **the all-zero bit pattern MUST represent `null`** for every nullable type, so that zeroed memory (arrays, struct defaults, fresh objects) is a valid null.
- A nullable value is never passed to native code (§21.1).
- Boxing: §28.9.

## 28.5 References

A **reference** is a pointer-sized value identifying a managed object (*Reference layout*: the address of its object header). `null` is address 0 and is representable only for nullable reference types. An **interface reference** has the *same representation* as an object reference (no separate interface pointer); dispatch finds the implementation through the object's type descriptor (§28.7). A reference to a scoped object has the same representation as any other.

## 28.6 Type Descriptors

Every concrete type that needs run-time identity has exactly one **type descriptor** in the running program: every class, struct and enum (needed for boxing and tests), interface, delegate type, array type (element type and rank), and every distinct **generic instantiation**. Type identity at run time is **descriptor identity**; linking MUST merge duplicate descriptors for the same type (for example the same instantiation emitted by two compilation units).

A descriptor provides at least:

| Content | Purpose |
|---|---|
| Kind (class, struct, enum, interface, delegate, array, primitive, pointer) | Test and dispatch logic |
| Fully qualified name (UTF-8, NUL-terminated) | Default `ToString`, diagnostics, error reports |
| Instance size and alignment (element size for arrays) | Allocation, boxing |
| Base type | Casts |
| Implemented interfaces (with dispatch tables) | Interface calls, `is`/`as` |
| Virtual table | Virtual dispatch |
| Finalizer entry (if the class declares one) | Destruction |
| **Reference map**: offsets/kinds of every reference slot (including those inside nested structs), in release order | Destruction, copying, boxing |
| Element type, rank (arrays); target type (pointers) | Tests, allocation |
| Generic type arguments (instantiations) | Diagnostics, identity |
| Static-initialization state and storage for the type's statics | §29.4 |
| Flags (has finalizer, has references, abstract, ...) | Fast paths |

*Reference layout:* Appendix E.1.

## 28.7 Virtual and Interface Dispatch

### 28.7.1 Virtual dispatch

Each class has a **virtual table** whose first entries are those of its base class; a new `virtual` method gets a new slot and an `override` reuses its overridden slot. A virtual call loads the object's descriptor from the header and calls through the slot. Non-virtual calls, calls to `base.M()`, struct calls and sealed-class calls (any class without `virtual`) are direct. Because a non-`virtual` class cannot be inherited, calls through its type are always direct.

### 28.7.2 Interface dispatch

An interface call finds, in the object's descriptor, the dispatch table for the interface and calls the slot. The lookup strategy (search, hashing, caching, thunks) is implementation-defined and not observable. Structs reach interface calls only after boxing, except in generic code constrained to the interface where the compiler may call the struct's method directly.

### 28.7.3 Dispatch during construction

While a constructor chain runs (§8.3.2) the object's dynamic type is the class whose constructor is currently executing. The runtime interface provides a way to set the object's dynamic type (`luma_set_type`, Appendix E): a constructor sets its own class on entry after allocation (allocation sets the type to the most-derived class), sets the base class before calling `base(...)`, and restores its own class when `base(...)` returns. After the outermost constructor returns, the type is the most-derived class. A compiler MAY omit these updates for classes where no virtual call, interface call, type test, or `scope` receiver check could observe the intermediate type. During destruction after a failed construction, the dynamic type is set to each class before its finalizer runs (§8.4.1).

## 28.8 Run-Time Type Tests

`is`, `as`, casts and `catch` clauses use these rules on the **dynamic type** `D` of a non-null object:

| Target | Succeeds when |
|---|---|
| Class `C` | `D == C` or `C` is an ancestor of `D` |
| Interface `I` | `D` (or an ancestor) implements `I` or an interface that inherits `I` |
| `object` | always |
| Array `T[]`, `T[,]`, … | `D` is the array type with *exactly* element type `T` and the same rank (invariance) |
| Delegate type | `D` is exactly that delegate type |
| Value type `V` (unboxing) | `D` is the box type of exactly `V` (a boxed enum only matches its enum type; boxed `int32` does not match `int64`) |

A `null` reference fails every test (`is`, `as`), is returned unchanged by `(T?)` casts to nullable targets, and makes a cast to a non-nullable target throw `NullReferenceException`.

## 28.9 Boxes

A **box** is a managed object whose type descriptor is the boxed value type's and whose payload is a copy of the value. Boxing allocates a box (counted, count 1) and copies the value (retaining any contained references). Unboxing checks the descriptor exactly (§28.8) and copies the value out (retaining references), throwing `InvalidCastException` on a mismatch. Boxing a nullable value with no value yields `null`; with a value, a box of the underlying type. Boxes of structs are destroyed by releasing the contained references (§27.5).

## 28.10 Strings

### 28.10.1 Layout and kinds

A string is a managed object conceptually holding **{ data pointer, character set, length }** and flags.

*Reference layout:* `{ ObjectHeader h; const uint8* data; int32 length; int32 byteLength; uint8 charset; uint8 flags; }`, where `length` is the number of **UTF-16 code units** (§16.3), `byteLength` is the number of bytes of the stored text *excluding* the terminator, and `flags` includes: **OWNS_DATA** (the runtime frees `data` or the inline text on destruction) and **ASCII_ONLY** (every code unit is below 0x80; indexing is O(1)).

There are two kinds:

- **Static strings** (literals): the text lives in read-only program data; the object is *immortal*; `data` points at that text.
- **Owned strings** (results of operations such as concatenation and `Substring`): the text lives in memory owned by the string (inline after the fields, or in a separate block) and is freed when the string is destroyed.

Strings are immutable: neither the text nor `length`/`charset` ever changes after creation. Strings never share a mutable buffer.

### 28.10.2 Empty string literals

The empty string literal `""` is an ordinary immutable string value and may be stored as a literal without a per-use allocation. It is not the default value of a `string` storage location. Non-nullable string locals, fields, array elements, and generic arguments must receive an explicit non-null value before they can be read. Nullable `string?` storage defaults to null. Literal pooling MAY share identical string literals.

### 28.10.3 Character sets

| Id | Charset | Requirement |
|---|---|---|
| 0 | UTF-8 | **Mandatory.** Default for literals and library-produced strings. A terminating byte `0` follows the text and is **not** counted in `length` or `byteLength` |
| 1 | UTF-16 (native endian) | Optional; a 16-bit terminator is *recommended* |
| 2.. | Others (for example ASCII, Latin-1, UTF-32) | Optional / reserved |

Whatever the charset, the **logical content** of a string is a sequence of `char` values (UTF-16 code units); `Length`, indexing, equality and concatenation are defined on that view (§16). A text may contain the character `'\0'` anywhere; `length`/`byteLength` count it, while the terminator is the extra byte beyond the end. Safe code and the standard library MUST NOT create a UTF-8 string containing invalid UTF-8.

### 28.10.4 Length and indexing

`Length` returns `length`. For `ASCII_ONLY` strings and for UTF-16 strings without surrogate-affecting positions, `s[i]` is O(1). For other UTF-8 strings the cost of `s[i]` is implementation-defined (it may scan, or use a cached index), but the result is always the UTF-16 code unit at logical position `i`, and out-of-range indices throw `IndexOutOfRangeException`.

### 28.10.5 Equality and concatenation

`==` compares logical content (same length and equal code units), independent of charset, and MAY compare bytes directly when both strings are UTF-8. `a + b` returns a **new owned** string with the concatenated content (the charset of the result is implementation-defined, UTF-8 recommended), or one of the operands unchanged if the other is the empty string (the result may *be* the same object; strings are immutable so this is unobservable except by reference comparison, which is unspecified for non-empty strings).

### 28.10.6 Sharing with native code

For a UTF-8 string the `data` pointer refers to NUL-terminated text that is valid as long as the string is alive; this is what allows C-style APIs to share the buffer. The standard library may expose it in a future facility; the language provides no direct syntax in this version.

## 28.11 Arrays

An array is a managed object holding its dimension lengths and then its elements.

*Reference layout:*

```
{ ObjectHeader h; int32 rank; int32 length /*total elements*/; int32 dims[rank] /* rank > 1 only */;
  <padding to element alignment, minimum 8>  elements... }
```

Normative:

- The elements of a **one-dimensional array are always contiguous** in index order, with no per-element headers; arrays of structs hold the structs inline.
- **Multi-dimensional arrays** (`T[,]`, `T[,,]`): the implementation chooses the layout, but MUST support `Length`, `Rank`, `GetLength`, **per-dimension bounds checking**, and logical row-major iteration. *Recommended*: a single contiguous block in row-major order (last index fastest), so `&m[0,0] + k` addresses the k-th element in row-major order in unsafe code; programs relying on this are non-portable.
- **Jagged arrays** are arrays of references to independent arrays.
- A new array's elements are zero-filled, which represents `0`/`null`/`default` for value types and nullable elements. Arrays of non-nullable reference elements are only created from a complete initializer (§15.2), so no element is ever observed as null.
- The maximum element count is at least 2^31 − 1 per the `int32` length; creating an array whose total byte size overflows or exceeds available memory throws `OutOfMemoryException`; a negative dimension throws `OverflowException`.
- Element destruction order: §27.5.
- A **scoped array** has the scoped flag and the same layout.

## 28.12 Inline Arrays

An inline array `T[N]` is `N` elements stored contiguously, in index order, **at the position of the field or local** that declares it, with the layout of a C array `T[N]`. It has no header, no length field and no counting (its elements' references, if any, are counted individually). Its size is `N * sizeof(T)`; its alignment is that of `T`. It is part of the containing struct/class/stack frame and copies with it.

## 28.13 Delegates

A delegate value is a managed object *{ header; function pointer; target reference }* (*Reference layout*), immutable after creation.

- **Static method / non-capturing lambda:** `target` is null and the function takes no receiver.
- **Instance method group:** `target` is the receiver (retained). For a **struct** receiver the target is a **box** holding a copy of the struct.
- **Capturing lambda:** `target` is the closure environment object (§28.14) and the function receives it as its first argument.
- Invocation calls `function`, passing `target` first when it is non-null, followed by the arguments.
- Two delegates are `==` iff their functions are equal and their targets are the identical object (or both null).
- Destroying a delegate releases its target.
- A delegate holds exactly one target: there are no multicast lists.

## 28.14 Closure Environments

A lambda that captures variables causes the compiler to allocate a **closure environment**: a managed object with a compiler-generated type descriptor and one slot per captured variable (including `this`).

- A captured variable **lives in the environment** for its whole life, and both the declaring method and every lambda that captures it access that single storage location (shared by reference, §17.4). For captured reference variables the environment owns the variable's reference.
- An environment is created when execution enters the scope that declares the first captured variable of that scope. A variable declared inside a loop body is a **new variable for each iteration** (a new environment per iteration); the variable declared in a C-style `for` header is **one** variable for the whole loop; a `for`-in loop variable is new each iteration.
- Environments are counted objects (or scoped, when the closure is scope-bound, §14.5) and are destroyed by §27.5.

## 28.15 Generic Instantiation

How generics are implemented (monomorphization, shared code with dictionaries, or a mix) is implementation-defined. The following are normative:

- Each instantiation is a distinct type with its own descriptor (§28.6) when run-time identity is needed (boxing, `is`/`as`, casts, `catch`, array element types).
- **Each instantiation has its own static fields and its own static initialization** (§22.2); `C<int32>.x` and `C<string>.x` are distinct variables.
- Behaviour of generic code is identical for value-type and reference-type arguments; no implicit boxing occurs for generic storage or constrained interface calls.

---

# 29. Execution Semantics

This chapter states how the constructs of Part I execute. Where Part I already gives the rule, it is referenced rather than repeated.

## 29.1 Calls and Parameter Passing

- **Evaluation order:** the receiver, then arguments **left to right as written** (named arguments in written order, §7.3); then the call. Default values of omitted optional arguments are inserted at the call site.
- **By-value parameters:** value types are copied; reference types pass the reference. Passing may be a **borrow** (no count change) or a retain, at the compiler's discretion subject to §27.7.
- **`ref`/`in`/`out`:** the argument is the *address* of the caller's storage. `in` of a small value type may be passed by value instead (not observable, since `in` forbids mutation). `readonly` has no ABI effect: it only constrains the callee body.
- **`out`:** the callee's first assignment replaces the previous content of the storage and releases the previous reference, if any (§7.2.4). The compiler zero-initialises caller storage that was unassigned so that the release is a no-op on null.
- **Return values:** returned references are **owned** (+1) and transfer to the caller. Returned structs are copies.
- **Variadic arguments:** the compiler gathers trailing arguments into a sequence typed as the array (§7.4); the representation (new array, scoped array, stack array, existing array) is the compiler's choice. A single argument convertible to the array type is passed as is, without a copy.
- **Calling convention** between LumaSharp functions is implementation-defined for a given ABI version; the *Reference ABI* (Appendix E) treats reference parameters as **borrowed** (the callee retains if it stores) and returns as **owned**.

## 29.2 Object Creation

`new T(args)` executes in this order:

1. Evaluate `args` left to right.
2. Allocate zeroed storage and initialise the header: type `T`, count 1 (§27.3). If allocation fails, throw `OutOfMemoryException` (no constructor has run).
3. Run the constructor chain (§29.3).
4. If a constructor throws, destroy the object as in §8.4.1 and propagate the exception; no reference to the object ever existed in user code.
5. The result is an owned reference.

`scope T(args)` is identical except that step 2 reserves scoped storage with the *scoped* flag, and the object is registered for destruction at the end of its scope (§27.6). Struct construction `S(args)` allocates nothing: it produces the default value and runs the matching constructor body on it. `new T[...]`, boxing and closure creation follow §28.9, §28.11, §28.14.

## 29.3 Constructor Chain

For class `T` derived from `B`, the constructor chain executes: in `T`'s constructor, the field initializers of `T` (declaration order) → the `: base(...)` constructor of `B` (which itself runs `B`'s initializers, its base, then its body) → `T`'s constructor body. A constructor beginning with `: this(...)` runs the delegated constructor first (which performs the above), then its own body. The dynamic type follows §28.7.3. The null-safety restrictions in a constructor body (§8.3.2) are compile-time only and have no runtime cost.

## 29.4 Static Initialization Protocol

Each type (or generic instantiation) has an **initialization state**: *Uninitialized*, *InProgress(thread)*, *Done*, *Failed(exception)*. Before the operations listed in §22.2 execute, compiler-generated code ensures the type is initialised (a check that may be elided wherever the compiler can prove the state is *Done*, for example inside the type's own members, where initialization has necessarily been triggered already):

```
ensure_initialized(T):
    if state(T) == Done: return
    if state(T) == Failed(e): throw e                       // the same TypeInitializationException object
    lock T
        if state(T) == InProgress(current thread):
            throw TypeInitializationException("circular static initialization of T")
        if state(T) == InProgress(other thread): wait, then re-evaluate
        state(T) = InProgress(current thread)
    try:
        if T has a base class: ensure_initialized(base(T))   // base first
        run static field initializers of T in declaration order
        run static this() of T, if any
        state(T) = Done
    catch e:
        wrapper = new TypeInitializationException(message, inner: e)
        state(T) = Failed(wrapper)
        throw wrapper
```

An exception thrown while *ensuring a base class* is wrapped as a failure of the derived class too. Waiting threads (none exist in this version) wake after completion; detection of cross-thread initialization cycles is implementation-defined. The entry type is initialised by the startup code before `Main` (§26.3).

## 29.5 Conversions

- **Numeric:** as defined in §3.6, §5.3 and Chapter 30.
- **Reference up-conversions** (derived to base, to interface, to `object`) are free (the reference is unchanged).
- **Down-conversions and `as`/`is`:** §28.8; failed casts throw `InvalidCastException`.
- **Boxing and unboxing:** §28.9.
- **Nullable:** `T` → `T?` wraps; `T?` → `T` (cast, `!`) throws `NullReferenceException` when null (§13.4).
- **Enum ↔ integer:** reinterpretation of the underlying integer value with the numeric conversion rules.
- **Pointer conversions:** reinterpretation (§19.3); no checks.

## 29.6 `switch`

Labels are tested in textual order; the first match executes its section and control leaves the `switch`. The compiler may use jump tables or decision trees as long as this behaviour is preserved, including `when` guards evaluated only after their pattern matches, and `string` constant labels compared by content (§16.4). The governing expression is evaluated exactly once.

## 29.7 `for`-in

Arrays are iterated by index (multi-dimensional arrays in row-major order); other collections through `IEnumerable<T>.Iterate()` and `IIterator<T>.Next()`/`Current` as lowered in §6.6. The iterator object is a counted object released when the loop ends by any path.

## 29.8 Multiple Declaration and Assignment

Multiple declarations (§4.2.4) evaluate their initializers left to right and store them in order; remaining declarators receive defaults. Multiple assignment (§4.6.3) evaluates all right-hand sides to temporaries, then target sub-expressions, then assigns left to right; if evaluation of any part throws, no target has been modified. Each assignment to a reference-typed target releases the old value and retains the new one.

## 29.9 Property, Indexer and Operator Calls

Property accessors, indexers and overloaded operators are ordinary method calls (static for operators). `!=` expands to `!(a == b)` with each operand evaluated once, left to right (§5.6). A user-defined `==` on classes is a method call; without one, `==` on classes is reference identity.

## 29.10 Scope-Exit Cleanup

Scope-exit actions (scoped-object destruction, local releases) are emitted for every exit path of a block (§14.6). On the exceptional path they run during the cleanup phase of unwinding (§31.3). There is no other deferred-execution mechanism in the language.

---

# 30. Checked Operations and Numeric Behaviour

This chapter defines the run-time behaviour behind the numeric and safety checks of Part I. The compiler MUST generate code with *exactly* these results, including when it folds constants at compile time.

## 30.1 Integer Arithmetic

- In a **checked** context (the default), `+ - *`, unary `-`, `++`, `--`, compound assignments, and explicit narrowing integer conversions throw **`OverflowException`** when the mathematical result does not fit the result type. Implementations typically use overflow-detecting machine operations (for example LLVM `*.with.overflow` intrinsics).
- In an **unchecked** context (only inside `unsafe`), the same operations produce the result modulo 2^N (two's complement wrap).
- Operands narrower than 32 bits are promoted to `int32` before arithmetic (§5.3); overflow is judged at the promoted type, and the explicit narrowing cast back is what throws.
- **Division and remainder:** a zero divisor throws **`DivideByZeroException`** (always, both contexts). The overflowing case `MinValue / -1` (and `MinValue % -1`) throws **`OverflowException`** (always). Division truncates toward zero; the remainder has the sign of the dividend.
- Unary `-` of `MinValue` overflows (checked) / wraps (unchecked).

## 30.2 Shifts and Bit Operations

Shift counts are masked: `count & 31` for 32-bit (and promoted narrower) operands, `count & 63` for 64-bit operands. `<<` fills with zero; `>>` is arithmetic for signed and logical for unsigned operands. Shifts, `& | ^ ~` never throw.

## 30.3 Floating-Point

- `float` and `double` operations follow **IEEE 754** with round-to-nearest-even and **never throw**. Division by zero gives ±∞ or NaN; overflow gives ±∞.
- The compiler MUST NOT use higher intermediate precision (for example x87 extended precision) or fuse multiply-add, unless the project explicitly enables such an option; each operation's result is as if rounded to the operand type.
- Comparisons involving NaN: `==`, `<`, `>`, `<=`, `>=` are false and `!=` is true.
- `%` is IEEE `fmod` (sign of the dividend).
- **Integer → floating** conversion rounds to nearest even. `float` → `double` is exact; `double` → `float` rounds to nearest even, overflowing to ±∞.
- **Floating → integer** conversion truncates toward zero. If the operand is NaN or the truncated value is outside the target range: in a **checked** context the conversion throws `OverflowException`; in an **unchecked** context the result is **unspecified** (any value of the target type; not undefined behaviour).

## 30.4 Integer ↔ Integer Conversions

Widening conversions preserve value (sign-extension for signed sources, zero-extension for unsigned). Explicit narrowing or sign-changing conversions in a checked context throw `OverflowException` if the value is not representable; in an unchecked context the low-order bits are kept.

## 30.5 `bool` and `char`

`bool` operations produce only `0` or `1`. `char` converts to and from integers per §3.6 (a conversion from an integer that does not fit in 16 bits is an explicit narrowing conversion, checked).

## 30.6 Mandatory Run-Time Checks (Safe Code)

| Check | Failure | Omittable only when |
|---|---|---|
| Null assertion `e!`; cast of null to a non-nullable type | `NullReferenceException` | the compiler proves non-null |
| Array/string index, per dimension | `IndexOutOfRangeException` | proven in range, or inside `unsafe unchecked` |
| Cast / unboxing type test | `InvalidCastException` | proven to succeed |
| Checked arithmetic and numeric conversions | `OverflowException` | proven not to overflow, or `unchecked` |
| Integer division | `DivideByZeroException` / `OverflowException` | proven safe |
| Negative array length | `OverflowException` | never |
| Allocation failure | `OutOfMemoryException` | never |

## 30.7 Constant Folding

A constant expression is evaluated at compile time with the same results as run time. Integer overflow in a constant expression in a checked context is a compile-time error (not a run-time exception); in an unchecked context it wraps. Floating-point constant folding uses the IEEE results above.

---

# 31. Exceptions

## 31.1 Model

An exception is a managed object of a class deriving from `Exception`. Throwing transfers control to the nearest matching handler, running **cleanup actions** (scope-exit destruction, local releases, `finally` blocks) of every block it leaves. There are no checked exceptions and no resumable exceptions.

The mechanism (zero-cost tables with an unwinder, setjmp/longjmp, explicit return codes, …) is **implementation-defined**; the behaviour below is normative. *Recommended*: LLVM landing pads and an Itanium-style two-phase unwinder, which gives exactly this behaviour.

## 31.2 Throw

`throw e` evaluates `e` (non-null, §18.2), retains it for the duration of unwinding, and raises it. `throw;` in a `catch` block re-raises the *same object* currently being handled. The raising point's stack trace, if the implementation records one, is captured when first thrown.

## 31.3 Two-Phase Behaviour `[P-30]`

1. **Search phase.** From the raising point outward, the runtime looks for a `catch` clause whose type matches the exception's dynamic type (§28.8) (catch-all matches everything). **No user code and no cleanup runs during the search.**
2. **If no handler exists:** the exception is *unhandled*; the process terminates as a fatal error (§26.5, §31.5) **without any cleanup or finalizers**.
3. **Cleanup phase.** If a handler is found, execution unwinds outward to it, running at each block that is left, innermost first: scope-exit destruction of scoped objects and release of locals (§14.6) in reverse declaration order, and `finally` blocks, interleaved in block order. Destruction may run finalizers.
4. **Handler entry.** Control enters the `catch` block with the exception bound to the catch variable (retained; released when the block is left).

## 31.4 Exceptions During Unwinding

- If a `finally` block or a cleanup action throws a *new* exception while one is unwinding, unwinding of the first is abandoned: the **new exception replaces it** (the old one is released) and a **new search phase begins** from the point of the new throw.
- A finalizer that throws (including during unwinding) is a fatal error.
- An exception thrown inside a `catch` block propagates outward after the `try`'s `finally` runs.

## 31.5 Unhandled Exceptions

Fatal termination (§26.5) with a report on standard error that includes, for the exception and each `InnerException`: the type name (from the type descriptor), the `Message`, and a stack trace if available. The report format is implementation-defined. Exit status: non-zero (implementation-defined).

## 31.6 Exceptions Raised by the Runtime and Compiled Code

Compiled code and the runtime raise the exceptions of §18.5 by creating an instance of the named class through its `this(string message)` constructor (message text is implementation-defined) and throwing it. Because creating the object needs memory:

- **`OutOfMemoryException`** is raised using a **pre-allocated immortal instance** created at startup, so raising it never allocates. (Repeated out-of-memory conditions throw the same instance.)
- If creating any *other* runtime exception itself fails for lack of memory, `OutOfMemoryException` is raised instead.

## 31.7 Exceptions and Reference Counting

The exception object is a counted object. The runtime holds one reference from the throw until the handler's catch variable takes over; the catch block releases it on exit; a rethrow transfers it back; replacement releases the old exception. An exception object may be held longer by user code (stored in a field, for example).

## 31.8 Other Fatal Conditions

- **Stack overflow** is a fatal error (not an exception and not catchable). An implementation SHOULD detect it (guard page) and report it before terminating; behaviour on platforms where detection is impossible is unspecified.
- **Out-of-memory in the runtime itself** (for example while recording a stack trace) must not lose the original exception: the runtime drops the trace instead.

## 31.9 Stack Traces

Recording and printing stack traces is **optional and implementation-defined**; no language construct depends on it.

---

# 32. Runtime Bootstrapping and Compiler-Known Types (Informative)

This chapter is **informative**: it records the intended construction of the runtime so that an implementation can be bootstrapped, without constraining observable behaviour.

## 32.1 Goals

The core language knows as few library types as possible (§1.7). The runtime and standard library are meant to be **written in LumaSharp itself**, compiled by the same compiler. To make that possible without a pre-existing runtime, the compiler provides a small set of **intrinsics**.

## 32.2 Stages

| Stage | Contents |
|---|---|
| **0 — intrinsic runtime** | The compiler emits the runtime core directly (or links a minimal native shim written in a systems language): allocator calls, atomics, `memcpy`/`memset`, the program entry stub, type-descriptor emission, string-literal emission, and the unwinder hooks |
| **1 — runtime in LumaSharp** | Allocation wrappers, `luma_retain`/`luma_release`/`luma_destroy`, type tests, string and array support, static-initialization protocol, exception raising and fatal-error reporting are written in LumaSharp (inside `unsafe` code), using only the stage-0 intrinsics |
| **2 — standard library** | Collections, I/O, formatting, and the compiler-known types of §1.7 (`Exception`, `IEnumerable<T>`, …) in ordinary LumaSharp |

## 32.3 Intrinsic Operations the Compiler Must Provide

| Intrinsic | Purpose |
|---|---|
| Raw memory allocate/free/zero/copy | Backing the heap (calls into the platform C library) |
| Atomic load / store / add / sub / compare-exchange / fence | Reference counting, static-initialization locks |
| Overflow-checked arithmetic and conversions | Chapter 30 |
| Type-descriptor and string-literal emission | §28.6, §28.10.1 |
| Function-pointer call and delegate invocation | §28.13 |
| Unwinding support (personality, landing pads) | Chapter 31 |
| Program entry stub (calls `luma_start`) | §26.1 |
| Platform calls (`write`, `exit`, `abort`) | Fatal errors, exit |

## 32.4 Compiler-Known Types in the Runtime

The types of §1.7 are defined by the runtime/standard library. Their *layouts* are known to the compiler where the language needs them:

- `string` and arrays: layouts of §28.10 and §28.11 (the compiler emits indexing, length reads and bounds checks);
- `Exception`: its fields are library-defined, but the compiler needs only the type and the `this(string)` constructor;
- `Delegate`: layout of §28.13;
- `object`: the header of §27.2.

---

# 33. Native Interop at Run Time

## 33.1 Extern Calls

A call to a `static extern` method (§21) compiles to a **direct call** of the native symbol using the platform's default C calling convention. The runtime does not intercept, wrap, marshal, or time such calls. Parameters and results have exactly the C representations of §28.1 and §28.2 (structs by the platform C ABI for passing and returning).

## 33.2 Symbol Resolution

The symbol is the method's own name, unmangled (§21.1). It is resolved by the platform linker when the program is linked; an unresolved symbol is a **link-time error** reported by the toolchain (this version has no dynamic library loading).

## 33.3 Data Passing

- Pointers are passed as raw addresses. The runtime does not track them and does not keep the pointee alive (§19.2).
- Reference-typed values (classes, strings, arrays, delegates) are not passed to native code in this version (§21.1). For strings, the NUL-terminated UTF-8 buffer (§28.10.6) may be handed to C by library code that obtains a pointer to it; the pointer is valid only while the string is alive.
- Memory returned by native code is owned by that code; the runtime never frees it.

## 33.4 Threads and State

Native calls execute on the calling thread with its stack. Native code must not call back into LumaSharp code in this version.

## 33.5 Exceptions and Native Frames `[P-17]`

LumaSharp exceptions MUST NOT propagate through native frames. A runtime that can detect an exception reaching a native frame during the search phase reports a **fatal error** (kind: *exception crossing a native frame*, §26.5). Where detection is impossible the result is undefined behaviour. Since native code cannot call back into LumaSharp in this version, this arises only from a LumaSharp exception thrown on the *far* side of an extern call (not possible today) and is reserved for later versions.

## 33.6 Safety

Because the compiler cannot verify native code, extern calls are made only from `unsafe` contexts (§21.1). Native code may violate any invariant of this document (for example by freeing memory the runtime owns); the consequences are undefined.

---

# Appendix A: Grammar (EBNF)

Notation: `"x"` literal token; `a b` sequence; `a | b` alternative; `[ a ]` optional; `{ a }` zero or more; `( a )` grouping; `x?` / `( … )?` are shorthand for `[ … ]`. Lexical terminals (`identifier`, literals) are defined in §2. This grammar is the syntactic authority; where prose conflicts, the grammar decides syntax and the prose decides meaning.

```ebnf
(* ---------- Compilation unit ---------- *)
compilation_unit   = { using_directive } [ namespace_decl ] { type_decl } ;
using_directive    = "using" [ identifier "=" ] qualified_name [ type_args ] ";" ;
namespace_decl     = "namespace" qualified_name ";" ;
qualified_name     = identifier { "." identifier } ;

(* ---------- Type declarations ---------- *)
type_decl          = class_decl | struct_decl | interface_decl | enum_decl | delegate_decl ;
access             = "public" | "private" | "protected" | "internal"
                   | "protected" "internal" | "private" "protected" ;
modifier           = access | "virtual" | "override" | "static" | "extern" | "unsafe"
                   | "readonly" | "const" ;      (* applicability depends on the member; see Part I *)

class_decl         = { modifier } "class" identifier [ type_params ] [ base_list ]
                     { constraint_clause } "{" { class_member } "}" ;
struct_decl        = { modifier } "struct" identifier [ type_params ] [ base_list ]
                     { constraint_clause } "{" { class_member } "}" ;
interface_decl     = { modifier } "interface" identifier [ type_params ] [ base_list ]
                     { constraint_clause } "{" { interface_member } "}" ;
enum_decl          = { modifier } "enum" identifier [ ":" integral_type ]
                     "{" [ enum_member { "," enum_member } [ "," ] ] "}" ;
enum_member        = identifier [ "=" expression ] ;
delegate_decl      = { modifier } "delegate" return_type identifier [ type_params ]
                     "(" [ parameters ] ")" { constraint_clause } ";" ;

base_list          = ":" type { "," type } ;
type_params        = "<" identifier { "," identifier } ">" ;
type_args          = "<" type { "," type } ">" ;
constraint_clause  = "where" identifier ":" constraint { "," constraint } ;
constraint         = "class" | "struct" | "enum" | type | "new" "(" [ type { "," type } ] ")" ;

(* ---------- Members ---------- *)
class_member       = field | method | constructor | static_constructor | finalizer
                   | property | indexer | operator_decl | type_decl ;
interface_member   = interface_method | interface_property | interface_indexer ;

field              = { modifier } type identifier { "," identifier }
                     [ "=" initializer { "," initializer } ] ";" ;
initializer        = expression | collection_initializer ;
collection_initializer = "{" [ initializer { "," initializer } [ "," ] ] "}" ;

method             = { modifier } return_type identifier [ type_params ]
                     "(" [ parameters ] ")" [ "mut" ] { constraint_clause } method_body ;
method_body        = block | "=>" expression ";" | ";" ;
return_type        = type | "void" ;

constructor        = { modifier } "this" "(" [ parameters ] ")"
                     [ ":" ( "this" | "base" ) "(" [ arguments ] ")" ] ( block | ";" ) ;
static_constructor = "static" "this" "(" ")" block ;
finalizer          = "~" identifier "(" ")" block ;

property           = { modifier } type identifier
                     ( "{" accessor { accessor } "}" [ "=" initializer ";" ]
                     | "=>" expression ";" ) ;
accessor           = { access } ( "get" | "set" ) ( ";" | "=>" expression ";" | block ) ;
indexer            = { modifier } type "this" "[" parameters "]"
                     ( "{" accessor { accessor } "}" | "=>" expression ";" ) ;
operator_decl      = { modifier } type "operator" overloadable_op
                     "(" parameters ")" ( block | "=>" expression ";" ) ;
overloadable_op    = "+" | "-" | "!" | "~" | "++" | "--" | "*" | "/" | "%" | "&" | "|" | "^"
                   | "<<" | ">>" | "==" | "<" | ">" | "<=" | ">=" ;

interface_method   = return_type identifier [ type_params ] "(" [ parameters ] ")" ";" ;
                   (* "mut" is not permitted on interface members (§10.1) *)
interface_property = type identifier "{" ( "get" ";" | "set" ";" ) { "get" ";" | "set" ";" } "}" ;
interface_indexer  = type "this" "[" parameters "]" "{" ( "get" ";" | "set" ";" ) { "get" ";" | "set" ";" } "}" ;
(* explicit interface implementation uses a qualified member name:            *)
(*   return_type type_name "." identifier "(" ... ")" [ "mut" ] block         *)

(* ---------- Parameters ---------- *)
parameters         = parameter { "," parameter } ;
parameter          = { param_modifier } type identifier [ "=" expression ]
                   | { param_modifier } type "..." identifier ;
param_modifier     = "ref" | "in" | "out" | "readonly" ;
arguments          = argument { "," argument } ;
argument           = [ identifier ":" ] [ "ref" | "in" | "out" ] expression
                   | [ identifier ":" ] "out" ( type | "var" ) identifier ;

(* ---------- Types ---------- *)
type               = base_type [ "?" ] { rank_specifier [ "?" ] | "[" expression "]" | "*" } ;
                   (* "[" expression "]" is the inline-array form, valid only in unsafe declarations *)
rank_specifier     = "[" { "," } "]" ;           (* at most two commas (three dimensions) *)
base_type          = primitive_type | qualified_name [ type_args ] | "object" | "string" ;
primitive_type     = "bool" | "char" | integral_type | "float" | "double" ;
integral_type      = "int" | "uint" | "int8" | "int16" | "int32" | "int64" | "uint8" | "uint16" | "uint32" | "uint64" ;

(* ---------- Statements ---------- *)
block              = "{" { statement } "}" ;
statement          = block | local_decl | multi_assign | expression_statement
                   | if_stmt | switch_stmt | labeled_loop
                   | "break" [ identifier ] ";" | "continue" [ identifier ] ";"
                   | "return" [ expression ] ";" | "throw" [ expression ] ";"
                   | try_stmt
                   | "unsafe" block | "unsafe" "unchecked" block
                   | "checked" block | "unchecked" block | ";" ;

local_decl         = local_decl_core ";" ;
local_decl_core    = ( type | "var" | "let" [ type ] ) identifier { "," identifier }
                     [ "=" initializer { "," initializer } ]
                   | "const" type identifier { "," identifier } "=" initializer { "," initializer } ;
                   (* initializers are expressions without a top-level assignment; "var" and
                      "let" without a type require one initializer per identifier *)
multi_assign       = expression "," expression { "," expression } "=" expression { "," expression } ";" ;
expression_statement = expression ";" ;

if_stmt            = "if" "(" expression ")" statement [ "else" statement ] ;
labeled_loop       = [ identifier ":" ] ( while_stmt | do_stmt | for_stmt | for_in_stmt ) ;
while_stmt         = "while" "(" expression ")" statement ;
do_stmt            = "do" statement "while" "(" expression ")" ";" ;
for_stmt           = "for" "(" [ local_decl_core | expression { "," expression } ] ";"
                     [ expression ] ";" [ expression { "," expression } ] ")" statement ;
for_in_stmt        = "for" "(" ( "let" | "var" | type ) identifier "in" expression ")" statement ;

switch_stmt        = "switch" "(" expression ")" "{" { switch_section } "}" ;
switch_section     = switch_label statement { statement } ;
switch_label       = "case" pattern { "," pattern } [ "when" expression ] ":" | "default" ":" ;

try_stmt           = "try" block ( catch_clause { catch_clause } [ "finally" block ] | "finally" block ) ;
catch_clause       = "catch" "(" ( type [ identifier ] | "..." ) ")" block ;

(* ---------- Patterns ---------- *)
pattern            = or_pattern ;
or_pattern         = and_pattern { "or" and_pattern } ;
and_pattern        = not_pattern { "and" not_pattern } ;
not_pattern        = "not" not_pattern | primary_pattern ;
primary_pattern    = "_" | "null" | relational_pattern | constant_expression
                   | type [ identifier ] | "(" pattern ")" ;
relational_pattern = ( "<" | ">" | "<=" | ">=" ) constant_expression ;

(* ---------- Expressions (precedence: low to high; see Appendix C) ---------- *)
expression         = assignment | lambda ;
assignment         = conditional [ assign_op expression ] ;
assign_op          = "=" | "+=" | "-=" | "*=" | "/=" | "%=" | "&=" | "|=" | "^=" | "<<=" | ">>=" | "??=" ;
lambda             = ( identifier | "(" [ lambda_param { "," lambda_param } ] ")" ) "=>" ( expression | block ) ;
lambda_param       = [ type ] identifier ;

conditional        = null_coalesce [ "?" expression ":" expression ] ;
null_coalesce      = logical_or [ "??" null_coalesce ] ;
logical_or         = logical_and { "||" logical_and } ;
logical_and        = bit_or { "&&" bit_or } ;
bit_or             = bit_xor { "|" bit_xor } ;
bit_xor            = bit_and { "^" bit_and } ;
bit_and            = equality { "&" equality } ;
equality           = relational { ( "==" | "!=" ) relational } ;
relational         = shift { ( "<" | ">" | "<=" | ">=" ) shift }
                   | shift ( "is" ( "not" )? pattern | "as" type ) ;
shift              = additive { ( "<<" | ">>" ) additive } ;
additive           = multiplicative { ( "+" | "-" ) multiplicative } ;
multiplicative     = unary { ( "*" | "/" | "%" ) unary } ;
unary              = ( "+" | "-" | "!" | "~" | "++" | "--" | "&" | "*" ) unary
                   | "(" type ")" unary          (* cast; disambiguation as in C# *)
                   | postfix ;
postfix            = primary { "." identifier [ type_args ]      (* also auto-dereferences pointers *)
                             | "?." identifier [ type_args ]
                             | "[" arguments "]" | "?[" arguments "]"
                             | "(" [ arguments ] ")"
                             | "!" | "++" | "--" } ;
primary            = literal | identifier [ type_args ] | "this" | "base" "." identifier
                   | "base" "[" arguments "]" | "(" expression ")"
                   | "new" [ type ] "(" [ arguments ] ")"             (* new T(...), new(...) *)
                   | "scope" [ type ] "(" [ arguments ] ")"           (* scope T(...), scope(...) *)
                   | ( "new" | "scope" ) [ type ] array_suffix        (* arrays, incl. new [n] *)
                   | ( "new" | "scope" )? collection_initializer      (* { 1, 2, 3 }, new { … }, scope { … } *)
                   | primitive_or_type "(" [ arguments ] ")"          (* struct construction *)
                   | "default" [ "(" type ")" ] | "sizeof" "(" type ")"
                   | "checked" "(" expression ")" | "unchecked" "(" expression ")" ;
array_suffix       = "[" [ expression { "," expression } ] "]" [ collection_initializer ] ;
literal            = integer | float | char | string | "true" | "false" | "null" ;
```

Disambiguation notes (informative):

- A statement beginning with `identifier ","` followed eventually by `=` at statement level is parsed as multiple assignment; a statement beginning with a **type** (or `var`/`let`) and an identifier list is a declaration.
- Initializers of a multiple declaration are separated by commas and cannot contain a top-level assignment (so `int32 a = 1, b = 2;` is rejected: LS0214).
- `for (X y in e)` vs `for (init; …)`: the parser recognises the `in` form by `( let | var | type ) identifier "in"`.
- A loop label is `identifier ":"` at the start of a statement immediately followed by a loop keyword.
- Postfix `!` is the null assertion; `!=` is always the inequality token; prefix `!` is logical not.
- In a switch section, successive `case` lines form separate sections unless their patterns are comma-separated on one line.
- A bare `{ … }` in expression position is a collection initializer; it is valid only where the target array type is known (§15.3).
- Generic argument lists are distinguished from relational operators by the same look-ahead rule as C#.

---

# Appendix B: Keywords

| Category | Keywords |
|---|---|
| Types | `bool char int uint int8 int16 int32 int64 uint8 uint16 uint32 uint64 float double object string void` |
| Declarations | `class struct interface enum delegate namespace using const let var extern operator` |
| Modifiers | `public private protected internal static virtual override readonly mut unsafe` |
| Parameters | `ref in out readonly` |
| Control flow | `if else switch case default while do for in break continue return throw try catch finally` |
| Expressions | `new scope this base null true false is as sizeof default checked unchecked` |
| Contextual | `get set field value when and or not where` |
| Reserved for future versions | `async await lock` |

`when`, `and`, `or`, `not`, `where`, `get`, `set`, `field`, `value` are keywords only in the positions described in the text and are otherwise ordinary identifiers. `const` and `readonly` change meaning with context (field, local, parameter). There are no keywords for the excluded features of §24.1.

---

# Appendix C: Operator Precedence

From **highest** to **lowest** precedence. Binary operators are left-associative unless noted.

| # | Category | Operators |
|---|---|---|
| 1 | Primary | `x.y`  `x?.y`  `x[i]`  `x?[i]`  `f(x)`  `x!`  `x++`  `x--`  `new`  `scope`  `default`  `sizeof`  `checked()`  `unchecked()` |
| 2 | Unary (right-assoc.) | `+x`  `-x`  `!x`  `~x`  `++x`  `--x`  `&x`  `*x`  `(T)x` |
| 3 | Multiplicative | `*`  `/`  `%` |
| 4 | Additive | `+`  `-` |
| 5 | Shift | `<<`  `>>` |
| 6 | Relational / type test | `<`  `>`  `<=`  `>=`  `is`  `as` |
| 7 | Equality | `==`  `!=` |
| 8 | Bitwise AND | `&` |
| 9 | Bitwise XOR | `^` |
| 10 | Bitwise OR | `\|` |
| 11 | Logical AND | `&&` |
| 12 | Logical OR | `\|\|` |
| 13 | Null-coalescing (right-assoc.) | `??` |
| 14 | Conditional (right-assoc.) | `?:` |
| 15 | Assignment / lambda (right-assoc.) | `=  +=  -=  *=  /=  %=  &=  \|=  ^=  <<=  >>=  ??=  =>` |

# Appendix D: Decision Log

Every rule marked `[P-n]` in the text is listed here. D.1 records the remaining interpretation to confirm. D.2 lists other provisional proposals. D.3 summarises rules taken directly from the design instructions, so they can be traced.

## D.1 Interpretations to confirm

| ID | Where | Interpretation chosen | Alternative |
|---|---|---|---|
| P-7 | §5.12 | `e as T` is `T?`, except when the compiler can **prove** success and non-null, in which case its null-state is non-null and it converts implicitly to `T`. | Always `T?` |


## D.2 Other provisional proposals

| ID | Topic | Proposal |
|---|---|---|
| P-6 | Struct constructors | Structs may not declare a parameterless constructor; `S()` is always the default value |
| P-8 | Class pointers | `Foo*` is an unmanaged non-owning designation of the object; supports `.`, `*p` (yields a retained `Foo`), comparison, casts to `void*`/integers; no arithmetic/indexing |
| P-9 | Inline-array initializer | May contain fewer than `N` elements (rest default), not more |
| P-10 | Scope temporaries | An unnamed `scope` allocation lives to the end of the full statement; `scope` is rejected in field/static initializers; loop-body scope allocations die per iteration |
| P-11 | Scope escape analysis | The required-reject / required-accept lists, scope-bound closures, receiver-escape rule, destruction-order rule for scoped containers |
| P-12 | Constructor safety | `this` is restricted in a constructor body until non-nullable fields are assigned; during construction the dynamic type is the class whose constructor is running (C++ semantics); after a failed constructor, finalizers of classes whose constructors completed run |
| P-13 | Static initialization | Re-entrant access by the same thread throws `TypeInitializationException`; failure is permanent and rethrows the same exception; entry type initialised before `Main` (startup stub or `Main` prologue) |
| P-14 | Termination | Statics are not released and their finalizers do not run at exit; fatal errors run no cleanup or finalizers and exit non-zero (recommended: abort status) |
| P-15 | Delegates | Single-target; `==` compares method and target |
| P-16 | Generics limits | Invariant type parameters; no generic `virtual`/`override`/interface methods |
| P-17 | Exceptions and native frames | Exceptions must not cross native frames; reaching one is a fatal error where detectable |
| P-18 | Patterns | Constant, type, declaration, relational, `not/and/or`, `null`, `_`, `when` guards; property and operator forms as written |
| P-19 | Entry point | `static [void\|int32] Main([string[] args])`; none required for libraries |
| P-20 | Array members | `Length` (total), `Rank`, `GetLength(int32)`; recommended row-major contiguous layout for multi-dimensional arrays |
| P-21 | Cycle warning | Graph algorithm in §14.8 (nullable fields suppress; derived classes expanded) |
| P-22 | Reference ABI | Header, type-descriptor and string/array layouts and the runtime function set of Appendix E; compiler-known types of §1.7 and their members |
| P-23 | Reserved words | `async`, `await`, `lock`, and `[ … ]` attribute syntax reserved |
| P-24 | Overloads/named/optional | C#-like resolution; named after positional; defaults inserted at the call site |
| P-25 | C# fallback | Silent areas follow C# unless excluded in §24 |
| P-26 | Release timing | As-if rule for early release; finalizers derived→base; fields released in reverse declaration order; exceptions from finalizers are fatal |
| P-27 | Variadics | `T... name`: last parameter; normal form passes an existing array without copying |
| P-28 | Multiple assignment | Right-hand sides left to right → target sub-expressions left to right → assignments left to right |
| P-29 | Target typing | Contexts in which `new()`/`scope()`/`new [n]`/`{…}` infer their type (§5.14) |
| P-30 | Exception handling | Two-phase model: search first, then cleanup; unhandled exceptions terminate without cleanup; `catch (...)`; no filters |
| P-31 | `readonly` fields | Assignable in initializer and in constructors (`static this()` for static); get-only auto-properties likewise |
| P-32 | `const` types | `bool`, `char`, integers, `float`, `double`, enums, `string` |

## D.3 Rules taken directly from your instructions

| Topic | Rule in this document |
|---|---|
| Primitive types | `bool, char, string, int8, uint8, int16, uint16, int32, uint32, int64, uint64, int, uint, float, double` (§3.2); `int`/`uint` are native-width |
| Compiler-known types | `IEnumerable<T>` replaces `IIterable<T>`; `IIterator<T>` retained (§1.7, §6.6) |
| Null | Never assignable to a non-nullable reference (§4.6.1, §13.1) |
| Struct fields | No non-nullable reference fields; nullable references only (§9.1.1) |
| Arrays | Multi-dimensional up to three dimensions plus jagged; a one-dimensional array is always contiguous (§15.1, §28.11) |
| `new`/`scope` shorthand | `new()`, `scope(args)`, `new [5]` (§5.14) |
| Nullable member access | Compile error unless proven non-null; `!` asserts with an injected runtime check (§13.4, §13.5) |
| Multiple declarators | `int32 a, b = 1, 2;`; `let int32 a, b, c = 4, 5, 6;` (§4.2.4) |
| `const`/`readonly` | Explicit-type `const` locals and fields; `readonly` fields; context decides meaning (§4.2.3, §4.5) |
| `defer` | Does not exist; removed everywhere |
| Pointers | `.` member access with automatic dereference; no `->` (§19.3) |
| Loop labels | `label: loop`, `break label;`, `continue label;`; labeled `break` in a `switch` needs an enclosing loop (§6.7) |
| `for`-in | `IEnumerable<T>.Iterate()`, `IIterator<T>.Next()`, `IIterator<T>.Current` (§6.6) |
| Interfaces and `mut` | Interface methods never use `mut`; the implementation declares it (§10.1) |
| Reference arrays | Collection initializer or nullable element type; scoped elements only in a scope-allocated array (§15.2, §14.4.1) |
| Collection initializers | `{ … }`, `new { … }`, `scope { … }`; `let`/`var` need the expanded form; inline arrays take only `{ … }` (§15.3, §15.9) |
| Inline arrays | Struct elements allowed, classes not; pointer elements in unsafe; unsafe required (§15.9) |
| Empty string | Literal `""` is immutable literal data; it is not the default of `string` (§16.2.1, §28.10.2) |
| `NativeMemory` | Omitted (§19.7) |
| Entry point | Executables only; static libraries have none (§22.1) |
| Static initialization | Lazy on first access; per generic instantiation; entry-type failure crashes before `Main` (§22.2) |
| Excluded features | `event, weak/unowned, GC, partial, record, params, sealed, abstract, goto, fixed, stackalloc, dynamic, tuple syntax/deconstruction, linq` are not language features (§24.1); tuple-like types may be library-defined |
| Locals | Non-nullable reference declarations must be assigned immediately (§4.3) |
| Casts / `as` | Casts always throw; `as` returns a nullable unless provable (§5.12, §5.13) |
| String length | UTF-16 code units (C# `String.Length` semantics), regardless of storage encoding, excluding the terminator (§16.3) |
| Visibility | All C# modifiers; members default to `private` (§8.7) |
| Multicast delegates | Not supported (§17.1) |
| Diagnostic suppression | Deferred until a preprocessor exists (§23.1) |

---

# Appendix E: Runtime Interface Reference (Reference ABI v1)

This appendix is the **reference design** (§25.4): a concrete object layout and the set of runtime functions that compiler-generated code calls. Names and layouts here are *recommended*; an implementation MAY choose others, but a static library and the executable that links it MUST agree on one ABI version. All function semantics are normative to the extent they restate Part I/II rules.

## E.1 Reference Layouts

```c
typedef struct TypeDescriptor TypeDescriptor;

typedef struct ObjectHeader {
    TypeDescriptor*  type;      // dynamic type (changes during construction, §28.7.3)
    _Atomic uint32_t strong;    // strong count; 1 at creation
    uint32_t         flags;     // bit0 IMMORTAL, bit1 SCOPED, bit2 DESTROYING
} ObjectHeader;                 // 16 bytes on 64-bit targets; payload follows

typedef struct RefMapEntry {    // one reference slot, listed in RELEASE order (reverse declaration)
    uint32_t offset;            // byte offset from the payload start
    uint32_t kind;              // 0 = reference, 1 = string (null reads as empty), 2 = nested struct (map follows)
    const struct RefMapEntry* nested; uint32_t nestedCount;
} RefMapEntry;

typedef struct InterfaceEntry { TypeDescriptor* iface; void** table; } InterfaceEntry;

struct TypeDescriptor {
    uint32_t  kind;             // CLASS STRUCT ENUM INTERFACE DELEGATE ARRAY PRIMITIVE POINTER
    uint32_t  flags;            // HAS_FINALIZER HAS_REFERENCES ABSTRACT UNMANAGED ...
    uint32_t  size, alignment;  // instance payload size / element size for arrays
    const char* name;           // fully qualified UTF-8 name
    TypeDescriptor* base;       // base class or NULL
    TypeDescriptor* element;    // array element / pointer target
    uint32_t  rank;             // arrays
    const InterfaceEntry* interfaces; uint32_t interfaceCount;
    void**    vtable;           uint32_t vtableCount;
    void    (*finalizer)(void* object);   // this class's own finalizer or NULL (chain via base)
    const RefMapEntry* refMap;  uint32_t refMapCount;
    TypeDescriptor** typeArgs;  uint32_t typeArgCount;
    _Atomic uint32_t initState; // 0 Uninit, 1 InProgress, 2 Done, 3 Failed  (§29.4)
    void*     initOwner;        // thread id while InProgress
    void*     initException;    // the TypeInitializationException when Failed
    void*     statics;          // storage for static fields
};

typedef struct StringObject {
    ObjectHeader h; const uint8_t* data; int32_t length; int32_t byteLength;
    uint8_t charset; uint8_t flags;           // flags: OWNS_DATA, ASCII_ONLY
} StringObject;

typedef struct ArrayObject {
    ObjectHeader h; int32_t rank; int32_t length; /* int32_t dims[rank] if rank > 1 */
    /* elements follow, aligned to max(8, element alignment) */
} ArrayObject;

typedef struct DelegateObject { ObjectHeader h; void* function; void* target; } DelegateObject;
```

## E.2 Runtime Functions

Types: `Object*` is a pointer to an object header. "Throws" means the function raises the stated exception (§31.6) instead of returning normally.

| Function | Semantics |
|---|---|
| `Object* luma_alloc_object(TypeDescriptor* t)` | Zeroed object of `t`; header type `t`, count 1. Throws `OutOfMemoryException` |
| `Object* luma_init_scoped(void* storage, TypeDescriptor* t)` | Initialise a zeroed, scoped (uncounted) object in compiler-provided storage (stack or arena); header flag SCOPED |
| `ArrayObject* luma_array_new(TypeDescriptor* arrayType, int32_t rank, const int32_t* dims)` | Zeroed array; throws `OverflowException` (negative dimension) or `OutOfMemoryException` |
| `void* luma_alloc_raw(size_t size, size_t align)` | Zeroed raw block for runtime-internal use; throws `OutOfMemoryException` |
| `void luma_retain(Object* o)` | §27.4; no-op for NULL, immortal and scoped objects |
| `void luma_release(Object* o)` | §27.4; destroys on reaching zero (§27.5) |
| `void luma_destroy_scoped(Object* o)` | Scope-exit destruction of a scoped object (§27.6) |
| `void luma_set_type(Object* o, TypeDescriptor* t)` | Set the dynamic type (§28.7.3) |
| `bool luma_is_instance(Object* o, TypeDescriptor* t)` | §28.8; false for NULL |
| `Object* luma_cast(Object* o, TypeDescriptor* t)` | `(T)o`: throws `InvalidCastException` (non-null mismatch) or `NullReferenceException` (null to non-nullable) |
| `Object* luma_as(Object* o, TypeDescriptor* t)` | `o as T`: result or NULL |
| `Object* luma_box(TypeDescriptor* valueType, const void* value)` | §28.9; retains contained references |
| `void luma_unbox(Object* box, TypeDescriptor* valueType, void* out)` | Exact type check; throws `InvalidCastException`/`NullReferenceException` |
| `StringObject* luma_string_empty(void)` | The shared immortal empty string |
| `StringObject* luma_string_concat(StringObject* a, StringObject* b)` | §28.10.5; NULL reads as empty |
| `int32_t luma_string_length(StringObject*)`, `uint16_t luma_string_char(StringObject*, int32_t)` | `Length` and indexer (§28.10.4); indexer throws `IndexOutOfRangeException` |
| `bool luma_string_equals(StringObject*, StringObject*)` | Content equality (§16.4) |
| `DelegateObject* luma_delegate_new(TypeDescriptor* t, void* fn, Object* target)` | §28.13; retains `target` |
| `void luma_ensure_initialized(TypeDescriptor* t)` | §29.4 |
| `noreturn void luma_throw(Object* ex)` | Begin two-phase unwinding (§31.3); consumes the caller's reference |
| `noreturn void luma_rethrow(void)` | `throw;` |
| `noreturn void luma_throw_null_reference(void)`, `luma_throw_index_out_of_range`, `luma_throw_overflow`, `luma_throw_divide_by_zero`, `luma_throw_invalid_cast` | Raise the runtime exceptions of §18.5 |
| `noreturn void luma_fatal(const char* kind, const char* message)` | §26.5: report, terminate, no cleanup |
| `int32_t luma_start(int32_t argc, char** argv)` | Startup sequence of §26.1; calls the entry stub; never returns (calls `exit`) |
| `void luma_runtime_init(void)` | Steps 2–3 of §26.1, exposed for future hosting (§26.6) |

## E.3 Ownership Conventions (ABI v1)

- Reference **parameters are borrowed**: the callee retains only if it stores the reference beyond the call.
- Reference **return values are owned** (+1).
- `luma_alloc_object`, `luma_box`, `luma_string_concat` and `luma_array_new` return owned references.
- `luma_throw` takes ownership of the exception reference.
- A reference stored into a field, array element or captured variable is retained by the storing code and the old value released (§14.2).

---

# Appendix F: Complete Example Program

`Console.WriteLine` stands for a standard-library output facility, which is outside this specification.

```lumasharp
using System;

namespace Demo;

enum Shape : uint8 { Circle, Square }

struct Vector2
{
    public double x;
    public double y;
    public string? label;                // nullable; struct fields cannot be non-nullable references

    public this(double x, double y) { this.x = x; this.y = y; }

    public void Scale(double f) mut { x *= f; y *= f; }                 // mutating

    public static Vector2 operator +(Vector2 a, Vector2 b) => Vector2(a.x + b.x, a.y + b.y);
    public static bool operator ==(Vector2 a, Vector2 b) => a.x == b.x && a.y == b.y;
}

class Range : IEnumerable<int32>
{
    readonly int32 count;
    public this(int32 count) { this.count = count; }
    public IIterator<int32> Iterate() => new RangeIterator(count);
}

class RangeIterator : IIterator<int32>
{
    int32 current = -1;
    readonly int32 count;
    public this(int32 count) { this.count = count; }
    public bool Next() mut { current++; return current < count; }       // mut on the implementation
    public int32 Current => current;
}

virtual class Animal
{
    readonly string name;                 // must be assigned in every constructor
    Animal? mate;                         // nullable: may be cleared to break cycles

    protected this(string name) { this.name = name; }

    public string Name => name;
    public virtual string Sound();                // body-less virtual: Animal is abstract
    public string Describe() => name + " says " + Sound();

    public void Pair(Animal other) mut { mate = other; }
    public void Unpair() mut { mate = null; }

    ~Animal() { Console.WriteLine(name + " destroyed"); }
}

class Dog : Animal
{
    public this(string name) : base(name) { }
    public override string Sound() => "woof";
}

class Program
{
    const int32 Limit = 3;
    static readonly string Greeting = "hello";

    static int32 Sum(int32... values)
    {
        int32 total = 0;
        for (let v in values) { total += v; }
        return total;
    }

    static bool TryDivide(int32 a, int32 b, out int32 result)
    {
        if (b == 0) { result = 0; return false; }
        result = a / b;
        return true;
    }

    static void Show(readonly Animal a)           // cannot be stored, so scoped objects may be passed
    {
        Console.WriteLine(a.Describe());
    }

    static int32 Main(string[] args)
    {
        // multiple declaration and assignment
        int32 a, b = 2, 1;                        // a == 2, b == 1
        int32 c, d = 7;                           // c == 7, d == 0
        a, b = b, a;                              // swap: a == 1, b == 2

        // loops, labels, multi-dimensional arrays
        int32[,] grid = { { 1, 2, 3 }, { 4, 5, 6 } };
        int32 found = -1;
        rows: for (int32 y = 0; y < grid.GetLength(0); y++)
        {
            for (int32 x = 0; x < grid.GetLength(1); x++)
            {
                if (grid[y, x] == 5) { found = y * 10 + x; break rows; }
            }
        }                                         // found == 11

        for (let n in new Range(Limit)) { Console.WriteLine(n); }      // 0 1 2

        // switch: no fall-through, comma lists, relational patterns
        switch (found)
        {
            case 1, 3: Console.WriteLine("odd");
            case > 10: Console.WriteLine("big");                       // printed
            default:   Console.WriteLine("other");
        }

        // nullable flow analysis
        Animal? maybe = null;
        if (maybe != null) { Console.WriteLine(maybe.Name); }
        string title = maybe?.Name ?? "nobody";                        // "nobody"
        // maybe.Name;                                                  // error LS1303

        // scope allocation, heap allocation and reference counting
        var rex = scope Dog("Rex");
        Show(rex);                                // "Rex says woof"
        Animal pet = new Dog("Fido");
        Animal copy = pet;                        // Fido: count 2
        pet = new Dog("Spot");                    // Fido still held by 'copy'

        // boxing, as, casts
        object boxed = 42;
        int32? n42 = boxed as int32;              // 42
        string? s = boxed as string;              // null
        int32 m = (int32)boxed;                   // 42

        // exceptions
        try { int32 z = 10 / (a - 1); }           // a == 1 -> DivideByZeroException
        catch (DivideByZeroException) { Console.WriteLine("div by zero"); }

        if (TryDivide(10, 2, out int32 q)) { Console.WriteLine(q); }   // 5
        Console.WriteLine(Sum(1, 2, 3, 4));                              // 10

        // unsafe: pointers use '.' with automatic dereference
        unsafe
        {
            Vector2 v = Vector2(1.0, 2.0);
            Vector2* pv = &v;
            pv.Scale(2.0);                        // v == (2.0, 4.0)
            int32 value = 5;
            int32* p = &value;
            *p = 6;                               // value == 6
        }
        return 0;
    }   // scope exit, reverse declaration order:
        //   copy released -> "Fido destroyed"; pet released -> "Spot destroyed";
        //   rex destroyed -> "Rex destroyed". The process then exits with code 0.
}
```
