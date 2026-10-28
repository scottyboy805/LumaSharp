namespace System.Collections;

internal static class CollectionGuard
{
	public static void ThrowOutOfRange()
	{
		throw new System.ArgumentOutOfRangeException();
	}

	public static void ThrowInvalidOperation(string message)
	{
		throw new System.InvalidOperationException(message);
	}

	public static void ThrowArgument(string message)
	{
		throw new System.ArgumentException(message);
	}

	public static void ThrowKeyNotFound()
	{
		throw new KeyNotFoundException();
	}

	public static void ThrowNullArgument()
	{
		throw new System.ArgumentNullException();
	}
}

