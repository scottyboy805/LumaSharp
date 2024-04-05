### Local Variables
Local variable declarations are very similar to C# for the most part, and will be initialized to a default value unless explicitly assigned:
```cs
// Will be auto-initialized to 0, unlike C# which would give a compiler error if yhou attempt to use the value
u64 myDefaultVariable;

// Local variables are similar to normal C# : type id (optional assign)
i32 myLocalVariable = 123;

// It is also possible to initialize multiple variables similar to C# but with a different syntax
i32 variableA, variableB = 50;  // Both variables initialized with value '50'

// And you can also initialize each with a unique value
string varaibleA, variableB = { "Hello", "World" }
```

The only notable exception is that LumaSharp does not have a var keyword, but you can instead declare a variable with an inferred type using the `:=` operator. When using this operator it is no longer required to declare the variable type, unless the type cannot be inferred automatically. Note that an assignment is requried in this case otherwise a compiler error will be generated:
```cs
myInteger := 123;

myString := "Hello World";
```

### Loops
Luma sharp only supports loops using the `for` keyword, although the syntax supports most loop cases that you would expect using slightly differing syntax.  
  
An infinite loop is as simple as this:
```cs
for
{
  // This will run forever unless a break or return statement is used
}
```

A simple condition loop can be used where a bool or integer operand is used as the looping condition:
```cs
// An infinite loop with explicit condition
for(true){}

// LumaSharp supports inferring integer values as boolean for conditional checks, this is essentially the same as above:
for(1){}
```

A traditional for loop is supported using standard C# syntax:
```cs
for(i32 i = 0; i < 5; i++)
{
  // This will run 5 times where i = 0 - 4
}

// Optionally the infer assign operator can be used
for(i := 0; i < 5; i++);
```

Foreach loops are supported to iterate over items in a collection. The operand must implement `CIterator` in order to support for each usage:
```cs
i32[] arr = { 1, 2, 3, 4, 5 };
for(i32 i in arr)
{
  // This will run 5 times where i = 1 - 5  
}
```

Finally loops can support ranges using the special `..=` or `..<` range operators to denote a range that is either inclusive or exclusive:
```cs
for(i32 i in 0 ..< 5)
{
  // This will run 5 times where i = 0 - 4
}

// You can also use identifiers instead of literals as you might expect:
i32 min, max = { 0, 5 };
for(i32 i in min ..= max)
{
  // This will run 6 times since the inclusive operator is used where i = 0 - 5
}
```
