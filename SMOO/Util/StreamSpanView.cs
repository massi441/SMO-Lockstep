using System.Numerics;

namespace SMOO.Util;

internal ref struct StreamSpanView<TLength, T> 
    where TLength : unmanaged, IBinaryInteger<TLength>
    where T : unmanaged
{
    [RequiredField]
    public TLength Length;
    public ReadOnlySpan<T> Span;

    public StreamSpanView(TLength length, Span<T> span)
    {
        Length = length;
        Span = span;
    }

    public readonly void Serialize(Span<byte> destination)
    {
        SpanWriter writer = new SpanWriter(destination);

        writer.Write(Length);
        writer.WriteSpan(Span);
    }

    public readonly void Serialize(ref SpanWriter writer)
    {
        writer.Write(Length);
        writer.WriteSpan(Span);
    }

    public void Deserialize(ref SpanReader reader)
    {
        Length = reader.Read<TLength>();
        Span = reader.ReadView<T>(int.CreateChecked(Length));
    }
}
