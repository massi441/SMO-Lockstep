namespace SMOO.Util;

internal interface IDeserializableStruct
{
    void Deserialize(ReadOnlySpan<byte> source);
}
