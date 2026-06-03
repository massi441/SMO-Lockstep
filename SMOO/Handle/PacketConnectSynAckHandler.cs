//using System.Runtime.CompilerServices;
//using System.Runtime.InteropServices;
//using SMOO.Client;
//using SMOO.Protocol;
//using SMOO.Server;
//using SMOO.Util;

//namespace SMOO.Handle;

//internal class PacketConnectSynAckHandler : IPacketHandler
//{
//    public uint MinPayloadSize => 0;

//    /// <summary>
//    /// The packet sent to notify a room that a player has joined
//    /// </summary>
//    [StructLayout(LayoutKind.Sequential, Pack = 1)]
//    private struct PacketConnectSynAck
//    {
//        public required PacketHeader Header;
//        public ushort SequenceNumber;
//        public required byte PlayerPort;

//        public static ushort SizeOf()
//        {
//            return (ushort)Unsafe.SizeOf<PacketConnectSynAck>();
//        }

//        public static ushort SizeOfPayload()
//        {
//            return sizeof(byte);
//        }
//    }

//    public void Handle(Packet packet, Room room)
//    {
//        // TODO: Get player

//        RentedBuffer broadcastBuffer = MemoryUtil.Rent<PacketConnectSynAck>();

//        WriteBroadcast(broadcastBuffer, packet, null!);

//        PacketBroadcastRequest broadcastRequest = new PacketBroadcastRequest()
//        {
//            MaxRetries = Config.MaxRetries,
//            RentedPayload = broadcastBuffer
//        };
//    }

//    private static Result<Error> NotifyRoom(Packet packet, Room room, Player newPlayer)
//    {
//        byte otherPlayersCount = room.PlayerHolder.OtherPlayerCount;
//        //byte[] ackBuffer = ArrayPool<byte>.Shared.Rent(PacketConnectAck.SizeOf(otherPlayersCount)); ;

//        //WriteAck(ackBuffer, packet, room, newPlayer, otherPlayersCount);

//        //PacketBroadcastRequest newPlayerAckRequest = new PacketBroadcastRequest()
//        //{
//        //    MaxRetries = Config.MaxRetries,
//        //    Payload = ackBuffer
//        //};

//        return room.Broadcaster.BroadcastAckExceptWith(room, newPlayer, in newPlayerAckRequest, in broadcastRequest);
//    }

//    private static void WriteBroadcast(Span<byte> buffer, Packet packet, Player newPlayer)
//    {
//        PacketConnectSynAck broadcastPacket = new PacketConnectSynAck
//        {
//            PlayerPort = newPlayer.PortNumber,
//            Header = packet.Header.WithSizeType(PacketConnectSynAck.SizeOfPayload(), PacketType.PlayerJoinRoomBroadcast)
//        };

//        MemoryMarshal.Write(buffer, broadcastPacket);
//    }
//}
