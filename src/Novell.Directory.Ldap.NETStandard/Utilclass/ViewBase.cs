using System;
using System.Collections;
using System.Collections.Generic;

namespace Novell.Directory.Ldap.Utilclass;

public abstract class ViewBase<TIn, TOut> : IReadOnlyList<TOut>, IList<TOut>
{
    protected IReadOnlyList<TIn> InnerValues { get; }

    public ViewBase(IReadOnlyList<TIn> innerValues)
    {
        InnerValues = innerValues ?? Array.Empty<TIn>();
    }

    public bool IsReadOnly => true;
    public int Count => InnerValues.Count;

    public abstract TOut this[int index] { get; }

    TOut IList<TOut>.this[int index]
    {
        get => this[index];
        set => throw new NotSupportedException();
    }

    public IEnumerator<TOut> GetEnumerator()
    {
        for (int i = 0; i < InnerValues.Count; i++)
        {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void CopyTo(TOut[] array, int arrayIndex)
    {
        for (var i = 0; i < InnerValues.Count; i++)
        {
            array[arrayIndex + i] = this[i];
        }
    }

    public abstract int IndexOf(TOut item);

    public bool Contains(TOut item)
    {
        return IndexOf(item) >= 0;
    }

    void IList<TOut>.Insert(int index, TOut item)
    {
        throw new NotSupportedException();
    }

    void IList<TOut>.RemoveAt(int index)
    {
        throw new NotSupportedException();
    }

    void ICollection<TOut>.Add(TOut item)
    {
        throw new NotSupportedException();
    }

    void ICollection<TOut>.Clear()
    {
        throw new NotSupportedException();
    }

    bool ICollection<TOut>.Remove(TOut item)
    {
        throw new NotSupportedException();
    }
}
