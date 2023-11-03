
export type CEnumerable
{
	// Accessors
	CIterator Iterator => read;
}

export type CEnumerable<T>
{
	// Accessors
	CIterator<T> Iterator => read;
}