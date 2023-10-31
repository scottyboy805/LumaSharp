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
`Ld_Loc_0`: [code] Load the value from the local variable at index `0` onto the stack.
`Ld_Loc_1`: [code] Load the value from the local variable at index `1` onto the stack  
`Ld_Loc_2`: [code] Load the value from the local variable at index `2` onto the stack  
`Ld_Loc`: [code, 1 byte] Load the value from the local variable at index `X` onto the stack. Value `X` represents an 8-bit index value  
`Ld_Loc_E`: [code, 2 bytes] Load the value from the local variable at index `X` onto the stack. Value `X` represents a 16-bit index value  
`Ld_Loc_A`: [code, 1 byte] Load the address of the local variable at index `X` onto the stack. Value `X` represents an 8-bit index value  
`Ld_Loc_EA`: [code, 2 bytes] Load the address of the local variable at index `X` onto the stack. Value `X` represents a 16-bit index value  
`St_Loc_0`: [code] Store the value on top of the stack in the variable at index `0`.
`St_Loc_1`: [code] Store the value on top of the stack in the variable at index `1`  
`St_Loc_2`: [code] Store the value on top of the stack in the variable at index `2`
`St_Loc`: [code, 1 byte] Store the value on top of the stack in the variable at index `X`. Value `X` represents an 8-bit index value
`St_Loc_E`: [code, 1 bytes] Store the value on top of the stack in the variable at index `X`. Value `X` represents a 16-bit index value

### Arguments  
`Ld_Arg_0`: [code] Load the value from the argument at index `0` onto the stack. Reserved for `this instance` in non-global methods  
`Ld_Arg_1`: [code] Load the value from the argument at index `1` onto the stack  
`Ld_Arg_2`: [code] Load the value from the argument at index `2` onto the stack  
`Ld_Arg`: [code, 1 byte] Load the value from the argument at index `X` onto the stack. Value `X` represents an 8-bit index value  
`Ld_Arg_E`: [code, 2 bytes] Load the value from the argument at index `X` onto the stack. Value `X` represents a 16-bit index value  
`Ld_Arg_A`: [code, 1 byte] Load the address of the argument at index `X` onto the stack. Value `X` represents an 8-bit index value  
`Ld_Arg_EA`: [code, 2 bytes] Load the address of the argument at index `X` onto the stack. Value `X` represents a 16-bit index value  
`St_Arg_0`: [code] Store the value on top of the stack in the argument at index `0`. Invalid in non-global methods  
`St_Arg_1`: [code] Store the value on top of the stack in the argument at index `1`  
`St_Arg_2`: [code] Store the value on top of the stack in the argument at index `2`
`St_Arg`: [code, 1 byte] Store the value on top of the stack in the argument at index `X`. Value `X` represents an 8-bit index value
`St_Arg_E`: [code, 1 bytes] Store the value on top of the stack in the argument at index `X`. Value `X` represents a 16-bit index value

### Fields
`Ld_Fld`: [code, 4 bytes] Load the field value with the provided token onto the stack. Instance must be pushed to stack first for non-global fields  
`Ld_Fld_A`: [code, 4 bytes] Load the address of the field with the provided token onto the stack. Instance must be pushed to the stack first for non-global fields  
`St_Fld`: [cpde, 4 bytes] Store the value on top of the stack in the field with the provided token  

### Arrays
`Ld_Elem`: [code] Load the element at the provided index from the provided array onto the stack. Array instance followed by index must be pushed to stack first  
`Ld_Elem_A`: [code] Load the address of the element at the provided index from the provided array onto the stack. Array instance followed by index must be pushed to stack first  
`St_Elem`: [Code] Store the value on top of the stack in the array at the provided index. Array instance followed by index must be pushed to stack first  

