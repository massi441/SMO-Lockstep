using Lockstep.Util;

namespace Lockstep.Protocol;

internal static class PacketParser
{
    internal static Result<Error> ParseHeader(Packet packet)
    {
        if (!IsValidHeaderSize(packet.RentedBuffer.Span))
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

        if (!IsValidPayloadSize(packet.RentedBuffer.Span, header.PayloadSize))
        {
            return Result<Error>.Failure(Error.InvalidPayloadSize);
        }

        return Result<Error>.Success();
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
