
export contract CIterator<T>
{
	// Accessors
	T Current => read;

	// Methods
	void Begin();
	bool Next();
}