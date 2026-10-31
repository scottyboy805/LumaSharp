namespace System.Collections;

public interface IEnumerable<T> where T : default
{
	IIterator<T> Iterate();
}
