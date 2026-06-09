namespace SMOO.Util;

internal class RefCounter
{
    private int _count = 0;

    public int Count => _count;

    public int Increment()
    {
        return ++_count;
    }

    public int Decrement()
    {
        if (_count == 0)
        {
            return _count;
        }

        return --_count;
    }
}
