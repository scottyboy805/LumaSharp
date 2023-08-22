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

### Enums:
```cs
// C# enums are pretty close to prefect in my opinion so no real changes here
export enum MyEnum: u16
{
  Item1 = 10,
  Item2 = Item1,
}
```

### Access Modifiers
```cs
default visibility (No keyword required) // The member is visible by the current and all derived members - Same as C# protected
export // The member is accessible from the current and other libraries: IE. the type visiblity is exported. Same as C# public
internal // The member is  accessibly from the current library in any context, but not from external libraries - Same as C# internal
hidden // The member is only visible to the parent member (Only suitable for nested members) - Same as C# private
global // (Only suitable for field, accessor and method) The member is globally accessible via the type qualifier - Same as C# static
```

### Fields
```cs
// Fields are quite similar to C# - Only real difference is that they are automatically accessible to all derived types unless the 'hidden' access modifier is used
type MyType{
  hidden i32 myField = 6 + 12;
}
```

### Accessors
```cs
// Similar to C# properties although the aim is to make usage as easy as possible with only 2 possible usages - C# has too many variations for my liking
type MyType{
  export i64 MySimpleAccessor => myLongVariable;

  internal float MyAccessor
    => read: return myFloatVariable;
    => write: {
      CheckInputValue(input);
      myFloatVariable = input;
    }

  internal float MyReadOnlyAccessor => read: return myFloatVriable;
}
```

### Methods
```cs
// Much the same as C# methods - Only real thing to talk about is variable size parameters lists
type MyType{
  // Simple inlined method
  i8 MySimpleMethod(i16 val) => return (i8)val;

  // Simple pass by reference method - Same as using C# 'ref' keyword - but should also be used instead of 'in' and 'out'
  void MyRefMethod(i32& val) => val++;

  // Simple method with variable length parameter
  export i32 MyVariableParamMethod(i32 values ...)
  {
    // Values is converted to i32 array containing proveded number of parameters
    return values.Count;
  }
}
```

### Tasks
```cs
// A task is essentially just a C# delegate - a way to store a method as a variable to be invoked at a later time
type MyType{
  task i32 MyTask(i32 a, i32 b);

  void MyMethod()
  {
    // Create and call standard
    MyTask callA = (i32 a, i32 b) => return a + b;
    i32 result = callA(3, 5);

    // Unlike C# we can cast to common base and dynamic invoke
    task callB = callA;
    i32 result = (i32)callB(4, 6);
  }
}
```

### Attributes
```cs
// Custom attributes are very useful in C#, although I think they can be improved slightly using a hash tag type syntax
#Serializable
#UIDisplayable("My Type", 100)
type MyType{}
```
