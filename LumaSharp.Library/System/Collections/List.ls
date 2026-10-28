namespace System.Collections;

public class List<T> : IEnumerable<T>
{
	private T?[] _items;
	private int32 _count;

	public this()
	{
		_items = new T?[4];
		_count = 0;
	}

	public this(int32 capacity)
	{
		if (capacity < 0)
			CollectionGuard.ThrowOutOfRange();
		_items = new T?[capacity];
		_count = 0;
	}

	public int32 Count
	{
		get => _count;
	}

	public int32 Capacity
	{
		get => _items.Length;
	}

	public T this[int32 index]
	{
		get
		{
			if (index < 0 || index >= _count)
				CollectionGuard.ThrowOutOfRange();
			return _items[index]!;
		}
		set
		{
			if (index < 0 || index >= _count)
				CollectionGuard.ThrowOutOfRange();
			_items[index] = value;
		}
	}

	public void Add(T item) mut
	{
		EnsureCapacity(_count + 1);
		_items[_count] = item;
		_count++;
	}

	public void Insert(int32 index, T item) mut
	{
		if (index < 0 || index > _count)
			CollectionGuard.ThrowOutOfRange();
		EnsureCapacity(_count + 1);
		int32 current = _count;
		while (current > index)
		{
			_items[current] = _items[current - 1];
			current--;
		}
		_items[index] = item;
		_count++;
	}

	public void RemoveAt(int32 index) mut
	{
		if (index < 0 || index >= _count)
			CollectionGuard.ThrowOutOfRange();
		int32 current = index;
		while (current + 1 < _count)
		{
			_items[current] = _items[current + 1];
			current++;
		}
		_count--;
		_items[_count] = null;
	}

	public void Clear() mut
	{
		for (int32 index = 0; index < _count; index++)
			_items[index] = null;
		_count = 0;
	}

	public int32 IndexOf(T item)
	{
		for (int32 index = 0; index < _count; index++)
			if (_items[index] == item)
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
		T?[] result = new T?[_count];
		for (int32 index = 0; index < _count; index++)
			result[index] = _items[index];
		return result;
	}

	public IIterator<T> Iterate()
	{
		return new ListIterator<T>(this);
	}

	private void EnsureCapacity(int32 minimum) mut
	{
		if (minimum <= _items.Length)
			return;
		int32 capacity = _items.Length == 0 ? 4 : _items.Length * 2;
		if (capacity < minimum)
			capacity = minimum;
		T?[] replacement = new T?[capacity];
		for (int32 index = 0; index < _count; index++)
			replacement[index] = _items[index];
		_items = replacement;
	}
}

internal class ListIterator<T> : IIterator<T>
{
	private List<T> _list;
	private int32 _index;

	public this(List<T> list)
	{
		_list = list;
		_index = -1;
	}

	public bool Next() mut
	{
		_index++;
		return _index < _list.Count;
	}

	public T Current
	{
		get => _list[_index];
	}
}
