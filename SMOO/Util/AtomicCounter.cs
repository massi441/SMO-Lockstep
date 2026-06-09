namespace SMOO.Util;

internal class AtomicCounter
{
    private int _count = 0;

    public int Count => _count;

    public int Increment()
    {
        return Interlocked.Increment(ref _count);
    }

    public int Decrement()
    {
        if (_count == 0)
        {
            return _count;
        }

        return Interlocked.Decrement(ref _count);
    }
}
