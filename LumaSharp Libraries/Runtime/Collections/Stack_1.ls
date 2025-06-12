

export type Stack<T> : CReadOnlyCollection<T>, CEnumerable<T>
{
	// Private
	hidden T[] items;
	hidden i32 count;

	hidden global T[] empty = new T[0];

	// Accessor
	export i32 Count => read: return count;

	// Constructor
	export this()
	{
		this.array = empty;
	}

	export this(i32 capacity)
	{
		this.array = new T[capacity];
	}

	// Methods
	export T Peek()
	{
		// Check for any
		if count == 0
			;

		// Get element
		return array[count - 1];
	}

	export T Pop()
	{
		// Check for any
		if count == 0
			;

		// Get element
		T item = items[--count];

		// Allom memory to be freed
		items[count] = default;

		return item;
	}

	export void Push(T item)
	{
		// Check for resize
		if count == array.Count
		{
			// Grow array with copy
			array = array.Resize(array.Count * 2);
		}

		// Add the item
		array[count++] = item;
	}

	export bool Contains(T item)
	{
		return array.Contains(item);
	}

	export void Clear()
	{
		// Release elements
		for i32 i = 0; i < array.Count; i++
			array[i] = default;

		count = 0;
	}
}
