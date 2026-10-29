# LumaSharp quick reference

This page is a compact syntax guide to the language features currently represented by the compiler. It is not a complete semantics document: the language is still evolving, and parts of the draft specification remain unimplemented. See [the language specification](../Specification/LumaSharp_LanguageSpecification.md) for intended rules and [the compiler specification](../Specification/LumaSharp_CompilerSpecification.md) for build behaviour.

## Smallest program

Source files use `.ls`. An executable entry point is a public or accessible static `Main` returning `void` or `int32`, with no arguments or `string[] args`.

```lumasharp
class Program {
    public static int32 Main() {
        let answer = 40 + 2;
        return answer;
    }
}
```

The CLI compiles one or more files together: `luma build Program.ls Helpers.ls`. A single JSON project file instead triggers recursive `.ls` discovery under its directory.

## Lexical forms and types

```lumasharp
// line comment; /* block comment */
namespace Demo;
using Text = System.String;

bool ready = true;
char letter = 'A';
int8 small = -1; uint8 octet = 255;
int16 a = 12; uint16 b = 12;
int32 count = 42; uint32 flags = 0xFF;
int64 big = 3_000_000_000L; uint64 mask = 5UL;
int nativeSigned = 1; uint nativeUnsigned = 2; // pointer-width integers
float ratio = 0.5f; double exact = 0.25;
string message = "hello\nworld"; // strings are non-null; default is empty
object item = "text";
int32? maybeCount = null; // nullable value type
```

Integer literals support decimal, `0x` hexadecimal, `0b` binary, digit separators, and `u`, `l`, `ul` suffixes. Strings support escaped and verbatim forms (`@"C:\\path"`). Identifiers are case-sensitive; `@name` escapes a keyword. `async`, `await`, and `lock` are reserved for future use.

Reference types are non-null by default; append `?` for nullable references or value types. `!` asserts non-null at runtime. `??` supplies a fallback, `??=` assigns one only when null, `?.` and `?[...]` conditionally access nullable receivers, and `is null` / `is not null` test null state.

## Declarations and assignment

```lumasharp
let fixedValue = 10;             // inferred immutable local
var changing = 1;                // inferred mutable local
const int32 Limit = 100;         // compile-time constant
int32 x = 1, y = 2;              // typed locals
x = 3;
x, y = y, x;                    // simultaneous assignment
int32? optional = default;       // target-typed default
```

`let` is immutable, `var` is mutable, and explicit types may replace inference. Fields and locals can be `readonly` or `const` where allowed. `ref`, `in`, and `out` parameters express aliasing modes; `scope` can be combined with any of them to prevent references exposed to a method from escaping. `out` values must be assigned before return. Named and optional arguments use `name: value`; variadic parameters use `T... values`.

## Expressions and operators

Supported expression forms include calls, member/index access, object and array creation, casts, `is`/`as`, `default`, conditional `?:`, lambdas, and checked numeric operations. Arithmetic and narrowing conversions are checked by default; `checked (expr)` is explicit, and `unchecked` requires `unsafe`.

```lumasharp
int32 n = (int32)(4 * 3 + 2);
bool test = n >= 10 && n != 12;
int32 result = test ? n : 0;
int32? maybe = null;
int32 actual = maybe ?? 5;
```

Operator families: unary `+ - ! ~ ++ --`; arithmetic `* / % + -`; shifts `<< >>`; comparisons `< <= > >= == !=`; bitwise `& ^ |`; short-circuit `&& ||`; null coalescing `??`; assignment and compound assignment (`= += -= *= /= %= &= |= ^= <<= >>= ??=`). `!=` follows equality. Overloadable operators are described in the specification.

## Control flow and patterns

```lumasharp
if (n > 0) { n--; } else { n = 0; }
while (n < 3) { n++; }
do { n--; } while (n > 0);
for (var i = 0; i < 3; i++) { }
outer: while (ready) { break outer; }

switch (n) {
    case 0: break;
    case > 0 and < 10: break;
    default: break;
}

if (item is string text && text.Length > 0) { }
```

`switch` supports constants, null/discard, type/declaration, relational, and combined `and`/`or`/`not` patterns, plus ordered `when` guards. Sections do not fall through. `break` and `continue` can target loop labels. `for (let value in values)` iterates through the enumerable protocol. `return`, `throw`, and exception transfers are supported.

## Methods and parameters

```lumasharp
class MathBox {
    public static int32 Add(int32 left, int32 right = 0) {
        return left + right;
    }

    public static bool TryRead(string text, out int32 value) {
        value = text.Length;
        return true;
    }
}
```

