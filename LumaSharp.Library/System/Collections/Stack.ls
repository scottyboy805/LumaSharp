namespace System.Collections;

public class Stack<T> : IEnumerable<T> where T : default
{
	private T[] items;
	private int32 count;

	public this()
	{
		items = new T[4];
		count = 0;
	}

	public this(int32 capacity)
	{
		if (capacity < 0)
			throw new System.ArgumentOutOfRangeException();
		items = new T[capacity];
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

	public void Push(T item) mut
	{
		EnsureCapacity(count + 1);
		unsafe unchecked
		{
			items[count] = item;
		}
		count++;
	}

	public T Pop() mut
	{
		if (count == 0)
			throw new System.InvalidOperationException("The stack is empty.");
		count--;
		T item;
		unsafe unchecked
		{
			item = items[count];
			items[count] = default(T);
		}
		return item;
	}

	public T Peek()
	{
		if (count == 0)
			throw new System.InvalidOperationException("The stack is empty.");
		T item;
		unsafe unchecked
		{
			item = items[count - 1];
		}
		return item;
	}

	public void Clear() mut
	{
		for (int32 index = 0; index < count; index++)
			unsafe unchecked
			{
				items[index] = default(T);
			}
		count = 0;
	}

	public T GetAt(int32 index)
	{
		if (index < 0 || index >= count)
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
				replacement[index] = items[index];
			}
		items = replacement;
	}

	private class Iterator<TItem> : IIterator<TItem> where TItem : default
	{
		private Stack<TItem> stack;
		private int32 index;

		public this(Stack<TItem> source)
		{
			stack = source;
			index = source.Count;
		}

		public bool Next() mut
		{
			index--;
			return index >= 0;
		}

		public TItem Current
		{
			get => stack.GetAt(index);
		}
	}
}
