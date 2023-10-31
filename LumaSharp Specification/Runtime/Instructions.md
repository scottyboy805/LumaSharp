# Bytecode Instruction Set

The runtime and compiler will use the following bytecode instruction set which should support all possible use cases of the language. 
Inspiration taken from C# CLR and JVM to outline the minimum required instruction set:

All OpCodes `[code]` are stored as 1 byte

### Default Op
`Nop` - No operation - debug marker only

### Constant
`Ld_I1`: [code, 1 byte] Load 8-bit integer constant onto the stack as 32-bit integer  
`Ld_I2`: [code, 2 bytes] Load 16-bit integer constant onto the stack as 32-bit integer  
`Ld_I4`: [code, 4 bytes] Load 32-bit integer constant onto the stack as 32-bit integer  
`Ld_I8`: [code, 8 bytes] Load 64-bit integer constant onto the stack as 64-bit integer  
`Ld_F4`: [code, 4 bytes] Load 32-bit floating point constant onto the stack  
`Ld_F8`: [code, 8 bytes] Load 64-bit floating point constant onto the stack  
`Ld_Str`: [code, 4 byte token] Load a string value onto the stack from the string table with the provided token  
`Ld_Null`: [code] Load null pointer onto the stack  

#### Optimized Instructions:  
`Ld_I4_0`: [code] Load the value `0` onto the stack as a 32-bit integer  
`Ld_I4_1`: [code] Load the value `1` onto the stack as a 32-bit integer  
`Ld_I4_M1`: [code] Load the value `-1` onto the stack as a 32-bit integer  
`Ld_F4_0`: [code] Load the value `0` onto the stack as a 32-bit floating point value  

### Locals  
`Ld_Loc_0`: [code] Load the value from the local variable at index `0` onto the stack  
`Ld_Loc_1`: [code] Load the value from the local variable at index `1` onto the stack  
`Ld_Loc_2`: [code] Load the value from the local variable at index `2` onto the stack  
`Ld_Loc`: [code, 1 byte] Load the value from the local variable at index `X` onto the stack. Value `X` represents an 8-bit index value  
`Ld_Loc_E`: [code, 2 bytes] Load the value from the local variable at index `X` onto the stack. Value `X` represents a 16-bit index value  
`Ld_Loc_A`: [code, 1 byte] Load the address of the local variable at index `X` onto the stack. Value `X` represents an 8-bit index value  
`Ld_Loc_EA`: [code, 2 bytes] Load the address of the local variable at index `X` onto the stack. Value `X` represents a 16-bit index value  
