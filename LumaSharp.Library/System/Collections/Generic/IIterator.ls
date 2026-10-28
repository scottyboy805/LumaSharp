namespace System.Collections.Generic;

public interface IIterator<T>
{
	bool Next();
	T Current { get; }
}
