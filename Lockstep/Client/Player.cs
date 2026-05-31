
using System.Net;
using Lockstep.Server;

namespace Lockstep.Client;

internal class Player
{
    public required IPEndPoint Endpoint { get; init; }
    public required string Name { get; init; }
    public required Room Room { get; init; }
    public required byte PortNumber { get; init; }
    public DateTime LastSeen { get; private set; } = DateTime.UtcNow;

    public void RefreshLastSeen()
    {
        LastSeen = DateTime.UtcNow;
    }
}
