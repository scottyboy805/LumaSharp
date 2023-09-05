
export type array : CIterator
{
	internal type ArrayIterator<T> : CIterator<T>, CIterator	
	{
		// Private
		T[] arr;
		i32 position;
		i32 count;

		// Constructor
		export this ArrayIterator(T[] arr)
		{
			this.arr = arr;
			this.count = arr.Count;
		}

		export this ArrayIterator(T[] arr, i32 count)
		{
			this.arr = arr;
			this.count = count;
		}

		// Methods
		export bool CIterator.NextItem(any& item)
		{
			// Try to retreive item
			T val;
			bool success = NextItem(val);

			// Box
			item = val;

			return success;
		}

		export bool CIterator<T>.NextItem(T& item)
		{
			position++;

			// Check for success
			bool success = position < count;

			// Get item
			item = success ? arr[position] : default;

			return success;
		}
	}

	// Accessors
	#InternalCall("array_length")
	export i64 Count => read;
}