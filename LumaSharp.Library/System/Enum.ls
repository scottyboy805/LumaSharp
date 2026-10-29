namespace System;

public struct Enum : ValueType
{
	// The compiler replaces each closed enum specialization with its declared values.
	public static T[] GetValues<T>(T val) where T : enum
	{
		return new T[0];
	}
}
