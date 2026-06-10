using SMOO.Util;

namespace SMOO.Client;

internal class PlayerWorldInfo : ISerializableStruct
{
    public string? CurrentStage { get; set; }
    public string? CostumeBody { get; set; }
    public string? CostumeCap { get; set; }

    public ushort Size()
    {
        SizeStream stream = new SizeStream();

        stream.WriteString(CurrentStage ?? string.Empty);
        stream.WriteString(CostumeBody ?? string.Empty);
        stream.WriteString(CostumeCap ?? string.Empty);

        return stream.Size;
    }

    public void Serialize(Span<byte> destination)
    {
        SpanWriter writer = new SpanWriter(destination);

        writer.WriteString(CurrentStage ?? string.Empty);
        writer.WriteString(CostumeBody ?? string.Empty);
        writer.WriteString(CostumeCap ?? string.Empty);
    }
}
