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

    public void Deserialize(ref SpanReader reader)
    {
        _length = reader.Read<TLengthPrefix>();
        _string = Encoding.UTF8.GetString(reader.ReadBytes(int.CreateChecked(_length)));
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
