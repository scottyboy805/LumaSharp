# LumaSharp
C# programming language reimagined

# Motivation
C# is a powerful and mature language with frequent new features added all the time, and as a result has a certain amount of ambiguity as those features have been built upon over the years. Quite simply there are many ways to achieve the same thing in the language which can make it feel disjointed in some cases in my opinion. LumaSharp is my personal idea of what I think the perfect C# inspired language may look like if it was redesigned from stratch today, taking into consideration all the current features that a language like C# has to offer. The main goals are ease of use/simple or familiar syntax, and to eliminate as much ambiguity as possible without compromising on features.

# Progress
WIP - Define language specification and usage (Common language features outlined below but subject to change).  
WIP - Implement language parsing using Antlr4 (Partially implemented - work is on going to support all proposed features).  
WIP - Semantic analysis to ensure lanugage usage is valid (Link types, symbols, give errors/warnings, etc.).
TODO - Implement bytecode compiler to produce an executable format (Work required to transform the AST produced from parsing the source into semantic model for validation, and then into a stack based common bytecode instruction set).  
TODO - Implement bytecode runtime to execute code as an application or library (Create a bytecode runtime in software only (No JIT) just as a proof of concept to execute instructions).  

# Proposed syntax
Here is the proposed syntax for the language which may be subject to change: [Language Specification](https://github.com/scottyboy805/LumaSharp/blob/f43f670e914320d4399f02a3af5fbfb467bf7472/LumaSharp%20Specification/Overview.md)

