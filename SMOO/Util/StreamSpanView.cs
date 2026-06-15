using System.Numerics;

namespace SMOO.Util;

internal ref struct StreamSpanView<TLengthPrefix, T> 
    where TLengthPrefix : unmanaged, IBinaryInteger<TLengthPrefix>
    where T : unmanaged
{
    [RequiredField]
    private TLengthPrefix _length;
    private ReadOnlySpan<T> _span;

    public readonly TLengthPrefix Length => _length;
    public readonly ReadOnlySpan<T> Span => _span;

    public StreamSpanView(TLengthPrefix length, Span<T> span)
    {
        _length = length;
        _span = span;
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

    public void Deserialize(ref SpanReader reader, TLengthPrefix maxReadLength)
    {
        _length = reader.Read<TLengthPrefix>();
        if (_length > maxReadLength)
        {
            throw new InvalidDataException($"The span length prefix ({_length}) was bigger than the maximum size allowed ({maxReadLength})");
        }

        int length = int.CreateChecked(_length);
        if (length > reader.RemainingByteCount)
        {
            throw new InvalidDataException($"The span length prefix ({_length}) was bigger than the remaining bytes ({reader.RemainingByteCount}) in the reader");
        }

        _span = reader.ReadView<T>(int.CreateChecked(Length));
    }
}
