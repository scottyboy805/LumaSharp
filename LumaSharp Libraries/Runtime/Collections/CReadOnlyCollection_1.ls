

namespace Collections;

export contract CReadOnlyCollection<T> : CEnumerable<T>
{
	// Accessors
	i32 Count => read;

	T this[i32 index] => read;

	// Methods
	bool Contains(T item);
}
