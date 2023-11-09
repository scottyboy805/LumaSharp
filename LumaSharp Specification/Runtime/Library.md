# Runtime Library Format
The runtime will use the following structure/memory layout when loading libraries into memory. All memory should be nativley allocated so that it can be referenced quick by address (function address for exmple). There are 2 distinct aspects of the library
1. Library metadata - Describes types and members along with attributs such as visibiltiy. The metadata aspect can be loaded in isolation without requiring the library to initialize the actual executable code.
2. Executable - The actual memory structures and instructions that can be executed by the runtime.

## Metadata

## Executable

Type Address -> Points to start of type handle  
[TypeHandle]  
Small structure descibing type allocation size and also stores the type token pointing to the metadata type

Field Handles address -> Points to start of field handles (if any available)
[FieldHandles]

      
