### Evaluation Stack Structure
The eval stack is a vital part of a bytecode evauated runtime. This section outines how the stack is structured and how certain operations will affect it.

#### Call Scope
When a method is invoked at runtime, we enter a new portion of the stack region prepared specifically for the call. For the provided method layout we would have stack structure shown below:
```
void ExampleMethod(i32 a, string b)
{
  i8 myLocal1 = 0;
  MyType myLocal2 = new MyType();
  MyType myLocal3 = MyType();
}
```

#### Stack layout
The stack is simply represented as memory bytes. All required type information must be provided along with instructions or be implicitly determined from the instruction to know how many bytes to access on the stack for example.
This is why even simple operations like `Add` must provide a primitive type token.

[Max Stack Size - Precalulcated value = 8]  
##### --- 0x0000000  Call entry - Start of method invoke
##### ArgPtr 0x0000000 Method arguments  
[0-7] - implitict `this` arg0  (For non-global methods)    // 8 byte reference  
[8-11] - argument `a` arg 1                                // 4 byte i32  
[12-19] - argument `b` arg2                                // 8 byte reference  

##### LocPtr 0x14000000 Method locals (Zeroed memory)
[20] - `myLocal1` loc0                  // 1 byte i8  
[21-28] - `myLocal2` loc1               // 8 byte reference  
[29, 36] - `myLocal3` loc2              // 8 byte reference  

##### StackPtr 0x25000000 Execution space - for stack operations like load, store, arithmetic, etc.  
[37-44] - general purpose stack space - just the size of reference in this case for operations like `myLocal3 = MyType()` 

##### AllocPtr 0x2D000000 Stack allocation space - for stack allocations such as `MyType()`  
[45-XXX] - dynamic allocation - as large as required without stack overflow

*Important - New call entries must be placed on the stack after all current dynamic allocations to avoid corruption*
##### NextCallPtr = AllocPtr + dynamic allocations size

#### Stack allocation
When an instance or other data is allocated on the stack, the following process occurs (For `MyType` as an example):

1. Get the next available dynamic allocation address for the stack (AllocPtr + previous allocation sizes)
2. Zero the memory region at the dynamic allocation address for the allocation size required by `MyType`
3. Create an instance from the newly zeroed memory and run the constructor with arguments provided on execution space of the stack
4. Push the address of the instance back on to the stack (Evaluation region) at the current stack pointer

*All instances created store an address of the object even when allocated on the stack. Then when calling a method with an argument that is not passed by reference, the runtime should copy the instance data stored at the address (unbox) as an argument to achive the `Pass By Value` behaviour that would be expected*
