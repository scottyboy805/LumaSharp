namespace System.Collections;

public class Dictionary<TKey, TValue> where TKey : class, IEquality<TKey>, IHashable
{
	private TKey?[] _keys;
	private TValue?[] _values;
	private int32[] _hashes;
	private int32[] _states;
	private int32 _count;
	private int32 _used;

	public this()
	{
		_keys = new TKey?[8];
		_values = new TValue?[8];
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
		_keys = new TKey?[actualCapacity];
		_values = new TValue?[actualCapacity];
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

	public TValue this[TKey key]
	{
		get
		{
			ValidateKey(key);
			int32 index = Find(key, key.GetHashCode());
			if (index < 0)
				CollectionGuard.ThrowKeyNotFound();
			return _values[index]!;
		}
		set
		{
			ValidateKey(key);
			int32 hash = key.GetHashCode();
			int32 index = Find(key, hash);
			if (index >= 0)
			{
				_values[index] = value;
				return;
			}
			EnsureCapacity(_count + 1);
			InsertNew(key, value, hash);
		}
	}

	public void Add(TKey key, TValue value) mut
	{
		ValidateKey(key);
		int32 hash = key.GetHashCode();
		if (Find(key, hash) >= 0)
			CollectionGuard.ThrowArgument("An element with the same key already exists.");
		EnsureCapacity(_count + 1);
		InsertNew(key, value, hash);
	}

	public bool TryAdd(TKey key, TValue value) mut
	{
		ValidateKey(key);
		int32 hash = key.GetHashCode();
		if (Find(key, hash) >= 0)
			return false;
		EnsureCapacity(_count + 1);
		InsertNew(key, value, hash);
		return true;
	}

	public bool ContainsKey(TKey key)
	{
		ValidateKey(key);
		return Find(key, key.GetHashCode()) >= 0;
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		ValidateKey(key);
		int32 index = Find(key, key.GetHashCode());
		if (index >= 0)
		{
			value = _values[index]!;
			return true;
		}
		for (int32 empty = 0; empty < _states.Length; empty++)
			if (_states[empty] == 0)
			{
				value = _values[empty]!;
				return false;
			}
		value = _values[0]!;
		return false;
	}

	public bool Remove(TKey key) mut
	{
		ValidateKey(key);
		int32 index = Find(key, key.GetHashCode());
		if (index < 0)
			return false;
		_keys[index] = null;
		_values[index] = null;
		_hashes[index] = 0;
		_states[index] = 2;
		_count--;
		return true;
	}

	public void Clear() mut
	{
		Initialize(8);
	}

	private void Initialize(int32 capacity) mut
	{
		_keys = new TKey?[capacity];
		_values = new TValue?[capacity];
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

	private void ValidateKey(TKey key)
	{
		if (key == null)
			CollectionGuard.ThrowNullArgument();
	}

	private int32 Find(TKey key, int32 hash)
	{
		int32 slot = hash & 0x7FFFFFFF;
		for (int32 probe = 0; probe < _states.Length; probe++)
		{
			int32 index = (slot + probe) % _states.Length;
			if (_states[index] == 0)
				return -1;
			if (_states[index] == 1 && _hashes[index] == hash && _keys[index]!.Equals(key))
				return index;
		}
		return -1;
	}

	private void InsertNew(TKey key, TValue value, int32 hash) mut
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
				Store(firstDeleted >= 0 ? firstDeleted : index, key, value, hash);
				return;
			}
		}
		Store(firstDeleted, key, value, hash);
	}

	private void Store(int32 index, TKey key, TValue value, int32 hash) mut
	{
		if (_states[index] == 0)
			_used++;
		_keys[index] = key;
		_values[index] = value;
		_hashes[index] = hash;
		_states[index] = 1;
		_count++;
	}

	private void EnsureCapacity(int32 minimum) mut
	{
		if (minimum * 4 < _states.Length * 3 && _used * 4 < _states.Length * 3)
			return;
		int32 newCapacity = _states.Length * 2;
		TKey?[] oldKeys = _keys;
		TValue?[] oldValues = _values;
		int32[] oldHashes = _hashes;
		int32[] oldStates = _states;
		Initialize(newCapacity);
		for (int32 index = 0; index < oldStates.Length; index++)
			if (oldStates[index] == 1)
				InsertNew(oldKeys[index]!, oldValues[index]!, oldHashes[index]);
	}
}

