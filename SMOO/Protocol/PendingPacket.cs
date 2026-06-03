using SMOO.Client;
using SMOO.Util;

namespace SMOO.Protocol;

internal class PendingPacket
{
    private byte _tries;
    public required ushort SequenceNumber { get; init; }
    public required RentedBuffer RentedPayload { get; init; }
    public required Player Player { get; init; }
    public DateTime LastSent { get; private set; } = DateTime.UtcNow;

    public byte Tries
    { 
        get => _tries;
        init => _tries = value;
    }

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
