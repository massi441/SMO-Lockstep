namespace Lockstep.Client;

internal class Player
{
    public required PlayerInfo Info { get; set; }
    public int PortNumber { get; init; }
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
}
