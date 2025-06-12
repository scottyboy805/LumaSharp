
namespace Collections:Immutable;


export type ImmutableList<T> : CImmutableCollection<T>, CReadOnlyCollection<T>, CEnumerable<T>
{
    // Fields
    hidden CReadOnlyCollection<T> items;

	// Export
	#readonly
	export global ImmutableList<T> Empty = new ImmutableList<T>();

    // Accessors
    export i32 Count => read: return items.Count;

    export CIterator<T> => read: return items.Iterator;

    // Constructor
	export this()
	{
		this.items = List<T>.Empty;
	}

    export this(T[] items)
    {
        this.items = items;
    }

    export this(CEnumerable<T> items)
    {
        this.items = new List<T>(items);
    }

    export i32 Index(T item)
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

	export ImmutableList<T> Add(T item)	
	{
		List<T> newItems = new List<T>(this.items);
		newItems.Add(item);

		return new ImmutableList<T>(newItems);
	}

	export ImmutableList<T> Remove(T item)	
	{
		List<T> newItems = new List<T>(this.items);
		newItems.Remove(item);

		return new ImmutabeList<T>(newItems);
	}

	export ImmutableList<T> Clear()
	{
		return Empty;
	}
}