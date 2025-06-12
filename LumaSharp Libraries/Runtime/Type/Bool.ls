
#copy
export type Bool : ValueType
{
	// Internal
	internal bool value;

	// Export
	#const
	export global bool True => true;

	#const 
	export global bool False => false;

	#const
	export global string TrueString => "True";

	#const
	export global string FalseString => "False";

	// Accessors
	export i32 Hash override => read:
	{
		return value ? 1 : 0;
	}

	export string String override => read: 
	{
		return value ? TrueString : FalseString;
	}

	// Constructor
	export this() { }

	export this(bool value)
	{
		this.value = value;
	}

	// Methods
	export bool Equals(any other) override
	{
		if other is bool boolVal
			return boolValue == value;

		return false;
	}
}