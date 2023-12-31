# LumaSharp
C# programming language reimagined - Intended as a fun side project/learning exercise, but hope to grow into something useable and feature complete.

# Motivation
C# is a powerful and mature language with frequent new features added all the time, and as a result has a certain amount of ambiguity as those features have been built upon over the years. Quite simply there are many ways to achieve the same thing in the language which can make it feel disjointed in some cases in my opinion. LumaSharp is my personal idea of what I think the perfect C# inspired language may look like if it was redesigned from stratch today, taking into consideration all the current features that a language like C# has to offer. The main goals are ease of use/simple or familiar syntax, and to eliminate as much ambiguity as possible without compromising on features.

# Progress
WIP - Define language specification and usage (Common language features outlined below but subject to change).  
- [x] [Language Specification](https://github.com/scottyboy805/LumaSharp/blob/main/LumaSharp%20Specification/Language/Overview.md)
- [X] [Runtime Specification](https://github.com/scottyboy805/LumaSharp/blob/main/LumaSharp%20Specification/Runtime/Instructions.md)

WIP - Implement language parsing using Antlr4 (Partially implemented - work is on going to support all proposed features).  
- [X] Implement Antlr grammar for language specification - subject to change and refinement as the language structure develops.
- [X] Implement Antlr expressions, statements, members and units for language specification.
- [X] Generate structured parse tree using Antlr runtime.
- [X] Implement Syntax tree API for working with parsed language.
- [X] Implement conversion from Antlr parse tree to syntax tree.
- [ ] Fully test parsing source text to syntax tree for all cases.

WIP - Semantic analysis to ensure lanugage usage is valid (Link types, symbols, give errors/warnings, etc.).  
- [X] Convert parsed syntax tree to equivilent semanitc model.
- [ ] Implement loading external modules for referencing purposes.
- [ ] Implement type checking, inference, accessibility rules etc.
- [X] Implement resolving symbols and identifiers based on scope (Partially implemented).
- [ ] Semantic model validation to ensure that an output can be generated.

TODO - Implement bytecode compiler to produce an executable format (Work required to transform the AST produced from parsing the source into semantic model for validation, and then into a stack based common bytecode instruction set).  
- [X] Define bytecode model used for runtime and compiler purposes (instruction set, format, memory) (Partially implemented [Runtime Specification](https://github.com/scottyboy805/LumaSharp/blob/main/LumaSharp%20Specification/Runtime/Instructions.md).
- [X] Generate bytecode instruction set from semantic model statements (Partially implemented).
- [ ] Emit intermediate module that can be loaded and executed at runtime.

TODO - Implement bytecode runtime to execute code as an application or library (Create a bytecode runtime in software only (No JIT) just as a proof of concept to execute instructions).  
- [ ] Long term - complete work on compiler first.

# Proposed syntax
Here is the proposed syntax for the language which may be subject to change: [Language Specification](https://github.com/scottyboy805/LumaSharp/blob/main/LumaSharp%20Specification/Language/Overview.md)

# Sponsors
You can sponsor this project to help it grow
[:heart: Sponsor](https://github.com/sponsors/scottyboy805)
