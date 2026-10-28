namespace System.Collections;

public class HashSet<T> : IEnumerable<T> where T : class, IEquality<T>, IHashable
{
	private T?[] _items;
	private int32[] _hashes;
	private int32[] _states;
	private int32 _count;
	private int32 _used;

	public this()
	{
		_items = new T?[8];
		_hashes = new int32[8];
		_states = new int32[8];
		_count = 0;
		_used = 0;
	}

	public this(int32 capacity)
	{
		if (capacity < 0)
			CollectionGuard.ThrowOutOfRange();
		int32 actualCapacity = CapacityFor(capacity);
		_items = new T?[actualCapacity];
		_hashes = new int32[actualCapacity];
		_states = new int32[actualCapacity];
		_count = 0;
		_used = 0;
	}

	public int32 Count
	{
		get => _count;
	}

	public int32 Capacity
	{
		get => _states.Length;
	}

	public bool Add(T item) mut
	{
		ValidateItem(item);
		int32 hash = item.GetHashCode();
		if (Find(item, hash) >= 0)
			return false;
		EnsureCapacity(_count + 1);
		InsertNew(item, hash);
		return true;
	}

	public bool Contains(T item)
	{
		ValidateItem(item);
		return Find(item, item.GetHashCode()) >= 0;
	}

	public bool Remove(T item) mut
	{
		ValidateItem(item);
		int32 index = Find(item, item.GetHashCode());
		if (index < 0)
			return false;
		_items[index] = null;
		_hashes[index] = 0;
		_states[index] = 2;
		_count--;
		return true;
	}

	public void Clear() mut
	{
		Initialize(8);
	}

	public bool IsOccupied(int32 index)
	{
		return _states[index] == 1;
	}

	public T GetAt(int32 index)
	{
		return _items[index]!;
	}

	public IIterator<T> Iterate()
	{
		return new HashSetIterator<T>(this);
	}

	private int32 Find(T item, int32 hash)
	{
		int32 slot = hash & 0x7FFFFFFF;
		for (int32 probe = 0; probe < _states.Length; probe++)
		{
			int32 index = (slot + probe) % _states.Length;
			if (_states[index] == 0)
				return -1;
			if (_states[index] == 1 && _hashes[index] == hash && _items[index]!.Equals(item))
				return index;
		}
		return -1;
	}

	private void ValidateItem(T item)
	{
		if (item == null)
			CollectionGuard.ThrowNullArgument();
	}

	private void InsertNew(T item, int32 hash) mut
	{
		int32 firstDeleted = -1;
		int32 slot = hash & 0x7FFFFFFF;
		for (int32 probe = 0; probe < _states.Length; probe++)
		{
			int32 index = (slot + probe) % _states.Length;
			if (_states[index] == 2 && firstDeleted < 0)
				firstDeleted = index;
			if (_states[index] == 0)
			{
				Store(firstDeleted >= 0 ? firstDeleted : index, item, hash);
				return;
			}
		}
		Store(firstDeleted, item, hash);
	}

	private void Store(int32 index, T item, int32 hash) mut
	{
		if (_states[index] == 0)
			_used++;
		_items[index] = item;
		_hashes[index] = hash;
		_states[index] = 1;
		_count++;
	}

	private void EnsureCapacity(int32 minimum) mut
	{
		if (minimum * 4 < _states.Length * 3 && _used * 4 < _states.Length * 3)
			return;
		int32 newCapacity = _states.Length * 2;
		T?[] oldItems = _items;
		int32[] oldHashes = _hashes;
		int32[] oldStates = _states;
		Initialize(newCapacity);
		for (int32 index = 0; index < oldStates.Length; index++)
			if (oldStates[index] == 1)
				InsertNew(oldItems[index]!, oldHashes[index]);
	}

	private void Initialize(int32 capacity) mut
	{
		_items = new T?[capacity];
		_hashes = new int32[capacity];
		_states = new int32[capacity];
		_count = 0;
		_used = 0;
	}

	private static int32 CapacityFor(int32 requested)
	{
		int32 capacity = 8;
		while (capacity * 3 / 4 < requested)
			capacity *= 2;
		return capacity;
	}
}

internal class HashSetIterator<T> : IIterator<T> where T : class, IEquality<T>, IHashable
{
	private HashSet<T> _set;
	private int32 _index;

	public this(HashSet<T> set)
	{
		_set = set;
		_index = -1;
	}

	public bool Next() mut
	{
		_index++;
		while (_index < _set.Capacity && !_set.IsOccupied(_index))
			_index++;
		return _index < _set.Capacity;
	}

	public T Current
	{
		get => _set.GetAt(_index);
	}
}
