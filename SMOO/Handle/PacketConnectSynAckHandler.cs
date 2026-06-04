using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Handle;

internal class PacketConnectSynAckHandler : IPacketHandler
{
    private readonly ServerContext _context;

    public PacketConnectSynAckHandler(ServerContext context)
    {
        _context = context;
    }

    public uint MinPayloadSize => 0;

    /// <summary>
    /// The payload sent by a new player, to confirm that they have joined a room
    /// </summary>
    private ref struct PacketConnectSynAckPayload : IDeserializableStruct
    {
        /// <summary>
        /// The sequence number of the SynAck packet, starting at offset 0x0
        /// </summary>
        public ushort SequenceNumber { get; private set; }

        public void Deserialize(ReadOnlySpan<byte> source)
        {
            SequenceNumber = BinaryPrimitives.ReadUInt16LittleEndian(source);
        }
    }

    /// <summary>
    /// The packet sent to a room, to notify that a new player has joined
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PacketPlayerJoinRoom : ISerializableStruct
    {
        public required PacketHeader Header;
        public ushort SequenceNumber;
        public required byte PlayerNameLength;
        public required string PlayerName;

        public readonly int SizeOf()
        {
            return PacketHeader.SizeOf() + sizeof(ushort) + sizeof(byte) + (sizeof(byte) * PlayerNameLength);
        }

        public readonly void Serialize(Span<byte> destination)
        {
            SpanWriter writer = new SpanWriter(destination);

            writer.Write(Header);
            writer.Write(SequenceNumber);
            writer.Write(PlayerNameLength);

            Encoding.UTF8.GetBytes(PlayerName, writer.CurrentSpan);
        }
    }

    public void Handle(Packet packet, Room room, Player? player)
    {
        PacketConnectSynAckPayload synAckPayload = new PacketConnectSynAckPayload();

        synAckPayload.Deserialize(packet.Payload);

        ReliablePacket? ackPacket = room.Broadcaster.ReliablePacketStore.RemovePacket(synAckPayload.SequenceNumber);
        if (ackPacket == null)
        {
            _context.Logger.LogWarning("Invalid SYN ACK sequence number ({SequenceNumber}) received by {PlayerName} in Room #{RoomId}, broadcast will be skipped", synAckPayload.SequenceNumber, player?.Name, room.Id);
            return;
        }

        PacketPlayerJoinRoom joinPacket = new PacketPlayerJoinRoom()
        {
            Header = packet.Header.WithSizeType(MemoryUtil.PayloadSize<PacketPlayerJoinRoom>(), PacketType.PlayerJoinRoom),
            PlayerNameLength = (byte)player!.Name.Length,
            PlayerName = player!.Name,
        };

        RentedBuffer joinRoomBuffer = new RentedBuffer(joinPacket.SizeOf());

        PacketSerializer.Serialize(joinRoomBuffer.Span, in joinPacket);

        ReliablePacketBroadcastRequest request = new ReliablePacketBroadcastRequest()
        {
            RentedPayload = joinRoomBuffer,
            MaxRetries = Config.MaxRetries
        };

        Result<Error> broadcastResult = room.Broadcaster.BroadcastReliablyExcept(room, player, request);
        if (broadcastResult.IsFailed)
        {
            _context.Logger.LogError("Failed to broadcast new player in room");
            return;
        }

        _context.Logger.LogInformation("Player {PlayerName} has confirmed their connection in Room #{RoomId}, room will be notified", player.Name, room.Id);
    }
}
