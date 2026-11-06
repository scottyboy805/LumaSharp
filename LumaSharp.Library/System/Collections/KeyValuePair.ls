namespace System.Collections;

public struct KeyValuePair<TKey, TValue> where TKey : default where TValue : default
{
	private TKey key;
	private TValue value;

	public this(TKey key, TValue value)
	{
		this.key = key;
		this.value = value;
	}

	public TKey Key
	{
		get => key;
	}

	public TValue Value
	{
		get => value;
	}
}