namespace SMOO.Protocol;

internal interface IDeserializableStruct
{
    void Deserialize(ReadOnlySpan<byte> source);
}
