using Lockstep.Client;

namespace Lockstep.Protocol;

internal class PacketPending
{
    private byte _tries;
    public required ushort SequenceNumber { get; init; }
    public required byte[] Payload { get; init; }
    public required Player Player { get; init; }
    public DateTime LastSent { get; private set; } = DateTime.UtcNow;

    public byte Tries
    { 
        get => _tries;
        init => _tries = value;
    }

    public bool IsAlive => _tries > 0;
    public bool IsDead => _tries <= 0;

    public void UpdateTime()
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
