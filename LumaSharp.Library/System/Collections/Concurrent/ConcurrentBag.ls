using System.Collections;

namespace System.Collections.Concurrent;

public class ConcurrentBag<T> : IEnumerable<T> where T : default
{
	private readonly object synchronization = new object();
	private List<T> items;

	public this()
	{
		items = new List<T>();
	}

	public int32 Count
	{
		get
		{
			lock (synchronization)
				return items.Count;
		}
	}

	public bool IsEmpty
	{
		get => Count == 0;
	}

	public void Add(T item) mut
	{
		lock (synchronization)
			items.Add(item);
	}

	public bool TryTake(out T item) mut
	{
		item = default(T);
		lock (synchronization)
		{
			int32 count = items.Count;
			if (count == 0)
				return false;
			item = items[count - 1];
			items.RemoveAt(count - 1);
			return true;
		}
	}

	public bool TryPeek(out T item)
	{
		item = default(T);
		lock (synchronization)
		{
			int32 count = items.Count;
			if (count == 0)
				return false;
			item = items[count - 1];
			return true;
		}
	}

	public void Clear() mut
	{
		lock (synchronization)
			items.Clear();
	}

	public T[] ToArray()
	{
		lock (synchronization)
			return items.ToArray();
	}

	public Iterator<T> Iterate()
	{
		return new Iterator<T>(ToArray());
	}

	IIterator<T> IEnumerable<T>.Iterate()
	{
		return new Iterator<T>(ToArray());
	}

	public class Iterator<TItem> : IIterator<TItem> where TItem : default
	{
		private TItem[] snapshot;
		private int32 index;

		public this(TItem[] snapshot)
		{
			this.snapshot = snapshot;
			index = -1;
		}

		public bool Next() mut
		{
			index++;
			return index < snapshot.Length;
		}

		public TItem Current
		{
			get
			{
				if (index < 0 || index >= snapshot.Length)
					return default(TItem);
				return snapshot[index];
			}
		}
	}
}