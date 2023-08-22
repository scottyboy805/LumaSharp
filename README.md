# LumaSharp
C# programming language reimagined

# Motivation
C# is a powerful and mature language with frequent new features added all the time, and as a result has a certain amount of ambiguity as those features have been built upon over the years. Quite simply there are many ways to achieve the same thing in the language which can make it feel disjointed in some cases in my opinion. LumaSharp is my personal idea of what I think the perfect C# inspired language may look like if it was redesigned from stratch today, taking into consideration all the current features that a language like C# has to offer.

# Progress
WIP - Define language specification and usage.
WIP - Implement language parsing using Antlr4 .
TODO - Implement bytecode compiler to produce an executable format.
TODO - Implement bytecode runtime to execute code as an application or library.

# Proposed syntax
Here is the proposed syntax for the language which may eb subject to change:

Primitive types:
```cs
// Primitive integer types include the size/unsigned information in the name to easily understand how it is represented in memory
i8, u8, i16, u16, i32, u32, i64, u64, single, double, bool, string, char
```

Imports:
```cs
import Collections;
import Collections.Generic;
import MyList as Collections.Generic.List<i8>;
```
