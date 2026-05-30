namespace Lockstep.Protocol;

internal class PendingPacket
{
    private int _triesLeft;

    public DateTime LastSent { get; private set; } = DateTime.UtcNow; 
    public int SequenceNumber { get; init; }
    public int TriesLeft => _triesLeft;

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
