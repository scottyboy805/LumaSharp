
// No namespace
export contract CIterator
{
	bool NextItem(any& item);
}

export contract CIterator<T>
{
	bool NextItem(T& item);
}