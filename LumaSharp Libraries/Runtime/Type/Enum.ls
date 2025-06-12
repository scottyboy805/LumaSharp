
#copy
export type Enum override : ValueType
{
    // Constructor
    protected this() override;

    // Methods
    export string[] GetNames() override;

    export i32 GetSize() override;
}
