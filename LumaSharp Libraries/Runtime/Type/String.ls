
export type string : any
{
	// Properties
	#runtime("string_length")
	export i32 Length => read;

	// Methods
	#runtime("string_compare")
	export global bool Compare(string a, string b);

	#runtime("string_contains")
	export bool Contains(string val);

	#runtime("string_indexof")
	export i32 IndexOf(string val);
}
