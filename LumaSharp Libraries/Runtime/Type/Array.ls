
export type Array override : CReadOnlyCollection<any>, CIterator<any>
{
	#copy
	internal type ArrayIterator<T> : CIterator<T>	
	{
		// Private
		#readonly hidden array arr;
		hidden i32 position;
		hidden T current;

		// Accessor
		export T Current => current;

		// Constructor
		export this ArrayIterator(array arr)
		{
			this.arr = arr;
		}

		// Methods
		export void CIterator<T>.Begin()
		{
			position = 0;
			current = default;
		}

		export bool CIterator<T>.Next()
		{
			// Check for success
			bool success = position < arr.Count;

			// Check success
			if success
			{
				// Get item
				current = success ? arr[position] : default;

				// Increment position
				position++;
			}

			return success;
		}
	}

	// Accessors
	#runtime("array_length")
	export i64 Count => read;

	#runtime("array_element")
	export any this[i32 index] => read; => write;

	export CIterator<T> => read: return new ArrayIterator<T>(items);

	// Constructor
	protected this() override;

	// Methods
	export bool Contains(any item)
	{
		for i32 i = 0; i < Count; i++			
		{
			if array[i] == item
				return true;
		}
		return false;
	}

	export global T[] Repeat<T>(T value, int count)
	{
		// Create array
		T[] arr = new T[count];

		// Assign all elements
		for i32 i = 0; i < count; i++
			arr[i] = value;
	}
}