
#copy #readonly
export type String : ValueType, CReadOnlyCollection<char>, CEnumerable<char>
{
	// Internal
	#readonly
	internal char[]? Value;

	// Export
	#readonly
	export i32 Length = 0;

	#readonly
	export global string Empty = default;

	// Accessor
	i32 CReadOnlyCollection<char>.Count => read: return Length;

	CIterator<char> CEnumerable<char>.Iterator => read: return Value.Iterator;

	// Constructor
	export this() {}

	export this(string other)
	{
		this.Length = other.Length;

		// Copy array if valid
		if this.Length > 0
			this.Value = other.Value.Copy();		
	}

	export this(char c, int count)
	{
		this.Length = count;
		this.Value = Array.Repeat<char>(c, count);
	}

	// Methods
	export bool Contains(char c)
	{
		return Length > 0 && Value.Contains(c);
	}

	#runtime("string_compare")
	export global bool Compare(string a, string b);

	#runtime("string_contains")
	export bool Contains(string val);

	#runtime("string_indexof")
	export i32 IndexOf(string val);
}
