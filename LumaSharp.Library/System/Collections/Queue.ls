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
			unsafe unchecked
			{
				return &items[0];
			}
		}
	}

	public void Enqueue(T item) mut
	{
		EnsureCapacity(count + 1);
		int32 tail = (head + count) % items.Length;
		unsafe unchecked
		{
			items[tail] = item;
		}
		count++;
	}

	public T Dequeue() mut
	{
		if (count == 0)
			throw new System.InvalidOperationException("The queue is empty.");
		T item;
		unsafe unchecked
		{
			item = items[head];
			items[head] = default(T);
		}
		head = (head + 1) % items.Length;
		count--;
		return item;
	}

	public T Peek()
	{
		if (count == 0)
			throw new System.InvalidOperationException("The queue is empty.");
		T item;
		unsafe unchecked
		{
			item = items[head];
		}
		return item;
	}

	public void Clear() mut
	{
		while (count > 0)
			Dequeue();
	}

	public T GetAt(int32 index)
	{
		if (index < 0 || index >= count)
			throw new System.ArgumentOutOfRangeException();
		T item;
		unsafe unchecked
		{
			item = items[(head + index) % items.Length];
		}
		return item;
	}

	public Iterator<T> Iterate()
	{
		return Iterator<T>(this);
	}

	IIterator<T> IEnumerable<T>.Iterate()
	{
		return Iterator<T>(this);
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
			unsafe unchecked
			{
				replacement[index] = items[(head + index) % items.Length];
			}
		items = replacement;
		head = 0;
	}

	public struct Iterator<TItem> : IIterator<TItem> where TItem : default
	{
		private Queue<TItem>? queue;
		private int32 index;

		public this(Queue<TItem> source)
		{
			queue = source;
			index = -1;
		}

		public bool Next() mut
		{
			index++;
			return index < queue!.Count;
		}

		public TItem Current
		{
			get => queue!.GetAt(index);
		}
	}
}
