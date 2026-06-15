using System.Numerics;
using System.Text;

namespace SMOO.Util;

/// <summary>
/// Represents a length prefixed string in a byte stream
/// </summary>
/// <typeparam name="TLengthPrefix">The type of the length prefix</typeparam>
internal struct StreamStringView<TLengthPrefix> where TLengthPrefix : unmanaged, IBinaryInteger<TLengthPrefix>, IMinMaxValue<TLengthPrefix>
{
    [RequiredField]
    private TLengthPrefix _length;
    private string _string;

    public readonly TLengthPrefix Length => _length;
    public readonly string String => _string;

    public StreamStringView(string str)
    {
        _length = TLengthPrefix.CreateChecked(Encoding.UTF8.GetByteCount(str));
        _string = str;
    }

    public void Deserialize(ref SpanReader reader, TLengthPrefix maxReadLength)
    {
        _length = reader.Read<TLengthPrefix>();
        if (_length > maxReadLength)
        {
            throw new InvalidDataException($"The string length prefix ({_length}) was bigger than the maximum size allowed ({maxReadLength})");
        }

        int length = int.CreateChecked(_length);
        if (length > reader.RemainingByteCount)
        {
            throw new InvalidDataException($"The string length prefix ({_length}) was bigger than the remaining bytes ({reader.RemainingByteCount}) in the reader");
        }

        _string = Encoding.UTF8.GetString(reader.ReadBytes(length));
    }

    public readonly void Serialize(ref SpanWriter writer)
    {
        writer.Write(_length);
        writer.WriteString(_string);
    }

    public override readonly string ToString()
    {
        return _string;
    }
}
