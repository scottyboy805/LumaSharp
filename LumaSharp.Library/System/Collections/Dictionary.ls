namespace System.Collections;

public class Dictionary<TKey, TValue> where TKey : default, System.IHashable, System.IEquality<TKey> where TValue : default
{
	private TKey[] keys;
	private TValue[] values;
	private int32[] hashes;
	private int32[] states;
	private int32 count;
	private int32 used;

	public this()
	{
		keys = new TKey[8];
		values = new TValue[8];
		hashes = new int32[8];
		states = new int32[8];
		count = 0;
		used = 0;
	}

	public this(int32 capacity)
	{
		if (capacity < 0)
			throw new System.ArgumentOutOfRangeException();
		int32 actualCapacity = CapacityFor(capacity);
		keys = new TKey[actualCapacity];
		values = new TValue[actualCapacity];
		hashes = new int32[actualCapacity];
		states = new int32[actualCapacity];
		count = 0;
		used = 0;
	}

	public int32 Count
	{
		get => count;
	}

	public int32 Capacity
	{
		get => states.Length;
	}

	public unsafe void* Ptr
	{
		get
		{
			if (keys.Length == 0)
				return null;
			unsafe unchecked
			{
				return &keys[0];
			}
		}
	}

	public TValue this[TKey key]
	{
		get
		{
			if (!(key is System.Object))
				throw new System.ArgumentNullException();
			int32 index = Find(key, key.Hash);
			if (index < 0)
				throw new KeyNotFoundException();
			TValue value;
			unsafe unchecked
			{
				value = values[index];
			}
			return value;
		}
		set
		{
			if (!(key is System.Object))
				throw new System.ArgumentNullException();
			int32 hash = key.Hash;
			int32 index = Find(key, hash);
			if (index >= 0)
			{
				unsafe unchecked
				{
					values[index] = value;
				}
				return;
			}
			int32 insertionIndex = -index - 1;
			int32 previousCapacity = states.Length;
			EnsureCapacity(count + 1);
			if (states.Length != previousCapacity || insertionIndex >= previousCapacity)
				InsertNew(key, value, hash);
			else
				Store(insertionIndex, key, value, hash);
		}
	}

	public void Add(TKey key, TValue value) mut
	{
		if (!(key is System.Object))
			throw new System.ArgumentNullException();
		int32 hash = key.Hash;
		int32 index = Find(key, hash);
		if (index >= 0)
			throw new System.ArgumentException("An element with the same key already exists.");
		int32 insertionIndex = -index - 1;
		int32 previousCapacity = states.Length;
		EnsureCapacity(count + 1);
		if (states.Length != previousCapacity || insertionIndex >= previousCapacity)
			InsertNew(key, value, hash);
		else
			Store(insertionIndex, key, value, hash);
	}

	public bool TryAdd(TKey key, TValue value) mut
	{
		if (!(key is System.Object))
			throw new System.ArgumentNullException();
		int32 hash = key.Hash;
		int32 index = Find(key, hash);
		if (index >= 0)
			return false;
		int32 insertionIndex = -index - 1;
		int32 previousCapacity = states.Length;
		EnsureCapacity(count + 1);
		if (states.Length != previousCapacity || insertionIndex >= previousCapacity)
			InsertNew(key, value, hash);
		else
			Store(insertionIndex, key, value, hash);
		return true;
	}

	public bool ContainsKey(TKey key)
	{
		if (!(key is System.Object))
			throw new System.ArgumentNullException();
		return Find(key, key.Hash) >= 0;
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		if (!(key is System.Object))
			throw new System.ArgumentNullException();
		int32 index = Find(key, key.Hash);
		if (index >= 0)
		{
			unsafe unchecked
			{
				value = values[index];
			}
			return true;
		}
		value = default(TValue);
		return false;
	}

	public bool Remove(TKey key) mut
	{
		if (!(key is System.Object))
			throw new System.ArgumentNullException();
		int32 index = Find(key, key.Hash);
		if (index < 0)
			return false;
		unsafe unchecked
		{
			keys[index] = default(TKey);
			values[index] = default(TValue);
			hashes[index] = 0;
			states[index] = 2;
		}
		count--;
		return true;
	}

