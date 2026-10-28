namespace System.Collections.Generic;

public interface IEnumerable<T>
{
	IIterator<T> Iterate();
}
