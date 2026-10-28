namespace System.Collections;

public class Queue<T> : IEnumerable<T> where T : default
{
	private T[] items;
	private int32 head;
	private int32 count;

	public this()
	{
		items = new T[4];
		head = 0;
		count = 0;
	}

	public this(int32 capacity)
	{
		if (capacity < 0)
			throw new System.ArgumentOutOfRangeException();
		items = new T[capacity];
		head = 0;
		count = 0;
	}

	public int32 Count
	{
		get => count;
	}

	public unsafe T* Ptr
	{
		get
		{
			if (items.Length == 0)
				return null;
			return &items[0];
		}
	}

	public void Enqueue(T item) mut
	{
		EnsureCapacity(count + 1);
		int32 tail = (head + count) % items.Length;
		items[tail] = item;
		count++;
	}

	public T Dequeue() mut
	{
		if (count == 0)
			throw new System.InvalidOperationException("The queue is empty.");
		T item = items[head];
		items[head] = default(T);
		head = (head + 1) % items.Length;
		count--;
		return item;
	}

	public T Peek()
	{
		if (count == 0)
			throw new System.InvalidOperationException("The queue is empty.");
		return items[head];
	}

	public void Clear() mut
	{
		while (count > 0)
			Dequeue();
	}

	public T GetAt(int32 index)
	{
		return items[(head + index) % items.Length];
	}

	public IIterator<T> Iterate()
	{
		return new Iterator<T>(this);
	}

	private void EnsureCapacity(int32 minimum) mut
	{
		if (minimum <= items.Length)
			return;
		int32 capacity = items.Length == 0 ? 4 : items.Length * 2;
		if (capacity < minimum)
			capacity = minimum;
		T[] replacement = new T[capacity];
		for (int32 index = 0; index < count; index++)
			replacement[index] = items[(head + index) % items.Length];
		items = replacement;
		head = 0;
	}

	private class Iterator<TItem> : IIterator<TItem> where TItem : default
	{
		private Queue<TItem> queue;
		private int32 index;

		public this(Queue<TItem> source)
		{
			queue = source;
			index = -1;
		}

		public bool Next() mut
		{
			index++;
			return index < queue.Count;
		}

		public TItem Current
		{
			get => queue.GetAt(index);
		}
	}
}
