namespace SMOO.Enumerator;

internal static class SpanEnumeratorExentsions
{
    public static int Count<T, TEnumerator>(this TEnumerator enumerator) where TEnumerator : ISpanEnumerator<T, TEnumerator>, allows ref struct
    {
        int count = 0;
        while (enumerator.MoveNext())
        {
            count++;
        }

        return count;
    }
}

