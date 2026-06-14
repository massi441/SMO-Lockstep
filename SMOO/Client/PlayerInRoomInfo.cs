using SMOO.Util;

namespace SMOO.Client;

internal readonly struct PlayerInRoomInfo
{
    public readonly byte PlayerIndex;
    public readonly StreamStringView<byte> PlayerName;
    public readonly StreamStringView<byte> CostumeBody;
    public readonly StreamStringView<byte> CostumeCap;

    public PlayerInRoomInfo(Player player)
    {
        PlayerIndex = player.Slot;

        PlayerName = new StreamStringView<byte>(player.Name);
        CostumeBody = new StreamStringView<byte>(player.WorldInfo.CostumeBody);
        CostumeCap = new StreamStringView<byte>(player.WorldInfo.CostumeCap);
    }

    public void Serialize(ref SpanWriter writer)
    {
        writer.Write(PlayerIndex);

        PlayerName.Serialize(ref writer);
        CostumeBody.Serialize(ref writer);
        CostumeCap.Serialize(ref writer);
    }
}
