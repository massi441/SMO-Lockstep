using Lockstep.Util;

namespace Lockstep.Protocol;

internal static class PacketParser
{
    public const byte MaxVersion = 1;

    internal static Result<PacketHeader, Error> ParseHeader(ReadOnlySpan<byte> packet)
    {
        if (!IsValidHeaderSize(packet))
        {
            return Result<PacketHeader, Error>.Failure(Error.InvalidHeaderSize);
        }

        SpanReader reader = new SpanReader(packet);

        uint magic = reader.ReadUInt32LittleEndian();
        if (magic != PacketHeader.Magic)
        {
            return Result<PacketHeader, Error>.Failure(Error.InvalidMagic);
        }

        byte packetType = reader.ReadByte();
        if (!IsValidType(packetType))
        {
            return Result<PacketHeader, Error>.Failure(Error.InvalidPacketType);
        }

        ushort roomId = reader.ReadUInt16LittleEndian();

        byte version = reader.ReadByte();
        if (!IsValidVersion(version))
        {
            return Result<PacketHeader, Error>.Failure(Error.InvalidVersion);
        }

        ushort payloadSize = reader.ReadUInt16LittleEndian();
        if (!IsValidPayloadSize(packet, payloadSize))
        {
            return Result<PacketHeader, Error>.Failure(Error.InvalidPayloadSize);
        }

        PacketHeader header = new PacketHeader
        {
            Type = (PacketType)packetType,
            Version = version,
            RoomId = roomId,
            PayloadSize = payloadSize
        };

        return Result<PacketHeader, Error>.Success(header);
    }

    private static bool IsValidVersion(byte version)
    {
        return version >= 1 && version <= MaxVersion;
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
