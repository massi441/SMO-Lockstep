using SMOO.Util;

namespace SMOO.Client;

internal readonly struct PlayerInRoomInfo
{
    public readonly byte PlayerIndex;

    public readonly byte PlayerNameLength;
    public readonly string Name;

    public readonly byte CostumeBodyLength;
    public readonly string CostumeBody;

    public readonly byte CostumeCapLength;
    public readonly string CostumeCap;

    public PlayerInRoomInfo(Player player)
    {
        PlayerIndex = player.Slot;

        PlayerNameLength = (byte)player.Name.Length;
        Name = player.Name;

        CostumeBodyLength = (byte)player.WorldInfo.CostumeBody.Length;
        CostumeBody = player.WorldInfo.CostumeBody;

        CostumeCapLength = (byte)player.WorldInfo.CostumeCap.Length;
        CostumeCap = player.WorldInfo.CostumeCap;
    }

    public ushort Size()
    {
        SizeStream stream = new SizeStream();

        stream.Write<byte>(); // index

        stream.Write<byte>(); // name length
        stream.WriteString(Name);

        stream.Write<byte>(); // body length
        stream.WriteString(CostumeBody);

        stream.Write<byte>(); // cap length
        stream.WriteString(CostumeCap);

        return stream.Size;
    }

    // will clean up later, need prototype for now
    public void Serialize(ref SpanWriter writer)
    {
        writer.Write(PlayerIndex);

        writer.Write(PlayerNameLength);
        writer.WriteString(Name);

        writer.Write(CostumeBodyLength);
        writer.WriteString(CostumeBody);

        writer.Write(CostumeCapLength); 
        writer.WriteString(CostumeCap);
    }
}