Methods can be overloaded and generic, accept optional, named, variadic, `ref`, `in`, or `out` arguments, and return values or `void`. Entry points use `static [void|int32] Main([string[] args])`. Generic methods may infer type arguments from their calls; constraints include reference/value/enum, base/interface, and constructor constraints.

## Classes, structs, interfaces, and enums

```lumasharp
interface IValue { int32 Get(); }

class Counter : IValue {
    private int32 value;
    public this(int32 start) { value = start; }
    public int32 Get() { return value; }
    public mut void Increment() { value++; }
    public int32 Current { get; set; }
    public int32 this[int32 index] { get { return value + index; } }
    public virtual string ToString() { return "Counter"; }
}

struct Point {
    public int32 X;
    public int32 Y;
    public this(int32 x, int32 y) { X = x; Y = y; }
}

enum Color { Red, Green = 4, Blue }
```

Classes support fields, constructors (`this(...)`), static initialization (`static this()`), finalizers, inheritance, virtual/override dispatch, properties, indexers, and operator overloads. Classes are sealed by default; `virtual class` permits inheritance. Instance methods are read-only unless marked `mut`. Structs are value types with methods, constructors, properties, and operators; interfaces describe contracts; enums have integral values. Static classes contain static members only.

## Generics, delegates, and lambdas

```lumasharp
interface IBox<T> { T Get(); }
class Box<T> : IBox<T> {
    private T value;
    public this(T initial) { value = initial; }
    public T Get() { return value; }
}

delegate int32 Transform(int32 value);
Transform increment = value => value + 1;
```

Generic classes, structs, interfaces, and methods support type arguments and constraints. Delegate values represent static or instance methods and lambdas; target-typed lambdas can capture locals, parameters, and `this` subject to lifetime rules. Closure captures share mutable storage where required. Multicast delegates are not part of the language.

## Strings, arrays, and memory

```lumasharp
string greeting = "hi" + "!";
char first = greeting[0];
int32[] numbers = { 1, 2, 3 };
int32[,] grid = new int32[2, 3];
int32[][] rows = { { 1 }, { 2, 3 } };
for (let number in numbers) { }

scope Counter local = new Counter(0); // lexical lifetime
```

Strings are immutable UTF-8 values; `Length` and indexing use UTF-16 code units. Arrays include managed vectors, rectangular arrays (rank up to three), jagged arrays, and fixed-size inline arrays in structs. Array indexing is bounds-checked; arrays expose `Length`, `Rank`, and `GetLength`. Reference values are retained and released automatically. `scope` requests block lifetime, marks non-escaping borrowed parameters, and is subject to escape checks.

```lumasharp
unsafe {
    int32[4] inline = { 1, 2, 3, 4 };
    int32* address = &inline[0];
    int32 first = *address;
}
```

Pointers can address locals, fields, array elements, inline arrays, and native storage. Inline arrays are fixed-size values and require `unsafe`; they are distinct from heap arrays.

## Exceptions

```lumasharp
try {
    if (n < 0) { throw new System.Exception("negative"); }
} catch (System.Exception error) {
    throw;
} finally {
    n = 0;
}
```

Typed and catch-all handlers, rethrow, and `finally` are supported. Runtime checks can raise catchable exceptions including null-reference, invalid-cast, bounds, overflow, divide-by-zero, allocation, and static-initialization failures.

## Unsafe code and native interop

Unsafe constructs require both an unsafe context and CLI/project opt-in (`--unsafe`). They include `sizeof`, pointers, address-of, dereference, pointer indexing/arithmetic, and C ABI imports.

```lumasharp
public static class Native {
    public static extern int32 abs(int32 value);
}

unsafe { int32 magnitude = Native.abs(-5); }
```

Extern calls require unsafe context and a compatible native symbol when linking. Pointer code can express undefined behaviour; safe code retains runtime null, bounds, and overflow checks.

## Namespaces and imports

A file may begin with one file-scoped namespace declaration (`namespace A.B;`). Leading `using` directives import namespaces or define type aliases (`using ShortName = A.LongType;`). Visibility modifiers are `public`, `private`, `protected`, and `internal`.

## Build status and full rules

The compiler implements substantially more detail than fits in these examples, including nullable flow analysis, runtime type tests/casts, boxes, generic specialization, ownership cleanup, static initialization, and native target emission. Coverage is still incomplete; consult the specifications for intended behaviour and the test suite for exercised cases. The README at the repository root has build instructions.

