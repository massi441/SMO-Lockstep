using SMOO.Util;

namespace SMOO.Client;

internal class PlayerWorldInfo : ISerializableStruct
{
    public required string CurrentStage { get; set; }
    public required string CostumeBody { get; set; }
    public required string CostumeCap { get; set; }

    public void Serialize(ref SpanWriter writer)
    {
        writer.WriteString(CostumeBody);
        writer.WriteString(CostumeCap);
    }
}
