namespace System.Collections;

public interface IEnumerable<T>
{
	IIterator<T> Iterate();
}