### Indirect
`Ld_Addr_I1`: [code] Load 8-bit value from the given address onto the stack. Address must first be loaded onto the stack  
`Ld_Addr_I2`: [code] Load 16-bit value from the given address onto the stack. Address must first be loaded onto the stack 
`Ld_Addr_I4`: [code] Load 32-bit value from the given address onto the stack. Address must first be loaded onto the stack  
`Ld_Addr_I8`: [code] Load 64-bit value from the given address onto the stack. Address must first be loaded onto the stack  
`Ld_Addr_F4`: [code] Load 32-bit floating point value from the given address onto the stack. Address must first be loaded onto the stack  
`Ld_Addr_F8`: [code] Load 64-bit floating point value from the given address onto the stack. Address must first be loaded onto the stack
`Ld_Addr_Any`: [code] Load a reference value from the given address onto the stack. Address must first be loaded onto the stack
`St_Addr_I1`: [code] Store the 8-bit value on top of the stack at the given address. Address must first be loaded onto the stack folloed by value  
`St_Addr_I2`: [code] Store the 16-bit value on top of the stack at the given address. Address must first be loaded onto the stack folloed by value  
`St_Addr_I4`: [code] Store the 32-bit value on top of the stack at the given address. Address must first be loaded onto the stack folloed by value  
`St_Addr_I8`: [code] Store the 64-bit value on top of the stack at the given address. Address must first be loaded onto the stack folloed by value  
`St_Addr_F4`: [code] Store the 32-bit floating point value on top of the stack at the given address. Address must first be loaded onto the stack folloed by value  
`St_Addr_F8`: [code] Store the 64-bit floating point value on top of the stack at the given address. Address must first be loaded onto the stack folloed by value  
`St_Addr_Any`: [code] Store the reference value on top of the stack at the given address. Address must first be loaded onto the stack folloed by value

### Arithmetic
`Add` [code, 1 byte] Add the 2 values on top of the stack and push the result. Primitive type code is stored with instruction  
`Sub`: [code, 1 byte] Subtract the 2 values on top of the stack and push the result. Primitive type code is stored with instruction  
`Mul`: [code, 1 byte] Multiply the 2 values on top of the stack and push the result. Primitive type code is stored with instruction    
`Div`: [code, 1 byte] Divide the 2 values on top of the stack and push the result. Primitive type code is stored with instruction  
`Neg`: [code, 1 byte] Negate the value on top of the stack and push the result. Primitive type code is stored with instruction  
`And`: [code, 1 byte] Perform bitwise and with the 2 values on top of the stack and push the result. Primitive type code is stored with instruction  
`Or`: [code, 1 byte] Perform bitwise or with the 2 values on top of the stack and push the result. Primitive type code is stored with instruction  
`XOr`: [code, 1 byte] Perform bitwise xor with the 2 values on top of the stack and push the result. Primitive type code is stored with instruction  
`Not`: {code, 1 byte] Perform bitwise not with the value on top of the stack and push the result. Primitive type code is stored with instruction  
`Bit_Shl`: [code, 1 byte] Perform bitshift left with the 2 values on top of the stack. Primitive type code is stored with instruction  
`Bit_Shr`: [code, 1 byte] Perform bitshift right with the 2 values on top of the stack. Primitive type code is stored with instruction  
`Mod`: [code, 1 byte] Perform modulus with the  values on top of the stack. Primitive type code is stored with instruction  
`Cmp_L`: [code, 1 byte] Compare the 2 values on top of the stack using a less than check and push the result. Primitive type code is stored with instruction  
`Cmp_Le`: [code, 1 byte] Compare the 2 values on top of the stack using a less than equal check and push the result. Primitive type code is stored with instruction  
`Cmp_G`: [code, 1 byte] Compare the 2 values on top of the stack using a greater than check and push the result. Primitive type code is stored with instruction  
`Cmp_Ge`: [code, 1 byte] Compare the 2 values on top of the stack using a greater than equal check and push the result. Primitive type code is stored with instruction  
`Cmp_Eq`: [code, 1 byte] Compare the 2 values on top of the stack using an equal check and push the result. Primitive type code is stored with instruction  
`Cmp_NEq`: [code, 1 byte] Compare the 2 values on top of the stack using a not equal check and push the result. Primitive type code is stored with instruction  
