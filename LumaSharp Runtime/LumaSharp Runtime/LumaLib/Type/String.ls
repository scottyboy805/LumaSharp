
export type string : any
{
	// Properties
	export i32 op_hash => #InternalCall("string_hash") read;

	export i32 Length => #InternalCall("string_length") read;

	// Methods
	#InternalCall("string_compare")
	export global bool Compare(string a, string b);

	#InternalCall("string_contains")
	export bool Contains(string val);

	#InternalCall("string_indexof")
	export i32 IndexOf(string val);

	#InternalCall("string_equal")
	export bool op_equal(string other);

	#InternalCall("string_string")
	export string op_string();
}
