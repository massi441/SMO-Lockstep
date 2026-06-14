using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Protocol;

internal static class PacketDispatcher
{
    public static Result<Error> Dispatch(Packet packet, ServerContext context)
    {
        if (!IsValidHeaderSize(packet.RentedBuffer.UsedSpan))
        {
            return Result<Error>.Failure(Error.InvalidHeaderSize);
        }

        ref PacketHeader header = ref packet.Header;

        if (header.Magic != Config.Magic)
        {
            return Result<Error>.Failure(Error.InvalidMagic);
        }

        if (!IsValidType((byte)header.Type))
        {
            return Result<Error>.Failure(Error.InvalidPacketType);
        }

        if (!IsValidVersion(header.Version))
        {
            return Result<Error>.Failure(Error.InvalidVersion);
        }

        if (header.Type == PacketType.Ping)
        {
            context.Logger.LogTrace("Ping received from {Address}:{Port}", packet.Sender.Address, packet.Sender.Port);
            Result<Error> result = context.PacketSender.Send(packet.Sender, packet.RentedBuffer);
            packet.RentedBuffer.Return();
            return result;
        }

        Room? room = context.RoomHolder.GetRoom(header.RoomId);
        if (room == null)
        {
            return Result<Error>.Failure(Error.RoomNotFound);
        }

        room.Packets.Writer.TryWrite(packet);

        return Result<Error>.Success();
    }

    private static bool IsValidVersion(byte version)
    {
        return version == Config.Version;
    }

    private static bool IsValidHeaderSize(ReadOnlySpan<byte> span)
    {
        return span.Length >= Unsafe.SizeOf<PacketHeader>();
    }

    private static bool IsValidType(byte packetType)
    {
        return packetType >= 0 && packetType < (byte)PacketType.Invalid;
    }
}
