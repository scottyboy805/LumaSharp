# LumaSharp Language and Usage Guide

LumaSharp is a statically typed language with C#-style syntax. It compiles ahead of time to native code through LLVM and manages ordinary objects with deterministic reference counting. This guide explains the language as it is implemented today, using examples rather than specification terminology.

The language and compiler are still under active development. Some features described by the specifications are not implemented yet, and a few implemented features have narrower support than their C# equivalents. The [Quick Reference](Overview.md) is a compact syntax sheet; this page is the longer introduction and usage guide.

## Contents

- [1. Start here](#1-start-here)
  - [Your first program](#your-first-program)
  - [Build from the command line](#build-from-the-command-line)
  - [Use a project file](#use-a-project-file)
- [2. Source files and names](#2-source-files-and-names)
- [3. Types, variables, and values](#3-types-variables-and-values)
  - [Built-in types](#built-in-types)
  - [Declarations and defaults](#declarations-and-defaults)
  - [Conversions and boxing](#conversions-and-boxing)
- [4. Expressions and operators](#4-expressions-and-operators)
- [5. Decisions, loops, and patterns](#5-decisions-loops-and-patterns)
- [6. Methods and parameters](#6-methods-and-parameters)
- [7. Classes, structs, interfaces, and enums](#7-classes-structs-interfaces-and-enums)
- [8. Generic types and methods](#8-generic-types-and-methods)
- [9. Delegates, lambdas, and captures](#9-delegates-lambdas-and-captures)
- [10. Nullability](#10-nullability)
- [11. Memory, ownership, and scope](#11-memory-ownership-and-scope)
- [12. Arrays, strings, and spans](#12-arrays-strings-and-spans)
- [13. Collections](#13-collections)
- [14. Exceptions](#14-exceptions)
- [15. Threads](#15-threads)
- [16. Async methods, tasks, and locks](#16-async-methods-tasks-and-locks)
- [17. Unsafe code and native pointers](#17-unsafe-code-and-native-pointers)
- [18. Runtime library](#18-runtime-library)
- [19. Current boundaries](#19-current-boundaries)
- [20. Further reading](#20-further-reading)

## 1. Start here

### Your first program

Save this as `Hello.ls`:

```lumasharp
using System;

public class Program
{
    public static int32 Main()
    {
        Console.WriteLine("Hello, LumaSharp!");
        return 0;
    }
}
```

An executable has one public static `Main` method. It can return `void` or `int32`, and can take no parameters or one `string[] args` parameter. Returning an integer sets the process exit code.

### Build from the command line

With the .NET 10 SDK installed, build one or more `.ls` files from the repository's CLI project:

```powershell
dotnet run --project LumaSharp.Cli/LumaSharp.Cli.csproj -- build Hello.ls
```

Additional files passed on the same command line are compiled into the same program:

```powershell
dotnet run --project LumaSharp.Cli/LumaSharp.Cli.csproj -- build Hello.ls Helpers.ls
```

The CLI emits a native executable by default. Common options are:

| Option | Purpose |
|---|---|
| `--name <name>` | Set the output name. |
| `--out-dir <path>` | Choose the output directory. |
| `--intermediate-dir <path>` | Choose where intermediate files are written. |
| `--runtime-library <directory>` | Use runtime `.ls` sources from an explicit directory. |
| `--kind staticLibrary` | Build a static library instead of an executable. |
| `--target <triple>` | Choose an LLVM target triple; `host` selects the current machine. |
| `--unsafe` | Allow source files to use unsafe features. |
| `--optimize` | Enable LLVM optimization. |
| `--no-llvm` | Verify and emit LLVM IR without linking a final executable. |

Run `luma --help` (or `dotnet run --project LumaSharp.Cli/LumaSharp.Cli.csproj -- --help`) for the complete option list.

With `--optimize`, executable builds allow LLVM to discard LumaSharp methods and static fields that are unreachable from the entry point. Static-library builds keep their public definitions so downstream programs can use them. This reduces executable packaging size; parsing and semantic analysis still process the full runtime source library.

### Use a project file

A JSON project describes output settings. The CLI discovers `.ls` files recursively below the JSON file's directory. The compiler locates the shipped runtime library automatically, so a project does not need its own copy. A project can select a different runtime source directory with `runtimeLibraryPath`:

```json
{
  "schemaVersion": 1,
  "outputKind": "executable",
  "outputName": "Hello",
  "outputDirectory": "bin",
  "intermediateDirectory": "obj",
  "runtimeLibraryPath": "../LumaSharp.Library",
  "targetTriple": "host",
  "unsafe": false,
  "optimize": false
}
```

The runtime path is optional; when supplied, it is resolved relative to the JSON file. The equivalent CLI option is `--runtime-library <directory>`. Save the example as `Hello.json`, then build it with:

```powershell
dotnet run --project LumaSharp.Cli/LumaSharp.Cli.csproj -- build Hello.json
```

The project file sets defaults; supported CLI options can override them. See the [compiler specification](../Specification/LumaSharp_CompilerSpecification.md) for every project setting and validation rule.

## 2. Source files and names

LumaSharp source files use the `.ls` extension. A file may contain one file-scoped namespace, declared before its types. `using` directives can import a namespace or provide an alias.

```lumasharp
using System.Collections;
using Text = System.String;

namespace Samples;

public class Greeter
{
    public int32 MessageLength(Text message) => message.Length;
}
```

If a file has no namespace declaration, its types are in the global namespace. Different files can declare types in the same namespace. Identifiers are case-sensitive.

Use `//` for a line comment and `/* ... */` for a block comment. String literals use double quotes; character literals use single quotes. Escape sequences such as `\n`, `\t`, and `\\` are available. Verbatim strings begin with `@`, as in `@"C:\data\notes.txt"`.

Access modifiers are `public`, `private`, `protected`, `internal`, `protected internal`, and `private protected`. A top-level type with no modifier is internal; members with no modifier are private.

## 3. Types, variables, and values

### Built-in types

| LumaSharp type | Meaning |
|---|---|
| `bool` | Boolean value, `true` or `false`. |
| `char` | One UTF-16 code unit. |
| `int8`, `int16`, `int32`, `int64` | Signed fixed-width integers. |
| `uint8`, `uint16`, `uint32`, `uint64` | Unsigned fixed-width integers. |
| `int`, `uint` | Signed and unsigned integers the width of a native pointer. |
| `float`, `double` | IEEE 754 single- and double-precision numbers. |
| `string` | Immutable, non-null text. |
| `object` | Root reference type and boxing target. |

The primitive spellings are language keywords backed by corresponding types in `System`. For example, `int8` maps to `System.Int8`, and `bool` maps to `System.Bool`. Integer types expose `MinValue` and `MaxValue`; primitive types provide parsing helpers where applicable and format their values with `ToString()`.

```lumasharp
using System;

public class Program
{
    public static int32 Main()
    {
        int32 limit = int32.MaxValue;
        bool enabled = bool.Parse("true");

        if (int32.TryParse("42", out int32 parsed))
            Console.WriteLine(parsed.ToString());

        return enabled && limit > parsed ? 0 : 1;
    }
}
```

Integer literals may be decimal, hexadecimal (`0x`), or binary (`0b`). Underscores can separate digits for readability. Suffixes such as `u`, `l`, and `ul` specify unsigned or wider integer literals. A trailing `f` marks a `float` literal.

### Declarations and defaults

Use an explicit type when it improves clarity. `var` infers a mutable local; `let` infers an immutable local. `const` declares a compile-time constant.

```lumasharp
class Example
{
    public static void Run()
    {
        int32 attempts = 0;
        var message = "ready";
        let answer = 42;
        const int32 RetryLimit = 3;

        attempts = attempts + 1;
    }
}
```

Value types such as integers, booleans, chars, structs, and enums have zero-valued defaults. Nullable types default to `null`. Non-nullable reference types such as `string` and user classes do not have a valid default value: initialize them before use.

```lumasharp
class Example
{
    public static void Run()
    {
        int32 count = default;        // 0
        string? optionalName = null;  // nullable reference
        string name = "Ada";          // non-null string
    }
}
```

`readonly` fields can be assigned only during initialization or construction. Multiple declarations and simultaneous assignment are supported:

```lumasharp
class Example
{
    public static void SwapExample()
    {
        int32 left = 1;
        int32 right = 2;
        left, right = right, left;
    }
}
```

### Conversions and boxing

The compiler applies standard implicit conversions when they preserve the value, such as assigning a derived class to a base class or converting an integer to a wider compatible numeric type. Use a cast for an explicit conversion:

```lumasharp
class Example
{
    public static void Convert()
    {
        double ratio = 3;       // implicit numeric conversion
        int32 roundedInput = (int32)ratio;
    }
}
```

Checked integer operations report overflow by default. Use `checked` or `unchecked` to select behavior for an expression; unchecked operations require an unsafe context.

Value types can be assigned to `object`, which boxes the value. Casting back checks the boxed type at runtime:

```lumasharp
class Example
{
    public static int32 BoxAndUnbox()
    {
        int32 score = 42;
        object boxed = score;
        int32 restored = (int32)boxed;
        return restored;
    }
}
```

User-defined conversions can be declared on a class or struct with `implicit operator` or `explicit operator`. A direct user-defined conversion can participate in assignment or a cast; conversion chaining is not supported yet.

```lumasharp
public struct Number
{
    public int32 Value;

    public this(int32 value)
    {
        Value = value;
    }

    public static implicit operator int32(Number value)
    {
        return value.Value;
    }

    public static explicit operator Number(int32 value)
    {
        return Number(value);
    }
}

class Program
{
    public static int32 Main()
    {
        int32 raw = 42;
        Number number = (Number)raw;
        int32 scalar = number;
        return scalar == 42 ? 0 : 1;
    }
}
```

`is` performs a type test and `as` attempts a nullable reference/value conversion. A failed checked cast throws `InvalidCastException`; a failed `as` returns `null`.

## 4. Expressions and operators

Expressions include literals, calls, member access, indexing, object and array creation, casts, conditional expressions, and lambdas. Arithmetic, comparison, logical, bitwise, shift, and assignment operators are available.

```lumasharp
class Example
{
    public static int32 Calculate()
    {
        int32 total = 4 * 5 + 2;
        bool inRange = total >= 10 && total < 30;
        int32 selected = inRange ? total : 0;

        int32? maybeCount = null;
        int32 count = maybeCount ?? 0;
        return selected + count;
    }
}
```

The conditional operators `&&` and `||` short-circuit: the right operand is evaluated only if needed. `??` returns the left value when it is non-null, otherwise evaluates and returns its fallback. `??=` assigns a fallback only when the left side is null.

Use `checked (expression)` to make the intent explicit for integer overflow, or `unchecked (expression)` inside an unsafe context to allow wrapping arithmetic and, where applicable, omit array bounds checks. Ordinary array access remains bounds-checked.

User-defined operators are supported for classes and structs for the implemented unary and binary operator forms. They use ordinary methods and are resolved with the operands' types; consult the specification for the exact set and signature rules.

## 5. Decisions, loops, and patterns

LumaSharp supports `if`/`else`, `while`, `do`/`while`, C-style `for`, and `for-in`. `break` and `continue` can target a loop label.

```lumasharp
using System;

public class Program
{
    public static int32 Main()
    {
        int32 sum = 0;
        int32[] values = { 2, 4, 6 };

        for (let value in values)
        {
            sum += value;
        }

        if (sum == 12)
            Console.WriteLine("all values counted");
        return 0;
    }
}
```

`switch` supports constants, null and non-null patterns, type patterns, relational patterns, and combined `and`, `or`, and `not` patterns. A `when` guard adds a condition to a matching case. A successful `is T name` test creates a variable available in the true branch.

```lumasharp
using System;

public class Program
{
    public static int32 Main()
    {
        string text = "LumaSharp";
        object item = text;

        if (item is string matched && matched.Length > 0)
            Console.WriteLine(matched);

        switch (text.Length)
        {
            case 0:
                Console.WriteLine("empty");
                break;
            case > 0 and < 10:
                Console.WriteLine("small");
                break;
            default:
                Console.WriteLine("other");
                break;
        }
        return 0;
    }
}
```

Switch sections do not fall through. The compiler checks unreachable statements, missing returns, duplicate cases, and several forms of pattern subsumption.

## 6. Methods and parameters

Methods may be overloaded, static or instance, expression-bodied or block-bodied, and may return `void` or a value.

```lumasharp
public static class MathBox
{
    public static int32 Add(int32 left, int32 right = 0)
    {
        return left + right;
    }
}

public class Program
{
    public static int32 Main()
    {
        int32 first = MathBox.Add(5);
        int32 second = MathBox.Add(right: 4, left: 3);
        return first + second;
    }
}
```

Optional parameters have defaults. Named arguments can be written in any order, but positional arguments must come before named arguments. Variadic parameters use `T... values` and gather extra arguments into an array.

Parameters pass by value unless a modifier says otherwise:

| Form | How it works |
|---|---|
| `T value` | Passes a value copy or a reference value. The method may retain a class reference. |
| `ref T value` | Gives the method read/write access to the caller's variable. |
| `in T value` | Gives read-only access to the caller's variable. |
| `out T value` | The method must assign the value before returning normally. |
| `scope T value` | Prevents references exposed to the method from escaping that call. |

`scope` can be combined with `ref`, `in`, or `out`. `readonly` is not a parameter modifier; use `in` for a readonly reference and `scope` for a non-escaping lifetime.

### Ref locals and returns

Use `ref` on the initializer to make a local alias existing storage. `let` and `var` infer the element type automatically; when you write the type yourself, put `ref` before it:

```lumasharp
int32[] values = { 3, 5, 8 };
var first = ref values[0];
first = 10;

ref int32 second = ref values[1];
```

The alias can update the original location. `let` keeps the alias binding itself immutable, while writes through the alias still update the referenced value. A ref local cannot be captured by a lambda.

A method can return a reference to caller storage:

```lumasharp
public static ref int32 First(ref int32 value)
{
    return ref value;
}
```

Ref returns currently allow non-`scope` `ref`/`out` parameters and mutable static fields. The compiler rejects returning a method local or instance-owned field. Ref fields are not supported.

Mark a struct `ref struct` when it represents a view over other storage. Ref structs are limited to locals, parameters, and return values; they cannot be fields, array elements, boxed, or captured. `Span<T>` and `ReadOnlySpan<T>` are ref structs.

Spans created from arrays, strings, or collection backing arrays hold a strong reference to that storage, including after slicing or conversion to a read-only span. The pointer-and-length constructors do not hold an owner; use those only when unsafe code guarantees that the memory remains valid.

```lumasharp
using System;

public static class Reader
{
    public static bool TryRead(string text, out int32 length)
    {
        length = text.Length;
        return length > 0;
    }
}

public class Program
{
    public static int32 Main()
    {
        if (Reader.TryRead("hello", out int32 length))
            Console.WriteLine(length.ToString());
        return length;
    }
}
```

An ordinary heap object is also valid for a `scope` parameter. The method can use it while it runs, but cannot return it, save it into longer-lived storage, or capture it in a closure that outlives the call.

Generic methods can infer type arguments from calls:

```lumasharp
public static class Utilities
{
    public static T Identity<T>(T value)
    {
        return value;
    }
}

public class Program
{
    public static int32 Main()
    {
        int32 answer = Utilities.Identity(42); // T is int32
        return answer;
    }
}
```

## 7. Classes, structs, interfaces, and enums

### Classes

Classes are reference types. Each instance has identity, and assignments share the same instance.

```lumasharp
public class Counter
{
    private int32 count;

    public this(int32 initial)
    {
        count = initial;
    }

    public int32 Value => count;

    public void Increment() mut
    {
        count++;
    }
}

public class Program
{
    public static int32 Main()
    {
        Counter counter = new Counter(0);
        counter.Increment();
        return counter.Value;
    }
}
```

Instance methods are non-mutating by default: they can read receiver state but cannot modify it. Add `mut` after the parameter list to allow receiver state to change. Static methods do not use `mut`.

Classes support constructors, fields, properties, indexers, nested types, static fields and methods, static constructors (`static this()`), and finalizers (`~this()`). Computed and automatic properties are supported. Static initialization runs lazily once before the first relevant use of a type.

Classes are sealed by default. Mark a class `virtual` to allow inheritance. Virtual methods and properties use explicit `virtual` and `override` declarations:

```lumasharp
public virtual class Animal
{
    public virtual string Sound() => "...";
}

public class Dog : Animal
{
    public override string Sound() => "woof";
}
```

Every class implicitly derives from `object`. Overrides must explicitly match the base member's signature, including `ref`, `in`, `out`, and `scope` parameter modifiers.

### Structs

Structs are value types. Assignment copies their fields; they are not allocated with `new` as classes are. A struct can have fields, methods, constructors, properties, indexers, and operators.

```lumasharp
public struct Point
{
    public int32 X;
    public int32 Y;

    public this(int32 x, int32 y)
    {
        X = x;
        Y = y;
    }
}

public class Program
{
    public static int32 Main()
    {
        Point point = Point(3, 4);
        return point.X + point.Y;
    }
}
```

Struct methods are also non-mutating unless marked `mut`. Structs cannot inherit from classes; the compiler treats their relationship to `object` as an implicit conversion/boxing behavior.

### Interfaces and enums

Interfaces describe methods, properties, and indexers that implementing types provide. Class and struct implementations are checked against the declared contracts.

```lumasharp
public interface IReadable
{
    int32 Read();
}
```

Enums define named values backed by an integral type. They work with equality, casts, and `switch`. The runtime's `System.Enum.GetValues<T>(value)` helper returns an array of the declared values for a concrete enum type.

```lumasharp
using System;

public enum Color : int32
{
    Red = 1,
    Green = 2,
    Blue = 3
}

public class Program
{
    public static int32 Main()
    {
        Color color = Color.Green;
        Color[] colors = Enum.GetValues(Color.Red);
        return (int32)color + colors.Length;
    }
}
```

## 8. Generic types and methods

Classes, structs, interfaces, and methods can be generic. Type arguments can often be inferred for method calls; generic types specify their arguments when constructed.

```lumasharp
public class Box<T>
{
    private T value;

    public this(T initial)
    {
        value = initial;
    }

    public T Get() => value;
}

public class Program
{
    public static int32 Main()
    {
        Box<string> greeting = new Box<string>("hello");
        return greeting.Get().Length;
    }
}
```

Constraints state what a type argument must support:

```lumasharp
public class Factory<T> where T : class, new()
{
    public T Create() => new T();
}

public class Slot<T> where T : default
{
    private T value = default(T);
}
```

Implemented constraints include `class`, `struct`, `enum`, `default`, base-class and interface constraints, and constructor constraints such as `new()` or a listed constructor signature. The `default` constraint accepts value types and nullable types, including nullable references. It rejects non-nullable class types because those have no default value. For example, `Slot<int32>` and `Slot<MyClass?>` are valid, while `Slot<MyClass>` is not.

Inside a generic method or type, use only operations guaranteed by the type's constraints. Generic specialization supports common class, struct, interface, array, nullable, delegate, and nested generic patterns. Some unusual combinations of overload inference remain unsupported; see [Current boundaries](#17-current-boundaries).

## 9. Delegates, lambdas, and captures

A delegate is a typed reference to a function. Delegates can point to static methods, instance methods, or lambdas.

```lumasharp
public delegate int32 Transform(int32 value);

public static class Helpers
{
    public static int32 AddOne(int32 value) => value + 1;
}

public class Program
{
    public static int32 Main()
    {
        Transform transform = Helpers.AddOne;
        Transform alsoAddOne = value => value + 1;
        int32 result = transform(41);
        return result == alsoAddOne(41) ? 0 : 1;
    }
}
```

Lambdas may capture local variables, parameters, and `this`. Captured mutable locals share storage with the closure, so updates remain visible across calls. Capturing a `ref`, `in`, or `out` parameter is rejected, as is allowing a scope-bound value to escape through a longer-lived closure.

Multicast delegates and conversions between unrelated delegate types are not supported.

## 10. Nullability

Reference types are non-nullable unless `?` is added. Value types can also be made nullable with `?`.

```lumasharp
class Example
{
    public static int32 Run()
    {
        string title = "Guide";
        string? subtitle = null;
        int32? selectedPage = null;
        return selectedPage ?? title.Length;
    }
}
```

The compiler tracks null checks through conditions and common control-flow paths. Use a null test to make a nullable value safe to access:

```lumasharp
using System;

public static class Example
{
    public static int32 Print(string? subtitle)
    {
        if (subtitle != null)
            Console.WriteLine(subtitle.Length.ToString());
        return 0;
    }
}
```

`!` is a null assertion. It tells the compiler that a value is not null at that point and performs a runtime check:

```lumasharp
public static class Example
{
    public static int32 GetLength(string? subtitle)
    {
        string nonNullSubtitle = subtitle!;
        return nonNullSubtitle.Length;
    }
}
```

`?.` and `?[...]` skip member or index evaluation when the receiver is null. `??` supplies a fallback. For nullable value types, arithmetic and comparisons use lifted nullable behavior where supported.

## 11. Memory, ownership, and scope

Ordinary class references are reference-counted. Assignments, parameters, fields, and returns maintain ownership automatically; when the last strong reference goes away, the object is destroyed deterministically. Class finalizers run during destruction. There is no `Dispose` pattern in the current runtime; resource-owning types expose `Close` and may also use finalizers.

Reference counting does not collect cycles. If two objects strongly reference one another, that cycle may remain allocated until it is broken. `Weak<T>` provides a non-owning reference to a class:

```lumasharp
using System;

public class Counter { }

public class Program
{
    public static int32 Main()
    {
        Counter counter = new Counter();
        Weak<Counter> weak = new Weak<Counter>(counter);
        if (weak.IsAlive)
        {
            Counter? current = weak.Target;
            return current == null ? 1 : 0;
        }
        return 1;
    }
}
```

Reading `Target` obtains a normal nullable strong reference. Store it only when you want to keep the object alive.

`scope` allocation requests a lexical lifetime. The compiler checks that a scope-bound reference does not escape its block through a return, field, heap container, or escaping closure.

```lumasharp
public class Counter
{
    private int32 value;
    public this(int32 initial) { value = initial; }
    public int32 Value => value;
}

public static class Examples
{
    public static int32 ReadTemporary()
    {
        Counter local = scope Counter(10);
        return local.Value;
    }
}
```

A `scope` parameter is a separate feature: it marks a call as borrowing for the duration of that method. Both ordinary heap objects and scope allocations may be passed to it; the callee cannot let the reference escape.

## 12. Arrays, strings, and spans

### Arrays

Managed arrays are reference types. The compiler supports single-dimensional, jagged, and rectangular arrays up to three dimensions. Indexing is bounds-checked in safe code. Arrays expose `Length`, `Rank`, and `GetLength(dimension)`.

```lumasharp
using System;

public class Program
{
    public static int32 Main()
    {
        int32[] scores = { 8, 13, 21 };
        scores[1] = 14;

        int32[,] grid = new int32[2, 3];
        grid[1, 2] = 7;

        for (let score in scores)
            Console.WriteLine(score.ToString());
        return grid[1, 2];
    }
}
```

Elements are initialized according to their type. Value elements start with their zero value; nullable references start as null. A positive-length array of a non-nullable class or `string` needs an initializer for every element, because those types do not have a default value.

```lumasharp
public class Example
{
    public static int32 Run()
    {
        string?[] optionalNames = new string?[4];
        string[] names = { "Ada", "Linus" };
        optionalNames[0] = names[0];
        return names.Length;
    }
}
```

Arrays convert to the common `System.Array` base and to `object`. In unsafe code, an array's `Ptr` property exposes a pointer to its first element (or null for an empty array).

Fixed-size inline arrays can be embedded in structs for low-level storage. They are an unsafe feature and differ from heap-allocated arrays.

```lumasharp
unsafe struct SmallBuffer
{
    int32[8] values;
}
```

### Strings

Strings are immutable references. Literals can be stored directly in the executable; dynamically created strings are managed and released when no longer referenced. String length and indexing count UTF-16 code units, while the native-facing storage uses UTF-8.

```lumasharp
public class Example
{
    public static int32 Run()
    {
        string name = "LumaSharp";
        string greeting = "Hello, " + name;
        char first = name[0];
        string part = name.Substring(0, 4);
        int32 at = name.IndexOf("Sharp");
        if (first != 'L')
            return 0;
        return greeting.Length + part.Length + at;
    }
}
```

The library includes substring and search helpers, prefix/suffix checks, replacement, trimming, character-array conversion, `Repeat`, `Concat`, and null/whitespace checks. Primitive `ToString()` returns the value (`True`, `-12`, `1.5`, and so on); other types use their override or their type name by default.

In an unsafe context, `string.Ptr` returns a `uint8*` to the current UTF-8 bytes. The bytes are NUL-terminated, but the string owns them; keep the string alive while using the pointer and do not write through it.

### Spans

`Span<T>` and `ReadOnlySpan<T>` are lightweight pointer-and-length views. Both check index bounds and slicing ranges. `Span<T>` allows writes; `ReadOnlySpan<T>` does not. Collections and arrays provide implicit conversions where supported, and `Span<T>` converts to `ReadOnlySpan<T>`.

```lumasharp
using System;

public class Example
{
    public static int32 First()
    {
        int32[] values = { 3, 5, 8 };
        Span<int32> view = values;
        ReadOnlySpan<int32> readOnly = view;
        int32 first = readOnly[0];
        return first;
    }
}
```

Span values borrow the underlying memory. Ref-struct rules prevent storing them in fields or arrays and prevent lambda capture. Keep a span within the lifetime of its source storage.

## 13. Collections

The current generic collection types are in `System.Collections`:

```lumasharp
using System.Collections;

public class Example
{
    public static int32 Second()
    {
        List<int32> numbers = new List<int32>();
        numbers.Add(10);
        numbers.Add(20);
        int32 second = numbers[1];
        return second;
    }
}
```

| Type | Common operations |
|---|---|
| `List<T>` | `Count`, `Capacity`, indexer, `Add`, `Insert`, `Remove`, `RemoveAt`, `Contains`, `IndexOf`, `Clear`, `ToArray`; iterates in list order. |
| `Stack<T>` | `Push`, `Pop`, `Peek`, `Clear`; iterates from the top of the stack down. |
| `Queue<T>` | `Enqueue`, `Dequeue`, `Peek`, `Clear`; iterates from the head of the queue. |
| `Dictionary<TKey, TValue>` | Indexer, `Add`, `TryAdd`, `ContainsKey`, `TryGetValue`, `Remove`, `Clear`. |
| `HashSet<T>` | `Add`, `Contains`, `Remove`, `Clear`; iteration order is unspecified. |

These are an early runtime surface, not a complete copy of .NET collections. `List<T>`, `Stack<T>`, `Queue<T>`, and `HashSet<T>` can be used in a `for-in` loop. When the variable has a concrete collection type, the compiler uses its `Iterate()` method and keeps a returned struct iterator as a value, avoiding an iterator allocation or boxing in the loop. When the variable is typed as `IEnumerable<T>`, iteration goes through the interface implementation instead.

The same pattern can be used by your own types:

```lumasharp
int32 total = 0;
for (let number in numbers)
    total += number;

```

Implement `Iterate()` with no parameters and return a type that implements `IIterator<T>`. A struct iterator is a good choice for allocation-free concrete iteration. The iterator must expose accessible `bool Next()` and `T Current` members. A collection can also explicitly implement `IEnumerable<T>.Iterate()` for code that uses the interface type.

The built-in collection iterators treat a default iterator or one constructed with a null collection as empty: `Next()` returns `false`, and `Current` returns `default(T)`.

Dictionary keys can be value types and enums; hash behavior can use `System.IHashable.Hash`, and custom equality is expressed by the root `System.IEquality<T>` interface. Dictionary enumeration and broader comparer APIs are not yet available. Hash-set iteration order is unspecified. Collection mutation during iteration has collection-defined behaviour.

The contiguous array-backed collections expose an unsafe `Ptr` to their backing storage. It is a low-level view: for a collection with a ring buffer or hash table, it does not imply that the logical contents form a packed sequence.

## 14. Exceptions

Exceptions are ordinary class objects derived from `System.Exception`. Use `throw`, typed `catch` clauses, catch-all clauses, rethrow, and `finally` for structured cleanup.

```lumasharp
using System;

public class Example
{
    public static void Check(int32 count)
    {
        try
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException();
        }
        catch (ArgumentOutOfRangeException error)
        {
            Console.WriteLine(error.Message);
        }
        finally
        {
            Console.WriteLine("finished");
        }
    }
}
```

`finally` runs on normal exit, `return`, loop transfers, and exception unwinding. `throw;` inside a catch rethrows the current exception. Runtime checks can raise exceptions for null assertions, failed casts, invalid indices, integer overflow, division by zero, allocation failure, and static initialization errors.

Exceptions expose a nullable `StackTrace` string. The runtime captures it at each explicit `throw value` site; a bare `throw;` preserves the trace from the original throw. To capture the current call stack directly, use `System.Diagnostics.StackTrace`. Its optional `skipFrames` argument skips caller frames, which is useful when a helper captures a trace on behalf of its caller:

`Exception.ToString()` formats the runtime exception type and message, followed by any inner exception and captured stack trace. An empty message omits the colon and message text.

```lumasharp
using System;
using System.Diagnostics;

public class Example
{
    public static void PrintCallStack()
    {
        StackTrace trace = new StackTrace();
        Console.WriteLine(trace.FrameCount.ToString());
        Console.WriteLine(trace.ToString());

        StackTrace callerTrace = new StackTrace(1);
        Console.WriteLine(callerTrace.ToString());
    }

    public static void Report(Exception error)
    {
        string? trace = error.StackTrace;
        if (trace != null)
            Console.WriteLine(trace);
    }
}
```

Each line names a compiled method, starting with the active method and continuing outward. File names and line numbers are not available yet.

## 15. Threads

`System.Threading.Thread` runs a delegate on a native operating-system thread. Use `ThreadStart` for a callback without arguments or `ThreadStart<T>` when the callback needs one argument:

```lumasharp
using System.Threading;

public class Worker
{
    public static void PrintValue(int32 value)
    {
        Console.WriteLine(value.ToString());
    }

    public static int32 Main()
    {
        Thread thread = new Thread();
        thread.Start<int32>(PrintValue, 42);
        if (!thread.Join(5000))
            thread.Abort();
        return 0;
    }
}
```

`Join()` waits until the callback finishes. `Join(millisecondsTimeout)` returns `false` if the wait expires; the worker keeps running. If the timeout expires, call `Abort()` to forcefully terminate the worker and wait until it has exited. A successful join releases the native thread resources. If the thread is not joined explicitly, its finalizer waits for the worker and releases those resources. `Thread.Sleep(millisecondsTimeout)` pauses the current thread. `Thread.CurrentThread` returns the same thread instance each time it is read on a given thread, and `IsCurrentThread` identifies that instance.

`Abort()` first waits for tracked runtime critical regions, including held monitors and thread callback cleanup, then forcefully ends the worker and blocks until it has exited. It can still interrupt ordinary user code and skip `finally` blocks or object cleanup outside those tracked regions. Use `Join()` first whenever the worker can finish normally.

## 16. Async methods, tasks, and locks

An `async` method names the value it will eventually produce. LumaSharp wraps that result in a task automatically: `async void` returns `Task`, while `async T` returns `Task<T>`. Do not write `Task` as the source return type.

```lumasharp
using System.Threading.Tasks;

public static class Calculations
{
    public static async int32 AddOneAsync(int32 value)
    {
        return value + 1;
    }

    public static async int32 DoubleAfterAddAsync(int32 value)
    {
        int32 updated = await AddOneAsync(value);
        return updated * 2;
    }

    public static int32 Main()
    {
        Task<int32> pending = DoubleAfterAddAsync(20);
        int32 result = pending.Result; // waits and returns 42
        return result == 42 ? 0 : 1;
    }
}
```

Inside an async method, `await` returns the task's result directly:

```lumasharp
public static async int32 ReadValueAsync()
{
    return await AddOneAsync(41);
}
```

Calling an async method creates a task and queues its body to a runtime-managed worker pool. `await` polls for completion while blocking the current thread; when the task is ready, the method continues on that same thread. The runtime grows the pool when queued work has no idle worker, adds workers to compensate for workers blocked in nested task waits, and retires idle workers over time. Programs do not size or configure the pool. `Task.Wait()` waits without throwing a task fault; `Task<T>.Result` and `await` rethrow it with the original throw trace and logical async caller frames. Generated helper methods stay out of printed stack traces.

Use `lock` to protect shared state. The lock expression is evaluated once, and the monitor is always released when the body exits, including when it throws:

```lumasharp
public static class SharedCounter
{
    private static object gate = new object();
    private static int32 value = 0;

    public static void Increment()
    {
        lock (gate)
        {
            value++;
        }
    }
}
```

Locks are reentrant and synchronize on object identity, as with C# monitors. `await` is not allowed inside a `lock` body. Async methods also cannot use `ref`, `in`, `out`, or `scope` parameters, return by reference, or mutate a struct receiver. Async lambdas, cancellation, custom schedulers, and synchronization contexts are not implemented yet.

## 17. Unsafe code and native pointers

Unsafe code is opt-in at build time with `--unsafe` and must also appear in an unsafe context. It provides pointer types (`T*`), address-of (`&`), dereference (`*`), pointer arithmetic, `sizeof`, inline arrays, and native `extern` calls.
Generic types may use their type parameter as a pointer's element type, as in `T*`, inside an unsafe context. The compiler resolves that pointer when it specializes the generic type.

```lumasharp
public static unsafe class PointerExamples
{
    public static int32 ReadFirst(int32[] values)
    {
        if (values.Length == 0)
            return 0;

        int32* first = values.Ptr;
        return *first;
    }
}
```

Pointers do not retain or release objects. The programmer must keep the referenced storage alive. A dangling or out-of-range pointer can cause undefined behavior. Safe array indexing remains checked unless `unchecked` is used inside an unsafe context.

Static `extern` methods bind to C ABI symbols at link time. LumaSharp does not marshal managed objects, strings, arrays, or delegates for extern calls; pass C-compatible values and pointers only. The current runtime's OS file and console operations are compiler-provided native calls, so users can work with `System.IO` without shipping a separate C source shim.

`System.Interop.NativeMemory` provides raw, untracked native allocation and byte operations. `Allocate` returns uninitialized memory; `AllocateZeroed` initializes every byte to zero. `Zero` clears an existing region and returns its pointer. Use these only when you need storage outside normal LumaSharp ownership:

```lumasharp
using System.Interop;

public static unsafe class NativeBufferExample
{
    public static int32 Main()
    {
        int32* values = (int32*)NativeMemory.AllocateZeroed(sizeof(int32) * 2);
        if (values == null)
            return 1;

        values[0] = 12;
        NativeMemory.Copy(values + 1, values, sizeof(int32));
        bool copied = values[1] == 12;
        NativeMemory.Free(values);
        return copied ? 0 : 1;
    }
}
```

`Copy` requires non-overlapping memory; use `Move` when the source and destination may overlap. A negative byte count throws `ArgumentOutOfRangeException`. Allocation returns null on failure. The caller must ensure pointers and byte counts describe valid memory, and must free each allocation exactly once. Native memory is not released automatically.

`System.Interop.NativeLibrary` loads a library by path, looks up exported symbols, and can unload explicitly. Its finalizer performs a best-effort unload. Symbol addresses are raw `void*` values: they become invalid after unload, and this language version cannot invoke a function pointer directly. Paths are strings and are converted to the platform representation by the runtime; no C source shim is needed.

```lumasharp
using System.Interop;

public static unsafe class NativeLibraryExample
{
    public static int32 Main()
    {
        NativeLibrary library = new NativeLibrary("example-native-library");
        if (!library.IsLoaded)
            return 1;

        void* entry = library.GetProcAddress("example_entry");
        bool found = entry != null;
        library.Unload();
        return found ? 0 : 1;
    }
}
```

## 18. Runtime library

The compiler package includes the LumaSharp runtime sources and locates them beside its installation. In a source checkout it also searches source-tree ancestors for `LumaSharp.Library`. Projects can override discovery with `runtimeLibraryPath` in JSON or `--runtime-library` on the command line. The current library contains core types such as `Object`, `Array`, `Exception`, primitive keyword backing types, `String`, `Span<T>`, `ReadOnlySpan<T>`, `Weak<T>`, and the collections described above.

`System.Console` currently writes strings and chars through `Write` and `WriteLine`. To print a number or boolean, convert it explicitly with `ToString()`. `System.IO.TextWriter` also provides primitive overloads for derived writers.

`System.Environment` exposes the running executable's path, directory, and file name, the current processor count, and whether the platform is little-endian. Its `Platform` and `Architecture` properties report the target used to build the executable. The corresponding `System.Platform` values are `Windows`, `Linux`, `MacOS`, and `Unknown`; `System.Architecture` provides `X86`, `X64`, `Arm`, `Arm64`, and `Unknown`.

`System.Diagnostics.StopWatch` measures elapsed time with a monotonic high-resolution clock. Use `Start`, `Stop`, `Reset`, or `Restart`, or create and start one with `StartNew()`. Its `Elapsed` property returns `System.TimeSpan`, which exposes `Ticks`, `TotalMilliseconds`, and `TotalSeconds`; one tick is 100 nanoseconds. Timing uses `QueryPerformanceCounter` on Windows and `clock_gettime(CLOCK_MONOTONIC)` on Linux and macOS.

```lumasharp
using System;
using System.Diagnostics;

public class Program
{
    public static int32 Main()
    {
        StopWatch watch = StopWatch.StartNew();
        int32 value = 0;
        while (value < 100000)
            value++;
        watch.Stop();
        Console.WriteLine(watch.Elapsed.TotalMilliseconds.ToString());
        return 0;
    }
}
```

The synchronous `System.IO` layer includes `Stream`, `MemoryStream`, `FileStream`, `File`, `Directory`, `Path`, `TextReader`, and `TextWriter`. `Stream.Read` and `Stream.Write` operate on byte arrays; `MemoryStream` supports seeking and exposes an unsafe pointer to its buffer. `FileStream` uses native OS operations, and handles can be released with `Close` or its finalizer.

The IO surface is intentionally small: asynchronous methods, timeouts, `Dispose`, recursive directory deletion, file enumeration, and full canonical path normalization are not implemented.

Some low-level runtime operations use compiler-reserved names beginning with `__`. They let the runtime reach platform services such as memory allocation, file APIs, and timing without declaring those operations as application-facing `extern` methods. These names are for runtime source files only; ordinary programs should use public runtime classes such as `NativeMemory`, `NativeLibrary`, `File`, and `Environment`.

## 19. Current boundaries

LumaSharp implements a broad language subset, but this guide is not a promise of complete C# compatibility. Keep these current limits in mind:

- The specifications describe some future behavior as well as current behavior. This guide focuses on functionality implemented in the compiler/runtime.
- There is no garbage collector. Strong reference cycles are not reclaimed automatically; use weak references or break cycles explicitly.
- Generic method inference and generic constrained operations have edge cases that are still being completed.
- `List<T>` does not currently participate in the generic enumerable protocol. Some collection APIs, including dictionary enumeration and custom comparers, are absent.
- Unsafe code can bypass checks and create dangling pointers; use it only when needed.
- Native interop supports static C ABI calls and runtime library loading/symbol lookup. Function-pointer invocation, automatic marshalling, and callbacks into LumaSharp are not part of the current surface.
- IO is synchronous and minimal; timeouts and broad .NET `File`/`Directory` APIs are not present.
- Async lambdas, cancellation, custom schedulers, synchronization contexts, LINQ, reflection, attributes, and the complete .NET base class library are not included.

When behavior matters for a particular edge case, consult the tests and the specifications. The test suite reflects the compiler's executable behavior; the specifications also describe intended rules that may not yet be implemented.

## 20. Further reading

- [Quick Reference](Overview.md) — compact syntax lookup.
- [Language and Runtime Specification](../Specification/LumaSharp_LanguageSpecification.md) — detailed normative language and runtime rules.
- [Compiler Specification](../Specification/LumaSharp_CompilerSpecification.md) — CLI, project settings, outputs, diagnostics, and compiler behavior.
- [Implementation Roadmap](../Progress/IMPLEMENTATION_ROADMAP.md) — current implementation status and open work.
- [Repository README](../README.md) — build requirements and repository entry points.