	public void Clear() mut
	{
		Array.Clear(keys);
		Array.Clear(values);
		Array.Clear(hashes);
		Array.Clear(states);
		count = 0;
		used = 0;
	}

	private void Initialize(int32 capacity) mut
	{
		keys = new TKey[capacity];
		values = new TValue[capacity];
		hashes = new int32[capacity];
		states = new int32[capacity];
		count = 0;
		used = 0;
	}

	private static int32 CapacityFor(int32 requested)
	{
		int32 capacity = 8;
		while (capacity * 3 / 4 < requested)
			capacity *= 2;
		return capacity;
	}

	internal bool IsOccupied(int32 index)
	{
		if (index < 0 || index >= states.Length)
			throw new System.ArgumentOutOfRangeException();
		unsafe unchecked
		{
			return states[index] == 1;
		}
	}

	internal TKey GetKeyAt(int32 index)
	{
		if (!IsOccupied(index))
			throw new System.ArgumentOutOfRangeException();
		unsafe unchecked
		{
			return keys[index];
		}
	}

	internal TValue GetValueAt(int32 index)
	{
		if (!IsOccupied(index))
			throw new System.ArgumentOutOfRangeException();
		unsafe unchecked
		{
			return values[index];
		}
	}
	private int32 Find(TKey key, int32 hash)
	{
		int32 slot = hash & 0x7FFFFFFF;
		int32 firstDeleted = -1;
		for (int32 probe = 0; probe < states.Length; probe++)
		{
			int32 index = (slot + probe) & (states.Length - 1);
			int32 state;
			unsafe unchecked
			{
				state = states[index];
			}
			if (state == 0)
				return -(firstDeleted >= 0 ? firstDeleted : index) - 1;
			if (state == 2 && firstDeleted < 0)
				firstDeleted = index;
			else if (state == 1)
			{
				int32 existingHash;
				unsafe unchecked { existingHash = hashes[index]; }
				if (existingHash == hash)
				{
					TKey existingKey;
					unsafe unchecked { existingKey = keys[index]; }
					if (existingKey is System.Object && existingKey.Equals(key))
						return index;
				}
			}
		}
		return firstDeleted >= 0 ? -firstDeleted - 1 : -states.Length - 1;
	}

	private void InsertNew(TKey key, TValue value, int32 hash) mut
	{
		int32 firstDeleted = -1;
		int32 slot = hash & 0x7FFFFFFF;
		for (int32 probe = 0; probe < states.Length; probe++)
		{
			int32 index = (slot + probe) & (states.Length - 1);
			int32 state;
			unsafe unchecked
			{
				state = states[index];
			}
			if (state == 2 && firstDeleted < 0)
				firstDeleted = index;
			if (state == 0)
			{
				Store(firstDeleted >= 0 ? firstDeleted : index, key, value, hash);
				return;
			}
		}
		Store(firstDeleted, key, value, hash);
	}

	private void Store(int32 index, TKey key, TValue value, int32 hash) mut
	{
		unsafe unchecked
		{
			if (states[index] == 0)
				used++;
			keys[index] = key;
			values[index] = value;
			hashes[index] = hash;
			states[index] = 1;
		}
		count++;
	}

	private void EnsureCapacity(int32 minimum) mut
	{
		if (minimum * 4 < states.Length * 3 && used * 4 < states.Length * 3)
			return;
		int32 newCapacity = states.Length * 2;
		TKey[] oldKeys = keys;
		TValue[] oldValues = values;
		int32[] oldHashes = hashes;
		int32[] oldStates = states;
		Initialize(newCapacity);
		for (int32 index = 0; index < oldStates.Length; index++)
		{
			int32 state;
			TKey oldKey;
			TValue oldValue;
			int32 oldHash;
			unsafe unchecked
			{
				state = oldStates[index];
				oldKey = oldKeys[index];
				oldValue = oldValues[index];
				oldHash = oldHashes[index];
			}
			if (state == 1)
				InsertNew(oldKey, oldValue, oldHash);
		}
	}
}

