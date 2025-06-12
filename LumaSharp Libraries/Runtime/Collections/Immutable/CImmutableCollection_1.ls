

namespace Collections:Immutable;

export contract CImmutableCollection<T> : CReadOnlyCollection
{
    // Methods
    CImmutableCollection<T> Add(T item);

    CImmutableCollection<T> Remove(T item);

    CImmutableCollection<T> Clear(T item);
}
