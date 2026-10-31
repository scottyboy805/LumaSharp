namespace System.Collections;

public interface IIterator<T> where T : default
{
	bool Next();
	T Current { get; }
}
