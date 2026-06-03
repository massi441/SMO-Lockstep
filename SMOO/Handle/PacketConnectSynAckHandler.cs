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
    private readonly ref struct PacketConnectSynAckPayload
    {
        private readonly ReadOnlySpan<byte> _buffer;

        /// <summary>
        /// The sequence number of the SynAck packet, starting at offset 0x0
        /// </summary>
        public readonly ushort SequenceNumber => BinaryPrimitives.ReadUInt16LittleEndian(_buffer);

        public PacketConnectSynAckPayload(ReadOnlySpan<byte> buffer)
        {
            _buffer = buffer;
        }
    }

    /// <summary>
    /// The packet sent to a room, to notify that a new player has joined
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PacketPlayerJoinRoom
    {
        public required PacketHeader Header;
        public ushort SequenceNumber;
        public byte PlayerNameLength;

        public static int SizeOf()
        {
            return Unsafe.SizeOf<PacketPlayerJoinRoom>();
        }
    }

    public void Handle(Packet packet, Room room)
    {
        Player player = room.PlayerHolder.FindPlayerByHost(packet.Sender)!;

        PacketConnectSynAckPayload synAckPayload = new PacketConnectSynAckPayload(packet.Payload);

        ReliablePacket? ackPacket = room.Broadcaster.ReliablePacketStore.RemovePacket(synAckPayload.SequenceNumber);

        if (ackPacket == null)
        {
            _context.Logger.LogWarning("Invalid SYN ACK sequence number ({SequenceNumber}) received by {PlayerName} in Room #{RoomId}, broadcast will be skipped", synAckPayload.SequenceNumber, player.Name, room.Id);
            return;
        }

        PacketPlayerJoinRoom joinPacket = new PacketPlayerJoinRoom()
        {
            Header = packet.Header.WithSizeType(MemoryUtil.PayloadSize<PacketPlayerJoinRoom>(), PacketType.PlayerJoinRoom),
            PlayerNameLength = (byte)player.Name.Length
        };

        int bufferSize = PacketPlayerJoinRoom.SizeOf() + joinPacket.PlayerNameLength;
        RentedBuffer joinRoomBuffer = new RentedBuffer(bufferSize);

        joinRoomBuffer.Write(joinPacket);

        Encoding.UTF8.GetBytes(player.Name.AsSpan(), joinRoomBuffer.SpanAt(PacketPlayerJoinRoom.SizeOf()));

        Result<Error> broadcastResult = room.Broadcaster.BroadcastReliablyExcept(room, player, new ReliablePacketBroadcastRequest(joinRoomBuffer));

        if (broadcastResult.IsFailed)
        {
            _context.Logger.LogError("Failed to broadcast new player in room");
            return;
        }

        _context.Logger.LogInformation("Player {PlayerName} has confirmed their connection in Room #{RoomId}, room will be notified", player.Name, room.Id);
    }
}
