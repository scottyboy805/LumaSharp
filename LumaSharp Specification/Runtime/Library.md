# Runtime Library Format
The runtime will use the following structure/memory layout when loading libraries into memory. All memory should be nativley allocated so that it can be referenced quick by address (function address for exmple). There are 2 distinct aspects of the library
1. Library metadata - Describes types and members along with attributs such as visibiltiy. The metadata aspect can be loaded in isolation without requiring the library to initialize the actual executable code.
2. Executable - The actual memory structures and instructions that can be executed by the runtime.

## Metadata
TODO

## Executable

###### Type Address -> Points to start of type handle  
[TypeHandle]  
Small structure descibing type allocation size and also stores the type token pointing to the metadata type:  
- TypeToken - point to the metadata type
- TypeSize - the size of memory required to store an instance of this type

###### Field Table address -> Points to start of field handles (if any available)
[FieldHandles (Many)]  
Stores one or more field handles describing an individual field defined on the type:  

###### Field Address -> Points to start of specific field handle  
[Field Handle]  
Small structure describing field memory size and also stores the field token pointing to the metadata field:  
- FieldToken - point to the metadata field
- FieldOffset - the offset into memory of this field (instancePtr + offset)
- FieldSize - the size of memory required to store this field type (i32 = 4 for example)

###### Global Address -> Points to global field memory for this type  
[Raw memory - fields]  
Raw memory space allocated to store all global fields declared on the type taking into account each field size.  

###### Method Table address -> Points to start of method handles (if any available)  
[MethodHandles (Many)]  
Stores one or more method handles describing an individual method defined on the type:  

###### Method Address -> Points to start of a specific method handle  
[Method Handle]  
Small structure describing arguments, locals and instructions of a method:  
- MethodToken - point to the metadata method
- MaxStack - the max stack size needed to invoke this method
- LocalOffset - the offset where local variable handles are stored
- LocalPtrOffset - the offset where local variables are stored on the stack
- StackPtrOffset - the offset where the evaluation stack should start
- ArgLocalPtr - a pointer to all argument and local handles for this method
- InstructionPtr - a pointer to bytecode instructions for this method

###### ArgLocals Address -> Points to start of arg/local stack handles
[Stack Handles (Many)]  
Stores one or more stack handles describing all arguments and locals for this method

[Stack Handle]  
Small structure describing a method argument or local stored on the stack
- TypeHandle - the type handle for arg/local type
- Offset - the offset from the start of the call stack pointer where the arg/local is stored

###### Execution Address -> Points to start of bytecode for this method  
[Raw Memory - bytecode instructions]  
      
