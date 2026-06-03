using System.Runtime.InteropServices;
using SMOO.Client;
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
    public required RentedBuffer RentedPayload { get; init; }
    public required Player Player { get; init; }

    /// <summary>
    /// Returns a view of the header inside the packet's payload
    /// </summary>
    public ref PacketHeader Header => ref MemoryMarshal.AsRef<PacketHeader>(RentedPayload.Span);

    public DateTime LastSent { get; private set; } = DateTime.UtcNow;
    public bool IsAlive => _tries > 0;
    public bool IsDead => _tries <= 0;
    public bool IsResendTime => (DateTime.UtcNow - LastSent).Milliseconds > Config.MinimumResendSpan.Milliseconds;

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
}
