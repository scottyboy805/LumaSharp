
#copy
export type I32 : ValueType
{
    // Internal
    internal i32 value;

    // Accessors
    #runtime("i32_minvalue")
    gloabl export MinValue => read;

    #runtime("i32_maxvalue")
    global export MaxValue => read;
}
