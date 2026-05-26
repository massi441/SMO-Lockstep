using Lockstep.Util;

namespace Lockstep.Protocol;

internal static class PacketParser
{
    public const byte MaxVersion = 1;

    internal static Result<PacketHeader, Error> ParseHeader(Span<byte> payload)
    {
        if (!IsValidHeaderSize(payload))
        {
            return Result<PacketHeader, Error>.Failure(Error.InvalidHeaderSize);
        }

        SpanReader reader = new SpanReader(payload);

        byte packetType = reader.ReadByte();
        if (!IsValidType(packetType))
        {
            return Result<PacketHeader, Error>.Failure(Error.InvalidPacketType);
        }

        byte version = reader.ReadByte();
        if (!IsValidVersion(version))
        {
            return Result<PacketHeader, Error>.Failure(Error.InvalidVersion);
        }

        short payloadSize = reader.ReadInt16LittleEndian();
        if (!IsValidPayloadSize(payload, payloadSize))
        {
            return Result<PacketHeader, Error>.Failure(Error.InvalidPayloadSize);
        }

        PacketHeader header = new PacketHeader
        {
            Type = (PacketType)packetType,
            Version = version,
            PayloadSize = payloadSize
        };

        return Result<PacketHeader, Error>.Success(header);
    }

    private static bool IsValidVersion(byte version)
    {
        return version >= 1 && version <= MaxVersion;
    }

    private static bool IsValidPayloadSize(Span<byte> payload, short payloadSize)
    {
        return payloadSize == payload.Length - PacketHeader.SizeOf();
    }

    private static bool IsValidHeaderSize(Span<byte> span)
    {
        return span.Length >= PacketHeader.SizeOf();
    }

    private static bool IsValidType(byte packetType)
    {
        return packetType >= 0 && packetType < (byte)PacketType.Invalid;
    }
}
