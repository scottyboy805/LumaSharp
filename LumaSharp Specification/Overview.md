### Primitive types:
```cs
// Primitive integer types include the size/unsigned information in the name to easily understand how it is represented in memory
i8, u8, i16, u16, i32, u32, i64, u64, single, double, bool, string, char

// Any is a special type that can represent any built in or user type as a common base - Similar to C# object keyword - Leads to allocation on the heap same as c#
any = 123;
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
// Much the same, only the namespace separator character is `:` to avoid ambiguity.
// For example: `MyNamespace.MySomething.MyType` in C# `MySomething` could be either a namespace or a type which cannot be determined statically.
// That potential ambiguity is removed in Luma sharp: `MyNamespace:MySomething.MyType` as we can see that `MySomething` in indeed a type.
// Also just use block syntax only where C# supports `namespace Example;` type syntax which is not needed in my opinion
namespace My:Root:Namespace{}
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

```cs
// Possible alternative to eliminate the need to use reference syntax when declaring variables is to support `valuetype` keyword
// MyValueType will be allocated on the stack, but there is still support for generics and inheritance
export valuetype MyValueType<T> : MyBaseValueType{}
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

// Only thing to mention is the bitmask attribute which forces the enum values to be unique bit values, useful for flag/bitmask values:
#BitMask
export enum MyFlags: u8
{
  Item1,    // 1
  Item2,    // 2,
  Item3,    // 4,
  Item4,    // 8 and so on...
  MyItem = 7, // ERROR

  MyDuplicateItem = 4, // Allowed
  MyCombinedItem = Item1 | Item3, // Allowed
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

### Constructors
```cs
// Constructors are similar to C# with no return value, but use either the `this` of `global` keyword as I think it makes more sense and means you don't have to write out the type name every time
type MyType{
  // Empty constructor
  hidden this(){}

  // Parameter constructor
  export this(i32 size) base: (size){}

  // Call through constructor
  internal this(i64& bigSize) this: ((i32)bigSize){}

  // Global costructor - same as static constructor in C#
  internal global(string name){}
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

## A Complete Example - Bubble Sort
```cs
import Collections.Generic;

// Enclosing namespace
namespace BubbleSort.Example
{
  // Main type
  global type Program
  {
    // List of values to sort
    hidden List<i32>& unsortedValues = { 800, 11, 50, 771, 649 };
  
    // Main entry point to the program
    export global void Main()
    {
      BubbleSort(unsortedValues);
    }
  
    // Algorithm method
    export void BubbleSort(List<i32>& values)
    {
      i32 temp = 0;
  
      for(i32 i = 0; i < values.Count; i++)
      {
        for(int j = 0; j < values.Count - 1; j++)
        {
          if(values[j] > values[j + 1])
          {
            temp = values[j + 1];
            values[j + 1] = values[j];
            values[j] = temp;
          }
        }
      }
    }
  }
}
```
