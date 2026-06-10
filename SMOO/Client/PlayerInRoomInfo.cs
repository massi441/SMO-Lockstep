using System.Text;
using SMOO.Util;

namespace SMOO.Client;

internal readonly struct PlayerInRoomInfo
{
    public readonly byte PlayerIndex;
    public readonly byte PlayerNameLength;
    public readonly string Name;

    public PlayerInRoomInfo(Player player)
    {
        PlayerIndex = player.Slot;
        PlayerNameLength = (byte)Encoding.UTF8.GetByteCount(player.Name.AsSpan());
        Name = player.Name;
    }

    public ushort Size()
    {
        SizeStream stream = new SizeStream();

        stream.Write<byte>();
        stream.Write<byte>();
        stream.WriteString(Name);

        return stream.Size;
    }

    // will clean up later, need prototype for now
    public void Serialize(ref SpanWriter writer)
    {
        writer.Write(PlayerIndex);
        writer.Write(PlayerNameLength);
        writer.WriteString(Name);
    }
}
