using System.Net;
using Lockstep.Util;

namespace Lockstep.Protocol;

internal class PacketPending
{
    private byte _triesLeft;
    public required ushort SequenceNumber { get; init; }
    public required byte[] Payload { get; init; }
    public required IPEndPoint Receiver { get; init; }
    public DateTime LastSent { get; private set; } = DateTime.UtcNow;
    public Func<IPEndPoint, Result<Error>>? OnDropped { get; init; } = null;

    public byte TriesLeft => _triesLeft;
    public bool IsAlive => _triesLeft > 0;
    public bool IsDead => _triesLeft <= 0;

    public PacketPending(byte maxTries = 3)
    {
        _triesLeft = maxTries;
    }

    public void UpdateTime()
    {
        LastSent = DateTime.UtcNow;
    }

    public void DecrementTries()
    {
        if (_triesLeft > 0)
        {
            _triesLeft--;
        }
    }
}
