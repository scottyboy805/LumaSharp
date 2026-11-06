using System.Collections;

namespace System.Collections.Concurrent;

public class ConcurrentDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : default, System.IHashable, System.IEquality<TKey> where TValue : default
{
	private readonly object synchronization = new object();
	private System.Collections.Dictionary<TKey, TValue> dictionary;

	public this()
	{
		dictionary = new System.Collections.Dictionary<TKey, TValue>();
	}

	public this(int32 capacity)
	{
		dictionary = new System.Collections.Dictionary<TKey, TValue>(capacity);
	}

	public int32 Count
	{
		get
		{
			lock (synchronization)
				return dictionary.Count;
		}
	}

	public bool IsEmpty
	{
		get => Count == 0;
	}

	public TValue this[TKey key]
	{
		get
		{
			lock (synchronization)
				return dictionary[key];
		}
		set
		{
			lock (synchronization)
				dictionary[key] = value;
		}
	}

	public bool ContainsKey(TKey key)
	{
		lock (synchronization)
			return dictionary.ContainsKey(key);
	}

	public bool TryAdd(TKey key, TValue value) mut
	{
		lock (synchronization)
			return dictionary.TryAdd(key, value);
	}

	public TValue GetOrAdd(TKey key, TValue value) mut
	{
		lock (synchronization)
		{
			TValue existing;
			if (dictionary.TryGetValue(key, out existing))
				return existing;
			dictionary.Add(key, value);
			return value;
		}
	}

	public TValue AddOrUpdate(TKey key, TValue addValue, TValue updateValue) mut
	{
		lock (synchronization)
		{
			TValue existing;
			if (dictionary.TryGetValue(key, out existing))
			{
				dictionary[key] = updateValue;
				return updateValue;
			}
			dictionary.Add(key, addValue);
			return addValue;
		}
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		value = default(TValue);
		lock (synchronization)
			return dictionary.TryGetValue(key, out value);
	}

	public bool TryRemove(TKey key, out TValue value) mut
	{
		value = default(TValue);
		lock (synchronization)
		{
			if (!dictionary.TryGetValue(key, out value))
				return false;
			dictionary.Remove(key);
			return true;
		}
	}

	public void Clear() mut
	{
		lock (synchronization)
			dictionary.Clear();
	}

	public TKey[] Keys
	{
		get
		{
			lock (synchronization)
			{
				TKey[] result = new TKey[dictionary.Count];
				int32 copied = 0;
				for (int32 index = 0; index < dictionary.Capacity; index++)
				{
					if (dictionary.IsOccupied(index))
					{
						result[copied] = dictionary.GetKeyAt(index);
						copied++;
					}
				}
				return result;
			}
		}
	}

	public TValue[] Values
	{
		get
		{
			lock (synchronization)
			{
				TValue[] result = new TValue[dictionary.Count];
				int32 copied = 0;
				for (int32 index = 0; index < dictionary.Capacity; index++)
				{
					if (dictionary.IsOccupied(index))
					{
						result[copied] = dictionary.GetValueAt(index);
						copied++;
					}
				}
				return result;
			}
		}
	}

	public KeyValuePair<TKey, TValue>[] ToArray()
	{
		lock (synchronization)
		{
			KeyValuePair<TKey, TValue>[] result = new KeyValuePair<TKey, TValue>[dictionary.Count];
			int32 copied = 0;
			for (int32 index = 0; index < dictionary.Capacity; index++)
			{
				if (dictionary.IsOccupied(index))
				{
					result[copied] = KeyValuePair<TKey, TValue>(dictionary.GetKeyAt(index), dictionary.GetValueAt(index));
					copied++;
				}
			}
			return result;
		}
	}

	public Iterator<TKey, TValue> Iterate()
	{
		return new Iterator<TKey, TValue>(ToArray());
	}

	IIterator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.Iterate()
	{
		return new Iterator<TKey, TValue>(ToArray());
	}

	public class Iterator<TKeyItem, TValueItem> : IIterator<KeyValuePair<TKeyItem, TValueItem>> where TKeyItem : default, System.IHashable, System.IEquality<TKeyItem> where TValueItem : default
	{
		private KeyValuePair<TKeyItem, TValueItem>[] snapshot;
		private int32 index;

		public this(KeyValuePair<TKeyItem, TValueItem>[] snapshot)
		{
			this.snapshot = snapshot;
			index = -1;
		}

		public bool Next() mut
		{
			index++;
			return index < snapshot.Length;
		}

		public KeyValuePair<TKeyItem, TValueItem> Current
		{
			get
			{
				if (index < 0 || index >= snapshot.Length)
					return default(KeyValuePair<TKeyItem, TValueItem>);
				return snapshot[index];
			}
		}
	}
}