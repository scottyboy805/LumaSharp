namespace System.Collections;

public interface IIterator<T>
{
	bool Next();
	T Current { get; }
}
