using Lockstep.Util;

namespace Lockstep.Protocol;

internal static class PacketParser
{
    internal static Result<PacketHeader, Error> ParseHeader(ReadOnlySpan<byte> packet)
    {
        if (!IsValidHeaderSize(packet))
        {
            return Result<PacketHeader, Error>.Failure(Error.InvalidHeaderSize);
        }

        SpanReader reader = new SpanReader(packet);

        uint magic = reader.ReadUInt32LittleEndian();
        if (magic != Config.Magic)
        {
            return Result<PacketHeader, Error>.Failure(Error.InvalidMagic);
        }

        byte packetType = reader.ReadByte();
        if (!IsValidType(packetType))
        {
            return Result<PacketHeader, Error>.Failure(Error.InvalidPacketType);
        }

        byte flags = reader.ReadByte();

        byte version = reader.ReadByte();
        if (!IsValidVersion(version))
        {
            return Result<PacketHeader, Error>.Failure(Error.InvalidVersion);
        }

        ushort roomId = reader.ReadUInt16LittleEndian();

        ushort payloadSize = reader.ReadUInt16LittleEndian();
        if (!IsValidPayloadSize(packet, payloadSize))
        {
            return Result<PacketHeader, Error>.Failure(Error.InvalidPayloadSize);
        }

        PacketHeader header = new PacketHeader
        {
            Type = (PacketType)packetType,
            Flags = flags,
            Version = version,
            RoomId = roomId,
            PayloadSize = payloadSize
        };

        return Result<PacketHeader, Error>.Success(header);
    }

    private static bool IsValidVersion(byte version)
    {
        return version == Config.Version;
    }

    private static bool IsValidPayloadSize(ReadOnlySpan<byte> payload, ushort payloadSize)
    {
        return payloadSize == payload.Length - PacketHeader.SizeOf();
    }

    private static bool IsValidHeaderSize(ReadOnlySpan<byte> span)
    {
        return span.Length >= PacketHeader.SizeOf();
    }

    private static bool IsValidType(byte packetType)
    {
        return packetType >= 0 && packetType < (byte)PacketType.Invalid;
    }
}
