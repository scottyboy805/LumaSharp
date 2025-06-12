

// NOT USED ANYMORE - MOVED TO RUNTIME

namespace Collections:Generic
{
	export type List<T> : CEnumerable<T>
	{
		// Fields
		hidden T[] items = null;
		hidden i32 capacity;
		hidden i32 count;

		export global i32 DefaultCapacity = 32 const;

		// Accessors
		export i32 Count => read: return count;

		export i32 Capacity
			=> read: return capacity;
			=> write: 
			{
				this.capacity = input;
				EnsureCapacity(input);
			}

		export CIterator<T> => read: return array.ArrayIterator<T>(items, count);

		// Constructor
		export this()
		{
			this.capacity = DefaultCapacity;
			this.items = new T[capacity];
		}

		export this(i32 capacity)
		{
			this.capacity = capacity;
			this.items = new T[capacity];
		}

		export this(CEnumerable<T> items)
		{
			this.capacity = DefaultCapacity;
			this.items = new T[capacity];

			AddMany(items);
		}

		// Methods
		export void Add(T item)
		{
			// Make sure array size is large enough
			EnsureCapacity(count + 1);

			// Add the item
			this.items[count] = item;

			// Increase count
			this.count++;
		}

		export void AddMany(IEnumerable<T> items)
		{
			foreach(T item in items)
				Add(item);
		}

		export bool Remove(T item)
		{
			// Try to find item index
			i32 index = Index(item);

			// Check for index
			if(index != -1)
				return RemoveIndex(index);

			return false;
		}

		export bool RemoveIndex(i32 index)
		{
			// Check for out of bounds
			if(index < 0 || index >= this.count)
				return false;

			// Reset index
			this.items[index] = default;

			// Shift items
			for(i32 i = index; i < this.count - 1; i++)
			{
				this.items[index] = this.items[index + 1];
			}

			return true;
		}

		export i32 Index(T& item)
		{	
			i32 index = -1;

			// Check all items
			for(i32 i = 0; i < this.count; i++)
			{
				if(this.items[i].Equals(item) == true)
				{
					index = i;
					break;
				}
			}

			return index;
		}

		export void Clear()
		{
			this.count = 0;
		}

		hidden void EnsureCapacity(i32 size)
		{
			if(this.capacity < size)
			{
				// Select best size
				i32 newSize = (capacity * 2 > size) ? capacity * 2 : size;

				// Resize array
				this.items = items.Resize(newSize);

				// Update capacity
				this.capacity = newSize;
			}
		}
	}
}