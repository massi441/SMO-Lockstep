namespace SMOO.Util;

internal interface IDeserializableStruct
{
    void Deserialize(ref SpanReader reader);
}
