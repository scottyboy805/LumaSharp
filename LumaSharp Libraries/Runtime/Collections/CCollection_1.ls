

namespace Collections;

export contract CCollection<T> : CReadOnlyCollection<T>, CEnumerable<T>
{
    // Methods
    void Add(T item);

    void Remove(T item);

    void Clear(T item);
}
