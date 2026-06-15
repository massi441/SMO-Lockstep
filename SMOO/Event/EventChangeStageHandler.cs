using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Enumerator;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Event;

internal class EventChangeStageHandler : IEventHandler
{
    public static ushort MinDataSize => RequiredSize<ChangeStageData>.MinSize;
    public static ushort MaxDataSize => RequiredSize<ChangeStageData>.MaxSize;

    private struct ChangeStageData : IDeserializableStruct
    {
        [RequiredField]
        public byte ScenarioId;

        [DynamicField(MaxSize = Config.MaxStageNameLength)]
        public StreamStringView<byte> NewStage; // string.Empty is the "Left" stage signal

        public void Deserialize(ref SpanReader reader)
        {
            ScenarioId = reader.ReadByte();
            NewStage.Deserialize(ref reader, Config.MaxStageNameLength);
        }
    }

    public static void Handle(ParsedEventPacket packet, Room room, ServerContext context)
    {
        ChangeStageData data = PacketSerializer.Deserialize<ChangeStageData>(packet.EventData);

        Player player = packet.BasePacket.SenderPlayer!;
        string previousPlayerStage = player.WorldInfo.CurrentStage;

        player.WorldInfo.CurrentStage = data.NewStage.String;

        room.Broadcaster.BroadcastReliably(room.Players.Except(player), packet.BasePacket.RentedBuffer);

        if (data.NewStage.String.Length > 0)
        {
            context.Logger.LogInformation("Player {PlayerName} changed to stage {StageName} (Scenario {ScenarioId}) in Room #{RoomId}", player.Name, data.NewStage, data.ScenarioId, player.Room.Id);

            PlayerSameStageEnumerator playersInStage = room.Players.SameStageAs(player);

            byte inStageCount = (byte)playersInStage.Count<Player, PlayerSameStageEnumerator>();

            if (inStageCount > 0)
            {
                PacketPlayersInStage playersInStagePacket = new PacketPlayersInStage()
                {
                    Header = packet.BasePacket.Header.WithType(PacketType.PlayersInStage),
                    PlayerCount = inStageCount,
                    PlayersInStage = playersInStage
                };

                RentedBuffer buffer = PacketSerializer.Serialize(ref playersInStagePacket, RequiredSize<PacketPlayersInStage>.MaxSize);

                context.Logger.LogInformation("{PlayerCount} were already in stage {StageName}, {PlayerName} will be notified", inStageCount, data.NewStage, player.Name);

                context.PacketSender.SendReliably(player, buffer, room);
            }
        }
        else
        {
            context.Logger.LogInformation("Player {PlayerName} left stage {StageName} in Room #{RoomId}", player.Name, previousPlayerStage, player.Room.Id);
        }
    }
}
