using SMOO.Util;

namespace SMOO.Client;

internal class PlayerWorldInfo : ISerializableStruct
{
    public string? CurrentStage { get; set; }
    public required string CostumeBody { get; set; }
    public required string CostumeCap { get; set; }

    public ushort Size()
    {
        SizeStream stream = new SizeStream();

        stream.WriteString(CostumeBody);
        stream.WriteString(CostumeCap);

        return stream.Size;
    }

    public void Serialize(Span<byte> destination)
    {
        SpanWriter writer = new SpanWriter(destination);

        writer.WriteString(CostumeBody);
        writer.WriteString(CostumeCap);
    }
}
