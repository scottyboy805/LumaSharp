using System.Collections;

namespace System.Collections.Concurrent;

public class ConcurrentQueue<T> : IEnumerable<T> where T : default
{
	private readonly object synchronization = new object();
	private Queue<T> queue;

	public this()
	{
		queue = new Queue<T>();
	}

	public int32 Count
	{
		get
		{
			lock (synchronization)
				return queue.Count;
		}
	}

	public bool IsEmpty
	{
		get => Count == 0;
	}

	public void Enqueue(T item) mut
	{
		lock (synchronization)
			queue.Enqueue(item);
	}

	public bool TryDequeue(out T item) mut
	{
		item = default(T);
		lock (synchronization)
		{
			if (queue.Count == 0)
				return false;
			item = queue.Dequeue();
			return true;
		}
	}

	public bool TryPeek(out T item)
	{
		item = default(T);
		lock (synchronization)
		{
			if (queue.Count == 0)
				return false;
			item = queue.Peek();
			return true;
		}
	}

	public void Clear() mut
	{
		lock (synchronization)
			queue.Clear();
	}

	public T[] ToArray()
	{
		lock (synchronization)
		{
			int32 count = queue.Count;
			T[] snapshot = new T[count];
			for (int32 index = 0; index < count; index++)
				snapshot[index] = queue.GetAt(index);
			return snapshot;
		}
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