namespace System.Collections;

public class Stack<T> : IEnumerable<T>
{
	private T?[] _items;
	private int32 _count;

	public this()
	{
		_items = new T?[4];
		_count = 0;
	}

	public this(int32 capacity)
	{
		if (capacity < 0)
			CollectionGuard.ThrowOutOfRange();
		_items = new T?[capacity];
		_count = 0;
	}

	public int32 Count
	{
		get => _count;
	}

	public void Push(T item) mut
	{
		EnsureCapacity(_count + 1);
		_items[_count] = item;
		_count++;
	}

	public T Pop() mut
	{
		if (_count == 0)
			CollectionGuard.ThrowInvalidOperation("The stack is empty.");
		_count--;
		T item = _items[_count]!;
		_items[_count] = null;
		return item;
	}

	public T Peek()
	{
		if (_count == 0)
			CollectionGuard.ThrowInvalidOperation("The stack is empty.");
		return _items[_count - 1]!;
	}

	public void Clear() mut
	{
		for (int32 index = 0; index < _count; index++)
			_items[index] = null;
		_count = 0;
	}

	public T GetAt(int32 index)
	{
		return _items[index]!;
	}

	public IIterator<T> Iterate()
	{
		return new StackIterator<T>(this);
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
			replacement[index] = _items[index];
		_items = replacement;
	}
}

internal class StackIterator<T> : IIterator<T>
{
	private Stack<T> _stack;
	private int32 _index;

	public this(Stack<T> stack)
	{
		_stack = stack;
		_index = stack.Count;
	}

	public bool Next() mut
	{
		_index--;
		return _index >= 0;
	}

	public T Current
	{
		get => _stack.GetAt(_index);
	}
}
