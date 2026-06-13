using System.Numerics;
using System.Text;

namespace SMOO.Util;

internal struct StreamStringView<T> where T : struct, IBinaryInteger<T>
{
    public T Length;
    public string String;

    public void Deserialize(ref SpanReader reader)
    {
        Length = reader.Read<T>();
        String = Encoding.UTF8.GetString(reader.ReadBytes(int.CreateChecked(Length)));
    }

    public readonly void Serialize(ref SpanWriter writer)
    {
        writer.Write(Length);
        writer.WriteString(String);
    }
}
