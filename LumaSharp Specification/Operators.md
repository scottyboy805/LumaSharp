Much like C#, types in LumaSharp have a few common methods that can be overriden by a type. How they are used and implemented is slightly different to C#

```cs
// A simple type declaration
type MyType{}

// Later we can use the following methods
MyType a = MyType();
MyType b = MyType();

bool equal = a == b;
i32 hash = a.Hash + b.Hash;
string asString = (string)a;
```

Much like C# we have equals, hash code and string conversion methods available for all types by default. They can also be overriden per type using the special reserved operator names:

```cs
type MyType
{
  i32 value = 0;

  global i32 op_hash => read: return value.Hash;

  global bool op_equal(MyType& a, MyType& b)
  {
    return a.value == b.value;
  }

  global string op_string(MyType& a)
  {
    return "MyType: ${a.value}";
  }
}
```

Similarly we can also override or define other arithmetic operators like so:

```cs
type MyType
{
  float value = 0F;

  export this(float val) => value = val;

  global MyType op_add(MyType& a, MyType& b)
  {
    return MyType(a.value + b.value);
  }

  global bool op_greater(MyType& a, MyType& b)
  {
    return a.value > b.value;
  }
}
```
