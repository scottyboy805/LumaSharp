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
		if (count == items.Length)
			EnsureCapacity(count + 1);
		int32 tail = head + count;
		if (tail >= items.Length)
			tail -= items.Length;
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
		head++;
		if (head == items.Length)
			head = 0;
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
		Array.ClearReferences(items, 0, items.Length);
		head = 0;
		count = 0;
	}

	public T GetAt(int32 index)
	{
		if (index < 0 || index >= count)
			throw new System.ArgumentOutOfRangeException();
		T item;
		unsafe unchecked
		{
			int32 itemIndex = head + index;
			if (itemIndex >= items.Length)
				itemIndex -= items.Length;
			item = items[itemIndex];
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
		int32 firstLength = count;
		if (firstLength > items.Length - head)
			firstLength = items.Length - head;
		Array.Copy(items, head, replacement, 0, firstLength);
		Array.Copy(items, 0, replacement, firstLength, count - firstLength);
		items = replacement;
		head = 0;
	}

	public struct Iterator<TItem> : IIterator<TItem> where TItem : default
	{
		private Queue<TItem>? queue;
		private int32 index;

		public this(Queue<TItem>? source)
		{
			queue = source;
			index = -1;
		}

		public bool Next() mut
		{
			if (queue == null)
				return false;
			index++;
			return index < queue.Count;
		}

		public TItem Current
		{
			get
			{
				if (queue == null)
					return default(TItem);
				return queue.GetAt(index);
			}
		}
	}
}
