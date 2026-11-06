using System.Collections;

namespace System.Collections.Concurrent;

public class ConcurrentStack<T> : IEnumerable<T> where T : default
{
	private readonly object synchronization = new object();
	private Stack<T> stack;

	public this()
	{
		stack = new Stack<T>();
	}

	public int32 Count
	{
		get
		{
			lock (synchronization)
				return stack.Count;
		}
	}

	public bool IsEmpty
	{
		get => Count == 0;
	}

	public void Push(T item) mut
	{
		lock (synchronization)
			stack.Push(item);
	}

	public void PushRange(T[]? source) mut
	{
		if (source == null)
			throw new System.ArgumentNullException();
		PushRange(source, 0, source.Length);
	}

	public void PushRange(T[]? source, int32 sourceIndex, int32 count) mut
	{
		if (source == null)
			throw new System.ArgumentNullException();
		if (sourceIndex < 0 || count < 0 || sourceIndex > source.Length - count)
			throw new System.ArgumentOutOfRangeException();
		lock (synchronization)
		{
			for (int32 index = 0; index < count; index++)
				stack.Push(source[sourceIndex + index]);
		}
	}

	public bool TryPop(out T item) mut
	{
		item = default(T);
		lock (synchronization)
		{
			if (stack.Count == 0)
				return false;
			item = stack.Pop();
			return true;
		}
	}

	public bool TryPeek(out T item)
	{
		item = default(T);
		lock (synchronization)
		{
			if (stack.Count == 0)
				return false;
			item = stack.Peek();
			return true;
		}
	}

	public int32 TryPopRange(T[]? destination) mut
	{
		if (destination == null)
			throw new System.ArgumentNullException();
		return TryPopRange(destination, 0, destination.Length);
	}

	public int32 TryPopRange(T[]? destination, int32 destinationIndex, int32 count) mut
	{
		if (destination == null)
			throw new System.ArgumentNullException();
		if (destinationIndex < 0 || count < 0 || destinationIndex > destination.Length - count)
			throw new System.ArgumentOutOfRangeException();
		lock (synchronization)
		{
			int32 popped = 0;
			while (popped < count && stack.Count > 0)
			{
				destination[destinationIndex + popped] = stack.Pop();
				popped++;
			}
			return popped;
		}
	}

	public void Clear() mut
	{
		lock (synchronization)
			stack.Clear();
	}

	public T[] ToArray()
	{
		lock (synchronization)
		{
			int32 count = stack.Count;
			T[] snapshot = new T[count];
			for (int32 index = 0; index < count; index++)
				snapshot[index] = stack.GetAt(count - index - 1);
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