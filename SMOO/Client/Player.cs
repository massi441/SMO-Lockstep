
using System.Net;
using SMOO.Server;

namespace SMOO.Client;

internal class Player
{
    public required IPEndPoint Endpoint { get; init; }
    public required string Name { get; init; }
    public required Room Room { get; init; }
    public DateTime LastSeen { get; private set; } = DateTime.UtcNow;

    public void RefreshLastSeen()
    {
        LastSeen = DateTime.UtcNow;
    }
}
