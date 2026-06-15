namespace SMOO.Util;

internal class RefCounter
{
    private int _count = 0;

    public int Count => _count;

    public int Increment()
    {
        return Interlocked.Increment(ref _count);
    }

    public int Decrement()
    {
        return Interlocked.Decrement(ref _count);
    }
}
