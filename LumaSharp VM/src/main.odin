package LumaVM

import "core:fmt"

main :: proc() {
    
    fmt.printf("Hello World");

    luma_execute_bytecode(nil, nil, 0)
}