namespace System.Collections;

public class HashSet<T> : IEnumerable<T> where T : default
{
	private T[] items;
	private int32[] hashes;
	private int32[] states;
	private int32 count;
	private int32 used;

	public this()
	{
		items = new T[8];
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
		items = new T[actualCapacity];
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

	public unsafe T* Ptr
	{
		get
		{
			if (items.Length == 0)
				return null;
			unsafe unchecked
			{
				return &items[0];
			}
		}
	}

	public bool Add(T item) mut
	{
		int32 hash = GetHashCode(item);
		if (Find(item, hash) >= 0)
			return false;
		EnsureCapacity(count + 1);
		InsertNew(item, hash);
		return true;
	}

	public bool Contains(T item)
	{
		return Find(item, GetHashCode(item)) >= 0;
	}

	public bool Remove(T item) mut
	{
		int32 index = Find(item, GetHashCode(item));
		if (index < 0)
			return false;
		unsafe unchecked
		{
			items[index] = default(T);
			hashes[index] = 0;
			states[index] = 2;
		}
		count--;
		return true;
	}

	public void Clear() mut
	{
		Initialize(8);
	}

	public bool IsOccupied(int32 index)
	{
		if (index < 0 || index >= states.Length)
			throw new System.ArgumentOutOfRangeException();
		int32 state;
		unsafe unchecked
		{
			state = states[index];
		}
		return state == 1;
	}

	public T GetAt(int32 index)
	{
		if (index < 0 || index >= states.Length)
			throw new System.ArgumentOutOfRangeException();
		T item;
		unsafe unchecked
		{
			item = items[index];
		}
		return item;
	}

	public IIterator<T> Iterate()
	{
		return new Iterator<T>(this);
	}

	private int32 GetHashCode(T item)
	{
		if (item is System.IHashable hashable)
			return hashable.Hash;
		return 0;
	}

	private bool ItemsEqual(T left, T right)
	{
		if (left is System.IEquality<T> equality)
			return equality.Equals(right);
		return left == right;
	}

	private int32 Find(T item, int32 hash)
	{
		int32 slot = hash & 0x7FFFFFFF;
		for (int32 probe = 0; probe < states.Length; probe++)
		{
			int32 index = (slot + probe) % states.Length;
			int32 state;
			T current;
			int32 currentHash;
			unsafe unchecked
			{
				state = states[index];
				currentHash = hashes[index];
				current = items[index];
			}
			if (state == 0)
				return -1;
			if (state == 1 && currentHash == hash && ItemsEqual(current, item))
				return index;
		}
		return -1;
	}

	private void InsertNew(T item, int32 hash) mut
	{
		int32 firstDeleted = -1;
		int32 slot = hash & 0x7FFFFFFF;
		for (int32 probe = 0; probe < states.Length; probe++)
		{
			int32 index = (slot + probe) % states.Length;
			int32 state;
			unsafe unchecked
			{
				state = states[index];
			}
			if (state == 2 && firstDeleted < 0)
				firstDeleted = index;
			if (state == 0)
			{
				Store(firstDeleted >= 0 ? firstDeleted : index, item, hash);
				return;
			}
		}
		Store(firstDeleted, item, hash);
	}

	private void Store(int32 index, T item, int32 hash) mut
	{
		unsafe unchecked
		{
			if (states[index] == 0)
				used++;
			items[index] = item;
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
		T[] oldItems = items;
		int32[] oldHashes = hashes;
		int32[] oldStates = states;
		Initialize(newCapacity);
		for (int32 index = 0; index < oldStates.Length; index++)
		{
			int32 state;
			T oldItem;
			int32 oldHash;
			unsafe unchecked
			{
				state = oldStates[index];
				oldItem = oldItems[index];
				oldHash = oldHashes[index];
			}
			if (state == 1)
				InsertNew(oldItem, oldHash);
		}
	}

	private void Initialize(int32 capacity) mut
	{
		items = new T[capacity];
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

	private class Iterator<TItem> : IIterator<TItem> where TItem : default
	{
		private HashSet<TItem> set;
		private int32 index;

		public this(HashSet<TItem> source)
		{
			set = source;
			index = -1;
		}

		public bool Next() mut
		{
			index++;
			while (index < set.Capacity && !set.IsOccupied(index))
				index++;
			return index < set.Capacity;
		}

		public TItem Current
		{
			get => set.GetAt(index);
		}
	}
}
