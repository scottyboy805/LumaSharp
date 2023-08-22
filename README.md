# LumaSharp
C# programming language reimagined

# Motivation
C# is a powerful and mature language with frequent new features added all the time, and as a result has a certain amount of ambiguity as those features have been built upon over the years. Quite simply there are many ways to achieve the same thing in the language which can make it feel disjointed in some cases in my opinion. LumaSharp is my personal idea of what I think the perfect C# inspired language may look like if it was redesigned from stratch today, taking into consideration all the current features that a language like C# has to offer.

# Progress
WIP - Define language specification and usage.  
WIP - Implement language parsing using Antlr4.  
TODO - Implement bytecode compiler to produce an executable format.  
TODO - Implement bytecode runtime to execute code as an application or library.  

# Proposed syntax
Here is the proposed syntax for the language which may eb subject to change:

### Primitive types:
```cs
// Primitive integer types include the size/unsigned information in the name to easily understand how it is represented in memory
i8, u8, i16, u16, i32, u32, i64, u64, single, double, bool, string, char
```

### Imports:
```cs
// Import seems like a better fit rather than using
import Collections;
import Collections.Generic;
import MyList as Collections.Generic.List<i8>;  // Aliasing is supported
```

### Namespaces:
```cs
// Much the same, no changes needed here
namespace My.Root.Namespace{}
```
### Types:
```cs
// No class or struct, just the 'type' keyword to declare a new user type with support for generics, inheritance, and multiple contract implementation.
// Types can be value or reference based depending upon how they are allocated
export type MyType<T0, T1: enum> : MyBaseType<T0>, CIterator<T0>, CResource{}

// Stack allocated
MyType<i8, MyEnum> myVar = MyType<i8, MyEnum>();

// Heap allocated - reference type returned by 'new' and referenced by '&'
MyType<i8, MyEnum>& myHeapVar = new MyType<i8, MyEnum>();
```

### Contracts:
```cs
// Contracts are essentially interfaces but I think contract has a better meaning in such a case
internal contract CResourceCollection<T> : CIterator<T>, CDispose{}
```
