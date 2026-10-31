namespace System;

public interface IEquality<T> where T : default
{
	bool Equals(T other);
}
