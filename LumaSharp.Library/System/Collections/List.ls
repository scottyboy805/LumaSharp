namespace System.Collections;

// TODO: Re-enable IEnumerable<T> when nested generic iterator allocation is supported.
public class List<T>
{
	private T?[] items;
	private int32 count;

	public this()
	{
		items = new T?[4];
		count = 0;
	}

	public this(int32 capacity)
	{
		if (capacity < 0)
			throw new System.ArgumentOutOfRangeException();
		items = new T?[capacity];
		count = 0;
	}

	public int32 Count
	{
		get => count;
	}

	public int32 Capacity
	{
		get => items.Length;
	}

	public unsafe T* Ptr
	{
		get
		{
			if (items.Length == 0)
				return null;
			return (T*)&items[0];
		}
	}

	public T this[int32 index]
	{
		get
		{
			if (index < 0 || index >= count)
				throw new System.ArgumentOutOfRangeException();
			return items[index]!;
		}
		set
		{
			if (index < 0 || index >= count)
				throw new System.ArgumentOutOfRangeException();
			items[index] = value;
		}
	}

	public void Add(T item) mut
	{
		EnsureCapacity(count + 1);
		items[count] = item;
		count++;
	}

	public void Insert(int32 index, T item) mut
	{
		if (index < 0 || index > count)
			throw new System.ArgumentOutOfRangeException();
		EnsureCapacity(count + 1);
		int32 current = count;
		while (current > index)
		{
			items[current] = items[current - 1];
			current--;
		}
		items[index] = item;
		count++;
	}

	public void RemoveAt(int32 index) mut
	{
		if (index < 0 || index >= count)
			throw new System.ArgumentOutOfRangeException();
		int32 current = index;
		while (current + 1 < count)
		{
			items[current] = items[current + 1];
			current++;
		}
		count--;
		items[count] = null;
	}

	public void Clear() mut
	{
		for (int32 index = 0; index < count; index++)
			items[index] = null;
		count = 0;
	}

	public int32 IndexOf(T item)
	{
		for (int32 index = 0; index < count; index++)
			if (items[index] == item)
				return index;
		return -1;
	}

	public bool Contains(T item)
	{
		return IndexOf(item) >= 0;
	}

	public bool Remove(T item) mut
	{
		int32 index = IndexOf(item);
		if (index < 0)
			return false;
		RemoveAt(index);
		return true;
	}

	public T?[] ToArray()
	{
		T?[] result = new T?[count];
		for (int32 index = 0; index < count; index++)
			result[index] = items[index];
		return result;
	}

	private void EnsureCapacity(int32 minimum) mut
	{
		if (minimum <= items.Length)
			return;
		int32 capacity = items.Length == 0 ? 4 : items.Length * 2;
		if (capacity < minimum)
			capacity = minimum;
		T?[] replacement = new T?[capacity];
		for (int32 index = 0; index < count; index++)
			replacement[index] = items[index];
		items = replacement;
	}

}
