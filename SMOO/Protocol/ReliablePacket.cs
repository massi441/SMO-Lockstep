using System.Runtime.InteropServices;
using SMOO.Client;
using SMOO.Server;
using SMOO.Util;

namespace SMOO.Protocol;

internal class ReliablePacket
{
    private byte _tries;
    public byte Tries
    {
        get => _tries;
        init => _tries = value;
    }
    public required ushort SequenceNumber { get; init; }
    public required RentedBuffer RentedBuffer { get; init; }
    public required Player Receiver { get; init; }

    /// <summary>
    /// The reference counter to the rented buffer, as a single buffer can be shared by many reliable packets
    /// </summary>
    public required RefCounter RefCounter { get; init; }

    /// <summary>
    /// Returns a view of the header inside the packet's payload
    /// </summary>
    public ref PacketHeader Header => ref MemoryMarshal.AsRef<PacketHeader>(RentedBuffer.UsedSpan);

    public DateTime LastSent { get; private set; } = DateTime.UtcNow;
    public bool HasTriesLeft => _tries > 0;
    public bool IsResendTime => (DateTime.UtcNow - LastSent).TotalMilliseconds > Config.MinimumResendDelay.TotalMilliseconds;

    public void RefreshLastSent()
    {
        LastSent = DateTime.UtcNow;
    }

    public void DecrementTries()
    {
        if (_tries > 0)
        {
            _tries--;
        }
    }

    public void WriteSequenceNumber()
    {
        PacketUtil.WriteSequenceNumber(RentedBuffer.UsedSpan, SequenceNumber);
    }
}
