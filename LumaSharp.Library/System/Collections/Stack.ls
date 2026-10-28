namespace System.Collections;

public class Stack<T> : IEnumerable<T>
{
	private T?[] items;
	private int32 count;

	public this()
	{
		items = new T?[4];
		count = 0;
	}

	public this(int32 capacity)
	{
		if (capacity < 0)
			throw new System.ArgumentOutOfRangeException();
		items = new T?[capacity];
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

	public void Push(T item) mut
	{
		EnsureCapacity(count + 1);
		items[count] = item;
		count++;
	}

	public T Pop() mut
	{
		if (count == 0)
			throw new System.InvalidOperationException("The stack is empty.");
		count--;
		T item = items[count]!;
		items[count] = null;
		return item;
	}

	public T Peek()
	{
		if (count == 0)
			throw new System.InvalidOperationException("The stack is empty.");
		return items[count - 1]!;
	}

	public void Clear() mut
	{
		for (int32 index = 0; index < count; index++)
			items[index] = null;
		count = 0;
	}

	public T GetAt(int32 index)
	{
		return items[index]!;
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
		T?[] replacement = new T?[capacity];
		for (int32 index = 0; index < count; index++)
			replacement[index] = items[index];
		items = replacement;
	}

	private class Iterator<TItem> : IIterator<TItem>
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
