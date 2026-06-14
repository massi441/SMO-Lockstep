namespace SMOO.Enumerator;

internal interface ISpanEnumerator<T, TSelf> : IDisposable where TSelf : allows ref struct
{
    T Current { get; }
    TSelf GetEnumerator();
    bool MoveNext();
}

