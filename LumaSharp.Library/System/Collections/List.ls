using System;

namespace System.Collections;

public class List<T> : IEnumerable<T> where T : default, System.IEquality<T>
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

	public int32 Capacity
	{
		get => items.Length;
	}

	public unsafe T* Ptr
	{
		get
		{
			if (count == 0)
				return null;
			unsafe unchecked
			{
				return &items[0];
			}
		}
	}

	public static unsafe implicit operator Span<T>(List<T> source)
	{
		return Span<T>(source.Ptr, source.Count, source.items);
	}

	public static unsafe implicit operator ReadOnlySpan<T>(List<T> source)
	{
		return ReadOnlySpan<T>(source.Ptr, source.Count, source.items);
	}

	public T this[int32 index]
	{
		get
		{
			if (index < 0 || index >= count)
				throw new System.ArgumentOutOfRangeException();
			return items[index];
		}
		set
		{
			if (index < 0 || index >= count)
				throw new System.ArgumentOutOfRangeException();
			items[index] = value;
		}
	}

	public void Add(T item) mut
	{
		if (count == items.Length)
			EnsureCapacity(count + 1);
		items[count] = item;
		count++;
	}

	public void Insert(int32 index, T item) mut
	{
		if (index < 0 || index > count)
			throw new System.ArgumentOutOfRangeException();
		EnsureCapacity(count + 1);
		Array.Copy(items, index, items, index + 1, count - index);
		items[index] = item;
		count++;
	}

	public void RemoveAt(int32 index) mut
	{
		if (index < 0 || index >= count)
			throw new System.ArgumentOutOfRangeException();
		Array.Copy(items, index + 1, items, index, count - index - 1);
		count--;
		items[count] = default(T);
	}

	public void Clear() mut
	{
		Array.Clear(items, 0, count);
		count = 0;
	}

	public int32 IndexOf(T item)
	{
		return Array.IndexOf(items, item, 0, count);
	}
	public bool Contains(T item)
	{
		return IndexOf(item) >= 0;
	}

	public Iterator<T> Iterate()
	{
		return Iterator<T>(this);
	}

	IIterator<T> IEnumerable<T>.Iterate()
	{
		return Iterator<T>(this);
	}

	public bool Remove(T item) mut
	{
		int32 index = IndexOf(item);
		if (index < 0)
			return false;
		RemoveAt(index);
		return true;
	}

	public T[] ToArray()
	{
		T[] result = new T[count];
		Array.Copy(items, 0, result, 0, count);
		return result;
	}

	private void EnsureCapacity(int32 minimum) mut
	{
		if (minimum <= items.Length)
			return;
		int32 capacity = items.Length == 0 ? 4 : items.Length * 2;
		if (capacity < minimum)
			capacity = minimum;
		T[] replacement = new T[capacity];
		Array.Copy(items, 0, replacement, 0, count);
		items = replacement;
	}

	public struct Iterator<TItem> : IIterator<TItem> where TItem : default
	{
		private List<TItem>? list;
		private int32 index;

		public this(List<TItem>? source)
		{
			list = source;
			index = -1;
		}

		public bool Next() mut
		{
			if (list == null)
				return false;
			index++;
			return index < list.Count;
		}

		public TItem Current
		{
			get
			{
				if (list == null)
					return default(TItem);
				return list[index];
			}
		}
	}

}
