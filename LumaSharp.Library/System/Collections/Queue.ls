namespace System.Collections;

public class Queue<T> : IEnumerable<T>
{
	private T?[] _items;
	private int32 _head;
	private int32 _count;

	public this()
	{
		_items = new T?[4];
		_head = 0;
		_count = 0;
	}

	public this(int32 capacity)
	{
		if (capacity < 0)
			CollectionGuard.ThrowOutOfRange();
		_items = new T?[capacity];
		_head = 0;
		_count = 0;
	}

	public int32 Count
	{
		get => _count;
	}

	public void Enqueue(T item) mut
	{
		EnsureCapacity(_count + 1);
		int32 tail = (_head + _count) % _items.Length;
		_items[tail] = item;
		_count++;
	}

	public T Dequeue() mut
	{
		if (_count == 0)
			CollectionGuard.ThrowInvalidOperation("The queue is empty.");
		T item = _items[_head]!;
		_items[_head] = null;
		_head = (_head + 1) % _items.Length;
		_count--;
		return item;
	}

	public T Peek()
	{
		if (_count == 0)
			CollectionGuard.ThrowInvalidOperation("The queue is empty.");
		return _items[_head]!;
	}

	public void Clear() mut
	{
		while (_count > 0)
			Dequeue();
	}

	public T GetAt(int32 index)
	{
		return _items[(_head + index) % _items.Length]!;
	}

	public IIterator<T> Iterate()
	{
		return new QueueIterator<T>(this);
	}

	private void EnsureCapacity(int32 minimum) mut
	{
		if (minimum <= _items.Length)
			return;
		int32 capacity = _items.Length == 0 ? 4 : _items.Length * 2;
		if (capacity < minimum)
			capacity = minimum;
		T?[] replacement = new T?[capacity];
		for (int32 index = 0; index < _count; index++)
			replacement[index] = _items[(_head + index) % _items.Length];
		_items = replacement;
		_head = 0;
	}
}

internal class QueueIterator<T> : IIterator<T>
{
	private Queue<T> _queue;
	private int32 _index;

	public this(Queue<T> queue)
	{
		_queue = queue;
		_index = -1;
	}

	public bool Next() mut
	{
		_index++;
		return _index < _queue.Count;
	}

	public T Current
	{
		get => _queue.GetAt(_index);
	}
}
